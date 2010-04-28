#########################################################################################
#
#	scbuild.ps1 -- Generic Sandcastle Build Script
#
#	SYNOPSIS
#		scbuild.ps1 <options>
#
#	OPTIONS
#		-BuildAssemblerConfig {file} -- user specified BuildAssembler config file for
#			non-website builds. Use -WebBuildConfig to specify the config file for web
#			builds.
#			Default: The config file specified by the doc model.
#
#		-BuildChm, -BuildHxs, -BuildWebsite -- select build target.
#
#		-Clean -- remove Sandcastle temporary directory. Use this option to clean up
#			temporary files and for a clean build, including framework dependencies.
#
#		-Config {file} -- read options from config file. Options specified on the 
#			command line take precedence over options in the config file.
#
#		-Dependencies {files} -- comma separated list of other dependencies (other 
#			than the framework).
#
#		-DxRoot {path} -- alternate DxRoot. Default: DxRoot environment variable.
#
#		-Framework {version} -- build depends on framework version specified. Currently
#			supported: 2.0, 3.0, 3.5. Default: no framework dependencies.
#
#		-Lcid {version} -- locale ID for help file. Default: 1033
#
#		-Mixin {file} -- PowerShell script that gets loaded after initialization and
#			after the doc model has been loaded. It can be used to add functionality 
#			and to override functions in this script, except Init.
#
#		-Name {path-name} -- name of output file (or directory for Website builds).
#			Default: The name and location of the first assembly listed.
#
#		-ScriptSharp -- remove topics that do not apply for Script#
#
#		-Sources {files} -- comma separated list of files that should be added to the
#			output, both assemblies and related comment files. To build, you must
#			specify at least one assembly and comment file.
#
#		-Style {name} -- name of supported presentation style. Default: vs2005.
#
#		-TempDir {path} -- alternate location for the Sandcastle temporary directory.
#			Default: .\SandcastleTemp
#
#		-WebBuildConfig {file} -- user specified BuildAssembler config file for website
#			builds. Default: The config file specified by the doc model.
#
#		-WebTemplate (path) -- all files from the folder will be copied to the website
#			output directory. Default: The website folder underneath the selected
#			presentation folder, $env:DxRoot\Presentation\$Style\website\*
#
#	EXAMPLE
#		To build a help file for the assembly named test.dll, compile your project 
#		with comment file generation. Then with the files test.dll and comments.xml,
#		run the following command. The help file will be named test.chm.
#
#			scbuild -framework 2.0 -sources test.dll,comments.xml -BuildChm
#
#		To build a website use:
#			scbuild -framework 2.0 -sources test.dll,comments.xml -BuildWebsite
#
#   PREREQUISITES
#       - Sandcastle
#       - .NET 2.0
#       - PowerShell 1.0
#		- hhc.exe -- to compile CHM files
#		- hxcomp.exe -- to compile HxS files
#
#########################################################################################

param (
    # Actions
    [Switch]$BuildChm,
    [Switch]$BuildHxS,
    [Switch]$BuildWebsite,
    [Switch]$Clean,
    [Switch]$Test,	

    # Resources, Folders, Options
    [String]$BuildAssemblerConfig,
    [String]$Config,
    [Object[]]$Dependencies,
    [String]$DxRoot,
    [String]$Framework,          
    [String]$Lcid,
    [String]$Mixin,       
    [String]$Name,                     
    [Object[]]$Sources,
    [String]$Style,                    
    [String]$TempDir,
    [String]$WebBuildConfig,
    [String]$WebTemplate,
    [Switch]$ScriptSharp
    
)

#
# Framework locations.
#
$FrameworkDirs = @{
    "2.0" = "$env:SystemRoot\Microsoft.NET\Framework\v2.0.50727";
    "3.0" = "$env:SystemRoot\Microsoft.NET\Framework\v2.0.50727", 
			"$env:SystemRoot\Microsoft.NET\Framework\v3.0",
			"$env:ProgramFiles\Reference Assemblies\Microsoft\Framework\v3.0";
    "3.5" = "$env:SystemRoot\Microsoft.NET\Framework\v2.0.50727", 
			"$env:SystemRoot\Microsoft.NET\Framework\v3.0",
			"$env:ProgramFiles\Reference Assemblies\Microsoft\Framework\v3.0",
			"$env:SystemRoot\Microsoft.NET\Framework\v3.5",
			"$env:ProgramFiles\Reference Assemblies\Microsoft\Framework\v3.5";
}


#
# Init -- Process command line options and config file. Options specified on the command 
# line override the options specified in the config file. Reasonable defaults are used 
# if we can guess. Also, some rudimentary error checking.
#
function Init {
    # Sync .NET current directory with PowerShell (required for [String]::GetFullPath())
    [IO.Directory]::SetCurrentDirectory($pwd)
    
    # Read options from the config file.
    if ($Config) {
        if (test-path $Config) {
            . $Config
        }
        else {
            FatalError "Config file does not exist: $Config"
        }
    }
    
    # Initialize options: Config file takes precedence over defaults; command line
    # takes precedence over config file. Also, perform some basic sanity checks
    # before spending a lot of time in the build script.
    
    InitOption DxRoot $env:DxRoot
    if (-not $Script:DxRoot) {
        FatalError "You must specify a value for DxRoot. It should point to the root folder of your Sandcastle installation."
    }
    $Script:MrefBuilder = "$($Script:DxRoot)\ProductionTools\MrefBuilder.exe"
    $Script:XslTransform = "$($Script:DxRoot)\ProductionTools\XslTransform.exe"
    $Script:BuildAssembler = "$($Script:DxRoot)\ProductionTools\BuildAssembler.exe"
    $Script:ChmBuilder = "$($Script:DxRoot)\ProductionTools\ChmBuilder.exe"
    $Script:DBCSFix = "$($Script:DxRoot)\ProductionTools\DBCSFix.exe"
    
    if (-not (test-path $Script:MrefBuilder)) {
		FatalError "DxRoot does not point to a valid Sandcastle installation: $($DxRoot))"
    }
    
    # Make sure we have the help compiler if we're building a CHM.
    if ($BuildChm -and -not (get-command hhc)) {
        WriteInfo 'No help compiler found. Make sure hhc.exe is installed and in your PATH varaible.'
        WriteInfo 'You can download hhc from: http://msdn2.microsoft.com/en-us/library/ms669985.aspx'
        FatalError 'Cannot proceed without a help compiler.'
    }
    $Script:HHC = "hhc"
    
    # Make sure we have hxcomp.exe  if we're building a HxS.
    if ($BuildHxS -and -not (get-command hxcomp)) {
        WriteInfo 'HxS compiler not found. Make sure hxcomp.exe is installed and in your PATH variable.'
        WriteInfo 'You can download hxcomp from: http://www.microsoft.com/downloads/details.aspx?FamilyID=51a5c65b-c020-4e08-8ac0-3eb9c06996f4&DisplayLang=en'
        FatalError 'Cannot proceed without hxcomp.exe.'
    }
    $Script:HxComp = "hxcomp"
    
    InitOption Clean $false
    InitOption BuildChm $false
    InitOption BuildHxS $false
    InitOption BuildWebSite $false
    InitOption Test $false
    InitOption ScriptSharp $false
    
    if (-not ($Clean -or $BuildChm -or $BuildHxs -or $BuildWebsite -or $Test)) {
		FatalError "You must specify a build action: -Clean, -BuildChm, -BuildHxs, -BuildWebsite."
    }

    InitOption Dependencies @()
    $Script:Dependencies = ExpandWildcards $Script:Dependencies
    
    InitOption Framework ""
    if ($Script:Framework -and (-not $FrameworkDirs[$Script:Framework])) {
        FatalError "Unknown framework version: $($Script:Framework)"
    }
    
    InitOption Lcid "1033"
    
    InitOption Mixin ""

	InitOption Name ""
	
    InitOption Style vs2005
    if ($Script:Style -and -not (test-path "$Script:DxRoot\Presentation\$($Style)\DocModel.ps1")) {
		FatalError "Unknown presentation style: $($Script:Style)"
    }

	InitOption BuildAssemblerConfig "$DxRoot\Presentation\$Style\Configuration\sandcastle-scbuild.config"
	$Script:BuildAssemblerConfig = [IO.Path]::GetFullPath($Script:BuildAssemblerConfig)

	InitOption WebBuildConfig "$DxRoot\Presentation\$Style\Configuration\sandcastle-webref.config"
	$Script:WebBuildConfig = [IO.Path]::GetFullPath($Script:WebBuildConfig)

	InitOption Sources @()
	$Script:Sources = ExpandWildcards $Script:Sources
	if ($Script:Sources) {
		$Script:Comments = @()
		$Script:Targets = @()
		foreach ($src in $Script:Sources) {
			switch -regex ($src) {
				'.*\.xml$' {
						$Script:Comments += $src
					}
				'.*\.(dll|exe)$' {
						$Script:Targets += $src
					}
				default {
					FatalError "Unknown source file type: $src"
				}
			}
		}
	}
    
    InitOption TempDir "."
    $Script:TempDir = [IO.Path]::GetFullPath("$($Script:TempDir)\SandcastleTemp")
    
    if ($BuildChm -or $BuildHxS -or $BuildWebsite) {
        if (-not $Targets) {
            FatalError "To generate documentation, you must specify one or more target assemblies."
        }
        if (-not $Comments) {
            FatalError "To generate documentation, you must specify one or more comments files."
        }
    }
    
	if (-not $Script:Name -and $Script:Targets) {
		$Script:Name = [IO.Path]::ChangeExtension($Script:Targets[0].FullName, '')
	}
    
    if ($Script:Name) {
		$sn = [IO.Path]::GetFullPath($Script:Name)
		$Script:Name = [IO.Path]::GetFileNameWithoutExtension($sn)
		$Script:OutputDir = [IO.Path]::GetDirectoryName($sn)
    }
    
    $Script:WebOutputDir = "$($Script:OutputDir)\$($Script:Name)_website"
    
    InitOption WebTemplate "$DxRoot\Presentation\$Style\website"
   	$Script:WebTemplate = [IO.Path]::GetFullPath($Script:WebTemplate)
}


#
# Run Actions
#
function Run {
	WriteInfo 'Sandcastle build...'
	
	if ($BuildWebsite -and -not $DocModelSupportsWebBuild) {
		FatalError "The '$Style' style does not support web builds."
	}

	if ($Test) {
		Test
		exit 0
	}	
	
    if ($Clean) {
        Clean -all
    }
	elseif ($BuildChm -or $BuildHxs -or $BuildWebsite) {
        Clean
    }

    if ($BuildChm -or $BuildHxS -or $BuildWebsite) {
        MakeSandcastleDirs
    }

    if ($BuildChm) {
        BuildChm
    }
    
    if ($BuildHxS) {
        BuildHxs
    }
    
    if ($BuildWebsite) {
        BuildWebsite
    }
}


#
# Clean Out Generated Content
#
function Clean {
	param (
		[Switch]$All
	)
	if ($TempDir -match '.*(\\|/)SandcastleTemp$') {
		if (test-path $TempDir) {
			WriteInfo "Cleaning up temporary files."
			if ($All) {
				SafeDelete $TempDir
			}
			else {
				SafeDelete $TempDir\ReflectionData\*.xml
				SafeDelete $TempDir\ReflectionData\Dependencies
				SafeDelete $TempDir\Output
				SafeDelete $TempDir\Chm
				SafeDelete $TempDir\Intellisense
				SafeDelete $TempDir\Comments
				SafeDelete $TempDir\TopicInfo
			}
			SafeDelete $WebOutputDir
		}
    }
    else {
		FatalError "Cannot clean up temporary files, because the path name does not end in 'SandcastleTemp' as expected.`n+ Name: $TempDir"
    }
}


#
# MakeSandcastleDirs
#
function MakeSandcastleDirs {
    MakePath $TempDir\ReflectionData\Framework
    MakePath $TempDir\ReflectionData\Dependencies
    MakePath $TempDir\Comments
    MakePath $TempDir\Output
    MakePath $TempDir\TopicInfo
}


#
# Build CHM File
#
function BuildChm {
    WriteInfo "Building CHM file..."
    GenerateReflectionData
    GenerateManifest
    CreateOutputTemplate
    CopyComments
    RunBuildAssembler $BuildAssemblerConfig
    CreateToc
    CreateChmTemplate
    CreateChmProject
    CompileHelpProject
    CopyChmToOutputDir
}


#
# Build HxS File
#
function BuildHxs {
    WriteInfo "Building HxS file..."
    if (-not (test-path "$TempDir\ReflectionData\targets.xml")) {
		GenerateReflectionData
		GenerateManifest
		CreateOutputTemplate
		CopyComments
		RunBuildAssembler $BuildAssemblerConfig
		CreateToc
    }
	CreateHxsTemplate
	CreateHxsToc
	CreateHxsProject
	CompileHxsProject
	CopyHxsToOutputDir
}


#
# Build MSDN Style Website
#
function BuildWebsite {
    WriteInfo "Building website..."
    if (-not (test-path "$TempDir\ReflectionData\targets.xml")) {
		GenerateReflectionData
		GenerateManifest
		CopyComments
		CreateToc
    }
	CreateWebsiteTemplate
	RunBuildAssembler $WebBuildConfig
	CreateSitemap
}


#
# Generate reflection data if the current style does not match the style the caller is
# looking for.
#
# The list of dependencies is split up and seperated by a comma to be passed to mrefbuilder.
#
function GenerateReflectionData {
	$fdirs = $($FrameworkDirs[$Framework])
	if ($fdirs -is [String]) {
		$fdirs = @($fdirs)
	}	
		
	if($fdirs) {	
	    foreach ($fdir in $fdirs) {
	        GenerateDependencyReflectionData (get-childitem -r -include "*.dll" $fdir) $TempDir\ReflectionData\Framework "" $false
	    }
	}
	
    GenerateDependencyReflectionData $Dependencies "$TempDir\ReflectionData\Dependencies" $Dependencies $ScriptSharp
    GenerateTargetReflectionData $Targets "$TempDir\ReflectionData\targets.xml" $Dependencies $ScriptSharp
}


#
# GenerateDependencyReflectionData - generates reflection data for framework and other
# dependencies. The data is used as is from MrefBuilder. No post processing occurs. The
# file is generated only if the output doesn't exist already.
#
function GenerateDependencyReflectionData($assemblies, $outputDir, $dependencyAssemblies, $applyScriptSharp) {
    if ($assemblies -is [String]) {
        $assemblies = ExpandWildcards $assemblies
    }
    if ($assemblies) {
        foreach ($pn in $assemblies) {
			$outputFile = "$outputDir\$([IO.Path]::ChangeExtension($pn.Name, '.xml'))"
			if (-not (test-path $outputFile)) {
				GenerateTargetReflectionData $pn $outputFile $dependencyAssemblies $applyScriptSharp
				copy-item -ea SilentlyContinue $([IO.Path]::ChangeExtension($pn.FullName, '.xml')) $TempDir\Comments
            }
        }
    }
}


#
# GenerateTargetReflectionData - generates a reflection data file for the target
# assemblies. The data for all target assemblies is combined into a single file and then
# post processed using different transforms depending on doc style.
#
# Dependencies are passed to mrefbuilder if there are any.
#
function GenerateTargetReflectionData($assemblies, $outputFile, $dependencyAssemblies, $applyScriptSharp) {
    if ($assemblies -is [String]) {
        $assemblies = ExpandWildcards $assemblies
    }
    WriteInfo "Generate reflection data for: $assemblies"
    $targetFiles = [String]::Join(" ", ($assemblies | foreach {"`"$_`""}))
    $tmpName = "$TempDir\tmp.xml"

	if ($dependencyAssemblies) {
		$dependencyFiles = [String]::Join(",", ($dependencyAssemblies | foreach {"`"$_`""}))
		&$MrefBuilder /dep:$dependencyFiles /out:$tmpName $targetFiles
	} else {
		&$MrefBuilder /out:$tmpName $targetFiles
	}

	if ($applyScriptSharp)	{
		$tmpFixedName = "$TempDir\tmp-Fixed.xml"
		ApplyFixScriptSharp $tmpName $tmpFixedName		        
		SafeDelete $tmpName
		$tmpName = $tmpFixedName
	}
		
    PostProcessReflectionData $tmpName $outputFile
	SafeDelete $tmpName
}

#
# PostProcessReflectionData sub -- the Doc Model implementation should run any post processing
# transforms required.
#
function PostProcessReflectionData {
	FatalError "Doc Model must define a PostProcessReflectionData function."
}

#
# ScriptSharp requires an additional transform to remove unneeded elements.
#
function ApplyFixScriptSharp($tmpName, $tmpFixedName) {
	&$XslTransform $tmpName `
        /xsl:$DxRoot\ProductionTransforms\FixScriptSharp.xsl `
        /out:$tmpFixedName
}


#
# GenerateManifest() -- creates a manifest from the reflection data. This requires
# all of the reflection data in a single XML file.
#
function GenerateManifest {
    WriteInfo "Generating manifest..."
    &$XslTransform $TempDir\ReflectionData\targets.xml `
        /xsl:$DxRoot\ProductionTransforms\ReflectionToManifest.xsl `
        /out:$TempDir\manifest.xml
}


#
# CreateToc -- an override for this function is defined in DocModel.ps1. It should 
# create a TOC file from the reflection data.
#
function CreateToc {
	FatalError "Doc Model must define a CreateToc function."
}


#
# RunBuildAssembler
#
function RunBuildAssembler($configFile) {
    $env:DxTempDir = $TempDir
    $env:DxWebOutputDir = $WebOutputDir
    &$BuildAssembler $TempDir\manifest.xml `
        /config:$configFile
}

#
# CreateChmTemplate stub -- an override for this function is defined in DocModel.ps1. It
# creates a template for the build.
#
function CreateChmTemplate {
	FatalError "Doc Model must define a CreateChmTemplate function."
}


#
# CreateHxsTemplate stub -- the override for this function defined in DocModel.ps1
# creates a template needed for the HxS build.
#
function CreateHxsTemplate {
	FatalError "Doc Model must define a CreateHxsTemplate function."
}


#
# CreateChmProject
#
function CreateChmProject {
    WriteInfo "Creating CHM project file."
	&$ChmBuilder /project:$Name /html:$TempDir\Output\html /lcid:$Lcid /toc:$TempDir\toc.xml /out:$TempDir\chm

	if ($Style -eq "prototype") {
    &$XslTransform $TempDir\ReflectionData\targets.xml `
        /xsl:$DxRoot\ProductionTransforms\ReflectionToChmIndex.xsl `
        /out:"$TempDir\chm\$($Name).hhk"
	}

	&$DBCSFix /d:$TempDir\chm /l:$Lcid
}


#
# CopyComments
#
function CopyComments {
    WriteInfo "Copying comments..."
    foreach ($pn in $Comments) {
        WriteInfo "+ $pn"
        copy-item $pn $TempDir\Comments
    }
}


#
# CompileHelpProject
#        
function CompileHelpProject {
    WriteInfo "Compiling help project."
    &$HHC $TempDir\chm\$Name.hhp
}


#
# CopyChmToOutputDir
#
function CopyChmToOutputDir {
	WriteInfo "Copying CHM file to output directory: $OutputDir"
    MakePath $OutputDir
	copy-item -force "$TempDir\chm\$($Name).chm" $OutputDir
}


#
# CreateHxsToc
#
function CreateHxsToc {
    WriteInfo "Creating HXS TOC file."
	&$XslTransform $TempDir\toc.xml `
		/xsl:$DxRoot\ProductionTransforms\TocToHxSContents.xsl `
		/out:"$TempDir\Output\$($Name).HxT"
}


#
# CreateHxsProject
#
function CreateHxsProject {
	WriteInfo "Creating HxS project."
	&$XslTransform $TempDir\toc.xml `
		/xsl:$DxRoot\ProductionTransforms\CreateHxC.xsl `
		/arg:"fileNamePrefix=$($Name)" `
		/out:"$TempDir\Output\$($Name).HxC"
}


#
# CompileHxsProject
#
function CompileHxsProject {
    WriteInfo "Compiling help project."
    &$HxComp -p $TempDir\Output\$Name.HxC
}


#
# CopyHxsToOutputDir
#
function CopyHxsToOutputDir {
	WriteInfo "Copying HxS file to output directory: $OutputDir"
    MakePath $OutputDir
	copy-item -force "$TempDir\Output\$($Name).HxS" $OutputDir
}


#
# Test -- show options.
#
function Test {
	echo "*** TEST ONLY ***"
    echo "+ Clean: $Clean"
    echo "+ BuildChm: $BuildChm"
    echo "+ BuildHxs: $BuildHxs"
    echo "+ BuildWebsite: $BuildWebsite"
    echo "---"
    echo "+ BuildAssemblerConfig: $BuildAssemblerConfig"
    echo "+ [Comments]: $Comments"
    echo "+ Config: $Config"
    echo "+ Dependencies: $Dependencies"
    echo "+ DxRoot: $DxRoot"
    echo "+ Lcid: $Lcid"
    echo "+ Mixin: $Mixin"
    echo "+ Framework: $Framework -> $($FrameworkDirs[$Script:Framework])"
    echo "+ Mixin: $Mixin"
    echo "+ Name: $Name"
    echo "+ [OutputDir]: $OutputDir"
    echo "+ ScriptSharp: $ScriptSharp"
    echo "+ Sources: $Sources"
    echo "+ Style: $Style"
    echo "+ [Targets]: $Targets"
    echo "+ TempDir: $TempDir"
    echo "+ WebOutputDir: $WebOutputDir"
    echo "+ WebBuildConfig: $WebBuildConfig"
    echo "+ WebTemplate: $WebTemplate"
}
    

#========================================================================================
# Utility Functions
#========================================================================================

#
# Init option from config file or default, if not specified on the command line.
#
function InitOption($_name_, $_default_ = $null) {
    $_v_ = get-variable -scope script -name $_name_ -value
    if (-not $_v_) {
        $_v_ = get-variable -scope local -name $_name_ -value -ea SilentlyContinue
        if (-not $_v_) {
            $_v_ = $_default_
        }
        set-variable -scope script -name $_name_ -value $_v_
    }
}


#
# Expand wildcard characters in file names and return array of path names.
#
function ExpandWildcards($spec) {
    if ($spec -is [String]) {
        return get-childitem $spec
    }
    elseif ($spec -is [System.Collections.IEnumerable]) {
        $r = @()
        foreach ($s in $spec) {
            $r += (get-childitem $s)
        }
        return $r
    }
    else {
        return @()
    }
}


#
#	SafeDelete - this function attempts to deal with temporary file locks being held
#   by other programs on the files we're trying to delete.
#
#   NOTE: This function is *EXPERIMENTAL*. It does not time out and does not
#   distinguish between locking related errors and everything else that might cause a
#   permanent failure.
#
function SafeDelete($path) {
    $n = 0
    while (test-path $path) {
        if ($n -gt 0) {
            start-sleep -m 100
        }
        remove-item -r -force -ea SilentlyContinue $path
        $n++
        if (-not ($n % 10)) {
            WriteWarning "Waiting to delete $path ($([int]($n / 10)))"
        }
    }
}


#
# MakePath - create specified path unless it exists already.
#
function MakePath($path) {
    if (-not (test-path $path)) {
        [void](mkdir $path)
    }
}


function WriteInfo {
    foreach ($arg in $Args) {
        echo "$arg"
    }
}


function WriteDebug {
    foreach ($arg in $Args) {
        echo "DEBUG: $arg"
    }
}


function WriteWarning {
    foreach ($arg in $Args) {
        echo "WARNING: $arg"
    }
}


function FatalError {
    foreach ($arg in $Args) {
        echo "ERROR: $arg"
    }
    exit 1
}


#========================================================================================
# Init and Run Actions
#========================================================================================

Init
. $DxRoot\Presentation\$Style\DocModel.ps1


#
# Load user mix-ins.
#
if ($Mixin) {
	WriteInfo "Loading user mixins from: $Mixin"
	. $Mixin
}

Run