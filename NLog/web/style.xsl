<?xml version="1.0" encoding="windows-1250" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

    <xsl:param name="page_id_override"></xsl:param>
    <xsl:param name="subpage_id_override"></xsl:param>
    <xsl:param name="file_extension">xml</xsl:param>
    <xsl:param name="sourceforge">0</xsl:param>
    <xsl:param name="log4net_comparison">0</xsl:param>
    <xsl:param name="build_time">2006-01-01</xsl:param>
    <xsl:param name="mode">web</xsl:param>
    <xsl:param name="nlog_package">temp</xsl:param>

    <xsl:variable name="page_id" select="concat(/*[position()=1]/@id,$page_id_override)" />
    <xsl:variable name="subpage_id" select="concat(/*[position()=1]/@subid,$subpage_id_override)" />
    <xsl:variable name="common" select="document(concat($mode,'.menu'))" />
    
    <xsl:output method="xml" omit-xml-declaration="yes" 
                doctype-system="http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd" 
                doctype-public="-//W3C//DTD XHTML 1.0 Transitional//EN" indent="no" />

    <xsl:template match="/">
        <html>
            <head>
                <xsl:apply-templates select="//base" />
                <link rel="stylesheet" href="style.css" type="text/css" />
                <link rel="stylesheet" href="syntax.css" type="text/css" />
                <link rel="icon" href="http://www.nlog-project.org/favicon.ico" type="image/x-icon" />
                <link rel="shortcut icon" href="http://www.nlog-project.org/favicon.ico" type="image/x-icon" /> 
                <meta name="keywords" content="nlog, server application logging, audit, .net logging, enterprise logging, central log management, log routing, event log, web application logging, cross-platform logging, log filtering, log rotation, database logging, silverlight tracing, logging, diagnostics, debug, trace, open source, free, .net logging api, system.diagnostics, log4net" />
                <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
                <title>NLog - <xsl:value-of select="$common/common/navigation/nav[@href=$page_id]/@label" /></title>
            </head>
            <body>
                <xsl:choose>
                    <xsl:when test="$mode='plain'">
                        <div class="plaincontent">
                            <xsl:apply-templates select="/" mode="content" />
                        </div>
                    </xsl:when>
                    <xsl:otherwise>
<div id="page">

<div id="header">
          <xsl:if test="$mode='web'">
                <div id="googlesearch">
<form action="http://www.google.com/cse" id="cse-search-box" target="_blank">
  <div>
    <input type="hidden" name="cx" value="partner-pub-2855917711217299:9kfg9pgji1w" />
    <input type="hidden" name="ie" value="UTF-8" />
    <input type="text" name="q" size="26" />
    <input type="submit" id="search-submit" name="sa" value="Search" />
  </div>
</form>
 
<script type="text/javascript" src="http://www.google.com/coop/cse/brand?form=cse-search-box&amp;lang=en"></script>
                </div>
          </xsl:if>

	  <div id="logo">
                    <img src="NLog.png" title="NLog - Advanced .NET Logging" />
          </div>
</div>

<div id="menu">
   <xsl:call-template name="controls" />
</div>

<div id="content">
                            <xsl:apply-templates select="/" mode="content" />
</div>

<div id="footer">
     Copyright &#169; 2004-2009 by <a style="text-decoration: none" href="http://www.nlog-project.org/disclaimer.html">Jaroslaw Kowalski</a> | <a style="text-decoration: none" href="http://www.nlog-project.org/disclaimer.html">Disclaimer</a>
</div>

</div>
            </xsl:otherwise>
        </xsl:choose>
        <xsl:if test="$mode = 'web'">
                <div id="counterCode">
                    <!-- Google Analytics -->
                    <script src="http://www.google-analytics.com/urchin.js" type="text/javascript">
                    </script>
                    <script type="text/javascript">
                        _uacct = "UA-256960-2";
                        urchinTracker();
                    </script>
                    <!-- End of Google Analytics -->
                </div>
            </xsl:if>
        </body>
    </html>
</xsl:template>

<xsl:template name="nlog-package-name" match="nlog-package-name">
    <xsl:value-of select="$nlog_package" />
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
    </xsl:template>

    <xsl:template match="code[@lang]">
        <pre class="example">
            <xsl:copy-of select="." />
        </pre>
    </xsl:template>

    <xsl:template match="navigation">
	<ul>
	   <xsl:apply-templates select="nav" />
	</ul>
    </xsl:template>

    <xsl:template match="a[starts-with(@href,'http://') and not(starts-with(@href,'http://www.nlog-project')) and not(@nomangle)]">
        <xsl:if test="$mode!='plain'">
            <img class="out_link" src="out_link.gif" />
        </xsl:if>
        <a style="padding-left: 4px" href="http://www.nlog-project.org/external/{substring-after(@href,'http://')}">
            <xsl:apply-templates />
        </a>
    </xsl:template>

    <xsl:template match="nav">
	<li>
	    <xsl:if test="$page_id = @href">
                <xsl:attribute name="class">selected</xsl:attribute>
            </xsl:if>
            <a class="nav_selected" href="{@href}.{$file_extension}"><xsl:value-of select="@label" /></a>
        </li>
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
            <xsl:when test="contains($type,'#')"><xsl:value-of select="substring-after($type,'#')" /></xsl:when>
            <xsl:otherwise>
                <xsl:value-of select="$type" />
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>

    <xsl:include href="syntax.xsl" />

    <!--
        static string MakeCamelCase(string s)
        {
            if (s.Length < 1)
                return s.ToLower();

            int firstLower = s.Length;
            for (int i = 0; i < s.Length; ++i)
            {
                if (Char.IsLower(s[i]))
                {
                    firstLower = i;
                    break;
                }
            }

            if (firstLower == 0)
                return s;

            // DBType
            if (firstLower != 1 && firstLower != s.Length)
                firstLower = firstLower - 1;
            return s.Substring(0, firstLower).ToLower() + s.Substring(firstLower);
        }

        -->

    <xsl:template name="isLower">
        <xsl:param name="char" />
        <xsl:variable name="lowerCase" select="translate($char,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')" />

        <xsl:choose>
            <xsl:when test="$char=$lowerCase">1</xsl:when>
            <xsl:otherwise>0</xsl:otherwise>
        </xsl:choose>
    </xsl:template>

    <xsl:template name="firstLower">
        <xsl:param name="text" />
        <xsl:param name="pos">0</xsl:param>

        <xsl:variable name="isLower">
            <xsl:call-template name="isLower">
                <xsl:with-param name="char" select="substring($text,$pos+1,$pos+1)"></xsl:with-param>
            </xsl:call-template>
        </xsl:variable>

        <xsl:choose>
            <xsl:when test="$isLower=1"><xsl:value-of select="$pos" /></xsl:when>
            <xsl:otherwise>
                <xsl:call-template name="firstLower">
                    <xsl:with-param name="text" select="$text" />
                    <xsl:with-param name="pos" select="$pos + 1" />
                </xsl:call-template>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>

    <xsl:template name="camelCase">
        <xsl:param name="text" />
        <xsl:variable name="textLength" select="string-length($text)" />

        <xsl:variable name="firstLower"><xsl:call-template name="firstLower">
                <xsl:with-param name="text" select="$text" />
        </xsl:call-template></xsl:variable>
        <xsl:choose>
            <xsl:when test="$textLength &lt;= 1"><xsl:value-of  select="translate($text,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')" /></xsl:when>
            <xsl:when test="$firstLower = 0"><xsl:value-of select="$text" /></xsl:when>
            <xsl:when test="$firstLower = 1 or $firstLower = $textLength">
                <xsl:value-of select="translate(substring($text,1,$firstLower),'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')" />
                <xsl:value-of select="substring($text,$firstLower+1)" />
            </xsl:when>
            <xsl:otherwise>
                <xsl:value-of select="translate(substring($text,1,$firstLower - 1),'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')" />
                <xsl:value-of select="substring($text,$firstLower)" />
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>

    <xsl:template match="camel">
        <xsl:call-template name="camelCase">
            <xsl:with-param name="text"><xsl:apply-templates /></xsl:with-param>
        </xsl:call-template>
    </xsl:template>

    <xsl:template name="parameter_info">
        <tr>
            <td class="parametername">
                <span>
                    <xsl:if test="attribute/@name='NLog.Config.RequiredParameterAttribute'">
                        <xsl:attribute name="class">required</xsl:attribute>
                    </xsl:if>
                    <xsl:call-template name="camelCase">
                        <xsl:with-param name="text" select="@name" />
                    </xsl:call-template>
                </span>
            </td>
            <td class="parametertype">
                <nobr>
                    <xsl:call-template name="simple-type-name">
                        <xsl:with-param name="type" select="@type" />
                    </xsl:call-template>
                    <xsl:if test="attribute/@name='NLog.Config.AcceptsLayoutAttribute'">
                        &#160;<a href="layoutrenderers.html"><span class="acceptslayout" title="This parameter accepts layout specification. Click here to learn more about layouts.">${}</span></a>
                    </xsl:if>
                    <xsl:if test="attribute/@name='NLog.Config.AcceptsConditionAttribute'">
                        &#160;<a href="conditions.html"><span class="acceptscondition" title="This parameter accepts condition expressions. Click here to learn more about condition expressions.">[c()]</span></a>
                    </xsl:if>
                </nobr>
            </td>
            <td class="parametervalue" width="100%">
                <table cellpadding="0" cellspacing="0">
                    <tr>
                        <td class="parametervalue2">
                            <xsl:apply-templates select="documentation/summary" />
                            <xsl:if test="attribute[@name='System.ComponentModel.DefaultValueAttribute']">
                                <p>Default value is: <code><xsl:value-of select="attribute[@name='System.ComponentModel.DefaultValueAttribute']/property[@name='Value']/@value" /></code>.</p>
                            </xsl:if>
                            <xsl:variable name="typename" select="concat('T:',translate(@type,'#','.'))" />
                            <xsl:variable name="enumnode" select="//enumeration[@id=$typename]" />
                            <xsl:if test="$enumnode">
                                <p>
                                    Possible values are:
                                    <ul>
                                        <xsl:for-each select="$enumnode/field">
                                            <li><b><code><xsl:value-of select="@name" /></code></b> - <xsl:apply-templates select="documentation/summary" /></li>
                                        </xsl:for-each>
                                    </ul>
                                </p>
                            </xsl:if>
                            <xsl:if test="documentation/remarks">
                                <p><xsl:apply-templates select="documentation/remarks" /></p>
                            </xsl:if>
                            <xsl:if test="documentation/example">
                                <h4>Example</h4>
                                <p><xsl:apply-templates select="documentation/example" /></p>
                            </xsl:if>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </xsl:template>


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

    <xsl:template name="detailssupportmatrix">
        <div class="listtable">
            <table>
                <tr>
                    <th rowspan="2">Assembly</th>
                    <th rowspan="2">Class</th>
                    <th colspan="3">.NET Framework</th>
                    <th colspan="2">.NET CF</th>
                    <th colspan="2">Mono on Windows</th>
                    <th colspan="2">Mono on Unix</th>
                </tr>
                <tr>
                    <th>1.0</th>
                    <th>1.1</th>
                    <th>2.0</th>
                    <th>1.0</th>
                    <th>2.0</th>
                    <th>1.0</th>
                    <th>2.0</th>
                    <th>1.0</th>
                    <th>2.0</th>
                </tr>
                <tr>
                    <td><xsl:value-of select="../../@name" /></td>
                    <td><xsl:value-of select="substring-after(@id,'T:')" /></td>
                        
                    <xsl:call-template name="supportmatrixvalues" />
                </tr>
            </table>
        </div>
    </xsl:template>

    <xsl:template name="supportmatrixheader">
        <tr>
            <th rowspan="2">Name</th>
            <th rowspan="2">Description</th>
            <th colspan="3">.NET Framework</th>
            <th colspan="2">.NET CF</th>
            <th colspan="2">Mono on Windows</th>
            <th colspan="2">Mono on Unix</th>
        </tr>
        <tr>
            <th>1.0</th>
            <th>1.1</th>
            <th>2.0</th>
            <th>1.0</th>
            <th>2.0</th>
            <th>1.0</th>
            <th>2.0</th>
            <th>1.0</th>
            <th>2.0</th>
        </tr>
    </xsl:template>

    <!-- returns a string containing '*' character if the 'attribute' matches
    the specified framework and OS -->

    <xsl:template match="attribute" mode="supported-runtime-matches">
        <xsl:param name="framework" />
        <xsl:param name="frameworkVersion" />
        <xsl:param name="os" />
        <xsl:param name="osVersion" />
        <xsl:param name="mode" />

        <xsl:variable name="attrFramework" select="property[@name='Framework']/@value" />
        <xsl:variable name="attrOS" select="property[@name='OS']/@value" />
        <xsl:variable name="attrMinRuntimeVersion" select="property[@name='MinRuntimeVersion']/@value" />
        <xsl:variable name="attrMaxRuntimeVersion" select="property[@name='MaxRuntimeVersion']/@value" />
        <xsl:variable name="attrMinOSVersion" select="property[@name='MinOSVersion']/@value" />
        <xsl:variable name="attrMaxOSVersion" select="property[@name='MaxOSVersion']/@value" />

        <xsl:variable name="result">
            I:
            <xsl:value-of select="$framework" />
            A:
            <xsl:value-of select="$attrFramework" />
            <xsl:choose>
                <xsl:when test="not($framework)">F1</xsl:when>
                <xsl:when test="not($attrFramework)">F1</xsl:when>
                <xsl:when test="$attrFramework = 'RuntimeFramework.Any'">F1</xsl:when>
                <xsl:when test="$attrFramework = $framework">F1</xsl:when>
                <xsl:otherwise>F0</xsl:otherwise>
            </xsl:choose>
            <xsl:choose>
                <xsl:when test="not($os)">O1</xsl:when>
                <xsl:when test="not($attrOS)">O1</xsl:when>
                <xsl:when test="$os = 'RuntimeOS.AnyWindows' and $attrOS='RuntimeOS.Windows'">O1</xsl:when>
                <xsl:when test="$os = 'RuntimeOS.AnyWindows' and $attrOS='RuntimeOS.WindowsNT'">O1</xsl:when>
                <xsl:when test="$attrOS = 'RuntimeOS.Any'">O1</xsl:when>
                <xsl:when test="$attrOS = $os">O1</xsl:when>
                <xsl:otherwise>O0</xsl:otherwise>
            </xsl:choose>
        </xsl:variable>

        <xsl:value-of select="$result" />

        <xsl:choose>
            <xsl:when test="contains($result,'0')">N</xsl:when>
            <xsl:otherwise>*</xsl:otherwise>
        </xsl:choose>
    </xsl:template>

    <xsl:template name="supported-on">
        <xsl:param name="framework" />
        <xsl:param name="frameworkVersion" />
        <xsl:param name="os" />
        <xsl:param name="osVersion" />

        <xsl:variable name="supportedAttributes" select="attribute[@name='NLog.Config.SupportedRuntimeAttribute']" />
        <xsl:variable name="notSupportedAttributes" select="attribute[@name='NLog.Config.NotSupportedRuntimeAttribute']" />

        <xsl:variable name="supportedAttributeMatches">
            <xsl:apply-templates select="$supportedAttributes" mode="supported-runtime-matches">
                <xsl:with-param name="framework"><xsl:value-of select="$framework" /></xsl:with-param>
                <xsl:with-param name="os"><xsl:value-of select="$os" /></xsl:with-param>
                <xsl:with-param name="frameworkVersion"><xsl:value-of select="$frameworkVersion" /></xsl:with-param>
                <xsl:with-param name="osVersion"><xsl:value-of select="$osVersion" /></xsl:with-param>
                <xsl:with-param name="mode">1</xsl:with-param>
            </xsl:apply-templates>
        </xsl:variable>
        
        <xsl:variable name="notSupportedAttributeMatches">
            <xsl:apply-templates select="$notSupportedAttributes" mode="supported-runtime-matches">
                <xsl:with-param name="framework"><xsl:value-of select="$framework" /></xsl:with-param>
                <xsl:with-param name="os"><xsl:value-of select="$os" /></xsl:with-param>
                <xsl:with-param name="frameworkVersion"><xsl:value-of select="$frameworkVersion" /></xsl:with-param>
                <xsl:with-param name="osVersion"><xsl:value-of select="$osVersion" /></xsl:with-param>
                <xsl:with-param name="mode">0</xsl:with-param>
            </xsl:apply-templates>
        </xsl:variable>

        <td class="support">
            <!--
            S[<xsl:value-of select="$supportedAttributeMatches" />]
            NS[<xsl:value-of select="$notSupportedAttributeMatches" />]
            -->
            <xsl:choose>
                <xsl:when test="$supportedAttributeMatches='' and $notSupportedAttributeMatches=''"><img src="yes.gif" /></xsl:when>
                <xsl:when test="contains($supportedAttributeMatches,'*') and not(contains($notSupportedAttributeMatches,'*'))"><img src="yes.gif" /></xsl:when>
                <xsl:when test="$supportedAttributeMatches='' and not(contains($notSupportedAttributeMatches,'*'))"><img src="yes.gif" /></xsl:when>
                <xsl:otherwise>&#160;</xsl:otherwise>
            </xsl:choose>
        </td>
    </xsl:template>

    <xsl:template name="supportmatrixvalues">
        <xsl:call-template name="supported-on">
            <xsl:with-param name="framework">RuntimeFramework.DotNetFramework</xsl:with-param>
            <xsl:with-param name="frameworkVersion">1.0</xsl:with-param>
            <xsl:with-param name="os">RuntimeOS.AnyWindows</xsl:with-param>
        </xsl:call-template>
        <xsl:call-template name="supported-on">
            <xsl:with-param name="framework">RuntimeFramework.DotNetFramework</xsl:with-param>
            <xsl:with-param name="frameworkVersion">1.1</xsl:with-param>
            <xsl:with-param name="os">RuntimeOS.AnyWindows</xsl:with-param>
        </xsl:call-template>
        <xsl:call-template name="supported-on">
            <xsl:with-param name="framework">RuntimeFramework.DotNetFramework</xsl:with-param>
            <xsl:with-param name="frameworkVersion">2.0</xsl:with-param>
            <xsl:with-param name="os">RuntimeOS.AnyWindows</xsl:with-param>
        </xsl:call-template>
        <xsl:call-template name="supported-on">
            <xsl:with-param name="framework">RuntimeFramework.DotNetCompactFramework</xsl:with-param>
            <xsl:with-param name="frameworkVersion">1.0</xsl:with-param>
            <xsl:with-param name="os">RuntimeOS.WindowsCE</xsl:with-param>
        </xsl:call-template>
        <xsl:call-template name="supported-on">
            <xsl:with-param name="framework">RuntimeFramework.DotNetCompactFramework</xsl:with-param>
            <xsl:with-param name="frameworkVersion">2.0</xsl:with-param>
            <xsl:with-param name="os">RuntimeOS.WindowsCE</xsl:with-param>
        </xsl:call-template>
        <xsl:call-template name="supported-on">
            <xsl:with-param name="framework">RuntimeFramework.Mono</xsl:with-param>
            <xsl:with-param name="frameworkVersion">1.0</xsl:with-param>
            <xsl:with-param name="os">RuntimeOS.AnyWindows</xsl:with-param>
        </xsl:call-template>
        <xsl:call-template name="supported-on">
            <xsl:with-param name="framework">RuntimeFramework.Mono</xsl:with-param>
            <xsl:with-param name="frameworkVersion">2.0</xsl:with-param>
            <xsl:with-param name="os">RuntimeOS.AnyWindows</xsl:with-param>
        </xsl:call-template>
        <xsl:call-template name="supported-on">
            <xsl:with-param name="framework">RuntimeFramework.Mono</xsl:with-param>
            <xsl:with-param name="frameworkVersion">1.0</xsl:with-param>
            <xsl:with-param name="os">RuntimeOS.Unix</xsl:with-param>
        </xsl:call-template>
        <xsl:call-template name="supported-on">
            <xsl:with-param name="framework">RuntimeFramework.Mono</xsl:with-param>
            <xsl:with-param name="frameworkVersion">2.0</xsl:with-param>
            <xsl:with-param name="os">RuntimeOS.Unix</xsl:with-param>
        </xsl:call-template>
    </xsl:template>

    <xsl:template match="link">
        <a href="{@href}.{$file_extension}"><xsl:apply-templates /></a>
    </xsl:template>

    <xsl:template name="last-component">
        <xsl:param name="t" />
        <xsl:choose>
            <xsl:when test="contains($t,'.')"><xsl:call-template name="last-component"><xsl:with-param name="t" select="substring-after($t,'.')" /></xsl:call-template></xsl:when>
            <xsl:otherwise><xsl:value-of select="$t" /></xsl:otherwise>
        </xsl:choose>
    </xsl:template>

    <xsl:template match="see[@cref]">
        <xsl:call-template name="last-component">
            <xsl:with-param name="t" select="@cref" />
        </xsl:call-template>
    </xsl:template>

    <xsl:template match="faq-index">
        <ol>
            <xsl:for-each select="//faq">
                <li><a href="#faq{generate-id(.)}"><xsl:apply-templates select="faq-question" /></a></li>
            </xsl:for-each>
        </ol>
    </xsl:template>

    <xsl:template match="faq">
        <hr />
        <a name="faq{generate-id(.)}"></a>
        <p>
            <b><xsl:apply-templates select="faq-question" /></b>
            <br/>
            <xsl:apply-templates select="faq-answer" />
        </p>
    </xsl:template>

    <xsl:template match="faq-question">
        <xsl:apply-templates />
    </xsl:template>

    <xsl:template match="faq-answer">
        <xsl:apply-templates />
    </xsl:template>

    <xsl:template match="last-changed-date">
    </xsl:template>

    <xsl:template match="last-changed-date" mode="lastchangeddate">
        <xsl:variable name="lastUpdated"><xsl:value-of select="substring(.,18,20)" /></xsl:variable>
        <xsl:if test="string-length($lastUpdated)=20"><p style="font-size: 80%">Last updated: <xsl:value-of select="$lastUpdated" /></p></xsl:if>
    </xsl:template>

</xsl:stylesheet>
