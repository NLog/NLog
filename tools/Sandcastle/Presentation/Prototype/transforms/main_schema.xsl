<?xml version="1.0" encoding="utf-8"?>

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

  <xsl:template match="/">
    <html>
      <head>
        <title>
          <xsl:value-of select="/element/@name" />
        </title>
        <meta name="file" content="{/element/@name}" />
      </head>    
      <body>
        <xsl:call-template name="syntax" />
        <xsl:call-template name="attributes" />
      </body>
    </html>
  </xsl:template>

  <xsl:template name="syntax">
    &lt;<span class="identifier"><xsl:value-of select="/element/@name"/></span>&gt;
  </xsl:template>

  <xsl:template name="attributes">
    <table>
      <tr>
        <th>Name</th>
        <th>Type</th>
        <th>Description</th>
      </tr>
      <tr>
        <xsl:for-each select="/element/attributes/attribute">
          <td>
            <xsl:value-of select="@name" />
          </td>
          <td>
            <xsl:value-of select="type/@name" />
          </td>
        </xsl:for-each>
      </tr>
    </table>
  </xsl:template>

</xsl:stylesheet> 
