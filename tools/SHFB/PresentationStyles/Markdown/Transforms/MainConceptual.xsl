<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="2.0"
								xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
								xmlns:msxsl="urn:schemas-microsoft-com:xslt"
								xmlns:ddue="http://ddue.schemas.microsoft.com/authoring/2003/5"
								xmlns:xlink="http://www.w3.org/1999/xlink"
>
	<!-- ======================================================================================== -->

	<xsl:import href="GlobalTemplates.xsl"/>
	<xsl:import href="CodeTemplates.xsl"/>
	<xsl:import href="MAMLTemplates.xsl"/>
	<xsl:import href="MAMLSeeAlsoTemplates.xsl"/>
	<xsl:import href="Bibliography.xsl"/>

	<xsl:output method="xml" omit-xml-declaration="yes" indent="no" encoding="utf-8"/>

	<!-- ============================================================================================
	Parameters - key parameter is the API identifier string
	============================================================================================= -->

	<xsl:param name="key"/>
	<xsl:param name="bibliographyData" select="'../data/bibliography.xml'"/>

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
		<document>
			<xsl:text>&#xa;</xsl:text>
			<xsl:text>&#xa;</xsl:text>
			<xsl:text># </xsl:text>
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
			<span id="PageHeader">
				<xsl:text> </xsl:text>
			</span>
			<xsl:text>&#xa;</xsl:text>
			<xsl:apply-templates select="topic"/>
			<xsl:text>&#xa;</xsl:text>
		</document>
	</xsl:template>

	<!-- ============================================================================================
	Sections that behave differently in conceptual and reference
	============================================================================================= -->

	<!-- Ignore the title -->
	<xsl:template match="ddue:title" />

	<xsl:template match="ddue:introduction">
		<!-- Display the introduction only if it has content -->
		<xsl:if test="count(*) &gt; 0">
			<xsl:if test="@address">
				<span>
					<xsl:attribute name="id">
						<!-- Special case.  Links to this address will get converted to a link to the page header
								 since the introduction has no title itself. -->
						<xsl:text>@pageHeader_</xsl:text>
						<xsl:value-of select="@address"/>
					</xsl:attribute>
					<xsl:text> </xsl:text>
				</span>
			</xsl:if>
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
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude" select="'title_relatedTopics'"/>
				<xsl:with-param name="p_id" select="'seeAlsoSection'"/>
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
					<xsl:text>&#xa;</xsl:text>
				</xsl:when>
				<xsl:when test="normalize-space($p_intro)">
					<xsl:text>&#xa;</xsl:text>
					<xsl:value-of select="normalize-space($p_intro)"/>
					<xsl:text>&#xa;</xsl:text>
				</xsl:when>
				<xsl:when test="$p_outlineType='toplevel' or $p_outlineType='topNoRelated'">
					<xsl:text>&#xa;</xsl:text>
					<include item="boilerplate_autoOutlineTopLevelIntro"/>
					<xsl:text>&#xa;</xsl:text>
				</xsl:when>
				<xsl:when test="$p_outlineType='subsection'">
					<xsl:text>&#xa;</xsl:text>
					<include item="boilerplate_autoOutlineSubsectionIntro"/>
					<xsl:text>&#xa;</xsl:text>
				</xsl:when>
			</xsl:choose>
			<xsl:text>&#xa;</xsl:text>
			<xsl:text>&#160;</xsl:text>
			<ul>
				<xsl:for-each select="ddue:section[ddue:title[normalize-space(.)!='']]">
					<xsl:call-template name="t_outlineSectionEntry"/>

					<!-- Expand sub-sections too if wanted -->
					<xsl:if test="$p_depth &lt; $p_maxDepth">
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
					<li>
						<a>
							<xsl:attribute name="href">#seeAlsoSection</xsl:attribute>
							<include item="title_relatedTopics"/>
						</a>
					</li>
					<xsl:text>&#xa;</xsl:text>
				</xsl:if>
			</ul>
			<xsl:text>&#160;</xsl:text>
			<xsl:text>&#xa;</xsl:text>
		</xsl:if>
	</xsl:template>

	<!-- A list item in the outline's bullet list -->
	<xsl:template name="t_outlineSectionEntry">
		<xsl:if test="descendant::ddue:content[normalize-space(.)] or count(ddue:content/*) &gt; 0">
			<xsl:text>&#xa;</xsl:text>
			<li>
				<xsl:choose>
					<xsl:when test="@address">
						<a href="#{@address}">
							<xsl:value-of select="ddue:title"/>
						</a>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="ddue:title"/>
					</xsl:otherwise>
				</xsl:choose>
				<xsl:if test="normalize-space(ddue:summary)">
					<xsl:apply-templates select="ddue:summary/node()"/>
				</xsl:if>
			</li>
			<xsl:text>&#160;</xsl:text>
			<xsl:text>&#xa;</xsl:text>
		</xsl:if>
	</xsl:template>

	<!-- ============================================================================================
	Bibliography
	============================================================================================= -->

	<xsl:key name="k_citations" match="//ddue:cite" use="text()"/>

	<xsl:variable name="g_hasCitations" select="boolean(count(//ddue:cite) > 0)"/>

	<xsl:template match="ddue:cite" name="t_ddue_cite">
		<xsl:variable name="v_currentCitation" select="text()"/>
		<xsl:for-each select="//ddue:cite[generate-id(.)=generate-id(key('k_citations',text()))]">
			<!-- Distinct citations only -->
			<xsl:if test="$v_currentCitation=.">
				<xsl:choose>
					<xsl:when test="document($bibliographyData)/bibliography/reference[@name=$v_currentCitation]">
						<sup>
							<a>
								<xsl:attribute name="href">
									#cite<xsl:value-of select="position()"/>
								</xsl:attribute>[<xsl:value-of select="position()"/>]
							</a>
						</sup>
					</xsl:when>
					<xsl:otherwise>
						<xsl:apply-templates/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:if>
		</xsl:for-each>
	</xsl:template>

	<xsl:template match="ddue:bibliography" name="t_ddue_bibliography">
		<xsl:if test="$g_hasCitations">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude" select="'bibliographyTitle'"/>
				<xsl:with-param name="p_content">
					<xsl:call-template name="t_autogenBibliographyLinks"/>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template name="t_autogenBibliographyLinks">
		<xsl:for-each select="//ddue:cite[generate-id(.)=generate-id(key('k_citations',text()))]">
			<!-- Distinct citations only -->
			<xsl:variable name="v_citation" select="."/>
			<xsl:variable name="entry" select="document($bibliographyData)/bibliography/reference[@name=$v_citation]"/>

			<xsl:call-template name="bibliographyReference">
				<xsl:with-param name="number" select="position()"/>
				<xsl:with-param name="data" select="$entry"/>
			</xsl:call-template>
		</xsl:for-each>
	</xsl:template>

</xsl:stylesheet>
