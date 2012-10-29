# Lingua/ZH/CC_CEDICT/CSVHandler.pm
#
# Copyright (c) 2012 Larry Knibb. All rights reserved.
#

package Lingua::ZH::CC_CEDICT::CSVHandler;
use base qw(Lingua::ZH::CC_CEDICT::Handler);
use strict;

use Carp;
use Data::Dumper;
use Encode;
use Lingua::ZH::CC_CEDICT::EN::Analyser;

sub preprocess {
    my $self = shift;
    my $path = shift;

    my $time = time();
    $self->{en} = Lingua::ZH::CC_CEDICT::EN::Analyser->new();
    #$self->{zh} = Lingua::ZH::Analyser->new();
    $self->{count} = 0;
}

sub acceptRecord {
    my $self = shift;
    my $record = shift; # Lingua::ZH::CC_CEDICT::Record

    print "Processing...$record->{index}\r";
    $self->{count}++;

    $self->_addChineseLookups($record);
    $self->_addEnglishLookups($record);
}

sub insert {
    my $self = shift;
    my($name, $key, $index, $relevance) = @_;
    $key = encode('UTF-8', lc($key));

    if (!exists $self->{index}{$name}) {
        $self->{index}{$name} = { $key => { $index => $relevance } };
    } elsif (!exists $self->{index}{$name}{$key}) {
        $self->{index}{$name}{$key} = { $index => $relevance };
    } elsif (!exists $self->{index}{$name}{$key}{$index} || $self->{index}{$name}{$key}{$index} < $relevance) {
        $self->{index}{$name}{$key}{$index} = $relevance;
    }
}

sub _addChineseLookups {
    my $self = shift;
    my $record = shift;

    my @chinese = $record->chinese();
    my $simplified = '';
    my $traditional = '';
    my $relevance = int(100 / scalar(@chinese));

    for my $pinyin (@chinese) {

        # insert simplified
        my $hs = $pinyin->simplifiedHanzi();
        $self->insert('hanzi', $hs, $record->{index}, $relevance);
        $simplified .= $hs;

        # insert traditional
        my $ht = $pinyin->traditionalHanzi();
        $self->insert('hanzi', $ht, $record->{index}, $relevance);
        $traditional .= $ht;

        # insert pinyin
        my $ps = lc($pinyin->syllable());
        my $pt = $pinyin->tone();
        $self->insert('pinyin', $ps.$pt, $record->{index}, $relevance);
    }

    $self->insert('hanzi', $simplified, $record->{index}, 110);
    $self->insert('hanzi', $traditional, $record->{index}, 110);
}

sub _addEnglishLookups {
    my $self = shift;
    my $record = shift;

    my $pos = 0; # used for weighting analysis scores by phrase position

    for my $phrase ($record->english()) {

        for my $result ($self->{en}->analyse($phrase, 1-(0.05*$pos++))) {

            my $word = lc($result->[0]);
            my $score = $result->[1];

            # insert english
            $self->insert('english', $word, $record->{index}, $score);
        }
    }
}

sub postprocess {
    my $self = shift;

    print "Processing...DONE\n";

    for my $index (sort keys %{$self->{index}}) {

        print "Indexing $index...";
        open(FILE, ">$index.csv");
        binmode(FILE);

        $index = $self->{index}{$index};
        for my $key (sort keys %{$index}) {

            my @refs = map join('|', $_, $index->{$key}{$_}), sort {$a <=> $b} keys %{$index->{$key}};
            print FILE join(',', $key, @refs), "\n";
        }

        close(FILE);
        print "DONE\n";
    }

    print "Complete. ($self->{count} records in ".(time - $^T)."s.)\n";
}

1;

=pod

=head1 NAME

Lingua::ZH::CC_CEDICT::CSVHandler - foo.

=head1 SYNOPSIS

 use Lingua::ZH::CC_CEDICT::CSVHandler;

 my $foo = Lingua::ZH::CC_CEDICT::CSVHandler->new();

 # whatever

=head1 DESCRIPTION

foo

=head1 METHODS

=head2 new()

Creates the Lingua::ZH::CC_CEDICT::CSVHandler object.

=head1 SEE ALSO

L<bar>

=head1 COPYRIGHT

Copyright (c) 2011 L<Larry Knibb|mailto://larry.knibb@gmail.com>. All rights
reserved.

=head1 LICENSE

This program is free software; you can redistribute it and/or modify it under the same terms as Perl itself.

=cut
