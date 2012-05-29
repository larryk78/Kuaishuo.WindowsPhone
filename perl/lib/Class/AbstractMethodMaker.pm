# Class/AbstractMethodMaker.pm
#
# Copyright (c) 2006 Larry Knibb. All rights reserved.
#

package Class::AbstractMethodMaker;
use strict;

use Carp;

sub import {
    my $class = shift;
    my($package) = caller;
    no strict qw(refs);
    for my $method (@_) {
        *{"${package}::$method"} = sub {
            my $class = ref($_[0]) || $_[0];
            my(undef, $file, $line) = caller;
            croak "Package $package declares the abstract method $method, not implemented in $class, called at $file line $line.\n";
        };
    }
}

1;

=pod

=head1 NAME

Class::AbstractMethodMaker - automated abstract method creation.

=head1 SYNOPSIS

 package Foo;
 use strict;

 use Class::AbstractMethodMaker qw(Bar);

 sub new {
     bless {}, ref($_[0]) || $_[0];
 }

 1;

 package Foo::Impl;
 use base qw(Foo);
 use strict;

 #sub Bar {
 #    print "Bar called\n";
 #}

 1;

 package main;
 use strict;
 use warnings;

 my $foo = Foo::Impl->new();

 $foo->Bar(); # dies with an error

=head1 DESCRIPTION

This class can inject "abstract" (at runtime) methods into the namespace
of the calling class. Any derived classes must then implement the abstract
methods lest any calls to them die with a helpful message (for developers).

=head1 METHODS

=head2 import()

Called automatically by perl when you I<use> this class with parameters.
LIST parameters to I<use> (see L<use|perldoc -f use>) are translated into
abstract method declarations, the definitions of which are mandated to be
present in a subclass (at runtime).

=head1 CAVEAT

This only works for class/instance methods as it uses the first argument
to get the classname to display in the error. If you are simply using
inheritance to override plain functions then this is not the class for you.

=head1 COPYRIGHT

Copyright (c) 2011 L<Larry Knibb|mailto://larry.knibb@gmail.com>. All rights
reserved.

=head1 LICENSE

This program is free software; you can redistribute it and/or modify it under the same terms as Perl itself.

=cut
