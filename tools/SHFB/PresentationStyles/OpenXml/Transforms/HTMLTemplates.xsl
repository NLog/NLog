<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="2.0"
								xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
								xmlns:msxsl="urn:schemas-microsoft-com:xslt"
								xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main"
								xmlns:o="urn:schemas-microsoft-com:office:office"
								xmlns:v="urn:schemas-microsoft-com:vml"
>

	<!-- ==========================================================================================================
	HTML elements.  Open XML does not support HTML elements but they are prevalent in XML comments so we'll make
	our best attempt at converting those with Open XML equivalents and removing those that don't but will pull in
	their content.
	=========================================================================================================== -->

	<!-- These HTML elements are dropped but we'll include their content if any -->
	<xsl:template match="abbr|acronym|area|del|div|dl|font|ins|map" name="t_HtmlContentOnly">
		<xsl:apply-templates />
	</xsl:template>

	<!-- Treat pre elements like code blocks -->
	<xsl:template match="pre" name="t_HtmlPre">
		<w:tbl>
			<w:tblPr>
				<w:tblStyle w:val="CodeTable"/>
				<w:tblW w:w="5000" w:type="pct"/>
				<w:tblLook w:firstRow="0" w:noHBand="1" w:noVBand="1"/>
			</w:tblPr>
			<w:tr>
				<w:tc>
					<w:p>
						<xsl:apply-templates mode="preserveFormatting"/>
					</w:p>
				</w:tc>
			</w:tr>
		</w:tbl>
		<w:p>
			<w:pPr>
				<w:spacing w:after="0" />
			</w:pPr>
		</w:p>
	</xsl:template>

	<!-- Pass these elements through as-is.  The Open XML file builder task will convert them accordingly. -->
	<xsl:template match="a|br|img|span" name="t_HtmlPassthrough">
		<xsl:copy>
			<xsl:copy-of select="@*"/>
			<xsl:apply-templates/>
		</xsl:copy>
	</xsl:template>

	<xsl:template match="span" name="t_codeSpan" mode="preserveFormatting">
		<xsl:copy>
			<xsl:copy-of select="@*"/>
			<xsl:apply-templates/>
		</xsl:copy>
	</xsl:template>

	<!-- Normal paragraph.  Note that unlike HTML, self-closing and empty paragraphs will be rendered in the
			 document and will consume space.  However, we can't remove them as it could then combine text into a
			 single paragraph that is not intended to be combined.  Best to let the user sort it out later.  The fix
			 is to wrap the text in the paragraph elements and not use self-closing paragraphs. -->
	<xsl:template match="p" name="t_HtmlPara">
		<w:p>
			<xsl:choose>
				<!-- Indent the paragraph if it is within a block quote -->
				<xsl:when test="parent::blockquote">
					<w:pPr>
						<w:pStyle w:val="Quote" />
						<w:ind w:left="432" w:right="432" />
					</w:pPr>
				</xsl:when>
				<!-- In table header cells, keep them together with the next row to avoid splitting them across pages -->
				<xsl:when test="th">
					<w:pPr>
						<w:keepNext />
					</w:pPr>
				</xsl:when>
				<xsl:otherwise />
			</xsl:choose>
			<xsl:apply-templates />
		</w:p>
	</xsl:template>

	<xsl:template match="hr">
		<w:p>
			<w:r>
				<w:pict>
					<v:rect style="width:0;height:1.5pt" o:hr="t" o:hrstd="t" o:hralign="center" fillcolor="#a0a0a0"
						stroked="f" />
				</w:pict>
			</w:r>
		</w:p>		
	</xsl:template>

	<xsl:template match="b|strong" name="t_HtmlBold">
		<xsl:if test="normalize-space(.)">
			<span class="Bold">
				<xsl:apply-templates/>
			</span>
		</xsl:if>
	</xsl:template>

	<xsl:template match="i|em" name="t_HtmlItalic">
		<xsl:if test="normalize-space(.)">
			<span class="Emphasis">
				<xsl:apply-templates />
			</span>
		</xsl:if>
	</xsl:template>

	<xsl:template match="u" name="t_HtmlUnderline">
		<xsl:if test="normalize-space(.)">
			<span class="Underline">
				<xsl:apply-templates />
			</span>
		</xsl:if>
	</xsl:template>

	<xsl:template match="sub" name="t_HtmlSubscript">
		<xsl:if test="normalize-space(.)">
			<span class="Subscript">
				<xsl:apply-templates/>
			</span>
		</xsl:if>
	</xsl:template>

	<xsl:template match="sup" name="t_HtmlSuperscript">
		<xsl:if test="normalize-space(.)">
			<span class="Superscript">
				<xsl:apply-templates/>
			</span>
		</xsl:if>
	</xsl:template>

	<xsl:template match="blockquote" name="t_HtmlBlockQuote">
		<xsl:if test="normalize-space(.)">
			<xsl:choose>
				<xsl:when test="p">
					<xsl:apply-templates/>
				</xsl:when>
				<xsl:otherwise>
					<w:p>
						<w:pPr>
							<w:pStyle w:val="Quote" />
							<w:ind w:left="432" w:right="432" />
						</w:pPr>
						<xsl:apply-templates/>
					</w:p>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:if>
	</xsl:template>

	<xsl:template match="h1|h2|h3|h4|h5|h6" name="t_HtmlHeading">
		<w:p>
			<w:pPr>
				<xsl:choose>
					<xsl:when test="local-name() = 'h1'">
						<w:pStyle w:val="Heading1" />
					</xsl:when>
					<xsl:when test="local-name() = 'h2'">
						<w:pStyle w:val="Heading2" />
					</xsl:when>
					<xsl:when test="local-name() = 'h3'">
						<w:pStyle w:val="Heading3" />
					</xsl:when>
					<xsl:when test="local-name() = 'h4'">
						<w:pStyle w:val="Heading4" />
					</xsl:when>
					<xsl:when test="local-name() = 'h5'">
						<w:pStyle w:val="Heading5" />
					</xsl:when>
					<xsl:otherwise>
						<w:pStyle w:val="Heading6" />
					</xsl:otherwise>
				</xsl:choose>
			</w:pPr>
			<xsl:apply-templates />
		</w:p>
	</xsl:template>

	<xsl:template match="table" name="t_HtmlTable">
		<w:tbl>
			<w:tblPr>
				<w:tblStyle w:val="GeneralTable"/>
				<w:tblW w:w="5000" w:type="pct"/>
				<xsl:choose>
					<xsl:when test="tr/th">
						<w:tblLook w:firstRow="1" w:noHBand="1" w:noVBand="1"/>
					</xsl:when>
					<xsl:otherwise>
						<w:tblLook w:firstRow="0" w:noHBand="1" w:noVBand="1"/>
					</xsl:otherwise>
				</xsl:choose>
			</w:tblPr>
			<xsl:apply-templates/>
		</w:tbl>
		<w:p>
			<w:pPr>
				<w:spacing w:after="0" />
			</w:pPr>
		</w:p>
	</xsl:template>

	<xsl:template match="tr" name="t_HtmlTableRow">
		<w:tr>
			<xsl:if test="th">
				<w:trPr>
					<w:cnfStyle w:firstRow="1" />
				</w:trPr>
			</xsl:if>
			<xsl:apply-templates/>
		</w:tr>
	</xsl:template>

	<xsl:template match="th|td" name="t_HtmlTableCell">
		<w:tc>
			<xsl:apply-templates/>
		</w:tc>
	</xsl:template>

	<!-- The Open XML file builder will convert lists accordingly -->
	<xsl:template match="ol|ul" name="t_HtmlList">
		<ul>
			<xsl:attribute name="class">
				<xsl:choose>
					<xsl:when test="local-name() = 'ol'">
						<xsl:text>ordered</xsl:text>
					</xsl:when>
					<xsl:otherwise>
						<xsl:text>bullet</xsl:text>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:attribute>
			<xsl:apply-templates/>
		</ul>
	</xsl:template>

	<xsl:template match="li" name="t_HtmlListItem">
		<li>
			<xsl:apply-templates/>
		</li>
	</xsl:template>

	<xsl:template match="dt" name="t_HtmlDefinedTerm">
		<w:p>
			<w:pPr>
				<w:spacing w:after="0" />
			</w:pPr>
			<span class="Bold">
				<xsl:apply-templates/>
			</span>
		</w:p>
	</xsl:template>

	<xsl:template match="dd" name="t_HtmlDefinition">
		<xsl:apply-templates/>
	</xsl:template>

</xsl:stylesheet>
