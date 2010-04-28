<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.1">

	<xsl:output indent="yes" />

	<xsl:key name="index" match="/reflection/apis/api" use="@id" />

  <xsl:template match="/">
    <cDocMLDocument>
			<content>
				<assemblies>
					<xsl:apply-templates select="/reflection/assemblies/assembly" />
				</assemblies>
				<namespaces>
					<xsl:apply-templates select="/reflection/apis/api[apidata/@group='namespace']" />
				</namespaces>
			</content>
      <idMap>
        <xsl:for-each select="/reflection/apis/api[apidata/@group='namespace']">
          <entity namespaceName="{apidata/@name}" typeName="" memberName="" commentId="{@id}" textId="" />
        </xsl:for-each>
        <xsl:for-each select="/reflection/apis/api[apidata/@group='type']">
          <entity namespaceName="{key('index',containers/namespace/@api)/apidata/@name}" typeName="{apidata/@name}" memberName="" commentId="{@id}" textId="" />
        </xsl:for-each>
        <xsl:for-each select="/reflection/apis/api[apidata/@group='member']">
          <entity namespaceName="{key('index',containers/namespace/@api)/apidata/@name}" typeName="{key('index',containers/type/@api)/apidata/@name}" memberName="{apidata/@name}" commentId="{@id}" textId="" />
        </xsl:for-each>
      </idMap>
      <rMap>
	      <type namespace="System" name="Object" commentId="T:Sytem.Object">
          <xsl:attribute name="external">
            <xsl:choose>
              <xsl:when test="/reflection/apis/api[@id='T:System.Object']">false</xsl:when>
              <xsl:otherwise>true</xsl:otherwise>
            </xsl:choose>
          </xsl:attribute>
          <type namespace="System" name="ValueType" commentId="T:System.ValueType">
            <xsl:attribute name="external">
              <xsl:choose>
                <xsl:when test="/reflection/apis/api[@id='T:System.ValueType']">false</xsl:when>
                <xsl:otherwise>true</xsl:otherwise>
              </xsl:choose>
            </xsl:attribute>
	          <xsl:for-each select="/reflection/apis/api[apidata/@subgroup='structure']">
	            <type namespace="{key('index',containers/namespace/@api)/apidata/@name}" name="{apidata/@name}" commentId="{@id}" external="false" />
	          </xsl:for-each>
          </type>
          <xsl:for-each select="/reflection/apis/api[apidata/@subgroup='class']">
            <xsl:if test="not(@id='T:System.Object' or @id='T:System.ValueType')">
	      <type namespace="{key('index',containers/namespace/@api)/apidata/@name}" name="{apidata/@name}" commentId="{@id}" external="false" />
            </xsl:if>
	  </xsl:for-each>
	</type>
      </rMap>

    </cDocMLDocument>
  </xsl:template>

  <!-- utility templates -->

  <xsl:template name="writeIdentity">
    <xsl:param name="namespaceName" />
    <xsl:param name="typeName" />
    <xsl:param name="memberName" />
    <xsl:param name="commentId" select="@id|@api" />
    <xsl:param name="textId" />
    <identity namespaceName="{$namespaceName}" typeName="{$typeName}" memberName="{$memberName}" commentId="{$commentId}" textId="{$textId}" />
  </xsl:template>

  <xsl:template name="writeTypeIdentity">
    <xsl:call-template name="writeIdentity">
      <xsl:with-param name="namespaceName" select="key('index',containers/namespace/@api)/apidata/@name" />
      <xsl:with-param name="typeName" select="apidata/@name" />
    </xsl:call-template>    
  </xsl:template>

  <xsl:template name="writeMemberIdentity">
    <xsl:call-template name="writeIdentity">
      <xsl:with-param name="namespaceName" select="key('index',containers/namespace/@api)/apidata/@name" />
      <xsl:with-param name="typeName" select="key('index',containers/type/@api)/apidata/@name" />
      <xsl:with-param name="memberName" select="apidata/@name" />
    </xsl:call-template>
  </xsl:template>


  <xsl:template name="writeSource">
    <xsl:param name="assembly" select="containers/library/@assembly" />
    <xsl:param name="module" select="containers/library/@module" />
    <source assembly="{$assembly}" module="{$module}" />
  </xsl:template>

  <xsl:template name="writeTypeAttributes">
    <xsl:param name="visibility" select="string('Public')" />
    <xsl:param name="classLayout" select="string('AutoLayout')" />
    <xsl:param name="abstract">
      <xsl:choose>
        <xsl:when test="typedata/@abstract">
          <xsl:value-of select="typedata/@abstract" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>false</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:param>
    <xsl:param name="sealed">
      <xsl:choose>
        <xsl:when test="typedata/@sealed">
          <xsl:value-of select="typedata/@sealed" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>false</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:param>
    <xsl:param name="specialName" select="string('false')" />
    <attributes visibility="{$visibility}" classLayout="{$classLayout}" abstract="{$abstract}" sealed="{$sealed}" specialName="{$specialName}" import="false" stringFormat="Ansi" beforeFieldInitialization="false" runtimeSpecialName="false" />
  </xsl:template>

  <xsl:template name="writeMemberAccess">
    <xsl:param name="visibility" select="string('Public')" />
    <xsl:param name="static">
      <xsl:choose>
        <xsl:when test="memberdata/@static">
          <xsl:value-of select="memberdata/@static" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>false</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:param>
    <xsl:param name="declaringType" select="containers/type/@api" />
    <xsl:param name="specialName" select="string('false')" />
    <xsl:param name="runtimeSpecialName" select="string('false')"/>
    <xsl:param name="hasDefaultValue" select="string('false')" />
    <xsl:param name="hasMarshallingInformation" select="string('false')" />
    <xsl:param name="hasRelativeVirtualAddress" select="string('false')" />
    <xsl:param name="isInitializedOnly" select="fielddata/@initonly" />
    <xsl:param name="isLiteral" select="fielddata/@literal" />
    <xsl:param name="isNotSerialized" select="string('false')" />
    <access visibility="{$visibility}" static="{$static}" declaringType="{$declaringType}" specialName="{$specialName}" runtimeSpecialName="{$runtimeSpecialName}" hasDefaultValue="{$hasDefaultValue}" hasMarshallingInformation="{$hasMarshallingInformation}" hasRelativeVirtualAddress="{$hasRelativeVirtualAddress}" isInitializedOnly="{$isInitializedOnly}" isLiteral="{$isLiteral}" isNotSerialized="{$isNotSerialized}" isPInvokeImplementation="false" />
  </xsl:template>

  <xsl:template name="writeMethodAccess">
    <xsl:param name="visibility" select="string('Public')" />
    <xsl:param name="static">
      <xsl:choose>
        <xsl:when test="memberdata/@static">
          <xsl:value-of select="memberdata/@static" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>false</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:param>
    <xsl:param name="declaringType" select="containers/type/@api" />
    <xsl:param name="specialName">
      <xsl:choose>
        <xsl:when test="memberdata/@special">
          <xsl:value-of select="memberdata/@special" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>false</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:param>
    <xsl:param name="runtimeSpecialName" select="string('false')"/>
    <xsl:param name="abstract">
      <xsl:choose>
        <xsl:when test="proceduredata/@abstract">
          <xsl:value-of select="proceduredata/@abstract" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>false</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:param>
    <xsl:param name="virtual">
      <xsl:choose>
        <xsl:when test="proceduredata/@virtual">
          <xsl:value-of select="proceduredata/@virtual" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>false</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:param>
    <xsl:param name="final">
      <xsl:choose>
        <xsl:when test="proceduredata/@final">
          <xsl:value-of select="proceduredata/@final" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>false</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:param>
    <xsl:param name="vtable" select="string('reuseSlot')" />
    <xsl:param name="callingConvention" select="string('HasThis')" />
    <xsl:param name="pInvokeImplementation" select="string('false')" />
    <xsl:param name="hideBySignature" select="string('true')" />
    <xsl:param name="hasSecurity" select="string('false')" />
    <xsl:param name="requiresSecurityObject" select="string('false')" />
    <xsl:param name="isUnmanagedExport" select="string('false')" />
    <access visibility="{$visibility}" static="{$static}" declaringType="{$declaringType}" specialName="{$specialName}" runtimeSpecialName="{$runtimeSpecialName}" abstract="{$abstract}" virtual="{$virtual}" final="{$final}" vtable="{$vtable}" callingConvention="{$callingConvention}" pInvokeImplementation="{$pInvokeImplementation}" hideBySignature="{$hideBySignature}" hasSecurity="{$hasSecurity}" requiresSecurityObject="{$requiresSecurityObject}" isUnmanagedExport="{$isUnmanagedExport}" />
  </xsl:template>

  <xsl:template name="writePropertyAccess">
    <xsl:param name="visibility" select="string('Public')" />
    <xsl:param name="static">
      <xsl:choose>
        <xsl:when test="memberdata/@static">
          <xsl:value-of select="memberdata/@static" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>false</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:param>
    <xsl:param name="declaringType" select="containers/type/@api" />
    <xsl:param name="specialName">
      <xsl:choose>
        <xsl:when test="memberdata/@special">
          <xsl:value-of select="memberdata/@special" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>false</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:param>
    <xsl:param name="runtimeSpecialName" select="string('false')"/>
    <xsl:param name="abstract">
      <xsl:choose>
        <xsl:when test="proceduredata/@abstract">
          <xsl:value-of select="proceduredata/@abstract" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>false</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:param>
    <xsl:param name="virtual">
      <xsl:choose>
        <xsl:when test="proceduredata/@virtual">
          <xsl:value-of select="proceduredata/@virtual" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>false</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:param>
    <xsl:param name="final">
      <xsl:choose>
        <xsl:when test="proceduredata/@final">
          <xsl:value-of select="proceduredata/@final" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>false</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:param>
    <xsl:param name="readable" select="propertydata/@get" />
    <xsl:param name="writeable" select="string('false')" />
    <xsl:param name="hasDefaultValue" select="string('false')" />
    <access visibility="{$visibility}" static="{$static}" declaringType="{$declaringType}" specialName="{$specialName}" runtimeSpecialName="{$runtimeSpecialName}" abstract="{$abstract}" virtual="{$virtual}" final="{$final}" readable="{$readable}" writeable="{$writeable}" hasDefaultValue="{$hasDefaultValue}" />
  </xsl:template>

  <xsl:template name="writeEventAccess">
    <xsl:param name="visibility" select="string('Public')" />
    <xsl:param name="static">
      <xsl:choose>
        <xsl:when test="memberdata/@static">
          <xsl:value-of select="memberdata/@static" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>false</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:param>
    <xsl:param name="declaringType" select="containers/type/@api" />
    <xsl:param name="specialName">
      <xsl:choose>
        <xsl:when test="memberdata/@special">
          <xsl:value-of select="memberdata/@special" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>false</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:param>
    <xsl:param name="runtimeSpecialName" select="string('false')"/>
    <xsl:param name="abstract">
      <xsl:choose>
        <xsl:when test="proceduredata/@abstract">
          <xsl:value-of select="proceduredata/@abstract" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>false</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:param>
    <xsl:param name="virtual">
      <xsl:choose>
        <xsl:when test="proceduredata/@virtual">
          <xsl:value-of select="proceduredata/@virtual" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>false</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:param>
    <xsl:param name="final">
      <xsl:choose>
        <xsl:when test="proceduredata/@final">
          <xsl:value-of select="proceduredata/@final" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>false</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:param>
    <access visibility="{$visibility}" static="{$static}" declaringType="{$declaringType}" specialName="{$specialName}" runtimeSpecialName="{$runtimeSpecialName}" abstract="{$abstract}" virtual="{$virtual}" final="{$final}" />
  </xsl:template>

  <xsl:template name="writeConstructorAccess">
    <xsl:param name="visibility" select="string('Public')" />
    <xsl:param name="static">
      <xsl:choose>
        <xsl:when test="memberdata/@static">
          <xsl:value-of select="memberdata/@static" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>false</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:param>
    <xsl:param name="declaringType" select="containers/type/@api" />
    <xsl:param name="specialName">
      <xsl:choose>
        <xsl:when test="memberdata/@special">
          <xsl:value-of select="memberdata/@special" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>false</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:param>
    <xsl:param name="runtimeSpecialName" select="string('false')"/>
    <access visibility="{$visibility}" static="{$static}" declaringType="{$declaringType}" specialName="{$specialName}" runtimeSpecialName="{$runtimeSpecialName}" />
  </xsl:template>

  <xsl:template name="writeMemberInheritance">
        <xsl:choose>
          <xsl:when test="overrides">
            <inheritance doesOverride="true" isInherited="false" doesHide="false">
	            <baseMember name="{key('index',overrides/member/@api)/apidata/@name}" commentId="{overrides/member/@api}">
	              <type name="{key('index',overrides/member/type/@api)/apidata/@name}" namespace="{key('index',key('index',overrides/member/type/@api)/containers/namespace/@api)/apidata/@name}" commentId="{overrides/member/type/@api}" />
	            </baseMember>
            </inheritance>
          </xsl:when>
          <xsl:otherwise>
            <inheritance doesOverride="false" isInherited="false" doesHide="false">
	            <baseMember name="{apidata/@name}" commentId="{@id}">
	              <type name="{key('index',containers/type/@api)/apidata/@name}" namespace="{key('index',containers/namespace/@api)/apidata/@name}" commentId="{containers/type/@api}" />
	            </baseMember>
            </inheritance>
          </xsl:otherwise>
        </xsl:choose>
  </xsl:template>

  <xsl:template name="writeImplementedInterfaces">
    <implementedInterfaces>
      <xsl:for-each select="implements/type">
        <interface name="{key('index',@api)/apidata/@name}" namespace="{key('index',key('index',@api)/containers/namespace/@api)/apidata/@name}" commentId="{@api}" >
          <xsl:if test="specialization">
            <parameters>
              <xsl:for-each select="specialization/*">
                <parameter pointerIndirections="0" byRef="false" pinned="false" sentinel="false">
		  <xsl:apply-templates select="(descendant-or-self::type | descendant-or-self::template)[1]" />
                </parameter>
              </xsl:for-each>
            </parameters>
          </xsl:if>
        </interface>
      </xsl:for-each>
    </implementedInterfaces>
  </xsl:template>

  <xsl:template name="writeMembers">
    <xsl:param name="type" select="@id" />
    <members>
<!--
      <fields />
      <constructors />
      <methods />
      <properties />
      <events />
-->
      
      <fields>
				<xsl:apply-templates select="(key('index',elements/element/@api) | elements/element)[apidata/@subgroup='field' and containers/type/@api=$type]" />
      </fields>
      <constructors>
				<xsl:apply-templates select="key('index',elements/element/@api)[apidata/@subgroup='constructor']" />
      </constructors>
      <methods>
				<xsl:apply-templates select="(key('index',elements/element/@api) | elements/element)[apidata/@subgroup='method' and containers/type/@api=$type]" />
      </methods>
      <properties>
				<xsl:apply-templates select="(key('index',elements/element/@api) | elements/element)[apidata/@subgroup='property' and containers/type/@api=$type]" />
      </properties>
      <events>
				<xsl:apply-templates select="(key('index',elements/element/@api) | elements/element)[apidata/@subgroup='event' and containers/type/@api=$type]" />
      </events>

    </members>
  </xsl:template>

  <xsl:template name="writeCustomAttributes">
    <customAttributes />
  </xsl:template>

  <xsl:template name="writeParameters">
    <parameters>
      <xsl:for-each select="parameters/parameter">
        <parameter name="{@name}" in="false" out="false" optional="false" retval="false" params="false">
          <parameterTypeReference pointerIndirections="0" byRef="false" pinned="false" sentinel="false">
            <xsl:apply-templates select="(.//type | .//template)[1]" />
          </parameterTypeReference>
        </parameter>
      </xsl:for-each>
    </parameters>
  </xsl:template>

  <xsl:template name="writeReturnValue">
    <returnValue pointerIndirections="0" byRef="false" pinned="false" sentinel="false">
      <xsl:choose>
        <xsl:when test="returns">
          <xsl:apply-templates select="(returns//type | returns//template)[1]" />
        </xsl:when>
        <xsl:otherwise>
          <typeReference name="Void" namespace="System" commentId="T:System.Void" />
          <customModifiers />
          <arrayDefinitions />
        </xsl:otherwise>
      </xsl:choose>
    </returnValue>
  </xsl:template>

  <xsl:template name="writeValue">
    <value pointerIndirections="0" byRef="false" pinned="false" sentinel="false">
      <xsl:choose>
        <xsl:when test="returns">
          <xsl:apply-templates select="(returns//type | returns//template)[1]" />
        </xsl:when>
        <xsl:otherwise>
          <typeReference name="Void" namespace="System" commentId="T:System.Void" />
          <customModifiers />
          <arrayDefinitions />
        </xsl:otherwise>
      </xsl:choose>
    </value>
  </xsl:template>

  <xsl:template match="type">
    <typeReference name="" namespace="" commentId="{@api}" />
    <customModifiers />
    <arrayDefinitions />
  </xsl:template>

  <xsl:template match="template">
    <typeReference name="{@name}" index="{@index}" target="Type" />
    <customModifiers />
    <arrayDefinitions />
  </xsl:template>

  <xsl:template match="templates">
    <genericTypeParameters>
      <xsl:for-each select="template">
        <genericTypeParameter name="{@name}">
          <constraints />
        </genericTypeParameter>
      </xsl:for-each>
    </genericTypeParameters>
  </xsl:template>

  <!-- api entities -->

  <xsl:template match="assembly">
    <assembly>
      <attributes name="{@name}" hashAlgorithm="SHA" culture="" sideBySideCompatible="true" retargetable="false" enableJitCompileTracking="false" enableJitCompileOptimizer="false" />
      <version  majorVersion="2" minorVersion="0" buildNumber="0" revisionNumber="0" />
      <operatingSystems />
      <processors />
      <xsl:call-template name="writeCustomAttributes" />
      <modules />
      <assemblyReferences />
      <files />
      <manifestResources />
    </assembly>
  </xsl:template>

  <xsl:template match="api[apidata/@group='namespace']">
    <namespace>
      <frameworks />
      <xsl:call-template name="writeIdentity">
        <xsl:with-param name="namespaceName" select="apidata/@name" />
      </xsl:call-template>
      <xsl:call-template name="writeSource" />
      <comments />
      <xsl:call-template name="writeCustomAttributes" />
      <xsl:apply-templates select="key('index',elements/element/@api)[apidata/@subgroup='enumeration']" />
      <xsl:apply-templates select="key('index',elements/element/@api)[apidata/@subgroup='structure']" />
      <xsl:apply-templates select="key('index',elements/element/@api)[apidata/@subgroup='delegate']" />
      <xsl:apply-templates select="key('index',elements/element/@api)[apidata/@subgroup='interface']" />
      <xsl:apply-templates select="key('index',elements/element/@api)[apidata/@subgroup='class']" />
    </namespace>
  </xsl:template>

  <xsl:template match="api[apidata/@group='type' and apidata/@subgroup='class']">
    <class>
      <frameworks />
      <xsl:call-template name="writeTypeIdentity" />
      <xsl:call-template name="writeSource" />
      <xsl:call-template name="writeTypeAttributes" />
      <inheritance>
        <baseType name="Object" namespace="System" commentId="T:System.Object" />
        <xsl:call-template name="writeImplementedInterfaces" />
      </inheritance>
      <xsl:apply-templates select="templates" />
      <comments />
      <xsl:call-template name="writeCustomAttributes" />
      <xsl:call-template name="writeMembers">
        <xsl:with-param name="type" select="@id" />
      </xsl:call-template>
      <PermissionSetAttributes />
    </class>
  </xsl:template>

  <xsl:template match="api[apidata/@group='type' and apidata/@subgroup='structure']">
    <structure>
      <frameworks />
      <xsl:call-template name="writeTypeIdentity" />
      <xsl:call-template name="writeSource" />
      <xsl:call-template name="typeIdentity" />
      <xsl:call-template name="writeTypeAttributes">
        <xsl:with-param name="classLayout">SequentialLayout</xsl:with-param>
      </xsl:call-template>
      <inheritance>
        <baseType name="ValueType" namespace="System" commentId="T:System.ValueType" />
        <xsl:call-template name="writeImplementedInterfaces" />
       </inheritance>
      <xsl:apply-templates select="templates" />
      <comments />
      <xsl:call-template name="writeCustomAttributes" />
      <xsl:call-template name="writeMembers" />
    </structure>
  </xsl:template>


  <xsl:template match="api[apidata/@group='type' and apidata/@subgroup='enumeration']">
    <enumeration>
      <frameworks />
      <xsl:call-template name="writeTypeIdentity" />
      <xsl:call-template name="writeSource" />
      <xsl:call-template name="writeTypeAttributes" />
      <inheritance>
        <baseType name="Enum" namespace="System" commentId="T:System.Enum" />
        <implementedInterfaces />
      </inheritance>
      <comments />
      <xsl:call-template name="writeCustomAttributes" />
      <xsl:call-template name="writeMembers" />
    </enumeration>
  </xsl:template>

  <xsl:template match="api[apidata/@group='type' and apidata/@subgroup='delegate']">
    <delegate>
      <frameworks />
      <xsl:call-template name="writeTypeIdentity" />
      <xsl:call-template name="writeSource" />
      <xsl:call-template name="writeTypeAttributes" />
      <inheritance>
        <baseType name="MulticastDelegate" namespace="System" commentId="T:System.MulticastDelegate" />
        <implementedInterfaces />
      </inheritance>
      <comments />
      <xsl:call-template name="writeCustomAttributes" />
      <xsl:call-template name="writeMembers" />
      <xsl:call-template name="writeParameters" />
      <xsl:call-template name="writeReturnValue" />
     </delegate>
  </xsl:template>
  
  <xsl:template match="api[apidata/@group='type' and apidata/@subgroup='interface']">
    <interface>
      <frameworks />
      <xsl:call-template name="writeTypeIdentity" />
      <xsl:call-template name="writeSource" />
      <xsl:call-template name="writeTypeAttributes" />
      <inheritance>
        <baseType name="" namespace="" commentId="" />
        <implementedInterfaces />
      </inheritance>
      <xsl:apply-templates select="templates" />
      <comments />
      <xsl:call-template name="writeCustomAttributes" />
      <xsl:call-template name="writeMembers" />
    </interface>
  </xsl:template>


  <!-- member entries -->

	<xsl:template match="*[apidata/@group='member' and apidata/@subgroup='field']">
		<field>
			<frameworks />
			<xsl:call-template name="writeMemberIdentity" />
			<xsl:call-template name="writeSource" />
			<comments />
			<xsl:call-template name="writeCustomAttributes" />
			<xsl:variable name="type" select="key('index',containers/type/@api)" />
			<inheritance doesOverride="false" isInherited="false" doesHide="false">
				<baseMember name="{apidata/@name}" commentId="{@id}">
					<type name="{key('index',containers/type/@api)/apidata/@name}" namespace="{key('index',containers/namespace/@api)/apidata/@name}" commentId="{containers/type/@api}" />
				</baseMember>
			</inheritance>

      			<xsl:call-template name="writeMemberAccess" />
			<value pointerIndirections="0" byRef="false" pinned="false" sentinel="false">
				<typeReference name="{key('index',containers/type/@api)/apidata/@name}" namespace="{key('index',containers/namespace/@api)/apidata/@name}" commentId="{containers/type/@api}" />
				<customModifiers />
				<arrayDefinitions />
			</value>
		</field>
	</xsl:template>

	<xsl:template match="*[apidata/@group='member' and apidata/@subgroup='method']">
		<method>
			<xsl:attribute name="isOverload">
				<xsl:choose>
					<xsl:when test="memberdata/@overload='true'">
						<xsl:text>true</xsl:text>
					</xsl:when>
					<xsl:otherwise>
						<xsl:text>false</xsl:text>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:attribute>
			<xsl:attribute name="hasVarArgs">
				<xsl:text>false</xsl:text>
			</xsl:attribute>
			<xsl:attribute name="isExplicitInterfaceImpl">
				<xsl:text>false</xsl:text>
			</xsl:attribute>
			<frameworks />
			<xsl:call-template name="writeMemberIdentity" />
			<xsl:call-template name="writeSource" />
			<xsl:call-template name="writeMethodAccess" />
			<xsl:call-template name="writeMemberInheritance" />
			<xsl:call-template name="writeCustomAttributes" />
			<PermissionSetAttributes />
			<xsl:call-template name="writeParameters" />
			<xsl:call-template name="writeReturnValue" />
			<explicitImplRefs />
			<comments />
		</method>
	</xsl:template>

	<xsl:template match="*[apidata/@group='member' and apidata/@subgroup='property']">
		<property>
			<xsl:attribute name="isOverload">
				<xsl:choose>
					<xsl:when test="memberdata/@overload='true'">
						<xsl:text>true</xsl:text>
					</xsl:when>
					<xsl:otherwise>
						<xsl:text>false</xsl:text>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:attribute>
			<frameworks />
			<xsl:call-template name="writeMemberIdentity" />
			<xsl:call-template name="writeSource" />
			<comments />
			<xsl:call-template name="writeCustomAttributes" />
			<xsl:call-template name="writeMemberInheritance" />
			<xsl:if test="propertydata/@get='true'">
				<propertyGet>
					<method isOverload="false" hasVarArgs="false" isExplicitInterfaceImpl="false">
            <frameworks />
            <xsl:call-template name="writeMemberIdentity">
              <!-- <xsl:with-param name="memberName" select="concat('get_',apidata/@name)" /> -->
            </xsl:call-template>
						<xsl:call-template name="writeSource" />
						<xsl:call-template name="writeMethodAccess" />
						<xsl:call-template name="writeMemberInheritance" />
						<xsl:call-template name="writeCustomAttributes" />
						<PermissionSetAttributes />
						<xsl:call-template name="writeParameters" />
						<xsl:call-template name="writeReturnValue" />
						<explicitImplRefs />
						<comments />
 					</method>
				</propertyGet>
			</xsl:if>
			<xsl:if test="propertydata/@set='true'">
				
			</xsl:if>
			<xsl:call-template name="writePropertyAccess" />
			<xsl:call-template name="writeParameters" />
			<xsl:call-template name="writeValue" />
		</property>
	</xsl:template>

	<xsl:template match="*[apidata/@group='member' and apidata/@subgroup='event']">
		<event>
			<frameworks />
			<xsl:call-template name="writeMemberIdentity" />
			<xsl:call-template name="writeSource" />
			<comments />
			<xsl:call-template name="writeCustomAttributes" />
			<xsl:call-template name="writeMemberInheritance" />
			<eventAdd>
					<method isOverload="false" hasVarArgs="false" isExplicitInterfaceImpl="false">
            <frameworks />
						<xsl:call-template name="writeMemberIdentity" />
						<xsl:call-template name="writeSource" />
						<xsl:call-template name="writeMethodAccess" />
						<xsl:call-template name="writeMemberInheritance" />
						<xsl:call-template name="writeCustomAttributes" />
						<PermissionSetAttributes />
						<xsl:call-template name="writeParameters" />
						<xsl:call-template name="writeReturnValue" />
						<explicitImplRefs />
						<comments />
 					</method>
			</eventAdd>
			<eventRemove>
					<method isOverload="false" hasVarArgs="false" isExplicitInterfaceImpl="false">
            <frameworks />
						<xsl:call-template name="writeMemberIdentity" />
						<xsl:call-template name="writeSource" />
						<xsl:call-template name="writeMethodAccess" />
						<xsl:call-template name="writeMemberInheritance" />
						<xsl:call-template name="writeCustomAttributes" />
						<PermissionSetAttributes />
						<xsl:call-template name="writeParameters" />
						<xsl:call-template name="writeReturnValue" />
						<explicitImplRefs />
						<comments />
 					</method>
			</eventRemove>
			<xsl:call-template name="writeEventAccess" />
			<eventHandler name="" namespace="" commentId="{eventhandler/type/@api}" />
		</event>
	</xsl:template>


	<xsl:template match="*[apidata/@group='member' and apidata/@subgroup='constructor']">
		<constructor>
			<xsl:attribute name="isOverload">
				<xsl:choose>
					<xsl:when test="false()"><xsl:text>true</xsl:text></xsl:when>
					<xsl:otherwise><xsl:text>false</xsl:text></xsl:otherwise>
				</xsl:choose>
			</xsl:attribute>
			<frameworks />
			<xsl:call-template name="writeMemberIdentity" />
			<xsl:call-template name="writeSource" />
			<xsl:call-template name="writeConstructorAccess" />
			<xsl:call-template name="writeMemberInheritance" />
			<xsl:call-template name="writeParameters" />
			<xsl:call-template name="writeCustomAttributes" />
			<PermissionSetAttributes />
			<comments />
		</constructor>
	</xsl:template>

	<xsl:template match="attribute">
		<CustomAttribute>
			<TypeRef name="{key('index',type/@api)/apidata/@name}" namespace="{key('index',key('index',type/@api)/containers/containingNamespace/@api)/apidata/@name}" commendId="{type/@api}" />
			<CustomAttributeNamedValues />
			<CustomattributeConstructor />
		</CustomAttribute>
	</xsl:template>

	<xsl:template name="typeIdentity">
			<identity namespaceName="{key('index',containers/namespace/@api)/apidata/@name}" typeName="{apidata/@name}" memberName="" commentId="{@id}" textId="" />
	</xsl:template>

	<xsl:template name="memberIdentity">
			<identity namespaceName="{key('index',containers/namespace/@api)/apidata/@name}" typeName="{key('index',containers/type/@api)/apidata/@name}" memberName="{apidata/@name}" commentId="{@id}" textId="" />
	</xsl:template>

	<xsl:template name="source">
			<source assembly="{containers/library/@assembly}" module="{containers/library/@module}.dll" />
	</xsl:template>

	<xsl:template match="templates">
		<genericTypeParameters>
			<xsl:for-each select="template">
				<genericTypeParameter name="{@name}">
					<constraints />
				</genericTypeParameter>
			</xsl:for-each>
		</genericTypeParameters>
	</xsl:template>

	<xsl:template name="typeReference">
		<xsl:attribute name="pointerIndirections">
			<xsl:value-of select="count(pointerTo)" />
		</xsl:attribute>
		<xsl:attribute name="byRef">
			<xsl:value-of select="type/@ref" />
		</xsl:attribute>
		<xsl:attribute name="pinned">
			<xsl:text>false</xsl:text>
		</xsl:attribute>
		<typeReference name="{key('index',.//type[1]/@api)/apidata/@name}" namespace="{key('index',key('index',.//type[1]/@api)/containers/namespace/@api)/apidata/@name}" commendId="{.//type[1]/@api}" />
		<customModifiers />
		<arrayDefinitions>
			<xsl:if test="arrayOf">
				<xsl:attribute name="rank">
					<xsl:value-of select="arrayOf/@rank" />
				</xsl:attribute>
				<dimensions />
			</xsl:if>
		</arrayDefinitions>
	</xsl:template>

	<xsl:template name="visibility">
		<xsl:param name="visibility" />
		<xsl:attribute name="visibility">
			<xsl:choose>
				<xsl:when test="$visibility='public'">
					<xsl:text>Public</xsl:text>
				</xsl:when>
				<xsl:when test="$visibility='family'">
					<xsl:text>Family</xsl:text>
				</xsl:when>
			</xsl:choose>
		</xsl:attribute>
	</xsl:template>

</xsl:stylesheet>