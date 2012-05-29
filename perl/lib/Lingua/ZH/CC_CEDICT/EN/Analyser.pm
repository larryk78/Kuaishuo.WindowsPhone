# Lingua/ZH/CC_CEDICT/EN/Analyser.pm
#
# Copyright (c) 2011 Larry Knibb. All rights reserved.
#

package Lingua::ZH::CC_CEDICT::EN::Analyser;
use strict;

use Carp;
use Data::Dumper;
use Data::Types qw(:decimal);
use Lingua::ZH::CC_CEDICT::EN::Definition;
use Net::Domain::TLD qw(tld_exists);

sub new {
    my $class = shift;
    my $self = bless {}, $class;
    return $self;
}

sub analyse {
    my $self = shift;
    my $string = shift;
    my $weight = shift || 1;
    my @scored = ();

    # $string contains one or more semicolon-separated sub-definitions
    for my $part (split ';', $string) {

        # make sure we didn't accidentally split inside a () or ""
        if (($part =~ s/\(/\(/g) != ($part =~ s/\)/\)/g)) {
            warn "Unmatched parenthesis on line $.: $part\n";
            next;
        } elsif (($part =~ s/\"/\"/g)%2) {
            warn "Unmatched double-quote on line $.: $part\n";
            next;
        }

        # use a Definition object as the processing container
        my $def = Lingua::ZH::CC_CEDICT::EN::Definition->new($part);
        $self->parse($def);

        # uncomment for debugging
        #print Dumper $def;

        # request aggregated [ $word, $score ] from Definition
        push @scored, $def->score($weight);
    }

    # uncomment for debugging
    #print Dumper \@scored;

    return @scored;
}

sub parse {
    my $self = shift;
    my $def = shift;
    my $string = $def->string();

    # make current (working) definition accessible during nested processing
    $self->{current} = $def;

    # high-value Unicode suggests an x-ref to another dictionary entry
    my $zh = qr/[\x{2600}-\x{ffff}]/;
    while ($string =~ s/(?:($zh+)\|)?($zh+)(?:\[(.+?)\])?//) { # trad|simple[pinyin]
        $def->xref($1, $2, $3);
    }

    # TODO: handle pr[onounciation]. x-refs
    # ...

    # handle parenthesised content separately
    while ($string =~ s/\((.*?)\)//) {
        $def->parenthesised($self->wp($1));
    }

    # remove surplus classifiers
    $string =~ s/^\s*(e\.g|esp|fig|i\.e|lit)\.\s+//g;

    # update definition after (possible) parenthesised content removed
    $def->noparen($string);

    # handle quoted content separately
    while ($string =~ s/"(.*?)"//) {
        $def->quoted($self->wp($1));
    }

    # handle remaining content
    $def->words($self->wp($string));
}

sub wp {
    my $self = shift;
    my $string = shift;

    # split $string to words and depunctuate
    my @words = grep /\w/, map { s/(?:[,:;!?\'\"]\z|\.{3})//g; $_ } split(' ', $string);

    # expand hypenated words so they can be partially matched
    /-/ and $self->{current}->partial(split('-', $_)) for @words;

    # set up blacklist of false-positives for posessive 's
    my %blacklist = qw(it's 1 let's 1 one's 1 sb's 1 that's 1 what's 1);

    # trim posessive 's for another partial match
    /^(.*)\'s$/ and $self->{current}->partial($1) for grep !$blacklist{$_}, @words;

    # words containing . (e.g. abbrev., full-stop, decimal, URLs, etc.)
    @words = map $self->dotty($_), @words;

    return @words;
}

sub dotty {
    my $self = shift;
    my $word = shift;

    # ignore plain words and decimal numbers
    return $word if $word !~ /\./ or is_decimal($word);

    # substitute known multi-dot abbreviations
    my %abbrev = qw(
        a.k.a.  aka
        a.m.    am
        e.g.    eg
        i.e.    ie
        p.m.    pm
    );
    return $abbrev{$word} if exists $abbrev{$word};

    # split $word on dot to analyse parts
    my @parts = split /\./, $word;

    if (tld_exists($parts[-1])) { # looks like a URL
        $self->{current}->partial(@parts);
        return $word;
    }

    # trim a single dot
    return $parts[0] if @parts == 1;

    # collapse parts (acronymise) remaining multi-dot "words"
    return join('', @parts);
}

# fixer-uppers
# Unhandled dotted word: r.y.: r y (4494)
# Unhandled dotted word: a.m.-1: a m -1 (13752)
# Unhandled dotted word: 1,609.344: 1,609 344 (17930)
# Unhandled dotted word: p.m.-1: p m -1 (25626)
# Unhandled dotted word: b.Dec: b Dec (43935)
# Unhandled dotted word: Joule.seconds: Joule seconds (43947)
# Unhandled dotted word: volt.seconds: volt seconds (43947)
# Unhandled dotted word: 1911-c.1929: 1911-c 1929 (63177)
# Unhandled dotted word: pro-U.S.: pro-U S (81554)
# Unhandled dotted word: 5.4.3.2.1: 5 4 3 2 1 (94381)
# Unhandled dotted word: i.e: i e (102101)

1;

=pod

=head1 NAME

Lingua::ZH::CC_CEDICT::EN::Analyser - foo.

=head1 SYNOPSIS

 use Lingua::ZH::CC_CEDICT::EN::Analyser;

 my $foo = Lingua::ZH::CC_CEDICT::EN::Analyser->new();

 # whatever

=head1 DESCRIPTION

foo

=head1 METHODS

=head2 new()

Creates the Lingua::ZH::CC_CEDICT::EN::Analyser object.

Accepted %args are:

=over 4

=item bar => OBJECT

Mandatory. The Bar OBJECT where Lingua::ZH::CC_CEDICT::EN::Analyser is going for a drink later.

=item baz => 0|1

Optional. Whether to invite Baz to Lingua::ZH::CC_CEDICT::EN::Analyser Bar.

=back

=head1 SEE ALSO

L<bar>

=head1 COPYRIGHT

Copyright (c) 2011 L<Larry Knibb|mailto://larry.knibb@gmail.com>. All rights reserved.

=head1 LICENSE

This program is free software; you can redistribute it and/or modify it under the same terms as Perl itself.

=cut

__DATA__
