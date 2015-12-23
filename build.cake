//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var outputDirectory = Argument("outputDirectory", "./artifacts");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var buildDir = Directory("./src/Example/bin") + Directory(configuration);

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory(buildDir);
});

Task("DnuRestore")
    .IsDependentOn("Clean")
    .Does(() =>
{
    DNURestore();
});

Task("Build")
    .IsDependentOn("DnuRestore")
    .Does(() =>
{
    string[] frameworks = null;
    if(IsRunningOnWindows())
        frameworks = new[] { ".NETFramework,Version=v3.5", "dnx451", "dnxcore50", "uap10.0", "Silverlight,Version=v5.0" };
    else
        frameworks = new[] { "dnx451", "dnxcore50" };
        
    DNUBuildSettings dnuBuildSettings = null;
    foreach(var framework in frameworks)
    {
        dnuBuildSettings = new DNUBuildSettings
		{
		    Frameworks = new [] { framework },
		    Configurations = new[] { configuration },
		    OutputDirectory = outputDirectory,
		    Quiet = true
		};
        
        DNUBuild("./src/NLog", dnuBuildSettings);
    }
    
    dnuBuildSettings = new DNUBuildSettings
		{
		    Frameworks = new [] { "dnx451" },
		    Configurations = new[] { configuration },
		    OutputDirectory = outputDirectory,
		    Quiet = true
		};
	DNUBuild("./src/NLog.Extended", dnuBuildSettings);
    DNUBuild("./src/NLogAutoLoadExtension", dnuBuildSettings);
    
    foreach(var framework in frameworks)
    {
        dnuBuildSettings = new DNUBuildSettings
		{
		    Frameworks = new [] { framework },
		    Configurations = new[] { configuration },
		    OutputDirectory = outputDirectory,
		    Quiet = true
		};
        
        DNUBuild("./tests/SampleExtensions", dnuBuildSettings);
        DNUBuild("./tests/NLog.UnitTests", dnuBuildSettings);
    }

});

Task("Test")
	.IsDependentOn("Build")
	.Does(() =>
{
	DNVMUse("default", new DNVMSettings(){ Arch = "x64", Runtime = "coreclr"});
	
	var settings = new DNXRunSettings
	{
		Framework = "dnxcore50"
	};
	DNXRun("./tests/NLog.UnitTests/", "test", settings);

	DNVMUse("default", new DNVMSettings() { Arch = "x64", Runtime = "clr"});
	settings = new DNXRunSettings
	{
		Framework = "dnx451"
	};
	DNXRun("./tests/NLog.UnitTests/", "test", settings);
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Test");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
