<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.1" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:MSHelp="http://msdn.microsoft.com/mshelp"
    xmlns:mshelp="http://msdn.microsoft.com/mshelp"
    xmlns:ddue="http://ddue.schemas.microsoft.com/authoring/2003/5"
    xmlns:xlink="http://www.w3.org/1999/xlink"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt">

<!--
// System  : Sandcastle Help File Builder Utilities
// File    : Hana.xsl
// Author  : Eric Woodruff
// Updated : 03/15/2008
// Note    : Copyright 2007-2008, Eric Woodruff, All rights reserved
//
// This is used to convert *.topic additional content files into *.html files
// that have the same appearance as API topics using the Hana presentation
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
<link rel="stylesheet" type="text/css" href="ms-help://Dx/DxRuntime/DxLink.css" />
<script type="text/javascript" src="{$pathToRoot}scripts/EventUtilities.js"></script>
<script type="text/javascript" src="{$pathToRoot}scripts/SplitScreen.js"></script>
<script type="text/javascript" src="{$pathToRoot}scripts/Dropdown.js"></script>
<script type="text/javascript" src="{$pathToRoot}scripts/script_manifold.js"></script>
<script type="text/javascript" src="{$pathToRoot}scripts/LanguageFilter.js"></script>
<script type="text/javascript" src="{$pathToRoot}scripts/DataStore.js"></script>
<script type="text/javascript" src="{$pathToRoot}scripts/CommonUtilities.js"></script>
<script type="text/javascript" src="{$pathToRoot}scripts/MemberFilter.js"></script>
<META HTTP-EQUIV="Content-Type" CONTENT="text/html; charset=UTF-8" />
<META NAME="save" CONTENT="history" />

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
<input type="hidden" id="userDataCache" class="userDataStyle" />
<input type="hidden" id="hiddenScrollOffset" />
<img id="collapseImage" style="display:none; height:0; width:0;" src="{$pathToRoot}icons/collapse_all.gif" alt="Collapse image"/>
<img id="expandImage" style="display:none; height:0; width:0;" src="{$pathToRoot}icons/expand_all.gif" alt="Expand Image"/>
<img id="collapseAllImage" style="display:none; height:0; width:0;" src="{$pathToRoot}icons/collall.gif" alt="CollapseAll image"/>
<img id="expandAllImage" style="display:none; height:0; width:0;" src="{$pathToRoot}icons/expall.gif" alt="ExpandAll image"/>
<img id="dropDownImage" style="display:none; height:0; width:0;" src="{$pathToRoot}icons/twirl_unselected.gif" alt="DropDown image"/>
<img id="dropDownHoverImage" style="display:none; height:0; width:0;" src="{$pathToRoot}icons/twirl_unselected_hover.gif" alt="DropDownHover image"/>
<img id="copyImage" style="display:none; height:0; width:0;" src="{$pathToRoot}icons/copycode.gif" alt="Copy image"/>
<img id="copyHoverImage" style="display:none; height:0; width:0;" src="{$pathToRoot}icons/copycodeHighlight.gif" alt="CopyHover image"/>
<img id="checkBoxSelectImage" style="display:none; height:0; width:0;" src="{$pathToRoot}icons/ch_selected.gif"/>
<img id="checkBoxUnSelectImage" style="display:none; height:0; width:0;" src="{$pathToRoot}icons/ch_unselected.gif"/>
<img id="checkBoxSelectHoverImage" style="display:none; height:0; width:0;" src="{$pathToRoot}icons/ch_selected_hover.gif"/>
<img id="checkBoxUnSelectHoverImage" style="display:none; height:0; width:0;" src="{$pathToRoot}icons/ch_unselected_hover.gif"/>
<img id="radioSelectImage" style="display:none; height:0; width:0;" src="{$pathToRoot}icons/r_select.gif"/>
<img id="radioUnSelectImage" style="display:none; height:0; width:0;" src="{$pathToRoot}icons/r_unselect.gif"/>
<img id="radioSelectHoverImage" style="display:none; height:0; width:0;" src="{$pathToRoot}icons/r_select_hover.gif"/>
<img id="radioUnSelectHoverImage" style="display:none; height:0; width:0;" src="{$pathToRoot}icons/r_unselect_hover.gif"/>
<img id="curvedLeftSelectImage" style="display:none; height:0; width:0;" src="{$pathToRoot}icons/tab_sel_lft_cnr.gif"/>
<img id="curvedRightSelectImage" style="display:none; height:0; width:0;" src="{$pathToRoot}icons/tab_sel_rt_cnr.gif"/>
<img id="curvedLeftUnSelectImage" style="display:none; height:0; width:0;" src="{$pathToRoot}icons/tab_unsel_lft_cnr.gif"/>
<img id="curvedRightUnSelectImage" style="display:none; height:0; width:0;" src="{$pathToRoot}icons/tab_unsel_rt_cnr.gif"/>
<img id="gradLeftSelectImage" style="display:none; height:0; width:0;" src="{$pathToRoot}icons/tab_sel_lft_grad.gif"/>
<img id="gradRightSelectImage" style="display:none; height:0; width:0;" src="{$pathToRoot}icons/tab_sel_rt_grad.gif"/>
<img id="gradLeftUnSelectImage" style="display:none; height:0; width:0;" src="{$pathToRoot}icons/tab_unsel_lft_grad.gif"/>
<img id="gradRightUnSelectImage" style="display:none; height:0; width:0;" src="{$pathToRoot}icons/tab_unsel_rt_grad.gif"/>
<img id="twirlSelectImage" style="display:none; height:0; width:0;" src="{$pathToRoot}icons/twirl_selected.gif"/>
<img id="twirlUnSelectImage" style="display:none; height:0; width:0;" src="{$pathToRoot}icons/twirl_unselected.gif"/>
<img id="twirlSelectHoverImage" style="display:none; height:0; width:0;" src="{$pathToRoot}icons/twirl_selected_hover.gif"/>
<img id="twirlUnSelectHoverImage" style="display:none; height:0; width:0;" src="{$pathToRoot}icons/twirl_unselected_hover.gif"/>
<img id="NSRBottomImage" style="display:none; height:0; width:0;" src="{$pathToRoot}icons/NSRbottomgrad.gif"/>

<div id="header">
<!-- Include the logo if present -->
<xsl:choose>
  <xsl:when test="logoFile">
    <xsl:apply-templates select="logoFile"/>
  </xsl:when>
  <xsl:otherwise>
<table id="topTable">
  <tr id="headerTableRow1">
    <td align="left"><span id="runningHeaderText"><xsl:value-of select="$customHeader"/></span></td>
  </tr>
  <tr id="headerTableRow2">
    <td align="left"><span id="nsrTitle"><xsl:value-of select="title"/></span></td>
  </tr>
  <tr>
    <td class="nsrBottom" background="{$pathToRoot}icons/NSRbottomgrad.gif"></td>
  </tr>
</table>
  </xsl:otherwise>
</xsl:choose>
</div>

<div id="mainSection">
<div id="mainBody">
<div id="allHistory" class="saveHistory" onsave="saveAll()" onload="loadAll()" />

<!-- Process the body text -->
<xsl:apply-templates select="bodyText" />

<div id="footer">
<div class="footerLine"><img width="100%" height="3px" src="{$pathToRoot}icons/footer.gif" alt="Footer image"/></div>

<!-- This includes the footer item from the shared content -->
<include item="footer"/>
</div>
</div>
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
<table id="topTable">
  <tr>
    <td>
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
  <tr id="headerTableRow1">
    <td align="left"><span id="runningHeaderText"><xsl:value-of select="$customHeader"/></span></td>
  </tr>
  <tr id="headerTableRow2">
    <td align="left"><span id="nsrTitle"><xsl:value-of select="parent::*/title"/></span></td>
  </tr>
  <tr>
    <td class="nsrBottom" background="{$pathToRoot}icons/NSRbottomgrad.gif"></td>
  </tr>
</table>
      </xsl:when>
      <xsl:when test="@placement='right'">
<table id="topTable">
  <tr id="headerTableRow1">
    <td align="left"><span id="runningHeaderText"><xsl:value-of select="$customHeader"/></span></td>
    <td rowspan="2" align="center" style="width: 1px; padding: 0px"><img>
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
  <tr id="headerTableRow2">
    <td align="left"><span id="nsrTitle"><xsl:value-of select="parent::*/title"/></span></td>
  </tr>
  <tr>
    <td class="nsrBottom" colspan="2" background="{$pathToRoot}icons/NSRbottomgrad.gif"></td>
  </tr>
</table>
      </xsl:when>
      <xsl:otherwise>
<table id="topTable">
  <tr>
    <td rowspan="3" align="center" style="width: 1px; padding: 0px"><img>
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
  <tr id="headerTableRow1">
    <td align="left"><span id="runningHeaderText"><xsl:value-of select="$customHeader"/></span></td>
  </tr>
  <tr id="headerTableRow2">
    <td align="left"><span id="nsrTitle"><xsl:value-of select="parent::*/title"/></span></td>
  </tr>
  <tr>
    <td class="nsrBottom" colspan="2" background="{$pathToRoot}icons/NSRbottomgrad.gif"></td>
  </tr>
</table>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

</xsl:stylesheet>
