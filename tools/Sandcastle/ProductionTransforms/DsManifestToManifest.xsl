<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.1">

	<xsl:output indent="yes" encoding="utf-8" />

	<xsl:template match="/">
		<topics>
			<xsl:apply-templates select="/manifest/manifestExecution/assetDetail/fileAsset" />
		</topics>
	</xsl:template>

	<xsl:template match="fileAsset[@assetType='Topic']">
		<topic>
			<xsl:attribute name="id">
				<xsl:value-of select="@fileAssetGuid" />
			</xsl:attribute>
		</topic>
		<xsl:apply-templates select="topic" />
	</xsl:template>

	<xsl:template match="fileAsset" />

</xsl:stylesheet>
