my $targetName = shift || die;
my $variantName = shift;
my $projectGuid = `uuidgen -c`;
my $projectName = "$targetName.$variantName";
my $projectDir = "$targetName/$variantName";

if ($variantName eq "")
{
    $projectDir = "$targetName/Simple";
    $projectName = "$targetName";
}
chomp $projectGuid;

sub apply_template
{
    my $src = shift || die;
    my $dst = shift || die;

    open(IN, "<$src") || die;
    open(OUT, ">$dst") || die;
    while (<IN>)
    {
        s/_TARGET_/$targetName/g;
        s/_VARIANT_/$variantName/g;
        s/_PROJECT_GUID_/$projectGuid/g;
        s/_PROJECT_NAME_/$projectName/g;
        s/_PROJECT_DIR_/$projectDir/g;
        print OUT;
    }
    close(OUT);
    close(IN);
}

mkdir("$targetName",0666);
mkdir("$projectDir",0666);
apply_template("Template.sln", "$projectDir/$projectName.sln");
apply_template("Template.csproj", "$projectDir/$projectName.csproj");
