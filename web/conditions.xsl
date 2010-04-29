<?xml version="1.0" encoding="windows-1250" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
    <xsl:include href="style.xsl" />

    <xsl:template match="*" mode="content">
        <h1>Conditions</h1>
        <p>
            Conditions are used in various places to filter log events based on their content.
        </p>
        
        <h3>Condition language</h3>
        <p>
            NLog supports a simple language to express conditions. The language consists of:
        </p>
        <ul>
            <li>relational operators (<b>==</b>, <b>!=</b>, <b>&lt;=</b>, <b>&lt;=</b>, <b>&gt;=</b> and <b>&gt;</b>)</li>
            <li><b>and</b>, <b>or</b>, <b>not</b> boolean operators</li>
            <li>string literals which are always evaluated as <a href="layoutrenderers.html">layouts</a> - <b>'${somerenderer}'</b></li>
            <li>boolean literals - <b>true</b> and <b>false</b></li>
            <li>log level literals - <b>LogLevel.Trace</b>, <b>LogLevel.Debug</b>, ... <b>LogLevel.Fatal</b></li>
            <li>numeric literals - <b>12345</b> (integer literal) and <b>12345.678</b> (floating point literal)</li>
            <li>predefined keywords to access the most common log event properties - <b>level</b>, <b>message</b> and <b>logger</b></li>
            <li>braces - to override default priorities and group expressions together</li>
        </ul>
        <h3>Examples</h3>
        <p>
            Here are some examples of conditions:
        </p>
        <ul>
            <li><b>level &gt; LogLevel.Debug</b> - matches the messages whose level is greater than Debug</li>
            <li><b>(level &gt; LogLevel.Debug) or contains(message,'xxx')</b> - matches the messages whose level is greater than Debug or which include the xxx substring in the log message</li>
            <li><b>starts-with(logger,'Kopytko.')</b> - matches the loggers whose names start with <code>Kopytko.</code></li>
            <li><b>ends-with(logger,'.SQL') or ends-with(logger,'.XML')</b> - matches the loggers whose names end with either <code>.SQL</code> or <code>.XML</code></li>
            <li><b>true</b> - matches everything</li>
            <li><b>false</b> - matches nothing</li>
            <li><b>length(message) &gt; 100</b> - matches the log events where the length of the log message is greater than 100</li>
            <li><b>'${shortdate}' == '2005-11-10'</b> - matches on the specified date</li>
        </ul>
        <h3>Available functions</h3>
        <p>
            The following functions are available:
        </p>
        <div class="noborder" style="width: 600px">
            <table class="listtable">
                <tr>
                    <th>Name</th>
                    <th>Description</th>
                    <th><nobr>Defined in</nobr></th>
                </tr>
                <xsl:apply-templates select="//method[attribute/@name='NLog.ConditionMethodAttribute']" mode="list">
                    <xsl:sort select="attribute[@name='NLog.ConditionMethodAttribute']/property[@name='Name']/@value" />
                </xsl:apply-templates>
            </table>
        </div>
    </xsl:template>

    <xsl:template match="method" mode="list">
        <xsl:variable name="method_tag" select="attribute[@name='NLog.ConditionMethodAttribute']/property[@name='Name']/@value" />
        <tr>
            <td class="name"><xsl:value-of select="$method_tag" />(<xsl:for-each select="parameter"><xsl:if test="position() != 1">,</xsl:if><xsl:value-of select="@name" /></xsl:for-each>)</td>
            <td class="description"><xsl:apply-templates select="documentation/summary" />
                Returns: <xsl:apply-templates select="documentation/returns" /></td>
            <td class="assembly"><xsl:value-of select="../../../@name" /></td>
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

    <xsl:template match="property[@set='false']" mode="parameter">
        <!-- ignore -->
    </xsl:template>

    <xsl:template match="property[@name='Name']" mode="parameter">
        <!-- ignore -->
    </xsl:template>

    <xsl:template match="property[@name='Type']" mode="parameter">
        <!-- ignore -->
    </xsl:template>

    <xsl:template match="property[@type='NLog.Layout']" mode="parameter">
        <!-- ignore -->
    </xsl:template>

    <xsl:template match="property[@type='NLog.ILayout']" mode="parameter">
        <!-- ignore -->
    </xsl:template>

    <xsl:template match="c">
        <code><xsl:apply-templates /></code>
    </xsl:template>

    <xsl:template match="code">
        <code><pre class="example"><xsl:apply-templates /></pre></code>
    </xsl:template>

    <xsl:template match="see">
        <code><xsl:value-of select="@cref" /></code>
    </xsl:template>
</xsl:stylesheet>
