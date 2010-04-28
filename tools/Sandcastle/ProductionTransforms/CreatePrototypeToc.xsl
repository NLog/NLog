<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.1">

  <xsl:param name="segregated" select="false()" />
  
	<xsl:output indent="yes" />

	<xsl:key name="index" match="/reflection/apis/api" use="@id" />

	<xsl:template match="/">
		<topics>
			<xsl:choose>
				<xsl:when test="count(/reflection/apis/api[apidata/@group='root']) > 0">
					<xsl:apply-templates select="/reflection/apis/api[apidata/@group='root']" />
				</xsl:when>
				<xsl:when test="count(/reflection/apis/api[apidata/@group='namespace']) > 0">
					<xsl:apply-templates select="/reflection/apis/api[apidata/@group='namespace']">
						<xsl:sort select="apidata/@name" />
					</xsl:apply-templates>
				</xsl:when>
				<xsl:otherwise>
					<xsl:apply-templates select="/reflection/apis/api[apidata/@group='type']">
						<xsl:sort select="apidata/@name" />
					</xsl:apply-templates>
				</xsl:otherwise>
			</xsl:choose>
		</topics>
	</xsl:template>

	<!-- create a root entry and namespace sub-entries -->
	<xsl:template match="api[apidata/@group='root']">
		<topic id="{@id}" file="{file/@name}">
			<xsl:apply-templates select="key('index',elements/element/@api)">
				<xsl:sort select="apidata/@name" />
			</xsl:apply-templates>
		</topic>
	</xsl:template>


	<!-- for each namespace, create namespace entry and type sub-entries -->
	<xsl:template match="api[apidata/@group='namespace']">
		<topic id="{@id}" file="{file/@name}">
			<xsl:apply-templates select="key('index',elements/element/@api)">
                <xsl:sort select="@id" />
			</xsl:apply-templates>
		</topic>
	</xsl:template>

	<!-- for each type, create type entry and either overload entries or member entries as sub-entries -->
  <xsl:template match="api[apidata/@group='type']">
    <xsl:choose>
      <xsl:when test="$segregated">
        <stopic id="{@id}" project="{containers/library/@assembly}" file="{file/@name}">
          <xsl:call-template name="processType" />
        </stopic>
      </xsl:when>
      <xsl:otherwise>
        <topic id="{@id}" file="{file/@name}">
          <xsl:call-template name="processType" />
        </topic>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="processType">
			<xsl:variable name="typeId" select="@id" />
			<xsl:variable name="members" select="key('index',elements/element/@api)[containers/type/@api=$typeId]" />
			<xsl:for-each select="$members">
				<xsl:sort select="apidata/@name" />
				<xsl:variable name="name" select="apidata/@name" />
				<xsl:variable name="subgroup" select="apidata/@subgroup" />
				<xsl:variable name="set" select="$members[apidata/@name=$name and apidata/@subgroup=$subgroup]" />
				<xsl:choose>
					<xsl:when test="count($set) &gt; 1">
						<xsl:if test="($set[1]/@id)=@id">
							<xsl:variable name="overloadId">
                                <xsl:value-of select="overload/@api" />
							</xsl:variable>
              <xsl:choose>
                <xsl:when test="$segregated">
                  <stopic id="{@id}" project="{containers/library/@assembly}" file="{key('index',$overloadId)/file/@name}">
                    <xsl:for-each select="$set">
                      <xsl:apply-templates select="." />
                    </xsl:for-each>
                  </stopic>
                </xsl:when>
                <xsl:otherwise>
                  <topic id="{@id}" file="{key('index',$overloadId)/file/@name}">
                    <xsl:for-each select="$set">
                      <xsl:apply-templates select="." />
                    </xsl:for-each>
                  </topic>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:if>
					</xsl:when>
					<xsl:otherwise>
						<xsl:apply-templates select="." />
					</xsl:otherwise>
				</xsl:choose>
			</xsl:for-each>
	</xsl:template>

	<!-- for each member, create a leaf entry -->
  <xsl:template match="api[apidata/@group='member']">
    <xsl:choose>
      <xsl:when test="$segregated">
        <stopic id="{@id}" project="{containers/library/@assembly}" file="{file/@name}" />
      </xsl:when>
      <xsl:otherwise>
        <topic id="{@id}" file="{file/@name}" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

	<xsl:template name="overloadId">
		<xsl:param name="memberId" />
		<xsl:text>Overload:</xsl:text>
		<xsl:variable name="noParameters">
			<xsl:choose>
				<xsl:when test="contains($memberId,'(')">
					<xsl:value-of select="substring-before($memberId,'(')" />
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="$memberId" />
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="noGeneric">
			<xsl:choose>
				<xsl:when test="contains($noParameters,'``')">
					<xsl:value-of select="substring-before($noParameters,'``')" />
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="$noParameters" />
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:value-of select="substring($noGeneric,3)" />
	</xsl:template>

</xsl:stylesheet>
