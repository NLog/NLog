set NETFX_VERSION=%1
set DEST_DIR=%2
copy %~dp0\NLog.ComInteropTest.* %DEST_DIR%
echo %%WINDIR%%\Microsoft.NET\Framework\%NETFX_VERSION%\RegAsm.exe /nologo /unregister "%%~dp0NLog.dll" > %DEST_DIR%\Uninstall_NLog_ComInterop.cmd
echo exit /b %%ERRORLEVEL%% >> %DEST_DIR%\Uninstall_NLog_ComInterop.cmd
echo %%WINDIR%%\Microsoft.NET\Framework\%NETFX_VERSION%\RegAsm.exe /nologo /codebase "%%~dp0NLog.dll" /tlb > %DEST_DIR%\Install_NLog_ComInterop.cmd
echo exit /b %%ERRORLEVEL%% >> %DEST_DIR%\Install_NLog_ComInterop.cmd

