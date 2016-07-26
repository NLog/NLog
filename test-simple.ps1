dotnet restore .\tests\SampleExtensions\
dotnet restore .\tests\NLog.UnitTests\
dotnet build .\tests\SampleExtensions --configuration release 
dotnet build .\tests\NLog.UnitTests\ --configuration release 


write-output "-----start xunit .NET Core-----"
dotnet test .\tests\NLog.UnitTests\ -f netcoreapp1.0  --configuration release 
write-output "-----end xunit .NET Core-----"

dir .\tests\NLog.UnitTests\bin\Release\net45
dir .\tests\NLog.UnitTests\bin\Release\net45\win7-x64

$dir =  @(get-item .\tests\NLog.UnitTests\bin\Release\net45\* | ?{ $_.PSIsContainer })[0].FullName




#version print
.\tools\xunit22\xunit.console.exe

write-output "-----start xunit .NET 4.5-----"
.\tools\xunit22\xunit.console.exe "${dir}\NLog.UnitTests.dll" 
write-output "-----end xunit .NET 4.5-----"