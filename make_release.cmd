@echo off
if (%1)==() (
	echo Usage: %0 releaseName
	echo.
	echo releaseName is the name of the release without NLog prefix
	exit /b 1
)
nant -D:nlog.package.name=%1 fullrelease website
