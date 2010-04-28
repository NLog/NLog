<?xml version="1.0"?>
<xsl:stylesheet
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:msxsl="urn:schemas-microsoft-com:xslt"
	xmlns:ddue="urn:ddue-extensions"
	version="1.1">

	<msxsl:script language="C#" implements-prefix="ddue">
		<msxsl:using namespace="System.Security.Cryptography" />
		<![CDATA[
			public static string getFileName(string id) {
				HashAlgorithm md5 = HashAlgorithm.Create("MD5");
				byte[] input = Encoding.UTF8.GetBytes(id);
				byte[] output = md5.ComputeHash(input);
				Guid guid = new Guid(output);
				return( guid.ToString() );
			}
		]]>
	</msxsl:script>

	<xsl:output indent="yes" encoding="UTF-8" />
	
	<xsl:template match="/">
		<reflection>
			<xsl:apply-templates select="/reflection/assemblies" />
			<xsl:apply-templates select="/reflection/apis" />
		</reflection>
	</xsl:template>

	<xsl:template match="assemblies">
		<xsl:copy-of select="." />
	</xsl:template>

	<xsl:template match="apis">
		<apis>
			<xsl:apply-templates select="api" />
		</apis>
	</xsl:template>

	<xsl:template match="api">
		<api id="{@id}">
			<xsl:copy-of select="*" />
			<file name="{ddue:getFileName(@id)}" />
		</api>
	</xsl:template>

</xsl:stylesheet>
