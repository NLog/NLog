<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.1">

	<!-- stuff specified to comments authored in DDUEXML -->

	<xsl:include href="utilities_reference.xsl" />

	<xsl:variable name="summary" select="normalize-space(/document/comments/summary)" />

	<xsl:template name="body">

		<!-- auto-inserted info -->
		<!-- <xsl:apply-templates select="/document/reference/attributes" /> -->
    <xsl:apply-templates select="/document/comments/preliminary" />
		<xsl:apply-templates select="/document/comments/summary" />
    <xsl:if test="$group='member' and /document/reference/elements">
      <xsl:apply-templates select="/document/reference/elements/element/overloads" />
    </xsl:if>
    <!-- syntax -->
		<xsl:apply-templates select="/document/syntax" />
		<!-- generic templates -->
		<xsl:apply-templates select="/document/reference/templates" />
		<!-- parameters & return value -->
		<xsl:apply-templates select="/document/reference/parameters" />
		<xsl:apply-templates select="/document/comments/value" />
		<xsl:apply-templates select="/document/comments/returns" />
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
		<xsl:apply-templates select="/document/comments/remarks" />
    <xsl:apply-templates select="/document/comments/threadsafety" />
		<!-- example -->
		<xsl:apply-templates select="/document/comments/example" />
		<!-- other comment sections -->
		<!-- permissions -->
		<xsl:call-template name="permissions" />
		<!-- exceptions -->
		<xsl:call-template name="exceptions" />
		<!-- inheritance -->
		<xsl:apply-templates select="/document/reference/family" />
    <!--versions-->
    <xsl:if test="not($group='member' or $group='namespace' or $tgroup='root' )">
      <xsl:apply-templates select="/document/reference/versions" />
    </xsl:if>
		<!-- see also -->
		<xsl:call-template name="seealso" />
		<!-- assembly information -->
		<xsl:apply-templates select="/document/reference/containers/library" />

	</xsl:template> 


	<xsl:template name="getParameterDescription">
		<xsl:param name="name" />
		<xsl:apply-templates select="/document/comments/param[@name=$name]" />
	</xsl:template>

	<xsl:template name="getReturnsDescription">
		<xsl:param name="name" />
		<xsl:apply-templates select="/document/comments/param[@name=$name]" />
	</xsl:template>

	<xsl:template name="getElementDescription">
		<xsl:apply-templates select="summary" />
	</xsl:template>

	<xsl:template name="getInternalOnlyDescription">
    		<!-- To do -->
  	</xsl:template>

	<!-- block sections -->

	<xsl:template match="summary">
		<div class="summary">
			<xsl:apply-templates />
		</div>
	</xsl:template>

  <xsl:template match="overloads">
    <xsl:choose>
      <xsl:when test="count(*) > 0">
        <xsl:apply-templates />
      </xsl:when>
      <xsl:otherwise>
        <div class="summary">
          <xsl:apply-templates/>
        </div>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

	<xsl:template match="templates">
		<xsl:call-template name="section">
			<xsl:with-param name="title"><include item="templatesTitle" /></xsl:with-param>
			<xsl:with-param name="content">
				<dl>
					<xsl:for-each select="template">
						<xsl:variable name="templateName" select="@name" />
						<dt>
							<span class="parameter"><xsl:value-of select="$templateName"/></span>
						</dt>
						<dd>
							<xsl:apply-templates select="/document/comments/typeparam[@name=$templateName]" />			
						</dd>
					</xsl:for-each>
				</dl>
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="value">
		<xsl:call-template name="section">
			<xsl:with-param name="title"><include item="fieldValueTitle" /></xsl:with-param>
			<xsl:with-param name="content"><xsl:apply-templates /></xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="returns">
		<xsl:call-template name="section">
			<xsl:with-param name="title"><include item="methodValueTitle" /></xsl:with-param>
			<xsl:with-param name="content"><xsl:apply-templates /></xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="remarks">
		<xsl:call-template name="section">
			<xsl:with-param name="title"><include item="remarksTitle" /></xsl:with-param>
			<xsl:with-param name="content"><xsl:apply-templates /></xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="example">
		<xsl:call-template name="section">
			<xsl:with-param name="title"><include item="examplesTitle" /></xsl:with-param>
			<xsl:with-param name="content">
	      <xsl:choose>
	        <xsl:when test="code/@language">
        <xsl:variable name="codeId" select="generate-id()" />
            <div name="snippetGroup">
        <table class="filter"><tr class="tabs" id="ct_{$codeId}">
            <xsl:for-each select="code">
                    <td class="tab" x-lang="{@language}" onclick="toggleClass('ct_{$codeId}','x-lang','{@language}','activeTab','tab'); toggleStyle('cb_{$codeId}','x-lang','{@language}','display','block','none');"><include item="{@language}Label" /></td>
            </xsl:for-each></tr></table>
        <div id="cb_{$codeId}">
          <xsl:for-each select="code">
            <div class="code" x-lang="{@language}"><pre><xsl:copy-of select="node()" /></pre></div>
          </xsl:for-each>
        </div>
           </div>
	        </xsl:when>
          <xsl:otherwise>
            <xsl:apply-templates />
          </xsl:otherwise>
        </xsl:choose>
      </xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="para">
		<p><xsl:apply-templates /></p>
	</xsl:template>

	<xsl:template match="code">
		<div class="code"><pre><xsl:apply-templates /></pre></div>
	</xsl:template>

	<xsl:template name="exceptions">
		<xsl:if test="count(/document/comments/exception) &gt; 0">
			<xsl:call-template name="section">
				<xsl:with-param name="title"><include item="exceptionsTitle" /></xsl:with-param>
				<xsl:with-param name="content">
				<table class="exceptions">
					<tr>
						<th class="exceptionNameColumn"><include item="exceptionNameHeader" /></th>
						<th class="exceptionConditionColumn"><include item="exceptionConditionHeader" /></th>
					</tr>
					<xsl:for-each select="/document/comments/exception">
						<tr>
							<td><referenceLink target="{@cref}" /></td>
							<td><xsl:apply-templates select="." /><br /></td>
						</tr>
					</xsl:for-each>
				</table>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template name="permissions">
		<xsl:if test="count(/document/comments/permission) &gt; 0">
			<xsl:call-template name="section">
				<xsl:with-param name="title"><include item="permissionsTitle" /></xsl:with-param>
				<xsl:with-param name="content">
				<table class="permissions">
					<tr>
						<th class="permissionNameColumn"><include item="permissionNameHeader" /></th>
						<th class="permissionDescriptionColumn"><include item="permissionDescriptionHeader" /></th>
					</tr>
					<xsl:for-each select="/document/comments/permission">
						<tr>
							<td><referenceLink target="{@cref}" /></td>
              <td><xsl:apply-templates select="." /><br /></td>
						</tr>
					</xsl:for-each>
				</table>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template name="seealso">
		<xsl:if test="count(/document/comments//seealso | /document/reference/elements/element/overloads//seealso) &gt; 0">
			<xsl:call-template name="section">
				<xsl:with-param name="title"><include item="relatedTitle" /></xsl:with-param>
				<xsl:with-param name="content">
					<xsl:for-each select="/document/comments//seealso | /document/reference/elements/element/overloads//seealso">
            <xsl:apply-templates select=".">
              <xsl:with-param name="displaySeeAlso" select="true()" />
            </xsl:apply-templates>
						<br />
					</xsl:for-each>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="list[@type='bullet']">
		<ul>
			<xsl:for-each select="item">
				<li><xsl:apply-templates /></li>
			</xsl:for-each>
		</ul>
	</xsl:template>

	<xsl:template match="list[@type='number']">
		<ol>
			<xsl:for-each select="item">
				<li><xsl:apply-templates /></li>
			</xsl:for-each>
		</ol>
	</xsl:template>

	<xsl:template match="list[@type='table']">
		<table class="authoredTable">
			<xsl:for-each select="listheader">
				<tr>
					<xsl:for-each select="*">
						<th><xsl:apply-templates /></th>
					</xsl:for-each>
				</tr>
			</xsl:for-each>
			<xsl:for-each select="item">
				<tr>
					<xsl:for-each select="*">
						<td><xsl:apply-templates /><br /></td>
					</xsl:for-each>
				</tr>
			</xsl:for-each>
		</table>
	</xsl:template>

	<!-- inline tags -->

	<xsl:template match="see[@cref]">
        <xsl:choose>
          <xsl:when test="normalize-space(.)">
            <referenceLink target="{@cref}">
              <xsl:value-of select="." />
            </referenceLink>
          </xsl:when>
          <xsl:otherwise>
            <referenceLink target="{@cref}"/>
          </xsl:otherwise>
        </xsl:choose>
	</xsl:template>

  <xsl:template match="see[@href]">
    <xsl:choose>
      <xsl:when test="normalize-space(.)">
        <a>
          <xsl:attribute name="href"><xsl:value-of select="@href"/></xsl:attribute>
          <xsl:value-of select="." />
        </a>
      </xsl:when>
      <xsl:otherwise>
        <a>
          <xsl:attribute name="href"><xsl:value-of select="@href"/></xsl:attribute>
          <xsl:value-of select="@href" />
        </a>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

	<xsl:template match="seealso[@href]">
    <xsl:param name="displaySeeAlso" select="false()" />
    <xsl:if test="$displaySeeAlso">
    <xsl:choose>
      <xsl:when test="normalize-space(.)">
        <a>
          <xsl:attribute name="href"><xsl:value-of select="@href"/></xsl:attribute>
          <xsl:value-of select="." />
        </a>
      </xsl:when>
      <xsl:otherwise>
        <a>
          <xsl:attribute name="href"><xsl:value-of select="@href"/></xsl:attribute>
          <xsl:value-of select="@href" />
        </a>
      </xsl:otherwise>
    </xsl:choose>
    </xsl:if>
  </xsl:template>

	<xsl:template match="see[@langword]">
		<span class="keyword">
			<xsl:choose>
				<xsl:when test="@langword='null' or @langword='Nothing' or @langword='nullptr'">
					<span class="cs">null</span>
					<span class="vb">Nothing</span>
					<span class="cpp">nullptr</span>
				</xsl:when>
				<xsl:when test="@langword='static' or @langword='Shared'">
					<span class="cs">static</span>
					<span class="vb">Shared</span>
					<span class="cpp">static</span>
				</xsl:when>
				<xsl:when test="@langword='virtual' or @langword='Overridable'">
					<span class="cs">virtual</span>
					<span class="vb">Overridable</span>
					<span class="cpp">virtual</span>
				</xsl:when>
				<xsl:when test="@langword='true' or @langword='True'">
					<span class="cs">true</span>
					<span class="vb">True</span>
					<span class="cpp">true</span>
				</xsl:when>
				<xsl:when test="@langword='false' or @langword='False'">
					<span class="cs">false</span>
					<span class="vb">False</span>
					<span class="cpp">false</span>
				</xsl:when>
        <xsl:when test="@langword='abstract'">
          <span class="cs">abstract</span>
          <span class="vb">MustInherit</span>
          <span class="cpp">abstract</span>
        </xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="@langword" />
				</xsl:otherwise>
			</xsl:choose>
		</span>
	</xsl:template>


	<xsl:template match="seealso">
    <xsl:param name="displaySeeAlso" select="false()" />
    <xsl:if test="$displaySeeAlso">
    <xsl:choose>
      <xsl:when test="normalize-space(.)">
        <referenceLink target="{@cref}">
          <xsl:value-of select="." />
        </referenceLink>
      </xsl:when>
      <xsl:otherwise>
        <referenceLink target="{@cref}" />
      </xsl:otherwise>
    </xsl:choose>
    </xsl:if>
	</xsl:template>

	<xsl:template match="c">
		<span class="code"><xsl:apply-templates /></span>
	</xsl:template>

	<xsl:template match="paramref">
		<span class="parameter"><xsl:value-of select="@name" /></span>
	</xsl:template>

	<xsl:template match="typeparamref">
		<span class="typeparameter"><xsl:value-of select="@name" /></span>
	</xsl:template>

	<!-- pass through html tags -->

	<xsl:template match="p|ol|ul|li|dl|dt|dd|table|tr|th|td|h1|h2|h3|h4|h5|h6|hr|br|pre|blockquote|div|span|a|img|b|i|strong|em|del|sub|sup|abbr|acronym|u|font|map|area">
		<xsl:copy>
			<xsl:copy-of select="@*" />
			<xsl:apply-templates />
		</xsl:copy>
	</xsl:template>

  <!-- extra tag support -->

  <xsl:template match="threadsafety">
    <xsl:call-template name="section">
      <xsl:with-param name="title">
        <include item="threadSafetyTitle" />
      </xsl:with-param>
      <xsl:with-param name="content">
        <xsl:choose>
          <xsl:when test="normalize-space(.)">
            <xsl:apply-templates />
          </xsl:when>
          <xsl:otherwise>
            <xsl:if test="@static='true'">
              <include item="staticThreadSafe" />
            </xsl:if>
            <xsl:if test="@static='false'">
              <include item="staticNotThreadSafe" />
            </xsl:if>
            <xsl:if test="@instance='true'">
              <include item="instanceThreadSafe" />
            </xsl:if>
            <xsl:if test="@instance='false'">
              <include item="instanceNotThreadSafe" />
            </xsl:if>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:with-param>
    </xsl:call-template>
  </xsl:template>

  <xsl:template match="note">
    <div class="alert">
      <img>
        <includeAttribute item="iconPath" name="src">
          <parameter>alert_note.gif</parameter>
        </includeAttribute>
      </img>
      <xsl:text> </xsl:text>
      <include item="noteTitle" />
      <xsl:apply-templates />
    </div>
  </xsl:template>

  <xsl:template match="preliminary">
    <div class="preliminary">
      <include item="preliminaryText" />
    </div>
  </xsl:template>

  <!-- move these off into a shared file -->

	<xsl:template name="createReferenceLink">
		<xsl:param name="id" />
		<xsl:param name="qualified" select="false()" />
		<b><referenceLink target="{$id}" qualified="{$qualified}" /></b>
	</xsl:template>

	<xsl:template name="section">
		<xsl:param name="title" />
		<xsl:param name="content" />
		<div class="section">
			<div class="sectionTitle" onclick="toggleSection(this.parentNode)">
				<img>
					<includeAttribute name="src" item="iconPath">
						<parameter>collapse_all.gif</parameter>
					</includeAttribute>
				</img>
				<xsl:text> </xsl:text>
				<xsl:copy-of select="$title" />
			</div>
			<div class="sectionContent">
				<xsl:copy-of select="$content" />
			</div>
		</div>
	</xsl:template>


</xsl:stylesheet>
