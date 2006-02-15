<?xml version="1.0" encoding="windows-1250" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

    <xsl:param name="page_id_override"></xsl:param>
    <xsl:param name="subpage_id_override"></xsl:param>
    <xsl:param name="file_extension">xml</xsl:param>
    <xsl:param name="sourceforge">0</xsl:param>
    <xsl:param name="log4net_comparison">0</xsl:param>
    <xsl:param name="build_time">2006-01-01</xsl:param>
    <xsl:param name="mode">web</xsl:param>

    <xsl:variable name="page_id" select="concat(/*[position()=1]/@id,$page_id_override)" />
    <xsl:variable name="subpage_id" select="concat(/*[position()=1]/@subid,$subpage_id_override)" />
    <xsl:variable name="common" select="document(concat($mode,'menu.xml'))" />
    
    <xsl:output method="html" indent="no" />

    <xsl:template match="/">
        <html>
            <head>
                <link rel="stylesheet" href="style.css" type="text/css" />
                <link rel="stylesheet" href="syntax.css" type="text/css" />
                <meta name="generator" content="NAnt 0.85 style task" />
                <meta name="keywords" content="NLog logging tracing debugging library easy simple C# .NET log4net log4j Logger C/C++ COM" />
                <title>NLog - <xsl:value-of select="$common/common/navigation/nav[@href=$page_id]/@label" /></title>
            </head>
            <body width="100%">
                <img src="title.png" style="display: none" /> <!-- need this for CHM -->
                <div class="titleimage" style="overflow: hidden">
                    <img src="NLog.jpg" />
                </div>
                <table class="page" cellpadding="0" cellspacing="0" style="table-layout: fixed">
                    <tr>
                        <td valign="top" class="controls" rowspan="2">
                            <xsl:call-template name="controls" />
                            <p/>
                            <xsl:if test="$sourceforge='1'">
<!-- Start of StatCounter Code -->
<script type="text/javascript" language="javascript">
var sc_project=575077; 
var sc_partition=4; 
var sc_security="6fe22c9a"; 
</script>

<script type="text/javascript" language="javascript" src="http://www.statcounter.com/counter/counter.js"></script><noscript><a href="http://www.statcounter.com/" target="_blank"><img  src="http://c5.statcounter.com/counter.php?sc_project=575077&amp;java=0&amp;security=6fe22c9a" alt="website tracking" border="0" /></a> </noscript>
<!-- End of StatCounter Code -->
                            </xsl:if>
                            <div class="lastupdated">Last updated: <xsl:value-of select="$build_time" /></div>
                        </td>
                        <td valign="top" align="left" class="content">
                            <!-- <p style="color: blue; font-weight: bold; padding: 4px; margin-bottom: 10px; border: 1px solid #ABC8E5; background-color: #DFEAF5;">THIS SITE IS UNDER CONSTRUCTION. SOME SECTIONS ARE MISSING.</p> -->
                            <xsl:if test="$mode = 'web'">
                                <span class="underconstruction">
                                    This web site is under construction and describes a version of NLog currently under development. Some sections may be missing or not up-to-date.
                                </span>
                            </xsl:if>
                            <xsl:apply-templates select="/" mode="content" />
                        </td>
                    </tr>
                    <tr>
                        <td class="copyright">Copyright &#169; 2004-2006 by Jarosław Kowalski.</td>
                    </tr>
                </table>
         <xsl:if test="$mode = 'web'">
                <div id="googlesearch">
                    <!-- SiteSearch Google -->
                    <form method="get" action="http://www.google.com/custom" target="_top">
                        <table border="0">
                            <tr><td nowrap="nowrap" valign="top" align="left" height="32">
<input type="hidden" name="domains" value="www.nlog-project.org"></input>
<input type="text" name="q" size="20" maxlength="255" value=""></input>
<input type="submit" name="sa" value="Google Search"></input>
</td></tr>
<tr>
<td nowrap="nowrap">
<table>
<tr>
<td>
<input type="radio" name="sitesearch" value=""></input>
<font size="-1" color="#000080">Web</font>
</td>
<td>
<input type="radio" name="sitesearch" value="www.nlog-project.org" checked="checked"></input>
<font size="-1" color="#000080">www.nlog-project.org</font>
</td>
</tr>
</table>
<input type="hidden" name="client" value="pub-2535373996863248"></input>
<input type="hidden" name="forid" value="1"></input>
<input type="hidden" name="ie" value="UTF-8"></input>
<input type="hidden" name="oe" value="UTF-8"></input>
<input type="hidden" name="cof" value="GALT:#0066CC;GL:1;DIV:#999999;VLC:336633;AH:center;BGC:FFFFFF;LBGC:FF9900;ALC:0066CC;LC:0066CC;T:000000;GFNT:666666;GIMP:666666;FORID:1;"></input>
<input type="hidden" name="hl" value="en"></input>
</td></tr></table>
</form>
<!-- SiteSearch Google -->
                </div>
            </xsl:if>
            <xsl:if test="$mode = 'web'">
                <div id="googleads">
                    <script type="text/javascript"><xsl:comment>
                            google_ad_client = "pub-2535373996863248";
                            google_ad_width = 120;
                            google_ad_height = 600;
                            google_ad_format = "120x600_as";
                            google_ad_type = "text_image";
                            google_ad_channel ="";
                            google_color_border = "5290ee";
                            google_color_bg = "FFFFFF";
                            google_color_link = "0000FF";
                            google_color_url = "008000";
                            google_color_text = "000000";
                            //</xsl:comment></script>
                    <script type="text/javascript"
                        src="http://pagead2.googlesyndication.com/pagead/show_ads.js">
                    </script>
                </div>
            </xsl:if>
            </body>
        </html>
    </xsl:template>

    <xsl:template match="@* | node()">
        <xsl:copy>
            <xsl:apply-templates select="@* | node()" />
        </xsl:copy>
    </xsl:template>

    <xsl:template match="content" mode="content">
        <xsl:apply-templates select="*" />
    </xsl:template>

    <xsl:template name="controls">
        <xsl:apply-templates select="$common/common/navigation" />
        <xsl:if test="$sourceforge = '1'">
            <p/>
            <a href="http://www.cenqua.com/clover.net"><img src="http://www.cenqua.com/images/cloverednet1.gif" width="89" height="33" border="0" alt="Code Coverage by Clover.NET"/></a>
        </xsl:if>
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
                <tr><td class="nav"><a class="nav"><xsl:attribute name="href"><xsl:value-of select="@href" /><xsl:if test="not(@noext)">.<xsl:value-of select="$file_extension" /></xsl:if></xsl:attribute><xsl:value-of select="@label" /></a></td></tr>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>

    <xsl:template match="subnav">
        <xsl:choose>
            <xsl:when test="$subpage_id = @href"><tr><td><a class="subnav_selected" href="{@href}.{$file_extension}"><xsl:value-of select="@label" /></a></td></tr></xsl:when>
            <xsl:otherwise>
                <tr><td><a class="subnav"><xsl:attribute name="href"><xsl:value-of select="@href" /><xsl:if test="not(@noext)">.<xsl:value-of select="$file_extension" /></xsl:if></xsl:attribute><xsl:value-of select="@label" /></a></td></tr>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>

    <xsl:template name="simple-type-name">
        <xsl:param name="type" />

        <xsl:choose>
            <xsl:when test="contains($type,'.')">
                <xsl:call-template name="simple-type-name">
                    <xsl:with-param name="type" select="substring-after($type,'.')" />
                </xsl:call-template>
            </xsl:when>
            <xsl:when test="$type = 'Int32'">integer</xsl:when>
            <xsl:when test="$type = 'String'">string</xsl:when>
            <xsl:when test="$type = 'Boolean'">boolean</xsl:when>
            <xsl:otherwise>
                <xsl:value-of select="$type" />
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>

    <xsl:include href="syntax.xsl" />

    <xsl:template match="benchmark-table">
        <xsl:variable name="nlog_results" select="document('nlog.results.xml')" />
        <xsl:variable name="log4net_results" select="document('log4net.results.xml')" />

        <div class="table">
            <table width="620">
                <col width="30%" />
                <col width="30%" />
                <col width="12%" />
                <col width="12%" />
                <xsl:if test="$log4net_comparison = '1'">
                    <col width="12%" />
                    <col width="12%" />
                </xsl:if>
                <tr>
                    <th rowspan="2">Appender</th>
                    <th rowspan="2">Call mode</th>
                    <th colspan="2">Results</th>
                    <xsl:if test="$log4net_comparison = '1'">
                        <th colspan="2">log4net</th>
                    </xsl:if>
                </tr>
                <tr>
                    <th>nanoseconds per log</th>
                    <th>logs per second</th>
                    <xsl:if test="$log4net_comparison = '1'">
                        <th>nanoseconds per log</th>
                        <th>logs per second</th>
                    </xsl:if>
                </tr>
                <xsl:for-each select="$nlog_results/results/test/timing">
                    <xsl:variable name="logger_name" select="../@logger" />
                    <xsl:variable name="timing_name" select="@name" />

                    <xsl:variable name="log4net_timing"
                        select="$log4net_results/results/test[@logger=$logger_name]/timing[@name=$timing_name]" />

                    <tr>
                        <td><xsl:value-of select="$logger_name" /></td>
                        <td><xsl:value-of select="$timing_name" /></td>
                        <td>
                            <xsl:if test="$log4net_comparison = '1'">
                                <xsl:if test="@nanosecondsPerLog &lt; $log4net_timing/@nanosecondsPerLog">
                                    <xsl:attribute name="class">benchmark-winner</xsl:attribute>
                                </xsl:if>
                            </xsl:if>
                            <xsl:value-of select="@nanosecondsPerLog" /></td>
                        <td>
                            <xsl:if test="$log4net_comparison = '1'">
                                <xsl:if test="@nanosecondsPerLog &lt; $log4net_timing/@nanosecondsPerLog">
                                    <xsl:attribute name="class">benchmark-winner</xsl:attribute>
                                </xsl:if>
                            </xsl:if>
                            <xsl:value-of select="@logsPerSecond" /></td>
                        <xsl:if test="$log4net_comparison = '1'">
                            <td>
                                <xsl:if test="@nanosecondsPerLog &gt; $log4net_timing/@nanosecondsPerLog">
                                    <xsl:attribute name="class">benchmark-winner</xsl:attribute>
                                </xsl:if>
                                <xsl:value-of select="$log4net_timing/@nanosecondsPerLog" /></td>
                            <td>
                                <xsl:if test="@nanosecondsPerLog &gt; $log4net_timing/@nanosecondsPerLog">
                                    <xsl:attribute name="class">benchmark-winner</xsl:attribute>
                                </xsl:if>
                                <xsl:value-of select="$log4net_timing/@logsPerSecond" /></td>
                        </xsl:if>
                    </tr>
                </xsl:for-each>
            </table>
        </div>
    </xsl:template>
</xsl:stylesheet>
