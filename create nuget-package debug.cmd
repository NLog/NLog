ECHO off
Rem Displays messages or turns on or off the display of commands in a batch file.
rem create nuget package for NLog, debug config. 
rem Package will be in NLog\src\NLog\bin\Debug
rem version number will be from project.json

CALL dnvm install 1.0.0-rc1-update1

CALL dnu restore --quiet 
CALL dnu pack src/NLog --quiet --configuration Debug
