sub scan_file
{
	my $f = shift || die;

	$namespace = "";
	$className = "";

	open(IN, $f);
	while (<IN>)
	{
		chomp;
		$namespace = $1 if (m/^namespace (.*)$/);
		if (m/^    public.*? class (.*?)\<(.*?)>/)
		{
			die "Multiple public classes in $f" if ($className);
			$className = ($1 . "Of" . $2);
			next;
		}
		if (m/^    public.*? class (.*?)[ :]/)
		{
			die "Multiple public classes in $f" if ($className);
			$className = $1;
		}

	}
	close(IN);
}

sub scan_dir
{
	my $dir = shift || die;
	my $dir0 = shift;
	my $expected_namespace = shift || die;
	# print "Scanning $dir Namespace: $expected_namespace\n";
	opendir(DIR, $dir);
	my @files = readdir(DIR);
	closedir(DIR);

	for my $fileName (@files)
	{
		next if ($fileName =~ m/^\./);
		next if ($fileName =~ m/^\_/);
		my $fullname = "$dir/$fileName";

		if (-d $fullname)
		{
			scan_dir($fullname, ($dir0 ? "$dir0\\$fileName" : $fileName), "$expected_namespace.$fileName");
			next;
		}

		next if (!($fullname =~ m/.cs$/));

		scan_file($fullname);

		# if ($namespace ne "" && $expected_namespace ne $namespace)
		# { 	
		# 	print "$fullname: FIXNAMESPACE('$namespace','$expected_namespace')\n";
		# }

		if ($className ne "" && "$className.cs" ne $fileName)
		{
			$basedir0 = "$dir0\\";
			$basedir0 =~ s!^\\!!;
		
			print "call do_rename.bat $basedir0$fileName $basedir0$className.cs\n";
		}
	}
}

chdir("../src/NLog");
scan_dir(".", "", "NLog");
