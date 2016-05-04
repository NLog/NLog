<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
								version="2.0"
								xmlns:MSHelp="http://msdn.microsoft.com/mshelp"
								xmlns:ddue="http://ddue.schemas.microsoft.com/authoring/2003/5"
								xmlns:xlink="http://www.w3.org/1999/xlink"
								xmlns:msxsl="urn:schemas-microsoft-com:xslt"
   >
	<!-- ======================================================================================== -->

	<xsl:template name="t_insertMetadataHelp20">
		<xsl:if test="$metadata='true'">
			<xml>
				<MSHelp:Attr Name="AssetID"
										 Value="{$key}" />
				<!-- toc title, rl title, etc. -->
				<xsl:call-template name="t_mshelpTitles" />
				<!-- keywords for the A (link target) index -->
				<xsl:call-template name="t_linkMetadata" />
				<!-- keywords for the K (index) index -->
				<xsl:call-template name="t_indexMetadata" />
				<!-- keywords for the F (F1 help) index -->
				<xsl:call-template name="t_helpMetadata" />
				<!-- help priority settings -->
				<xsl:call-template name="t_helpPriorityMetadata" />
				<!-- attributes for api identification -->
				<xsl:call-template name="t_apiTaggingMetadata" />
				<!-- attributes for filtering -->
				<xsl:call-template name="t_mshelpDevlangAttributes" />
				<MSHelp:Attr Name="Locale">
					<includeAttribute name="Value" item="locale" />
				</MSHelp:Attr>
				<!-- attribute to allow F1 help integration -->
				<MSHelp:Attr Name="TopicType"
										 Value="kbSyntax" />
				<MSHelp:Attr Name="TopicType"
										 Value="apiref" />

				<!-- Abstract -->
				<xsl:choose>
					<xsl:when test="string-length(normalize-space($g_abstractSummary)) &gt; 254">
						<MSHelp:Attr Name="Abstract" Value="{concat(substring(normalize-space($g_abstractSummary),1,250), ' ...')}" />
					</xsl:when>
					<xsl:when test="string-length(normalize-space($g_abstractSummary)) &gt; 0 and $g_abstractSummary != '&#160;'">
						<MSHelp:Attr Name="Abstract" Value="{normalize-space($g_abstractSummary)}" />
					</xsl:when>
				</xsl:choose>

				<!-- Assembly Version-->
				<xsl:if test="$g_apiGroup != 'namespace' and $g_apiGroup != 'namespaceGroup'">
					<MSHelp:Attr Name="AssemblyVersion" Value="{/document/reference/containers/library/assemblydata/@version}" />
				</xsl:if>

				<xsl:call-template name="t_versionMetadata" />
				<xsl:call-template name="t_authoredMetadata" />
			</xml>
		</xsl:if>
	</xsl:template>

	<!-- ============================================================================================
	Add DocSet and Technology attributes depending on the versions that support this api
	============================================================================================= -->

	<xsl:template name="t_versionMetadata">
		<xsl:variable name="v_supportedOnCf">
			<xsl:call-template name="t_isMemberSupportedOnCf"/>
		</xsl:variable>
		<xsl:variable name="v_supportedOnXNA">
			<xsl:call-template name="t_isMemberSupportedOnXna" />
		</xsl:variable>
		<xsl:if test="count(/document/reference/versions/versions[@name='netfw']//version) &gt; 0 or count(/document/reference/topicdata/versions/versions[@name='netfw']//version) &gt; 0 ">
			<MSHelp:Attr Name="Technology">
				<includeAttribute name="Value"
													item="meta_help20_desktopTechnologyAttribute" />
			</MSHelp:Attr>
		</xsl:if>
		<!-- insert CF values for Technology and DocSet attributes for: 
            api topics that have netcfw version nodes
            memberlist topics where topicdata/versions has netcfw version nodes
            overload list topics where any of the elements has netcfw version nodes
    -->
		<xsl:if test="count(/document/reference/versions/versions[@name='netcfw']//version) &gt; 0 or count(/document/reference/topicdata/versions/versions[@name='netcfw']//version) &gt; 0  or count(/document/reference[topicdata[@subgroup='overload']]/elements//element/versions/versions[@name='netcfw']//version) &gt; 0 or normalize-space($v_supportedOnCf)!=''">
			<MSHelp:Attr Name="Technology">
				<includeAttribute name="Value"
													item="meta_help20_netcfTechnologyAttribute" />
			</MSHelp:Attr>
			<MSHelp:Attr Name="DocSet">
				<includeAttribute name="Value"
													item="meta_help20_netcfDocSetAttribute" />
			</MSHelp:Attr>
		</xsl:if>
		<!-- insert XNA values for Technology and DocSet attributes for: 
            api topics that have xnafw version nodes
            memberlist topics where topicdata/versions has xnafw version nodes
            overload list topics where any of the elements has xnafw version nodes
    -->
		<xsl:if test="count(/document/reference/versions/versions[@name='xnafw']//version) &gt; 0 or count(/document/reference/topicdata/versions/versions[@name='xnafw']//version) &gt; 0  or count(/document/reference[topicdata[@subgroup='overload']]/elements//element/versions/versions[@name='xnafw']//version) &gt; 0 or normalize-space($v_supportedOnXNA)!=''">
			<MSHelp:Attr Name="Technology">
				<includeAttribute name="Value"
													item="meta_help20_xnaTechnologyAttribute" />
			</MSHelp:Attr>
			<MSHelp:Attr Name="DocSet">
				<includeAttribute name="Value"
													item="meta_help20_xnaDocSetAttribute" />
			</MSHelp:Attr>
		</xsl:if>
	</xsl:template>

	<!-- ============================================================================================
	Attributes and keywords added to topics by authors
	============================================================================================= -->

	<xsl:template name="t_authoredMetadata">

		<!-- authored attributes -->
		<xsl:for-each select="/document/metadata/attribute">
			<MSHelp:Attr Name="{@name}"
									 Value="{text()}" />
		</xsl:for-each>

		<!-- authored K -->
		<xsl:for-each select="/document/metadata/keyword[@index='K']">
			<MSHelp:Keyword Index="K">
				<xsl:attribute name="Term">
					<xsl:value-of select="text()" />
					<xsl:for-each select="keyword[@index='K']">
						<xsl:text>, </xsl:text>
						<xsl:value-of select="text()"/>
					</xsl:for-each>
				</xsl:attribute>
			</MSHelp:Keyword>
		</xsl:for-each>

		<!-- authored S -->
		<xsl:for-each select="/document/metadata/keyword[@index='S']">
			<MSHelp:Keyword Index="S">
				<xsl:attribute name="Term">
					<xsl:value-of select="text()" />
					<xsl:for-each select="keyword[@index='S']">
						<xsl:text>, </xsl:text>
						<xsl:value-of select="text()"/>
					</xsl:for-each>
				</xsl:attribute>
			</MSHelp:Keyword>
			<!-- S index keywords need to be converted to F index keywords -->
			<MSHelp:Keyword Index="F">
				<xsl:attribute name="Term">
					<xsl:value-of select="text()" />
					<xsl:for-each select="keyword[@index='S']">
						<xsl:text>, </xsl:text>
						<xsl:value-of select="text()"/>
					</xsl:for-each>
				</xsl:attribute>
			</MSHelp:Keyword>
		</xsl:for-each>

		<!-- authored F -->
		<xsl:for-each select="/document/metadata/keyword[@index='F']">
			<MSHelp:Keyword Index="F">
				<xsl:attribute name="Term">
					<xsl:value-of select="text()" />
					<xsl:for-each select="keyword[@index='F']">
						<xsl:text>, </xsl:text>
						<xsl:value-of select="text()"/>
					</xsl:for-each>
				</xsl:attribute>
			</MSHelp:Keyword>
		</xsl:for-each>

		<!-- authored B -->
		<xsl:for-each select="/document/metadata/keyword[@index='B']">
			<MSHelp:Keyword Index="B">
				<xsl:attribute name="Term">
					<xsl:value-of select="text()" />
					<xsl:for-each select="keyword[@index='B']">
						<xsl:text>, </xsl:text>
						<xsl:value-of select="text()"/>
					</xsl:for-each>
				</xsl:attribute>
			</MSHelp:Keyword>
		</xsl:for-each>

	</xsl:template>

	<!-- ============================================================================================
	toc title and rl title
	============================================================================================= -->

	<xsl:template name="t_mshelpTitles">

		<!-- TOC List title-->
		<MSHelp:TOCTitle>
			<includeAttribute name="Title" item="meta_mshelp_tocTitle">
				<parameter>
					<!-- For namespaces TOC titles, only show the namespace without any descriptive suffix -->
					<xsl:choose>
						<xsl:when test="$g_apiTopicGroup='namespace'">
							<xsl:call-template name="t_shortNamePlain" />
						</xsl:when>
						<xsl:otherwise>
							<xsl:call-template name="t_topicTitlePlain" />
						</xsl:otherwise>
					</xsl:choose>
				</parameter>
			</includeAttribute>
		</MSHelp:TOCTitle>

		<!-- The Results List title -->
		<MSHelp:RLTitle>
			<includeAttribute name="Title" item="meta_mshelp_rlTitle">
				<parameter>
					<xsl:call-template name="t_topicTitlePlain">
						<xsl:with-param name="p_qualifyMembers" select="true()" />
					</xsl:call-template>
				</parameter>
				<parameter>
					<xsl:value-of select="$g_namespaceName"/>
				</parameter>
			</includeAttribute>
		</MSHelp:RLTitle>

	</xsl:template>

	<xsl:template name="t_apiTaggingMetadata">
		<xsl:if test="$g_topicGroup='api' and ($g_apiGroup='type' or $g_apiGroup='member')">
			<MSHelp:Attr Name="APIType"
									 Value="Managed" />
			<MSHelp:Attr Name="APILocation"
									 Value="{/document/reference/containers/library/@assembly}.dll" />
			<xsl:choose>
				<xsl:when test="$g_apiGroup='type'">
					<xsl:variable name="v_apiTypeName">
						<xsl:choose>
							<xsl:when test="/document/reference/containers/namespace/apidata/@name != ''">
								<xsl:value-of select="concat(/document/reference/containers/namespace/apidata/@name,'.',/document/reference/apidata/@name)" />
							</xsl:when>
							<xsl:otherwise>
								<xsl:value-of select="/document/reference/apidata/@name" />
							</xsl:otherwise>
						</xsl:choose>
						<xsl:if test="count(/document/reference/templates/template) > 0">
							<xsl:value-of select="concat('`',count(/document/reference/templates/template))" />
						</xsl:if>
					</xsl:variable>
					<!-- Namespace + Type -->
					<MSHelp:Attr Name="APIName"
											 Value="{$v_apiTypeName}" />
					<xsl:choose>
						<xsl:when test="boolean($g_apiSubGroup='delegate')">
							<MSHelp:Attr Name="APIName"
													 Value="{concat($v_apiTypeName,'..ctor')}" />
							<MSHelp:Attr Name="APIName"
													 Value="{concat($v_apiTypeName,'.','Invoke')}" />
							<MSHelp:Attr Name="APIName"
													 Value="{concat($v_apiTypeName,'.','BeginInvoke')}" />
							<MSHelp:Attr Name="APIName"
													 Value="{concat($v_apiTypeName,'.','EndInvoke')}" />
						</xsl:when>
						<xsl:when test="$g_apiSubGroup='enumeration'">
							<xsl:for-each select="/document/reference/elements/element">
								<MSHelp:Attr Name="APIName"
														 Value="{substring(@api,3)}" />
							</xsl:for-each>
							<!-- Namespace + Type + Member for each member -->
						</xsl:when>
					</xsl:choose>
				</xsl:when>
				<xsl:when test="$g_apiGroup='member'">
					<xsl:variable name="v_namespace"
												select="/document/reference/containers/namespace/apidata/@name" />
					<xsl:variable name="v_type">
						<xsl:for-each select="/document/reference/containers/type[1]">
							<xsl:call-template name="typeNameWithTicks" />
						</xsl:for-each>
					</xsl:variable>
					<xsl:variable name="v_member"
												select="/document/reference/apidata/@name" />
					<!-- Namespace + Type + Member -->
					<MSHelp:Attr Name="APIName"
											 Value="{concat($v_namespace, '.', $v_type, '.', $v_member)}" />
					<xsl:choose>
						<!-- for properties, add APIName attribute get/set accessor methods -->
						<xsl:when test="boolean($g_apiSubGroup='property')">
							<xsl:if test="/document/reference/propertydata[@get='true']">
								<MSHelp:Attr Name="APIName"
														 Value="{concat($v_namespace, '.', $v_type, '.get_', $v_member)}" />
							</xsl:if>
							<xsl:if test="/document/reference/propertydata[@set='true']">
								<MSHelp:Attr Name="APIName"
														 Value="{concat($v_namespace, '.', $v_type, '.set_', $v_member)}" />
							</xsl:if>
						</xsl:when>
						<!-- for events, add APIName attribute add/remove accessor methods -->
						<xsl:when test="boolean($g_apiSubGroup='event')">
							<xsl:if test="/document/reference/eventdata[@add='true']">
								<MSHelp:Attr Name="APIName"
														 Value="{concat($v_namespace, '.', $v_type, '.add_', $v_member)}" />
							</xsl:if>
							<xsl:if test="/document/reference/eventdata[@remove='true']">
								<MSHelp:Attr Name="APIName"
														 Value="{concat($v_namespace, '.', $v_type, '.remove_', $v_member)}" />
							</xsl:if>
						</xsl:when>
						<!-- for operators, add APIName attribute op accessor methods -->
						<xsl:when test="boolean($g_apiSubSubGroup='operator')">
							<MSHelp:Attr Name="APIName"
													 Value="{concat($v_namespace, '.', $v_type, '.op_', $v_member)}" />
						</xsl:when>
					</xsl:choose>
				</xsl:when>
			</xsl:choose>
		</xsl:if>
	</xsl:template>

	<!-- ============================================================================================
	link target (A index) keywords
	============================================================================================= -->

	<xsl:template name="t_linkMetadata">

		<!-- code entity reference keyword -->
		<MSHelp:Keyword Index="A"
										Term="{$key}" />

		<xsl:if test="$g_topicGroup='api' and $g_apiSubGroup='enumeration'">
			<xsl:for-each select="/document/reference/elements/element">
				<MSHelp:Keyword Index="A"
												Term="{@api}" />
			</xsl:for-each>
		</xsl:if>

		<!-- frlrf keywords -->
		<xsl:call-template name="t_FrlrfKeywords"/>

	</xsl:template>

	<xsl:template name="t_FrlrfKeywords">
		<xsl:variable name="v_FrlrfTypeName">
			<!-- for members and nested types, start with the containing type name -->
			<xsl:for-each select="/document/reference/containers/type">
				<xsl:call-template name="t_FrlrfTypeName"/>
			</xsl:for-each>
			<!-- for types and member list topics, append the type name -->
			<xsl:if test="/document/reference/apidata[@group='type']">
				<xsl:for-each select="/document/reference">
					<xsl:call-template name="t_FrlrfTypeName"/>
				</xsl:for-each>
			</xsl:if>
		</xsl:variable>
		<xsl:variable name="v_memberName">
			<xsl:choose>
				<xsl:when test="/document/reference/apidata[@subgroup='constructor']">
					<xsl:value-of select="'ctor'"/>
				</xsl:when>
				<xsl:when test="/document/reference/apidata[@subsubgroup='operator']">
					<xsl:value-of select="concat('op_', /document/reference/apidata/@name)"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="/document/reference/apidata/@name"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:choose>
			<!-- namespace topic -->
			<xsl:when test="/document/reference/apidata/@group='namespace'">
				<MSHelp:Keyword Index="A"
												Term="{translate(concat('frlrf',$v_memberName),'.','')}"/>
			</xsl:when>
			<!-- Overload topic -->
			<xsl:when test="/document/reference/topicdata[@subgroup='overload']">
				<xsl:variable name="v_FrlrfBaseId">
					<xsl:value-of select="translate(concat('frlrf', $g_namespaceName, $v_FrlrfTypeName, 'Class', $v_memberName, 'Topic'),'.','')"/>
				</xsl:variable>
				<MSHelp:Keyword Index="A"
												Term="{$v_FrlrfBaseId}"/>
				<!-- whidbey included frlrf keyword for each overload, but I don't think we need in Manifold, so commenting it out -->
				<!--
        <xsl:for-each select="elements/element">
          <MSHelp:Keyword Index="A" Term="{concat($v_FrlrfBaseId, string(position()))}"/>
        </xsl:for-each>
        -->
			</xsl:when>
			<!-- Member list topic (other than overload list captured above) -->
			<xsl:when test="/document/reference/topicdata[@group='list']">
				<xsl:variable name="v_memberListSubgroup">
					<xsl:choose>
						<xsl:when test="/document/reference/topicdata/@subgroup='members'">Members</xsl:when>
						<xsl:otherwise>
							<xsl:value-of select="/document/reference/topicdata/@subgroup"/>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:variable>
				<MSHelp:Keyword Index="A"
												Term="{translate(concat('frlrf', $g_namespaceName, $v_FrlrfTypeName, $v_memberListSubgroup, 'Topic'),'.','')}"/>
			</xsl:when>
			<!-- type topic -->
			<xsl:when test="/document/reference/apidata[@group='type']">
				<MSHelp:Keyword Index="A"
												Term="{translate(concat('frlrf',$g_namespaceName, $v_FrlrfTypeName, 'ClassTopic'),'.','')}"/>
			</xsl:when>
			<!-- no frlrf ID for overload signature topics-->
			<xsl:when test="/document/reference/apidata[@group='member'] and /document/reference/memberdata/@overload"/>
			<!-- non-overload member topic -->
			<xsl:when test="/document/reference/apidata[@group='member']">
				<MSHelp:Keyword Index="A"
												Term="{translate(concat('frlrf',$g_namespaceName, $v_FrlrfTypeName, 'Class', $v_memberName, 'Topic'),'.','')}"/>
			</xsl:when>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="t_FrlrfTypeName">
		<xsl:for-each select="type">
			<xsl:call-template name="t_FrlrfTypeName"/>
		</xsl:for-each>
		<xsl:choose>
			<xsl:when test="templates/template">
				<xsl:value-of select="concat(apidata/@name, count(templates/template))"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="apidata/@name"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<!-- ============================================================================================
	F1 (F index) keywords
	============================================================================================= -->

	<xsl:template name="t_helpMetadata">
		<!-- F keywords -->
		<xsl:choose>
			<!-- namespace pages get the namespace keyword, if it exists -->
			<xsl:when test="$g_apiTopicGroup='namespace'">
				<xsl:variable name="v_namespace" select="/document/reference/apidata/@name" />
				<xsl:if test="string($v_namespace) != ''">
					<MSHelp:Keyword Index="F" Term="{$v_namespace}" />
				</xsl:if>
			</xsl:when>
			<!-- Type overview page gets type and namespace.type keywords. -->
			<xsl:when test="$g_apiTopicGroup='type'">
				<xsl:variable name="v_namespace" select="/document/reference/containers/namespace/apidata/@name" />
				<xsl:variable name="v_type">
					<xsl:for-each select="/document/reference[1]">
						<xsl:call-template name="typeNameWithTicks" />
					</xsl:for-each>
				</xsl:variable>
				<xsl:if test="string($v_namespace) != ''">
					<MSHelp:Keyword Index="F" Term="{concat($v_namespace,'.',$v_type)}" />
				</xsl:if>
				<MSHelp:Keyword Index="F" Term="{$v_type}" />
				<xsl:if test="$g_apiTopicSubGroup = 'enumeration'">
					<xsl:for-each select="/document/reference/elements/element">
						<xsl:choose>
							<xsl:when test="string($v_namespace) != ''">
								<MSHelp:Keyword Index="F" Term="{concat($v_namespace,'.',$v_type, '.', apidata/@name)}" />
							</xsl:when>
							<xsl:otherwise>
								<MSHelp:Keyword Index="F" Term="{concat($v_type, '.', apidata/@name)}" />
							</xsl:otherwise>
						</xsl:choose>
					</xsl:for-each>
				</xsl:if>
				<xsl:call-template name="xamlMSHelpFKeywords"/>
			</xsl:when>

			<!-- No F keywords on AllMembers pages, TFS 851543. -->
			<xsl:when test="$g_apiTopicGroup='list' and $g_apiTopicSubGroup='members'" />

			<!-- overload list pages get member, type.member, and namepsace.type.member keywords -->
			<xsl:when test="$g_apiTopicGroup='list' and $g_apiTopicSubGroup='overload'">
				<xsl:variable name="v_namespace" select="/document/reference/containers/namespace/apidata/@name" />
				<xsl:variable name="v_type">
					<xsl:for-each select="/document/reference/containers/type[1]">
						<xsl:call-template name="typeNameWithTicks" />
					</xsl:for-each>
				</xsl:variable>
				<xsl:variable name="v_member">
					<xsl:choose>
						<!-- if the member is a constructor, use the member name for the type name -->
						<xsl:when test="/document/reference/apidata[@subgroup='constructor']">
							<xsl:value-of select="$v_type" />
						</xsl:when>
						<xsl:otherwise>
							<xsl:value-of select="/document/reference/apidata/@name"/>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:variable>

				<xsl:if test="string($v_namespace) != ''">
					<MSHelp:Keyword Index="F" Term="{concat($v_namespace,'.',$v_type, '.', $v_member)}" />
				</xsl:if>
				<MSHelp:Keyword Index="F" Term="{concat($v_type, '.', $v_member)}" />
				<MSHelp:Keyword Index="F" Term="{$v_member}" />
			</xsl:when>

			<!-- no F1 help entries for overload signature topics -->
			<xsl:when test="$g_apiTopicGroup='member' and /document/reference/memberdata/@overload"/>

			<!-- member pages get member, type.member, and namepsace.type.member keywords -->
			<xsl:when test="$g_apiTopicGroup='member'">
				<xsl:variable name="v_namespace" select="/document/reference/containers/namespace/apidata/@name" />
				<xsl:variable name="v_type">
					<xsl:for-each select="/document/reference/containers/type[1]">
						<xsl:call-template name="typeNameWithTicks" />
					</xsl:for-each>
				</xsl:variable>
				<xsl:variable name="v_member">
					<xsl:choose>
						<!-- if the member is a constructor, use the member name for the type name -->
						<xsl:when test="$g_apiTopicSubGroup='constructor'">
							<xsl:value-of select="$v_type" />
						</xsl:when>
						<!-- explicit interface implementation -->
						<xsl:when test="document/reference[memberdata[@visibility='private'] and proceduredata[@virtual = 'true']]">
							<xsl:for-each select="/document/reference/implements/member">
								<xsl:call-template name="typeNameWithTicks" />
							</xsl:for-each>
						</xsl:when>
						<xsl:otherwise>
							<xsl:value-of select="/document/reference/apidata/@name"/>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:variable>

				<xsl:if test="string($v_namespace) != ''">
					<MSHelp:Keyword Index="F" Term="{concat($v_namespace,'.',$v_type, '.', $v_member)}" />
				</xsl:if>
				<MSHelp:Keyword Index="F" Term="{concat($v_type, '.', $v_member)}" />
				<MSHelp:Keyword Index="F" Term="{$v_member}" />
			</xsl:when>
		</xsl:choose>
	</xsl:template>

	<!-- ============================================================================================
	Set high help priority for namespace and member list pages, lower priority for type overview pages
	============================================================================================= -->

	<xsl:template name="t_helpPriorityMetadata">
		<xsl:choose>
			<xsl:when test="($g_topicGroup='api' and $g_apiGroup='namespace') or ($g_topicGroup='list' and $g_topicSubGroup='members')">
				<MSHelp:Attr Name="HelpPriority"
										 Value="1"/>
			</xsl:when>
			<xsl:when test="$g_topicGroup='api' and $g_apiGroup='type'">
				<MSHelp:Attr Name="HelpPriority"
										 Value="2"/>
			</xsl:when>
		</xsl:choose>
	</xsl:template>

	<!-- ============================================================================================
	Language attributes
	============================================================================================= -->

	<xsl:template name="t_mshelpDevlangAttributes">
		<!-- First insert a DevLang attribute for each language in the $languages argument passed to the transform -->
		<xsl:for-each select="$languages/language">
			<xsl:variable name="v_devlang">
				<xsl:call-template name="t_codeLangName">
					<xsl:with-param name="p_codeLang" select="@name"/>
				</xsl:call-template>
			</xsl:variable>
			<xsl:if test="normalize-space($v_devlang)!=''">
				<MSHelp:Attr Name="DevLang">
					<includeAttribute name="Value" item="metaLang_{$v_devlang}"/>
				</MSHelp:Attr>
			</xsl:if>
		</xsl:for-each>

		<!-- Make a list of the languages that have already been included via $languages -->
		<xsl:variable name="languagesList">
			<xsl:for-each select="$languages/language">
				<xsl:variable name="v_devlang">
					<xsl:call-template name="t_codeLangName">
						<xsl:with-param name="p_codeLang" select="@name"/>
					</xsl:call-template>
				</xsl:variable>
				<xsl:if test="normalize-space($v_devlang)!=''">
					<xsl:value-of select="$v_devlang"/>
					<xsl:text>;</xsl:text>
				</xsl:if>
			</xsl:for-each>
		</xsl:variable>

		<!-- Add DevLang attribute for any additional languages referred to in the topic's snippet and code nodes -->
		<xsl:for-each select="//*[@language]">
			<xsl:if test="not(@language=preceding::*/@language)">
				<xsl:variable name="v_devlang">
					<xsl:call-template name="t_codeLangName">
						<xsl:with-param name="p_codeLang" select="@language"/>
					</xsl:call-template>
				</xsl:variable>
				<xsl:choose>
					<xsl:when test="normalize-space($v_devlang)=''"/>
					<xsl:when test="contains($languagesList,concat($v_devlang,';'))"/>
					<xsl:otherwise>
						<MSHelp:Attr Name="DevLang">
							<includeAttribute name="Value" item="metaLang_{$v_devlang}"/>
						</MSHelp:Attr>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:if>
		</xsl:for-each>

		<!-- Extend the list of languages that have already been included -->
		<xsl:variable name="languagesList2">
			<xsl:value-of select="$languagesList"/>
			<xsl:for-each select="//*[@language]">
				<xsl:variable name="v_devlang">
					<xsl:call-template name="t_codeLangName">
						<xsl:with-param name="p_codeLang" select="@language"/>
					</xsl:call-template>
				</xsl:variable>
				<xsl:if test="normalize-space($v_devlang)!=''">
					<xsl:value-of select="$v_devlang"/>
					<xsl:text>;</xsl:text>
				</xsl:if>
			</xsl:for-each>
		</xsl:variable>

		<!-- Add DevLang attribute for any additional languages referred to in the topic's syntax blocks -->
		<xsl:for-each select="/document/syntax/div[@codeLanguage and not(div[@class='nonXamlAssemblyBoilerplate'])]">
			<xsl:if test="not(@codeLanguage=preceding::*/@codeLanguage)">
				<xsl:variable name="v_devlang">
					<xsl:call-template name="t_codeLangName">
						<xsl:with-param name="p_codeLang" select="@codeLanguage"/>
					</xsl:call-template>
				</xsl:variable>
				<xsl:choose>
					<xsl:when test="normalize-space($v_devlang)=''"/>
					<xsl:when test="contains($languagesList2,concat($v_devlang,';'))"/>
					<xsl:otherwise>
						<MSHelp:Attr Name="DevLang">
							<includeAttribute name="Value" item="metaLang_{$v_devlang}"/>
						</MSHelp:Attr>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:if>
		</xsl:for-each>
	</xsl:template>

	<!-- ============================================================================================
  Additional F1 keywords for class, struct, and enum topics in a set of WPF namespaces. 
  This template inserts the MSHelp:Keyword nodes.
  The keyword prefixes and the WPF namespaces are hard-coded in variables.
	============================================================================================= -->

	<xsl:variable name="g_wpf_f1index_prefix_1">http://schemas.microsoft.com/winfx/2006/xaml/presentation#</xsl:variable>
	<xsl:variable name="g_wpf_f1index_prefix_1_namespaces">N:System.Windows.Controls#N:System.Windows.Documents#N:System.Windows.Shapes#N:System.Windows.Navigation#N:System.Windows.Data#N:System.Windows#N:System.Windows.Controls.Primitives#N:System.Windows.Media.Animation#N:System.Windows.Annotations#N:System.Windows.Annotations.Anchoring#N:System.Windows.Annotations.Storage#N:System.Windows.Media#N:System.Windows.Media.Animation#N:System.Windows.Media.Media3D#N:</xsl:variable>

	<xsl:template name="xamlMSHelpFKeywords">
		<xsl:if test="$g_apiTopicSubGroup='class' or $g_apiTopicSubGroup='enumeration' or $g_apiTopicSubGroup='structure'">
			<xsl:if test="boolean(contains($g_wpf_f1index_prefix_1_namespaces, concat('#',/document/reference/containers/namespace/@api,'#'))
                           or starts-with($g_wpf_f1index_prefix_1_namespaces, concat(/document/reference/containers/namespace/@api,'#')))">
				<MSHelp:Keyword Index="F"
												Term="{concat($g_wpf_f1index_prefix_1, /document/reference/apidata/@name)}"/>
			</xsl:if>
		</xsl:if>
	</xsl:template>

	<!-- ============================================================================================
  Index Logic
	============================================================================================= -->

	<xsl:template name="t_indexMetadata">
		<xsl:choose>
			<!-- namespace topics get one unqualified index entry -->
			<xsl:when test="$g_topicGroup='api' and $g_apiGroup='namespace'">
				<xsl:variable name="v_names">
					<xsl:for-each select="/document/reference">
						<xsl:call-template name="textNames" />
					</xsl:for-each>
				</xsl:variable>
				<MSHelp:Keyword Index="K">
					<includeAttribute name="Term"
														item="indexEntry_namespace">
						<parameter>
							<xsl:value-of select="msxsl:node-set($v_names)/name" />
						</parameter>
					</includeAttribute>
				</MSHelp:Keyword>
			</xsl:when>
			<!-- type overview topics get qualified and unqualified index entries, and an about index entry -->
			<xsl:when test="$g_topicGroup='api' and $g_apiGroup='type'">
				<xsl:variable name="v_names">
					<xsl:for-each select="/document/reference">
						<xsl:call-template name="textNames" />
					</xsl:for-each>
				</xsl:variable>
				<xsl:variable name="v_namespace"
											select="/document/reference/containers/namespace/apidata/@name" />
				<xsl:for-each select="msxsl:node-set($v_names)/name">
					<MSHelp:Keyword Index="K">
						<includeAttribute name="Term"
															item="indexEntry_{$g_apiSubGroup}">
							<parameter>
								<xsl:copy-of select="."/>
							</parameter>
						</includeAttribute>
					</MSHelp:Keyword>
					<xsl:if test="boolean($v_namespace != '')">
						<MSHelp:Keyword Index="K">
							<includeAttribute name="Term"
																item="indexEntry_{$g_apiSubGroup}">
								<parameter>
									<xsl:value-of select="$v_namespace"/>
									<xsl:text>.</xsl:text>
									<xsl:copy-of select="." />
								</parameter>
							</includeAttribute>
						</MSHelp:Keyword>
					</xsl:if>
					<!-- multi-topic types (not delegates and enumerations) get about entries, too-->
					<xsl:if test="$g_apiSubGroup='class' or $g_apiSubGroup='structure' or $g_apiSubGroup='interface'">
						<MSHelp:Keyword Index="K">
							<includeAttribute name="Term"
																item="indexEntry_aboutType">
								<parameter>
									<include item="indexEntry_{$g_apiSubGroup}">
										<parameter>
											<xsl:copy-of select="."/>
										</parameter>
									</include>
								</parameter>
							</includeAttribute>
						</MSHelp:Keyword>
					</xsl:if>
				</xsl:for-each>
				<!-- enumerations get the index entries for their members -->
				<xsl:if test="$g_apiSubGroup='enumeration'">
					<xsl:for-each select="/document/reference/elements/element">
						<MSHelp:Keyword Index="K">
							<includeAttribute name="Term"
																item="indexEntry_{$g_apiSubGroup}Member">
								<parameter>
									<xsl:value-of select="apidata/@name" />
								</parameter>
							</includeAttribute>
						</MSHelp:Keyword>
					</xsl:for-each>
				</xsl:if>
			</xsl:when>
			<!-- all member lists get unqualified entries, qualified entries, and unqualified sub-entries -->
			<xsl:when test="$g_topicGroup='list' and $g_topicSubGroup='members'">
				<xsl:variable name="v_namespace"
											select="/document/reference/containers/namespace/apidata/@name" />
				<xsl:variable name="v_names">
					<xsl:for-each select="/document/reference">
						<xsl:call-template name="textNames" />
					</xsl:for-each>
				</xsl:variable>
				<xsl:for-each select="msxsl:node-set($v_names)/name">
					<MSHelp:Keyword Index="K">
						<includeAttribute name="Term"
															item="indexEntry_{$g_apiSubGroup}">
							<parameter>
								<xsl:value-of select="." />
							</parameter>
						</includeAttribute>
					</MSHelp:Keyword>
					<MSHelp:Keyword Index="K">
						<includeAttribute name="Term"
															item="indexEntry_members">
							<parameter>
								<include item="indexEntry_{$g_apiSubGroup}">
									<parameter>
										<xsl:value-of select="." />
									</parameter>
								</include>
							</parameter>
						</includeAttribute>
					</MSHelp:Keyword>
				</xsl:for-each>
				<xsl:variable name="v_qnames">
					<xsl:for-each select="/document/reference">
						<xsl:call-template name="qualifiedTextNames" />
					</xsl:for-each>
				</xsl:variable>
				<xsl:if test="boolean($v_namespace != '')">
					<xsl:for-each select="msxsl:node-set($v_qnames)/name">
						<MSHelp:Keyword Index="K">
							<includeAttribute name="Term"
																item="indexEntry_{$g_apiSubGroup}">
								<parameter>
									<xsl:value-of select="." />
								</parameter>
							</includeAttribute>
						</MSHelp:Keyword>
					</xsl:for-each>
				</xsl:if>
			</xsl:when>
			<!-- other member list pages get unqualified sub-entries -->
			<xsl:when test="$g_topicGroup='list' and not($g_topicSubGroup = 'overload')">
				<xsl:variable name="v_names">
					<xsl:for-each select="/document/reference">
						<xsl:call-template name="textNames" />
					</xsl:for-each>
				</xsl:variable>
				<xsl:choose>
					<xsl:when test="$g_topicSubGroup='Operators'">
						<xsl:variable name="v_operators"
													select="document/reference/elements/element[not(apidata[@name='Explicit' or @name='Implicit'])]"/>
						<xsl:variable name="v_conversions"
													select="document/reference/elements/element[apidata[@name='Explicit' or @name='Implicit']]" />
						<xsl:variable name="v_entryType">
							<xsl:choose>
								<!-- operators + type conversions -->
								<xsl:when test="count($v_operators) &gt; 0 and count($v_conversions) &gt; 0">
									<xsl:value-of select="'operatorsAndTypeConversions'" />
								</xsl:when>
								<!-- no operators + type conversions -->
								<xsl:when test="not(count($v_operators) &gt; 0) and count($v_conversions) &gt; 0">
									<xsl:value-of select="'typeConversions'" />
								</xsl:when>
								<!-- operators + no type conversions -->
								<xsl:otherwise>
									<xsl:value-of select="$g_topicSubGroup" />
								</xsl:otherwise>
							</xsl:choose>
						</xsl:variable>
						<xsl:for-each select="msxsl:node-set($v_names)/name">
							<MSHelp:Keyword Index="K">
								<includeAttribute name="Term"
																	item="indexEntry_{$v_entryType}">
									<parameter>
										<include item="indexEntry_{$g_apiSubGroup}">
											<parameter>
												<xsl:value-of select="." />
											</parameter>
										</include>
									</parameter>
								</includeAttribute>
							</MSHelp:Keyword>
						</xsl:for-each>
					</xsl:when>
					<xsl:otherwise>
						<xsl:for-each select="msxsl:node-set($v_names)/name">
							<MSHelp:Keyword Index="K">
								<includeAttribute name="Term"
																	item="indexEntry_{$g_apiTopicSubGroup}">
									<parameter>
										<include item="indexEntry_{$g_apiSubGroup}">
											<parameter>
												<xsl:value-of select="." />
											</parameter>
										</include>
									</parameter>
								</includeAttribute>
							</MSHelp:Keyword>
						</xsl:for-each>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:when>
			<!-- constructor (or constructor overload) topics get unqualified sub-entries using the type names -->
			<xsl:when test="($g_topicGroup='api' and $g_apiSubGroup='constructor' and not(/document/reference/memberdata/@overload)) or ($g_topicSubGroup='overload' and $g_apiSubGroup = 'constructor')">
				<xsl:variable name="v_typeSubgroup"
											select="/document/reference/containers/type/apidata/@subgroup" />
				<xsl:variable name="v_names">
					<xsl:for-each select="/document/reference/containers/type">
						<xsl:call-template name="textNames" />
					</xsl:for-each>
				</xsl:variable>
				<xsl:for-each select="msxsl:node-set($v_names)/name">
					<MSHelp:Keyword Index="K">
						<includeAttribute name="Term"
															item="indexEntry_constructor">
							<parameter>
								<include item="indexEntry_{$v_typeSubgroup}">
									<parameter>
										<xsl:value-of select="." />
									</parameter>
								</include>
							</parameter>
						</includeAttribute>
					</MSHelp:Keyword>
				</xsl:for-each>
				<xsl:variable name="v_qnames">
					<xsl:for-each select="/document/reference">
						<xsl:call-template name="qualifiedTextNames" />
					</xsl:for-each>
				</xsl:variable>
				<xsl:for-each select="msxsl:node-set($v_qnames)/name">
					<MSHelp:Keyword Index="K">
						<includeAttribute name="Term"
															item="indexEntry_constructorType">
							<parameter>
								<xsl:value-of select="." />
							</parameter>
						</includeAttribute>
					</MSHelp:Keyword>
				</xsl:for-each>
			</xsl:when>
			<!-- op_explicit and op_implicit members -->
			<xsl:when test="$g_topicGroup='api' and $g_apiSubSubGroup='operator' and (document/reference/apidata/@name='Explicit' or document/reference/apidata/@name='Implicit')">
				<xsl:variable name="v_names">
					<xsl:for-each select="/document/reference">
						<xsl:call-template name="operatorTextNames" />
					</xsl:for-each>
				</xsl:variable>
				<xsl:for-each select="msxsl:node-set($v_names)/name">
					<MSHelp:Keyword Index="K">
						<includeAttribute name="Term"
															item="indexEntry_conversionOperator">
							<parameter>
								<xsl:copy-of select="."/>
							</parameter>
						</includeAttribute>
					</MSHelp:Keyword>
				</xsl:for-each>
			</xsl:when>
			<!-- other member (or overload) topics get qualified and unqualified entries using the member names -->
			<xsl:when test="($g_topicGroup='api' and $g_apiGroup='member' and not(/document/reference/memberdata/@overload)) or $g_topicSubGroup='overload'">

				<xsl:choose>
					<!-- overload op_explicit and op_implicit topics -->
					<xsl:when test="$g_apiSubSubGroup='operator' and (document/reference/apidata/@name='Explicit' or document/reference/apidata/@name='Implicit')">
					</xsl:when>
					<!-- explicit interface implementation -->
					<xsl:when test="/document/reference/proceduredata/@virtual='true' and /document/reference/memberdata/@visibility='private'">
						<xsl:variable name="v_entryType">
							<xsl:choose>
								<xsl:when test="string($g_apiTopicSubSubGroup)">
									<xsl:value-of select="$g_apiTopicSubSubGroup" />
								</xsl:when>
								<xsl:otherwise>
									<xsl:choose>
										<xsl:when test="$g_apiTopicSubGroup='overload'">
											<xsl:value-of select="/document/reference/apidata/@subgroup"/>
										</xsl:when>
										<xsl:otherwise>
											<xsl:value-of select="$g_apiTopicSubGroup" />
										</xsl:otherwise>
									</xsl:choose>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:variable>
						<xsl:variable name="v_names">
							<xsl:for-each select="/document/reference/implements/member">
								<xsl:call-template name="textNames" />
							</xsl:for-each>
						</xsl:variable>
						<xsl:for-each select="msxsl:node-set($v_names)/name">
							<MSHelp:Keyword Index="K">
								<includeAttribute name="Term"
																	item="indexEntry_{$v_entryType}Explicit">
									<parameter>
										<xsl:copy-of select="."/>
									</parameter>
								</includeAttribute>
							</MSHelp:Keyword>
						</xsl:for-each>
						<xsl:variable name="v_qnames">
							<xsl:for-each select="/document/reference">
								<xsl:call-template name="qualifiedTextNames" />
							</xsl:for-each>
						</xsl:variable>
						<xsl:for-each select="msxsl:node-set($v_qnames)/name">
							<MSHelp:Keyword Index="K">
								<includeAttribute name="Term"
																	item="indexEntry_{$v_entryType}Explicit">
									<parameter>
										<xsl:copy-of select="."/>
									</parameter>
								</includeAttribute>
							</MSHelp:Keyword>
						</xsl:for-each>
					</xsl:when>
					<xsl:otherwise>
						<xsl:variable name="v_entryType">
							<xsl:choose>
								<xsl:when test="string($g_apiTopicSubSubGroup)">
									<xsl:value-of select="$g_apiTopicSubSubGroup" />
								</xsl:when>
								<xsl:otherwise>
									<xsl:choose>
										<xsl:when test="$g_apiSubSubGroup='operator'">
											<xsl:value-of select="$g_apiSubSubGroup"/>
										</xsl:when>
										<xsl:when test="$g_apiTopicSubGroup='overload'">
											<xsl:value-of select="/document/reference/apidata/@subgroup"/>
										</xsl:when>
										<xsl:otherwise>
											<xsl:value-of select="$g_apiTopicSubGroup" />
										</xsl:otherwise>
									</xsl:choose>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:variable>
						<xsl:variable name="v_names">
							<xsl:for-each select="/document/reference">
								<xsl:call-template name="textNames" />
							</xsl:for-each>
						</xsl:variable>
						<xsl:for-each select="msxsl:node-set($v_names)/name">
							<MSHelp:Keyword Index="K">
								<includeAttribute name="Term"
																	item="indexEntry_{$v_entryType}">
									<parameter>
										<xsl:copy-of select="."/>
									</parameter>
								</includeAttribute>
							</MSHelp:Keyword>
						</xsl:for-each>
						<xsl:variable name="v_qnames">
							<xsl:for-each select="/document/reference">
								<xsl:call-template name="qualifiedTextNames" />
							</xsl:for-each>
						</xsl:variable>
						<xsl:for-each select="msxsl:node-set($v_qnames)/name">
							<MSHelp:Keyword Index="K">
								<includeAttribute name="Term"
																	item="indexEntry_{$v_entryType}">
									<parameter>
										<xsl:copy-of select="."/>
									</parameter>
								</includeAttribute>
							</MSHelp:Keyword>
						</xsl:for-each>
					</xsl:otherwise>
				</xsl:choose>

			</xsl:when>
			<!-- derived type lists get unqualified sub-entries -->
		</xsl:choose>
	</xsl:template>

</xsl:stylesheet>
