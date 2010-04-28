<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0" 
				xmlns:MSHelp="http://msdn.microsoft.com/mshelp"
        xmlns:mshelp="http://msdn.microsoft.com/mshelp"
				xmlns:ddue="http://ddue.schemas.microsoft.com/authoring/2003/5"
				xmlns:xlink="http://www.w3.org/1999/xlink"
        xmlns:msxsl="urn:schemas-microsoft-com:xslt"
        >

  <xsl:template name="autogenSeeAlsoLinks">

    <!-- a link to the containing type on all list and member topics -->
    <xsl:if test="$group='member' or $group='list'">
      <xsl:variable name="typeTopicId">
        <xsl:choose>
          <xsl:when test="/document/reference/topicdata/@typeTopicId">
            <xsl:value-of select="/document/reference/topicdata/@typeTopicId"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="/document/reference/containers/type/@api"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <div class="seeAlsoStyle">
        <referenceLink target="{$typeTopicId}" display-target="format">
          <include item="SeeAlsoTypeLinkText">
            <parameter>{0}</parameter>
            <parameter>
              <xsl:choose>
                <xsl:when test="/document/reference/topicdata/@typeTopicId">
                  <xsl:value-of select="/document/reference/apidata/@subgroup"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="/document/reference/containers/type/apidata/@subgroup"/>
                </xsl:otherwise>
              </xsl:choose>
            </parameter>
          </include>
        </referenceLink>
      </div>
    </xsl:if>
    
    <!-- a link to the type's All Members list -->
    <xsl:variable name="allMembersTopicId">
      <xsl:choose>
        <xsl:when test="/document/reference/topicdata/@allMembersTopicId">
          <xsl:value-of select="/document/reference/topicdata/@allMembersTopicId"/>
        </xsl:when>
        <xsl:when test="$group='member' or ($group='list' and $subgroup='overload')">
          <xsl:value-of select="/document/reference/containers/type/topicdata/@allMembersTopicId"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <xsl:if test="normalize-space($allMembersTopicId) and not($allMembersTopicId=$key)">
      <div class="seeAlsoStyle">
        <referenceLink target="{$allMembersTopicId}" display-target="format">
          <include item="SeeAlsoMembersLinkText">
            <parameter>{0}</parameter>
          </include>
        </referenceLink>
      </div>
    </xsl:if>

    <xsl:if test="/document/reference/memberdata/@overload">
      <!-- a link to the overload topic -->
      <div class="seeAlsoStyle">
        <referenceLink target="{/document/reference/memberdata/@overload}" display-target="format" show-parameters="false">
          <include item="SeeAlsoOverloadLinkText">
            <parameter>{0}</parameter>
          </include>
        </referenceLink>
      </div>
    </xsl:if>
  
    <!-- a link to the namespace topic -->
    <xsl:variable name="namespaceId">
      <xsl:value-of select="/document/reference/containers/namespace/@api"/>
    </xsl:variable>
    <xsl:if test="normalize-space($namespaceId)">
      <div class="seeAlsoStyle">
        <referenceLink target="{$namespaceId}" display-target="format">
          <include item="SeeAlsoNamespaceLinkText">
            <parameter>{0}</parameter>
          </include>
        </referenceLink>
      </div>
    </xsl:if>

  </xsl:template>

  <xsl:variable name="typeId">
    <xsl:choose>
      <xsl:when test="/document/reference/topicdata[@group='api'] and /document/reference/apidata[@group='type']">
        <xsl:value-of select="$key"/>
      </xsl:when>
      <xsl:when test="/document/reference/topicdata/@typeTopicId">
        <xsl:value-of select="/document/reference/topicdata/@typeTopicId"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="/document/reference/containers/type/@api"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  
  <!-- indent by 2*n spaces -->
  <xsl:template name="indent">
    <xsl:param name="count" />
    <xsl:if test="$count &gt; 1">
      <xsl:text>&#160;&#160;</xsl:text>
      <xsl:call-template name="indent">
        <xsl:with-param name="count" select="$count - 1" />
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <!-- Gets the substring after the last occurence of a period in a given string -->
  <xsl:template name="subString">
    <xsl:param name="name" />

    <xsl:choose>
      <xsl:when test="contains($name, '.')">
        <xsl:call-template name="subString">
          <xsl:with-param name="name" select="substring-after($name, '.')" />
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$name" />
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

  <xsl:template name="codeSection">
        
        <table width="100%" cellspacing="0" cellpadding="0">
          <tr>
            <th>
              <span class="copyCode" onclick="CopyCode(this)" onkeypress="CopyCode_CheckKey(this, event)" onmouseover="ChangeCopyCodeIcon(this)" onmouseout="ChangeCopyCodeIcon(this)" tabindex="0">
                <img class="copyCodeImage" name="ccImage" align="absmiddle">
                  <includeAttribute name="title" item="copyImage" />
                  <includeAttribute name="src" item="iconPath">
                    <parameter>copycode.gif</parameter>
                  </includeAttribute>
                </img>
                <include item="copyCode"/>
              </span>
            </th>
          </tr>
          <tr>
            <td colspan="2">
              <pre><xsl:text/><xsl:copy-of select="node()"/><xsl:text/></pre>
            </td>
          </tr>
        </table>
     
  </xsl:template>

  <xsl:template name="languageCheck">
    <xsl:param name="codeLanguage"/>

    <xsl:if test="$languages != 'false'">
      <xsl:if test="count($languages/language) &gt; 0">
        <xsl:for-each select="$languages/language">
          <xsl:if test="$codeLanguage = @name">
            <xsl:value-of select="@style"/>
          </xsl:if>
        </xsl:for-each>
      </xsl:if>
    </xsl:if>
  </xsl:template>

</xsl:stylesheet>