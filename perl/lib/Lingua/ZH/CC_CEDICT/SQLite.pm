# Lingua/ZH/CC_CEDICT/SQLite.pm
#
# Copyright (c) 2011 Larry Knibb. All rights reserved.
#

package Lingua::ZH::CC_CEDICT::SQLite;
use strict;

use Carp;
use DBI;

sub new {
    my $class = shift;
    my $self = bless {}, $class;
    return $self->_init(@_);
}

sub _init {
    my $self = shift;
    my %args = @_;

    if ($args{noop}) {
        $self->{noop} = 1;
        return $self;
    }

    my $dbh = DBI->connect("dbi:SQLite:dbname=$args{path}", '', '', { sqlite_unicode => 1 });
    $dbh->do($_) or croak($dbh->errorstr()) for split(';', join('', <DATA>)); # see __DATA__

    $self->{query} = {
        insertDictionaryEntry   => $dbh->prepare('INSERT INTO dictionary VALUES (?, ?, ?, ?)'),
        insertEnglishSearchTerm => $dbh->prepare('INSERT INTO english VALUES (?)'),
        insertEnglishIndex      => $dbh->prepare('INSERT INTO english_index VALUES (?, ?, ?)'),
        insertHanziSearchTerm   => $dbh->prepare('INSERT INTO hanzi VALUES (?, ?)'),
        insertPinyinSearchTerm  => $dbh->prepare('INSERT INTO pinyin VALUES (?, ?)'),
        insertHanziPinyinIndex  => $dbh->prepare('INSERT INTO hp_index VALUES (?, ?, ?, ?)'),
    };

    $self->{dbh} = $dbh;
    $self->{count} = 0; # set-up counter to manage SQL TRANSACTION size
    return $self;
}

sub AUTOLOAD {
    my $self = shift;
    return 1 if $self->{noop};
    no strict qw(vars);
   (my $method = $AUTOLOAD) =~ s/^.*:://;
    if (defined $self->{query}{$method}) {
        if (!($self->{count}++%1000)) { # one TRANSACTION = 1000 INSERTs
            $self->{dbh}->commit();
            $self->{dbh}->do('BEGIN TRANSACTION;');
        }
        $self->{query}{$method}->execute(@_);
        return $self->{dbh}->last_insert_id('', '', '', ''); # assumes all queries are of type INSERT
    }
    croak "Error: $AUTOLOAD not implemented";
}

sub DESTROY {
    my $self = shift;
    return if $self->{noop};
    my $dbh = $self->{dbh};
    $dbh->disconnect();
}

sub createIndexes {
    my $self = shift;
    return if $self->{noop};
    my $dbh = $self->{dbh};
    $dbh->commit();
    $dbh->do('CREATE INDEX i1 ON english(phrase);');
    $dbh->do('CREATE INDEX i2 ON english_index(english_id, dictionary_id);');
    $dbh->do('CREATE INDEX i3 ON hanzi(traditional)');
    $dbh->do('CREATE INDEX i4 ON hanzi(simplified);');
    $dbh->do('CREATE INDEX i5 ON pinyin(syllable, tone);');
    $dbh->do('CREATE INDEX i6 ON hp_index(hanzi_id, dictionary_id);');
    $dbh->do('CREATE INDEX i7 ON hp_index(pinyin_id, dictionary_id);');
}

1;

=pod

=head1 NAME

Lingua::ZH::CC_CEDICT::SQLite - foo.

=head1 SYNOPSIS

 use Lingua::ZH::CC_CEDICT::SQLite;

 my $foo = Lingua::ZH::CC_CEDICT::SQLite->new();

 # whatever

=head1 DESCRIPTION

foo

=head1 METHODS

=head2 new()

Creates the Lingua::ZH::CC_CEDICT::SQLite object.

=head1 SEE ALSO

Database schema diagram - L<http://schemabank.com/a/ZXw89>

=head1 COPYRIGHT

Copyright (c) 2011 L<Larry Knibb|mailto://larry.knibb@gmail.com>. All rights
reserved.

=head1 LICENSE

This program is free software; you can redistribute it and/or modify it under the same terms as Perl itself.

=cut

__DATA__
BEGIN TRANSACTION;

DROP TABLE IF EXISTS dictionary;
DROP TABLE IF EXISTS english;
DROP TABLE IF EXISTS english_index;
DROP TABLE IF EXISTS hanzi;
DROP TABLE IF EXISTS pinyin;
DROP TABLE IF EXISTS hp_index;

PRAGMA foreign_keys = ON;
PRAGMA journal_mode = OFF;

CREATE TABLE dictionary (
    traditional TEXT NOT NULL,
    simplified TEXT NOT NULL,
    pinyin TEXT NOT NULL,
    english TEXT NOT NULL
);

CREATE TABLE english (
    phrase TEXT NOT NULL
);

CREATE TABLE english_index (
    english_id INT REFERENCES english(rowid),
    dictionary_id INT REFERENCES dictionary(rowid),
    relevance INT NOT NULL
);

CREATE TABLE hanzi (
    traditional TEXT,
    simplified TEXT
);

CREATE TABLE pinyin (
    syllable TEXT NOT NULL,
    tone INT
);

CREATE TABLE hp_index (
    hanzi_id INT REFERENCES hanzi(rowid),
    pinyin_id INT REFERENCES pinyin(rowid),
    dictionary_id INT REFERENCES dictionary(rowid),
    relevance INT NOT NULL
);

COMMIT;
