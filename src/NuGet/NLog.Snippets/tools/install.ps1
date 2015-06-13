param($installPath, $toolsPath, $package, $project)

$snippetFolder = "NLog"
$source = "$toolsPath\Snippets\*"
$vsVersions = @("2010", "2012")
Foreach ($vsVersion in $vsVersions)
{
	$myCodeSnippetsFolder = "$HOME\Documents\Visual Studio $vsVersion\Code Snippets\Visual C#\My Code Snippets\"
	if (Test-Path $myCodeSnippetsFolder)
	{
		$destination = "$myCodeSnippetsFolder$snippetFolder"
		if (!(Test-Path $destination))
		{
		  New-Item $destination -itemType "directory"
		}

		write-host ========================================================================================================================================================================
		write-host Copying snippets to $destination
		write-host 

		Copy-Item $source -Destination $destination -Recurse -Force

		write-host Snippets are available for every project in every solution!
		write-host
		write-host To uninstall snippets just remove $destination directory
		write-host ========================================================================================================================================================================
	}
}
uninstall-package NLog.Snippets -ProjectName $project.Name
