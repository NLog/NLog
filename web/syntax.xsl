<?xml version="1.0" encoding="windows-1250" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

    <xsl:template name="external-iframe">
        <p/>
        <a href="{@src}.html" style="display:none">A</a>
        <script language="JavaScript">
        function enlarge_iframe_<xsl:value-of select="generate-id(.)" />()
        {
            try
            {
                var f = document.getElementById('<xsl:value-of select="generate-id(.)" />');
                var f2 = window.frames[f.name];
                var h;
                if (!document.all)
                {
                    h = f2.document.body.offsetHeight + 15;
                }
                else
                {
                    h = f2.document.body.scrollHeight + 15;
                }
                // alert('enlarge_iframe_' + f.name + " h = " + h);
                f.style.height = h + "px";
            }
            catch (e)
            {
                alert(e.description);
            }
        }
        </script>

        <table cellpadding="0" cellspacing="0" style="width: 95%;" border="0">
            <tr>
                <td valign="bottom">
                    <iframe src="{@src}.html" onload='enlarge_iframe_{generate-id(.)}()'>
                        <xsl:attribute name="style">width: 100%; height: 200px; border: 1px solid #c0c0c0</xsl:attribute>
                        <xsl:attribute name="id"><xsl:value-of select="generate-id(.)" /></xsl:attribute>
                        <xsl:attribute name="name"><xsl:value-of select="generate-id(.)" /></xsl:attribute>
                    </iframe>
                </td>
            </tr>
            <tr>
                <td style="font-size: 12px"><a href="{@src}">Download this file</a></td>
            </tr>
        </table>
        <p/>
    </xsl:template>

    <xsl:template match="cs[@src]">
        <xsl:call-template name="external-iframe" />
    </xsl:template>

    <xsl:template match="js[@src]">
        <xsl:call-template name="external-iframe" />
    </xsl:template>

    <xsl:template match="xml[@src]">
        <xsl:call-template name="external-iframe" />
    </xsl:template>

    <xsl:template match="x">
        <xsl:apply-templates mode="xml-example" />
    </xsl:template>

    <xsl:template match="link">
        <a href="{@href}.{$file_extension}"><xsl:apply-templates /></a>
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
    <xsl:template match="@*" mode="xml-example">
        <span class="xmlattribute"><xsl:text> </xsl:text><xsl:value-of select="name()"/></span>
        <span class="xmlpunct">=</span><span class="xmlattribtext">"<xsl:value-of select="." />"</span>
    </xsl:template>

    <xsl:template match="comment()" mode="xml-example">
        <span class="xmlcomment">&lt;!--<xsl:value-of select="." />--&gt;</span>
    </xsl:template>
    <xsl:template match="node()" mode="xml-example" priority="-10">
        <xsl:copy>
            <xsl:apply-templates mode="xml-example" />
        </xsl:copy>
    </xsl:template>

    <xsl:template match="xml[@src]" mode="slashdoc">
        <xsl:call-template name="external-iframe" />
    </xsl:template>
    
    <xsl:template match="js[@src]" mode="slashdoc">
        <xsl:call-template name="external-iframe" />
    </xsl:template>
    
    <xsl:template match="cs[@src]" mode="slashdoc">
        <xsl:call-template name="external-iframe" />
    </xsl:template>
</xsl:stylesheet>
