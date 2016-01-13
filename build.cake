//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Debug");
var outputDirectory = Argument<string>("outputDirectory", "./artifacts");
var nugetDirectory = Argument<string>("nugetDirectory", "./nuget");
var samplesDirectory = Argument<string>("samplesDirectory", "./samples");
var dnxVersion = Argument<string>("dnxVersion", "1.0.0-rc1-update1");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var buildDir = Directory(outputDirectory);
var samplesDir = Directory(samplesDirectory);
var nugetDir = Directory(nugetDirectory);

// Define runtime
DNRuntime runtime = DNRuntime.Clr;
if(IsRunningOnUnix())
{
    runtime = DNRuntime.Mono;
}

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////
Task("Clean")
    .Does(() =>
{
    CleanDirectory(buildDir);
	CleanDirectory(samplesDir);
	CleanDirectory(nugetDir);
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("pack");

Task("pack")
	.IsDependentOn("uap10")
	.Does(() => 
{
	string[] frameworks = null;
	if(IsRunningOnUnix())
	{
		frameworks = new [] { "dnx451","dnxcore50" };
	}
	else
	{
		frameworks = new [] { "dnxcore50", "dnx451", "net35", "sl5", "uap10.0" };
	}

	DNUPackSettings packSettings = new DNUPackSettings()
	{
		Architecture = DNArchitecture.X64,
        Runtime = runtime,
        Version = dnxVersion,
	    Configurations = new[] { configuration },
	    Quiet = true
	};

	DNUPack("./src/NLog/project.json", packSettings);
});

Task("uap10")
	.ContinueOnError()
	.IsDependentOn("sl5")
    .WithCriteria(IsRunningOnWindows())
	.Does(() =>
{
    // Restore
    DNURestoreSettings restoreSettings = new DNURestoreSettings()
    {
        Architecture = DNArchitecture.X64,
        Runtime = runtime,
        Version = dnxVersion,
		Quiet = true
    };
	DNURestore(restoreSettings);

	// Build
	DNUBuildSettings dnuBuildSettings = new DNUBuildSettings
	{
        Architecture = DNArchitecture.X64,
        Runtime = runtime,
        Version = dnxVersion,
	    Frameworks = new [] { "uap10.0" },
	    Configurations = new[] { configuration },
	    OutputDirectory = buildDir,
	    Quiet = true
	};
        
    DNUBuild("./src/NLog", dnuBuildSettings);
    DNUBuild("./src/NLogAutoLoadExtension", dnuBuildSettings);
	DNUBuild("./tests/SampleExtensions", dnuBuildSettings);
	
    dnuBuildSettings = new DNUBuildSettings
	{
        Architecture = DNArchitecture.X64,
        Runtime = runtime,
        Version = dnxVersion,
	    Frameworks = new [] { "uap10.0" },
	    Configurations = new[] { configuration },
	    OutputDirectory = buildDir,
	    Quiet = true
	};
    DNUBuild("./tests/NLog.UnitTests", dnuBuildSettings);

	// Test
	var settings = new DNXRunSettings
	{ 
        Architecture = DNArchitecture.X64,
        Runtime = runtime,
        Version = dnxVersion	
    };
	DNXRun("./tests/NLog.UnitTests/", "test", settings);
	CopyFileToDirectory("./tests/NLog.UnitTests/testresults.xml", buildDir + Directory(configuration) + Directory("uap10.0"));
});
   
Task("sl5")
	.ContinueOnError()
	.IsDependentOn("net35")
    .WithCriteria(IsRunningOnWindows())
	.Does(() =>
{
    // Restore
    DNURestoreSettings restoreSettings = new DNURestoreSettings()
    {
        Architecture = DNArchitecture.X64,
        Runtime = runtime,
        Version = dnxVersion,
		Quiet = true
    };
	DNURestore(restoreSettings);

	// Build
	DNUBuildSettings dnuBuildSettings = new DNUBuildSettings
	{
        Architecture = DNArchitecture.X64,
        Runtime = runtime,
        Version = dnxVersion,
	    Frameworks = new [] { "sl5" },
	    Configurations = new[] { configuration },
	    OutputDirectory = buildDir,
	    Quiet = true
	};
        
    DNUBuild("./src/NLog", dnuBuildSettings);
    DNUBuild("./src/NLogAutoLoadExtension", dnuBuildSettings);
	DNUBuild("./tests/SampleExtensions", dnuBuildSettings);
	
    dnuBuildSettings = new DNUBuildSettings
	{
        Architecture = DNArchitecture.X64,
        Runtime = runtime,
        Version = dnxVersion,
	    Frameworks = new [] { "sl5" },
	    Configurations = new[] { configuration },
	    OutputDirectory = buildDir,
	    Quiet = true
	};
    DNUBuild("./tests/NLog.UnitTests", dnuBuildSettings);

	// Test
	var settings = new DNXRunSettings
	{ 
        Architecture = DNArchitecture.X64,
        Runtime = runtime,
        Version = dnxVersion	
    };
	DNXRun("./tests/NLog.UnitTests/", "test", settings);
	CopyFileToDirectory("./tests/NLog.UnitTests/testresults.xml", buildDir + Directory(configuration) + Directory("sl5"));
});

Task("net35")
	.ContinueOnError()
	.IsDependentOn("Dnx451")
    .WithCriteria(IsRunningOnWindows())
	.Does(() =>
{
    // Restore
    DNURestoreSettings restoreSettings = new DNURestoreSettings()
    {
        Architecture = DNArchitecture.X64,
        Runtime = runtime,
        Version = dnxVersion,
		Quiet = true
    };
	DNURestore(restoreSettings);

	// Build
	DNUBuildSettings dnuBuildSettings = new DNUBuildSettings
	{
        Architecture = DNArchitecture.X64,
        Runtime = runtime,
        Version = dnxVersion,
	    Frameworks = new [] { "net35" },
	    Configurations = new[] { configuration },
	    OutputDirectory = buildDir,
	    Quiet = true
	};
        
    DNUBuild("./src/NLog", dnuBuildSettings);
    DNUBuild("./src/NLogAutoLoadExtension", dnuBuildSettings);
	DNUBuild("./tests/SampleExtensions", dnuBuildSettings);
	
    dnuBuildSettings = new DNUBuildSettings
	{
        Architecture = DNArchitecture.X64,
        Runtime = runtime,
        Version = dnxVersion,
	    Frameworks = new [] { "dnx451" },
	    Configurations = new[] { configuration },
	    OutputDirectory = buildDir,
	    Quiet = true
	};
    DNUBuild("./tests/NLog.UnitTests", dnuBuildSettings);

	// Test
	var settings = new DNXRunSettings
	{ 
        Architecture = DNArchitecture.X64,
        Runtime = runtime,
        Version = dnxVersion	
    };
	DNXRun("./tests/NLog.UnitTests/", "test", settings);
	CopyFileToDirectory("./tests/NLog.UnitTests/testresults.xml", buildDir + Directory(configuration) + Directory("net35"));
});

Task("Dnx451")
	.ContinueOnError()
	.IsDependentOn("Dnxcore50")
	.Does(() =>
{
	
    DNURestoreSettings restoreSettings = new DNURestoreSettings()
    {
        Architecture = DNArchitecture.X64,
        Runtime = runtime,
        Version = dnxVersion,
		Quiet = true
    };

	// Build
	DNUBuildSettings dnuBuildSettings = new DNUBuildSettings
	{
        Architecture = DNArchitecture.X64,
        Runtime = runtime,
        Version = dnxVersion,
	    Frameworks = new [] { "dnx451" },
	    Configurations = new[] { configuration },
	    OutputDirectory = buildDir,
	    Quiet = true
	};
    
	// Restore & build NLog
	DNURestore("./src/NLog/project.json", restoreSettings);
    DNUBuild("./src/NLog", dnuBuildSettings);
	DNURestore("./src/NLog.Extended/project.json", restoreSettings);
	DNUBuild("./src/NLog.Extended", dnuBuildSettings);
	DNURestore("./src/NLogAutoLoadExtension/project.json", restoreSettings);
    DNUBuild("./src/NLogAutoLoadExtension", dnuBuildSettings);

	DNURestore("./tests/SampleExtensions/project.json", restoreSettings);
	DNUBuild("./tests/SampleExtensions", dnuBuildSettings);
	DNURestore("./tests/NLog.UnitTests/project.json", restoreSettings);
	DNUBuild("./tests/NLog.UnitTests", dnuBuildSettings);

	// Test
	var settings = new DNXRunSettings
	{	
        Architecture = DNArchitecture.X64,
        Runtime = runtime,
        Version = dnxVersion
    };
	DNXRun("./tests/NLog.UnitTests/", "test", settings);
	CopyFileToDirectory("./tests/NLog.UnitTests/testresults.xml", buildDir + Directory(configuration) + Directory("dnx451"));
});

Task("Dnxcore50")
    .ContinueOnError()
	.IsDependentOn("Clean")
	.Does(() =>
{
	// Restore
    DNURestoreSettings restoreSettings = new DNURestoreSettings()
    {
        Architecture = DNArchitecture.X64,
        Runtime = DNRuntime.CoreClr,
        Version = dnxVersion,
		Quiet = true
    };
	DNURestore("./src/NLog/project.json", restoreSettings);
	
	// Build
	DNUBuildSettings dnuBuildSettings = new DNUBuildSettings
	{
        Architecture = DNArchitecture.X64,
        Runtime = DNRuntime.CoreClr,
        Version = dnxVersion,
	    Frameworks = new [] { "dnxcore50" },
	    Configurations = new[] { configuration },
	    OutputDirectory = buildDir,
	    Quiet = true
	};
        
    DNUBuild("./src/NLog", dnuBuildSettings);
	
	DNURestore("./tests/SampleExtensions/project.json", restoreSettings);
	DNURestore("./tests/NLog.UnitTests/project.json", restoreSettings);
	
	DNUBuild("./tests/SampleExtensions", dnuBuildSettings);
	DNUBuild("./tests/NLog.UnitTests", dnuBuildSettings);

	// Test
	var settings = new DNXRunSettings
	{	
        Architecture = DNArchitecture.X64,
        Runtime = DNRuntime.CoreClr,
        Version = dnxVersion
    };
	DNXRun("./tests/NLog.UnitTests/", "test", settings);

	CopyFileToDirectory("./tests/NLog.UnitTests/testresults.xml", buildDir + Directory(configuration) + Directory("dnxcore50"));
});
//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
