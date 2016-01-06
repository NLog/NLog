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
var buildDir = Directory(outputDirectory);
var samplesDir = Directory(samplesDirectory);

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
    .IsDependentOn("uap10");

Task("uap10")
	.ContinueOnError()
	.IsDependentOn("sl5")
	.Does(() =>
{
    // Restore
    DNURestoreSettings restoreSettings = new DNURestoreSettings()
    {
        Architecture = DNArchitecture.X64,
        Runtime = DNRuntime.Clr,
        Version = dnxVersion
    };
	DNURestore(restoreSettings);

	// Build
	DNUBuildSettings dnuBuildSettings = new DNUBuildSettings
	{
        Architecture = DNArchitecture.X64,
        Runtime = DNRuntime.Clr,
        Version = dnxVersion,
	    Frameworks = new [] { "uap10" },
	    Configurations = new[] { configuration },
	    OutputDirectory = (buildDir + Directory("uap10")).ToString(),
	    Quiet = false
	};
        
    DNUBuild("./src/NLog", dnuBuildSettings);
    DNUBuild("./src/NLogAutoLoadExtension", dnuBuildSettings);
	DNUBuild("./tests/SampleExtensions", dnuBuildSettings);
	
    dnuBuildSettings = new DNUBuildSettings
	{
        Architecture = DNArchitecture.X64,
        Runtime = DNRuntime.Clr,
        Version = dnxVersion,
	    Frameworks = new [] { "uap10" },
	    Configurations = new[] { configuration },
	    OutputDirectory = (buildDir + Directory("uap10")).ToString(),
	    Quiet = true
	};
    DNUBuild("./tests/NLog.UnitTests", dnuBuildSettings);

	// Test
	var settings = new DNXRunSettings
	{ 
        Architecture = DNArchitecture.X64,
        Runtime = DNRuntime.Clr,
        Version = dnxVersion	
    };
	DNXRun("./tests/NLog.UnitTests/", "test", settings);

});
   
Task("sl5")
	.ContinueOnError()
	.IsDependentOn("net35")
	.Does(() =>
{
    // Restore
    DNURestoreSettings restoreSettings = new DNURestoreSettings()
    {
        Architecture = DNArchitecture.X64,
        Runtime = DNRuntime.Clr,
        Version = dnxVersion
    };
	DNURestore(restoreSettings);

	// Build
	DNUBuildSettings dnuBuildSettings = new DNUBuildSettings
	{
        Architecture = DNArchitecture.X64,
        Runtime = DNRuntime.Clr,
        Version = dnxVersion,
	    Frameworks = new [] { "sl5" },
	    Configurations = new[] { configuration },
	    OutputDirectory = (buildDir + Directory("sl5")).ToString(),
	    Quiet = false
	};
        
    DNUBuild("./src/NLog", dnuBuildSettings);
    DNUBuild("./src/NLogAutoLoadExtension", dnuBuildSettings);
	DNUBuild("./tests/SampleExtensions", dnuBuildSettings);
	
    dnuBuildSettings = new DNUBuildSettings
	{
        Architecture = DNArchitecture.X64,
        Runtime = DNRuntime.Clr,
        Version = dnxVersion,
	    Frameworks = new [] { "sl5" },
	    Configurations = new[] { configuration },
	    OutputDirectory = (buildDir + Directory("sl5")).ToString(),
	    Quiet = true
	};
    DNUBuild("./tests/NLog.UnitTests", dnuBuildSettings);

	// Test
	var settings = new DNXRunSettings
	{ 
        Architecture = DNArchitecture.X64,
        Runtime = DNRuntime.Clr,
        Version = dnxVersion	
    };
	DNXRun("./tests/NLog.UnitTests/", "test", settings);

});

Task("net35")
	.ContinueOnError()
	.IsDependentOn("Dnx451")
	.Does(() =>
{
    // Restore
    DNURestoreSettings restoreSettings = new DNURestoreSettings()
    {
        Architecture = DNArchitecture.X64,
        Runtime = DNRuntime.Clr,
        Version = dnxVersion
    };
	DNURestore(restoreSettings);

	// Build
	DNUBuildSettings dnuBuildSettings = new DNUBuildSettings
	{
        Architecture = DNArchitecture.X64,
        Runtime = DNRuntime.Clr,
        Version = dnxVersion,
	    Frameworks = new [] { "net35" },
	    Configurations = new[] { configuration },
	    OutputDirectory = (buildDir + Directory("net35")).ToString(),
	    Quiet = false
	};
        
    DNUBuild("./src/NLog", dnuBuildSettings);
    DNUBuild("./src/NLogAutoLoadExtension", dnuBuildSettings);
	DNUBuild("./tests/SampleExtensions", dnuBuildSettings);
	
    dnuBuildSettings = new DNUBuildSettings
	{
        Architecture = DNArchitecture.X64,
        Runtime = DNRuntime.Clr,
        Version = dnxVersion,
	    Frameworks = new [] { "dnx451" },
	    Configurations = new[] { configuration },
	    OutputDirectory = (buildDir + Directory("net35")).ToString(),
	    Quiet = true
	};
    DNUBuild("./tests/NLog.UnitTests", dnuBuildSettings);

	// Test
	var settings = new DNXRunSettings
	{ 
        Architecture = DNArchitecture.X64,
        Runtime = DNRuntime.Clr,
        Version = dnxVersion	
    };
	DNXRun("./tests/NLog.UnitTests/", "test", settings);

});

Task("Dnx451")
	.ContinueOnError()
	.IsDependentOn("Dnxcore50")
	.Does(() =>
{
	
	// Restore
    DNURestoreSettings restoreSettings = new DNURestoreSettings()
    {
        Architecture = DNArchitecture.X64,
        Runtime = DNRuntime.Clr,
        Version = dnxVersion
    };
	DNURestore(restoreSettings);

	// Build
	DNUBuildSettings dnuBuildSettings = new DNUBuildSettings
	{
        Architecture = DNArchitecture.X64,
        Runtime = DNRuntime.Clr,
        Version = dnxVersion,
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
        Architecture = DNArchitecture.X64,
        Runtime = DNRuntime.Clr,
        Version = dnxVersion
    };
	DNXRun("./tests/NLog.UnitTests/", "test", settings);

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
        Version = dnxVersion
    };
	DNURestore(restoreSettings);

	// Build
	DNUBuildSettings dnuBuildSettings = new DNUBuildSettings
	{
        Architecture = DNArchitecture.X64,
        Runtime = DNRuntime.CoreClr,
        Version = dnxVersion,
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
	{	
        Architecture = DNArchitecture.X64,
        Runtime = DNRuntime.CoreClr,
        Version = dnxVersion
    };
	DNXRun("./tests/NLog.UnitTests/", "test", settings);

});
//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
