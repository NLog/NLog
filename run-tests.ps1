dotnet restore .\src\NLog\
if (-Not $LastExitCode -eq 0)
	{ exit $LastExitCode }

dotnet restore .\src\NLogAutoLoadExtension\
if (-Not $LastExitCode -eq 0)
	{ exit $LastExitCode }

dotnet restore .\tests\SampleExtensions\
if (-Not $LastExitCode -eq 0)
	{ exit $LastExitCode }

msbuild /t:Build /p:Configuration=Release .\src\NLogAutoLoadExtension\ /verbosity:minimal
if (-Not $LastExitCode -eq 0)
	{ exit $LastExitCode }

msbuild /t:Build /p:Configuration=Release .\tests\SampleExtensions\ /verbosity:minimal
if (-Not $LastExitCode -eq 0)
	{ exit $LastExitCode }

msbuild /t:Restore,Build /p:Configuration=Release .\tests\NLog.UnitTests\ /verbosity:minimal
if (-Not $LastExitCode -eq 0)
	{ exit $LastExitCode }

dotnet test .\tests\NLog.UnitTests\  --configuration release --framework netcoreapp2.0 --no-build
if (-Not $LastExitCode -eq 0)
	{ exit $LastExitCode }

msbuild /t:Build /p:targetFramework=net452 .\tests\NLog.UnitTests\ /p:Configuration=Release /p:TestTargetFramework=net35 /p:OutputPath=.\bin\release\net35 /verbosity:minimal
if (-Not $LastExitCode -eq 0)
	{ exit $LastExitCode }

& ${env:xunit20}\xunit.console.x86.exe .\tests\NLog.UnitTests\bin\release\net35\NLog.UnitTests.dll -appveyor -noshadow
if (-Not $LastExitCode -eq 0)
	{ exit $LastExitCode }

msbuild /t:Build /p:targetFramework=net452 .\tests\NLog.UnitTests\ /p:Configuration=Release /p:TestTargetFramework=net40-client /p:OutputPath=.\bin\release\net40 /verbosity:minimal
if (-Not $LastExitCode -eq 0)
	{ exit $LastExitCode }

& ${env:xunit20}\xunit.console.x86.exe .\tests\NLog.UnitTests\bin\release\net40\NLog.UnitTests.dll -appveyor -noshadow
if (-Not $LastExitCode -eq 0)
	{ exit $LastExitCode }

msbuild /t:Build /p:Configuration=Debug .\tests\NLog.UnitTests\ /verbosity:minimal
if (-Not $LastExitCode -eq 0)
	{ exit $LastExitCode }