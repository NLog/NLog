<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns="http://www.w3.org/1999/xhtml" version="1.0">

<xsl:import href="common.xsl" />

<xsl:template match="/">

<html xmlns="http://www.w3.org/1999/xhtml">
    <xsl:call-template name="page-head" />
    <body onload="paintColors();">
        <h3>Appenders</h3>
        <h4>Available appenders</h4>
        <p>The following appenders are available. Items marked with color may not be supported on all platforms. See particular layout appender documentation for more information. Click on a link to see appender usage and configuration details.</p>

        <div class="table">
        <table>
            <col width="20%" />
            <col width="20%" />
            <col width="60%" />
            <tr>
                <th>Appender</th>
                <th>Assembly</th>
                <th>Description</th>
                <th>Configurable</th>
            </tr>
            <xsl:for-each select="/appenders/appender">
                <tr>
                    <xsl:if test="count(support) != 6">
                        <xsl:attribute name="class">notall</xsl:attribute>
                    </xsl:if>
                    <td><a>
                            <xsl:attribute name="href">#<xsl:value-of select="@name" />Appender</xsl:attribute>
                            <xsl:value-of select="displayName" />
                    </a></td>
                    <td><xsl:value-of select="assembly" /></td>
                    <td><xsl:value-of select="description" /></td>
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
        <h4>Loading additional appenders</h4>
        <p>If you want to use appenders not found in <code>NLog.dll</code> (such as <code>ASPNetTraceAppender</code>), 
            you need to load them by using the <code>&lt;extensions&gt;</code> element in 
            your <a href="configfile.html">config file</a>.</p>
        <p>For example:</p>
<xmp class="code-xml">
<nlog autoReload="true">
    <extensions>
        <add assemblyFile="NLog.ASPNet.dll" />
     </extensions>
        ...
</nlog></xmp>

        <h3>Common Appender Configuration</h3>
        <a name="common" />
        <p>The following configuration parameters may be used on with all appenders. Note that particular appenders
            may choose to interpret some parameters differently (for example by ignoring the <code>layout</code> parameter).
            This is clearly indicated in the appender documentation.</p>
 
        <div class="table">
        <table>
            <tr>
                <th>Parameter&#160;Name</th>
                <th>Type</th>
                <th>Required</th>
                <th>Description</th>
                <th>Default</th>
            </tr>
            <tr>
                <td><code>name</code></td>
                <td>String</td>
                <td>Yes</td>
                <td>The name of the appender. This should be a human-readable name. For example: <code>console</code>, <code>masterlogfile</code>.</td>
                <td></td>
            </tr>
            <tr>
                <td><code>type</code></td>
                <td>String</td>
                <td>Yes</td>
                <td>The type of the appender. This can be a simple type name (for appenders defined in <code>NLog.dll</code>) or a fully qualified type name.</td>
                <td></td>
            </tr>
            <tr>
                <td><code>layout</code></td>
                <td>Layout</td>
                <td>No</td>
                <td>The format of logged messages. See <a href="layoutappender.html">Layout Appenders</a> for more info.</td>
                <td><code>${longdate}|${level}|${message}</code></td>
            </tr>
        </table>
        </div>
        <xsl:for-each select="/appenders/appender">
            <hr size="1" />
            <h3><xsl:value-of select="displayName" /></h3>
            <h4>Summary</h4>
            <div class="table">
                <table>
                    <tr><td>Assembly Name:</td><td><xsl:value-of select="assembly" /></td></tr>
                    <tr><td>Class Name:</td><td><xsl:value-of select="namespace" />.<xsl:value-of select="className" /></td></tr>
                    <tr><td>Frameworks supported:</td><td>
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
                <div class="table">
                    <table>
                        <tr>
                            <th>Parameter&#160;Name</th>
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
            </xsl:if>
            <xsl:if test="example">
                <h4>Example</h4>
            </xsl:if>
        </xsl:for-each>
    </body>
</html>

</xsl:template>

</xsl:stylesheet>
