<?xml version="1.0" encoding="windows-1250" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
    <xsl:param name="target_name" />
    
    <xsl:include href="style.xsl" />

    <xsl:template match="*" mode="content">
        <xsl:if test="$target_name">
            <xsl:apply-templates select="//class[attribute/@name='NLog.TargetAttribute' and attribute/property[@name='Name']/@value=$target_name]" mode="details">
                <xsl:sort select="attribute[@name='NLog.TargetAttribute']/property[@name='Name']/@value" />
            </xsl:apply-templates>
        </xsl:if>
        <xsl:if test="not($target_name)">
            <h1>Log Targets</h1>
            <p>
                The following types of targets are supported by NLog:
            </p>
            <ul>
                <li><b><a href="#regular">Regular Targets</a></b> - which write the log messages to some output</li>
                <li><b><a href="#wrappers">Target Wrappers</a></b> - which modify the behaviour of a target by adding 
                    features such as asynchronous processing, buffering, filtering and so on.</li>
                <li><b><a href="#compound">Compound Targets</a></b> - which route the log messages to one or more attached targets -
                    they can be used to provide failover, load balancing, log splitting and so on</li>
            </ul>

            <a name="regular"></a>

            <h3>Regular Targets</h3>
            <p>
                The following log targets are available. Click on the target name for full reference.
            </p>
            <div class="noborder" style="width: 600px">
                <table class="listtable">
                    <xsl:call-template name="supportmatrixheader" />
                    <xsl:apply-templates select="//class[attribute[@name='NLog.TargetAttribute' and not(property[@name='IsWrapper' and @value='True']) and not(property[@name='IsCompound' and @value='True'])]]" mode="list">
                        <xsl:sort select="../../@name" />
                        <xsl:sort select="attribute[@name='NLog.TargetAttribute']/property[@name='Name']/@value" />
                    </xsl:apply-templates>
                </table>
            </div>
            <a name="wrappers" />
            <h3>Target Wrappers</h3>
            <p>
                The following target wrappers are available. Click on the target name for full reference.
            </p>
            <div class="noborder" style="width: 600px">
                <table class="listtable">
                    <xsl:call-template name="supportmatrixheader" />
                    <xsl:apply-templates select="//class[attribute[@name='NLog.TargetAttribute' and property[@name='IsWrapper' and @value='True']]]" mode="list">
                        <xsl:sort select="../../@name" />
                        <xsl:sort select="attribute[@name='NLog.TargetAttribute']/property[@name='Name']/@value" />
                    </xsl:apply-templates>
                </table>
            </div>
            <a name="compound" />
            <h3>Compound Targets</h3>
            <p>
                The following compound targets are available. Click on the target name for full reference.
            </p>
            <div class="noborder" style="width: 600px">
                <table class="listtable">
                    <xsl:call-template name="supportmatrixheader" />
                    <xsl:apply-templates select="//class[attribute[@name='NLog.TargetAttribute' and property[@name='IsCompound' and @value='True']]]" mode="list">
                        <xsl:sort select="../../@name" />
                        <xsl:sort select="attribute[@name='NLog.TargetAttribute']/property[@name='Name']/@value" />
                    </xsl:apply-templates>
                </table>
            </div>
        </xsl:if>
    </xsl:template>

    <xsl:template name="supportmatrixheader">
        <tr>
            <th rowspan="2">Name</th>
            <th rowspan="2">Description</th>
            <th colspan="3">.NET Framework</th>
            <th colspan="2">.NET Compact Framework</th>
            <th colspan="2">Mono/Windows</th>
            <th colspan="2">Mono/Unix</th>
        </tr>
        <tr>
            <th>1.0</th>
            <th>1.1</th>
            <th>2.0</th>
            <th>1.0</th>
            <th>2.0</th>
            <th>1.0</th>
            <th>1.0</th>
            <th>2.0</th>
            <th>2.0</th>
        </tr>
    </xsl:template>

    <xsl:template match="attribute" mode="supported-runtime-matches">
        <xsl:param name="framework" />
        <xsl:param name="frameworkVersion" />
        <xsl:param name="os" />
        <xsl:param name="osVersion" />

        <xsl:variable name="attrFramework" select="property[@name='Framework']/@value" />
        <xsl:variable name="attrOS" select="property[@name='OS']/@value" />
        <xsl:variable name="attrMinRuntimeVersion" select="property[@name='MinRuntimeVersion']/@value" />
        <xsl:variable name="attrMaxRuntimeVersion" select="property[@name='MaxRuntimeVersion']/@value" />
        <xsl:variable name="attrMinOSVersion" select="property[@name='MinOSVersion']/@value" />
        <xsl:variable name="attrMaxOSVersion" select="property[@name='MaxOSVersion']/@value" />

        <xsl:variable name="result">
            <xsl:choose>
                <xsl:when test="not($framework)">F1</xsl:when>
                <xsl:when test="$attrFramework = 'RuntimeFramework.Any'">F1</xsl:when>
                <xsl:when test="$attrFramework = $framework">F1</xsl:when>
                <xsl:otherwise>F0</xsl:otherwise>
            </xsl:choose>
        </xsl:variable>

        <xsl:choose>
            <xsl:when test="contains($result,'0')">0</xsl:when>
            <xsl:otherwise>1</xsl:otherwise>
        </xsl:choose>
    </xsl:template>

    <xsl:template name="supported-on">
        <xsl:param name="framework" />
        <xsl:param name="frameworkVersion" />
        <xsl:param name="os" />
        <xsl:param name="osVersion" />

        <xsl:variable name="supportedAttributes" select="attribute[@name='NLog.Config.SupportedRuntimeAttribute']" />
        <xsl:variable name="notSupportedAttributes" select="attribute[@name='NLog.Config.NotSupportedRuntimeAttribute']" />

        <xsl:variable name="supportedAttributeMatches">
            <xsl:apply-templates select="$supportedAttributes" mode="supported-runtime-matches">
                <xsl:with-param name="framework"><xsl:value-of select="$framework" /></xsl:with-param>
                <xsl:with-param name="os"><xsl:value-of select="$os" /></xsl:with-param>
                <xsl:with-param name="frameworkVersion"><xsl:value-of select="$frameworkVersion" /></xsl:with-param>
                <xsl:with-param name="osVersion"><xsl:value-of select="$osVersion" /></xsl:with-param>
            </xsl:apply-templates>
        </xsl:variable>
        
        <xsl:variable name="notSupportedAttributeMatches">
            <xsl:apply-templates select="$notSupportedAttributes" mode="supported-runtime-matches">
                <xsl:with-param name="framework"><xsl:value-of select="$framework" /></xsl:with-param>
                <xsl:with-param name="os"><xsl:value-of select="$os" /></xsl:with-param>
                <xsl:with-param name="frameworkVersion"><xsl:value-of select="$frameworkVersion" /></xsl:with-param>
                <xsl:with-param name="osVersion"><xsl:value-of select="$osVersion" /></xsl:with-param>
            </xsl:apply-templates>
        </xsl:variable>

        <td class="support">
            <img>
                <xsl:attribute name="src">
                    <xsl:choose>
                        <xsl:when test="$supportedAttributeMatches = '' and $notSupportedAttributeMatches = ''">checkbox1.gif</xsl:when>
                        <xsl:when test="contains($supportedAttributeMatches,'1') and not(contains($notSupportedAttributeMatches,'1'))">checkbox1.gif</xsl:when>
                        <xsl:otherwise>checkbox0.gif</xsl:otherwise>
                    </xsl:choose>
                </xsl:attribute>
            </img>
        </td>
    </xsl:template>

    <xsl:template match="class" mode="list">
        <xsl:variable name="type_tag" select="attribute[@name='NLog.TargetAttribute']/property[@name='Name']/@value" />
        <tr>
            <td class="name"><a href="target.{$type_tag}.html"><xsl:value-of select="$type_tag" /></a></td>
            <td class="description"><xsl:apply-templates select="documentation/summary" /></td>
            <xsl:call-template name="supported-on">
                <xsl:with-param name="framework">RuntimeFramework.DotNetFramework</xsl:with-param>
                <xsl:with-param name="frameworkVersion">1.0</xsl:with-param>
                <xsl:with-param name="os">RuntimeOS.Windows</xsl:with-param>
            </xsl:call-template>
            <xsl:call-template name="supported-on">
                <xsl:with-param name="framework">RuntimeFramework.DotNetFramework</xsl:with-param>
                <xsl:with-param name="frameworkVersion">1.1</xsl:with-param>
                <xsl:with-param name="os">RuntimeOS.Windows</xsl:with-param>
            </xsl:call-template>
            <xsl:call-template name="supported-on">
                <xsl:with-param name="framework">RuntimeFramework.DotNetFramework</xsl:with-param>
                <xsl:with-param name="frameworkVersion">2.0</xsl:with-param>
                <xsl:with-param name="os">RuntimeOS.Windows</xsl:with-param>
            </xsl:call-template>
            <xsl:call-template name="supported-on">
                <xsl:with-param name="framework">RuntimeFramework.DotNetCompactFramework</xsl:with-param>
                <xsl:with-param name="frameworkVersion">1.0</xsl:with-param>
                <xsl:with-param name="os">RuntimeOS.WindowsCE</xsl:with-param>
            </xsl:call-template>
            <xsl:call-template name="supported-on">
                <xsl:with-param name="framework">RuntimeFramework.DotNetCompactFramework</xsl:with-param>
                <xsl:with-param name="frameworkVersion">2.0</xsl:with-param>
                <xsl:with-param name="os">RuntimeOS.WindowsCE</xsl:with-param>
            </xsl:call-template>
            <xsl:call-template name="supported-on">
                <xsl:with-param name="framework">RuntimeFramework.Mono</xsl:with-param>
                <xsl:with-param name="frameworkVersion">1.0</xsl:with-param>
                <xsl:with-param name="os">RuntimeOS.Windows</xsl:with-param>
            </xsl:call-template>
            <xsl:call-template name="supported-on">
                <xsl:with-param name="framework">RuntimeFramework.Mono</xsl:with-param>
                <xsl:with-param name="frameworkVersion">1.0</xsl:with-param>
                <xsl:with-param name="os">RuntimeOS.Unix</xsl:with-param>
            </xsl:call-template>
            <xsl:call-template name="supported-on">
                <xsl:with-param name="framework">RuntimeFramework.Mono</xsl:with-param>
                <xsl:with-param name="frameworkVersion">2.0</xsl:with-param>
                <xsl:with-param name="os">RuntimeOS.Windows</xsl:with-param>
            </xsl:call-template>
            <xsl:call-template name="supported-on">
                <xsl:with-param name="framework">RuntimeFramework.Mono</xsl:with-param>
                <xsl:with-param name="frameworkVersion">2.0</xsl:with-param>
                <xsl:with-param name="os">RuntimeOS.Unix</xsl:with-param>
            </xsl:call-template>
        </tr>
    </xsl:template>

    <xsl:template match="@* | node()">
        <xsl:copy>
            <xsl:apply-templates select="@* | node()" />
        </xsl:copy>
    </xsl:template>

    <xsl:template match="summary">
        <xsl:apply-templates />
    </xsl:template>

    <xsl:template match="class" mode="details">
        <xsl:variable name="type_tag" select="attribute[@name='NLog.TargetAttribute']/property[@name='Name']/@value" />
        <h3><xsl:value-of select="$type_tag" /> Target</h3>
        <hr size="1" />
        <h4>Defined in:</h4>
        <table class="definedin" cellspacing="0">
            <tr><td>Assembly:</td><td><xsl:value-of select="../../@name" /></td></tr>
            <tr><td>Class name:</td><td><xsl:value-of select="substring-after(@id,'T:')" /></td></tr>
        </table>
        <xsl:if test="documentation/summary">
            <h4>Summary</h4>
            <xsl:apply-templates select="documentation/summary" /><p/>
        </xsl:if>
        <h4>Parameters (blue fields are required):</h4>
        <table cellspacing="0" cellpadding="0" class="paramtable">
            <tr>
                <th>Name</th>
                <th>Type</th>
                <th>Description</th>
            </tr>
            <xsl:apply-templates select="property" mode="parameter">
                <xsl:sort select="count(attribute[@name='NLog.Config.RequiredParameterAttribute'])" order="descending" />
                <xsl:sort select="@name" />
            </xsl:apply-templates>
            <xsl:if test="property[attribute/@name='NLog.Config.ArrayParameterAttribute']">
                <xsl:apply-templates select="property" mode="parameter2">
                    <xsl:sort select="count(attribute[@name='NLog.Config.RequiredParameterAttribute'])" order="descending" />
                    <xsl:sort select="@name" />
                </xsl:apply-templates>
            </xsl:if>
        </table>
        <xsl:if test="documentation/example">
            <h4>Example:</h4>
            <xsl:apply-templates select="documentation/example" />
        </xsl:if>
        <xsl:if test="documentation/remarks">
            <h4>Remarks:</h4>
            <xsl:apply-templates select="documentation/remarks" />
        </xsl:if>
        <hr size="1" />
        <a href="targets.html">Back to the target list.</a>
    </xsl:template>

    <xsl:template match="property[@set='false']" mode="parameter">
        <!-- ignore -->
    </xsl:template>

    <xsl:template match="property[@id='P:NLog.Target.Name']" mode="parameter">
        <!-- ignore -->
    </xsl:template>

    <xsl:template match="property[@id='P:NLog.Target.Type']" mode="parameter">
        <!-- ignore -->
    </xsl:template>

    <xsl:template match="property[@type='NLog.Layout']" mode="parameter">
        <!-- ignore -->
    </xsl:template>

    <xsl:template match="property[@type='NLog.Conditions.ConditionExpression']" mode="parameter">
        <!-- ignore -->
    </xsl:template>

    <xsl:template match="property" mode="parameter">
        <xsl:if test="@name != 'Layout' or not(../attribute[@name='NLog.TargetAttribute']/property[@name='IgnoresLayout']/@value='True')">
            <xsl:call-template name="parameter_info" />
        </xsl:if>
    </xsl:template>

    <xsl:template match="property[attribute/@name='NLog.Config.ArrayParameterAttribute']" mode="parameter2">
        <xsl:variable name="itemname" select="attribute[@name='NLog.Config.ArrayParameterAttribute']/property[@name='ElementName']/@value" />
        <xsl:variable name="itemtype" select="attribute[@name='NLog.Config.ArrayParameterAttribute']/property[@name='ItemType']/@value" />
        <tr>
            <td valign="top" class="parametername" rowspan="2">
                <xsl:value-of select="@name" />
            </td>
            <td class="parametertype" colspan="2">
                Collection of 
                <xsl:call-template name="simple-type-name">
                    <xsl:with-param name="type"><xsl:value-of select="$itemtype" /></xsl:with-param>
                </xsl:call-template>. Each element is represented as &lt;<xsl:value-of select="$itemname" />/&gt;
            </td>
        </tr>
        <tr>
            <td class="parametervalue" colspan="2">
                <table class="subparamtable" cellspacing="0" cellpadding="0" width="100%">
                    <tr>
                        <th>Name</th>
                        <th>Type</th>
                        <th>Description</th>
                    </tr>
                    <xsl:apply-templates select="//class[@id=concat('T:',$itemtype)]/property" mode="parameter" />
                </table>
            </td>
        </tr>
    </xsl:template>

    <xsl:template match="property" mode="parameter2">
    </xsl:template>

    <xsl:template match="c">
        <code><xsl:apply-templates /></code>
    </xsl:template>

    <xsl:template match="code[@escaped='true']">
        <code><pre class="xml-example"><xsl:apply-templates mode="xml-example" /></pre></code>
    </xsl:template>

    <xsl:template match="code">
        <code><pre class="example"><xsl:apply-templates /></pre></code>
    </xsl:template>

    <xsl:template match="see">
        <code><xsl:value-of select="@cref" /></code>
    </xsl:template>
</xsl:stylesheet>
