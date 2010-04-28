<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.1" >
  
  <!-- *** GLOBAL VARIABLES *** -->

  <xsl:variable name="topic-group" select="/document/reference/topicdata/@group" />
  <xsl:variable name="topic-subgroup" select="/document/reference/topicdata/@subgroup" />

  <xsl:variable name="api-group" select="/document/reference/apidata/@group" />
  <xsl:variable name="api-subgroup" select="/document/reference/apidata/@subgroup" />
  <xsl:variable name="api-subsubgroup" select="/document/reference/apidata/@subsubgroup" />
  
  <!-- *** LOGIC FOR GENERATING ELEMENT LISTS *** -->
  <!-- Seperate logic for namespace lists, type lists, member lists, enumeration member lists, and overload lists -->
  
  <!-- *** LOGIC FOR GENERATING LINKS *** -->
  
  <xsl:template match="type" mode="link">
    <xsl:param name="qualified" select="false()" />
    <!-- we don't display outer types, because the link will show them -->
    <referenceLink target="{@api}" prefer-overload="false">
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
    <xsl:apply-templates select="specialization" mode="link"/>
  </xsl:template>

  <xsl:template match="specialization" mode="link">
    <span class="languageSpecificText">
    <span class="cs">&lt;</span>
    <span class="vb">
      <xsl:text>(Of </xsl:text>
    </span>
    <span class="cpp">&lt;</span>
    <span class="nu">(</span>
    </span>
    <xsl:for-each select="*">
      <xsl:apply-templates select="." mode="link" />
      <xsl:if test="position() != last()">
        <xsl:text>, </xsl:text>
      </xsl:if>
    </xsl:for-each>
    <span class="languageSpecificText">
    <span class="cs">&gt;</span>
    <span class="vb">)</span>
    <span class="cpp">&gt;</span>
    <span class="nu">)</span>
    </span>
  </xsl:template>

  <xsl:template match="arrayOf" mode="link">
    <xsl:param name="qualified" select="false()" />
    <span class="languageSpecificText">
      <span class="cpp">array&lt;</span>
    </span>
    <xsl:apply-templates mode="link">
      <xsl:with-param name="qualified" select="$qualified" />
    </xsl:apply-templates>
    <span class="languageSpecificText">
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
      <span class="nu">
        <xsl:text>[</xsl:text>
        <xsl:if test="number(@rank) &gt; 1">,</xsl:if>
        <xsl:text>]</xsl:text>
      </span>
    </span>
  </xsl:template>

  <xsl:template match="pointerTo" mode="link">
    <xsl:param name="qualified" select="false()" />
    <xsl:apply-templates mode="link">
      <xsl:with-param name="qualified" select="$qualified" />
    </xsl:apply-templates>
    <xsl:text>*</xsl:text>
  </xsl:template>

  <xsl:template match="referenceTo" mode="link">
    <xsl:param name="qualified" select="false()" />
    <xsl:apply-templates mode="link">
      <xsl:with-param name="qualified" select="$qualified" />
    </xsl:apply-templates>
    <span class="cpp">%</span>
  </xsl:template>

  <xsl:template match="template" mode="link">
    <xsl:choose>
      <xsl:when test="@api">
        <referenceLink target="{@api}">
          <span class="typeparam"><xsl:value-of select="@name" /></span>
        </referenceLink>
      </xsl:when>
      <xsl:otherwise>
        <span class="typeparam"><xsl:value-of select="@name" /></span>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="member" mode="link">
    <xsl:param name="qualified" select="true()" />
    <xsl:choose>
      <xsl:when test="@display-api">
        <referenceLink target="{@api}" display-target="{@display-api}">
          <xsl:if test="$qualified">
            <xsl:attribute name="show-container">true</xsl:attribute>
          </xsl:if>
        </referenceLink>
      </xsl:when>
      <xsl:otherwise>
        <referenceLink target="{@api}">
          <xsl:if test="$qualified">
            <xsl:attribute name="show-container">true</xsl:attribute>
          </xsl:if>
        </referenceLink>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- *** LOGIC FOR GENERATING TITLES *** -->
  
  <!-- when positioned on a parameterized api, produces a (plain) comma-seperated list of parameter types -->
  <xsl:template name="parameterTypesPlain">
    <xsl:if test="parameters/parameter">
      <xsl:text>(</xsl:text>
      <xsl:for-each select="parameters/parameter">
        <xsl:apply-templates select="type|arrayOf|pointerTo|referenceTo|template" mode="plain" />
        <xsl:if test="position() != last()">
          <xsl:text>, </xsl:text>
        </xsl:if>
      </xsl:for-each>
      <xsl:text>)</xsl:text>
    </xsl:if>
  </xsl:template>

  <!-- when position on a type api, produces a (plain) name; outer types are indicated by dot-seperators; -->
  <!-- generic types are indicated by a keyword, because we can't show templates in a language-independent way -->
  <xsl:template name="typeNamePlain">
    <xsl:if test="type|(containers/type)">
      <xsl:apply-templates select="type|(containers/type)" mode="plain" />
      <xsl:text>.</xsl:text>
    </xsl:if>
    <xsl:value-of select="apidata/@name" />
    <xsl:choose>
      <xsl:when test="specialization">
        <xsl:apply-templates select="specialization" mode="plain" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:apply-templates select="templates" mode="plain" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="type" mode="plain">
    <xsl:call-template name="typeNamePlain" />
  </xsl:template>

  <xsl:template match="specialization|templates" mode="plain">
    <xsl:text>(</xsl:text>
    <xsl:for-each select="*">
      <xsl:apply-templates select="." mode="plain" />
      <xsl:if test="position() != last()">
        <xsl:text>, </xsl:text>
      </xsl:if>
    </xsl:for-each>
    <xsl:text>)</xsl:text>
  </xsl:template>
  
  <xsl:template match="arrayOf" mode="plain">
    <xsl:apply-templates select="type|arrayOf|pointerTo|referenceTo|template|specialization|templates" mode="plain"/>
    <xsl:text>[</xsl:text>
    <xsl:if test="number(@rank) &gt; 1">,</xsl:if>
    <xsl:text>]</xsl:text>
  </xsl:template>

  <xsl:template match="pointerTo" mode="plain">
    <xsl:apply-templates select="type|arrayOf|pointerTo|referenceTo|template|specialization|templates" mode="plain"/>
    <xsl:text>*</xsl:text>
  </xsl:template>
  
  <xsl:template match="referenceTo" mode="plain">
    <xsl:apply-templates select="type|arrayOf|pointerTo|referenceTo|template|specialization|templates" mode="plain"/>
  </xsl:template>

  <xsl:template match="template" mode="plain">
    <xsl:value-of select="@name" />
  </xsl:template>

  <!-- when positioned on a generic api, produces a (decorated) comma-seperated list of template names -->
  <xsl:template name="parameterTypesDecorated">
    <xsl:if test="parameters/parameter">
      <xsl:text>(</xsl:text>
      <xsl:for-each select="parameters/parameter">
        <xsl:apply-templates select="type|arrayOf|pointerTo|referenceTo|template" mode="decorated"/>
        <xsl:if test="position() != last()">
          <xsl:text>, </xsl:text>
        </xsl:if>
      </xsl:for-each>
      <xsl:text>)</xsl:text>
    </xsl:if>
  </xsl:template>

  <!-- when position on a type api, produces a (decorated) name, including outer types and templates -->
  <xsl:template name="typeNameDecorated">
    <xsl:if test="type|(containers/type)">
      <xsl:apply-templates select="type|(containers/type)" mode="decorated" />
      <span class="languageSpecificText">
      <span class="cs">.</span>
      <span class="vb">.</span>
      <span class="cpp">::</span>
      <span class="nu">.</span>
      </span>
    </xsl:if>
    <xsl:value-of select="apidata/@name" />
    <xsl:choose>
      <xsl:when test="specialization">
        <xsl:apply-templates select="specialization" mode="decorated" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:apply-templates select="templates" mode="decorated" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  
  <xsl:template match="type" mode="decorated">
    <xsl:call-template name="typeNameDecorated" />
  </xsl:template>
  
  <xsl:template match="specialization|templates" mode="decorated">
    <span class="languageSpecificText">
    <span class="cs">&lt;</span>
    <span class="vb">
      <xsl:text>(Of </xsl:text>
    </span>
    <span class="cpp">&lt;</span>
    <span class="nu">(</span>
    </span>
    <xsl:for-each select="*">
      <xsl:apply-templates select="." mode="decorated" />
      <xsl:if test="position() != last()">
        <xsl:text>, </xsl:text>
      </xsl:if>
    </xsl:for-each>
    <span class="languageSpecificText">
    <span class="cs">&gt;</span>
    <span class="vb">)</span>
    <span class="cpp">&gt;</span>
    <span class="nu">)</span>
    </span>
  </xsl:template>

  <xsl:template match="arrayOf" mode="decorated">
    <span class="languageSpecificText">
      <span class="cpp">array&lt;</span>
    </span>
    <xsl:apply-templates select="type|arrayOf|pointerTo|referenceTo|template|specialization|templates" mode="decorated" />
    <span class="languageSpecificText">
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
      <span class="nu">
        <xsl:text>[</xsl:text>
        <xsl:if test="number(@rank) &gt; 1">,</xsl:if>
        <xsl:text>]</xsl:text>
      </span>
    </span>
  </xsl:template>

  <xsl:template match="pointerTo" mode="decorated">
    <xsl:apply-templates select="type|arrayOf|pointerTo|referenceTo|template|specialization|templates" mode="decorated"/>
    <xsl:text>*</xsl:text>
  </xsl:template>

  <xsl:template match="referenceTo" mode="decorated">
    <xsl:apply-templates select="type|arrayOf|pointerTo|referenceTo|template|specialization|templates" mode="decorated"/>
    <span class="cpp">%</span>
  </xsl:template>

  <xsl:template match="template" mode="decorated">
    <span class="typeparameter"><xsl:value-of select="@name" /></span>
  </xsl:template>
  
  <!-- when positioned on a parameterized api, produces a (plain) comma-seperated list of parameter names-->
  <xsl:template name="parameterNames">
    <xsl:if test="parameters/parameter">
      <xsl:text>(</xsl:text>
      <xsl:for-each select="parameters/parameter">
        <xsl:value-of select="@name" />
        <xsl:if test="position() != last()">
          <xsl:text>, </xsl:text>
        </xsl:if>
      </xsl:for-each>
      <xsl:text>)</xsl:text>
    </xsl:if>
  </xsl:template>
  
</xsl:stylesheet>
