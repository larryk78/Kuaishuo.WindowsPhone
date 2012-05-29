#!perl
use lib '../lib';
use strict;
use warnings;

$|++;

use Lingua::ZH::CC_CEDICT;

# http://www.mdbg.net/chindict/chindict.php?page=cc-cedict
# http://www.mdbg.net/chindict/export/cedict/cedict_1_0_ts_utf-8_mdbg.zip

die "Usage: perl $0 <path/to/cedict_ts.u8>\n" if !@ARGV;

my $dict = Lingua::ZH::CC_CEDICT->new(path => $ARGV[0]);

__END__
