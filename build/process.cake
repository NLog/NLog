
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
		
		Information("Running unit tests in project " + testTarget);
		DNXRun(testTarget, "test", settings);
		
		var di = new System.IO.DirectoryInfo(testTarget);
		if(!di.Exists)
			di.Create();
		string unitTestResultFile = "testresults_" + di.Name + ".xml";
		CopyFile(testTarget + "/testresults.xml", (buildDir + Directory(configuration) + Directory(target)) + File(unitTestResultFile));
	}
	
};
