# restore and builds all projects as release.
# creates NuGet package at \artifacts
dotnet --version

# dotnet restore .\src\NLog\
# dotnet pack .\src\NLog\  --configuration release --include-symbols -o ..\..\artifacts

$versionPrefix = "4.6.0"
$versionSuffix = "rc1"
$versionFile = $versionPrefix + "." + ${env:APPVEYOR_BUILD_NUMBER}
$versionProduct = $versionPrefix;
if (-Not $versionSuffix.Equals(""))
	{ $versionProduct = $versionProduct + "-" + $versionSuffix }

if ($env:APPVEYOR_PULL_REQUEST_NUMBER)
{
   $versionPrefix = $versionPrefix + "." + ${env:APPVEYOR_BUILD_NUMBER}
   $versionSuffix = "PR" + $env:APPVEYOR_PULL_REQUEST_NUMBER
}

# download nuget.exe

$sourceNugetExe = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
$targetNugetExe = "tools\nuget.exe"
Invoke-WebRequest $sourceNugetExe -OutFile $targetNugetExe

msbuild /t:Restore,Pack .\src\NLog\ /p:targetFrameworks='"net45;net40-client;net35;netstandard1.3;netstandard1.5;netstandard2.0;sl4;sl5;wp8;monoandroid44;xamarinios10"' /p:VersionPrefix=$versionPrefix /p:VersionSuffix=$versionSuffix /p:FileVersion=$versionFile /p:ProductVersion=$versionProduct /p:Configuration=Release /p:IncludeSymbols=true /p:SymbolPackageFormat=snupkg /p:PackageOutputPath=..\..\artifacts /verbosity:minimal
if (-Not $LastExitCode -eq 0)
	{ exit $LastExitCode }

function create-package($packageName)
{

	$path = ".\src\$packageName\"
	msbuild /t:Restore,Pack $path /p:VersionPrefix=$versionPrefix /p:VersionSuffix=$versionSuffix /p:FileVersion=$versionFile /p:ProductVersion=$versionProduct /p:Configuration=Release /p:IncludeSymbols=true /p:SymbolPackageFormat=snupkg /p:PackageOutputPath=..\..\artifacts /verbosity:minimal
	if (-Not $LastExitCode -eq 0)
		{ exit $LastExitCode }

}

create-package('NLog.Extended')
create-package('NLog.Wcf')
create-package('NLog.WindowsEventLog')
create-package('NLog.WindowsIdentity')

msbuild /t:xsd /t:NuGetSchemaPackage /t:NuGetConfigPackage .\src\NLog.proj /p:Configuration=Release /p:BuildNetFX45=true /p:BuildVersion=$versionProduct /p:Configuration=Release /p:BuildLabelOverride=NONE /verbosity:minimal

exit $LastExitCode
