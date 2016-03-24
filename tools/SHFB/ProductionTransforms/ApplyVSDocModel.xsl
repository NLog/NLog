<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                xmlns:ms="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="ms"
                version="2.0">

	<xsl:output indent="yes" encoding="UTF-8" />
	<xsl:param name="derivedTypesLimit" />
	<xsl:param name="project" />

	<!-- Set to true for vs2005; set to false for vsorcas/prototype. -->
	<xsl:param name="IncludeAllMembersTopic" select="'false'" />

	<!-- If member list topics handle overloads with one row that points to an overload topic, set IncludeInheritedOverloadTopics to false. -->
	<!-- If member list topics show a separate row for each overload signature, set IncludeInheritedOverloadTopics to false. -->
	<xsl:param name="IncludeInheritedOverloadTopics" select="'false'" />

	<xsl:key name="index" match="/reflection/apis/api" use="@id" />

	<xsl:variable name="root" select="/" />
	<xsl:template match="/">
		<reflection>
			<xsl:apply-templates select="/reflection/assemblies" />
			<xsl:apply-templates select="/reflection/apis" />
		</reflection>
	</xsl:template>

	<xsl:template match="assemblies">
		<xsl:copy-of select="." />
	</xsl:template>

	<xsl:template match="apis">
		<apis>
			<xsl:apply-templates select="api" />
			<xsl:if test="normalize-space($project)">
				<xsl:call-template name="projectTopic" />
			</xsl:if>
		</apis>
	</xsl:template>

	<!-- Process a generic API (for namespaces and members; types and overloads are handled explicitly below) -->

	<xsl:template match="api">
		<xsl:call-template name="updateApiNode" />
	</xsl:template>

	<xsl:template match="api[apidata/@group='member']">
		<xsl:call-template name="updateApiNode" />
	</xsl:template>

	<xsl:template name="updateApiNode">
		<xsl:variable name="apidataName" select="apidata/@name"/>
		<xsl:variable name="templatesTotal" select="count(templates/*)"/>
		<xsl:variable name="templateParams" select="count(templates/template)"/>
		<xsl:variable name="templateArgs" select="count(templates/*[not(self::template)])"/>
		<!-- strip off any parameters from the end of the id -->
		<xsl:variable name="idWithoutParams">
			<xsl:call-template name="RemoveParametersFromId"/>
		</xsl:variable>
		<xsl:variable name="subgroup" select="apidata/@subgroup"/>
		<xsl:variable name="subsubgroup" select="apidata/@subsubgroup" />
		<xsl:variable name="typeId" select="containers/type/@api"/>
		<xsl:variable name="isEII" select="proceduredata/@eii"/>
		<xsl:variable name="eiiTypeId" select="implements/member/type/@api" />
		<xsl:variable name="containingType" select="containers/type/@api"/>

		<api>
			<xsl:copy-of select="@*"/>
			<topicdata group="api">
				<xsl:if test="key('index',containers/type/@api)[apidata/@subgroup='enumeration']">
					<!-- enum members do not get separate topics; mark them so they are excluded from the manifest -->
					<xsl:attribute name="notopic"/>
				</xsl:if>
				<xsl:if test="proceduredata[@eii='true']">
					<xsl:attribute name="eiiName">
						<xsl:value-of select="concat(key('index', $eiiTypeId)/apidata/@name, '.', $apidataName)"/>
					</xsl:attribute>
				</xsl:if>
			</topicdata>
			<xsl:for-each select="*">
				<xsl:choose>
					<xsl:when test="local-name(.)='containers'">
						<containers>
							<xsl:for-each select="library">
								<xsl:call-template name="addLibraryAssemblyData">
									<xsl:with-param name="addNoAptca" select="'true'" />
								</xsl:call-template>
							</xsl:for-each>
							<xsl:copy-of select="namespace"/>
							<xsl:copy-of select="type"/>
						</containers>
					</xsl:when>
					<xsl:when test="local-name(.)='memberdata'">
						<memberdata>
							<xsl:copy-of select="@*"/>
							<!-- if the member is overloaded, add @overload = id of overload topic, if any -->
							<xsl:choose>
								<!-- skip this processing for members that cannot be overloaded -->
								<xsl:when test="$subgroup='field'"/>
								<xsl:when test="$isEII='true'">
									<!-- get all the elements of this type -->
									<xsl:variable name="siblingElements" select="key('index',$typeId)/elements"/>
									<!-- get the reflection data for each element, e.g. parameters, apidata, etc. -->
									<xsl:variable name="siblingApiInfo" select="key('index',$siblingElements/element[not(apidata)]/@api) | $siblingElements/element[apidata]" />
									<!-- get the set of sibling elements that are overloads of the current member -->
									<xsl:variable name="overloadSet" select="
                                $siblingApiInfo[
                                  proceduredata/@eii='true' and implements/member/type/@api=$eiiTypeId and
                                  apidata[
                                    @name=$apidataName and 
                                    @subgroup=$subgroup and 
                                    (@subsubgroup=$subsubgroup or (not(@subsubgroup) and normalize-space($subsubgroup)=''))
                                  ]
                                ]" />
									<!-- When the reflection data includes multiple versions, $overloadSet may include multiple elements that have 
                       the same signature (same params and return type), which we call a signature set. 
                       For example, NetFx and CF apis that have the same signature but different ids. 
                       These are treated as a primary <element> node for the NetFx api, with a nested <element> node for CF api.
                  -->
									<xsl:variable name="signatureSet">
										<xsl:call-template name="GetSignatureSet">
											<xsl:with-param name="apidataName" select="$apidataName" />
											<xsl:with-param name="overloadSet" select="$overloadSet"/>
											<xsl:with-param name="typeId" select="$typeId"/>
										</xsl:call-template>
									</xsl:variable>
									<!-- it's an overload if there are multiple signature sets -->
									<xsl:if test="count(msxsl:node-set($signatureSet)/*) &gt; 1">
										<!-- the api is overloaded, so add @overload = idOfOverloadTopic -->
										<xsl:attribute name="overload">
											<xsl:call-template name="eiiOverloadId">
												<xsl:with-param name="typeId" select="$containingType" />
												<xsl:with-param name="idWithoutParams" select="$idWithoutParams"/>
												<xsl:with-param name="containingTypeName" select="substring($containingType,3)"/>
											</xsl:call-template>
										</xsl:attribute>
									</xsl:if>
								</xsl:when>
								<xsl:otherwise>
									<xsl:variable name="siblingElements" select="key('index',$typeId)/elements"/>
									<xsl:variable name="siblingApiInfo" select="key('index',$siblingElements/element[not(apidata)]/@api) | $siblingElements/element[apidata]" />
									<xsl:variable name="overloadSet" select="
                                $siblingApiInfo[
                                  not(proceduredata/@eii='true') and
                                  apidata[
                                    @name=$apidataName and 
                                    @subgroup=$subgroup and 
                                    (@subsubgroup=$subsubgroup or (not(@subsubgroup) and normalize-space($subsubgroup)=''))
                                  ]
                                ]" />
									<xsl:variable name="signatureSet">
										<xsl:call-template name="GetSignatureSet">
											<xsl:with-param name="apidataName" select="$apidataName" />
											<xsl:with-param name="overloadSet" select="$overloadSet"/>
											<xsl:with-param name="typeId" select="$typeId"/>
										</xsl:call-template>
									</xsl:variable>
									<xsl:if test="count(msxsl:node-set($signatureSet)/*) &gt; 1">
										<!-- the api is overloaded, so add @overload = idOfOverloadTopic -->
										<xsl:attribute name="overload">
											<xsl:call-template name="overloadId">
												<xsl:with-param name="typeId" select="$typeId"/>
												<xsl:with-param name="name" select="$apidataName"/>
												<xsl:with-param name="templatesTotal" select="$templatesTotal"/>
												<xsl:with-param name="subgroup" select="$subgroup"/>
												<xsl:with-param name="subsubgroup" select="$subsubgroup"/>
											</xsl:call-template>
										</xsl:attribute>
									</xsl:if>
								</xsl:otherwise>
							</xsl:choose>
							<!-- memberdata shouldn't have any children, but copy just in case -->
							<xsl:copy-of select="*"/>
						</memberdata>
					</xsl:when>
					<xsl:otherwise>
						<xsl:copy-of select="."/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:for-each>
		</api>
	</xsl:template>

	<!-- Special logic for type APIs -->

	<xsl:template name="UpdateTypeApiNode">
		<xsl:param name="derivedTypesTopicId" />
		<xsl:param name="allMembersTopicId" />
		<xsl:variable name="typeId" select="@id"/>
		<api>
			<xsl:copy-of select="@*"/>
			<xsl:choose>
				<xsl:when test="normalize-space($allMembersTopicId)">
					<topicdata group="api" allMembersTopicId="{$allMembersTopicId}"/>
				</xsl:when>
				<xsl:otherwise>
					<topicdata group="api" />
				</xsl:otherwise>
			</xsl:choose>

			<xsl:for-each select="*">
				<xsl:choose>
					<xsl:when test="self::elements">
						<xsl:choose>
							<xsl:when test="../apidata/@subgroup='enumeration'">
								<xsl:copy-of select="."/>
							</xsl:when>
							<xsl:when test="not(normalize-space($allMembersTopicId))">
								<xsl:if test="element">
									<xsl:call-template name="memberListElements">
										<xsl:with-param name="typeId" select="$typeId"/>
									</xsl:call-template>
								</xsl:if>
							</xsl:when>
						</xsl:choose>
					</xsl:when>
					<xsl:when test="local-name(.)='family' and $derivedTypesTopicId!=''">
						<family>
							<!-- copy the ancestors node -->
							<xsl:copy-of select="ancestors"/>
							<!-- Modify the descendents node -->
							<descendents>
								<xsl:attribute name="derivedTypes">
									<xsl:value-of select="$derivedTypesTopicId"/>
								</xsl:attribute>
								<type>
									<xsl:attribute name="api">
										<xsl:value-of select="$derivedTypesTopicId"/>
									</xsl:attribute>
									<xsl:attribute name="ref">
										<xsl:value-of select="'true'"/>
									</xsl:attribute>
								</type>
							</descendents>
						</family>
					</xsl:when>
					<xsl:when test="local-name(.)='containers'">
						<containers>
							<xsl:for-each select="library">
								<xsl:call-template name="addLibraryAssemblyData"/>
							</xsl:for-each>
							<xsl:copy-of select="namespace"/>
							<xsl:copy-of select="type"/>
						</containers>
					</xsl:when>
					<xsl:otherwise>
						<xsl:copy-of select="."/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:for-each>
		</api>
	</xsl:template>

	<xsl:template name="addLibraryAssemblyData">
		<xsl:param name="addNoAptca" />
		<xsl:variable name="assembly" select="@assembly"/>
		<library>
			<xsl:copy-of select="@*"/>
			<xsl:for-each select="/*/assemblies/assembly[@name=$assembly]">
				<assemblydata>
					<xsl:attribute name="version">
						<xsl:choose>
							<!-- If AssemblyInformationalVersionAttribute is present, use it alone -->
							<xsl:when test="attributes/attribute[type/@api='T:System.Reflection.AssemblyInformationalVersionAttribute']/argument/value">
								<xsl:value-of select="attributes/attribute[type/@api='T:System.Reflection.AssemblyInformationalVersionAttribute']/argument/value"/>
							</xsl:when>
							<!-- If AssemblyFileVersionAttribute is present, include its value.  Otherwise, just use the assembly version. -->
							<xsl:when test="attributes/attribute[type/@api='T:System.Reflection.AssemblyFileVersionAttribute']/argument/value">
								<xsl:value-of select="assemblydata/@version"/>
								<xsl:text> (</xsl:text>
								<xsl:value-of select="attributes/attribute[type/@api='T:System.Reflection.AssemblyFileVersionAttribute']/argument/value"/>
								<xsl:text>)</xsl:text>
							</xsl:when>
							<xsl:otherwise>
								<xsl:value-of select="assemblydata/@version"/>
							</xsl:otherwise>
						</xsl:choose>
					</xsl:attribute>
				</assemblydata>
			</xsl:for-each>
			<xsl:if test="$addNoAptca='true' and not(/*/assemblies/assembly[@name=$assembly]/attributes/attribute[type/@api='T:System.Security.AllowPartiallyTrustedCallersAttribute'])">
				<noAptca/>
			</xsl:if>
		</library>
	</xsl:template>

	<!-- Type logic; types get a lot of massaging to create member list pages, overload pages, etc. -->

	<xsl:template match="api[apidata/@group='type']">

		<xsl:variable name="typeId" select="@id" />

		<xsl:variable name="allMembersTopicId">
			<xsl:if test="$IncludeAllMembersTopic!='false' and (count(elements/*) &gt; 0) and apidata[not(@subgroup='enumeration')]">
				<xsl:value-of select="concat('AllMembers.', $typeId)"/>
			</xsl:if>
		</xsl:variable>

		<xsl:variable name="derivedTypesTopicId">
			<xsl:if test="count(family/descendents/*) &gt; $derivedTypesLimit">
				<xsl:value-of select="concat('DerivedTypes.', $typeId)"/>
			</xsl:if>
		</xsl:variable>

		<!-- a topic for the type overview -->
		<xsl:call-template name="UpdateTypeApiNode">
			<xsl:with-param name="derivedTypesTopicId" select="$derivedTypesTopicId" />
			<xsl:with-param name="allMembersTopicId" select="$allMembersTopicId" />
		</xsl:call-template>

		<!-- enumerations don't get all these extra topics -->
		<xsl:if test="not(apidata[@subgroup='enumeration'])">

			<!-- derived types topic -->
			<xsl:if test="$derivedTypesTopicId!=''">
				<api>
					<xsl:attribute name="id">
						<xsl:text>DerivedTypes.</xsl:text>
						<xsl:value-of select="$typeId"/>
					</xsl:attribute>
					<topicdata name="{apidata/@name}" group="list" subgroup="DerivedTypeList" typeTopicId="{@id}" allMembersTopicId="{$allMembersTopicId}" />
					<xsl:copy-of select="apidata" />
					<xsl:copy-of select="typedata" />
					<xsl:copy-of select="templates" />
					<elements>
						<xsl:for-each select="family/descendents/*">
							<element api="{@api}" />
						</xsl:for-each>
					</elements>
					<containers>
						<xsl:for-each select="containers/library">
							<xsl:call-template name="addLibraryAssemblyData"/>
						</xsl:for-each>
						<xsl:copy-of select="containers/namespace"/>
						<xsl:copy-of select="containers/type"/>
					</containers>
				</api>
			</xsl:if>

			<!-- all members topic -->
			<xsl:if test="$allMembersTopicId!=''">
				<api>
					<xsl:attribute name="id">
						<xsl:text>AllMembers.</xsl:text>
						<xsl:value-of select="$typeId"/>
					</xsl:attribute>
					<topicdata name="{apidata/@name}" group="list" subgroup="members" typeTopicId="{@id}" />
					<xsl:copy-of select="apidata" />
					<xsl:copy-of select="typedata" />
					<xsl:copy-of select="templates" />
					<!-- elements -->
					<xsl:for-each select="elements[element]">
						<xsl:call-template name="memberListElements">
							<xsl:with-param name="typeId" select="$typeId"/>
						</xsl:call-template>
					</xsl:for-each>
					<containers>
						<xsl:for-each select="containers/library">
							<xsl:call-template name="addLibraryAssemblyData"/>
						</xsl:for-each>
						<xsl:copy-of select="containers/namespace"/>
						<type api="{$typeId}"/>
					</containers>
				</api>
			</xsl:if>

			<!-- method/extension method list topic -->
			<!-- pass in $members so subsubgroup=operator IS excluded and subsubgroup=extension IS NOT excluded -->
			<xsl:call-template name="AddMemberlistAPI">
				<xsl:with-param name="subgroup">method</xsl:with-param>
				<xsl:with-param name="topicSubgroup">Methods</xsl:with-param>
				<xsl:with-param name="typeId" select="$typeId" />
				<xsl:with-param name="members" select="key('index',elements/element[not(apidata)]/@api)[apidata[@subgroup='method' and not(@subsubgroup='operator')]]
                  | elements/element[apidata[@subgroup='method' and not(@subsubgroup='operator')]]"/>
			</xsl:call-template>

			<!-- operator list topic -->
			<xsl:call-template name="AddMemberlistAPI">
				<xsl:with-param name="subsubgroup">operator</xsl:with-param>
				<xsl:with-param name="topicSubgroup">Operators</xsl:with-param>
				<xsl:with-param name="typeId" select="$typeId" />
			</xsl:call-template>

			<!-- property list topic -->
			<xsl:call-template name="AddMemberlistAPI">
				<xsl:with-param name="subgroup">property</xsl:with-param>
				<xsl:with-param name="topicSubgroup">Properties</xsl:with-param>
				<xsl:with-param name="typeId" select="$typeId" />
			</xsl:call-template>

			<!-- event list topic -->
			<xsl:call-template name="AddMemberlistAPI">
				<xsl:with-param name="subgroup">event</xsl:with-param>
				<xsl:with-param name="topicSubgroup">Events</xsl:with-param>
				<xsl:with-param name="typeId" select="$typeId" />
			</xsl:call-template>

			<!-- field list topic -->
			<xsl:call-template name="AddMemberlistAPI">
				<xsl:with-param name="subgroup">field</xsl:with-param>
				<xsl:with-param name="topicSubgroup">Fields</xsl:with-param>
				<xsl:with-param name="typeId" select="$typeId" />
			</xsl:call-template>

			<!-- attached properties topic -->
			<xsl:call-template name="AddMemberlistAPI">
				<xsl:with-param name="subsubgroup">attachedProperty</xsl:with-param>
				<xsl:with-param name="topicSubgroup">AttachedProperties</xsl:with-param>
				<xsl:with-param name="typeId" select="$typeId" />
			</xsl:call-template>

			<!-- attached events topic -->
			<xsl:call-template name="AddMemberlistAPI">
				<xsl:with-param name="subsubgroup">attachedEvent</xsl:with-param>
				<xsl:with-param name="topicSubgroup">AttachedEvents</xsl:with-param>
				<xsl:with-param name="typeId" select="$typeId" />
			</xsl:call-template>

			<!-- overload topics -->
			<xsl:call-template name="overloadTopics">
				<xsl:with-param name="allMembersTopicId" select="$allMembersTopicId"/>
			</xsl:call-template>

		</xsl:if>

	</xsl:template>

	<!--  -->
	<xsl:template name="RemoveParametersFromId">
		<xsl:variable name="memberId" select="@id | @api"/>
		<xsl:variable name="paramString" select="substring-after($memberId,'(')"/>
		<xsl:choose>
			<xsl:when test="boolean($paramString)">
				<xsl:value-of select="substring-before($memberId,'(')"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$memberId"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<!-- overload topics -->

	<xsl:template name="overloadTopics">
		<xsl:param name="allMembersTopicId"/>
		<xsl:variable name="typeId" select="@id"/>
		<xsl:variable name="members" select="key('index',elements/element[not(apidata)]/@api) | elements/element[apidata]" />
		<xsl:variable name="declaredPrefix" select="concat(substring($typeId,2), '.')"/>

		<xsl:for-each select="$members[not(@api=preceding-sibling::element/@api)]">
			<xsl:variable name="apidataName" select="apidata/@name"/>
			<xsl:variable name="templatesTotal" select="count(templates/*)"/>
			<xsl:variable name="templateParams" select="count(templates/template)"/>
			<xsl:variable name="templateArgs" select="count(templates/*[not(self::template)])"/>
			<!-- strip off any parameters from the end of the id -->
			<xsl:variable name="idWithoutParams">
				<xsl:call-template name="RemoveParametersFromId"/>
			</xsl:variable>
			<xsl:variable name="subgroup" select="apidata/@subgroup" />
			<xsl:variable name="subsubgroup" select="apidata/@subsubgroup" />
			<xsl:variable name="memberId" select="@id | @api"/>

			<xsl:choose>
				<!-- handle EII members -->
				<xsl:when test="proceduredata/@eii='true'">
					<xsl:variable name="eiiTypeId" select="implements/member/type/@api" />
					<!-- get the set of overloads: EII members with same name and subgroup -->
					<xsl:variable name="overloadSet" select="
                        $members[
                          proceduredata/@eii='true' and implements/member/type/@api=$eiiTypeId and 
                          apidata[
                            @name=$apidataName and 
                            @subgroup=$subgroup and 
                            (@subsubgroup=$subsubgroup or (not(@subsubgroup) and normalize-space($subsubgroup)=''))
                          ]
                        ]" />

					<!-- are there any declared members in the overload set? -->
					<xsl:variable name="declaredMembers" select="$overloadSet[starts-with(substring(@id,2),$declaredPrefix)]" />

					<!-- if more than one member in overloadSet, add an overload topic if necessary -->
					<xsl:if test="(count($overloadSet) &gt; 1) and $overloadSet[1][@id=$memberId or @api=$memberId]">
						<!-- When merging multiple versions, an overload set may have multiple members with the same signature,
               e.g. when one version inherits a member and another version overrides it.
               We want an overload topic only when there are multiple signatures. -->
						<!-- get the set of unique signatures for this overload set -->
						<xsl:variable name="signatureSet">
							<xsl:call-template name="GetSignatureSet">
								<xsl:with-param name="overloadSet" select="$overloadSet"/>
								<xsl:with-param name="typeId" select="$typeId"/>
							</xsl:call-template>
						</xsl:variable>
						<xsl:choose>
							<!-- don't need an overload topic if only one signature -->
							<xsl:when test="count(msxsl:node-set($signatureSet)/*) &lt; 2"/>
							<!-- don't need an overload topic if all overloads are inherited and config'd to omit overload topics when all are inherited -->
							<xsl:when test="(not(boolean($declaredMembers)) and $IncludeInheritedOverloadTopics='false')"/>
							<!-- otherwise, add an overload topic -->
							<xsl:otherwise>
								<api>
									<xsl:attribute name="id">
										<xsl:call-template name="eiiOverloadId">
											<xsl:with-param name="typeId" select="$typeId" />
											<xsl:with-param name="idWithoutParams" select="$idWithoutParams"/>
											<xsl:with-param name="containingTypeName" select="substring(containers/type/@api,3)"/>
										</xsl:call-template>
									</xsl:attribute>
									<topicdata name="{apidata/@name}" group="list" subgroup="overload" memberSubgroup="{$subgroup}"  pseudo="true" allMembersTopicId="{$allMembersTopicId}">
										<xsl:if test="not(boolean($declaredMembers))">
											<xsl:attribute name="allInherited">true</xsl:attribute>
											<xsl:attribute name="parentTopicId">
												<xsl:call-template name="eiiOverloadId">
													<xsl:with-param name="typeId" select="containers/type/@api" />
													<xsl:with-param name="idWithoutParams" select="$idWithoutParams"/>
													<xsl:with-param name="containingTypeName" select="substring(containers/type/@api,3)"/>
												</xsl:call-template>
											</xsl:attribute>
										</xsl:if>
									</topicdata>
									<xsl:copy-of select="apidata" />
									<!-- elements -->
									<elements>
										<xsl:for-each select="msxsl:node-set($signatureSet)/*">
											<xsl:copy-of select="."/>
										</xsl:for-each>
									</elements>
									<!-- containers -->
									<xsl:choose>
										<xsl:when test="boolean($declaredMembers)">
											<containers>
												<xsl:for-each select="$declaredMembers[1]/containers/library">
													<xsl:call-template name="addLibraryAssemblyData"/>
												</xsl:for-each>
												<xsl:copy-of select="$declaredMembers[1]/containers/namespace"/>
												<xsl:copy-of select="$declaredMembers[1]/containers/type"/>
											</containers>
										</xsl:when>
										<xsl:otherwise>
											<containers>
												<xsl:for-each select="key('index',$typeId)/containers/library">
													<xsl:call-template name="addLibraryAssemblyData"/>
												</xsl:for-each>
												<xsl:copy-of select="key('index',$typeId)/containers/namespace"/>
												<type api="{$typeId}"/>
											</containers>
										</xsl:otherwise>
									</xsl:choose>
								</api>
							</xsl:otherwise>
						</xsl:choose>
					</xsl:if>
				</xsl:when>

				<!-- handle non-EII members -->
				<xsl:otherwise>
					<!-- get the set of non-EII members with same name and subgroup -->
					<xsl:variable name="overloadSet" select="
                        $members[
                          not(proceduredata/@eii='true') and 
                          apidata[
                            @name=$apidataName and 
                            @subgroup=$subgroup and 
                            (@subsubgroup=$subsubgroup or (not(@subsubgroup) and normalize-space($subsubgroup)=''))
                          ]
                        ]" />

					<!-- are there any declared members in the overload set? -->
					<xsl:variable name="declaredMembers" select="$overloadSet[starts-with(substring(@id,2),$declaredPrefix)]" />

					<!-- if more than one member in overloadSet, add an overload topic if necessary -->
					<xsl:if test="(count($overloadSet) &gt; 1) and $overloadSet[1][@id=$memberId or @api=$memberId]">
						<!-- When merging multiple versions, an overload set may have multiple members with the same signature, 
               e.g. when one version inherits a member and another version overrides it. 
               We want an overload topic only when there are multiple signatures. -->
						<!-- get the set of unique signatures for this overload set -->
						<xsl:variable name="signatureSet">
							<xsl:call-template name="GetSignatureSet">
								<xsl:with-param name="overloadSet" select="$overloadSet"/>
								<xsl:with-param name="typeId" select="$typeId"/>
							</xsl:call-template>
						</xsl:variable>
						<xsl:choose>
							<!-- don't need an overload topic if only one signature -->
							<xsl:when test="count(msxsl:node-set($signatureSet)/*) &lt; 2"/>
							<!-- don't need an overload topic if all overloads are inherited and config'd to omit overload topics when all are inherited -->
							<xsl:when test="(not(boolean($declaredMembers)) and $IncludeInheritedOverloadTopics='false')"/>
							<!-- extension methods do not get overload topics -->
							<xsl:when test="$subsubgroup='extension'"/>
							<!-- otherwise, add an overload topic -->
							<xsl:otherwise>
								<api>
									<xsl:attribute name="id">
										<xsl:call-template name="overloadId">
											<xsl:with-param name="typeId" select="$typeId"/>
											<xsl:with-param name="name" select="$apidataName"/>
											<xsl:with-param name="templatesTotal" select="$templatesTotal"/>
											<xsl:with-param name="subgroup" select="$subgroup"/>
											<xsl:with-param name="subsubgroup" select="$subsubgroup"/>
										</xsl:call-template>
									</xsl:attribute>
									<topicdata name="{apidata/@name}" group="list" subgroup="overload" memberSubgroup="{$subgroup}"  pseudo="true" allMembersTopicId="{$allMembersTopicId}">
										<xsl:if test="not(boolean($declaredMembers))">
											<xsl:attribute name="allInherited">true</xsl:attribute>
											<xsl:attribute name="parentTopicId">
												<xsl:call-template name="overloadId">
													<xsl:with-param name="typeId" select="containers/type/@api"/>
													<xsl:with-param name="name" select="$apidataName"/>
													<xsl:with-param name="templatesTotal" select="$templatesTotal"/>
													<xsl:with-param name="subgroup" select="$subgroup"/>
													<xsl:with-param name="subsubgroup" select="$subsubgroup"/>
												</xsl:call-template>
											</xsl:attribute>
										</xsl:if>
									</topicdata>
									<xsl:copy-of select="apidata" />
									<!-- elements -->
									<elements>
										<xsl:for-each select="msxsl:node-set($signatureSet)/*">
											<xsl:copy-of select="."/>
										</xsl:for-each>
									</elements>
									<!-- containers -->
									<xsl:choose>
										<xsl:when test="boolean($declaredMembers)">
											<containers>
												<xsl:for-each select="$declaredMembers[1]/containers/library">
													<xsl:call-template name="addLibraryAssemblyData"/>
												</xsl:for-each>
												<xsl:copy-of select="$declaredMembers[1]/containers/namespace"/>
												<xsl:copy-of select="$declaredMembers[1]/containers/type"/>
											</containers>
										</xsl:when>
										<xsl:otherwise>
											<containers>
												<xsl:for-each select="key('index',$typeId)/containers/library">
													<xsl:call-template name="addLibraryAssemblyData"/>
												</xsl:for-each>
												<xsl:copy-of select="key('index',$typeId)/containers/namespace"/>
												<type api="{$typeId}"/>
											</containers>
										</xsl:otherwise>
									</xsl:choose>
								</api>
							</xsl:otherwise>
						</xsl:choose>
					</xsl:if>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:for-each>
	</xsl:template>

	<xsl:template name="WriteElementNode">
		<xsl:param name="typeId"/>
		<xsl:choose>
			<xsl:when test="local-name()='element'">
				<xsl:copy-of select="."/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:variable name="elementId" select="@id"/>
				<xsl:for-each select="$root">
					<xsl:copy-of select="key('index', $typeId)/elements/element[@api=$elementId]"/>
				</xsl:for-each>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="GetSignatureSet">
		<xsl:param name="apidataName" select="apidata/@name" />
		<xsl:param name="overloadSet"/>
		<xsl:param name="typeId"/>

		<xsl:choose>
			<xsl:when test="count($overloadSet) = 1">
				<xsl:for-each select="$overloadSet">
					<xsl:call-template name="WriteElementNode">
						<xsl:with-param name="typeId" select="$typeId"/>
					</xsl:call-template>
				</xsl:for-each>
			</xsl:when>
			<xsl:otherwise>
				<xsl:for-each select="$overloadSet">
					<xsl:sort select="count(parameters/parameter)" data-type="number"/>
					<xsl:sort select="key('index', parameters/parameter[1]//type[1]/@api)/apidata/@name"/>
					<xsl:variable name="memberId" select="@id | @api"/>
					<xsl:variable name="signature">
						<xsl:call-template name="GetSignature">
							<xsl:with-param name="apidataName" select="$apidataName"/>
						</xsl:call-template>
					</xsl:variable>
					<xsl:variable name="sameParamSignatureSet" select="$overloadSet[contains(@id|@api,$signature) and string-length(substring-after(@id|@api,$signature))=0]"/>
					<!-- make sure all elements in the sameParamSignatureSet have the same return value -->
					<xsl:variable name="returnsType" select="string(returns//type[1]/@api)"/>
					<xsl:variable name="sameSignatureSetAllVersions" select="$sameParamSignatureSet[(returns//type[1][@api=$returnsType]) or ($returnsType='' and not(returns//type))]"/>
					<xsl:variable name="sameSignatureSet" >
						<xsl:call-template name="removeElementsOfSameVersion">
							<xsl:with-param name="sameSignatureSetAllVersions" select="$sameSignatureSetAllVersions" />
							<xsl:with-param name="typeId" select="$typeId" />
						</xsl:call-template>
					</xsl:variable>
					<xsl:choose>
						<xsl:when test="count(msxsl:node-set($sameSignatureSet)/*) &gt; 1">
							<xsl:if test="msxsl:node-set($sameSignatureSet)/*[1][@id=$memberId or @api=$memberId]">
								<!-- $sameSignatureSet is set of api nodes; for version check we need the corresponding set of element nodes -->
								<xsl:variable name="elementSet">
									<xsl:for-each select="msxsl:node-set($sameSignatureSet)/*">
										<xsl:call-template name="WriteElementNode">
											<xsl:with-param name="typeId" select="$typeId"/>
										</xsl:call-template>
									</xsl:for-each>
								</xsl:variable>
								<!-- The first versions/versions node determines the primary version group, e.g. 'netfw'. -->
								<xsl:variable name="primaryVersionGroup" select="versions/versions[1]/@name"/>
								<!-- The primary element is the one with the most recent version for the primary version group. -->
								<xsl:variable name="primaryVersionMemberId">
									<xsl:call-template name="GetSignatureWithLatestVersion">
										<xsl:with-param name="versionGroup" select="$primaryVersionGroup"/>
										<xsl:with-param name="signatureset" select="$elementSet"/>
									</xsl:call-template>
								</xsl:variable>
								<xsl:variable name="primaryMemberId">
									<xsl:choose>
										<xsl:when test="normalize-space($primaryVersionMemberId)!=''">
											<xsl:value-of select="normalize-space(substring-before($primaryVersionMemberId,';'))"/>
										</xsl:when>
										<xsl:otherwise>
											<xsl:value-of select="msxsl:node-set($sameSignatureSet)/*[1]/@id|@api"/>
										</xsl:otherwise>
									</xsl:choose>
								</xsl:variable>
								<xsl:for-each select="msxsl:node-set($sameSignatureSet)/*[@id=$primaryMemberId or @api=$primaryMemberId]">
									<xsl:variable name="nonPrimaryVersionElementSet">
										<xsl:call-template name="nonPrimaryVersionElements">
											<xsl:with-param name="usedIds" select="concat($primaryMemberId,';')"/>
											<xsl:with-param name="elementSet" select="$elementSet"/>
											<xsl:with-param name="sameSignatureSet" select="$sameSignatureSet"/>
											<xsl:with-param name="typeId" select="$typeId"/>
										</xsl:call-template>
									</xsl:variable>
									<element api="{$primaryMemberId}">
										<xsl:if test="count(msxsl:node-set($nonPrimaryVersionElementSet)/*) &gt; 0">
											<xsl:attribute name="signatureset"/>
										</xsl:if>
										<!-- copy attributes and inner xml from the original element node -->
										<xsl:choose>
											<xsl:when test="local-name()='element'">
												<xsl:copy-of select="@*"/>
												<xsl:copy-of select="*"/>
											</xsl:when>
											<xsl:otherwise>
												<xsl:for-each select="$root">
													<xsl:copy-of select="key('index', $typeId)/elements/element[@api=$primaryMemberId]/@*"/>
												</xsl:for-each>
											</xsl:otherwise>
										</xsl:choose>
										<!-- for the secondary version groups, copy in the signature set's latest member (if different from primary member) -->
										<xsl:copy-of select="msxsl:node-set($nonPrimaryVersionElementSet)/*"/>
									</element>
								</xsl:for-each>
							</xsl:if>
						</xsl:when>
						<xsl:otherwise>
							<xsl:call-template name="WriteElementNode">
								<xsl:with-param name="typeId" select="$typeId"/>
							</xsl:call-template>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:for-each>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="removeElementsOfSameVersion">
		<xsl:param name="sameSignatureSetAllVersions" />
		<xsl:param name="typeId" />
		<xsl:variable name="elementSet">
			<xsl:for-each select="$sameSignatureSetAllVersions">
				<xsl:call-template name="WriteElementNode">
					<xsl:with-param name="typeId" select="$typeId"/>
				</xsl:call-template>
			</xsl:for-each>
		</xsl:variable>
		<xsl:variable name="firstElementId" select="$sameSignatureSetAllVersions[1]/@id" />
		<xsl:variable name ="versionValue" select="msxsl:node-set($elementSet)//element[@api=$firstElementId]/@*[2]"/>
		<xsl:for-each select="msxsl:node-set($elementSet)/*" >
			<xsl:variable name="elementId" select="@api" />
			<xsl:if test="@api=$firstElementId or @*[2]!=$versionValue">
				<xsl:copy-of select="$sameSignatureSetAllVersions[@id=$elementId]"/>
			</xsl:if>
		</xsl:for-each>
	</xsl:template>

	<xsl:template name="nonPrimaryVersionElements">
		<xsl:param name="usedIds"/>
		<xsl:param name="elementSet"/>
		<xsl:param name="sameSignatureSet"/>
		<xsl:param name="typeId"/>

		<xsl:if test="count(versions/versions)&gt;1">
			<xsl:variable name="versionGroupId2" select="versions/versions[2]/@name"/>
			<xsl:variable name="versionMemberIdSet2">
				<xsl:call-template name="GetSignatureWithLatestVersion">
					<xsl:with-param name="versionGroup" select="$versionGroupId2"/>
					<xsl:with-param name="signatureset" select="$elementSet"/>
				</xsl:call-template>
			</xsl:variable>
			<xsl:variable name="versionMemberId2">
				<xsl:value-of select="normalize-space(substring-before($versionMemberIdSet2,';'))"/>
			</xsl:variable>
			<xsl:if test="(normalize-space($versionMemberId2)!='') and not(contains($usedIds,concat($versionMemberId2,';')))">
				<xsl:for-each select="msxsl:node-set($sameSignatureSet)/*[@id=$versionMemberId2 or @api=$versionMemberId2]">
					<xsl:call-template name="WriteElementNode">
						<xsl:with-param name="typeId" select="$typeId"/>
					</xsl:call-template>
				</xsl:for-each>
			</xsl:if>

			<xsl:if test="count(versions/versions)&gt;2">
				<xsl:variable name="usedIds2" select="concat($usedIds,$versionMemberId2,';')"/>
				<xsl:variable name="versionGroupId3" select="versions/versions[3]/@name"/>
				<xsl:variable name="versionMemberIdSet3">
					<xsl:call-template name="GetSignatureWithLatestVersion">
						<xsl:with-param name="versionGroup" select="$versionGroupId3"/>
						<xsl:with-param name="signatureset" select="$elementSet"/>
					</xsl:call-template>
				</xsl:variable>
				<xsl:variable name="versionMemberId3">
					<xsl:value-of select="normalize-space(substring-before($versionMemberIdSet3,';'))"/>
				</xsl:variable>
				<xsl:if test="(normalize-space($versionMemberId3)!='') and not(contains($usedIds2,concat($versionMemberId3,';')))">
					<xsl:for-each select="msxsl:node-set($sameSignatureSet)/*[@id=$versionMemberId3 or @api=$versionMemberId3]">
						<xsl:call-template name="WriteElementNode">
							<xsl:with-param name="typeId" select="$typeId"/>
						</xsl:call-template>
					</xsl:for-each>
				</xsl:if>
			</xsl:if>
		</xsl:if>
	</xsl:template>

	<xsl:template name="GetSignatureWithLatestVersion">
		<xsl:param name="versionGroup"/>
		<xsl:param name="signatureset"/>
		<xsl:for-each select="msxsl:node-set($signatureset)/*[@*[local-name()=$versionGroup]]">
			<xsl:variable name="currVersion" select="@*[local-name()=$versionGroup]"/>
			<xsl:variable name="isLatest">
				<xsl:call-template name="IsLatestVersion">
					<xsl:with-param name="version" select="$currVersion"/>
					<xsl:with-param name="versionGroup" select="$versionGroup"/>
					<xsl:with-param name="signatureset" select="$signatureset"/>
				</xsl:call-template>
			</xsl:variable>
			<!-- IsLatestVersion returns '' if this is the latest version number -->
			<xsl:if test="normalize-space($isLatest)=''">
				<xsl:value-of select="@api"/>
				<xsl:text>;</xsl:text>
			</xsl:if>
		</xsl:for-each>
	</xsl:template>

	<xsl:template name="IsLatestVersion">
		<xsl:param name="version"/>
		<xsl:param name="versionGroup"/>
		<xsl:param name="signatureset"/>
		<!-- loop through the versions; output is '' if there are no lower versions -->
		<xsl:for-each select="msxsl:node-set($signatureset)/*[@*[local-name()=$versionGroup]]">
			<xsl:variable name="currVersion" select="@*[local-name()=$versionGroup]"/>
			<xsl:if test="ms:string-compare($currVersion, $version) = 1">false</xsl:if>
		</xsl:for-each>
	</xsl:template>

	<!--  -->
	<xsl:template name="GetSignature">
		<xsl:param name="apidataName" />
		<xsl:param name="memberId" select="@id | @api"/>
		<xsl:variable name="paramString" select="substring-after($memberId,'(')"/>
		<xsl:variable name="tickString" select="substring-after($memberId,'``')"/>
		<xsl:variable name="memberName">
			<xsl:choose>
				<xsl:when test="$apidataName='.ctor' or $apidataName='.cctor'">ctor</xsl:when>
				<!-- for explicit interface implementation members, return the member name with # instead of ., so it matches cref ids -->
				<xsl:when test="memberdata[@visibility='private'] and proceduredata[@virtual = 'true']">
					<xsl:value-of select="translate($apidataName,'.','#')"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="$apidataName"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:choose>
			<xsl:when test="string-length($tickString) &gt; string-length($paramString)">
				<xsl:value-of select="concat('.',$memberName,'``',$tickString)"/>
			</xsl:when>
			<xsl:when test="boolean($paramString)">
				<xsl:value-of select="concat('.',$memberName,'(',$paramString)"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="concat('.',$memberName)"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<!-- member list topics -->

	<xsl:template name="AddMemberlistAPI">
		<xsl:param name="subgroup"/>
		<xsl:param name="subsubgroup"/>
		<xsl:param name="topicSubgroup"/>
		<xsl:param name="typeId" />
		<!-- get all the type's members for this subgroup -->
		<xsl:param name="members" select="key('index',elements/element[not(apidata)]/@api)[apidata[($subgroup='' and @subsubgroup=$subsubgroup) or ($subsubgroup='' and not(@subsubgroup) and @subgroup=$subgroup)]]
                  | elements/element[apidata[($subgroup='' and @subsubgroup=$subsubgroup) or ($subsubgroup='' and not(@subsubgroup) and @subgroup=$subgroup)]]"/>

		<!-- Fix for bug:365255, add a member list topic for all the type's members-->
		<xsl:if test="count($members) &gt; 0">
			<api>
				<xsl:attribute name="id">
					<xsl:value-of select="concat($topicSubgroup, '.', $typeId)"/>
				</xsl:attribute>
				<topicdata name="{apidata/@name}" group="list" subgroup="{$topicSubgroup}">
					<xsl:if test="boolean($subsubgroup)">
						<xsl:attribute name="subsubgroup">
							<xsl:value-of select="$topicSubgroup"/>
						</xsl:attribute>
					</xsl:if>
					<xsl:attribute name="typeTopicId">
						<xsl:value-of select="$typeId"/>
					</xsl:attribute>
				</topicdata>
				<xsl:copy-of select="apidata" />
				<xsl:copy-of select="typedata" />
				<xsl:copy-of select="templates" />
				<!-- elements -->
				<xsl:call-template name="memberListElements">
					<xsl:with-param name="members" select="$members"/>
					<xsl:with-param name="typeId" select="$typeId"/>
				</xsl:call-template>
				<containers>
					<xsl:for-each select="containers/library">
						<xsl:call-template name="addLibraryAssemblyData"/>
					</xsl:for-each>
					<xsl:copy-of select="containers/namespace"/>
					<type api="{$typeId}"/>
				</containers>
			</api>
		</xsl:if>
	</xsl:template>

	<xsl:template name="memberListElements">
		<xsl:param name="members" select="key('index',element[not(apidata)]/@api) | element[apidata]" />
		<xsl:param name="typeId" />
		<xsl:variable name="declaredPrefix" select="concat(substring($typeId,2), '.')"/>

		<elements>
			<xsl:for-each select="$members[not(@api=preceding-sibling::element/@api)]">
				<xsl:variable name="apidataName" select="apidata/@name"/>
				<xsl:variable name="templatesTotal" select="count(templates/*)"/>
				<xsl:variable name="templateParams" select="count(templates/template)"/>
				<xsl:variable name="templateArgs" select="count(templates/*[not(self::template)])"/>
				<!-- strip off any parameters from the end of the id -->
				<xsl:variable name="idWithoutParams">
					<xsl:call-template name="RemoveParametersFromId"/>
				</xsl:variable>
				<xsl:variable name="subgroup" select="apidata/@subgroup" />
				<xsl:variable name="subsubgroup" select="apidata/@subsubgroup" />
				<xsl:variable name="memberId" select="@id | @api"/>

				<xsl:choose>
					<!-- handle EII members -->
					<xsl:when test="proceduredata/@eii='true'">
						<xsl:variable name="eiiTypeId" select="implements/member/type/@api"/>
						<!-- get the set of overloads: EII members with same name and subgroup -->
						<xsl:variable name="overloadSet" select="
                          $members[
                            proceduredata/@eii='true' and implements/member/type/@api=$eiiTypeId and
                            apidata[
                              @name=$apidataName and
                              @subgroup=$subgroup and
                              (@subsubgroup=$subsubgroup or (not(@subsubgroup) and normalize-space($subsubgroup)=''))
                            ]
                          ]" />

						<!-- are there any declared members in the overload set? -->
						<xsl:variable name="declaredMembers" select="$overloadSet[starts-with(substring(@id,2),$declaredPrefix)]" />

						<xsl:variable name="signatureSet">
							<xsl:call-template name="GetSignatureSet">
								<xsl:with-param name="overloadSet" select="$overloadSet"/>
								<xsl:with-param name="typeId" select="$typeId"/>
							</xsl:call-template>
						</xsl:variable>

						<!-- make sure we add to the list only once -->
						<xsl:if test="$overloadSet[1][@id=$memberId or @api=$memberId]">
							<!-- When merging multiple versions, an overload set may have multiple members with the same signature,
                         e.g. when one version inherits a member and another version overrides it.
                         We want an overload topic only when there are multiple signatures. -->
							<!-- get the set of unique signatures for this overload set -->
							<xsl:choose>
								<!-- don't need an overload topic if only one signature -->
								<xsl:when test="count(msxsl:node-set($signatureSet)/*) = 1">
									<xsl:copy-of select="msxsl:node-set($signatureSet)/*"/>
								</xsl:when>
								<!-- just copy the elements if all overloads are inherited and config'd to omit overload topics when all are inherited -->
								<!-- !EFW - Do generate overloads in the member and method list pages so that the parameters show up -->
								<!--                <xsl:when test="(not(boolean($declaredMembers)) and $IncludeInheritedOverloadTopics='false')">
                  <xsl:copy-of select="msxsl:node-set($signatureSet)/*"/>
                </xsl:when> -->
								<xsl:otherwise>
									<element>
										<xsl:attribute name="api">
											<xsl:call-template name="eiiOverloadId">
												<xsl:with-param name="typeId" select="$typeId" />
												<xsl:with-param name="idWithoutParams" select="$idWithoutParams"/>
												<xsl:with-param name="containingTypeName" select="substring(containers/type/@api,3)"/>
											</xsl:call-template>
										</xsl:attribute>
										<xsl:copy-of select="msxsl:node-set($signatureSet)/*"/>
									</element>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:if>
					</xsl:when>

					<!-- field members cannot be overloaded, so skip the overload logic and just write the element node -->
					<xsl:when test="$subgroup='field'">
						<xsl:call-template name="WriteElementNode">
							<xsl:with-param name="typeId" select="$typeId"/>
						</xsl:call-template>
					</xsl:when>

					<!-- for other non-EII members, handle overloads and signature sets -->
					<xsl:otherwise>
						<!-- get the set of overloads: non-EII members with same name and subgroup -->
						<xsl:variable name="overloadSet" select="
                          $members[
                            not(string(proceduredata/@eii)='true') and 
                            apidata[
                              @name=$apidataName and 
                              @subgroup=$subgroup and 
                              (@subsubgroup=$subsubgroup or (not(@subsubgroup) and normalize-space($subsubgroup)=''))
                            ]
                          ]"/>

						<!-- are there any declared members in the overload set? -->
						<xsl:variable name="declaredMembers" select="$overloadSet[starts-with(substring(@id,2),$declaredPrefix)]" />

						<xsl:variable name="signatureSet">
							<xsl:call-template name="GetSignatureSet">
								<xsl:with-param name="overloadSet" select="$overloadSet"/>
								<xsl:with-param name="typeId" select="$typeId"/>
							</xsl:call-template>
						</xsl:variable>

						<!-- make sure we add to the list only once -->
						<xsl:if test="$overloadSet[1][@id=$memberId or @api=$memberId]">
							<!-- When merging multiple versions, an overload set may have multiple members with the same signature,
                         e.g. when one version inherits a member and another version overrides it.
                         We want an overload topic only when there are multiple signatures. -->
							<!-- get the set of unique signatures for this overload set -->
							<xsl:choose>
								<!-- don't need an overload topic if only one signature -->
								<xsl:when test="count(msxsl:node-set($signatureSet)/*) = 1">
									<xsl:copy-of select="msxsl:node-set($signatureSet)/*"/>
								</xsl:when>
								<!-- just copy the elements if all overloads are inherited and config'd to omit overload topics when all are inherited -->
								<!-- !EFW - Do generate overloads in the member and method list pages so that the parameters show up -->
								<!--                <xsl:when test="(not(boolean($declaredMembers)) and $IncludeInheritedOverloadTopics='false')">
                  <xsl:copy-of select="msxsl:node-set($signatureSet)/*"/>
                </xsl:when> -->
								<!-- no overload topics for extension methods -->
								<!-- but we need to mark the elements as overloaded (so the member list link shows parameters) -->
								<xsl:when test="$subsubgroup='extension'">
									<xsl:for-each select="msxsl:node-set($signatureSet)/*">
										<element>
											<xsl:copy-of select="@*"/>
											<xsl:attribute name="overload">true</xsl:attribute>
											<xsl:copy-of select="*"/>
										</element>
									</xsl:for-each>
								</xsl:when>
								<xsl:otherwise>
									<element>
										<xsl:attribute name="api">
											<xsl:call-template name="overloadId">
												<xsl:with-param name="typeId" select="$typeId"/>
												<xsl:with-param name="name" select="$apidataName"/>
												<xsl:with-param name="templatesTotal" select="$templatesTotal"/>
												<xsl:with-param name="subgroup" select="$subgroup"/>
												<xsl:with-param name="subsubgroup" select="$subsubgroup"/>
											</xsl:call-template>
										</xsl:attribute>
										<xsl:copy-of select="msxsl:node-set($signatureSet)/*"/>
									</element>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:if>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:for-each>
		</elements>
	</xsl:template>

	<xsl:template name="overloadId">
		<xsl:param name="typeId"/>
		<xsl:param name="name"/>
		<xsl:param name="templatesTotal"/>
		<xsl:param name="subgroup"/>
		<xsl:param name="subsubgroup"/>
		<xsl:choose>
			<xsl:when test="$subgroup='constructor'">
				<xsl:value-of select="concat('Overload:',substring($typeId,3),'.#ctor')"/>
			</xsl:when>
			<xsl:when test="$subsubgroup='operator'">
				<xsl:value-of select="concat('Overload:',substring($typeId,3),'.op_',$name)"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="concat('Overload:',substring($typeId,3),'.',$name)"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="eiiOverloadId">
		<xsl:param name="typeId" />
		<xsl:param name="idWithoutParams" />
		<xsl:param name="containingTypeName" />
		<xsl:variable name="idWithoutTicks">
			<xsl:choose>
				<xsl:when test="contains($idWithoutParams, '``')">
					<xsl:value-of select="substring-before($idWithoutParams, '``')"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="$idWithoutParams"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:value-of select="concat('Overload:',substring($typeId,3),substring-after($idWithoutTicks,$containingTypeName))"/>
	</xsl:template>

	<xsl:template name="projectTopic">
		<api id="R:{$project}">
			<topicdata group="root" />
			<elements>
				<xsl:for-each select="/*/apis/api[apidata/@group='namespace']">
					<element api="{@id}" />
				</xsl:for-each>
			</elements>
		</api>
	</xsl:template>

</xsl:stylesheet>
