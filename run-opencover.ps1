  nuget.exe install OpenCover -ExcludeVersion
  OpenCover\tools\OpenCover.Console.exe -register:user -target:"%xunit20%\xunit.console.x86.exe" -targetargs:"\"C:\projects\nlog\tests\NLog.UnitTests\bin\debug\net452\NLog.UnitTests.dll\" -appveyor -noshadow"  -returntargetcode -filter:"+[NLog]* +[NLog.Extended]* -[NLog]JetBrains.Annotations.*" -excludebyattribute:*.ExcludeFromCodeCoverage* -hideskipped:All -output:coverage.xml
  "SET PATH=C:\\Python34;C:\\Python34\\Scripts;%PATH%"
  pip install codecov
  codecov -f "coverage.xml"