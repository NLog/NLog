
if(.\Test-XmlFile.ps1){
	Write-Output "Valid XSD"
}else {
	exit 400;
}

dotnet restore .\src\NLog\
if (-Not $LastExitCode -eq 0)
	{ exit $LastExitCode }

dotnet restore .\tests\NLogAutoLoadExtension\
if (-Not $LastExitCode -eq 0)
	{ exit $LastExitCode }

dotnet restore .\tests\SampleExtensions\
if (-Not $LastExitCode -eq 0)
	{ exit $LastExitCode }

msbuild /t:Build /p:Configuration=Release /p:DebugType=Full .\tests\NLogAutoLoadExtension\ /verbosity:minimal
if (-Not $LastExitCode -eq 0)
	{ exit $LastExitCode }

msbuild /t:Build /p:Configuration=Release /p:DebugType=Full .\tests\SampleExtensions\ /verbosity:minimal
if (-Not $LastExitCode -eq 0)
	{ exit $LastExitCode }

msbuild /t:Restore,Build /p:Configuration=Release /p:DebugType=Full .\tests\NLog.UnitTests\ /verbosity:minimal
if (-Not $LastExitCode -eq 0)
	{ exit $LastExitCode }

dotnet test .\tests\NLog.UnitTests\  --configuration release --framework netcoreapp2.0 --no-build
if (-Not $LastExitCode -eq 0)
	{ exit $LastExitCode }

msbuild /t:Build /p:targetFramework=net461 .\tests\NLog.UnitTests\ /p:Configuration=Release /p:DebugType=Full /p:TestTargetFramework=net35 /verbosity:minimal
if (-Not $LastExitCode -eq 0)
	{ exit $LastExitCode }

& ${env:xunit20}\xunit.console.x86.exe .\tests\NLog.UnitTests\bin\Release\net35\NLog.UnitTests.dll -appveyor -noshadow
if (-Not $LastExitCode -eq 0)
	{ exit $LastExitCode }

msbuild /t:Build /p:targetFramework=net461 .\tests\NLog.UnitTests\ /p:Configuration=Release /p:DebugType=Full /p:TestTargetFramework=net45 /verbosity:minimal
if (-Not $LastExitCode -eq 0)
	{ exit $LastExitCode }

& ${env:xunit20}\xunit.console.x86.exe .\tests\NLog.UnitTests\bin\Release\net45\NLog.UnitTests.dll -appveyor -noshadow
if (-Not $LastExitCode -eq 0)
	{ exit $LastExitCode }

msbuild /t:Build /p:Configuration=Debug /p:DebugType=Full .\tests\NLog.UnitTests\ /verbosity:minimal
if (-Not $LastExitCode -eq 0)
	{ exit $LastExitCode }

& ${env:xunit20}\xunit.console.x86.exe .\tests\NLog.UnitTests\bin\Debug\net461\NLog.UnitTests.dll -appveyor -noshadow
if (-Not $LastExitCode -eq 0)
	{ exit $LastExitCode }