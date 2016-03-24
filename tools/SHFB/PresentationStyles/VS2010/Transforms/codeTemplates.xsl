<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
								version="2.0"
								xmlns:msxsl="urn:schemas-microsoft-com:xslt"
								xmlns:MSHelp="http://msdn.microsoft.com/mshelp"
								xmlns:ddue="http://ddue.schemas.microsoft.com/authoring/2003/5"
								xmlns:xlink="http://www.w3.org/1999/xlink"
	>
	<!-- ======================================================================================== -->

	<xsl:import href="globalTemplates.xsl"/>

	<!-- ============================================================================================
	Code languages
	============================================================================================= -->

	<!-- This gets the language ID for syntax section and code example titles -->
	<xsl:template name="t_codeLang">
		<xsl:param name="p_codeLang"/>
		<xsl:variable name="v_codeLangLC" select="translate($p_codeLang,$g_allUpperCaseLetters,$g_allLowerCaseLetters)"/>
		<xsl:choose>
			<!-- Languages without a syntax generator.  The presentation style content files will contain any required
					 resource items for these (i.e. devlang_HTML). -->
			<xsl:when test="$v_codeLangLC = 'html' or $v_codeLangLC = 'htm'">
				<xsl:text>HTML</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangLC = 'pshell' or $v_codeLangLC = 'powershell' or $v_codeLangLC = 'ps1'">
				<xsl:text>PShell</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangLC = 'py'">
				<xsl:text>Python</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangLC = 'sql' or $v_codeLangLC = 'sqlserver' or $v_codeLangLC = 'sql server'">
				<xsl:text>SQL</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangLC = 'vbs' or $v_codeLangLC = 'vbscript'">
				<xsl:text>VBScript</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangLC = 'vb-c#' or $v_codeLangLC = 'visualbasicandcsharp'">
				<xsl:text>VisualBasicAndCSharp</xsl:text>
			</xsl:when>
			<xsl:when test="$v_codeLangLC = 'xml' or $v_codeLangLC = 'xmllang' or $v_codeLangLC = 'xsl'">
				<xsl:text>XML</xsl:text>
			</xsl:when>
			<!-- Special case for XAML.  It has a syntax generator but we treat the code elements differently and must
					 use a common ID. -->
			<xsl:when test="$v_codeLangLC = 'xaml' or $v_codeLangLC = 'xamlusage'">
				<xsl:text>XAML</xsl:text>
			</xsl:when>
			<!-- None/other.  No resource items are needed for these. -->
			<xsl:when test="$v_codeLangLC = 'none' or $v_codeLangLC = 'other'">
				<xsl:value-of select="$v_codeLangLC"/>
			</xsl:when>
			<!-- If none of the above, assume it is a language with a syntax generator.  The syntax generator content
					 files will contain any required resource items for the language. -->
			<xsl:otherwise>
				<xsl:value-of select="$p_codeLang"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<!-- This gets the language name for metadata -->
	<xsl:template name="t_codeLangName">
		<xsl:param name="p_codeLang"/>
		<xsl:variable name="v_codeLangUnique">
			<xsl:call-template name="t_codeLang">
				<xsl:with-param name="p_codeLang" select="$p_codeLang"/>
			</xsl:call-template>
		</xsl:variable>
		<xsl:choose>
			<xsl:when test="$v_codeLangUnique = 'none' or $v_codeLangUnique = 'other'" />
			<xsl:otherwise>
				<xsl:value-of select="$v_codeLangUnique"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<!-- ============================================================================================
	Syntax and Code sections
	============================================================================================= -->

	<xsl:template name="t_putSyntaxSections">
		<xsl:param name="p_nodes"/>

		<xsl:variable name="v_id" select="generate-id(msxsl:node-set($p_nodes))" />

		<!-- Count non-XAML snippets and XAML snippets with something other than boilerplate content -->
		<xsl:variable name="v_nodeCount" select="count(msxsl:node-set($p_nodes)/self::node()[@codeLanguage != 'XAML' or
			(@codeLanguage = 'XAML' and boolean(./div[@class='xamlAttributeUsageHeading' or
			@class='xamlObjectElementUsageHeading' or @class='xamlContentElementUsageHeading' or
			@class='xamlPropertyElementUsageHeading']))])" />

		<div class="OH_CodeSnippetContainer">
			<div class="OH_CodeSnippetContainerTabs">
				<xsl:choose>
					<xsl:when test="$v_nodeCount = 1">
						<div class="OH_CodeSnippetContainerTabLeftActive" id="{$v_id}_tabimgleft"><xsl:text> </xsl:text></div>
					</xsl:when>
					<xsl:otherwise>
						<div class="OH_CodeSnippetContainerTabLeft" id="{$v_id}_tabimgleft"><xsl:text> </xsl:text></div>
					</xsl:otherwise>
				</xsl:choose>

				<xsl:for-each select="msxsl:node-set($p_nodes)">
					<xsl:choose>
						<!-- Suppress tabs for boilerplate XAML which isn't currently shown -->
						<xsl:when test="@codeLanguage='XAML' and not(boolean(./div[
										@class='xamlAttributeUsageHeading' or @class='xamlObjectElementUsageHeading' or
										@class='xamlContentElementUsageHeading' or @class='xamlPropertyElementUsageHeading']))" />
						<xsl:otherwise>
							<div id="{$v_id}_tab{position()}">
								<xsl:attribute name="class">
									<xsl:choose>
										<xsl:when test="$v_nodeCount = 1">
											<xsl:text>OH_CodeSnippetContainerTabSolo</xsl:text>
										</xsl:when>
										<xsl:when test="position() = 1">
											<xsl:text>OH_CodeSnippetContainerTabFirst</xsl:text>
										</xsl:when>
										<xsl:otherwise>
											<xsl:text>OH_CodeSnippetContainerTab</xsl:text>
										</xsl:otherwise>
									</xsl:choose>
								</xsl:attribute>
								<xsl:choose>
									<xsl:when test="$v_nodeCount = 1">
										<include item="devlang_{@codeLanguage}" />
									</xsl:when>
									<xsl:otherwise>
										<!-- Use onclick rather than href or HV 2.0 messes up the link -->
										<a href="#" onclick="javascript:ChangeTab('{$v_id}','{@style}','{position()}','{$v_nodeCount}');return false;">
											<include item="devlang_{@codeLanguage}" />
										</a>
									</xsl:otherwise>
								</xsl:choose>
							</div>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:for-each>

				<xsl:choose>
					<xsl:when test="$v_nodeCount = 1">
						<div class="OH_CodeSnippetContainerTabRightActive" id="{$v_id}_tabimgright"><xsl:text> </xsl:text></div>
					</xsl:when>
					<xsl:otherwise>
						<div class="OH_CodeSnippetContainerTabRight" id="{$v_id}_tabimgright"><xsl:text> </xsl:text></div>
					</xsl:otherwise>
				</xsl:choose>
			</div>

			<div class="OH_CodeSnippetContainerCodeCollection">
				<div class="OH_CodeSnippetToolBar">
					<div class="OH_CodeSnippetToolBarText">
						<a id="{$v_id}_copyCode" href="#" onclick="javascript:CopyToClipboard('{$v_id}');return false;">
							<includeAttribute name="title" item="copyCode" />
							<include item="copyCode" />
						</a>
					</div>
				</div>

				<xsl:for-each select="msxsl:node-set($p_nodes)">
					<xsl:choose>
						<!-- Suppress snippets for boilerplate XAML which isn't currently shown -->
						<xsl:when test="@codeLanguage='XAML' and not(boolean(./div[
										@class='xamlAttributeUsageHeading' or @class='xamlObjectElementUsageHeading' or
										@class='xamlContentElementUsageHeading' or @class='xamlPropertyElementUsageHeading']))" />
						<xsl:otherwise>
							<div id="{$v_id}_code_Div{position()}" class="OH_CodeSnippetContainerCode">
								<xsl:attribute name="style">
									<xsl:choose>
										<xsl:when test="$v_nodeCount = 1 or position() = 1">
											<xsl:text>display: block</xsl:text>
										</xsl:when>
										<xsl:otherwise>
											<xsl:text>display: none</xsl:text>
										</xsl:otherwise>
									</xsl:choose>
								</xsl:attribute>
								<xsl:choose>
									<xsl:when test="@codeLanguage='XAML'">
										<xsl:call-template name="XamlSyntaxBlock" />
									</xsl:when>
									<xsl:otherwise>
										<pre xml:space="preserve"><xsl:copy-of select="node()"/></pre>
									</xsl:otherwise>
								</xsl:choose>
							</div>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:for-each>
			</div>
		</div>

		<!-- Register the tab set even for single tabs as we may need to hide the Copy link -->
		<script type="text/javascript">AddLanguageTabSet("<xsl:value-of select="$v_id" />");</script>
	</xsl:template>

	<xsl:template name="t_putCodeSections">
		<xsl:param name="p_nodes"/>

		<xsl:variable name="v_id" select="generate-id(msxsl:node-set($p_nodes))" />
		<xsl:variable name="v_nodeCount" select="count(msxsl:node-set($p_nodes))" />

		<div class="OH_CodeSnippetContainer">
			<xsl:choose>
				<!-- Omit the tab if there is a title attribute with a single space -->
				<xsl:when test="$v_nodeCount = 1 and msxsl:node-set($p_nodes)//@title = ' '" />
				<xsl:otherwise>
					<div class="OH_CodeSnippetContainerTabs">
						<xsl:choose>
							<xsl:when test="$v_nodeCount = 1">
								<div class="OH_CodeSnippetContainerTabLeftActive" id="{$v_id}_tabimgleft">
									<xsl:text> </xsl:text>
								</div>
							</xsl:when>
							<xsl:otherwise>
								<div class="OH_CodeSnippetContainerTabLeft" id="{$v_id}_tabimgleft">
									<xsl:text> </xsl:text>
								</div>
							</xsl:otherwise>
						</xsl:choose>

						<xsl:for-each select="msxsl:node-set($p_nodes)">
							<div id="{$v_id}_tab{position()}">
								<xsl:attribute name="class">
									<xsl:choose>
										<xsl:when test="$v_nodeCount = 1">
											<xsl:text>OH_CodeSnippetContainerTabSolo</xsl:text>
										</xsl:when>
										<xsl:when test="@phantom and position() = 1">
											<xsl:text>OH_CodeSnippetContainerTabDisabled</xsl:text>
										</xsl:when>
										<xsl:when test="@phantom">
											<xsl:text>OH_CodeSnippetContainerTabDisabledNotFirst</xsl:text>
										</xsl:when>
										<xsl:when test="position() = 1">
											<xsl:text>OH_CodeSnippetContainerTabFirst</xsl:text>
										</xsl:when>
										<xsl:otherwise>
											<xsl:text>OH_CodeSnippetContainerTab</xsl:text>
										</xsl:otherwise>
									</xsl:choose>
								</xsl:attribute>
								<xsl:choose>
									<xsl:when test="$v_nodeCount = 1">
										<xsl:choose>
											<xsl:when test="@title">
												<xsl:value-of select="@title" />
											</xsl:when>
											<xsl:otherwise>
												<include item="devlang_{@codeLanguage}" />
											</xsl:otherwise>
										</xsl:choose>
									</xsl:when>
									<xsl:otherwise>
										<!-- Use onclick rather than href or HV 2.0 messes up the link -->
										<a href="#" onclick="javascript:ChangeTab('{$v_id}','{@style}','{position()}','{$v_nodeCount}');return false;">
											<include item="devlang_{@codeLanguage}" />
										</a>
									</xsl:otherwise>
								</xsl:choose>
							</div>
						</xsl:for-each>

						<xsl:choose>
							<xsl:when test="$v_nodeCount = 1">
								<div class="OH_CodeSnippetContainerTabRightActive" id="{$v_id}_tabimgright">
									<xsl:text> </xsl:text>
								</div>
							</xsl:when>
							<xsl:otherwise>
								<div class="OH_CodeSnippetContainerTabRight" id="{$v_id}_tabimgright">
									<xsl:text> </xsl:text>
								</div>
							</xsl:otherwise>
						</xsl:choose>
					</div>
				</xsl:otherwise>
			</xsl:choose>

			<div class="OH_CodeSnippetContainerCodeCollection">
				<div class="OH_CodeSnippetToolBar">
					<div class="OH_CodeSnippetToolBarText">
						<a id="{$v_id}_copyCode" href="#" onclick="javascript:CopyToClipboard('{$v_id}');return false;">
							<includeAttribute name="title" item="copyCode" />
							<include item="copyCode" />
						</a>
					</div>
				</div>

				<xsl:for-each select="msxsl:node-set($p_nodes)">
					<div id="{$v_id}_code_Div{position()}" class="OH_CodeSnippetContainerCode">
						<xsl:attribute name="style">
							<xsl:choose>
								<xsl:when test="$v_nodeCount = 1 or position() = 1">
									<xsl:text>display: block</xsl:text>
								</xsl:when>
								<xsl:otherwise>
									<xsl:text>display: none</xsl:text>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:attribute>
						<xsl:choose>
							<xsl:when test="@phantom">
								<include item="noCodeExample" />
							</xsl:when>
							<xsl:otherwise>
								<pre xml:space="preserve"><xsl:copy-of select="node()"/></pre>
							</xsl:otherwise>
						</xsl:choose>
					</div>
				</xsl:for-each>
			</div>
		</div>

		<!-- Register the tab set even for single tabs as we may need to hide the Copy link -->
		<script type="text/javascript">AddLanguageTabSet("<xsl:value-of select="$v_id" />");</script>
	</xsl:template>

	<!-- ============================================================================================
	XAML Syntax
	============================================================================================= -->

	<xsl:template name="XamlSyntaxBlock">
		<!-- Branch based on page type -->
		<xsl:choose>
			<!-- Display boilerplate for page types that cannot be used in XAML -->
			<xsl:when test="$g_apiTopicSubGroup='method' or $g_apiTopicSubGroup='constructor' or
                      $g_apiTopicSubGroup='interface' or $g_apiTopicSubGroup='delegate' or
                      $g_apiTopicSubGroup='field'">
				<xsl:call-template name="ShowXamlSyntaxBoilerplate"/>
			</xsl:when>

			<!-- Class and structure -->
			<xsl:when test="$g_apiTopicSubGroup='class' or $g_apiTopicSubGroup='structure'">
				<xsl:choose>
					<xsl:when test="div[@class='xamlObjectElementUsageHeading']">
						<xsl:call-template name="ShowAutogeneratedXamlSyntax">
							<xsl:with-param name="autogenContent">
								<xsl:copy-of select="div[@class='xamlObjectElementUsageHeading']"/>
							</xsl:with-param>
						</xsl:call-template>
					</xsl:when>
					<xsl:otherwise>
						<xsl:call-template name="ShowXamlSyntaxBoilerplate">
							<xsl:with-param name="p_messageId">
								<xsl:copy-of select="."/>
							</xsl:with-param>
						</xsl:call-template>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:when>

			<!-- Enumeration -->
			<xsl:when test="$g_apiTopicSubGroup='enumeration'">
				<xsl:choose>
					<xsl:when test="div[@class='nonXamlAssemblyBoilerplate']"/>
					<xsl:otherwise>
						<pre xml:space="preserve"><include item="enumerationOverviewXamlSyntax"/><xsl:text/></pre>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:when>

			<!-- Property -->
			<xsl:when test="$g_apiTopicSubGroup='property' or $g_apiTopicSubSubGroup='attachedProperty'">
				<!-- Property Element Usage -->
				<xsl:if test="div[@class='xamlPropertyElementUsageHeading' or @class='xamlContentElementUsageHeading']">
					<xsl:call-template name="ShowAutogeneratedXamlSyntax">
						<xsl:with-param name="autogenContent">
							<xsl:copy-of select="div[@class='xamlPropertyElementUsageHeading' or @class='xamlContentElementUsageHeading']"/>
						</xsl:with-param>
					</xsl:call-template>
				</xsl:if>
				<!-- Attribute Usage -->
				<xsl:if test="div[@class='xamlAttributeUsageHeading']">
					<xsl:call-template name="ShowAutogeneratedXamlSyntax">
						<xsl:with-param name="autogenContent">
							<xsl:copy-of select="div[@class='xamlAttributeUsageHeading']"/>
						</xsl:with-param>
					</xsl:call-template>
				</xsl:if>
				<!-- Show auto-generated boilerplate if no other content to override it -->
				<xsl:if test="not(div[@class='xamlPropertyElementUsageHeading' or
								@class='xamlContentElementUsageHeading' or @class='xamlAttributeUsageHeading'])">
					<xsl:call-template name="ShowXamlSyntaxBoilerplate">
						<xsl:with-param name="p_messageId">
							<xsl:copy-of select="div/*"/>
						</xsl:with-param>
					</xsl:call-template>
				</xsl:if>
			</xsl:when>

			<!-- Event -->
			<xsl:when test="$g_apiTopicSubGroup='event' or $g_apiTopicSubSubGroup='attachedEvent'">
				<!-- If XamlSyntaxUsage component generated an Attribute Usage block, this template will show it -->
				<xsl:call-template name="ShowAutogeneratedXamlSyntax">
					<xsl:with-param name="autogenContent">
						<xsl:copy-of select="div[@class='xamlAttributeUsageHeading']"/>
					</xsl:with-param>
				</xsl:call-template>
				<!-- If XamlSyntaxUsage component generated a boilerplate block, this template will show it -->
				<xsl:call-template name="ShowXamlSyntaxBoilerplate">
					<xsl:with-param name="p_messageId">
						<xsl:copy-of select="div/*"/>
					</xsl:with-param>
				</xsl:call-template>
			</xsl:when>

		</xsl:choose>
	</xsl:template>

	<!-- Displays one of the standard XAML boilerplate strings. -->
	<xsl:template name="ShowXamlSyntaxBoilerplate">
		<xsl:param name="p_messageId"/>

		<!-- Do not show any XAML syntax boilerplate strings -->
		<xsl:variable name="boilerplateId"/>

		<!-- If future requirements call for showing one or more boilerplate strings for XAML, use the commented out
				 code to specify the ids of the shared content items to include.
         NOTE: The markup like div/@class='interfaceOverviewXamlSyntax' is added by XamlUsageSyntax.cs in
				 BuildAssembler. -->
		<!--
    <xsl:variable name="boilerplateId">
      <xsl:value-of select="div/@class[.='interfaceOverviewXamlSyntax' or
                    .='propertyXamlSyntax_abstractType' or                    
                    .='classXamlSyntax_abstract']"/>
    </xsl:variable>
    -->

		<xsl:if test="$boilerplateId != ''">
			<pre xml:space="preserve"><include item="{$boilerplateId}">
          <xsl:choose>
            <xsl:when test="$p_messageId !='' or (count(msxsl:node-set($p_messageId)/*) &gt; 0)">
              <parameter><xsl:copy-of select="msxsl:node-set($p_messageId)"/></parameter>
            </xsl:when>
            <!-- Make sure we at least pass in an empty param because some boilerplates expect them -->
            <xsl:otherwise>
              <parameter/>
            </xsl:otherwise>
          </xsl:choose>
        </include><xsl:text/></pre>
		</xsl:if>
	</xsl:template>

	<!-- Displays the auto-generated XAML syntax for page types other than enumerations -->
	<xsl:template name="ShowAutogeneratedXamlSyntax">
		<xsl:param name="autogenContent"/>
		<xsl:if test="count(msxsl:node-set($autogenContent))>0">
			<xsl:for-each select="msxsl:node-set($autogenContent)/div">
				<pre xml:space="preserve"><xsl:copy-of select="node()"/><xsl:text/></pre>
			</xsl:for-each>
		</xsl:if>
	</xsl:template>

</xsl:stylesheet>
