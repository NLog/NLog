@echo off
rem Try to find the highest version of MSBuild available...
set MSBUILD=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe
set MSBUILD_SCRIPT="%~dp0src\NLog.proj"
set POST_BUILD_COMMAND=
if not exist %MSBUILD% set MSBUILD=%WINDIR%\Microsoft.NET\Framework\v3.5\msbuild.exe
if not exist %MSBUILD% (
	echo MSBuild not found, please update %0
	exit /b 1
) 

set MSBUILD_ARGUMENTS=
:next
if (%1)==() goto build
if (%1)==(usemsbuild35) (
	set MSBUILD=%WINDIR%\Microsoft.NET\Framework\v3.5\msbuild.exe
	shift
	goto next
)

if (%1)==(netfx20) (
	set MSBUILD_ARGUMENTS=%MSBUILD_ARGUMENTS% /p:BuildNetFX20=true
	shift
	goto next
)

if (%1)==(netfx35) (
	set MSBUILD_ARGUMENTS=%MSBUILD_ARGUMENTS% /p:BuildNetFX35=true
	shift
	goto next
)

if (%1)==(netfx40) (
	set MSBUILD_ARGUMENTS=%MSBUILD_ARGUMENTS% /p:BuildNetFX40=true
	shift
	goto next
)

if (%1)==(netcf20) (
	set MSBUILD_ARGUMENTS=%MSBUILD_ARGUMENTS% /p:BuildNetCF20=true
	shift
	goto next
)

if (%1)==(netcf35) (
	set MSBUILD_ARGUMENTS=%MSBUILD_ARGUMENTS% /p:BuildNetCF35=true
	shift
	goto next
)

if (%1)==(mono2) (
	set MSBUILD_ARGUMENTS=%MSBUILD_ARGUMENTS% /p:BuildMono2=true
	shift
	goto next
)

if (%1)==(sl2) (
	set MSBUILD_ARGUMENTS=%MSBUILD_ARGUMENTS% /p:BuildSL2=true
	shift
	goto next
)

if (%1)==(sl3) (
	set MSBUILD_ARGUMENTS=%MSBUILD_ARGUMENTS% /p:BuildSL3=true
	shift
	goto next
)

if (%1)==(sl4) (
	set MSBUILD_ARGUMENTS=%MSBUILD_ARGUMENTS% /p:BuildSL4=true
	shift
	goto next
)

if (%1)==(wp7) (
	set MSBUILD_ARGUMENTS=%MSBUILD_ARGUMENTS% /p:BuildWP7=true
	shift
	goto next
)

if (%1)==(wp71) (
	set MSBUILD_ARGUMENTS=%MSBUILD_ARGUMENTS% /p:BuildWP71=true
	shift
	goto next
)

if (%1)==(debug) (
	set MSBUILD_ARGUMENTS=%MSBUILD_ARGUMENTS% /p:Configuration=Debug
	shift
	goto next
)

if (%1)==(release) (
	set MSBUILD_ARGUMENTS=%MSBUILD_ARGUMENTS% /p:Configuration=Release
	shift
	goto next
)

if (%1)==(doc) (
	set MSBUILD_ARGUMENTS=%MSBUILD_ARGUMENTS% /t:Documentation
	shift
	goto next
)

if (%1)==(dumpapi) (
	set MSBUILD_ARGUMENTS=%MSBUILD_ARGUMENTS% /t:DumpApi
	shift
	goto next
)

if (%1)==(builddocpages) (
	set MSBUILD_ARGUMENTS=%MSBUILD_ARGUMENTS% /t:BuildDocPages
	shift
	goto next
)

if (%1)==(clean) (
	set MSBUILD_ARGUMENTS=%MSBUILD_ARGUMENTS% /t:Clean
	shift
	goto next
)

if (%1)==(deepclean) (
	set MSBUILD_ARGUMENTS=%MSBUILD_ARGUMENTS% /t:DeepClean
	shift
	goto next
)

if (%1)==(build) (
	set MSBUILD_ARGUMENTS=%MSBUILD_ARGUMENTS% /t:Build
	shift
	goto next
)

if (%1)==(buildtests) (
	set MSBUILD_ARGUMENTS=%MSBUILD_ARGUMENTS% /t:BuildTests
	shift
	goto next
)

if (%1)==(runtests) (
	set MSBUILD_ARGUMENTS=%MSBUILD_ARGUMENTS% /t:RunTests
	set POST_BUILD_COMMAND="%~dp0build\bin\LastTestRunSummary.cmd"
	shift
	goto next
)

if (%1)==(rebuild) (
	set MSBUILD_ARGUMENTS=%MSBUILD_ARGUMENTS% /t:Rebuild
	shift
	goto next
)

if (%1)==(tools) (
	set MSBUILD_ARGUMENTS=%MSBUILD_ARGUMENTS% /t:Tools
	shift
	goto next
)

if (%1)==(syncprojectitems) (
	set MSBUILD_ARGUMENTS=%MSBUILD_ARGUMENTS% /t:SyncProjectItems
	shift
	goto next
)

if (%1)==(web) (
	set MSBUILD_ARGUMENTS=%MSBUILD_ARGUMENTS% /t:Web
	shift
	goto next
)

if (%1)==(checkinsuite) (
	set MSBUILD_ARGUMENTS=%MSBUILD_ARGUMENTS% /t:CheckinSuite
	set POST_BUILD_COMMAND="%~dp0src\LastTestRunSummary.cmd"
	shift
	goto next
)

if (%1)==(xsd) (
	set MSBUILD_ARGUMENTS=%MSBUILD_ARGUMENTS% /t:XSD
	shift
	goto next
)

if (%1)==(archive) (
	set MSBUILD_ARGUMENTS=%MSBUILD_ARGUMENTS% /t:Archive
	shift
	goto next
)

if (%1)==(sourcearchive) (
	set MSBUILD_ARGUMENTS=%MSBUILD_ARGUMENTS% /t:SourceArchive
	shift
	goto next
)

if (%1)==(label) (
	set MSBUILD_ARGUMENTS=%MSBUILD_ARGUMENTS% /p:BuildLabel=%2
	shift
	shift
	goto next
)

if (%1)==(ccnetlabel) (
	set MSBUILD_ARGUMENTS=%MSBUILD_ARGUMENTS% /p:CCNetLabel=%2
	shift
	shift
	goto next
)

if (%1)==(buildpath) (
	set MSBUILD_ARGUMENTS=%MSBUILD_ARGUMENTS% /p:BaseOutputDirectory=%2
	shift
	shift
	goto next
)

if (%1)==(websitedir) (
	set MSBUILD_ARGUMENTS=%MSBUILD_ARGUMENTS% /p:WebsiteDir=%2
	shift
	shift
	goto next
)

if (%1)==(all) (
	set MSBUILD_ARGUMENTS=%MSBUILD_ARGUMENTS% /t:All
	set POST_BUILD_COMMAND="%~dp0src\LastTestRunSummary.cmd"
	shift
	goto next
)

if (%1)==(nightlybuild) (
	set MSBUILD_ARGUMENTS=%MSBUILD_ARGUMENTS% /t:NightlyBuild
	shift
	goto next
)

if (%1)==(installer) (
	set MSBUILD_ARGUMENTS=%MSBUILD_ARGUMENTS% /t:Installer
	shift
	goto next
)

if (%1)==(nuget) (
	set MSBUILD_ARGUMENTS=%MSBUILD_ARGUMENTS% /t:NuGetPackage
	shift
	goto next
)

echo Usage: %0 [configuration] [platform]... [target]...
echo.
echo Where platform is one or more of the following:
echo.
echo  netfx20            .NET Framework 2.0
echo  netfx35            .NET Framework 3.5
echo  netfx40            .NET Framework 4.0
echo  netcf20            .NET Compact Framework 2.0
echo  netcf35            .NET Compact Framework 3.5
echo  sl2                Silverlight 2.0
echo  sl3                Silverlight 3.0
echo  sl4                Silverlight 4.0
echo  wp7                Silverlight for Windows Phone 7
echo  wp71               Silverlight for Windows Phone 7.1
echo  mono2              Mono 2.x
echo.
echo Configurations are: 
echo.
echo  debug
echo  release
echo.
echo Targets can be:
echo.
echo  clean              Removes output files
echo  deepclean          Removes temporary and intermediate files
echo  archive            Produce ZIP files for each framework
echo  sourcearchive      Produce ZIP file of the source code
echo  build              Compiles assemblies
echo  buildtests         Compiles tests
echo  runtests           Runs unit tests
echo  checkinsuite       Cleans, builds and runs all tests
echo  doc                Builds documentation
echo  all                Full build
echo  nightlybuild       Nightly build
echo  web                Website files
echo  xsd                NLog.xsd Intellisense files
echo  installer          Installer
echo  nuget              NuGet package
echo  tools              Tools
echo  label {suffix}     Define build label (defaults to 'PrivateBuild')
echo  ccnetlabel {label} Override CCNetLabel
echo  buildpath {dir}    Set build directory.
exit /b 1

:build
echo MSBUILD: %MSBUILD%
echo MSBUILD_SCRIPT: %MSBUILD_SCRIPT%
echo MSBUILD_ARGUMENTS: %MSBUILD_ARGUMENTS%
%MSBUILD% /nologo /fl %MSBUILD_SCRIPT% %MSBUILD_ARGUMENTS%
%POST_BUILD_COMMAND%
exit /b 0
