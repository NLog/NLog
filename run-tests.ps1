dotnet restore .\src\NLogAutoLoadExtension\
dotnet restore .\tests\SampleExtensions\
dotnet restore .\tests\NLog.UnitTests\

dotnet build .\src\NLogAutoLoadExtension\ --configuration release 
dotnet build .\tests\SampleExtensions\ --configuration release 
dotnet build .\tests\NLog.UnitTests\ --configuration debug
dotnet build .\tests\NLog.UnitTests\ --configuration release 

dotnet test .\tests\NLog.UnitTests\  --configuration release 
exit $LASTEXITCODE