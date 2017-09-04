# restore and builds all projects as release.
# creates NuGet package at \artifacts
dotnet --version

dotnet restore .\src\NLog\
dotnet pack .\src\NLog\  --configuration release -o artifacts

exit $LASTEXITCODE