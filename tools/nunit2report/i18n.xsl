<?xml version="1.0" encoding="iso-8859-1"?>
<xsl:stylesheet version="1.0"
xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

<xsl:output method="html" encoding="iso-8859-1" />

<xsl:param name="lang" />

  <xsl:variable name="traduc"
  select="document('Traductions.xml')" />

  <xsl:template match="text()">
    <xsl:variable name="key"
    select="substring-after(substring-after(../@id, ':'), ':')" />

	<xsl:choose>
      <xsl:when test="contains(../@id, 'i18n') and $lang!=''">
        <xsl:value-of select="$traduc//Traduction[@key=$key and @lang=$lang]" />
      </xsl:when>
      <xsl:otherwise>
       <xsl:copy-of select="." />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="*">
    <xsl:copy>
	  <!--  Copy attributs <> of id  -->
      <xsl:copy-of select="@*[name() != 'id']" />

	<!--  special case of id attribut -->
	<xsl:choose>
		<xsl:when test="contains(@id, 'i18n')">
		</xsl:when>
		<xsl:otherwise>
			<xsl:copy-of select="@id" />
		</xsl:otherwise>
	</xsl:choose>

      <xsl:apply-templates select="node()" />
    </xsl:copy>
  </xsl:template>
</xsl:stylesheet>

