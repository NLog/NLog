<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.1" 
				xmlns:MSHelp="http://msdn.microsoft.com/mshelp"
        xmlns:mshelp="http://msdn.microsoft.com/mshelp"
				xmlns:ddue="http://ddue.schemas.microsoft.com/authoring/2003/5"
				xmlns:xlink="http://www.w3.org/1999/xlink"
        xmlns:msxsl="urn:schemas-microsoft-com:xslt"
        >

  <msxsl:script language="C#" implements-prefix="ddue">
    <msxsl:using namespace="System" />
    <msxsl:using namespace="System.Globalization"/>
    <msxsl:using namespace="System.Text.RegularExpressions" />
    <![CDATA[
			public static string ToUpper(string id) {
        return id.Trim().ToUpper(System.Globalization.CultureInfo.InvariantCulture);
			}
      //Regular expression to check that a string is in a valid Guid representation.
      private static Regex guidChecker = new Regex("[A-Fa-f0-9]{8}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{12}", RegexOptions.None);
      
      public static string GuidChecker(string id) {
        return guidChecker.IsMatch(id).ToString();
      }
      
      public static string CompareDate(string RTMReleaseDate, string changedHistoryDate) {
       
        CultureInfo culture = CultureInfo.InvariantCulture;
        DateTime dt1 = DateTime.MinValue;
        DateTime dt2 = DateTime.MinValue;
        
        try {
          dt1 = DateTime.Parse(RTMReleaseDate, culture);
        }
        catch (FormatException) {
          Console.WriteLine(string.Format("Error: CompareDate: Unable to convert '{0}' for culture {1}.", RTMReleaseDate, culture.Name));
          return "notValidDate";
        }
        
        try {
          dt2 = DateTime.Parse(changedHistoryDate,culture);
        }
        catch (FormatException) {
          Console.WriteLine(string.Format("Error: CompareDate: Unable to convert '{0}' for culture {1}.", changedHistoryDate, culture.Name));
          return "notValidDate";
        }
       
        if (DateTime.Compare(dt2, dt1) > 0) return changedHistoryDate;
        else return RTMReleaseDate;
      }

    ]]>
  </msxsl:script>

  <!-- Tasks -->
  <xsl:variable name="HowTo" select="'DAC3A6A0-C863-4E5B-8F65-79EFC6A4BA09'" />
  <xsl:variable name="Walkthrough" select="'4779DD54-5D0C-4CC3-9DB3-BF1C90B721B3'" />
  <xsl:variable name="Sample" select="'069EFD88-412D-4E2F-8848-2D5C3AD56BDE'" />
  <xsl:variable name="Troubleshooting" select="'38C8E0D1-D601-4DBA-AE1B-5BEC16CD9B01'" />

  <!-- Reference -->
  <xsl:variable name="ReferenceWithoutSyntax" select="'F9205737-4DEC-4A58-AA69-0E621B1236BD'" />
  <xsl:variable name="ReferenceWithSyntax" select="'95DADC4C-A2A6-447A-AA36-B6BE3A4F8DEC'" />
  <xsl:variable name="XMLReference" select="'3272D745-2FFC-48C4-9E9D-CF2B2B784D5F'" />
  <xsl:variable name="ErrorMessage" select="'A635375F-98C2-4241-94E7-E427B47C20B6'" />
  <xsl:variable name="UIReference" select="'B8ED9F21-39A4-4967-928D-160CD2ED9DCE'" />

  <!-- Concepts -->
  <xsl:variable name="Conceptual" select="'1FE70836-AA7D-4515-B54B-E10C4B516E50'" />
  <xsl:variable name="SDKTechnologyOverviewArchitecture" select="'68F07632-C4C5-4645-8DFA-AC87DCB4BD54'" />
  <xsl:variable name="SDKTechnologyOverviewCodeDirectory" select="'4BBAAF90-0E5F-4C86-9D31-A5CAEE35A416'" />
  <xsl:variable name="SDKTechnologyOverviewScenarios" select="'356C57C4-384D-4AF2-A637-FDD6F088A033'" />
  <xsl:variable name="SDKTechnologyOverviewTechnologySummary" select="'19F1BB0E-F32A-4D5F-80A9-211D92A8A715'" />

  <!-- Other Resources -->
  <xsl:variable name="Orientation" select="'B137C930-7BF7-48A2-A329-3ADCAEF8868E'" />
  <xsl:variable name="WhitePaper" select="'56DB00EC-28BA-4C0D-8694-28E8B244E236'" />
  <xsl:variable name="CodeEntity" select="'4A273212-0AC8-4D72-8349-EC11CD2FF8CD'" />
  <xsl:variable name="Glossary" select="'A689E19C-2687-4881-8CE1-652FF60CF46C'" />
  <xsl:variable name="SDKTechnologyOverviewOrientation" select="'CDB8C120-888F-447B-8AF8-F9540562E7CA'" />

  <xsl:template match="ddue:relatedTopics" mode="seeAlso">
    <xsl:param name="autoGenerateLinks" select="'false'" />

    <xsl:if test="(ddue:link | ddue:legacyLink)[(ddue:ToUpper(@topicType_id) = $HowTo or ddue:ToUpper(@topicType_id) = $Walkthrough or ddue:ToUpper(@topicType_id) = $Sample or ddue:ToUpper(@topicType_id) = $Troubleshooting) and ddue:GuidChecker(@xlink:href) = 'True']" >
      <xsl:call-template name="seeAlsoSubSection">
        <xsl:with-param name="headerGroup" select="'SeeAlsoTasks'" />
        <xsl:with-param name="members" select="(ddue:link | ddue:legacyLink)[(ddue:ToUpper(@topicType_id) = $HowTo or ddue:ToUpper(@topicType_id) = $Walkthrough or ddue:ToUpper(@topicType_id) = $Sample or ddue:ToUpper(@topicType_id) = $Troubleshooting) and ddue:GuidChecker(@xlink:href) = 'True']" />
        <xsl:with-param name="autoGenerateLinks" select="'false'" />
      </xsl:call-template>
    </xsl:if>

    <xsl:if test="(ddue:link | ddue:legacyLink)[(ddue:ToUpper(@topicType_id) = $Conceptual or ddue:ToUpper(@topicType_id) = $SDKTechnologyOverviewArchitecture or ddue:ToUpper(@topicType_id) = $SDKTechnologyOverviewCodeDirectory or ddue:ToUpper(@topicType_id) = $SDKTechnologyOverviewScenarios or ddue:ToUpper(@topicType_id) = $SDKTechnologyOverviewTechnologySummary) and ddue:GuidChecker(@xlink:href) = 'True']">
      <xsl:call-template name="seeAlsoSubSection">
        <xsl:with-param name="headerGroup" select="'SeeAlsoConcepts'" />
        <xsl:with-param name="members" select="(ddue:link | ddue:legacyLink)[(ddue:ToUpper(@topicType_id) = $Conceptual or ddue:ToUpper(@topicType_id) = $SDKTechnologyOverviewArchitecture or ddue:ToUpper(@topicType_id) = $SDKTechnologyOverviewCodeDirectory or ddue:ToUpper(@topicType_id) = $SDKTechnologyOverviewScenarios or ddue:ToUpper(@topicType_id) = $SDKTechnologyOverviewTechnologySummary) and ddue:GuidChecker(@xlink:href) = 'True']" /> 
        <xsl:with-param name="autoGenerateLinks" select="'false'" />
      </xsl:call-template>
    </xsl:if>

    <xsl:if test="(ddue:link | ddue:legacyLink)[((ddue:ToUpper(@topicType_id) = $ReferenceWithoutSyntax or ddue:ToUpper(@topicType_id) = $ReferenceWithSyntax or ddue:ToUpper(@topicType_id) = $XMLReference or ddue:ToUpper(@topicType_id) = $ErrorMessage or ddue:ToUpper(@topicType_id) = $UIReference) and ddue:GuidChecker(@xlink:href) = 'True') or ddue:GuidChecker(@xlink:href) = 'False'] |
                  ddue:codeEntityReference or
                  $autoGenerateLinks = 'true'">
      <xsl:call-template name="seeAlsoSubSection">
        <xsl:with-param name="headerGroup" select="'SeeAlsoReference'" />
        <xsl:with-param name="members" select="(ddue:link | ddue:legacyLink)[((ddue:ToUpper(@topicType_id) = $ReferenceWithoutSyntax or ddue:ToUpper(@topicType_id) = $ReferenceWithSyntax or ddue:ToUpper(@topicType_id) = $XMLReference or ddue:ToUpper(@topicType_id) = $ErrorMessage or ddue:ToUpper(@topicType_id) = $UIReference) and ddue:GuidChecker(@xlink:href) = 'True') or ddue:GuidChecker(@xlink:href) = 'False'] |
                                               ddue:codeEntityReference" />
        <xsl:with-param name="autoGenerateLinks" select="$autoGenerateLinks" />
      </xsl:call-template>
    </xsl:if>

    <xsl:if test="(ddue:link | ddue:legacyLink)[(ddue:ToUpper(@topicType_id) != $HowTo and ddue:ToUpper(@topicType_id) != $Walkthrough and ddue:ToUpper(@topicType_id) != $Sample and ddue:ToUpper(@topicType_id) != $Troubleshooting and ddue:ToUpper(@topicType_id) != $Conceptual and ddue:ToUpper(@topicType_id) != $SDKTechnologyOverviewArchitecture and ddue:ToUpper(@topicType_id) != $SDKTechnologyOverviewCodeDirectory and 
                  ddue:ToUpper(@topicType_id) != $SDKTechnologyOverviewScenarios and ddue:ToUpper(@topicType_id) != $SDKTechnologyOverviewTechnologySummary and ddue:ToUpper(@topicType_id) != $ReferenceWithoutSyntax and ddue:ToUpper(@topicType_id) != $ReferenceWithSyntax and ddue:ToUpper(@topicType_id) != $XMLReference and ddue:ToUpper(@topicType_id) != $ErrorMessage and ddue:ToUpper(@topicType_id) != $UIReference and 
                  ddue:GuidChecker(@xlink:href) = 'True') or (not(@topicType_id) and ddue:GuidChecker(@xlink:href) = 'True')] or
                  ddue:dynamicLink[@type = 'inline'] or
                  ddue:externalLink" >
      <xsl:call-template name="seeAlsoSubSection">
        <xsl:with-param name="headerGroup" select="'SeeAlsoOtherResources'" />
          <xsl:with-param name="members" select="(ddue:link | ddue:legacyLink)[(ddue:ToUpper(@topicType_id) != $HowTo and ddue:ToUpper(@topicType_id) != $Walkthrough and ddue:ToUpper(@topicType_id) != $Sample and ddue:ToUpper(@topicType_id) != $Troubleshooting and ddue:ToUpper(@topicType_id) != $Conceptual and ddue:ToUpper(@topicType_id) != $SDKTechnologyOverviewArchitecture and ddue:ToUpper(@topicType_id) != $SDKTechnologyOverviewCodeDirectory and 
                                                 ddue:ToUpper(@topicType_id) != $SDKTechnologyOverviewScenarios and ddue:ToUpper(@topicType_id) != $SDKTechnologyOverviewTechnologySummary and ddue:ToUpper(@topicType_id) != $ReferenceWithoutSyntax and ddue:ToUpper(@topicType_id) != $ReferenceWithSyntax and ddue:ToUpper(@topicType_id) != $XMLReference and ddue:ToUpper(@topicType_id) != $ErrorMessage and ddue:ToUpper(@topicType_id) != $UIReference and 
                                                 ddue:GuidChecker(@xlink:href) = 'True') or (not(@topicType_id) and ddue:GuidChecker(@xlink:href) = 'True')] |
                                                 ddue:dynamicLink[@type = 'inline'] |
                                                 ddue:externalLink" />
          <xsl:with-param name="autoGenerateLinks" select="'false'" />
        </xsl:call-template>
      </xsl:if>
      
  </xsl:template>

  <xsl:template name="seeAlsoSubSection">
    <xsl:param name="headerGroup" />
    <xsl:param name="members" />
    <xsl:param name="autoGenerateLinks" />
    <xsl:call-template name="subSection">
      <xsl:with-param name="title">
        <include item="{$headerGroup}"/>
      </xsl:with-param>
      <xsl:with-param name="content">
        <xsl:if test="$autoGenerateLinks='true'">
          <xsl:call-template name="autogenSeeAlsoLinks"/>
        </xsl:if>
        <xsl:for-each select="$members">
          <div class="seeAlsoStyle">
            <xsl:apply-templates select="." />
          </div>
        </xsl:for-each>
      </xsl:with-param>
    </xsl:call-template>
  </xsl:template>
</xsl:stylesheet>