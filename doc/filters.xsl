<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns="http://www.w3.org/1999/xhtml" version="1.0">

    <xsl:import href="common.xsl" />

    <xsl:template match="/">

        <html xmlns="http://www.w3.org/1999/xhtml">
            <xsl:call-template name="page-head" />
            <body onload="paintColors();">
                <a name="top"></a>
                <h1>Filter Reference</h1>
                <h3>Introduction</h3>
                <p>
                    Filters are a fine-grained mechanism that can be used to enable or disable logging of particular
                    messages. For example you can add filter to eliminate some noisy log strings or to limit logging
                    to some particular user name, thread name or process name.
                </p>

                <h3>Available filters</h3>
                <p>
                    The following filters are available. Items marked with color may not be supported on all platforms. See particular filter documentation for more information.
                </p>
                <div class="table">
                    <table>
                        <tr>
                            <th>Name</th>
                            <th>Display Name</th>
                            <th>Description</th>
                            <th>Parameters?</th>
                        </tr>

                        <xsl:for-each select="filters/filter">
                            <tr>
                                <xsl:if test="count(support) != 6">
                                    <xsl:attribute name="class">notall</xsl:attribute>
                                </xsl:if>
                                <td><a><xsl:attribute name="href">#<xsl:value-of select="@name" /></xsl:attribute><xsl:value-of select="@name" /></a></td>
                                <td><xsl:value-of select="displayName" /></td>
                                <td><xsl:copy-of select="description" /></td>
                                <td>
                                    <xsl:choose>
                                        <xsl:when test="count(parameter) != 0">Yes</xsl:when>
                                        <xsl:otherwise>No</xsl:otherwise>
                                    </xsl:choose>
                                </td>
                            </tr>
                        </xsl:for-each>
                    </table>
                </div>
                <a name="common"></a>
                <h3>Configuring Filters</h3>
                <p>
                    Filters can take parameters which modify their behaviour. The following parameters
                    are supported by all filters, particular filters may support additional parameters - 
                    see individual filter information for configuration details.
                </p>

                <xsl:for-each select="/filters/common-config">
                    <xsl:call-template name="parameters-table" />
                </xsl:for-each>

                <h3>Passing parameters</h3>

                <p>
                    To pass parameters to a filter, use XML attributes where the filter name is the tag name. For example
                    this filter eliminates all messages containing the 'fault' substring.:
                </p>
                    
<xmp class="code-xml" xml:space="preserve">
    <filters>
        <whenContains layout="${message}" substring="fault" action="ignore" />
    </filters>
</xmp>
                
                <xsl:for-each select="/filters/filter">
                    <hr size="1" />
                    <a><xsl:attribute name="name"><xsl:value-of select="@name" /></xsl:attribute></a>
                    <h3><xsl:value-of select="displayName" /></h3>
                    <h4>Summary</h4>
                    <div class="summarytable">
                        <table>
                            <tr><th>Usage:</th><td><code>&lt;<xsl:value-of select="@name" /> ... /&gt;</code></td></tr>
                            <tr><th>Description:</th><td><xsl:value-of select="description" /></td></tr>
                            <tr><th>Defined in:</th><td><xsl:value-of select="assembly" /></td></tr>
                            <tr><th>Class Name:</th><td><xsl:value-of select="namespace" />.<xsl:value-of select="className" /></td></tr>
                            <tr><th>Frameworks supported:</th><td>
                                    <xsl:for-each select="support">
                                        <xsl:if test="position() != 1">, </xsl:if>
                                        <xsl:if test="@framework='net-1.0'">.NET 1.0</xsl:if>
                                        <xsl:if test="@framework='net-1.1'">.NET 1.1</xsl:if>
                                        <xsl:if test="@framework='net-2.0'">.NET 2.0</xsl:if>
                                        <xsl:if test="@framework='netcf-1.0'">.NET CF 1.0</xsl:if>
                                        <xsl:if test="@framework='mono-1.0'">Mono 1.0</xsl:if>
                                        <xsl:if test="@framework='mono-1.1'">Mono 1.1</xsl:if>
                                        <xsl:if test="@framework='mono-2.0'">Mono 2.0</xsl:if>
                                    </xsl:for-each>
                            </td></tr>
                        </table>
                    </div>
                    <h4>Configuration</h4>
                    <xsl:if test="not(parameter)">
                        <p>
                            This appender doesn't support any configuration parameters beside the <a href="#common">common</a> ones. 
                        </p>
                    </xsl:if>
                    <xsl:if test="parameter">
                        <p>
                            This appender supports the following configuration parameters in addition to the <a href="#common">common</a> ones. 
                        </p>
                        <xsl:call-template name="parameters-table" />
                    </xsl:if>
                    <xsl:if test="example">
                        <h4>Example</h4>
                    </xsl:if>
                    <br/>
                    <a href="#top">Back to top</a>
                </xsl:for-each>
            </body>
        </html>

    </xsl:template>

    <xsl:template name="parameters-table">
        <div class="table">
            <table>
                <tr>
                    <th>Parameter Name</th>
                    <th align="center">Type</th>
                    <th align="center">Required</th>
                    <th>Description</th>
                    <th align="center">Default</th>
                </tr>
                <xsl:for-each select="parameter">
                    <tr valign="top">
                        <td><code><xsl:value-of select="@name" /></code></td>
                        <td align="center"><xsl:value-of select="@type" /></td>
                        <td align="center"><xsl:value-of select="@required" /></td>
                        <td align="left"><xsl:value-of select="@description" /></td>
                        <td align="center"><xsl:value-of select="@default" /></td>
                    </tr>
                </xsl:for-each>
            </table>
        </div>
    </xsl:template>

</xsl:stylesheet>
