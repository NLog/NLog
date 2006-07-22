<?xml version="1.0" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
    <xsl:variable name="framework_count" select="count(/results/framework)" />
    <xsl:variable name="scale_max" select="/results/scale/@max" />

    <xsl:template match="/">
        <html>
            <head>
                <title></title>
                <style>
                    body
                    {
                    font-family: Tahoma;
                    font-size: 80%;
                    }

                    table
                    {
                    font-size: 100%;
                    }

                    tr.title
                    {
                    background-color: white;
                    font-weight: bold;
                    background-color: #c0c0ff;
                    }
                    tr.graph
                    {
                    background-color: #e0e0ff;
                    }

                    td
                    {
                    padding: 2px;
                    }
                </style>
            </head>
            <body>
                <table border="0" cellspacing="0" width="100%">
                    <xsl:for-each select="/results/framework[position()=1]/test">
                        <xsl:variable name="timing_name" select="@name" />

                        <xsl:apply-templates select="//test[@name=$timing_name]">
                        </xsl:apply-templates>
                    </xsl:for-each>
                </table>
            </body>
        </html>

    </xsl:template>

    <xsl:template match="test">
        <xsl:if test="position() = 1">
            <tr class="title">
                <td colspan="3">
                    <xsl:value-of select="@name" />
                </td>
            </tr>
        </xsl:if>
        <tr class="graph">
            <td width="1%">
                <xsl:value-of select="../@name" />
            </td>
            <td width="1%">
                <xsl:value-of select="@avg" />
            </td>
            <td width="100%">
                <xsl:variable name="pixels" select="100 * (@avg div $scale_max)" />
                <div style="border: 1px solid #606060; width: {$pixels}%; height: 12px">
                    <img src="bar_{../@name}.png" style="width: 100%; height: 12px" />
                </div>
            </td>
        </tr>
    </xsl:template>
</xsl:stylesheet>
