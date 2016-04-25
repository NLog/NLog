#load "./build/process.cake"
//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var cakeTarget = Argument("target", "Default");
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
var srcProjects = System.IO.Directory.GetDirectories(string.Format(".{0}src", System.IO.Path.DirectorySeparatorChar)).Except(excludedSrcProjects); // It's absolutely ugly but it works. TODO: Find a better way to work with directories / files
// Not so standard in NLog, tests dir and not test
var testProjects = System.IO.Directory.GetDirectories(string.Format(".{0}tests", System.IO.Path.DirectorySeparatorChar)).Except(excludedTestProjects);


// Error Handling
List<Exception> errors = new List<Exception>();


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
	.IsDependentOn("Clean")
	.IsDependentOn("dotnet5.4")
	.IsDependentOn("net35")
	.IsDependentOn("net451")
	.IsDependentOn("sl5")
	.IsDependentOn("Pack")
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
	.IsDependentOn("restore")
    .WithCriteria(() => !errors.Any())
	.Does(() => 
{
	string[] frameworks = null;
	if(!IsRunningOnWindows())
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
        //Frameworks = frameworks, // DNU Pack command doesn't really support frameworks option ... 
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
	.IsDependentOn("restore")
    .WithCriteria(IsRunningOnWindows())
	.Does(() =>
{
	buildAndTest("sl5", buildDnxVersion, 
					buildRuntime, DNArchitecture.X86,
					srcProjects.Except(new string[]{ string.Format(@".{0}src{0}NLog.Extended", System.IO.Path.DirectorySeparatorChar),
													 string.Format(@".{0}src{0}NLogAutoLoadExtension", System.IO.Path.DirectorySeparatorChar)}),
					testProjects, "sl5");
}).OnError(exception => {
    errors.Add(exception);
});

Task("net35")
	.IsDependentOn("restore")
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
	.IsDependentOn("restore")
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
					DNRuntime.CoreClr, DNArchitecture.X64,
					srcProjects.Except(new string[]{ string.Format(@".{0}src{0}NLog.Extended", System.IO.Path.DirectorySeparatorChar) }),
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

RunTarget(cakeTarget);