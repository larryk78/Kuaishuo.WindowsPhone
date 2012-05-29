# Lingua/ZH/CC_CEDICT/Record.pm
#
# Copyright (c) 2011 Larry Knibb. All rights reserved.
#

package Lingua::ZH::CC_CEDICT::Record;
use strict;

use Carp;
use Lingua::ZH::Pinyin;

sub new {
    my $class = shift;
    my $self = bless {}, $class;
    return $self->_init(@_);
}

sub _init {
    my $self = shift;
    my %args = @_;

    # copy raw dictionary data
    $self->{_lineno} = $args{lineno};
    $self->{index} = $args{index};
    $self->{trad} = $args{trad};
    $self->{simple} = $args{simple};
    $self->{pinyin} = $args{pinyin};
    $self->{english} = [ split('/', $args{english}) ];

    # enrich Chinese data so it's more useful later
    return $self->_processChinese() ? $self : undef; # bail if this fails
}

sub _processChinese {
    my $self = shift;

    # break up Hanzi/Pinyin into individual characters/syllables (ignore punct.)
    my @hanzi_t = split /[ \x{ff0c}\x{30fb}]*/, $self->{trad};   # \x{ff0c}: Unicode comma
    my @hanzi_s = split /[ \x{ff0c}\x{30fb}]*/, $self->{simple}; # \x{30fb}: Unicode middle-dot
    my @pinyin  = split /[ ,\x{b7}]+/,          $self->{pinyin}; # \x{b7}  : regular middle dot

    # array lengths must match or treat this as a bad record and bail
    if (@hanzi_t != @hanzi_s or @hanzi_s != @pinyin) {
        warn "Warning: non-matching Hanzi-Pinyin at line $self->{_lineno}\n";
        return 0; # failed
    }

    # objectify Pinyin
    my $i = 0;
    $self->{chinese} = [];
    for my $token (@pinyin) {
        my $pinyin = Lingua::ZH::Pinyin->new($token) or
            warn "Warning: invalid Pinyin '$token' at line ".$self->{_lineno} and return 0; # failed
        $pinyin->setHanzi($hanzi_t[$i], $hanzi_s[$i++]); # merge Hanzi
        $self->{pinyin} =~ s/$token/$pinyin->markup()/e; # markup Pinyin
        push @{$self->{chinese}}, $pinyin;
    }

    return 1; # success
}

sub traditionalHanzi {
    my $self = shift;
    return $self->{trad}; # raw data from the dictionary
}

sub simplifiedHanzi {
    my $self = shift;
    return $self->{simple}; # raw data from the dictionary
}

sub pinyin {
    my $self = shift;
    return $self->{pinyin}; # raw data from the dictionary
}

sub chinese {
    my $self = shift;
    return @_ ? $self->{chinese}[$_[0]] : @{$self->{chinese}};
}

sub english {
    my $self = shift;
    return @_ ? $self->{english}[$_[0]] : @{$self->{english}};
}

1;

=pod

=head1 NAME

Lingua::ZH::CC_CEDICT::Record - foo.

=head1 SYNOPSIS

 use Lingua::ZH::CC_CEDICT::Record;

 my $foo = Lingua::ZH::CC_CEDICT::Record->new();

 # whatever

=head1 DESCRIPTION

foo

=head1 METHODS

=head2 new()

Creates the Lingua::ZH::CC_CEDICT::Record object.

=head1 SEE ALSO

L<bar>

=head1 COPYRIGHT

Copyright (c) 2011 L<Larry Knibb|mailto://larry.knibb@gmail.com>. All rights
reserved.

=head1 LICENSE

This program is free software; you can redistribute it and/or modify it under the same terms as Perl itself.

=cut
