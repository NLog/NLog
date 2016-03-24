<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

	<xsl:template name="bibliographyReference">
		<xsl:param name="number" />
		<xsl:param name="data" />

		<xsl:if test="$data">
			<xsl:text>&#xa;</xsl:text>
			<xsl:text>&#xa;</xsl:text>
			<xsl:text>###### </xsl:text>
			<span>
				<xsl:attribute name="id">
					cite<xsl:value-of select="$number"/>
				</xsl:attribute>
				\[<xsl:value-of select="$number"/>\]
			</span>
			<xsl:text> **</xsl:text>
			<xsl:value-of select="$data/author/text()" />
			<xsl:text>**</xsl:text>
			<xsl:text>, </xsl:text>
			<xsl:text>*</xsl:text>
			<xsl:value-of select="$data/title/text()" />
			<xsl:text>*</xsl:text>
			<xsl:if test="$data/publisher">
				<xsl:text>, </xsl:text>
				<xsl:value-of select="$data/publisher/text()" />
			</xsl:if>
			<xsl:if test="$data/link">
				<xsl:text>, </xsl:text>
				<a>
					<xsl:attribute name="target">_blank</xsl:attribute>
					<xsl:attribute name="href">
						<xsl:value-of select="$data/link/text()" />
					</xsl:attribute>
					<xsl:value-of select="$data/link/text()" />
				</a>
			</xsl:if>
		</xsl:if>
	</xsl:template>

</xsl:stylesheet>
