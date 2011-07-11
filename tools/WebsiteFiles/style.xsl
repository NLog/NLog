<?xml version="1.0" encoding="utf-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0" xmlns="http://www.w3.org/1999/xhtml">
  <!-- 
     to modify/debug, copy build\bin\Debug\NLogMerged.api.xml to the directory containing the xsl
     and open it in IE
  -->
  <xsl:param name="kind">target</xsl:param>
  <xsl:param name="kindName">Target</xsl:param>
  <xsl:param name="name"></xsl:param>
  <xsl:param name="mode">merged</xsl:param>
  <xsl:param name="slug"></xsl:param>

  <xsl:output omit-xml-declaration="yes" method="xml"
              doctype-public="-//W3C//DTD XHTML 1.0 Transitional//EN"
              doctype-system="http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd" />

  <xsl:template match="/">
    <xsl:choose>
      <xsl:when test="$mode = 'web'">
        <div class="generated-doc">
          <xsl:choose>
            <xsl:when test="$slug = 'targets'">
              <xsl:call-template name="target-list" />
            </xsl:when>
            <xsl:when test="$slug = 'wrapper-targets'">
              <xsl:call-template name="wrapper-target-list" />
            </xsl:when>
            <xsl:when test="$slug = 'layout-renderers'">
              <xsl:call-template name="layout-renderer-list" />
            </xsl:when>
            <xsl:when test="$slug = 'wrapper-layout-renderers'">
              <xsl:call-template name="wrapper-layout-renderer-list" />
            </xsl:when>
            <xsl:when test="$slug = 'filters'">
              <xsl:call-template name="filter-list" />
            </xsl:when>
            <xsl:when test="$slug = 'layouts'">
              <xsl:call-template name="layout-list" />
            </xsl:when>
            <xsl:otherwise>
              <xsl:apply-templates select="/types/type[@slug=$slug]" />
            </xsl:otherwise>
          </xsl:choose>
        </div>
      </xsl:when>
      <xsl:otherwise>
        <html>
          <head>
            <style>
              body
              {
                font-family: Tahoma;
              }
            </style>
            <link type="text/css" rel="stylesheet" href="docstyle.css" />
          </head>
          <body>
            <div style="width: 600px; margin-left: auto; margin-right: auto;">
              <xsl:call-template name="target-list" />
              <xsl:call-template name="wrapper-target-list" />
              <xsl:call-template name="filter-list" />
              <xsl:call-template name="layout-list" />
              <xsl:call-template name="layout-renderer-list" />
              <xsl:call-template name="wrapper-layout-renderer-list" />
            </div>
            <xsl:apply-templates select="/types/type">
              <xsl:sort select="@kind" />
              <xsl:sort select="@title" />
            </xsl:apply-templates>
          </body>
        </html>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="target-list">
    <ul>
      <xsl:for-each select="/types/type[@kind='target' and not(@iswrapper) and not(@iscompound)]">
        <xsl:sort select="@title" order="ascending" />
        <li>
          <a>
            <xsl:attribute name="href">
              <xsl:apply-templates select="." mode="typeLink" />
            </xsl:attribute>
            <xsl:value-of select="@name"/>
          </a>
          <xsl:apply-templates select="supported-in" mode="float" />
          <xsl:text> - </xsl:text>
          <xsl:apply-templates select="doc/summary" />
        </li>
      </xsl:for-each>
    </ul>
  </xsl:template>

  <xsl:template name="wrapper-target-list">
    <ul>
      <xsl:for-each select="/types/type[@kind='target' and (@iswrapper or @iscompound)]">
        <xsl:sort select="@title" order="ascending" />
        <li>
          <a>
            <xsl:attribute name="href">
              <xsl:apply-templates select="." mode="typeLink" />
            </xsl:attribute>
            <xsl:value-of select="@name"/>
          </a>
          <xsl:apply-templates select="supported-in" mode="float" />
          <xsl:text> - </xsl:text>
          <xsl:apply-templates select="doc/summary" />
        </li>
      </xsl:for-each>
    </ul>
  </xsl:template>

  <xsl:template name="layout-renderer-list">
    <ul>
      <xsl:for-each select="/types/type[@kind='layout-renderer' and not(@iswrapper)]">
        <xsl:sort select="@title" order="ascending" />
        <li>
          <a>
            <xsl:attribute name="href">
              <xsl:apply-templates select="." mode="typeLink" />
            </xsl:attribute>
            <xsl:value-of select="@title"/>
          </a>
          <xsl:apply-templates select="supported-in" mode="float" />
          <xsl:text> - </xsl:text>
          <xsl:apply-templates select="doc/summary" />
        </li>
      </xsl:for-each>
    </ul>
  </xsl:template>
  <xsl:template name="wrapper-layout-renderer-list">
    <ul>
      <xsl:for-each select="/types/type[@kind='layout-renderer' and @iswrapper]">
        <xsl:sort select="@title" order="ascending" />
        <li>
          <a>
            <xsl:attribute name="href">
              <xsl:apply-templates select="." mode="typeLink" />
            </xsl:attribute>
            <xsl:value-of select="@title"/>
          </a>
          <xsl:apply-templates select="supported-in" mode="float" />
          <xsl:text> - </xsl:text>
          <xsl:apply-templates select="doc/summary" />
        </li>
      </xsl:for-each>
    </ul>
  </xsl:template>

  <xsl:template name="layout-list">
    <ul>
      <xsl:for-each select="/types/type[@kind='layout']">
        <xsl:sort select="@title" order="ascending" />
        <li>
          <a>
            <xsl:attribute name="href">
              <xsl:apply-templates select="." mode="typeLink" />
            </xsl:attribute>
            <xsl:value-of select="@title"/>
          </a>
          <xsl:apply-templates select="supported-in" mode="float" />
          <xsl:text> - </xsl:text>
          <xsl:apply-templates select="doc/summary" />
        </li>
      </xsl:for-each>
    </ul>
  </xsl:template>

  <xsl:template name="filter-list">
    <ul>
      <xsl:for-each select="/types/type[@kind='filter']">
        <xsl:sort select="@title" order="ascending" />
        <li>
          <a>
            <xsl:attribute name="href">
              <xsl:apply-templates select="." mode="typeLink" />
            </xsl:attribute>
            <xsl:value-of select="@title"/>
          </a>
          <xsl:apply-templates select="supported-in" mode="float" />
          <xsl:text> - </xsl:text>
          <xsl:apply-templates select="doc/summary" />
        </li>
      </xsl:for-each>
    </ul>
  </xsl:template>

  <xsl:template match="type">
    <p class="summary">
      <xsl:apply-templates select="doc/summary" />
      <a name="#{@slug}">&#160;</a>
    </p>
    <xsl:if test="$mode = 'merged'">
      <hr />
      <h3>
        <xsl:value-of select="@title"/>
      </h3>
    </xsl:if>
    <xsl:text>Supported in </xsl:text><xsl:apply-templates select="supported-in" />
    <h4>Configuration Syntax</h4>
    <xsl:apply-templates select="." mode="usage-example" />
    <small>
      <xsl:text>Read more about using the </xsl:text><a href="Configuration_file">Configuration File.</a>
    </small>
    <xsl:if test="property">
      <h4>Parameters</h4>
      <ul class="config-properties">
        <xsl:call-template name="property-grouping">
          <xsl:with-param name="list" select="property" />
        </xsl:call-template>
      </ul>
    </xsl:if>
    <xsl:if test="doc/remarks">
      <h4>Remarks</h4>
      <p class="remarks">
        <xsl:apply-templates select="doc/remarks" />
      </p>
    </xsl:if>
  </xsl:template>

  <xsl:template name="property-grouping">
    <xsl:param name="list" />
    <xsl:variable name="group-identifier" select="$list[1]/@category"/>
    <xsl:variable name="group" select="$list[@category = $group-identifier]"/>

    <li class="property-group">
      <xsl:value-of select="$group-identifier" />
    </li>
      <xsl:apply-templates select="$group" />

    <xsl:if test="count($list) > count($group)">
      <xsl:call-template name="property-grouping">
        <xsl:with-param name="list" select="$list[not(@category=$group-identifier)]"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <xsl:template match="type[@kind='layout-renderer']" mode="usage-example">
    <div class="usage-example">
      <xsl:variable name="spacing" select="substring('&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;',1,string-length(@name)+2)" />
      <xsl:text>${</xsl:text><xsl:value-of select="@name"/>
      <xsl:variable name="lineBreaks" select="count(property) > 3" />
      <xsl:for-each select="property">
        <xsl:if test="$lineBreaks and (position() mod 3) = 1 and position() != 1"><br/>
          <xsl:value-of select="$spacing"/>
        </xsl:if><span>
        <xsl:if test="@required='1'">
          <xsl:attribute name="class">requiredparameter</xsl:attribute>
        </xsl:if>
        <xsl:text>:</xsl:text><xsl:call-template name="property-link" />=<span class="typeplaceholder"><xsl:value-of select="@type"/></span>
      </span>
      </xsl:for-each><xsl:text>}</xsl:text>
    </div>

	<xsl:variable name="ambientPropertyName" select="@ambientProperty" />
	<xsl:if test="$ambientPropertyName != ''">
	<p>or by using ambient property to modify output of other layout renderer:</p>
    <div class="usage-example">
      <xsl:variable name="spacing" select="substring('&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;',1,string-length(@name)+2)" />
      <xsl:text>${other</xsl:text>
      <xsl:variable name="lineBreaks" select="count(property) > 3" />
      <xsl:for-each select="property">
	  	<xsl:if test="@name=$ambientPropertyName">
        <span>
        <xsl:if test="@required='1'">
          <xsl:attribute name="class">requiredparameter</xsl:attribute>
        </xsl:if>
        <xsl:text>:</xsl:text><xsl:call-template name="property-link" />=<span class="typeplaceholder"><xsl:value-of select="@type"/></span>
      </span>
	  </xsl:if>
      </xsl:for-each><xsl:text>}</xsl:text>
    </div>
	</xsl:if>
  </xsl:template>
  
  <xsl:template match="type[@kind='target']" mode="usage-example">
    <xsl:variable name="isWrapper" select="@iswrapper = '1'" />
    <xsl:variable name="isCompound" select="@iscompound = '1'" />

    <div class="usage-example">
      <xsl:text>&lt;targets&gt;</xsl:text><br/>
      <xsl:text>&#160;&#160;&lt;target </xsl:text><span class="requiredparameter">
        <xsl:text>xsi:type="</xsl:text><xsl:value-of select="@name"/><xsl:text>"</xsl:text>
      </span>
      <xsl:variable name="lineBreaks" select="count(property) > 3" />
      <xsl:for-each select="property[not(@type='Collection') and not(@type='Target')]">
        <xsl:if test="$lineBreaks">
          <br/>
          <xsl:text>&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;</xsl:text>
        </xsl:if>
        <span>
          <xsl:if test="@required='1'">
            <xsl:attribute name="class">requiredparameter</xsl:attribute>
          </xsl:if>
          <xsl:text> </xsl:text>
          <xsl:call-template name="property-link" />
          <xsl:text>="</xsl:text><span class="typeplaceholder">
            <xsl:value-of select="@type"/>
          </span><xsl:text>"</xsl:text>
        </span>
      </xsl:for-each>

      <xsl:if test="property[@type='Collection' or @type='Target'] or $isWrapper or $isCompound">
        <xsl:text>&gt;</xsl:text><br/>
      </xsl:if>

      <xsl:if test="$isWrapper">
        <xsl:text>&#160;&#160;&#160;&#160;</xsl:text><span class="requiredparameter">&lt;target xsi:type="wrappedTargetType" ...target properties... /&gt;</span><br/>
      </xsl:if>

      <xsl:if test="$isCompound">
        <xsl:text>&#160;&#160;&#160;&#160;</xsl:text><span class="requiredparameter">&lt;target xsi:type="wrappedTargetType" ... /&gt;</span><br/>
        <xsl:text>&#160;&#160;&#160;&#160;&lt;target xsi:type="wrappedTargetType" ... /&gt;</xsl:text><br/>
        <xsl:text>&#160;&#160;&#160;&#160;...</xsl:text><br/>
        <xsl:text>&#160;&#160;&#160;&#160;&lt;target xsi:type="wrappedTargetType" ... /&gt;</xsl:text>
        <br/>
      </xsl:if>

      <xsl:for-each select="property[@type='Collection']">
        <xsl:text>&#160;&#160;&#160;&#160;&lt;</xsl:text><xsl:value-of select="elementType/@elementTag"/>
        <xsl:variable name="lineBreaks2" select="count(elementType/property[not(@type='Collection')]) > 3" />
        <xsl:variable name="spacing2" select="substring('&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;',1,string-length(@name)-2)" />
        <xsl:for-each select="elementType/property[not(@type='Collection')]">
          <xsl:if test="$lineBreaks and (position() mod 3) = 1 and position() != 1">
            <br/>
            <xsl:value-of select="$spacing2"/>
          </xsl:if>
          <xsl:text> </xsl:text>
          <xsl:call-template name="property-link" /><xsl:text>="</xsl:text><span class="typeplaceholder">
            <xsl:value-of select="@type"/>
          </span><xsl:text>"</xsl:text>
        </xsl:for-each>
        <xsl:text>/&gt;</xsl:text>
        <span class="comment">&lt;!-- repeated --&gt;</span><br/>
      </xsl:for-each>

      <xsl:choose>
        <xsl:when test="property[@type='Collection' or @type='Target'] or $isWrapper or $isCompound">
          <xsl:text>&#160;&#160;&lt;/target&gt;</xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text> /&gt;</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
      <br/><xsl:text>&lt;/targets&gt;</xsl:text>
    </div>
  </xsl:template>

  <xsl:template match="type[@kind='filter']" mode="usage-example">
    <div class="usage-example">
      <xsl:text>&lt;rules&gt;</xsl:text><br/>
      <xsl:text>&#160;&#160;&lt;logger ... &gt;</xsl:text><br/>
      <xsl:text>&#160;&#160;&#160;&#160;&lt;</xsl:text><xsl:value-of select="@name"/>
      <xsl:for-each select="property[not(@type='Collection') and not(@type='Target')]">
        <span>
          <xsl:if test="@required='1'">
            <xsl:attribute name="class">requiredparameter</xsl:attribute>
          </xsl:if>
          <xsl:text>&#160;</xsl:text>
          <xsl:call-template name="property-link" />
          <xsl:text>="</xsl:text><span class="typeplaceholder">
            <xsl:value-of select="@type"/>
          </span><xsl:text>"</xsl:text>
        </span>
      </xsl:for-each>

      <xsl:text>/&gt;</xsl:text><br/>
      <xsl:text>&#160;&#160;&lt;/logger&gt;</xsl:text><br/>
      <xsl:text>&lt;/rules&gt;</xsl:text>
    </div>
  </xsl:template>

  <xsl:template match="type[@kind='layout']" mode="usage-example">
    <div class="usage-example">
      <xsl:text>&lt;targets&gt;</xsl:text><br/>
      <xsl:text>&#160;&#160;&lt;target&gt;</xsl:text><br/>
      <xsl:text>&#160;&#160;&#160;&#160;&lt;layout xsi:type="</xsl:text><xsl:value-of select="@name"/><xsl:text>"&gt;</xsl:text>
      <xsl:for-each select="property[@type!='Collection']">
        <xsl:variable name="lastCategory" select="preceding-sibling::property[1]/@category" />
        <xsl:if test="$lastCategory != @category or position() = 1">
          <br/>
          <xsl:text>&#160;&#160;&#160;&#160;&#160;&#160;&lt;!-- </xsl:text><xsl:value-of select="@category" /><xsl:text> --&gt;</xsl:text><br/>
        </xsl:if>
        <span>
          <xsl:if test="@required='1'">
            <xsl:attribute name="class">requiredparameter</xsl:attribute>
          </xsl:if>
          <xsl:text>&#160;&#160;&#160;&#160;&#160;&#160;&lt;</xsl:text><xsl:call-template name="property-link" />
          <xsl:if test="@type='Layout'">
            <xsl:text> xsi:type="layoutType"</xsl:text>
          </xsl:if>
          <xsl:text>&gt;</xsl:text><span class="typeplaceholder">
            <xsl:value-of select="@type"/>
          </span><xsl:text>&lt;/</xsl:text><xsl:call-template name="property-link" /><xsl:text>&gt;</xsl:text>
          <br/>
        </span>
      </xsl:for-each>
      
      <xsl:for-each select="property[@type='Collection']">
        <xsl:text>&#160;&#160;&#160;&#160;&#160;&#160;&lt;</xsl:text><xsl:value-of select="elementType/@elementTag"/>
        <xsl:for-each select="elementType/property[not(@type='Collection')]">
          <xsl:text>&#160;</xsl:text>
          <xsl:call-template name="property-link" />="<span class="typeplaceholder">
            <xsl:value-of select="@type"/>
          </span><xsl:text>"</xsl:text>
        </xsl:for-each>
        <xsl:text>/&gt; </xsl:text><span class="comment"><xsl:text>&lt;!-- repeated --&gt;</xsl:text>
        </span>
        <br/>
      </xsl:for-each>

      <br/>
      <xsl:text>&#160;&#160;&#160;&#160;&lt;/layout&gt;</xsl:text><br/>
      <xsl:text>&#160;&#160;&lt;/target&gt;</xsl:text><br/>
      <xsl:text>&lt;/targets&gt;</xsl:text></div>
  </xsl:template>

  <xsl:template name="property-link">
    <a href="#{../@name}_{@camelName}">
      <xsl:attribute name="title">
        <xsl:value-of select="doc/summary" />
        <xsl:if test="@defaultValue">
          <xsl:text> Default value is </xsl:text>
          <xsl:value-of select="@defaultValue"/>
          <xsl:text>.</xsl:text>
        </xsl:if>
      </xsl:attribute>
      <xsl:value-of select="@camelName" />
    </a>
  </xsl:template>

  <xsl:template match="property">
    <li class="config-property">
      <xsl:apply-templates select="supported-in" mode="float" />
      <a name="{../@name}_{@camelName}">
        <span class="propertyName">
          <xsl:value-of select="@camelName"/>
        </span>
      </a>
      <xsl:text> - </xsl:text><span class="summaryText"><xsl:apply-templates select="doc/summary" />
        </span>
      <xsl:if test="not(@type='Enum') and not (@type='String')">
        <span class="typeName">
          <a href="Data_types#{@type}">
            <xsl:value-of select="@type"/>
          </a>
        </span>
      </xsl:if>

      <xsl:if test="@required='1'">
        <xsl:text> Required.</xsl:text>
      </xsl:if>

      <xsl:if test="@defaultValue">
        <xsl:text> Default: </xsl:text><span class="defaultValue"><xsl:value-of select="@defaultValue"/></span>
      </xsl:if>

      <xsl:if test="@type='Collection'">
        <br/>
        <xsl:text> Each collection item is represented by </xsl:text><code>
          <xsl:text>&lt;</xsl:text><xsl:value-of select="elementType/@elementTag"/><xsl:text> /&gt;</xsl:text>
        </code><xsl:text> element with the following attributes:</xsl:text>
        <ul class="parameters">
          <xsl:apply-templates select="elementType/property" />
        </ul>
      </xsl:if>

      <xsl:if test="@type='Enum'">
        <br/>
        <xsl:text>Possible values:</xsl:text>
        <ul class="enum-options">
          <xsl:apply-templates select="enum">
            <xsl:sort select="@name"/>
          </xsl:apply-templates>
        </ul>
      </xsl:if>

      <xsl:if test="doc/remarks">
        <div class="remarks">
          <xsl:apply-templates select="doc/remarks" />
        </div>
      </xsl:if>

      <xsl:if test="doc/example">
        <br/>
        <xsl:apply-templates select="doc/example" />
      </xsl:if>

      <xsl:call-template name="showFrameworkDifferences">
         <xsl:with-param name="parent" select="../supported-in" />
         <xsl:with-param name="this" select="supported-in" />
      </xsl:call-template>

    </li>
  </xsl:template>

  <xsl:template name="showFrameworkDifferences">
    <xsl:param name="parent" />
    <xsl:param name="this" />

    <xsl:variable name="parentSupported">
      <xsl:for-each select="$parent/release"> 
        <xsl:value-of select="@name" /><xsl:text>/</xsl:text><xsl:value-of select="@framework" />
       </xsl:for-each>
    </xsl:variable>

    <xsl:variable name="thisSupported">
      <xsl:for-each select="$this/release"> 
        <xsl:value-of select="@name" /><xsl:text>/</xsl:text><xsl:value-of select="@framework" />
       </xsl:for-each>
    </xsl:variable>
    
    <xsl:if test="$thisSupported != $parentSupported">
        <div class='notsupportedin'>
        <h5>This parameter is not supported in:</h5>
        <ul>
        <xsl:for-each select="$parent/release">
            <xsl:variable name="rname" select="@name" />
            <xsl:variable name="rframework" select="@framework" />

            <xsl:if test="count($this/release[@name=$rname and @framework=$rframework])=0">
                <li>NLog v<xsl:value-of select="$rname" /> for <xsl:value-of select="@framework" /></li>
            </xsl:if>
        </xsl:for-each>
       </ul>
       </div>
    </xsl:if>
  </xsl:template>

  <xsl:template match="enum">
    <li>
      <b><xsl:value-of select="@name" /></b> - <xsl:apply-templates select="doc/summary" />
      <xsl:call-template name="showFrameworkDifferences">
         <xsl:with-param name="parent" select="../supported-in" />
         <xsl:with-param name="this" select="supported-in" />
      </xsl:call-template>
    </li>
  </xsl:template>

  <xsl:template match="ul">
    <xsl:copy-of select="." />
  </xsl:template>

  <xsl:template match="a[starts-with(@href, 'http')]">
     <xsl:copy-of select="." />
  </xsl:template>

  <xsl:template match="c">
     <xsl:copy><xsl:apply-templates /></xsl:copy>
  </xsl:template>

  <xsl:template match="pre">
    <pre>
      <xsl:value-of select="." disable-output-escaping="yes"/>
    </pre>
  </xsl:template>

  <xsl:template match="see">
    <xsl:call-template name="simple-type-name">
      <xsl:with-param name="fullName" select="@cref" />
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="simple-type-name">
    <xsl:param name="fullName" />
    <xsl:variable name="tail" select="substring-after($fullName, '.')" />
    <xsl:choose>
      <xsl:when test="$tail != ''">
        <xsl:call-template name="simple-type-name">
          <xsl:with-param name="fullName" select="$tail" />
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$fullName" />
      </xsl:otherwise>
    </xsl:choose>
    
  </xsl:template>

  <xsl:template match="table">
    <table>
      <xsl:apply-templates />
    </table>
  </xsl:template>

  <xsl:template match="ul">
    <ul>
      <xsl:apply-templates />
    </ul>
  </xsl:template>

  <xsl:template match="ol">
    <ol>
      <xsl:apply-templates />
    </ol>
  </xsl:template>

  <xsl:template match="li">
    <li>
      <xsl:apply-templates />
    </li>
  </xsl:template>

  <xsl:template match="b">
    <b>
      <xsl:apply-templates />
    </b>
  </xsl:template>

  <xsl:template match="tr">
    <tr>
      <xsl:apply-templates />
    </tr>
  </xsl:template>

  <xsl:template match="td">
    <td>
      <xsl:apply-templates />
    </td>
  </xsl:template>

  <xsl:template match="th">
    <th>
      <xsl:apply-templates />
    </th>
  </xsl:template>

  <xsl:template match="supported-in" mode="float">
    <xsl:variable name="prefix">
      <xsl:choose>
        <xsl:when test="$mode='web'">/</xsl:when>
        <xsl:otherwise></xsl:otherwise>
      </xsl:choose>

    </xsl:variable>
    <div style="float: right">
      <xsl:text>&#32;</xsl:text>
      <xsl:if test="not(release[@name='1.0']) and release[@name='2.0']">
        <img src="{$prefix}NewInNLog2.png" title="New in NLog 2.0" class="supported-icon"/>
      </xsl:if>
    </div>
  </xsl:template>
  
  <xsl:template match="supported-in">
    <xsl:variable name="prefix">
      <xsl:choose>
        <xsl:when test="$mode='web'">/</xsl:when>
        <xsl:otherwise></xsl:otherwise>
      </xsl:choose>
      
    </xsl:variable>
    <xsl:if test="release[starts-with(@framework, '.NET Framework')]">
      <img src="{$prefix}NetFxSupport.png" class="supported-icon">
        <xsl:attribute name="title">
          <xsl:text>Supported in .NET Framework: </xsl:text>
          <xsl:if test="release[@framework='.NET Framework 1.0']">1.0; </xsl:if>
          <xsl:if test="release[@framework='.NET Framework 1.1']">1.1; </xsl:if>
          <xsl:if test="release[@framework='.NET Framework 2.0']">2.0; </xsl:if>
          <xsl:if test="release[@framework='.NET Framework 3.5']">3.5; </xsl:if>
          <xsl:if test="release[@framework='.NET Framework 4.0']">4.0</xsl:if>
        </xsl:attribute>
      </img>
    </xsl:if>
    <xsl:if test="release[starts-with(@framework, 'Silverlight')]">
      <img src="{$prefix}SilverlightSupport.png" class="supported-icon">
        <xsl:attribute name="title">
          <xsl:text>Supported in Silverlight: </xsl:text>
          <xsl:if test="release[@framework='Silverlight 2.0']">2.0; </xsl:if>
          <xsl:if test="release[@framework='Silverlight 3.0']">3.0; </xsl:if>
          <xsl:if test="release[@framework='Silverlight 4.0']">4.0; </xsl:if>
          <xsl:if test="release[@framework='Silverlight for Windows Phone 7']">WP 7; </xsl:if>
          <xsl:if test="release[@framework='Silverlight for Windows Phone 7.1']">WP 7.1; </xsl:if>
        </xsl:attribute>
      </img>
    </xsl:if>
    <xsl:if test="release[starts-with(@framework, '.NET Compact Framework')]">
      <img src="{$prefix}NetCfSupport.png" class="supported-icon">
        <xsl:attribute name="title">
          <xsl:text>Supported in .NET Compact Framework: </xsl:text>
          <xsl:if test="release[@framework='.NET Compact Framework 1.0']">1.0; </xsl:if>
          <xsl:if test="release[@framework='.NET Compact Framework 2.0']">2.0; </xsl:if>
          <xsl:if test="release[@framework='.NET Compact Framework 3.5']">3.5; </xsl:if>
        </xsl:attribute>
      </img>
    </xsl:if>
    <xsl:if test="release[starts-with(@framework, 'Mono ')]">
      <img src="{$prefix}MonoSupport.png" class="supported-icon">
        <xsl:attribute name="title">
          <xsl:text>Supported in Mono</xsl:text>
        </xsl:attribute>
      </img>
    </xsl:if>

    <xsl:if test="not(release[@name='1.0']) and release[@name='2.0']">
      <img src="{$prefix}NewInNLog2.png" title="New in NLog 2.0" class="supported-icon"/>
    </xsl:if>
  </xsl:template>

  <xsl:template match="type" mode="typeLink">
    <xsl:choose>
      <xsl:when test="$mode = 'web'">
        <xsl:value-of select="@slug"/>
      </xsl:when>
      <xsl:otherwise>
        #<xsl:value-of select="@slug" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="type" mode="listLink">
    <xsl:choose>
      <xsl:when test="$mode = 'web'">
        <xsl:text>/</xsl:text><xsl:value-of select="@kind"/><xsl:text>s</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        #<xsl:value-of select="@kind" />-list
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  
</xsl:stylesheet>
