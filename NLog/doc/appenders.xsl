<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns="http://www.w3.org/1999/xhtml" version="1.0">

<xsl:import href="common.xsl" />

<xsl:template match="/">

<html xmlns="http://www.w3.org/1999/xhtml">
    <xsl:call-template name="page-head" />
    <body onload="paintColors();">
        <h3>Appenders</h3>
        <h4>Available appenders</h4>
        <p>The following appenders are available. Click on a link to see appender usage and configuration details.</p>

        <div class="table">
        <table>
            <col width="20%" />
            <col width="20%" />
            <col width="60%" />
            <tr>
                <th>Appender</th>
                <th>Assembly</th>
                <th>Description</th>
            </tr>
            <xsl:for-each select="/appenders/appender">
            <tr>
                <td><a>
                        <xsl:attribute name="href">#<xsl:value-of select="@name" />Appender</xsl:attribute>
                        <xsl:value-of select="displayName" />
                    </a></td>
                    <td><xsl:value-of select="assembly" /></td>
                    <td><xsl:value-of select="description" /></td>
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

        <h3>Configuration</h3>
        <a name="#common" />
 
        <div class="table">
        <table>
            <tr>
                <th>Parameter&#160;Name</th>
                <th>Type</th>
                <th>Required</th>
                <th>Description</th>
            </tr>
            <tr>
                <td>Name</td>
                <td>String</td>
                <td>Yes</td>
                <td>The name of the appender. This should be a human-readable name. For example: <code>console</code>, <code>masterlogfile</code>.</td>
            </tr>
            <tr>
                <td>Type</td>
                <td>String</td>
                <td>Yes</td>
                <td>The type of the appender. This can be a simple type name (for appenders defined in <code>NLog.dll</code>) or a fully qualified type name.</td>
            </tr>
            <tr>
                <td>Layout</td>
                <td>Layout</td>
                <td>No</td>
                <td>The format of logged messages. See <a href="layoutappender.html">Layout Appenders</a> for more info.</td>
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
        <!--
        <a name="#common" />
        <h3>Common Appender Configuration</h3>
        <p>
        All appenders accept the following parameters:
        </p>
        <div class="table">
        <table>
            <tr>
                <th>Parameter&#160;Name</th>
                <th>Required</th>
                <th>Description</th>
            </tr>
            <tr>
                <td>Name</td>
                <td>Yes</td>
                <td>The name of the appender. This should be a human-readable name. For example: <code>console</code>, <code>masterlogfile</code>.</td>
            </tr>
            <tr>
                <td>Type</td>
                <td>Yes</td>
                <td>The type of the appender. This can be a simple type name (for appenders defined in <code>NLog.dll</code>) or a fully qualified type name.</td>
            </tr>
            <tr>
                <td>Layout</td>
                <td>No</td>
                <td>The format of logged messages. See <a href="layoutappender.html">Layout Appenders</a> for more info.</td>
            </tr>
        </table>
        </div>
        <h3>Console Appender</h3>
        <h4>Summary</h4>
        <p>
        The Console Appender is very simple. It logs all output to the console.
        </p>
        <h4>Configuration</h4>
        Console Appender accepts no additional parameters beside the <a href="#common">common</a> ones.
        <h4>Example</h4>
        The following example outputs all messages to the console using a simple 4-column format:
        </p>

<xmp class="code-xml">
<nlog>
   <appenders>
       <appender name="console" type="Console" layout="${longdate}|{$level}|${logger}|${message}" />
   </appenders>

   <rules>
       <logger name="*" appendTo="console" />
   </rules>
</nlog></xmp>

        <hr size="1" />

        <h3>File Appender</h3>
        <p>
        <h4>Summary</h4>
        This appender is quite powerful. It lets you write your output to one or more files 
        in an excusive or concurrent manner. 
        <h4>Configuration</h4>
        <p>
        File Appender accepts a the following parameters in addition to the <a href="#common">common parameters</a>.
        </p>
        <div class="table">
        <table>
            <tr>
                <th>Parameter&#160;Name</td>
                <th>Required</td>
                <th>Description</th>
            </tr>
            <tr>
                <td>Name</td>
                <td>Yes</td>
                <td>The name of the appender. This should be a human-readable name. For example: <code>console</code>, <code>masterlogfile</code>.</td>
            </tr>
            <tr>
                <td>Type</td>
                <td>Yes</td>
                <td>The type of the appender. This can be a simple type name (for appenders defined in <code>NLog.dll</code>) or a fully qualified type name.</td>
            </tr>
            <tr>
                <td>Layout</td>
                <td>No</td>
                <td>The format of logged messages. See <a href="layoutappender.html">Layout Appenders</a> for more info.</td>
            </tr>
        </table>
        </div>

        <h4>Example</h4>
        The following is an example of the configuration which causes 
        each log message to be written to a file named LOGGER_NAME.SHORT_DATE.log.
        </p>

<xmp class="code-xml">
<nlog>
   <appenders>
   <appender name="logfile" type="File" layout="${longdate}|{$level}|${logger}|${message}">
        <filename>c:\path_to\log\files\${logger}.${shortdate}.log</filename>
   </appenders>

   <rules>
        <logger name="*" appendTo="logfile" />
   </rules>
</nlog></xmp>
<hr size="1" />
-->
    </body>
</html>

</xsl:template>

</xsl:stylesheet>
