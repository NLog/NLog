<?xml version="1.0" encoding="windows-1250" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0" xmlns="http://www.w3.org/1999/xhtml">
  <xsl:param name="kind">target</xsl:param>
  <xsl:param name="kindName">Target</xsl:param>
  <xsl:param name="name"></xsl:param>
  <xsl:param name="mode"></xsl:param>
  <xsl:param name="slug"></xsl:param>

  <xsl:output omit-xml-declaration="yes" method="xml"
              doctype-public="-//W3C//DTD XHTML 1.0 Transitional//EN"
              doctype-system="http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd" />

  <xsl:template match="/">
    <html>
      <head>
        <link type="text/css" rel="stylesheet" href="style.css" />
       </head>
       <body>
         <xsl:apply-templates select="/types/type" />
       </body>
    </html>
  </xsl:template>

  <xsl:template match="type">
    <div class="config-element">
      <hr />
      <h4><xsl:value-of select="@title" /></h4>
      <p class="summary">
        <xsl:apply-templates select="doc/summary" />
      </p>
      <h4>Configuration File Usage</h4>
      <xsl:apply-templates select="." mode="usage-example" />
      <h4>Parameters</h4>
      <ul class="config-properties">
        <xsl:call-template name="property-grouping">
          <xsl:with-param name="list" select="property" />
        </xsl:call-template>
      </ul>
      <xsl:if test="doc/remarks">
        <h4>Remarks</h4>
        <p class="remarks">
          <xsl:apply-templates select="doc/remarks" />
        </p>
      </xsl:if>
    </div>
  </xsl:template>

  <xsl:template name="property-grouping">
    <xsl:param name="list" />
    <xsl:variable name="group-identifier" select="$list[1]/@category"/>
    <xsl:variable name="group" select="$list[@category = $group-identifier]"/>

    <li class="property-group">
      <xsl:value-of select="$group-identifier" />
    </li>
      <xsl:apply-templates select="$group">
      </xsl:apply-templates>

    <xsl:if test="count($list) > count($group)">
      <xsl:call-template name="property-grouping">
        <xsl:with-param name="list" select="$list[not(@category=$group-identifier)]"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <xsl:template match="type[@kind='layout-renderer']" mode="usage-example">
    <p>
      You can use this layout renderer by embedding the following <b>${<xsl:value-of select="@name" />}</b> tag wherever
      layouts are allowed. (Note that line breaks have been added for readability here, you can omit them in your declaration.
      Required parameters are marked with <span class="requiredparameter">bold</span>, other parameters are optional:
      )
    </p>
    <div class="usage-example">
      ${<xsl:value-of select="@name"/>
      <xsl:for-each select="property">
        <br/>&#160;&#160;&#160;&#160;<span>
        <xsl:if test="@required='1'">
          <xsl:attribute name="class">requiredparameter</xsl:attribute>
        </xsl:if>
         :<xsl:call-template name="property-link" />=<span class="typeplaceholder"><xsl:value-of select="@type"/></span>
      </span>
      </xsl:for-each>}
    </div>
    <p></p>
  </xsl:template>
  
  <xsl:template match="type[@kind='target']" mode="usage-example">
    <p>
      When setting up this target in the configuration file, use the following syntax (required parameters are marked with <span class="requiredparameter">bold</span>, other parameters are optional):
    </p>
    <div class="usage-example">
      &lt;targets&gt;<br/>
        &#160;&#160;&lt;target <span class="requiredparameter">
        xsi:type="<xsl:value-of select="@name"/>"
      </span>
      <xsl:for-each select="property[not(@type='Collection') and not(@type='Target')]">
        <span>
          <xsl:if test="@required='1'">
            <xsl:attribute name="class">requiredparameter</xsl:attribute>
          </xsl:if>
          <xsl:call-template name="property-link" />="<span class="typeplaceholder">
            <xsl:value-of select="@type"/>
          </span>"
        </span>
      </xsl:for-each>

      <xsl:if test="property[@type='Collection' or @type='Target'] or @iswrapper or @iscompound">
        &gt;<br/>
      </xsl:if>

      <xsl:if test="@iswrapper">
        &#160;&#160;&#160;&#160;<span class="requiredparameter">&lt;target xsi:type="wrappedTargetType" ... /&gt;</span><br/>
      </xsl:if>

      <xsl:if test="@iscompound">
        &#160;&#160;&#160;&#160;<span class="requiredparameter">&lt;target xsi:type="wrappedTargetType1" ... /&gt;</span><br/>
        &#160;&#160;&#160;&#160;<span class="requiredparameter">&lt;target xsi:type="wrappedTargetType2" ... /&gt;</span><br/>
        &#160;&#160;&#160;&#160;<span class="requiredparameter">&lt;target xsi:type="wrappedTargetTypeN" ... /&gt;</span><br/>
      </xsl:if>

      <xsl:for-each select="property[@type='Collection']">
        &#160;&#160;&#160;&#160;&lt;<xsl:value-of select="elementType/@elementTag"/>&#160;
        <xsl:for-each select="elementType/property[not(@type='Collection')]">
          sss

          <xsl:call-template name="property-link" />="<span class="typeplaceholder">
            <xsl:value-of select="@type"/>
          </span>"
        </xsl:for-each>
        /&gt;<br/>
      </xsl:for-each>

      <xsl:choose>
        <xsl:when test="property[@type='Collection' or @type='Target'] or @iswrapper or @iscompound">
          &#160;&#160;&lt;/target&gt;
        </xsl:when>
        <xsl:otherwise> /&gt;</xsl:otherwise>
      </xsl:choose>
      <br/>&lt;/targets&gt;
    </div>
    <p>
      Parameters can also be specified as elements instead of attributes which can be more readable:
    </p>
    <div class="usage-example">
      &lt;targets&gt;<br/>
      &#160;&#160;&lt;target <span class="requiredparameter">
        xsi:type="<xsl:value-of select="@name"/>"
      </span> ...&gt;<br/>

      <xsl:if test="@iswrapper">
        &#160;&#160;&#160;&#160;<span class="requiredparameter">&lt;target xsi:type="wrappedTargetType" ... /&gt;</span><br/>
      </xsl:if>

      <xsl:if test="@iscompound">
        &#160;&#160;&#160;&#160;<span class="requiredparameter">&lt;target xsi:type="wrappedTargetType1" ... /&gt;</span><br/>
        &#160;&#160;&#160;&#160;<span class="requiredparameter">&lt;target xsi:type="wrappedTargetType2" ... /&gt;</span><br/>
        &#160;&#160;&#160;&#160;<span class="requiredparameter">&lt;target xsi:type="wrappedTargetTypeN" ... /&gt;</span><br/>
      </xsl:if>

      <xsl:for-each select="property[@type != 'Collection']">
        <xsl:variable name="lastCategory" select="preceding-sibling::property[1]/@category" />
        <xsl:if test="$lastCategory != @category or position() = 1">
          <br/>
          &#160;&#160;&#160;&#160;&lt;!-- <xsl:value-of select="@category" /> --&gt;<br/>
        </xsl:if>
        <span>
          <xsl:if test="@required='1'">
            <xsl:attribute name="class">requiredparameter</xsl:attribute>
          </xsl:if>
          &#160;&#160;&#160;&#160;&lt;<xsl:call-template name="property-link" />
          <xsl:if test="@type='Layout'">
            xsi:type="layoutType"
          </xsl:if>
          <xsl:if test="@type='Target'">
            xsi:type="wrappedTargetType"
          </xsl:if>&gt;<span class="typeplaceholder">
            <xsl:value-of select="@type"/>
          </span>&lt;/<xsl:call-template name="property-link" />&gt;<br/>
        </span>
      </xsl:for-each>

      <xsl:for-each select="property[@type='Collection']">
        &#160;&#160;&#160;&#160;&lt;<xsl:value-of select="elementType/@elementTag"/>&#160;
        <xsl:for-each select="elementType/property[not(@type='Collection')]">
          <xsl:call-template name="property-link" />="<span class="typeplaceholder">
            <xsl:value-of select="@type"/>
          </span>"
        </xsl:for-each>
        /&gt;<br/>
      </xsl:for-each>

      &#160;&#160;&lt;/target&gt;
      <br/>&lt;/targets&gt;
    </div>
  </xsl:template>

  <xsl:template name="property-link">
    <a href="#{../@name}_{@camelName}">
      <xsl:attribute name="title">
        <xsl:value-of select="doc/summary" />
        <xsl:if test="@defaultValue">
          Default value is <xsl:value-of select="@defaultValue"/>.
        </xsl:if>
      </xsl:attribute>
      <xsl:value-of select="@camelName" />
    </a>
  </xsl:template>

  <xsl:template match="property">
    <li class="config-property">
      <a name="{../@name}_{@camelName}">
        <span class="propertyName">
          <xsl:value-of select="@camelName"/>
        </span>
      </a>
        - <span class="summaryText"><xsl:apply-templates select="doc/summary" />
        </span>
      <xsl:if test="not(@type='Enum') and not (@type='String')">
        <span class="typeName">
          <xsl:value-of select="@type"/>
        </span>
      </xsl:if>

      <xsl:if test="@required='1'">
        This parameter is required.
      </xsl:if>

      <xsl:if test="@defaultValue">
        Default value is <span class="defaultValue"><xsl:value-of select="@defaultValue"/></span>.
      </xsl:if>

      <xsl:if test="@type='Collection'">
        Each collection item is represented by <code>
          &lt;<xsl:value-of select="elementType/@elementTag"/> /&gt;
        </code> element with the following parameters.
        <ul class="parameters">
          <xsl:apply-templates select="elementType/property" />
        </ul>
      </xsl:if>

      <xsl:if test="@type='Enum'">
        Possible values for this options are:
        <ul class="enum-options">
          <xsl:apply-templates select="enum">
            <xsl:sort select="@name"/>
          </xsl:apply-templates>
        </ul>
      </xsl:if>
      
      <div class="summary">
        <xsl:if test="doc/remarks">
          <br/>
          <xsl:apply-templates select="doc/remarks" />
        </xsl:if>
      </div>

      <xsl:if test="doc/example">
        <br/>
        <xsl:apply-templates select="doc/example" />
      </xsl:if>
    </li>
  </xsl:template>

  <xsl:template match="enum">
    <li>
      <b><xsl:value-of select="@name" /></b> - <xsl:apply-templates select="doc/summary" />
    </li>
  </xsl:template>

  <xsl:template match="pre">
    <pre>
      <xsl:value-of select="." disable-output-escaping="yes"/>
    </pre>
  </xsl:template>

  <xsl:template match="see">
    <span class="typeName">
      <xsl:call-template name="simple-type-name">
         <xsl:with-param name="fullName" select="substring-after(@cref, ':')" />
      </xsl:call-template>
    </span>
  </xsl:template>

  <xsl:template name="simple-type-name">
    <xsl:param name="fullName" /> 
    <xsl:choose>
      <xsl:when test="contains($fullName, '.')">
        <xsl:call-template name="simple-type-name">
          <xsl:with-param name="fullName" select="substring-after($fullName, '.')" />
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$fullName" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
</xsl:stylesheet>
