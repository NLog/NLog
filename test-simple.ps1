dotnet restore .\tests\SampleExtensions\
dotnet restore .\tests\NLog.UnitTests\
dotnet build .\tests\SampleExtensions --configuration release 
dotnet build .\tests\NLog.UnitTests\ --configuration release 
dotnet test .\tests\NLog.UnitTests\ -f netcoreapp1.0
dir .\tests\NLog.UnitTests\bin\Release\net45

$dir =  @(get-item .\tests\NLog.UnitTests\bin\Release\net45\* | ?{ $_.PSIsContainer })[0].FullName


xunit.console.exe "${dir}\NLog.UnitTests.dll"
