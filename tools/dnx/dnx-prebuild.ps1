# bootstrap DNVM into this session.
&{$Branch='dev';iex ((new-object net.webclient).DownloadString('https://raw.githubusercontent.com/aspnet/Home/dev/dnvminstall.ps1'))}

# load up the global.json so we can find the DNX version
$globalJson = Get-Content -Path $PSScriptRoot\global.json -Raw -ErrorAction Ignore | ConvertFrom-Json -ErrorAction Ignore

if($globalJson)
{
    $dnxVersion = $globalJson.sdk.version
}
else
{
    Write-Warning "Unable to locate global.json to determine using 'latest'"
    $dnxVersion = "latest"
}

# install DNX
# ex: & $env:USERPROFILE\.dnx\bin\dnvm install $dnxVersion -r coreclr
& $env:USERPROFILE\.dnx\bin\dnvm install $dnxVersion -runtime CoreCLR -arch x86 -u
& $env:USERPROFILE\.dnx\bin\dnvm install $dnxVersion -runtime CLR     -arch x86 -u
& $env:USERPROFILE\.dnx\bin\dnvm install $dnxVersion -runtime CoreCLR -arch x64 -u
& $env:USERPROFILE\.dnx\bin\dnvm install $dnxVersion -runtime CLR     -arch x64 -u

 # run DNU restore on all project.json files in the src folder including 2>1 to redirect stderr to stdout for badly behaved tools
Get-ChildItem -Path $PSScriptRoot\src -Filter project.json -Recurse | ForEach-Object { Write-Verbose "Running dnu restore $_.FullName"; & dnu restore $_.FullName 2>1 }
# do the same on tests folder
Get-ChildItem -Path $PSScriptRoot\tests -Filter project.json -Recurse | ForEach-Object { Write-Verbose "Running dnu restore $_.FullName"; & dnu restore $_.FullName 2>1 }