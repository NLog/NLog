<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0">

  <xsl:output indent="yes" encoding="UTF-8" />

  <!-- Merges reflection nodes for apis that are declared in multiple assemblies.
  For example, some of the same apis in the Microsoft.Windows.Themes namespace are declared in:
    PresentationFramework.Aero.dll
    PresentationFramework.Luna.dll
    PresentationFramework.Classic.dll
    PresentationFramework.Royale.dll
    
    This transform:
      - gets rid of duplicate element nodes in a namespace's api node
      - type api nodes: collapses duplicates into a single api node; saves library info for each duplicate
      - member api nodes: collapses duplicates into a single api node; saves library info for each duplicate
      - for element lists, add library info to elements that are not in all duplicates
  -->
  <xsl:key name="index" match="/*/apis/api" use="@id" />

  <xsl:template match="/">
    <reflection>
      <xsl:copy-of select="/*/@*"/>
      <xsl:copy-of select="/*/assemblies" />
      <xsl:apply-templates select="/*/apis" />
    </reflection>
  </xsl:template>

  <xsl:template match="apis">
    <apis>
      <xsl:apply-templates select="api" />
    </apis>
  </xsl:template>

  <xsl:template match="api">
    <xsl:copy-of select="." />
  </xsl:template>

  <xsl:template match="api[apidata/@group='namespace']">
    <api>
      <xsl:copy-of select="@*" />
      <xsl:for-each select="*">
        <xsl:choose>
          <xsl:when test="local-name()='elements'">
            <elements>
              <xsl:for-each select="element[not(@api=preceding-sibling::element/@api)]">
                <xsl:copy-of select="." />
              </xsl:for-each>
            </elements>
          </xsl:when>
          <xsl:otherwise>
            <xsl:copy-of select="." />
          </xsl:otherwise>
        </xsl:choose>
      </xsl:for-each>
    </api>
  </xsl:template>

  <xsl:template match="api[apidata/@group='type']">
    <xsl:variable name="ancestorId" select="family/ancestors/type[last()]/@api" />
    <xsl:variable name="duplicates" select="key('index',@id)" />
    <xsl:choose>
      <!-- if dupes, merge them -->
      <xsl:when test="count($duplicates)&gt;1">
        <xsl:variable name="typeId" select="@id" />
        <xsl:if test="not(preceding-sibling::api[@id=$typeId])">
          <xsl:call-template name="mergeDuplicateTypes">
            <xsl:with-param name="duplicates" select="$duplicates"/>
          </xsl:call-template>
        </xsl:if>
      </xsl:when>
      <!-- if no dupes, just copy it -->
      <xsl:otherwise>
        <xsl:copy-of select="." />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="mergeDuplicateTypes">
    <xsl:param name="duplicates"/>
    <xsl:variable name="typeId" select="@id"/>
    <xsl:variable name="duplicatesCount" select="count($duplicates)"/>
    <api>
      <xsl:copy-of select="@*" />
      <xsl:for-each select="*">
        <xsl:choose>
          <xsl:when test="local-name()='containers'">
            <containers>
              <xsl:copy-of select="$duplicates/containers/library" />
              <xsl:copy-of select="namespace|type" />
            </containers>
          </xsl:when>
          <xsl:when test="local-name()='elements'">
            <elements>
              <xsl:for-each select="$duplicates/elements/element">
                <xsl:variable name="elementId" select="@api"/>
                <xsl:if test="not(preceding::api[@id=$typeId]/elements/element[@api=$elementId])">
                  <!-- need to add library info to elements that are not in all duplicates -->
                  <element>
                    <xsl:copy-of select="@*"/>
                    <xsl:copy-of select="*"/>
                    <xsl:if test="count($duplicates/elements/element[@api=$elementId]) != $duplicatesCount">
                      <libraries>
                        <xsl:copy-of select="$duplicates/elements/element[@api=$elementId]/../../containers/library"/>
                      </libraries>
                    </xsl:if>
                  </element>
                </xsl:if>
              </xsl:for-each>
            </elements>
          </xsl:when>
          <xsl:otherwise>
            <xsl:copy-of select="." />
          </xsl:otherwise>
        </xsl:choose>
      </xsl:for-each>
    </api>
  </xsl:template>

  <xsl:template match="api[apidata/@group='member']">
    <xsl:variable name="subgroup" select="apidata/@subgroup" />
    <xsl:variable name="duplicates" select="key('index',@id)[apidata[@subgroup=$subgroup]]" />
    <xsl:choose>
      <!-- if dupes, merge them -->
      <xsl:when test="count($duplicates)&gt;1">
        <xsl:variable name="memberId" select="@id" />
        <xsl:if test="not(preceding-sibling::api[@id=$memberId][apidata[@subgroup=$subgroup]])">
          <xsl:call-template name="mergeDuplicateMembers">
            <xsl:with-param name="duplicates" select="$duplicates"/>
          </xsl:call-template>
        </xsl:if>
      </xsl:when>
      <!-- if no dupes, just copy it -->
      <xsl:otherwise>
        <xsl:copy-of select="." />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="mergeDuplicateMembers">
    <xsl:param name="duplicates"/>
    <api>
      <xsl:copy-of select="@*" />
      <xsl:for-each select="*">
        <xsl:choose>
          <xsl:when test="local-name()='containers'">
            <containers>
              <!-- include the library node for all the duplicates -->
              <xsl:copy-of select="$duplicates/containers/library" />
              <xsl:copy-of select="namespace|type" />
            </containers>
          </xsl:when>
          <xsl:otherwise>
            <xsl:copy-of select="." />
          </xsl:otherwise>
        </xsl:choose>
      </xsl:for-each>
    </api>
  </xsl:template>
  
</xsl:stylesheet>