@echo off
set DBNAME=NLogDatabase
if (%1)==() goto usage
set LOGIN=
if (%2)==() set LOGIN=-E
cls
echo.
echo This will create %DBNAME% database on %1
echo.
echo You can press Ctrl+C to quit now.
echo.
pause
osql -S %1 %LOGIN% %2 %3 %4 %5 %6 -n -i create_nlog_database.sql
pause
goto quit

:usage
echo Usage: create_nlog_database.bat HOSTNAME
echo This will create %DBNAME% database on HOSTNAME

:quit
