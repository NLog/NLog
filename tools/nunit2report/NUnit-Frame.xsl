<?xml version="1.0" encoding="ISO-8859-1"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" xmlns:nunit2report="urn:my-scripts">

<xsl:output method="html" indent="yes" encoding="ISO-8859-1"/>

<xsl:param name="nant.filename" />
<xsl:param name="nant.version" />
<xsl:param name="nant.project.name" />
<xsl:param name="nant.project.buildfile" />
<xsl:param name="nant.project.basedir" />
<xsl:param name="nant.project.default" />
<xsl:param name="sys.os" />
<xsl:param name="sys.os.platform" />
<xsl:param name="sys.os.version" />
<xsl:param name="sys.clr.version" />

<!-- 
Ts les noeuds à transformer en dossier
//test-suite[not(child::results/test-case) and not(@time=0)] 
//Ts les noeuds Suite de tests
//test-suite[(child::results/test-case)]
Ts les noms des dossier à créer
//test-suite[(child::results/test-case)]/ancestor::*[not(contains(@name,'.dll'))]/@name
-->
<msxsl:script language="C#" implements-prefix="nunit2report">
	
	public string assemblie(string path) {
	
	string[] a = path.Split('\\');

	return(a[a.Length-1]);
	}

	public string TestCaseName(string path) {
	
	string[] a = path.Split('.');

	return(a[a.Length-1]);
	}

</msxsl:script>

<xsl:template name="index.html">
<html>
    <head>
        <title>Unit Test Results.</title>
    </head>
    <frameset cols="20%,80%" framespacing="0">
        <frameset rows="30%,70%">
            <frame src="overview-frame.html" name="packageListFrame"/>
            <frame src="allclasses-frame.html" name="classListFrame"/>
        </frameset>
        <frame src="overview-summary.html" name="classFrame"/>
        <noframes>
            <h2>Frame Alert</h2>
            <p>
                This document is designed to be viewed using the frames feature. If you see this message, you are using a non-frame-capable web client.
            </p>
        </noframes>
    </frameset>
</html>
</xsl:template>

<xsl:template name="stylesheet.css">
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
</xsl:template>


<!--
    Creates an html file that contains a link to all package-summary.html files on
    each package existing on testsuites.
    @bug there will be a problem here, I don't know yet how to handle unnamed package :(
-->
<xsl:template name="all.packages">
    <html>
        <head>
            <title>All Unit Test Packages</title>
            <xsl:call-template name="create.stylesheet.link">
                <xsl:with-param name="package.name"/>
            </xsl:call-template>
        </head>
        <body>
		<h2><a href="overview-summary.html"  id=":i18n:Home" target="classFrame">Home</a></h2>

            <h2>Assemblies</h2>
			<!-- //test-results/@name -->
            <table width="100%">
				<tr>
					<td nowrap="nowrap">
						<a target="classFrame" href="overview-summary.html"><xsl:value-of select="nunit2report:assemblie(@name)"/></a>
					</td>
				</tr>
			</table>
        </body>
    </html>
</xsl:template>

<xsl:template name="overview.packages">
    <html>
        <head>
            <title>Unit Test Results: Summary</title>
            <xsl:call-template name="create.stylesheet.link">
                <xsl:with-param name="package.name"/>
            </xsl:call-template>
			<xsl:call-template name="toggle"/>
        </head>
        <body>
		<xsl:attribute name="onload">open('allclasses-frame.html','classListFrame')</xsl:attribute>
		<xsl:call-template name="pageHeader"/>
		<xsl:call-template name="envinfo"/>

		<h2 id=":i18n:Summary">Summary</h2>
		<xsl:variable name="runCount" select="@total"/><!-- testCount -->
		<!-- <xsl:variable name="errorCount" select="@not-run"/>
		<xsl:variable name="failureCount" select="@failures"/>-->

		<xsl:variable name="failureCount" select="@failures"/>
		<xsl:variable name="ignoreCount" select="@not-run"/>
		<xsl:variable name="total" select="$runCount + $ignoreCount + $failureCount"/>

		<xsl:variable name="timeCount" select="translate(test-suite/@time,',','.')"/>

		<!-- <xsl:variable name="successRate" select="($testCount - $failureCount - $errorCount) div $testCount"/>-->
		<xsl:variable name="successRate" select="$runCount div $total"/>

		<table border="0" cellpadding="2" cellspacing="0" width="95%" style="border: #dcdcdc 1px solid;">
		<xsl:call-template name="summaryHeader"/>
		<tr valign="top">
			<xsl:attribute name="class">
    			<xsl:choose>
    			    <xsl:when test="$failureCount &gt; 0">Failure</xsl:when>
    				<xsl:when test="$ignoreCount &gt; 0">Error</xsl:when>
    				<xsl:otherwise>Pass</xsl:otherwise>
    			</xsl:choose>			
			</xsl:attribute>		
			<td><xsl:value-of select="$runCount"/></td>
			<td><xsl:value-of select="$failureCount"/></td>
			<td><xsl:value-of select="$ignoreCount"/></td>
			<td  width="280px">
			    <xsl:call-template name="display-percent">
			        <xsl:with-param name="value" select="$successRate"/>
			    </xsl:call-template>&#160;
				<xsl:if test="round($runCount * 200 div $total )!=0">
					<span class="covered">
						<xsl:attribute name="style">width:<xsl:value-of select="round($runCount * 200 div $total )"/>px</xsl:attribute>
					</span>
				</xsl:if>
				<xsl:if test="round($ignoreCount * 200 div $total )!=0">
				<span class="ignored">
					<xsl:attribute name="style">width:<xsl:value-of select="round($ignoreCount * 200 div $total )"/>px</xsl:attribute>
				</span>
				</xsl:if>
				<xsl:if test="round($failureCount * 200 div $total )!=0">
					<span class="uncovered">
						<xsl:attribute name="style">width:<xsl:value-of select="round($failureCount * 200 div $total )"/>px</xsl:attribute>
					</span>
				</xsl:if>
			</td>
			<td>
			    <xsl:call-template name="display-time">
			        <xsl:with-param name="value" select="$timeCount"/>
			    </xsl:call-template>
			</td>
		</tr>
		</table>
		<span id=":i18n:Note">Note</span>: <i id=":i18n:failures">failures</i>&#160;<span id=":i18n:anticipated">are anticipated and checked for with assertions while</span>&#160;<i id=":i18n:errors">errors</i>&#160;<span id=":i18n:unanticipated">are unanticipated.</span>
		
		<h2 id=":i18n:TestSuiteSummary">TestSuite Summary</h2>
		<table border="0" cellpadding="2" cellspacing="0" width="95%">
			<xsl:call-template name="packageSummaryHeader"/>
			<!-- list all packages recursively -->
			<xsl:for-each select="//test-suite[(child::results/test-case)]">
				<xsl:sort select="@name"/>
				<!--<xsl:variable name="testCount2" select="count(child::results/test-case)"/>
				<xsl:variable name="errorCount2" select="count(child::results/test-case[@executed='False'])"/>
				<xsl:variable name="failureCount2" select="count(child::results/test-case[@success='False'])"/>
				<xsl:variable name="timeCount2" select="translate(@time,',','.')"/>-->
		
				<xsl:variable name="runCount2" select="count(child::results/test-case)"/>
				<xsl:variable name="errorCount2" select="count(child::results/test-case[@executed='False'])"/>
				<xsl:variable name="failureCount2" select="count(child::results/test-case[@success='False'])"/>
				<xsl:variable name="testCount2" select="$runCount2 + $errorCount2 + $failureCount2"/>
				<xsl:variable name="timeCount2" select="translate(@time,',','.')"/>

				<!-- write a summary for the package -->
				<tr valign="top">
					<!-- set a nice color depending if there is an error/failure -->
					<xsl:attribute name="class">
						<xsl:choose>
						    <xsl:when test="$failureCount2 &gt; 0">Failure</xsl:when>
							<xsl:when test="$errorCount2 &gt; 0"> Error</xsl:when>
							<xsl:otherwise>Pass</xsl:otherwise>
						</xsl:choose>
					</xsl:attribute> 	
					<td>

					<!-- ******************************************************* -->
					<!-- Rajout chemin  http://msdn.microsoft.com/library/default.asp?url=/library/en-us/xmlsdk/htm/xpath_hdi_2_5veb.asp -->
					<!-- ******************************************************* -->
					<xsl:variable name="path.dir">
						<xsl:for-each select="ancestor-or-self::*"><xsl:if test="not(contains(@name,'.dll')) and not(name()='results' or name()='testsummary')"><xsl:value-of select="concat(@name,'/')"/></xsl:if>
						</xsl:for-each>
					</xsl:variable>

					<a>
						<xsl:attribute name="href">
							<xsl:value-of select="$path.dir"/>
							<xsl:value-of select="@name"/>.html</xsl:attribute> 	
						<xsl:attribute name="class">
							<xsl:choose>
								<xsl:when test="$failureCount2 &gt; 0">Failure</xsl:when>
							</xsl:choose>
						</xsl:attribute> 	
						<xsl:value-of select="@name"/>
					</a>
					</td>
<!-- ******************************************************* -->
					<td width="15%" align="right">
						<xsl:variable name="successRate2" select="$runCount2 div $testCount2"/>
						<b>
						<xsl:call-template name="display-percent">
							<xsl:with-param name="value" select="$successRate2"/>
						</xsl:call-template>
						</b>
					</td>
					<td width="40%" height="9px">
						<xsl:if test="round($runCount2 * 200 div $testCount2 )!=0">
							<span class="covered">
								<xsl:attribute name="style">width:<xsl:value-of select="round($runCount2 * 200 div $testCount2 )"/>px</xsl:attribute>
							</span>
						</xsl:if>
						<xsl:if test="round($errorCount2 * 200 div $testCount2 )!=0">
						<span class="ignored">
							<xsl:attribute name="style">width:<xsl:value-of select="round($errorCount2 * 200 div $testCount2 )"/>px</xsl:attribute>
						</span>
						</xsl:if>
						<xsl:if test="round($failureCount2 * 200 div $testCount2 )!=0">
							<span class="uncovered">
								<xsl:attribute name="style">width:<xsl:value-of select="round($failureCount2 * 200 div $testCount2 )"/>px</xsl:attribute>
							</span>
						</xsl:if>
					</td>
<!--  ******************************************************* -->
					<td><xsl:value-of select="$runCount2"/>
					</td>
					<td><xsl:value-of select="$errorCount2"/></td>
					<td><xsl:value-of select="$failureCount2"/></td>
					<td>
                       <xsl:call-template name="display-time">
                        	<xsl:with-param name="value" select="$timeCount2"/>
                        </xsl:call-template>				
					</td>					
				</tr>
			</xsl:for-each>
		</table>		
	
		</body>
        </html>
</xsl:template>

<!--
    Creates an all-classes.html file that contains a link to all package-summary.html
    on each class.
-->
<xsl:template name="all.classes" >
    <html>
        <head>
            <title>All Unit Test Classes</title>
            <xsl:call-template name="create.stylesheet.link">
                <xsl:with-param name="package.name"/>
            </xsl:call-template>
        </head>
        <body>
            <h2>Test Suite</h2>
		<table border="0" width="95%">
			<!-- list all packages recursively -->
			<xsl:for-each select="//test-suite[(child::results/test-case)]">
				<xsl:sort select="@name"/>

				<xsl:variable name="errorCount" select="count(child::results/test-case[@executed='False'])"/>
				<xsl:variable name="failureCount" select="count(child::results/test-case[@success='False'])"/>

				<xsl:variable name="path.dir">
					<xsl:for-each select="ancestor-or-self::*"><xsl:if test="not(contains(@name,'.dll')) and not(name()='results' or name()='testsummary')"><xsl:value-of select="concat(@name,'/')"/></xsl:if>
					</xsl:for-each>
				</xsl:variable>

				<!-- write a summary for the package  -->
				<tr nowrap="nowrap">
					<td>
						<a target="classFrame" >
							<xsl:attribute name="href">
								<xsl:value-of select="$path.dir"/>
								<xsl:value-of select="@name"/>.html</xsl:attribute> 	
							<xsl:value-of select="@name"/>
						</a>
					</td>
				</tr>
			</xsl:for-each>
		</table>		

        </body>
    </html>
</xsl:template>

<!-- create the link to the stylesheet based on the package name -->
<xsl:template name="create.stylesheet.link">
    <xsl:param name="package.name"/>
    <link rel="stylesheet" type="text/css" title="Style"><xsl:attribute name="href"><xsl:if test="not($package.name = 'unnamed package')"><xsl:call-template name="path"><xsl:with-param name="path" select="$package.name"/></xsl:call-template></xsl:if>stylesheet.css</xsl:attribute></link>
</xsl:template>


<!--
    transform string like a.b.c to ../../../
    @param path the path to transform into a descending directory path
-->
<xsl:template name="path">
    <xsl:param name="path"/>
    <xsl:if test="contains($path,'.')">
        <xsl:text>../</xsl:text>    
        <xsl:call-template name="path">
            <xsl:with-param name="path"><xsl:value-of select="substring-after($path,'.')"/></xsl:with-param>
        </xsl:call-template>    
    </xsl:if>
    <xsl:if test="not(contains($path,'.')) and not($path = '')">
        <xsl:text>../</xsl:text>    
    </xsl:if>
</xsl:template>

<!-- Page HEADER -->
<xsl:template name="pageHeader">
<xsl:param name="path"/>

<h1><span id=":i18n:UnitTestsResults">Unit Tests Results</span> -  <xsl:value-of select="$nant.project.name"/></h1>
	<table width="100%">
	<tr>
	   <td align="left">
	      <span id=":i18n:GeneratedBy">Generated by</span>&#160;<a target="_blank" href="http://sourceforge.net/projects/nunit2report/">NUnit2Report</a> : <xsl:value-of select="/testsummary/test-results/@date"/> - <xsl:value-of select="concat(/testsummary/test-results/@time,' ')"/>&#160;<a href="#" onclick="javascript:Toggle('blabla')" id=":i18n:EnvironmentInformation">Environment Information</a>
	   </td>
		<td align="right"><span id=":i18n:Designed">Designed for use with</span>&#160;<a href='http://nunit.sourceforge.net/'>NUnit</a>&#160;<span id=":i18n:and">and</span>&#160;<a href='http://nant.sourceforge.net/'>NAnt</a>.
		</td>
	</tr>
	</table>
	<hr size="1"/>
</xsl:template>


<xsl:template name="summaryHeader">
	<tr valign="top" class="TableHeader">
		<td><b id=":i18n:Tests">Tests</b></td>
		<td><b id=":i18n:Failures">Failures</b></td>
		<td><b id=":i18n:Errors">Errors</b></td>
		<td><b id=":i18n:SuccessRate">Success Rate</b></td>
		<td nowrap="nowrap"><b id=":i18n:Time">Time(s)</b></td>
	</tr>
</xsl:template>

<!-- 
		=====================================================================
		testcase report
		=====================================================================
-->
<xsl:template name="test-case">

	<xsl:param name="dir.test"/>
	<xsl:param name="summary.xml"/>
	<xsl:param name="open.description"/>

	<xsl:variable name="summaries" select="document($summary.xml)" />

    <html>
        <head>
            <title>Unit Test for class <xsl:value-of select="./@name"/></title>
			<xsl:call-template name="create.stylesheet.link">
                <xsl:with-param name="package.name">
					<xsl:value-of select="$dir.test"/>
				</xsl:with-param>
            </xsl:call-template>
			<xsl:call-template name="toggle"/>
        </head>
        <body>
			<xsl:call-template name="pageHeader">
				<xsl:with-param name="path">
					<xsl:value-of select="$dir.test"/>
				</xsl:with-param>
            </xsl:call-template>
			
			<xsl:call-template name="envinfo"/>

			<h3>Test Suite</h3>

			<!-- Summary -->
			<table border="0" cellpadding="2" cellspacing="0" width="95%">
					<xsl:call-template name="packageSummaryHeader"/>

					<!--<xsl:variable name="testCount" select="count(./results/test-case)"/>
					<xsl:variable name="errorCount" select="count(./results/test-case[@executed='False'])"/>
					<xsl:variable name="failureCount" select="count(./results/test-case[@success='False'])"/>
					<xsl:variable name="timeCount" select="translate(@time,',','.')"/>-->

					<xsl:variable name="testCount" select="count(./results/test-case)"/>
					<xsl:variable name="errorCount" select="count(./results/test-case[@executed='False'])"/>
					<xsl:variable name="failureCount" select="count(./results/test-case[@success='False'])"/>
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
						<td>
							<xsl:value-of select="@name"/>
						</td>
<!-- ******************************************************* -->
					<td width="15%" align="right">
						<xsl:variable name="successRate" select="$runCount div $testCount"/>
						<b>
						<xsl:call-template name="display-percent">
							<xsl:with-param name="value" select="$successRate"/>
						</xsl:call-template>
						</b>
					</td>
					<td width="40%" height="9px">
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
<!--  ******************************************************* -->
						<td><xsl:value-of select="$runCount"/>
						</td>
						<td><xsl:value-of select="$errorCount"/></td>
						<td><xsl:value-of select="$failureCount"/></td>
						<td>
						   <xsl:call-template name="display-time">
								<xsl:with-param name="value" select="$timeCount"/>
							</xsl:call-template>				
						</td>		
					</tr>
			</table>
			<br/>
			<h3>Test Case</h3>
			<table border="0" cellpadding="1" cellspacing="1" width="95%" >
			<!-- Header -->
			<xsl:call-template name="classesSummaryHeader"/>


			<!-- match the testcases of this package -->
			<xsl:for-each select="child::results/test-case">
				<xsl:sort select="@name"/>
				<xsl:variable name="newid" select="generate-id(@name)" />
				<xsl:variable name="Mname" select="concat('M:',@name)" />

			   <xsl:variable name="result">
						<xsl:choose>
						  <xsl:when test="./failure"><span id=":i18n:Failure">Failure</span></xsl:when>
							<xsl:when test="./error"><span id=":i18n:Error">Error</span></xsl:when>
							<xsl:when test="@executed='False'"><span id=":i18n:Ignored">Ignored</span></xsl:when>
							<xsl:otherwise><span id=":i18n:Pass">Pass</span></xsl:otherwise>
						</xsl:choose>
			   </xsl:variable>

				<tr valign="top">
					<xsl:attribute name="class"><xsl:value-of select="$result"/></xsl:attribute>
		
					<td width="20%">
						<xsl:choose>
							<xsl:when test="$summary.xml != ''">
								<!-- Triangle image -->
								<a class="summarie"><xsl:attribute name="href">javascript:Toggle('<xsl:value-of select="concat('M:',$newid)"/>');ToggleImage('<xsl:value-of select="concat('I:',$newid)"/>')</xsl:attribute><xsl:attribute name="id"><xsl:value-of select="concat('I:',$newid)"/></xsl:attribute>
								<!-- Set the good triangle image -->
								<xsl:choose>
								<xsl:when test="$result != &quot;Pass&quot;">-</xsl:when>
								<xsl:otherwise>
									<xsl:choose>
										<xsl:when test="$open.description='yes'">-</xsl:when>
										<xsl:otherwise>+</xsl:otherwise>
									</xsl:choose>
								</xsl:otherwise>
								</xsl:choose>						
								</a>
							</xsl:when>
						</xsl:choose>

						<!-- If failure, you can click on the test method name and color is red -->
						<xsl:choose>
						<xsl:when test="$result = 'Failure' or $result = 'Error'">
							<a style="text-decoration:none">
							<xsl:attribute name="href">javascript:Toggle('<xsl:value-of select="$newid"/>')</xsl:attribute>
							<xsl:attribute name="class">Failure</xsl:attribute>
							<xsl:value-of select="nunit2report:TestCaseName(./@name)"/>
							</a>
						</xsl:when>
						<xsl:when test="$result = 'Ignored'">
							<a>
							<xsl:attribute name="href">javascript:Toggle('<xsl:value-of select="$newid"/>')</xsl:attribute>
							<xsl:attribute name="class">ignored</xsl:attribute>
							<xsl:value-of select="nunit2report:TestCaseName(./@name)"/>
							</a>
						</xsl:when>
						<xsl:otherwise>
							<xsl:attribute name="class">method</xsl:attribute>
							<xsl:value-of select="nunit2report:TestCaseName(./@name)"/>
						</xsl:otherwise>
						</xsl:choose>
<!-- *************************************** -->
					</td>
					<td width="65%" style="padding-left:3px" height="9px">
						<xsl:choose>
							<xsl:when test="$result = 'Pass'">
								<span class="covered" style="width:200px"></span>
							</xsl:when>
							<xsl:when test="$result = 'Ignored'">
								<span class="ignored" style="width:200px"></span>
							</xsl:when>			
							<xsl:when test="$result = 'Failure' or $result = 'Error'">
								<span class="uncovered" style="width:200px"></span>
							</xsl:when>			
						</xsl:choose>
<!-- ****************************** -->
						<!-- The  test method description-->
						<xsl:choose>
							<xsl:when test="$summary.xml != ''">
							<div class="description">
								<!-- Attribute id -->
								<xsl:attribute name="id"><xsl:value-of select="concat('M:',$newid)"/></xsl:attribute>
								<!-- Open method description if failure -->
								<xsl:choose>
								<xsl:when test="$result != &quot;Pass&quot;">
									<xsl:attribute name="style">display:block</xsl:attribute>
								</xsl:when>
								<xsl:otherwise>
									<xsl:choose>
										<xsl:when test="$open.description = 'yes'">
											<xsl:attribute name="style">display:block</xsl:attribute>
										</xsl:when>
										<xsl:otherwise>
											<xsl:attribute name="style">display:none</xsl:attribute>
										</xsl:otherwise>
									</xsl:choose>
								</xsl:otherwise>
								</xsl:choose>
								<!-- The description of the test method -->
								<xsl:value-of select="normalize-space($summaries//member[@name=$Mname]/summary/text())" />
							</div>
							</xsl:when>
						</xsl:choose>
					</td>
					<td><xsl:attribute name="id">:i18n:<xsl:value-of select="$result"/></xsl:attribute><xsl:value-of select="$result"/></td>
					<td>
						<xsl:call-template name="display-time">
							<xsl:with-param name="value" select="@time"/>
						</xsl:call-template>				
					</td>
				</tr>
				<xsl:if test="$result != &quot;Pass&quot;">
					<tr style="display: block;">
						<xsl:attribute name="id">
							<xsl:value-of select="$newid"/>
						</xsl:attribute>
						<td colspan="3" class="FailureDetail">
							<xsl:apply-templates select="./failure"/>
							<xsl:apply-templates select="./error"/>
							<xsl:apply-templates select="./reason"/>
						</td>	      
					</tr>
				</xsl:if>
			</xsl:for-each>
			</table>

		</body>
	</html>
</xsl:template>

<!-- 
		=====================================================================
		package summary header
		=====================================================================
-->
<xsl:template name="packageSummaryHeader">
	<tr class="TableHeader" valign="top">
		<td width="75%" colspan="3"><b id=":i18n:Name">Name</b></td>
		<td width="5%"><b id=":i18n:Tests">Tests</b></td>
		<td width="5%"><b id=":i18n:Errors">Errors</b></td>
		<td width="5%"><b id=":i18n:Failures">Failures</b></td>
		<td width="10%" nowrap="nowrap"><b id=":i18n:Time">Time(s)</b></td>
	</tr>
</xsl:template>

<!-- 
		=====================================================================
		classes summary header
		=====================================================================
-->
<xsl:template name="classesSummaryHeader">
	<tr class="TableHeader" valign="top">
		<td width="85%" colspan="2"><b id=":i18n:Name">Name</b></td>
		<td width="10%"><b id=":i18n:Status">Status</b></td>
		<td width="5%" nowrap="nowrap"><b id=":i18n:Time">Time(s)</b></td>
	</tr>
</xsl:template>


<!--
    format a number in to display its value in percent
    @param value the number to format
-->
<xsl:template name="display-time">
	<xsl:param name="value"/>
	<xsl:value-of select="format-number($value,'0.000')"/>
</xsl:template>

<!--
    format a number in to display its value in percent
    @param value the number to format
-->
<xsl:template name="display-percent">
	<xsl:param name="value"/>
	<xsl:value-of select="format-number($value,'0.00 %')"/>
</xsl:template>

<!-- 
		=====================================================================
		Environtment Info Report
		=====================================================================
-->
<xsl:template name="envinfo">
	<div id="blabla" style="display: none;">
		<a name="envinfo"></a>
		<h2 id=":i18n:EnvironmentInformation">Environment Information</h2>
		<table border="0" cellpadding="5" cellspacing="2" width="95%">
		<tr class="EnvInfoHeader">
		<td id=":i18n:Property">Property</td>
		<td id=":i18n:Value">Value</td>
		</tr>
		<tr class="EnvInfoRow">
		<td id=":i18n:NAntLocation">NAnt Location</td>
		<td><xsl:value-of select="$nant.filename"/></td>
		</tr>
		<tr class="EnvInfoRow">
		<td id=":i18n:NAntVersion">NAnt Version</td>
		<td><xsl:value-of select="$nant.version"/></td>
		</tr>
		<tr class="EnvInfoRow">
		<td id=":i18n:Buildfile">Buildfile</td>
		<td><xsl:value-of select="$nant.project.buildfile"/></td>
		</tr>
		<tr class="EnvInfoRow">
		<td id=":i18n:BaseDirectory">Base Directory</td>
		<td><xsl:value-of select="$nant.project.basedir"/></td>
		</tr>
		<tr class="EnvInfoRow">
		<td id=":i18n:OperatingSystem">Operating System</td>
		<td><xsl:value-of select="$sys.os"/></td>
		</tr>
		<tr class="EnvInfoRow">
		<td id=":i18n:NETCLRVersion">.NET CLR Version</td>
		<td><xsl:value-of select="$sys.clr.version"/></td>
		</tr>
		</table>	
		<hr size="1" width="95%" align="left"/>
	</div>
</xsl:template>

<xsl:template name="toggle">
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
</xsl:template>

</xsl:stylesheet>