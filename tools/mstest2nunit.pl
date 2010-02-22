# 
# Copyright (c) 2004-2010 Jaroslaw Kowalski <jaak@jkowalski.net>
# 
# All rights reserved.
# 
# Redistribution and use in source and binary forms, with or without 
# modification, are permitted provided that the following conditions 
# are met:
# 
# * Redistributions of source code must retain the above copyright notice, 
#   this list of conditions and the following disclaimer. 
# 
# * Redistributions in binary form must reproduce the above copyright notice,
#   this list of conditions and the following disclaimer in the documentation
#   and/or other materials provided with the distribution. 
# 
# * Neither the name of Jaroslaw Kowalski nor the names of its 
#   contributors may be used to endorse or promote products derived from this
#   software without specific prior written permission. 
# 
# THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
# AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
# IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
# ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
# LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
# CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
# SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
# INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
# CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
# ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
# THE POSSIBILITY OF SUCH DAMAGE.
# 

# this script copies source files from one directory and replaces common MSTest calls
# with their NUnit counterparts

$searchfor = "using Microsoft.VisualStudio.TestTools.UnitTesting;";

$replacement = q!using NUnit.Framework;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestContext = System.Object;
using TestProperty = NUnit.Framework.PropertyAttribute;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;!;

sub process_file
{
	my $infile = shift || die;
	my $outfile = shift || die;

	open(IN, "$infile") || die;
	open(OUT, ">$outfile") || die;
	while (<IN>)
	{
		s/$searchfor/$replacement/g;
		s/Assert.IsInstanceOfType\((.*), (.*)\);/Assert.IsInstanceOfType($2, $1);/g;
		print OUT;
	}
	close(OUT);
	close(IN);
}

sub process_dir
{
	my $indir = shift || die;
	my $outdir = shift || die;

	mkdir($outdir, 0755);

	opendir(DIR, $indir);
	my @files = readdir(DIR);
	closedir(DIR);

	for (@files)
	{
		next if m/^\./;

		$infile = "$indir/$_";
		$outfile = "$outdir/$_";

		if (-d $infile)
		{
			process_dir($infile, $outfile);
			next;
		}

		process_file($infile, $outfile);
	}
}

$indir = shift || die;
$outdir = shift || die;

process_dir($indir, $outdir);

