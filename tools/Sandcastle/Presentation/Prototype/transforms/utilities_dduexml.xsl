<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.1"
		xmlns:ddue="http://ddue.schemas.microsoft.com/authoring/2003/5"
		xmlns:xlink="http://www.w3.org/1999/xlink"
		xmlns:mshelp="http://msdn.microsoft.com/mshelp" >

  <xsl:import href="../../shared/transforms/utilities_dduexml.xsl" />

  <!-- sections -->

	<xsl:template match="ddue:remarks">
		<xsl:if test="normalize-space(.)">
			<xsl:call-template name="section">
				<xsl:with-param name="title"><include item="remarksTitle" /></xsl:with-param>
				<xsl:with-param name="content"><xsl:apply-templates /></xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:codeExamples">
		<xsl:if test="normalize-space(.)">
			<xsl:call-template name="section">
				<xsl:with-param name="title"><include item="examplesTitle" /></xsl:with-param>
				<xsl:with-param name="content"><xsl:apply-templates /></xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:threadSafety">
		<xsl:call-template name="section">
			<xsl:with-param name="title"><include item="threadSafetyTitle" /></xsl:with-param>
			<xsl:with-param name="content"><xsl:apply-templates /></xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="ddue:notesForImplementers">
		<xsl:call-template name="section">
			<xsl:with-param name="title"><include item="notesForImplementersTitle" /></xsl:with-param>
			<xsl:with-param name="content"><xsl:apply-templates /></xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="ddue:notesForCallers">
		<xsl:call-template name="section">
			<xsl:with-param name="title"><include item="notesForCallersTitle" /></xsl:with-param>
			<xsl:with-param name="content"><xsl:apply-templates /></xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="ddue:syntaxSection">
		<xsl:call-template name="section">
			<xsl:with-param name="title"><include item="syntaxTitle" /></xsl:with-param>
			<xsl:with-param name="content">
				<xsl:apply-templates />
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="ddue:legacySyntax">
		<pre><xsl:copy-of select="."/></pre>
	</xsl:template>

	<xsl:template match="ddue:relatedTopics">
		<xsl:if test="count(*) &gt; 0">
			<xsl:call-template name="section">
				<xsl:with-param name="title"><include item="relatedTitle" /></xsl:with-param>
				<xsl:with-param name="content">
					<xsl:for-each select="ddue:codeEntityReference|ddue:link|ddue:legacyLink|ddue:externalLink">
						<xsl:apply-templates select="." />
						<br />
					</xsl:for-each>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<!-- just skip over these -->
	<xsl:template match="ddue:content | ddue:codeExample | ddue:legacy">
		<xsl:apply-templates />
	</xsl:template>

	<!-- block elements -->

	<xsl:template match="ddue:table">
		<table class="authoredTable">
			<xsl:apply-templates />
		</table>
	</xsl:template>

	<xsl:template match="ddue:tableHeader">
		<xsl:apply-templates />
	</xsl:template>

	<xsl:template match="ddue:row">
		<tr>
			<xsl:apply-templates />
		</tr>
	</xsl:template>

	<xsl:template match="ddue:entry">
		<td>
			<xsl:apply-templates />
		</td>
	</xsl:template>

	<xsl:template match="ddue:tableHeader/ddue:row/ddue:entry">
		<th>
			<xsl:apply-templates />
		</th>
	</xsl:template>

	<xsl:template match="ddue:definitionTable">
		<dl>
			<xsl:apply-templates />
		</dl>
	</xsl:template>

	<xsl:template match="ddue:definedTerm">
		<dt>
			<xsl:apply-templates />
		</dt>
	</xsl:template>

	<xsl:template match="ddue:definition">
		<dd>
			<xsl:apply-templates />
		</dd>
	</xsl:template>

	<xsl:template match="ddue:code">
		<div class="code"><pre><xsl:apply-templates /></pre></div>
	</xsl:template>

	<xsl:template match="ddue:sampleCode">
		<div><b><xsl:value-of select="@language"/></b></div>
		<div class="code"><pre><xsl:apply-templates /></pre></div>
	</xsl:template>

	<xsl:template name="composeCode">
		<xsl:copy-of select="." />
		<xsl:variable name="next" select="following-sibling::*[1]" />
		<xsl:if test="boolean($next/@language) and boolean(local-name($next)=local-name())">
			<xsl:for-each select="$next">
				<xsl:call-template name="composeCode" />
			</xsl:for-each>
		</xsl:if>
	</xsl:template>

	<xsl:template match="ddue:alert">
		<div class="alert">
			<xsl:choose>
				<xsl:when test="@class='caution'">
					<img>
						<includeAttribute item="iconPath" name="src">
							<parameter>alert_caution.gif</parameter>
						</includeAttribute>
					</img>
					<xsl:text> </xsl:text>
					<include item="cautionTitle" />
				</xsl:when>
        <xsl:when test="@class='security note'">
					<img>
						<includeAttribute item="iconPath" name="src">
							<parameter>alert_security.gif</parameter>
						</includeAttribute>
					</img>
					<xsl:text> </xsl:text>
					<include item="securityTitle" />
        </xsl:when>
        <xsl:when test="@class='important'">
          <img>
            <includeAttribute item="iconPath" name="src">
              <parameter>alert_caution.gif</parameter>
            </includeAttribute>
          </img>
          <xsl:text> </xsl:text>
          <include item="importantTitle" />
        </xsl:when>
        <xsl:when test="@class='visual basic note'">
          <img>
            <includeAttribute item="iconPath" name="src">
              <parameter>alert_note.gif</parameter>
            </includeAttribute>
          </img>
          <xsl:text> </xsl:text>
          <include item="visualBasicTitle" />
        </xsl:when>
        <xsl:when test="@class='visual c# note'">
          <img>
            <includeAttribute item="iconPath" name="src">
              <parameter>alert_note.gif</parameter>
            </includeAttribute>
          </img>
          <xsl:text> </xsl:text>
          <include item="visualC#Title" />
        </xsl:when>
        <xsl:when test="@class='visual c++ note'">
          <img>
            <includeAttribute item="iconPath" name="src">
              <parameter>alert_note.gif</parameter>
            </includeAttribute>
          </img>
          <xsl:text> </xsl:text>
          <include item="visualC++Title" />
        </xsl:when>
        <xsl:when test="@class='visual j# note'">
          <img>
            <includeAttribute item="iconPath" name="src">
              <parameter>alert_note.gif</parameter>
            </includeAttribute>
          </img>
          <xsl:text> </xsl:text>
          <include item="visualJ#Title" />
				</xsl:when>
				<xsl:when test="@class='note'">
					<img>
						<includeAttribute item="iconPath" name="src">
							<parameter>alert_note.gif</parameter>
						</includeAttribute>
					</img>
					<xsl:text> </xsl:text>
					<include item="noteTitle" />
				</xsl:when>
				<xsl:otherwise>
          <img>
            <includeAttribute item="iconPath" name="src">
              <parameter>alert_note.gif</parameter>
            </includeAttribute>
          </img>
          <xsl:text> </xsl:text>
          <include item="{@class}" />
				</xsl:otherwise>
			</xsl:choose>
			<xsl:apply-templates />
		</div>
	</xsl:template>

  <xsl:template match="ddue:sections">
    <xsl:apply-templates select="ddue:section" />
  </xsl:template>

  <xsl:template match="ddue:section">
    <xsl:apply-templates select="@address" />
    <span class="subsectionTitle">
      <xsl:value-of select="ddue:title"/>
    </span>
    <div class="subsection">
      <xsl:apply-templates select="ddue:content"/>
      <xsl:apply-templates select="ddue:sections" />
    </div>
  </xsl:template>
<!--
  <xsl:template match="@address">
    <a name="{string(.)}" />
  </xsl:template>
-->
  <xsl:template match="ddue:mediaLink|ddue:mediaLinkInline">
		<div class="media">
			<artLink target="{ddue:image/@xlink:href}" />
      <xsl:if test="ddue:caption">
  			<div class="caption">
	  			<xsl:apply-templates select="ddue:caption" />
		  	</div>
      </xsl:if>
    </div>
	</xsl:template>

	<xsl:template match="ddue:procedure">
		<xsl:call-template name="section">
			<xsl:with-param name="title"><xsl:value-of select="ddue:title" /></xsl:with-param>
			<xsl:with-param name="content">
				<xsl:apply-templates select="ddue:steps" />
				<xsl:apply-templates select="ddue:conclusion" />
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="ddue:steps">
		<xsl:choose>
			<xsl:when test="@class='ordered'">
				<ol>
					<xsl:apply-templates select="ddue:step" />
				</ol>
			</xsl:when>
			<xsl:when test="@class='bullet'">
				<ul>
					<xsl:apply-templates select="ddue:step" />
				</ul>
			</xsl:when>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="ddue:step">
		<li><xsl:apply-templates /></li>
	</xsl:template>


	<xsl:template match="ddue:inThisSection">
		<xsl:call-template name="section">
			<xsl:with-param name="title"><include item="inThisSectionTitle" /></xsl:with-param>
			<xsl:with-param name="content">
				<xsl:apply-templates />
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="ddue:buildInstructions">
		<xsl:call-template name="section">
			<xsl:with-param name="title"><include item="buildInstructionsTitle" /></xsl:with-param>
			<xsl:with-param name="content">
				<xsl:apply-templates />
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="ddue:nextSteps">
		<xsl:call-template name="section">
			<xsl:with-param name="title"><include item="nextStepsTitle" /></xsl:with-param>
			<xsl:with-param name="content">
				<xsl:apply-templates />
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="ddue:requirements">
		<xsl:call-template name="section">
			<xsl:with-param name="title"><include item="requirementsTitle" /></xsl:with-param>
			<xsl:with-param name="content">
				<xsl:apply-templates />
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<!-- inline elements -->

	<xsl:template match="ddue:languageKeyword">
			<xsl:variable name="word" select="." />
    <span class="keyword" sdata="langKeyword" value="{$word}">
			<xsl:choose>
				<xsl:when test="$word='null' or $word='Nothing' or $word='nullptr'">
					<span class="cs">null</span>
					<span class="vb">Nothing</span>
					<span class="cpp">nullptr</span>
				</xsl:when>
				<xsl:when test="$word='static' or $word='Shared'">
					<span class="cs">static</span>
					<span class="vb">Shared</span>
					<span class="cpp">static</span>
				</xsl:when>
				<xsl:when test="$word='virtual' or $word='Overridable'">
					<span class="cs">virtual</span>
					<span class="vb">Overridable</span>
					<span class="cpp">virtual</span>
				</xsl:when>
				<xsl:when test="$word='true' or $word='True'">
					<span class="cs">true</span>
					<span class="vb">True</span>
					<span class="cpp">true</span>
				</xsl:when>
				<xsl:when test="$word='false' or $word='False'">
					<span class="cs">false</span>
					<span class="vb">False</span>
					<span class="cpp">false</span>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="." />
				</xsl:otherwise>
			</xsl:choose>
		</span>
	</xsl:template>

  <!-- links -->

  <xsl:template match="ddue:dynamicLink[@type='inline']">
    <mshelp:ktable disambiguator='span' indexMoniker='!DefaultDynamicLinkIndex'>
      <xsl:attribute name="keywords">
        <xsl:for-each select="ddue:keyword">
          <xsl:value-of select="."/>
          <xsl:if test="position() != last()">;</xsl:if>
        </xsl:for-each>
      </xsl:attribute>
      <includeAttribute name="prefix" item="dynamicLinkInlinePreFixText" />
      <includeAttribute name="postfix" item="dynamicLinkInlinePostFixText" />
      <includeAttribute name="separator" item="dynamicLinkInlineSeperatorText" />
    </mshelp:ktable>
  </xsl:template>

  <xsl:template match="ddue:dynamicLink[@type='table']">
    <include item="mshelpKTable">
      <parameter>
        <xsl:for-each select="ddue:keyword">
          <xsl:value-of select="."/>
          <xsl:if test="position() != last()">;</xsl:if>
        </xsl:for-each>
      </parameter>
    </include>
  </xsl:template>

  <xsl:template match="ddue:dynamicLink[@type='bulleted']">
    <mshelp:ktable disambiguator='span' indexMoniker='!DefaultDynamicLinkIndex'>
      <xsl:attribute name="keywords">
        <xsl:for-each select="ddue:keyword">
          <xsl:value-of select="."/>
          <xsl:if test="position() != last()">;</xsl:if>
        </xsl:for-each>
      </xsl:attribute>
      <xsl:attribute name="prefix">&lt;ul&gt;&lt;li&gt;</xsl:attribute>
      <xsl:attribute name="postfix">&lt;/li&gt;&lt;/ul&gt;</xsl:attribute>
      <xsl:attribute name="separator">&lt;/li&gt;&lt;li&gt;</xsl:attribute>
    </mshelp:ktable>
  </xsl:template>

  <xsl:template match="ddue:codeFeaturedElement">
    <xsl:if test="normalize-space(.)">
      <b><xsl:apply-templates/></b>
    </xsl:if>
  </xsl:template>

  <xsl:template match="ddue:languageReferenceRemarks">
    <xsl:call-template name="section">
      <xsl:with-param name="title"><include item="remarksTitle" /></xsl:with-param>
      <xsl:with-param name="content"><xsl:apply-templates /></xsl:with-param>
    </xsl:call-template>
  </xsl:template>

  <xsl:template match="ddue:attributesandElements">
    <xsl:call-template name="section">
      <xsl:with-param name="title"><include item="attributesAndElements" /></xsl:with-param>
      <xsl:with-param name="content"><xsl:apply-templates /></xsl:with-param>
    </xsl:call-template>
  </xsl:template>

  <xsl:template match="ddue:attributes">
    <h4 class="subHeading"><include item="attributes"/></h4>
    <xsl:apply-templates/>
  </xsl:template>

  <xsl:template match="ddue:attribute">
    <xsl:apply-templates/>
  </xsl:template>

  <xsl:template match="ddue:attribute/ddue:title">
    <h4 class="subHeading"><xsl:apply-templates/></h4>
  </xsl:template>

  <xsl:template match="ddue:childElement">
    <h4 class="subHeading"><include item="childElement"/></h4>
    <xsl:apply-templates/>
  </xsl:template>

  <xsl:template match="ddue:parentElement">
    <h4 class="subHeading"><include item="parentElement"/></h4>
    <xsl:apply-templates/>
  </xsl:template>

  <xsl:template match="ddue:textValue">
    <xsl:call-template name="section">
      <xsl:with-param name="title"><include item="textValue" /></xsl:with-param>
      <xsl:with-param name="content"><xsl:apply-templates /></xsl:with-param>
    </xsl:call-template>
  </xsl:template>

  <xsl:template match="ddue:elementInformation">
    <xsl:call-template name="section">
      <xsl:with-param name="title"><include item="elementInformation" /></xsl:with-param>
      <xsl:with-param name="content"><xsl:apply-templates /></xsl:with-param>
    </xsl:call-template>
  </xsl:template>

  <xsl:template match="ddue:dotNetFrameworkEquivalent">
    <xsl:call-template name="section">
      <xsl:with-param name="title"><include item="dotNetFrameworkEquivalent" /></xsl:with-param>
      <xsl:with-param name="content"><xsl:apply-templates /></xsl:with-param>
    </xsl:call-template>
  </xsl:template>

  <xsl:template match="ddue:prerequisites">
    <xsl:call-template name="section">
      <xsl:with-param name="title"><include item="prerequisites" /></xsl:with-param>
      <xsl:with-param name="content"><xsl:apply-templates /></xsl:with-param>
    </xsl:call-template>
  </xsl:template>

  <xsl:template match="ddue:type">
    <xsl:apply-templates/>
  </xsl:template>

  <xsl:template match="ddue:robustProgramming">
    <xsl:call-template name="section">
      <xsl:with-param name="title"><include item="robustProgramming" /></xsl:with-param>
      <xsl:with-param name="content"><xsl:apply-templates /></xsl:with-param>
    </xsl:call-template>
  </xsl:template>

  <xsl:template match="ddue:security">
    <xsl:call-template name="section">
      <xsl:with-param name="title"><include item="securitySection" /></xsl:with-param>
      <xsl:with-param name="content"><xsl:apply-templates /></xsl:with-param>
    </xsl:call-template>
  </xsl:template>

  <xsl:template match="ddue:externalResources">
    <xsl:call-template name="section">
      <xsl:with-param name="title">
        <include item="externalResources" />
      </xsl:with-param>
      <xsl:with-param name="content">
        <xsl:apply-templates />
      </xsl:with-param>
    </xsl:call-template>
  </xsl:template>

  <xsl:template match="ddue:demonstrates">
    <xsl:call-template name="section">
      <xsl:with-param name="title">
        <include item="demonstrates" />
      </xsl:with-param>
      <xsl:with-param name="content">
        <xsl:apply-templates />
      </xsl:with-param>
    </xsl:call-template>
  </xsl:template>

  <xsl:template match="ddue:appliesTo">
    <xsl:call-template name="section">
      <xsl:with-param name="title">
        <include item="appliesTo" />
      </xsl:with-param>
      <xsl:with-param name="content">
        <xsl:apply-templates />
      </xsl:with-param>
    </xsl:call-template>
  </xsl:template>

  <xsl:template match="ddue:conclusion">
    <xsl:call-template name="section">
      <xsl:with-param name="title">
        <include item="conclusion" />
      </xsl:with-param>
      <xsl:with-param name="content">
        <xsl:apply-templates />
      </xsl:with-param>
    </xsl:call-template>
  </xsl:template>

  <xsl:template match="ddue:background">
    <xsl:call-template name="section">
      <xsl:with-param name="title">
        <include item="background" />
      </xsl:with-param>
      <xsl:with-param name="content">
        <xsl:apply-templates />
      </xsl:with-param>
    </xsl:call-template>
  </xsl:template>

  <xsl:template match="ddue:whatsNew">
    <xsl:call-template name="section">
      <xsl:with-param name="title">
        <include item="whatsNew" />
      </xsl:with-param>
      <xsl:with-param name="content">
        <xsl:apply-templates />
      </xsl:with-param>
    </xsl:call-template>
  </xsl:template>
  
	<xsl:template name="createReferenceLink">
		<xsl:param name="id" />
		<xsl:param name="qualified" select="false()" />
		<referenceLink target="{$id}" qualified="{$qualified}" />
	</xsl:template>


	<!-- this is temporary -->
        <xsl:template match="ddue:snippets">
		<xsl:variable name="codeId" select="generate-id()" />
          <div name="snippetGroup">
		<table class="filter"><tr class="tabs" id="ct_{$codeId}">
			<xsl:for-each select="ddue:snippet">
                  <td class="tab" x-lang="{@language}" onclick="toggleClass('ct_{$codeId}','x-lang','{@language}','activeTab','tab'); toggleStyle('cb_{$codeId}','x-lang','{@language}','display','block','none');"><include item="{@language}Label" /></td>
			</xsl:for-each>
		</tr></table>
		<div id="cb_{$codeId}">
			<xsl:for-each select="ddue:snippet">
				<div class="code" x-lang="{@language}"><pre><xsl:copy-of select="node()" /></pre></div>
			</xsl:for-each>
		</div>
          </div>
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
