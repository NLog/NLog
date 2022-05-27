@echo off

set BuildVersion=5.0

dotnet msbuild /t:restore,rebuild %~dp0\dll_to_doc /p:Configuration=Release /verbosity:minimal
dotnet msbuild /t:restore %~dp0NLog.shfbproj /p:Configuration=Release /verbosity:minimal
dotnet msbuild %~dp0NLog.shfbproj /p:Configuration=Release /p:Framework=%FRAMEWORK% /p:AssemblyName=NLog /p:BuildVersion=%BuildVersion%

rem copy favicon
copy favicon.ico ".\Doc\icons\favicon.ico"
