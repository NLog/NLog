<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0">

	<xsl:param name="projectName" />
	<xsl:param name="maxProjectNameLength" select="499" />
	<!-- Default provided by paorear, bug 230840, changed due to 767672 -->
	<xsl:variable name="leftLength" select="$maxProjectNameLength div 2 - 1" />
	<xsl:variable name="rightLength" select="$maxProjectNameLength - $leftLength - 2" />
	<xsl:variable name="projectPrefix">
		<xsl:if test="boolean($projectName)">
			<xsl:value-of select="concat($projectName,'_')"/>
		</xsl:if>
	</xsl:variable>

	<xsl:output indent="yes" encoding="UTF-8" />

	<xsl:key name="index" match="/reflection/apis/api" use="@id" />

	<xsl:template match="/">
		<topics>
			<xsl:choose>
				<xsl:when test="count(/reflection/apis/api[apidata/@group='root']) > 0">
					<xsl:apply-templates select="/reflection/apis/api[apidata/@group='root']" />
				</xsl:when>
				<xsl:when test="count(/reflection/apis/api[topicdata/@group='root']) > 0">
					<xsl:apply-templates select="/reflection/apis/api[topicdata/@group='root']" />
				</xsl:when>
				<xsl:when test="count(/reflection/apis/api[topicdata/@group='rootGroup']) > 0">
					<xsl:apply-templates select="/reflection/apis/api[topicdata/@group='rootGroup']" />
				</xsl:when>
				<xsl:when test="count(/reflection/apis/api[apidata/@group='namespace']) > 0">
					<xsl:apply-templates select="/reflection/apis/api[apidata/@group='namespace']">
						<xsl:sort select="apidata/@name" />
					</xsl:apply-templates>
				</xsl:when>
				<xsl:when test="count(/reflection/apis/api[apidata/@group='type']) > 0">
					<xsl:apply-templates select="/reflection/apis/api[apidata/@group='type'][topicdata[@group='api']]">
						<xsl:sort select="@id" />
					</xsl:apply-templates>
				</xsl:when>
				<xsl:otherwise>
					<xsl:apply-templates select="/reflection/apis/api[apidata/@group='member']">
						<xsl:sort select="@id" />
					</xsl:apply-templates>
				</xsl:otherwise>
			</xsl:choose>
		</topics>
	</xsl:template>

	<!-- Create a root entry and namespace sub-entries -->
	<xsl:template match="api[apidata/@group='root'] | api[topicdata/@group='root']">
		<topic id="{@id}" project="{$projectName}" file="{file/@name}">
			<xsl:apply-templates select="key('index',elements/element/@api)">
				<xsl:sort select="apidata/@name" />
			</xsl:apply-templates>
		</topic>
	</xsl:template>

	<!-- Create a list of namespace and namespace group entries without a root container -->
	<xsl:template match="api[topicdata/@group='rootGroup']">
		<xsl:apply-templates select="key('index',elements/element/@api)">
			<xsl:sort select="apidata/@name" />
		</xsl:apply-templates>
	</xsl:template>

	<!-- Create a root entry and namespace sub-entries -->
	<xsl:template match="api[apidata/@group='namespaceGroup']">
		<topic id="{@id}" project="{$projectName}_NamespaceGroup" file="{file/@name}">
			<xsl:apply-templates select="key('index',elements/element/@api)">
				<xsl:sort select="apidata/@name" />
			</xsl:apply-templates>
		</topic>
	</xsl:template>

	<!-- For each namespace, create namespace entry and type sub-entries -->
	<xsl:template match="api[apidata/@group='namespace']">
		<topic id="{@id}" project="{$projectName}_Namespaces" file="{file/@name}">
			<xsl:apply-templates select="key('index',elements/element/@api)">
				<xsl:sort select="@id" />
			</xsl:apply-templates>
		</topic>
	</xsl:template>

	<!-- Logic to shorten component names if needed -->
	<xsl:template name="GetComponentName">
		<xsl:param name="initialName" select="containers/library/@assembly" />
		<xsl:variable name="componentNameLength" select="string-length($initialName)" />
		<xsl:choose>
			<xsl:when test="$componentNameLength >= $maxProjectNameLength">
				<xsl:variable name="left" select="substring($initialName, 1, $leftLength)" />
				<xsl:variable name="right" select="substring($initialName, $componentNameLength - $rightLength)" />
				<xsl:value-of select="concat($left,'_',$right)" />
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$initialName" />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<!-- For each type, create type entry and either overload entries or member entries as sub-entries -->
	<xsl:template match="api[apidata/@group='type'][topicdata[@group='api']]">
		<xsl:variable name="componentName">
			<xsl:call-template name="GetComponentName">
				<xsl:with-param name="initialName" select="concat($projectPrefix,containers/library/@assembly)" />
			</xsl:call-template>
		</xsl:variable>
		<topic id="{@id}" project="{$componentName}" file="{file/@name}">
			<xsl:call-template name="AddMemberListTopics"/>
		</topic>
	</xsl:template>

	<!-- For class, struct, and interface, insert nodes for the member list topics,
			 and insert nodes for the declared member topics under the appropriate list topic. -->
	<xsl:template name="AddMemberListTopics">
		<xsl:variable name="typeId" select="@id" />
		<xsl:variable name="declaredPrefix" select="concat(substring($typeId,3), '.')"/>
		<xsl:variable name="componentName">
			<xsl:call-template name="GetComponentName">
				<xsl:with-param name="initialName" select="concat($projectPrefix,containers/library/@assembly)" />
			</xsl:call-template>
		</xsl:variable>

		<xsl:if test="apidata[@subgroup='class' or @subgroup='structure' or @subgroup='interface']">
			<!-- Insert the all members topic, if present -->
			<xsl:for-each select="key('index', topicdata/@allMembersTopicId)">
				<topic id="{@id}" project="{$componentName}" file="{file/@name}"/>
			</xsl:for-each>

			<!-- Insert constructors -->
			<!-- The context now is the type's API node, which has an element list in vsorcas doc model, but not in vs2005 -->
			<xsl:choose>
				<xsl:when test="topicdata/@allMembersTopicId">
					<xsl:for-each select="key('index', key('index', topicdata/@allMembersTopicId)/elements/*[starts-with(substring-after(@api,':'), $declaredPrefix)]/@api)[apidata[@subgroup='constructor']]">
						<xsl:call-template name="AddMember">
							<xsl:with-param name="declaredPrefix" select="$declaredPrefix"/>
						</xsl:call-template>
					</xsl:for-each>
				</xsl:when>
				<xsl:otherwise>
					<xsl:for-each select="key('index', elements/*[starts-with(substring-after(@api,':'), $declaredPrefix)]/@api)[apidata[@subgroup='constructor']]">
						<xsl:call-template name="AddMember">
							<xsl:with-param name="declaredPrefix" select="$declaredPrefix"/>
						</xsl:call-template>
					</xsl:for-each>
				</xsl:otherwise>
			</xsl:choose>

			<!-- Insert the Properties topic, if present -->
			<xsl:for-each select="key('index', concat('Properties.', $typeId))">
				<xsl:call-template name="AddMemberListTree"/>
			</xsl:for-each>

			<!-- Insert the Methods topic, if present -->
			<xsl:for-each select="key('index', concat('Methods.', $typeId))">
				<xsl:call-template name="AddMemberListTree"/>
			</xsl:for-each>

			<!-- Insert the Events topic, if present -->
			<xsl:for-each select="key('index', concat('Events.', $typeId))">
				<xsl:call-template name="AddMemberListTree"/>
			</xsl:for-each>

			<!-- Insert the Operators topic, if present -->
			<xsl:for-each select="key('index', concat('Operators.', $typeId))">
				<xsl:call-template name="AddMemberListTree"/>
			</xsl:for-each>

			<!-- Insert the Fields topic, if present -->
			<xsl:for-each select="key('index', concat('Fields.', $typeId))">
				<xsl:call-template name="AddMemberListTree"/>
			</xsl:for-each>

			<!-- Insert the AttachedProperties topic, if present -->
			<xsl:for-each select="key('index', concat('AttachedProperties.', $typeId))">
				<xsl:call-template name="AddMemberListTree"/>
			</xsl:for-each>

			<!-- Insert the AttachedEvents topic, if present -->
			<xsl:for-each select="key('index', concat('AttachedEvents.', $typeId))">
				<xsl:call-template name="AddMemberListTree"/>
			</xsl:for-each>

		</xsl:if>
	</xsl:template>

	<xsl:template name="AddMemberListTree">
		<xsl:variable name="componentName">
			<xsl:call-template name="GetComponentName">
				<xsl:with-param name="initialName" select="concat($projectPrefix,containers/library/@assembly)" />
			</xsl:call-template>
		</xsl:variable>
		<xsl:variable name="declaredPrefix" select="concat(substring(topicdata/@typeTopicId,3), '.')"/>
		<topic id="{@id}" project="{$componentName}" file="{file/@name}">
			<!-- Recurse to get declared child element topics, if any -->
			<xsl:for-each select="key('index', elements/*[starts-with(substring-after(@api,':'), $declaredPrefix)]/@api)">
				<!-- Sort the elements in a member list topic by name -->
				<xsl:sort select="topicdata/@eiiName | apidata/@name" />
				<xsl:call-template name="AddMember">
					<xsl:with-param name="declaredPrefix" select="$declaredPrefix"/>
				</xsl:call-template>
			</xsl:for-each>
		</topic>
	</xsl:template>

	<xsl:template name="AddMember">
		<xsl:param name="declaredPrefix" />
		<xsl:variable name="componentName">
			<xsl:call-template name="GetComponentName">
				<xsl:with-param name="initialName" select="concat($projectPrefix,containers/library/@assembly)" />
			</xsl:call-template>
		</xsl:variable>
		<topic id="{@id}" project="{$componentName}" file="{file/@name}">
			<!-- Loop through the declared elements, if any, which are already pre-sorted by the ApplyVsDocModel
					 transform; if you were to loop through the key('index', elements) as in the AddMemberListTree
					 template, you'd lose the pre-sort and get the order of APIs in the document
			-->
			<xsl:for-each select="elements/*[starts-with(substring-after(@api,':'), $declaredPrefix)]">
				<xsl:for-each select="key('index',@api)">
					<xsl:call-template name="AddMember">
						<xsl:with-param name="declaredPrefix" select="$declaredPrefix"/>
					</xsl:call-template>
				</xsl:for-each>
			</xsl:for-each>
		</topic>
	</xsl:template>

	<!-- If only members are present, create TOC entries for them (this should never be a visible TOC) -->
	<xsl:template match="api[apidata/@group='member']">
		<topic id="{@id}" project="{$projectName}" file="{file/@name}" />
	</xsl:template>

</xsl:stylesheet>
