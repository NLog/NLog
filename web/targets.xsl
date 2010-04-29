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
                Targets represents possible log outputs. You can define one or more targets in the <link href="config">configuration file</link>
                with the <x><target /></x> directive. When defining a target you need to specify its name and type.
            </p>
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
                Regular targets are responsible for writing log output to persistent media, such as <a href="target.File.html">files</a>,
                <a href="target.Database.html">databases</a>, <a href="target.Network.html">network receivers</a> or <a href="target.MSMQ">message queues</a>. 
                Each target has its own set of type-specific configuration parameters which are passed as XML attributes or elements.
            </p>
            <p>
                The following example defines a single file target with a file name of 'file.txt': 
            </p>
            <pre class="XML">
                <span style="color:#0000ff">&lt;</span><span style="color:#800000">targets</span><span style="color:#0000ff">&gt;</span>&#160;
&#160;&#160;&#160;&#160;<span style="color:#0000ff">&lt;</span><span style="color:#800000">target</span>&#160;<span style="color:#ff0000">name</span><span style="color:#0000ff">=</span><span style="color:#0000ff">"n"</span>&#160;<span style="color:#ff0000">type</span><span style="color:#0000ff">=</span><span style="color:#0000ff">"File"</span>&#160;<span style="color:#ff0000">fileName</span><span style="color:#0000ff">=</span><span style="color:#0000ff">"file.txt"</span><span style="color:#0000ff">/&gt;</span>&#160;
<span style="color:#0000ff">&lt;/</span><span style="color:#800000">targets</span><span style="color:#0000ff">&gt;</span> &#160;
</pre>
            
            <p>
                The following log targets are available. Click on a target name for a reference of possible target parameters.
            </p>
            <div class="noborder">
                <table class="listtable">
                    <xsl:call-template name="supportmatrixheader" />
                    <xsl:apply-templates select="//class[attribute[@name='NLog.TargetAttribute' and not(property[@name='IsWrapper' and @value='True']) and not(property[@name='IsCompound' and @value='True'])]]" mode="list">
                        <xsl:sort select="../../@name" />
                        <xsl:sort select="attribute[@name='NLog.TargetAttribute']/property[@name='Name']/@value" />
                    </xsl:apply-templates>
                </table>
            </div>
            <a name="wrappers"></a>
            <h3>Target Wrappers</h3>
            <p>
                Target wrappers are used to modify the behavior of other targets by adding features such as:
            </p>
            <ul>
                <li>asynchronous processing (wrapped target runs in a separate thread)</li>
                <li>retry-on-error</li>
                <li>buffering</li>
            </ul>
            <p>
                The following target wrappers are available. Click on a target name for full reference.
            </p>
            <div class="noborder">
                <table class="listtable">
                    <xsl:call-template name="supportmatrixheader" />
                    <xsl:apply-templates select="//class[attribute[@name='NLog.TargetAttribute' and property[@name='IsWrapper' and @value='True']]]" mode="list">
                        <xsl:sort select="../../@name" />
                        <xsl:sort select="attribute[@name='NLog.TargetAttribute']/property[@name='Name']/@value" />
                    </xsl:apply-templates>
                </table>
            </div>
            <a name="compound"></a>
            <h3>Compound Targets</h3>
            <p>
                The following compound targets are available. Click on the target name for full reference.
            </p>
            <div class="noborder">
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

    <xsl:template match="class" mode="list">
        <xsl:variable name="type_tag" select="attribute[@name='NLog.TargetAttribute']/property[@name='Name']/@value" />
        <tr>
            <td class="name"><a href="target.{$type_tag}.html"><xsl:value-of select="$type_tag" /></a></td>
            <td class="description"><xsl:apply-templates select="documentation/summary" /></td>
            <xsl:call-template name="supportmatrixvalues" />
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
        <xsl:apply-templates select="documentation/summary" /><p/>
        <xsl:call-template name="detailssupportmatrix" />
        <xsl:if test="documentation/remarks">
            <h4>Remarks:</h4>
            <xsl:apply-templates select="documentation/remarks" />
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

    <xsl:template match="property[@type='NLog.ILayout']" mode="parameter">
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
