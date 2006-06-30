<?xml version="1.0" encoding="windows-1250" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
    <xsl:param name="page_id_override"></xsl:param>
    <xsl:param name="subpage_id_override"></xsl:param>
    <xsl:param name="file_extension">xml</xsl:param>
    <xsl:param name="mode">web</xsl:param>

    <xsl:variable name="page_id" select="concat(/*[position()=1]/@id,$page_id_override)" />
    <xsl:variable name="subpage_id" select="concat(/*[position()=1]/@subid,$subpage_id_override)" />
    <xsl:variable name="common" select="document(concat($mode,'menu.xml'))" />
    
    <xsl:output method="xml" 
        indent="yes" 
        doctype-public="-//W3C//DTD XHTML 1.0 Transitional//EN" 
        doctype-system="http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd" />

    <xsl:template match="/">
        <html>
            <head>
                <link rel="stylesheet" href="style.css" type="text/css" />
                <title>NLog Viewer - <xsl:value-of select="$common/common/navigation/nav[@href=$page_id]/@label" />
                    <xsl:if test="$subpage_id"> - <xsl:value-of select="$common/common/navigation//subnav[@href=$subpage_id]/@label" /></xsl:if></title>
                <meta name="keywords" content="NLogViewer NLog log4j tcp udp xml viewer" />
                <meta name="author" content="Jaroslaw Kowalski" />
            </head>
            <body>
                <xsl:if test="$mode = 'web'">
                    <div id="googleads">
                    <script type="text/javascript"><xsl:comment>
google_ad_client = "pub-2535373996863248";
google_ad_width = 234;
google_ad_height = 60;
google_ad_format = "234x60_as";
google_ad_type = "text_image";
google_ad_channel ="";
google_color_border = "CCCCCC";
google_color_bg = "FFFFFF";
google_color_link = "000000";
google_color_url = "666666";
google_color_text = "333333";
                            //</xsl:comment></script>
                    <script type="text/javascript"
                        src="http://pagead2.googlesyndication.com/pagead/show_ads.js">
                    </script>
                </div>
            </xsl:if>
                <img src="NLogViewer.png" />
                <div id="controls">
                    <xsl:call-template name="controls" />
                </div>
                <div id="{$mode}content">
                    <span class="underconstruction">
                        This web site is under construction. Some sections may be missing or not up-to-date.
                    </span>
                    <xsl:apply-templates select="content" />
                </div>
                </body>
            </html>
        </xsl:template>

        <xsl:template match="@* | node()">
            <xsl:copy>
                <xsl:apply-templates select="@* | node()" />
            </xsl:copy>
        </xsl:template>

        <xsl:template match="content">
            <xsl:apply-templates />
        </xsl:template>

        <xsl:template name="controls">
            <xsl:apply-templates select="$common/common/navigation" />
        </xsl:template>

        <xsl:template match="navigation">
            <table border="0" cellpadding="0" cellspacing="0" class="navtable">
                <xsl:apply-templates select="nav" />
            </table>
        </xsl:template>

        <xsl:template match="nav">
            <xsl:choose>
                <xsl:when test="$page_id = @href and subnav">
                    <tr>
                        <td class="nav_selected">
                            <table class="submenu" cellpadding="0" cellspacing="0">
                                <tr><td>
                                        <a class="nav_selected">
                                            <xsl:attribute name="href"><xsl:value-of select="@href" />.<xsl:value-of select="$file_extension" /></xsl:attribute>
                                            <xsl:value-of select="@label" />
                                        </a>
                                </td></tr>
                                <xsl:if test="subnav">
                                    <xsl:apply-templates select="subnav" />
                                </xsl:if>
                            </table>
                        </td>
                    </tr>
                </xsl:when>
                <xsl:when test="$page_id = @href">
                    <tr>
                        <td class="nav_selected">
                            <a class="nav_selected">
                                <xsl:attribute name="href"><xsl:value-of select="@href" />.<xsl:value-of select="$file_extension" /></xsl:attribute>
                                <xsl:value-of select="@label" />
                        </a>
                    </td>
                </tr>
            </xsl:when>
            <xsl:otherwise>
                <tr><td class="nav"><a class="nav"><xsl:attribute name="href"><xsl:value-of select="@href" /><xsl:if test="not(@noext)">.<xsl:value-of select="$file_extension" /></xsl:if></xsl:attribute><xsl:value-of select="@label" /></a></td></tr>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>

    <xsl:template match="subnav">
        <xsl:choose>
            <xsl:when test="$subpage_id = @href"><tr class="subnav"><td><a class="subnav_selected" href="{@href}.{$file_extension}"><xsl:value-of select="@label" /></a></td></tr></xsl:when>
            <xsl:otherwise>
                <tr class="subnav"><td><a class="subnav"><xsl:attribute name="href"><xsl:value-of select="@href" /><xsl:if test="not(@noext)">.<xsl:value-of select="$file_extension" /></xsl:if></xsl:attribute><xsl:value-of select="@label" /></a></td></tr>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>

    <xsl:template match="link">
        <a href="{@href}.{$file_extension}"><xsl:apply-templates /></a>
    </xsl:template>

    <xsl:template match="x">
        <xsl:apply-templates mode="xml-example" />
    </xsl:template>

    <xsl:template match="commandline">
        <code class="commandline">
            <xsl:apply-templates />
        </code>
    </xsl:template>

    <!-- FAQ -->

    <xsl:template match="faq-index">
        <ol>
            <xsl:apply-templates select="//faq" mode="faq-index" />
        </ol>
    </xsl:template>

    <xsl:template match="faq-answers">
        <xsl:apply-templates select="faq" mode="faq-body" />
    </xsl:template>

    <xsl:template match="faq" mode="faq-index">
        <li><a>
                <xsl:attribute name="href">#<xsl:value-of select="@id" /></xsl:attribute>
                <xsl:value-of select="@title" />
            </a>
        </li>
    </xsl:template>

    <xsl:template match="faq" mode="faq-body">
        <hr />
        <h5>
            <a><xsl:value-of select="position()" />. 
                <xsl:attribute name="name"><xsl:value-of select="@id" /></xsl:attribute>
                <xsl:value-of select="@title" />
            </a>
        </h5>
        <xsl:apply-templates />
    </xsl:template>
</xsl:stylesheet>
