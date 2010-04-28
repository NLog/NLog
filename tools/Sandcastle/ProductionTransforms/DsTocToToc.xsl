<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.1">

	<xsl:output indent="yes" />

	<xsl:template match="/">
		<topics>
			<xsl:apply-templates select="/*" />
		</topics>
	</xsl:template>

	<xsl:template match="topic">
    <topic id="{@id}">
      <xsl:if test="not(@isCategoryOnly='True')">
        <xsl:attribute name="file">
          <xsl:value-of select="@id" />
        </xsl:attribute>
      </xsl:if>
      <xsl:apply-templates />
    </topic>
 	</xsl:template>

  <xsl:template match="sharedTOC|sharedManagedReferenceTOC">
    <stoc project="{@projectName}" />
  </xsl:template>

  <xsl:template match="sharedTopic">
    <stopic project="{@projectName}" id="{@id}" file="{@id}">
      <xsl:apply-templates />
    </stopic>
  </xsl:template>

</xsl:stylesheet>
