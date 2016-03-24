<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
		xmlns:msxsl="urn:schemas-microsoft-com:xslt" >

	<xsl:template name="typeNameWithTicks">
		<xsl:for-each select="type|(containers/type)">
			<xsl:call-template name="typeNameWithTicks" />
			<xsl:text>.</xsl:text>
		</xsl:for-each>
		<xsl:value-of select="apidata/@name" />
		<xsl:if test="boolean(templates/template)">
			<xsl:text>`</xsl:text>
			<xsl:value-of select="count(templates/template)"/>
		</xsl:if>
	</xsl:template>

	<xsl:template name="qualifiedTextNames">
		<xsl:choose>
			<!-- explicit interface implementations -->
			<xsl:when test="memberdata[@visibility='private'] and proceduredata[@virtual = 'true']">
				<xsl:variable name="left">
					<xsl:for-each select="containers/type">
						<xsl:call-template name="textNames"/>
					</xsl:for-each>
				</xsl:variable>
				<xsl:variable name="right">
					<xsl:for-each select="implements/member">
						<xsl:call-template name="textNames" />
					</xsl:for-each>
				</xsl:variable>
				<xsl:call-template name="combineTextNames">
					<xsl:with-param name="left" select="msxsl:node-set($left)" />
					<xsl:with-param name="right" select="msxsl:node-set($right)" />
				</xsl:call-template>
			</xsl:when>
			<!-- members get qualified by type name -->
			<xsl:when test="apidata/@group='member' and containers/type">
				<xsl:variable name="left">
					<xsl:for-each select="containers/type">
						<xsl:call-template name="textNames"/>
					</xsl:for-each>
				</xsl:variable>
				<xsl:variable name="right">
					<xsl:call-template name="simpleTextNames" />
				</xsl:variable>
				<xsl:call-template name="combineTextNames">
					<xsl:with-param name="left" select="msxsl:node-set($left)" />
					<xsl:with-param name="right" select="msxsl:node-set($right)" />
				</xsl:call-template>
			</xsl:when>
			<!-- types get qualified by namespace name -->
			<xsl:when test="typedata and containers/namespace/apidata/@name">
				<xsl:variable name="left">
					<xsl:for-each select="containers/namespace">
						<xsl:call-template name="simpleTextNames"/>
					</xsl:for-each>
				</xsl:variable>
				<xsl:variable name="right">
					<xsl:call-template name="textNames" />
				</xsl:variable>
				<xsl:call-template name="combineTextNames">
					<xsl:with-param name="left" select="msxsl:node-set($left)" />
					<xsl:with-param name="right" select="msxsl:node-set($right)" />
				</xsl:call-template>
			</xsl:when>
		</xsl:choose>
	</xsl:template>

	<!-- given two XML lists of API names (produced by textNames template below), produces an XML list
  that dot-concatenates them, respecting the @language attributes -->
	<xsl:template name="combineTextNames">
		<xsl:param name="left" />
		<xsl:param name="right" />
		<xsl:param name="concatenateOperator" select="'.'" />

		<xsl:choose>
			<xsl:when test="count($left/name) &gt; 1">
				<xsl:choose>
					<xsl:when test="count($right/name) &gt; 1">
						<!-- both left and right are multi-language -->
						<xsl:for-each select="$left/name">
							<xsl:variable name="language" select="@language" />
							<name language="{$language}">
								<xsl:apply-templates select="." />
								<xsl:copy-of select="$concatenateOperator" />
								<xsl:apply-templates select="$right/name[@language=$language]" />
							</name>
						</xsl:for-each>
					</xsl:when>
					<xsl:otherwise>
						<!-- left is multi-language, right is not -->
						<xsl:for-each select="$left/name">
							<xsl:variable name="language" select="@language" />
							<name language="{$language}">
								<xsl:apply-templates select="." />
								<xsl:if test="$right/name">
									<xsl:copy-of select="$concatenateOperator"/>
								</xsl:if>
								<xsl:value-of select="$right/name"/>
							</name>
						</xsl:for-each>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:when>
			<xsl:otherwise>
				<xsl:choose>
					<xsl:when test="count($right/name) &gt; 1">
						<!-- right is multi-language, left is not -->
						<xsl:for-each select="$right/name">
							<xsl:variable name="language" select="@language" />
							<name language="{.}">
								<xsl:value-of select="$left/name"/>
								<xsl:if test="$left/name">
									<xsl:copy-of select="$concatenateOperator"/>
								</xsl:if>
								<xsl:apply-templates select="." />
							</name>
						</xsl:for-each>
					</xsl:when>
					<xsl:otherwise>
						<!-- neiter is multi-language -->
						<name>
							<xsl:value-of select="$left/name"/>
							<xsl:if test="$left/name and $right/name">
								<xsl:copy-of select="$concatenateOperator"/>
							</xsl:if>
							<xsl:value-of select="$right/name"/>
						</name>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<!-- produces an XML list of API names; context is parent of apidata element -->
	<!-- if there are no templates: <name>Blah</name> -->
	<!-- if there are templates: <name langauge="c">Blah<T></name><name language="v">Blah(Of T)</name> -->
	<xsl:template name="simpleTextNames">
		<xsl:choose>
			<xsl:when test="specialization">
				<xsl:apply-templates select="specialization" mode="index">
					<xsl:with-param name="name" select="apidata/@name" />
				</xsl:apply-templates>
			</xsl:when>
			<xsl:when test="templates">
				<xsl:apply-templates select="templates" mode="index">
					<xsl:with-param name="name" select="apidata/@name" />
				</xsl:apply-templates>
			</xsl:when>
			<xsl:otherwise>
				<name>
					<xsl:choose>
						<xsl:when test="apidata/@subgroup = 'constructor'">
							<xsl:value-of select="containers/type/apidata/@name"/>
						</xsl:when>
						<xsl:otherwise>
							<xsl:value-of select="apidata/@name"/>
						</xsl:otherwise>
					</xsl:choose>
				</name>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="textNames">
		<xsl:choose>
			<xsl:when test="typedata and (containers/type | type) and not($g_topicGroup='list')">
				<xsl:variable name="left">
					<xsl:apply-templates select="type | (containers/type)" mode="index" />
				</xsl:variable>
				<xsl:variable name="right">
					<xsl:call-template name="simpleTextNames" />
				</xsl:variable>
				<xsl:call-template name="combineTextNames">
					<xsl:with-param name="left" select="msxsl:node-set($left)" />
					<xsl:with-param name="right" select="msxsl:node-set($right)" />
				</xsl:call-template>
			</xsl:when>
			<xsl:when test="type">
				<xsl:variable name="left">
					<xsl:apply-templates select="type" mode="index" />
				</xsl:variable>
				<xsl:variable name="right">
					<xsl:call-template name="simpleTextNames" />
				</xsl:variable>
				<xsl:call-template name="combineTextNames">
					<xsl:with-param name="left" select="msxsl:node-set($left)" />
					<xsl:with-param name="right" select="msxsl:node-set($right)" />
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<xsl:call-template name="simpleTextNames" />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<!-- produces a C#/C++ style generic template parameter list for inclusion in the index -->
	<xsl:template name="csTemplateText">
		<xsl:text>%3C</xsl:text>
		<xsl:call-template name="templateText" />
		<xsl:text>%3E</xsl:text>
	</xsl:template>

	<!-- produces a VB-style generic template parameter list for inclusion in the index -->
	<xsl:template name="vbTemplateText">
		<xsl:text>(Of </xsl:text>
		<xsl:call-template name="templateText" />
		<xsl:text>)</xsl:text>
	</xsl:template>

	<!-- produces a comma-separated list of generic template parameter names -->
	<!-- comma character is URL-encoded so as not to create sub-index entries -->
	<xsl:template name="templateText">
		<xsl:for-each select="*">
			<xsl:apply-templates select="." mode="index" />
			<xsl:if test="not(position()=last())">
				<xsl:text>%2C </xsl:text>
			</xsl:if>
		</xsl:for-each>
	</xsl:template>


	<xsl:template match="specialization | templates" mode="index" >
		<xsl:param name="name" />
		<name language="c">
			<xsl:value-of select="$name" />
			<xsl:call-template name="csTemplateText" />
		</name>
		<name language="v">
			<xsl:value-of select="$name" />
			<xsl:call-template name="vbTemplateText" />
		</name>
	</xsl:template>

	<xsl:template match="template" mode="index">
		<xsl:value-of select="@name" />
	</xsl:template>

	<xsl:template match="arrayOf" mode="index">
		<name language="c">
			<xsl:apply-templates select="type|arrayOf|pointerTo|referenceTo|template|specialization|templates" mode="index"/>
			<xsl:text>[</xsl:text>
			<xsl:if test="number(@rank) &gt; 1">,</xsl:if>
			<xsl:text>]</xsl:text>
		</name>
		<name language="v">
			<xsl:apply-templates select="type|arrayOf|pointerTo|referenceTo|template|specialization|templates" mode="index"/>
			<xsl:text>(</xsl:text>
			<xsl:if test="number(@rank) &gt; 1">,</xsl:if>
			<xsl:text>)</xsl:text>
		</name>
	</xsl:template>

	<xsl:template match="pointerTo" mode="index">
		<xsl:apply-templates select="type|arrayOf|pointerTo|referenceTo|template|specialization|templates" mode="index"/>
		<xsl:text>*</xsl:text>
	</xsl:template>

	<xsl:template match="referenceTo" mode="index">
		<xsl:apply-templates select="type|arrayOf|pointerTo|referenceTo|template|specialization|templates" mode="index"/>
	</xsl:template>

	<xsl:template match="type" mode="index">
		<xsl:call-template name="textNames" />
	</xsl:template>

	<xsl:template match="name/name">
		<xsl:variable name="lang" select="ancestor::*/@language"/>

		<xsl:if test="not(@language) or @language = $lang">
			<xsl:value-of select="."/>
		</xsl:if>
	</xsl:template>

	<xsl:template match="name/text()">
		<xsl:value-of select="."/>
	</xsl:template>

	<xsl:template name="operatorTextNames">
		<xsl:variable name="left">
			<xsl:if test="parameters/parameter[1]">
				<xsl:choose>
					<xsl:when test="parameters/parameter[1]//specialization | parameters/parameter[1]//templates | parameters/parameter[1]//arrayOf">
						<xsl:apply-templates select="parameters/parameter[1]" mode="index" />
					</xsl:when>
					<xsl:otherwise>
						<name>
							<xsl:apply-templates select="parameters/parameter[1]" mode="index" />
						</name>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:if>
		</xsl:variable>

		<xsl:variable name="right">
			<xsl:if test="returns[1]">
				<xsl:choose>
					<xsl:when test="returns[1]//specialization | returns[1]//templates | returns[1]//arrayOf">
						<xsl:apply-templates select="returns[1]" mode="index" />
					</xsl:when>
					<xsl:otherwise>
						<name>
							<xsl:apply-templates select="returns[1]" mode="index" />
						</name>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:if>
		</xsl:variable>

		<xsl:call-template name="combineTextNames">
			<xsl:with-param name="left" select="msxsl:node-set($left)" />
			<xsl:with-param name="right" select="msxsl:node-set($right)" />
			<xsl:with-param name="concatenateOperator">
				<xsl:text> to </xsl:text>
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

</xsl:stylesheet>
