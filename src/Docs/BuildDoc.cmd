rem @echo off
set FRAMEWORK=%1
if (%1)==() set FRAMEWORK=".NET Framework 3.5"
set CONFIGURATION=%2
if (%2)==() set CONFIGURATION=Debug
%WINDIR%\Microsoft.NET\Framework\v3.5\MSBuild.exe %~dp0NLog.shfbproj /p:Framework=%FRAMEWORK%

