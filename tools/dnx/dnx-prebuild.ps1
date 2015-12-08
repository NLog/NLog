[CmdletBinding()]
param([string]$dnxVersion = "latest")

# bootstrap DNVM into this session.
&{$Branch='dev';iex ((new-object net.webclient).DownloadString('https://raw.githubusercontent.com/aspnet/Home/dev/dnvminstall.ps1'))}

# install DNX
# ex: & $env:USERPROFILE\.dnx\bin\dnvm install $dnxVersion -r coreclr
& $env:USERPROFILE\.dnx\bin\dnvm install $dnxVersion -runtime CoreCLR -arch x86
& $env:USERPROFILE\.dnx\bin\dnvm install $dnxVersion -runtime CLR     -arch x86
& $env:USERPROFILE\.dnx\bin\dnvm install $dnxVersion -runtime CoreCLR -arch x64
& $env:USERPROFILE\.dnx\bin\dnvm install $dnxVersion -runtime CLR     -arch x64

 # run DNU restore on all project.json files in the src folder including 2>1 to redirect stderr to stdout for badly behaved tools
Get-ChildItem -Path $PSScriptRoot\src -Filter project.json -Recurse | ForEach-Object { Write-Verbose "Running dnu restore $_.FullName"; & dnu restore $_.FullName 2>1 }
# do the same on tests folder
Get-ChildItem -Path $PSScriptRoot\tests -Filter project.json -Recurse | ForEach-Object { Write-Verbose "Running dnu restore $_.FullName"; & dnu restore $_.FullName 2>1 }