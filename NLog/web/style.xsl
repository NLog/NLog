<?xml version="1.0" encoding="windows-1250" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

    <xsl:variable name="result_lang" select="/*[position()=1]/@lang" />
    <xsl:variable name="common_file" select="concat('common.', $result_lang, '.xml')" />
    <xsl:variable name="page_id" select="/*[position()=1]/@id" />
    <xsl:variable name="subpage_id" select="/*[position()=1]/@subid" />
    <xsl:variable name="common" select="document($common_file)" />
    <xsl:param name="file_extension">xml</xsl:param>
    <xsl:param name="sourceforge">0</xsl:param>

    <xsl:output method="html" indent="no" />

    <xsl:template match="/">
        <html>
            <head>
                <link rel="stylesheet" href="style.css" type="text/css" />
                <meta name="generator" content="NAnt 0.85 style task" />
                <meta name="keywords" content="NLog logging tracing debugging library easy simple C# .NET log4net log4j Logger C/C++ COM" />
                <title>NLog - <xsl:value-of select="$common/common/navigation/nav[@href=$page_id]/@label" /></title>
            </head>
            <body width="100%">
                <div class="titleimage" style="overflow: hidden">
                    <img src="NLog.jpg" />
                </div>
                <h6>THIS SITE IS UNDER CONSTRUCTION. SOME SECTIONS ARE MISSING.</h6><br/>
                <table class="page" cellpadding="0" cellspacing="0">
                    <tr>
                        <td valign="top" class="controls">
                            <xsl:call-template name="controls" />
                        </td>
                        <td valign="top" align="left" class="content">
                            <xsl:apply-templates select="content" />
                        </td>
                    </tr>
                    <tr>
                        <td class="hostedby">
                            <xsl:if test="$sourceforge='1'">
<!-- Start of StatCounter Code -->
<script type="text/javascript" language="javascript">
var sc_project=575077; 
var sc_partition=4; 
var sc_security="6fe22c9a"; 
</script>

<script type="text/javascript" language="javascript" src="http://www.statcounter.com/counter/counter.js"></script><noscript><a href="http://www.statcounter.com/" target="_blank"><img  src="http://c5.statcounter.com/counter.php?sc_project=575077&amp;java=0&amp;security=6fe22c9a" alt="website tracking" border="0" /></a> </noscript>
<!-- End of StatCounter Code --><br/>
                                <a href="http://sourceforge.net"><img src="http://sourceforge.net/sflogo.php?group_id=116456&amp;type=1" width="88" height="31" border="0" alt="SourceForge.net Logo" /></a>
                            </xsl:if>
                        </td>
                        <td class="copyright">Copyright (c) 2003-2004 by Jarosław Kowalski</td>
                    </tr>
                </table>
            </body>
        </html>
    </xsl:template>

    <xsl:template match="@* | node()">
        <xsl:copy>
            <xsl:apply-templates select="@* | node()" />
        </xsl:copy>
    </xsl:template>

    <xsl:template match="content">
        <xsl:apply-templates select="*" />
    </xsl:template>

    <xsl:template name="controls">
        <xsl:apply-templates select="$common/common/navigation" />
    </xsl:template>

    <xsl:template match="navigation">
        <table border="0" cellpadding="0" cellspacing="0">
            <xsl:apply-templates select="nav" />
        </table>
    </xsl:template>
    
    <xsl:template match="nav">
        <xsl:choose>
            <xsl:when test="$page_id = @href"><tr><td class="nav_selected"><a class="nav_selected"><xsl:attribute name="href"><xsl:value-of select="@href" />.<xsl:value-of select="$file_extension" /></xsl:attribute><xsl:value-of select="@label" /></a><table class="submenu" width="100%"><xsl:apply-templates select="subnav" /></table></td></tr></xsl:when>
            <xsl:otherwise>
                <tr><td class="nav"><a class="nav"><xsl:attribute name="href"><xsl:value-of select="@href" />.<xsl:value-of select="$file_extension" /></xsl:attribute><xsl:value-of select="@label" /></a></td></tr>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>

    <xsl:template match="subnav">
        <xsl:choose>
            <xsl:when test="$subpage_id = @href"><tr><td><a class="subnav_selected"><xsl:value-of select="@label" /></a></td></tr></xsl:when>
            <xsl:otherwise>
                <tr><td><a class="subnav"><xsl:attribute name="href"><xsl:value-of select="@href" />.<xsl:value-of select="$file_extension" /></xsl:attribute><xsl:value-of select="@label" /></a></td></tr>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>

    <xsl:template match="cs">
        <pre class="csharp-example">
            <xsl:copy-of select="document(concat(@src,'.html'))" />
        </pre>
        <!-- <a href="{@src}">Download this sample</a><br/> -->
    </xsl:template>

    <xsl:template match="js">
        <pre class="jscript-example">
            <xsl:copy-of select="document(concat(@src,'.html'))" />
        </pre>
        <!-- <a href="{@src}">Download this sample</a><br/> -->
    </xsl:template>

    <xsl:template match="x">
        <xsl:apply-templates mode="xml-example" />
    </xsl:template>

    <xsl:template match="link">
        <a href="{@href}.{$file_extension}"><xsl:apply-templates /></a>
    </xsl:template>

    <xsl:template match="xml-example[@src]">
        <pre class="xml-example">
            <xsl:apply-templates mode="xml-example" select="document(@src)" />
        </pre>
        <!-- <a href="{@src}">Download this sample</a><br/> -->
    </xsl:template>

    <xsl:template match="xml-example">
        <pre class="xml-example">
            <xsl:apply-templates mode="xml-example" />
        </pre>
    </xsl:template>

    <xsl:template match="*" mode="xml-example">
        <xsl:choose>
            <xsl:when test="count(descendant::node()) = 0">
                <span class="xmlbracket">&lt;</span>
                <span class="xmlelement"><xsl:value-of select="name()" /></span>
                <xsl:apply-templates select="@*" mode="xml-example" />
                <span class="xmlbracket"> /&gt;</span>
            </xsl:when>
            <xsl:otherwise>
                <span class="xmlbracket">&lt;</span>
                <span class="xmlelement"><xsl:value-of select="name()" /></span>
                <xsl:apply-templates select="@*" mode="xml-example" />
                <span class="xmlbracket">&gt;</span>
                <xsl:apply-templates mode="xml-example" />
                <span class="xmlbracket">&lt;/</span>
                <span class="xmlelement"><xsl:value-of select="name()" /></span>
                <span class="xmlbracket">&gt;</span>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>

    <xsl:template match="@*[name()='xml:space']" mode="xml-example"></xsl:template>
    <xsl:template match="@*" mode="xml-example"><span class="xmlattribute">&#160;<xsl:value-of select="name()"/></span><span class="xmlpunct">=</span><span class="xmlattribtext">"<xsl:value-of select="." />"</span></xsl:template>

    <xsl:template match="comment()" mode="xml-example">
        <span class="xmlcomment">&lt;!--<xsl:value-of select="." />--&gt;</span>
    </xsl:template>
    <xsl:template match="node()" mode="xml-example" priority="-10">
        <xsl:copy>
            <xsl:apply-templates mode="xml-example" />
        </xsl:copy>
    </xsl:template>
    
    <xsl:template match="appender-list">
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
                <xsl:for-each select="/content/appenders/appender">
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
    </xsl:template>

    <xsl:template match="appenders">
        <xsl:apply-templates />
    </xsl:template>
    
    <xsl:template match="appender">
        <hr size="1" />
        <a>
            <xsl:attribute name="name"><xsl:value-of select="@name" />Appender</xsl:attribute>
        </a>
        <h3><xsl:value-of select="displayName" /></h3>
        <h4>Summary</h4>
        <div class="summarytable">
            <table>
                <tr><th>Usage:</th><td><code>&lt;appender name="..." type="<xsl:value-of select="@name" />" layout="..." 
                            <xsl:for-each select="parameter">
                                <xsl:value-of select="@name" />="..."
                            </xsl:for-each>
                            /&gt;</code></td></tr>
                <tr><th>Assembly Name:</th><td><xsl:value-of select="assembly" /></td></tr>
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
        <br/>
        <a href="#top">Back to top</a>
    </xsl:template>

    <xsl:template match="layoutappender-list">
    <div class="table">
        <table>
            <tr>
                <th>Token</th>
                <th>Name</th>
                <th>Description</th>
                <th>Parameters?</th>
            </tr>

            <xsl:for-each select="layoutappenders/la">
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
    </xsl:template>

    <xsl:template match="layoutappenders">
        <xsl:apply-templates select="la" />
    </xsl:template>
    
    <xsl:template match="la">
        <hr size="1" />
        <a><xsl:attribute name="name"><xsl:value-of select="@name" /></xsl:attribute></a>
        <h3><xsl:value-of select="displayName" /></h3>
        <h4>Summary</h4>
        <div class="summarytable">
            <table>
                <tr><th>Usage:</th><td><code>${<xsl:value-of select="@name" />}</code></td></tr>
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
