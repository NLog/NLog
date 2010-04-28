<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.1"
				xmlns:MSHelp="http://msdn.microsoft.com/mshelp"
        xmlns:mshelp="http://msdn.microsoft.com/mshelp"
				xmlns:ddue="http://ddue.schemas.microsoft.com/authoring/2003/5"
				xmlns:xlink="http://www.w3.org/1999/xlink"
        xmlns:msxsl="urn:schemas-microsoft-com:xslt"
    >

	<xsl:output method="xml" indent="no" encoding="utf-8" />

  <xsl:include href="htmlBody.xsl" />
	<xsl:include href="utilities_dduexml.xsl" />

  <xsl:variable name="hasSeeAlsoSection" select="boolean(count(/document/topic/*/ddue:relatedTopics/*[local-name()!='sampleRef']) > 0)"/>
  <xsl:variable name="examplesSection" select="boolean(string-length(/document/topic/*/ddue:codeExample[normalize-space(.)]) > 0)"/>
  <xsl:variable name="languageFilterSection" select="normalize-space(/document/topic/*/ddue:codeExample) 
                or normalize-space(/document/topic/*//ddue:snippets/ddue:snippet)
                or /document/topic/ddue:developerSampleDocument/ddue:relatedTopics/ddue:sampleRef[@srcID]" />
  <xsl:variable name="group" select="/document/reference/apidata/@group" />
  <xsl:variable name="subgroup" select="/document/reference/apidata/@subgroup" />
  <xsl:variable name="subsubgroup" select="/document/reference/apidata/@subsubgroup" />
  <xsl:variable name="pseudo" select="boolean(/document/reference/apidata[@pseudo='true'])"/>
  <!-- key parameter is the api identifier string -->
	<xsl:param name="key" />
  <xsl:param name="metadata" value="false" />
  <xsl:param name="languages">false</xsl:param>

	<xsl:template match="/document">
    <html>
      <head>
        <META NAME="save" CONTENT="history"/>
        <title>
          <xsl:call-template name="topicTitlePlain"/>
        </title>
        <xsl:call-template name="insertStylesheets" />
        <xsl:call-template name="insertScripts" />
        <xsl:call-template name="insertMetadata" />
      </head>
      <body>
        <xsl:call-template name="upperBodyStuff"/>
        <xsl:call-template name="main"/>
      </body>
    </html>
  </xsl:template>
	
	<!-- document head -->

  <xsl:template name="insertStylesheets">
    <link rel="stylesheet" type="text/css" href="../styles/presentation.css" />
    <!-- make mshelp links work -->
    <link rel="stylesheet" type="text/css" href="ms-help://Hx/HxRuntime/HxLink.css" />
    <link rel="stylesheet" type="text/css" href="ms-help://Dx/DxRuntime/DxLink.css" />
  </xsl:template>

  <xsl:template name="insertScripts">
    <script type="text/javascript">
      <includeAttribute name="src" item="scriptPath">
        <parameter>EventUtilities.js</parameter>
      </includeAttribute>
      <xsl:text> </xsl:text>
    </script>
    <script type="text/javascript">
      <includeAttribute name="src" item="scriptPath">
        <parameter>SplitScreen.js</parameter>
      </includeAttribute>
      <xsl:text> </xsl:text>
    </script>
    <script type="text/javascript">
      <includeAttribute name="src" item="scriptPath">
        <parameter>Dropdown.js</parameter>
      </includeAttribute>
      <xsl:text> </xsl:text>
    </script>
    <script type="text/javascript">
      <includeAttribute name="src" item="scriptPath">
        <parameter>script_manifold.js</parameter>
      </includeAttribute>
      <xsl:text> </xsl:text>
    </script>
    <script type="text/javascript">
      <includeAttribute name="src" item="scriptPath">
        <parameter>LanguageFilter.js</parameter>
      </includeAttribute>
      <xsl:text> </xsl:text>
    </script>
    <script type="text/javascript">
      <includeAttribute name="src" item="scriptPath">
        <parameter>DataStore.js</parameter>
      </includeAttribute>
      <xsl:text> </xsl:text>
    </script>
    <script type="text/javascript">
      <includeAttribute name="src" item="scriptPath">
        <parameter>CommonUtilities.js</parameter>
      </includeAttribute>
      <xsl:text> </xsl:text>
    </script>
    <script type="text/javascript">
      <includeAttribute name="src" item="scriptPath">
        <parameter>MemberFilter.js</parameter>
      </includeAttribute>
      <xsl:text> </xsl:text>
    </script>
  </xsl:template>

	<xsl:template name="insertMetadata">
    <xsl:if test="$metadata='true'">
		<xml>
		<!-- mshelp metadata -->

      <!-- insert toctitle -->
      <MSHelp:TOCTitle Title="{/document/metadata/tableOfContentsTitle}" />

      <!-- link index -->
  		<MSHelp:Keyword Index="A" Term="{$key}" />

      <!-- authored K -->
      <xsl:variable name="docset" select="translate(/document/metadata/attribute[@name='DocSet'][1]/text(),'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz ')"/>
      <xsl:for-each select="/document/metadata/keyword[@index='K']">
        <xsl:variable name="nestedKeywordText">
          <xsl:call-template name="nestedKeywordText"/>
        </xsl:variable>
        <MSHelp:Keyword Index="K">
          <xsl:choose>
            <xsl:when test="normalize-space($docset)='' or contains(text(),'[')">
              <xsl:attribute name="Term">
                <xsl:value-of select="concat(text(),$nestedKeywordText)"/>
              </xsl:attribute>
            </xsl:when>
            <xsl:otherwise>
              <includeAttribute name="Term" item="kIndexTermWithTechQualifier">
                <parameter><xsl:value-of select="text()"/></parameter>
                <parameter><xsl:value-of select="$docset"/></parameter>
                <parameter><xsl:value-of select="$nestedKeywordText"/></parameter>
              </includeAttribute>
            </xsl:otherwise>
          </xsl:choose>
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
      <xsl:choose>
        <xsl:when test="string-length($abstract) &gt; 254">
          <MSHelp:Attr Name="Abstract" Value="{concat(substring($abstract,1,250), ' ...')}" />
        </xsl:when>
        <xsl:when test="string-length($abstract) &gt; 0">
          <MSHelp:Attr Name="Abstract" Value="{$abstract}" />
        </xsl:when>
      </xsl:choose>

      <!-- authored attributes -->
      <xsl:for-each select="/document/metadata/attribute">
        <MSHelp:Attr Name="{@name}" Value="{text()}" />
      </xsl:for-each>

      <!-- TopicType attribute -->
      <xsl:for-each select="/document/topic/*[1]">
        <MSHelp:Attr Name="TopicType">
          <includeAttribute name="Value" item="TT_{local-name()}"/>
        </MSHelp:Attr>
      </xsl:for-each>

      <!-- Locale attribute -->
      <MSHelp:Attr Name="Locale">
        <includeAttribute name="Value" item="locale"/>
      </MSHelp:Attr>

    </xml>
    </xsl:if>
	</xsl:template>

  <xsl:template name="nestedKeywordText">
    <xsl:for-each select="keyword[@index='K']">
      <xsl:text>, </xsl:text>
      <xsl:value-of select="text()"/>
    </xsl:for-each>
  </xsl:template>
  
	<!-- document body -->

	<!-- Title in topic -->

  <xsl:template name="topicTitleDecorated">
    <xsl:call-template name="topicTitle" />
  </xsl:template>

  <xsl:template name="topicTitlePlain">
    <xsl:call-template name="topicTitle" />
  </xsl:template>

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
    <div id="mainSection">

      <div id="mainBody">
        <div id="allHistory" class="saveHistory" onsave="saveAll()" onload="loadAll()"/>
        <xsl:call-template name="head" />
        <xsl:call-template name="body" />
        <xsl:call-template name="foot" />
      </div>
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
    <xsl:if test="normalize-space(.)">
		<xsl:call-template name="section">
      <xsl:with-param name="toggleSwitch" select="'parameters'"/>
			<xsl:with-param name="title"><include item="parametersTitle" /></xsl:with-param>
			<xsl:with-param name="content">
				<xsl:apply-templates />
			</xsl:with-param>
		</xsl:call-template>
    </xsl:if>
	</xsl:template>

	<xsl:template match="ddue:returnValue">
    <xsl:if test="normalize-space(.)">
		<xsl:call-template name="section">
      <xsl:with-param name="toggleSwitch" select="'returnValue'"/>
			<xsl:with-param name="title"><include item="returnValueTitle" /></xsl:with-param>
			<xsl:with-param name="content">
				<xsl:apply-templates />
			</xsl:with-param>
		</xsl:call-template>
    </xsl:if>
	</xsl:template>

	<xsl:template match="ddue:exceptions">
    <xsl:if test="normalize-space(.)">
		<xsl:call-template name="section">
      <xsl:with-param name="toggleSwitch" select="'ddueExceptions'"/>
			<xsl:with-param name="title"><include item="exceptionsTitle" /></xsl:with-param>
			<xsl:with-param name="content">
				<xsl:apply-templates />
			</xsl:with-param>
		</xsl:call-template>
    </xsl:if>
	</xsl:template>

	<xsl:template match="ddue:relatedSections">
    <xsl:if test="normalize-space(.)">
    <xsl:call-template name="section">
      <xsl:with-param name="toggleSwitch" select="'relatedSections'"/>
      <xsl:with-param name="title"><include item="relatedSectionsTitle" /></xsl:with-param>
      <xsl:with-param name="content">
        <xsl:apply-templates />
      </xsl:with-param>
    </xsl:call-template>
    </xsl:if>
	</xsl:template>

  <xsl:template match="ddue:relatedTopics">
    <xsl:if test="$hasSeeAlsoSection">
      <xsl:call-template name="section">
        <xsl:with-param name="toggleSwitch" select="'seeAlso'"/>
        <xsl:with-param name="title">
          <include item="relatedTopicsTitle" />
        </xsl:with-param>
        <xsl:with-param name="content">

          <!-- Concepts sub-section -->
          <xsl:if test="normalize-space(ddue:link) or normalize-space(ddue:dynamicLink[@type='inline'])">
            <xsl:call-template name="subSection">
              <xsl:with-param name="title">
                <include item="SeeAlsoConcepts"/>
              </xsl:with-param>
              <xsl:with-param name="content">
                <xsl:for-each select="*">
                  <xsl:if test="name() = 'link' or (name() = 'dynamicLink' and @type = 'inline') or (name() = 'legacyLink' and not(starts-with(@xlink:href,'frlrf') 
                    or starts-with(@xlink:href,'N:') or starts-with(@xlink:href,'T:') or starts-with(@xlink:href,'M:') or starts-with(@xlink:href,'P:') 
                    or starts-with(@xlink:href,'F:') or starts-with(@xlink:href,'E:') or starts-with(@xlink:href,'Overload:')))">
                    <div class="seeAlsoStyle">
                      <xsl:apply-templates select="."/>
                    </div>
                  </xsl:if>
                </xsl:for-each>
              </xsl:with-param>
            </xsl:call-template>
          </xsl:if>

          <!-- Reference sub-section -->
          <xsl:if test="normalize-space(ddue:codeEntityReference)">
            <xsl:call-template name="subSection">
              <xsl:with-param name="title">
                <include item="SeeAlsoReference"/>
              </xsl:with-param>
              <xsl:with-param name="content">
                <xsl:for-each select="*">
                  <xsl:if test="name() = 'codeEntityReference' or (name() = 'legacyLink' and (starts-with(@xlink:href,'frlrf') 
                    or starts-with(@xlink:href,'N:') or starts-with(@xlink:href,'T:') or starts-with(@xlink:href,'M:') or starts-with(@xlink:href,'P:') 
                    or starts-with(@xlink:href,'F:') or starts-with(@xlink:href,'E:') or starts-with(@xlink:href,'Overload:')))">
                    <div class="seeAlsoStyle">
                      <xsl:apply-templates select="."/>
                    </div>
                  </xsl:if>
                </xsl:for-each>
              </xsl:with-param>
            </xsl:call-template>
          </xsl:if>

          <!-- Other Resources sub-section -->
          <xsl:if test="ddue:externalLink">
            <xsl:call-template name="subSection">
              <xsl:with-param name="title">
                <include item="SeeAlsoOtherResources"/>
              </xsl:with-param>
              <xsl:with-param name="content">
                <xsl:for-each select="*">
                  <xsl:if test="name() = 'externalLink'">
                    <div class="seeAlsoStyle">
                      <xsl:apply-templates select="."/>
                    </div>
                  </xsl:if>
                </xsl:for-each>
              </xsl:with-param>
            </xsl:call-template>
          </xsl:if>

        </xsl:with-param>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <xsl:template match="ddue:codeExample">
    <!-- create Example section for the first codeExample node -->
    <xsl:if test="not(preceding-sibling::ddue:codeExample) and ../ddue:codeExample[normalize-space(.)!='']">
      <xsl:call-template name="section">
        <xsl:with-param name="toggleSwitch" select="'example'"/>
        <xsl:with-param name="title">
          <include item="Example" />
        </xsl:with-param>
        <xsl:with-param name="content">
          <xsl:apply-templates />
          <!-- if there are additional codeExample nodes, put them inside this section -->
          <xsl:for-each select="following-sibling::ddue:codeExample">
            <xsl:apply-templates />
          </xsl:for-each>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <xsl:template match="ddue:codeReference">
    <xsl:apply-templates/>
  </xsl:template>

  <xsl:template name="runningHeader">
    <xsl:variable name="runningHeaderText">
      <xsl:value-of select="/document/metadata/runningHeaderText/@uscid"/>
    </xsl:variable>
    <include item="{$runningHeaderText}" />
  </xsl:template>

	<!-- Footer stuff -->

  <xsl:template name="foot">
    <div id="footer">
      <div class="footerLine">
        <img width="100%" height="3px">
          <includeAttribute name="src" item="iconPath">
            <parameter>footer.gif</parameter>
          </includeAttribute>
          <includeAttribute name="title" item="footerImage" />
        </img>
      </div>

      <include item="footer">
        <parameter>
          <xsl:value-of select="$key"/>
        </parameter>
        <parameter>
          <xsl:call-template name="topicTitlePlain"/>
        </parameter>
      </include>
    </div>
  </xsl:template>


</xsl:stylesheet>
