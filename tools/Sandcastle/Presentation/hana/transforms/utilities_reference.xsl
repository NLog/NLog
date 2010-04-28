<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.1"
 				xmlns:MSHelp="http://msdn.microsoft.com/mshelp"
        xmlns:mshelp="http://msdn.microsoft.com/mshelp"
				xmlns:ddue="http://ddue.schemas.microsoft.com/authoring/2003/5"
				xmlns:xlink="http://www.w3.org/1999/xlink"
        xmlns:msxsl="urn:schemas-microsoft-com:xslt"
    >

  <xsl:import href="../../shared/transforms/utilities_reference.xsl"/>

  <xsl:output method="xml" omit-xml-declaration="yes" encoding="utf-8" />
<!--
  <xsl:output method="xml" omit-xml-declaration="yes" encoding="utf-8" doctype-public="-//W3C//DTD HTML 4.0 Transitional//EN" doctype-system="http://www.w3.org/TR/html4/loose.dtd" />
-->
	<!-- key parameter is the api identifier string -->
	<xsl:param name="key" />
	<xsl:param name="metadata" value="false" />
  <xsl:param name="languages">false</xsl:param>
  <xsl:param name="useOverloadRowsInMemberlists" select="false()"/>
    
	<xsl:include href="utilities_metadata.xsl" />
  <xsl:include href="xamlSyntax.xsl"/>

  <xsl:template match="/">
		<html>
			<head>
        <META NAME="save" CONTENT="history"/>
        <title><xsl:call-template name="topicTitlePlain"/></title>
				<xsl:call-template name="insertStylesheets" />
				<xsl:call-template name="insertScripts" />
				<xsl:call-template name="insertFilename" />
				<xsl:call-template name="insertMetadata" />
			</head>
			<body>
        
       <xsl:call-template name="upperBodyStuff"/>
				<!--<xsl:call-template name="control"/>-->
				<xsl:call-template name="main"/>
      </body>
		</html>
	</xsl:template>

	<!-- useful global variables -->

  <xsl:variable name="group">
    <xsl:choose>
      <xsl:when test="/document/reference/topicdata/@group = 'api'">
        <xsl:value-of select="/document/reference/apidata/@group" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="/document/reference/topicdata/@group" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="subgroup">
    <xsl:choose>
      <xsl:when test="/document/reference/topicdata/@group = 'api'">
        <xsl:value-of select="/document/reference/apidata/@subgroup" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="/document/reference/topicdata/@subgroup" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  
  <xsl:variable name="subsubgroup">
    <xsl:choose>
      <xsl:when test="/document/reference/topicdata/@group = 'api'">
        <xsl:value-of select="/document/reference/apidata/@subsubgroup" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="/document/reference/topicdata/@subsubgroup" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  	<xsl:variable name="pseudo" select="boolean(/document/reference/topicdata[@pseudo='true'])"/>
  
   	<xsl:variable name="namespaceName">
    		<xsl:value-of select="substring-after(/document/reference/containers/namespace/@api,':')"/>
  	</xsl:variable>
  
 
	<!-- document head -->

	<xsl:template name="insertStylesheets">
		<link rel="stylesheet" type="text/css" href="../styles/presentation.css" />
		<!-- make mshelp links work -->
		<link rel="stylesheet" type="text/css" href="ms-help://Hx/HxRuntime/HxLink.css" />
    <link rel="stylesheet" type="text/css" href="ms-help://Dx/DxRuntime/DxLink.css" />
	</xsl:template>

	<xsl:template name="insertScripts">
    <script type="text/javascript">
      <includeAttribute name="src" item="scriptPath"><parameter>EventUtilities.js</parameter></includeAttribute>
      <xsl:text> </xsl:text>
    </script>
    <script type="text/javascript">
      <includeAttribute name="src" item="scriptPath"><parameter>SplitScreen.js</parameter></includeAttribute>
      <xsl:text> </xsl:text>
    </script>
    <script type="text/javascript">
      <includeAttribute name="src" item="scriptPath"><parameter>Dropdown.js</parameter></includeAttribute>
      <xsl:text> </xsl:text>
    </script>
    <script type="text/javascript">
      <includeAttribute name="src" item="scriptPath"><parameter>script_manifold.js</parameter></includeAttribute>
      <xsl:text> </xsl:text>
    </script>
    <script type="text/javascript">
      <includeAttribute name="src" item="scriptPath"><parameter>LanguageFilter.js</parameter></includeAttribute>
      <xsl:text> </xsl:text>
    </script>
    <script type="text/javascript">
      <includeAttribute name="src" item="scriptPath"><parameter>DataStore.js</parameter></includeAttribute>
      <xsl:text> </xsl:text>
    </script>
    <script type="text/javascript">
      <includeAttribute name="src" item="scriptPath"><parameter>CommonUtilities.js</parameter></includeAttribute>
      <xsl:text> </xsl:text>
    </script>
    <script type="text/javascript">
      <includeAttribute name="src" item="scriptPath">
        <parameter>MemberFilter.js</parameter>
      </includeAttribute>
      <xsl:text> </xsl:text>
    </script>
  </xsl:template>

  <xsl:template match="parameters">
    <div id="parameters">
    <xsl:call-template name="subSection">
      <xsl:with-param name="title">
        <include item="parametersTitle"/>
      </xsl:with-param>
      <xsl:with-param name="content">
        <xsl:for-each select="parameter">
          <xsl:variable name="paramName" select="@name"/>
          <dl paramName="{$paramName}">
            <dt>
              <span class="parameter">
                <xsl:value-of select="$paramName"/>
              </span>
            </dt>
            <dd>
              <xsl:apply-templates select="*[1]" mode="link" />
              <!--
              <xsl:choose>
                <xsl:when test="type">
                  <xsl:call-template name="typeReferenceLink">
                    <xsl:with-param name="api" select="type/@api" />
                    <xsl:with-param name="qualified" select="true()" />
                    <xsl:with-param name="specialization" select="boolean(type/specialization)" />
                  </xsl:call-template>
                  <xsl:apply-templates select="type/specialization" />
                </xsl:when>
                <xsl:otherwise>
                  <xsl:apply-templates select="*[1]" />
                </xsl:otherwise>
              </xsl:choose>
              -->
                <br />
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
 
  <xsl:template match="implements">
    <xsl:if test="member">
      <xsl:call-template name="subSection">
        <xsl:with-param name="title">
          <include item="implementsTitle" />
        </xsl:with-param>
        <xsl:with-param name="content">
          <xsl:for-each select="member">
            <referenceLink target="{@api}" qualified="true" />
            <br />
          </xsl:for-each>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>
  
	<xsl:template match="element" mode="root">
		<tr>
			<td>
        <xsl:choose>
          <xsl:when test="apidata/@name = ''">
            <referenceLink target="{@api}" qualified="false">
              <include item="defaultNamespace" />
            </referenceLink>
          </xsl:when>
          <xsl:otherwise>
            <xsl:call-template name="createReferenceLink">
              <xsl:with-param name="id" select="@api" />
            </xsl:call-template>
          </xsl:otherwise>
        </xsl:choose>
      </td>
			<td>
        <xsl:call-template name="getElementDescription" />
      </td>
		</tr>
	</xsl:template>

  <xsl:template match="element" mode="namespace">
    <xsl:variable name="typeVisibility">
      <xsl:choose>
        <xsl:when test="typedata/@visibility='family' or typedata/@visibility='family or assembly' or typedata/@visibility='assembly'">prot</xsl:when>
        <xsl:when test="typedata/@visibility='private'">priv</xsl:when>
        <xsl:otherwise>pub</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <tr>
      <xsl:attribute name="data">
        <xsl:value-of select="apidata/@subgroup" />
        <xsl:text>; public</xsl:text>
      </xsl:attribute>
      <td>
        <xsl:call-template name="typeIcon">
          <xsl:with-param name="typeVisibility" select="$typeVisibility" />
        </xsl:call-template>
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
          <include item="obsoleteRed" />
        </xsl:if>
        <xsl:if test="attributes/attribute/type[@api='T:System.Security.Permissions.HostProtectionAttribute']">
          <xsl:text> </xsl:text>
          <include item="hostProtectionAttributeShort" />
        </xsl:if>
        <xsl:call-template name="getElementDescription" />
      </td>
    </tr>
  </xsl:template>

  <xsl:template match="element" mode="member">
    <xsl:variable name="inheritedMember">
      <xsl:call-template name="IsMemberInherited"/>
    </xsl:variable>
    <xsl:variable name="staticMember">
      <xsl:call-template name="IsMemberStatic"/>
    </xsl:variable>
    <xsl:variable name="supportedOnXna">
      <xsl:call-template name="IsMemberSupportedOnXna"/>
    </xsl:variable>
    <xsl:variable name="supportedOnCf">
      <xsl:call-template name="IsMemberSupportedOnCf"/>
    </xsl:variable>
    <xsl:variable name="protectedMember">
      <xsl:call-template name="IsMemberProtected"/>
    </xsl:variable>
    <tr>
      <xsl:attribute name="data">
        <xsl:choose>
          <xsl:when test="memberdata[@visibility='private'] and proceduredata[@virtual = 'true']">
            <xsl:text>explicit</xsl:text>
          </xsl:when>
          <xsl:when test="apidata/@subsubgroup">
            <xsl:value-of select="apidata/@subsubgroup"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="apidata/@subgroup" />
          </xsl:otherwise>
        </xsl:choose>
        <xsl:choose>
          <xsl:when test="memberdata/@visibility='public'">
            <xsl:text>; public</xsl:text>
          </xsl:when>
          <xsl:when test="normalize-space($protectedMember)!=''">
            <xsl:text>; protected</xsl:text>
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>; public</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:choose>
          <xsl:when test="normalize-space($staticMember)!=''">
            <xsl:text>; static</xsl:text>
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>; instance</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:choose>
          <xsl:when test="normalize-space($inheritedMember)!=''">
            <xsl:text>; inherited</xsl:text>
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>; declared</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:choose>
          <xsl:when test="normalize-space($supportedOnCf)!=''">
            <xsl:text>; compact</xsl:text>
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>; none</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:choose>
       <xsl:when test="normalize-space($supportedOnXna)!=''">
          <xsl:text>; xna</xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>; none</xsl:text>
        </xsl:otherwise>
        </xsl:choose>
      </xsl:attribute>
      <td>
        <xsl:call-template name="memberIcons">
          <xsl:with-param name="memberVisibility">
            <xsl:choose>
              <xsl:when test="memberdata/@visibility='family' or memberdata/@visibility='family or assembly' or memberdata/@visibility='assembly'">prot</xsl:when>
              <xsl:when test="memberdata/@visibility='private'">priv</xsl:when>
              <xsl:when test="memberdata[@visibility='public']">pub</xsl:when>
              <xsl:otherwise>pub</xsl:otherwise>
            </xsl:choose>
          </xsl:with-param>
          <xsl:with-param name="staticMember" select="normalize-space($staticMember)" />
          <xsl:with-param name="supportedOnXna" select="normalize-space($supportedOnXna)"/>
          <xsl:with-param name="supportedOnCf" select="normalize-space($supportedOnCf)"/>
        </xsl:call-template>
      </td>
      <td>
        <xsl:choose>
          <xsl:when test="@display-api">
            <referenceLink target="{@api}" display-target="{@display-api}" />
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
          <include item="obsoleteRed" />
        </xsl:if>
        <xsl:if test="attributes/attribute/type[@api='T:System.Security.Permissions.HostProtectionAttribute']">
          <xsl:text> </xsl:text>
          <include item="hostProtectionAttributeShort" />
        </xsl:if>
        <xsl:if test="memberdata[@overload='true']">
          <include item="Overloaded"/>
          <xsl:text> </xsl:text>
        </xsl:if>
        <xsl:call-template name="getElementDescription" />
        <xsl:choose>
          <xsl:when test="@signatureset">
            <!-- TODO add boilerplate for other members in the sig set -->
          </xsl:when>
          <xsl:when test="not(topicdata[@subgroup='overload'])">
            <xsl:choose>
              <xsl:when test="normalize-space($inheritedMember)!=''">
            <xsl:text> </xsl:text>
            <include item="inheritedFrom">
              <parameter>
                <xsl:apply-templates select="containers/type" mode="link" />
                <!--
                <xsl:call-template name="typeReferenceLink">
                  <xsl:with-param name="api" select="containers/type/@api" />
                  <xsl:with-param name="qualified" select="false()" />
                      <xsl:with-param name="specialization" select="boolean(type/specialization)" />
                </xsl:call-template>
                    <xsl:apply-templates select="type/specialization" />
                    -->
              </parameter>
            </include>
          </xsl:when>
              <xsl:when test="overrides/member">
            <xsl:text> </xsl:text>
            <include item="overridesMember">
              <parameter>
                    <xsl:call-template name="createReferenceLink">
                      <xsl:with-param name="id" select="overrides/member/@api"/>
                      <xsl:with-param name="qualified" select="true()"/>
                    </xsl:call-template>
                  </parameter>
                </include>
              </xsl:when>
            </xsl:choose>
          </xsl:when>
        </xsl:choose>
      </td>
    </tr>
  </xsl:template>

	<xsl:template match="element" mode="enumeration">
		<tr>
      <xsl:variable name="id" select="@api" />
      <td target="{$id}">
        <span class="referenceNoLink"><xsl:value-of select="apidata/@name"/></span>
      </td>
      <td>
        <xsl:call-template name="getElementDescription" />
      </td>
		</tr>
	</xsl:template>

  <xsl:template match="element" mode="derivedType">
    <tr>
      <td>
        <xsl:choose>
          <xsl:when test="@display-api">
            <referenceLink target="{@api}" display-target="{@display-api}" />
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
          <include item="obsoleteRed" />
        </xsl:if>
        <xsl:if test="attributes/attribute/type[@api='T:System.Security.Permissions.HostProtectionAttribute']">
          <xsl:text> </xsl:text>
          <include item="hostProtectionAttributeShort" />
        </xsl:if>
        <xsl:call-template name="getElementDescription" />
        <xsl:choose>
          <xsl:when test="($group != 'member') and ($subgroup != 'DerivedTypeList') and not(contains($key, containers/type/@api))">
            <xsl:text> </xsl:text>
            <include item="inheritedFrom">
              <parameter>
                <xsl:apply-templates select="containers/type" mode="link" />
                <!--
                <xsl:call-template name="typeReferenceLink">
                  <xsl:with-param name="api" select="containers/type/@api" />
                  <xsl:with-param name="qualified" select="false()" />
                  <xsl:with-param name="specialization" select="boolean(type/specialization)" />
                </xsl:call-template>
                <xsl:apply-templates select="type/specialization" />
                -->
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
      </td>
    </tr>


  </xsl:template>

  <xsl:template match="element" mode="overload">
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
        <xsl:choose>
          <xsl:when test="@display-api">
            <referenceLink target="{@api}" display-target="{@display-api}" />
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
          <include item="obsoleteRed" />
        </xsl:if>
        <xsl:if test="attributes/attribute/type[@api='T:System.Security.Permissions.HostProtectionAttribute']">
          <xsl:text> </xsl:text>
          <include item="hostProtectionAttributeShort" />
        </xsl:if>
        <xsl:call-template name="getElementDescription" />
        <xsl:choose>
					<xsl:when test="($group != 'member') and ($subgroup != 'overload') and not(contains($key, containers/type/@api))">
            <xsl:text> </xsl:text>
            <include item="inheritedFrom">
              <parameter>
                <xsl:apply-templates select="containers/type" mode="link" />
                <!--
                <xsl:call-template name="typeReferenceLink">
                  <xsl:with-param name="api" select="containers/type/@api" />
                  <xsl:with-param name="qualified" select="false()" />
                  <xsl:with-param name="specialization" select="boolean(type/specialization)" />
                </xsl:call-template>
                <xsl:apply-templates select="type/specialization" />
                -->
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
        
      </td>
    </tr>
  </xsl:template>

	<xsl:template name="insertFilename">
		<meta name="guid">
			<xsl:attribute name="content">
				<xsl:value-of select="/document/reference/file/@name" />
			</xsl:attribute>
		</meta>
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
			<span class="topicTitle"><xsl:call-template name="topicTitleDecorated" /></span><br/>
		</div>
	</xsl:template>

	<!-- Title in topic -->

  <!-- Title in topic -->

  <xsl:template name="topicTitlePlain">
    <xsl:param name="qualifyMembers" select="false()" />
    <include>
      <xsl:attribute name="item">
        <xsl:if test="boolean(/document/reference/templates) and not($group='list')">
          <xsl:text>generic_</xsl:text>
        </xsl:if>
        <xsl:choose>
          <xsl:when test="string($subsubgroup)">
            <xsl:value-of select="$subsubgroup" />
          </xsl:when>
          <xsl:when test="string($subgroup)">
             <xsl:choose>
               <xsl:when test="$subgroup='overload'">
                 <xsl:value-of select="/document/reference/apidata/@subgroup"/>
               </xsl:when>
               <xsl:otherwise>
                 <xsl:value-of select="$subgroup" />
               </xsl:otherwise>
             </xsl:choose>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$group" />
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text>TopicTitle</xsl:text>
      </xsl:attribute>
      <parameter>
        <xsl:call-template name="shortNamePlain">
          <xsl:with-param name="qualifyMembers" select="$qualifyMembers" />
        </xsl:call-template>
      </parameter>
      <parameter>
        <xsl:if test="document/reference/memberdata/@overload" >
          <xsl:for-each select="/document/reference">
            <xsl:call-template name="parameterTypesPlain" />
          </xsl:for-each>
        </xsl:if>
      </parameter>
    </include>
  </xsl:template>

  <xsl:template name="topicTitleDecorated">
    <xsl:param name="titleType" />
    <include>
      <xsl:attribute name="item">
        <xsl:choose>
          <xsl:when test="$titleType = 'tocTitle' and $group='namespace'">
            <xsl:text>tocTitle</xsl:text>
          </xsl:when>
          <xsl:otherwise>
            <xsl:choose>
              <xsl:when test="string($subsubgroup)">
                <xsl:value-of select="$subsubgroup" />
              </xsl:when>
              <xsl:when test="string($subgroup)">
                <xsl:choose>
                  <xsl:when test="$subgroup='overload'">
                    <xsl:value-of select="/document/reference/apidata/@subgroup" />
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="$subgroup" />
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$group" />
              </xsl:otherwise>
            </xsl:choose>
            <xsl:text>TopicTitle</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:attribute>
      <parameter>
        <xsl:call-template name="shortNameDecorated">
          <xsl:with-param name="titleType" select="$titleType" />
        </xsl:call-template>
      </parameter>
      <parameter>
        <xsl:if test="document/reference/memberdata/@overload" >
          <xsl:for-each select="/document/reference">
            <xsl:call-template name="parameterTypesDecorated" />
          </xsl:for-each>
        </xsl:if>
      </parameter>
    </include>
  </xsl:template>

	
	<!-- Title in TOC -->

	<!-- Index entry -->
	
	<!-- main window -->

  <xsl:template name="main">
    <div id="mainSection">

      <div id="mainBody">
        <div id="allHistory" class="saveHistory" onsave="saveAll()" onload="loadAll()"/>
        <xsl:call-template name="head" />
        <xsl:call-template name="body" />
        <xsl:call-template name="foot" />
      </div>
    </div>
    
  </xsl:template>

	<xsl:template name="head">
		<include item="header" />
	</xsl:template>
  
  <xsl:template name="syntaxBlocks">
    <table class="filter" cellspacing="0" cellpadding="0">
      <tr id="curvedSyntaxTabs">
        <xsl:for-each select="div[@codeLanguage]">
          <td class="leftTab" x-lang="{@codeLanguage}">&#xa0;</td>
          <td class="middleTab" x-lang="{@codeLanguage}">&#xa0;</td>
          <td class="rightTab" x-lang="{@codeLanguage}">&#xa0;</td>
        </xsl:for-each>
      </tr>
      <tr class="tabs" id="syntaxTabs">
        <xsl:for-each select="div[@codeLanguage]">
          
          <xsl:variable name="style">
            <xsl:call-template name="languageCheck">
              <xsl:with-param name="codeLanguage" select="@codeLanguage" />
            </xsl:call-template>
          </xsl:variable>
          
          <xsl:variable name="languageEvent">
            <xsl:choose>
            <xsl:when test="$style != ''">
              <xsl:text>changeLanguage(data, '</xsl:text><xsl:value-of select="@codeLanguage"/>
              <xsl:text>', '</xsl:text><xsl:value-of select="$style" />
              <xsl:text>');</xsl:text>
            </xsl:when>
              <xsl:otherwise>
                <xsl:text>toggleClass('syntaxTabs','x-lang','</xsl:text><xsl:value-of select="@codeLanguage"/>
                <xsl:text>','activeTab','tab'); curvedToggleClass('curvedSyntaxTabs','x-lang','</xsl:text><xsl:value-of select="@codeLanguage"/>
                <xsl:text>');toggleStyle('syntaxBlocks','x-lang','</xsl:text><xsl:value-of select="@codeLanguage"/>
                <xsl:text>','display','block','none');</xsl:text>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>
         
          <td class="leftGrad" x-lang="{@codeLanguage}">&#xa0;</td>
          <td class="tab" x-lang="{@codeLanguage}" onclick="{$languageEvent}"><include item="{@codeLanguage}Label" /></td>
          <td class="rightGrad" x-lang="{@codeLanguage}">&#xa0;</td>
        </xsl:for-each>
       </tr>
    </table>
    <div id="syntaxBlocks">
      <xsl:for-each select="div[@codeLanguage]">
        <xsl:variable name="language" select="@codeLanguage" />
        <div class="code" x-lang="{@codeLanguage}">
          <xsl:if test="/document/USyntax/div/@codeLanguage = $language">
            <div id="{$language}Declaration" onclick="toggleSelect({$language}DeclarationImage,{$language}DeclarationSection);">
              <img id="{$language}DeclarationImage" onmouseover="mouseOverCheck({$language}DeclarationImage,twirlSelectImage,twirlUnSelectImage,twirlSelectHoverImage,twirlUnSelectHoverImage)" onmouseout="mouseOutCheck({$language}DeclarationImage,twirlSelectImage,twirlUnSelectImage,twirlSelectHoverImage,twirlUnSelectHoverImage)">
                <includeAttribute name="src" item="iconPath">
                  <parameter>twirl_selected.gif</parameter>
                </includeAttribute>
                <xsl:text>&#xa0;</xsl:text>
                <span class="syntaxLabel"><include item="declarationLabel" /></span>
              </img>
            </div>
            <br/>
          </xsl:if>
          <div id="{$language}DeclarationSection">
            <pre><xsl:copy-of select="./node()" /></pre>
          </div>
          <xsl:for-each select="/document/USyntax/div[@codeLanguage]">
            <xsl:if test="@codeLanguage = $language">
              <div id="{$language}Usage" onclick="toggleSelect({$language}UsageImage,{$language}UsageSection);">
                <img id="{$language}UsageImage" onmouseover="mouseOverCheck({$language}UsageImage,twirlSelectImage,twirlUnSelectImage,twirlSelectHoverImage,twirlUnSelectHoverImage)" onmouseout="mouseOutCheck({$language}UsageImage,twirlSelectImage,twirlUnSelectImage,twirlSelectHoverImage,twirlUnSelectHoverImage)">
                  <includeAttribute name="src" item="iconPath">
                    <parameter>twirl_selected.gif</parameter>
                  </includeAttribute>
                  <xsl:text>&#xa0;</xsl:text>
                  <span class="syntaxLabel"><include item="usageLabel" /></span>
                </img>
              </div>
              <div id="{$language}UsageSection">
                <pre><xsl:copy-of select="./node()" /></pre>
              </div>
            </xsl:if>
          </xsl:for-each>
        </div>
      </xsl:for-each>
    </div>
  </xsl:template>

  <xsl:template name="languageSyntaxBlock">
    <xsl:param name="language" select="@codeLanguage"/>
    <span codeLanguage="{$language}">
      <table>
        <tr>
          <th>
            <include item="{$language}" />
          </th>
        </tr>
        <tr>
          <td>
            <pre xml:space="preserve"><xsl:text/><xsl:copy-of select="node()"/><xsl:text/></pre>
          </td>
        </tr>
      </table>
    </span>
  </xsl:template>

	<xsl:template match="elements" mode="root">
		<xsl:if test="count(element) > 0">
           
			<xsl:call-template name="section">
        <xsl:with-param name="toggleSwitch" select="'namespaces'"/>
				<xsl:with-param name="title"><include item="namespacesTitle" /></xsl:with-param>
				<xsl:with-param name="content">
          <div class="listsection">
            <table class="members" id="memberList" frame="lhs" cellspacing="0">
              <tr>
                <th class="nameColumn">
                  <include item="namespaceNameHeader"/>
                </th>
                <th class="descriptionColumn">
                  <include item="namespaceDescriptionHeader" />
                </th>
              </tr>
              <xsl:apply-templates select="element" mode="root">
                <xsl:sort select="apidata/@name" />
              </xsl:apply-templates>
            </table>
          </div>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

  <xsl:template name="namespaceSection">
    <xsl:param name="listSubgroup" />
    <xsl:variable name="header" select="concat($listSubgroup, 'TypesFilterLabel')"/>
    <xsl:call-template name="section">
      <xsl:with-param name="toggleSwitch" select="$listSubgroup"/>
      <xsl:with-param name="title">
        <include item="{$header}" />
      </xsl:with-param>
      <xsl:with-param name="content">
        <xsl:call-template name="namespaceList">
          <xsl:with-param name="listSubgroup" select="$listSubgroup" />
        </xsl:call-template>
      </xsl:with-param>
    </xsl:call-template>
  </xsl:template>

  <xsl:template match="elements" mode="namespace">
    <xsl:call-template name="section">
      <xsl:with-param name="toggleSwitch" select="'types'" />
      <xsl:with-param name="title">
        <include item="typesTitle" />
      </xsl:with-param>
      <xsl:with-param name="content">
        <div id="typeSection">
        <table class="filter" cellspacing="0" cellpadding="0">
          <tr id="curvedTypeTabs">
            <td class="leftTab" value="all">&#xa0;</td>
            <td class="middleTab" value="all">&#xa0;</td>
            <td class="rightTab" value="all">&#xa0;</td>
            <xsl:if test="element/apidata[@subgroup='class']">
              <td class="leftTab" value="class">&#xa0;</td>
              <td class="middleTab" value="class">&#xa0;</td>
              <td class="rightTab" value="class">&#xa0;</td>
            </xsl:if>
            <xsl:if test="element/apidata[@subgroup='structure']">
              <td class="leftTab" value="structure">&#xa0;</td>
              <td class="middleTab" value="structure">&#xa0;</td>
              <td class="rightTab" value="structure">&#xa0;</td>
            </xsl:if>
            <xsl:if test="element/apidata[@subgroup='interface']">
              <td class="leftTab" value="interface">&#xa0;</td>
              <td class="middleTab" value="interface">&#xa0;</td>
              <td class="rightTab" value="interface">&#xa0;</td>
            </xsl:if>
            <xsl:if test="element/apidata[@subgroup='enumeration']">
              <td class="leftTab" value="enumeration">&#xa0;</td>
              <td class="middleTab" value="enumeration">&#xa0;</td>
              <td class="rightTab" value="enumeration">&#xa0;</td>
            </xsl:if>
            <xsl:if test="element/apidata[@subgroup='delegate']">
              <td class="leftTab" value="delegate">&#xa0;</td>
              <td class="middleTab" value="delegate">&#xa0;</td>
              <td class="rightTab" value="delegate">&#xa0;</td>
            </xsl:if>
          </tr>
          <tr class="tabs" id="typeFilter">
            <td class="leftGrad" value="all">&#xa0;</td>
              <td class="tab" value="all" onclick="toggleClass('typeFilter','value','all','activeTab','tab'); curvedToggleClass('curvedTypeTabs','value','all');processSubgroup('all', 'type'); process('typeList','filterElement','type');">
              <include item="allTypesFilterLabel" />
            </td>
            <td class="rightGrad" value="all">&#xa0;</td>
            <xsl:if test="element/apidata[@subgroup='class']">
              <td class="leftGrad" value="class">&#xa0;</td>
                <td class="tab" value="class" onclick="toggleClass('typeFilter','value','class','activeTab','tab'); curvedToggleClass('curvedTypeTabs','value','class'); processSubgroup('class', 'type'); process('typeList','filterElement','type');">
                <include item="classTypesFilterLabel" />
                <xsl:text>&#xa0;</xsl:text>
                <img>
                  <includeAttribute name="src" item="iconPath">
                    <parameter>pubclass.gif</parameter>
                  </includeAttribute>
                  <includeAttribute name="title" item="pubClassAltText" />
                </img>
              </td>
              <td class="rightGrad" value="class">&#xa0;</td>
            </xsl:if>
            <xsl:if test="element/apidata[@subgroup='structure']">
              <td class="leftGrad" value="structure">&#xa0;</td>
                <td class="tab" value="structure" onclick="toggleClass('typeFilter','value','structure','activeTab','tab'); curvedToggleClass('curvedTypeTabs','value','structure'); processSubgroup('structure', 'type'); process('typeList','filterElement','type');">
                <include item="structureTypesFilterLabel" />
                <img>
                  <includeAttribute name="src" item="iconPath">
                    <parameter>pubstructure.gif</parameter>
                  </includeAttribute>
                  <includeAttribute name="title" item="pubStructureAltText" />
                </img>
              </td>
              <td class="rightGrad" value="structure">&#xa0;</td>
            </xsl:if>
            <xsl:if test="element/apidata[@subgroup='interface']">
              <td class="leftGrad" value="interface">&#xa0;</td>
                <td class="tab" value="interface" onclick="toggleClass('typeFilter','value','interface','activeTab','tab'); curvedToggleClass('curvedTypeTabs','value','interface'); processSubgroup('interface','type'); process('typeList','filterElement','type');">
                <include item="interfaceTypesFilterLabel" />
                <img>
                  <includeAttribute name="src" item="iconPath">
                    <parameter>pubinterface.gif</parameter>
                  </includeAttribute>
                  <includeAttribute name="title" item="pubInterfaceAltText" />
                </img>
              </td>
              <td class="rightGrad" value="interface">&#xa0;</td>
            </xsl:if>
            <xsl:if test="element/apidata[@subgroup='enumeration']">
              <td class="leftGrad" value="enumeration">&#xa0;</td>
                <td class="tab" value="enumeration" onclick="toggleClass('typeFilter','value','enumeration','activeTab','tab'); curvedToggleClass('curvedTypeTabs','value','enumeration'); processSubgroup('enumeration','type'); process('typeList','filterElement','type');">
                <include item="enumerationTypesFilterLabel" />
                <img>
                  <includeAttribute name="src" item="iconPath">
                    <parameter>pubenum.gif</parameter>
                  </includeAttribute>
                  <includeAttribute name="title" item="pubEnumerationAltText" />
                </img>
              </td>
              <td class="rightGrad" value="enumeration">&#xa0;</td>
            </xsl:if>
            <xsl:if test="element/apidata[@subgroup='delegate']">
              <td class="leftGrad" value="delegate">&#xa0;</td>
                <td class="tab" value="delegate" onclick="toggleClass('typeFilter','value','delegate','activeTab','tab'); curvedToggleClass('curvedTypeTabs','value','delegate'); processSubgroup('delegate','type'); process('typeList','filterElement','type');">
                <include item="delegateTypesFilterLabel" />
                <img>
                  <includeAttribute name="src" item="iconPath">
                    <parameter>pubdelegate.gif</parameter>
                  </includeAttribute>
                  <includeAttribute name="title" item="pubDelegateAltText" />
                </img>
              </td>
              <td class="rightGrad" value="delegate">&#xa0;</td>
            </xsl:if>
          </tr>
        </table>
        <div class="memberSection">
          <table id="typeList" class="members" cellspacing="0">
          <tr>
            <th class="iconColumn">
              <xsl:text>&#xa0;</xsl:text>
            </th>
            <th class="nameColumn">
              <include item="typeNameHeader"/>
            </th>
            <th class="descriptionColumn">
              <include item="typeDescriptionHeader" />
            </th>
          </tr>
          <xsl:apply-templates select="element" mode="namespace">
            <xsl:sort select="apidata/@name" />
          </xsl:apply-templates>
        </table>
        </div>
        </div>
      </xsl:with-param>
    </xsl:call-template>
   
  </xsl:template>

  <xsl:template name="namespaceList">
    <xsl:param name="listSubgroup" />

    <table id="typeList" class="members" frame="lhs">
      <tr>
        <th class="iconColumn">
          &#160;
       </th>
        <th class="nameColumn">
          <include item="{$listSubgroup}NameHeader"/>
        </th>
        <th class="descriptionColumn">
          <include item="typeDescriptionHeader" />
        </th>
      </tr>
      <xsl:apply-templates select="element[apidata/@subgroup=$listSubgroup]" mode="namespace">
        <xsl:sort select="@api" />
      </xsl:apply-templates>
    </table>
    
  </xsl:template>

  <xsl:template match="elements" mode="enumeration">
    <xsl:if test="count(element) > 0">
      <div id="enumerationSection">
      <xsl:call-template name="section">
        <xsl:with-param name="toggleSwitch" select="'members'"/>
        <xsl:with-param name="title">
          <include item="enumMembersTitle" />
        </xsl:with-param>
        <xsl:with-param name="content">
          <div class="listSection">
            <table class="members" id="memberList" frame="lhs" cellspacing="0">
              <tr>
                <th class="nameColumn">
                  <include item="memberNameHeader"/>
                </th>
                <th class="descriptionColumn">
                  <include item="memberDescriptionHeader" />
                </th>
              </tr>
              <!-- do not sort enumeration elements -->
              <xsl:apply-templates select="element" mode="enumeration"/>
            </table>
          </div>
        </xsl:with-param>
      </xsl:call-template>
      </div>
    </xsl:if>
  </xsl:template>

  <xsl:template match="element" mode="members">
    <xsl:param name="subgroup"/>
      <xsl:if test="memberdata[@visibility='public'] and apidata[@subgroup=$subgroup]">
          public;
      </xsl:if>
      <xsl:if test="memberdata[@visibility='family' or @visibility='family or assembly' or @visibility='assembly'] and apidata[@subgroup=$subgroup]">
        protected;
      </xsl:if>
      <xsl:if test="memberdata[@visibility='private'] and apidata[@subgroup=$subgroup] and not(proceduredata[@virtual = 'true'])">
        private;
      </xsl:if>
      <xsl:if test="memberdata[@visibility='private'] and proceduredata[@virtual = 'true']">
        explicit;
      </xsl:if>
  </xsl:template>

  <xsl:template match="elements" mode="member">

    <xsl:call-template name="memberIntro" />

    <xsl:if test="count(element) > 0">
      <xsl:variable name="header">
        <xsl:choose>
          <xsl:when test="element[apidata/@subsubgroup]">
            <xsl:value-of select="concat(element/apidata/@subsubgroup, 'MembersFilterLabel')"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="concat(element/apidata/@subgroup, 'MembersFilterLabel')"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:call-template name="section">
        <xsl:with-param name="toggleSwitch" select="$header" />
        <xsl:with-param name="title">
          <include item="{$header}" />
        </xsl:with-param>
        <xsl:with-param name="content">
          <div class="listSection">
            <table class="memberOptions">
              <tr>
                <td class="line">
                  <div id="public" onclick="var checked=toggleCheck(publicImage); toggleCheckState('public',checked); process('memberList','filterElement','member');">
                    <img id="publicImage" onmouseover="mouseOverCheck(publicImage,checkBoxSelectImage,checkBoxUnSelectImage,checkBoxSelectHoverImage,checkBoxUnSelectHoverImage)" onmouseout="mouseOutCheck(publicImage,checkBoxSelectImage,checkBoxUnSelectImage,checkBoxSelectHoverImage,checkBoxUnSelectHoverImage)">
                      <includeAttribute name="src" item="iconPath">
                        <parameter>ch_selected.gif</parameter>
                      </includeAttribute>
                    </img>
                    <xsl:text>&#xa0;</xsl:text>
                    <include item="publicMembersFilterLabel" />
                  </div>
                  <br />
                  <div id="protected" onclick="var checked=toggleCheck(protectedImage); toggleCheckState('protected',checked); process('memberList','filterElement','member');">
                    <img id="protectedImage" onmouseover="mouseOverCheck(protectedImage,checkBoxSelectImage,checkBoxUnSelectImage,checkBoxSelectHoverImage,checkBoxUnSelectHoverImage)" onmouseout="mouseOutCheck(protectedImage,checkBoxSelectImage,checkBoxUnSelectImage,checkBoxSelectHoverImage,checkBoxUnSelectHoverImage)">
                      <includeAttribute name="src" item="iconPath">
                        <parameter>ch_selected.gif</parameter>
                      </includeAttribute>
                    </img>
                    <xsl:text>&#xa0;</xsl:text>
                    <include item="protectedMembersFilterLabel" />
                  </div>
                </td>
                <td class="line">
                  <div id="instance" onclick="var checked=toggleCheck(instanceImage); toggleCheckState('instance',checked); process('memberList','filterElement','member');">
                    <img id="instanceImage" onmouseover="mouseOverCheck(instanceImage,checkBoxSelectImage,checkBoxUnSelectImage,checkBoxSelectHoverImage,checkBoxUnSelectHoverImage)" onmouseout="mouseOutCheck(instanceImage,checkBoxSelectImage,checkBoxUnSelectImage,checkBoxSelectHoverImage,checkBoxUnSelectHoverImage)">
                      <includeAttribute name="src" item="iconPath">
                        <parameter>ch_selected.gif</parameter>
                      </includeAttribute>
                    </img>
                    <xsl:text>&#xa0;</xsl:text>
                    <include item="instanceMembersFilterLabel" />
                  </div>
                  <br />
                  <div id="static" onclick="var checked=toggleCheck(staticImage); toggleCheckState('static',checked); process('memberList','filterElement','member');">
                    <img id="staticImage" onmouseover="mouseOverCheck(staticImage,checkBoxSelectImage,checkBoxUnSelectImage,checkBoxSelectHoverImage,checkBoxUnSelectHoverImage)" onmouseout="mouseOutCheck(staticImage,checkBoxSelectImage,checkBoxUnSelectImage,checkBoxSelectHoverImage,checkBoxUnSelectHoverImage)">
                      <includeAttribute name="src" item="iconPath">
                        <parameter>ch_selected.gif</parameter>
                      </includeAttribute>
                    </img>
                    <xsl:text>&#xa0;</xsl:text>
                    <include item="staticMembersFilterLabel" />
                  </div>
                </td>
                <td class="line">
                  <div id="declared" onclick="var checked=toggleCheck(declaredImage); toggleCheckState('declared',checked); process('memberList','filterElement','member');">
                    <img id="declaredImage" onmouseover="mouseOverCheck(declaredImage,checkBoxSelectImage,checkBoxUnSelectImage,checkBoxSelectHoverImage,checkBoxUnSelectHoverImage)" onmouseout="mouseOutCheck(declaredImage,checkBoxSelectImage,checkBoxUnSelectImage,checkBoxSelectHoverImage,checkBoxUnSelectHoverImage)">
                      <includeAttribute name="src" item="iconPath">
                        <parameter>ch_selected.gif</parameter>
                      </includeAttribute>
                    </img>
                    <xsl:text>&#xa0;</xsl:text>
                    <include item="declaredMembersFilterLabel" />
                  </div>
                  <br />
                  <div id="inherited" onclick="var checked=toggleCheck(inheritedImage); toggleCheckState('inherited',checked); process('memberList','filterElement','member');">
                    <img id="inheritedImage" onmouseover="mouseOverCheck(inheritedImage,checkBoxSelectImage,checkBoxUnSelectImage,checkBoxSelectHoverImage,checkBoxUnSelectHoverImage)" onmouseout="mouseOutCheck(inheritedImage,checkBoxSelectImage,checkBoxUnSelectImage,checkBoxSelectHoverImage,checkBoxUnSelectHoverImage)">
                      <includeAttribute name="src" item="iconPath">
                        <parameter>ch_selected.gif</parameter>
                      </includeAttribute>
                    </img>
                    <xsl:text>&#xa0;</xsl:text>
                    <include item="inheritedMembersFilterLabel" />
                  </div>
                </td>
                <td class="line">
                  <div id="xna" onclick="var checked=toggleCheck(xnaImage); toggleCheckState('xna',checked); process('memberList','filterElement','member');">
                    <img id="xnaImage" onmouseover="mouseOverCheck(xnaImage,checkBoxSelectImage,checkBoxUnSelectImage,checkBoxSelectHoverImage,checkBoxUnSelectHoverImage)" onmouseout="mouseOutCheck(xnaImage,checkBoxSelectImage,checkBoxUnSelectImage,checkBoxSelectHoverImage,checkBoxUnSelectHoverImage)">
                      <includeAttribute name="src" item="iconPath">
                        <parameter>ch_selected.gif</parameter>
                      </includeAttribute>
                    </img>
                    <xsl:text>&#xa0;</xsl:text>
                    <include item="XNAFilterLabel" />
                  </div>
                  <br/>
                  <div id="compact" onclick="var checked=toggleCheck(compactImage); toggleCheckState('compact',checked); process('memberList','filterElement','member');">
                    <img id="compactImage" onmouseover="mouseOverCheck(compactImage,checkBoxSelectImage,checkBoxUnSelectImage,checkBoxSelectHoverImage,checkBoxUnSelectHoverImage)" onmouseout="mouseOutCheck(compactImage,checkBoxSelectImage,checkBoxUnSelectImage,checkBoxSelectHoverImage,checkBoxUnSelectHoverImage)">
                      <includeAttribute name="src" item="iconPath">
                        <parameter>ch_selected.gif</parameter>
                      </includeAttribute>
                    </img>
                    <xsl:text>&#xa0;</xsl:text>
                    <include item=".NETCompactFilterLabel" />
                  </div>
                </td>
                <td class="line">
                  <br/>
                </td>
              </tr>
            </table>
            <table class="members" id="memberList" cellspacing="0" frame="lhs">
              <tr>
                <th class="iconColumn">
                  <xsl:text>&#xa0;</xsl:text>
                </th>
                <th class="nameColumn">
                  <include item="memberNameHeader"/>
                </th>
                <th class="descriptionColumn">
                  <include item="memberDescriptionHeader" />
                </th>
              </tr>
              <xsl:apply-templates select=".//element[not(child::element)]" mode="member">
                <xsl:sort select="apidata/@name" />
              </xsl:apply-templates>
            </table>
          </div>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:if>

  </xsl:template>

  <xsl:template name="memberlistSectionGroup">
    <xsl:param name="listSubgroup" />

    <xsl:if test="element[apidata[@subgroup=$listSubgroup and not(@subsubgroup)] and memberdata[@visibility='public']]">
      <xsl:call-template name="memberlistSection">
        <xsl:with-param name="listSubgroup" select="$listSubgroup" />
        <xsl:with-param name="listVisibility">public</xsl:with-param>
      </xsl:call-template>
    </xsl:if>

    <xsl:if test="element[apidata[@subgroup=$listSubgroup and not(@subsubgroup)] and memberdata[@visibility='protected']]">
      <xsl:call-template name="memberlistSection">
        <xsl:with-param name="listSubgroup" select="$listSubgroup" />
        <xsl:with-param name="listVisibility">protected</xsl:with-param>
      </xsl:call-template>
    </xsl:if>

  </xsl:template>
  
  <xsl:template name="memberlistSection">
    <xsl:param name="listSubgroup" />
    <xsl:param name="listSubsubgroup" />
    <xsl:param name="listVisibility" />
    <xsl:param name="explicit" />

    <xsl:variable name="header">
      <xsl:choose>
        <xsl:when test="$explicit='true'">ExplicitInterfaceImplementation</xsl:when>
        <xsl:when test="$listSubgroup='constructor'">constructorsTable</xsl:when>
        <xsl:when test="boolean($listSubsubgroup)">
          <xsl:value-of select="concat('Public', $listSubsubgroup)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="concat($listVisibility, $listSubgroup)"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:call-template name="section">
      <xsl:with-param name="toggleSwitch" select="$header" />
      <xsl:with-param name="title">
        <include item="{$header}" />
      </xsl:with-param>
      <xsl:with-param name="content">
        <table id="typeList" class="members" frame="lhs">
          <tr>
            <th class="iconColumn">
              &#160;
            </th>
            <th class="nameColumn">
              <include item="typeNameHeader"/>
            </th>
            <th class="descriptionColumn">
              <include item="typeDescriptionHeader" />
            </th>
          </tr>

          <xsl:choose>
            <xsl:when test="boolean($listSubgroup) and boolean($useOverloadRowsInMemberlists)">
              <xsl:apply-templates select="element[not(starts-with(@api,'Overload:'))][apidata[@subgroup=$listSubgroup and not(@subsubgroup)] and memberdata[@visibility=$listVisibility]] 
                        | element[starts-with(@api,'Overload:')][element[apidata[@subgroup=$listSubgroup and not(@subsubgroup)] and memberdata[@visibility=$listVisibility]]]" 
                        mode="memberlistRow">
                <xsl:sort select="apidata/@name" />
                <xsl:with-param name="listVisibility" select="$listVisibility"/>
              </xsl:apply-templates>
            </xsl:when>
            <xsl:when test="boolean($listSubgroup)">
              <xsl:apply-templates select="element[not(starts-with(@api,'Overload:'))][apidata[@subgroup=$listSubgroup and not(@subsubgroup)] and memberdata[@visibility=$listVisibility]] 
                        | element[starts-with(@api,'Overload:')]/element[apidata[@subgroup=$listSubgroup and not(@subsubgroup)] and memberdata[@visibility=$listVisibility]]" 
                        mode="memberlistRow">
                <xsl:sort select="apidata/@name" />
                <xsl:with-param name="listVisibility" select="$listVisibility"/>
              </xsl:apply-templates>
            </xsl:when>
            <xsl:when test="boolean($listSubsubgroup)">
              <xsl:apply-templates select="element[apidata[@subsubgroup=$listSubsubgroup]]" mode="memberlistRow">
                <xsl:sort select="apidata/@name" />
                <xsl:with-param name="listVisibility" select="$listVisibility"/>
              </xsl:apply-templates>
            </xsl:when>
            <xsl:when test="$explicit='true'">
              <xsl:apply-templates select="element[memberdata[@visibility='private'] and proceduredata[@virtual = 'true']]" mode="memberlistRow">
                <xsl:sort select="apidata/@name" />
              </xsl:apply-templates>
            </xsl:when>
          </xsl:choose>
        </table>
      </xsl:with-param>
    </xsl:call-template>

  </xsl:template>

  <xsl:template match="elements" mode="type">
    <xsl:if test="count(element) > 0">
      <xsl:call-template name="section">
        <xsl:with-param name="toggleSwitch" select="'members'" />
        <xsl:with-param name="title">
          <include item="allMembersTitle" />
        </xsl:with-param>
        <xsl:with-param name="content">
          <div id="allMemberSection">
          <table class="filter" cellspacing="0" cellpadding="0">
            <tr id="curvedMemberTabs">
              <td class="leftTab" value="all">&#xa0;</td>
              <td class="middleTab" value="all">&#xa0;</td>
              <td class="rightTab" value="all">&#xa0;</td>
              <xsl:if test="element/apidata[@subgroup='constructor']">
                <td class="leftTab" value="constructor">&#xa0;</td>
                <td class="middleTab" value="constructor">&#xa0;</td>
                <td class="rightTab" value="constructor">&#xa0;</td>
              </xsl:if>
              <xsl:if test="element/apidata[@subgroup='field']">
                <td class="leftTab" value="field">&#xa0;</td>
                <td class="middleTab" value="field">&#xa0;</td>
                <td class="rightTab" value="field">&#xa0;</td>
              </xsl:if>
              <xsl:if test="element/apidata[@subgroup='property']">
                <td class="leftTab" value="property">&#xa0;</td>
                <td class="middleTab" value="property">&#xa0;</td>
                <td class="rightTab" value="property">&#xa0;</td>
              </xsl:if>
              <xsl:if test="element/apidata[@subgroup='method']">
                <td class="leftTab" value="method">&#xa0;</td>
                <td class="middleTab" value="method">&#xa0;</td>
                <td class="rightTab" value="method">&#xa0;</td>
              </xsl:if>
              <xsl:if test="element/apidata[@subgroup='event']">
                <td class="leftTab" value="event">&#xa0;</td>
                <td class="middleTab" value="event">&#xa0;</td>
                <td class="rightTab" value="event">&#xa0;</td>
              </xsl:if>
              <xsl:if test="element/apidata[@subsubgroup='attachedProperty']">
                <td class="leftTab" value="attachedProperty">&#xa0;</td>
                <td class="middleTab" value="attachedProperty">&#xa0;</td>
                <td class="rightTab" value="attachedProperty">&#xa0;</td>
              </xsl:if>
              <xsl:if test="element/apidata[@subsubgroup='attachedEvent']">
                <td class="leftTab" value="attachedEvent">&#xa0;</td>
                <td class="middleTab" value="attachedEvent">&#xa0;</td>
                <td class="rightTab" value="attachedEvent">&#xa0;</td>
              </xsl:if>
              <xsl:if test="element[memberdata[@visibility='private'] and proceduredata[@virtual = 'true']]">
                <td class="leftTab" value="explicit">&#xa0;</td>
                <td class="middleTab" value="explicit">&#xa0;</td>
                <td class="rightTab" value="explicit">&#xa0;</td>
              </xsl:if>
            </tr>
            <tr class="tabs" id="memberTabs">
              <td class="leftGrad" value="all">&#xa0;</td>
                <td class="tab" value="all" onclick="toggleClass('memberTabs','value','all','activeTab','tab'); curvedToggleClass('curvedMemberTabs', 'value', 'all'); processSubgroup('all','member'); process('memberList','filterElement','member');">
                <include item="allMembersFilterLabel" />
              </td>
              <td class="rightGrad" value="all">&#xa0;</td>
              <xsl:if test="element/apidata[@subgroup='constructor']">
                <td class="leftGrad" value="constructor">&#xa0;</td>
                  <td class="tab" value="constructor" onclick="toggleClass('memberTabs','value','constructor','activeTab','tab'); curvedToggleClass('curvedMemberTabs', 'value', 'constructor'); processSubgroup('constructor','member'); process('memberList','filterElement','member');">
                  <include item="constructorMembersFilterLabel" />
                  <xsl:text>&#xa0;</xsl:text>
                  <img>
                    <includeAttribute name="src" item="iconPath">
                      <parameter>pubmethod.gif</parameter>
                    </includeAttribute>
                    <includeAttribute name="title" item="pubMethodAltText" />
                  </img>
                </td>
                <td class="rightGrad" value="constructor">&#xa0;</td>
              </xsl:if>
              <xsl:if test="element/apidata[@subgroup='field']">
                <td class="leftGrad" value="field">&#xa0;</td>
                  <td class="tab" value="field" onclick="toggleClass('memberTabs','value','field','activeTab','tab'); curvedToggleClass('curvedMemberTabs', 'value', 'field'); processSubgroup('field','member'); process('memberList','filterElement','member');">
                  <include item="fieldMembersFilterLabel" />
                  <xsl:text>&#xa0;</xsl:text>
                  <img>
                    <includeAttribute name="src" item="iconPath">
                      <parameter>pubfield.gif</parameter>
                    </includeAttribute>
                    <includeAttribute name="title" item="pubFieldAltText" />
                  </img>
                </td>
                <td class="rightGrad" value="field">&#xa0;</td>
              </xsl:if>
              <xsl:if test="element/apidata[@subgroup='property' and not(@subsubgroup)]">
                <td class="leftGrad" value="property">&#xa0;</td>
                  <td class="tab" value="property" onclick="toggleClass('memberTabs','value','property','activeTab','tab'); curvedToggleClass('curvedMemberTabs', 'value', 'property'); processSubgroup('property','member'); process('memberList','filterElement','member');">
                  <include item="propertyMembersFilterLabel" />
                  <xsl:text>&#xa0;</xsl:text>
                  <img>
                    <includeAttribute name="src" item="iconPath">
                      <parameter>pubproperty.gif</parameter>
                    </includeAttribute>
                    <includeAttribute name="title" item="pubPropertyAltText" />
                  </img>
                </td>
                <td class="rightGrad" value="property">&#xa0;</td>
              </xsl:if>
              <xsl:if test="element/apidata[@subgroup='method']">
                <td class="leftGrad" value="method">&#xa0;</td>
                  <td class="tab" value="method" onclick="toggleClass('memberTabs','value','method','activeTab','tab'); curvedToggleClass('curvedMemberTabs', 'value', 'method'); processSubgroup('method','member'); process('memberList','filterElement','member');">
                  <include item="methodMembersFilterLabel" />
                  <xsl:text>&#xa0;</xsl:text>
                  <img>
                    <includeAttribute name="src" item="iconPath">
                      <parameter>pubmethod.gif</parameter>
                    </includeAttribute>
                    <includeAttribute name="title" item="pubMethodAltText" />
                  </img>
                </td>
                <td class="rightGrad" value="method">&#xa0;</td>
              </xsl:if>
              <xsl:if test="element/apidata[@subgroup='event' and not(@subsubgroup)]">
                <td class="leftGrad" value="event">&#xa0;</td>
                  <td class="tab" value="event" onclick="toggleClass('memberTabs','value','event','activeTab','tab'); curvedToggleClass('curvedMemberTabs', 'value', 'event'); processSubgroup('event','member'); process('memberList','filterElement','member');">
                  <include item="eventMembersFilterLabel" />
                  <xsl:text>&#xa0;</xsl:text>
                  <img>
                    <includeAttribute name="src" item="iconPath">
                      <parameter>pubevent.gif</parameter>
                    </includeAttribute>
                    <includeAttribute name="title" item="pubEventAltText" />
                  </img>
                </td>
                <td class="rightGrad" value="event">&#xa0;</td>
              </xsl:if>
              <xsl:if test="element/apidata[@subsubgroup='attachedProperty']">
                <td class="leftGrad" value="attachedProperty">&#xa0;</td>
                  <td class="tab" value="attachedProperty" onclick="toggleClass('memberTabs','value','attachedProperty','activeTab','tab'); curvedToggleClass('curvedMemberTabs', 'value', 'attachedProperty'); processSubgroup('attachedProperty','member'); process('memberList','filterElement','member');">
                  <include item="attachedPropertyMembersFilterLabel" />
                  <xsl:text>&#xa0;</xsl:text>
                  <img>
                    <includeAttribute name="src" item="iconPath">
                      <parameter>pubproperty.gif</parameter>
                    </includeAttribute>
                    <includeAttribute name="title" item="pubPropertyAltText" />
                  </img>
                </td>
                <td class="rightGrad" value="attachedProperty">&#xa0;</td>
              </xsl:if>
              <xsl:if test="element/apidata[@subsubgroup='attachedEvent']">
                <td class="leftGrad" value="attachedEvent">&#xa0;</td>
                  <td class="tab" value="attachedEvent" onclick="toggleClass('memberTabs','value','attachedEvent','activeTab','tab'); curvedToggleClass('curvedMemberTabs', 'value', 'attachedEvent'); processSubgroup('attachedEvent','member'); process('memberList', 'filterElement', 'member');">
                  <include item="attachedEventMembersFilterLabel" />
                  <xsl:text>&#xa0;</xsl:text>
                  <img>
                    <includeAttribute name="src" item="iconPath">
                      <parameter>pubevent.gif</parameter>
                    </includeAttribute>
                    <includeAttribute name="title" item="pubEventAltText" />
                  </img>
                </td>
                <td class="rightGrad" value="attachedEvent">&#xa0;</td>
              </xsl:if>
              <xsl:if test="element[memberdata[@visibility='private'] and proceduredata[@virtual = 'true']]">
                <td class="leftGrad" value="explicit">&#xa0;</td>
                  <td class="tab" value="explicit" onclick="toggleClass('memberTabs','value','explicit','activeTab','tab'); curvedToggleClass('curvedMemberTabs', 'value', 'explicit'); processSubgroup('explicit','member'); process('memberList', 'filterElement', 'member');">
                  <include item="explicitInterfaceMembersFilterLabel" />
                  <xsl:text>&#xa0;</xsl:text>
                  <img>
                    <includeAttribute name="src" item="iconPath">
                      <parameter>pubinterface.gif</parameter>
                    </includeAttribute>
                    <includeAttribute name="title" item="ExplicitInterfaceAltText" />
                  </img>
                </td>
                <td class="rightGrad" value="explicit">&#xa0;</td>
              </xsl:if>
            </tr>
          </table>
          <div class="memberSection">
            <table class="memberOptions">
              <tr>
                <td class="line">
                    <div id="public" onclick="var checked=toggleCheck(publicImage); toggleCheckState('public',checked); process('memberList','filterElement','member');">
                    <img id="publicImage" onmouseover="mouseOverCheck(publicImage,checkBoxSelectImage,checkBoxUnSelectImage,checkBoxSelectHoverImage,checkBoxUnSelectHoverImage)" onmouseout="mouseOutCheck(publicImage,checkBoxSelectImage,checkBoxUnSelectImage,checkBoxSelectHoverImage,checkBoxUnSelectHoverImage)">
                      <includeAttribute name="src" item="iconPath">
                        <parameter>ch_selected.gif</parameter>
                      </includeAttribute>
                    </img>
                    <xsl:text>&#xa0;</xsl:text>
                    <include item="publicMembersFilterLabel" />
                  </div>
                <br />
                    <div id="protected" onclick="var checked=toggleCheck(protectedImage); toggleCheckState('protected',checked); process('memberList','filterElement','member');">
                  <img id="protectedImage" onmouseover="mouseOverCheck(protectedImage,checkBoxSelectImage,checkBoxUnSelectImage,checkBoxSelectHoverImage,checkBoxUnSelectHoverImage)" onmouseout="mouseOutCheck(protectedImage,checkBoxSelectImage,checkBoxUnSelectImage,checkBoxSelectHoverImage,checkBoxUnSelectHoverImage)">
                    <includeAttribute name="src" item="iconPath">
                      <parameter>ch_selected.gif</parameter>
                    </includeAttribute>
                  </img>
                  <xsl:text>&#xa0;</xsl:text>
                    <include item="protectedMembersFilterLabel" />
                  </div>
                </td>
                <td class="line">
                    <div id="instance" onclick="var checked=toggleCheck(instanceImage); toggleCheckState('instance',checked); process('memberList','filterElement','member');">
                    <img id="instanceImage" onmouseover="mouseOverCheck(instanceImage,checkBoxSelectImage,checkBoxUnSelectImage,checkBoxSelectHoverImage,checkBoxUnSelectHoverImage)" onmouseout="mouseOutCheck(instanceImage,checkBoxSelectImage,checkBoxUnSelectImage,checkBoxSelectHoverImage,checkBoxUnSelectHoverImage)">
                      <includeAttribute name="src" item="iconPath">
                        <parameter>ch_selected.gif</parameter>
                      </includeAttribute>
                    </img>
                    <xsl:text>&#xa0;</xsl:text>
                    <include item="instanceMembersFilterLabel" />
                  </div>
                  <br />
                    <div id="static" onclick="var checked=toggleCheck(staticImage); toggleCheckState('static',checked); process('memberList','filterElement','member');">
                    <img id="staticImage" onmouseover="mouseOverCheck(staticImage,checkBoxSelectImage,checkBoxUnSelectImage,checkBoxSelectHoverImage,checkBoxUnSelectHoverImage)" onmouseout="mouseOutCheck(staticImage,checkBoxSelectImage,checkBoxUnSelectImage,checkBoxSelectHoverImage,checkBoxUnSelectHoverImage)">
                      <includeAttribute name="src" item="iconPath">
                        <parameter>ch_selected.gif</parameter>
                      </includeAttribute>
                    </img>
                    <xsl:text>&#xa0;</xsl:text>
                    <include item="staticMembersFilterLabel" />
                    <xsl:text>&#xa0;</xsl:text>
                    <img>
                      <includeAttribute name="src" item="iconPath">
                        <parameter>static.gif</parameter>
                      </includeAttribute>
                      <includeAttribute name="title" item="staticAltText" />
                    </img>
                  </div>
                </td>
                <td class="line"> 
                    <div id="declared" onclick="var checked=toggleCheck(declaredImage); toggleCheckState('declared',checked); process('memberList','filterElement','member');">
                    <img id="declaredImage" onmouseover="mouseOverCheck(declaredImage,checkBoxSelectImage,checkBoxUnSelectImage,checkBoxSelectHoverImage,checkBoxUnSelectHoverImage)" onmouseout="mouseOutCheck(declaredImage,checkBoxSelectImage,checkBoxUnSelectImage,checkBoxSelectHoverImage,checkBoxUnSelectHoverImage)">
                      <includeAttribute name="src" item="iconPath">
                        <parameter>ch_selected.gif</parameter>
                      </includeAttribute>
                    </img>
                    <xsl:text>&#xa0;</xsl:text>
                    <include item="declaredMembersFilterLabel" />
                  </div>
                  <br />
                    <div id="inherited" onclick="var checked=toggleCheck(inheritedImage); toggleCheckState('inherited',checked); process('memberList','filterElement','member');">
                    <img id="inheritedImage" onmouseover="mouseOverCheck(inheritedImage,checkBoxSelectImage,checkBoxUnSelectImage,checkBoxSelectHoverImage,checkBoxUnSelectHoverImage)" onmouseout="mouseOutCheck(inheritedImage,checkBoxSelectImage,checkBoxUnSelectImage,checkBoxSelectHoverImage,checkBoxUnSelectHoverImage)">
                      <includeAttribute name="src" item="iconPath">
                        <parameter>ch_selected.gif</parameter>
                      </includeAttribute>
                    </img>
                    <xsl:text>&#xa0;</xsl:text>
                    <include item="inheritedMembersFilterLabel" />
                  </div>
                </td>
                <td class="line">
                    <div id="xna" onclick="var checked=toggleCheck(xnaImage); toggleCheckState('xna',checked); process('memberList','filterElement','member');">
                    <img id="xnaImage" onmouseover="mouseOverCheck(xnaImage,checkBoxSelectImage,checkBoxUnSelectImage,checkBoxSelectHoverImage,checkBoxUnSelectHoverImage)" onmouseout="mouseOutCheck(xnaImage,checkBoxSelectImage,checkBoxUnSelectImage,checkBoxSelectHoverImage,checkBoxUnSelectHoverImage)">
                      <includeAttribute name="src" item="iconPath">
                        <parameter>ch_selected.gif</parameter>
                      </includeAttribute>
                    </img>
                    <xsl:text>&#xa0;</xsl:text>
                    <include item="XNAFilterLabel" />
                    <xsl:text>&#xa0;</xsl:text>
                    <img>
                      <includeAttribute name="src" item="iconPath">
                        <parameter>xna.gif</parameter>
                      </includeAttribute>
                      <includeAttribute name="title" item="XNAFrameworkAltText" />
                    </img>
                  </div>
                  <br/>
                    <div id="compact" onclick="var checked=toggleCheck(compactImage); toggleCheckState('compact',checked); process('memberList','filterElement','member');">
                    <img id="compactImage" onmouseover="mouseOverCheck(compactImage,checkBoxSelectImage,checkBoxUnSelectImage,checkBoxSelectHoverImage,checkBoxUnSelectHoverImage)" onmouseout="mouseOutCheck(compactImage,checkBoxSelectImage,checkBoxUnSelectImage,checkBoxSelectHoverImage,checkBoxUnSelectHoverImage)">
                      <includeAttribute name="src" item="iconPath">
                        <parameter>ch_selected.gif</parameter>
                      </includeAttribute>
                    </img>
                    <xsl:text>&#xa0;</xsl:text>
                    <include item=".NETCompactFilterLabel" />
                    <xsl:text>&#xa0;</xsl:text>
                  <img>
                    <includeAttribute name="src" item="iconPath">
                      <parameter>CFW.gif</parameter>
                    </includeAttribute>
                    <includeAttribute name="title" item="compactFrameworkAltText" />
                  </img>
                  </div>
                </td>
                <td class="line">
                  <br/>
                </td>
              </tr>
            </table>
            <table class="members" id="memberList" cellspacing="0" frame="lhs">
              <tr>
                <th class="iconColumn">
                  <xsl:text>&#xa0;</xsl:text>
                </th>
                <th class="nameColumn">
                  <include item="memberNameHeader"/>
                </th>
                <th class="descriptionColumn">
                  <include item="memberDescriptionHeader" />
                </th>
              </tr>
              <!-- use select="element" to show overload-sets, select=".//element[not(parent::element)]" to show all overloads -->
              <xsl:apply-templates select=".//element[not(child::element)]" mode="member">
                <xsl:sort select="apidata/@name" />
              </xsl:apply-templates>
            </table>
          </div>
          </div>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:if>
	</xsl:template>

  <xsl:template name="IsMemberSupportedOnXna">
    <xsl:choose>
      <xsl:when test="element and not(@signatureset)">
        <xsl:for-each select="element">
          <xsl:call-template name="IsMemberSupportedOnXna"/>
        </xsl:for-each>
      </xsl:when>
      <xsl:otherwise>
        <xsl:variable name="platformFilterExcludesXna" select="boolean(platforms and not(platforms/platform[.='Xbox360']))" />
        <xsl:if test="boolean(not($platformFilterExcludesXna) and @xnafw)">
          <xsl:text>supported</xsl:text>
        </xsl:if>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="IsMemberSupportedOnCf">
    <xsl:choose>
      <xsl:when test="document/reference/topicdata[@subgroup='overload'] and document/reference/elements/element and not(@signatureset)">
        <xsl:for-each select="document/reference/elements/element">
          <xsl:call-template name="IsMemberSupportedOnCf"/>
        </xsl:for-each>
      </xsl:when> 	
      <xsl:when test="element and not(@signatureset)">
        <xsl:for-each select="element">
          <xsl:call-template name="IsMemberSupportedOnCf"/>
        </xsl:for-each>
      </xsl:when>
      <xsl:otherwise>
        <xsl:variable name="platformFilterExcludesCF" select="boolean( platforms and not(platforms[platform[.='PocketPC'] or platform[.='SmartPhone'] or platform[.='WindowsCE']]) )" />
        <xsl:if test="boolean(not($platformFilterExcludesCF) and @netcfw)">
          <xsl:text>yes</xsl:text>
        </xsl:if>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  
  <xsl:template name="IsMemberStatic">
    <xsl:choose>
      <xsl:when test="element and not(@signatureset)">
        <xsl:for-each select="element">
          <xsl:call-template name="IsMemberStatic"/>
        </xsl:for-each>
      </xsl:when>
      <xsl:when test="apidata[@subsubgroup='attachedProperty' or @subsubgroup='attachedEvent']"/>
      <xsl:otherwise>
        <xsl:if test="memberdata/@static='true'">
          <xsl:text>yes</xsl:text>
        </xsl:if>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  
  <xsl:template name="IsMemberInherited">
    <xsl:choose>
      <xsl:when test="element and not(@signatureset)">
        <xsl:for-each select="element">
          <xsl:call-template name="IsMemberInherited"/>
        </xsl:for-each>
      </xsl:when>
      <xsl:otherwise>
        <xsl:if test="not(contains($key, containers/type/@api))">
          <xsl:text>yes</xsl:text>
        </xsl:if>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  
  <xsl:template name="IsMemberProtected">
    <xsl:choose>
      <xsl:when test="element and not(@signatureset)">
        <xsl:for-each select="element">
          <xsl:call-template name="IsMemberProtected"/>
        </xsl:for-each>
      </xsl:when>
      <xsl:otherwise>
        <xsl:if test="memberdata[@visibility='family' or @visibility='family or assembly' or @visibility='assembly']">
          <xsl:text>yes</xsl:text>
        </xsl:if>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="element" mode="memberlistRow">
    <xsl:param name="listVisibility"/>
    <xsl:variable name="supportedOnXna">
      <xsl:call-template name="IsMemberSupportedOnXna"/>
    </xsl:variable>
    <xsl:variable name="supportedOnCf">
      <xsl:call-template name="IsMemberSupportedOnCf"/>
    </xsl:variable>
    <xsl:variable name="staticMember">
      <xsl:call-template name="IsMemberStatic"/>
    </xsl:variable>
    <xsl:variable name="inheritedMember">
      <xsl:call-template name="IsMemberInherited"/>
    </xsl:variable>
    <xsl:variable name="protectedMember">
      <xsl:call-template name="IsMemberProtected"/>
    </xsl:variable>
    <tr>
      <xsl:if test="normalize-space($inheritedMember)!=''">
        <xsl:attribute name="name">inheritedMember</xsl:attribute>
      </xsl:if>
      <xsl:if test="normalize-space($protectedMember)!=''">
        <xsl:attribute name="protected">true</xsl:attribute>
      </xsl:if>
      <xsl:if test="normalize-space($supportedOnXna)=''">
        <xsl:attribute name="notSupportedOnXna">true</xsl:attribute>
      </xsl:if>
      <xsl:if test="normalize-space($supportedOnCf)=''">
        <xsl:attribute name="notSupportedOn">netcf</xsl:attribute>
      </xsl:if>

      <td>
        <xsl:call-template name="memberIcons">
          <xsl:with-param name="memberVisibility">
            <xsl:choose>
              <xsl:when test="$listVisibility='public'">pub</xsl:when>
              <xsl:when test="$listVisibility='private'">priv</xsl:when>
              <xsl:when test="$listVisibility='protected'">prot</xsl:when>
              <xsl:otherwise>
                <xsl:choose>
                  <xsl:when test="memberdata/@visibility='family' or memberdata/@visibility='family or assembly' or memberdata/@visibility='assembly'">prot</xsl:when>
                  <xsl:when test="memberdata/@visibility='private'">priv</xsl:when>
                  <xsl:otherwise>pub</xsl:otherwise>
                </xsl:choose>
                <xsl:choose>
                  <xsl:when test="memberdata[@visibility='public']">pub</xsl:when>
                </xsl:choose>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:with-param>
          <xsl:with-param name="staticMember" select="normalize-space($staticMember)" />
          <xsl:with-param name="supportedOnXna" select="normalize-space($supportedOnXna)"/>
          <xsl:with-param name="supportedOnCf" select="normalize-space($supportedOnCf)"/>
        </xsl:call-template>
      </td>
      <td>
        <xsl:choose>
          <xsl:when test="@display-api">
            <referenceLink target="{@api}" display-target="{@display-api}" show-parameters="false" />
          </xsl:when>
          <xsl:otherwise>
            <referenceLink target="{@api}" show-parameters="false" />
          </xsl:otherwise>
        </xsl:choose>
      </td>
      <td>
        <xsl:call-template name="getInternalOnlyDescription" />
       	<xsl:if test="attributes/attribute/type[@api='T:System.ObsoleteAttribute']">
          <xsl:text> </xsl:text>
          <include item="obsoleteRed" />
        </xsl:if>
        <xsl:if test="attributes/attribute/type[@api='T:System.Security.Permissions.HostProtectionAttribute']">
          <xsl:text> </xsl:text>
          <include item="hostProtectionAttributeShort" />
        </xsl:if>
        <xsl:if test="topicdata[@subgroup='overload']">
          <include item="Overloaded"/>
          <xsl:text> </xsl:text>
        </xsl:if>
          <xsl:apply-templates select="element" mode="overloadSummary" />
        <xsl:call-template name="getElementDescription" />
        <xsl:choose>
          <xsl:when test="@signatureset">
            <!-- TODO add boilerplate for other members in the sig set -->
          </xsl:when>
          <xsl:when test="not(topicdata[@subgroup='overload'])">
            <xsl:choose>
              <xsl:when test="normalize-space($inheritedMember)!=''">
                <xsl:text> </xsl:text>
                <include item="inheritedFrom">
                  <parameter>
                    <xsl:apply-templates select="containers/type" mode="link" />
                    <!--
                    <xsl:call-template name="typeReferenceLink">
                      <xsl:with-param name="api" select="containers/type/@api" />
                      <xsl:with-param name="qualified" select="false()" />
                      <xsl:with-param name="specialization" select="boolean(type/specialization)" />
                    </xsl:call-template>
                    <xsl:apply-templates select="type/specialization" />
                    -->
                  </parameter>
                </include>
              </xsl:when>
              <xsl:when test="overrides/member">
                <xsl:text> </xsl:text>
                <include item="overridesMember">
                  <parameter>
                    <xsl:call-template name="createReferenceLink">
                      <xsl:with-param name="id" select="overrides/member/@api"/>
                      <xsl:with-param name="qualified" select="true()"/>
                    </xsl:call-template>
                  </parameter>
                </include>
              </xsl:when>
            </xsl:choose>
          </xsl:when>
        </xsl:choose>
      </td>
    </tr>
  </xsl:template>

  <xsl:template match="elements" mode="derivedType">
    <xsl:if test="count(element) > 0">
      <xsl:call-template name="section">
        <xsl:with-param name="toggleSwitch" select="'DerivedClasses'"/>
        <xsl:with-param name="title">
          <include item="derivedClasses" />
        </xsl:with-param>
        <xsl:with-param name="content">
          <div class="listSection">
            <table class="members" id="memberList" frame="lhs" cellspacing="0">
              <tr>
                <th class="nameColumn">
                  <include item="memberNameHeader"/>
                </th>
                <th class="descriptionColumn">
                  <include item="memberDescriptionHeader" />
                </th>
              </tr>
              <xsl:apply-templates select="element" mode="derivedType">
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
        <xsl:with-param name="toggleSwitch" select="'overloadMembers'"/>
				<xsl:with-param name="title"><include item="membersTitle" /></xsl:with-param>
				<xsl:with-param name="content">
          <div class="listSection">
            <table class="members" id="memberList" frame="lhs" cellspacing="0">
              <tr>
                <th class="nameColumn">
                  <include item="typeNameHeader"/>
                </th>
                <th class="descriptionColumn">
                  <include item="typeDescriptionHeader" />
                </th>
              </tr>
              <xsl:apply-templates select="element" mode="overload">
                <xsl:sort select="apidata/@name" />
              </xsl:apply-templates>
            </table>
          </div>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
    <xsl:apply-templates select="element" mode="overloadSections">
      <xsl:sort select="apidata/@name" />
    </xsl:apply-templates>
  </xsl:template>

  <xsl:template match="elements" mode="overloadSummary">
    <xsl:apply-templates select="element" mode="overloadSummary" >
      <xsl:sort select="apidata/@name"/>
    </xsl:apply-templates>
  </xsl:template>

  <xsl:template match="element" mode="overloadSummary">
    <xsl:call-template name="getOverloadSummary" />
  </xsl:template>

  <xsl:template match="element" mode="overloadSections">
    <xsl:call-template name="getOverloadSections" />
  </xsl:template>

	<xsl:template name="typeIcon">
    <xsl:param name="typeVisibility" />

    <xsl:variable name="typeSubgroup" select="apidata/@subgroup" />
    <img>
      <includeAttribute name="src" item="iconPath">
        <parameter>
          <xsl:value-of select="concat($typeVisibility,$typeSubgroup,'.gif')" />
        </parameter>
      </includeAttribute>
      <includeAttribute name="title" item="{concat($typeVisibility,$typeSubgroup,'AltText')}" />
    </img>
   
  </xsl:template>

	<xsl:template name="memberIcons">
    <xsl:param name="memberVisibility" />
    <xsl:param name="staticMember" />
    <xsl:param name="supportedOnXna"/>
    <xsl:param name="supportedOnCf"/>
    
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

    <!-- test for explicit interface implementations, which get the interface icon -->
    <xsl:if test="memberdata/@visibility='private' and proceduredata/@virtual='true'">
      <img>
        <includeAttribute name="src" item="iconPath">
          <parameter>pubinterface.gif</parameter>
        </includeAttribute>
        <includeAttribute name="title" item="ExplicitInterfaceAltText" />
      </img>
    </xsl:if>
    
    <img>
      <includeAttribute name="src" item="iconPath">
        <parameter>
          <xsl:value-of select="concat($memberVisibility,$memberSubgroup,'.gif')" />
        </parameter>
      </includeAttribute>
      <includeAttribute name="title" item="{concat($memberVisibility,$memberSubgroup,'AltText')}" />
    </img>

    <xsl:if test="$staticMember!=''">
      <img>
        <includeAttribute name="src" item="iconPath">
          <parameter>static.gif</parameter>
        </includeAttribute>
        <includeAttribute name="title" item="staticAltText" />
      </img>
    </xsl:if>
    
    <xsl:if test="$supportedOnCf!=''">
      <img>
        <includeAttribute name="src" item="iconPath">
          <parameter>CFW.gif</parameter>
        </includeAttribute>
        <includeAttribute name="title" item="CompactFrameworkAltText" />
      </img>
    </xsl:if>
    
    <xsl:if test="$supportedOnXna!=''">
      <img>
        <includeAttribute name="src" item="iconPath">
          <parameter>xna.gif</parameter>
        </includeAttribute>
        <includeAttribute name="title" item="XNAFrameworkAltText" />
      </img>
    </xsl:if>
  </xsl:template>

	<!-- Footer stuff -->
	
	<xsl:template name="foot">
    <div id="footer">
      <div class="footerLine">
        <img width="100%" height="3px">
          <includeAttribute name="src" item="iconPath">
            <parameter>footer.gif</parameter>
          </includeAttribute>
          <includeAttribute name="title" item="footerImage" />
        </img>
      </div>

      <include item="footer">
        <parameter>
          <xsl:value-of select="$key"/>
        </parameter>
        <parameter>
          <xsl:call-template name="topicTitlePlain"/>
        </parameter>
      </include>
    </div>
	</xsl:template>

	<!-- Assembly information -->

	<xsl:template name="requirementsInfo">
    <p/>
    <include item="requirementsNamespaceLayout" />
    <xsl:text>&#xa0;</xsl:text>
    <referenceLink target="{/document/reference/containers/namespace/@api}" />
    <br/>
    <xsl:call-template name="assembliesInfo"/>

    <!-- some apis display a XAML xmlns uri -->
    <xsl:call-template name="xamlXmlnsInfo"/>
  </xsl:template>

  <xsl:template name="assemblyNameAndModule">
    <xsl:param name="library" select="/document/reference/containers/library"/>
    <include item="assemblyNameAndModule">
      <parameter>
        <span sdata="assembly">
        <xsl:value-of select="$library/@assembly"/>
        </span>
      </parameter>
      <parameter>
        <xsl:value-of select="$library/@module"/>
      </parameter>
      <parameter>
        <xsl:choose>
          <xsl:when test="$library/@kind = 'DynamicallyLinkedLibrary'">
            <xsl:text>dll</xsl:text>
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>exe</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
      </parameter>
    </include>
  </xsl:template>

  <xsl:template name="assembliesInfo">
    <xsl:choose>
      <xsl:when test="count(/document/reference/containers/library)&gt;1">
        <include item="requirementsAssembliesLabel"/>
        <xsl:for-each select="/document/reference/containers/library">
          <xsl:text>&#xa0;&#xa0;</xsl:text>
          <xsl:call-template name="assemblyNameAndModule">
            <xsl:with-param name="library" select="."/>
          </xsl:call-template>
          <br/>
        </xsl:for-each>
      </xsl:when>
      <xsl:otherwise>
        <include item="requirementsAssemblyLabel"/>
        <xsl:text>&#xa0;</xsl:text>
        <xsl:call-template name="assemblyNameAndModule"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- Platform information -->

  <xsl:template match="platforms">
    <xsl:call-template name="section">
      <xsl:with-param name="toggleSwitch" select="'platformsTitle'"/>
      <xsl:with-param name="title">
        <include item="platformsTitle" />
      </xsl:with-param>
      <xsl:with-param name="content">
        <p>
          <xsl:for-each select="platform">
            <include item="{.}" /><xsl:if test="position()!=last()"><xsl:text>, </xsl:text></xsl:if>
          </xsl:for-each>
        </p>
        <p>
          <include item="developmentPlatformsLayout"/>
        </p>
      </xsl:with-param>
    </xsl:call-template>
  </xsl:template>

  <!-- Version information -->

  <xsl:template match="versions">
    <xsl:call-template name="section">
      <xsl:with-param name="toggleSwitch" select="'versionsTitle'"/>
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
        <xsl:for-each select="versions">
          <!-- $platformFilterExcluded is based on platform filtering information -->
          <xsl:variable name="platformFilterExcluded" select="boolean(/document/reference/platforms and ( (@name='netcfw' and not(/document/reference/platforms/platform[.='PocketPC']) and not(/document/reference/platforms/platform[.='SmartPhone']) and not(/document/reference/platforms/platform[.='WindowsCE']) ) or (@name='xnafw' and not(/document/reference/platforms/platform[.='Xbox360']) ) ) )" />
          <xsl:if test="not($platformFilterExcluded) and count(version) &gt; 0">
            <h4 class ="subHeading">
              <include item="{@name}" />
            </h4>
            <xsl:call-template name="processVersions" />
          </xsl:if>
        </xsl:for-each>
      </xsl:when>
      <xsl:otherwise>
        <!-- show the versions in which the api is supported, if any -->
        <xsl:variable name="supportedCount" select="count(version[not(@obsolete)])"/>
        <xsl:if test="$supportedCount &gt; 0">
          <include item="supportedIn_{$supportedCount}">
            <xsl:for-each select="version[not(@obsolete)]">
              <parameter>
                <include item="{@name}" />
              </parameter>
            </xsl:for-each>
          </include>
          <br/>
        </xsl:if>
        <!-- show the versions in which the api is obsolete with a compiler warning, if any -->
        <xsl:for-each select="version[@obsolete='warning']">
          <include item="obsoleteWarning">
            <parameter>
              <include item="{@name}" />
            </parameter>
          </include>
          <br/>
        </xsl:for-each>
        <!-- show the versions in which the api is obsolete and does not compile, if any -->
        <xsl:for-each select="version[@obsolete='error']">
          <xsl:if test="position()=last()">
            <include item="obsoleteError">
              <parameter>
                <include item="{@name}" />
              </parameter>
            </include>
            <br/>
          </xsl:if>
        </xsl:for-each>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- Inheritance hierarchy -->

  <xsl:template match="family">

    <xsl:call-template name="section">
      <xsl:with-param name="toggleSwitch" select="'family'"/>
      <xsl:with-param name="title">
        <include item="familyTitle" />
      </xsl:with-param>
      <xsl:with-param name="content">
        <xsl:variable name="ancestorCount" select="count(ancestors/*)" />
        <xsl:variable name="childCount" select="count(descendents/*)" />
        
        <xsl:for-each select="ancestors/type">
			<xsl:sort select="position()" data-type="number" order="descending" />
			<!--<xsl:sort select="@api"/> -->

          <xsl:call-template name="indent">
            <xsl:with-param name="count" select="position()" />
          </xsl:call-template>

          <xsl:apply-templates select="self::type" mode="link">
            <xsl:with-param name="qualified" select="true()" />
          </xsl:apply-templates>

          <!--
         <xsl:call-template name="typeReferenceLink">
           <xsl:with-param name="api" select="@api" />
            <xsl:with-param name="qualified" select="true()" />
            <xsl:with-param name="specialization" select="boolean(specialization)" />
          </xsl:call-template>
          <xsl:apply-templates select="type/specialization" />
          -->
          <br/>
        </xsl:for-each>

        <xsl:call-template name="indent">
          <xsl:with-param name="count" select="$ancestorCount + 1" />
        </xsl:call-template>
       
        <referenceLink target="{$key}" qualified="true"/>
        <br/>
        
        <xsl:choose>

          <xsl:when test="descendents/@derivedTypes">
            <xsl:call-template name="indent">
              <xsl:with-param name="count" select="$ancestorCount + 2" />
            </xsl:call-template>
            <referenceLink target="{descendents/@derivedTypes}" qualified="true">
              <include item="derivedClasses"/>
            </referenceLink>
          </xsl:when>
          <xsl:otherwise>
            
            <xsl:for-each select="descendents/type">
              <xsl:sort select="@api"/>
              <xsl:call-template name="indent">
                <xsl:with-param name="count" select="$ancestorCount + 2" />
              </xsl:call-template>

              <xsl:apply-templates select="self::type" mode="link">
                <xsl:with-param name="qualified" select="true()" />
              </xsl:apply-templates>
              <!--
              <xsl:call-template name="typeReferenceLink">
                <xsl:with-param name="api" select="@api" />
                <xsl:with-param name="qualified" select="true()" />
                <xsl:with-param name="specialization" select="boolean(specialization)" />
              </xsl:call-template>
              <xsl:apply-templates select="specialization" />
              -->
              <br/>
            </xsl:for-each>
          </xsl:otherwise>
        </xsl:choose>

      </xsl:with-param>
    </xsl:call-template>
  </xsl:template>
  
  <xsl:template name="createTableEntries">
    <xsl:param name="count" />
    <xsl:if test="number($count) > 0">
      <td>&#x20;</td>
      <xsl:call-template name="createTableEntries">
        <xsl:with-param name="count" select="number($count)-1" />
      </xsl:call-template>
		</xsl:if>
	</xsl:template>

  <xsl:template name="typeReferenceLink">
    <xsl:param name="api" />
    <xsl:param name="qualified" />
    <xsl:param name="specialization" />
    
   <referenceLink target="{$api}" qualified="{$qualified}">
      <xsl:choose>
        <xsl:when test="$specialization = 'true'">
          <xsl:attribute name="show-templates">false</xsl:attribute>
        </xsl:when>
        <xsl:otherwise>
          <xsl:attribute name="show-templates">true</xsl:attribute>
        </xsl:otherwise>
      </xsl:choose>
    </referenceLink>
    
  </xsl:template>
 
	<xsl:template match="template">
    <xsl:choose>
    <xsl:when test="@api=$key">
      <xsl:value-of select="@name" />
    </xsl:when>
      <xsl:otherwise>
        <include item="typeLinkToTypeParameter">
            <parameter>
              <xsl:value-of select="@name"/>
            </parameter>
            <parameter>
              <referenceLink target="{@api}" qualified="true" />
            </parameter>
         </include>
      </xsl:otherwise>
    </xsl:choose>
	</xsl:template>

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

	<xsl:template name="shortName">
		<xsl:choose>
			<xsl:when test="$subgroup='constructor'">
				<xsl:value-of select="/document/reference/containers/type/apidata/@name" />
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="/document/reference/apidata/@name" />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

  <!-- decorated names -->

  <xsl:template name="shortNameDecorated">
    <!--<xsl:param name="titleType" /> -->
    <xsl:choose>
      <!-- type overview pages get the type name -->
      <xsl:when test="$group='type' or ($group='list' and not($subgroup='overload'))">
        <xsl:for-each select="/document/reference[1]">
          <xsl:call-template name="typeNameDecorated" />
        </xsl:for-each>
      </xsl:when>
      <!-- constructors and member list pages also use the type name -->
      <xsl:when test="$subgroup='constructor' or ($subgroup='overload' and /document/reference/apidata/@subgroup='constructor')">
        <xsl:for-each select="/document/reference/containers/type[1]">
          <xsl:call-template name="typeNameDecorated" />
        </xsl:for-each>
      </xsl:when>
      <!--
      <xsl:when test="$group='member'">
        <xsl:variable name="type">
          <xsl:for-each select="/document/reference">
            <xsl:call-template name="GetTypeName" />
          </xsl:for-each>
        </xsl:variable>
        <xsl:choose>
          <xsl:when test="$titleType = 'tocTitle'">
            <xsl:value-of select="$type" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="concat($typeName, '.', $type)"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      -->
      <!-- member pages use the qualified member name -->
      <xsl:when test="$group='member' or ($subgroup='overload' and /document/reference/apidata/@group='member')">
        <xsl:for-each select="/document/reference/containers/type[1]">
          <xsl:call-template name="typeNameDecorated" />
        </xsl:for-each>
        <span class="cs">.</span>
        <span class="vb">.</span>
        <span class="cpp">::</span>
        <xsl:for-each select="/document/reference[1]">
          <xsl:value-of select="apidata/@name" />
          <xsl:apply-templates select="templates" mode="decorated" />
        </xsl:for-each>
      </xsl:when>
      <!-- namespace (and any other) topics just use the name -->
      <xsl:when test="/document/reference/apidata/@name = ''">
        <include item="defaultNamespace" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="/document/reference/apidata/@name" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- plain names -->

  <xsl:template name="shortNamePlain">
    <xsl:param name="qualifyMembers" select="false()" />
    <xsl:choose>
      <!-- type overview pages get the type name -->
      <xsl:when test="$group='type' or (group='list' and not($subgroup = 'overload'))">
        <xsl:for-each select="/document/reference[1]">
          <xsl:call-template name="typeNamePlain" />
        </xsl:for-each>
      </xsl:when>
      <!-- constructors and member list pages also use the type name -->
      <xsl:when test="$subgroup='constructor' or ($subgroup='overload' and /document/reference/apidata/@subgroup='constructor')">
        <xsl:for-each select="/document/reference/containers/type[1]">
          <xsl:call-template name="typeNamePlain" />
        </xsl:for-each>
      </xsl:when>
      <!-- namespace, member (and any other) topics just use the name -->
      <xsl:when test="/document/reference/apidata/@name = ''">
        <include item="defaultNamespace" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:if test="$qualifyMembers and /document/reference/apidata/@group='member'">
          <xsl:for-each select="/document/reference/containers/type[1]">
            <xsl:call-template name="typeNamePlain" />
          </xsl:for-each>
          <xsl:text>.</xsl:text>
        </xsl:if>
        <xsl:value-of select="/document/reference/apidata/@name" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

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

</xsl:stylesheet>
