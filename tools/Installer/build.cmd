C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe Installer.wixproj /p:Flavor=All /p:Configuration=Release || exit /b 1
msiexec /x {a93e5783-ae19-41cb-a99d-4b04de0b0196} /passive
msiexec /i C:\Work\NLog\build\bin\Release\Setup\NLog2-All.msi /passive
