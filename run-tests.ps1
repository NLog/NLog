dotnet restore ./tests/NLog.UnitTests/
if (-Not $LastExitCode -eq 0)
	{ exit $LastExitCode }

dotnet test ./tests/NLog.UnitTests/ --framework netcoreapp3.1 --configuration release --no-restore
if (-Not $LastExitCode -eq 0)
	{ exit $LastExitCode }

if ($isWindows -or $Env:WinDir)
{
	dotnet test ./tests/NLog.UnitTests/ --framework net461 --configuration release --no-restore
	if (-Not $LastExitCode -eq 0)
		{ exit $LastExitCode }

	dotnet test ./tests/NLog.Database.Tests/ --configuration release
	if (-Not $LastExitCode -eq 0)
		{ exit $LastExitCode }

	dotnet test ./tests/NLog.WindowsRegistry.Tests/ --configuration release
	if (-Not $LastExitCode -eq 0)
		{ exit $LastExitCode }

	dotnet msbuild /t:Build /p:targetFramework=net461 /p:Configuration=Release /p:DebugType=Full /p:TestTargetFramework=net35 ./tests/NLog.UnitTests/
	if (-Not $LastExitCode -eq 0)
		{ exit $LastExitCode }
	dotnet vstest ./tests/NLog.UnitTests/bin/release/net35/NLog.UnitTests.dll
	if (-Not $LastExitCode -eq 0)
		{ exit $LastExitCode }

	dotnet msbuild /t:Build /p:targetFramework=net461 /p:Configuration=Release /p:DebugType=Full /p:TestTargetFramework=net45 ./tests/NLog.UnitTests/
	if (-Not $LastExitCode -eq 0)
		{ exit $LastExitCode }
	dotnet vstest ./tests/NLog.UnitTests/bin/release/net45/NLog.UnitTests.dll
	if (-Not $LastExitCode -eq 0)
		{ exit $LastExitCode }

	dotnet list ./src package --vulnerable --include-transitive | findstr /S /c:"has the following vulnerable packages"
	if (-Not $LastExitCode -eq 1)
	{
		dotnet list ./src package --vulnerable --include-transitive
		exit 1
	}
}
else
{
	dotnet test ./tests/NLog.Database.Tests/ --framework netcoreapp3.1 --configuration release
	if (-Not $LastExitCode -eq 0)
		{ exit $LastExitCode }

	# Need help from MONO to run normal .NetFramework tests
	dotnet msbuild /t:restore ./tests/NLog.UnitTests/ /p:RestoreForce=true /p:monobuild=1
	if (-Not $LastExitCode -eq 0)
		{ exit $LastExitCode }
	dotnet build ./tests/NLog.UnitTests/ --framework net461 --configuration release --no-restore --no-incremental /p:monobuild=1
    if (-Not $LastExitCode -eq 0)
	    { exit $LastExitCode }

	dotnet vstest ./tests/NLog.UnitTests/bin/release/net461/NLog.UnitTests.dll
    if (-Not $LastExitCode -eq 0)
	    { exit $LastExitCode }
}