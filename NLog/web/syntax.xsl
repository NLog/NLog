<?xml version="1.0" encoding="windows-1250" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
    <xsl:template match="cs[@src]">
        <p/>
        <a href="{@src}.html" style="display:none">A</a>
        <iframe src="{@src}.html" style="width: 100%; border: 1px solid #c0c0c0">
        </iframe>
        <br />
        <a href="{@src}">Download this file</a>
        <p/>
    </xsl:template>

    <xsl:template match="js[@src]">
        <p/>
        <a href="{@src}.html" style="display:none">A</a>
        <iframe src="{@src}.html" style="width: 100%; border: 1px solid #c0c0c0">
        </iframe>
        <br/>
        <a href="{@src}">Download this file</a>
        <p/>
    </xsl:template>

    <xsl:template match="xml[@src]">
        <p/>
        <a href="{@src}.html" style="display:none">A</a>
        <iframe src="{@src}.html" style="width: 100%; border: 1px solid #c0c0c0">
        </iframe>
        <br/>
        <a href="{@src}">Download this file</a>
        <p/>
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
        <p/>
        <a href="{@src}.html" style="display:none">A</a>
        <iframe src="{@src}.html" style="width: 100%; border: 1px solid #c0c0c0">
        </iframe>
        <br/>
        <a href="{@src}">Download this file</a>
        <p/>
    </xsl:template>
    
    <xsl:template match="js[@src]" mode="slashdoc">
        <p/>
        <a href="{@src}.html" style="display:none">A</a>
        <iframe src="{@src}.html" style="width: 100%; border: 1px solid #c0c0c0">
        </iframe>
        <br/>
        <a href="{@src}">Download this file</a>
        <p/>
    </xsl:template>
    
    <xsl:template match="cs[@src]" mode="slashdoc">
        <p/>
        <a href="{@src}.html" style="display:none">A</a>
        <iframe src="{@src}.html" style="width: 100%; border: 1px solid #c0c0c0">
        </iframe>
        <br/>
        <a href="{@src}">Download this file</a>
        <p/>
    </xsl:template>
</xsl:stylesheet>
