<?xml version="1.0" encoding="ISO-8859-1"?>

<!--
   This XSL File is based on the NUnitSummary.xsl
   template created by Tomas Restrepo fot NAnt's NUnitReport.
   
   Modified by Gilles Bayon (gilles.bayon@laposte.net) for use
   with NUnit2Report.

-->

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0" xmlns:html="http://www.w3.org/Profiles/XHTML-transitional">

   <xsl:output method="html" indent="yes"/>
   <xsl:include href="toolkit.xsl"/>
   <xsl:preserve-space elements='a root'/>


<!--
	====================================================
		Create the page structure
    ====================================================
-->
<xsl:template match="test-results">
	<HTML>
		<HEAD>
		<style type="text/css">
			body {
				font:normal 68% verdana,arial,helvetica;
				color:#000000;
			}

			span.covered {
				background: #00df00; 
				border:#9c9c9c 1px solid;
			}
			span.uncovered {
				background: #df0000; 
				border-top:#9c9c9c 1px solid;
				border-bottom:#9c9c9c 1px solid;
				border-right:#9c9c9c 1px solid;
				}
			span.ignored {
				background: #ffff00;
				border-top:#9c9c9c 1px solid;
				border-bottom:#9c9c9c 1px solid;
				border-right:#9c9c9c 1px solid;
			}

			td {
				FONT-SIZE: 68%;
				BORDER-BOTTOM: #dcdcdc 1px solid; 
				BORDER-RIGHT: #dcdcdc 1px solid;
			}
			p {
				line-height:1.5em;
				margin-top:0.5em; 
				margin-bottom:1.0em;
			}
			h1 {
				MARGIN: 0px 0px 5px; 
				FONT: 165% verdana,arial,helvetica;
			}
			h2 {
				MARGIN-TOP: 1em; 
				MARGIN-BOTTOM: 0.5em; 
				FONT: bold 125% verdana,arial,helvetica;
			}
			h3 {
				MARGIN-BOTTOM: 0.5em; 
				FONT: bold 115% verdana,arial,helvetica;
			}
			h4 {
				MARGIN-BOTTOM: 0.5em; 
				FONT: bold 100% verdana,arial,helvetica;
			}
			h5 {
				MARGIN-BOTTOM: 0.5em; 
				FONT: bold 100% verdana,arial,helvetica
			}
			h6 {
				MARGIN-BOTTOM: 0.5em; 
				FONT: bold 100% verdana,arial,helvetica
			}	
			.Error {
				font-weight:bold; 
			}
			.Failure {
				font-weight:bold; 
				color:red;
			}
			.Ignored {
				font-weight:bold; 
			}
			.FailureDetail {
				font-size: -1;
				padding-left: 2.0em;
				background:#cdcdcd;
			}
			.Pass {
				padding-left:2px;
			}
			.TableHeader {
				background: #efefef;
				color: #000;
				font-weight: bold;
				horizontal-align: center;
			}
			a:visited {
				color: #0000ff;
			}
			a {
				color: #0000ff;
			}
			a:active {
				color: #800000;
			}
			a.summarie {
				color:#000;
				text-decoration: none;
			}
			a.summarie:active {
				color:#000;
				text-decoration: none;
			}
			a.summarie:visited {
				color:#000;
				text-decoration: none;
			}
			.description {
				margin-top:1px;
				padding:3px;
				background-color:#dcdcdc;
				color:#000;
				font-weight:normal;
			}
			.method{
				color:#000;
				font-weight:normal;
				padding-left:5px;
			}
			a.method{
				text-decoration: none;
				color:#000;
				font-weight:normal;
				padding-left:5px;
			}
			a.Failure {
				font-weight:bold; 
				color:red;
				text-decoration: none;
			}
			a.Failure:visited {
				font-weight:bold; 
				color:red;
				text-decoration: none;
			}
			a.Failure:active {
				font-weight:bold; 
				color:red;
				text-decoration: none;
			}
			a.error {
				font-weight:bold; 
				color:red;
			}
			a.error:visited {
				font-weight:bold; 
				color:red;
			}
			a.error:active {
				font-weight:bold; 
				color:red;
				/*text-decoration: none;
				padding-left:5px;*/
			}
			a.ignored {
				font-weight:bold; 
				text-decoration: none;
				padding-left:5px;
			}
			a.ignored:visited {
				font-weight:bold; 
				text-decoration: none;
				padding-left:5px;
			}
			a.ignored:active {
				font-weight:bold; 
				text-decoration: none;
				padding-left:5px;
			}
	  </style>
      <script language="JavaScript"><![CDATA[   
	  function Toggle(id) {
	  var element = document.getElementById(id);

		 if ( element.style.display == "none" )
			element.style.display = "block";
		 else 
			element.style.display = "none";
	  }

	  function ToggleImage(id) {
	  var element = document.getElementById(id);

		 if ( element.innerText   == "-" )
			element.innerText   = "+";
		 else 
			element.innerText = "-";
	  }
	]]></script>
		</HEAD>
		<body text="#000000" bgColor="#ffffff">
			<a name="#top"></a>
			<xsl:call-template name="header"/>
			
			<!-- Summary part -->
			<xsl:call-template name="summary"/>
			<hr size="1" width="95%" align="left"/>
			
			<!-- Package List part -->
			<xsl:call-template name="packagelist"/>
			<hr size="1" width="95%" align="left"/>
			
			<!-- For each testsuite create the part -->
			<xsl:call-template name="testsuites"/>
			<hr size="1" width="95%" align="left"/>
			
			<!-- Environment info part -->
 			
			<xsl:call-template name="envinfo"/>

		</body>
	</HTML>
</xsl:template>
	
	
	
	<!-- ================================================================== -->
	<!-- Write a list of all packages with an hyperlink to the anchor of    -->
	<!-- of the package name.                                               -->
	<!-- ================================================================== -->
	<xsl:template name="packagelist">	
		<h2 id=":i18n:TestSuiteSummary">TestSuite Summary</h2>
		<table border="0" cellpadding="2" cellspacing="0" width="95%">
			<xsl:call-template name="packageSummaryHeader"/>
			<!-- list all packages recursively -->
			<xsl:for-each select="//test-suite[(child::results/test-case)]">
				<xsl:sort select="@name"/>
				<xsl:variable name="testCount" select="count(child::results/test-case)"/>
				<xsl:variable name="errorCount" select="count(child::results/test-case[@executed='False'])"/>
				<xsl:variable name="failureCount" select="count(child::results/test-case[@success='False'])"/>
				<xsl:variable name="runCount" select="$testCount - $errorCount - $failureCount"/>
				<xsl:variable name="timeCount" select="translate(@time,',','.')"/>
		
				<!-- write a summary for the package -->
				<tr valign="top">
					<!-- set a nice color depending if there is an error/failure -->
					<xsl:attribute name="class">
						<xsl:choose>
						    <xsl:when test="$failureCount &gt; 0">Failure</xsl:when>
							<xsl:when test="$errorCount &gt; 0"> Error</xsl:when>
							<xsl:otherwise>Pass</xsl:otherwise>
						</xsl:choose>
					</xsl:attribute> 	
					<td width="25%">
						<a href="#{generate-id(@name)}">
						<xsl:attribute name="class">
							<xsl:choose>
								<xsl:when test="$failureCount &gt; 0">Failure</xsl:when>
							</xsl:choose>
						</xsl:attribute> 	
						<xsl:value-of select="@name"/>
						</a>
					</td>
					<td nowrap="nowrap" width="6%" align="right">
						<xsl:variable name="successRate" select="$runCount div $testCount"/>
						<b>
						<xsl:call-template name="display-percent">
							<xsl:with-param name="value" select="$successRate"/>
						</xsl:call-template>
						</b>
					</td>
					<td width="20%" height="9px">
						<xsl:if test="round($runCount * 200 div $testCount )!=0">
							<span class="covered">
								<xsl:attribute name="style">width:<xsl:value-of select="round($runCount * 200 div $testCount )"/>px</xsl:attribute>
							</span>
						</xsl:if>
						<xsl:if test="round($errorCount * 200 div $testCount )!=0">
						<span class="ignored">
							<xsl:attribute name="style">width:<xsl:value-of select="round($errorCount * 200 div $testCount )"/>px</xsl:attribute>
						</span>
						</xsl:if>
						<xsl:if test="round($failureCount * 200 div $testCount )!=0">
							<span class="uncovered">
								<xsl:attribute name="style">width:<xsl:value-of select="round($failureCount * 200 div $testCount )"/>px</xsl:attribute>
							</span>
						</xsl:if>
					</td>
					<td><xsl:value-of select="$runCount"/></td>
					<td><xsl:value-of select="$errorCount"/></td>
					<td><xsl:value-of select="$failureCount"/></td>
					<td>
                       <xsl:call-template name="display-time">
                        	<xsl:with-param name="value" select="$timeCount"/>
                        </xsl:call-template>				
					</td>					
				</tr>
			</xsl:for-each>
		</table>		
	</xsl:template>
	

	<xsl:template name="testsuites">   
		<xsl:for-each select="//test-suite[(child::results/test-case)]">
			<xsl:sort select="@name"/>
			<!-- create an anchor to this class name -->
			<a name="#{generate-id(@name)}"></a>
			<h3>TestSuite <xsl:value-of select="@name"/></h3>

			<table border="0" cellpadding="1" cellspacing="1" width="95%">
				<!-- Header -->
				<xsl:call-template name="classesSummaryHeader"/>

				<!-- match the testcases of this package -->
				<xsl:apply-templates select="results/test-case">
				   <xsl:sort select="@name" /> 
				</xsl:apply-templates>
			</table>
			<a href="#top" id=":i18n:Backtotop">Back to top</a>
		</xsl:for-each>
	</xsl:template>
	

  <xsl:template name="dot-replace">
	  <xsl:param name="package"/>
	  <xsl:choose>
		  <xsl:when test="contains($package,'.')"><xsl:value-of select="substring-before($package,'.')"/>_<xsl:call-template name="dot-replace"><xsl:with-param name="package" select="substring-after($package,'.')"/></xsl:call-template></xsl:when>
		  <xsl:otherwise><xsl:value-of select="$package"/></xsl:otherwise>
	  </xsl:choose>
  </xsl:template>

</xsl:stylesheet>
