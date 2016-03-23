<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="2.0"
								xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
								xmlns:msxsl="urn:schemas-microsoft-com:xslt"
								xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main"
>
	
	<!-- ============================================================================================
	Globals
	============================================================================================= -->

	<xsl:variable name="g_allUpperCaseLetters">ABCDEFGHIJKLMNOPQRSTUVWXYZ</xsl:variable>
	<xsl:variable name="g_allLowerCaseLetters">abcdefghijklmnopqrstuvwxyz</xsl:variable>

	<!-- ============================================================================================
	String formatting
	============================================================================================= -->

	<!-- indent by 2*n spaces -->
	<xsl:template name="t_putIndent">
		<xsl:param name="p_count" />
		<xsl:if test="$p_count &gt; 1">
			<xsl:text>&#160;&#160;</xsl:text>
			<xsl:call-template name="t_putIndent">
				<xsl:with-param name="p_count" select="$p_count - 1" />
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<!-- Gets the substring after the last occurrence of a period in a given string -->
	<xsl:template name="t_getTrimmedLastPeriod">
		<xsl:param name="p_string" />

		<xsl:choose>
			<xsl:when test="contains($p_string, '.')">
				<xsl:call-template name="t_getTrimmedLastPeriod">
					<xsl:with-param name="p_string"
													select="substring-after($p_string, '.')" />
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$p_string" />
			</xsl:otherwise>
		</xsl:choose>

	</xsl:template>

	<!-- ============================================================================================
	Text handling
	============================================================================================= -->

	<!-- This is used for most text which needs normalizing to remove extra whitespace -->
	<xsl:template match="text()">
		<w:r>
			<!-- Keep this on the same line to prevent extra space from getting included -->
			<w:t xml:space="preserve"><xsl:call-template name="t_normalize"><xsl:with-param name="p_text" select="."/></xsl:call-template></w:t>
		</w:r>
	</xsl:template>

	<!-- This is used to keep extra whitespace and line breaks intact for things like code blocks -->
	<xsl:template match="text()" mode="preserveFormatting">
		<w:r>
			<!-- Keep this on the same line to prevent extra space from getting included -->
			<w:t xml:space="preserve"><xsl:value-of select="." /></w:t>
		</w:r>
	</xsl:template>

	<!-- Space normalization with handling for inserting a space before and/or after if there are preceding and/or
			 following elements. -->
	<xsl:template name="t_normalize">
		<xsl:param name="p_text" />

		<!-- If there is a preceding sibling and the text started with whitespace, add a leading space -->
		<xsl:if test="preceding-sibling::* and starts-with(translate($p_text, '&#x20;&#x9;&#xD;&#xA;', '&#xFF;&#xFF;&#xFF;&#xFF;'), '&#xFF;')">
			<xsl:text> </xsl:text>
		</xsl:if>

		<xsl:value-of select="normalize-space($p_text)"/>

		<!-- If there is a following sibling and the text ended with whitespace, add a trailing space -->
		<xsl:if test="following-sibling::* and substring(translate($p_text, '&#x20;&#x9;&#xD;&#xA;', '&#xFF;&#xFF;&#xFF;&#xFF;'), string-length($p_text)) = '&#xFF;'">
			<xsl:text> </xsl:text>
		</xsl:if>
	</xsl:template>

	<!-- ============================================================================================
	SeeAlso links
	============================================================================================= -->

	<xsl:template match="referenceLink">
		<xsl:copy-of select="."/>
	</xsl:template>

	<xsl:template match="referenceLink" mode="preserveFormatting">
		<xsl:copy-of select="."/>
	</xsl:template>

	<xsl:template name="t_autogenSeeAlsoLinks">

		<!-- a link to the containing type on all list and member topics -->
		<xsl:if test="($g_apiTopicGroup='member' or $g_apiTopicGroup='list')">
			<xsl:variable name="v_typeTopicId">
				<xsl:choose>
					<xsl:when test="/document/reference/topicdata/@typeTopicId">
						<xsl:value-of select="/document/reference/topicdata/@typeTopicId"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="/document/reference/containers/type/@api"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:variable>
			<w:p>
				<w:pPr>
					<w:spacing w:after="0" />
				</w:pPr>
				<referenceLink target="{$v_typeTopicId}" display-target="format">
					<include item="boilerplate_seeAlsoTypeLink">
						<parameter>{0}</parameter>
						<parameter>
							<xsl:choose>
								<xsl:when test="/document/reference/topicdata/@typeTopicId">
									<xsl:value-of select="/document/reference/apidata/@subgroup"/>
								</xsl:when>
								<xsl:otherwise>
									<xsl:value-of select="/document/reference/containers/type/apidata/@subgroup"/>
								</xsl:otherwise>
							</xsl:choose>
						</parameter>
					</include>
				</referenceLink>
			</w:p>
		</xsl:if>

		<!-- a link to the type's All Members list -->
		<xsl:variable name="v_allMembersId">
			<xsl:choose>
				<xsl:when test="/document/reference/topicdata/@allMembersTopicId">
					<xsl:value-of select="/document/reference/topicdata/@allMembersTopicId"/>
				</xsl:when>
				<xsl:when test="$g_apiTopicGroup='member' or ($g_apiTopicGroup='list' and $g_apiTopicSubGroup='overload')">
					<xsl:value-of select="/document/reference/containers/type/topicdata/@allMembersTopicId"/>
				</xsl:when>
			</xsl:choose>
		</xsl:variable>
		<xsl:if test="normalize-space($v_allMembersId) and not($v_allMembersId=$key)">
			<w:p>
				<w:pPr>
					<w:spacing w:after="0" />
				</w:pPr>
				<referenceLink target="{$v_allMembersId}" display-target="format">
					<include item="boilerplate_seeAlsoMembersLink">
						<parameter>{0}</parameter>
					</include>
				</referenceLink>
			</w:p>
		</xsl:if>

		<!-- a link to the overload topic -->
		<xsl:variable name="v_overloadId">
			<xsl:value-of select="/document/reference/memberdata/@overload"/>
		</xsl:variable>
		<xsl:if test="normalize-space($v_overloadId)">
			<w:p>
				<w:pPr>
					<w:spacing w:after="0" />
				</w:pPr>
				<referenceLink target="{$v_overloadId}" display-target="format" show-parameters="false">
					<include item="boilerplate_seeAlsoOverloadLink">
						<parameter>{0}</parameter>
					</include>
				</referenceLink>
			</w:p>
		</xsl:if>

		<!-- a link to the namespace topic -->
		<xsl:variable name="v_namespaceId">
			<xsl:value-of select="/document/reference/containers/namespace/@api"/>
		</xsl:variable>
		<xsl:if test="normalize-space($v_namespaceId)">
			<w:p>
				<w:pPr>
					<w:spacing w:after="0" />
				</w:pPr>
				<referenceLink target="{$v_namespaceId}" display-target="format">
					<include item="boilerplate_seeAlsoNamespaceLink">
						<parameter>{0}</parameter>
					</include>
				</referenceLink>
			</w:p>
		</xsl:if>

	</xsl:template>

	<!-- ============================================================================================
	Section headers
	============================================================================================= -->

	<xsl:template name="t_putSection">
		<xsl:param name="p_title" />
		<xsl:param name="p_content" />

		<xsl:if test="normalize-space($p_title)">
			<w:p>
				<w:pPr>
					<w:pStyle w:val="Heading2" />
				</w:pPr>
				<xsl:copy-of select="$p_title" />
			</w:p>
		</xsl:if>

		<xsl:copy-of select="$p_content" />
	</xsl:template>

	<xsl:template name="t_putSectionInclude">
		<xsl:param name="p_titleInclude" />
		<xsl:param name="p_content" />
		<w:p>
			<w:pPr>
				<w:pStyle w:val="Heading2" />
			</w:pPr>
			<w:r>
				<w:t>
					<include item="{$p_titleInclude}"/>
				</w:t>
			</w:r>
		</w:p>
		<xsl:copy-of select="$p_content" />
	</xsl:template>

	<xsl:template name="t_putSubSection">
		<xsl:param name="p_title" />
		<xsl:param name="p_content" />
		<w:p>
			<w:pPr>
				<w:pStyle w:val="Heading4" />
			</w:pPr>
			<xsl:copy-of select="$p_title" />
		</w:p>
		<xsl:copy-of select="$p_content" />
	</xsl:template>

	<!-- ============================================================================================
	Alerts
	============================================================================================= -->

	<xsl:template name="t_putAlert">
		<xsl:param name="p_alertClass" select="@class"/>
		<xsl:param name="p_alertContent" select="''"/>
		<xsl:variable name="v_title">
			<xsl:choose>
				<xsl:when test="$p_alertClass='note'">
					<xsl:text>alert_title_note</xsl:text>
				</xsl:when>
				<xsl:when test="$p_alertClass='tip'">
					<xsl:text>alert_title_tip</xsl:text>
				</xsl:when>
				<xsl:when test="$p_alertClass='caution' or $p_alertClass='warning'">
					<xsl:text>alert_title_caution</xsl:text>
				</xsl:when>
				<xsl:when test="$p_alertClass='security' or $p_alertClass='security note'">
					<xsl:text>alert_title_security</xsl:text>
				</xsl:when>
				<xsl:when test="$p_alertClass='important'">
					<xsl:text>alert_title_important</xsl:text>
				</xsl:when>
				<xsl:when test="$p_alertClass='vb' or $p_alertClass='VB' or $p_alertClass='VisualBasic' or $p_alertClass='visual basic note'">
					<xsl:text>alert_title_visualBasic</xsl:text>
				</xsl:when>
				<xsl:when test="$p_alertClass='cs' or $p_alertClass='CSharp' or $p_alertClass='c#' or $p_alertClass='C#' or $p_alertClass='visual c# note'">
					<xsl:text>alert_title_visualC#</xsl:text>
				</xsl:when>
				<xsl:when test="$p_alertClass='cpp' or $p_alertClass='c++' or $p_alertClass='C++' or $p_alertClass='CPP' or $p_alertClass='visual c++ note'">
					<xsl:text>alert_title_visualC++</xsl:text>
				</xsl:when>
				<xsl:when test="$p_alertClass='JSharp' or $p_alertClass='j#' or $p_alertClass='J#' or $p_alertClass='visual j# note'">
					<xsl:text>alert_title_visualJ#</xsl:text>
				</xsl:when>
				<xsl:when test="$p_alertClass='implement'">
					<xsl:text>text_NotesForImplementers</xsl:text>
				</xsl:when>
				<xsl:when test="$p_alertClass='caller'">
					<xsl:text>text_NotesForCallers</xsl:text>
				</xsl:when>
				<xsl:when test="$p_alertClass='inherit'">
					<xsl:text>text_NotesForInheritors</xsl:text>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text>alert_title_note</xsl:text>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="v_altTitle">
			<xsl:choose>
				<xsl:when test="$p_alertClass='note' or $p_alertClass='implement' or $p_alertClass='caller' or $p_alertClass='inherit'">
					<xsl:text>alert_altText_note</xsl:text>
				</xsl:when>
				<xsl:when test="$p_alertClass='tip'">
					<xsl:text>alert_altText_tip</xsl:text>
				</xsl:when>
				<xsl:when test="$p_alertClass='caution' or $p_alertClass='warning'">
					<xsl:text>alert_altText_caution</xsl:text>
				</xsl:when>
				<xsl:when test="$p_alertClass='security' or $p_alertClass='security note'">
					<xsl:text>alert_altText_security</xsl:text>
				</xsl:when>
				<xsl:when test="$p_alertClass='important'">
					<xsl:text>alert_altText_important</xsl:text>
				</xsl:when>
				<xsl:when test="$p_alertClass='vb' or $p_alertClass='VB' or $p_alertClass='VisualBasic' or $p_alertClass='visual basic note'">
					<xsl:text>alert_altText_visualBasic</xsl:text>
				</xsl:when>
				<xsl:when test="$p_alertClass='cs' or $p_alertClass='CSharp' or $p_alertClass='c#' or $p_alertClass='C#' or $p_alertClass='visual c# note'">
					<xsl:text>alert_altText_visualC#</xsl:text>
				</xsl:when>
				<xsl:when test="$p_alertClass='cpp' or $p_alertClass='c++' or $p_alertClass='C++' or $p_alertClass='CPP' or $p_alertClass='visual c++ note'">
					<xsl:text>alert_altText_visualC++</xsl:text>
				</xsl:when>
				<xsl:when test="$p_alertClass='JSharp' or $p_alertClass='j#' or $p_alertClass='J#' or $p_alertClass='visual j# note'">
					<xsl:text>alert_altText_visualJ#</xsl:text>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text>alert_altText_note</xsl:text>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="v_noteImg">
			<xsl:choose>
				<xsl:when test="$p_alertClass='note' or $p_alertClass='tip' or $p_alertClass='implement' or $p_alertClass='caller' or $p_alertClass='inherit'">
					<xsl:text>alert_note.gif</xsl:text>
				</xsl:when>
				<xsl:when test="$p_alertClass='caution' or $p_alertClass='warning'">
					<xsl:text>alert_caution.gif</xsl:text>
				</xsl:when>
				<xsl:when test="$p_alertClass='security' or $p_alertClass='security note'">
					<xsl:text>alert_security.gif</xsl:text>
				</xsl:when>
				<xsl:when test="$p_alertClass='important'">
					<xsl:text>alert_caution.gif</xsl:text>
				</xsl:when>
				<xsl:when test="$p_alertClass='vb' or $p_alertClass='VB' or $p_alertClass='VisualBasic' or $p_alertClass='visual basic note'">
					<xsl:text>alert_note.gif</xsl:text>
				</xsl:when>
				<xsl:when test="$p_alertClass='cs' or $p_alertClass='CSharp' or $p_alertClass='c#' or $p_alertClass='C#' or $p_alertClass='visual c# note'">
					<xsl:text>alert_note.gif</xsl:text>
				</xsl:when>
				<xsl:when test="$p_alertClass='cpp' or $p_alertClass='c++' or $p_alertClass='C++' or $p_alertClass='CPP' or $p_alertClass='visual c++ note'">
					<xsl:text>alert_note.gif</xsl:text>
				</xsl:when>
				<xsl:when test="$p_alertClass='JSharp' or $p_alertClass='j#' or $p_alertClass='J#' or $p_alertClass='visual j# note'">
					<xsl:text>alert_note.gif</xsl:text>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text>alert_note.gif</xsl:text>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<w:tbl>
			<w:tblPr>
				<w:tblStyle w:val="AlertTable"/>
				<w:tblW w:w="5000" w:type="pct"/>
				<w:tblLook w:firstRow="1" w:noHBand="1" w:noVBand="1"/>
			</w:tblPr>
			<w:tr>
				<w:trPr>
					<w:cnfStyle w:firstRow="1" />
				</w:trPr>
				<w:tc>
					<w:p>
						<w:pPr>
							<w:keepNext />
						</w:pPr>
						<w:r>
							<img src="../media/{$v_noteImg}">
								<includeAttribute name="alt" item="{$v_altTitle}"/>
							</img>
						</w:r>
						<w:r>
							<w:t xml:space="preserve">  </w:t>
						</w:r>
						<include item="{$v_title}"/>
					</w:p>
				</w:tc>
			</w:tr>
			<w:tr>
				<w:tc>
					<xsl:choose>
						<xsl:when test="$p_alertContent=''">
							<xsl:apply-templates/>
						</xsl:when>
						<xsl:otherwise>
							<xsl:copy-of select="$p_alertContent"/>
						</xsl:otherwise>
					</xsl:choose>
				</w:tc>
			</w:tr>
		</w:tbl>
		<w:p>
			<w:pPr>
				<w:spacing w:after="0" />
			</w:pPr>
		</w:p>
	</xsl:template>

	<!-- ============================================================================================
	Debugging template for showing an element in comments
	============================================================================================= -->

	<xsl:template name="t_dumpContent">
		<xsl:param name="indent" select="''"/>
		<xsl:param name="content" select="."/>
		<xsl:for-each select="msxsl:node-set($content)">
			<xsl:choose>
				<xsl:when test="self::text()">
					<xsl:comment>
						<xsl:value-of select="$indent"/>
						<xsl:value-of select="."/>
					</xsl:comment>
				</xsl:when>
				<xsl:otherwise>
					<xsl:comment>
						<xsl:value-of select="$indent"/>
						<xsl:value-of select="'«'"/>
						<xsl:value-of select="name()"/>
						<xsl:for-each select="@*">
							<xsl:text xml:space="preserve"> </xsl:text>
							<xsl:value-of select="name()"/>
							<xsl:value-of select="'='"/>
							<xsl:value-of select="."/>
						</xsl:for-each>
						<xsl:choose>
							<xsl:when test="./node()">
								<xsl:value-of select="'»'"/>
							</xsl:when>
							<xsl:otherwise>
								<xsl:value-of select="'/»'"/>
							</xsl:otherwise>
						</xsl:choose>
					</xsl:comment>
					<xsl:for-each select="node()">
						<xsl:call-template name="t_dumpContent">
							<xsl:with-param name="indent" select="concat($indent,'  ')"/>
						</xsl:call-template>
					</xsl:for-each>
					<xsl:if test="./node()">
						<xsl:comment>
							<xsl:value-of select="$indent"/>
							<xsl:value-of select="'«/'"/>
							<xsl:value-of select="name()"/>
							<xsl:value-of select="'»'"/>
						</xsl:comment>
					</xsl:if>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:for-each>
	</xsl:template>

</xsl:stylesheet>
