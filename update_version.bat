@echo off
rem
rem This script updates AssemblyBuildInfo.cs in source code directories based on
rem the contents of NLog.version file.
rem
rem When using Visual C# 2003, 2005 or NAnt this is done automatically, you only
rem need to do this by hand when compiling .NET Compact Framework project.
rem 
tools\UpdateBuildNumber.exe NLog.version src/NLog/AssemblyBuildInfo.cs build/NLog.buildversion
tools\UpdateBuildNumber.exe NLog.version src/NLog.ComInterop/AssemblyBuildInfo.cs build/NLog.buildversion

