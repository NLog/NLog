<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.1"
		xmlns:MSHelp="http://msdn.microsoft.com/mshelp" >

  <xsl:import href="../../shared/transforms/utilities_reference.xsl"/>

  <xsl:output method="xml" omit-xml-declaration="yes" encoding="utf-8" />
  
	<!-- key parameter is the api identifier string -->
	<xsl:param name="key" />
	<xsl:param name="metadata" value="false" />
  <xsl:param name="languages">false</xsl:param>

	<xsl:include href="utilities_metadata.xsl" />

	<xsl:template match="/">
		<html>
			<head>
				<title><xsl:call-template name="topicTitlePlain"/></title>
				<xsl:call-template name="insertStylesheets" />
				<xsl:call-template name="insertScripts" />
				<xsl:call-template name="insertFilename" />
				<xsl:call-template name="insertMetadata" />
			</head>
			<body>
				<xsl:call-template name="control"/>
				<xsl:call-template name="main"/>
			</body>
		</html>
	</xsl:template>

	<!-- useful global variables -->

  <xsl:variable name="tgroup" select="/document/reference/topicdata/@group" />
  <xsl:variable name="tsubgroup" select="/document/reference/topicdata/@subgroup" />

  <xsl:variable name="group" select="/document/reference/apidata/@group" />
	<xsl:variable name="subgroup" select="/document/reference/apidata/@subgroup" />
	<xsl:variable name="subsubgroup" select="/document/reference/apidata/@subsubgroup" />

	<!-- document head -->

	<xsl:template name="insertStylesheets">
		<link rel="stylesheet" type="text/css" href="../styles/presentation.css" />
		<!-- make mshelp links work -->
		<link rel="stylesheet" type="text/css" href="ms-help://Hx/HxRuntime/HxLink.css" />
	</xsl:template>

	<xsl:template name="insertScripts">
    <script type="text/javascript">
      <includeAttribute name="src" item="scriptPath"><parameter>script_prototype.js</parameter></includeAttribute>
      <xsl:text> </xsl:text>
    </script>
		<script type="text/javascript">
			<includeAttribute name="src" item="scriptPath"><parameter>EventUtilities.js</parameter></includeAttribute>
      <xsl:text> </xsl:text>
		</script>
		<script type="text/javascript">
			<includeAttribute name="src" item="scriptPath"><parameter>StyleUtilities.js</parameter></includeAttribute>
      <xsl:text> </xsl:text>
    </script>
    <script type="text/javascript">
      <includeAttribute name="src" item="scriptPath">
        <parameter>SplitScreen.js</parameter>
      </includeAttribute>
      <xsl:text> </xsl:text>
    </script>
		<script type="text/javascript">
			<includeAttribute name="src" item="scriptPath"><parameter>ElementCollection.js</parameter></includeAttribute>
      <xsl:text> </xsl:text>
    </script>
		<script type="text/javascript">
			<includeAttribute name="src" item="scriptPath"><parameter>MemberFilter.js</parameter></includeAttribute>
      <xsl:text> </xsl:text>
    </script>
		<script type="text/javascript">
			<includeAttribute name="src" item="scriptPath"><parameter>CollapsibleSection.js</parameter></includeAttribute>
      <xsl:text> </xsl:text>
    </script>
		<script type="text/javascript">
			<includeAttribute name="src" item="scriptPath"><parameter>LanguageFilter.js</parameter></includeAttribute>
      <xsl:text> </xsl:text>
    </script>
    <script type="text/javascript">
      <includeAttribute name="src" item="scriptPath">
        <parameter>CookieDataStore.js</parameter>
      </includeAttribute>
      <xsl:text> </xsl:text>
    </script>
	</xsl:template>

	<xsl:template match="parameters">
    <div id="parameters">
		<xsl:call-template name="section">
			<xsl:with-param name="title"><include item="parametersTitle" /></xsl:with-param>
			<xsl:with-param name="content">
					<xsl:for-each select="parameter">
						<xsl:variable name="parameterName" select="@name" />
              <dl paramName="{$parameterName}">
						<dt>
							<span class="parameter"><xsl:value-of select="$parameterName"/></span>
							<xsl:text>&#xa0;(</xsl:text>
							<xsl:apply-templates select="*[1]" mode="link" />
							<xsl:text>)</xsl:text>
						</dt>
						<dd>
							<xsl:call-template name="getParameterDescription">
								<xsl:with-param name="name" select="@name" />
							</xsl:call-template>
						</dd>
				</dl>
          </xsl:for-each>
			</xsl:with-param>
		</xsl:call-template>
    </div>
	</xsl:template>

	<xsl:template match="element" mode="root">
		<tr>
			<td>
				<xsl:call-template name="createReferenceLink">
					<xsl:with-param name="id" select="@api" />
				</xsl:call-template>
			</td>
			<td>
				<xsl:call-template name="getElementDescription" /><br />
			</td>
		</tr>
	</xsl:template>

	<xsl:template match="element" mode="namespace">
		<tr>
			<xsl:attribute name="data">
				<xsl:value-of select="apidata/@subgroup" />
				<xsl:text>; public</xsl:text>
			</xsl:attribute>
			<td>
				<xsl:call-template name="apiIcon" />
			</td>
			<td>
				<xsl:call-template name="createReferenceLink">
					<xsl:with-param name="id" select="@api" />
				</xsl:call-template>
			</td>
			<td>
        <xsl:call-template name="getInternalOnlyDescription" />
        <xsl:if test="attributes/attribute/type[@api='T:System.ObsoleteAttribute']">
          <xsl:text> </xsl:text>
          <include item="obsoleteShort" />
        </xsl:if>
        <xsl:if test="attributes/attribute/type[@api='T:System.Security.Permissions.HostProtectionAttribute']">
          <xsl:text> </xsl:text>
          <include item="hostProtectionAttributeShort" />
        </xsl:if>
				<xsl:call-template name="getElementDescription" />
        <br />
      </td>
		</tr>
	</xsl:template>

	<xsl:template match="element" mode="enumeration">
		<tr>
      <xsl:variable name="id" select="@api" />
      <td target="{$id}">
				<xsl:call-template name="createReferenceLink">
					<xsl:with-param name="id" select="@api" />
				</xsl:call-template>
			</td>
			<td>
        <xsl:call-template name="getInternalOnlyDescription" />
				<xsl:call-template name="getElementDescription" />
        <br />
			</td>
		</tr>
	</xsl:template>

	<xsl:template match="element" mode="type">
		<tr>
			<xsl:attribute name="data">
				<xsl:value-of select="apidata/@subgroup" />
				<xsl:choose>
					<xsl:when test="memberdata/@visibility='public'">
						<xsl:text>; public</xsl:text>
					</xsl:when>
					<xsl:when test="memberdata/@visibility='family'">
						<xsl:text>; protected</xsl:text>
					</xsl:when>
					<xsl:otherwise>
						<xsl:text>; public</xsl:text>
					</xsl:otherwise>
				</xsl:choose>
				<xsl:choose>
					<xsl:when test="memberdata/@static = 'true'">
						<xsl:text>; static</xsl:text>
					</xsl:when>
					<xsl:otherwise>
						<xsl:text>; instance</xsl:text>
					</xsl:otherwise>
				</xsl:choose>
				<xsl:choose>
					<xsl:when test="string(containers/type/@api) = $key">
						<xsl:text>; declared</xsl:text>
					</xsl:when>
					<xsl:otherwise>
						<xsl:text>; inherited</xsl:text>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:attribute>
			<td>
				<xsl:call-template name="apiIcon" />
			</td>
			<td>
				<xsl:choose>
					<xsl:when test="@display-api">
            <referenceLink target="{@api}" display-target="content">
              <member api="{@api}">
                <xsl:copy-of select="containers/type" />
              </member>
            </referenceLink>
            <!--
						<referenceLink target="{@api}" display-target="{@display-api}" />
            -->
					</xsl:when>
					<xsl:otherwise>
						<referenceLink target="{@api}" />
					</xsl:otherwise>
				</xsl:choose>
			</td>
			<td>
        <xsl:call-template name="getInternalOnlyDescription" />
        <xsl:if test="attributes/attribute/type[@api='T:System.ObsoleteAttribute']">
          <xsl:text> </xsl:text>
          <include item="obsoleteShort" />
        </xsl:if>
        <xsl:if test="attributes/attribute/type[@api='T:System.Security.Permissions.HostProtectionAttribute']">
          <xsl:text> </xsl:text>
          <include item="hostProtectionAttributeShort" />
        </xsl:if>
				<xsl:call-template name="getElementDescription" />
				<xsl:choose>
					<xsl:when test="($group != 'member') and (string(containers/type/@api) != $key)">
						<xsl:text> </xsl:text>
						<include item="inheritedFrom">
							<parameter>
								<xsl:apply-templates select="containers/type" mode="link" />
							</parameter>
						</include>
					</xsl:when>
					<xsl:when test="overrides">
						<xsl:text> </xsl:text>
						<include item="overridesMember">
							<parameter>
								<xsl:apply-templates select="overrides/member" />
							</parameter>
						</include>
					</xsl:when>
				</xsl:choose>
        <br />
      </td>
		</tr>
	</xsl:template>

	<xsl:template name="insertFilename">
    <meta name="file" content="{/document/reference/file/@name}" />
	</xsl:template>

	<!-- writing templates -->

	<xsl:template name="csTemplates">
		<xsl:param name="seperator" select="string(',')" />
		<xsl:text>&lt;</xsl:text>
		<xsl:for-each select="template">
			<xsl:value-of select="@name" />
			<xsl:if test="not(position()=last())">
				<xsl:value-of select="$seperator" />
			</xsl:if>
		</xsl:for-each>
		<xsl:text>&gt;</xsl:text>
	</xsl:template>

	<xsl:template name="csTemplatesInIndex" >
		<xsl:text>%3C</xsl:text>
		<xsl:for-each select="template">
			<xsl:value-of select="@name" />
			<xsl:if test="not(position()=last())">
				<xsl:text>%2C </xsl:text>
			</xsl:if>
		</xsl:for-each>
		<xsl:text>%3E</xsl:text>
	</xsl:template>

	<xsl:template name="vbTemplates">
		<xsl:param name="seperator" select="string(',')" />
		<xsl:text>(Of </xsl:text>
		<xsl:for-each select="template">
			<xsl:value-of select="@name" />
			<xsl:if test="not(position()=last())">
				<xsl:value-of select="$seperator" />
			</xsl:if>
		</xsl:for-each>
		<xsl:text>)</xsl:text>
	</xsl:template>

	<xsl:template name="typeTitle">
		<xsl:if test="containers/container[@type]">
			<xsl:for-each select="containers/container[@type]">
				<xsl:call-template name="typeTitle" />
			</xsl:for-each>
			<xsl:text>.</xsl:text>
		</xsl:if>
		<xsl:value-of select="apidata/@name" />
		<xsl:if test="count(templates/template) > 0">
			<xsl:for-each select="templates"><xsl:call-template name="csTemplates" /></xsl:for-each>
		</xsl:if> 
	</xsl:template>

	<!-- document body -->

	<!-- control window -->

	<xsl:template name="control">
		<div id="control">
			<span class="productTitle"><include item="productTitle" /></span><br/>
			<span class="topicTitle"><xsl:call-template name="topicTitleDecorated" /></span><br/>
			<div id="toolbar">
				<span id="chickenFeet"><xsl:call-template name="chickenFeet" /></span>
        <xsl:if test="boolean(($languages != 'false') and (count($languages/language) &gt; 0))">
					<span id="languageFilter">
						<select id="languageSelector" onchange="var names = this.value.split(' '); toggleVisibleLanguage(names[1]); lfc.switchLanguage(names[0]); store.set('lang',this.value); store.save();">
							<xsl:for-each select="$languages/language">
								<option value="{@name} {@style}"><include item="{@label}Label" /></option>
							</xsl:for-each>
						</select>
					</span>
				</xsl:if>
			</div>
		</div>
	</xsl:template>

	<!-- Title in topic -->

	<xsl:template name="topicTitlePlain">
		<include>
			<xsl:attribute name="item">
				<xsl:choose>
					<xsl:when test="boolean($subsubgroup)">
						<xsl:value-of select="$subsubgroup" />
					</xsl:when>
					<xsl:when test="boolean($subgroup)">
						<xsl:value-of select="$subgroup" />
					</xsl:when>
          <xsl:when test="boolean($group)">
            <xsl:value-of select="$group" />
          </xsl:when>
          <xsl:when test="boolean($tsubgroup)">
            <xsl:value-of select="$tsubgroup" />
          </xsl:when>
				</xsl:choose>
				<xsl:text>TopicTitle</xsl:text>
			</xsl:attribute>
			<parameter>
				<xsl:call-template name="shortNamePlain" />
<!--
				<xsl:choose>
					<xsl:when test="boolean($group='type')">
						<xsl:for-each select="/document/reference"><xsl:call-template name="typeTitle" /></xsl:for-each>
					</xsl:when>
					<xsl:when test="$subgroup='constructor'">
						<xsl:for-each select="/document/reference/containers/type"><xsl:call-template name="typeTitle"/></xsl:for-each>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="/document/reference/apidata/@name" />
					</xsl:otherwise>
				</xsl:choose>
-->
			</parameter>
			<parameter>
        <xsl:for-each select="/document/reference[1]">
          <xsl:call-template name="parameterNames" />
        </xsl:for-each>
			</parameter>
		</include>
	</xsl:template>

	<xsl:template name="topicTitleDecorated">
		<include>
			<xsl:attribute name="item">
				<xsl:choose>
					<xsl:when test="boolean($subsubgroup)">
						<xsl:value-of select="$subsubgroup" />
					</xsl:when>
					<xsl:when test="boolean($subgroup)">
						<xsl:value-of select="$subgroup" />
					</xsl:when>
					<xsl:when test="boolean($group)">
						<xsl:value-of select="$group" />
					</xsl:when>
          <xsl:when test="boolean($tsubgroup)">
            <xsl:value-of select="$tsubgroup" />
          </xsl:when>
        </xsl:choose>
				<xsl:text>TopicTitle</xsl:text>
			</xsl:attribute>
			<parameter>
				<xsl:call-template name="shortNameDecorated" />
			</parameter>
			<parameter>
        <xsl:for-each select="/document/reference[1]">
          <xsl:call-template name="parameterNames" />
        </xsl:for-each>
			</parameter>
		</include>
	</xsl:template>

	<!-- Title in TOC -->

	<!-- Index entry -->

	<!-- chicken feet navigation -->

	<xsl:template name="chickenFeet">
		<include item="rootLink" />
		<xsl:if test="boolean(/document/reference/containers/namespace)">
				<xsl:text> &#x25ba; </xsl:text>
				<referenceLink target="{document/reference/containers/namespace/@api}" />
		</xsl:if>
		<xsl:if test="boolean(/document/reference/containers/type)">
				<xsl:text> &#x25ba; </xsl:text>
				<xsl:apply-templates select="/document/reference/containers/type" mode="link" />
		</xsl:if>
		<xsl:if test="$group">
			<xsl:text> &#x25ba; </xsl:text>
			<referenceLink target="{$key}" />
		</xsl:if>
	</xsl:template>

	<!-- main window -->

	<xsl:template name="main">
		<div id="main">
			<xsl:call-template name="head" />
			<xsl:call-template name="body" />
			<xsl:call-template name="foot" />
		</div>
	</xsl:template>

	<xsl:template name="head">
		<include item="header" />
	</xsl:template>

  <xsl:template match="syntax">
    <xsl:if test="count(*) > 0">
      <xsl:call-template name="section">
        <xsl:with-param name="title">
          <include item="syntaxTitle" />
        </xsl:with-param>
        <xsl:with-param name="content">
          <xsl:call-template name="syntaxContent" />
        </xsl:with-param>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

    <xsl:template match="usyntax">
      <xsl:if test="count(*) > 0">
        <xsl:call-template name="section">
          <xsl:with-param name="title">
            <include item="usyntaxTitle" />
          </xsl:with-param>
          <xsl:with-param name="content">
            <xsl:call-template name="usyntaxContent" />
          </xsl:with-param>
        </xsl:call-template>
      </xsl:if>
    </xsl:template>

      <xsl:template name="syntaxContent">
        <div id="syntaxSection">
					<table class="filter">
						<tr class="tabs" id="syntaxTabs">
							<xsl:for-each select="div[@codeLanguage]">
                <td class="tab" x-lang="{@codeLanguage}" onclick="toggleClass('syntaxTabs','x-lang','{@codeLanguage}','activeTab','tab'); toggleStyle('syntaxBlocks','x-lang','{@codeLanguage}','display','block','none');" ><include item="{@codeLanguage}Label" /></td>
							</xsl:for-each>
						</tr>
					</table>
					<div id="syntaxBlocks">
							<xsl:for-each select="div[@codeLanguage]">
								<div class="code" x-lang="{@codeLanguage}"><pre><xsl:copy-of select="./node()" /></pre></div>
							</xsl:for-each>
					</div>
        </div>
	</xsl:template>

      <xsl:template name="usyntaxContent">
        <div id="usyntaxSection">
        <table class="filter">
          <tr class="tabs" id="usyntaxTabs">
            <xsl:for-each select="div[@codeLanguage]">
                <td class="tab" x-lang="{@codeLanguage}" onclick="toggleClass('usyntaxTabs','x-lang','{@codeLanguage}','activeTab','tab'); toggleStyle('usyntaxBlocks','x-lang','{@codeLanguage}','display','block','none');" ><include item="{@codeLanguage}Label" /></td>
            </xsl:for-each>
          </tr>
        </table>
        <div id="usyntaxBlocks">
          <xsl:for-each select="div[@codeLanguage]">
            <div class="code" x-lang="{@codeLanguage}">
              <pre>
                <xsl:copy-of select="./node()" />
              </pre>
            </div>
          </xsl:for-each>
        </div>
        </div>
      </xsl:template>

  <xsl:template match="elements" mode="root">
		<xsl:if test="count(element) > 0">
			<xsl:call-template name="section">
				<xsl:with-param name="title"><include item="namespacesTitle" /></xsl:with-param>
				<xsl:with-param name="content">
				<table class="members" id="memberList">
					<tr>
						<th class="nameColumn"><include item="namespaceNameHeader"/></th>
						<th class="descriptionColumn"><include item="namespaceDescriptionHeader" /></th>
					</tr>
					<xsl:apply-templates select="element" mode="root">
						<xsl:sort select="apidata/@name" />
					</xsl:apply-templates>
				</table>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="elements" mode="namespace">
		<xsl:call-template name="section">
			<xsl:with-param name="title"><include item="typesTitle" /></xsl:with-param>
			<xsl:with-param name="content">
        <div id="typeSection">
				<table class="filter">
					<tr class="tabs" id="typeFilter">
              <td class="tab" value="all" onclick="toggleClass('typeFilter','value','all','activeTab','tab'); processSubgroup('all', 'type'); processList('typeList','filterElement', 'type');"><include item="allTypesFilterLabel" /></td>
            <xsl:if test="element/apidata[@subgroup='class']">
                <td class="tab" value="class" onclick="toggleClass('typeFilter','value','class','activeTab','tab'); processSubgroup('class', 'type'); processList('typeList','filterElement','type');"><include item="classTypesFilterLabel" /></td>
            </xsl:if>
            <xsl:if test="element/apidata[@subgroup='structure']">
                <td class="tab" value="structure" onclick="toggleClass('typeFilter','value','structure','activeTab','tab'); processSubgroup('structure', 'type'); processList('typeList','filterElement','type');"><include item="structureTypesFilterLabel" /></td>
            </xsl:if>
            <xsl:if test="element/apidata[@subgroup='interface']">
                <td class="tab" value="interface" onclick="toggleClass('typeFilter','value','interface','activeTab','tab'); processSubgroup('interface', 'type'); processList('typeList','filterElement','type');"><include item="interfaceTypesFilterLabel" /></td>
            </xsl:if>
            <xsl:if test="element/apidata[@subgroup='enumeration']">
                <td class="tab" value="enumeration" onclick="toggleClass('typeFilter','value','enumeration','activeTab','tab'); processSubgroup('enumeration', 'type'); processList('typeList','filterElement','type');"><include item="enumerationTypesFilterLabel" /></td>
            </xsl:if>
            <xsl:if test="element/apidata[@subgroup='delegate']">
                <td class="tab" value="delegate" onclick="toggleClass('typeFilter','value','delegate','activeTab','tab'); processSubgroup('delegate', 'type'); processList('typeList','filterElement','type');"><include item="delegateTypesFilterLabel" /></td>
            </xsl:if>
          </tr>
				</table>
				<table id="typeList" class="members">
					<tr>
						<th class="iconColumn"><include item="typeIconHeader"/></th>
						<th class="nameColumn"><include item="typeNameHeader"/></th>
						<th class="descriptionColumn"><include item="typeDescriptionHeader" /></th>
					</tr>
					<xsl:apply-templates select="element" mode="namespace">
						<xsl:sort select="apidata/@name" />
					</xsl:apply-templates>
				</table>
        </div>
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="elements" mode="enumeration">
		<xsl:if test="count(element) > 0">
      <div id="enumerationSection">
			<xsl:call-template name="section">
				<xsl:with-param name="title"><include item="membersTitle" /></xsl:with-param>
				<xsl:with-param name="content">
					<table class="members" id="memberList">
						<tr>
							<th class="nameColumn"><include item="memberNameHeader"/></th>
							<th class="descriptionColumn"><include item="memberDescriptionHeader" /></th>
						</tr>
            <!-- don't sort for enumeration members -->
						<xsl:apply-templates select="element" mode="enumeration" />
					</table>
				</xsl:with-param>
			</xsl:call-template>
      </div>
		</xsl:if>
	</xsl:template>

	<xsl:template match="elements" mode="type">
		<xsl:if test="count(element) > 0">
			<xsl:call-template name="section">
				<xsl:with-param name="title"><include item="membersTitle" /></xsl:with-param>
				<xsl:with-param name="content">
          <div id="allMembersSection">
				<table class="filter">
					<tr class="tabs" id="memberTabs">
                <td class="tab" value="all" onclick="toggleClass('memberTabs','value','all','activeTab','tab'); processSubgroup('all', 'member'); processList('memberList','filterElement','member');"><include item="allMembersFilterLabel" /></td>
            <xsl:if test="element/apidata[@subgroup='constructor']">
                  <td class="tab" value="constructor" onclick="toggleClass('memberTabs','value','constructor','activeTab','tab'); processSubgroup('constructor','member');processList('memberList','filterElement','member');"><include item="constructorMembersFilterLabel" /></td>
            </xsl:if>
            <xsl:if test="element/apidata[@subgroup='method']">
                  <td class="tab" value="method" onclick="toggleClass('memberTabs','value','method','activeTab','tab'); processSubgroup('method','member'); processList('memberList','filterElement','member');"><include item="methodMembersFilterLabel" /></td>
            </xsl:if>
            <xsl:if test="element/apidata[@subgroup='property']">
                  <td class="tab" value="property" onclick="toggleClass('memberTabs','value','property','activeTab','tab'); processSubgroup('property','member'); processList('memberList','filterElement','member');"><include item="propertyMembersFilterLabel" /></td>
            </xsl:if>
            <xsl:if test="element/apidata[@subgroup='field']">
                  <td class="tab" value="field" onclick="toggleClass('memberTabs','value','field','activeTab','tab'); processSubgroup('field','member'); processList('memberList','filterElement','member');"><include item="fieldMembersFilterLabel" /></td>
            </xsl:if>
            <xsl:if test="element/apidata[@subgroup='event']">
                  <td class="tab" value="event" onclick="toggleClass('memberTabs','value','event','activeTab','tab'); processSubgroup('event','member'); processList('memberList','filterElement','member');"><include item="eventMembersFilterLabel" /></td>
            </xsl:if>
					</tr>
					<tr>
						<td class="line" colspan="2">
                  <label for="public"><input id="public" type="checkbox" checked="true" onclick="toggleCheckState('public',this.checked); processList('memberList','filterElement','member');" /><include item="publicMembersFilterLabel" /></label><br/>
                  <label for="protected"><input id="protected" type="checkbox" checked="true" onclick="toggleCheckState('protected',this.checked); processList('memberList','filterElement','member');" /><include item="protectedMembersFilterLabel" /></label>
						</td>
						<td class="line" colspan="2">
                  <label for="instance"><input id="instance" type="checkbox" checked="true" onclick="toggleCheckState('instance',this.checked); processList('memberList','filterElement','member');" /><include item="instanceMembersFilterLabel" /></label><br/>
                  <label for="static"><input id="static" type="checkbox" checked="true" onclick="toggleCheckState('static',this.checked); processList('memberList','filterElement','member');" /><include item="staticMembersFilterLabel" /></label>
						</td>
						<td class="line" colspan="2">
                  <label for="declared"><input id="declared" type="checkbox" checked="true" onclick="toggleCheckState('declared',this.checked); processList('memberList','filterElement','member');" /><include item="declaredMembersFilterLabel" /></label><br/>
                  <label for="inherited"><input id="inherited" type="checkbox" checked="true" onclick="toggleCheckState('inherited',this.checked); processList('memberList','filterElement','member');" /><include item="inheritedMembersFilterLabel" /></label>
						</td>
					</tr>
				</table>
				<table class="members" id="memberList">
					<tr>
						<th class="iconColumn"><include item="memberIconHeader"/></th>
						<th class="nameColumn"><include item="memberNameHeader"/></th>
						<th class="descriptionColumn"><include item="memberDescriptionHeader" /></th>
					</tr>
					<xsl:apply-templates select="element" mode="type">
						<xsl:sort select="apidata/@name" />
					</xsl:apply-templates>
				</table>
          </div>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="elements" mode="overload">
		<xsl:if test="count(element) > 0">
			<xsl:call-template name="section">
				<xsl:with-param name="title"><include item="membersTitle" /></xsl:with-param>
				<xsl:with-param name="content">
				<table class="members" id="memberList">
					<tr>
						<th class="iconColumn"><include item="memberIconHeader"/></th>
						<th class="nameColumn"><include item="memberNameHeader"/></th>
						<th class="descriptionColumn"><include item="memberDescriptionHeader" /></th>
					</tr>
					<xsl:apply-templates select="element" mode="type">
						<xsl:sort select="apidata/@name" />
					</xsl:apply-templates>
				</table>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template name="apiIcon">
    <!-- determine visibility prefix -->
    <xsl:variable name="visibility">
      <xsl:choose>
        <xsl:when test="apidata/@group='type'">
          <xsl:choose>
            <xsl:when test="typedata/@visibility='public'">
              <xsl:text>pub</xsl:text>
            </xsl:when>
            <xsl:otherwise>
              <xsl:text>priv</xsl:text>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>
        <xsl:when test="apidata/@group='member'">
          <xsl:choose>
            <xsl:when test="memberdata/@visibility='public'">
              <xsl:text>pub</xsl:text>
            </xsl:when>
            <xsl:when test="memberdata/@visibility='family' or memberdata/@visibility='family and assembly'">
              <xsl:text>prot</xsl:text>
            </xsl:when>
            <xsl:otherwise>
              <xsl:text>priv</xsl:text>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <!-- determine icon file name -->
    <xsl:variable name="kind">
      <xsl:choose>
        <xsl:when test="apidata/@group='type'">
          <xsl:choose>
            <xsl:when test="apidata/@subgroup='class'">
              <xsl:text>class</xsl:text>
            </xsl:when>
            <xsl:when test="apidata/@subgroup='structure'">
              <xsl:text>structure</xsl:text>
            </xsl:when>
            <xsl:when test="apidata/@subgroup='interface'">
              <xsl:text>interface</xsl:text>
            </xsl:when>
            <xsl:when test="apidata/@subgroup='delegate'">
              <xsl:text>delegate</xsl:text>
            </xsl:when>
            <xsl:when test="apidata/@subgroup='enumeration'">
              <xsl:text>enum</xsl:text>
            </xsl:when>
          </xsl:choose>
        </xsl:when>
        <xsl:when test="apidata/@group='member'">
          <xsl:choose>
            <xsl:when test="apidata/@subgroup='field'">
              <xsl:text>field</xsl:text>
            </xsl:when>
            <xsl:when test="apidata/@subgroup='property'">
              <xsl:text>property</xsl:text>
            </xsl:when>
            <xsl:when test="apidata/@subgroup='method'">
              <xsl:choose>
                <xsl:when test="apidata/@subsubgroup='operator'">
                  <xsl:text>operator</xsl:text>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:text>method</xsl:text>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
            <xsl:when test="apidata/@subgroup='constructor'">
              <xsl:text>method</xsl:text>
            </xsl:when>
            <xsl:when test="apidata/@subgroup='event'">
              <xsl:text>event</xsl:text>
            </xsl:when>
          </xsl:choose>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <!-- write out an image tag for the icon -->
    <img>
      <includeAttribute name="src" item="iconPath">
        <parameter>
          <xsl:value-of select="concat($visibility,$kind,'.gif')"/>
        </parameter>
      </includeAttribute>
    </img>
    <!--
		<xsl:choose>
			<xsl:when test="apidata/@group='type'">
				<xsl:choose>
					<xsl:when test="apidata/@subgroup='class'">
						<img>
							<includeAttribute name="src" item="iconPath">
								<parameter>pubclass.gif</parameter>
							</includeAttribute>
							<includeAttribute name="title" item="publicClassAltText" />
						</img>
					</xsl:when>
					<xsl:when test="apidata/@subgroup='structure'">
						<img>
							<includeAttribute name="src" item="iconPath">
								<parameter>pubstructure.gif</parameter>
							</includeAttribute>
							<includeAttribute name="title" item="publicStructureAltText" />
						</img>
					</xsl:when>
					<xsl:when test="apidata/@subgroup='interface'">
						<img>
							<includeAttribute name="src" item="iconPath">
								<parameter>pubinterface.gif</parameter>
							</includeAttribute>
							<includeAttribute name="title" item="publicInterfaceAltText" />
						</img>
					</xsl:when>
					<xsl:when test="apidata/@subgroup='delegate'">
						<img>
							<includeAttribute name="src" item="iconPath">
								<parameter>pubdelegate.gif</parameter>
							</includeAttribute>
							<includeAttribute name="title" item="publicDelegateAltText" />
						</img>
					</xsl:when>
					<xsl:when test="apidata/@subgroup='enumeration'">
						<img>
							<includeAttribute name="src" item="iconPath">
								<parameter>pubenum.gif</parameter>
							</includeAttribute>
							<includeAttribute name="title" item="publicEnumerationAltText" />
						</img>
					</xsl:when>
				</xsl:choose>
			</xsl:when>
			<xsl:when test="apidata/@group='member'">
				<xsl:variable name="memberVisibility">
					<xsl:choose>
						<xsl:when test="memberdata/@visibility='public'">
							<xsl:text>pub</xsl:text>
						</xsl:when>
            <xsl:when test="memberdata/@visibility='family' or memberdata/@visibility='family or assembly' or memberdata/@visibility='assembly'">
							<xsl:text>prot</xsl:text>
            </xsl:when>
            <xsl:when test="memberdata/@visibility='private'">
              <xsl:text>priv</xsl:text>
						</xsl:when>
						<xsl:otherwise>
							<xsl:text>pub</xsl:text>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:variable>
				<xsl:variable name="memberSubgroup">
					<xsl:choose>
						<xsl:when test="apidata/@subgroup='constructor'">
							<xsl:text>method</xsl:text>
						</xsl:when>
						<xsl:otherwise>
							<xsl:value-of select="apidata/@subgroup" />
						</xsl:otherwise>
					</xsl:choose>
				</xsl:variable>
				<xsl:variable name="memberIcon" select="concat($memberVisibility,$memberSubgroup,'.gif')" />
        <xsl:if test="memberdata/@visibility='private' and proceduredata/@virtual='true'">
          <img>
            <includeAttribute name="src" item="iconPath">
              <parameter>pubinterface.gif</parameter>
            </includeAttribute>
          </img>
        </xsl:if>
        <img>
          <includeAttribute name="src" item="iconPath">
            <parameter>
              <xsl:value-of select="$memberIcon" />
            </parameter>
          </includeAttribute>
        </img>
				</xsl:if>
			</xsl:when>
		</xsl:choose>
        -->
    <xsl:if test="memberdata/@static='true'">
      <img>
        <includeAttribute name="src" item="iconPath">
          <parameter>static.gif</parameter>
        </includeAttribute>
        <includeAttribute name="title" item="staticAltText" />
      </img>
    </xsl:if>

  </xsl:template>

	<!-- Footer stuff -->
	
	<xsl:template name="foot">
		<include item="footer" />
	</xsl:template>

	<!-- Assembly information -->

	<xsl:template match="library">
		<p><include item="locationInformation">
			<parameter><span sdata="assembly"><xsl:value-of select="@assembly"/></span></parameter>
			<parameter><xsl:value-of select="@module" /></parameter>
		</include></p>
	</xsl:template>

  <!-- Version information -->

  <xsl:template match="versions">
    <xsl:call-template name="section">
      <xsl:with-param name="title">
        <include item="versionsTitle" />
      </xsl:with-param>
      <xsl:with-param name="content">
        <xsl:call-template name="processVersions" />
      </xsl:with-param>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="processVersions">
    <xsl:choose>
      <xsl:when test="versions">
        <ul>
          <xsl:for-each select="versions">
            <li>
              <include item="{@name}" />
              <xsl:text>: </xsl:text>
              <xsl:call-template name="processVersions" />
            </li>
          </xsl:for-each>
        </ul>
      </xsl:when>
      <xsl:otherwise>
        <xsl:for-each select="version">
          <include item="{@name}" />
          <xsl:if test="not(position()=last())">
            <xsl:text>, </xsl:text>
          </xsl:if>
        </xsl:for-each>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
 
  <!-- Interface implementors -->

  <xsl:template match="implementors">
    <xsl:call-template name="section">
      <xsl:with-param name="title">
        <include item="implementorsTitle" />
      </xsl:with-param>
      <xsl:with-param name="content">
        <ul>
          <xsl:for-each select="type">
            <li>
              <xsl:apply-templates select="self::type" mode="link" />
            </li>
          </xsl:for-each>
        </ul>
      </xsl:with-param>
    </xsl:call-template>
  </xsl:template>
  
	<!-- Inheritance hierarchy -->

	<xsl:template match="family">
		<xsl:call-template name="section">
			<xsl:with-param name="title"><include item="familyTitle" /></xsl:with-param>
			<xsl:with-param name="content">
				<xsl:variable name="ancestorCount" select="count(ancestors/*)" />
				<xsl:variable name="childCount" select="count(descendents/*)" />
				<xsl:variable name="columnCount">
					<xsl:choose>
						<xsl:when test="$childCount = 0">
							<xsl:value-of select="$ancestorCount + 1" />
						</xsl:when>
						<xsl:otherwise>
							<xsl:value-of select="$ancestorCount + 2" />
						</xsl:otherwise>
					</xsl:choose>
				</xsl:variable>

				<table cellspacing="0" cellpadding="0">
					<xsl:for-each select="ancestors/type">
						<xsl:sort select="position()" data-type="number" order="descending" />
						<!-- <xsl:sort select="@api"/> -->
						<tr>
							<xsl:call-template name="createTableEntries">
								<xsl:with-param name="count" select="position() - 2" />
							</xsl:call-template>

							<xsl:if test="position() &gt; 1">
								<td>
									<img>
										<includeAttribute name="src" item="iconPath">
											<parameter>LastChild.gif</parameter>
										</includeAttribute>
									</img>
								</td>
							</xsl:if>

							<td colspan="{$columnCount - position() + 1}">
								<xsl:apply-templates select="self::type" mode="link" />
							</td>
						</tr>
					</xsl:for-each>

					<tr>
						<xsl:call-template name="createTableEntries">
							<xsl:with-param name="count" select="$ancestorCount - 1" />
						</xsl:call-template>

						<xsl:if test="$ancestorCount &gt; 0">
							<td>
								<img>
									<includeAttribute name="src" item="iconPath">
										<parameter>LastChild.gif</parameter>
									</includeAttribute>
								</img>
							</td>
						</xsl:if>

						<td>
							<xsl:if test="$childCount &gt; 0">
								<xsl:attribute name="colspan">2</xsl:attribute>
							</xsl:if>
							<referenceLink target="{$key}" />
						</td>
					</tr>

					<xsl:for-each select="descendents/type">
            <xsl:sort select="@api"/>
						<tr>

						<xsl:call-template name="createTableEntries">
							<xsl:with-param name="count" select="$ancestorCount" />
						</xsl:call-template>

						<td>
							<xsl:choose>
								<xsl:when test="position()=last()">
									<img>
										<includeAttribute name="src" item="iconPath">
											<parameter>LastChild.gif</parameter>
										</includeAttribute>
									</img>
								</xsl:when>
								<xsl:otherwise>
									<img>
										<includeAttribute name="src" item="iconPath">
											<parameter>NotLastChild.gif</parameter>
										</includeAttribute>
									</img>
								</xsl:otherwise>
							</xsl:choose>
						</td>

						<td><xsl:apply-templates select="self::type" mode="link" /></td>

						</tr>

					</xsl:for-each>
					
				</table>
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<xsl:template name="createTableEntries">
		<xsl:param name="count" />
		<xsl:if test="number($count) > 0">
			<td>&#xa0;</td>
			<xsl:call-template name="createTableEntries">
				<xsl:with-param name="count" select="number($count)-1" />
			</xsl:call-template>
		</xsl:if>
	</xsl:template>


	<!-- Link to create type -->
<!--
	<xsl:template match="arrayOf">
    <span class="cpp">array&lt;</span>
    <xsl:apply-templates />
    <span class="cpp">
      <xsl:if test="number(@rank) &gt; 1">
        <xsl:text>,</xsl:text>
        <xsl:value-of select="@rank"/>
      </xsl:if>
      <xsl:text>&gt;</xsl:text>
    </span>
    <span class="cs">
      <xsl:text>[</xsl:text>
      <xsl:if test="number(@rank) &gt; 1">,</xsl:if>
      <xsl:text>]</xsl:text>
    </span>
    <span class="vb">
      <xsl:text>(</xsl:text>
      <xsl:if test="number(@rank) &gt; 1">,</xsl:if>
      <xsl:text>)</xsl:text>
    </span>
  </xsl:template>

	<xsl:template match="pointerTo">
		<xsl:apply-templates /><xsl:text>*</xsl:text>
	</xsl:template>

	<xsl:template match="referenceTo">
		<xsl:apply-templates />
    <span class="cpp">%</span>
	</xsl:template>

	<xsl:template match="type">
    <xsl:param name="qualified" select="false()" />
		<referenceLink target="{@api}">
			<xsl:choose>
				<xsl:when test="specialization">
					<xsl:attribute name="show-templates">false</xsl:attribute>
				</xsl:when>
				<xsl:otherwise>
					<xsl:attribute name="show-templates">true</xsl:attribute>
				</xsl:otherwise>
			</xsl:choose>
      <xsl:if test="$qualified">
        <xsl:attribute name="show-container">true</xsl:attribute>
      </xsl:if>
		</referenceLink>
		<xsl:apply-templates select="specialization" />
	</xsl:template>

	<xsl:template match="template">
		<xsl:value-of select="@name" />
	</xsl:template>

	<xsl:template match="specialization">
		<span class="cs">&lt;</span>
		<span class="vb"><xsl:text>(Of </xsl:text></span>
    <span class="cpp">&lt;</span>
    <xsl:for-each select="*">
			<xsl:apply-templates select="." />
			<xsl:if test="position() != last()">
				<xsl:text>, </xsl:text>
			</xsl:if>
		</xsl:for-each>
		<span class="cs">&gt;</span>
		<span class="vb">)</span>
    <span class="cpp">&gt;</span>
  </xsl:template>
-->
	<xsl:template match="member">
		<xsl:apply-templates select="type" mode="link" />
		<xsl:text>.</xsl:text>
		<xsl:choose>
			<xsl:when test="@display-api">
				<referenceLink target="{@api}" display-target="{@display-api}" />
			</xsl:when>
			<xsl:otherwise>
				<referenceLink target="{@api}" />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<!-- Naming -->

	<!-- decorated names -->
  
	<xsl:template name="shortNameDecorated">
		<xsl:choose>
      <!-- type names must be handled specially to account for nested types -->
			<xsl:when test="$group='type'">
				<xsl:for-each select="/document/reference[1]"><xsl:call-template name="typeNameDecorated" /></xsl:for-each>
			</xsl:when>
      <!-- constructors use the type name -->
			<xsl:when test="$subgroup='constructor'">
				<xsl:for-each select="/document/reference/containers/type[1]"><xsl:call-template name="typeNameDecorated" /></xsl:for-each>
			</xsl:when>
      <!-- a normal name is just the name, followed by any generic templates -->
			<xsl:otherwise>
        <xsl:for-each select="/document/reference[1]">
				  <xsl:value-of select="apidata/@name" />
          <xsl:apply-templates select="templates" mode="decorated" />
        </xsl:for-each>
      </xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<!-- plain names -->

	<xsl:template name="shortNamePlain">
		<xsl:choose>
			<xsl:when test="$group='type'">
				<xsl:for-each select="/document/reference"><xsl:call-template name="typeNamePlain" /></xsl:for-each>
			</xsl:when>
			<xsl:when test="$subgroup='constructor'">
				<xsl:for-each select="/document/reference/containers/type"><xsl:call-template name="typeNamePlain" /></xsl:for-each>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="/document/reference/apidata/@name" />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
<!--
	<xsl:template name="typeNamePlain">
    <xsl:param name="annotate" select="false()" />
		<xsl:if test="(containers/type)|type">
			<xsl:for-each select="(containers/type)|type">
        <xsl:call-template name="typeNamePlain">
          <xsl:with-param name="annotate" select="$annotate" />
        </xsl:call-template>
			</xsl:for-each>
			<xsl:text>.</xsl:text>
		</xsl:if>
		<xsl:value-of select="apidata/@name" />
    <xsl:if test="$annotate and templates/template">
      <xsl:value-of select="concat('`',count(templates/template))"/>
    </xsl:if>
	</xsl:template>
-->    
</xsl:stylesheet>
