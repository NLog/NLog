@echo off

set BuildVersion=5.0

msbuild /t:restore,build %~dp0\..\NLog.sln /p:Configuration=Release /verbosity:minimal
msbuild /t:restore %~dp0NLog.shfbproj /p:Configuration=Release
msbuild %~dp0NLog.shfbproj /p:Configuration=Release /p:Framework=%FRAMEWORK% /p:AssemblyName=NLog /p:BuildVersion=%BuildVersion%

rem copy favicon
copy favicon.ico ".\Doc\icons\favicon.ico"