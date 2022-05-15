rem @echo off
rem First rebuild Nlog and update BuildVersion in this script

set FRAMEWORK1=.NET Framework 4.5
set FRAMEWORK="%FRAMEWORK1%"
set BuildVersion=4.4
set Configuration=Release

msbuild /t:restore %~dp0NLog.shfbproj /p:Configuration=Release
msbuild %~dp0NLog.shfbproj /p:Configuration=Release /p:Framework=%FRAMEWORK% /p:AssemblyName=NLog /p:BuildVersion=%BuildVersion%

rem copy favicon
copy favicon.ico ".\Doc\icons\favicon.ico"