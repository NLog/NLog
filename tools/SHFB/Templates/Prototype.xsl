<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.1" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt"
    xmlns:MSHelp="http://msdn.microsoft.com/mshelp">

<!--
// System  : Sandcastle Help File Builder Utilities
// File    : Prototype.xsl
// Author  : Eric Woodruff
// Updated : 03/15/2008
// Note    : Copyright 2007-2008, Eric Woodruff, All rights reserved
//
// This is used to convert *.topic additional content files into *.html files
// that have the same appearance as API topics using the Prototype presentation
// style.
-->

  <xsl:output method="xml" omit-xml-declaration="yes" encoding="utf-8" />

  <!-- This parameter, if specified, defines the path to the root folder -->
  <xsl:param name="pathToRoot" select="string('')" />

  <!-- Allow for alternate header text -->
  <xsl:variable name="customHeader">
    <xsl:choose>
      <xsl:when test="topic/headerTitle">
        <xsl:value-of select="topic/headerTitle"/>
      </xsl:when>
      <xsl:otherwise>
        <!-- The product title is replaced with the project's HTML encoded HelpTitle value -->
        <@HtmlEncHelpTitle/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <!-- Main template for the topic -->
  <xsl:template match="/topic">
<html>
<head>
<title><xsl:value-of select="title"/></title>
<link rel="stylesheet" type="text/css" href="{$pathToRoot}styles/presentation.css" />
<link rel="stylesheet" type="text/css" href="ms-help://Hx/HxRuntime/HxLink.css" />
<META HTTP-EQUIV="Content-Type" CONTENT="text/html; charset=UTF-8" />
<script type="text/javascript" src="{$pathToRoot}scripts/SplitScreen.js"></script>
<script type="text/javascript" src="{$pathToRoot}scripts/EventUtilities.js"></script>

<!-- Stylesheet and script for colorized code blocks -->
<link type="text/css" rel="stylesheet" href="{$pathToRoot}styles/highlight.css" />
<script type="text/javascript" src="{$pathToRoot}scripts/highlight.js"></script>

<xml>
    <MSHelp:Attr Name="DocSet" Value="NetFramework" />
    <MSHelp:Attr Name="DocSet" Value="<@HtmlHelpName>" />
    <MSHelp:Attr Name="Locale" Value="<@Locale>" />
    <MSHelp:Attr Name="TargetOS" Value="Windows" />

<!-- Include the user's XML data island for MS Help 2.0 if present -->
<xsl:if test="xml">
    <xsl:copy-of select="xml/*"/>
</xsl:if>
</xml>

<!-- Add a link to an additional stylesheet if specified -->
<xsl:if test="styleSheet">
    <link rel="stylesheet" type="text/css">
      <xsl:attribute name="href">
        <xsl:value-of select="$pathToRoot"/>
        <xsl:value-of select="styleSheet/@filename"/>
      </xsl:attribute>
    </link>
</xsl:if>
</head>

<body>
<script type="text/javascript">
registerEventHandler(window, 'load', function() {
    var ss = new SplitScreen('control', 'main');
});
</script>

<div id="control">
<!-- Include the logo if present -->
<xsl:choose>
  <xsl:when test="logoFile">
    <xsl:apply-templates select="logoFile"/>
  </xsl:when>
  <xsl:otherwise>
<table border="0" width="100%" cellpadding="0" cellspacing="0">
  <tr>
    <td valign="top" width="100%"><span class="productTitle"><xsl:value-of select="$customHeader"/></span><br /><span class="topicTitle"><xsl:value-of select="title"/></span><br /></td>
  </tr>
</table>
  </xsl:otherwise>
</xsl:choose>
</div>

<div id="main">

<!-- Process the body text -->
<xsl:apply-templates select="bodyText" />

<br/><br/>

<!-- This includes the footer item from the shared content -->
<include item="footer"/>

</div>
</body>
</html>
  </xsl:template>

  <!-- Pass through html tags from the body -->
  <xsl:template match="p|ol|ul|li|dl|dt|dd|table|tr|th|td|h1|h2|h3|h4|h5|h6|hr|br|pre|blockquote|div|span|a|img|b|i|strong|em|del|sub|sup|abbr|acronym|u|font|link|script|code|map|area">
    <xsl:copy>
      <xsl:copy-of select="@*" />
      <xsl:apply-templates />
    </xsl:copy>
  </xsl:template>

  <!-- Add the logo -->
  <xsl:template match="logoFile">
    <xsl:choose>
      <xsl:when test="@placement='above'">
<table border="0" width="100%" cellpadding="0" cellspacing="0">
  <tr>
    <td style="padding-bottom: 5px">
      <xsl:if test="@alignment">
        <xsl:attribute name="align">
          <xsl:value-of select="@alignment"/>
        </xsl:attribute>
      </xsl:if>
    <img>
      <xsl:attribute name="src">
        <xsl:value-of select="$pathToRoot"/>
        <xsl:value-of select="@filename"/>
      </xsl:attribute>
      <xsl:attribute name="altText">
        <xsl:value-of select="@altText"/>
      </xsl:attribute>
      <xsl:if test="@height">
        <xsl:attribute name="height">
          <xsl:value-of select="@height"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@width">
        <xsl:attribute name="width">
          <xsl:value-of select="@width"/>
        </xsl:attribute>
      </xsl:if>
    </img></td>
  </tr>
  <tr>
    <td valign="top" width="100%"><span class="productTitle"><xsl:value-of select="$customHeader"/></span><br /><span class="topicTitle"><xsl:value-of select="parent::*/title"/></span><br /></td>
  </tr>
</table>
      </xsl:when>
      <xsl:when test="@placement='right'">
<table border="0" width="100%" cellpadding="0" cellspacing="0">
  <tr>
    <td valign="top" width="100%"><span class="productTitle"><xsl:value-of select="$customHeader"/></span><br /><span class="topicTitle"><xsl:value-of select="parent::*/title"/></span><br /></td>
    <td align="center" style="padding-left: 10px"><img>
      <xsl:attribute name="src">
        <xsl:value-of select="$pathToRoot"/>
        <xsl:value-of select="@filename"/>
      </xsl:attribute>
      <xsl:attribute name="altText">
        <xsl:value-of select="@altText"/>
      </xsl:attribute>
      <xsl:if test="@height">
        <xsl:attribute name="height">
          <xsl:value-of select="@height"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@width">
        <xsl:attribute name="width">
          <xsl:value-of select="@width"/>
        </xsl:attribute>
      </xsl:if>
    </img></td>
  </tr>
</table>
      </xsl:when>
      <xsl:otherwise>
<table border="0" width="100%" cellpadding="0" cellspacing="0">
  <tr>
    <td align="center" style="padding-right: 10px"><img>
      <xsl:attribute name="src">
        <xsl:value-of select="$pathToRoot"/>
        <xsl:value-of select="@filename"/>
      </xsl:attribute>
      <xsl:attribute name="altText">
        <xsl:value-of select="@altText"/>
      </xsl:attribute>
      <xsl:if test="@height">
        <xsl:attribute name="height">
          <xsl:value-of select="@height"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@width">
        <xsl:attribute name="width">
          <xsl:value-of select="@width"/>
        </xsl:attribute>
      </xsl:if>
    </img></td>
    <td valign="top" width="100%"><span class="productTitle"><xsl:value-of select="$customHeader"/></span><br /><span class="topicTitle"><xsl:value-of select="parent::*/title"/></span><br /></td>
  </tr>
</table>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

</xsl:stylesheet>
