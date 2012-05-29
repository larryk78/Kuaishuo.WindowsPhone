# Lingua/ZH/CC_CEDICT/Handler.pm
#
# Copyright (c) 2011 Larry Knibb. All rights reserved.
#

package Lingua::ZH::CC_CEDICT::Handler;
use base qw(Class::Abstract);
use strict;

use Class::AbstractMethodMaker qw(preprocess acceptRecord postprocess);

sub _new {
    my $class = shift;
    return bless {}, ref($class) || $class;
}

1;

=pod

=head1 NAME

Lingua::ZH::CC_CEDICT::Handler - Lingua::ZH::CC_CEDICT handler base class.

=head1 SYNOPSIS

 package MyHandler;
 use base qw(Lingua::ZH::CC_CEDICT::Handler);
 use strict;

 sub preprocess {
     my($self, $dictionary) = @_;
     # initialise your handler
 }

 sub acceptRecord {
     my($self, $record) = @_;
     # do something with $record
 }

 sub postprocess {
     my $self = shift;
     # finalise your handler
 }

 1;

=head1 DESCRIPTION

This class defines the L<record|Lingua::ZH::CC_CEDICT::Record> handler
interface for working with the L<CC-CEDICT|Lingua::ZH::CC_CEDICT> dictionary
reader.

To create your own handler you must subclass this package. The basic overview
is covered in the SYNOPSIS above and the details of

=head1 METHODS

=head2 new()

Creates the Lingua::ZH::CC_CEDICT::Handler object.

=head1 SEE ALSO

L<bar>

=head1 COPYRIGHT

Copyright (c) 2011 L<Larry Knibb|mailto://larry.knibb@gmail.com>. All rights
reserved.

=head1 LICENSE

This program is free software; you can redistribute it and/or modify it under the same terms as Perl itself.

=cut
