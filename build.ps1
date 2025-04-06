# restore and builds all projects as release.
# creates NuGet package at \artifacts
dotnet --version

$versionPrefix = "5.3.4"
$versionSuffix = ""
$versionFile = $versionPrefix + "." + ${env:APPVEYOR_BUILD_NUMBER}
$versionProduct = $versionPrefix;
if (-Not $versionSuffix.Equals(""))
	{ $versionProduct = $versionProduct + "-" + $versionSuffix }

if ($env:APPVEYOR_PULL_REQUEST_NUMBER)
{
   $versionPrefix = $versionPrefix + "." + ${env:APPVEYOR_BUILD_NUMBER}
   $versionSuffix = "PR" + $env:APPVEYOR_PULL_REQUEST_NUMBER
}

$targetNugetExe = "tools/nuget.exe"
if (-Not (test-path $targetNugetExe))
{
	# download nuget.exe
	$sourceNugetExe = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
	Invoke-WebRequest $sourceNugetExe -OutFile $targetNugetExe
}

msbuild /t:Restore,Pack ./src/NLog/ /p:targetFrameworks='"net46;net45;net35;netstandard2.0;netstandard2.1"' /p:VersionPrefix=$versionPrefix /p:VersionSuffix=$versionSuffix /p:FileVersion=$versionFile /p:ProductVersion=$versionProduct /p:Configuration=Release /p:IncludeSymbols=true /p:SymbolPackageFormat=snupkg /p:ContinuousIntegrationBuild=true  /p:EmbedUntrackedSources=true /p:PackageOutputPath=..\..\artifacts /verbosity:minimal /maxcpucount
if (-Not $LastExitCode -eq 0)
	{ exit $LastExitCode }

function create-package($packageName, $targetFrameworks)
{
	$path = "./src/$packageName/"
	msbuild /t:Restore,Pack $path /p:targetFrameworks=$targetFrameworks /p:VersionPrefix=$versionPrefix /p:VersionSuffix=$versionSuffix /p:FileVersion=$versionFile /p:ProductVersion=$versionProduct /p:Configuration=Release /p:IncludeSymbols=true /p:SymbolPackageFormat=snupkg /p:ContinuousIntegrationBuild=true /p:EmbedUntrackedSources=true /p:PackageOutputPath=..\..\artifacts /verbosity:minimal  /maxcpucount
	if (-Not $LastExitCode -eq 0)
		{ exit $LastExitCode }
}

create-package 'NLog.AutoReloadConfig' '"net35;net45;net46;netstandard2.0"'
create-package 'NLog.Database' '"net35;net45;net46;netstandard2.0"'
create-package 'NLog.Targets.Mail' '"net35;net45;net46;netstandard2.0"'
create-package 'NLog.Targets.Network' '"net45;net46;netstandard2.0"'
create-package 'NLog.Targets.Trace' '"net35;net45;net46;netstandard2.0"'
create-package 'NLog.Targets.WebService' '"net45;net46;netstandard2.0"'
create-package 'NLog.OutputDebugString' '"net35;net45;net46;netstandard2.0"'
create-package 'NLog.RegEx' '"net35;net45;net46;netstandard2.0"'
create-package 'NLog.WindowsRegistry' '"net35;net45;net46;netstandard2.0"'
create-package 'NLog.Targets.ConcurrentFile' '"net35;net45;net46;netstandard2.0"'
msbuild /t:Restore,Pack ./src/NLog.Targets.AtomicFile/ /p:VersionPrefix=$versionPrefix /p:VersionSuffix=$versionSuffix /p:FileVersion=$versionFile /p:ProductVersion=$versionProduct /p:Configuration=Release /p:IncludeSymbols=true /p:SymbolPackageFormat=snupkg /p:ContinuousIntegrationBuild=true /p:EmbedUntrackedSources=true /p:PackageOutputPath=..\..\artifacts /verbosity:minimal  /maxcpucount
create-package 'NLog.WindowsEventLog' '"netstandard2.0"'

msbuild /t:xsd /t:NuGetSchemaPackage ./src/NLog.proj /p:Configuration=Release /p:BuildNetFX45=true /p:BuildVersion=$versionProduct /p:Configuration=Release /p:BuildLabelOverride=NONE /verbosity:minimal

exit $LastExitCode
