dotnet restore .\tests\SampleExtensions\
dotnet restore .\tests\NLog.UnitTests\
dotnet build .\tests\SampleExtensions --configuration release 
dotnet build .\tests\NLog.UnitTests\ --configuration release 

dotnet test .\tests\NLog.UnitTests\  --configuration release 
