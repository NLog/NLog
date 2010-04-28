<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.1">

	<xsl:param name="project" select="string('test')" />

	<xsl:output method="text" encoding="iso-8859-1" />

	<xsl:key name="index" match="/reflection/apis/api" use="@id" />

	<xsl:variable name="topic">
		<xsl:choose>
			<xsl:when test="/reflection/apis/api[apidata/@group='root']">
				<xsl:value-of select="/reflection/apis/api[apidata/@group='root'][1]/file/@name"/>
			</xsl:when>
			<xsl:when test="/reflection/apis/api[apidata/@group='namespace']">
				<xsl:value-of select="/reflection/apis/api[apidata/@group='namespace'][1]/file/@name"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="/reflection/apis/api[apidata/@group='type'][1]/file/@name"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:variable>

	<xsl:template match="/">
		<xsl:text>[OPTIONS]&#x0a;</xsl:text>
		<xsl:text>Compatibility=1.1 or later&#x0a;</xsl:text>
		<xsl:text>Compiled file=</xsl:text><xsl:value-of select="$project" /><xsl:text>.chm&#x0a;</xsl:text>
		<xsl:text>Contents file=</xsl:text><xsl:value-of select="$project" /><xsl:text>.hhc&#x0a;</xsl:text>
		<xsl:text>Index file=</xsl:text><xsl:value-of select="$project" /><xsl:text>.hhk&#x0a;</xsl:text>
		<xsl:text>Default Topic=html/</xsl:text><xsl:value-of select="$topic" /><xsl:text>.htm&#x0a;</xsl:text>
		<!-- <xsl:text>Display compile progress=No&#x0a;</xsl:text> -->
    <xsl:text>Full-text search=Yes&#x0a;</xsl:text>
    <xsl:text>Language=0x409 English (United States)&#x0a;</xsl:text>
	<xsl:text>Title=</xsl:text><xsl:value-of select="$project" /><xsl:text>&#x0a;</xsl:text>

		<xsl:text>[FILES]&#x0a;</xsl:text>
		<xsl:text>icons\*.gif&#x0a;</xsl:text>
    <xsl:text>art\*.gif&#x0a;</xsl:text>
    <xsl:text>media\*.gif&#x0a;</xsl:text>
    <xsl:text>scripts\*.js&#x0a;</xsl:text>
    <xsl:text>styles\*.css&#x0a;</xsl:text>
    <xsl:text>html\*.htm&#x0a;</xsl:text>

    <xsl:text>[INFOTYPES]&#x0a;</xsl:text>
	</xsl:template>

</xsl:stylesheet>
