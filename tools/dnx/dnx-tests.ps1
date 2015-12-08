[CmdletBinding()]
param([string][Parameter(Mandatory=$true)]$projectJsonForTests,[string][Parameter(Mandatory=$true)]$configuration, [string][Parameter(Mandatory=$false)]$dnxVersion = "1.0.0-rc2-16249", [string][Parameter(Mandatory=$false)]$dnxRuntime = "CLR", [string][Parameter(Mandatory=$false)]$dnxArch = "x86")

$dnvm = "dnvm.ps1"

& $dnvm use $dnxVersion -runtime $dnxRuntime -arch $dnxArch

# check for solution pattern
if ($projectJsonForTests.Contains("*") -or $projectJsonForTests.Contains("?"))
{
    Write-Verbose "Pattern found in solution parameter. Calling Find-Files."
    Write-Verbose "Calling Find-Files with pattern: $projectJsonForTests"
    $projectJsonForTestsFiles = Get-ChildItem -Path $projectJsonForTests -Recurse | ?{ $_.fullname -notmatch "\\packages|obj|testresults\\?" }
    Write-Verbose "Found files: $projectJsonForTestsFiles"
}
else
{
    Write-Verbose "No Pattern found in solution parameter."
    $projectJsonForTestsFiles = ,$projectJsonForTests
}

ForEach($testProjectJsonFile in $projectJsonForTestsFiles)
{
	$testProjectDir = [System.IO.Path]::GetDirectoryName($testProjectJsonFile);
	Push-Location -Path "$testProjectDir"
	&dnx -p "$testProjectJsonFile" --configuration $configuration test

	Pop-Location
}