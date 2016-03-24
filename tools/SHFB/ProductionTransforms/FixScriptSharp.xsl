<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0">

	<xsl:output indent="yes" encoding="UTF-8" />

	<xsl:template match="node() | @*">
		<xsl:copy>
			<xsl:apply-templates select="node() | @*" />
		</xsl:copy>
	</xsl:template>

	<!-- EFW - Add a "scriptSharp" element to each API node so that the
         JavaScript syntax generator will apply the casing rules to the
         member name. -->
	<xsl:template match="api">
		<xsl:copy>
			<xsl:apply-templates select="node() | @*" />
			<scriptSharp />
		</xsl:copy>
	</xsl:template>

	<!-- Fix subgroups for enumerations -->
	<xsl:template match="reflection/apis/api[apidata/@group='type' and family/ancestors/type/@api='T:System.Enum']/apidata/@subgroup">
		<xsl:attribute name="subgroup">
			<xsl:value-of select="'enumeration'"/>
		</xsl:attribute>
	</xsl:template>

	<!-- Strip ancestors from enumerations -->
	<xsl:template match="reflection/apis/api[apidata/@group='type' and family/ancestors/type/@api='T:System.Enum']/family">
	</xsl:template>

	<!-- Strip invalid members from enumerations -->
	<xsl:template match="reflection/apis/api[apidata/@group='type' and family/ancestors/type/@api='T:System.Enum']/elements">
		<elements>
			<xsl:for-each select="element">
				<xsl:if test="(starts-with(@api, 'F:') and not(contains(@api, 'value__')))">
					<xsl:copy>
						<xsl:apply-templates select="node() | @*" />
					</xsl:copy>
				</xsl:if>
			</xsl:for-each>
		</elements>
	</xsl:template>

	<!-- Fix subgroups for enumerations -->
	<xsl:template match="reflection/apis/api[apidata/@group='type' and family/ancestors/type/@api='T:System.MulticastDelegate']/apidata/@subgroup">
		<xsl:attribute name="subgroup">
			<xsl:value-of select="'delegate'"/>
		</xsl:attribute>
	</xsl:template>

	<!-- Strip ancestors from delegates -->
	<xsl:template match="reflection/apis/api[apidata/@group='type' and family/ancestors/type/@api='T:System.MulticastDelegate']/family">
	</xsl:template>

	<!-- Strip elements from delegates -->
	<xsl:template match="reflection/apis/api[apidata/@group='type' and family/ancestors/type/@api='T:System.MulticastDelegate']/elements">
	</xsl:template>

	<!-- Insert parameters into delegates -->
	<xsl:template match="reflection/apis/api[apidata/@group='type' and family/ancestors/type/@api='T:System.MulticastDelegate']/apidata">
		<xsl:copy>
			<xsl:apply-templates select="node() | @*" />
		</xsl:copy>
		<xsl:variable name="id" select="../@id" />
		<xsl:copy-of select="/reflection/apis/api[starts-with(@id, concat('M:', substring-after($id, 'T:'), '.Invoke('))]/parameters">
		</xsl:copy-of>
	</xsl:template>

	<!-- Annotate members whose types have the GlobalMethodsAttribute -->
	<xsl:template match="reflection/apis/api[apidata/@group='member']/apidata/@name">
			<xsl:copy>
				<xsl:apply-templates select="node() | @*" />
			</xsl:copy>
			<xsl:variable name="type" select="../../containers/type/@api" />
			<xsl:if test="/reflection/apis/api[@id=$type]/attributes/attribute/type/@api='T:System.GlobalMethodsAttribute'">
				<xsl:attribute name="global">
					<xsl:value-of select="'true'"/>
				</xsl:attribute>
			</xsl:if>
	</xsl:template>

	<!-- Annotate constructors whose types have the RecordAttribute -->
	<xsl:template match="reflection/apis/api[apidata/@group='member']/apidata[@subgroup='constructor']/@subgroup">
			<xsl:copy>
				<xsl:apply-templates select="node() | @*" />
			</xsl:copy>
			<xsl:variable name="type" select="../../containers/type/@api" />
			<xsl:if test="/reflection/apis/api[@id=$type]/attributes/attribute/type/@api='T:System.RecordAttribute'">
				<xsl:attribute name="record">
					<xsl:value-of select="'true'"/>
				</xsl:attribute>
			</xsl:if>
	</xsl:template>

</xsl:stylesheet>
