<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="2.0"
								xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
								xmlns:MSHelp="http://msdn.microsoft.com/mshelp"
								xmlns:ddue="http://ddue.schemas.microsoft.com/authoring/2003/5"
								xmlns:msxsl="urn:schemas-microsoft-com:xslt"
								exclude-result-prefixes="msxsl"
>
	<!-- ======================================================================================== -->

	<xsl:import href="globalTemplates.xsl"/>

	<!-- ======================================================================================== -->

	<xsl:template name="t_insertMetadataHelp20">
		<xsl:if test="$metadata='true'">
			<xml>
				<!-- mshelp metadata -->

				<!-- insert toctitle -->
				<xsl:if test="normalize-space(/document/metadata/tableOfContentsTitle) and (/document/metadata/tableOfContentsTitle != /document/metadata/title)">
					<MSHelp:TOCTitle Title="{/document/metadata/tableOfContentsTitle}" />
				</xsl:if>

				<!-- link index -->
				<MSHelp:Keyword Index="A"
												Term="{$key}" />

				<!-- authored NamedUrlIndex -->
				<xsl:for-each select="/document/metadata/keyword[@index='NamedUrlIndex']">
					<MSHelp:Keyword Index="NamedUrlIndex">
						<xsl:attribute name="Term">
							<xsl:value-of select="text()" />
						</xsl:attribute>
					</MSHelp:Keyword>
				</xsl:for-each>

				<!-- authored K -->
				<xsl:variable name="v_docset"
											select="translate(/document/metadata/attribute[@name='DocSet'][1]/text(),$g_allUpperCaseLetters,'abcdefghijklmnopqrstuvwxyz ')"/>
				<xsl:for-each select="/document/metadata/keyword[@index='K']">
					<xsl:variable name="v_nestedKeywordText">
						<xsl:call-template name="t_nestedKeywordText"/>
					</xsl:variable>
					<xsl:choose>
						<xsl:when test="not(contains(text(),'[')) and ($v_docset='avalon' or $v_docset='wpf' or $v_docset='wcf' or $v_docset='windowsforms')">
							<MSHelp:Keyword Index="K">
								<includeAttribute name="Term"
																	item="meta_kIndexTermWithTechQualifier">
									<parameter>
										<xsl:value-of select="text()"/>
									</parameter>
									<parameter>
										<xsl:value-of select="$v_docset"/>
									</parameter>
									<parameter>
										<xsl:value-of select="$v_nestedKeywordText"/>
									</parameter>
								</includeAttribute>
							</MSHelp:Keyword>
						</xsl:when>
						<xsl:otherwise>
							<MSHelp:Keyword Index="K"
															Term="{concat(text(),$v_nestedKeywordText)}" />
						</xsl:otherwise>
					</xsl:choose>
				</xsl:for-each>

				<!-- authored S -->
				<xsl:for-each select="/document/metadata/keyword[@index='S']">
					<MSHelp:Keyword Index="S">
						<xsl:attribute name="Term">
							<xsl:value-of select="text()" />
							<xsl:for-each select="keyword[@index='S']">
								<xsl:text>, </xsl:text>
								<xsl:value-of select="text()"/>
							</xsl:for-each>
						</xsl:attribute>
					</MSHelp:Keyword>
					<!-- S index keywords need to be converted to F index keywords -->
					<MSHelp:Keyword Index="F">
						<xsl:attribute name="Term">
							<xsl:value-of select="text()" />
							<xsl:for-each select="keyword[@index='S']">
								<xsl:text>, </xsl:text>
								<xsl:value-of select="text()"/>
							</xsl:for-each>
						</xsl:attribute>
					</MSHelp:Keyword>
				</xsl:for-each>

				<!-- authored F -->
				<xsl:for-each select="/document/metadata/keyword[@index='F']">
					<MSHelp:Keyword Index="F">
						<xsl:attribute name="Term">
							<xsl:value-of select="text()" />
							<xsl:for-each select="keyword[@index='F']">
								<xsl:text>, </xsl:text>
								<xsl:value-of select="text()"/>
							</xsl:for-each>
						</xsl:attribute>
					</MSHelp:Keyword>
				</xsl:for-each>

				<!-- authored B -->
				<xsl:for-each select="/document/metadata/keyword[@index='B']">
					<MSHelp:Keyword Index="B">
						<xsl:attribute name="Term">
							<xsl:value-of select="text()" />
							<xsl:for-each select="keyword[@index='B']">
								<xsl:text>, </xsl:text>
								<xsl:value-of select="text()"/>
							</xsl:for-each>
						</xsl:attribute>
					</MSHelp:Keyword>
				</xsl:for-each>

				<!-- Topic version -->
				<MSHelp:Attr Name="RevisionNumber"
										 Value="{/document/topic/@revisionNumber}" />

				<!-- Asset ID -->
				<MSHelp:Attr Name="AssetID"
										 Value="{/document/topic/@id}" />

				<!-- Abstract -->
				<xsl:variable name="v_abstract" select="normalize-space(string(/document/topic//ddue:para[1]))" />
				<xsl:choose>
					<xsl:when test="string-length($v_abstract) &gt; 254">
						<MSHelp:Attr Name="Abstract" Value="{concat(substring($v_abstract,1,250), ' ...')}" />
					</xsl:when>
					<xsl:when test="string-length($v_abstract) &gt; 0 and $v_abstract != '&#160;'">
						<MSHelp:Attr Name="Abstract" Value="{$v_abstract}" />
					</xsl:when>
				</xsl:choose>

				<!-- Auto-generate DevLang attributes based on the snippets -->
				<xsl:for-each select="//*[@language]">
					<xsl:if test="not(@language=preceding::*/@language)">
						<xsl:variable name="v_codeLang">
							<xsl:call-template name="t_codeLang">
								<xsl:with-param name="p_codeLang" select="@language"/>
							</xsl:call-template>
						</xsl:variable>
						<xsl:choose>
							<xsl:when test="$v_codeLang='none' or $v_codeLang='other'"/>
							<!-- If $v_codeLang is already authored, then do nothing -->
							<xsl:when test="/document/metadata/attribute[@name='codelang']/text() = $v_codeLang"/>
							<xsl:otherwise>
								<MSHelp:Attr Name="DevLang">
									<includeAttribute name="Value" item="metaLang_{$v_codeLang}"/>
								</MSHelp:Attr>
							</xsl:otherwise>
						</xsl:choose>
					</xsl:if>
				</xsl:for-each>

				<!-- authored attributes -->
				<xsl:for-each select="/document/metadata/attribute">
					<MSHelp:Attr Name="{@name}" Value="{text()}" />
				</xsl:for-each>

				<!-- TopicType attribute -->
				<xsl:for-each select="/document/topic/*[1]">
					<MSHelp:Attr Name="TopicType">
						<includeAttribute name="Value" item="meta_mshelp_topicType_{local-name()}"/>
					</MSHelp:Attr>
				</xsl:for-each>

				<!-- Locale attribute -->
				<MSHelp:Attr Name="Locale">
					<includeAttribute name="Value" item="locale"/>
				</MSHelp:Attr>

			</xml>
		</xsl:if>
	</xsl:template>

</xsl:stylesheet>
