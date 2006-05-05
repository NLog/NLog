<?xml version="1.0" encoding="windows-1250" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
    <xsl:include href="style.xsl" />

    <xsl:param name="lr_name" />

    <xsl:template match="*" mode="content">
        <xsl:if test="not($lr_name)">
            <h1>Layout Renderers</h1>
            <p>
                The following layout renderers are available. Click on a name for full reference.
            </p>
            <div class="noborder" style="width: 600px">
                <table class="listtable">
                    <xsl:call-template name="supportmatrixheader" />
                    <xsl:apply-templates select="//class[attribute/@id='T:NLog.LayoutRendererAttribute']" mode="list">
                        <xsl:sort select="../../@name" />
                        <xsl:sort select="attribute[@id='T:NLog.LayoutRendererAttribute']/argument[position()=1]/@value" />
                    </xsl:apply-templates>
                </table>
            </div>
        </xsl:if>
        <xsl:if test="$lr_name">
            <xsl:apply-templates select="//class[attribute/@id='T:NLog.LayoutRendererAttribute' and attribute/argument[position()=1]/@value=$lr_name]" mode="details">
                <xsl:sort select="../../@name" />
                <xsl:sort select="attribute[@id='T:NLog.LayoutRendererAttribute']/argument[position()=1]/@value" />
            </xsl:apply-templates>
        </xsl:if>
    </xsl:template>

    <xsl:template match="class" mode="list">
        <xsl:variable name="type_tag" select="attribute[@id='T:NLog.LayoutRendererAttribute']/argument[position()=1]/@value" />
        <tr>
            <td class="name"><a href="lr.{$type_tag}.html">${<xsl:value-of select="$type_tag" />}</a></td>
            <td class="description"><xsl:apply-templates select="documentation/summary" /></td>
            <xsl:call-template name="supportmatrixvalues" />
        </tr>
    </xsl:template>

    <xsl:template match="@* | node()">
        <xsl:copy>
            <xsl:apply-templates select="@* | node()" />
        </xsl:copy>
    </xsl:template>

    <xsl:template match="summary">
        <xsl:apply-templates />
    </xsl:template>

    <xsl:template match="class" mode="details">
        <xsl:variable name="type_tag" select="attribute[@id='T:NLog.LayoutRendererAttribute']/argument[position()=1]/@value" />
        <h3>${<xsl:value-of select="$type_tag" />} Layout Renderer</h3>
        <hr size="1" />
        <table class="definedin" cellspacing="0">
            <tr><td>Class name:</td><td><xsl:value-of select="substring-after(@id,'T:')" /></td></tr>
        </table>
        <p><xsl:apply-templates select="documentation/summary" /></p>
        <xsl:if test="property[not(@declaringType='T:NLog.LayoutRenderer')] or attribute[@id='T:NLog.LayoutRendererAttribute']/property[@name='IgnoresPadding']/@value">
            <h4>Parameters:</h4>
            <table class="paramtable">
                <tr>
                    <th>Name</th>
                    <th>Type</th>
                    <th>Description</th>
                </tr>
                <tr>
                    <td colspan="3" class="subheader">
                        Parameters specific to this layout renderer
                    </td>
                </tr>
                <xsl:apply-templates select="property[not(@declaringType='T:NLog.LayoutRenderer')]" mode="parameter">
                    <xsl:sort select="count(attribute[@id='T:NLog.Config.RequiredParameterAttribute'])" order="descending" />
                    <xsl:sort select="@name" />
                </xsl:apply-templates>
                <xsl:if test="not(attribute[@id='T:NLog.LayoutRendererAttribute']/property[@name='IgnoresPadding']/@value='True')">
                    <tr>
                        <td colspan="3" class="subheader">
                            Parameters common to all layout renderers
                        </td>
                    </tr>
                    <xsl:apply-templates select="property[@declaringType='T:NLog.LayoutRenderer']" mode="parameter">
                        <xsl:sort select="count(attribute[@id='T:NLog.Config.RequiredParameterAttribute'])" order="descending" />
                        <xsl:sort select="@name" />
                    </xsl:apply-templates>
                </xsl:if>
            </table>
        </xsl:if>
        <xsl:if test="documentation/example">
            <h4>Example:</h4>
            <xsl:apply-templates select="documentation/example" />
        </xsl:if>
        <xsl:if test="documentation/remarks">
            <h4>Remarks:</h4>
            <xsl:apply-templates select="documentation/remarks" />
        </xsl:if>
        <hr size="1" />
        <a href="layoutrenderers.html">Back to the layout renderer list.</a>
    </xsl:template>

    <xsl:template match="property[@set='false']" mode="parameter">
        <!-- ignore -->
    </xsl:template>

    <xsl:template match="property[@name='Name']" mode="parameter">
        <!-- ignore -->
    </xsl:template>

    <xsl:template match="property[@name='Type']" mode="parameter">
        <!-- ignore -->
    </xsl:template>

    <xsl:template match="property[@type='NLog.Layout']" mode="parameter">
        <!-- ignore -->
    </xsl:template>

    <xsl:template match="property" mode="parameter">
        <xsl:call-template name="parameter_info" />
    </xsl:template>

    <xsl:template match="property[attribute/@id='T:NLog.Config.ArrayParameterAttribute']" mode="parameter2">
        <xsl:variable name="itemtype" select="attribute[@id='T:NLog.Config.ArrayParameterAttribute']/property[@name='ElementType']/@value" />
        <br/>&lt;<xsl:value-of select="@name" />&gt;<br/>
        <xsl:value-of select="$itemtype" />
        <xsl:apply-templates select="//class[@name='$itemtype']" mode="parameter" />
        &lt;/<xsl:value-of select="@name" />&gt;
    </xsl:template>

    <xsl:template match="property" mode="parameter2">
    </xsl:template>

    <xsl:template match="c">
        <code><xsl:apply-templates /></code>
    </xsl:template>

    <xsl:template match="code">
        <code><pre class="example"><xsl:apply-templates /></pre></code>
    </xsl:template>

    <xsl:template match="see">
        <code><xsl:value-of select="@cref" /></code>
    </xsl:template>
</xsl:stylesheet>
