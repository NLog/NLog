use strict;
use warnings;

# get the name of the desired output file
my $outputFilePath = $ARGV[0];

# get the SHA for 'master'
my $masterCommit = `git rev-list -n 1 master`;
chomp $masterCommit; # remove newline at end of output
print "'master' is " . $masterCommit . "\n";

# get space-separated SHAs of the current commit plus all its parents
my $output = `git rev-list --parents -n 1 HEAD`;
chomp $output; # remove newline at end of output

# get SHAs into array
my @allCommits = split / /, $output;
print "Current commit is " . $allCommits[0] . "\n";

# check each parent commit
# TODO: somebody more familiar with perl should turn this "I'm a C programmer and can't bother to learn perl array slices" loop into something cleaner
my $i = 1; # skip current commit
while ($i < (scalar @allCommits))
{
  my $parentCommit = $allCommits[$i];
  chomp $parentCommit;
  
  if ($parentCommit eq $masterCommit)
  {
    print "Parent " . $parentCommit . " is same as master.\n";
    
    # When a parent is master, create the output file
    # (when no parent is master, do not create the output file so appveyor.yml can do simple cmd 'if exist' check to determine the result of this script)
    open(my $fh, '>', $outputFilePath) or die "Could not open file '$outputFilePath' $!";
    print $fh "Parent " . $parentCommit . " is same as master.";;
    close $fh;
  }
  else
  {
    print "Parent " . $parentCommit . " is not master.\n";
  }
  
  $i++;
}