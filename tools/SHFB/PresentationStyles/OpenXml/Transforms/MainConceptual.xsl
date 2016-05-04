<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="2.0"
								xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
								xmlns:msxsl="urn:schemas-microsoft-com:xslt"
								xmlns:ddue="http://ddue.schemas.microsoft.com/authoring/2003/5"
								xmlns:xlink="http://www.w3.org/1999/xlink"
								xmlns:wpc="http://schemas.microsoft.com/office/word/2010/wordprocessingCanvas"
								xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
								xmlns:o="urn:schemas-microsoft-com:office:office"
								xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships"
								xmlns:m="http://schemas.openxmlformats.org/officeDocument/2006/math"
								xmlns:v="urn:schemas-microsoft-com:vml"
								xmlns:wp14="http://schemas.microsoft.com/office/word/2010/wordprocessingDrawing"
								xmlns:wp="http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing"
								xmlns:w10="urn:schemas-microsoft-com:office:word"
								xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main"
								xmlns:w14="http://schemas.microsoft.com/office/word/2010/wordml"
								xmlns:wpg="http://schemas.microsoft.com/office/word/2010/wordprocessingGroup"
								xmlns:wpi="http://schemas.microsoft.com/office/word/2010/wordprocessingInk"
								xmlns:wne="http://schemas.microsoft.com/office/word/2006/wordml"
								xmlns:wps="http://schemas.microsoft.com/office/word/2010/wordprocessingShape"
								xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main"
								xmlns:a14="http://schemas.microsoft.com/office/drawing/2010/main"
								xmlns:pic="http://schemas.openxmlformats.org/drawingml/2006/picture"
>
	<!-- ======================================================================================== -->

	<xsl:import href="GlobalTemplates.xsl"/>
	<xsl:import href="CodeTemplates.xsl"/>
	<xsl:import href="MAMLTemplates.xsl"/>
	<xsl:import href="MAMLSeeAlsoTemplates.xsl"/>

	<xsl:output method="xml" omit-xml-declaration="yes" indent="no" encoding="utf-8"/>

	<!-- ============================================================================================
	Parameters - key parameter is the API identifier string - see globalTemplates for others
	============================================================================================= -->

	<xsl:param name="key"/>

	<!-- ============================================================================================
	Global Variables
	============================================================================================= -->

	<xsl:variable name="g_hasSeeAlsoSection"
								select="boolean(count(/document/topic/*/ddue:relatedTopics/*[local-name()!='sampleRef']) > 0)"/>
	<xsl:variable name="g_apiTopicGroup" />
	<xsl:variable name="g_apiTopicSubGroup" />
	<xsl:variable name="g_apiTopicSubSubGroup" />

	<!-- ============================================================================================
	Document
	============================================================================================= -->

	<xsl:template match="/document" name="t_document">
		<w:document mc:Ignorable="w14 wp14">
			<w:body>
				<w:p>
					<w:pPr>
						<w:pStyle w:val="Heading1" />
					</w:pPr>
					<!-- The Open XML file builder task will reformat the bookmark name and ID to ensure that they are unique -->
					<w:bookmarkStart w:name="_Topic" w:id="0" />
					<w:bookmarkEnd w:id="0" />
					<w:r>
						<w:t>
							<include item="boilerplate_pageTitle">
								<parameter>
									<xsl:choose>
										<xsl:when test="normalize-space(/document/metadata/title)">
											<xsl:value-of select="normalize-space(/document/metadata/title)"/>
										</xsl:when>
										<xsl:otherwise>
											<xsl:value-of select="normalize-space(/document/topic/*/ddue:title)"/>
										</xsl:otherwise>
									</xsl:choose>
								</parameter>
							</include>
						</w:t>
					</w:r>
				</w:p>
				<xsl:apply-templates select="topic"/>
			</w:body>
		</w:document>
	</xsl:template>

	<!-- ============================================================================================
	Sections that behave differently in conceptual and reference
	============================================================================================= -->

	<!-- Ignore the title -->
	<xsl:template match="ddue:title" />

	<xsl:template match="ddue:introduction">
		<!-- Display the introduction only if it has content -->
		<xsl:if test="count(*) &gt; 0">
			<xsl:apply-templates select="@address"/>
			<xsl:apply-templates/>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:parameters">
		<xsl:if test="normalize-space(.)">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude" select="'title_parameters'"/>
				<xsl:with-param name="p_content">
					<xsl:apply-templates/>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:returnValue">
		<xsl:if test="normalize-space(.)">
			<xsl:choose>
				<xsl:when test="(normalize-space(ddue:content)='') and ddue:sections/ddue:section[ddue:title='Property Value']">
					<xsl:call-template name="t_putSectionInclude">
						<xsl:with-param name="p_titleInclude" select="'title_propertyValue'"/>
						<xsl:with-param name="p_content">
							<xsl:apply-templates select="ddue:sections/ddue:section[ddue:title='Property Value']/*"/>
						</xsl:with-param>
					</xsl:call-template>
				</xsl:when>
				<xsl:otherwise>
					<xsl:call-template name="t_putSectionInclude">
						<xsl:with-param name="p_titleInclude" select="'title_returnValue'"/>
						<xsl:with-param name="p_content">
							<xsl:apply-templates/>
						</xsl:with-param>
					</xsl:call-template>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:exceptions">
		<xsl:if test="normalize-space(.)">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude" select="'title_exceptions'"/>
				<xsl:with-param name="p_content">
					<xsl:apply-templates/>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:relatedSections">
		<xsl:if test="normalize-space(.)">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude" select="'title_relatedSections'"/>
				<xsl:with-param name="p_content">
					<xsl:apply-templates/>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:relatedTopics">
		<xsl:if test="$g_hasSeeAlsoSection">
			<!-- The Open XML file builder task will reformat the bookmark name and ID to ensure that they are unique -->
			<w:bookmarkStart w:name="_SeeAlso" w:id="0" />
			<w:bookmarkEnd w:id="0" />
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude" select="'title_relatedTopics'"/>
				<xsl:with-param name="p_content">
					<xsl:apply-templates select="/document/topic/*/ddue:relatedTopics" mode="seeAlso"/>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:codeExample">
		<!-- Create Example section for the first codeExample node -->
		<xsl:if test="not(preceding-sibling::ddue:codeExample) and ../ddue:codeExample[normalize-space(.)!='']">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude" select="'title_example'"/>
				<xsl:with-param name="p_content">
					<xsl:apply-templates/>
					<!-- If there are additional codeExample nodes, put them inside this section -->
					<xsl:for-each select="following-sibling::ddue:codeExample">
						<xsl:apply-templates/>
					</xsl:for-each>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:codeReference">
		<xsl:apply-templates/>
	</xsl:template>

	<!-- ============================================================================================
	<autoOutline/> or <autoOutline>[#]</autoOutline>
  
	Inserts a bullet list of links to the topic's sections or a section's sub-sections with optional support for
	limiting the expansion down to a specific level.  Authors can use the tag directly or specify a token (defined
	in a token file) in a topic's introduction to get a bullet list of the sections; or in a
	ddue:section/ddue:content to get a bullet list of the section's sub-sections.  If the token is used, the
	shared content component replaces <token>autoOutline</token> with an <autoOutline/> node that you specify.
	This was the old way of doing it but this version allows it to be specified directly like any other MAML tag.
	Examples:

  <autoOutline/>                Show only top-level topic titles
  <autoOutline>1</autoOutline>  Show top-level titles and titles for one level down
  <autoOutline>3</autoOutline>  Show titles from the top down to three levels
	============================================================================================= -->

	<xsl:template match="autoOutline|ddue:autoOutline" name="t_autoOutline">
		<xsl:variable name="v_maxDepth">
			<xsl:choose>
				<xsl:when test="normalize-space(.)">
					<xsl:value-of select="number(normalize-space(.))"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="number(0)"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="v_intro" select="@lead"/>
		<xsl:variable name="p_outlineType">
			<xsl:choose>
				<xsl:when test="@excludeRelatedTopics = 'true'">
					<xsl:value-of select="string('topNoRelated')"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="string('toplevel')"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:choose>
			<!-- If <autoOutline/> is in introduction, it outlines the topic's top level sections -->
			<xsl:when test="ancestor::ddue:introduction">
				<xsl:for-each select="ancestor::ddue:introduction/parent::*">
					<xsl:call-template name="t_insertAutoOutline">
						<xsl:with-param name="p_intro">
							<xsl:value-of select="$v_intro"/>
						</xsl:with-param>
						<xsl:with-param name="p_outlineType">
							<xsl:value-of select="$p_outlineType"/>
						</xsl:with-param>
						<xsl:with-param name="p_depth">
							<xsl:value-of select="number(0)"/>
						</xsl:with-param>
						<xsl:with-param name="p_maxDepth">
							<xsl:value-of select="$v_maxDepth"/>
						</xsl:with-param>
					</xsl:call-template>
				</xsl:for-each>
			</xsl:when>
			<!-- If <autoOutline/> is in section/content, it outlines the section's subsections -->
			<xsl:when test="ancestor::ddue:content[parent::ddue:section]">
				<xsl:for-each select="ancestor::ddue:content/parent::ddue:section/ddue:sections">
					<xsl:call-template name="t_insertAutoOutline">
						<xsl:with-param name="p_intro">
							<xsl:value-of select="$v_intro"/>
						</xsl:with-param>
						<xsl:with-param name="p_outlineType">subsection</xsl:with-param>
						<xsl:with-param name="p_depth">
							<xsl:value-of select="number(0)"/>
						</xsl:with-param>
						<xsl:with-param name="p_maxDepth">
							<xsl:value-of select="$v_maxDepth"/>
						</xsl:with-param>
					</xsl:call-template>
				</xsl:for-each>
			</xsl:when>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="t_insertAutoOutline">
		<xsl:param name="p_intro"/>
		<xsl:param name="p_outlineType"/>
		<xsl:param name="p_depth"/>
		<xsl:param name="p_maxDepth"/>
		<!-- Insert an outline if there are sections with title and address -->
		<xsl:if test="ddue:section[ddue:title[normalize-space(.)!='']]">
			<!-- Insert a boilerplate intro -->
			<xsl:choose>
				<xsl:when test="normalize-space($p_intro) = 'none'">
					<xsl:text></xsl:text>
				</xsl:when>
				<xsl:when test="normalize-space($p_intro)">
					<w:p>
						<w:r>
							<w:t>
								<xsl:value-of select="normalize-space($p_intro)"/>
							</w:t>
						</w:r>
					</w:p>
				</xsl:when>
				<xsl:when test="$p_outlineType='toplevel' or $p_outlineType='topNoRelated'">
					<w:p>
						<w:r>
							<w:t>
								<include item="boilerplate_autoOutlineTopLevelIntro"/>
							</w:t>
						</w:r>
					</w:p>
				</xsl:when>
				<xsl:when test="$p_outlineType='subsection'">
					<w:p>
						<w:r>
							<w:t>
								<include item="boilerplate_autoOutlineSubsectionIntro"/>
							</w:t>
						</w:r>
					</w:p>
				</xsl:when>
			</xsl:choose>
			<xsl:for-each select="ddue:section[ddue:title[normalize-space(.)!='']]">
				<xsl:call-template name="t_outlineSectionEntry">
					<xsl:with-param name="p_depth">
						<xsl:value-of select="$p_depth"/>
					</xsl:with-param>
				</xsl:call-template>

				<!-- Expand sub-sections too if wanted up to a maximum of 9 level (Open XML's limit) -->
				<xsl:if test="$p_depth &lt; $p_maxDepth and $p_depth &lt; 9">
					<xsl:for-each select="ddue:sections">
						<xsl:call-template name="t_insertAutoOutline">
							<xsl:with-param name="p_outlineType">subsubsection</xsl:with-param>
							<xsl:with-param name="p_depth">
								<xsl:value-of select="$p_depth + 1"/>
							</xsl:with-param>
							<xsl:with-param name="p_maxDepth">
								<xsl:value-of select="$p_maxDepth"/>
							</xsl:with-param>
						</xsl:call-template>
					</xsl:for-each>
				</xsl:if>
			</xsl:for-each>
			<!-- For top level outlines include a link to See Also -->
			<xsl:if test="starts-with($p_outlineType,'toplevel') and count(//ddue:relatedTopics/*) > 0">
				<w:p>
					<w:pPr>
						<w:pStyle w:val="ListParagraph" />
						<w:numPr>
							<w:ilvl w:val="0" />
							<w:numId w:val="1" />
						</w:numPr>
					</w:pPr>
					<w:hyperlink w:history="1" w:anchor="_SeeAlso">
						<w:r>
							<w:rPr>
								<w:rStyle w:val="Hyperlink" />
							</w:rPr>
							<w:t>
								<include item="title_relatedTopics"/>
							</w:t>
						</w:r>
					</w:hyperlink>
				</w:p>
			</xsl:if>
		</xsl:if>
	</xsl:template>

	<!-- A list item in the outline's bullet list-->
	<xsl:template name="t_outlineSectionEntry">
		<xsl:param name="p_depth"/>
		<xsl:if test="descendant::ddue:content[normalize-space(.)] or count(ddue:content/*) &gt; 0">
			<xsl:choose>
				<xsl:when test="@address">
					<w:p>
						<w:pPr>
							<w:pStyle w:val="ListParagraph" />
							<w:numPr>
								<w:ilvl w:val="{string($p_depth)}" />
								<w:numId w:val="1" />
							</w:numPr>
						</w:pPr>
						<w:hyperlink w:history="1" w:anchor="_{string(@address)}">
							<w:r>
								<w:rPr>
									<w:rStyle w:val="Hyperlink" />
								</w:rPr>
								<w:t>
									<xsl:value-of select="ddue:title"/>
								</w:t>
							</w:r>
						</w:hyperlink>
					</w:p>
				</xsl:when>
				<xsl:otherwise>
					<w:p>
						<w:pPr>
							<w:pStyle w:val="ListParagraph" />
							<w:numPr>
								<w:ilvl w:val="{string($p_depth)}" />
								<w:numId w:val="1" />
							</w:numPr>
						</w:pPr>
						<w:r>
							<w:t>
								<xsl:value-of select="ddue:title"/>
							</w:t>
						</w:r>
					</w:p>
				</xsl:otherwise>
			</xsl:choose>
			<xsl:if test="normalize-space(ddue:summary)">
				<w:p>
					<w:pPr>
						<w:ind w:left="720" w:hanging="360" />
					</w:pPr>
					<xsl:apply-templates select="ddue:summary/para[1]/*"/>
				</w:p>
			</xsl:if>
		</xsl:if>
	</xsl:template>

	<!-- ====================================================================================================== -->

	<!-- Bibliography is not supported -->
	<xsl:template match="ddue:bibliography" />
	<xsl:template match="ddue:cite" />

</xsl:stylesheet>
