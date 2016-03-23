rem @echo off
set FRAMEWORK=%1
if (%1)==() set FRAMEWORK=".NET Framework 4.5"
set CONFIGURATION=%2
if (%2)==() set CONFIGURATION=Release
%WINDIR%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe %~dp0NLog.shfbproj /p:Framework=%FRAMEWORK%

