// 
// Copyright (c) 2004-2019 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
// 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions 
// are met:
// 
// * Redistributions of source code must retain the above copyright notice, 
//   this list of conditions and the following disclaimer. 
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution. 
// 
// * Neither the name of Jaroslaw Kowalski nor the names of its 
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
// THE POSSIBILITY OF SUCH DAMAGE.
// 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using NLog.Common;
using NLog.Filters;
using NLog.Internal;
using NLog.Layouts;
using NLog.Targets;
using NLog.Targets.Wrappers;
using NLog.Time;

namespace NLog.Config
{
    /// <summary>
    /// Loads NLog configuration from <see cref="ILoggingConfigurationElement"/>
    /// </summary>
    public abstract class LoggingConfigurationParser : LoggingConfiguration
    {
        private ConfigurationItemFactory _configurationItemFactory;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logFactory"></param>
        protected LoggingConfigurationParser(LogFactory logFactory)
            : base(logFactory)
        {
        }

        /// <summary>
        /// Loads NLog configuration from provided config section
        /// </summary>
        /// <param name="nlogConfig"></param>
        /// <param name="basePath"></param>
        protected void LoadConfig(ILoggingConfigurationElement nlogConfig, string basePath)
        {
            InternalLogger.Trace("ParseNLogConfig");
            nlogConfig.AssertName("nlog");

            var dict = CreateNLogConfigDictionary(nlogConfig);

            //check first exception throwing and internal logging, so that erros in this section could be handled correctly
            SetThrowExceptions(dict);
            SetThrowConfigExceptions(dict);
            var internalLoggerEnabled = SetInternalLogLevel(dict);

            SetNLogElementSettings(dict, out var parseMessageTemplates, out var internalLogFile);

            if (internalLogFile != null)
            {
                internalLogFile = ExpandFilePathVariables(internalLogFile);
                InternalLogger.LogFile = internalLogFile;
            }

            if (!internalLoggerEnabled && !InternalLogger.HasActiveLoggers())
            {
                InternalLogger.LogLevel = LogLevel.Off; // Reduce overhead of the InternalLogger when not configured
            }

            _configurationItemFactory = ConfigurationItemFactory.Default;
            _configurationItemFactory.ParseMessageTemplates = parseMessageTemplates;

            var children = nlogConfig.Children.ToList();

            //first load the extensions, as the can be used in other elements (targets etc)
            var extensionsChilds = children.Where(child => child.MatchesName("extensions")).ToList();
            foreach (var extensionsChild in extensionsChilds)
            {
                ParseExtensionsElement(extensionsChild, basePath);
            }

            var rulesList = new List<ILoggingConfigurationElement>();

            //parse all other direct elements
            foreach (var child in children)
            {
                if (child.MatchesName("rules"))
                {
                    //postpone parsing <rules> to the end
                    rulesList.Add(child);
                }
                else if (child.MatchesName("extensions"))
                {
                    //already parsed
                }
                else if (!ParseNLogSection(child))
                {
                    InternalLogger.Warn("Skipping unknown 'NLog' child node: {0}", child.Name);
                }
            }

            foreach (var ruleChild in rulesList)
            {
                ParseRulesElement(ruleChild, LoggingRules);
            }
        }

        private void SetNLogElementSettings(Dictionary<string, string> dict, out bool? parseMessageTemplates, out string internalLogFile)
        {
            parseMessageTemplates = null;
            internalLogFile = null;
            foreach (var configItem in dict)
            {
                switch (configItem.Key.ToUpperInvariant())
                {
                    case "USEINVARIANTCULTURE":
                        if (ParseBooleanValue(configItem.Key, configItem.Value, false))
                            DefaultCultureInfo = CultureInfo.InvariantCulture;
                        break;
#pragma warning disable 618
                    case "EXCEPTIONLOGGINGOLDSTYLE":
                        ExceptionLoggingOldStyle =
                            ParseBooleanValue(configItem.Key, configItem.Value, ExceptionLoggingOldStyle);
                        break;
#pragma warning restore 618
                    case "KEEPVARIABLESONRELOAD":
                        LogFactory.KeepVariablesOnReload = ParseBooleanValue(configItem.Key, configItem.Value,
                            LogFactory.KeepVariablesOnReload);
                        break;
                    case "INTERNALLOGTOCONSOLE":
                        InternalLogger.LogToConsole = ParseBooleanValue(configItem.Key, configItem.Value,
                            InternalLogger.LogToConsole);
                        break;
                    case "INTERNALLOGTOCONSOLEERROR":
                        InternalLogger.LogToConsoleError = ParseBooleanValue(configItem.Key, configItem.Value,
                            InternalLogger.LogToConsoleError);
                        break;
                    case "INTERNALLOGFILE":
                        internalLogFile = configItem.Value?.Trim();
                        break;
#if !SILVERLIGHT && !__IOS__ && !__ANDROID__
                    case "INTERNALLOGTOTRACE":
                        InternalLogger.LogToTrace =
                            ParseBooleanValue(configItem.Key, configItem.Value, InternalLogger.LogToTrace);
                        break;
#endif
                    case "INTERNALLOGINCLUDETIMESTAMP":
                        InternalLogger.IncludeTimestamp = ParseBooleanValue(configItem.Key, configItem.Value,
                            InternalLogger.IncludeTimestamp);
                        break;
                    case "GLOBALTHRESHOLD":
                        LogFactory.GlobalThreshold =
                            ParseLogLevelSafe(configItem.Key, configItem.Value, LogFactory.GlobalThreshold);
                        break; // expanding variables not possible here, they are created later
                    case "PARSEMESSAGETEMPLATES":
                        parseMessageTemplates = string.IsNullOrEmpty(configItem.Value)
                            ? (bool?)null
                            : ParseBooleanValue(configItem.Key, configItem.Value, true);
                        break;
                    default:
                        InternalLogger.Warn("Skipping unknown 'NLog' property {0}={1}", configItem.Key, configItem.Value);
                        break;
                }
            }
        }

        /// <summary>
        /// Set <see cref="InternalLogger.LogLevel"/> and return internalLoggerEnabled
        /// </summary>
        /// <param name="dict"></param>
        /// <returns>internalLoggerEnabled?</returns>
        private static bool SetInternalLogLevel(IDictionary<string, string> dict)
        {
            bool internalLoggerEnabled;
            if (dict.TryGetValue("INTERNALLOGLEVEL", out var val))
            {
                // expanding variables not possible here, they are created later
                InternalLogger.LogLevel = ParseLogLevelSafe("InternalLogLevel", val, InternalLogger.LogLevel);
                internalLoggerEnabled = InternalLogger.LogLevel != LogLevel.Off;
            }
            else
            {
                internalLoggerEnabled = false;
            }

            return internalLoggerEnabled;
        }

        /// <summary>
        /// Set <see cref="LogFactory.ThrowConfigExceptions"/>
        /// </summary>
        /// <param name="dict"></param>
        private void SetThrowConfigExceptions(IDictionary<string, string> dict)
        {
            if (dict.TryGetValue("THROWCONFIGEXCEPTIONS", out var val))
            {
                LogFactory.ThrowConfigExceptions = StringHelpers.IsNullOrWhiteSpace(val)
                    ? (bool?)null
                    : ParseBooleanValue("ThrowConfigExceptions", val, false);
            }
        }

        /// <summary>
        /// Set <see cref="LogFactory.ThrowExceptions"/>
        /// </summary>
        /// <param name="dict"></param>
        private void SetThrowExceptions(IDictionary<string, string> dict)
        {
            if (dict.TryGetValue("THROWEXCEPTIONS", out var val))
            {
                LogFactory.ThrowExceptions = ParseBooleanValue("ThrowExceptions", val, LogFactory.ThrowExceptions);
            }
        }

        /// <summary>
        /// build dictionary, use last value of duplicates
        /// </summary>
        /// <param name="nlogConfig"></param>
        /// <returns></returns>
        private static Dictionary<string, string> CreateNLogConfigDictionary(ILoggingConfigurationElement nlogConfig)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var configItem in nlogConfig.Values)
            {
                if (configItem.Key != null)
                {
                    dict[configItem.Key.Trim()] = configItem.Value;
                }
            }

            return dict;
        }

        private static string ExpandFilePathVariables(string internalLogFile)
        {
            try
            {
#if !SILVERLIGHT
                if (ContainsSubStringIgnoreCase(internalLogFile, "${currentdir}", out string currentDirToken))
                    internalLogFile = internalLogFile.Replace(currentDirToken, System.IO.Directory.GetCurrentDirectory() + System.IO.Path.DirectorySeparatorChar.ToString());
                if (ContainsSubStringIgnoreCase(internalLogFile, "${basedir}", out string baseDirToken))
                    internalLogFile = internalLogFile.Replace(baseDirToken, LogFactory.CurrentAppDomain.BaseDirectory + System.IO.Path.DirectorySeparatorChar.ToString());
                if (ContainsSubStringIgnoreCase(internalLogFile, "${tempdir}", out string tempDirToken))
                    internalLogFile = internalLogFile.Replace(tempDirToken, System.IO.Path.GetTempPath() + System.IO.Path.DirectorySeparatorChar.ToString());
                if (internalLogFile.IndexOf("%", StringComparison.OrdinalIgnoreCase) >= 0)
                    internalLogFile = Environment.ExpandEnvironmentVariables(internalLogFile);
#endif
                return internalLogFile;
            }
            catch
            {
                return internalLogFile;
            }
        }

        private static bool ContainsSubStringIgnoreCase(string haystack, string needle, out string result)
        {
            int needlePos = haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase);
            result = needlePos >= 0 ? haystack.Substring(needlePos, needle.Length) : null;
            return result != null;
        }

        /// <summary>
        /// Parse loglevel, but don't throw if exception throwing is disabled
        /// </summary>
        /// <param name="attributeName">Name of attribute for logging.</param>
        /// <param name="attributeValue">Value of parse.</param>
        /// <param name="default">Used if there is an exception</param>
        /// <returns></returns>
        private static LogLevel ParseLogLevelSafe(string attributeName, string attributeValue, LogLevel @default)
        {
            try
            {
                var internalLogLevel = LogLevel.FromString(attributeValue?.Trim());
                return internalLogLevel;
            }
            catch (Exception e)
            {
                const string message = "attribute '{0}': '{1}' isn't valid LogLevel. {2} will be used.";
                var configException =
                    new NLogConfigurationException(e, message, attributeName, attributeValue, @default);
                if (configException.MustBeRethrown())
                {
                    throw;
                }

                return @default;
            }
        }

        /// <summary>
        /// Parses a single config section within the NLog-config
        /// </summary>
        /// <param name="configSection"></param>
        /// <returns>Section was recognized</returns>
        protected virtual bool ParseNLogSection(ILoggingConfigurationElement configSection)
        {
            switch (configSection.Name?.Trim().ToUpperInvariant())
            {
                case "TIME":
                    ParseTimeElement(configSection);
                    return true;

                case "VARIABLE":
                    ParseVariableElement(configSection);
                    return true;

                case "VARIABLES":
                    ParseVariablesElement(configSection);
                    return true;

                case "APPENDERS":
                case "TARGETS":
                    ParseTargetsElement(configSection);
                    return true;
            }

            return false;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability",
            "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Reflection.Assembly.LoadFrom",
            Justification = "Need to load external assembly.")]
        private void ParseExtensionsElement(ILoggingConfigurationElement extensionsElement, string baseDirectory)
        {
            extensionsElement.AssertName("extensions");

            foreach (var childItem in extensionsElement.Children)
            {
                string prefix = null;
                string type = null;
                string assemblyFile = null;
                string assemblyName = null;

                foreach (var childProperty in childItem.Values)
                {
                    if (MatchesName(childProperty.Key, "prefix"))
                    {
                        prefix = childProperty.Value + ".";
                    }
                    else if (MatchesName(childProperty.Key, "type"))
                    {
                        type = childProperty.Value;
                    }
                    else if (MatchesName(childProperty.Key, "assemblyFile"))
                    {
                        assemblyFile = childProperty.Value;
                    }
                    else if (MatchesName(childProperty.Key, "assembly"))
                    {
                        assemblyName = childProperty.Value;
                    }
                    else
                    {
                        InternalLogger.Warn("Skipping unknown property {0} for element {1} in section {2}",
                            childProperty.Key, childItem.Name, extensionsElement.Name);
                    }
                }

                if (!StringHelpers.IsNullOrWhiteSpace(type))
                {
                    RegisterExtension(type, prefix);
                }

#if !NETSTANDARD1_3
                if (!StringHelpers.IsNullOrWhiteSpace(assemblyFile))
                {
                    ParseExtensionWithAssemblyFile(baseDirectory, assemblyFile, prefix);
                    continue;
                }
#endif
                if (!StringHelpers.IsNullOrWhiteSpace(assemblyName))
                {
                    ParseExtensionWithAssembly(assemblyName, prefix);
                }
            }
        }

        private void RegisterExtension(string type, string itemNamePrefix)
        {
            try
            {
                _configurationItemFactory.RegisterType(Type.GetType(type, true), itemNamePrefix);
            }
            catch (Exception exception)
            {
                if (exception.MustBeRethrownImmediately())
                {
                    throw;
                }

                InternalLogger.Error(exception, "Error loading extensions.");
                NLogConfigurationException configException =
                    new NLogConfigurationException("Error loading extensions: " + type, exception);

                if (configException.MustBeRethrown())
                {
                    throw configException;
                }
            }
        }

#if !NETSTANDARD1_3
        private void ParseExtensionWithAssemblyFile(string baseDirectory, string assemblyFile, string prefix)
        {
            try
            {
                Assembly asm = AssemblyHelpers.LoadFromPath(assemblyFile, baseDirectory);
                _configurationItemFactory.RegisterItemsFromAssembly(asm, prefix);
            }
            catch (Exception exception)
            {
                if (exception.MustBeRethrownImmediately())
                {
                    throw;
                }

                InternalLogger.Error(exception, "Error loading extensions.");
                NLogConfigurationException configException =
                    new NLogConfigurationException("Error loading extensions: " + assemblyFile, exception);

                if (configException.MustBeRethrown())
                {
                    throw configException;
                }
            }
        }
#endif

        private void ParseExtensionWithAssembly(string assemblyName, string prefix)
        {
            try
            {
                Assembly asm = AssemblyHelpers.LoadFromName(assemblyName);
                _configurationItemFactory.RegisterItemsFromAssembly(asm, prefix);
            }
            catch (Exception exception)
            {
                if (exception.MustBeRethrownImmediately())
                {
                    throw;
                }

                InternalLogger.Error(exception, "Error loading extensions.");
                NLogConfigurationException configException =
                    new NLogConfigurationException("Error loading extensions: " + assemblyName, exception);

                if (configException.MustBeRethrown())
                {
                    throw configException;
                }
            }
        }

        private void ParseVariableElement(ILoggingConfigurationElement variableElement)
        {
            string variableName = null;
            string variableValue = null;
            foreach (var childProperty in variableElement.Values)
            {
                if (MatchesName(childProperty.Key, "name"))
                    variableName = childProperty.Value;
                else if (MatchesName(childProperty.Key, "value"))
                    variableValue = childProperty.Value;
                else
                    InternalLogger.Warn("Skipping unknown property {0} for element {1} in section {2}",
                        childProperty.Key, variableElement.Name, "variables");
            }

            if (!AssertNonEmptyValue(variableName, "name", variableElement.Name, "variables"))
                return;

            if (!AssertNotNullValue(variableValue, "value", variableElement.Name, "variables"))
                return;

            string value = ExpandSimpleVariables(variableValue);
            Variables[variableName] = value;
        }

        private void ParseVariablesElement(ILoggingConfigurationElement variableElement)
        {
            variableElement.AssertName("variables");

            foreach (var childItem in variableElement.Children)
            {
                ParseVariableElement(childItem);
            }
        }

        private void ParseTimeElement(ILoggingConfigurationElement timeElement)
        {
            timeElement.AssertName("time");

            string timeSourceType = null;
            foreach (var childProperty in timeElement.Values)
            {
                if (MatchesName(childProperty.Key, "type"))
                    timeSourceType = childProperty.Value;
                else
                    InternalLogger.Warn("Skipping unknown property {0} for element {1} in section {2}",
                        childProperty.Key, timeElement.Name, timeElement.Name);
            }

            if (!AssertNonEmptyValue(timeSourceType, "type", timeElement.Name, string.Empty))
                return;

            TimeSource newTimeSource = _configurationItemFactory.TimeSources.CreateInstance(timeSourceType);
            ConfigureObjectFromAttributes(newTimeSource, timeElement, true);

            InternalLogger.Info("Selecting time source {0}", newTimeSource);
            TimeSource.Current = newTimeSource;
        }
        [ContractAnnotation("value:notnull => true")]
        private static bool AssertNotNullValue(string value, string propertyName, string elementName, string sectionName)
        {
            if (value != null)
                return true;

            return AssertNonEmptyValue(string.Empty, propertyName, elementName, sectionName);
        }
        [ContractAnnotation("value:null => false")]
        private static bool AssertNonEmptyValue(string value, string propertyName, string elementName, string sectionName)
        {
            if (!StringHelpers.IsNullOrWhiteSpace(value))
                return true;

            if (LogManager.ThrowConfigExceptions ?? LogManager.ThrowExceptions)
                throw new NLogConfigurationException(
                    $"Expected property {propertyName} on element name: {elementName} in section: {sectionName}");

            InternalLogger.Warn("Skipping element name: {0} in section: {1} because property {2} is blank", elementName,
                sectionName, propertyName);
            return false;
        }

        /// <summary>
        /// Parse {Rules} xml element
        /// </summary>
        /// <param name="rulesElement"></param>
        /// <param name="rulesCollection">Rules are added to this parameter.</param>
        private void ParseRulesElement(ILoggingConfigurationElement rulesElement, IList<LoggingRule> rulesCollection)
        {
            InternalLogger.Trace("ParseRulesElement");
            rulesElement.AssertName("rules");

            foreach (var childItem in rulesElement.Children)
            {
                LoggingRule loggingRule = ParseRuleElement(childItem);
                if (loggingRule != null)
                {
                    lock (rulesCollection)
                    {
                        rulesCollection.Add(loggingRule);
                    }
                }
            }
        }

        private LogLevel LogLevelFromString(string text)
        {
            return LogLevel.FromString(ExpandSimpleVariables(text).Trim());
        }

        /// <summary>
        /// Parse {Logger} xml element
        /// </summary>
        /// <param name="loggerElement"></param>
        private LoggingRule ParseRuleElement(ILoggingConfigurationElement loggerElement)
        {
            string minLevel = null;
            string maxLevel = null;
            string enableLevels = null;

            string ruleName = null;
            string namePattern = null;
            bool enabled = true;
            bool final = false;
            string writeTargets = null;
            foreach (var childProperty in loggerElement.Values)
            {
                switch (childProperty.Key?.Trim().ToUpperInvariant())
                {
                    case "NAME":
                        if (loggerElement.MatchesName("logger"))
                            namePattern = childProperty.Value; // Legacy Style
                        else
                            ruleName = childProperty.Value;
                        break;
                    case "LOGGER":
                        namePattern = childProperty.Value;
                        break;
                    case "ENABLED":
                        enabled = ParseBooleanValue(childProperty.Key, childProperty.Value, true);
                        break;
                    case "APPENDTO":
                        writeTargets = childProperty.Value;
                        break;
                    case "WRITETO":
                        writeTargets = childProperty.Value;
                        break;
                    case "FINAL":
                        final = ParseBooleanValue(childProperty.Key, childProperty.Value, false);
                        break;
                    case "LEVEL":
                        enableLevels = childProperty.Value;
                        break;
                    case "LEVELS":
                        enableLevels = StringHelpers.IsNullOrWhiteSpace(childProperty.Value) ? "," : childProperty.Value;
                        break;
                    case "MINLEVEL":
                        minLevel = childProperty.Value;
                        break;
                    case "MAXLEVEL":
                        maxLevel = childProperty.Value;
                        break;
                    default:
                        InternalLogger.Warn("Skipping unknown property {0} for element {1} in section {2}",
                            childProperty.Key, loggerElement.Name, "rules");
                        break;
                }
            }

            if (string.IsNullOrEmpty(ruleName) && string.IsNullOrEmpty(namePattern) &&
                string.IsNullOrEmpty(writeTargets) && !final)
            {
                InternalLogger.Debug("Logging rule without name or filter or targets is ignored");
                return null;
            }

            namePattern = namePattern ?? "*";

            if (!enabled)
            {
                InternalLogger.Debug("Logging rule {0} with filter `{1}` is disabled", ruleName, namePattern);
                return null;
            }

            var rule = new LoggingRule(ruleName)
            {
                LoggerNamePattern = namePattern
            };

            EnableLevelsForRule(rule, enableLevels, minLevel, maxLevel);

            ParseLoggingRuleTargets(writeTargets, rule);

            rule.Final = final;

            ParseLoggingRuleChildren(loggerElement, rule);

            return rule;
        }

        private void EnableLevelsForRule(LoggingRule rule, string enableLevels, string minLevel, string maxLevel)
        {
            if (enableLevels != null)
            {
                enableLevels = ExpandSimpleVariables(enableLevels);
                if (enableLevels.IndexOf('{') >= 0)
                {
                    SimpleLayout simpleLayout = ParseLevelLayout(enableLevels);
                    rule.EnableLoggingForLevels(simpleLayout);
                }
                else
                {
                    if (enableLevels.IndexOf(',') >= 0)
                    {
                        IEnumerable<LogLevel> logLevels = ParseLevels(enableLevels);
                        foreach (var logLevel in logLevels)
                            rule.EnableLoggingForLevel(logLevel);
                    }
                    else
                    {
                        rule.EnableLoggingForLevel(LogLevelFromString(enableLevels));
                    }
                }
            }
            else
            {
                minLevel = minLevel != null ? ExpandSimpleVariables(minLevel) : minLevel;
                maxLevel = maxLevel != null ? ExpandSimpleVariables(maxLevel) : maxLevel;
                if (minLevel?.IndexOf('{') >= 0 || maxLevel?.IndexOf('{') >= 0)
                {
                    SimpleLayout minLevelLayout = ParseLevelLayout(minLevel);
                    SimpleLayout maxLevelLayout = ParseLevelLayout(maxLevel);
                    rule.EnableLoggingForRange(minLevelLayout, maxLevelLayout);
                }
                else
                {
                    LogLevel minLogLevel = minLevel != null ? LogLevelFromString(minLevel) : LogLevel.MinLevel;
                    LogLevel maxLogLevel = maxLevel != null ? LogLevelFromString(maxLevel) : LogLevel.MaxLevel;
                    rule.SetLoggingLevels(minLogLevel, maxLogLevel);
                }
            }
        }

        private SimpleLayout ParseLevelLayout(string levelLayout)
        {
            SimpleLayout simpleLayout = !StringHelpers.IsNullOrWhiteSpace(levelLayout) ? new SimpleLayout(levelLayout, _configurationItemFactory) : null;
            simpleLayout?.Initialize(this);
            return simpleLayout;
        }

        private IEnumerable<LogLevel> ParseLevels(string enableLevels)
        {
            string[] tokens = enableLevels.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var logLevels = tokens.Where(t => !StringHelpers.IsNullOrWhiteSpace(t)).Select(LogLevelFromString);
            return logLevels;
        }

        private void ParseLoggingRuleTargets(string writeTargets, LoggingRule rule)
        {
            if (string.IsNullOrEmpty(writeTargets))
                return;

            foreach (string t in writeTargets.Split(','))
            {
                string targetName = t.Trim();
                if (string.IsNullOrEmpty(targetName))
                    continue;

                Target target = FindTargetByName(targetName);
                if (target != null)
                {
                    rule.Targets.Add(target);
                }
                else
                {
                    throw new NLogConfigurationException(
                        $"Target '{targetName}' not found for logging rule: {(string.IsNullOrEmpty(rule.RuleName) ? rule.LoggerNamePattern : rule.RuleName)}.");
                }
            }
        }

        private void ParseLoggingRuleChildren(ILoggingConfigurationElement loggerElement, LoggingRule rule)
        {
            foreach (var child in loggerElement.Children)
            {
                LoggingRule childRule = null;
                if (child.MatchesName("filters"))
                {
                    ParseFilters(rule, child);
                }
                else if (child.MatchesName("logger") && loggerElement.MatchesName("logger"))
                {
                    childRule = ParseRuleElement(child);
                }
                else if (child.MatchesName("rule") && loggerElement.MatchesName("rule"))
                {
                    childRule = ParseRuleElement(child);
                }
                else
                {
                    InternalLogger.Warn("Skipping unknown child {0} for element {1} in section {2}", child.Name,
                        loggerElement.Name, "rules");
                }

                if (childRule != null)
                {
                    lock (rule.ChildRules)
                    {
                        rule.ChildRules.Add(childRule);
                    }
                }
            }
        }

        private void ParseFilters(LoggingRule rule, ILoggingConfigurationElement filtersElement)
        {
            filtersElement.AssertName("filters");

            var defaultActionResult = filtersElement.GetOptionalValue("defaultAction", null);
            if (defaultActionResult != null)
            {
                PropertyHelper.SetPropertyFromString(rule, nameof(rule.DefaultFilterResult), defaultActionResult,
                    _configurationItemFactory);
            }

            foreach (var filterElement in filtersElement.Children)
            {
                string name = filterElement.Name;

                Filter filter = _configurationItemFactory.Filters.CreateInstance(name);
                ConfigureObjectFromAttributes(filter, filterElement, false);
                rule.Filters.Add(filter);
            }
        }

        private void ParseTargetsElement(ILoggingConfigurationElement targetsElement)
        {
            targetsElement.AssertName("targets", "appenders");

            var asyncItem = targetsElement.Values.FirstOrDefault(configItem => MatchesName(configItem.Key, "async"));
            bool asyncWrap = !string.IsNullOrEmpty(asyncItem.Value) &&
                             ParseBooleanValue(asyncItem.Key, asyncItem.Value, false);

            ILoggingConfigurationElement defaultWrapperElement = null;
            var typeNameToDefaultTargetParameters =
                new Dictionary<string, ILoggingConfigurationElement>(StringComparer.OrdinalIgnoreCase);

            foreach (var targetElement in targetsElement.Children)
            {
                string targetTypeName = GetConfigItemTypeAttribute(targetElement);
                string targetValueName = targetElement.GetOptionalValue("name", null);
                Target newTarget = null;
                if (!string.IsNullOrEmpty(targetValueName))
                    targetValueName = $"{targetElement.Name}(Name={targetValueName})";
                else
                    targetValueName = targetElement.Name;

                switch (targetElement.Name?.Trim().ToUpperInvariant())
                {
                    case "DEFAULT-WRAPPER":
                        if (AssertNonEmptyValue(targetTypeName, "type", targetValueName, targetsElement.Name))
                        {
                            defaultWrapperElement = targetElement;
                        }

                        break;

                    case "DEFAULT-TARGET-PARAMETERS":
                        if (AssertNonEmptyValue(targetTypeName, "type", targetValueName, targetsElement.Name))
                        {
                            ParseDefaultTargetParameters(targetElement, targetTypeName, typeNameToDefaultTargetParameters);
                        }

                        break;

                    case "TARGET":
                    case "APPENDER":
                    case "WRAPPER":
                    case "WRAPPER-TARGET":
                    case "COMPOUND-TARGET":
                        if (AssertNonEmptyValue(targetTypeName, "type", targetValueName, targetsElement.Name))
                        {
                            newTarget = _configurationItemFactory.Targets.CreateInstance(targetTypeName);
                            ParseTargetElement(newTarget, targetElement, typeNameToDefaultTargetParameters);
                        }

                        break;

                    default:
                        InternalLogger.Warn("Skipping unknown element {0} in section {1}", targetValueName,
                            targetsElement.Name);
                        break;
                }

                if (newTarget != null)
                {
                    if (asyncWrap)
                    {
                        newTarget = WrapWithAsyncTargetWrapper(newTarget);
                    }

                    if (defaultWrapperElement != null)
                    {
                        newTarget = WrapWithDefaultWrapper(newTarget, defaultWrapperElement);
                    }

                    InternalLogger.Info("Adding target {0}(Name={1})", newTarget.GetType().Name, newTarget.Name);
                    AddTarget(newTarget.Name, newTarget);
                }
            }
        }

        void ParseDefaultTargetParameters(ILoggingConfigurationElement defaultTargetElement, string targetType,
            Dictionary<string, ILoggingConfigurationElement> typeNameToDefaultTargetParameters)
        {
            typeNameToDefaultTargetParameters[targetType.Trim()] = defaultTargetElement;
        }

        private void ParseTargetElement(Target target, ILoggingConfigurationElement targetElement,
            Dictionary<string, ILoggingConfigurationElement> typeNameToDefaultTargetParameters = null)
        {
            string targetTypeName = GetConfigItemTypeAttribute(targetElement, "targets");
            ILoggingConfigurationElement defaults;
            if (typeNameToDefaultTargetParameters != null &&
                typeNameToDefaultTargetParameters.TryGetValue(targetTypeName, out defaults))
            {
                ParseTargetElement(target, defaults, null);
            }

            var compound = target as CompoundTargetBase;
            var wrapper = target as WrapperTargetBase;

            ConfigureObjectFromAttributes(target, targetElement, true);

            foreach (var childElement in targetElement.Children)
            {
                string name = childElement.Name;

                if (compound != null &&
                    ParseCompoundTarget(typeNameToDefaultTargetParameters, name, childElement, compound, null))
                {
                    continue;
                }

                if (wrapper != null &&
                    ParseTargetWrapper(typeNameToDefaultTargetParameters, name, childElement, wrapper))
                {
                    continue;
                }

                SetPropertyFromElement(target, childElement);
            }
        }

        private bool ParseTargetWrapper(
            Dictionary<string, ILoggingConfigurationElement> typeNameToDefaultTargetParameters, string name,
            ILoggingConfigurationElement childElement,
            WrapperTargetBase wrapper)
        {
            if (IsTargetRefElement(name))
            {
                var targetName = childElement.GetRequiredValue("name",
                    GetName(wrapper));
                Target newTarget = FindTargetByName(targetName);
                if (newTarget == null)
                {
                    throw new NLogConfigurationException($"Referenced target '{targetName}' not found.");
                }

                wrapper.WrappedTarget = newTarget;
                return true;
            }

            if (IsTargetElement(name))
            {
                string targetTypeName = GetConfigItemTypeAttribute(childElement, GetName(wrapper));

                Target newTarget = _configurationItemFactory.Targets.CreateInstance(targetTypeName);
                if (newTarget != null)
                {
                    ParseTargetElement(newTarget, childElement, typeNameToDefaultTargetParameters);
                    if (newTarget.Name != null)
                    {
                        // if the new target has name, register it
                        AddTarget(newTarget.Name, newTarget);
                    }

                    if (wrapper.WrappedTarget != null)
                    {
                        throw new NLogConfigurationException("Wrapped target already defined.");
                    }

                    wrapper.WrappedTarget = newTarget;
                }

                return true;
            }

            return false;
        }

        private static string GetConfigItemTypeAttribute(ILoggingConfigurationElement childElement, string sectionNameForRequiredValue = null)
        {
            var typeAttributeValue = sectionNameForRequiredValue != null ? childElement.GetRequiredValue("type", sectionNameForRequiredValue) : childElement.GetOptionalValue("type", null);
            return StripOptionalNamespacePrefix(typeAttributeValue);
        }

        private bool ParseCompoundTarget(
            Dictionary<string, ILoggingConfigurationElement> typeNameToDefaultTargetParameters, string name,
            ILoggingConfigurationElement childElement,
            CompoundTargetBase compound, string targetName)
        {
            if (MatchesName(name, "targets") || MatchesName(name, "appenders"))
            {
                foreach (var child in childElement.Children)
                {
                    ParseCompoundTarget(typeNameToDefaultTargetParameters, child.Name, child, compound, null);
                }

                return true;
            }

            if (IsTargetRefElement(name))
            {
                targetName = childElement.GetRequiredValue("name", GetName(compound));

                Target newTarget = FindTargetByName(targetName);
                if (newTarget == null)
                {
                    throw new NLogConfigurationException("Referenced target '" + targetName + "' not found.");
                }

                compound.Targets.Add(newTarget);
                return true;
            }

            if (IsTargetElement(name))
            {
                string targetTypeName = GetConfigItemTypeAttribute(childElement, GetName(compound));

                Target newTarget = _configurationItemFactory.Targets.CreateInstance(targetTypeName);
                if (newTarget != null)
                {
                    if (targetName != null)
                        newTarget.Name = targetName;

                    ParseTargetElement(newTarget, childElement, typeNameToDefaultTargetParameters);
                    if (newTarget.Name != null)
                    {
                        // if the new target has name, register it
                        AddTarget(newTarget.Name, newTarget);
                    }

                    compound.Targets.Add(newTarget);
                }

                return true;
            }

            return false;
        }

      
        private void ConfigureObjectFromAttributes(object targetObject, ILoggingConfigurationElement element,
            bool ignoreType)
        {
            foreach (var kvp in element.Values)
            {
                string childName = kvp.Key;
                string childValue = kvp.Value;

                if (ignoreType && MatchesName(childName, "type"))
                {
                    continue;
                }

                try
                {
                    PropertyHelper.SetPropertyFromString(targetObject, childName, ExpandSimpleVariables(childValue),
                        _configurationItemFactory);
                }
                catch (Exception ex)
                {
                    InternalLogger.Warn(ex, "Error when setting '{0}' on attibute '{1}'", childValue, childName);
                    throw;
                }
            }
        }


        private void SetPropertyFromElement(object o, ILoggingConfigurationElement element)
        {
            if (!PropertyHelper.TryGetPropertyInfo(o, element.Name, out var propInfo))
            {
                return;
            }

            if (AddArrayItemFromElement(o, propInfo, element))
            {
                return;
            }

            if (SetLayoutFromElement(o, propInfo, element))
            {
                return;
            }

            SetItemFromElement(o, propInfo, element);
        }

        private bool AddArrayItemFromElement(object o, PropertyInfo propInfo, ILoggingConfigurationElement element)
        {
            Type elementType = PropertyHelper.GetArrayItemType(propInfo);
            if (elementType != null)
            {
                IList propertyValue = (IList)propInfo.GetValue(o, null);

                if (string.Equals(propInfo.Name, element.Name, StringComparison.OrdinalIgnoreCase))
                {
                    var children = element.Children.ToList();
                    if (children.Count > 0)
                    {
                        foreach (var child in children)
                        {
                            propertyValue.Add(ParseArrayItemFromElement(elementType, child));
                        }

                        return true;
                    }
                }

                object arrayItem = ParseArrayItemFromElement(elementType, element);
                propertyValue.Add(arrayItem);
                return true;
            }

            return false;
        }

        private object ParseArrayItemFromElement(Type elementType, ILoggingConfigurationElement element)
        {
            object arrayItem = TryCreateLayoutInstance(element, elementType);
            // arrayItem is not a layout
            if (arrayItem == null)
                arrayItem = FactoryHelper.CreateInstance(elementType);

            ConfigureObjectFromAttributes(arrayItem, element, true);
            ConfigureObjectFromElement(arrayItem, element);
            return arrayItem;
        }

        private bool SetLayoutFromElement(object o, PropertyInfo propInfo, ILoggingConfigurationElement layoutElement)
        {
            Layout layout = TryCreateLayoutInstance(layoutElement, propInfo.PropertyType);

            // and is a Layout and 'type' attribute has been specified
            if (layout != null)
            {
                ConfigureObjectFromAttributes(layout, layoutElement, true);
                ConfigureObjectFromElement(layout, layoutElement);
                propInfo.SetValue(o, layout, null);
                return true;
            }

            return false;
        }

        private Layout TryCreateLayoutInstance(ILoggingConfigurationElement element, Type type)
        {
            // Check if it is a Layout
            if (!typeof(Layout).IsAssignableFrom(type))
                return null;

            // Check if the 'type' attribute has been specified
            string layoutTypeName = GetConfigItemTypeAttribute(element);
            if (layoutTypeName == null)
                return null;

            return _configurationItemFactory.Layouts.CreateInstance(ExpandSimpleVariables(layoutTypeName));
        }

        private void SetItemFromElement(object o, PropertyInfo propInfo, ILoggingConfigurationElement element)
        {
            object item = propInfo.GetValue(o, null);
            ConfigureObjectFromAttributes(item, element, true);
            ConfigureObjectFromElement(item, element);
        }

        private void ConfigureObjectFromElement(object targetObject, ILoggingConfigurationElement element)
        {
            foreach (var child in element.Children)
            {
                SetPropertyFromElement(targetObject, child);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability",
            "CA2000:Dispose objects before losing scope", Justification = "Target is disposed elsewhere.")]
        private static Target WrapWithAsyncTargetWrapper(Target target)
        {
            var asyncTargetWrapper = new AsyncTargetWrapper();
            asyncTargetWrapper.WrappedTarget = target;
            asyncTargetWrapper.Name = target.Name;
            target.Name = target.Name + "_wrapped";
            InternalLogger.Debug("Wrapping target '{0}' with AsyncTargetWrapper and renaming to '{1}",
                asyncTargetWrapper.Name, target.Name);
            target = asyncTargetWrapper;
            return target;
        }

        private Target WrapWithDefaultWrapper(Target t, ILoggingConfigurationElement defaultParameters)
        {
            string wrapperTypeName = GetConfigItemTypeAttribute(defaultParameters, "targets");
            Target wrapperTargetInstance = _configurationItemFactory.Targets.CreateInstance(wrapperTypeName);
            WrapperTargetBase wtb = wrapperTargetInstance as WrapperTargetBase;
            if (wtb == null)
            {
                throw new NLogConfigurationException("Target type specified on <default-wrapper /> is not a wrapper.");
            }

            ParseTargetElement(wrapperTargetInstance, defaultParameters);
            while (wtb.WrappedTarget != null)
            {
                wtb = wtb.WrappedTarget as WrapperTargetBase;
                if (wtb == null)
                {
                    throw new NLogConfigurationException(
                        "Child target type specified on <default-wrapper /> is not a wrapper.");
                }
            }

            wtb.WrappedTarget = t;
            wrapperTargetInstance.Name = t.Name;
            t.Name = t.Name + "_wrapped";

            InternalLogger.Debug("Wrapping target '{0}' with '{1}' and renaming to '{2}", wrapperTargetInstance.Name,
                wrapperTargetInstance.GetType().Name, t.Name);
            return wrapperTargetInstance;
        }

        private static bool MatchesName(string key, string expectedKey)
        {
            return string.Equals(key?.Trim(), expectedKey, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Parse boolean
        /// </summary>
        /// <param name="propertyName">Name of the property for logging.</param>
        /// <param name="value">value to parse</param>
        /// <param name="defaultValue">Default value to return if the parse failed</param>
        /// <returns>Boolean attribute value or default.</returns>
        private static bool ParseBooleanValue(string propertyName, string value, bool defaultValue)
        {
            try
            {
                return Convert.ToBoolean(value?.Trim(), CultureInfo.InvariantCulture);
            }
            catch (Exception e)
            {
                const string message = "'{0}' hasn't a valid boolean value '{1}'. {2} will be used";
                var configException = new NLogConfigurationException(e, message, propertyName, value, defaultValue);
                if (configException.MustBeRethrown())
                {
                    throw;
                }

                return defaultValue;
            }
        }

        private static bool IsTargetElement(string name)
        {
            return name.Equals("target", StringComparison.OrdinalIgnoreCase)
                   || name.Equals("wrapper", StringComparison.OrdinalIgnoreCase)
                   || name.Equals("wrapper-target", StringComparison.OrdinalIgnoreCase)
                   || name.Equals("compound-target", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsTargetRefElement(string name)
        {
            return name.Equals("target-ref", StringComparison.OrdinalIgnoreCase)
                   || name.Equals("wrapper-target-ref", StringComparison.OrdinalIgnoreCase)
                   || name.Equals("compound-target-ref", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Remove the namespace (before :)
        /// </summary>
        /// <example>
        /// x:a, will be a
        /// </example>
        /// <param name="attributeValue"></param>
        /// <returns></returns>
        private static string StripOptionalNamespacePrefix(string attributeValue)
        {
            if (attributeValue == null)
            {
                return null;
            }

            int p = attributeValue.IndexOf(':');
            if (p < 0)
            {
                return attributeValue;
            }

            return attributeValue.Substring(p + 1);
        }

        /// <summary>
        /// Remove all spaces, also in between text. 
        /// </summary>
        /// <param name="s">text</param>
        /// <returns>text without spaces</returns>
        /// <remarks>Tabs and other whitespace is not removed!</remarks>
        private static string CleanSpaces(string s)
        {
            s = s.Replace(" ", string.Empty); // get rid of the whitespace
            return s;
        }

        private static string GetName(Target target)
        {
            return string.IsNullOrEmpty(target.Name) ? target.GetType().Name : target.Name;
        }

    }

    static class ILoggingConfigurationSectionExtensions
    {
        public static bool MatchesName(this ILoggingConfigurationElement section, string expectedName)
        {
            return string.Equals(section?.Name?.Trim(), expectedName, StringComparison.OrdinalIgnoreCase);
        }

        public static void AssertName(this ILoggingConfigurationElement section, params string[] allowedNames)
        {
            foreach (var en in allowedNames)
            {
                if (section.MatchesName(en))
                    return;
            }

            throw new InvalidOperationException(
                $"Assertion failed. Expected element name '{string.Join("|", allowedNames)}', actual: '{section?.Name}'.");
        }

        public static string GetRequiredValue(this ILoggingConfigurationElement element, string attributeName,
            string section)
        {
            string value = element.GetOptionalValue(attributeName, null);
            if (value == null)
            {
                throw new NLogConfigurationException($"Expected {attributeName} on {element.Name} in {section}");
            }

            if (StringHelpers.IsNullOrWhiteSpace(value))
            {
                throw new NLogConfigurationException(
                    $"Expected non-empty {attributeName} on {element.Name} in {section}");
            }

            return value;
        }

        public static string GetOptionalValue(this ILoggingConfigurationElement section, string attributeName,
            string defaultValue)
        {
            string value = section.Values
                .Where(configItem => string.Equals(configItem.Key, attributeName, StringComparison.OrdinalIgnoreCase))
                .Select(configItem => configItem.Value).FirstOrDefault();
            if (value == null)
            {
                return defaultValue;
            }

            return value;
        }

        /// <summary>
        /// Gets the optional boolean attribute value.
        /// </summary>
        /// <param name="section"></param>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="defaultValue">Default value to return if the attribute is not found or if there is a parse error</param>
        /// <returns>Boolean attribute value or default.</returns>
        public static bool GetOptionalBooleanValue(this ILoggingConfigurationElement section, string attributeName,
            bool defaultValue)
        {
            string value = section.GetOptionalValue(attributeName, null);
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            try
            {
                return Convert.ToBoolean(value.Trim(), CultureInfo.InvariantCulture);
            }
            catch (Exception e)
            {
                const string message = "'{0}' hasn't a valid boolean value '{1}'. {2} will be used";
                var configException = new NLogConfigurationException(e, message, attributeName, value, defaultValue);
                if (configException.MustBeRethrown())
                {
                    throw;
                }

                return defaultValue;
            }
        }
    }
}