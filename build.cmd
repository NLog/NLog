@echo off
cd %~dp0

SETLOCAL
SET NUGET_VERSION=latest
SET CACHED_NUGET=%LocalAppData%\NuGet\nuget.%NUGET_VERSION%.exe
SET BUILDCMD_KOREBUILD_VERSION=
SET BUILDCMD_DNX_VERSION=1.0.0-rc1-update1
SET SKIP_DNX_INSTALL=

IF EXIST %CACHED_NUGET% goto copynuget
echo Downloading latest version of NuGet.exe...
IF NOT EXIST %LocalAppData%\NuGet md %LocalAppData%\NuGet
@powershell -NoProfile -ExecutionPolicy unrestricted -Command "$ProgressPreference = 'SilentlyContinue'; Invoke-WebRequest 'https://dist.nuget.org/win-x86-commandline/%NUGET_VERSION%/nuget.exe' -OutFile '%CACHED_NUGET%'"

:copynuget
IF EXIST .nuget\nuget.exe goto restore
md .nuget
copy %CACHED_NUGET% .nuget\nuget.exe > nul

:restore
@powershell -NoProfile -ExecutionPolicy unrestricted -Command "Invoke-WebRequest http://cakebuild.net/bootstrapper/windows -OutFile build.ps1"

:getdnx
@powershell -NoProfile -ExecutionPolicy unrestricted -Command "&{$Branch='dev';iex ((new-object net.webclient).DownloadString('https://raw.githubusercontent.com/aspnet/Home/dev/dnvminstall.ps1'))}"

IF "%BUILDCMD_DNX_VERSION%"=="" (
    SET BUILDCMD_DNX_VERSION=latest
)
IF "%SKIP_DNX_INSTALL%"=="" (
    CALL dnvm install %BUILDCMD_DNX_VERSION% -runtime CLR -arch x86 -alias default
	CALL dnvm install %BUILDCMD_DNX_VERSION% -runtime coreclr -arch x86
	CALL dnvm install %BUILDCMD_DNX_VERSION% -runtime coreclr -arch x64
	
	CALL dnvm use default -runtime CLR -arch x86
) ELSE (
    CALL dnvm use default -runtime CLR -arch x86
)

@powershell -NoProfile -ExecutionPolicy unrestricted -Command ".\build.ps1 build.cake %*"