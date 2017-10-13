rem @echo off
rem First rebuild Nlog and update BuildVersion in this script

rem install SHFB
..\..\tools\nuget.exe install EWSoftware.SHFB -excludeversion -OutputDirectory ..\..\tools\
..\..\tools\nuget.exe install EWSoftware.SHFB.NETFramework -excludeversion -OutputDirectory ..\..\tools\

set FRAMEWORK1=.NET Framework 4.5
set FRAMEWORK="%FRAMEWORK1%"
set BuildVersion=4.4

set Configuration=Release

msbuild.exe %~dp0NLog.shfbproj /p:Configuration=Release /p:Framework=%FRAMEWORK% /p:AssemblyName=NLog /p:BuildVersion=%BuildVersion% /p:BuildLabelOverride=NONE
rem copy favicon
copy favicon.ico ".\Doc\icons\favicon.ico"