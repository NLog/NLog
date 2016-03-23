<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.1" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt"
    xmlns:shfb="urn:shfb-extensions">

<!--
// System  : Sandcastle Help File Builder
// File    : BuildLog.xsl
// Author  : Eric Woodruff
// Updated : 01/07/2012
// Note    : Copyright 2008-2012, Eric Woodruff, All rights reserved
//
// This is used to convert a SHFB build log into a viewable HTML page.
-->

  <xsl:param name="filterOn" select="'false'" />
	<xsl:param name="highlightOn" select="'false'" />

	<msxsl:script language="C#" implements-prefix="shfb">
  <msxsl:using namespace="System.Text" />
  <msxsl:using namespace="System.Text.RegularExpressions" />
    <![CDATA[
    private static Regex reWarning = new Regex(@"(Warn|Warning( HXC\d+)?):|" +
        @"SHFB\s*:\s*(W|w)arning\s.*?:|.*?(\(\d*,\d*\))?:\s*(W|w)arning\s.*?:");

    private static Regex reErrors = new Regex(
        @"^\s*((Error|UnrecognizedOption|Unhandled Exception|Fatal Error|" +
        @"Unexpected error.*|HHC\d+: Error|(Fatal )?Error HXC\d+):|" +
        @"Process is terminated|BUILD FAILED|\w+\s*:\s*(E|e)rror\s.*?:|" +
        @".*?\(\d*,\d*\):\s*(E|e)rror\s.*?:)", RegexOptions.Multiline);

    // Encode a few special characters, add a style to warnings and errors, and
		// return a non-breaking space if empty.
    public static string StyleLogText(string logText, string filterOn, string highlightOn)
    {
        // System.Web isn't always available so do some simple encoding
        logText = logText.Trim().Replace("&", "&amp;");
        logText = logText.Replace("<", "&lt;");
        logText = logText.Replace(">", "&gt;");

        // Include all text or just filter for warnings and errors?
        if(filterOn == "false")
        {
						// Highlight warnings and errors in the full text?
						if(highlightOn == "true")
						{
								logText = reWarning.Replace(logText, "<span class=\"Warning\">$0</span>");
								logText = reErrors.Replace(logText, "<span class=\"Error\">$0</span>");
						}
        }
        else
        {
            StringBuilder sb = new StringBuilder(2048);

            foreach(string s in logText.Split('\n'))
                if(reWarning.IsMatch(s))
                {
                    sb.Append(reWarning.Replace(s, "<span class=\"Warning\">$0</span>"));
                    sb.Append('\n');
                }
                else
                    if(reErrors.IsMatch(s))
                    {
                        sb.Append(reErrors.Replace(s, "<span class=\"Error\">$0</span>"));
                        sb.Append('\n');
                    }

            logText = sb.ToString();
        }

        return (logText.Length == 0) ? "&#160;" : logText;
    }
  	]]>
  </msxsl:script>

  <xsl:output method="xml" omit-xml-declaration="yes" encoding="utf-8" />

  <!-- Main template for the log -->
  <xsl:template match="/shfbBuild">
<html>
<head>
<title><xsl:value-of select="product"/></title>
<META HTTP-EQUIV="Content-Type" CONTENT="text/html; charset=UTF-8" />
<style>
  body { font-size: 8pt; font-family: Arial, Verdana, sans-serif; color: black; background-color: white; }
  h3 { margin: 0px; }
  h4 { margin: 0px; }
  pre { font-family: Consolas, "Courier New", Courier, monospace; font-size: 8pt; margin-top: 0px; margin-left: 20px; margin-bottom: 20px; padding: 0px; }
  .SectionHeader { background-color: #0066cc; color: white; padding: 5px; width: 95%; margin-left: 0px; margin-right: 2px; margin-top: 0px; padding: 2px; }
  .CollapsedHeader { background-color: #dcdcdc; color: black; padding: 5px; width: 95%; margin-left: 0px; margin-right: 2px; margin-top: 0px; padding: 2px; }
  .Warning { font-weight: bold; background-color: #ffd700; padding: 2px; }
  .Error { font-weight: bold; background-color: #b22222; color: #ffffff; padding: 2px; }
  .CollapseBox { cursor: pointer; color: black; text-align: center; border-style: solid; border-width: 1px; border-color: gray; margin-left: 0px; margin-right: 2px; margin-top: 0px; padding: 2px; width: 20px; }
  .PlugIn { border-left: black 5px solid; padding-top: 5px; padding-bottom: 5px; padding-left: 10px; }
  .PlugInHeader { background-color: #cccc99; color: black; width: 95%; padding: 2px; }
</style>
</head>

<body>
<h3><xsl:value-of select="@product"/>&#160;<xsl:value-of select="@version"/> Build Log</h3>
<h4>Project File: <xsl:value-of select="@projectFile"/></h4>
<h4>Build Started: <xsl:value-of select="@started"/></h4>

<xsl:if test="$filterOn = 'true'">
(Filtered for warnings and errors only)
</xsl:if>

<br/><hr/>
<a href="#" onclick="javascript: ExpandCollapseAll(false);">Collapse All</a>&#160;&#160;&#160;&#160;<a href="#" onclick="javascript: ExpandCollapseAll(true);">Expand All</a>
<hr/>

<!-- Process the build steps -->
<xsl:apply-templates select="buildStep" />

<hr/>
End of Log
<hr/>
<a href="#" onclick="javascript: ExpandCollapseAll(false);">Collapse All</a>&#160;&#160;&#160;&#160;<a href="#" onclick="javascript: ExpandCollapseAll(true);">Expand All</a>

<script type="text/javascript">
// Expand/collapse a section
function ExpandCollapse(showId, hideId)
{
    var showSpan = document.getElementById(showId),
        hideSpan = document.getElementById(hideId);

    showSpan.style.display = "inline";
    hideSpan.style.display = "none";
}

// Expand or collapse all sections
function ExpandCollapseAll(expand)
{
    var spans = document.getElementsByTagName("span")
    var spanIdx, id;

	for(spanIdx = 0; spanIdx != spans.length - 1; spanIdx++)
	{
	    id = spans[spanIdx].getAttribute('id');

        if(id.substr(0, 4) == "col_")
            if(expand)
                ExpandCollapse("exp_" + id.substr(4), id);
            else
                ExpandCollapse(id, "exp_" + id.substr(4));
    }
}
</script>

</body>
</html>
  </xsl:template>

  <!-- Build step template -->
  <xsl:template match="buildStep">
    <span id="col_{@step}" style="display: none;"><span class="CollapseBox" onclick="javascript: ExpandCollapse('exp_{@step}', 'col_{@step}');">+</span>
    <span><span class="CollapsedHeader"><xsl:value-of select="@step"/></span><br/><br/></span></span><span id="exp_{@step}" style="display: inline;">
    <span class="CollapseBox" onclick="javascript: ExpandCollapse('col_{@step}', 'exp_{@step}');">-</span>

    <span class="SectionHeader"><xsl:value-of select="@step"/></span><br/><br/>
    <pre>
        <xsl:apply-templates />
    </pre>
    </span>
  </xsl:template>

  <!-- Plug-in template -->
  <xsl:template match="plugIn">
    <div class="PlugIn"><span class="PlugInHeader"><b>Plug-In:</b>&#160;<xsl:value-of select="@name" />&#160;&#160;<b>Running:</b>&#160;<xsl:value-of select="@behavior" />&#160;&#160;<b>Priority:</b>&#160;<xsl:value-of select="@priority" /></span><br/>
      <xsl:value-of select="shfb:StyleLogText(text(), $filterOn, $highlightOn)" disable-output-escaping="yes" />
    </div>
  </xsl:template>

  <!-- Text template -->
  <xsl:template match="text()">
    <xsl:value-of select="shfb:StyleLogText(., $filterOn, $highlightOn)" disable-output-escaping="yes" />
  </xsl:template>

</xsl:stylesheet>
