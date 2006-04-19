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
                    <tr>
                        <th>Name</th>
                        <th>Description</th>
                        <th><nobr>Defined in</nobr></th>
                    </tr>
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
                    <tr>
                        <th>Name</th>
                        <th>Description</th>
                        <th><nobr>Defined in</nobr></th>
                    </tr>
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
                    <tr>
                        <th>Name</th>
                        <th>Description</th>
                        <th><nobr>Defined in</nobr></th>
                    </tr>
                    <xsl:apply-templates select="//class[attribute[@name='NLog.TargetAttribute' and property[@name='IsCompound' and @value='True']]]" mode="list">
                        <xsl:sort select="../../@name" />
                        <xsl:sort select="attribute[@name='NLog.TargetAttribute']/property[@name='Name']/@value" />
                    </xsl:apply-templates>
                </table>
            </div>
        </xsl:if>
    </xsl:template>

    <xsl:template match="class" mode="list">
        <xsl:variable name="type_tag" select="attribute[@name='NLog.TargetAttribute']/property[@name='Name']/@value" />
        <tr>
            <td class="name"><a href="target.{$type_tag}.html"><xsl:value-of select="$type_tag" /></a></td>
            <td class="description"><xsl:apply-templates select="documentation/summary" /></td>
            <td class="assembly"><xsl:value-of select="../../@name" /></td>
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
