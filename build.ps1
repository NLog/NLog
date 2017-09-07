# restore and builds all projects as release.
# creates NuGet package at \artifacts
dotnet --version

# dotnet restore .\src\NLog\
msbuild /t:Restore /p:targetFrameworks='"net45;net40-client;net35;netstandard2.0;sl4;sl5;wp8;monoandroid44;xamarinios10"' .\src\NLog\

# dotnet pack .\src\NLog\  --configuration release --include-symbols -o ..\..\artifacts
msbuild /t:Pack /p:targetFrameworks='"net45;net40-client;net35;netstandard2.0;sl4;sl5;wp8;monoandroid44;xamarinios10"' /p:Configuration=Release /p:IncludeSymbols=true .\src\NLog\ /p:PackageOutputPath=..\..\artifacts

exit $LASTEXITCODE