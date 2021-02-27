function Test-XmlFile
{
    <#
    from: https://stackoverflow.com/a/16618560/201303

    .Synopsis
        Validates an xml file against an xml schema file.
    .Example
        PS> dir *.xml | Test-XmlFile schema.xsd
    #>
    [CmdletBinding()]
    param (     
        [Parameter(Mandatory=$true)]
        [string] $SchemaFile,

        [Parameter(ValueFromPipeline=$true, Mandatory=$true, ValueFromPipelineByPropertyName=$true)]
        [alias('Fullname')]
        [string] $XmlFile,

        [scriptblock] $ValidationEventHandler = { Write-Error $args[1].Exception }
    )

    begin {
        $schemaReader = New-Object System.Xml.XmlTextReader $SchemaFile
        $schema = [System.Xml.Schema.XmlSchema]::Read($schemaReader, $ValidationEventHandler)
    }

    process {
        $ret = $true
        try {
            $xml = New-Object System.Xml.XmlDocument
            $xml.Schemas.Add($schema) | Out-Null
            $xml.Load($XmlFile)
            $xml.Validate({
                    throw ([PsCustomObject] @{
                        SchemaFile = $SchemaFile
                        XmlFile = $XmlFile
                        Exception = $args[1].Exception
                    })
                })
        } catch {
            Write-Error $_
            $ret = $false
        }
        $ret
    }

    end {
        $schemaReader.Close()
    }
}

# Needs absolute paths. Will throw a error if one of the files is not found
$pwd = get-location;

# Returns true if valid
return Test-XmlFile "$pwd\src\NLog\bin\Release\NLog.xsd" "$pwd\examples\targets\Configuration File\Null\NLog.config"
