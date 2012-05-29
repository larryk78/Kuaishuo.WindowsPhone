# Lingua/ZH/CC_CEDICT/EN/Definition.pm
#
# Copyright (c) 2011 Larry Knibb. All rights reserved.
#

package Lingua::ZH::CC_CEDICT::EN::Definition;
use strict;

use Carp;

sub new {
    my $class = shift;
    my $string = shift;

    my $self = bless {
        original      => $string,
        xref          => [],
        parenthesised => [],
        quoted        => [],
        partial       => [],
        words         => []
    }, $class;

    return $self;
}

sub string {
    my $self = shift;
    return $self->{original};
}

sub xref {
    my $self = shift;
    push @{$self->{xref}}, [ @_ ];
}

sub parenthesised {
    my $self = shift;
    push @{$self->{parenthesised}}, @_;
}

sub noparen {
    my $self = shift;
    my $string = shift;

    # trim leading/trailing whitespace
    $string =~ s/(\A\s+|\s+\z)//g;

    # store if not duplicate
    $self->{noparen} = $string if $string ne $self->{original};
}

sub quoted {
    my $self = shift;
    push @{$self->{quoted}}, @_;
}

sub partial {
    my $self = shift;
    push @{$self->{partial}}, @_;
}

sub words {
    my $self = shift;
    push @{$self->{words}}, @_;
}

sub score {
    my $self = shift;
    my $weight = shift || 1;
    my %scored = ();

    # full string scored at 110% (if clean)
    my $full = defined $self->{noparen} ? $self->{noparen} : $self->{original};
    my @full = split(' ', $full);
    if (@full > 1 and @full < 4 and $full =~ /^[a-zA-Z0-9\- .]+$/) {
        $scored{$full} = [ $full, int($weight*110) ];
    }

    # quoted words are separately scored out of 100%
    my @quoted = $self->filter(@{$self->{quoted}});
    my $x = scalar @quoted;
    $scored{$_} ||= [ $_, int($weight*100/$x) ] for @quoted;

    # primary content words are scored out of 100%
    my @words = $self->filter(@{$self->{words}});
    my $y = scalar @words;
    $scored{$_} ||= [ $_, int($weight*100/$y) ] for @words;

    # partial (alternate) content is scored out of 75% (of primary)
    my @partial = $self->filter(@{$self->{partial}});
    $y ||= scalar @partial;
    $scored{$_} ||= [ $_, int($weight*75/$y) ] for @partial;

    # parenthesised content scored out of 50%
    my @parenthesised = $self->filter(@{$self->{parenthesised}});
    my $z = scalar @parenthesised;
    $scored{$_} ||= [ $_, int($weight*50/$z) ] for @parenthesised;

    return values %scored;
}

sub filter {
    my $self = shift;

    my %stopwords = ();
    $stopwords{$_}++ for qw(a am an and are as at by eg etc esp etc fig for ie if in is it it\'s lit of on one's or pm sb sb's sth the to);

    # remove the stopwords (defined in DATA)
    my @filtered = grep !$stopwords{$_}, @_;

    # return the filtered list or the original list (in case too effective)
    return scalar(@filtered) ? @filtered : @_;
}

1;

=pod

=head1 NAME

Lingua::ZH::CC_CEDICT::EN::Definition - foo.

=head1 SYNOPSIS

 use Lingua::ZH::CC_CEDICT::EN::Definition;

 my $foo = Lingua::ZH::CC_CEDICT::EN::Definition->new();

 # whatever

=head1 DESCRIPTION

foo

=head1 METHODS

=head2 new()

Creates the Lingua::ZH::CC_CEDICT::EN::Definition object.

Accepted %args are:

=over 4

=item bar => OBJECT

Mandatory. The Bar OBJECT where Lingua::ZH::CC_CEDICT::EN::Definition is going for a drink later.

=item baz => 0|1

Optional. Whether to invite Baz to Lingua::ZH::CC_CEDICT::EN::Definition Bar.

=back

=head1 SEE ALSO

L<bar>

=head1 COPYRIGHT

Copyright (c) 2011 L<Larry Knibb|mailto://larry.knibb@gmail.com>. All rights reserved.

=head1 LICENSE

This program is free software; you can redistribute it and/or modify it under the same terms as Perl itself.

=cut

__DATA__
