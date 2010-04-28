<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.1"
		xmlns:MSHelp="http://msdn.microsoft.com/mshelp" xmlns:msxsl="urn:schemas-microsoft-com:xslt" >

  <xsl:import href="../../shared/transforms/utilities_metadata.xsl" />

	<xsl:template name="insertMetadata">
		<xsl:if test="$metadata='true'">
		<xml>
			<MSHelp:Attr Name="AssetID" Value="{$key}" />
			<!-- toc metadata -->
			<xsl:call-template name="linkMetadata" />
      <xsl:call-template name="indexMetadata" />
			<xsl:call-template name="helpMetadata" />
			<MSHelp:Attr Name="TopicType" Value="apiref" />
      <!-- attribute to allow F1 help integration -->
      <MSHelp:Attr Name="TopicType" Value="kbSyntax" />
      <xsl:call-template name="apiTaggingMetadata" />
			<MSHelp:Attr Name="Locale">
				<includeAttribute name="Value" item="locale" />
			</MSHelp:Attr>
			<xsl:if test="boolean($summary) and (string-length($summary) &lt; 255)">
				<MSHelp:Attr Name="Abstract">
					<xsl:attribute name="Value"><xsl:value-of select="$summary" /></xsl:attribute>
				</MSHelp:Attr>
			</xsl:if>
		</xml>
		</xsl:if>
	</xsl:template>

	<xsl:template name="apiTaggingMetadata">
		<xsl:if test="$tgroup='api' and ($group='type' or $group='member')">
			<MSHelp:Attr Name="APIType" Value="Managed" />
			<MSHelp:Attr Name="APILocation" Value="{/document/reference/containers/library/@assembly}.dll" />
			<xsl:choose>
				<xsl:when test="$group='type'">
					<xsl:variable name="apiTypeName">
						<xsl:value-of select="concat(/document/reference/containers/namespace/apidata/@name,'.',/document/reference/apidata/@name)" />
						<xsl:if test="count(/document/reference/templates/template) > 0">
							<xsl:value-of select="concat('`',count(/document/reference/templates/template))" />
						</xsl:if>
					</xsl:variable>
					<!-- Namespace + Type -->
					<MSHelp:Attr Name="APIName" Value="{$apiTypeName}" />
					<xsl:choose>
						<xsl:when test="boolean($subgroup='delegate')">
							<MSHelp:Attr Name="APIName" Value="{concat($apiTypeName,'.','.ctor')}" />
							<MSHelp:Attr Name="APIName" Value="{concat($apiTypeName,'.','Invoke')}" />
							<MSHelp:Attr Name="APIName" Value="{concat($apiTypeName,'.','BeginInvoke')}" />
							<MSHelp:Attr Name="APIName" Value="{concat($apiTypeName,'.','EndInvoke')}" />
						</xsl:when>
						<xsl:when test="$subgroup='enumeration'">
							<xsl:for-each select="/document/reference/elements/element">
								<MSHelp:Attr Name="APIName" Value="{substring(@api,2)}" />
							</xsl:for-each>
							<!-- Namespace + Type + Member for each member -->
						</xsl:when>
					</xsl:choose>
				</xsl:when>
				<xsl:when test="$group='member'">
					<xsl:variable name="apiTypeName">
						<xsl:value-of select="concat(/document/reference/containers/namespace/apidata/@name,'.',/document/reference/containers/container[@type]/apidata/@name)" />
						<xsl:if test="count(/document/reference/templates/template) > 0">
							<xsl:value-of select="concat('`',count(/document/reference/templates/template))" />
						</xsl:if>
					</xsl:variable>
					<!-- Namespace + Type + Member -->
					<MSHelp:Attr Name="APIName" Value="{concat($apiTypeName,'.',/document/reference/apidata/@name)}" />
					<xsl:choose>
						<xsl:when test="boolean($subgroup='property')">
							<!-- Namespace + Type + get_Member if get-able -->
							<!-- Namespace + Type + set_Member if set-able -->
						</xsl:when>
						<xsl:when test="boolean($subgroup='event')">
							<!-- Namespace + Type + add_Member -->
							<!-- Namespace + Type + remove_Member -->
						</xsl:when>
					</xsl:choose>
				</xsl:when>
			</xsl:choose>
		</xsl:if>
	</xsl:template>

	<xsl:template name="linkMetadata">
		<!-- code entity reference keyword -->
		<MSHelp:Keyword Index="A" Term="{$key}" />
		<!-- frlrf keywords -->
		<xsl:choose>
			<xsl:when test="$group='namespace'">
				<MSHelp:Keyword Index="A" Term="{translate(concat('frlrf',/document/reference/apidata/@name),'.','')}" />
			</xsl:when>
			<!-- types & members, too -->
      <xsl:when test="$group='type'">
        <MSHelp:Keyword Index="A" Term="{translate(concat('frlrf',/document/reference/containers/namespace/apidata/@name, /document/reference/apidata/@name, 'ClassTopic'),'.','')}" />
        <MSHelp:Keyword Index="A" Term="{translate(concat('frlrf',/document/reference/containers/namespace/apidata/@name, /document/reference/apidata/@name, 'MembersTopic'),'.','')}" />
      </xsl:when>
      <xsl:when test="$group='member'">
        <MSHelp:Keyword Index="A" Term="{translate(concat('frlrf',/document/reference/containers/namespace/apidata/@name, /document/reference/containers/type/apidata/@name, 'Class', /document/reference/apidata/@name, 'Topic'),'.','')}" />      
      </xsl:when>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="helpMetadata">
		<!-- F keywords -->
		<xsl:choose>
      <!-- namespace pages get the namespace keyword, if it exists -->
      <xsl:when test="$group='namespace'">
        <xsl:variable name="namespace" select="/document/reference/apidata/@name" />
        <xsl:if test="boolean($namespace)">
          <MSHelp:Keyword Index="F" Term="{$namespace}" />
        </xsl:if>
			</xsl:when>
      <!-- type pages get type and namespace.type keywords -->
      <xsl:when test="$group='type'">
        <xsl:variable name="namespace" select="/document/reference/containers/namespace/apidata/@name" />
        <xsl:variable name="type">
           <xsl:for-each select="/document/reference[1]">
             <xsl:call-template name="typeNamePlain">
               <xsl:with-param name="annotate" select="true()" />
             </xsl:call-template>
          </xsl:for-each>
        </xsl:variable>
        <MSHelp:Keyword Index="F" Term="{$type}" />
        <xsl:if test="boolean($namespace)">
          <MSHelp:Keyword Index="F" Term="{concat($namespace,'.',$type)}" />
        </xsl:if>
        <!-- some extra F keywords for some special types in the System namespace -->
        <xsl:if test="$namespace='System'">
          <xsl:choose>
            <xsl:when test="$type='Object'">
              <MSHelp:Keyword Index="F" Term="object" />
            </xsl:when>
            <xsl:when test="$type='String'">
              <MSHelp:Keyword Index="F" Term="string" />
            </xsl:when>
            <xsl:when test="$type='Int32'">
              <MSHelp:Keyword Index="F" Term="int" />
            </xsl:when>
            <xsl:when test="$type='Boolean'">
              <MSHelp:Keyword Index="F" Term="bool" />
            </xsl:when>
          </xsl:choose>
        </xsl:if>
      </xsl:when>
      <!-- member pages get member, type.member, and namepsace.type.member keywords -->
      <xsl:when test="$group='member'">
        <xsl:variable name="namespace" select="/document/reference/containers/namespace/apidata/@name" />
        <xsl:variable name="type">
          <xsl:for-each select="/document/reference/containers/type[1]">
            <xsl:call-template name="typeNamePlain">
              <xsl:with-param name="annotate" select="true()" />
            </xsl:call-template>
          </xsl:for-each>
        </xsl:variable>
        <xsl:variable name="member">
          <xsl:choose>
            <!-- if the member is a constructor, use the member name for the type name -->
            <xsl:when test="$subgroup='constructor'">
              <xsl:value-of select="$type" />
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="/document/reference/apidata/@name"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <MSHelp:Keyword Index="F" Term="{$member}" />
        <MSHelp:Keyword Index="F" Term="{concat($type, '.', $member)}" />
        <xsl:if test="boolean($namespace)">
          <MSHelp:Keyword Index="F" Term="{concat($namespace, '.', $type, '.', $member)}" />
        </xsl:if>
			</xsl:when>
		</xsl:choose>
	</xsl:template>

  <!--
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
  -->
  <xsl:template name="indexMetadata">
    <xsl:choose>
      <!-- namespace topics get one unqualified index entry -->
      <xsl:when test="$group='namespace'">
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
      <!-- type topics get unqualified and qualified index entries -->
      <xsl:when test="$group='type'">
        <xsl:variable name="names">
          <xsl:for-each select="/document/reference">
            <xsl:call-template name="textNames" />
          </xsl:for-each>
        </xsl:variable>
        <xsl:for-each select="msxsl:node-set($names)/name">
          <MSHelp:Keyword Index="K">
            <includeAttribute name="Term" item="{$subgroup}IndexEntry">
              <parameter>
                <xsl:value-of select="." />
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
            <includeAttribute name="Term" item="{$subgroup}IndexEntry">
              <parameter>
                <xsl:value-of select="." />
              </parameter>
            </includeAttribute>
          </MSHelp:Keyword>
        </xsl:for-each>
        <!-- enumeration topics also get entries for each member -->
      </xsl:when>
      <!-- constructor (or constructor overload) topics get unqualified entries using the type names -->
      <xsl:when test="$subgroup='constructor' and not(/document/reference/memberdata/@overload='true')">
        <xsl:variable name="names">
          <xsl:for-each select="/document/reference/containers/type">
            <xsl:call-template name="textNames" />
          </xsl:for-each>
        </xsl:variable>
        <xsl:for-each select="msxsl:node-set($names)/name">
          <MSHelp:Keyword Index="K">
            <includeAttribute name="Term" item="constructorIndexEntry">
                  <parameter>
                    <xsl:value-of select="." />
                  </parameter>
            </includeAttribute>
          </MSHelp:Keyword>
        </xsl:for-each>
      </xsl:when>
      <!-- other member (or overload) topics get qualified and unqualified entries using the member names -->
      <xsl:when test="$group='member' and not(/document/reference/memberdata/@overload='true')">
        <!-- don't create index entries for explicit interface implementations -->
        <xsl:if test="not(/document/reference/proceduredata/@virtual='true' and /document/reference/memberdata/@visibility='private')">
          <xsl:variable name="entryType">
            <xsl:choose>
              <xsl:when test="$subsubgroup">
                <xsl:value-of select="$subsubgroup" />
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$subgroup" />
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
                  <xsl:value-of select="." />
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
                  <xsl:value-of select="." />
                </parameter>
              </includeAttribute>
            </MSHelp:Keyword>
          </xsl:for-each>
        </xsl:if>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

</xsl:stylesheet>
