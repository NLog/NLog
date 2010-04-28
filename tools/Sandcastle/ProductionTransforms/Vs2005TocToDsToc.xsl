<?xml version="1.0" encoding="utf-8"?>

<!--  This transform turns a mref TOC produced by CreateVSToc.xsl into a format suitable for TocToSitemap.xsl
      We're also using this step to resolve topic titles and urls.

      Invoke with:
          xsltransform /xsl:FixMrefToc.xsl /arg:topicInfo=topic-info-dir /out:mreftoc.xml
-->

<xsl:stylesheet version="1.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:ddue="http://ddue.schemas.microsoft.com/authoring/2003/5">

    <xsl:param name="topicInfo" select="'Temp\TopicInfo'"/>
  
	<xsl:output indent="yes" encoding="utf-8"/>

    <xsl:namespace-alias stylesheet-prefix="ddue" result-prefix="#default"/>

	<xsl:template match="/topics">
		<tableOfContents>
      <xsl:apply-templates/>
		</tableOfContents>
	</xsl:template>

	<xsl:template match="topic">
		<topic>
            <xsl:attribute name="id">
		        <xsl:value-of select="@file"/>
            </xsl:attribute>
            <xsl:attribute name="url">
                <xsl:value-of select="document(concat($topicInfo, '/', @file, '.xml'))//pageUrl[1]"/>
            </xsl:attribute>
            <xsl:attribute name="title">
                <xsl:value-of select="document(concat($topicInfo, '/', @file, '.xml'))//title[1]"/>
            </xsl:attribute>
            <xsl:apply-templates/>
		</topic>
	</xsl:template>

</xsl:stylesheet> 
