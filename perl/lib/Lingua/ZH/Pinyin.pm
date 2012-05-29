# Lingua/ZH/Pinyin.pm
#
# Copyright (c) 2011 Larry Knibb. All rights reserved.
#

package Lingua::ZH::Pinyin;
use strict;

use Carp;

sub new {
    my $class = shift;
    my $self = bless {}, $class;
    return $self->_init(@_);
}

sub _init {
    my $self = shift;
    my $pinyin = shift;
    if ($pinyin =~ /^\D+[1-5]$/) {
        $self->{tone} = chop($pinyin);
        $pinyin =~ s/u:/v/g;
        $self->{syllable} = $pinyin;
    } elsif ($pinyin =~ /^[a-zA-Z]$/) { # terms like T-shirt (T xu4)
        $self->{syllable} = $pinyin;    # use single-character syllables
        $self->{tone} = undef;          # with tone omitted
    } else {
        return undef; # invalid Pinyin
    }
    return $self;
}

sub syllable {
    my $self = shift;
    return $self->{syllable};
}

sub tone {
    my $self = shift;
    return $self->{tone};
}

sub setHanzi {
    my $self = shift;
    $self->{trad} = shift;
    $self->{simple} = shift;
}

sub traditionalHanzi {
    my $self = shift;
    return $self->{trad};
}

sub simplifiedHanzi {
    my $self = shift;
    return $self->{simple};
}

sub markup   {
    my $self = shift;
    return $self->{markup} ||= $self->_markup();
}

our $I = qr/(?:[bcdfghjklmnpqrstwxyz]|[csz]h)/i; # I = pinyin Initial
our $V = qr/[aeiouv]/i;                          # V = pinyin Vowel
our $P = qr/[aeo]/i;                             # P = Priority vowel
our %fancy = (
    'a' => [ "a",      "\x{101}", "\x{e1}",  "\x{1ce}", "\x{e0}"  ],
    'A' => [ "A",      "\x{100}", "\x{c1}",  "\x{1cd}", "\x{c0}"  ],
    'e' => [ "e",      "\x{113}", "\x{e9}",  "\x{11b}", "\x{e8}"  ],
    'E' => [ "E",      "\x{112}", "\x{c9}",  "\x{11a}", "\x{c8}"  ],
    'i' => [ "i",      "\x{12b}", "\x{ed}",  "\x{1d0}", "\x{ec}"  ],
    'I' => [ "I",      "\x{12a}", "\x{cd}",  "\x{1cf}", "\x{cc}"  ],
    'o' => [ "o",      "\x{14d}", "\x{f3}",  "\x{1d2}", "\x{f2}"  ],
    'O' => [ "O",      "\x{14c}", "\x{d3}",  "\x{1d1}", "\x{d2}"  ],
    'u' => [ "u",      "\x{16b}", "\x{fa}",  "\x{1d4}", "\x{f9}"  ],
    'U' => [ "U",      "\x{16a}", "\x{da}",  "\x{1d3}", "\x{d9}"  ],
    'v' => [ "\x{fc}", "\x{1d6}", "\x{1d8}", "\x{1da}", "\x{1dc}" ],
    'V' => [ "\x{fc}", "\x{1d5}", "\x{1d7}", "\x{1d9}", "\x{1db}" ]
);

sub _markup {
    my $self = shift;
    my $syllable = $self->syllable();
    my $tone = $self->tone();
    if (!defined $tone) { # drop through to return
    } elsif ($syllable =~ s/^($I)($V)(?!$V)/$1$fancy{$2}[$tone%5]/ ) { # single vowel
    } elsif ($syllable =~ s/^($V)(?!$V)/$fancy{$1}[$tone%5]/       ) { # single vowel, no initial
    } elsif ($syllable =~ s/^($I)($P)(?=$V)/$1$fancy{$2}[$tone%5]/ ) { # multi vowel (first is Priority)
    } elsif ($syllable =~ s/^($I?$V)($V)/$1$fancy{$2}[$tone%5]/    ) { # multi vowel (second)
    } else {
        $syllable .= $tone; # just append the tone
    }
    return $syllable;
}

1;

=pod

=head1 NAME

Lingua::ZH::Pinyin - foo.

=head1 SYNOPSIS

 use Lingua::ZH::Pinyin;

 my $foo = Lingua::ZH::Pinyin->new();

 # whatever

=head1 DESCRIPTION

foo

=head1 METHODS

=head2 new()

Creates the Lingua::ZH::Pinyin object.

=head1 SEE ALSO

L<bar>

=head1 COPYRIGHT

Copyright (c) 2011 L<Larry Knibb|mailto://larry.knibb@gmail.com>. All rights
reserved.

=head1 LICENSE

This program is free software; you can redistribute it and/or modify it under the same terms as Perl itself.

=cut
