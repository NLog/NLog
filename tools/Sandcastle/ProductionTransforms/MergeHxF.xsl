<?xml version="1.0"?>
<xsl:stylesheet
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:ds="http://ddue.schemas.microsoft.com/authoring/2003/5"
  version="1.1">

  <xsl:output doctype-system="MS-Help://Hx/Resources/HelpFileList.dtd" indent="yes" encoding="UTF-8" />

  <xsl:param name="hxfToMerge" />

  <xsl:template match="/">
    <xsl:for-each select="*">
      <xsl:copy>
        <xsl:copy-of select="@*"/>
        <xsl:copy-of select="File"/>
        <xsl:copy-of select="document($hxfToMerge)//File"/>
      </xsl:copy>
    </xsl:for-each>
  </xsl:template>

  <!--
        <xsl:for-each select="document($hxfToMerge)//File">
          <xsl:copy-of select="."/>
        </xsl:for-each>
  <xsl:template match="File">
  </xsl:template>
  -->
</xsl:stylesheet>