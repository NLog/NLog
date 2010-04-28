<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.1"
		xmlns:ddue="http://ddue.schemas.microsoft.com/authoring/2003/5"
		xmlns:MSHelp="http://msdn.microsoft.com/mshelp" >

  <!-- stuff specific to comments authored in DDUEXML -->

	<xsl:include href="utilities_reference.xsl" />
	<xsl:include href="utilities_dduexml.xsl" />

	<xsl:variable name="summary" select="normalize-space(/document/comments/ddue:dduexml/ddue:summary)" />

	<xsl:template name="body">

    <!--internalOnly boilerplate -->
    <xsl:call-template name="internalOnly"/>
    <!-- obsolete boilerplate -->
    <xsl:if test="/document/reference/attributes/attribute/type[@api='T:System.ObsoleteAttribute']">
      <xsl:call-template name="obsoleteSection" />
    </xsl:if>
    <!-- HostProtectionAttribute boilerplate -->
    <xsl:if test="/document/reference/attributes/attribute/type[@api='T:System.Security.Permissions.HostProtectionAttribute']">
      <p><include item="hostProtectionAttributeLong" /></p>
    </xsl:if>
		<!-- summary -->
    <span sdata="authoredSummary">
		<xsl:apply-templates select="/document/comments/ddue:dduexml/ddue:summary" />
    </span>
		<!-- syntax -->
		<xsl:apply-templates select="/document/syntax" />
    <xsl:apply-templates select="/document/usyntax" />
    <!-- generic templates -->
		<xsl:apply-templates select="/document/templates" />
		<!-- parameters & return value -->
		<xsl:apply-templates select="/document/reference/templates" />
		<xsl:apply-templates select="/document/reference/parameters" />
		<xsl:apply-templates select="/document/reference/returns" />
		<!-- members -->
		<xsl:choose>
			<xsl:when test="$tgroup='root'">
				<xsl:apply-templates select="/document/reference/elements" mode="root" />
			</xsl:when>
			<xsl:when test="$group='namespace'">
				<xsl:apply-templates select="/document/reference/elements" mode="namespace" />
			</xsl:when>
			<xsl:when test="$subgroup='enumeration'">
				<xsl:apply-templates select="/document/reference/elements" mode="enumeration" />
			</xsl:when>
			<xsl:when test="$group='type'">
				<xsl:apply-templates select="/document/reference/elements" mode="type" />
			</xsl:when>
			<xsl:when test="$group='member'">
				<xsl:apply-templates select="/document/reference/elements" mode="overload" />
			</xsl:when>
		</xsl:choose>
		<!-- remarks -->
		<xsl:apply-templates select="/document/comments/ddue:dduexml/ddue:remarks" />
		<!-- example -->
		<xsl:apply-templates select="/document/comments/ddue:dduexml/ddue:codeExamples" />
		<!-- other comment sections -->
		<xsl:apply-templates select="/document/comments/ddue:dduexml/ddue:threadSaftey" />
		<xsl:apply-templates select="/document/comments/ddue:dduexml/ddue:notesForImplementers" />
		<xsl:apply-templates select="/document/comments/ddue:dduexml/ddue:notesForCallers" />
		<!-- permissions -->
		<xsl:apply-templates select="/document/comments/ddue:dduexml/ddue:permissions" />
		<!-- exceptions -->
		<xsl:apply-templates select="/document/comments/ddue:dduexml/ddue:exceptions" />
		<!-- inheritance -->
		<xsl:apply-templates select="/document/reference/family" />
    <!-- interface implementors -->
    <xsl:apply-templates select="/document/reference/implementors" />
    <!-- versioning -->
    <xsl:apply-templates select="/document/reference/versions" />
		<!-- see also -->
		<xsl:apply-templates select="/document/comments/ddue:dduexml/ddue:relatedTopics" />
		<!-- assembly information -->
		<xsl:apply-templates select="/document/reference/containers/library" />

	</xsl:template> 

  <xsl:template name="obsoleteSection">
    <p>
      <include item="obsoleteLong" />
      <xsl:for-each select="/document/comments/ddue:dduexml/ddue:obsoleteCodeEntity">
        <xsl:text> </xsl:text>
        <include item="nonobsoleteAlternative">
          <parameter>
            <xsl:apply-templates select="ddue:codeEntityReference" />
          </parameter>
        </include>
      </xsl:for-each>
    </p>
  </xsl:template>

  <xsl:template name="internalOnly">
    <xsl:if test="/document/comments/ddue:dduexml/ddue:internalOnly">
      <div id="internalonly" class="seeAlsoNoToggleSection">
        <p/>
        <include item="internalOnly" />
      </div>
    </xsl:if>
  </xsl:template>

	<xsl:template match="templates">
    <div id="genericParameters">
		<xsl:call-template name="section">
			<xsl:with-param name="title"><include item="templatesTitle" /></xsl:with-param>
			<xsl:with-param name="content">
					<xsl:for-each select="template">
						<xsl:variable name="templateName" select="@name" />
            <dl paramName="{$templateName}">
						<dt>
							<span class="parameter"><xsl:value-of select="$templateName"/></span>
						</dt>
						<dd>
							<xsl:apply-templates select="/document/comments/ddue:dduexml/ddue:genericParameters/ddue:genericParameter[string(ddue:parameterReference)=$templateName]/ddue:content" />			
						</dd>
				</dl>
          </xsl:for-each>
			</xsl:with-param>
		</xsl:call-template>
    </div>
	</xsl:template>

	<xsl:template name="getParameterDescription">
		<xsl:param name="name" />
    <span sdata="authoredParameterSummary">
		<xsl:apply-templates select="/document/comments/ddue:dduexml/ddue:parameters/ddue:parameter[string(ddue:parameterReference)=$name]/ddue:content" />
    </span>
	</xsl:template>

	<xsl:template match="returns">
		<xsl:if test="normalize-space(/document/comments/ddue:dduexml/ddue:returnValue)">
      <div id="returns">
			<xsl:call-template name="section">
				<xsl:with-param name="title">
					<include>
						<xsl:attribute name="item">
							<xsl:value-of select="$subgroup" />
							<xsl:text>ValueTitle</xsl:text>
						</xsl:attribute>
					</include>
				</xsl:with-param>
				<xsl:with-param name="content">
					<xsl:call-template name="getReturnsDescription" />
				</xsl:with-param>
			</xsl:call-template>
      </div>
		</xsl:if>
	</xsl:template>

	<xsl:template name="getReturnsDescription">
    <span sdata="authoredValueSummary">
		<xsl:apply-templates select="/document/comments/ddue:dduexml/ddue:returnValue" />
    </span>
	</xsl:template>

	<xsl:template name="getElementDescription">
    <span sdata="memberAuthoredSummary">
		<xsl:apply-templates select="ddue:summary/ddue:para/node()" />
    </span>
	</xsl:template>

  <xsl:template name="getInternalOnlyDescription">
    <xsl:if test="ddue:internalOnly">
      <include item="infraStructure" />
    </xsl:if>
  </xsl:template>

	<!-- DDUEXML elements that behave differently in conceptual and reference -->

	<xsl:template match="ddue:exceptions">
		<xsl:call-template name="section">
			<xsl:with-param name="title"><include item="exceptionsTitle" /></xsl:with-param>
			<xsl:with-param name="content">
				<xsl:choose>
					<xsl:when test="ddue:exception">
						<table class="exceptions">
							<tr>
								<th class="exceptionNameColumn"><include item="exceptionNameHeader" /></th>
								<th class="exceptionConditionColumn"><include item="exceptionConditionHeader" /></th>
							</tr>
							<xsl:for-each select="ddue:exception">
								<tr>
									<td><xsl:apply-templates select="ddue:codeEntityReference" /><br /></td>
									<td><xsl:apply-templates select="ddue:content" /><br /></td>
								</tr>
							</xsl:for-each>
						</table>
					</xsl:when>
					<xsl:otherwise>
						<xsl:apply-templates />
					</xsl:otherwise>
				</xsl:choose>
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="ddue:permissions">
		<xsl:call-template name="section">
			<xsl:with-param name="title"><include item="permissionsTitle" /></xsl:with-param>
			<xsl:with-param name="content">
				<ul>
					<xsl:for-each select="ddue:permission">
						<li>
							<xsl:apply-templates select="ddue:codeEntityReference" />
							<xsl:text> </xsl:text>
							<xsl:apply-templates select="ddue:content" />
						</li>
					</xsl:for-each>
				</ul>
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

  <xsl:template match="ddue:codeExample">
    <xsl:apply-templates />
  </xsl:template>
  
</xsl:stylesheet>
