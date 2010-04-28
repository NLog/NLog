<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.1"
  			xmlns:MSHelp="http://msdn.microsoft.com/mshelp"
        xmlns:mshelp="http://msdn.microsoft.com/mshelp"
				xmlns:ddue="http://ddue.schemas.microsoft.com/authoring/2003/5"
				xmlns:xlink="http://www.w3.org/1999/xlink"
        xmlns:msxsl="urn:schemas-microsoft-com:xslt"
   >

  <xsl:import href="../../shared/transforms/utilities_metadata.xsl" />

  <xsl:template name="insertMetadata">
		<xsl:if test="$metadata='true'">
		<xml>
      <MSHelp:Attr Name="AssetID" Value="{$key}" />
      <!-- toc title, rl title, etc. -->
      <xsl:call-template name="mshelpTitles" />
      <!-- keywords for the A (link target) index -->
			<xsl:call-template name="linkMetadata" />
      <!-- keywords for the K (index) index -->
      <xsl:call-template name="indexMetadata" />
      <!-- keywords for the F (F1 help) index -->
			<xsl:call-template name="helpMetadata" />
      <!-- help priority settings -->
      <xsl:call-template name="helpPriorityMetadata" />
      <!-- attributes for api identification -->
			<xsl:call-template name="apiTaggingMetadata" />
      <!-- atributes for filtering -->
      <xsl:call-template name="mshelpDevlangAttributes" />
			<MSHelp:Attr Name="Locale">
				<includeAttribute name="Value" item="locale" />
			</MSHelp:Attr>
      <!-- attribute to allow F1 help integration -->
      <MSHelp:Attr Name="TopicType" Value="kbSyntax" />
      <MSHelp:Attr Name="TopicType" Value="apiref" />

      <!-- Abstract -->
      <xsl:choose>
        <xsl:when test="string-length($abstractSummary) &gt; 254">
          <MSHelp:Attr Name="Abstract" Value="{normalize-space(concat(substring($abstractSummary,1,250), ' ...'))}" />
        </xsl:when>
        <xsl:when test="string-length($abstractSummary) &gt; 0">
          <MSHelp:Attr Name="Abstract" Value="{normalize-space($abstractSummary)}" />
        </xsl:when>
      </xsl:choose>

      <!-- Assembly Version-->
      <xsl:if test="$api-group != 'namespace'">
        <MSHelp:Attr Name="AssemblyVersion" Value="{/document/reference/containers/library/assemblydata/@version}" />
      </xsl:if>
      
      <xsl:call-template name="codelangAttributes" />
			<xsl:call-template name="versionMetadata" />
      <xsl:call-template name="authoredMetadata" />
		</xml>
		</xsl:if>
	</xsl:template>

  <!-- add DocSet and Technology attributes depending on the versions that support this api -->
  <xsl:template name="versionMetadata">
    <xsl:variable name="supportedOnCf">
      <xsl:call-template name="IsMemberSupportedOnCf"/>
    </xsl:variable>
    <xsl:variable name="supportedOnXNA">
      <xsl:call-template name="IsMemberSupportedOnXna" />
    </xsl:variable>
    <xsl:if test="count(/document/reference/versions/versions[@name='netfw']//version) &gt; 0 or count(/document/reference/topicdata/versions/versions[@name='netfw']//version) &gt; 0 ">
      <MSHelp:Attr Name="Technology">
        <includeAttribute name="Value" item="desktopTechnologyAttribute" />
      </MSHelp:Attr>
    </xsl:if>
    <!-- insert CF values for Technology and DocSet attributes for: 
            api topics that have netcfw version nodes
            memberlist topics where topicdata/versions has netcfw version nodes
            overload list topics where any of the elements has netcfw version nodes
    -->
    <xsl:if test="count(/document/reference/versions/versions[@name='netcfw']//version) &gt; 0 or count(/document/reference/topicdata/versions/versions[@name='netcfw']//version) &gt; 0  or count(/document/reference[topicdata[@subgroup='overload']]/elements//element/versions/versions[@name='netcfw']//version) &gt; 0 or normalize-space($supportedOnCf)!=''">
      <MSHelp:Attr Name="Technology">
        <includeAttribute name="Value" item="netcfTechnologyAttribute" />
      </MSHelp:Attr>
      <MSHelp:Attr Name="DocSet">
        <includeAttribute name="Value" item="netcfDocSetAttribute" />
      </MSHelp:Attr>
    </xsl:if>
    <!-- insert XNA values for Technology and DocSet attributes for: 
            api topics that have xnafw version nodes
            memberlist topics where topicdata/versions has xnafw version nodes
            overload list topics where any of the elements has xnafw version nodes
    -->
    <xsl:if test="count(/document/reference/versions/versions[@name='xnafw']//version) &gt; 0 or count(/document/reference/topicdata/versions/versions[@name='xnafw']//version) &gt; 0  or count(/document/reference[topicdata[@subgroup='overload']]/elements//element/versions/versions[@name='xnafw']//version) &gt; 0 or normalize-space($supportedOnXNA)!=''">
      <MSHelp:Attr Name="Technology">
        <includeAttribute name="Value" item="xnaTechnologyAttribute" />
      </MSHelp:Attr>
      <MSHelp:Attr Name="DocSet">
        <includeAttribute name="Value" item="xnaDocSetAttribute" />
      </MSHelp:Attr>
    </xsl:if>
  </xsl:template>
  
  <!-- attributes and keywords added to topics by authors -->
  
  <xsl:template name="authoredMetadata">

    <!-- authored attributes -->
    <xsl:for-each select="/document/metadata/attribute">
      <MSHelp:Attr Name="{@name}" Value="{text()}" />
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
  
  <!-- toc title and rl title -->
  
  <xsl:template name="mshelpTitles">
    
    <!-- Toc List title-->
    <MSHelp:TOCTitle>
      <includeAttribute name="Title" item="tocTitle">
        <parameter>
          <xsl:call-template name="topicTitlePlain" />
        </parameter>
      </includeAttribute>
    </MSHelp:TOCTitle>

    <!-- The Results List title -->
    <MSHelp:RLTitle>
      <includeAttribute name="Title" item="rlTitle">
        <parameter>
          <xsl:call-template name="topicTitlePlain">
            <xsl:with-param name="qualifyMembers" select="true()" />
          </xsl:call-template>
        </parameter>
        <parameter>
          <xsl:value-of select="$namespaceName"/>
        </parameter>
      </includeAttribute>
    </MSHelp:RLTitle>
    
  </xsl:template>

	<xsl:template name="apiTaggingMetadata">
		<xsl:if test="$topic-group='api' and ($api-group='type' or $api-group='member')">
			<MSHelp:Attr Name="APIType" Value="Managed" />
			<MSHelp:Attr Name="APILocation" Value="{/document/reference/containers/library/@assembly}.dll" />
			<xsl:choose>
				<xsl:when test="$api-group='type'">
					<xsl:variable name="apiTypeName">
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
					<MSHelp:Attr Name="APIName" Value="{$apiTypeName}" />
					<xsl:choose>
						<xsl:when test="boolean($api-subgroup='delegate')">
							<MSHelp:Attr Name="APIName" Value="{concat($apiTypeName,'.ctor')}" />
							<MSHelp:Attr Name="APIName" Value="{concat($apiTypeName,'.','Invoke')}" />
							<MSHelp:Attr Name="APIName" Value="{concat($apiTypeName,'.','BeginInvoke')}" />
							<MSHelp:Attr Name="APIName" Value="{concat($apiTypeName,'.','EndInvoke')}" />
						</xsl:when>
						<xsl:when test="$api-subgroup='enumeration'">
							<xsl:for-each select="/document/reference/elements/element">
								<MSHelp:Attr Name="APIName" Value="{substring(@api,3)}" />
							</xsl:for-each>
							<!-- Namespace + Type + Member for each member -->
						</xsl:when>
					</xsl:choose>
				</xsl:when>
				<xsl:when test="$api-group='member'">
					<xsl:variable name="apiTypeName">
						<xsl:value-of select="concat(/document/reference/containers/namespace/apidata/@name,'.',/document/reference/containers/type/apidata/@name)" />
						<xsl:if test="count(/document/reference/templates/template) > 0">
							<xsl:value-of select="concat('`',count(/document/reference/templates/template))" />
						</xsl:if>
					</xsl:variable>
					<!-- Namespace + Type + Member -->
					<MSHelp:Attr Name="APIName" Value="{concat($apiTypeName,'.',/document/reference/apidata/@name)}" />
					<xsl:choose>
            <!-- for properties, add APIName attribute get/set accessor methods -->
						<xsl:when test="boolean($api-subgroup='property')">
              <xsl:if test="/document/reference/propertydata[@get='true']">
                <MSHelp:Attr Name="APIName" Value="{concat($apiTypeName,'.get_',/document/reference/apidata/@name)}" />
              </xsl:if>
              <xsl:if test="/document/reference/propertydata[@set='true']">
                <MSHelp:Attr Name="APIName" Value="{concat($apiTypeName,'.set_',/document/reference/apidata/@name)}" />
              </xsl:if>
						</xsl:when>
            <!-- for events, add APIName attribute add/remove accessor methods -->
						<xsl:when test="boolean($api-subgroup='event')">
              <xsl:if test="/document/reference/eventdata[@add='true']">
					      <MSHelp:Attr Name="APIName" Value="{concat($apiTypeName,'.add_',/document/reference/apidata/@name)}" />
              </xsl:if>
              <xsl:if test="/document/reference/eventdata[@remove='true']">
					      <MSHelp:Attr Name="APIName" Value="{concat($apiTypeName,'.remove_',/document/reference/apidata/@name)}" />
              </xsl:if>
						</xsl:when>
					</xsl:choose>
				</xsl:when>
			</xsl:choose>
		</xsl:if>
	</xsl:template>

  <!-- link target (A index) keywords -->  
  
	<xsl:template name="linkMetadata">
    
		<!-- code entity reference keyword -->
		<MSHelp:Keyword Index="A" Term="{$key}" />

    <xsl:if test="$topic-group='api' and $api-subgroup='enumeration'">
      <xsl:for-each select="/document/reference/elements/element">
        <MSHelp:Keyword Index="A" Term="{@api}" />
      </xsl:for-each>
    </xsl:if>
    
		<!-- frlrf keywords -->
    <xsl:call-template name="FrlrfKeywords"/>

  </xsl:template>

  <xsl:template name="FrlrfKeywords">
    <xsl:variable name="frlrfTypeName">
      <!-- for members and nested types, start with the containing type name -->
      <xsl:for-each select="/document/reference/containers/type">
        <xsl:call-template name="FrlrfTypeName"/>
      </xsl:for-each>
      <!-- for types and member list topics, append the type name -->
      <xsl:if test="/document/reference/apidata[@group='type']">
        <xsl:for-each select="/document/reference">
          <xsl:call-template name="FrlrfTypeName"/>
        </xsl:for-each>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="memberName">
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
        <MSHelp:Keyword Index="A" Term="{translate(concat('frlrf',$memberName),'.','')}"/>
      </xsl:when>
      <!-- Overload topic -->
      <xsl:when test="/document/reference/topicdata[@subgroup='overload']">
        <xsl:variable name="frlrfBaseId">
          <xsl:value-of select="translate(concat('frlrf', $namespaceName, $frlrfTypeName, 'Class', $memberName, 'Topic'),'.','')"/>
        </xsl:variable>
        <MSHelp:Keyword Index="A" Term="{$frlrfBaseId}"/>
        <!-- whidbey included frlrf keyword for each overload, but I don't think we need in Manifold, so commenting it out -->
        <!--
        <xsl:for-each select="elements/element">
          <MSHelp:Keyword Index="A" Term="{concat($frlrfBaseId, string(position()))}"/>
        </xsl:for-each>
        -->
      </xsl:when>
      <!-- Member list topic (other than overload list captured above) -->
      <xsl:when test="/document/reference/topicdata[@group='list']">
        <xsl:variable name="memberListSubgroup">
          <xsl:choose>
            <xsl:when test="/document/reference/topicdata/@subgroup='members'">Members</xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="/document/reference/topicdata/@subgroup"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <MSHelp:Keyword Index="A" Term="{translate(concat('frlrf', $namespaceName, $frlrfTypeName, $memberListSubgroup, 'Topic'),'.','')}"/>
      </xsl:when>
      <!-- type topic -->
      <xsl:when test="/document/reference/apidata[@group='type']">
        <MSHelp:Keyword Index="A" Term="{translate(concat('frlrf',$namespaceName, $frlrfTypeName, 'ClassTopic'),'.','')}"/>
      </xsl:when>
      <!-- no frlrf ID for overload signature topics-->
      <xsl:when test="/document/reference/apidata[@group='member'] and /document/reference/memberdata/@overload"/>
      <!-- non-overload member topic -->
      <xsl:when test="/document/reference/apidata[@group='member']">
        <MSHelp:Keyword Index="A" Term="{translate(concat('frlrf',$namespaceName, $frlrfTypeName, 'Class', $memberName, 'Topic'),'.','')}"/>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="FrlrfTypeName">
    <xsl:for-each select="type">
      <xsl:call-template name="FrlrfTypeName"/>
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

  <xsl:template name="helpMetadata">
    <!-- F keywords -->
    <xsl:choose>
      <!-- namespace pages get the namespace keyword, if it exists -->
			<xsl:when test="$group='namespace'">
        <xsl:variable name="namespace" select="/document/reference/apidata/@name" />
        <xsl:if test="boolean($namespace != '')">
          <MSHelp:Keyword Index="F" Term="{$namespace}" />
        </xsl:if>
      </xsl:when>
      <!-- type overview and member list pages get type and namespace.type keywords -->
			<xsl:when test="$group='type' or ($group='list' and $subgroup='members')">
        <xsl:variable name="namespace" select="/document/reference/containers/namespace/apidata/@name" />
        <xsl:variable name="type">
          <xsl:for-each select="/document/reference[1]">
            <xsl:call-template name="typeNameWithTicks" />
          </xsl:for-each>
        </xsl:variable>
        <MSHelp:Keyword Index="F" Term="{$type}" />
        <xsl:if test="boolean($namespace != '')">
          <MSHelp:Keyword Index="F" Term="{concat($namespace,'.',$type)}" />
        </xsl:if>
        <xsl:if test="$subgroup = 'enumeration'">
          <xsl:for-each select="/document/reference/elements/element">
            <MSHelp:Keyword Index="F" Term="{concat($type, '.', apidata/@name)}" />
            <xsl:if test="boolean($namespace)">
              <MSHelp:Keyword Index="F" Term="{concat($namespace,'.',$type, '.', apidata/@name)}" />
            </xsl:if>
          </xsl:for-each>
        </xsl:if>
        <xsl:call-template name="xamlMSHelpFKeywords"/>
			</xsl:when>
      
      <!-- overload list pages get member, type.member, and namepsace.type.member keywords -->
      <xsl:when test="$group='list' and $subgroup='overload'">
        <xsl:variable name="namespace" select="/document/reference/containers/namespace/apidata/@name" />
        <xsl:variable name="type">
          <xsl:for-each select="/document/reference/containers/type[1]">
            <xsl:call-template name="typeNameWithTicks" />
          </xsl:for-each>
        </xsl:variable>
        <xsl:variable name="member">
          <xsl:choose>
            <!-- if the member is a constructor, use the member name for the type name -->
            <xsl:when test="/document/reference/apidata[@subgroup='constructor']">
              <xsl:value-of select="$type" />
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="/document/reference/apidata/@name"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <MSHelp:Keyword Index="F" Term="{$member}" />
        <MSHelp:Keyword Index="F" Term="{concat($type, '.', $member)}" />
        <xsl:if test="boolean($namespace != '')">
          <MSHelp:Keyword Index="F" Term="{concat($namespace, '.', $type, '.', $member)}" />
        </xsl:if>
      </xsl:when>

      <!-- no F1 help entries for overload signature topics -->
      <xsl:when test="$group='member' and /document/reference/memberdata/@overload"/>

      <!-- member pages get member, type.member, and namepsace.type.member keywords -->
			<xsl:when test="$group='member'">
        <xsl:variable name="namespace" select="/document/reference/containers/namespace/apidata/@name" />
        <xsl:variable name="type">
          <xsl:for-each select="/document/reference/containers/type[1]">
            <xsl:call-template name="typeNameWithTicks" />
          </xsl:for-each>
        </xsl:variable>
        <xsl:variable name="member">
          <xsl:choose>
            <!-- if the member is a constructor, use the member name for the type name -->
            <xsl:when test="$subgroup='constructor'">
              <xsl:value-of select="$type" />
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
        <!--
        <xsl:choose>
          -->
          <!--
          <xsl:when test="$subgroup='constructor'">
            <MSHelp:Keyword Index="F" Term="{$type}" />
            <MSHelp:Keyword Index="F" Term="{concat($type, '.', $type)}" />
            <xsl:if test="boolean($namespace)">
              <MSHelp:Keyword Index="F" Term="{concat($namespace, '.', $type, '.', $type)}" />
            </xsl:if>
          </xsl:when>
          <xsl:otherwise>
            -->
            <MSHelp:Keyword Index="F" Term="{$member}" />
            <MSHelp:Keyword Index="F" Term="{concat($type, '.', $member)}" />
            <xsl:if test="boolean($namespace != '')">
              <MSHelp:Keyword Index="F" Term="{concat($namespace, '.', $type, '.', $member)}" />
            </xsl:if>
        <!--
          </xsl:otherwise>
        </xsl:choose>
        -->
    </xsl:when>
  </xsl:choose>
</xsl:template>

  <!-- set high help priority for namespace and member list pages, lower priority for type overview pages -->
  
  <xsl:template name="helpPriorityMetadata">
    <xsl:choose>
      <xsl:when test="($topic-group='api' and $api-group='namespace') or ($topic-group='list' and $topic-subgroup='members')">
        <MSHelp:Attr Name="HelpPriority" Value="1"/>
      </xsl:when>
      <xsl:when test="$topic-group='api' and $api-group='type'">
        <MSHelp:Attr Name="HelpPriority" Value="2"/>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

	<xsl:template name="apiName">
		<xsl:choose>
			<xsl:when test="$subgroup='constructor'">
				<xsl:value-of select="/document/reference/containers/type/apidata/@name" />
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="/document/reference/apidata/@name" />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

  <!-- make a semicolon-separated list of the $languages-->
  <xsl:template name="languagesList">
    <xsl:for-each select="$languages/language">
      <xsl:variable name="devlang">
        <xsl:call-template name="GetDevLangAttrValue">
          <xsl:with-param name="devlang" select="@name"/>
        </xsl:call-template>
      </xsl:variable>
      <xsl:if test="normalize-space($devlang)!=''">
        <xsl:value-of select="$devlang"/><xsl:text>;</xsl:text>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="mshelpDevlangAttributes">
    <!-- first insert a DevLang attr for each language in the $languages arg passed to the transform -->
    <xsl:for-each select="$languages/language">
      <xsl:variable name="devlang">
        <xsl:call-template name="GetDevLangAttrValue">
          <xsl:with-param name="devlang" select="@name"/>
        </xsl:call-template>
      </xsl:variable>
      <xsl:choose>
        <xsl:when test="normalize-space($devlang)=''"/>
        <xsl:when test="$devlang = 'VJ#'">
          <xsl:if test="boolean(/document/reference/versions/versions[@name='netfw']//version[not(@name='netfw35')])">
            <MSHelp:Attr Name="DevLang" Value="{$devlang}" />
          </xsl:if>
        </xsl:when>
        <xsl:otherwise>
          <MSHelp:Attr Name="DevLang" Value="{$devlang}" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:for-each>

    <!-- make a list of the languages that have already been included via $languages -->
    <xsl:variable name="languagesList">
      <xsl:call-template name="languagesList"/>
    </xsl:variable>

    <!-- add DevLang attr for any additional languages referred to in the topic's snippet and code nodes -->
    <xsl:for-each select="//*[@language]">
      <xsl:if test="not(@language=preceding::*/@language)">
        <xsl:variable name="devlang">
          <xsl:call-template name="GetDevLangAttrValue">
            <xsl:with-param name="devlang" select="@language"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:choose>
          <xsl:when test="normalize-space($devlang)=''"/>
          <xsl:when test="contains($languagesList,concat($devlang,';'))"/>
          <xsl:otherwise>
            <MSHelp:Attr Name="DevLang" Value="{$devlang}" />
          </xsl:otherwise>
        </xsl:choose>
      </xsl:if>
    </xsl:for-each>

    <!-- extend the list of languages that have already been included -->
    <xsl:variable name="languagesList2">
      <xsl:value-of select="$languagesList"/>
      <xsl:for-each select="//*[@language]">
        <xsl:variable name="devlang">
          <xsl:call-template name="GetDevLangAttrValue">
            <xsl:with-param name="devlang" select="@language"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:if test="normalize-space($devlang)!=''">
          <xsl:value-of select="$devlang"/><xsl:text>;</xsl:text>
        </xsl:if>
      </xsl:for-each>
    </xsl:variable>

    <!-- add DevLang attr for any additional languages referred to in the topic's syntax blocks -->
    <xsl:for-each select="/document/syntax/div[@codeLanguage and not(div[@class='nonXamlAssemblyBoilerplate'])]">
      <xsl:if test="not(@codeLanguage=preceding::*/@codeLanguage)">
        <xsl:variable name="devlang">
          <xsl:call-template name="GetDevLangAttrValue">
            <xsl:with-param name="devlang" select="@codeLanguage"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:choose>
          <xsl:when test="normalize-space($devlang)=''"/>
          <xsl:when test="contains($languagesList2,concat($devlang,';'))"/>
          <xsl:otherwise>
            <MSHelp:Attr Name="DevLang" Value="{$devlang}" />
          </xsl:otherwise>
        </xsl:choose>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="GetDevLangAttrValue">
    <xsl:param name="devlang"/>
    <xsl:choose>
      <xsl:when test="$devlang = 'CSharp' or $devlang = 'c#' or $devlang = 'cs' or $devlang = 'C#'" >
        <xsl:text>CSharp</xsl:text>
      </xsl:when>
      <xsl:when test="$devlang = 'ManagedCPlusPlus' or $devlang = 'cpp' or $devlang = 'cpp#' or $devlang = 'c' or $devlang = 'c++' or $devlang = 'C++' or $devlang = 'kbLangCPP'" >
        <xsl:text>C++</xsl:text>
      </xsl:when>
      <xsl:when test="$devlang = 'JScript' or $devlang = 'js' or $devlang = 'jscript#' or $devlang = 'jscript' or $devlang = 'JScript' or $devlang = 'kbJScript'">
        <xsl:text>JScript</xsl:text>
      </xsl:when>
      <xsl:when test="$devlang = 'VisualBasic' or $devlang = 'VisualBasicUsage' or $devlang = 'vb' or $devlang = 'vb#' or $devlang = 'VB' or $devlang = 'kbLangVB'" >
        <xsl:text>VB</xsl:text>
      </xsl:when>
      <xsl:when test="$devlang = 'VBScript' or $devlang = 'vbs'">
        <xsl:text>VBScript</xsl:text>
      </xsl:when>
      <xsl:when test="$devlang = 'JSharp' or $devlang = 'j#' or $devlang = 'jsharp' or $devlang = 'VJ#'">
        <xsl:text>VJ#</xsl:text>
      </xsl:when>
      <xsl:when test="$devlang = 'xaml' or $devlang = 'XAML'">
        <xsl:text>XAML</xsl:text>
      </xsl:when>
      <xsl:when test="$devlang = 'xml' or $devlang = 'XML'">
        <xsl:text>XML</xsl:text>
      </xsl:when>
      <xsl:otherwise/>
    </xsl:choose>
  </xsl:template>

  <!-- 
     Additional F1 keywords for class, struct, and enum topics in a set of WPF namespaces. 
     This template inserts the MSHelp:Keyword nodes.
     The keyword prefixes and the WPF namespaces are hard-coded in variables.
 -->
  <xsl:variable name="var_wpf_f1index_prefix_1">http://schemas.microsoft.com/winfx/2006/xaml/presentation#</xsl:variable>
  <xsl:variable name="var_wpf_f1index_prefix_1_namespaces">N:System.Windows.Controls#N:System.Windows.Documents#N:System.Windows.Shapes#N:System.Windows.Navigation#N:System.Windows.Data#N:System.Windows#N:System.Windows.Controls.Primitives#N:System.Windows.Media.Animation#N:System.Windows.Annotations#N:System.Windows.Annotations.Anchoring#N:System.Windows.Annotations.Storage#N:System.Windows.Media#N:System.Windows.Media.Animation#N:System.Windows.Media.Media3D#N:</xsl:variable>

  <xsl:template name="xamlMSHelpFKeywords">
    <xsl:if test="$subgroup='class' or $subgroup='enumeration' or $subgroup='structure'">
      <xsl:if test="boolean(contains($var_wpf_f1index_prefix_1_namespaces, concat('#',/document/reference/containers/namespace/@api,'#'))
                           or starts-with($var_wpf_f1index_prefix_1_namespaces, concat(/document/reference/containers/namespace/@api,'#')))">
        <MSHelp:Keyword Index="F" Term="{concat($var_wpf_f1index_prefix_1, /document/reference/apidata/@name)}"/>
      </xsl:if>
    </xsl:if>
  </xsl:template>

  <!-- Index Logic -->
  
  <xsl:template name="indexMetadata">
    <xsl:choose>
      <!-- namespace topics get one unqualified index entry -->
      <xsl:when test="$topic-group='api' and $api-group='namespace'">
        <xsl:variable name="names">
          <xsl:for-each select="/document/reference">
            <xsl:call-template name="textNames" />
          </xsl:for-each>
        </xsl:variable>
        <MSHelp:Keyword Index="K">
          <includeAttribute name="Term" item="namespaceIndexEntry">
            <parameter>
              <xsl:value-of select="msxsl:node-set($names)/name" />
            </parameter>
          </includeAttribute>
        </MSHelp:Keyword>
      </xsl:when>
      <!-- type overview topics get qualified and unqualified index entries, and an about index entry -->
      <xsl:when test="$topic-group='api' and $api-group='type'">
        <xsl:variable name="names">
          <xsl:for-each select="/document/reference">
            <xsl:call-template name="textNames" />
          </xsl:for-each>
        </xsl:variable>
        <xsl:variable name="namespace" select="/document/reference/containers/namespace/apidata/@name" />
        <xsl:for-each select="msxsl:node-set($names)/name">
          <MSHelp:Keyword Index="K">
            <includeAttribute name="Term" item="{$api-subgroup}IndexEntry">
              <parameter>
                <xsl:copy-of select="."/>
              </parameter>
            </includeAttribute>
          </MSHelp:Keyword>
          <xsl:if test="boolean($namespace != '')">
            <MSHelp:Keyword Index="K">
              <includeAttribute name="Term" item="{$api-subgroup}IndexEntry">
                <parameter>
                  <xsl:value-of select="$namespace"/>
                  <xsl:text>.</xsl:text>
                  <xsl:copy-of select="." />
                </parameter>
              </includeAttribute>
            </MSHelp:Keyword>
          </xsl:if>
          <!-- multi-topic types (not delegates and enumerations) get about entries, too-->
          <xsl:if test="$api-subgroup='class' or $api-subgroup='structure' or $api-subgroup='interface'">
          <MSHelp:Keyword Index="K">
            <includeAttribute name="Term" item="aboutTypeIndexEntry">
              <parameter>
                <include item="{$api-subgroup}IndexEntry">
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
        <xsl:if test="$api-subgroup='enumeration'">
          <xsl:for-each select="/document/reference/elements/element">
            <MSHelp:Keyword Index="K">
              <includeAttribute name="Term" item="{$api-subgroup}MemberIndexEntry">
                <parameter>
                  <xsl:value-of select="apidata/@name" />
                </parameter>
              </includeAttribute>
            </MSHelp:Keyword>
          </xsl:for-each>
        </xsl:if>
      </xsl:when>
      <!-- all member lists get unqualified entries, qualified entries, and unqualified sub-entries -->
      <xsl:when test="$topic-group='list' and $topic-subgroup='members'">
        <xsl:variable name="namespace" select="/document/reference/containers/namespace/apidata/@name" />
        <xsl:variable name="names">
          <xsl:for-each select="/document/reference">
            <xsl:call-template name="textNames" />
          </xsl:for-each>
        </xsl:variable>
        <xsl:for-each select="msxsl:node-set($names)/name">
          <MSHelp:Keyword Index="K">
            <includeAttribute name="Term" item="{$api-subgroup}IndexEntry">
              <parameter>
                <xsl:value-of select="." />
              </parameter>
            </includeAttribute>
          </MSHelp:Keyword>
          <MSHelp:Keyword Index="K">
            <includeAttribute name="Term" item="membersIndexEntry">
              <parameter>
                <include item="{$api-subgroup}IndexEntry">
                  <parameter>
                    <xsl:value-of select="." />
                  </parameter>
                </include>
              </parameter>
            </includeAttribute>
          </MSHelp:Keyword>
        </xsl:for-each>
        <xsl:variable name="qnames">
          <xsl:for-each select="/document/reference">
            <xsl:call-template name="qualifiedTextNames" />
          </xsl:for-each>
        </xsl:variable>
        <xsl:if test="boolean($namespace != '')">
          <xsl:for-each select="msxsl:node-set($qnames)/name">
            <MSHelp:Keyword Index="K">
              <includeAttribute name="Term" item="{$api-subgroup}IndexEntry">
                <parameter>
                  <xsl:value-of select="." />
                </parameter>
              </includeAttribute>
            </MSHelp:Keyword>
          </xsl:for-each>
        </xsl:if>
      </xsl:when>
      <!-- other member list pages get unqualified sub-entries -->
      <xsl:when test="$topic-group='list' and not($topic-subgroup = 'overload')">
        <xsl:variable name="names">
          <xsl:for-each select="/document/reference">
            <xsl:call-template name="textNames" />
          </xsl:for-each>
        </xsl:variable>
        <xsl:for-each select="msxsl:node-set($names)/name">
          <MSHelp:Keyword Index="K">
            <includeAttribute name="Term" item="{$subgroup}IndexEntry">
              <parameter>
                <include item="{$api-subgroup}IndexEntry">
                  <parameter>
                    <xsl:value-of select="." />
                  </parameter>
                </include>
              </parameter>
            </includeAttribute>
          </MSHelp:Keyword>
        </xsl:for-each>
      </xsl:when>
      <!-- constructor (or constructor overload) topics get unqualified sub-entries using the type names -->
      <xsl:when test="($topic-group='api' and $api-subgroup='constructor' and not(/document/reference/memberdata/@overload)) or ($topic-subgroup='overload' and $api-subgroup = 'constructor')">
        <xsl:variable name="typeSubgroup" select="/document/reference/containers/type/apidata/@subgroup" />
        <xsl:variable name="names">
          <xsl:for-each select="/document/reference/containers/type">
            <xsl:call-template name="textNames" />
          </xsl:for-each>
        </xsl:variable>
        <xsl:for-each select="msxsl:node-set($names)/name">
          <MSHelp:Keyword Index="K">
            <includeAttribute name="Term" item="constructorIndexEntry">
              <parameter>
                <include item="{$typeSubgroup}IndexEntry">
                  <parameter>
                    <xsl:value-of select="." />
                  </parameter>
                </include>
              </parameter>
            </includeAttribute>
          </MSHelp:Keyword>
        </xsl:for-each>
        <xsl:variable name="qnames">
          <xsl:for-each select="/document/reference">
            <xsl:call-template name="qualifiedTextNames" />
          </xsl:for-each>
        </xsl:variable>
        <xsl:for-each select="msxsl:node-set($qnames)/name">
          <MSHelp:Keyword Index="K">
            <includeAttribute name="Term" item="constructorTypeIndexEntry">
              <parameter>
                <xsl:value-of select="." />
              </parameter>
            </includeAttribute>
          </MSHelp:Keyword>
        </xsl:for-each>
      </xsl:when>
      <!-- other member (or overload) topics get qualified and unqualified entries using the member names -->
      <xsl:when test="($topic-group='api' and $api-group='member' and not(/document/reference/memberdata/@overload)) or $topic-subgroup='overload'">
        
        <xsl:choose>
          <!-- explicit interface implementation -->
          <xsl:when test="/document/reference/proceduredata/@virtual='true' and /document/reference/memberdata/@visibility='private'">
            <xsl:variable name="entryType">
              <xsl:choose>
                <xsl:when test="string($subsubgroup)">
                  <xsl:value-of select="$subsubgroup" />
                </xsl:when>
                <xsl:otherwise>
                  <xsl:choose>
                    <xsl:when test="$subgroup='overload'">
                      <xsl:value-of select="/document/reference/apidata/@subgroup"/>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="$subgroup" />
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            <xsl:variable name="names">
              <xsl:for-each select="/document/reference/implements/member">
                <xsl:call-template name="textNames" />
              </xsl:for-each>
            </xsl:variable>
            <xsl:for-each select="msxsl:node-set($names)/name">
              <MSHelp:Keyword Index="K">
                <includeAttribute name="Term" item="{$entryType}ExplicitIndexEntry">
                  <parameter>
                    <xsl:copy-of select="."/>
                  </parameter>
                </includeAttribute>
              </MSHelp:Keyword>
            </xsl:for-each>
            <xsl:variable name="qnames">
              <xsl:for-each select="/document/reference">
                <xsl:call-template name="qualifiedTextNames" />
              </xsl:for-each>
            </xsl:variable>
            <xsl:for-each select="msxsl:node-set($qnames)/name">
              <MSHelp:Keyword Index="K">
                <includeAttribute name="Term" item="{$entryType}ExplicitIndexEntry">
                  <parameter>
                    <xsl:copy-of select="."/>
                  </parameter>
                </includeAttribute>
              </MSHelp:Keyword>
            </xsl:for-each>
          </xsl:when>
          <xsl:otherwise>
            <xsl:variable name="entryType">
              <xsl:choose>
                <xsl:when test="string($subsubgroup)">
                  <xsl:value-of select="$subsubgroup" />
                </xsl:when>
                <xsl:otherwise>
                  <xsl:choose>
                    <xsl:when test="$subgroup='overload'">
                      <xsl:value-of select="/document/reference/apidata/@subgroup"/>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="$subgroup" />
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            <xsl:variable name="names">
              <xsl:for-each select="/document/reference">
                <xsl:call-template name="textNames" />
              </xsl:for-each>
            </xsl:variable>
            <xsl:for-each select="msxsl:node-set($names)/name">
              <MSHelp:Keyword Index="K">
                <includeAttribute name="Term" item="{$entryType}IndexEntry">
                  <parameter>
                    <xsl:copy-of select="."/>
                  </parameter>
                </includeAttribute>
              </MSHelp:Keyword>
            </xsl:for-each>
            <xsl:variable name="qnames">
              <xsl:for-each select="/document/reference">
                <xsl:call-template name="qualifiedTextNames" />
              </xsl:for-each>
            </xsl:variable>
            <xsl:for-each select="msxsl:node-set($qnames)/name">
              <MSHelp:Keyword Index="K">
                <includeAttribute name="Term" item="{$entryType}IndexEntry">
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
