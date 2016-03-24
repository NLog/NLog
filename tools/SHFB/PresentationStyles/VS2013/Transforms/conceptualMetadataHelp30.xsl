<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="2.0"
								xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
								xmlns:ddue="http://ddue.schemas.microsoft.com/authoring/2003/5"
								xmlns:msxsl="urn:schemas-microsoft-com:xslt"
								exclude-result-prefixes="msxsl"
	>
	<!-- ======================================================================================== -->

	<xsl:import href="globalTemplates.xsl"/>
	<xsl:import href="conceptualTopicTypes.xsl"/>

	<!-- ======================================================================================== -->

	<xsl:template name="t_insertMetadataHelp30">

		<!-- System.Language -->
		<meta name="Language">
			<includeAttribute name="content" item="locale" />
		</meta>

		<!-- System.Title -->
		<!-- <title> is set elsewhere -->

		<!-- System.Keywords -->
		<xsl:call-template name="t_insertKeywordsF1Metadata" />

		<!-- Microsoft.Help.Id -->
		<meta name="Microsoft.Help.Id" content="{/document/topic/@id}" />

		<!-- Microsoft.Help.Description -->
		<xsl:variable name="v_abstract" select="normalize-space(string(/document/topic//ddue:para[1]))" />
		<xsl:variable name="v_description">
			<xsl:call-template name="t_getTrimmedAtPeriod">
				<xsl:with-param name="p_string" select="$v_abstract" />
			</xsl:call-template>
		</xsl:variable>
		<xsl:if test="normalize-space($v_description)">
			<meta name="Description">
				<xsl:attribute name="content">
					<xsl:value-of select="normalize-space($v_description)"/>
				</xsl:attribute>
			</meta>
		</xsl:if>

		<!-- Microsoft.Help.TocParent -->
		<xsl:if test="/document/metadata/attribute[@name='TOCParent']">
			<meta name="Microsoft.Help.TocParent" content="{/document/metadata/attribute[@name='TOCParent']}" />
		</xsl:if>
		<xsl:if test="/document/metadata/attribute[@name='TOCOrder']">
			<meta name="Microsoft.Help.TocOrder" content="{/document/metadata/attribute[@name='TOCOrder']}" />
		</xsl:if>

		<!-- Microsoft.Help.Category -->
		<xsl:for-each select="/document/metadata/attribute[@name='Category']">
			<meta name="Microsoft.Help.Category" content="{.}" />
		</xsl:for-each>

		<!-- Microsoft.Help.ContentFilter -->
		<xsl:for-each select="/document/metadata/attribute[@name='ContentFilter']">
			<meta name="Microsoft.Help.ContentFilter" content="{.}" />
		</xsl:for-each>

		<!-- Microsoft.Help.ContentType -->
		<xsl:variable name="v_contentTypeDocStudio">
			<xsl:variable name="v_lookupValue">
				<xsl:value-of select="local-name(/document/topic/*[1])"/>
			</xsl:variable>
			<xsl:value-of select="msxsl:node-set($g_topicTypes)/topic[@name = $v_lookupValue]/@contentType"/>
		</xsl:variable>

		<xsl:variable name="v_contentTypeTopicType">
			<xsl:variable name="v_lookupValue">
				<xsl:value-of select="translate(/document/metadata/topicType/@id, $g_allLowerCaseLetters, $g_allUpperCaseLetters)"/>
			</xsl:variable>
			<xsl:value-of select="msxsl:node-set($g_topicTypes)/topic[@guid = $v_lookupValue]/@contentType"/>
		</xsl:variable>

		<xsl:choose>
			<xsl:when test="$v_contentTypeDocStudio">
				<meta name="Microsoft.Help.ContentType" content="{$v_contentTypeDocStudio}" />
			</xsl:when>
			<xsl:when test="$v_contentTypeTopicType">
				<meta name="Microsoft.Help.ContentType" content="{$v_contentTypeTopicType}" />
			</xsl:when>
		</xsl:choose>

		<!-- Microsoft.Package.Book -->
		<xsl:variable name="Book" select="/document/metadata/attribute[@name='Book']/text()" />
		<xsl:if test="$Book">
			<meta name="Microsoft.Package.Book" content="{$Book}" />
		</xsl:if>

		<!-- Source -->
		<xsl:for-each select="/document/metadata/attribute[@name='Source']">
			<meta name="Source" content="{.}" />
		</xsl:for-each>

		<!-- Branding aware.  This prevents the MSHC Component from changing a couple of CSS style names. -->
		<meta name="BrandingAware" content="true"/>
	</xsl:template>

	<!-- ======================================================================================== -->

	<xsl:template name="t_insertKeywordsF1Metadata">

		<!-- authored K -->
		<xsl:variable name="v_docset"
									select="translate(/document/metadata/attribute[@name='DocSet'][1]/text(),$g_allUpperCaseLetters,'abcdefghijklmnopqrstuvwxyz ')"/>
		<xsl:for-each select="/document/metadata/keyword[@index='K']">
			<xsl:variable name="v_nestedKeywordText">
				<xsl:call-template name="t_nestedKeywordText"/>
			</xsl:variable>
			<xsl:choose>
				<xsl:when test="not(contains(text(),'[')) and ($v_docset='avalon' or $v_docset='wpf' or $v_docset='wcf' or $v_docset='windowsforms')">
					<meta name="System.Keywords">
						<includeAttribute name="content"
															item="meta_kIndexTermWithTechQualifier">
							<parameter>
								<xsl:value-of select="text()"/>
							</parameter>
							<parameter>
								<xsl:value-of select="$v_docset"/>
							</parameter>
							<parameter>
								<xsl:value-of select="$v_nestedKeywordText"/>
							</parameter>
						</includeAttribute>
					</meta>
				</xsl:when>
				<xsl:otherwise>
					<meta name="System.Keywords"
								content="{concat(text(),$v_nestedKeywordText)}" />
				</xsl:otherwise>
			</xsl:choose>
		</xsl:for-each>

		<!-- authored F -->
		<xsl:for-each select="/document/metadata/keyword[@index='F']">
			<meta name="Microsoft.Help.F1">
				<xsl:attribute name="content">
					<xsl:value-of select="text()" />
					<xsl:for-each select="keyword[@index='F']">
						<xsl:text>, </xsl:text>
						<xsl:value-of select="text()"/>
					</xsl:for-each>
				</xsl:attribute>
			</meta>
		</xsl:for-each>

		<!-- authored B -->
		<xsl:for-each select="/document/metadata/keyword[@index='B']">
			<meta name="Microsoft.Help.F1">
				<xsl:attribute name="content">
					<xsl:value-of select="text()" />
					<xsl:for-each select="keyword[@index='B']">
						<xsl:text>, </xsl:text>
						<xsl:value-of select="text()"/>
					</xsl:for-each>
				</xsl:attribute>
			</meta>
		</xsl:for-each>

	</xsl:template>

</xsl:stylesheet>
