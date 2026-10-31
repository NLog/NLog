# restore and builds all projects as release.
# creates NuGet package at \artifacts
dotnet --version

$versionPrefix = "6.2.2"
$versionSuffix = ""
$versionFile = $versionPrefix + "." + ${env:APPVEYOR_BUILD_NUMBER}
$versionProduct = $versionPrefix;
if (-not $versionSuffix.Equals(""))
	{ $versionProduct = $versionProduct + "-" + $versionSuffix }

if ($env:APPVEYOR_PULL_REQUEST_NUMBER)
{
   $versionPrefix = $versionPrefix + "." + ${env:APPVEYOR_BUILD_NUMBER}
   $versionSuffix = "PR" + $env:APPVEYOR_PULL_REQUEST_NUMBER
}

$targetNugetExe = "tools/nuget.exe"
if (-not (Test-Path $targetNugetExe))
{
	# download nuget.exe
	$sourceNugetExe = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
	Invoke-WebRequest $sourceNugetExe -OutFile $targetNugetExe
}

msbuild /t:Restore,Pack ./src/NLog/ /p:VersionPrefix=$versionPrefix /p:VersionSuffix=$versionSuffix /p:FileVersion=$versionFile /p:ProductVersion=$versionProduct /p:Configuration=Release /p:IncludeSymbols=true /p:SymbolPackageFormat=snupkg /p:ContinuousIntegrationBuild=true  /p:EmbedUntrackedSources=true /p:PackageOutputPath=..\..\artifacts /verbosity:minimal /maxcpucount
if (-not $LastExitCode -eq 0)
	{ exit $LastExitCode }

function Create-Package($packageName)
{
	$path = "./src/$packageName/"
	msbuild /t:Restore,Pack $path /p:VersionPrefix=$versionPrefix /p:VersionSuffix=$versionSuffix /p:FileVersion=$versionFile /p:ProductVersion=$versionProduct /p:Configuration=Release /p:IncludeSymbols=true /p:SymbolPackageFormat=snupkg /p:ContinuousIntegrationBuild=true /p:EmbedUntrackedSources=true /p:PackageOutputPath=..\..\artifacts /verbosity:minimal  /maxcpucount
	if (-not $LastExitCode -eq 0)
		{ exit $LastExitCode }
}

Create-Package 'NLog.Targets.GZipFile'
Create-Package 'NLog.OutputDebugString'
Create-Package 'NLog.RegEx'
msbuild /t:Restore,Pack ./src/NLog.Targets.AtomicFile/ /p:VersionPrefix=$versionPrefix /p:VersionSuffix=$versionSuffix /p:FileVersion=$versionFile /p:ProductVersion=$versionProduct /p:Configuration=Release /p:IncludeSymbols=true /p:SymbolPackageFormat=snupkg /p:ContinuousIntegrationBuild=true /p:EmbedUntrackedSources=true /p:PackageOutputPath=..\..\artifacts /verbosity:minimal  /maxcpucount
if (-not $LastExitCode -eq 0)
	{ exit $LastExitCode }
Create-Package 'NLog.WindowsEventLog' '"netstandard2.0;netstandard2.1"'

msbuild /t:xsd /t:NuGetSchemaPackage ./src/NLog.proj /p:Configuration=Release /p:BuildNetFX45=true /p:BuildVersion=$versionProduct /p:Configuration=Release /p:BuildLabelOverride=NONE /verbosity:minimal
if (-not $LastExitCode -eq 0)
	{ exit $LastExitCode }

exit $LastExitCode
