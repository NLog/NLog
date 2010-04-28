
# Sandcastle build script overrides for prototype doc model.

. $DxRoot\Presentation\Shared\SharedDocModel.ps1

function PostProcessReflectionData($sourceFile, $targetFile) {
	WriteInfo "Post processing reflection data."
    &$XslTransform $sourceFile `
        /xsl:$DxRoot\ProductionTransforms\ApplyPrototypeDocModel.xsl `
        /xsl:$DxRoot\ProductionTransforms\AddGuidFilenames.xsl `
        /out:$targetFile
}

function CreateToc {
    WriteInfo "Creating TOC."
    &$XslTransform $TempDir\ReflectionData\targets.xml `
        /xsl:$DxRoot\ProductionTransforms\createPrototypeToc.xsl `
        /out:$TempDir\toc.xml
}

