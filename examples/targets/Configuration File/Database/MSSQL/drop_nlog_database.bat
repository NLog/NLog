@echo off
set DBNAME=NLogDatabase
if (%1)==() goto usage
cls
echo.
echo This will drop %DBNAME% database on %1
echo.
echo You can press Ctrl+C to quit now.
echo.
pause
osql -S %1 -E -n -i drop_nlog_database.sql
pause
goto quit

:usage
echo Usage: drop_nlog_database.bat HOSTNAME
echo This will drop %DBNAME% database on HOSTNAME

:quit
