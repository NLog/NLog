@echo off
rem Try to find the highest version of MSBuild available...
set MSBUILD=%WINDIR%\Microsoft.NET\Framework\v4.0.30128\msbuild.exe
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

if (%1)==(netfx35client) (
	set MSBUILD_ARGUMENTS=%MSBUILD_ARGUMENTS% /p:BuildNetFX35Client=true
	shift
	goto next
)

if (%1)==(netfx40) (
	set MSBUILD_ARGUMENTS=%MSBUILD_ARGUMENTS% /p:BuildNetFX40=true
	shift
	goto next
)

if (%1)==(netfx40client) (
	set MSBUILD_ARGUMENTS=%MSBUILD_ARGUMENTS% /p:BuildNetFX40Client=true
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

if (%1)==(clean) (
	set MSBUILD_ARGUMENTS=%MSBUILD_ARGUMENTS% /t:Clean
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
	shift
	goto next
)

if (%1)==(rebuild) (
	set MSBUILD_ARGUMENTS=%MSBUILD_ARGUMENTS% /t:Rebuild
	shift
	goto next
)

if (%1)==(all) (
	set MSBUILD_ARGUMENTS=%MSBUILD_ARGUMENTS% /t:All
	shift
	goto next
)

echo Usage: %0 [configuration] [platform]... [target]...
echo.
echo Where platform is one or more of the following:
echo.
echo  netfx20            .NET Framework 2.0
echo  netfx35            .NET Framework 3.5
echo  netfx35client      .NET Framework 3.5 Client Profile
echo  netfx40            .NET Framework 4.0
echo  netfx40client      .NET Framework 4.0 Client Profile
echo  netcf20            .NET Compact Framework 2.0
echo  netcf35            .NET Compact Framework 3.5
echo  sl2                Silverlight 2.0
echo  sl3                Silverlight 3.0
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
echo  build              Compiles assemblies
echo  buildtests         Compiles tests
echo  runtests           Runs unit tests
echo  doc                Builds documentation
echo  all                Full build
exit /b 1


:build
echo MSBUILD: %MSBUILD%
echo MSBUILD_ARGUMENTS: %MSBUILD_ARGUMENTS%
%MSBUILD% /nologo /fl %~dp0src\NLog.proj %MSBUILD_ARGUMENTS%
