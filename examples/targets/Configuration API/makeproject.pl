my $targetName = shift || die;
my $variantName = shift || die;
my $projectGuid = `uuidgen -c`;
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
        print OUT;
    }
    close(OUT);
    close(IN);
}

mkdir("$targetName",0666);
mkdir("$targetName/$variantName",0666);
apply_template("Template.sln", "$targetName/$variantName/${targetName}.${variantName}.sln");
apply_template("Template.csproj", "$targetName/$variantName/${targetName}.${variantName}.csproj");
