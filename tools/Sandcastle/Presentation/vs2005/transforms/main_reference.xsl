<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.1"
				xmlns:MSHelp="http://msdn.microsoft.com/mshelp"
        xmlns:mshelp="http://msdn.microsoft.com/mshelp"
				xmlns:ddue="http://ddue.schemas.microsoft.com/authoring/2003/5"
				xmlns:xlink="http://www.w3.org/1999/xlink"
        xmlns:msxsl="urn:schemas-microsoft-com:xslt"
    >

  <xsl:param name="omitAptcaBoilerplate"/>
  <xsl:param name="RTMReleaseDate" />

  <!-- stuff specific to comments authored in DDUEXML -->

	<xsl:include href="utilities_reference.xsl" />
	<xsl:include href="utilities_dduexml.xsl" />
  <xsl:include href="htmlBody.xsl"/>
  <xsl:include href="seeAlsoSection.xsl"/>
  
  <xsl:variable name="summary" select="normalize-space(/document/comments/ddue:dduexml/ddue:summary)" />
  
  <xsl:variable name="abstractSummary">
    <xsl:for-each select="/document/comments/ddue:dduexml/ddue:summary">
      <xsl:apply-templates select="." mode="abstract" />
    </xsl:for-each>
  </xsl:variable>
  
  <xsl:variable name="hasSeeAlsoSection" 
                select="boolean( 
                           (count(/document/comments/ddue:dduexml/ddue:relatedTopics/*) > 0)  or 
                           ($group='type' or $group='member' or $group='list')
                        )"/>
  <xsl:variable name="examplesSection" select="boolean(string-length(/document/comments/ddue:dduexml/ddue:codeExamples[normalize-space(.)]) > 0)"/>
  <xsl:variable name="languageFilterSection" select="boolean(string-length(/document/comments/ddue:dduexml/ddue:codeExamples[normalize-space(.)]) > 0)" />
  <xsl:variable name="securityCriticalSection" 
                select="boolean(
                          (/document/reference/attributes/attribute/type[@api='T:System.Security.SecurityCriticalAttribute'] and
 			                      not(/document/reference/attributes/attribute/type[@api='T:System.Security.SecurityTreatAsSafeAttribute'])) or  
                          (/document/reference/containers/type/attributes/attribute/type[@api='T:System.Security.SecurityCriticalAttribute'] and
			                      not(/document/reference/containers/type/attributes/attribute/type[@api='T:System.Security.SecurityTreatAsSafeAttribute'])) or
                          ($api-subgroup='property' and 
                            (((/document/reference/getter and (/document/reference/getter/attributes/attribute/type[@api='T:System.Security.SecurityCriticalAttribute'] and not(/document/reference/getter/attributes/attribute/type[@api='T:System.Security.SecurityTreatAsSafeAttribute']))) and 
		 	                        (/document/reference/setter and (/document/reference/setter/attributes/attribute/type[@api='T:System.Security.SecurityCriticalAttribute'] and not(/document/reference/setter/attributes/attribute/type[@api='T:System.Security.SecurityTreatAsSafeAttribute'])))) or
                             ((/document/reference/getter and (/document/reference/getter/attributes/attribute/type[@api='T:System.Security.SecurityCriticalAttribute'] and not(/document/reference/getter/attributes/attribute/type[@api='T:System.Security.SecurityTreatAsSafeAttribute']))) and not(/document/reference/setter)) or
                             (not(/document/reference/getter) and (/document/reference/setter and (/document/reference/setter/attributes/attribute/type[@api='T:System.Security.SecurityCriticalAttribute'] and not(/document/reference/setter/attributes/attribute/type[@api='T:System.Security.SecurityTreatAsSafeAttribute'])))) 
                            )) or
                            ($api-subgroup='event' and 
                            (((/document/reference/adder and (/document/reference/adder/attributes/attribute/type[@api='T:System.Security.SecurityCriticalAttribute'] and not(/document/reference/adder/attributes/attribute/type[@api='T:System.Security.SecurityTreatAsSafeAttribute']))) and 							      
                              (/document/reference/remover and (/document/reference/remover/attributes/attribute/type[@api='T:System.Security.SecurityCriticalAttribute'] and not(/document/reference/remover/attributes/attribute/type[@api='T:System.Security.SecurityTreatAsSafeAttribute'])))) or
                             ((/document/reference/adder and (/document/reference/adder/attributes/attribute/type[@api='T:System.Security.SecurityCriticalAttribute'] and	not(/document/reference/adder/attributes/attribute/type[@api='T:System.Security.SecurityTreatAsSafeAttribute']))) and not(/document/reference/remover)) or
                             (not(/document/reference/adder) and (/document/reference/remover and (/document/reference/remover/attributes/attribute/type[@api='T:System.Security.SecurityCriticalAttribute'] and not(/document/reference/remover/attributes/attribute/type[@api='T:System.Security.SecurityTreatAsSafeAttribute']))))
                            ))
                        )" />
  
	<xsl:template name="body">
    <!-- freshness date -->
    <xsl:call-template name="writeFreshnessDate">
      <xsl:with-param name="ChangedHistoryDate" select="/document/comments/ddue:dduexml//ddue:section[ddue:title = 'Change History']/ddue:content/ddue:table/ddue:row[1]/ddue:entry[1]"/>
    </xsl:call-template>

    <!--internalOnly boilerplate -->
    <xsl:if test="not($securityCriticalSection)">
    <xsl:call-template name="internalOnly"/>
    </xsl:if>

    <!-- obsolete boilerplate -->
    <xsl:if test="/document/reference/attributes/attribute/type[@api='T:System.ObsoleteAttribute']">
      <xsl:call-template name="obsoleteSection" />
    </xsl:if>
        
      <!-- SecurityCritical boilerplate -->
      <xsl:if test="$securityCriticalSection">
        <xsl:choose>
          <xsl:when test="boolean($api-group='type')">
            <include item="typeSecurityCriticalBoilerplate" />
          </xsl:when>
          <xsl:when test="boolean($api-group='member')">
            <xsl:choose>
              <xsl:when test="(/document/reference/containers/type/attributes/attribute/type[@api='T:System.Security.SecurityCriticalAttribute'] and
 			                      not(/document/reference/containers/type/attributes/attribute/type[@api='T:System.Security.SecurityTreatAsSafeAttribute']))">
                <include item="typeSecurityCriticalBoilerplate" />
              </xsl:when>
              <xsl:otherwise>
                <include item="memberSecurityCriticalBoilerplate" />
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
        </xsl:choose>
      </xsl:if>

    <!-- summary -->
    <!-- useBase boilerplate -->
    <xsl:if test="/document/comments/ddue:dduexml/ddue:useBase and /document/reference/overrides/member">
      <include item="useBaseBoilerplate">
        <parameter>
          <xsl:apply-templates select="/document/reference/overrides/member" mode="link"/>
        </parameter>
      </include>
    </xsl:if>
    <xsl:choose>
      <xsl:when test="normalize-space(/document/comments/ddue:dduexml/ddue:summary[1]) != ''">
        <span sdata="authoredSummary">
            <xsl:if test="$securityCriticalSection">
              <p><include item="securityCritical" /></p>
            </xsl:if>
        <xsl:apply-templates select="/document/comments/ddue:dduexml/ddue:summary[1]" />
        </span>
      </xsl:when>
      <!-- if no authored summary, and not in primary framework (e.g. netfw), and overrides a base member: show link to base member -->
      <xsl:when test="/document/reference/overrides/member and not(/document/reference/versions/versions[1]//version)">
        <include item="useBaseSummary">
          <parameter>
            <xsl:apply-templates select="/document/reference/overrides/member" mode="link"/>
          </parameter>
        </include>
      </xsl:when>
      <xsl:otherwise>
        <xsl:apply-templates select="/document/comments/ddue:dduexml/ddue:summary[2]" />
      </xsl:otherwise>
    </xsl:choose>

    <!-- Flags attribute boilerplate -->
    <xsl:if test="/document/reference/attributes/attribute/type[@api='T:System.FlagsAttribute']">
      <p>
        <include item="flagsSummary">
          <parameter><referenceLink target="{/document/reference/attributes/attribute/type/@api}" /></parameter>
        </include>
      </p>
    </xsl:if>

    <!-- Non Cls Compliant boilerplate -->
    <xsl:if test="/document/reference/attributes/attribute[type[@api='T:System.CLSCompliantAttribute']]/argument[value='False']">
      <p/>
      <include item="NotClsCompliant"/>
      <xsl:text>&#160;</xsl:text>
      <xsl:if test="/document/comments/ddue:dduexml/ddue:clsCompliantAlternative">
        <include item="AltClsCompliant">
          <parameter>
            <xsl:apply-templates select="/document/comments/ddue:dduexml/ddue:clsCompliantAlternative/ddue:codeEntityReference"/>
          </parameter>
        </include>
      </xsl:if>
    </xsl:if>

    <xsl:if test="$group='namespace'">
      <xsl:apply-templates select="/document/comments/ddue:dduexml/ddue:remarks" />
    </xsl:if>
       
    <!-- assembly information -->
    <xsl:if test="not($group='list' or $group='root' or $group='namespace')">
      <xsl:call-template name="requirementsInfo"/>
    </xsl:if>
    
    <!-- syntax -->
    <xsl:if test="not($group='list' or $group='namespace')">
      <xsl:apply-templates select="/document/syntax" />
    </xsl:if>

    <!-- show authored Dependency Property Information section for properties -->
    <xsl:if test="$subgroup='property'">
      <xsl:apply-templates select="//ddue:section[starts-with(@address,'dependencyPropertyInfo')]" mode="section"/>
    </xsl:if>

    <!-- show authored Routed Event Information section for events -->
    <xsl:if test="$subgroup='event'">
      <xsl:apply-templates select="//ddue:section[starts-with(@address,'routedEventInfo')]" mode="section"/>
    </xsl:if>

    <!-- members -->
		<xsl:choose>
			<xsl:when test="$group='root'">
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
      <xsl:when test="$group='list'">
        <xsl:choose>
          <xsl:when test="$subgroup='overload'">
            <xsl:apply-templates select="/document/reference/elements" mode="overload" />
          </xsl:when>
          <xsl:when test="$subgroup='DerivedTypeList'">
            <xsl:apply-templates select="/document/reference/elements" mode="derivedType" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:apply-templates select="/document/reference/elements" mode="member" />
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
		</xsl:choose>
    <!-- exceptions -->
      <xsl:if test="not($securityCriticalSection)">
    <xsl:apply-templates select="/document/comments/ddue:dduexml/ddue:exceptions" />
      </xsl:if>
		<!-- remarks -->
      <xsl:if test="not($group='namespace') and not($securityCriticalSection)">
        <xsl:apply-templates select="/document/comments/ddue:dduexml/ddue:remarks[1]" />
    </xsl:if>
		<!-- example -->
      <xsl:if test="not($securityCriticalSection)">
		<xsl:apply-templates select="/document/comments/ddue:dduexml/ddue:codeExamples" />
      </xsl:if>
    <!-- permissions -->
      <xsl:if test="not($securityCriticalSection)">
    <xsl:call-template name="permissionsSection"/>
      </xsl:if>
    <!-- inheritance -->
    <xsl:apply-templates select="/document/reference/family" />
		<!-- other comment sections -->
    <xsl:if test="$subgroup='class' or $subgroup='structure'">
      <xsl:call-template name="threadSafety" />
    </xsl:if>
    <xsl:if test="not($group='list' or $group='namespace' or $group='root')">
      <!--platforms-->
      <xsl:apply-templates select="/document/reference/platforms" />
      <!--versions-->
      <xsl:apply-templates select="/document/reference/versions" />
    </xsl:if>
    <!-- see also -->
    <xsl:call-template name="seeAlsoSection"/>

    <!-- changed table section -->
    <xsl:call-template name="writeChangedTable" />

  </xsl:template> 

	<xsl:template name="obsoleteSection">
    <p>
      <include item="ObsoleteBoilerPlate">
        <parameter>
          <xsl:value-of select="$subgroup"/>
        </parameter>
      </include>
      <xsl:for-each select="/document/comments/ddue:dduexml/ddue:obsoleteCodeEntity">
				<xsl:text> </xsl:text>
				<include item="nonobsoleteAlternative">
					<parameter><xsl:apply-templates select="ddue:codeEntityReference" /></parameter>
				</include>
			</xsl:for-each>
		</p>
  </xsl:template>

  <xsl:template name="internalOnly">
    <xsl:if test="/document/comments/ddue:dduexml/ddue:internalOnly or /document/reference/containers/ddue:internalOnly">
      <div id="internalonly" class="seeAlsoNoToggleSection">
        <p/>
        <include item="internalOnly" />
      </div>
    </xsl:if>
  </xsl:template>
	
	<xsl:template name="getParameterDescription">
		<xsl:param name="name" />
		<xsl:choose>
      <xsl:when test="normalize-space(/document/comments/ddue:dduexml/ddue:parameters[1]/ddue:parameter) != ''">
        <span sdata="authoredParameterSummary">
        <xsl:apply-templates select="/document/comments/ddue:dduexml/ddue:parameters[1]/ddue:parameter[string(ddue:parameterReference)=$name]/ddue:content" />
        </span>
      </xsl:when>
      <xsl:otherwise>
        <xsl:apply-templates select="/document/comments/ddue:dduexml/ddue:parameters[2]/ddue:parameter[string(ddue:parameterReference)=$name]/ddue:content" />
      </xsl:otherwise>
    </xsl:choose>
	</xsl:template>

	<xsl:template name="getReturnsDescription">
		<xsl:choose>
      <xsl:when test="normalize-space(/document/comments/ddue:dduexml/ddue:returnValue[1]) != ''">
        <span sdata="authoredValueSummary">
        <xsl:apply-templates select="/document/comments/ddue:dduexml/ddue:returnValue[1]" />
        </span>
      </xsl:when>
      <xsl:otherwise>
        <xsl:apply-templates select="/document/comments/ddue:dduexml/ddue:returnValue[2]" />
      </xsl:otherwise>
    </xsl:choose>
	</xsl:template>

  <xsl:template match="returns">
    <xsl:choose>
      <xsl:when test="$api-subgroup='field' and normalize-space(/document/comments/ddue:dduexml/ddue:returnValue[1]) = '' and normalize-space(/document/comments/ddue:dduexml/ddue:returnValue[2]) = ''"/>
      <xsl:otherwise>
    <div id="returns">
      <xsl:call-template name="subSection">
        <xsl:with-param name="title">
          <include>
            <!-- title is propertyValueTitle or methodValueTitle or fieldValueTitle -->
            <xsl:attribute name="item">
              <xsl:value-of select="$api-subgroup" />
              <xsl:text>ValueTitle</xsl:text>
            </xsl:attribute>
          </include>
        </xsl:with-param>
        <xsl:with-param name="content">
          <include item="typeLink">
            <parameter>
              <xsl:apply-templates select="*[1]" mode="link">
                <xsl:with-param name="qualified" select="true()" />
              </xsl:apply-templates>
            </parameter>
          </include>
          <br />
          <xsl:call-template name="getReturnsDescription" />
        </xsl:with-param>
      </xsl:call-template>
    </div>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
	<xsl:template match="templates">
    <div id="genericParameters">
		<xsl:call-template name="subSection">
      <xsl:with-param name="title"><include item="templatesTitle" /></xsl:with-param>
			<xsl:with-param name="content">
					<xsl:for-each select="template">
						<xsl:variable name="parameterName" select="@name" />
              <dl paramName="{$parameterName}">
						<dt>
							<span class="parameter"><xsl:value-of select="$parameterName"/></span>
						</dt>
						<dd>
              		<xsl:apply-templates select="/document/comments/ddue:dduexml/ddue:genericParameters/ddue:genericParameter[string(ddue:parameterReference)=$parameterName]/ddue:content" />
            </dd>
				</dl>
            </xsl:for-each>
			</xsl:with-param>
		</xsl:call-template>
    </div>
	</xsl:template>

	<xsl:template name="getElementDescription">
    <xsl:choose>
      <xsl:when test="normalize-space(ddue:summary[1]) != ''">
        <span sdata="memberAuthoredSummary">
        <xsl:apply-templates select="ddue:summary[1]/ddue:para/node()" />
        </span>
      </xsl:when>
      <xsl:otherwise>
        <xsl:apply-templates select="ddue:summary[2]/ddue:para/node()" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="getInternalOnlyDescription">
    <xsl:choose>
      <xsl:when test="ddue:internalOnly">
        <include item="infraStructure" />
      </xsl:when>
      <xsl:when test="count(element) &gt; 0">
        <xsl:variable name="internal">
          <xsl:for-each select="element">
            <xsl:if test="not(ddue:internalOnly)">
              <xsl:text>no</xsl:text>
            </xsl:if>
          </xsl:for-each>
        </xsl:variable>
        <xsl:if test="not(normalize-space($internal))">
          <include item="infraStructure" />
        </xsl:if>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="getOverloadSummary">
   
  </xsl:template>

  <xsl:template name="getOverloadSections">
    
  </xsl:template>
  
  <xsl:template match="syntax">
    <xsl:if test="count(*) > 0">
      <xsl:call-template name="section">
        <xsl:with-param name="toggleSwitch" select="'syntax'" />
        <xsl:with-param name="title">
          <include item="syntaxTitle"/>
        </xsl:with-param>
        <xsl:with-param name="content">
          <div id="syntaxCodeBlocks" class="code">
            <xsl:call-template name="syntaxBlocks" />
          </div>
          <!-- parameters & return value -->
          <xsl:apply-templates select="/document/reference/templates" />
          <xsl:apply-templates select="/document/reference/parameters" />
          <xsl:apply-templates select="/document/reference/returns" />
          <xsl:apply-templates select="/document/reference/implements" />
          <!-- usage note for extension methods -->
          <xsl:if test="/document/reference/attributes/attribute/type[@api='T:System.Runtime.CompilerServices.ExtensionAttribute'] and boolean($api-subgroup='method')">
            <xsl:call-template name="subSection">
              <xsl:with-param name="title">
                <include item="extensionUsageTitle" />
              </xsl:with-param>
              <xsl:with-param name="content">
                <include item="extensionUsageText">
                  <parameter>
                    <xsl:apply-templates select="/document/reference/parameters/parameter[1]/type" mode="link" />
                  </parameter>
                </include>
              </xsl:with-param>
            </xsl:call-template>
          </xsl:if>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>
  
  
	<!-- DDUEXML elements that behave differently in conceptual and reference -->

	<xsl:template match="ddue:exceptions">
    <xsl:if test="normalize-space(.)">
		<xsl:call-template name="section">
      <xsl:with-param name="toggleSwitch" select="'ddueExceptions'"/>
			<xsl:with-param name="title"><include item="exceptionsTitle" /></xsl:with-param>
			<xsl:with-param name="content">
				<xsl:choose>
					<xsl:when test="ddue:exception">
            <div class="tableSection">
            <table width="100%" cellspacing="2" cellpadding="5" frame="lhs">
							<tr>
								<th class="exceptionNameColumn"><include item="exceptionNameHeader" /></th>
								<th class="exceptionConditionColumn"><include item="exceptionConditionHeader" /></th>
							</tr>
							<xsl:for-each select="ddue:exception">
								<tr>
									<td>
                    <xsl:apply-templates select="ddue:codeEntityReference" />
                  </td>
									<td>
                    <xsl:apply-templates select="ddue:content" />
                  </td>
								</tr>
							</xsl:for-each>
						</table>
            </div>
					</xsl:when>
					<xsl:otherwise>
						<xsl:apply-templates />
					</xsl:otherwise>
				</xsl:choose>
			</xsl:with-param>
		</xsl:call-template>
    </xsl:if>
	</xsl:template>

  <xsl:template name="permissionsSection">
    <!-- the containers/library/noAptca is added to reflection data by the ApplyVsDocModel transform -->
    <xsl:variable name="showAptcaBoilerplate" select="boolean(/document/reference/containers/library/noAptca and $omitAptcaBoilerplate!='true')"/>
    <xsl:if test="/document/comments/ddue:dduexml/ddue:permissions[normalize-space(.)] or $showAptcaBoilerplate">
      <xsl:call-template name="section">
        <xsl:with-param name="toggleSwitch" select="'permissions'" />
        <xsl:with-param name="title">
          <include item="permissionsTitle" />
        </xsl:with-param>
        <xsl:with-param name="content">
          <ul>
            <xsl:for-each select="/document/comments/ddue:dduexml/ddue:permissions/ddue:permission">
              <li>
                <xsl:apply-templates select="ddue:codeEntityReference"/>&#160;<xsl:apply-templates select="ddue:content"/>
              </li>
            </xsl:for-each>
            <xsl:if test="$showAptcaBoilerplate">
              <li>
                <include item="aptca" />
              </li>
            </xsl:if>
          </ul>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>
  
  <xsl:template match="ddue:codeExample">
    <xsl:apply-templates />
  </xsl:template>

  <xsl:template name="runningHeader">
   <include item="runningHeaderText" />
  </xsl:template>

  <xsl:template name="memberIntro">
    <xsl:if test="$subgroup='members'">
      <p>
        <xsl:apply-templates select="/document/reference/containers/ddue:summary"/>
      </p>
    </xsl:if>
    <xsl:call-template name="memberIntroBoilerplate"/>
  </xsl:template>

  <xsl:template name="codelangAttributes">
    <xsl:call-template name="mshelpCodelangAttributes">
      <xsl:with-param name="snippets" select="/document/comments/ddue:dduexml/ddue:codeExamples/ddue:codeExample/ddue:legacy/ddue:content/ddue:snippets/ddue:snippet" />
    </xsl:call-template>
  </xsl:template>

  <xsl:template match="ddue:codeEntityReference" mode="abstract">
    <xsl:call-template name="subString">
      <xsl:with-param name="name" select="." />
    </xsl:call-template>
  </xsl:template>

  <!-- Footer stuff -->
  
	<xsl:template name="foot">
    <div id="footer">
      <div class="footerLine">
        <img width="100%" height="3px">
          <includeAttribute name="src" item="iconPath">
            <parameter>footer.gif</parameter>
          </includeAttribute>
          <includeAttribute name="title" item="footerImage" />
        </img>
      </div>

      <include item="footer">
        <parameter>
          <xsl:value-of select="$key"/>
        </parameter>
        <parameter>
          <xsl:call-template name="topicTitlePlain"/>
        </parameter>
        <parameter>
          <xsl:value-of select="/document/metadata/item[@id='PBM_FileVersion']" />
        </parameter>
        <parameter>
          <xsl:value-of select="/document/metadata/attribute[@name='TopicVersion']" />
        </parameter>
      </include>
    </div>
	</xsl:template>

  <xsl:template name="seeAlsoSection">

    <xsl:if test="$hasSeeAlsoSection">
      <xsl:call-template name="section">
        <xsl:with-param name="toggleSwitch" select="'seeAlso'"/>
        <xsl:with-param name="title">
          <include item="relatedTitle" />
        </xsl:with-param>
        <xsl:with-param name="content">
          <xsl:choose>
            <xsl:when test="count(/document/comments/ddue:dduexml/ddue:relatedTopics/*) > 0">
              <xsl:apply-templates select="/document/comments/ddue:dduexml/ddue:relatedTopics" mode="seeAlso">
                <xsl:with-param name="autoGenerateLinks" select="'true'" />
              </xsl:apply-templates>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name="subSection">
                <xsl:with-param name="title">
                  <include item="SeeAlsoReference"/>
                </xsl:with-param>
                <xsl:with-param name="content">
                  <xsl:call-template name="autogenSeeAlsoLinks"/>
                </xsl:with-param>
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>
          
        </xsl:with-param>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <xsl:template name="writeChangedTable">
    <xsl:if test="/document/comments/ddue:dduexml//ddue:section/ddue:title = 'Change History' and (/document/comments/ddue:dduexml//ddue:section[ddue:title = 'Change History']/ddue:content/ddue:table and /document/comments/ddue:dduexml//ddue:section[ddue:title = 'Change History']/ddue:content/ddue:table/ddue:row/ddue:entry[normalize-space(.)])">
      <xsl:apply-templates select="/document/comments/ddue:dduexml//ddue:section[ddue:title = 'Change History']">
        <xsl:with-param name="showChangedHistoryTable" select="true()" />
      </xsl:apply-templates>
    </xsl:if>
  </xsl:template>

</xsl:stylesheet>
