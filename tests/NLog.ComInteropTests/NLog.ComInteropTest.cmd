@echo off
echo Registering NLog.ComInterop...
call "%~dp0Install_NLog_ComInterop.cmd" || exit /b 1
echo Running tests...
cscript //nologo "%~dp0NLog.ComInteropTest.js" "%~dp0NLog.ComInteropTest.config"
set RESULT=%ERRORLEVEL%
echo Exit code: %RESULT%.
echo Unregistering NLog.ComInterop...
call "%~dp0Uninstall_NLog_ComInterop.cmd" || exit /b 1
exit /b %RESULT%
