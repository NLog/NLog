C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe Installer.wixproj /p:Flavor=All /p:Configuration=Release || exit /b 1
msiexec /x {a93e5783-ae19-41cb-a99d-4b04de0b0192} /passive
msiexec /x {a93e5783-ae19-41cb-a99d-4b04de0b0193} /passive
msiexec /x {a93e5783-ae19-41cb-a99d-4b04de0b0194} /passive
msiexec /x {a93e5783-ae19-41cb-a99d-4b04de0b0195} /passive
msiexec /x {a93e5783-ae19-41cb-a99d-4b04de0b0196} /passive
msiexec /lv* log.txt /i D:\Work\NLog\build\bin\Release\Packages\NLog2-All-PrivateBuild.msi
echo ERRORLEVEL: %ERRORLEVEL%