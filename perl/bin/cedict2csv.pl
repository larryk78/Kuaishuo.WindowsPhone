#!perl
use lib '../lib';
use strict;
use warnings;

$|++;

use File::Basename;
use Lingua::ZH::CC_CEDICT;
use Lingua::ZH::CC_CEDICT::CSVHandler;

# http://www.mdbg.net/chindict/chindict.php?page=cc-cedict
# http://www.mdbg.net/chindict/export/cedict/cedict_1_0_ts_utf-8_mdbg.zip

die "Usage: perl ".basename($0)." <path/to/cedict_ts.u8>\n" if !@ARGV;

my $workdir = dirname($ARGV[0]);
chdir($workdir) or die "Couldn't chdir to $workdir: $!\n";

my $dict = Lingua::ZH::CC_CEDICT->new(
    path    => basename($ARGV[0]),
    handler => Lingua::ZH::CC_CEDICT::CSVHandler->new());

__END__
