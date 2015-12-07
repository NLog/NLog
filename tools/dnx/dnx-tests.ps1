[CmdletBinding()]
param([string][Parameter(Mandatory=$true)]$projectJsonForTests,[string][Parameter(Mandatory=$true)]$configuration)


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