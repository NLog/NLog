<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="2.0"
								xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
								xmlns:msxsl="urn:schemas-microsoft-com:xslt"
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

	<xsl:import href="GlobalTemplates.xsl" />
	<xsl:import href="CodeTemplates.xsl" />
	<xsl:import href="ReferenceUtilities.xsl" />
	<xsl:import href="HTMLTemplates.xsl" />

	<xsl:output method="xml" omit-xml-declaration="yes" indent="no" encoding="utf-8"/>

	<!-- ============================================================================================
	Parameters
	============================================================================================= -->

	<xsl:param name="omitXmlnsBoilerplate" select="'false'"/>
	<xsl:param name="omitVersionInformation" select="'false'"/>

	<!-- ============================================================================================
	Global Variables
	============================================================================================= -->

	<xsl:variable name="g_abstractSummary" select="/document/comments/summary"/>
	<xsl:variable name="g_hasSeeAlsoSection"
		select="boolean((count(/document/comments//seealso[not(ancestor::overloads)] |
		/document/comments/conceptualLink |
		/document/reference/elements/element/overloads//seealso) > 0)  or 
    ($g_apiTopicGroup='type' or $g_apiTopicGroup='member' or $g_apiTopicGroup='list'))"/>

	<!-- ============================================================================================
	Document
	============================================================================================= -->

	<xsl:template match="/">
		<w:document mc:Ignorable="w14 wp14">
			<!-- This is used by the Save Component to get the filename.  It won't end up in the final result. -->
			<file>
				<xsl:attribute name="name">
					<xsl:value-of select="/document/reference/file/@name" />
				</xsl:attribute>
			</file>
			<w:body>
				<w:p>
					<w:pPr>
						<w:pStyle w:val="Heading1" />
					</w:pPr>
					<!-- The Open XML file builder task will reformat the bookmark name and ID to ensure that they are unique -->
					<w:bookmarkStart w:name="_Topic" w:id="0" />
					<w:bookmarkEnd w:id="0" />
					<include item="boilerplate_pageTitle">
						<parameter>
							<xsl:call-template name="t_topicTitleDecorated"/>
						</parameter>
					</include>
				</w:p>
				<xsl:call-template name="t_body"/>
			</w:body>
		</w:document>
	</xsl:template>


	<!-- ============================================================================================
	Body
	============================================================================================= -->

	<xsl:template name="t_body">

		<!-- Auto-inserted info -->
		<xsl:apply-templates select="/document/comments/preliminary"/>

		<xsl:if test="/document/reference/attributes/attribute/type[@api='T:System.ObsoleteAttribute']">
			<w:p>
				<include item="boilerplate_obsoleteLong"/>
			</w:p>
		</xsl:if>


		<xsl:apply-templates select="/document/comments/summary"/>
		<xsl:if test="$g_apiTopicSubGroup='overload'">
			<xsl:apply-templates select="/document/reference/elements" mode="overloadSummary"/>
		</xsl:if>

		<!-- Inheritance -->
		<xsl:apply-templates select="/document/reference/family"/>

		<!-- Assembly information -->
		<xsl:if test="not($g_apiTopicGroup='list' or $g_apiTopicGroup='root' or $g_apiTopicGroup='namespace' or $g_apiTopicGroup='namespaceGroup')">
			<xsl:call-template name="t_putRequirementsInfo"/>
		</xsl:if>

		<!-- Syntax -->
		<xsl:if test="not($g_apiTopicGroup='list' or $g_apiTopicGroup='root' or $g_apiTopicGroup='namespace' or $g_apiTopicGroup='namespaceGroup')">
			<xsl:apply-templates select="/document/syntax"/>
		</xsl:if>

		<!-- Members -->
		<xsl:choose>
			<xsl:when test="$g_apiTopicGroup='root'">
				<xsl:apply-templates select="/document/reference/elements" mode="root"/>
			</xsl:when>
			<xsl:when test="$g_apiTopicGroup='namespace'">
				<xsl:apply-templates select="/document/reference/elements" mode="namespace"/>
			</xsl:when>
			<xsl:when test="$g_apiTopicGroup='namespaceGroup'">
				<xsl:apply-templates select="/document/reference/elements" mode="namespaceGroup" />
			</xsl:when>
			<xsl:when test="$g_apiTopicSubGroup='enumeration'">
				<xsl:apply-templates select="/document/reference/elements" mode="enumeration"/>
			</xsl:when>
			<xsl:when test="$g_apiTopicGroup='type'">
				<xsl:apply-templates select="/document/reference/elements" mode="type"/>
			</xsl:when>
			<xsl:when test="$g_apiTopicGroup='list'">
				<xsl:choose>
					<xsl:when test="$g_apiTopicSubGroup='overload'">
						<xsl:apply-templates select="/document/reference/elements" mode="overload"/>
					</xsl:when>
					<xsl:when test="$g_apiTopicSubGroup='DerivedTypeList'">
						<xsl:apply-templates select="/document/reference/elements" mode="derivedType"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:apply-templates select="/document/reference/elements" mode="member"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:when>
		</xsl:choose>

		<!-- Events -->
		<xsl:call-template name="t_events"/>
		<!-- Exceptions -->
		<xsl:call-template name="t_exceptions"/>
		<!-- Remarks -->
		<xsl:apply-templates select="/document/comments/remarks"/>
		<!-- Examples -->
		<xsl:apply-templates select="/document/comments/example"/>

		<!-- Contracts -->
		<xsl:call-template name="t_contracts"/>
		<!-- Versions -->
		<xsl:if test="not($g_apiTopicGroup='list' or $g_apiTopicGroup='root' or $g_apiTopicGroup='namespace' or $g_apiTopicGroup='namespaceGroup')">
			<xsl:apply-templates select="/document/reference/versions"/>
		</xsl:if>
		<!-- Permissions -->
		<xsl:call-template name="t_permissions"/>
		<!-- Thread safety -->
		<xsl:apply-templates select="/document/comments/threadsafety"/>

		<!-- See also -->
		<xsl:call-template name="t_putSeeAlsoSection"/>

	</xsl:template>

	<!-- ============================================================================================
	Inline tags
	============================================================================================= -->

	<xsl:template match="para" name="t_para">
		<w:p>
			<xsl:apply-templates/>
		</w:p>
	</xsl:template>

	<xsl:template match="c" name="t_codeInline">
		<span class="CodeInline">
			<xsl:apply-templates/>
		</span>
	</xsl:template>

	<xsl:template match="preliminary" name="t_preliminary">
		<w:p>
			<w:pPr>
				<w:pStyle w:val="Emphasis" />
			</w:pPr>
			<xsl:choose>
				<xsl:when test="normalize-space(.)">
					<xsl:apply-templates/>
				</xsl:when>
				<xsl:otherwise>
					<include item="preliminaryText" />
				</xsl:otherwise>
			</xsl:choose>
		</w:p>
	</xsl:template>

	<xsl:template match="paramref" name="t_paramref">
		<span class="Parameter">
			<xsl:value-of select="@name"/>
		</span>
	</xsl:template>

	<xsl:template match="typeparamref" name="t_typeparamref">
		<span class="TypeParameter">
			<xsl:value-of select="@name"/>
		</span>
	</xsl:template>

	<!-- ============================================================================================
	Block sections
	============================================================================================= -->

	<xsl:template match="summary" name="t_summary">
		<xsl:apply-templates/>
	</xsl:template>

	<xsl:template match="value" name="t_value">
		<xsl:call-template name="t_putSubSection">
			<xsl:with-param name="p_title">
				<xsl:choose>
					<xsl:when test="/document/reference/apidata[@subgroup='property']">
						<include item="title_propertyValue" />
					</xsl:when>
					<xsl:otherwise>
						<include item="title_fieldValue"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:with-param>

			<xsl:with-param name="p_content">
				<include item="typeLink">
					<parameter>
						<xsl:apply-templates select="/document/reference/returns[1]" mode="link">
							<xsl:with-param name="qualified" select="true()" />
						</xsl:apply-templates>
					</parameter>
				</include>
				<br />
				<xsl:apply-templates/>
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="returns" name="t_returns">
		<xsl:call-template name="t_putSubSection">
			<xsl:with-param name="p_title">
				<include item="title_methodValue"/>
			</xsl:with-param>
			<xsl:with-param name="p_content">
				<include item="typeLink">
					<parameter>
						<xsl:apply-templates select="/document/reference/returns[1]" mode="link">
							<xsl:with-param name="qualified" select="true()" />
						</xsl:apply-templates>
					</parameter>
				</include>
				<br />
				<xsl:apply-templates/>
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="remarks" name="t_remarks">
		<xsl:call-template name="t_putSectionInclude">
			<xsl:with-param name="p_titleInclude" select="'title_remarks'"/>
			<xsl:with-param name="p_content">
				<xsl:apply-templates/>
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="example" name="t_example">
		<xsl:call-template name="t_putSectionInclude">
			<xsl:with-param name="p_titleInclude" select="'title_examples'"/>
			<xsl:with-param name="p_content">
				<xsl:apply-templates/>
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="code" name="t_code">
		<xsl:variable name="v_codeLang">
			<xsl:call-template name="t_codeLang">
				<xsl:with-param name="p_codeLang" select="@language" />
			</xsl:call-template>
		</xsl:variable>

		<xsl:call-template name="t_putCodeSection">
			<xsl:with-param name="p_codeLang" select="$v_codeLang" />
		</xsl:call-template>
	</xsl:template>

	<!-- ======================================================================================== -->

	<xsl:template match="syntax" name="t_syntax">
		<xsl:if test="count(*) > 0">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude" select="'title_syntax'"/>
				<xsl:with-param name="p_content">
					<xsl:call-template name="t_putCodeSections">
						<xsl:with-param name="p_codeNodes" select="./div[@codeLanguage]"/>
						<xsl:with-param name="p_nodeCount" select="count(./div[@codeLanguage])"/>
						<xsl:with-param name="p_codeLangAttr" select="'codeLanguage'"/>
					</xsl:call-template>

					<!-- Parameters & return value -->
					<xsl:apply-templates select="/document/reference/parameters"/>
					<xsl:apply-templates select="/document/reference/templates"/>
					<xsl:choose>
						<xsl:when test="/document/comments/value | /document/comments/returns">
							<xsl:apply-templates select="/document/comments/value" />
							<xsl:apply-templates select="/document/comments/returns" />
						</xsl:when>
						<xsl:otherwise>
							<xsl:if test="/document/reference/returns[1] | /document/reference/eventhandler/type">
								<xsl:call-template name="defaultReturnSection" />
							</xsl:if>
						</xsl:otherwise>
					</xsl:choose>

					<xsl:apply-templates select="/document/reference/implements"/>

					<!-- Usage note for extension methods -->
					<xsl:if test="/document/reference/attributes/attribute/type[@api='T:System.Runtime.CompilerServices.ExtensionAttribute'] and boolean($g_apiSubGroup='method')">
						<xsl:call-template name="t_putSubSection">
							<xsl:with-param name="p_title">
								<include item="title_extensionUsage"/>
							</xsl:with-param>
							<xsl:with-param name="p_content">
								<include item="text_extensionUsage">
									<parameter>
										<xsl:apply-templates select="/document/reference/parameters/parameter[1]/type" mode="link"/>
									</parameter>
								</include>
							</xsl:with-param>
						</xsl:call-template>
					</xsl:if>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template name="defaultReturnSection">
		<xsl:call-template name="t_putSubSection">
			<xsl:with-param name="p_title">
				<xsl:choose>
					<xsl:when test="/document/reference/apidata[@subgroup='property']">
						<include item="title_propertyValue" />
					</xsl:when>
					<xsl:when test="/document/reference/apidata[@subgroup='field']">
						<include item="title_fieldValue" />
					</xsl:when>
					<xsl:when test="/document/reference/apidata[@subgroup='event']">
						<include item="title_value" />
					</xsl:when>
					<xsl:otherwise>
						<include item="title_methodValue" />
					</xsl:otherwise>
				</xsl:choose>
			</xsl:with-param>
			<xsl:with-param name="p_content">
				<include item="typeLink">
					<parameter>
						<xsl:choose>
							<xsl:when test="/document/reference/attributes/attribute/type[@api='T:System.Runtime.CompilerServices.FixedBufferAttribute']">
								<xsl:apply-templates select="/document/reference/attributes/attribute/type[@api='T:System.Runtime.CompilerServices.FixedBufferAttribute']/../argument/typeValue/type" mode="link">
									<xsl:with-param name="qualified" select="true()" />
								</xsl:apply-templates>
							</xsl:when>
							<xsl:when test="/document/reference/apidata[@subgroup='event']">
								<xsl:apply-templates select="/document/reference/eventhandler/type" mode="link">
									<xsl:with-param name="qualified" select="true()" />
								</xsl:apply-templates>
							</xsl:when>
							<xsl:otherwise>
								<xsl:apply-templates select="/document/reference/returns[1]" mode="link">
									<xsl:with-param name="qualified" select="true()" />
								</xsl:apply-templates>
							</xsl:otherwise>
						</xsl:choose>
					</parameter>
				</include>
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<!-- ======================================================================================== -->

	<xsl:template match="overloads" mode="summary" name="t_overloadsSummary">
		<xsl:choose>
			<xsl:when test="count(summary) > 0">
				<xsl:apply-templates select="summary"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:apply-templates/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="overloads" mode="sections" name="t_overloadsSections">
		<xsl:apply-templates select="remarks"/>
		<xsl:apply-templates select="example"/>
	</xsl:template>

	<xsl:template match="templates" name="t_templates">
		<xsl:call-template name="t_putSubSection">
			<xsl:with-param name="p_title">
				<include item="title_templates"/>
			</xsl:with-param>
			<xsl:with-param name="p_content">
				<xsl:for-each select="template">
					<xsl:variable name="templateName" select="@name"/>
					<w:p>
						<w:pPr>
							<w:spacing w:after="0" />
						</w:pPr>
						<span class="Parameter">
							<xsl:value-of select="$templateName"/>
						</span>
					</w:p>
					<xsl:apply-templates select="/document/comments/typeparam[@name=$templateName]"/>
				</xsl:for-each>
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<!-- ======================================================================================== -->

	<xsl:template name="t_events">
		<xsl:if test="count(/document/comments/event) &gt; 0">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude" select="'title_events'"/>
				<xsl:with-param name="p_content">
					<w:tbl>
						<w:tblPr>
							<w:tblStyle w:val="GeneralTable"/>
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
									<include item="header_eventType"/>
								</w:p>
							</w:tc>
							<w:tc>
								<w:p>
									<w:pPr>
										<w:keepNext />
									</w:pPr>
									<include item="header_eventReason"/>
								</w:p>
							</w:tc>
						</w:tr>
						<xsl:for-each select="/document/comments/event">
							<w:tr>
								<w:tc>
									<referenceLink target="{@cref}" qualified="false"/>
								</w:tc>
								<w:tc>
									<xsl:apply-templates select="."/>
								</w:tc>
							</w:tr>
						</xsl:for-each>
					</w:tbl>
					<w:p>
						<w:pPr>
							<w:spacing w:after="0" />
						</w:pPr>
					</w:p>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template name="t_exceptions">
		<xsl:if test="count(/document/comments/exception) &gt; 0">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude" select="'title_exceptions'"/>
				<xsl:with-param name="p_content">
					<w:tbl>
						<w:tblPr>
							<w:tblStyle w:val="GeneralTable"/>
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
									<include item="header_exceptionName"/>
								</w:p>
							</w:tc>
							<w:tc>
								<w:p>
									<w:pPr>
										<w:keepNext />
									</w:pPr>
									<include item="header_exceptionCondition"/>
								</w:p>
							</w:tc>
						</w:tr>
						<xsl:for-each select="/document/comments/exception">
							<w:tr>
								<w:tc>
									<referenceLink target="{@cref}" qualified="false"/>
								</w:tc>
								<w:tc>
									<xsl:apply-templates select="."/>
								</w:tc>
							</w:tr>
						</xsl:for-each>
					</w:tbl>
					<w:p>
						<w:pPr>
							<w:spacing w:after="0" />
						</w:pPr>
					</w:p>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="threadsafety" name="t_threadsafety">
		<xsl:call-template name="t_putSectionInclude">
			<xsl:with-param name="p_titleInclude" select="'title_threadSafety'"/>
			<xsl:with-param name="p_content">
				<xsl:choose>
					<xsl:when test="normalize-space(.)">
						<xsl:apply-templates/>
					</xsl:when>
					<xsl:when test="(not(@instance) and not(@static)) or (@static='true' and @instance='false')">
						<include item="boilerplate_threadSafety" />
					</xsl:when>
					<xsl:otherwise>
						<xsl:if test="@static='true'">
							<include item="text_staticThreadSafe"/>
						</xsl:if>
						<xsl:if test="@static='false'">
							<include item="text_staticNotThreadSafe"/>
						</xsl:if>
						<xsl:if test="@instance='true'">
							<include item="text_instanceThreadSafe"/>
						</xsl:if>
						<xsl:if test="@instance='false'">
							<include item="text_instanceNotThreadSafe"/>
						</xsl:if>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<xsl:template name="t_permissions">
		<xsl:if test="count(/document/comments/permission) &gt; 0">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude" select="'title_permissions'"/>
				<xsl:with-param name="p_content">
					<w:tbl>
						<w:tblPr>
							<w:tblStyle w:val="GeneralTable"/>
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
									<include item="header_permissionName"/>
								</w:p>
							</w:tc>
							<w:tc>
								<w:p>
									<w:pPr>
										<w:keepNext />
									</w:pPr>
									<include item="header_permissionDescription"/>
								</w:p>
							</w:tc>
						</w:tr>
						<xsl:for-each select="/document/comments/permission">
							<w:tr>
								<w:tc>
									<referenceLink target="{@cref}" qualified="false"/>
								</w:tc>
								<w:tc>
									<xsl:apply-templates select="."/>
								</w:tc>
							</w:tr>
						</xsl:for-each>
					</w:tbl>
					<w:p>
						<w:pPr>
							<w:spacing w:after="0" />
						</w:pPr>
					</w:p>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<!-- ======================================================================================== -->

	<xsl:template name="t_contracts">
		<xsl:variable name="v_requires" select="/document/comments/requires"/>
		<xsl:variable name="v_ensures" select="/document/comments/ensures"/>
		<xsl:variable name="v_ensuresOnThrow" select="/document/comments/ensuresOnThrow"/>
		<xsl:variable name="v_invariants" select="/document/comments/invariant"/>
		<xsl:variable name="v_setter" select="/document/comments/setter"/>
		<xsl:variable name="v_getter" select="/document/comments/getter"/>
		<xsl:variable name="v_pure" select="/document/comments/pure"/>
		<xsl:if test="$v_requires or $v_ensures or $v_ensuresOnThrow or $v_invariants or $v_setter or $v_getter or $v_pure">
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude" select="'title_contracts'"/>
				<xsl:with-param name="p_content">
					<!-- Purity -->
					<xsl:if test="$v_pure">
						<include item="contracts_PureMethod" />
					</xsl:if>
					<!-- Contracts -->
					<xsl:if test="$v_getter">
						<xsl:variable name="v_getterRequires"	select="$v_getter/requires"/>
						<xsl:variable name="v_getterEnsures" select="$v_getter/ensures"/>
						<xsl:variable name="v_getterEnsuresOnThrow" select="$v_getter/ensuresOnThrow"/>
						<xsl:call-template name="t_putSubSection">
							<xsl:with-param name="p_title">
								<include item="title_getter"/>
							</xsl:with-param>
							<xsl:with-param name="p_content">
								<xsl:if test="$v_getterRequires">
									<xsl:call-template name="t_contractsTable">
										<xsl:with-param name="p_title">
											<include item="header_requiresName"/>
										</xsl:with-param>
										<xsl:with-param name="p_contracts" select="$v_getterRequires"/>
									</xsl:call-template>
								</xsl:if>
								<xsl:if test="$v_getterEnsures">
									<xsl:call-template name="t_contractsTable">
										<xsl:with-param name="p_title">
											<include item="header_ensuresName"/>
										</xsl:with-param>
										<xsl:with-param name="p_contracts" select="$v_getterEnsures"/>
									</xsl:call-template>
								</xsl:if>
								<xsl:if test="$v_getterEnsuresOnThrow">
									<xsl:call-template name="t_contractsTable">
										<xsl:with-param name="p_title">
											<include item="header_ensuresOnThrowName"/>
										</xsl:with-param>
										<xsl:with-param name="p_contracts" select="$v_getterEnsuresOnThrow"/>
									</xsl:call-template>
								</xsl:if>
							</xsl:with-param>
						</xsl:call-template>
					</xsl:if>
					<xsl:if test="$v_setter">
						<xsl:variable name="v_setterRequires" select="$v_setter/requires"/>
						<xsl:variable name="v_setterEnsures" select="$v_setter/ensures"/>
						<xsl:variable name="v_setterEnsuresOnThrow" select="$v_setter/ensuresOnThrow"/>
						<xsl:call-template name="t_putSubSection">
							<xsl:with-param name="p_title">
								<include item="title_setter"/>
							</xsl:with-param>
							<xsl:with-param name="p_content">
								<xsl:if test="$v_setterRequires">
									<xsl:call-template name="t_contractsTable">
										<xsl:with-param name="p_title">
											<include item="header_requiresName"/>
										</xsl:with-param>
										<xsl:with-param name="p_contracts" select="$v_setterRequires"/>
									</xsl:call-template>
								</xsl:if>
								<xsl:if test="$v_setterEnsures">
									<xsl:call-template name="t_contractsTable">
										<xsl:with-param name="p_title">
											<include item="header_ensuresName"/>
										</xsl:with-param>
										<xsl:with-param name="p_contracts" select="$v_setterEnsures"/>
									</xsl:call-template>
								</xsl:if>
								<xsl:if test="$v_setterEnsuresOnThrow">
									<xsl:call-template name="t_contractsTable">
										<xsl:with-param name="p_title">
											<include item="header_ensuresOnThrowName"/>
										</xsl:with-param>
										<xsl:with-param name="p_contracts" select="$v_setterEnsuresOnThrow"/>
									</xsl:call-template>
								</xsl:if>
							</xsl:with-param>
						</xsl:call-template>
					</xsl:if>
					<xsl:if test="$v_requires">
						<xsl:call-template name="t_contractsTable">
							<xsl:with-param name="p_title">
								<include item="header_requiresName"/>
							</xsl:with-param>
							<xsl:with-param name="p_contracts" select="$v_requires"/>
						</xsl:call-template>
					</xsl:if>
					<xsl:if test="$v_ensures">
						<xsl:call-template name="t_contractsTable">
							<xsl:with-param name="p_title">
								<include item="header_ensuresName"/>
							</xsl:with-param>
							<xsl:with-param name="p_contracts" select="$v_ensures"/>
						</xsl:call-template>
					</xsl:if>
					<xsl:if test="$v_ensuresOnThrow">
						<xsl:call-template name="t_contractsTable">
							<xsl:with-param name="p_title">
								<include item="header_ensuresOnThrowName"/>
							</xsl:with-param>
							<xsl:with-param name="p_contracts" select="$v_ensuresOnThrow"/>
						</xsl:call-template>
					</xsl:if>
					<xsl:if test="$v_invariants">
						<xsl:call-template name="t_contractsTable">
							<xsl:with-param name="p_title">
								<include item="header_invariantsName"/>
							</xsl:with-param>
							<xsl:with-param name="p_contracts" select="$v_invariants"/>
						</xsl:call-template>
					</xsl:if>
					<!-- Contracts link -->
					<w:p>
						<a>
							<xsl:attribute name="href">
								<xsl:text>http://research.microsoft.com/en-us/projects/contracts/</xsl:text>
							</xsl:attribute>
							<include item="contracts_LearnMore" />
						</a>
					</w:p>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template name="t_contractsTable">
		<xsl:param name="p_title"/>
		<xsl:param name="p_contracts"/>
		<w:tbl>
			<w:tblPr>
				<w:tblStyle w:val="GeneralTable"/>
				<w:tblW w:w="5000" w:type="pct"/>
				<w:tblLook w:firstRow="1" w:noHBand="1" w:noVBand="1"/>
			</w:tblPr>
			<w:tr>
				<w:trPr>
					<w:cnfStyle w:firstRow="1" />
				</w:trPr>
				<w:tc>
					<w:p>
						<w:r>
							<w:t><xsl:copy-of select="$p_title"/></w:t>
						</w:r>
					</w:p>
				</w:tc>
			</w:tr>
			<xsl:for-each select="$p_contracts">
				<w:tr>
					<w:tc>
						<w:tbl>
							<w:tblPr>
								<w:tblStyle w:val="CodeTable"/>
								<w:tblW w:w="5000" w:type="pct"/>
								<w:tblLook w:firstRow="0" w:noHBand="1" w:noVBand="1"/>
							</w:tblPr>
							<w:tr>
								<w:tc>
									<w:p>
										<w:r>
											<!-- Keep this on the same line to prevent extra space from getting included -->
											<w:t xml:space="preserve"><xsl:value-of select="."/></w:t>
										</w:r>
									</w:p>
								</w:tc>
							</w:tr>
						</w:tbl>
						<xsl:if test="@description or @inheritedFrom or @exception">
							<w:tbl>
								<w:tblPr>
									<w:tblStyle w:val="GeneralTable"/>
									<w:tblW w:w="5000" w:type="pct"/>
									<w:tblLook w:firstRow="0" w:noHBand="1" w:noVBand="1"/>
								</w:tblPr>
								<xsl:if test="@description">
									<w:tr>
										<w:tc>
											<span class="Emphasis"><include item="contracts_Description" /></span>
										</w:tc>
										<w:tc>
											<xsl:value-of select="@description"/>
										</w:tc>
									</w:tr>
								</xsl:if>
								<xsl:if test="@inheritedFrom">
									<w:tr>
										<w:tc>
											<span class="Emphasis">
												<include item="contracts_InheritedFrom" />
											</span>
										</w:tc>
										<w:tc>
											<!-- Change the ID type and strip "get_" and "set_" prefixes from property member IDs -->
											<xsl:variable name="inheritedMemberId">
												<xsl:choose>
													<xsl:when test="contains(@inheritedFrom, '.get_')">
														<xsl:value-of select="concat('P:', substring-before(substring(@inheritedFrom, 3), '.get_'), '.', substring-after(@inheritedFrom, '.get_'))"/>
													</xsl:when>
													<xsl:when test="contains(@inheritedFrom, '.set_')">
														<!-- For the setter, we need to strip the last parameter too -->
														<xsl:variable name="lastParam">
															<xsl:call-template name="t_getLastParameter">
																<xsl:with-param name="p_string" select="@inheritedFrom" />
															</xsl:call-template>
														</xsl:variable>
														<xsl:variable name="setterName">
															<xsl:value-of select="concat('P:', substring-before(substring(@inheritedFrom, 3), '.set_'), '.', substring-after(@inheritedFrom, '.set_'))"/>
														</xsl:variable>
														<xsl:value-of select="concat(substring-before($setterName, $lastParam), ')')"/>
													</xsl:when>
													<xsl:otherwise>
														<xsl:value-of select="@inheritedFrom"/>
													</xsl:otherwise>
												</xsl:choose>
											</xsl:variable>
											<referenceLink target="{$inheritedMemberId}">
												<xsl:value-of select="@inheritedFromTypeName"/>
											</referenceLink>
										</w:tc>
									</w:tr>
								</xsl:if>
								<xsl:if test="@exception">
									<w:tr>
										<w:tc>
											<span class="Emphasis">
												<include item="contracts_Exception" />
											</span>
										</w:tc>
										<w:tc>
											<referenceLink target="{@exception}" qualified="false"/>
										</w:tc>
									</w:tr>
								</xsl:if>
							</w:tbl>
						</xsl:if>
						<w:p/>
					</w:tc>
				</w:tr>
			</xsl:for-each>
		</w:tbl>
		<w:p>
			<w:pPr>
				<w:spacing w:after="0" />
			</w:pPr>
		</w:p>
	</xsl:template>

	<!-- Gets the parameter following the last comma in the given string -->
	<xsl:template name="t_getLastParameter">
		<xsl:param name="p_string" />
		<xsl:choose>
			<xsl:when test="contains($p_string, ',')">
				<xsl:call-template name="t_getLastParameter">
					<xsl:with-param name="p_string" select="substring-after($p_string, ',')" />
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="concat(',', $p_string)" />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<!-- ======================================================================================== -->

	<xsl:template name="t_putSeeAlsoSection">
		<xsl:if test="$g_hasSeeAlsoSection">
			<!-- The Open XML file builder task will reformat the bookmark name and ID to ensure that they are unique -->
			<w:bookmarkStart w:name="_SeeAlso" w:id="0" />
			<w:bookmarkEnd w:id="0" />
			<xsl:call-template name="t_putSectionInclude">
				<xsl:with-param name="p_titleInclude" select="'title_relatedTopics'"/>
				<xsl:with-param name="p_content">
					<xsl:call-template name="t_autogenSeeAlsoLinks"/>
					<xsl:for-each select="/document/comments//seealso[not(ancestor::overloads)] | /document/reference/elements/element/overloads//seealso">
						<w:p>
							<w:pPr>
								<w:spacing w:after="0" />
							</w:pPr>
							<xsl:apply-templates select=".">
								<xsl:with-param name="displaySeeAlso" select="true()"/>
							</xsl:apply-templates>
						</w:p>
					</xsl:for-each>
					<!-- Copy conceptualLink elements as-is -->
					<xsl:for-each select="/document/comments/conceptualLink">
						<w:p>
							<w:pPr>
								<w:spacing w:after="0" />
							</w:pPr>
							<xsl:copy-of select="."/>
						</w:p>
					</xsl:for-each>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<!-- ============================================================================================
	Lists
	============================================================================================= -->

	<xsl:template match="list[@type='bullet' or @type='number' or @type='nobullet' or @type='']" name="t_itemList">
		<ul>
			<xsl:attribute name="class">
				<xsl:value-of select="@type" />
			</xsl:attribute>
			<xsl:if test="@type='number' and @start">
				<xsl:attribute name="start">
					<xsl:value-of select="@start"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:for-each select="item">
				<li>
					<xsl:choose>
						<xsl:when test="term and description">
							<w:p>
								<span class="Bold">
									<xsl:apply-templates select="term" />
								</span>
								<xsl:text> - </xsl:text>
								<xsl:apply-templates select="description" />
							</w:p>
						</xsl:when>
						<xsl:when test="term or description">
							<xsl:apply-templates select="term" />
							<xsl:apply-templates select="description" />
						</xsl:when>
						<xsl:otherwise>
							<xsl:apply-templates />
						</xsl:otherwise>
					</xsl:choose>
				</li>
			</xsl:for-each>
		</ul>
	</xsl:template>

	<xsl:template match="list[@type='table']" name="t_tableList">
		<w:tbl>
			<w:tblPr>
				<w:tblStyle w:val="GeneralTable"/>
				<w:tblW w:w="5000" w:type="pct"/>
				<xsl:choose>
					<xsl:when test="listheader">
						<w:tblLook w:firstRow="1" w:noHBand="1" w:noVBand="1"/>
					</xsl:when>
					<xsl:otherwise>
						<w:tblLook w:firstRow="0" w:noHBand="1" w:noVBand="1"/>
					</xsl:otherwise>
				</xsl:choose>
			</w:tblPr>
			<xsl:for-each select="listheader">
				<w:tr>
					<xsl:for-each select="*">
						<w:tc>
							<xsl:apply-templates/>
						</w:tc>
					</xsl:for-each>
				</w:tr>
			</xsl:for-each>
			<xsl:for-each select="item">
				<w:tr>
					<xsl:for-each select="*">
						<w:tc>
							<xsl:apply-templates/>
						</w:tc>
					</xsl:for-each>
				</w:tr>
			</xsl:for-each>
		</w:tbl>
		<w:p>
			<w:pPr>
				<w:spacing w:after="0" />
			</w:pPr>
		</w:p>
	</xsl:template>

	<xsl:template match="list[@type='definition']" name="t_definitionList">
		<xsl:for-each select="item">
			<w:p>
				<w:pPr>
					<w:spacing w:after="0" />
				</w:pPr>
				<span class="Bold">
					<xsl:apply-templates select="term"/>
				</span>
			</w:p>
			<xsl:apply-templates select="description"/>
		</xsl:for-each>
	</xsl:template>

	<!-- ============================================================================================
	Inline tags
	============================================================================================= -->

	<xsl:template match="conceptualLink">
		<xsl:choose>
			<xsl:when test="normalize-space(.)">
				<conceptualLink target="{@target}">
					<xsl:apply-templates/>
				</conceptualLink>
			</xsl:when>
			<xsl:otherwise>
				<conceptualLink target="{@target}"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="see[@cref]" name="t_seeCRef">
		<xsl:choose>
			<xsl:when test="starts-with(@cref,'O:')">
				<referenceLink target="{concat('Overload:',substring(@cref,3))}" display-target="format"
					show-parameters="false">
					<xsl:if test="@autoUpgrade">
						<xsl:attribute name="prefer-overload">
							<xsl:value-of select="@autoUpgrade"/>
						</xsl:attribute>
					</xsl:if>
					<xsl:choose>
						<xsl:when test="normalize-space(.)">
							<xsl:value-of select="." />
						</xsl:when>
						<xsl:otherwise>
							<xsl:if test="@qualifyHint">
								<xsl:attribute name="show-container">
									<xsl:value-of select="@qualifyHint"/>
								</xsl:attribute>
								<xsl:attribute name="show-parameters">
									<xsl:value-of select="@qualifyHint"/>
								</xsl:attribute>
							</xsl:if>
							<include item="boilerplate_seeAlsoOverloadLink">
								<parameter>{0}</parameter>
							</include>
						</xsl:otherwise>
					</xsl:choose>
				</referenceLink>
			</xsl:when>
			<xsl:when test="normalize-space(.)">
				<referenceLink target="{@cref}">
					<xsl:if test="@autoUpgrade">
						<xsl:attribute name="prefer-overload">
							<xsl:value-of select="@autoUpgrade"/>
						</xsl:attribute>
					</xsl:if>
					<xsl:apply-templates/>
				</referenceLink>
			</xsl:when>
			<xsl:otherwise>
				<referenceLink target="{@cref}">
					<xsl:if test="@autoUpgrade">
						<xsl:attribute name="prefer-overload">
							<xsl:value-of select="@autoUpgrade"/>
						</xsl:attribute>
					</xsl:if>
					<xsl:if test="@qualifyHint">
						<xsl:attribute name="show-container">
							<xsl:value-of select="@qualifyHint"/>
						</xsl:attribute>
						<xsl:attribute name="show-parameters">
							<xsl:value-of select="@qualifyHint"/>
						</xsl:attribute>
					</xsl:if>
				</referenceLink>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="see[@href]" name="t_seeHRef">
		<xsl:call-template name="t_hyperlink">
			<xsl:with-param name="p_content" select="."/>
			<xsl:with-param name="p_href" select="@href"/>
			<xsl:with-param name="p_alt" select="@alt"/>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="see[@langword]">
		<xsl:choose>
			<xsl:when test="@langword='null' or @langword='Nothing' or @langword='nullptr'">
				<include item="devlang_nullKeyword"/>
			</xsl:when>
			<xsl:when test="@langword='static' or @langword='Shared'">
				<include item="devlang_staticKeyword"/>
			</xsl:when>
			<xsl:when test="@langword='virtual' or @langword='Overridable'">
				<include item="devlang_virtualKeyword"/>
			</xsl:when>
			<xsl:when test="@langword='true' or @langword='True'">
				<include item="devlang_trueKeyword"/>
			</xsl:when>
			<xsl:when test="@langword='false' or @langword='False'">
				<include item="devlang_falseKeyword"/>
			</xsl:when>
			<xsl:when test="@langword='abstract' or @langword='MustInherit'">
				<include item="devlang_abstractKeyword"/>
			</xsl:when>
			<xsl:when test="@langword='sealed' or @langword='NotInheritable'">
				<include item="devlang_sealedKeyword"/>
			</xsl:when>
			<xsl:when test="@langword='async' or @langword='async'">
				<include item="devlang_asyncKeyword"/>
			</xsl:when>
			<xsl:when test="@langword='await' or @langword='Await' or @langword='let!'">
				<include item="devlang_awaitKeyword"/>
			</xsl:when>
			<xsl:when test="@langword='async/await' or @langword='Async/Await' or @langword='async/let!'">
				<include item="devlang_asyncAwaitKeyword"/>
			</xsl:when>
			<xsl:otherwise>
				<span class="Keyword">
					<xsl:value-of select="@langword" />
				</span>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="seealso[@href]" name="t_seealsoHRef">
		<xsl:param name="displaySeeAlso" select="false()"/>
		<xsl:if test="$displaySeeAlso">
			<xsl:call-template name="t_hyperlink">
				<xsl:with-param name="p_content" select="."/>
				<xsl:with-param name="p_href" select="@href"/>
				<xsl:with-param name="p_alt" select="@alt"/>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="seealso" name="t_seealso">
		<xsl:param name="displaySeeAlso" select="false()"/>
		<xsl:if test="$displaySeeAlso">
			<xsl:choose>
				<xsl:when test="starts-with(@cref,'O:')">
					<referenceLink target="{concat('Overload:',substring(@cref,3))}" display-target="format"
						show-parameters="false">
						<xsl:choose>
							<xsl:when test="normalize-space(.)">
								<xsl:apply-templates />
							</xsl:when>
							<xsl:otherwise>
								<include item="boilerplate_seeAlsoOverloadLink">
									<parameter>{0}</parameter>
								</include>
							</xsl:otherwise>
						</xsl:choose>
					</referenceLink>
				</xsl:when>
				<xsl:when test="normalize-space(.)">
					<referenceLink target="{@cref}" qualified="true">
						<xsl:if test="@autoUpgrade">
							<xsl:attribute name="prefer-overload">
								<xsl:value-of select="@autoUpgrade"/>
							</xsl:attribute>
						</xsl:if>
						<xsl:apply-templates />
					</referenceLink>
				</xsl:when>
				<xsl:otherwise>
					<referenceLink target="{@cref}" qualified="true">
						<xsl:if test="@autoUpgrade">
							<xsl:attribute name="prefer-overload">
								<xsl:value-of select="@autoUpgrade"/>
							</xsl:attribute>
						</xsl:if>
					</referenceLink>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:if>
	</xsl:template>

	<!-- ======================================================================================== -->

	<xsl:template name="t_hyperlink">
		<xsl:param name="p_content"/>
		<xsl:param name="p_href"/>
		<xsl:param name="p_alt"/>

		<a>
			<xsl:attribute name="href">
				<xsl:value-of select="$p_href"/>
			</xsl:attribute>
			<xsl:attribute name="target">_blank</xsl:attribute>
			<xsl:if test="normalize-space($p_alt)">
				<xsl:attribute name="title">
					<xsl:value-of select="normalize-space($p_alt)"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:choose>
				<xsl:when test="normalize-space($p_content)">
					<xsl:value-of select="normalize-space($p_content)"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="$p_href"/>
				</xsl:otherwise>
			</xsl:choose>
		</a>
	</xsl:template>

	<!-- ======================================================================================== -->

	<xsl:template match="note" name="t_note">
		<xsl:call-template name="t_putAlert">
			<xsl:with-param name="p_alertClass" select="@type"/>
		</xsl:call-template>
	</xsl:template>

	<!-- ======================================================================================== -->

	<xsl:template name="t_getParameterDescription">
		<xsl:param name="name"/>
		<xsl:apply-templates select="/document/comments/param[@name=$name]"/>
	</xsl:template>

	<xsl:template name="t_getReturnsDescription">
		<xsl:param name="name"/>
		<xsl:apply-templates select="/document/comments/param[@name=$name]"/>
	</xsl:template>

	<xsl:template name="t_getElementDescription">
		<xsl:apply-templates select="summary[1]"/>
	</xsl:template>

	<xsl:template name="t_getOverloadSummary">
		<xsl:apply-templates select="overloads" mode="summary"/>
	</xsl:template>

	<xsl:template name="t_getOverloadSections">
		<xsl:apply-templates select="overloads" mode="sections"/>
	</xsl:template>

	<!-- ============================================================================================ -->

	<!-- Pass through a chunk of markup.  This will allow build components to add Open XML or other elements such
			 as "include" for localized shared content to a pre-transformed document.  This prevents it being removed
			 as unrecognized content by the transformations. -->
	<xsl:template match="markup">
		<xsl:copy-of select="node()"/>
	</xsl:template>
	
	<!-- Bibliography is not supported -->
	<xsl:template match="cite" />

</xsl:stylesheet>
