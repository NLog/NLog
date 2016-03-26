<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="2.0"
								xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
								xmlns:msxsl="urn:schemas-microsoft-com:xslt"
								xmlns:ddue="http://ddue.schemas.microsoft.com/authoring/2003/5"
								xmlns:xlink="http://www.w3.org/1999/xlink"
								xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main"
>
	<!-- ======================================================================================== -->

	<xsl:import href="GlobalTemplates.xsl"/>
	<xsl:import href="CodeTemplates.xsl"/>

	<!-- ============================================================================================
	The Remarks section includes content from these nodes, excluding the xaml sections which are captured in the xaml syntax processing
	============================================================================================= -->

	<xsl:template name="t_hasRemarksContent">
		<xsl:param name="p_node"/>
		<xsl:choose>
			<xsl:when test="/document/reference/attributes/attribute/type[@api='T:System.Security.Permissions.HostProtectionAttribute']">true</xsl:when>
			<xsl:when test="normalize-space($p_node/ddue:remarks/ddue:content)">true</xsl:when>
			<xsl:when test="normalize-space($p_node/ddue:notesForImplementers)">true</xsl:when>
			<xsl:when test="normalize-space($p_node/ddue:notesForCallers)">true</xsl:when>
			<xsl:when test="normalize-space($p_node/ddue:notesForInheritors)">true</xsl:when>
			<xsl:when test="normalize-space($p_node/ddue:platformNotes)">true</xsl:when>
			<xsl:when test="normalize-space($p_node/ddue:remarks/ddue:sections/ddue:section[not(
                starts-with(@address,'xamlValues') or 
                starts-with(@address,'xamlTextUsage') or 
                starts-with(@address,'xamlAttributeUsage') or 
                starts-with(@address,'xamlPropertyElementUsage') or 
                starts-with(@address,'xamlImplicitCollectionUsage') or 
                starts-with(@address,'xamlObjectElementUsage') or 
                starts-with(@address,'dependencyPropertyInfo') or 
                starts-with(@address,'routedEventInfo')
                )])">true</xsl:when>
		</xsl:choose>
	</xsl:template>

	<!-- ============================================================================================
	Remarks
	============================================================================================= -->

	<xsl:template match="ddue:remarks" name="t_ddue_Remarks">
		<xsl:call-template name="t_writeRemarksSection">
			<xsl:with-param name="p_node" select=".."/>
		</xsl:call-template>
	</xsl:template>

	<xsl:template name="t_writeRemarksSection">
		<xsl:param name="p_node"/>

		<xsl:variable name="v_hasRemarks">
			<xsl:call-template name="t_hasRemarksContent">
				<xsl:with-param name="p_node" select="$p_node"/>
			</xsl:call-template>
		</xsl:variable>

		<xsl:if test="$v_hasRemarks='true'">
			<xsl:choose>
				<xsl:when test="not($g_apiTopicGroup = 'namespace')">
					<xsl:call-template name="t_putSectionInclude">
						<xsl:with-param name="p_titleInclude" select="'title_remarks'"/>
						<xsl:with-param name="p_content">
							<xsl:apply-templates select="$p_node/ddue:remarks/*"/>
							<!-- HostProtectionAttribute -->
							<xsl:if test="/document/reference/attributes/attribute/type[@api='T:System.Security.Permissions.HostProtectionAttribute']">
								<xsl:call-template name="t_hostProtectionContent"/>
							</xsl:if>
							<xsl:apply-templates select="$p_node/ddue:notesForImplementers"/>
							<xsl:apply-templates select="$p_node/ddue:notesForCallers"/>
							<xsl:apply-templates select="$p_node/ddue:notesForInheritors"/>
							<xsl:apply-templates select="$p_node/ddue:platformNotes"/>
						</xsl:with-param>
					</xsl:call-template>
				</xsl:when>
				<xsl:otherwise>
					<xsl:apply-templates select="$p_node/ddue:remarks/*"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:if>
	</xsl:template>

	<xsl:template name="t_hostProtectionContent">
		<!-- HostProtectionAttribute boilerplate -->
		<xsl:call-template name="t_putAlert">
			<xsl:with-param name="p_alertClass" select="'note'"/>
			<xsl:with-param name="p_alertContent">
				<include item="boilerplate_hostProtectionAttribute">
					<parameter>
						<xsl:value-of select="concat('text_', $g_apiTopicSubGroup, 'Lower')"/>
					</parameter>
					<parameter>
						<span class="Label">
							<xsl:for-each select="/document/reference/attributes/attribute[type[@api='T:System.Security.Permissions.HostProtectionAttribute']]/assignment">
								<xsl:value-of select="@name"/>
								<xsl:if test="position() != last()">
									<xsl:text xml:space="preserve"> | </xsl:text>
								</xsl:if>
							</xsl:for-each>
						</span>
					</parameter>
				</include>
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<!-- ============================================================================================
	Sections
	============================================================================================= -->

	<xsl:template match="ddue:sections" name="t_ddue_sections">
		<xsl:apply-templates select="ddue:section"/>
	</xsl:template>

	<xsl:template match="ddue:section" name="t_ddue_section">
		<!-- Display the section only if it has content (text or media)-->
		<xsl:if test="descendant::ddue:content[normalize-space(.)] or descendant::ddue:content/*">

			<xsl:apply-templates select="@address"/>
			<!-- Count all the possible ancestor root nodes -->
			<xsl:variable name="a1" select="count(ancestor::ddue:attributesandElements)"/>
			<xsl:variable name="a2" select="count(ancestor::ddue:codeExample)"/>
			<xsl:variable name="a3" select="count(ancestor::ddue:dotNetFrameworkEquivalent)"/>
			<xsl:variable name="a4" select="count(ancestor::ddue:elementInformation)"/>
			<xsl:variable name="a5" select="count(ancestor::ddue:exceptions)"/>
			<xsl:variable name="a6" select="count(ancestor::ddue:introduction)"/>
			<xsl:variable name="a7" select="count(ancestor::ddue:languageReferenceRemarks)"/>
			<xsl:variable name="a8" select="count(ancestor::ddue:nextSteps)"/>
			<xsl:variable name="a9" select="count(ancestor::ddue:parameters)"/>
			<xsl:variable name="a10" select="count(ancestor::ddue:prerequisites)"/>
			<xsl:variable name="a11" select="count(ancestor::ddue:procedure)"/>
			<xsl:variable name="a12" select="count(ancestor::ddue:relatedTopics)"/>
			<xsl:variable name="a13" select="count(ancestor::ddue:remarks)"/>
			<xsl:variable name="a14" select="count(ancestor::ddue:requirements)"/>
			<xsl:variable name="a15" select="count(ancestor::ddue:schemaHierarchy)"/>
			<xsl:variable name="a16" select="count(ancestor::ddue:syntaxSection)"/>
			<xsl:variable name="a17" select="count(ancestor::ddue:textValue)"/>
			<xsl:variable name="a18" select="count(ancestor::ddue:type)"/>
			<xsl:variable name="a19" select="count(ancestor::ddue:section)"/>
			<xsl:variable name="total" select="$a1+$a2+$a3+$a4+$a5+$a6+$a7+$a8+$a9+$a10+$a11+$a12+$a13+$a14+$a15+$a16+$a17+$a18+$a19"/>
			<xsl:choose>
				<xsl:when test="$total = 0">
					<xsl:call-template name="t_putSection">
						<xsl:with-param name="p_title">
							<xsl:apply-templates select="ddue:title" mode="section"/>
						</xsl:with-param>
						<xsl:with-param name="p_content">
							<xsl:apply-templates select="ddue:content"/>
							<xsl:apply-templates select="ddue:sections"/>
						</xsl:with-param>
					</xsl:call-template>
				</xsl:when>
				<xsl:when test="$total = 1">
					<w:p>
						<w:pPr>
							<w:pStyle w:val="Heading3" />
						</w:pPr>
						<xsl:apply-templates select="ddue:title" mode="section"/>
					</w:p>
					<xsl:apply-templates select="ddue:content"/>
					<xsl:apply-templates select="ddue:sections"/>
				</xsl:when>
				<xsl:otherwise>
					<w:p>
						<w:pPr>
							<w:pStyle w:val="Heading4" />
						</w:pPr>
						<xsl:apply-templates select="ddue:title" mode="section"/>
					</w:p>
					<xsl:apply-templates select="ddue:content"/>
					<xsl:apply-templates select="ddue:sections"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:title" mode="section" name="t_ddue_sectionTitle">
		<xsl:apply-templates/>
	</xsl:template>

	<xsl:template match="@address" name="t_ddue_address">
		<!-- The Open XML file builder task will reformat the bookmark name and ID to ensure that they are unique -->
		<w:bookmarkStart w:name="_{string(.)}" w:id="0" />
		<w:bookmarkEnd w:id="0" />
	</xsl:template>

	<!-- ============================================================================================
	Block Elements
	============================================================================================= -->

	<xsl:template match="ddue:para" name="t_ddue_para">
		<w:p>
			<xsl:choose>
				<!-- Indent the paragraph if it is within a definition -->
				<xsl:when test="parent::ddue:definition">
					<w:pPr>
						<w:ind w:left="432" />
					</w:pPr>
				</xsl:when>
				<!-- In table header cells, keep them together with the next row to avoid splitting them across pages -->
				<xsl:when test="ancestor::ddue:tableHeader">
					<w:pPr>
						<w:keepNext />
					</w:pPr>
				</xsl:when>
				<xsl:otherwise />
			</xsl:choose>
			<xsl:apply-templates />
		</w:p>
	</xsl:template>

	<xsl:template match="ddue:summary" name="t_ddue_summary">
		<xsl:if test="not(@abstract='true')">
			<!-- The ddue:summary element is redundant since it's optional in the MAML schema but ddue:introduction is
					 not.  Using abstract='true' will prevent the summary from being included in the topic. -->
			<xsl:apply-templates />
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:notesForImplementers" name="t_ddue_notesForImplementers">
		<w:p>
			<w:r>
				<w:rPr>
					<w:b />
				</w:rPr>
				<w:t><include item="text_NotesForImplementers"/></w:t>
			</w:r>
		</w:p>
		<xsl:apply-templates/>
	</xsl:template>

	<xsl:template match="ddue:notesForCallers" name="t_ddue_notesForCallers">
		<w:p>
			<w:r>
				<w:rPr>
					<w:b />
				</w:rPr>
				<w:t>
					<include item="text_NotesForCallers"/>
				</w:t>
			</w:r>
		</w:p>
		<xsl:apply-templates/>
	</xsl:template>

	<xsl:template match="ddue:notesForInheritors" name="t_ddue_notesForInheritors">
		<w:p>
			<w:r>
				<w:rPr>
					<w:b />
				</w:rPr>
				<w:t>
					<include item="text_NotesForInheritors"/>
				</w:t>
			</w:r>
		</w:p>
		<xsl:apply-templates/>
	</xsl:template>

	<xsl:template match="ddue:schemaHierarchy" name="t_ddue_schemaHierarchy">
		<xsl:for-each select="ddue:link">
			<xsl:call-template name="t_putIndent">
				<xsl:with-param name="p_count" select="position()"/>
			</xsl:call-template>
			<xsl:apply-templates select="."/>
			<br />
		</xsl:for-each>
	</xsl:template>

	<xsl:template match="ddue:syntaxSection" name="t_ddue_syntaxSection">
		<xsl:if test="ddue:legacySyntax">
			<xsl:for-each select="ddue:legacySyntax">
				<xsl:variable name="v_codeLang">
					<xsl:call-template name="t_codeLang">
						<xsl:with-param name="p_codeLang" select="@language" />
					</xsl:call-template>
				</xsl:variable>

				<xsl:call-template name="t_putCodeSection">
					<xsl:with-param name="p_codeLang" select="$v_codeLang" />
				</xsl:call-template>
			</xsl:for-each>
		</xsl:if>
	</xsl:template>

	<!-- Just pass these through -->
	<xsl:template match="ddue:content" name="t_ddue_content">
		<xsl:apply-templates/>
	</xsl:template>

	<xsl:template match="ddue:legacy" name="t_ddue_legacy">
		<xsl:apply-templates/>
	</xsl:template>

	<xsl:template match="ddue:procedure" name="t_ddue_procedure">
		<xsl:apply-templates select="@address"/>
		<xsl:if test="normalize-space(ddue:title)">
			<w:p>
				<w:pPr>
					<w:pStyle w:val="Heading5" />
				</w:pPr>
				<xsl:value-of select="ddue:title"/>
			</w:p>
		</xsl:if>
		<xsl:apply-templates select="ddue:steps"/>
		<xsl:apply-templates select="ddue:conclusion"/>
	</xsl:template>

	<xsl:template match="ddue:steps" name="t_ddue_steps">
		<xsl:choose>
			<xsl:when test="@class = 'ordered'">
				<xsl:variable name="v_temp">
					<xsl:value-of select="count(ddue:step)"/>
				</xsl:variable>
				<xsl:if test="$v_temp = 1">
					<ul class="bullet">
						<xsl:apply-templates select="ddue:step"/>
					</ul>
				</xsl:if>
				<xsl:if test="$v_temp &gt; 1">
					<ul class="ordered">
						<xsl:apply-templates select="ddue:step"/>
					</ul>
				</xsl:if>
			</xsl:when>
			<xsl:when test="@class='bullet'">
				<ul class="bullet">
					<xsl:apply-templates select="ddue:step"/>
				</ul>
			</xsl:when>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="ddue:step" name="t_ddue_step">
		<li>
			<xsl:apply-templates select="@address"/>
			<xsl:apply-templates/>
		</li>
	</xsl:template>

	<xsl:template match="ddue:inThisSection" name="t_ddue_inThisSection">
		<xsl:call-template name="t_putSectionInclude">
			<xsl:with-param name="p_titleInclude" select="'title_inThisSection'"/>
			<xsl:with-param name="p_content">
				<xsl:apply-templates/>
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="ddue:buildInstructions" name="t_ddue_buildInstructions">
		<xsl:if test="normalize-space(.)">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude" select="'title_buildInstructions'"/>
				<xsl:with-param name="p_content">
					<xsl:apply-templates/>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:nextSteps" name="t_ddue_nextSteps">
		<xsl:if test="normalize-space(.)">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude" select="'title_nextSteps'"/>
				<xsl:with-param name="p_content">
					<xsl:apply-templates/>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:requirements" name="t_ddue_requirements">
		<xsl:if test="normalize-space(.)">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude" select="'title_requirements'"/>
				<xsl:with-param name="p_content">
					<xsl:apply-templates/>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:languageReferenceRemarks" name="t_ddue_languageReferenceRemarks">
		<xsl:if test="normalize-space(.)">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude" select="'title_remarks'"/>
				<xsl:with-param name="p_content">
					<xsl:apply-templates/>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:attributesandElements" name="t_ddue_attributesandElements">
		<xsl:if test="normalize-space(.)">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude" select="'title_attributesAndElements'"/>
				<xsl:with-param name="p_content">
					<xsl:apply-templates/>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:attributes" name="t_ddue_attributes">
		<xsl:if test="normalize-space(.)">
			<w:p>
				<w:pPr>
					<w:pStyle w:val="Heading4" />
				</w:pPr>
				<include item="title_attributes"/>
			</w:p>
			<xsl:apply-templates/>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:attribute" name="t_ddue_attribute">
		<xsl:apply-templates/>
	</xsl:template>

	<xsl:template match="ddue:attribute/ddue:title" name="t_ddue_attributeTitle">
		<w:p>
			<w:pPr>
				<w:pStyle w:val="Heading4" />
			</w:pPr>
			<xsl:apply-templates/>
		</w:p>
	</xsl:template>

	<xsl:template match="ddue:childElement" name="t_ddue_childElement">
		<xsl:if test="normalize-space(.)">
			<w:p>
				<w:pPr>
					<w:pStyle w:val="Heading4" />
				</w:pPr>
				<include item="title_childElement"/>
			</w:p>
			<xsl:apply-templates/>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:parentElement" name="t_ddue_parentElement">
		<xsl:if test="normalize-space(.)">
			<w:p>
				<w:pPr>
					<w:pStyle w:val="Heading4" />
				</w:pPr>
				<include item="title_parentElement"/>
			</w:p>
			<xsl:apply-templates/>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:textValue" name="t_ddue_textValue">
		<xsl:if test="normalize-space(.)">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude" select="'title_textValue'"/>
				<xsl:with-param name="p_content">
					<xsl:apply-templates/>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:elementInformation" name="t_ddue_elementInformation">
		<xsl:if test="normalize-space(.)">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude" select="'title_elementInformation'"/>
				<xsl:with-param name="p_content">
					<xsl:apply-templates/>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:dotNetFrameworkEquivalent" name="t_ddue_dotNetFrameworkEquivalent">
		<xsl:if test="normalize-space(.)">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude" select="'title_dotNetFrameworkEquivalent'"/>
				<xsl:with-param name="p_content">
					<xsl:apply-templates/>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:prerequisites" name="t_ddue_prerequisites">
		<xsl:if test="normalize-space(.)">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude" select="'title_prerequisites'"/>
				<xsl:with-param name="p_content">
					<xsl:apply-templates/>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:type" name="t_ddue_type">
		<xsl:apply-templates/>
	</xsl:template>

	<xsl:template match="ddue:title_robustProgramming" name="t_ddue_robustProgramming">
		<xsl:if test="normalize-space(.)">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude" select="'title_robustProgramming'"/>
				<xsl:with-param name="p_content">
					<xsl:apply-templates/>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:security" name="t_ddue_security">
		<xsl:if test="normalize-space(.)">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude" select="'title_securitySection'"/>
				<xsl:with-param name="p_content">
					<xsl:apply-templates/>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:externalResources" name="t_ddue_externalResources">
		<xsl:if test="normalize-space(.)">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude" select="'title_externalResources'"/>
				<xsl:with-param name="p_content">
					<xsl:apply-templates/>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:demonstrates" name="t_ddue_demonstrates">
		<xsl:if test="normalize-space(.)">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude" select="'title_demonstrates'"/>
				<xsl:with-param name="p_content">
					<xsl:apply-templates/>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:appliesTo" name="t_ddue_appliesTo">
		<xsl:if test="normalize-space(.)">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude" select="'title_appliesTo'"/>
				<xsl:with-param name="p_content">
					<xsl:apply-templates/>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:conclusion" name="t_ddue_conclusion">
		<xsl:apply-templates/>
	</xsl:template>

	<xsl:template match="ddue:background" name="t_ddue_background">
		<xsl:if test="normalize-space(.)">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude" select="'title_background'"/>
				<xsl:with-param name="p_content">
					<xsl:apply-templates/>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:whatsNew" name="t_ddue_whatsNew">
		<xsl:if test="normalize-space(.)">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude" select="'title_whatsNew'"/>
				<xsl:with-param name="p_content">
					<xsl:apply-templates/>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:reference" name="t_ddue_reference">
		<xsl:if test="normalize-space(.)">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude" select="'title_reference'"/>
				<xsl:with-param name="p_content">
					<xsl:apply-templates/>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:developerErrorMessageDocument" name="t_ddue_developerErrorMessageDocument">
		<xsl:for-each select="*">
			<xsl:choose>
				<xsl:when test="name() = 'secondaryErrorTitle'">
					<xsl:if test="not(../ddue:nonLocErrorTitle)">
						<xsl:apply-templates select=".">
							<xsl:with-param name="newSection">yes</xsl:with-param>
						</xsl:apply-templates>
					</xsl:if>
				</xsl:when>

				<xsl:otherwise>
					<xsl:apply-templates select="."/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:for-each>

	</xsl:template>

	<xsl:template match="ddue:nonLocErrorTitle" name="t_ddue_nonLocErrorTitle">
		<xsl:if test="string-length(../ddue:nonLocErrorTitle[normalize-space(.)]) > 0 or string-length(../ddue:secondaryErrorTitle[normalize-space(.)]) > 0">
			<xsl:if test="../ddue:secondaryErrorTitle">
				<w:p>
					<w:pPr>
						<w:pStyle w:val="Heading4" />
					</w:pPr>
					<include item="title_errorMessage"/>
				</w:p>
				<xsl:apply-templates select="../ddue:secondaryErrorTitle">
					<xsl:with-param name="newSection">no</xsl:with-param>
				</xsl:apply-templates>
			</xsl:if>
			<xsl:apply-templates/>
			<w:p />
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:secondaryErrorTitle" name="t_ddue_secondaryErrorTitle">
		<xsl:param name="newSection"/>
		<xsl:if test="string-length(../ddue:secondaryErrorTitle[normalize-space(.)]) > 0">
			<xsl:choose>
				<xsl:when test="$newSection = 'yes'">
					<xsl:apply-templates/>
					<w:p />
				</xsl:when>
				<xsl:otherwise>
					<xsl:apply-templates/>
					<br />
				</xsl:otherwise>
			</xsl:choose>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:developerSampleDocument" name="t_ddue_developerSampleDocument">
		<!-- Show the topic intro -->
		<xsl:apply-templates select="ddue:introduction"/>

		<!-- The sample download list section from dsSample -->
		<xsl:if test="ddue:relatedTopics/ddue:sampleRef">
			<include item="{ddue:relatedTopics/ddue:sampleRef/@srcID}"/>
		</xsl:if>

		<!-- Then the rest of the topic's content -->
		<xsl:for-each select="*">
			<xsl:choose>
				<!-- Introduction was already captured above -->
				<xsl:when test="name() = 'introduction'"/>

				<xsl:otherwise>
					<xsl:apply-templates select="."/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:for-each>

	</xsl:template>

	<!-- ============================================================================================
	Lists and Tables
	============================================================================================= -->

	<xsl:template match="ddue:list" name="t_ddue_list">
		<ul>
			<xsl:attribute name="class">
				<xsl:value-of select="@class" />
			</xsl:attribute>
			<xsl:if test="@class='ordered' and @start">
				<xsl:attribute name="start">
					<xsl:value-of select="@start"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:apply-templates select="ddue:listItem"/>
		</ul>
	</xsl:template>

	<xsl:template match="ddue:listItem" name="t_ddue_listItem">
		<li>
			<xsl:apply-templates select="@address" />
			<xsl:apply-templates />
		</li>
	</xsl:template>

	<xsl:template match="ddue:table" name="t_ddue_table">
		<xsl:if test="normalize-space(ddue:title)">
			<w:p>
				<w:pPr>
					<w:pStyle w:val="Heading3" />
				</w:pPr>
				<w:r>
					<w:t><xsl:value-of select="ddue:title"/></w:t>
				</w:r>
			</w:p>
		</xsl:if>
		<w:tbl>
			<w:tblPr>
				<w:tblStyle w:val="GeneralTable"/>
				<w:tblW w:w="5000" w:type="pct"/>
				<xsl:choose>
					<xsl:when test="ddue:tableHeader">
						<w:tblLook w:firstRow="1" w:noHBand="1" w:noVBand="1"/>
					</xsl:when>
					<xsl:otherwise>
						<w:tblLook w:firstRow="0" w:noHBand="1" w:noVBand="1"/>
					</xsl:otherwise>
				</xsl:choose>
			</w:tblPr>
			<xsl:apply-templates/>
		</w:tbl>
		<w:p>
			<w:pPr>
				<w:spacing w:after="0" />
			</w:pPr>
		</w:p>
	</xsl:template>

	<xsl:template match="ddue:tableHeader" name="t_ddue_tableHeader">
		<xsl:apply-templates/>
	</xsl:template>

	<xsl:template match="ddue:tableHeader/ddue:row" name="t_ddue_tableHeaderRow">
		<w:tr>
			<w:trPr>
				<w:cnfStyle w:firstRow="1" />
			</w:trPr>
			<xsl:apply-templates/>
		</w:tr>
	</xsl:template>

	<xsl:template match="ddue:row" name="t_ddue_row">
		<w:tr>
			<xsl:apply-templates/>
		</w:tr>
	</xsl:template>

	<xsl:template match="ddue:entry" name="t_ddue_entry">
		<w:tc>
			<xsl:apply-templates select="@address"/>
			<xsl:apply-templates/>
		</w:tc>
	</xsl:template>

	<xsl:template match="ddue:definitionTable" name="t_ddue_definitionTable">
		<xsl:apply-templates/>
	</xsl:template>

	<xsl:template match="ddue:definedTerm" name="t_ddue_definedTerm">
		<xsl:apply-templates select="@address"/>
		<w:p>
			<w:pPr>
				<w:spacing w:after="0" />
			</w:pPr>
			<span class="Bold">
				<xsl:apply-templates/>
			</span>
		</w:p>
	</xsl:template>

	<xsl:template match="ddue:definition" name="t_ddue_definition">
		<xsl:apply-templates/>
	</xsl:template>

	<!-- ============================================================================================
	Code
	============================================================================================= -->

	<xsl:template match="ddue:snippets" name="t_ddue_snippets">
		<xsl:if test="ddue:snippet">
			<xsl:for-each select="ddue:snippet">
				<xsl:call-template name="t_putCodeSection">
					<xsl:with-param name="p_codeLang" select="@language" />
				</xsl:call-template>
			</xsl:for-each>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:code | ddue:sampleCode" name="t_ddue_code">
		<xsl:variable name="v_codeLang">
			<xsl:call-template name="t_codeLang">
				<xsl:with-param name="p_codeLang" select="@language" />
			</xsl:call-template>
		</xsl:variable>

		<xsl:call-template name="t_putCodeSection">
			<xsl:with-param name="p_codeLang" select="$v_codeLang" />
		</xsl:call-template>
	</xsl:template>

	<!-- ============================================================================================
	Alerts
	============================================================================================= -->

	<xsl:template match="ddue:alert" name="t_ddue_alert">
		<xsl:call-template name="t_putAlert"/>
	</xsl:template>

	<!-- ============================================================================================
	Media
	============================================================================================= -->

	<xsl:template match="ddue:mediaLink" name="t_ddue_mediaLink">
		<xsl:if test="ddue:caption and not(ddue:caption[@placement='after'])">
			<w:p>
				<w:pPr>
					<w:pStyle w:val="Caption" />
					<xsl:choose>
						<xsl:when test="ddue:image[@placement='center']">
							<w:jc w:val="center" />
						</xsl:when>
						<xsl:when test="ddue:image[@placement='far']">
							<w:pPr>
								<w:jc w:val="right" />
							</w:pPr>
						</xsl:when>
						<xsl:otherwise>
							<w:pPr>
								<w:jc w:val="left" />
							</w:pPr>
						</xsl:otherwise>
					</xsl:choose>
					<w:keepNext />
				</w:pPr>
				<xsl:if test="ddue:caption[@lead]">
					<w:r>
						<w:rPr>
							<w:b />
						</w:rPr>
						<w:t xml:space="preserve"><xsl:value-of select="normalize-space(ddue:caption/@lead)"/>: </w:t>
					</w:r>
				</xsl:if>
				<xsl:apply-templates select="ddue:caption"/>
			</w:p>
		</xsl:if>
		<w:p>
			<w:pPr>
				<xsl:choose>
					<xsl:when test="ddue:image[@placement='center']">
						<w:jc w:val="center" />
					</xsl:when>
					<xsl:when test="ddue:image[@placement='far']">
						<w:jc w:val="right" />
					</xsl:when>
					<xsl:otherwise>
						<w:jc w:val="left" />
					</xsl:otherwise>
				</xsl:choose>
				<xsl:if test="ddue:caption and ddue:caption[@placement='after']">
					<w:keepNext />
				</xsl:if>
			</w:pPr>
			<w:r>
				<artLink target="{ddue:image/@xlink:href}"/>
			</w:r>
		</w:p>
		<xsl:if test="ddue:caption and ddue:caption[@placement='after']">
			<w:p>
				<w:pPr>
					<w:pStyle w:val="Caption" />
					<xsl:choose>
						<xsl:when test="ddue:image[@placement='center']">
							<w:jc w:val="center" />
						</xsl:when>
						<xsl:when test="ddue:image[@placement='far']">
							<w:pPr>
								<w:jc w:val="right" />
							</w:pPr>
						</xsl:when>
						<xsl:otherwise>
							<w:pPr>
								<w:jc w:val="left" />
							</w:pPr>
						</xsl:otherwise>
					</xsl:choose>
				</w:pPr>
				<xsl:if test="ddue:caption[@lead]">
					<w:r>
						<w:rPr>
							<w:b />
						</w:rPr>
						<w:t xml:space="preserve"><xsl:value-of select="normalize-space(ddue:caption/@lead)"/>: </w:t>
					</w:r>
				</xsl:if>
				<xsl:apply-templates select="ddue:caption"/>
			</w:p>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:mediaLinkInline" name="t_ddue_mediaLinkInline">
		<w:r>
			<artLink target="{ddue:image/@xlink:href}"/>
		</w:r>
	</xsl:template>

	<!-- ============================================================================================
	Inline elements
	============================================================================================= -->

	<!-- Pass spans through.  They'll be converted by the document builder task. -->
	<xsl:template match="ddue:span" name="t_ddue_span">
		<span>
			<xsl:attribute name="class">
				<xsl:value-of select="@class" />
			</xsl:attribute>
			<xsl:copy-of select="node()"/>
		</span>
	</xsl:template>

	<xsl:template match="ddue:span" name="t_ddue_spanPreserveFormatting" mode="preserveFormatting">
		<span>
			<xsl:attribute name="class">
				<xsl:value-of select="@class" />
			</xsl:attribute>
			<xsl:copy-of select="node()"/>
		</span>
	</xsl:template>

	<xsl:template match="ddue:languageKeyword" name="t_ddue_languageKeyword">
		<xsl:variable name="v_keyword" select="."/>
		<xsl:choose>
			<xsl:when test="$v_keyword='null' or $v_keyword='Nothing' or $v_keyword='nullptr'">
				<include item="devlang_nullKeyword"/>
			</xsl:when>
			<xsl:when test="$v_keyword='static' or $v_keyword='Shared'">
				<include item="devlang_staticKeyword"/>
			</xsl:when>
			<xsl:when test="$v_keyword='virtual' or $v_keyword='Overridable'">
				<include item="devlang_virtualKeyword"/>
			</xsl:when>
			<xsl:when test="$v_keyword='true' or $v_keyword='True'">
				<include item="devlang_trueKeyword"/>
			</xsl:when>
			<xsl:when test="$v_keyword='false' or $v_keyword='False'">
				<include item="devlang_falseKeyword"/>
			</xsl:when>
			<xsl:when test="$v_keyword='abstract' or $v_keyword='MustInherit'">
				<include item="devlang_abstractKeyword"/>
			</xsl:when>
			<xsl:when test="$v_keyword='sealed' or $v_keyword='NotInheritable'">
				<include item="devlang_sealedKeyword"/>
			</xsl:when>
			<xsl:otherwise>
				<w:r>
					<w:rPr>
						<w:rStyle w:val="CodeInline" />
					</w:rPr>
					<!-- Keep this on the same line to prevent extra space from getting included -->
					<w:t xml:space="preserve"><xsl:value-of select="."/></w:t>
				</w:r>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<!-- ======================================================================================== -->

	<xsl:template match="ddue:application" name="t_ddue_application">
		<xsl:if test="normalize-space(.)">
			<span class="Bold">
				<xsl:apply-templates/>
			</span>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:codeFeaturedElement" name="t_ddue_codeFeaturedElement">
		<xsl:if test="normalize-space(.)">
			<span class="Label">
				<xsl:apply-templates/>
			</span>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:codeInline" name="t_ddue_codeInline">
		<xsl:if test="normalize-space(.)">
			<w:r>
				<w:rPr>
					<w:rStyle w:val="CodeInline"/>
				</w:rPr>
				<w:t>
					<xsl:value-of select="." />
				</w:t>
			</w:r>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:command" name="t_ddue_command">
		<xsl:if test="normalize-space(.)">
			<span class="Command">
				<xsl:apply-templates />
			</span>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:computerOutputInline" name="t_ddue_computerOutputInline">
		<xsl:call-template name="t_ddue_codeInline"/>
	</xsl:template>

	<xsl:template match="ddue:corporation" name="t_ddue_corporation">
		<xsl:apply-templates/>
	</xsl:template>

	<xsl:template match="ddue:country" name="t_ddue_country">
		<xsl:apply-templates/>
	</xsl:template>

	<xsl:template match="ddue:database" name="t_ddue_database">
		<xsl:if test="normalize-space(.)">
			<span class="Bold">
				<xsl:apply-templates/>
			</span>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:date" name="t_ddue_date">
		<xsl:apply-templates/>
	</xsl:template>

	<xsl:template match="ddue:embeddedLabel" name="t_ddue_embeddedLabel">
		<xsl:if test="normalize-space(.)">
			<span class="Label">
				<xsl:apply-templates/>
			</span>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:environmentVariable" name="t_ddue_environmentVariable">
		<xsl:call-template name="t_ddue_codeInline"/>
	</xsl:template>

	<xsl:template match="ddue:errorInline" name="t_ddue_errorInline">
		<xsl:if test="normalize-space(.)">
			<span class="Emphasis">
				<xsl:apply-templates/>
			</span>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:hardware" name="t_ddue_hardware">
		<xsl:if test="normalize-space(.)">
			<span class="Bold">
				<xsl:apply-templates/>
			</span>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:fictitiousUri" name="t_ddue_fictitiousUri">
		<xsl:call-template name="t_ddue_localUri"/>
	</xsl:template>

	<xsl:template match="ddue:foreignPhrase" name="t_ddue_foreignPhrase">
		<xsl:if test="normalize-space(.)">
			<span class="ForeignPhrase">
				<xsl:apply-templates/>
			</span>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:legacyBold" name="t_ddue_legacyBold">
		<xsl:if test="normalize-space(.)">
			<span class="Bold">
				<xsl:apply-templates/>
			</span>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:legacyItalic" name="t_ddue_legacyItalic">
		<xsl:if test="normalize-space(.)">
			<span class="Emphasis">
				<xsl:apply-templates />
			</span>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:legacyUnderline" name="t_ddue_legacyUnderline">
		<xsl:if test="normalize-space(.)">
			<span class="Underline">
				<xsl:apply-templates />
			</span>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:literal" name="t_ddue_literal">
		<xsl:if test="normalize-space(.)">
			<span class="Literal">
				<xsl:apply-templates/>
			</span>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:localizedText" name="t_ddue_localizedText">
		<xsl:apply-templates/>
	</xsl:template>

	<xsl:template match="ddue:localUri" name="t_ddue_localUri">
		<xsl:if test="normalize-space(.)">
			<span class="Emphasis">
				<xsl:apply-templates/>
			</span>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:math" name="t_ddue_math">
		<xsl:if test="normalize-space(.)">
			<span class="Math">
				<xsl:apply-templates/>
			</span>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:newTerm" name="t_ddue_newTerm">
		<xsl:if test="normalize-space(.)">
			<span class="Term">
				<xsl:apply-templates/>
			</span>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:parameterReference" name="t_ddue_parameterReference">
		<xsl:if test="normalize-space(.)">
			<span class="Parameter">
				<xsl:apply-templates/>
			</span>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:phrase" name="t_ddue_phrase">
		<xsl:if test="normalize-space(.)">
			<span class="Phrase">
				<xsl:apply-templates/>
			</span>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:quote" name="t_ddue_quote">
		<xsl:if test="normalize-space(.)">
			<w:p>
				<w:pPr>
					<w:pStyle w:val="Quote" />
					<w:ind w:left="432" w:right="432" />
				</w:pPr>
				<xsl:apply-templates/>
			</w:p>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:quoteInline" name="t_ddue_quoteInline">
		<xsl:if test="normalize-space(.)">
			<span class="QuoteChar">
				<xsl:apply-templates/>
			</span>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:placeholder|ddue:replaceable" name="t_ddue_replaceable">
		<xsl:if test="normalize-space(.)">
			<span class="Placeholder">
				<xsl:apply-templates/>
			</span>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:subscript|ddue:subscriptType" name="t_ddue_subscript">
		<xsl:if test="normalize-space(.)">
			<span class="Subscript">
				<xsl:apply-templates/>
			</span>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:superscript|ddue:superscriptType" name="t_ddue_superscript">
		<xsl:if test="normalize-space(.)">
			<span class="Superscript">
				<xsl:apply-templates/>
			</span>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:system" name="t_ddue_system">
		<xsl:if test="normalize-space(.)">
			<span class="Bold">
				<xsl:apply-templates/>
			</span>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:ui" name="t_ddue_ui">
		<xsl:if test="normalize-space(.)">
			<span class="UserInterface">
				<xsl:apply-templates/>
			</span>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:unmanagedCodeEntityReference" name="t_ddue_unmanagedCodeEntityReference">
		<xsl:if test="normalize-space(.)">
			<span class="Bold">
				<xsl:apply-templates/>
			</span>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:userInput" name="t_ddue_userInput">
		<xsl:if test="normalize-space(.)">
			<span class="UserInput">
				<xsl:value-of select="." />
			</span>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:userInputLocalizable" name="t_ddue_userInputLocalizable">
		<xsl:call-template name="t_ddue_userInput"/>
	</xsl:template>

	<!-- ============================================================================================
	Pass through a chunk of markup.  This differs from the API markup template in that it must strip
	off the "ddue" namespace.  This will allow build components to add Open XML elements to a
	pre-transformed document.  You can also use it in topics to support things that aren't addressed
	by the MAML schema and the Sandcastle transforms.
	============================================================================================= -->

	<xsl:template match="ddue:markup" name="t_ddue_markup">
		<xsl:apply-templates select="node()" mode="markup"/>
	</xsl:template>

	<xsl:template match="*" mode="markup" name="t_ddue_markup_content">
		<xsl:element name="{name()}">
			<xsl:copy-of select="@*"/>
			<xsl:apply-templates select="node()" mode="markup"/>
		</xsl:element>
	</xsl:template>

	<xsl:template match="text() | comment()" mode="markup" name="t_ddue_markup_text">
		<xsl:copy-of select="."/>
	</xsl:template>

	<!-- ============================================================================================
	Links
	============================================================================================= -->

	<xsl:template match="ddue:externalLink" name="t_ddue_externalLink">
		<a>
			<xsl:attribute name="href">
				<xsl:value-of select="normalize-space(ddue:linkUri)"/>
			</xsl:attribute>
			<xsl:if test="normalize-space(ddue:linkAlternateText)">
				<xsl:attribute name="title">
					<xsl:value-of select="normalize-space(ddue:linkAlternateText)"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:value-of select="normalize-space(ddue:linkText)"/>
		</a>
	</xsl:template>

	<xsl:template match="ddue:link" name="t_ddue_link">
		<xsl:choose>
			<xsl:when test="starts-with(@xlink:href,'#')">
				<!-- in-page link -->
				<a href="{@xlink:href}">
					<xsl:apply-templates/>
				</a>
			</xsl:when>
			<xsl:otherwise>
				<!-- verified, external link -->
				<conceptualLink target="{@xlink:href}">
					<xsl:apply-templates/>
				</conceptualLink>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="ddue:legacyLink" name="t_ddue_legacyLink">
		<a href="{@xlink:href}">
			<xsl:apply-templates />
		</a>
	</xsl:template>

	<xsl:template match="ddue:codeEntityReference" name="t_ddue_codeEntityReference">
		<referenceLink target="{normalize-space(string(.))}">
			<xsl:if test="@qualifyHint">
				<xsl:attribute name="show-container">
					<xsl:value-of select="@qualifyHint"/>
				</xsl:attribute>
				<xsl:attribute name="show-parameters">
					<xsl:value-of select="@qualifyHint"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="@autoUpgrade">
				<xsl:attribute name="prefer-overload">
					<xsl:value-of select="@autoUpgrade"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="normalize-space(@linkText)">
				<xsl:value-of select="normalize-space(@linkText)"/>
			</xsl:if>
		</referenceLink>
	</xsl:template>

	<!-- ============================================================================================
	Copyright notice
	============================================================================================= -->

	<xsl:template match="ddue:copyright" name="t_ddue_copyright">
		<!-- Paragraph: {0} &copy;{1}{2}. All rights reserved. -->
		<include item="boilerplate_copyrightNotice">
			<parameter>
				<xsl:value-of select="ddue:trademark" />
			</parameter>
			<parameter>
				<xsl:for-each select="ddue:year">
					<xsl:if test="position() = 1">
						<xsl:text> </xsl:text>
					</xsl:if>
					<xsl:value-of select="."/>
					<xsl:if test="position() != last()">
						<xsl:text>, </xsl:text>
					</xsl:if>
				</xsl:for-each>
			</parameter>
			<parameter>
				<xsl:for-each select="ddue:holder">
					<xsl:if test="position() = 1">
						<xsl:text> </xsl:text>
					</xsl:if>
					<xsl:value-of select="."/>
					<xsl:if test="position() != last()">
						<xsl:text>, </xsl:text>
					</xsl:if>
				</xsl:for-each>
			</parameter>
		</include>
	</xsl:template>

	<!-- ============================================================================================
	Glossary
	============================================================================================= -->

	<xsl:key name="k_glossaryTermFirstLetters" match="//ddue:glossaryEntry"
		use="translate(substring(ddue:terms/ddue:term/text(),1,1),'abcdefghijklmnopqrstuvwxyz','ABCDEFGHIJKLMNOPQRSTUVWXYZ ')"/>

	<xsl:template match="ddue:glossary" name="t_ddue_glossary">
		<xsl:if test="ddue:title">
			<w:p>
				<w:pPr>
					<w:pStyle w:val="Heading1" />
				</w:pPr>
				<w:r>
					<w:t><xsl:value-of select="normalize-space(ddue:title)" /></w:t>
				</w:r>
			</w:p>
		</xsl:if>
		<xsl:choose>
			<xsl:when test="ddue:glossaryDiv">
				<w:p>
					<!-- Organized glossary with glossaryDiv elements -->
					<xsl:for-each select="ddue:glossaryDiv">
						<xsl:if test="ddue:title">
							<xsl:choose>
								<xsl:when test="@address">
									<w:hyperlink w:history="1" w:anchor="_{string(@address)}">
										<w:r>
											<w:rPr>
												<w:rStyle w:val="Hyperlink" />
											</w:rPr>
											<w:t><xsl:value-of select="ddue:title" /></w:t>
										</w:r>
									</w:hyperlink>
								</xsl:when>
								<xsl:otherwise>
									<w:r>
										<w:t><xsl:value-of select="ddue:title" /></w:t>
									</w:r>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:if>
						<xsl:if test="position() != last()">
							<w:r>
								<w:t xsl:space="preserve"> | </w:t>
							</w:r>
						</xsl:if>
					</xsl:for-each>
				</w:p>
				<xsl:apply-templates select="ddue:glossaryDiv"/>
			</xsl:when>
			<xsl:otherwise>
				<!-- Simple glossary consisting of nothing by glossaryEntry elements -->
				<xsl:call-template name="t_glossaryLetterBar"/>
				<xsl:call-template name="t_glossaryGroupByEntriesTermFirstLetter"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="ddue:glossaryDiv" name="t_ddue_glossaryDiv">
		<xsl:if test="@address">
			<!-- The Open XML file builder task will reformat the bookmark name and ID to ensure that they are unique -->
			<w:bookmarkStart w:name="_{string(@address)}" w:id="0" />
			<w:bookmarkEnd w:id="0" />
		</xsl:if>
		<xsl:if test="ddue:title">
			<w:p>
				<w:pPr>
					<w:pStyle w:val="Heading2" />
				</w:pPr>
				<w:r>
					<w:t>
						<xsl:value-of select="normalize-space(ddue:title)" />
					</w:t>
				</w:r>
			</w:p>
		</xsl:if>
		<xsl:call-template name="t_glossaryLetterBar">
			<xsl:with-param name="p_sectionPrefix" select="generate-id()"/>
		</xsl:call-template>
		<xsl:call-template name="t_glossaryGroupByEntriesTermFirstLetter">
			<xsl:with-param name="p_sectionPrefix" select="generate-id()"/>
		</xsl:call-template>
	</xsl:template>

	<xsl:template name="t_glossaryGroupByEntriesTermFirstLetter">
		<xsl:param name="p_sectionPrefix" select="''"/>
		<xsl:variable name="v_div" select="."/>
		<!-- Group entries by the first letter of their terms using the Muenchian method.
         http://www.jenitennison.com/xslt/grouping/muenchian.html -->
		<xsl:for-each select="ddue:glossaryEntry[generate-id() = 
                  generate-id(key('k_glossaryTermFirstLetters',
                  translate(substring(ddue:terms/ddue:term[1]/text(),1,1),$g_allLowerCaseLetters,concat($g_allUpperCaseLetters,' ')))
                  [parent::node() = $v_div][1])]">
			<xsl:sort select="ddue:terms/ddue:term[1]" />
			<xsl:variable name="v_letter"
										select="translate(substring(ddue:terms/ddue:term[1]/text(),1,1),$g_allLowerCaseLetters,concat($g_allUpperCaseLetters,' '))"/>

			<xsl:call-template name="t_glossaryEntryGroup">
				<xsl:with-param name="p_link" select="concat($p_sectionPrefix,$v_letter)"/>
				<xsl:with-param name="p_name" select="$v_letter"/>
				<xsl:with-param name="p_nodes"
												select="key('k_glossaryTermFirstLetters',
                        translate($v_letter,$g_allLowerCaseLetters,concat($g_allUpperCaseLetters,' ')))
                        [parent::node() = $v_div]"/>
			</xsl:call-template>
		</xsl:for-each>
	</xsl:template>

	<xsl:template match="ddue:glossaryEntry" name="t_ddue_glossaryEntry">
		<w:p>
			<w:pPr>
				<w:spacing w:after="0" />
			</w:pPr>
			<xsl:apply-templates select="@address" />
			<xsl:for-each select="ddue:terms/ddue:term">
				<xsl:sort select="normalize-space(.)" />

				<xsl:if test="@termId">
					<!-- The Open XML file builder task will reformat the bookmark name and ID to ensure that they are unique -->
					<w:bookmarkStart w:name="_{string(@termId)}" w:id="0" />
					<w:bookmarkEnd w:id="0" />
				</xsl:if>
				<span class="Bold">
					<xsl:value-of select="normalize-space(.)" />
				</span>
				<xsl:if test="position() != last()">
					<w:r>
						<w:t xsl:space="preserve"><xsl:text>, </xsl:text></w:t>
					</w:r>
				</xsl:if>
			</xsl:for-each>
		</w:p>
		<xsl:apply-templates select="ddue:definition/*"/>

		<xsl:if test="ddue:relatedEntry">
			<w:p>
				<w:pPr>
					<w:ind w:left="432" />
				</w:pPr>

				<w:r>
					<w:t xsl:space="preserve"><include item="text_relatedEntries" />&#160;&#160;</w:t>
				</w:r>

				<xsl:for-each select="ddue:relatedEntry">
					<xsl:variable name="id" select="@termId" />
					<w:hyperlink w:history="1" w:anchor="_{string(@termId)}">
						<w:r>
							<w:rPr>
								<w:rStyle w:val="Hyperlink" />
							</w:rPr>
							<w:t>
								<xsl:value-of select="//ddue:term[@termId=$id]"/>
							</w:t>
						</w:r>
					</w:hyperlink>
					<xsl:if test="position() != last()">
						<w:r>
							<w:t xsl:space="preserve">,&#160;</w:t>
						</w:r>
					</xsl:if>
				</xsl:for-each>
			</w:p>
		</xsl:if>
	</xsl:template>

	<xsl:template name="t_glossaryEntryGroup">
		<xsl:param name="p_link"/>
		<xsl:param name="p_name"/>
		<xsl:param name="p_nodes"/>
		<!-- The Open XML file builder task will reformat the bookmark name and ID to ensure that they are unique -->
		<w:bookmarkStart w:name="_{string($p_link)}" w:id="0" />
		<w:bookmarkEnd w:id="0" />
		<w:p>
			<w:pPr>
				<w:pStyle w:val="Heading3" />
			</w:pPr>
			<w:r>
				<w:t><xsl:value-of select="$p_name"/></w:t>
			</w:r>
		</w:p>
		<xsl:apply-templates select="$p_nodes">
			<xsl:sort select="ddue:terms/ddue:term"/>
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template name="t_glossaryLetterBar">
		<xsl:param name="p_sectionPrefix" select="''"/>
		<w:p>
			<xsl:call-template name="t_glossaryLetterBarLinkRecursive">
				<xsl:with-param name="p_sectionPrefix" select="$p_sectionPrefix"/>
				<xsl:with-param name="p_bar" select="$g_allUpperCaseLetters"/>
				<xsl:with-param name="p_characterPosition" select="1"/>
			</xsl:call-template>
		</w:p>
	</xsl:template>

	<xsl:template name="t_glossaryLetterBarLinkRecursive">
		<xsl:param name="p_sectionPrefix"/>
		<xsl:param name="p_bar"/>
		<xsl:param name="p_characterPosition"/>
		<xsl:variable name="v_letter" select="substring($p_bar,$p_characterPosition,1)"/>
		<xsl:if test="$v_letter">
			<xsl:choose>
				<xsl:when test="ddue:glossaryEntry[ddue:terms/ddue:term[1]
                  [translate(substring(text(),1,1),$g_allLowerCaseLetters,concat($g_allUpperCaseLetters,' ')) = $v_letter]]">
					<xsl:call-template name="t_glossaryLetterBarLink">
						<xsl:with-param name="p_link" select="concat($p_sectionPrefix,$v_letter)"/>
						<xsl:with-param name="p_name" select="$v_letter"/>
					</xsl:call-template>
					<xsl:if test="not($p_characterPosition = string-length($p_bar))">
						<w:r>
							<w:t xml:space="preserve"> | </w:t>
						</w:r>
					</xsl:if>
				</xsl:when>
				<xsl:otherwise>
					<xsl:call-template name="t_glossaryLetterBarLink">
						<xsl:with-param name="p_name" select="$v_letter"/>
					</xsl:call-template>
					<xsl:if test="not($p_characterPosition = string-length($p_bar))">
						<w:r>
							<w:t xml:space="preserve"> | </w:t>
						</w:r>
					</xsl:if>
				</xsl:otherwise>
			</xsl:choose>
			<xsl:call-template name="t_glossaryLetterBarLinkRecursive">
				<xsl:with-param name="p_sectionPrefix" select="$p_sectionPrefix"/>
				<xsl:with-param name="p_bar" select="$p_bar"/>
				<xsl:with-param name="p_characterPosition" select="$p_characterPosition + 1"/>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template name="t_glossaryLetterBarLink">
		<xsl:param name="p_link"/>
		<xsl:param name="p_name"/>
		<xsl:choose>
			<xsl:when test="$p_link">
				<w:hyperlink w:history="1" w:anchor="_{string($p_link)}">
					<w:r>
						<w:rPr>
							<w:rStyle w:val="Hyperlink" />
						</w:rPr>
						<w:t><xsl:value-of select="$p_name"/></w:t>
					</w:r>
				</w:hyperlink>
			</xsl:when>
			<xsl:otherwise>
				<span class="Bold">
					<xsl:value-of select="$p_name"/>
				</span>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

</xsl:stylesheet>
