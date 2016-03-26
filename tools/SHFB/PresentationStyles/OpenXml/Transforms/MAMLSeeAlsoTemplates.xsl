<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="2.0"
								xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
								xmlns:msxsl="urn:schemas-microsoft-com:xslt"
								xmlns:ddue="http://ddue.schemas.microsoft.com/authoring/2003/5"
								xmlns:xlink="http://www.w3.org/1999/xlink"
								xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main"
>
	<!-- ======================================================================================== -->

	<xsl:import href="ConceptualTopicTypes.xsl"/>

	<!-- ============================================================================================
	Process relatedTopics
	============================================================================================= -->

	<xsl:template match="ddue:relatedTopics" mode="seeAlso">
		<xsl:param name="p_autoGenerateLinks" select="false()"/>

		<xsl:variable name="v_taskLinks">
			<xsl:for-each select="(ddue:link | ddue:legacyLink)[@topicType_id]">
				<xsl:variable name="v_topicTypeId">
					<xsl:value-of select="translate(@topicType_id, $g_allLowerCaseLetters, $g_allUpperCaseLetters)"/>
				</xsl:variable>
				<xsl:variable name="v_seeAlsoGroup">
					<xsl:value-of select="translate(msxsl:node-set($g_topicTypes)/topic[@guid = $v_topicTypeId]/@seeAlsoGroup, $g_allUpperCaseLetters, $g_allLowerCaseLetters)"/>
				</xsl:variable>
				<xsl:if test="$v_seeAlsoGroup='tasks'">
					<xsl:copy-of select="."/>
				</xsl:if>
			</xsl:for-each>
		</xsl:variable>

		<xsl:variable name="v_conceptLinks">
			<xsl:for-each select="(ddue:link | ddue:legacyLink)[@topicType_id]">
				<xsl:variable name="v_topicTypeId">
					<xsl:value-of select="translate(@topicType_id, $g_allLowerCaseLetters, $g_allUpperCaseLetters)"/>
				</xsl:variable>
				<xsl:variable name="v_seeAlsoGroup">
					<xsl:value-of select="translate(msxsl:node-set($g_topicTypes)/topic[@guid = $v_topicTypeId]/@seeAlsoGroup, $g_allUpperCaseLetters, $g_allLowerCaseLetters)"/>
				</xsl:variable>
				<xsl:if test="$v_seeAlsoGroup='concepts'">
					<xsl:copy-of select="."/>
				</xsl:if>
			</xsl:for-each>
		</xsl:variable>

		<xsl:variable name="v_referenceLinks">
			<xsl:for-each select="(ddue:link | ddue:legacyLink)[@topicType_id] | ddue:codeEntityReference">
				<xsl:choose>
					<xsl:when test="self::ddue:codeEntityReference">
						<xsl:copy-of select="."/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:variable name="v_topicTypeId">
							<xsl:value-of select="translate(@topicType_id, $g_allLowerCaseLetters, $g_allUpperCaseLetters)"/>
						</xsl:variable>
						<xsl:variable name="v_seeAlsoGroup">
							<xsl:value-of select="translate(msxsl:node-set($g_topicTypes)/topic[@guid = $v_topicTypeId]/@seeAlsoGroup, $g_allUpperCaseLetters, $g_allLowerCaseLetters)"/>
						</xsl:variable>
						<xsl:if test="$v_seeAlsoGroup='reference'">
							<xsl:copy-of select="."/>
						</xsl:if>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:for-each>
		</xsl:variable>

		<xsl:variable name="v_otherLinks">
			<xsl:for-each select="ddue:link | ddue:legacyLink | ddue:dynamicLink[@type='inline'] | ddue:externalLink">
				<xsl:choose>
					<xsl:when test="self::ddue:dynamicLink">
						<xsl:copy-of select="."/>
					</xsl:when>
					<xsl:when test="self::ddue:externalLink">
						<xsl:copy-of select="."/>
					</xsl:when>
					<xsl:when test="@topicType_id">
						<xsl:variable name="v_topicTypeId">
							<xsl:value-of select="translate(@topicType_id, $g_allLowerCaseLetters, $g_allUpperCaseLetters)"/>
						</xsl:variable>
						<xsl:variable name="v_seeAlsoGroup">
							<xsl:value-of select="translate(msxsl:node-set($g_topicTypes)/topic[@guid = $v_topicTypeId]/@seeAlsoGroup, $g_allUpperCaseLetters, $g_allLowerCaseLetters)"/>
						</xsl:variable>
						<xsl:if test="($v_seeAlsoGroup!='tasks') and ($v_seeAlsoGroup!='concepts') and ($v_seeAlsoGroup!='reference')">
							<xsl:copy-of select="."/>
						</xsl:if>
					</xsl:when>
					<xsl:otherwise>
						<xsl:copy-of select="."/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:for-each>
		</xsl:variable>

		<xsl:if test="msxsl:node-set($v_taskLinks)/*">
			<xsl:call-template name="t_putSeeAlsoSubSection">
				<xsl:with-param name="p_headerGroup" select="'title_seeAlso_tasks'"/>
				<xsl:with-param name="p_members" select="$v_taskLinks"/>
				<xsl:with-param name="p_autoGenerateLinks" select="false()"/>
			</xsl:call-template>
		</xsl:if>

		<xsl:if test="msxsl:node-set($v_referenceLinks)/* or boolean($p_autoGenerateLinks)">
			<xsl:call-template name="t_putSeeAlsoSubSection">
				<xsl:with-param name="p_headerGroup" select="'title_seeAlso_reference'"/>
				<xsl:with-param name="p_members" select="$v_referenceLinks"/>
				<xsl:with-param name="p_autoGenerateLinks" select="$p_autoGenerateLinks"/>
			</xsl:call-template>
		</xsl:if>

		<xsl:if test="msxsl:node-set($v_conceptLinks)/*">
			<xsl:call-template name="t_putSeeAlsoSubSection">
				<xsl:with-param name="p_headerGroup" select="'title_seeAlso_concepts'"/>
				<xsl:with-param name="p_members" select="$v_conceptLinks"/>
				<xsl:with-param name="p_autoGenerateLinks" select="false()"/>
			</xsl:call-template>
		</xsl:if>

		<xsl:if test="msxsl:node-set($v_otherLinks)/*">
			<xsl:call-template name="t_putSeeAlsoSubSection">
				<xsl:with-param name="p_headerGroup" select="'title_seeAlso_otherResources'"/>
				<xsl:with-param name="p_members" select="$v_otherLinks"/>
				<xsl:with-param name="p_autoGenerateLinks" select="false()"/>
			</xsl:call-template>
		</xsl:if>

	</xsl:template>

	<xsl:template name="t_putSeeAlsoSubSection">
		<xsl:param name="p_headerGroup"/>
		<xsl:param name="p_members"/>
		<xsl:param name="p_autoGenerateLinks" select="false()"/>
		<xsl:call-template name="t_putSubSection">
			<xsl:with-param name="p_title">
				<include item="{$p_headerGroup}"/>
			</xsl:with-param>
			<xsl:with-param name="p_content">
				<xsl:if test="boolean($p_autoGenerateLinks)">
					<xsl:call-template name="t_autogenSeeAlsoLinks"/>
				</xsl:if>
				<xsl:for-each select="msxsl:node-set($p_members)/*">
					<w:p>
						<w:pPr>
							<w:spacing w:after="0" />
						</w:pPr>
						<xsl:apply-templates select="."/>
					</w:p>
				</xsl:for-each>
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>
</xsl:stylesheet>
