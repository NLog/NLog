<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

  <xsl:template name="bibliographyReference">
    <xsl:param name="number" />
    <xsl:param name="data" />

    <xsl:if test="$data">
      <div class="bibliographStyle">
        <span class="bibliographyNumber">
          <xsl:attribute name="id">cite<xsl:value-of select="$number"/></xsl:attribute>
          [<xsl:value-of select="$number"/>]
        </span>
        <span class="bibliographyAuthor"><xsl:value-of select="$data/author/text()" /></span>
        <xsl:text>, </xsl:text>
        <span class="bibliographyTitle"><xsl:value-of select="$data/title/text()" /></span>
        <xsl:if test="$data/publisher">
          <xsl:text>, </xsl:text>
          <span class="bibliographyPublisher"><xsl:value-of select="$data/publisher/text()" /></span>
        </xsl:if>
        <xsl:if test="$data/link">
          <xsl:text>, </xsl:text>
          <a>
            <xsl:attribute name="target">_blank</xsl:attribute>
            <xsl:attribute name="href"><xsl:value-of select="$data/link/text()" /></xsl:attribute>
            <xsl:value-of select="$data/link/text()" />
          </a>
        </xsl:if>
      </div>
    </xsl:if>
  </xsl:template>
  
</xsl:stylesheet>
