[CmdletBinding()]
param([string][Parameter(Mandatory=$true)]$projectJson,[string][Parameter(Mandatory=$true)]$configuration, [string][Parameter(Mandatory=$true)]$outputPath, [string][Parameter(Mandatory=$false)]$dnxVersion = "1.0.0-rc1-update1", [string][Parameter(Mandatory=$false)]$dnxRuntime = "CLR", [string][Parameter(Mandatory=$false)]$dnxArch = "x86")

$toolsPath = split-path $MyInvocation.MyCommand.Definition
$dnvm = "dnvm.ps1"

$solutionPath = [System.IO.Path]::GetFullPath($(join-path $toolsPath ".."))


& $dnvm use $dnxVersion -runtime $dnxRuntime -arch $dnxArch

# Restore packages and build
& dnu pack $projectJson --configuration $configuration --out "$outputPath"
