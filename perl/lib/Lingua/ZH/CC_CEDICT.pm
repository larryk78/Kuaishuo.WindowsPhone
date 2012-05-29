# Lingua/ZH/CC_CEDICT.pm
#
# Copyright (c) 2011 Larry Knibb. All rights reserved.
#

package Lingua::ZH::CC_CEDICT;
use strict;

use Carp;
use Lingua::ZH::CC_CEDICT::Record;
use Lingua::ZH::CC_CEDICT::SQLiteHandler;

sub new {
    my $class = shift;
    my $self = bless {}, $class;
    $self->_init(@_);
    return $self;
}

sub _init {
    my $self = shift;
    my %args = @_;
    if (ref($args{handler}) eq 'ARRAY') {
        $self->attachHandler($_) for @{$args{handler}};
    } else {
        $self->attachHandler($args{handler});
    }
    $self->load($args{path});
}

sub attachHandler {
    my $self = shift;
    my $handler = shift || return;
    my $class = ref($handler);
    no strict qw(refs);
    if (!grep /^Lingua::ZH::CC_CEDICT::Handler$/, @{"${class}::ISA"}) {
        croak "$handler ($class) is not a Lingua::ZH::CC_CEDICT::Handler";
    }
    push @{$self->{handlers}}, $handler;
}

sub setDefaultHandler {
    my $self = shift;
    $self->attachHandler(Lingua::ZH::CC_CEDICT::SQLiteHandler->new());
}

sub load {
    my $self = shift;
    my $path = shift || return;

    # prepare dictionary
    open(DICT, $path) or croak "Error: can't open dictionary at $path: $!\n";
    $_ = <DICT>;
    /^# CC-CEDICT$/ or croak "Error: not a valid CC-CEDICT dictionary at $path\n";
    binmode(DICT, ':encoding(utf8)');

    # make sure there's at least one handler attached and run pre-processing
    $self->setDefaultHandler() if !$self->{handlers};
    $_->preprocess($path) for @{$self->{handlers}};

    my $lineno = 1;
    my $index = 0;
    while (<DICT>) {

        $lineno++;
        next if /^#/; # ignore comments and headers

        m!^(\S+) (\S+) \[(.*?)\] /(.*)/$! or warn "Warning: invalid dictionary entry at line $lineno\n" and next;
        my $record = Lingua::ZH::CC_CEDICT::Record->new(
            lineno  => $lineno,
            index   => $index++,
            trad    => $1,
            simple  => $2,
            pinyin  => $3,
            english => $4 ) or next;

        # push record to handlers for processing
        $_->acceptRecord($record) for @{$self->{handlers}};
    }

    close(DICT);

    # run post-processing
    $_->postprocess() for @{$self->{handlers}};
}

1;

=pod

=head1 NAME

Lingua::ZH::CC_CEDICT - cedict_ts.u8 dictionary file reader.

=head1 SYNOPSIS

 use Lingua::ZH::CC_CEDICT;

 # 1: You're just getting started and haven't written your own handler yet

 my $dict = Lingua::ZH::CC_CEDICT->new(path => '/path/to/cedict_ts.u8');

 # 2: You've written your own handler :)

 my $dict = Lingua::ZH::CC_CEDICT->new(
     handler => MyHandler->new(),
     path    => '/path/to/cedict_ts.u8');

 # 3: You've written multiple handlers :):)

 my $dict = Lingua::ZH::CC_CEDICT->new(
     handler => [ MyHandler->new(), MyHandler2->new(), ... ],
     path    => '/path/to/cedict_ts.u8');

 # 4: You want to manage the steps explicitly

 my $dict = Lingua::ZH::CC_CEDICT->new();
 $dict->attachHandler(...); # zero or more times
 $dict->load('/path/to/cedict_ts.u8');

=head1 DESCRIPTION

This class provides a SAX-style interface to
L<CC-CEDICT|http://www.mdbg.net/chindict/chindict.php?page=cc-cedict>
dictionary files.

The way to use it is to create one or more
L<handler|Lingua::ZH::CC_CEDICT::Handler> classes to encapsulate your desired
dictionary-handling behaviour and attach those to the dictionary reader object.

Then, when the dictionary is read, a L<record|Lingua::ZH::CC_CEDICT::Record>
object is created for each dictionary entry and passed to all handler(s).

For more information about creating a handler, see
L<Lingua::ZH::CC_CEDICT::Handler>.

If no handler is specified, the default behaviour is to use the bundled
L<Lingua::ZH::CC_CEDICT::SQLiteHandler>. This generates a SQLite database
edition of the dictionary at F</path/to/cedict_ts.db>.

=head1 METHODS

=head2 new(%args)

Creates the Lingua::ZH::CC_CEDICT object.

Accepted %args are:

=over 4

=item path => $path

Optional. The relative or absolute location of the dictionary file on disk.

N.B. if you provide a C<path> argument then the dictionary will be loaded
immediately. If you do not, you can call C<$obj-E<gt>load($path)> later.

=item handler => $object or [ $object, $object2, ... ]

Optional. A L<Lingua::ZH::CC_CEDICT::Handler> derived object or an anonymous
array of objects of the same type (i.e. when you want multiple handlers).

N.B. if you do not provide a C<handler> argument you can call
C<$obj-E<gt>attachHandler($object)> later. If no handler is provided at all
the default handler is L<Lingua::ZH::CC_CEDICT::SQLiteHandler>.

=back

=head2 attachHandler(Lingua::ZH::CC_CEDICT::Handler $object)

Add $object to the list of handlers to be called for each record.

=head2 load($path)

If a I<path> argument was not provided to new(), or if you want to reload a
dictionary (or load a new dictionary), call C<$obj-E<gt>load($path)>.

=head1 SEE ALSO

CC-CEDICT homepage: L<http://www.mdbg.net/chindict/chindict.php?page=cc-cedict>

L<Lingua::ZH::CC_CEDICT::Handler>

L<Lingua::ZH::CC_CEDICT::Record>

L<Lingua::ZH::CC_CEDICT::SQLiteHandler>

=head1 COPYRIGHT

Copyright (c) 2011 L<Larry Knibb|mailto://larry.knibb@gmail.com>. All rights
reserved.

=head1 LICENSE

This program is free software; you can redistribute it and/or modify it under the same terms as Perl itself.

=cut
