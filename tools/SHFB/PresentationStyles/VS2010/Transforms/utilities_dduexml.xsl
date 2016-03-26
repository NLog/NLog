<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
								version="2.0"
								xmlns:ddue="http://ddue.schemas.microsoft.com/authoring/2003/5"
								xmlns:xlink="http://www.w3.org/1999/xlink"
								xmlns:msxsl="urn:schemas-microsoft-com:xslt"
								xmlns:MSHelp="http://msdn.microsoft.com/mshelp"
>
	<!-- ======================================================================================== -->

	<xsl:import href="globalTemplates.xsl"/>
	<xsl:import href="codeTemplates.xsl"/>

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

	<xsl:template match="ddue:remarks"
								name="t_ddue_Remarks">
		<xsl:call-template name="t_writeRemarksSection">
			<xsl:with-param name="p_node"
											select=".."/>
		</xsl:call-template>
	</xsl:template>

	<xsl:template name="t_writeRemarksSection">
		<xsl:param name="p_node"/>

		<xsl:variable name="v_hasRemarks">
			<xsl:call-template name="t_hasRemarksContent">
				<xsl:with-param name="p_node"
												select="$p_node"/>
			</xsl:call-template>
		</xsl:variable>

		<xsl:if test="$v_hasRemarks='true'">
			<xsl:choose>
				<xsl:when test="not($g_apiTopicGroup = 'namespace')">
					<xsl:call-template name="t_putSectionInclude">
						<xsl:with-param name="p_titleInclude"
														select="'title_remarks'"/>
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
							<include item="meta_mshelp_KTable">
								<parameter>
									<xsl:text>tt_</xsl:text>
									<xsl:value-of select="$key"/>
								</parameter>
							</include>
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
			<xsl:with-param name="p_alertClass"
											select="'note'"/>
			<xsl:with-param name="p_alertContent">
				<include item="boilerplate_hostProtectionAttribute">
					<parameter>
						<xsl:value-of select="concat('text_', $g_apiTopicSubGroup, 'Lower')"/>
					</parameter>
					<parameter>
						<span class="label">
							<xsl:for-each select="/document/reference/attributes/attribute[type[@api='T:System.Security.Permissions.HostProtectionAttribute']]/assignment">
								<xsl:value-of select="@name"/>
								<xsl:if test="position() != last()">
									<xsl:text> | </xsl:text>
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

	<xsl:template match="ddue:sections"
								name="t_ddue_sections">
		<xsl:apply-templates select="ddue:section"/>
	</xsl:template>

	<xsl:template match="ddue:section"
								name="t_ddue_section">
		<!-- display the section only if it has content (text or media)-->
		<xsl:if test="descendant::ddue:content[normalize-space(.)] or descendant::ddue:content/*">
			<!-- Count all the possible ancestor root nodes -->
			<xsl:variable name="a1"
										select="count(ancestor::ddue:attributesandElements)"/>
			<xsl:variable name="a2"
										select="count(ancestor::ddue:codeExample)"/>
			<xsl:variable name="a3"
										select="count(ancestor::ddue:dotNetFrameworkEquivalent)"/>
			<xsl:variable name="a4"
										select="count(ancestor::ddue:elementInformation)"/>
			<xsl:variable name="a5"
										select="count(ancestor::ddue:exceptions)"/>
			<xsl:variable name="a6"
										select="count(ancestor::ddue:introduction)"/>
			<xsl:variable name="a7"
										select="count(ancestor::ddue:languageReferenceRemarks)"/>
			<xsl:variable name="a8"
										select="count(ancestor::ddue:nextSteps)"/>
			<xsl:variable name="a9"
										select="count(ancestor::ddue:parameters)"/>
			<xsl:variable name="a10"
										select="count(ancestor::ddue:prerequisites)"/>
			<xsl:variable name="a11"
										select="count(ancestor::ddue:procedure)"/>
			<xsl:variable name="a12"
										select="count(ancestor::ddue:relatedTopics)"/>
			<xsl:variable name="a13"
										select="count(ancestor::ddue:remarks)"/>
			<xsl:variable name="a14"
										select="count(ancestor::ddue:requirements)"/>
			<xsl:variable name="a15"
										select="count(ancestor::ddue:schemaHierarchy)"/>
			<xsl:variable name="a16"
										select="count(ancestor::ddue:syntaxSection)"/>
			<xsl:variable name="a17"
										select="count(ancestor::ddue:textValue)"/>
			<xsl:variable name="a18"
										select="count(ancestor::ddue:type)"/>
			<xsl:variable name="a19"
										select="count(ancestor::ddue:section)"/>
			<xsl:variable name="total"
										select="$a1+$a2+$a3+$a4+$a5+$a6+$a7+$a8+$a9+$a10+$a11+$a12+$a13+$a14+$a15+$a16+$a17+$a18+$a19"/>
			<xsl:choose>
				<!-- Don't render the 'Change History' section here; it's handled in the t_writeChangeHistorySection template. -->
				<xsl:when test="ddue:title = 'Change History'"/>

				<xsl:when test="($total = 0) or ($total = 1)">
					<xsl:call-template name="t_putSection">
						<xsl:with-param name="p_title">
							<xsl:apply-templates select="ddue:title" mode="section"/>
						</xsl:with-param>
						<xsl:with-param name="p_id" select="@address" />
						<xsl:with-param name="p_content">
							<xsl:apply-templates select="ddue:content"/>
							<xsl:apply-templates select="ddue:sections"/>
						</xsl:with-param>
					</xsl:call-template>
				</xsl:when>
				<xsl:otherwise>
					<h4 class="subHeading">
						<xsl:if test="@address">
							<xsl:attribute name="id">
								<xsl:value-of select="@address"/>
							</xsl:attribute>
						</xsl:if>
						<xsl:apply-templates select="ddue:title" mode="section"/>
					</h4>
					<div class="subsection">
						<xsl:apply-templates select="ddue:content"/>
						<xsl:apply-templates select="ddue:sections"/>
					</div>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:title"
								mode="section"
								name="t_ddue_sectionTitle">
		<xsl:apply-templates/>
	</xsl:template>

	<!-- ============================================================================================
	Block Elements
	============================================================================================= -->

	<xsl:template match="ddue:para"
								name="t_ddue_para">
		<p>
			<xsl:apply-templates />
		</p>
	</xsl:template>

	<xsl:template match="ddue:summary"
								name="t_ddue_summary">
		<xsl:if test="not(@abstract='true')">
			<!-- The ddue:summary element is redundant since it's optional in
           the MAML schema but ddue:introduction is not.  Using abstract='true'
           will prevent the summary from being included in the topic. -->
			<div class="summary">
				<xsl:apply-templates />
			</div>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:notesForImplementers" name="t_ddue_notesForImplementers">
		<p>
			<span class="label">
				<include item="text_NotesForImplementers"/>
			</span>
		</p>
		<xsl:apply-templates/>
	</xsl:template>

	<xsl:template match="ddue:notesForCallers" name="t_ddue_notesForCallers">
		<p>
			<span class="label">
				<include item="text_NotesForCallers"/>
			</span>
		</p>
		<xsl:apply-templates/>
	</xsl:template>

	<xsl:template match="ddue:notesForInheritors" name="t_ddue_notesForInheritors">
		<p>
			<span class="label">
				<include item="text_NotesForInheritors"/>
			</span>
		</p>
		<xsl:apply-templates/>
	</xsl:template>

	<xsl:template match="ddue:platformNotes"
								name="t_ddue_platformNotes">
		<xsl:for-each select="ddue:platformNote[normalize-space(ddue:content)]">
			<p>
				<include item="boilerplate_PlatformNote">
					<parameter>
						<xsl:for-each select="ddue:platforms/ddue:platform">
							<xsl:variable name="v_platformName">
								<xsl:value-of select="."/>
							</xsl:variable>
							<include item="{$v_platformName}"/>
							<xsl:if test="position() != last()">, </xsl:if>
						</xsl:for-each>
					</parameter>
					<parameter>
						<xsl:apply-templates select="ddue:content"/>
					</parameter>
				</include>
			</p>
		</xsl:for-each>
	</xsl:template>

	<xsl:template match="ddue:platformNotes/ddue:platformNote/ddue:content/ddue:para"
								name="t_ddue_platformNote_para">
		<xsl:apply-templates/>
	</xsl:template>

	<xsl:template match="ddue:schemaHierarchy"
								name="t_ddue_schemaHierarchy">
		<xsl:for-each select="ddue:link">
			<xsl:call-template name="t_putIndent">
				<xsl:with-param name="p_count"
												select="position()"/>
			</xsl:call-template>
			<xsl:apply-templates select="."/>
			<br/>
		</xsl:for-each>
	</xsl:template>

	<xsl:template match="ddue:syntaxSection" name="t_ddue_syntaxSection">
		<div id="syntaxSection" class="section">
			<xsl:if test="ddue:legacySyntax">
				<div id="snippetGroup_Syntax" class="code">
					<xsl:for-each select="ddue:legacySyntax">
						<div class="OH_CodeSnippetContainerCode">
							<pre xml:space="preserve"><xsl:apply-templates xml:space="preserve"/></pre>
						</div>
					</xsl:for-each>
				</div>
			</xsl:if>
		</div>
	</xsl:template>

	<!-- just pass these through -->
	<xsl:template match="ddue:content"
								name="t_ddue_content">
		<xsl:apply-templates/>
	</xsl:template>
	<xsl:template match="ddue:legacy"
								name="t_ddue_legacy">
		<xsl:apply-templates/>
	</xsl:template>

	<xsl:template match="ddue:procedure"
								name="t_ddue_procedure">
		<xsl:if test="normalize-space(ddue:title)">
			<h3 class="procedureSubHeading">
				<xsl:if test="@address">
					<xsl:attribute name="id">
						<xsl:value-of select="@address"/>
					</xsl:attribute>
				</xsl:if>
				<xsl:value-of select="ddue:title"/>
			</h3>
		</xsl:if>
		<div class="subSection">
			<xsl:if test="@address and not(normalize-space(ddue:title))">
				<xsl:attribute name="id">
					<xsl:value-of select="@address"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:apply-templates select="ddue:steps"/>
			<xsl:apply-templates select="ddue:conclusion"/>
		</div>
	</xsl:template>

	<xsl:template match="ddue:steps"
								name="t_ddue_steps">
		<xsl:choose>
			<xsl:when test="@class = 'ordered'">
				<xsl:variable name="v_temp">
					<xsl:value-of select="count(ddue:step)"/>
				</xsl:variable>
				<xsl:if test="$v_temp = 1">
					<ul>
						<xsl:apply-templates select="ddue:step"/>
					</ul>
			</xsl:if>
				<xsl:if test="$v_temp &gt; 1">
					<ol>
						<xsl:apply-templates select="ddue:step"/>
					</ol>
				</xsl:if>
			</xsl:when>
			<xsl:when test="@class='bullet'">
				<ul>
					<xsl:apply-templates select="ddue:step"/>
				</ul>
			</xsl:when>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="ddue:step"
								name="t_ddue_step">
		<li>
			<xsl:if test="@address">
				<xsl:attribute name="id">
					<xsl:value-of select="@address"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:apply-templates/>
		</li>
	</xsl:template>


	<xsl:template match="ddue:inThisSection"
								name="t_ddue_inThisSection">
		<xsl:call-template name="t_putSectionInclude">
			<xsl:with-param name="p_titleInclude"
											select="'title_inThisSection'"/>
			<xsl:with-param name="p_content">
				<xsl:apply-templates/>
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="ddue:buildInstructions"
								name="t_ddue_buildInstructions">
		<xsl:if test="normalize-space(.)">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude"
												select="'title_buildInstructions'"/>
				<xsl:with-param name="p_content">
					<xsl:apply-templates/>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:nextSteps"
								name="t_ddue_nextSteps">
		<xsl:if test="normalize-space(.)">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude"
												select="'title_nextSteps'"/>
				<xsl:with-param name="p_content">
					<xsl:apply-templates/>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:requirements"
								name="t_ddue_requirements">
		<xsl:if test="normalize-space(.)">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude"
												select="'title_requirements'"/>
				<xsl:with-param name="p_content">
					<xsl:apply-templates/>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:languageReferenceRemarks"
								name="t_ddue_languageReferenceRemarks">
		<xsl:if test="normalize-space(.)">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude"
												select="'title_remarks'"/>
				<xsl:with-param name="p_content">
					<xsl:apply-templates/>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:attributesandElements"
								name="t_ddue_attributesandElements">
		<xsl:if test="normalize-space(.)">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude"
												select="'title_attributesAndElements'"/>
				<xsl:with-param name="p_content">
					<xsl:apply-templates/>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:attributes"
								name="t_ddue_attributes">
		<xsl:if test="normalize-space(.)">
			<h4 class="subHeading">
				<include item="title_attributes"/>
			</h4>
			<xsl:apply-templates/>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:attribute"
								name="t_ddue_attribute">
		<xsl:apply-templates/>
	</xsl:template>

	<xsl:template match="ddue:attribute/ddue:title"
								name="t_ddue_attributeTitle">
		<h4 class="subHeading">
			<xsl:apply-templates/>
		</h4>
	</xsl:template>

	<xsl:template match="ddue:childElement"
								name="t_ddue_childElement">
		<xsl:if test="normalize-space(.)">
			<h4 class="subHeading">
				<include item="title_childElement"/>
			</h4>
			<xsl:apply-templates/>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:parentElement"
								name="t_ddue_parentElement">
		<xsl:if test="normalize-space(.)">
			<h4 class="subHeading">
				<include item="title_parentElement"/>
			</h4>
			<xsl:apply-templates/>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:textValue"
								name="t_ddue_textValue">
		<xsl:if test="normalize-space(.)">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude"
												select="'title_textValue'"/>
				<xsl:with-param name="p_content">
					<xsl:apply-templates/>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:elementInformation"
								name="t_ddue_elementInformation">
		<xsl:if test="normalize-space(.)">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude"
												select="'title_elementInformation'"/>
				<xsl:with-param name="p_content">
					<xsl:apply-templates/>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:dotNetFrameworkEquivalent"
								name="t_ddue_dotNetFrameworkEquivalent">
		<xsl:if test="normalize-space(.)">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude"
												select="'title_dotNetFrameworkEquivalent'"/>
				<xsl:with-param name="p_content">
					<xsl:apply-templates/>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:prerequisites"
								name="t_ddue_prerequisites">
		<xsl:if test="normalize-space(.)">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude"
												select="'title_prerequisites'"/>
				<xsl:with-param name="p_content">
					<xsl:apply-templates/>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:type"
								name="t_ddue_type">
		<xsl:apply-templates/>
	</xsl:template>

	<xsl:template match="ddue:title_robustProgramming"
								name="t_ddue_robustProgramming">
		<xsl:if test="normalize-space(.)">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude"
												select="'title_robustProgramming'"/>
				<xsl:with-param name="p_content">
					<xsl:apply-templates/>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:security"
								name="t_ddue_security">
		<xsl:if test="normalize-space(.)">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude"
												select="'title_securitySection'"/>
				<xsl:with-param name="p_content">
					<xsl:apply-templates/>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:externalResources"
								name="t_ddue_externalResources">
		<xsl:if test="normalize-space(.)">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude"
												select="'title_externalResources'"/>
				<xsl:with-param name="p_content">
					<xsl:apply-templates/>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:demonstrates"
								name="t_ddue_demonstrates">
		<xsl:if test="normalize-space(.)">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude"
												select="'title_demonstrates'"/>
				<xsl:with-param name="p_content">
					<xsl:apply-templates/>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:appliesTo"
								name="t_ddue_appliesTo">
		<xsl:if test="normalize-space(.)">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude"
												select="'title_appliesTo'"/>
				<xsl:with-param name="p_content">
					<xsl:apply-templates/>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:conclusion"
								name="t_ddue_conclusion">
		<xsl:apply-templates/>
	</xsl:template>

	<xsl:template match="ddue:background"
								name="t_ddue_background">
		<xsl:if test="normalize-space(.)">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude"
												select="'title_background'"/>
				<xsl:with-param name="p_content">
					<xsl:apply-templates/>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:whatsNew"
								name="t_ddue_whatsNew">
		<xsl:if test="normalize-space(.)">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude"
												select="'title_whatsNew'"/>
				<xsl:with-param name="p_content">
					<xsl:apply-templates/>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:reference"
								name="t_ddue_reference">
		<xsl:if test="normalize-space(.)">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude"
												select="'title_reference'"/>
				<xsl:with-param name="p_content">
					<xsl:apply-templates/>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:developerErrorMessageDocument"
								name="t_ddue_developerErrorMessageDocument">
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
			<div id="errorTitleSection" class="section">
				<xsl:if test="../ddue:secondaryErrorTitle">
					<h4 class="subHeading">
						<include item="title_errorMessage"/>
					</h4>
					<xsl:apply-templates select="../ddue:secondaryErrorTitle">
						<xsl:with-param name="newSection">no</xsl:with-param>
					</xsl:apply-templates>
				</xsl:if>
				<xsl:apply-templates/>
				<p><xsl:text> </xsl:text></p>
			</div>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:secondaryErrorTitle" name="t_ddue_secondaryErrorTitle">
		<xsl:param name="newSection"/>
		<xsl:if test="string-length(../ddue:secondaryErrorTitle[normalize-space(.)]) > 0">
			<xsl:choose>
				<xsl:when test="$newSection = 'yes'">
					<div id="errorTitleSection" class="section">
						<xsl:apply-templates/>
						<p><xsl:text> </xsl:text></p>
					</div>
				</xsl:when>
				<xsl:otherwise>
					<xsl:apply-templates/>
					<br/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:developerSampleDocument"
								name="t_ddue_developerSampleDocument">
		<!-- show the topic intro -->
		<xsl:apply-templates select="ddue:introduction"/>

		<!-- the sample download list section from dsSample -->
		<xsl:if test="ddue:relatedTopics/ddue:sampleRef">
			<include item="{ddue:relatedTopics/ddue:sampleRef/@srcID}"/>
		</xsl:if>

		<!-- then the rest of the topic's content -->
		<xsl:for-each select="*">
			<xsl:choose>
				<!-- introduction was already captured above -->
				<xsl:when test="name() = 'introduction'"/>

				<xsl:otherwise>
					<xsl:apply-templates select="."/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:for-each>

	</xsl:template>

	<xsl:template name="t_threadSafety">
		<xsl:call-template name="t_putSectionInclude">
			<xsl:with-param name="p_titleInclude"
											select="'title_threadSafety'"/>
			<xsl:with-param name="p_content">
				<xsl:choose>
					<xsl:when test="/document/comments/ddue:dduexml/ddue:threadSafety">
						<xsl:apply-templates select="/document/comments/ddue:dduexml/ddue:threadSafety"/>
					</xsl:when>
					<xsl:otherwise>
						<include item="boilerplate_threadSafety"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<!-- ============================================================================================
	Lists and Tables
	============================================================================================= -->

	<xsl:template match="ddue:list" name="t_ddue_list">
		<xsl:choose>
			<xsl:when test="@class='bullet'">
				<ul>
					<xsl:apply-templates select="ddue:listItem | ddue:list"/>
				</ul>
			</xsl:when>
			<xsl:when test="@class='ordered'">
				<ol>
					<xsl:if test="@start">
						<xsl:attribute name="start">
							<xsl:value-of select="@start"/>
						</xsl:attribute>
					</xsl:if>
					<xsl:apply-templates select="ddue:listItem | ddue:list"/>
				</ol>
			</xsl:when>
			<xsl:otherwise>
				<ul style="list-style-type:none;">
					<xsl:apply-templates select="ddue:listItem | ddue:list"/>
				</ul>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="ddue:listItem" name="t_ddue_listItem">
		<li>
			<xsl:if test="@address">
				<xsl:attribute name="id">
					<xsl:value-of select="@address"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:apply-templates />
		</li>
	</xsl:template>

	<xsl:template match="ddue:table" name="t_ddue_table">
		<div class="tableSection">
			<xsl:if test="normalize-space(ddue:title)">
				<div class="caption">
					<xsl:value-of select="ddue:title"/>
				</div>
			</xsl:if>
			<table>
				<xsl:apply-templates/>
			</table>
		</div>
	</xsl:template>

	<xsl:template match="ddue:tableHeader" name="t_ddue_tableHeader">
		<xsl:apply-templates/>
	</xsl:template>

	<xsl:template match="ddue:row" name="t_ddue_row">
		<tr>
			<xsl:apply-templates/>
		</tr>
	</xsl:template>

	<xsl:template match="ddue:entry" name="t_ddue_entry">
		<td>
			<xsl:if test="@address">
				<xsl:attribute name="id">
					<xsl:value-of select="@address"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:apply-templates/>
		</td>
	</xsl:template>

	<xsl:template match="ddue:tableHeader/ddue:row/ddue:entry" name="t_ddue_tableHeaderRowEntry">
		<th>
			<xsl:apply-templates/>
		</th>
	</xsl:template>

	<xsl:template match="ddue:definitionTable" name="t_ddue_definitionTable">
		<dl class="authored">
			<xsl:apply-templates/>
		</dl>
	</xsl:template>

	<xsl:template match="ddue:definedTerm" name="t_ddue_definedTerm">
		<dt>
			<xsl:if test="@address">
				<xsl:attribute name="id">
					<xsl:value-of select="@address"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:apply-templates/>
		</dt>
	</xsl:template>

	<xsl:template match="ddue:definition" name="t_ddue_definition">
		<dd>
			<xsl:apply-templates/>
		</dd>
	</xsl:template>

	<!-- ============================================================================================
	Code
	============================================================================================= -->

	<xsl:template match="ddue:snippets" name="t_ddue_snippets">
		<xsl:if test="ddue:codeSnippetGroup">
			<xsl:for-each select="ddue:codeSnippetGroup">
				<xsl:call-template name="t_putCodeSections">
					<xsl:with-param name="p_nodes" select="./ddue:snippet" />
				</xsl:call-template>
			</xsl:for-each>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:codeSnippetGroup" name="t_ddue_code">
		<xsl:call-template name="t_putCodeSections">
			<xsl:with-param name="p_nodes" select="./ddue:code" />
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="ddue:sampleCode" name="t_ddue_sampleCode">
		<div>
			<span class="label">
				<xsl:value-of select="@language"/>
			</span>
		</div>
		<div class="code">
			<pre xml:space="preserve"><xsl:apply-templates/></pre>
		</div>
	</xsl:template>

	<xsl:template match="ddue:codeExamples" name="t_ddue_codeExamples">
		<xsl:if test="normalize-space(.)">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude" select="'title_examples'"/>
				<xsl:with-param name="p_content">
					<xsl:apply-templates/>
					<xsl:call-template name="t_moreCodeSection"/>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<!-- ============================================================================================
  tasks/task nodes are inserted by TaskGrabberComponent which gets content from HowTo topics
	these nodes are handled below in the t_moreCodeSection
	============================================================================================= -->
	<xsl:template match="ddue:codeExamples/ddue:codeExample/ddue:legacy/ddue:content/tasks"/>

	<xsl:template name="t_moreCodeSection">
		<xsl:variable name="v_gotCodeAlready"
									select="boolean(
													(ddue:codeExample/ddue:legacy/ddue:content[ddue:codeReference[ddue:sampleCode] | ddue:code | ddue:snippets//ddue:snippet]) or
													(ddue:codeExample[ddue:codeReference[ddue:sampleCode] | ddue:code | ddue:snippets//ddue:snippet])
													)"/>

		<xsl:variable name="v_gotMoreCode"
									select="(count(ddue:codeExample/ddue:legacy/ddue:content/tasks/task)&gt;1) or 
                           ($v_gotCodeAlready and count(ddue:codeExample/ddue:legacy/ddue:content/tasks/task)&gt;0)"/>

		<!-- if no preceding code in the code examples section, display the tasks[1]/task[1] -->
		<xsl:if test="not($v_gotCodeAlready)">
			<xsl:for-each select="ddue:codeExample/ddue:legacy/ddue:content/tasks[1]/task[1]">
				<xsl:apply-templates select="ddue:introduction | ddue:codeExample"/>
			</xsl:for-each>
		</xsl:if>

		<xsl:if test="$v_gotMoreCode">
			<sections>
				<h4 class="subHeading">
					<include item="mrefTaskMoreCodeHeading"/>
				</h4>
				<div class="subsection">
					<div class="tableSection">
						<table>
							<xsl:for-each select="ddue:codeExample/ddue:legacy/ddue:content/tasks/task">
								<xsl:choose>
									<xsl:when test="not($v_gotCodeAlready) and position()=1"/>
									<xsl:otherwise>
										<tr valign="top">
											<td>
												<conceptualLink target="{@topicId}">
													<xsl:value-of select="ddue:title"/>
												</conceptualLink>
											</td>
											<td>
												<xsl:choose>
													<xsl:when test="ddue:introduction/ddue:para[1][normalize-space(.)!='']">
														<xsl:apply-templates select="ddue:introduction/ddue:para[1]/node()"/>
													</xsl:when>
													<xsl:when test="ddue:codeExample/ddue:legacy/ddue:content/ddue:para[1][normalize-space(.)!='']">
														<xsl:apply-templates select="ddue:codeExample/ddue:legacy/ddue:content/ddue:para[1]/node()"/>
													</xsl:when>
												</xsl:choose>
											</td>
										</tr>
									</xsl:otherwise>
								</xsl:choose>
							</xsl:for-each>
						</table>
					</div>
				</div>
			</sections>
		</xsl:if>
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
		<div>
			<xsl:choose>
				<xsl:when test="ddue:image[@placement='center']">
					<xsl:attribute name="class">ps_mediaCenter</xsl:attribute>
				</xsl:when>
				<xsl:when test="ddue:image[@placement='far']">
					<xsl:attribute name="class">ps_mediaFar</xsl:attribute>
				</xsl:when>
				<xsl:otherwise>
					<xsl:attribute name="class">ps_mediaNear</xsl:attribute>
				</xsl:otherwise>
			</xsl:choose>
			<xsl:if test="ddue:caption and not(ddue:caption[@placement='after'])">
				<div class="caption">
					<xsl:if test="ddue:caption[@lead]">
						<span class="ps_captionLead">
							<xsl:value-of select="normalize-space(ddue:caption/@lead)"/>:
						</span>
					</xsl:if>
					<xsl:apply-templates select="ddue:caption"/>
				</div>
			</xsl:if>
			<artLink target="{ddue:image/@xlink:href}"/>
			<xsl:if test="ddue:caption and ddue:caption[@placement='after']">
				<div class="caption">
					<xsl:if test="ddue:caption[@lead]">
						<span class="ps_captionLead">
							<xsl:value-of select="normalize-space(ddue:caption/@lead)"/>:
						</span>
					</xsl:if>
					<xsl:apply-templates select="ddue:caption"/>
				</div>
			</xsl:if>
		</div>
	</xsl:template>

	<xsl:template match="ddue:mediaLinkInline" name="t_ddue_mediaLinkInline">
		<span class="media">
			<artLink target="{ddue:image/@xlink:href}"/>
		</span>
	</xsl:template>

	<!-- ============================================================================================
	Inline elements
	============================================================================================= -->

	<xsl:template match="ddue:span" name="t_ddue_span">
		<xsl:choose>
			<!-- fix bug 361746 - use copy-of, so that span class="keyword", "literal" and "comment" 
           nodes are copied to preserve code colorization in snippets -->
			<xsl:when test="@class='keyword' or @class='identifier' or @class='literal' or @class='parameter' or @class='typeparameter' or @class='comment'">
				<xsl:copy-of select="."/>
			</xsl:when>
			<!-- If the class is unrecognized skip it -->
		</xsl:choose>
	</xsl:template>

	<xsl:template match="ddue:languageKeyword" name="t_ddue_languageKeyword">
		<xsl:variable name="v_keyword" select="."/>
		<xsl:variable name="v_syntaxKeyword">
			<xsl:if test="/document/syntax">
				<xsl:value-of select="'true'"/>
			</xsl:if>
		</xsl:variable>
		<xsl:choose>
			<xsl:when test="$v_keyword='null' or $v_keyword='Nothing' or $v_keyword='nullptr'">
				<xsl:call-template name="t_nullKeyword">
					<xsl:with-param name="p_syntaxKeyword" select="$v_syntaxKeyword"/>
				</xsl:call-template>
			</xsl:when>
			<xsl:when test="$v_keyword='static' or $v_keyword='Shared'">
				<xsl:call-template name="t_staticKeyword">
					<xsl:with-param name="p_syntaxKeyword" select="$v_syntaxKeyword"/>
				</xsl:call-template>
			</xsl:when>
			<xsl:when test="$v_keyword='virtual' or $v_keyword='Overridable'">
				<xsl:call-template name="t_virtualKeyword">
					<xsl:with-param name="p_syntaxKeyword" select="$v_syntaxKeyword"/>
				</xsl:call-template>
			</xsl:when>
			<xsl:when test="$v_keyword='true' or $v_keyword='True'">
				<xsl:call-template name="t_trueKeyword">
					<xsl:with-param name="p_syntaxKeyword" select="$v_syntaxKeyword"/>
				</xsl:call-template>
			</xsl:when>
			<xsl:when test="$v_keyword='false' or $v_keyword='False'">
				<xsl:call-template name="t_falseKeyword">
					<xsl:with-param name="p_syntaxKeyword" select="$v_syntaxKeyword"/>
				</xsl:call-template>
			</xsl:when>
			<xsl:when test="$v_keyword='abstract' or $v_keyword='MustInherit'">
				<xsl:call-template name="t_abstractKeyword">
					<xsl:with-param name="p_syntaxKeyword" select="$v_syntaxKeyword"/>
				</xsl:call-template>
			</xsl:when>
			<xsl:when test="$v_keyword='sealed' or $v_keyword='NotInheritable'">
				<xsl:call-template name="t_sealedKeyword">
					<xsl:with-param name="p_syntaxKeyword" select="$v_syntaxKeyword"/>
				</xsl:call-template>
			</xsl:when>
			<xsl:when test="$v_keyword='async' or $v_keyword='Async'">
				<xsl:call-template name="t_asyncKeyword">
					<xsl:with-param name="p_syntaxKeyword" select="$v_syntaxKeyword"/>
				</xsl:call-template>
			</xsl:when>
			<xsl:when test="$v_keyword='await' or $v_keyword='Await'">
				<xsl:call-template name="t_awaitKeyword">
					<xsl:with-param name="p_syntaxKeyword" select="$v_syntaxKeyword"/>
				</xsl:call-template>
			</xsl:when>
			<xsl:when test="$v_keyword='async/await' or $v_keyword='Async/Await'">
				<xsl:call-template name="t_asyncAwaitKeyword">
					<xsl:with-param name="p_syntaxKeyword" select="$v_syntaxKeyword"/>
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<span class="code">
					<xsl:value-of select="."/>
				</span>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<!-- ======================================================================================== -->

	<xsl:template match="ddue:application"
								name="t_ddue_application">
		<xsl:if test="normalize-space(.)">
			<strong>
				<xsl:apply-templates/>
			</strong>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:codeFeaturedElement"
								name="t_ddue_codeFeaturedElement">
		<xsl:if test="normalize-space(.)">
			<span class="label">
				<xsl:apply-templates/>
			</span>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:codeInline"
								name="t_ddue_codeInline">
		<xsl:if test="normalize-space(.)">
			<span class="code">
				<xsl:value-of select="." />
			</span>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:command"
								name="t_ddue_command">
		<xsl:if test="normalize-space(.)">
			<span class="command">
				<xsl:apply-templates />
			</span>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:computerOutputInline"
								name="t_ddue_computerOutputInline">
		<xsl:call-template name="t_ddue_codeInline"/>
	</xsl:template>

	<xsl:template match="ddue:corporation"
								name="t_ddue_corporation">
		<xsl:apply-templates/>
	</xsl:template>

	<xsl:template match="ddue:country"
								name="t_ddue_country">
		<xsl:apply-templates/>
	</xsl:template>

	<xsl:template match="ddue:database"
								name="t_ddue_database">
		<xsl:if test="normalize-space(.)">
			<strong>
				<xsl:apply-templates/>
			</strong>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:date"
								name="t_ddue_date">
		<xsl:apply-templates/>
	</xsl:template>

	<xsl:template match="ddue:embeddedLabel"
								name="t_ddue_embeddedLabel">
		<xsl:if test="normalize-space(.)">
			<span class="label">
				<xsl:apply-templates/>
			</span>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:environmentVariable"
								name="t_ddue_environmentVariable">
		<xsl:call-template name="t_ddue_codeInline"/>
	</xsl:template>

	<xsl:template match="ddue:errorInline"
								name="t_ddue_errorInline">
		<xsl:if test="normalize-space(.)">
			<em>
				<xsl:apply-templates/>
			</em>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:hardware"
								name="t_ddue_hardware">
		<xsl:if test="normalize-space(.)">
			<strong>
				<xsl:apply-templates/>
			</strong>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:fictitiousUri"
								name="t_ddue_fictitiousUri">
		<xsl:call-template name="t_ddue_localUri"/>
	</xsl:template>

	<xsl:template match="ddue:foreignPhrase"
								name="t_ddue_foreignPhrase">
		<xsl:if test="normalize-space(.)">
			<span class="foreignPhrase">
				<xsl:apply-templates/>
			</span>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:legacyBold"
								name="t_ddue_legacyBold">
		<xsl:if test="normalize-space(.)">
			<strong>
				<xsl:apply-templates />
			</strong>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:legacyItalic" name="t_ddue_legacyItalic">
		<xsl:if test="normalize-space(.)">
			<em>
				<xsl:apply-templates />
			</em>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:legacyUnderline"
								name="t_ddue_legacyUnderline">
		<xsl:if test="normalize-space(.)">
			<u>
				<xsl:apply-templates />
			</u>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:literal"
								name="t_ddue_literal">
		<xsl:if test="normalize-space(.)">
			<span class="literal">
				<xsl:apply-templates/>
			</span>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:localizedText"
								name="t_ddue_localizedText">
		<xsl:apply-templates/>
	</xsl:template>

	<xsl:template match="ddue:localUri"
								name="t_ddue_localUri">
		<xsl:if test="normalize-space(.)">
			<em>
				<xsl:apply-templates/>
			</em>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:math"
								name="t_ddue_math">
		<xsl:if test="normalize-space(.)">
			<span class="math">
				<xsl:apply-templates/>
			</span>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:newTerm"
								name="t_ddue_newTerm">
		<xsl:if test="normalize-space(.)">
			<span class="term">
				<xsl:apply-templates/>
			</span>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:parameterReference"
								name="t_ddue_parameterReference">
		<xsl:if test="normalize-space(.)">
			<span class="parameter">
				<xsl:apply-templates/>
			</span>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:phrase"
								name="t_ddue_phrase">
		<xsl:if test="normalize-space(.)">
			<span class="phrase">
				<xsl:apply-templates/>
			</span>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:placeholder"
								name="t_ddue_placeholder">
		<xsl:call-template name="t_ddue_replaceable"/>
	</xsl:template>

	<xsl:template match="ddue:quote"
								name="t_ddue_quote">
		<xsl:if test="normalize-space(.)">
			<blockQuote>
				<xsl:apply-templates/>
			</blockQuote>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:quoteInline"
								name="t_ddue_quoteInline">
		<xsl:if test="normalize-space(.)">
			<q>
				<xsl:apply-templates/>
			</q>
	</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:replaceable"
								name="t_ddue_replaceable">
		<xsl:if test="normalize-space(.)">
			<span class="placeholder">
				<xsl:apply-templates/>
			</span>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:subscript|ddue:subscriptType" name="t_ddue_subscript">
		<xsl:if test="normalize-space(.)">
			<sub>
				<xsl:apply-templates/>
			</sub>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:superscript|ddue:superscriptType" name="t_ddue_superscript">
		<xsl:if test="normalize-space(.)">
			<sup>
				<xsl:apply-templates/>
			</sup>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:system"
								name="t_ddue_system">
		<xsl:if test="normalize-space(.)">
			<strong>
				<xsl:apply-templates/>
			</strong>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:ui"
								name="t_ddue_ui">
		<xsl:if test="normalize-space(.)">
			<span class="ui">
				<xsl:apply-templates/>
			</span>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:unmanagedCodeEntityReference"
								name="t_ddue_unmanagedCodeEntityReference">
		<xsl:if test="normalize-space(.)">
			<strong>
				<xsl:apply-templates/>
			</strong>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:userInput"
								name="t_ddue_userInput">
		<xsl:if test="normalize-space(.)">
			<span class="input">
				<xsl:value-of select="." />
			</span>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:userInputLocalizable"
								name="t_ddue_userInputLocalizable">
		<xsl:call-template name="t_ddue_userInput"/>
	</xsl:template>

	<!-- ============================================================================================
	Pass through a chunk of markup.  This differs from the API markup template in that it must strip
	off the "ddue" namespace.  This will allow build components to add HTML elements to a
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
			<xsl:attribute name="target">
				<xsl:choose>
					<xsl:when test="normalize-space(ddue:linkTarget)">
						<xsl:value-of select="normalize-space(ddue:linkTarget)"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:text>_blank</xsl:text>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:attribute>
			<xsl:value-of select="normalize-space(ddue:linkText)"/>
		</a>
	</xsl:template>

	<xsl:template match="ddue:link" name="t_ddue_link">
		<xsl:choose>
			<xsl:when test="starts-with(@xlink:href,'#')">
				<!-- In-page link -->
				<a href="{@xlink:href}">
					<xsl:apply-templates/>
				</a>
			</xsl:when>
			<xsl:otherwise>
				<!-- Verified, external link -->
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

	<xsl:template match="ddue:copyright"
								name="t_ddue_copyright">
		<!-- <p>{0} &copy;{1}{2}. All rights reserved.</p> -->
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
	Change history
	============================================================================================= -->

	<!-- Don't render the changeHistory section here; it's handled in the t_writeChangeHistorySection template. -->
	<xsl:template match="ddue:changeHistory"
								name="t_ddue_changeHistory"/>

	<!-- Display a date to show when the topic was last updated. -->
	<xsl:template name="t_writeFreshnessDate">
		<!-- The $p_changedHistoryDate param is from the authored changeHistory table, if any. -->
		<xsl:param name="p_changedHistoryDate"/>
		<!-- Determine whether the authored date is a valid date string.  -->
		<xsl:variable name="v_validChangeHistoryDate">
			<xsl:choose>
				<xsl:when test="normalize-space($p_changedHistoryDate)=''"/>
				<xsl:when test="ddue:IsValidDate(normalize-space($p_changedHistoryDate)) = 'true'">
					<xsl:value-of select="normalize-space($p_changedHistoryDate)"/>
				</xsl:when>
			</xsl:choose>
		</xsl:variable>
		<xsl:choose>
			<!-- display nothing if the 'changeHistoryOptions' argument is set to 'omit' -->
			<xsl:when test="$changeHistoryOptions = 'omit'"/>

			<!-- if it's a valid date, display the freshness line. -->
			<xsl:when test="normalize-space($v_validChangeHistoryDate)">
				<p>
					<include item="boilerplate_UpdateTitle">
						<parameter>
							<xsl:value-of select="normalize-space($v_validChangeHistoryDate)"/>
						</parameter>
					</include>
				</p>
			</xsl:when>

			<!-- use a default date if no p_changedHistoryDate and the 'changeHistoryOptions' argument is set to 'showDefaultFreshnessDate' -->
			<xsl:when test="$changeHistoryOptions = 'showDefaultFreshnessDate'">
				<p>
					<include item="boilerplate_UpdateTitle">
						<parameter>
							<include item="text_defaultFreshnessDate"/>
						</parameter>
					</include>
				</p>
			</xsl:when>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="t_writeChangeHistorySection">
		<xsl:if test="$changeHistoryOptions!='omit'">
			<!-- conceptual authored content is in /document/topic/*; mref content is in /document/comments/ddue:dduexml. -->
			<xsl:for-each select="/document/comments/ddue:dduexml | /document/topic/*">
				<!-- Get the change history section content, which can be in changeHistory or a section with title='Change History'. -->
				<xsl:variable name="v_changeHistoryContent">
					<xsl:choose>
						<xsl:when test="ddue:changeHistory/ddue:content/ddue:table/ddue:row/ddue:entry[normalize-space(.)]">
							<xsl:apply-templates select="ddue:changeHistory/ddue:content"/>
						</xsl:when>
						<xsl:when test=".//ddue:section[ddue:title = 'Change History']/ddue:content/ddue:table/ddue:row/ddue:entry[normalize-space(.)]">
							<xsl:apply-templates select=".//ddue:section[ddue:title = 'Change History']/ddue:content"/>
						</xsl:when>
					</xsl:choose>
				</xsl:variable>
				<xsl:if test="normalize-space($v_changeHistoryContent)">
					<xsl:call-template name="t_putSectionInclude">
						<xsl:with-param name="p_titleInclude"
														select="'title_changeHistory'"/>
						<xsl:with-param name="p_content">
							<xsl:copy-of select="$v_changeHistoryContent"/>
						</xsl:with-param>
					</xsl:call-template>
				</xsl:if>
			</xsl:for-each>
		</xsl:if>
	</xsl:template>

	<!-- ============================================================================================
	Glossary
	============================================================================================= -->

	<xsl:key name="k_glossaryTermFirstLetters"
					 match="//ddue:glossaryEntry"
					 use="translate(substring(ddue:terms/ddue:term/text(),1,1),'abcdefghijklmnopqrstuvwxyz','ABCDEFGHIJKLMNOPQRSTUVWXYZ ')"/>

	<xsl:template match="ddue:glossary"
								name="t_ddue_glossary">
		<xsl:if test="ddue:title">
			<h1 class="ps_glossaryTitle">
				<xsl:value-of select="normalize-space(ddue:title)" />
			</h1>
		</xsl:if>
		<xsl:choose>
			<xsl:when test="ddue:glossaryDiv">
				<!-- Organized glossary with glossaryDiv elements -->
				<br/>
				<xsl:for-each select="ddue:glossaryDiv">
					<xsl:if test="ddue:title">
						<xsl:choose>
							<xsl:when test="@address">
								<a href="#{@address}">
									<xsl:value-of select="ddue:title" />
								</a>
							</xsl:when>
							<xsl:otherwise>
								<xsl:value-of select="ddue:title" />
							</xsl:otherwise>
						</xsl:choose>
					</xsl:if>
					<xsl:if test="position() != last()">
						<xsl:text> | </xsl:text>
					</xsl:if>
				</xsl:for-each>

				<xsl:apply-templates select="ddue:glossaryDiv"/>
			</xsl:when>
			<xsl:otherwise>
				<!-- Simple glossary consisting of nothing by glossaryEntry elements -->
				<br/>
				<xsl:call-template name="t_glossaryLetterBar"/>
				<br/>
				<xsl:call-template name="t_glossaryGroupByEntriesTermFirstLetter"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="ddue:glossaryDiv" name="t_ddue_glossaryDiv">
		<div class="ps_glossaryDiv">
			<xsl:if test="@address">
				<xsl:attribute name="id">
					<xsl:value-of select="@address"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="ddue:title">
				<h2 class="ps_glossaryDivHeading">
					<xsl:value-of select="ddue:title"/>
				</h2>
			</xsl:if>
			<hr class="ps_glossaryRule"/>
			<xsl:call-template name="t_glossaryLetterBar">
				<xsl:with-param name="p_sectionPrefix" select="generate-id()"/>
			</xsl:call-template>
			<br/>
			<xsl:call-template name="t_glossaryGroupByEntriesTermFirstLetter">
				<xsl:with-param name="p_sectionPrefix" select="generate-id()"/>
			</xsl:call-template>
		</div>
	</xsl:template>

	<xsl:template name="t_glossaryGroupByEntriesTermFirstLetter">
		<xsl:param name="p_sectionPrefix"
							 select="''"/>
		<xsl:variable name="v_div"
									select="."/>
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
				<xsl:with-param name="p_link"
												select="concat($p_sectionPrefix,$v_letter)"/>
				<xsl:with-param name="p_name"
												select="$v_letter"/>
				<xsl:with-param name="p_nodes"
												select="key('k_glossaryTermFirstLetters',
                        translate($v_letter,$g_allLowerCaseLetters,concat($g_allUpperCaseLetters,' ')))
                        [parent::node() = $v_div]"/>
			</xsl:call-template>
		</xsl:for-each>
	</xsl:template>

	<xsl:template match="ddue:glossaryEntry" name="t_ddue_glossaryEntry">
		<dt class="ps_glossaryEntry">
			<xsl:if test="@address">
				<xsl:attribute name="id">
					<xsl:value-of select="@address"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:for-each select="ddue:terms/ddue:term">
				<xsl:sort select="normalize-space(.)" />

				<xsl:choose>
					<xsl:when test="@termId">
						<span id="{@termId}">
							<xsl:value-of select="normalize-space(.)" />
						</span>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="normalize-space(.)" />
					</xsl:otherwise>
				</xsl:choose>

				<xsl:if test="position() != last()">
					<xsl:text>, </xsl:text>
				</xsl:if>
			</xsl:for-each>
		</dt>
		<dd class="ps_glossaryEntry">
			<xsl:apply-templates select="ddue:definition/*"/>

			<xsl:if test="ddue:relatedEntry">
				<div class="ps_relatedEntry">
					<include item="text_relatedEntries" />&#160;

					<xsl:for-each select="ddue:relatedEntry">
						<xsl:variable name="id" select="@termId" />
						<a href="#{@termId}">
							<xsl:value-of select="//ddue:term[@termId=$id]"/>
						</a>
						<xsl:if test="position() != last()">
							<xsl:text>, </xsl:text>
						</xsl:if>
					</xsl:for-each>
				</div>
			</xsl:if>
		</dd>
	</xsl:template>

	<xsl:template name="t_glossaryEntryGroup">
		<xsl:param name="p_link"/>
		<xsl:param name="p_name"/>
		<xsl:param name="p_nodes"/>
		<div class="ps_glossaryGroup">
			<h3 class="ps_glossaryGroupHeading">
				<xsl:attribute name="id">
					<xsl:value-of select="$p_link"/>
				</xsl:attribute>
				<xsl:value-of select="$p_name"/>
			</h3>
			<dl class="ps_glossaryGroupList">
				<xsl:apply-templates select="$p_nodes">
					<xsl:sort select="ddue:terms/ddue:term"/>
				</xsl:apply-templates>
			</dl>
		</div>
	</xsl:template>

	<xsl:template name="t_glossaryLetterBar">
		<xsl:param name="p_sectionPrefix" select="''"/>
		<div class="ps_glossaryLetterBar">
			<xsl:call-template name="t_glossaryLetterBarLinkRecursive">
				<xsl:with-param name="p_sectionPrefix" select="$p_sectionPrefix"/>
				<xsl:with-param name="p_bar" select="$g_allUpperCaseLetters"/>
				<xsl:with-param name="p_characterPosition" select="1"/>
			</xsl:call-template>
		</div>
	</xsl:template>

	<xsl:template name="t_glossaryLetterBarLinkRecursive">
		<xsl:param name="p_sectionPrefix"/>
		<xsl:param name="p_bar"/>
		<xsl:param name="p_characterPosition"/>
		<xsl:variable name="v_letter"
									select="substring($p_bar,$p_characterPosition,1)"/>
		<xsl:if test="$v_letter">
			<xsl:choose>
				<xsl:when test="ddue:glossaryEntry[ddue:terms/ddue:term[1]
                  [translate(substring(text(),1,1),$g_allLowerCaseLetters,concat($g_allUpperCaseLetters,' ')) = $v_letter]]">
					<xsl:call-template name="t_glossaryLetterBarLink">
						<xsl:with-param name="p_link"
														select="concat($p_sectionPrefix,$v_letter)"/>
						<xsl:with-param name="p_name"
														select="$v_letter"/>
					</xsl:call-template>
					<xsl:if test="not($p_characterPosition = string-length($p_bar))">
						<xsl:text> | </xsl:text>
					</xsl:if>
				</xsl:when>
				<xsl:otherwise>
					<xsl:call-template name="t_glossaryLetterBarLink">
						<xsl:with-param name="p_name"
														select="$v_letter"/>
					</xsl:call-template>
					<xsl:if test="not($p_characterPosition = string-length($p_bar))">
						<xsl:text> | </xsl:text>
					</xsl:if>
				</xsl:otherwise>
			</xsl:choose>
			<xsl:call-template name="t_glossaryLetterBarLinkRecursive">
				<xsl:with-param name="p_sectionPrefix"
												select="$p_sectionPrefix"/>
				<xsl:with-param name="p_bar"
												select="$p_bar"/>
				<xsl:with-param name="p_characterPosition"
												select="$p_characterPosition + 1"/>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template name="t_glossaryLetterBarLink">
		<xsl:param name="p_link"/>
		<xsl:param name="p_name"/>
		<xsl:choose>
			<xsl:when test="$p_link">
				<a href="#{$p_link}">
					<xsl:value-of select="$p_name"/>
				</a>
			</xsl:when>
			<xsl:otherwise>
				<span class="nolink">
					<xsl:value-of select="$p_name"/>
				</span>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

</xsl:stylesheet>
