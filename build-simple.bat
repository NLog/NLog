rem restore and builds all projects as release.
rem creates NuGet package at \src\NLog\bin\Release
call dotnet restore .\src\NLog\ 
call dotnet restore .\src\NLog.Extended\ 
call dotnet restore .\src\NLogAutoLoadExtension\ 
call dotnet pack .\src\NLog\  --configuration release 
call dotnet build .\src\NLog.Extended\  --configuration release 
call dotnet build .\src\NLogAutoLoadExtension\  --configuration release 