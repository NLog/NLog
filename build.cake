//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var buildTarget = Argument("target", "Default");
var configuration = Argument("configuration", "Debug");
var outputDirectory = Argument<string>("outputDirectory", "./artifacts");
var nugetDirectory = Argument<string>("nugetDirectory", "./nuget");
var samplesDirectory = Argument<string>("samplesDirectory", "./samples");
var buildDnxVersion = Argument<string>("dnxVersion", "1.0.0-rc1-update1");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var buildDir = Directory(outputDirectory);
var samplesDir = Directory(samplesDirectory);
var nugetDir = Directory(nugetDirectory);

// Define runtime
DNRuntime buildRuntime = DNRuntime.Clr;
if(IsRunningOnUnix())
{
    buildRuntime = DNRuntime.Mono;
}

//////////////////////////////////////////////////////////////////////
// Build method
//////////////////////////////////////////////////////////////////////

Action<string, string, DNRuntime, DNArchitecture, string[], string, string> buildAndTest = (string target, string dnxVersion, DNRuntime runtime, DNArchitecture architecture, string[] buildTargets, string testTarget, string targetForDnx) =>
{
	foreach(var bTarget in buildTargets)
	{
		
		// Build
         Information("Build");
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
        
		DNUBuild(bTarget, dnuBuildSettings);
	}
	
	// Restore
    Information("Restore unit test");
	var restoreSettingsForTest = new DNURestoreSettings()
		{
			Architecture = architecture,
			Runtime = runtime,
			Version = dnxVersion,
			Quiet = true
		};
		DNURestore(testTarget + "/project.json", restoreSettingsForTest);
	
		// Build
        Information("Build unit test");
		var dnuBuildSettingsForTest = new DNUBuildSettings
		{
			Architecture = architecture,
			Runtime = runtime,
			Version = dnxVersion,
			Frameworks = new [] { targetForDnx },
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
        Version = dnxVersion
    };
    
    Information("Run unit test");
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
	.IsDependentOn("sl5") //last task in chain
	.Does(() => 
{
	string[] frameworks = null;
	if(IsRunningOnUnix())
	{
		frameworks = new [] { "net451","dotnet5.4" };
	}
	else
	{
		frameworks = new [] { "net451", "dotnet5.4", "net35", "sl5" };
	}

	DNUPackSettings packSettings = new DNUPackSettings()
	{
		Architecture = DNArchitecture.X86,
        Runtime = buildRuntime,
        Version = buildDnxVersion,
	    Configurations = new[] { configuration },
	    Quiet = true
	};

	DNUPack("./src/NLog/project.json", packSettings);
	
	CopyFiles("./src/NLog/bin/" + configuration + "/*.nupkg", nugetDir);
});

/*Task("uap10")
	.ContinueOnError()
	.IsDependentOn("sl5")
    .WithCriteria(IsRunningOnWindows())
	.Does(() =>
{
	
	// Restore
	DNURestoreSettings restoreSettings = new DNURestoreSettings()
	{
		Architecture = DNArchitecture.X86,
		Runtime = runtime,
		Version = dnxVersion,
		Quiet = true
	};
    Information("Restore");
	DNURestore(".", restoreSettings);
	
	buildAndTest("uap10.0", buildDnxVersion, 
					buildRuntime, DNArchitecture.X86,
					new [] { "./src/NLog" },
					"./tests/NLog.UnitTests", dnxVersion);
});*/
   
Task("sl5")
	.IsDependentOn("net35")
    .WithCriteria(IsRunningOnWindows())
	.Does(() =>
{
	// Restore
	DNURestoreSettings restoreSettings = new DNURestoreSettings()
	{
		Architecture = DNArchitecture.X86,
		Runtime = runtime,
		Version = dnxVersion,
		Quiet = true
	};
    Information("Restore");
	DNURestore(".", restoreSettings);
	
	buildAndTest("sl5", buildDnxVersion, 
					buildRuntime, DNArchitecture.X86,
					new [] { "./src/NLog", "./tests/SampleExtensions" },
					"./tests/NLog.UnitTests", "sl5");
});

Task("net35")
	.IsDependentOn("net451")
    .WithCriteria(IsRunningOnWindows())
	.Does(() =>
{
	// Restore
	DNURestoreSettings restoreSettings = new DNURestoreSettings()
	{
		Architecture = DNArchitecture.X86,
		Runtime = runtime,
		Version = dnxVersion,
		Quiet = true
	};
    Information("Restore");
	DNURestore(".", restoreSettings);
	
	buildAndTest("net35", buildDnxVersion, 
					buildRuntime, DNArchitecture.X86,
					new [] { "./src/NLog", "./src/NLogAutoLoadExtension", "./tests/SampleExtensions" },
					"./tests/NLog.UnitTests", "dnx451");  //unit test is application and not library, so dnx451
});

Task("net451")
	.IsDependentOn("dotnet5.4")
	.Does(() =>
{
	// Restore
	DNURestoreSettings restoreSettings = new DNURestoreSettings()
	{
		Architecture = DNArchitecture.X86,
		Runtime = runtime,
		Version = dnxVersion,
		Quiet = true
	};
    Information("Restore");
	DNURestore(".", restoreSettings);
	
	buildAndTest("net451", buildDnxVersion, 
					buildRuntime, DNArchitecture.X86,
					new [] { "./src/NLog", "./src/NLog.Extended", "./src/NLogAutoLoadExtension", "./tests/SampleExtensions" },
					"./tests/NLog.UnitTests", "dnx451");  //unit test is application and not library, so dnx451
});

Task("dotnet5.4")
	.Does(() =>
{
	// Restore
	DNURestoreSettings restoreSettings = new DNURestoreSettings()
	{
		Architecture = DNArchitecture.X86,
		Runtime = DNRuntime.CoreClr,
		Version = dnxVersion,
		Quiet = true
	};
    Information("Restore");
	DNURestore(".", restoreSettings);
	
	buildAndTest("dotnet5.4", buildDnxVersion, 
					DNRuntime.CoreClr, DNArchitecture.X64, // Coreclr doesn't support x86 on *unix
					new [] { "./src/NLog", "./src/NLogAutoLoadExtension", "./tests/SampleExtensions" },
					"./tests/NLog.UnitTests", "dnxcore50"); //unit test is application and not library, so dnxcore50
});


//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(buildTarget);
