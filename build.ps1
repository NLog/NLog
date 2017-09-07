# restore and builds all projects as release.
# creates NuGet package at \artifacts
dotnet --version

# dotnet restore .\src\NLog\
msbuild /t:Restore /p:targetFrameworks='"net45;net40-client;net35;netstandard2.0;sl4;sl5;wp8;monoandroid44;xamarinios10"' .\src\NLog\ /verbosity:minimal
if (-Not $LastExitCode -eq 0)
	{ exit $LastExitCode }

# dotnet pack .\src\NLog\  --configuration release --include-symbols -o ..\..\artifacts
msbuild /t:Pack /p:targetFrameworks='"net45;net40-client;net35;netstandard2.0;sl4;sl5;wp8;monoandroid44;xamarinios10"' /p:Configuration=Release /p:IncludeSymbols=true .\src\NLog\ /p:PackageOutputPath=..\..\artifacts /verbosity:minimal
if (-Not $LastExitCode -eq 0)
	{ exit $LastExitCode }

msbuild /t:Restore /p:targetFrameworks='"net45;net40-client;net35"' .\src\NLog.Extended\ /verbosity:minimal
if (-Not $LastExitCode -eq 0)
	{ exit $LastExitCode }

msbuild /t:Pack /p:targetFrameworks='"net45;net40-client;net35"' /p:Configuration=Release /p:IncludeSymbols=true .\src\NLog.Extended\ /p:PackageOutputPath=..\..\artifacts /verbosity:minimal
if (-Not $LastExitCode -eq 0)
	{ exit $LastExitCode }

exit $LastExitCode