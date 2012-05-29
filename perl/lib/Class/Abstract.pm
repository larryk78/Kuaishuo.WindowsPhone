# Class/Abstract.pm
#
# Copyright (c) 2006-2011 Larry Knibb. All rights reserved.
#

package Class::Abstract;
use strict;

use Carp;
use Class::AbstractMethodMaker qw(_new);

sub new {
    my $proto = shift;
    my $class = ref($proto) || $proto;

    my(undef, $file, $line) = caller;
    no strict qw(refs);
    if ($class eq __PACKAGE__ or grep $_ eq __PACKAGE__, @{"${class}::ISA"}) {
        croak "Don't instantiate abstract class $class directly at $file line $line.\n";
    }

    return $proto->_new(@_);
}

1;

=pod

=head1 NAME

Class::Abstract - base class for abstract base classes.

=head1 SYNOPSIS

 {
     package MyAbstractBaseClass;
     use base qw(Class::Abstract); # package becomes an abstract class

     use Class::AbstractMethodMaker qw(method1 method2 ...); # optional

     sub _new {
         my $proto = shift;
         my $self = bless {}, ref($proto) || $proto;
         #...
         return $self;
     }

     package MyAbstractBaseClass::TheirConcreteImplementation;
     use base qw(MyAbstractBaseClass);

     sub method1 {
         #...
     }

     sub method2 {
         #...
     }
 }

 my $foo = MyAbstractBaseClass->new(); # dies
 my $bar = MyAbstractBaseClass::TheirConcreteImplementation->new(); # ok

=head1 DESCRIPTION

If you are implementing an abstract base class (e.g. an interface), you have
probably done something like this in your base class constructor:

 croak "Don't instantiate me ($class) directly" if $class eq __PACKAGE__;

This class provides an alternative convention for defining abstract base
classes so you don't have to write that line of code any more.

=head2 use base qw(Class::Abstract);

This instruction sets L<Class::Abstract> as the base class of your class.

=head2 Default Constructor new()

Clients will call new() as usual, but the implementation of new() comes from
this class. It will check that the desired object isn't an abstract type. If
successful, your class' _new() constructor will be called.

=head1 METHODS

=head2 new()

Proxies calls to _new() in the subclass, checking first that the subclass is
not being directly instantiated.

=head2 _new()

This abstract method is called by the constructor. It allows (or forces) you
to provide an object initialiser in the standard fashion. Remember that you
should use the extended constructor form as would be expected for a class
that is to be implemented (see SYNOPSIS).

=head1 COPYRIGHT

Copyright (c) 2011 L<Larry Knibb|mailto://larry.knibb@gmail.com>. All rights
reserved.

=head1 LICENSE

This program is free software; you can redistribute it and/or modify it under the same terms as Perl itself.

=cut
