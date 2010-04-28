<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.1">
  <!-- project name something like "fxref" -->
  <xsl:param name="projectName" />

  <!-- can be assembly or namespace for HXT generation -->
  <xsl:param name="createBy" />

  <!-- Maximum project length allowed. Below is the default provided by paorear, bug 230840 -->
  <xsl:param name="maxProjectNameLength" select="49" />
  
  <xsl:output indent="yes" encoding="UTF-8" doctype-system="MS-Help://Hx/Resources/HelpTOC.dtd" />
  
  <xsl:variable name="leftLength" select="$maxProjectNameLength div 2 - 1" />
  
  <xsl:variable name="rightLength" select="$maxProjectNameLength - $leftLength - 2" />
  
  <xsl:variable name="projectPrefix">
    <xsl:if test="boolean($projectName)">
      <xsl:value-of select="concat($projectName,'_')"/>
    </xsl:if>
  </xsl:variable>
   
  <xsl:template match="/">
    <HelpTOC DTDVersion="1.0">
      <xsl:choose>
        <xsl:when test="$createBy = 'assembly'">
          <xsl:apply-templates select="/reflection/assemblies/assembly">
            <xsl:sort select="@name" />
          </xsl:apply-templates>
        </xsl:when>
        <xsl:otherwise>
          <xsl:apply-templates select="/reflection/apis/api[apidata/@group='namespace']">
            <xsl:sort select="apidata/@name" />
          </xsl:apply-templates>
        </xsl:otherwise>
      </xsl:choose>
    </HelpTOC>
  </xsl:template>

  <!-- Apply the template for assembly level -->
  <xsl:template match="assembly">
    <xsl:variable name="componentName">
      <xsl:call-template name="GetComponentName">
        <xsl:with-param name="initialName" select="concat($projectPrefix, @name)" />
      </xsl:call-template>
    </xsl:variable>

    <HelpTOCNode NodeType="TOC" Url="{$componentName}" />
  </xsl:template>
  
  <!-- Apply the template for namespace level -->
  <xsl:template match="api[apidata/@group='namespace']">
    
    <xsl:variable name="componentName">
      
      <xsl:call-template name="GetComponentName">
        <xsl:with-param name="initialName">
          <xsl:choose>
            <xsl:when test="apidata/@name = ''">
              <xsl:value-of select="concat($projectPrefix, 'default_namespace')" />
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="concat($projectPrefix, apidata/@name)" />
            </xsl:otherwise>
          </xsl:choose>
        </xsl:with-param> 
      </xsl:call-template>
    </xsl:variable>
    
    <HelpTOCNode NodeType="TOC" Url="{$componentName}" />
    
  </xsl:template>

  <!-- logic to shorten component names if needed -->
  <xsl:template name="GetComponentName">
    <xsl:param name="initialName" />
    <xsl:variable name="componentNameLength" select="string-length($initialName)" />
    <xsl:choose>
      <xsl:when test="$componentNameLength > $maxProjectNameLength">
        <xsl:variable name="left" select="substring($initialName, 1, $leftLength)" />
        <xsl:variable name="right" select="substring($initialName, $componentNameLength - $rightLength)" />
        <xsl:value-of select="concat($left,'_',$right)" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$initialName" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  
</xsl:stylesheet>
