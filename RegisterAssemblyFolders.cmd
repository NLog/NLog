@echo 
set REG=%WINDIR%\System32\reg.exe
if (%PROCESSOR_ARCHITECTURE%)==(AMD64) set REG=%WINDIR%\SysWow64\reg.exe

set BASEDIR=%~dp0build\bin\Debug
echo Registering NLog assembly folders (using %REG%)
set ROOT_HIVE=HKEY_LOCAL_MACHINE
%REG% add "%ROOT_HIVE%\SOFTWARE\Microsoft\.NETFramework\v2.0.50727\AssemblyFoldersEx\NLog2.0" /ve /d "%BASEDIR%\NetFx2.0" /f
%REG% add "%ROOT_HIVE%\SOFTWARE\Microsoft\.NETFramework\v2.0.50727\AssemblyFoldersEx\NLog2.0.Client" /ve /d "%BASEDIR%\NetFx2.0.Client" /f
%REG% add "%ROOT_HIVE%\SOFTWARE\Microsoft\.NETFramework\v3.5\AssemblyFoldersEx\NLog3.5" /ve /d "%BASEDIR%\NetFx3.5" /f
%REG% add "%ROOT_HIVE%\SOFTWARE\Microsoft\.NETFramework\v3.5\AssemblyFoldersEx\NLog3.5.Client" /ve /d "%BASEDIR%\NetFx3.5.Client" /f
%REG% add "%ROOT_HIVE%\SOFTWARE\Microsoft\.NETCompactFramework\v2.0.0.0\PocketPC\AssemblyFoldersEx\NLogCF2.0" /ve /d "%BASEDIR%\NetCF2.0" /f
%REG% add "%ROOT_HIVE%\SOFTWARE\Microsoft\.NETCompactFramework\v2.0.0.0\Smartphone\AssemblyFoldersEx\NLogCF2.0" /ve /d "%BASEDIR%\NetCF2.0" /f
%REG% add "%ROOT_HIVE%\SOFTWARE\Microsoft\.NETCompactFramework\v2.0.0.0\WindowsCE\AssemblyFoldersEx\NLogCF2.0" /ve /d "%BASEDIR%\NetCF2.0" /f
%REG% add "%ROOT_HIVE%\SOFTWARE\Microsoft\.NETCompactFramework\v3.5.0.0\PocketPC\AssemblyFoldersEx\NLogCF3.5" /ve /d "%BASEDIR%\NetCF3.5" /f
%REG% add "%ROOT_HIVE%\SOFTWARE\Microsoft\.NETCompactFramework\v3.5.0.0\Smartphone\AssemblyFoldersEx\NLogCF3.5" /ve /d "%BASEDIR%\NetCF3.5" /f
%REG% add "%ROOT_HIVE%\SOFTWARE\Microsoft\.NETCompactFramework\v3.5.0.0\WindowsCE\AssemblyFoldersEx\NLogCF3.5" /ve /d "%BASEDIR%\NetCF3.5" /f
%REG% add "%ROOT_HIVE%\SOFTWARE\Microsoft\Microsoft SDKs\Silverlight\v2.0\AssemblyFoldersEx\NLogSL2" /ve /d "%BASEDIR%\Silverlight2" /f
%REG% add "%ROOT_HIVE%\SOFTWARE\Microsoft\Microsoft SDKs\Silverlight\v3.0\AssemblyFoldersEx\NLogSL3" /ve /d "%BASEDIR%\Silverlight3" /f
