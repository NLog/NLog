// 
// Copyright (c) 2004-2021 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

            SetNLogElementSettings(nlogConfig);

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

        private void SetNLogElementSettings(ILoggingConfigurationElement nlogConfig)
        {
            var sortedList = CreateUniqueSortedListFromConfig(nlogConfig);

            bool? parseMessageTemplates = null;
            bool internalLoggerEnabled = false;
            foreach (var configItem in sortedList)
            {
                switch (configItem.Key.ToUpperInvariant())
                {
                    case "THROWEXCEPTIONS":
                        LogFactory.ThrowExceptions = ParseBooleanValue(configItem.Key, configItem.Value, LogFactory.ThrowExceptions);
                        break;
                    case "THROWCONFIGEXCEPTIONS":
                        LogFactory.ThrowConfigExceptions = ParseNullableBooleanValue(configItem.Key, configItem.Value, false);
                        break;
                    case "INTERNALLOGLEVEL":
                        InternalLogger.LogLevel = ParseLogLevelSafe(configItem.Key, configItem.Value, InternalLogger.LogLevel);
                        internalLoggerEnabled = InternalLogger.LogLevel != LogLevel.Off;
                        break;
                    case "USEINVARIANTCULTURE":
                        if (ParseBooleanValue(configItem.Key, configItem.Value, false))
                            DefaultCultureInfo = CultureInfo.InvariantCulture;
                        break;
#pragma warning disable 618
                    case "EXCEPTIONLOGGINGOLDSTYLE":
                        ExceptionLoggingOldStyle = ParseBooleanValue(configItem.Key, configItem.Value, ExceptionLoggingOldStyle);
                        break;
#pragma warning restore 618
                    case "KEEPVARIABLESONRELOAD":
                        LogFactory.KeepVariablesOnReload = ParseBooleanValue(configItem.Key, configItem.Value, LogFactory.KeepVariablesOnReload);
                        break;
                    case "INTERNALLOGTOCONSOLE":
                        InternalLogger.LogToConsole = ParseBooleanValue(configItem.Key, configItem.Value, InternalLogger.LogToConsole);
                        break;
                    case "INTERNALLOGTOCONSOLEERROR":
                        InternalLogger.LogToConsoleError = ParseBooleanValue(configItem.Key, configItem.Value, InternalLogger.LogToConsoleError);
                        break;
                    case "INTERNALLOGFILE":
                        var internalLogFile = configItem.Value?.Trim();
                        if (!string.IsNullOrEmpty(internalLogFile))
                        {
                            internalLogFile = ExpandFilePathVariables(internalLogFile);
                            InternalLogger.LogFile = internalLogFile;
                        }
                        break;
#if !SILVERLIGHT && !__IOS__ && !__ANDROID__
                    case "INTERNALLOGTOTRACE":
                        InternalLogger.LogToTrace = ParseBooleanValue(configItem.Key, configItem.Value, InternalLogger.LogToTrace);
                        break;
#endif
                    case "INTERNALLOGINCLUDETIMESTAMP":
                        InternalLogger.IncludeTimestamp = ParseBooleanValue(configItem.Key, configItem.Value, InternalLogger.IncludeTimestamp);
                        break;
                    case "GLOBALTHRESHOLD":
                        LogFactory.GlobalThreshold = ParseLogLevelSafe(configItem.Key, configItem.Value, LogFactory.GlobalThreshold);
                        break; // expanding variables not possible here, they are created later
                    case "PARSEMESSAGETEMPLATES":
                        parseMessageTemplates = ParseNullableBooleanValue(configItem.Key, configItem.Value, true);
                        break;
                    case "AUTOSHUTDOWN":
                        LogFactory.AutoShutdown = ParseBooleanValue(configItem.Key, configItem.Value, true);
                        break;
                    case "AUTORELOAD":
                        break;  // Ignore here, used by other logic
                    default:
                        InternalLogger.Debug("Skipping unknown 'NLog' property {0}={1}", configItem.Key, configItem.Value);
                        break;
                }
            }

            if (!internalLoggerEnabled && !InternalLogger.HasActiveLoggers())
            {
                InternalLogger.LogLevel = LogLevel.Off; // Reduce overhead of the InternalLogger when not configured
            }

            _configurationItemFactory = ConfigurationItemFactory.Default;
            _configurationItemFactory.ParseMessageTemplates = parseMessageTemplates;
        }

        /// <summary>
        /// Builds list with unique keys, using last value of duplicates. High priority keys placed first.
        /// </summary>
        /// <param name="nlogConfig"></param>
        /// <returns></returns>
        private static IList<KeyValuePair<string, string>> CreateUniqueSortedListFromConfig(ILoggingConfigurationElement nlogConfig)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var configItem in nlogConfig.Values)
            {
                if (!string.IsNullOrEmpty(configItem.Key))
                {
                    string key = configItem.Key.Trim();
                    if (dict.ContainsKey(key))
                    {
                        InternalLogger.Debug("Skipping duplicate value for 'NLog' property {0}. Value={1}", configItem.Key, dict[key]);
                    }
                    dict[key] = configItem.Value;
                }
            }

            var sortedList = new List<KeyValuePair<string, string>>(dict.Count);
            AddHighPrioritySetting("ThrowExceptions");
            AddHighPrioritySetting("ThrowConfigExceptions");
            AddHighPrioritySetting("InternalLogLevel");
            AddHighPrioritySetting("InternalLogFile");
            AddHighPrioritySetting("InternalLogToConsole");
            foreach (var configItem in dict)
            {
                sortedList.Add(configItem);
            }
            return sortedList;

            void AddHighPrioritySetting(string settingName)
            {
                if (dict.ContainsKey(settingName))
                {
                    sortedList.Add(new KeyValuePair<string, string>(settingName, dict[settingName]));
                    dict.Remove(settingName);
                }
            }
        }

        private string ExpandFilePathVariables(string internalLogFile)
        {
            try
            {
#if !SILVERLIGHT
                if (ContainsSubStringIgnoreCase(internalLogFile, "${currentdir}", out string currentDirToken))
                    internalLogFile = internalLogFile.Replace(currentDirToken, System.IO.Directory.GetCurrentDirectory() + System.IO.Path.DirectorySeparatorChar.ToString());
                if (ContainsSubStringIgnoreCase(internalLogFile, "${basedir}", out string baseDirToken))
                    internalLogFile = internalLogFile.Replace(baseDirToken, LogFactory.CurrentAppEnvironment.AppDomainBaseDirectory + System.IO.Path.DirectorySeparatorChar.ToString());
                if (ContainsSubStringIgnoreCase(internalLogFile, "${tempdir}", out string tempDirToken))
                    internalLogFile = internalLogFile.Replace(tempDirToken, LogFactory.CurrentAppEnvironment.UserTempFilePath + System.IO.Path.DirectorySeparatorChar.ToString());
#if !NETSTANDARD1_3
                if (ContainsSubStringIgnoreCase(internalLogFile, "${processdir}", out string processDirToken))
                    internalLogFile = internalLogFile.Replace(processDirToken, System.IO.Path.GetDirectoryName(LogFactory.CurrentAppEnvironment.CurrentProcessFilePath) + System.IO.Path.DirectorySeparatorChar.ToString());
#endif
                if (internalLogFile.IndexOf('%') >= 0)
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
        private LogLevel ParseLogLevelSafe(string attributeName, string attributeValue, LogLevel @default)
        {
            try
            {
                var internalLogLevel = LogLevel.FromString(attributeValue?.Trim());
                return internalLogLevel;
            }
            catch (Exception exception)
            {
                if (exception.MustBeRethrownImmediately())
                    throw;

                const string message = "attribute '{0}': '{1}' isn't valid LogLevel. {2} will be used.";
                var configException =
                    new NLogConfigurationException(exception, message, attributeName, attributeValue, @default);
                if (MustThrowConfigException(configException))
                    throw configException;

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
                        InternalLogger.Debug("Skipping unknown property {0} for element {1} in section {2}",
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
                    throw;

                var configException =
                    new NLogConfigurationException("Error loading extensions: " + type, exception);
                if (MustThrowConfigException(configException))
                    throw configException;
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
                    throw;

                var configException =
                    new NLogConfigurationException("Error loading extensions: " + assemblyFile, exception);
                if (MustThrowConfigException(configException))
                    throw configException;
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
                    throw;

                var configException =
                    new NLogConfigurationException("Error loading extensions: " + assemblyName, exception);
                if (MustThrowConfigException(configException))
                    throw configException;
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
                    InternalLogger.Debug("Skipping unknown property {0} for element {1} in section {2}",
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
                    InternalLogger.Debug("Skipping unknown property {0} for element {1} in section {2}",
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
            string filterDefaultAction = null;
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
                    case "RULENAME":
                        ruleName = childProperty.Value; // Legacy Style
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
                    case "FILTERDEFAULTACTION":
                        filterDefaultAction = childProperty.Value;
                        break;
                    default:
                        InternalLogger.Debug("Skipping unknown property {0} for element {1} in section {2}",
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
                LoggerNamePattern = namePattern,
                Final = final,
            };

            EnableLevelsForRule(rule, enableLevels, minLevel, maxLevel);

            ParseLoggingRuleTargets(writeTargets, rule);

            ParseLoggingRuleChildren(loggerElement, rule, filterDefaultAction);

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
            string[] tokens = enableLevels.SplitAndTrimTokens(',');
            var logLevels = tokens.Select(LogLevelFromString);
            return logLevels;
        }

        private void ParseLoggingRuleTargets(string writeTargets, LoggingRule rule)
        {
            if (string.IsNullOrEmpty(writeTargets))
                return;

            foreach (string targetName in writeTargets.SplitAndTrimTokens(','))
            {
                Target target = FindTargetByName(targetName);
                if (target != null)
                {
                    rule.Targets.Add(target);
                }
                else
                {
                    var configException = 
                        new NLogConfigurationException($"Target '{targetName}' not found for logging rule: {(string.IsNullOrEmpty(rule.RuleName) ? rule.LoggerNamePattern : rule.RuleName)}.");
                    if (MustThrowConfigException(configException))
                        throw configException;
                }
            }
        }

        private void ParseLoggingRuleChildren(ILoggingConfigurationElement loggerElement, LoggingRule rule, string filterDefaultAction = null)
        {
            foreach (var child in loggerElement.Children)
            {
                LoggingRule childRule = null;
                if (child.MatchesName("filters"))
                {
                    ParseFilters(rule, child, filterDefaultAction);
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
                    InternalLogger.Debug("Skipping unknown child {0} for element {1} in section {2}", child.Name,
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

        private void ParseFilters(LoggingRule rule, ILoggingConfigurationElement filtersElement, string filterDefaultAction = null)
        {
            filtersElement.AssertName("filters");

            filterDefaultAction = filtersElement.GetOptionalValue("defaultAction", null) ?? filtersElement.GetOptionalValue("filterDefaultAction", null) ?? filterDefaultAction;
            if (filterDefaultAction != null)
            {
                PropertyHelper.SetPropertyFromString(rule, nameof(rule.DefaultFilterResult), filterDefaultAction,
                    _configurationItemFactory);
            }

            foreach (var filterElement in filtersElement.Children)
            {
                var filterType = filterElement.GetOptionalValue("type", null) ?? filterElement.Name;
                Filter filter = _configurationItemFactory.Filters.CreateInstance(filterType);
                ConfigureObjectFromAttributes(filter, filterElement, true);
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
                string targetTypeName = targetElement.GetConfigItemTypeAttribute();
                string targetValueName = targetElement.GetOptionalValue("name", null);
                Target newTarget = null;
                if (!string.IsNullOrEmpty(targetValueName))
                    targetValueName = $"{targetElement.Name}(Name={targetValueName})";
                else
                    targetValueName = targetElement.Name;

                switch (targetElement.Name?.Trim().ToUpperInvariant())
                {
                    case "DEFAULT-WRAPPER":
                    case "TARGETDEFAULTWRAPPER":
                        if (AssertNonEmptyValue(targetTypeName, "type", targetValueName, targetsElement.Name))
                        {
                            defaultWrapperElement = targetElement;
                        }

                        break;

                    case "DEFAULT-TARGET-PARAMETERS":
                    case "TARGETDEFAULTPARAMETERS":
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
                            newTarget = CreateTargetType(targetTypeName);
                            if (newTarget != null)
                            {
                                ParseTargetElement(newTarget, targetElement, typeNameToDefaultTargetParameters);
                            }
                        }
                        break;

                    default:
                        InternalLogger.Debug("Skipping unknown element {0} in section {1}", targetValueName,
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

        private Target CreateTargetType(string targetTypeName)
        {
            Target newTarget = null;

            try
            {
                newTarget = _configurationItemFactory.Targets.CreateInstance(targetTypeName);
                if (newTarget == null)
                    throw new NLogConfigurationException($"Factory returned null for target type: {targetTypeName}");
            }
            catch (Exception ex)
            {
                if (ex.MustBeRethrownImmediately())
                    throw;

                var configException = new NLogConfigurationException($"Failed to create target type: {targetTypeName}", ex);
                if (MustThrowConfigException(configException))
                    throw configException;
            }

            return newTarget;
        }

        void ParseDefaultTargetParameters(ILoggingConfigurationElement defaultTargetElement, string targetType,
            Dictionary<string, ILoggingConfigurationElement> typeNameToDefaultTargetParameters)
        {
            typeNameToDefaultTargetParameters[targetType.Trim()] = defaultTargetElement;
        }

        private void ParseTargetElement(Target target, ILoggingConfigurationElement targetElement,
            Dictionary<string, ILoggingConfigurationElement> typeNameToDefaultTargetParameters = null)
        {
            string targetTypeName = targetElement.GetConfigItemTypeAttribute("targets");
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

                SetPropertyFromElement(target, childElement, targetElement);
            }
        }

        private bool ParseTargetWrapper(
            Dictionary<string, ILoggingConfigurationElement> typeNameToDefaultTargetParameters, string name,
            ILoggingConfigurationElement childElement,
            WrapperTargetBase wrapper)
        {
            if (IsTargetRefElement(name))
            {
                var targetName = childElement.GetRequiredValue("name", GetName(wrapper));

                Target newTarget = FindTargetByName(targetName);
                if (newTarget == null)
                {
                    var configException = new NLogConfigurationException($"Referenced target '{targetName}' not found.");
                    if (MustThrowConfigException(configException))
                        throw configException;
                }

                wrapper.WrappedTarget = newTarget;
                return true;
            }

            if (IsTargetElement(name))
            {
                string targetTypeName = childElement.GetConfigItemTypeAttribute(GetName(wrapper));

                Target newTarget = CreateTargetType(targetTypeName);
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
                        var configException = new NLogConfigurationException($"Failed to assign wrapped target {targetTypeName}, because target {wrapper.Name} already has one.");
                        if (MustThrowConfigException(configException))
                            throw configException;
                    }
                }

                wrapper.WrappedTarget = newTarget;
                return true;
            }

            return false;
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
                string targetTypeName = childElement.GetConfigItemTypeAttribute(GetName(compound));

                Target newTarget = CreateTargetType(targetTypeName);
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

        private void ConfigureObjectFromAttributes(object targetObject, ILoggingConfigurationElement element, bool ignoreType)
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
                catch (NLogConfigurationException ex)
                {
                    if (MustThrowConfigException(ex))
                        throw;
                }
                catch (Exception ex)
                {
                    if (ex.MustBeRethrownImmediately())
                        throw;

                    var configException = new NLogConfigurationException(ex, $"Error when setting value '{childValue}' for property '{childName}' on element '{element}'");
                    if (MustThrowConfigException(configException))
                        throw;
                }
            }
        }

        private void SetPropertyFromElement(object o, ILoggingConfigurationElement childElement, ILoggingConfigurationElement parentElement)
        {
            if (!PropertyHelper.TryGetPropertyInfo(o, childElement.Name, out var propInfo))
            {
                InternalLogger.Debug("Skipping unknown element {0} in section {1}. Not matching any property on {2} - {3}", childElement.Name, parentElement.Name, o, o?.GetType());
                return;
            }

            if (AddArrayItemFromElement(o, propInfo, childElement))
            {
                return;
            }

            if (SetLayoutFromElement(o, propInfo, childElement))
            {
                return;
            }

            if (SetFilterFromElement(o, propInfo, childElement))
            {
                return;
            }

            SetItemFromElement(o, propInfo, childElement);
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

        private bool SetLayoutFromElement(object o, PropertyInfo propInfo, ILoggingConfigurationElement element)
        {
            var layout = TryCreateLayoutInstance(element, propInfo.PropertyType);

            // and is a Layout and 'type' attribute has been specified
            if (layout != null)
            {
                SetItemOnProperty(o, propInfo, element, layout);
                return true;
            }

            return false;
        }
        
        private bool SetFilterFromElement(object o, PropertyInfo propInfo, ILoggingConfigurationElement element)
        {
            var type = propInfo.PropertyType;

            Filter filter = TryCreateFilterInstance(element, type);
            // and is a Filter and 'type' attribute has been specified
            if (filter != null)
            {
                SetItemOnProperty(o, propInfo, element, filter);
                return true;
            }

            return false;
        }

        private Layout TryCreateLayoutInstance(ILoggingConfigurationElement element, Type type)
        {
            return TryCreateInstance(element, type, _configurationItemFactory.Layouts);
        } 
        
        private Filter TryCreateFilterInstance(ILoggingConfigurationElement element, Type type)
        {
            return TryCreateInstance(element, type, _configurationItemFactory.Filters);
        }

        private T TryCreateInstance<T>(ILoggingConfigurationElement element, Type type, INamedItemFactory<T, Type> factory)
            where T : class
        {
            // Check if correct type
            if (!IsAssignableFrom<T>(type))
                return null;

            // Check if the 'type' attribute has been specified
            string layoutTypeName = element.GetConfigItemTypeAttribute();
            if (layoutTypeName == null)
                return null;

            return factory.CreateInstance(ExpandSimpleVariables(layoutTypeName));
        }

        private static bool IsAssignableFrom<T>(Type type)
        {
            return typeof(T).IsAssignableFrom(type);
        }

        private void SetItemOnProperty(object o, PropertyInfo propInfo, ILoggingConfigurationElement element, object properyValue)
        {
            ConfigureObjectFromAttributes(properyValue, element, true);
            ConfigureObjectFromElement(properyValue, element);
            propInfo.SetValue(o, properyValue, null);
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
                SetPropertyFromElement(targetObject, child, element);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability",
            "CA2000:Dispose objects before losing scope", Justification = "Target is disposed elsewhere.")]
        private static Target WrapWithAsyncTargetWrapper(Target target)
        {
#if !NET3_5 && !SILVERLIGHT4
            if (target is AsyncTaskTarget)
            {
                InternalLogger.Debug("Skip wrapping target '{0}' with AsyncTargetWrapper", target.Name);
                return target;
            }
#endif

            var asyncTargetWrapper = new AsyncTargetWrapper();
            asyncTargetWrapper.WrappedTarget = target;
            asyncTargetWrapper.Name = target.Name;
            target.Name = target.Name + "_wrapped";
            InternalLogger.Debug("Wrapping target '{0}' with AsyncTargetWrapper and renaming to '{1}",
                asyncTargetWrapper.Name, target.Name);
            target = asyncTargetWrapper;
            return target;
        }

        private Target WrapWithDefaultWrapper(Target target, ILoggingConfigurationElement defaultParameters)
        {
            string wrapperTypeName = defaultParameters.GetConfigItemTypeAttribute("targets");
            Target wrapperTargetInstance = CreateTargetType(wrapperTypeName);
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

#if !NET3_5 && !SILVERLIGHT4
            if (target is AsyncTaskTarget && wrapperTargetInstance is AsyncTargetWrapper && ReferenceEquals(wrapperTargetInstance, wtb))
            {
                InternalLogger.Debug("Skip wrapping target '{0}' with AsyncTargetWrapper", target.Name);
                return target;
            }
#endif

            wtb.WrappedTarget = target;
            wrapperTargetInstance.Name = target.Name;
            target.Name = target.Name + "_wrapped";

            InternalLogger.Debug("Wrapping target '{0}' with '{1}' and renaming to '{2}", wrapperTargetInstance.Name,
                wrapperTargetInstance.GetType().Name, target.Name);
            return wrapperTargetInstance;
        }

        /// <summary>
        /// Parse boolean
        /// </summary>
        /// <param name="propertyName">Name of the property for logging.</param>
        /// <param name="value">value to parse</param>
        /// <param name="defaultValue">Default value to return if the parse failed</param>
        /// <returns>Boolean attribute value or default.</returns>
        private bool ParseBooleanValue(string propertyName, string value, bool defaultValue)
        {
            try
            {
                return Convert.ToBoolean(value?.Trim(), CultureInfo.InvariantCulture);
            }
            catch (Exception exception)
            {
                if (exception.MustBeRethrownImmediately())
                    throw;

                var configException = new NLogConfigurationException(exception, $"'{propertyName}' hasn't a valid boolean value '{value}'. {defaultValue} will be used");
                if (MustThrowConfigException(configException))
                    throw configException;
                return defaultValue;
            }
        }

        private bool? ParseNullableBooleanValue(string propertyName, string value, bool defaultValue)
        {
            return StringHelpers.IsNullOrWhiteSpace(value)
                ? (bool?)null
                : ParseBooleanValue(propertyName, value, defaultValue);
        }

        private bool MustThrowConfigException(NLogConfigurationException configException)
        {
            if (configException.MustBeRethrown())
                return true;    // Global LogManager says throw

            if (LogFactory.ThrowConfigExceptions ?? LogFactory.ThrowExceptions)
                return true;    // Local LogFactory says throw

            return false;
        }

        private static bool MatchesName(string key, string expectedKey)
        {
            return string.Equals(key?.Trim(), expectedKey, StringComparison.OrdinalIgnoreCase);
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

        private static string GetName(Target target)
        {
            return string.IsNullOrEmpty(target.Name) ? target.GetType().Name : target.Name;
        }
    }
}