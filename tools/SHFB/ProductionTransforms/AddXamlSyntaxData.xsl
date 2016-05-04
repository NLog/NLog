<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0" xmlns:msxsl="urn:schemas-microsoft-com:xslt">

  <xsl:output indent="yes" encoding="UTF-8" />

  <!-- <xsl:key name="typeIndex" match="/reflection/types/type" use="@id" /> -->
  <xsl:key name="defaultContructorIndex" match="/*/apis/api[apidata[@subgroup='constructor'] and not(parameters) and memberdata[@visibility='public']]" use="@id" />
  <xsl:key name="typeIndex" match="/*/apis/api[apidata[@group='type']]" use="@id" />

  <xsl:key name="settablePropertyIndex" match="/*/apis/api[apidata[@subgroup='property']][propertydata[@set='true']][(memberdata[@visibility='public'] and not(propertydata[@set-visibility!='public'])) or propertydata[@set-visibility='public']]" use="@id" />
  
  <xsl:template match="/">
    <reflection>

      <!-- assemblies and namespaces get copied undisturbed -->
      <xsl:copy-of select="/*/assemblies" />

      <apis>
        <xsl:apply-templates select="/*/apis/api" />
      </apis>
      
    </reflection>
  </xsl:template>

  <xsl:template match="api">
    <xsl:choose>
      <xsl:when test="apidata[@group='type']">
        <xsl:call-template name="updateTypeNode"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:copy-of select="." />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="GetContentProperty">
    <xsl:if test="apidata[@subgroup='class' or @subgroup='structure']">
      <xsl:variable name="contentPropertyName">
        <xsl:value-of select="attributes/attribute[type[contains(@api,'.ContentPropertyAttribute')]]/argument/value"/>
      </xsl:variable>
      <xsl:if test="$contentPropertyName!=''">
        <xsl:value-of select="concat('P:',substring-after(@id,'T:'),'.',$contentPropertyName)"/>
      </xsl:if>
    </xsl:if>
  </xsl:template>
  
  <xsl:template name="updateTypeNode">
    <xsl:variable name="defaultConstructor">
      <xsl:if test="apidata[@subgroup='class' or @subgroup='structure']">
        <xsl:for-each select="elements/element">
          <xsl:if test="key('defaultContructorIndex',@api)">
            <xsl:value-of select="@api"/>
          </xsl:if>
        </xsl:for-each>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="hasSettableProperties">
      <xsl:if test="apidata[@subgroup='structure']">
        <xsl:for-each select="elements/element">
          <xsl:if test="key('settablePropertyIndex',@api)">true</xsl:if>
        </xsl:for-each>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="contentPropertyId">
      <xsl:call-template name="GetContentProperty"/>
    </xsl:variable>
    <xsl:variable name="typeSubGroup">
      <xsl:value-of select="apidata/@subgroup"/>
    </xsl:variable>
    <api>
      <xsl:copy-of select="@*"/>
      <xsl:for-each select="*">
        <xsl:choose>
          <xsl:when test="local-name()='typedata'">
            <typedata>
              <xsl:copy-of select="@*"/>
              <xsl:if test="normalize-space($defaultConstructor)!=''">
                <xsl:attribute name="defaultConstructor">
                  <xsl:value-of select="normalize-space($defaultConstructor)"/>
                </xsl:attribute>
              </xsl:if>
              <xsl:if test="$contentPropertyId!=''">
                <xsl:attribute name="contentProperty">
                  <xsl:value-of select="$contentPropertyId"/>
                </xsl:attribute>
              </xsl:if>
              <xsl:if test="$typeSubGroup='structure' and normalize-space($hasSettableProperties)=''">
                <xsl:attribute name="noSettableProperties">
                  <xsl:value-of select="'true'"/>
                </xsl:attribute>
              </xsl:if>
              <xsl:copy-of select="*"/>
            </typedata>
          </xsl:when>
          <xsl:when test="local-name()='family'">
            <xsl:choose>
              <xsl:when test="$contentPropertyId=''">
                <family>
                  <xsl:for-each select="*">
                    <xsl:choose>
                      <xsl:when test="local-name()='ancestors'">
                        <ancestors>
                          <xsl:for-each select="type">
                            <xsl:variable name="ancestorContentPropertyId">
                              <xsl:for-each select="key('typeIndex', @api)">
                                <xsl:call-template name="GetContentProperty"/>
                              </xsl:for-each>
                            </xsl:variable>
                            <type>
                              <xsl:copy-of select="@*"/>
                              <xsl:if test="$ancestorContentPropertyId!=''">
                                <xsl:attribute name="contentProperty">
                                  <xsl:value-of select="$ancestorContentPropertyId"/>
                                </xsl:attribute>
                              </xsl:if>
                              <xsl:copy-of select="*"/>
                            </type>
                          </xsl:for-each>
                        </ancestors>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:copy-of select="."/>
                      </xsl:otherwise>
                    </xsl:choose>
                  </xsl:for-each>
                </family>
              </xsl:when>
              <xsl:otherwise>
                <xsl:copy-of select="."/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:otherwise>
            <xsl:copy-of select="."/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:for-each>
    </api>
  </xsl:template>

</xsl:stylesheet>
