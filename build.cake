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

////
// Gets all src projects and unit tests projects
////

// Allow excluding some directories if needed
var excludedSrcProjects = new string[0];
var excludedTestProjects = new string[0];

// Look in standard .NET Core dirs
var srcProjects = System.IO.Directory.GetDirectories(".\\src").Except(excludedSrcProjects);
// Not so standard in NLog, tests dir and not test
var testProjects = System.IO.Directory.GetDirectories(".\\tests").Except(excludedTestProjects);


// Error Handling
List<Exception> errors = new List<Exception>();

//////////////////////////////////////////////////////////////////////
// Build method
//////////////////////////////////////////////////////////////////////

Action<string, string, DNRuntime, DNArchitecture, IEnumerable<string>, IEnumerable<string>, string> buildAndTest = (string target, string dnxVersion, DNRuntime runtime, DNArchitecture architecture, IEnumerable<string> buildTargets, IEnumerable<string> testTargets, string frameworkForTests) =>
{
	foreach(var buildTarget in buildTargets)
	{
		string projectJsonFile = buildTarget + "/project.json";
		if(!System.IO.File.Exists(projectJsonFile))
			continue;
	
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
        
		Information("Building project " + buildTarget);
		DNUBuild(buildTarget, dnuBuildSettings);
	}
	
	foreach(var testTarget in testTargets)
	{
		string testProjectJsonFile = testTarget + "/project.json";
		if(!System.IO.File.Exists(testProjectJsonFile))
			continue;
	
		// Build
		var dnuBuildSettingsForTest = new DNUBuildSettings
		{
			Architecture = architecture,
			Runtime = runtime,
			Version = dnxVersion,
			Frameworks = new [] { frameworkForTests },
			Configurations = new[] { configuration },
			OutputDirectory = buildDir,
			Quiet = true
		};
        
		Information("Building project " + testTarget);
		DNUBuild(testTarget, dnuBuildSettingsForTest);
	
		// Test
		var settings = new DNXRunSettings
		{	
			Architecture = architecture,
			Runtime = runtime,
			Version = dnxVersion
		};
		
		/*OpenCover(tool => {
			tool.DNXRun(testTarget, "test", settings);
		},
		new FilePath("./cover_result.xml"),
		new OpenCoverSettings());*/
		
		Information("Running unit tests in project " + testTarget);
		DNXRun(testTarget, "test", settings);
		
		var di = new System.IO.DirectoryInfo(testTarget);
		string unitTestResultFile = "testresults_" + di.Name + ".xml";
		CopyFile(testTarget + "/testresults.xml", (buildDir + Directory(configuration) + Directory(target)) + File(unitTestResultFile));
	}
	
};


//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////
Task("Clean")
    .Does(() =>
{
    CleanDirectory(outputDirectory);
    CleanDirectory(buildDir);
	CleanDirectory(samplesDir);
	CleanDirectory(nugetDir);
	
	CreateDirectory(outputDirectory);
	CreateDirectory(buildDir);
	CreateDirectory(samplesDir);
	CreateDirectory(nugetDir);
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("checkErrors");

Task("checkErrors")
    .IsDependentOn("pack")
    .Does(() => 
    {
        if(errors.Any())
        {
            foreach(var error in errors)
            {
                Error(error.Message);
            }
            throw new AggregateException("One or more errors occur during the build. Please check inner exceptions", errors);
        }
    });

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

	foreach(var project in srcProjects)
	{
		string projectJsonFile = project + "/project.json";
		if(!System.IO.File.Exists(projectJsonFile))
			continue;
			
		DNUPack(project, packSettings);
		CopyFiles(System.IO.Path.Combine(project, "bin" , configuration , "*.nupkg"), nugetDir);
	}
}).OnError(exception => {
    errors.Add(exception);
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
		Runtime = buildRuntime,
		Version = buildDnxVersion,
		Quiet = true
	};
    Information("Restore");
	DNURestore(".", restoreSettings);
	
	buildAndTest("uap10.0", buildDnxVersion, 
					buildRuntime, DNArchitecture.X86,
					new [] { "./src/NLog" },
					"./tests/NLog.UnitTests", dnxVersion);
}).OnError(exception => {
    errors.Add(exception);
});*/
   
Task("sl5")
	.IsDependentOn("net35")
    .WithCriteria(IsRunningOnWindows())
	.Does(() =>
{
	buildAndTest("sl5", buildDnxVersion, 
					buildRuntime, DNArchitecture.X86,
					srcProjects,
					testProjects, "sl5");
}).OnError(exception => {
    errors.Add(exception);
});

Task("net35")
	.IsDependentOn("net451")
    .WithCriteria(IsRunningOnWindows())
	.Does(() =>
{
	buildAndTest("net35", buildDnxVersion, 
					buildRuntime, DNArchitecture.X86,
					srcProjects,
					testProjects, "dnx451");
}).OnError(exception => {
    errors.Add(exception);
});

Task("net451")
	.IsDependentOn("dotnet5.4")
	.Does(() =>
{
	buildAndTest("net451", buildDnxVersion, 
					buildRuntime, DNArchitecture.X86,
					srcProjects,
					testProjects, "dnx451");
}).OnError(exception => {
    errors.Add(exception);
});

Task("dotnet5.4")
	.IsDependentOn("restore")
	.Does(() =>
{
	buildAndTest("dotnet5.4", buildDnxVersion, 
					DNRuntime.CoreClr, DNArchitecture.X86,
					srcProjects.Except(new string[]{ @".\src\NLog.Extended" }),
					testProjects, "dnxcore50");
}).OnError(exception => {
    errors.Add(exception);
});

Task("restore")
	.Does(() => 
{
	// Restore
	DNURestoreSettings restoreSettings = new DNURestoreSettings()
	{
		Architecture = DNArchitecture.X86,
		Runtime = buildRuntime,
		Version = buildDnxVersion,
		Quiet = true
	};
	DNURestore(".", restoreSettings);
}).OnError(exception => {
    errors.Add(exception);
});

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(buildTarget);
