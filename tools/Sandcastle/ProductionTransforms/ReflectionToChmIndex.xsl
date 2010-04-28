<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.1">

	<xsl:output method="text" encoding="iso-8859-1" />

	<xsl:key name="index" match="/reflection/apis/api" use="@id" />

	<xsl:template match="/">
		<xsl:text>&lt;!DOCTYPE HTML PUBLIC "-//IETF//DTD HTML/EN"&gt;&#x0a;</xsl:text>
		<xsl:text>&lt;HTML&gt;&#x0a;</xsl:text>
		<xsl:text>  &lt;BODY&gt;&#x0a;</xsl:text>
		<xsl:text>    &lt;UL&gt;&#x0a;</xsl:text>

		<xsl:apply-templates select="/reflection/apis/api"/>

		<xsl:text>    &lt;/UL&gt;&#x0a;</xsl:text>
		<xsl:text>  &lt;/BODY&gt;&#x0a;</xsl:text>
		<xsl:text>&lt;/HTML&gt;&#x0a;</xsl:text>
	</xsl:template>

	<xsl:template match="api[apidata/@group='namespace']">
		<xsl:call-template name="createIndexEntry">
			<xsl:with-param name="text" select="concat(apidata/@name,' namespace')" />
			<xsl:with-param name="file" select="file/@name" />
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="api[apidata/@group='type']">
		<xsl:variable name="namespace" select="key('index',containers/namespace/@api)/apidata/@name" />
		<xsl:call-template name="createIndexEntry">
			<xsl:with-param name="text" select="concat(apidata/@name,' ',apidata/@subgroup)" />
			<xsl:with-param name="file" select="file/@name" />
		</xsl:call-template>
		<xsl:call-template name="createIndexEntry">
			<xsl:with-param name="text" select="concat($namespace,'.',apidata/@name,' ',apidata/@subgroup)" />
			<xsl:with-param name="file" select="file/@name" />
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="api[apidata/@group='member' and topicdata/@notopic !='']" >
		<xsl:variable name="type" select="key('index',containers/type/@api)" />
		<xsl:if test="not(apidata/@subgroup='constructor')">
			<xsl:call-template name="createIndexEntry">
				<xsl:with-param name="text">
					<xsl:value-of select="concat(apidata/@name,' ',apidata/@subgroup)" />
				</xsl:with-param>
				<xsl:with-param name="file" select="file/@name" />
			</xsl:call-template>
		</xsl:if>
		<xsl:call-template name="createIndexEntry">
			<xsl:with-param name="text">
				<xsl:choose>
					<xsl:when test="apidata/@subgroup='constructor'">
						<xsl:value-of select="concat($type/apidata/@name,' ',$type/apidata/@subgroup,', constructor')" />
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="concat($type/apidata/@name,'.',apidata/@name,' ',apidata/@subgroup)" />
					</xsl:otherwise>
				</xsl:choose>
			</xsl:with-param>
			<xsl:with-param name="file" select="file/@name" />
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="api" />

	<xsl:template name="createIndexEntry">
		<xsl:param name="text" />
		<xsl:param name="file" />
		<xsl:text>      &lt;LI&gt;&lt;OBJECT type="text/sitemap"&gt;&#x0a;</xsl:text>
		<xsl:text>        &lt;param name="Name" value="</xsl:text><xsl:value-of select="$text" /><xsl:text>"&gt;&#x0a;</xsl:text>
		<xsl:text>        &lt;param name="Local" value="html/</xsl:text><xsl:value-of select="$file" /><xsl:text>.htm"&gt;&#x0a;</xsl:text>
		<xsl:text>      &lt;/OBJECT&gt;&lt;LI&gt;&#x0a;</xsl:text>
		
	</xsl:template>

</xsl:stylesheet>
