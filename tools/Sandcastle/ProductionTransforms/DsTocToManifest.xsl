<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.1">

	<xsl:output indent="yes" encoding="utf-8" />

	<xsl:key name="index" match="/apis/api" use="@id" />

	<xsl:template match="/">
		<topics>
			<xsl:apply-templates select="/tableOfContents/topic" />
		</topics>
	</xsl:template>

	<xsl:template match="topic">
		<topic>
			<xsl:attribute name="id">
				<xsl:value-of select="@id" />
			</xsl:attribute>
		</topic>
		<xsl:apply-templates select="topic" />
	</xsl:template>

</xsl:stylesheet>
