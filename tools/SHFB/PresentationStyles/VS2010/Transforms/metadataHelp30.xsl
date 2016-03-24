<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
								version="2.0"
								xmlns:ddue="http://ddue.schemas.microsoft.com/authoring/2003/5"
								xmlns:xlink="http://www.w3.org/1999/xlink"
								xmlns:msxsl="urn:schemas-microsoft-com:xslt"
   >
	<!-- ======================================================================================== -->

	<xsl:template name="t_insertMetadataHelp30">

		<!-- System.Language -->
		<meta name="Language">
			<includeAttribute name="content" item="locale" />
		</meta>

		<!-- System.Title -->
		<!-- <title> is set elsewhere -->

		<!-- System.Keywords -->
		<!-- Microsoft.Help.F1 -->
		<xsl:call-template name="t_indexMetadata30" />
		<xsl:call-template name="t_helpMetadata30" />
		<xsl:call-template name="t_authoredMetadata30" />

		<!-- Microsoft.Help.Id -->
		<meta name="Microsoft.Help.Id" content="{$key}" />

		<!-- Microsoft.Help.Description -->
		<xsl:if test="$g_abstractSummary">
			<xsl:variable name="v_description">
				<xsl:call-template name="t_getTrimmedAtPeriod">
					<xsl:with-param name="p_string" select="$g_abstractSummary" />
				</xsl:call-template>
			</xsl:variable>
			<xsl:if test="normalize-space($v_description)">
				<meta name="Description">
					<xsl:attribute name="content">
						<xsl:value-of select="normalize-space($v_description)"/>
					</xsl:attribute>
				</meta>
			</xsl:if>
		</xsl:if>

		<!-- Microsoft.Help.TocParent -->
		<xsl:if test="/document/metadata/attribute[@name='TOCParent']">
			<meta name="Microsoft.Help.TocParent" content="{/document/metadata/attribute[@name='TOCParent']}" />
		</xsl:if>
		<xsl:if test="/document/metadata/attribute[@name='TOCOrder']">
			<meta name="Microsoft.Help.TocOrder" content="{/document/metadata/attribute[@name='TOCOrder']}" />
		</xsl:if>

		<!-- Microsoft.Help.Category -->
		<xsl:for-each select="/document/metadata/attribute[@name='Category']">
			<meta name="Microsoft.Help.Category" content="{.}" />
		</xsl:for-each>

		<!-- Microsoft.Help.ContentFilter -->
		<xsl:for-each select="/document/metadata/attribute[@name='ContentFilter']">
			<meta name="Microsoft.Help.ContentFilter" content="{.}" />
		</xsl:for-each>

		<!-- Microsoft.Help.ContentType -->
		<meta name="Microsoft.Help.ContentType" content="Reference" />

		<!-- Microsoft.Package.Book -->
		<xsl:variable name="v_book" select="/document/metadata/attribute[@name='Book']/text()" />
		<xsl:if test="$v_book">
			<meta name="Microsoft.Package.Book" content="{$v_book}" />
		</xsl:if>

		<!-- Branding aware.  This prevents the MSHC Component from changing a couple of CSS style names. -->
		<meta name="BrandingAware" content="true"/>
	</xsl:template>

	<!-- ======================================================================================== -->

	<xsl:template name="t_indexMetadata30">
		<xsl:choose>
			<!-- namespace topics get one unqualified index entry -->
			<xsl:when test="$g_topicGroup='api' and $g_apiGroup='namespace'">
				<xsl:variable name="v_names">
					<xsl:for-each select="/document/reference">
						<xsl:call-template name="textNames" />
					</xsl:for-each>
				</xsl:variable>
				<meta name="System.Keywords">
					<includeAttribute name="content"
														item="indexEntry_namespace">
						<parameter>
							<xsl:value-of select="msxsl:node-set($v_names)/name" />
						</parameter>
					</includeAttribute>
				</meta>
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
					<meta name="System.Keywords">
						<includeAttribute name="content"
															item="indexEntry_{$g_apiSubGroup}">
							<parameter>
								<xsl:copy-of select="."/>
							</parameter>
						</includeAttribute>
					</meta>
					<xsl:if test="boolean($v_namespace != '')">
						<meta name="System.Keywords">
							<includeAttribute name="content"
																item="indexEntry_{$g_apiSubGroup}">
								<parameter>
									<xsl:value-of select="$v_namespace"/>
									<xsl:text>.</xsl:text>
									<xsl:copy-of select="." />
								</parameter>
							</includeAttribute>
						</meta>
					</xsl:if>
					<!-- multi-topic types (not delegates and enumerations) get about entries, too-->
					<xsl:if test="$g_apiSubGroup='class' or $g_apiSubGroup='structure' or $g_apiSubGroup='interface'">
						<meta name="System.Keywords">
							<includeAttribute name="content"
																item="indexEntry_aboutType">
								<parameter>
									<include item="indexEntry_{$g_apiSubGroup}">
										<parameter>
											<xsl:copy-of select="."/>
										</parameter>
									</include>
								</parameter>
							</includeAttribute>
						</meta>
					</xsl:if>
				</xsl:for-each>
				<!-- enumerations get the index entries for their members -->
				<xsl:if test="$g_apiSubGroup='enumeration'">
					<xsl:for-each select="/document/reference/elements/element">
						<meta name="System.Keywords">
							<includeAttribute name="content"
																item="indexEntry_{$g_apiSubGroup}Member">
								<parameter>
									<xsl:value-of select="apidata/@name" />
								</parameter>
							</includeAttribute>
						</meta>
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
					<meta name="System.Keywords">
						<includeAttribute name="content"
															item="indexEntry_{$g_apiSubGroup}">
							<parameter>
								<xsl:value-of select="." />
							</parameter>
						</includeAttribute>
					</meta>
					<meta name="System.Keywords">
						<includeAttribute name="content"
															item="indexEntry_members">
							<parameter>
								<include item="indexEntry_{$g_apiSubGroup}">
									<parameter>
										<xsl:value-of select="." />
									</parameter>
								</include>
							</parameter>
						</includeAttribute>
					</meta>
				</xsl:for-each>
				<xsl:variable name="v_qnames">
					<xsl:for-each select="/document/reference">
						<xsl:call-template name="qualifiedTextNames" />
					</xsl:for-each>
				</xsl:variable>
				<xsl:if test="boolean($v_namespace != '')">
					<xsl:for-each select="msxsl:node-set($v_qnames)/name">
						<meta name="System.Keywords">
							<includeAttribute name="content"
																item="indexEntry_{$g_apiSubGroup}">
								<parameter>
									<xsl:value-of select="." />
								</parameter>
							</includeAttribute>
						</meta>
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
							<meta name="System.Keywords">
								<includeAttribute name="content"
																	item="indexEntry_{$v_entryType}">
									<parameter>
										<include item="indexEntry_{$g_apiSubGroup}">
											<parameter>
												<xsl:value-of select="." />
											</parameter>
										</include>
									</parameter>
								</includeAttribute>
							</meta>
						</xsl:for-each>
					</xsl:when>
					<xsl:otherwise>
						<xsl:for-each select="msxsl:node-set($v_names)/name">
							<meta name="System.Keywords">
								<includeAttribute name="content"
																	item="indexEntry_{$g_apiTopicSubGroup}">
									<parameter>
										<include item="indexEntry_{$g_apiSubGroup}">
											<parameter>
												<xsl:value-of select="." />
											</parameter>
										</include>
									</parameter>
								</includeAttribute>
							</meta>
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
					<meta name="System.Keywords">
						<includeAttribute name="content"
															item="indexEntry_constructor">
							<parameter>
								<include item="indexEntry_{$v_typeSubgroup}">
									<parameter>
										<xsl:value-of select="." />
									</parameter>
								</include>
							</parameter>
						</includeAttribute>
					</meta>
				</xsl:for-each>
				<xsl:variable name="v_qnames">
					<xsl:for-each select="/document/reference">
						<xsl:call-template name="qualifiedTextNames" />
					</xsl:for-each>
				</xsl:variable>
				<xsl:for-each select="msxsl:node-set($v_qnames)/name">
					<meta name="System.Keywords">
						<includeAttribute name="content"
															item="indexEntry_constructorType">
							<parameter>
								<xsl:value-of select="." />
							</parameter>
						</includeAttribute>
					</meta>
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
					<meta name="System.Keywords">
						<includeAttribute name="content"
															item="indexEntry_conversionOperator">
							<parameter>
								<xsl:copy-of select="."/>
							</parameter>
						</includeAttribute>
					</meta>
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
							<meta name="System.Keywords">
								<includeAttribute name="content"
																	item="indexEntry_{$v_entryType}Explicit">
									<parameter>
										<xsl:copy-of select="."/>
									</parameter>
								</includeAttribute>
							</meta>
						</xsl:for-each>
						<xsl:variable name="v_qnames">
							<xsl:for-each select="/document/reference">
								<xsl:call-template name="qualifiedTextNames" />
							</xsl:for-each>
						</xsl:variable>
						<xsl:for-each select="msxsl:node-set($v_qnames)/name">
							<meta name="System.Keywords">
								<includeAttribute name="content"
																	item="indexEntry_{$v_entryType}Explicit">
									<parameter>
										<xsl:copy-of select="."/>
									</parameter>
								</includeAttribute>
							</meta>
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
							<meta name="System.Keywords">
								<includeAttribute name="content"
																	item="indexEntry_{$v_entryType}">
									<parameter>
										<xsl:copy-of select="."/>
									</parameter>
								</includeAttribute>
							</meta>
						</xsl:for-each>
						<xsl:variable name="v_qnames">
							<xsl:for-each select="/document/reference">
								<xsl:call-template name="qualifiedTextNames" />
							</xsl:for-each>
						</xsl:variable>
						<xsl:for-each select="msxsl:node-set($v_qnames)/name">
							<meta name="System.Keywords">
								<includeAttribute name="content"
																	item="indexEntry_{$v_entryType}">
									<parameter>
										<xsl:copy-of select="."/>
									</parameter>
								</includeAttribute>
							</meta>
						</xsl:for-each>
					</xsl:otherwise>
				</xsl:choose>

			</xsl:when>
			<!-- derived type lists get unqualified sub-entries -->
		</xsl:choose>
	</xsl:template>

	<!-- ======================================================================================== -->

	<xsl:template name="t_helpMetadata30">
		<!-- F keywords -->
		<xsl:choose>

			<!-- namespace pages get the namespace keyword, if it exists -->
			<xsl:when test="$g_apiTopicGroup='namespace'">
				<xsl:variable name="v_namespace"
											select="/document/reference/apidata/@name" />
				<xsl:if test="$v_namespace != ''">
					<meta name="Microsoft.Help.F1"
								content="{$v_namespace}" />
				</xsl:if>
			</xsl:when>

			<!-- type memberlist topics do NOT get F keywords -->
			<xsl:when test="$g_apiTopicGroup='list' and $g_apiTopicSubGroup='members'"/>

			<!-- type overview pages get namespace.type keywords -->
			<xsl:when test="$g_apiTopicGroup='type'">
				<xsl:variable name="v_namespace"
											select="/document/reference/containers/namespace/apidata/@name" />
				<xsl:variable name="v_type">
					<xsl:for-each select="/document/reference[1]">
						<xsl:call-template name="typeNameWithTicks" />
					</xsl:for-each>
				</xsl:variable>
				<xsl:call-template name="t_writeF1WithApiName">
					<xsl:with-param name="p_namespace"
													select="$v_namespace" />
					<xsl:with-param name="p_type"
													select="$v_type" />
					<xsl:with-param name="p_member"
													select="''" />
				</xsl:call-template>

				<!-- for enums, write F1 keywords for each enum member -->
				<xsl:if test="$g_apiTopicSubGroup = 'enumeration'">
					<xsl:for-each select="/document/reference/elements/element">
						<xsl:call-template name="t_writeF1WithApiName">
							<xsl:with-param name="p_namespace"
															select="$v_namespace" />
							<xsl:with-param name="p_type"
															select="$v_type" />
							<xsl:with-param name="p_member"
															select="apidata/@name" />
						</xsl:call-template>

					</xsl:for-each>
				</xsl:if>

				<!-- Insert additional F1 keywords to support XAML for class, struct, and enum topics in a set of namespaces. -->
				<xsl:call-template name="t_xamlMSHelpFKeywords30"/>
			</xsl:when>

			<!-- overload list pages get namespace.type.member keyword -->
			<xsl:when test="$g_apiTopicGroup='list' and $g_apiTopicSubGroup='overload'">
				<xsl:variable name="v_namespace"
											select="/document/reference/containers/namespace/apidata/@name" />
				<xsl:variable name="v_type">
					<xsl:for-each select="/document/reference[1]/containers">
						<xsl:call-template name="typeNameWithTicks" />
					</xsl:for-each>
				</xsl:variable>

				<xsl:variable name="v_containingTypeId"
											select="/document/reference/containers/type[1]/@api" />
				<!-- do not write F1 keyword for overload list topics that contain only inherited members -->
				<xsl:if test="/document/reference/elements//element/containers/type[1][@api=$v_containingTypeId]">

					<!-- Generate a result tree fragment with all of the names for this overload page, TFS 856956, 864173-->
					<xsl:variable name="v_F1Names">
						<xsl:choose>
							<xsl:when test="/document/reference/apidata[@subgroup='constructor']">
								<name>
									<xsl:text>#ctor</xsl:text>
								</name>
								<name>
									<xsl:value-of select="/document/reference/containers/type[1]/apidata/@name" />
								</name>
							</xsl:when>
							<xsl:otherwise>
								<name>
									<xsl:value-of select="/document/reference/apidata/@name" />
								</name>
								<xsl:for-each select="/document/reference/elements/element[templates and containers/type[1][@api=$v_containingTypeId]]">
									<name>
										<xsl:value-of select="apidata/@name" />
										<xsl:text>``</xsl:text>
										<xsl:value-of select="count(templates/template)" />
									</name>
								</xsl:for-each>
							</xsl:otherwise>
						</xsl:choose>
					</xsl:variable>

					<xsl:for-each select="msxsl:node-set($v_F1Names)//name[not(. = preceding::name)]">
						<xsl:sort select="." />
						<xsl:call-template name="t_writeF1WithApiName">
							<xsl:with-param name="p_namespace"
															select="$v_namespace" />
							<xsl:with-param name="p_type"
															select="$v_type" />
							<xsl:with-param name="p_member"
															select="." />
						</xsl:call-template>
					</xsl:for-each>
				</xsl:if>
			</xsl:when>

			<!-- member pages -->
			<xsl:when test="$g_apiTopicGroup='member'">
				<xsl:choose>
					<!-- no F1 help entries for overload signature topics -->
					<xsl:when test="/document/reference/memberdata/@overload"/>

					<!-- no F1 help entries for explicit interface implementation members -->
					<xsl:when test="/document/reference[memberdata[@visibility='private'] and proceduredata[@virtual = 'true']]"/>

					<!-- Property pages -->
					<xsl:when test="$g_apiTopicSubGroup = 'property'">

						<xsl:variable name="v_type">
							<xsl:for-each select="/document/reference[1]/containers">
								<xsl:call-template name="typeNameWithTicks" />
							</xsl:for-each>
						</xsl:variable>

						<xsl:for-each select="document/reference/apidata/@name | document/reference/getter/@name | document/reference/setter/@name">
							<xsl:call-template name="t_writeF1WithApiName">
								<xsl:with-param name="p_namespace"
																select="/document/reference/containers/namespace/apidata/@name" />
								<xsl:with-param name="p_type"
																select="$v_type" />
								<xsl:with-param name="p_member"
																select="." />
							</xsl:call-template>
						</xsl:for-each>
					</xsl:when>

					<!-- other member pages get namespace.type.member keywords -->
					<xsl:otherwise>
						<xsl:call-template name="t_memberF1KeywordsHelp30"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:when>

		</xsl:choose>
	</xsl:template>

	<!-- ============================================================================================
	Insert additional F1 keywords for class, struct, and enum topics in a set of WPF namespaces.
	The keyword prefixes and the WPF namespaces are hard-coded in variables.
	============================================================================================= -->

	<xsl:template name="t_xamlMSHelpFKeywords30">
		<xsl:if test="$g_apiTopicSubGroup='class' or $g_apiTopicSubGroup='enumeration' or $g_apiTopicSubGroup='structure'">
			<xsl:if test="boolean(contains($g_wpf_f1index_prefix_1_namespaces, concat('#',/document/reference/containers/namespace/@api,'#'))
                           or starts-with($g_wpf_f1index_prefix_1_namespaces, concat(/document/reference/containers/namespace/@api,'#')))">
				<meta name="Microsoft.Help.F1"
							content="{concat($g_wpf_f1index_prefix_1, /document/reference/apidata/@name)}"/>
			</xsl:if>
		</xsl:if>
	</xsl:template>

	<!-- ======================================================================================== -->

	<xsl:template name="t_writeF1WithApiName">
		<xsl:param name="p_namespace"/>
		<xsl:param name="p_type" />
		<xsl:param name="p_member" />

		<!-- Make versions of namespace and member that are joinable. -->

		<xsl:variable name="v_namespaceJoinable">
			<xsl:choose>
				<xsl:when test="$p_namespace = ''">
					<xsl:value-of select="''" />
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="concat($p_namespace, '.')" />
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>

		<xsl:variable name="v_memberJoinable">
			<xsl:choose>
				<xsl:when test="$p_member = ''">
					<xsl:value-of select="''" />
				</xsl:when>
				<xsl:when test="substring($p_type, string-length($p_type)) = '.'">
					<xsl:value-of select="$p_member" />
				</xsl:when>
				<xsl:when test="substring($p_member, string-length($p_member)) = '.'">
					<xsl:value-of select="substring($p_member, string-length($p_member) - 1)" />
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="concat('.', $p_member)" />
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>

		<xsl:variable name="v_apiName"
									select="concat($v_namespaceJoinable, $p_type, $v_memberJoinable)" />

		<xsl:if test="not($v_namespaceJoinable != '' and $p_type = '' and $v_memberJoinable != '') and $v_apiName != ''">
			<meta name="Microsoft.Help.F1"
						content="{concat($v_namespaceJoinable, $p_type, $v_memberJoinable)}" />
		</xsl:if>

	</xsl:template>

	<xsl:template name="t_memberF1KeywordsHelp30">
		<xsl:variable name="v_namespace"
									select="/document/reference/containers/namespace/apidata/@name" />
		<xsl:variable name="v_type">
			<xsl:for-each select="/document/reference/containers/type[1]">
				<xsl:call-template name="typeNameWithTicks" />
			</xsl:for-each>
		</xsl:variable>
		<xsl:variable name="v_member">
			<xsl:choose>
				<!-- if the member is a constructor, use "#ctor" as the member name -->
				<xsl:when test="/document/reference/apidata[@subgroup='constructor']">#ctor</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="/document/reference/apidata/@name"/>
					<!-- for generic members, include tick notation for number of generic template parameters. -->
					<xsl:if test="/document/reference/templates/template">
						<xsl:text>``</xsl:text>
						<xsl:value-of select="count(/document/reference/templates/template)"/>
					</xsl:if>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>

		<xsl:call-template name="t_writeF1WithApiName">
			<xsl:with-param name="p_namespace"
											select="$v_namespace" />
			<xsl:with-param name="p_type"
											select="$v_type" />
			<xsl:with-param name="p_member"
											select="$v_member" />
		</xsl:call-template>

		<!-- Write the constructor again as type.type -->
		<xsl:if test="/document/reference/apidata[@subgroup='constructor']">
			<xsl:call-template name="t_writeF1WithApiName">
				<xsl:with-param name="p_namespace"
												select="$v_namespace" />
				<xsl:with-param name="p_type"
												select="$v_type" />
				<xsl:with-param name="p_member"
												select="/document/reference/containers/type/apidata/@name" />
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<!-- ============================================================================================
	Attributes and keywords added to topics by authors
	============================================================================================= -->

	<xsl:template name="t_authoredMetadata30">

		<xsl:for-each select="/document/metadata/keyword[@index='K']">
			<meta name="System.Keywords">
				<xsl:attribute name="content">
					<xsl:value-of select="text()" />
					<xsl:for-each select="keyword[@index='K']">
						<xsl:text>, </xsl:text>
						<xsl:value-of select="text()"/>
					</xsl:for-each>
				</xsl:attribute>
			</meta>
		</xsl:for-each>

		<!-- authored F -->
		<xsl:for-each select="/document/metadata/keyword[@index='F']">
			<meta name="Microsoft.Help.F1">
				<xsl:attribute name="content">
					<xsl:value-of select="text()" />
					<xsl:for-each select="keyword[@index='F']">
						<xsl:text>, </xsl:text>
						<xsl:value-of select="text()"/>
					</xsl:for-each>
				</xsl:attribute>
			</meta>
		</xsl:for-each>

		<!-- authored B -->
		<xsl:for-each select="/document/metadata/keyword[@index='B']">
			<meta name="Microsoft.Help.F1">
				<xsl:attribute name="content">
					<xsl:value-of select="text()" />
					<xsl:for-each select="keyword[@index='B']">
						<xsl:text>, </xsl:text>
						<xsl:value-of select="text()"/>
					</xsl:for-each>
				</xsl:attribute>
			</meta>
		</xsl:for-each>

	</xsl:template>

</xsl:stylesheet>
