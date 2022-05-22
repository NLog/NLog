@echo off

set BuildVersion=5.0

msbuild /t:restore,build %~dp0\dll_to_doc /p:RestorePackagesConfig=true /p:Configuration=Release /verbosity:minimal
msbuild /t:restore %~dp0NLog.shfbproj /p:Configuration=Release /verbosity:minimal
msbuild %~dp0NLog.shfbproj /p:Configuration=Release /p:Framework=%FRAMEWORK% /p:AssemblyName=NLog /p:BuildVersion=%BuildVersion%

rem copy favicon
copy favicon.ico ".\Doc\icons\favicon.ico"
