<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" version="1.1">

  <xsl:output indent="yes" encoding="UTF-8" />

  <xsl:param name="project" select="string('Project')" />

  <xsl:key name="index" match="/*/apis/api" use="@id" />

  <xsl:variable name="types">
    <xsl:call-template name="types" />
  </xsl:variable>
  
  <xsl:template match="/">
    <reflection>
      <!--
      <xsl:call-template name="types" /> 
      -->
      <xsl:apply-templates select="/*/assemblies" />
      <xsl:apply-templates select="/*/apis" />
    </reflection>
  </xsl:template>


  <xsl:template match="assemblies">
    <xsl:copy-of select="." />
  </xsl:template>

  <xsl:template match="apis">
    <apis>
      <xsl:apply-templates select="api" />
      <xsl:call-template name="projectTopic" />
    </apis>
  </xsl:template>

  
  <!-- by default, an api is just copied and topicdata added -->
  <xsl:template match="api">
    <xsl:call-template name="apiTopic" />
  </xsl:template>

  <!-- for type apis, we also generate overload pages -->
  <xsl:template match="api[apidata/@group='type']">
    <!-- first reproduce the type API -->
    <xsl:call-template name="apiTopic" />
    <!-- now create overload APIs -->
    <xsl:variable name="type" select="." />
    <xsl:variable name="typeId" select="@id" />
    <xsl:for-each select="msxsl:node-set($types)/type[@api=$typeId]/overload">
      <api>
        <xsl:attribute name="id">
          <xsl:call-template name="overloadIdentifier">
            <xsl:with-param name="typeId" select="$typeId" />
            <xsl:with-param name="memberName" select="@name" />
          </xsl:call-template>
        </xsl:attribute>
        <topicdata group="list" subgroup="overload" />
        <apidata name="{@name}" group="member" subgroup="{@subgroup}" />
        <containers>
          <xsl:copy-of select="$type/containers/library"/>
          <xsl:copy-of select="$type/containers/namespace"/>
          <type api="{$typeId}">
            <xsl:copy-of select="$type/containers/type"/>
          </type>
        </containers>
        <elements>
          <xsl:for-each select="member">
            <element>
              <xsl:copy-of select="@api|@display-api"/>
            </element>
          </xsl:for-each>
        </elements>
      </api>
    </xsl:for-each>
    <!--
    <xsl:variable name="members" select="key('index',elements/element/@api)" />
    <xsl:for-each select="$members">
      <xsl:variable name="name" select="apidata/@name" />
      <xsl:variable name="subgroup" select="apidata/@subgroup" />
      <xsl:variable name="set" select="$members[apidata/@name=$name and apidata/@subgroup=$subgroup]" />
      <xsl:if test="(count($set) &gt; 1) and (($set[containers/type/@api=$typeId][1]/@id)=@id)">
        <api>
          <xsl:attribute name="id">
            <xsl:call-template name="overloadId">
              <xsl:with-param name="memberId" select="@id" />
            </xsl:call-template>
          </xsl:attribute>
          <topicdata group="list" />
          <apidata name="{apidata/@name}" group="{apidata/@group}" subgroup="{apidata/@subgroup}" />
          <containers>
            <library assembly="{containers/library/@assembly}" module="{containers/library/@module}"/>
            <namespace api="{containers/namespace/@api}" />
            <type api="{containers/type/@api}" />
          </containers>
          <elements>
            <xsl:for-each select="$set">
              <element api="{@id}" />
            </xsl:for-each>
          </elements>
        </api>
      </xsl:if>
    </xsl:for-each>
    -->
  </xsl:template>

  <!-- for member apis, we add an overload element if the member is overloaded -->
  <xsl:template match="api[apidata/@group='member']">
    <xsl:variable name="typeId" select="containers/type/@api" />
    <xsl:variable name="memberId" select="@id" />
    <xsl:if test="not(key('index',$typeId)/apidata/@subgroup='enumeration')">
      <api id="{$memberId}">
        <topicdata group="api" />
        <xsl:copy-of select="*" />
        <xsl:variable name="overloadId" select="msxsl:node-set($types)/type[@api=$typeId]//member[@api=$memberId]/parent::overload/@api" />
        <xsl:if test="$overloadId">
          <overload api="{$overloadId}" />
        </xsl:if>
      </api>
    </xsl:if>
  </xsl:template>

  <xsl:template name="apiTopic">
    <api id="{@id}">
      <topicdata group="api" />
      <xsl:copy-of select="*" />
    </api>
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
  
  <!--
  <xsl:template name="overloadId">
    <xsl:param name="memberId" />
    <xsl:text>Overload:</xsl:text>
    <xsl:variable name="noParameters">
      <xsl:choose>
        <xsl:when test="contains($memberId,'(')">
          <xsl:value-of select="substring-before($memberId,'(')" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$memberId" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="noGeneric">
      <xsl:choose>
        <xsl:when test="contains($noParameters,'``')">
          <xsl:value-of select="substring-before($noParameters,'``')" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$noParameters" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:value-of select="substring($noGeneric,3)" />
  </xsl:template>
  -->

  <!-- logic to construct an in-memory tree that represents types and members organized into overload sets -->

  <!-- create a list of types with member elements organized by overload set -->
  <xsl:template name="types">
    <xsl:for-each select="/*/apis/api[apidata/@group='type']">
      <type api="{@id}">
        <xsl:call-template name="overloads" />
      </type>
    </xsl:for-each>
  </xsl:template>

  <!-- organize member list into overload sets -->
  <!-- overload sets share a name and subgroup -->
  <xsl:template name="overloads">
    <xsl:variable name="typeId" select="@id" />
    <xsl:variable name="members">
      <xsl:call-template name="members" />
    </xsl:variable>
    <xsl:for-each select="msxsl:node-set($members)/member">
      <xsl:variable name="name" select="@name" />
      <!-- on the first occurence of a member name... -->
      <xsl:if test="not(preceding-sibling::member[@name=$name])">
        <xsl:choose>
          <!-- ...if there are subsequent members with that name, create an overload set -->
          <xsl:when test="following-sibling::member[@name=$name]">
            <overload name="{$name}" subgroup="{@subgroup}">
              <xsl:attribute name="api">
                <xsl:call-template name="overloadIdentifier">
                  <xsl:with-param name="typeId" select="$typeId" />
                  <xsl:with-param name="memberName" select="$name" />
                </xsl:call-template>
              </xsl:attribute>
              <xsl:for-each select="../member[@name=$name]">
                <member>
                  <xsl:copy-of select="@api|@display-api"/>
                </member>
               </xsl:for-each>
            </overload>
          </xsl:when>
          <!-- otherwise, just copy the member entry -->
          <xsl:otherwise>
            <member api="{@api}" />
          </xsl:otherwise>
        </xsl:choose>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>

  <!-- collect list of members for a given type -->
  <!-- each member lists api id, name, subgroup, and display-api -->
  <xsl:template name="members">
    <xsl:for-each select="elements/element">
      <member api="{@api}">
        <xsl:if test="@display-api">
          <xsl:attribute name="display-api">
            <xsl:value-of select="@display-api"/>
          </xsl:attribute>
        </xsl:if>
      <xsl:choose>
        <xsl:when test="apidata">
          <xsl:attribute name="name">
            <xsl:value-of select="apidata/@name" />
          </xsl:attribute>
          <xsl:attribute name="subgroup">
            <xsl:value-of select="apidata/@subgroup" />
          </xsl:attribute>
        </xsl:when>
        <xsl:otherwise>
          <xsl:variable name="api" select="key('index',@api)" />
          <xsl:attribute name="name">
            <xsl:value-of select="$api/apidata/@name"/>
          </xsl:attribute>
          <xsl:attribute name="subgroup">
            <xsl:value-of select="$api/apidata/@subgroup"/>
          </xsl:attribute>
        </xsl:otherwise>
      </xsl:choose>
      </member>
    </xsl:for-each>
  </xsl:template>

  <!-- given a type and a member name, construct an overload identifier -->
  <xsl:template name="overloadIdentifier">
    <xsl:param name="typeId" />
    <xsl:param name="memberName" />
    <xsl:text>Overload:</xsl:text>
    <xsl:value-of select="substring($typeId,3)"/>
    <xsl:text>.</xsl:text>
    <xsl:value-of select="$memberName"/>
  </xsl:template>
  
</xsl:stylesheet>
