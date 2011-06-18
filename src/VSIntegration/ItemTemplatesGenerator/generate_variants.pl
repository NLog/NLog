#
# generate item templates for all Visual Studio languages and configurations
# templates are stored in *NLogConfig directories and for each one
# we generate a version for C#/VB and Web/non-Web scenarios
#

sub replace_in_file
{
    my $file = shift || die;
    my $lookfor = shift || die;
    my $replace = shift;

    open(IN, "<$file") || die;
    open(OUT, ">$file.tmp") || die;
    while (<IN>)
    {
        if (s/$lookfor/$replace/g)
        {
            print;
        }

        print OUT;
    }
    close(OUT);
    close(IN);
    unlink($file);
    rename("$file.tmp", $file);
}

@projectTypes = ('Empty','File','Console','LogReceiver','NLogViewer');
@languages = ('CSharp', 'VisualBasic');

for $projectType (@projectTypes)
{
    for $language (@languages)
    {
        system("rd /s /q tmp");
        mkdir("tmp");
        system("xcopy /s ${projectType}NLogConfig tmp > nul");
        replace_in_file("tmp/MyTemplate.vstemplate", "\\\$projecttype\\\$", $language);
        replace_in_file("tmp/MyTemplate.vstemplate", "\\\$projectsubtype\\\$", "");
        system("cd tmp && zip -X ../../ItemTemplates/${language}${projectType}NLogConfig.zip *");

        system("rd /s /q tmp");
        mkdir("tmp");
        system("xcopy /s ${projectType}NLogConfig tmp > nul");
        replace_in_file("tmp/MyTemplate.vstemplate", "\\\$projecttype\\\$", "Web");
        replace_in_file("tmp/MyTemplate.vstemplate", "\\\$projectsubtype\\\$", $language);
        system("cd tmp && zip -X ../../ItemTemplates/Web${language}${projectType}NLogConfig.zip *");
    }
}
system("rd /s /q tmp");
