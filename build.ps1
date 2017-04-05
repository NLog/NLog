# restore and builds all projects as release.
# creates NuGet package at \artifacts
dotnet --version

dotnet restore .\src\NLog\ 
dotnet restore .\src\NLog.Extended\ 
dotnet restore .\src\NLogAutoLoadExtension\ 
dotnet pack .\src\NLog\  --configuration release   -o artifacts
dotnet build .\src\NLog.Extended\  --configuration release 
dotnet build .\src\NLogAutoLoadExtension\  --configuration release 
