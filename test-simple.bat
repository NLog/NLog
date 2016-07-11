call dotnet restore .\tests\SampleExtensions\
call dotnet restore .\tests\NLog.UnitTests\
call dotnet build .\tests\SampleExtensions --configuration release 
call dotnet build .\tests\NLog.UnitTests\ --configuration release
call dotnet test .\tests\NLog.UnitTests\
