# Lingua/ZH/CC_CEDICT/SQLiteHandler.pm
#
# Copyright (c) 2011 Larry Knibb. All rights reserved.
#

package Lingua::ZH::CC_CEDICT::SQLiteHandler;
use base qw(Lingua::ZH::CC_CEDICT::Handler);
use strict;

use Carp;
use Lingua::ZH::CC_CEDICT::SQLite;
use Lingua::ZH::CC_CEDICT::EN::Analyser;

sub preprocess {
    my $self = shift;
    my $path = shift;

    my $time = time();
   ($self->{dbfile} = $path) =~ s/\.[^\.]+$/.$time.db/;
    print "Creating SQLite dictionary at $self->{dbfile}\n";
    $self->{db} = Lingua::ZH::CC_CEDICT::SQLite->new(path => $self->{dbfile}, noop => 0);
    $self->{en} = Lingua::ZH::CC_CEDICT::EN::Analyser->new();
    #$self->{zh} = Lingua::ZH::Analyser->new();
    $self->{count} = 0;
}

sub acceptRecord {
    my $self = shift;
    my $record = shift; # Lingua::ZH::CC_CEDICT::Record

    my $id = $self->_createDatabaseRecord($record);
             $self->_addChineseLookups($record, $id);
             $self->_addEnglishLookups($record, $id);

    $self->{count}++;
}

sub _createDatabaseRecord {
    my $self = shift;
    my $record = shift;

    return $self->{db}->insertDictionaryEntry(
        $record->traditionalHanzi(),
        $record->simplifiedHanzi(),
        $record->pinyin(),
        join('/', $record->english()));
}

sub _addChineseLookups {
    my $self = shift;
    my $record = shift;
    my $id = shift;

    my $temp = {}; # temp cache to prevent duplicates within this record
    my @chinese = $record->chinese();
    my $n = scalar @chinese;

    for my $pinyin (@chinese) {

        # hanzi strings
        my $ht = $pinyin->traditionalHanzi();
        my $hs = $pinyin->simplifiedHanzi();
        my $h  = $ht.$hs;

        # insert hanzi
        $self->{cache}{zh}{$h} ||= $self->{db}->insertHanziSearchTerm($ht, $hs);

        # pinyin strings
        my $ps = lc($pinyin->syllable());
        my $pt = $pinyin->tone();
        my $p  = $ps.$pt;

        # insert pinyin
        $self->{cache}{zh}{$p} ||= $self->{db}->insertPinyinSearchTerm($ps, $pt);

        # insert dictionary lookup
        $temp->{$h.$p} ||= $self->{db}->insertHanziPinyinIndex(
            $self->{cache}{zh}{$h},
            $self->{cache}{zh}{$p},
            $id,
            int(100/$n));
    }
}

sub _addEnglishLookups {
    my $self = shift;
    my $record = shift;
    my $id = shift;

    my $temp = {}; # temp cache to prevent duplicates within this record
    my $pos = 0; # used for weighting analysis scores by phrase position

    for my $phrase ($record->english()) {

        for my $result ($self->{en}->analyse($phrase, 1-(0.05*$pos++))) {

            my $word = lc($result->[0]);
            my $score = $result->[1];

            # insert english
            $self->{cache}{en}{$word} ||= $self->{db}->insertEnglishSearchTerm($word);

            # insert dictionary lookup
            $temp->{$word} ||= $self->{db}->insertEnglishIndex(
                $self->{cache}{en}{$word},
                $id,
                $score);
        }
    }
}

sub postprocess {
    my $self = shift;

    $self->{db}->createIndexes();
    print "Complete. ($self->{count} records in ".(time - $^T)."s.)\n";
}

1;

=pod

=head1 NAME

Lingua::ZH::CC_CEDICT::SQLiteHandler - foo.

=head1 SYNOPSIS

 use Lingua::ZH::CC_CEDICT::SQLiteHandler;

 my $foo = Lingua::ZH::CC_CEDICT::SQLiteHandler->new();

 # whatever

=head1 DESCRIPTION

foo

=head1 METHODS

=head2 new()

Creates the Lingua::ZH::CC_CEDICT::SQLiteHandler object.

=head1 SEE ALSO

L<bar>

=head1 COPYRIGHT

Copyright (c) 2011 L<Larry Knibb|mailto://larry.knibb@gmail.com>. All rights
reserved.

=head1 LICENSE

This program is free software; you can redistribute it and/or modify it under the same terms as Perl itself.

=cut
