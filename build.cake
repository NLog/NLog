//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Debug");
var outputDirectory = Argument<string>("outputDirectory", "./artifacts");
var samplesDirectory = Argument<string>("samplesDirectory", "./samples");
var dnxVersion = Argument<string>("dnxVersion", "1.0.0-rc1-update1");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var buildDir = Directory(outputDirectory) + Directory(configuration);
var samplesDir = Directory(samplesDirectory) + Directory(configuration);

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////
Task("Clean")
    .Does(() =>
{
    CleanDirectory(buildDir);
	CleanDirectory(samplesDir);
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Dotnet35");
    
Task("Dotnet35")
	.ContinueOnError()
	.IsDependentOn("Dnx451")
	.Does(() =>
{
	// Use
	DNVMUse(dnxVersion, new DNVMSettings(){ Arch = "x64", Runtime = "clr"});
	
	// Restore
	DNURestore();

	// Build
	DNUBuildSettings dnuBuildSettings = new DNUBuildSettings
	{
	    Frameworks = new [] { ".NETFramework,Version=v3.5" },
	    Configurations = new[] { configuration },
	    OutputDirectory = (buildDir + Directory("Dotnet35")).ToString(),
	    Quiet = true
	};
        
    DNUBuild("./src/NLog", dnuBuildSettings);
    DNUBuild("./src/NLogAutoLoadExtension", dnuBuildSettings);
	DNUBuild("./tests/SampleExtensions", dnuBuildSettings);
	DNUBuild("./tests/NLog.UnitTests", dnuBuildSettings);

	// Test
	var settings = new DNXRunSettings
	{
		Framework = ".NETFramework,Version=v3.5"
	};
	DNXRun("./tests/NLog.UnitTests/", "test", settings);

});

Task("Dnx451")
	.ContinueOnError()
	.IsDependentOn("Dnxcore50")
	.Does(() =>
{
	// Use
	DNVMUse(dnxVersion, new DNVMSettings(){ Arch = "x64", Runtime = "clr"});
	
	// Restore
	DNURestore();

	// Build
	DNUBuildSettings dnuBuildSettings = new DNUBuildSettings
	{
	    Frameworks = new [] { "dnx451" },
	    Configurations = new[] { configuration },
	    OutputDirectory = (buildDir + Directory("dnx451")).ToString(),
	    Quiet = true
	};
        
    DNUBuild("./src/NLog", dnuBuildSettings);
	DNUBuild("./src/NLog.Extended", dnuBuildSettings);
    DNUBuild("./src/NLogAutoLoadExtension", dnuBuildSettings);
	DNUBuild("./tests/SampleExtensions", dnuBuildSettings);
	DNUBuild("./tests/NLog.UnitTests", dnuBuildSettings);

	// Test
	var settings = new DNXRunSettings
	{
		Framework = "dnx451"
	};
	DNXRun("./tests/NLog.UnitTests/", "test", settings);

});

Task("Dnxcore50")
	.ContinueOnError()
	.IsDependentOn("Clean")
	.Does(() =>
{
	// Use
	DNVMUse(dnxVersion, new DNVMSettings(){ Arch = "x64", Runtime = "coreclr"});
	
	// Restore
	DNURestore();

	// Build
	DNUBuildSettings dnuBuildSettings = new DNUBuildSettings
	{
	    Frameworks = new [] { "dnxcore50" },
	    Configurations = new[] { configuration },
	    OutputDirectory = (buildDir + Directory("dnxcore50")).ToString(),
	    Quiet = true
	};
        
    DNUBuild("./src/NLog", dnuBuildSettings);
	DNUBuild("./tests/SampleExtensions", dnuBuildSettings);
	DNUBuild("./tests/NLog.UnitTests", dnuBuildSettings);

	// Test
	var settings = new DNXRunSettings
	{	};
	DNXRun("./tests/NLog.UnitTests/", "test", settings);

});
//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
