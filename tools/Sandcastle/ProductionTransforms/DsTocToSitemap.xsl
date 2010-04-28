<?xml version="1.0" encoding="utf-8"?>

<!--  Doc Studio Contents to Sitemap Transform

      Invoke with: xsltransform TocToSitemap.xsl toc.xml [/arg:comments=comments-dir] /out:web.sitemap
      
      - topicInfo - specifies the directory where the comment files are stored. The default is
      'Temp\TopicInfo'.
-->

<xsl:stylesheet version="1.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
		xmlns:ddue="http://ddue.schemas.microsoft.com/authoring/2003/5">

  <xsl:param name="topicInfo" select="'Temp\TopicInfo'"/>
  
	<xsl:output indent="yes" encoding="utf-8"/>

  <xsl:namespace-alias stylesheet-prefix="ddue" result-prefix="#default"/>
  
	<xsl:key name="index" match="/apis/api" use="@id"/>

	<xsl:template match="/">
		<siteMap>
      <!--<siteMapNode>-->
		    <xsl:apply-templates select="/tableOfContents/topic"/>
      <!--</siteMapNode>-->
		</siteMap>
	</xsl:template>

	<xsl:template match="topic">
		<siteMapNode>
      <xsl:if test="@isCategoryOnly != 'True' or @url">
			  <xsl:attribute name="url">
			    <xsl:choose>
			      <xsl:when test="@url">
			        <xsl:value-of select="@url"/>
			      </xsl:when>
			      <xsl:otherwise>
              <xsl:value-of select="document(concat($topicInfo, '/', @id, '.xml'))//pageUrl[1]"/>
			      </xsl:otherwise>
			    </xsl:choose>
			  </xsl:attribute>
      </xsl:if>
      <xsl:call-template name="tocTitleAttr"/>
      <xsl:apply-templates/>
		</siteMapNode>
	</xsl:template>

	<xsl:template match="sharedTopic">
		<siteMapNode>
      <xsl:if test="@isCategoryOnly != 'True' or @url">
			  <xsl:attribute name="url">
          <xsl:choose>
            <xsl:when test="@url">
              <xsl:value-of select="@url"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="concat(document(concat($topicInfo, '/', @id, '.xml'))//pageUrl[1], '#', generate-id())"/>
            </xsl:otherwise>
          </xsl:choose>
			  </xsl:attribute>
      </xsl:if>
      <xsl:call-template name="tocTitleAttr"/>
      <xsl:apply-templates/>
		</siteMapNode>
	</xsl:template>
	
	<xsl:template name="tocTitleAttr">
    <xsl:attribute name="title">
      <xsl:choose>
        <xsl:when test="@title">
          <xsl:value-of select="@title"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="document(concat($topicInfo, '/', @id, '.xml'))//tableOfContentsTitle[1]"/>
        </xsl:otherwise>
      </xsl:choose>
      <!-- Not needed if the build uses the custom .cmp.xml files produced by GetTopicInfo.exe.
           Those files contain a copy of the title in the tableOfContentsTitle element as needed. 
      <xsl:if test="not(document(concat($topicInfo, '/', @id, '.xml'))//tableOfContentsTitle[1])">
        <xsl:value-of select="document(concat($topicInfo, '/', @id, '.xml'))//title[1]"/>
      </xsl:if>
      -->
    </xsl:attribute>
	</xsl:template>

</xsl:stylesheet> 
