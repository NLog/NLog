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
// Build method
//////////////////////////////////////////////////////////////////////

Action<string, string, DNRuntime, DNArchitecture, string[], string, string> buildAndTest = (string target, string dnxVersion, DNRuntime runtime, DNArchitecture architecture, string[] buildTargets, string testTarget, string dnxVersionForTest) =>
{
	foreach(var buildTarget in buildTargets)
	{
		// Restore
		DNURestoreSettings restoreSettings = new DNURestoreSettings()
		{
			Architecture = architecture,
			Runtime = runtime,
			Version = dnxVersion,
			Quiet = true
		};
		DNURestore(buildTarget + "/project.json", restoreSettings);
	
		// Build
		DNUBuildSettings dnuBuildSettings = new DNUBuildSettings
		{
			Architecture = architecture,
			Runtime = runtime,
			Version = dnxVersion,
			Frameworks = new [] { target },
			Configurations = new[] { configuration },
			OutputDirectory = buildDir,
			Quiet = true
		};
        
		DNUBuild(buildTarget, dnuBuildSettings);
	}
	
	// Restore
		var restoreSettingsForTest = new DNURestoreSettings()
		{
			Architecture = architecture,
			Runtime = runtime,
			Version = dnxVersionForTest,
			Quiet = true
		};
		DNURestore(testTarget + "/project.json", restoreSettingsForTest);
	
		// Build
		var dnuBuildSettingsForTest = new DNUBuildSettings
		{
			Architecture = architecture,
			Runtime = runtime,
			Version = dnxVersionForTest,
			Frameworks = new [] { target },
			Configurations = new[] { configuration },
			OutputDirectory = buildDir,
			Quiet = true
		};
        
		DNUBuild(testTarget, dnuBuildSettingsForTest);
	
	// Test
	var settings = new DNXRunSettings
	{	
        Architecture = architecture,
        Runtime = runtime,
        Version = dnxVersionForTest
    };
	DNXRun(testTarget, "test", settings);

	CopyFileToDirectory("./tests/NLog.UnitTests/testresults.xml", buildDir + Directory(configuration) + Directory(target));
};


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
		frameworks = new [] { "dnx451", "dnxcore50", "net35", "sl5", "uap10.0" };
	}

	DNUPackSettings packSettings = new DNUPackSettings()
	{
		Architecture = DNArchitecture.X86,
        Runtime = runtime,
        Version = dnxVersion,
	    Configurations = new[] { configuration },
	    Quiet = true
	};

	DNUPack("./src/NLog/project.json", packSettings);
	
	CopyFiles("./src/NLog/bin/" + configuration + "/*.nupkg", nugetDir);
});

Task("uap10")
	.ContinueOnError()
	.IsDependentOn("sl5")
    .WithCriteria(IsRunningOnWindows())
	.Does(() =>
{
	buildAndTest("uap10.0", dnxVersion, 
					runtime, DNArchitecture.X86,
					new [] { "./src/NLog" },
					"./tests/NLog.UnitTests", dnxVersion);
});
   
Task("sl5")
	.ContinueOnError()
	.IsDependentOn("net35")
    .WithCriteria(IsRunningOnWindows())
	.Does(() =>
{
	buildAndTest("sl5", dnxVersion, 
					runtime, DNArchitecture.X86,
					new [] { "./src/NLog", "./tests/SampleExtensions" },
					"./tests/NLog.UnitTests", "sl5");
});

Task("net35")
	.ContinueOnError()
	.IsDependentOn("dnx451")
    .WithCriteria(IsRunningOnWindows())
	.Does(() =>
{
	buildAndTest("net35", dnxVersion, 
					runtime, DNArchitecture.X86,
					new [] { "./src/NLog", "./src/NLogAutoLoadExtension", "./tests/SampleExtensions" },
					"./tests/NLog.UnitTests", "dnx451");
});

Task("dnx451")
	.ContinueOnError()
	.IsDependentOn("dnxcore50")
	.Does(() =>
{
	
	buildAndTest("dnx451", dnxVersion, 
					runtime, DNArchitecture.X86,
					new [] { "./src/NLog", "./src/NLog.Extended", "./src/NLogAutoLoadExtension", "./tests/SampleExtensions" },
					"./tests/NLog.UnitTests", "dnx451");
});

Task("dnxcore50")
	.ContinueOnError()
	.Does(() =>
{
	
	buildAndTest("dnxcore50", dnxVersion, 
					DNRuntime.CoreClr, DNArchitecture.X86,
					new [] { "./src/NLog", "./src/NLogAutoLoadExtension", "./tests/SampleExtensions" },
					"./tests/NLog.UnitTests", "dnxcore50");
});

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
