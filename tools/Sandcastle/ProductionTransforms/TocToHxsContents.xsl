<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.1">

	<xsl:output indent="yes" encoding="UTF-8" doctype-system="MS-Help://Hx/Resources/HelpTOC.dtd" />

  <xsl:param name="segregated" />
  <xsl:param name="includeIds" />

  <xsl:template match="/">
		<HelpTOC DTDVersion="1.0">
			<xsl:apply-templates select="/topics" />
		</HelpTOC>
	</xsl:template>

	<xsl:template match="topic">
		<HelpTOCNode>
      <xsl:if test="boolean($includeIds)">
        <xsl:attribute name="Id">
          <xsl:value-of select="@id"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:choose>
        <xsl:when test="@file">
          <xsl:attribute name="Url">
            <xsl:choose>
              <xsl:when test="boolean($segregated)">
                <xsl:value-of select="concat('ms-help:/../',@project,'/html/',@file,'.htm')" />
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="concat('html\',@file,'.htm')" />
              </xsl:otherwise>
            </xsl:choose>
          </xsl:attribute>
        </xsl:when>
        <xsl:otherwise>
          <xsl:attribute name="Title">
            <xsl:value-of select="@id" />
          </xsl:attribute>
        </xsl:otherwise>
      </xsl:choose>
			<xsl:apply-templates />
		</HelpTOCNode>
	</xsl:template>

  <xsl:template match="stoc">
    <HelpTOCNode NodeType="TOC" Url="{@project}" />
  </xsl:template>

  <xsl:template match="stopic">
    <HelpTOCNode Url="ms-help:/../{@project}/html/{@file}.htm" Id="{@id}">
      <xsl:apply-templates />
    </HelpTOCNode>
  </xsl:template>
</xsl:stylesheet>
