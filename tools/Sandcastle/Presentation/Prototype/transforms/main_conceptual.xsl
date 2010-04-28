<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.1"
		xmlns:ddue="http://ddue.schemas.microsoft.com/authoring/2003/5"
		xmlns:MSHelp="http://msdn.microsoft.com/mshelp" >

	<xsl:output method="xml" indent="no" encoding="utf-8" />

	<xsl:include href="utilities_dduexml.xsl" />

	<!-- key parameter is the api identifier string -->
	<xsl:param name="key" />
  <xsl:param name="languages">false</xsl:param>

	<xsl:template match="/document">
		<html>
			<head>
				<title><xsl:call-template name="topicTitle"/></title>
				<xsl:call-template name="insertStylesheets" />
				<xsl:call-template name="insertScripts" />
				<xsl:call-template name="insertMetadata" />
			</head>
			<body>
				<xsl:call-template name="control"/>
				<xsl:call-template name="main"/>
			</body>
		</html>
	</xsl:template>

	<!-- document head -->

	<xsl:template name="insertStylesheets">
		<link rel="stylesheet" type="text/css" href="../styles/presentation.css" />
		<!-- make mshelp links work -->
		<link rel="stylesheet" type="text/css" href="ms-help://Hx/HxRuntime/HxLink.css" />
	</xsl:template>

	<xsl:template name="insertScripts">
    <script type="text/javascript">
      <includeAttribute name="src" item="scriptPath"><parameter>script_prototype.js</parameter></includeAttribute>
      <xsl:text> </xsl:text>
    </script>
    <script type="text/javascript">
      <includeAttribute name="src" item="scriptPath"><parameter>EventUtilities.js</parameter></includeAttribute>
      <xsl:text> </xsl:text>
    </script>
    <script type="text/javascript">
      <includeAttribute name="src" item="scriptPath"><parameter>StyleUtilities.js</parameter></includeAttribute>
      <xsl:text> </xsl:text>
    </script>
    <script type="text/javascript">
      <includeAttribute name="src" item="scriptPath"><parameter>SplitScreen.js</parameter></includeAttribute>
      <xsl:text> </xsl:text>
    </script>
    <script type="text/javascript">
      <includeAttribute name="src" item="scriptPath"><parameter>ElementCollection.js</parameter></includeAttribute>
      <xsl:text> </xsl:text>
    </script>
    <script type="text/javascript">
      <includeAttribute name="src" item="scriptPath"><parameter>MemberFilter.js</parameter></includeAttribute>
      <xsl:text> </xsl:text>
    </script>
    <script type="text/javascript">
      <includeAttribute name="src" item="scriptPath"><parameter>CollapsibleSection.js</parameter></includeAttribute>
      <xsl:text> </xsl:text>
    </script>
    <script type="text/javascript">
      <includeAttribute name="src" item="scriptPath"><parameter>LanguageFilter.js</parameter></includeAttribute>
      <xsl:text> </xsl:text>
    </script>
    <script type="text/javascript">
      <includeAttribute name="src" item="scriptPath"><parameter>CookieDataStore.js</parameter></includeAttribute>
      <xsl:text> </xsl:text>
    </script>

  </xsl:template>

	<xsl:template name="insertMetadata">
		<xml>
		<!-- mshelp metadata -->

      <!-- link index -->
  		<MSHelp:Keyword Index="A" Term="{$key}" />

		  <!-- authored K -->
      <xsl:for-each select="/document/metadata/keyword[@index='K']">
        <MSHelp:Keyword Index="K">
          <xsl:attribute name="Term">
            <xsl:value-of select="text()" />
            <xsl:for-each select="keyword[@index='K']">
              <xsl:text>, </xsl:text>
              <xsl:value-of select="text()"/>
            </xsl:for-each>
          </xsl:attribute>
        </MSHelp:Keyword>
      </xsl:for-each>

      <!-- authored F -->
      <xsl:for-each select="/document/metadata/keyword[@index='F']">
        <MSHelp:Keyword Index="F">
          <xsl:attribute name="Term">
            <xsl:value-of select="text()" />
            <xsl:for-each select="keyword[@index='F']">
              <xsl:text>, </xsl:text>
              <xsl:value-of select="text()"/>
            </xsl:for-each>
          </xsl:attribute>
        </MSHelp:Keyword>
      </xsl:for-each>

      <!-- authored B -->
      <xsl:for-each select="/document/metadata/keyword[@index='B']">
        <MSHelp:Keyword Index="B">
          <xsl:attribute name="Term">
            <xsl:value-of select="text()" />
            <xsl:for-each select="keyword[@index='B']">
              <xsl:text>, </xsl:text>
              <xsl:value-of select="text()"/>
            </xsl:for-each>
          </xsl:attribute>
        </MSHelp:Keyword>
      </xsl:for-each>

      <!-- Topic version -->
      <MSHelp:Attr Name="RevisionNumber" Value="{/document/topic/@revisionNumber}" />

      <!-- Asset ID -->
      <MSHelp:Attr Name="AssetID" Value="{/document/topic/@id}" />

      <!-- Abstract -->
      <xsl:variable name="abstract" select="string(/document/topic//ddue:para[1])" />
      <xsl:if test="(string-length($abstract) &lt; 255) and (string-length($abstract) &gt; 0)">
        <MSHelp:Attr Name="Abstract" Value="{$abstract}" />
      </xsl:if>
      
      <!-- authored attributes -->
      <xsl:for-each select="/document/metadata/attribute">
        <MSHelp:Attr Name="{@name}" Value="{text()}" />
      </xsl:for-each>
      
    </xml>
	</xsl:template>

	<!-- document body -->

	<!-- control window -->

	<xsl:template name="control">
		<div id="control">
			<span class="productTitle"><include item="productTitle" /></span><br/>
			<span class="topicTitle"><xsl:call-template name="topicTitle" /></span><br/>
      
      <xsl:if test="boolean(($languages != 'false') and (count($languages/language) &gt; 0))">
        <div id="toolbar">
          <span id="languageFilter">
            <select id="languageSelector" onchange="var names = this.value.split(' '); toggleVisibleLanguage(names[1]); switchLanguage(names, this.value);">
              <xsl:for-each select="$languages/language">
                <option value="{@name} {@style}">
                  <include item="{@label}Label" />
                </option>
              </xsl:for-each>
            </select>
          </span>
        </div>
      </xsl:if>
     
<!--
			<div id="toolbar">
				<span class="chickenFeet"><xsl:call-template name="chickenFeet" /></span>
			</div>
-->
		</div>
	</xsl:template>

	<!-- Title in topic -->

	<xsl:template name="topicTitle">
    <xsl:choose>
      <xsl:when test="normalize-space(/document/metadata/title)">
        <xsl:value-of select="normalize-space(/document/metadata/title)"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="normalize-space(/document/topic/*/ddue:title)"/>
      </xsl:otherwise>
    </xsl:choose>
	</xsl:template>

	<!-- Title in TOC -->

	<!-- Index entry -->

	<!-- main window -->

	<xsl:template name="main">
		<div id="main">
			<xsl:call-template name="head" />
			<xsl:call-template name="body" />
			<xsl:call-template name="foot" />
		</div>
	</xsl:template>

	<xsl:template name="head">
		<include item="header" />
	</xsl:template>

	<xsl:template name="body">
		<xsl:apply-templates select="topic" />
	</xsl:template> 

	<!-- sections that behave differently in conceptual and reference -->

	<xsl:template match="ddue:title">
		<!-- don't print title -->
	</xsl:template>

	<xsl:template match="ddue:introduction">
    <xsl:apply-templates select="@address" />
		<div class="introduction">
			<xsl:apply-templates />
		</div>
	</xsl:template>

	<xsl:template match="ddue:parameters">
		<xsl:call-template name="section">
			<xsl:with-param name="title"><include item="parametersTitle" /></xsl:with-param>
			<xsl:with-param name="content">
				<xsl:apply-templates />
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="ddue:returnValue">
		<xsl:call-template name="section">
			<xsl:with-param name="title"><include item="returnValueTitle" /></xsl:with-param>
			<xsl:with-param name="content">
				<xsl:apply-templates />
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="ddue:exceptions">
		<xsl:call-template name="section">
			<xsl:with-param name="title"><include item="exceptionsTitle" /></xsl:with-param>
			<xsl:with-param name="content">
				<xsl:apply-templates />
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="ddue:relatedSections">
		<div class="section">
			<div class="sectionTitle"><include item="relatedSectionsTitle" /></div>
			<div class="sectionContent">
				<xsl:apply-templates />
			</div>
		</div>
	</xsl:template>

	<xsl:template match="ddue:relatedTopics">
		<xsl:if test="count(*) > 0">
		<div class="section">
			<div class="sectionTitle"><include item="relatedTopicsTitle" /></div>
			<div class="sectionContent">
        <xsl:for-each select="*">
          <xsl:apply-templates select="." />
          <br />
        </xsl:for-each>
			</div>
		</div>
		</xsl:if>
	</xsl:template>

  <xsl:template match="ddue:codeExample">
    <xsl:call-template name="section">
      <xsl:with-param name="title">
        <include item="Example" />
      </xsl:with-param>
      <xsl:with-param name="content">
        <xsl:apply-templates />
      </xsl:with-param>
    </xsl:call-template>
  </xsl:template>
	<!-- Footer stuff -->
	
	<xsl:template name="foot">
		<include item="footer" />
	</xsl:template>


</xsl:stylesheet>
