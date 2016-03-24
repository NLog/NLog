<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0">

  <xsl:output indent="yes" encoding="UTF-8" />

  <xsl:param name="componentizeBy"/>

  <xsl:key name="index" match="/reflection/apis/api" use="containers/namespace/@api" />
  <xsl:key name="indexByAssembly" match="/reflection/apis/api[containers/library]" use="containers/library[1]/@assembly" />

  <xsl:template match="/">
    <topics>
      <!-- process the root node -->
      <xsl:apply-templates select="/reflection/apis/api[topicdata[@group='root']]" />
      <xsl:choose>

        <!-- sort by assembly -->
        <xsl:when test="$componentizeBy='assembly'">
          <!-- loop through the assemblies and output the apis in each assembly -->
          <xsl:for-each select="/reflection/assemblies/assembly">
            <xsl:apply-templates select="key('indexByAssembly',@name)" />
          </xsl:for-each>
          <!-- process the namespace nodes, which may be associated with more than one assembly -->
          <xsl:apply-templates select="/reflection/apis/api[apidata[@group='namespace']]" />
        </xsl:when>

        <!-- default is to sort by namespace -->
        <xsl:otherwise>
          <!-- process each namespace's api node, then process nodes for all the apis in the namespace -->
          <xsl:for-each select="/reflection/apis/api[apidata[@group='namespace']]">
            <xsl:apply-templates select="." />
            <xsl:apply-templates select="key('index',@id)" />
          </xsl:for-each>
        </xsl:otherwise>

      </xsl:choose>
      <!-- Process namespace-groups as well -->
      <xsl:apply-templates select="/reflection/apis/api[apidata[@group='namespaceGroup']]" />
    </topics>
  </xsl:template>

  <!-- namespace and member topics -->
  <xsl:template match="api">
    <xsl:if test="not(topicdata/@notopic)">
      <topic id="{@id}"/>
    </xsl:if>
  </xsl:template>

</xsl:stylesheet>
