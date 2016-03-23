rem @echo off
set FRAMEWORK1=.NET Framework 4.5
set FRAMEWORK="%FRAMEWORK1%"
set BuildVersion=4.3.0


set Configuration=Release

%WINDIR%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe %~dp0NLog.shfbproj /p:Configuration=Release /p:Framework=%FRAMEWORK% /p:AssemblyName=NLog /p:BuildVersion=%BuildVersion% /p:BuildLabelOverride=NONE
 rem copy favicon
copy favicon.ico  "..\..\build\bin\%configuration%\%FRAMEWORK1%\doc\icons\favicon.ico"