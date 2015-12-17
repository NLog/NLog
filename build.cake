//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

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
	if(!IsRunningOnWindows())
		frameworks = new[] { "dnx451", "dnxcore50" };
	else
		frameworks = new[] { ".NETFramework,Version=v3.5", "dnx451", "dnxcore50", "uap10.0", "Silverlight,Version=v5.0" };

	foreach(var framework in frameworks)
	{
		var settings = new DNUBuildSettings
		{
		    Frameworks = new [] { framework },
		    Configurations = new[] { "Debug" },
		    OutputDirectory = "./artifacts/",
		    Quiet = true
		};
		DNUBuild("./src/**/project.json", settings);
	}
});

Task("Test")
	.IsDependentOn("Build")
	.Does(() =>
{
	DNVMUse(new DNVMSettings(){ Version = "default", Arch = "x64", Runtime = "coreclr"});
	var settings = new DNXRunSettings
	{
		Framework = "dnxcore50"
	};
	DNXRun("./tests/NLog.UnitTests/", "test", settings);

	DNVMUse(new DNVMSettings(){ Version = "default", Arch = "x64", Runtime = "clr"});
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
