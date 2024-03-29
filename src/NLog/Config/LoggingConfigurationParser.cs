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
        private readonly ServiceRepository _serviceRepository;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logFactory"></param>
        protected LoggingConfigurationParser(LogFactory logFactory)
            : base(logFactory)
        {
            _serviceRepository = logFactory.ServiceRepository;
        }

        /// <summary>
        /// Loads NLog configuration from provided config section
        /// </summary>
        /// <param name="nlogConfig"></param>
        /// <param name="basePath">Directory where the NLog-config-file was loaded from</param>
        protected void LoadConfig(ILoggingConfigurationElement nlogConfig, string basePath)
        {
            InternalLogger.Trace("ParseNLogConfig");
            nlogConfig.AssertName("nlog");

            SetNLogElementSettings(nlogConfig);

            var validatedConfig = ValidatedConfigurationElement.Create(nlogConfig, LogFactory); // Validate after having loaded initial settings

            //first load the extensions, as the can be used in other elements (targets etc)
            foreach (var extensionsChild in validatedConfig.ValidChildren)
            {
                if (extensionsChild.MatchesName("extensions"))
                {
                    ParseExtensionsElement(extensionsChild, basePath);
                }
            }

            var rulesList = new List<ValidatedConfigurationElement>();

            //parse all other direct elements
            foreach (var child in validatedConfig.ValidChildren)
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
                    var configException = new NLogConfigurationException($"Unrecognized element '{child.Name}' from section 'NLog'");
                    if (MustThrowConfigException(configException))
                        throw configException;
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

            CultureInfo defaultCultureInfo = DefaultCultureInfo ?? LogFactory._defaultCultureInfo;
            bool? parseMessageTemplates = null;
            bool internalLoggerEnabled = false;
            bool autoLoadExtensions = false;
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
                            defaultCultureInfo = DefaultCultureInfo = CultureInfo.InvariantCulture;
                        break;
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
                        InternalLogger.LogFile = configItem.Value?.Trim();
                        break;
                    case "INTERNALLOGTOTRACE":
                        InternalLogger.LogToTrace = ParseBooleanValue(configItem.Key, configItem.Value, InternalLogger.LogToTrace);
                        break;
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
                    case "AUTOLOADEXTENSIONS":
                        autoLoadExtensions = ParseBooleanValue(configItem.Key, configItem.Value, false);
                        break;
                    default:
                        var configException = new NLogConfigurationException($"Unrecognized value '{configItem.Key}'='{configItem.Value}' for element '{nlogConfig.Name}'");
                        if (MustThrowConfigException(configException))
                            throw configException;
                        break;
                }
            }

            if (defaultCultureInfo != null && !ReferenceEquals(DefaultCultureInfo, defaultCultureInfo))
            {
                DefaultCultureInfo = defaultCultureInfo;
            }

            if (!internalLoggerEnabled && !InternalLogger.HasActiveLoggers())
            {
                InternalLogger.LogLevel = LogLevel.Off; // Reduce overhead of the InternalLogger when not configured
            }

            if (autoLoadExtensions)
            {
                ConfigurationItemFactory.Default.AssemblyLoader.ScanForAutoLoadExtensions(ConfigurationItemFactory.Default);
            }

            _serviceRepository.ParseMessageTemplates(parseMessageTemplates);
        }

        /// <summary>
        /// Builds list with unique keys, using last value of duplicates. High priority keys placed first.
        /// </summary>
        /// <param name="nlogConfig"></param>
        /// <returns></returns>
        private ICollection<KeyValuePair<string, string>> CreateUniqueSortedListFromConfig(ILoggingConfigurationElement nlogConfig)
        {
            var dict = ValidatedConfigurationElement.Create(nlogConfig, LogFactory).ValueLookup;
            if (dict.Count <= 1)
                return dict;

            var sortedList = new List<KeyValuePair<string, string>>(dict.Count);
            var highPriorityList = new[]
            {
                "ThrowExceptions",
                "ThrowConfigExceptions",
                "InternalLogLevel",
                "InternalLogFile",
                "InternalLogToConsole",
            };
            foreach (var highPrioritySetting in highPriorityList)
            {
                if (dict.TryGetValue(highPrioritySetting, out var settingValue))
                {
                    sortedList.Add(new KeyValuePair<string, string>(highPrioritySetting, settingValue));
                    dict.Remove(highPrioritySetting);
                }
            }
            foreach (var configItem in dict)
            {
                sortedList.Add(configItem);
            }
            return sortedList;
        }

        /// <summary>
        /// Parse loglevel, but don't throw if exception throwing is disabled
        /// </summary>
        /// <param name="propertyName">Name of attribute for logging.</param>
        /// <param name="propertyValue">Value of parse.</param>
        /// <param name="fallbackValue">Used if there is an exception</param>
        /// <returns></returns>
        private LogLevel ParseLogLevelSafe(string propertyName, string propertyValue, LogLevel fallbackValue)
        {
            try
            {
                var internalLogLevel = LogLevel.FromString(propertyValue?.Trim());
                return internalLogLevel;
            }
            catch (Exception exception)
            {
                if (exception.MustBeRethrownImmediately())
                    throw;

                var configException = new NLogConfigurationException($"Property '{propertyName}' assigned invalid LogLevel value '{propertyValue}'. Fallback to '{fallbackValue}'", exception);
                if (MustThrowConfigException(configException))
                    throw configException;

                return fallbackValue;
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
                    ParseTimeElement(ValidatedConfigurationElement.Create(configSection, LogFactory));
                    return true;

                case "VARIABLE":
                    ParseVariableElement(ValidatedConfigurationElement.Create(configSection, LogFactory));
                    return true;

                case "VARIABLES":
                    ParseVariablesElement(ValidatedConfigurationElement.Create(configSection, LogFactory));
                    return true;

                case "APPENDERS":
                case "TARGETS":
                    ParseTargetsElement(ValidatedConfigurationElement.Create(configSection, LogFactory));
                    return true;
            }

            return false;
        }

        private void ParseExtensionsElement(ValidatedConfigurationElement extensionsElement, string baseDirectory)
        {
            extensionsElement.AssertName("extensions");

            foreach (var childItem in extensionsElement.ValidChildren)
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
                        var configException = new NLogConfigurationException($"Unrecognized value '{childProperty.Key}'='{childProperty.Value}' for element '{childItem.Name}' in section '{extensionsElement.Name}'");
                        if (MustThrowConfigException(configException))
                            throw configException;
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
                    ParseExtensionWithAssemblyName(assemblyName?.Trim(), prefix);
                }
            }
        }

        private void RegisterExtension(string typeName, string itemNamePrefix)
        {
            try
            {
                ConfigurationItemFactory.Default.AssemblyLoader.LoadTypeFromName(ConfigurationItemFactory.Default, typeName, itemNamePrefix);
            }
            catch (Exception exception)
            {
                if (exception.MustBeRethrownImmediately())
                    throw;

                var configException =
                    new NLogConfigurationException("Error loading extensions: " + typeName, exception);
                if (MustThrowConfigException(configException))
                    throw configException;
            }
        }

#if !NETSTANDARD1_3
        private void ParseExtensionWithAssemblyFile(string baseDirectory, string assemblyFile, string prefix)
        {
            try
            {
                ConfigurationItemFactory.Default.AssemblyLoader.LoadAssemblyFromPath(ConfigurationItemFactory.Default, assemblyFile, baseDirectory, prefix);
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

        private bool RegisterExtensionFromAssemblyName(string assemblyName, string originalTypeName)
        {
            InternalLogger.Debug("Loading Assembly-Name '{0}' for type: {1}", assemblyName, originalTypeName);
            return ParseExtensionWithAssemblyName(assemblyName, string.Empty);
        }

        private bool ParseExtensionWithAssemblyName(string assemblyName, string prefix)
        {
            try
            {
                ConfigurationItemFactory.Default.AssemblyLoader.LoadAssemblyFromName(ConfigurationItemFactory.Default, assemblyName, prefix);
                return true;
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

            return false;
        }

        private void ParseVariableElement(ValidatedConfigurationElement variableElement)
        {
            string variableName = null;
            string variableValue = null;
            foreach (var childProperty in variableElement.Values)
            {
                if (MatchesName(childProperty.Key, "name"))
                    variableName = childProperty.Value;
                else if (MatchesName(childProperty.Key, "value") || MatchesName(childProperty.Key, "layout"))
                    variableValue = childProperty.Value;
                else
                {
                    var configException = new NLogConfigurationException($"Unrecognized value '{childProperty.Key}'='{childProperty.Value}' for element '{variableElement.Name}' in section 'variables'");
                    if (MustThrowConfigException(configException))
                        throw configException;
                }
            }

            if (!AssertNonEmptyValue(variableName, "name", variableElement.Name, "variables"))
                return;

            Layout variableLayout = variableValue is null
                ? ParseVariableLayoutValue(variableElement)
                : CreateSimpleLayout(ExpandSimpleVariables(variableValue));

            if (!AssertNotNullValue(variableLayout, "value", variableElement.Name, "variables"))
                return;

            InsertParsedConfigVariable(variableName, variableLayout);
        }

        private Layout ParseVariableLayoutValue(ValidatedConfigurationElement variableElement)
        {
            var childElement = variableElement.ValidChildren.FirstOrDefault();
            if (childElement != null)
            {
                return TryCreateLayoutInstance(childElement, typeof(Layout));
            }

            return null;
        }

        private void ParseVariablesElement(ValidatedConfigurationElement variableElement)
        {
            variableElement.AssertName("variables");

            foreach (var childItem in variableElement.ValidChildren)
            {
                ParseVariableElement(childItem);
            }
        }

        private void ParseTimeElement(ValidatedConfigurationElement timeElement)
        {
            timeElement.AssertName("time");

            string timeSourceType = null;
            foreach (var childProperty in timeElement.Values)
            {
                if (MatchesName(childProperty.Key, "type"))
                {
                    timeSourceType = childProperty.Value;
                }
                else
                {
                    var configException = new NLogConfigurationException($"Unrecognized value '{childProperty.Key}'='{childProperty.Value}' for element '{timeElement.Name}'");
                    if (MustThrowConfigException(configException))
                        throw configException;
                }
            }

            if (!AssertNonEmptyValue(timeSourceType, "type", timeElement.Name, string.Empty))
                return;

            TimeSource newTimeSource = FactoryCreateInstance(timeSourceType, ConfigurationItemFactory.Default.TimeSourceFactory);
            if (newTimeSource != null)
            {
                ConfigureFromAttributesAndElements(newTimeSource, timeElement);
                InternalLogger.Info("Selecting time source {0}", newTimeSource);
                TimeSource.Current = newTimeSource;
            }
        }

        [ContractAnnotation("value:notnull => true")]
        private bool AssertNotNullValue(object value, string propertyName, string elementName, string sectionName)
        {
            if (value is null)
                return AssertNonEmptyValue(string.Empty, propertyName, elementName, sectionName);

            return true;
        }

        [ContractAnnotation("value:null => false")]
        private bool AssertNonEmptyValue(string value, string propertyName, string elementName, string sectionName)
        {
            if (!StringHelpers.IsNullOrWhiteSpace(value))
                return true;

            var configException = new NLogConfigurationException($"Property '{propertyName}' has blank value, for element '{elementName}' in section '{sectionName}'");
            if (MustThrowConfigException(configException))
                throw configException;

            return false;
        }

        /// <summary>
        /// Parse {Rules} xml element
        /// </summary>
        /// <param name="rulesElement"></param>
        /// <param name="rulesCollection">Rules are added to this parameter.</param>
        private void ParseRulesElement(ValidatedConfigurationElement rulesElement, IList<LoggingRule> rulesCollection)
        {
            InternalLogger.Trace("ParseRulesElement");
            rulesElement.AssertName("rules");

            foreach (var childItem in rulesElement.ValidChildren)
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
        private LoggingRule ParseRuleElement(ValidatedConfigurationElement loggerElement)
        {
            string minLevel = null;
            string maxLevel = null;
            string finalMinLevel = null;
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
                    case "LEVELS":
                        enableLevels = childProperty.Value;
                        break;
                    case "MINLEVEL":
                        minLevel = childProperty.Value;
                        break;
                    case "MAXLEVEL":
                        maxLevel = childProperty.Value;
                        break;
                    case "FINALMINLEVEL":
                        finalMinLevel = childProperty.Value;
                        break;
                    case "FILTERDEFAULTACTION":
                        filterDefaultAction = childProperty.Value;
                        break;
                    default:
                        var configException = new NLogConfigurationException($"Unrecognized value '{childProperty.Key}'='{childProperty.Value}' for element '{loggerElement.Name}' in section 'rules'");
                        if (MustThrowConfigException(configException))
                            throw configException;
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
                InternalLogger.Debug("Logging rule {0} with name pattern `{1}` is disabled", ruleName, namePattern);
                return null;
            }

            var rule = new LoggingRule(ruleName)
            {
                LoggerNamePattern = namePattern,
                Final = final,
            };

            EnableLevelsForRule(rule, enableLevels, minLevel, maxLevel, finalMinLevel);

            ParseLoggingRuleTargets(writeTargets, rule);

            ParseLoggingRuleChildren(loggerElement, rule, filterDefaultAction);

            ValidateLoggingRuleFilters(rule);

            return rule;
        }

        private void EnableLevelsForRule(
            LoggingRule rule,
            string enableLevels,
            string minLevel,
            string maxLevel,
            string finalMinLevel)
        {
            if (enableLevels != null)
            {
                enableLevels = ExpandSimpleVariables(enableLevels);
                finalMinLevel = finalMinLevel != null ? ExpandSimpleVariables(finalMinLevel) : finalMinLevel;
                if (IsLevelLayout(enableLevels) || IsLevelLayout(finalMinLevel))
                {
                    SimpleLayout simpleLayout = ParseLevelLayout(enableLevels);
                    SimpleLayout finalMinLevelLayout = ParseLevelLayout(finalMinLevel);
                    rule.EnableLoggingForLevelLayout(simpleLayout, finalMinLevelLayout);
                }
                else
                {
                    foreach (var logLevel in enableLevels.SplitAndTrimTokens(','))
                    {
                        rule.EnableLoggingForLevel(LogLevelFromString(logLevel));
                    }
                    if (finalMinLevel != null)
                        rule.FinalMinLevel = LogLevelFromString(finalMinLevel);
                }
            }
            else
            {
                minLevel = minLevel != null ? ExpandSimpleVariables(minLevel) : minLevel;
                maxLevel = maxLevel != null ? ExpandSimpleVariables(maxLevel) : maxLevel;
                finalMinLevel = finalMinLevel != null ? ExpandSimpleVariables(finalMinLevel) : finalMinLevel;
                if (IsLevelLayout(minLevel) || IsLevelLayout(maxLevel) || IsLevelLayout(finalMinLevel))
                {
                    SimpleLayout finalMinLevelLayout = ParseLevelLayout(finalMinLevel);
                    SimpleLayout minLevelLayout = ParseLevelLayout(minLevel) ?? finalMinLevelLayout;
                    SimpleLayout maxLevelLayout = ParseLevelLayout(maxLevel);
                    rule.EnableLoggingForLevelsLayout(minLevelLayout, maxLevelLayout, finalMinLevelLayout);
                }
                else
                {
                    LogLevel finalMinLogLevel = finalMinLevel != null ? LogLevelFromString(finalMinLevel) : null;
                    LogLevel minLogLevel = minLevel != null ? LogLevelFromString(minLevel) : (finalMinLogLevel ?? LogLevel.MinLevel);
                    LogLevel maxLogLevel = maxLevel != null ? LogLevelFromString(maxLevel) : LogLevel.MaxLevel;
                    rule.SetLoggingLevels(minLogLevel, maxLogLevel);
                    if (finalMinLogLevel != null)
                        rule.FinalMinLevel = finalMinLogLevel;
                }
            }
        }

        private static bool IsLevelLayout(string level)
        {
            return level?.IndexOf('{') >= 0;
        }

        private SimpleLayout ParseLevelLayout(string levelLayout)
        {
            SimpleLayout simpleLayout = !StringHelpers.IsNullOrWhiteSpace(levelLayout) ? CreateSimpleLayout(levelLayout) : null;
            simpleLayout?.Initialize(this);
            return simpleLayout;
        }

        private void ParseLoggingRuleTargets(string writeTargets, LoggingRule rule)
        {
            writeTargets = ExpandSimpleVariables(writeTargets);
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

        private void ParseLoggingRuleChildren(ValidatedConfigurationElement loggerElement, LoggingRule rule, string filterDefaultAction = null)
        {
            foreach (var child in loggerElement.ValidChildren)
            {
                LoggingRule childRule = null;
                if (child.MatchesName("filters"))
                {
                    ParseLoggingRuleFilters(rule, child, filterDefaultAction);
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
                    var configException = new NLogConfigurationException($"Unrecognized child element '{child.Name}' for element '{loggerElement.Name}' in section 'rules'");
                    if (MustThrowConfigException(configException))
                        throw configException;
                }

                if (childRule != null)
                {
                    ValidateLoggingRuleFilters(rule);

                    lock (rule.ChildRules)
                    {
                        rule.ChildRules.Add(childRule);
                    }
                }
            }
        }

        private void ParseLoggingRuleFilters(LoggingRule rule, ValidatedConfigurationElement filtersElement, string filterDefaultAction = null)
        {
            filtersElement.AssertName("filters");

            filterDefaultAction = filtersElement.GetOptionalValue("defaultAction", null) ?? filtersElement.GetOptionalValue(nameof(rule.FilterDefaultAction), null) ?? filterDefaultAction;
            if (filterDefaultAction != null)
            {
                if (ConversionHelpers.TryParseEnum(filterDefaultAction, typeof(FilterResult), out var enumValue))
                {
                    rule.FilterDefaultAction = (FilterResult)enumValue;
                }
                else
                {
                    var configException = new NLogConfigurationException($"Failed to parse Enum-value to assign property '{nameof(LoggingRule.FilterDefaultAction)}'='{filterDefaultAction}' for logging rule: {(string.IsNullOrEmpty(rule.RuleName) ? rule.LoggerNamePattern : rule.RuleName)}.");
                    if (MustThrowConfigException(configException))
                        throw configException;
                }
            }

            foreach (var filterElement in filtersElement.ValidChildren)
            {
                var filterType = filterElement.GetOptionalValue("type", null) ?? filterElement.Name;
                Filter filter = FactoryCreateInstance(filterType, ConfigurationItemFactory.Default.FilterFactory);
                ConfigureFromAttributesAndElements(filter, filterElement);
                rule.Filters.Add(filter);
            }
        }

        private void ValidateLoggingRuleFilters(LoggingRule rule)
        {
            bool overridesDefaultAction = rule.Filters.Count == 0 || rule.FilterDefaultAction != FilterResult.Ignore;
            for (int i = 0; i < rule.Filters.Count; ++i)
            {
                if (rule.Filters[i].Action != FilterResult.Ignore && rule.Filters[i].Action != FilterResult.IgnoreFinal && rule.Filters[i].Action != FilterResult.Neutral)
                    overridesDefaultAction = true;
            }
            if (!overridesDefaultAction)
            {
                var configException = new NLogConfigurationException($"LoggingRule where all filters and FilterDefaultAction=Ignore : {rule}");
                if (MustThrowConfigException(configException))
                    throw configException;
            }
        }

        private void ParseTargetsElement(ValidatedConfigurationElement targetsElement)
        {
            targetsElement.AssertName("targets", "appenders");

            bool asyncWrap = ParseBooleanValue("async", targetsElement.GetOptionalValue("async", "false"), false);

            ValidatedConfigurationElement defaultWrapperElement = null;
            Dictionary<string, ValidatedConfigurationElement> typeNameToDefaultTargetParameters = null;

            foreach (var targetElement in targetsElement.ValidChildren)
            {
                string targetTypeName = targetElement.GetConfigItemTypeAttribute();
                string targetValueName = targetElement.GetOptionalValue("name", null);
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
                            if (typeNameToDefaultTargetParameters is null)
                                typeNameToDefaultTargetParameters = new Dictionary<string, ValidatedConfigurationElement>(StringComparer.OrdinalIgnoreCase);
                            typeNameToDefaultTargetParameters[targetTypeName.Trim()] = targetElement;
                        }
                        break;

                    case "TARGET":
                    case "APPENDER":
                    case "WRAPPER":
                    case "WRAPPER-TARGET":
                    case "COMPOUND-TARGET":
                        if (AssertNonEmptyValue(targetTypeName, "type", targetValueName, targetsElement.Name))
                        {
                            AddNewTargetFromConfig(targetTypeName, targetElement, asyncWrap, typeNameToDefaultTargetParameters, defaultWrapperElement);
                        }
                        break;

                    default:
                        var configException = new NLogConfigurationException($"Unrecognized element '{targetValueName}' in section '{targetsElement.Name}'");
                        if (MustThrowConfigException(configException))
                            throw configException;
                        break;
                }
            }
        }

        private void AddNewTargetFromConfig(string targetTypeName, ValidatedConfigurationElement targetElement, bool asyncWrap, Dictionary<string, ValidatedConfigurationElement> typeNameToDefaultTargetParameters = null, ValidatedConfigurationElement defaultWrapperElement = null)
        {
            Target newTarget = null;

            try
            {
                newTarget = CreateTargetType(targetTypeName);
                if (newTarget != null)
                {
                    ParseTargetElement(newTarget, targetElement, typeNameToDefaultTargetParameters);

                    if (asyncWrap)
                    {
                        newTarget = WrapWithAsyncTargetWrapper(newTarget);
                    }

                    if (defaultWrapperElement != null)
                    {
                        newTarget = WrapWithDefaultWrapper(newTarget, defaultWrapperElement);
                    }

                    AddTarget(newTarget);
                }
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

                var configException = new NLogConfigurationException($"Target '{newTarget?.ToString() ?? targetTypeName}' has invalid config. Error: {ex.Message}", ex);
                if (MustThrowConfigException(configException))
                    throw;
            }
        }

        private Target CreateTargetType(string targetTypeName)
        {
            return FactoryCreateInstance(targetTypeName, ConfigurationItemFactory.Default.TargetFactory);
        }

        private void ParseTargetElement(Target target, ValidatedConfigurationElement targetElement, Dictionary<string, ValidatedConfigurationElement> typeNameToDefaultTargetParameters = null)
        {
            string targetTypeName = targetElement.GetConfigItemTypeAttribute("targets");
            if (typeNameToDefaultTargetParameters != null &&
                typeNameToDefaultTargetParameters.TryGetValue(targetTypeName, out var defaults))
            {
                ParseTargetElement(target, defaults, null);
            }

            var compound = target as CompoundTargetBase;
            var wrapper = target as WrapperTargetBase;

            ConfigureObjectFromAttributes(target, targetElement);

            foreach (var childElement in targetElement.ValidChildren)
            {
                if (compound != null &&
                    ParseCompoundTarget(compound, childElement, typeNameToDefaultTargetParameters, null))
                {
                    continue;
                }

                if (wrapper != null &&
                    ParseTargetWrapper(wrapper, childElement, typeNameToDefaultTargetParameters))
                {
                    continue;
                }

                SetPropertyValuesFromElement(target, childElement, targetElement);
            }
        }

        private bool ParseTargetWrapper(
            WrapperTargetBase wrapper,
            ValidatedConfigurationElement childElement,
            Dictionary<string, ValidatedConfigurationElement> typeNameToDefaultTargetParameters)
        {
            if (IsTargetRefElement(childElement.Name))
            {
                var targetName = childElement.GetRequiredValue("name", GetName(wrapper));

                Target newTarget = FindTargetByName(targetName);
                if (newTarget is null)
                {
                    var configException = new NLogConfigurationException($"Referenced target '{targetName}' not found.");
                    if (MustThrowConfigException(configException))
                        throw configException;
                }

                wrapper.WrappedTarget = newTarget;
                return true;
            }

            if (IsTargetElement(childElement.Name))
            {
                string targetTypeName = childElement.GetConfigItemTypeAttribute(GetName(wrapper));

                Target newTarget = CreateTargetType(targetTypeName);
                if (newTarget != null)
                {
                    ParseTargetElement(newTarget, childElement, typeNameToDefaultTargetParameters);
                    if (!string.IsNullOrEmpty(newTarget.Name))
                    {
                        // if the new target has name, register it
                        AddTarget(newTarget.Name, newTarget);
                    }
                    else if (!string.IsNullOrEmpty(wrapper.Name))
                    {
                        newTarget.Name = wrapper.Name + "_wrapped";
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
            CompoundTargetBase compound,
            ValidatedConfigurationElement childElement,
            Dictionary<string, ValidatedConfigurationElement> typeNameToDefaultTargetParameters,
            string targetName)
        {
            if (MatchesName(childElement.Name, "targets") || MatchesName(childElement.Name, "appenders"))
            {
                foreach (var child in childElement.ValidChildren)
                {
                    ParseCompoundTarget(compound, child, typeNameToDefaultTargetParameters, null);
                }

                return true;
            }

            if (IsTargetRefElement(childElement.Name))
            {
                targetName = childElement.GetRequiredValue("name", GetName(compound));

                Target newTarget = FindTargetByName(targetName);
                if (newTarget is null)
                {
                    throw new NLogConfigurationException("Referenced target '" + targetName + "' not found.");
                }

                compound.Targets.Add(newTarget);
                return true;
            }

            if (IsTargetElement(childElement.Name))
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

        private void ConfigureObjectFromAttributes<T>(T targetObject, ValidatedConfigurationElement element, bool ignoreType = true) where T : class
        {
            foreach (var kvp in element.ValueLookup)
            {
                string childName = kvp.Key;
                string childValue = kvp.Value;

                if (ignoreType && MatchesName(childName, "type"))
                {
                    continue;
                }

                SetPropertyValueFromString(targetObject, childName, childValue, element);
            }
        }

        private void SetPropertyValueFromString<T>(T targetObject, string propertyName, string propertyValue, ValidatedConfigurationElement element) where T : class
        {
            try
            {
                if (targetObject is null)
                {
                    throw new NLogConfigurationException($"'{typeof(T).Name}' is null, and cannot assign property '{propertyName}'='{propertyValue}'");
                }
                
                if (!PropertyHelper.TryGetPropertyInfo(ConfigurationItemFactory.Default, targetObject, propertyName, out var propertyInfo))
                {
                    throw new NLogConfigurationException($"'{targetObject.GetType()?.Name}' cannot assign unknown property '{propertyName}'='{propertyValue}'");
                }

                var propertyValueExpanded = ExpandSimpleVariables(propertyValue, out var matchingVariableName);
                if (matchingVariableName != null && TryLookupDynamicVariable(matchingVariableName, out var matchingLayout))
                {
                    if (propertyInfo.PropertyType.IsAssignableFrom(matchingLayout.GetType()))
                    {
                        PropertyHelper.SetPropertyValueForObject(targetObject, matchingLayout, propertyInfo);
                        return;
                    }
                }

                PropertyHelper.SetPropertyFromString(targetObject, propertyInfo, propertyValueExpanded, ConfigurationItemFactory.Default);
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

                var configException = new NLogConfigurationException($"'{targetObject?.GetType()?.Name}' cannot assign property '{propertyName}'='{propertyValue}' in section '{element.Name}'. Error: {ex.Message}", ex);
                if (MustThrowConfigException(configException))
                    throw;
            }
        }

        private void SetPropertyValuesFromElement<T>(T targetObject, ValidatedConfigurationElement childElement, ILoggingConfigurationElement parentElement) where T : class
        {
            if (targetObject is null)
            {
                var configException = new NLogConfigurationException($"'{typeof(T).Name}' is null, and cannot assign property '{childElement.Name}' in section '{parentElement.Name}'");
                if (MustThrowConfigException(configException))
                    throw configException;

                return;
            }
            
            if (!PropertyHelper.TryGetPropertyInfo(ConfigurationItemFactory.Default, targetObject, childElement.Name, out var propInfo))
            {
                var configException = new NLogConfigurationException($"'{targetObject.GetType()?.Name}' cannot assign unknown property '{childElement.Name}' in section '{parentElement.Name}'");
                if (MustThrowConfigException(configException))
                    throw configException;

                return;
            }

            if (AddArrayItemFromElement(targetObject, propInfo, childElement))
            {
                return;
            }

            if (SetLayoutFromElement(targetObject, propInfo, childElement))
            {
                return;
            }

            if (SetFilterFromElement(targetObject, propInfo, childElement))
            {
                return;
            }

            object propertyValue = propInfo.GetValue(targetObject, null);
            ConfigureFromAttributesAndElements(propertyValue, childElement);
        }

        private bool AddArrayItemFromElement(object o, PropertyInfo propInfo, ValidatedConfigurationElement element)
        {
            Type elementType = PropertyHelper.GetArrayItemType(propInfo);
            if (elementType != null)
            {
                IList propertyValue = (IList)propInfo.GetValue(o, null);

                if (string.Equals(propInfo.Name, element.Name, StringComparison.OrdinalIgnoreCase))
                {
                    bool foundChild = false;
                    foreach (var child in element.ValidChildren)
                    {
                        foundChild = true;
                        propertyValue.Add(ParseArrayItemFromElement(elementType, child));
                    }
                    if (foundChild)
                        return true;
                }

                object arrayItem = ParseArrayItemFromElement(elementType, element);
                propertyValue.Add(arrayItem);
                return true;
            }

            return false;
        }

        private object ParseArrayItemFromElement(Type elementType, ValidatedConfigurationElement element)
        {
            object arrayItem = TryCreateLayoutInstance(element, elementType);
            // arrayItem is not a layout
            if (arrayItem is null)
            {
                if (!ConfigurationItemFactory.Default.TryCreateInstance(elementType, out arrayItem))
                {
                    throw new NLogConfigurationException($"Factory returned null for {elementType}");
                }

                ConfigureFromAttributesAndElements(arrayItem, element);
            }
            return arrayItem;
        }

        private bool SetLayoutFromElement(object o, PropertyInfo propInfo, ValidatedConfigurationElement element)
        {
            var layout = TryCreateLayoutInstance(element, propInfo.PropertyType);
            // and is a Layout and 'type' attribute has been specified
            if (layout != null)
            {
                PropertyHelper.SetPropertyValueForObject(o, layout, propInfo);
                return true;
            }

            return false;
        }

        private bool SetFilterFromElement(object o, PropertyInfo propInfo, ValidatedConfigurationElement element)
        {
            Filter filter = TryCreateFilterInstance(element, propInfo.PropertyType);
            // and is a Filter and 'type' attribute has been specified
            if (filter != null)
            {
                PropertyHelper.SetPropertyValueForObject(o, filter, propInfo);
                return true;
            }

            return false;
        }

        private SimpleLayout CreateSimpleLayout(string layoutText)
        {
            return new SimpleLayout(layoutText, ConfigurationItemFactory.Default, LogFactory.ThrowConfigExceptions);
        }

        private Layout TryCreateLayoutInstance(ValidatedConfigurationElement element, Type type)
        {
            // Check if Layout type
            if (!typeof(Layout).IsAssignableFrom(type))
                return null;

            // Check if the 'type' attribute has been specified
            string classType = element.GetConfigItemTypeAttribute();
            if (classType is null)
                return null;

            var expandedClassType = ExpandSimpleVariables(classType, out var matchingVariableName);
            if (matchingVariableName != null && TryLookupDynamicVariable(matchingVariableName, out var matchingLayout))
            {
                if (type.IsAssignableFrom(matchingLayout.GetType()))
                {
                    return matchingLayout;
                }
            }

            var layoutInstance = FactoryCreateInstance(expandedClassType, ConfigurationItemFactory.Default.LayoutFactory);
            if (layoutInstance != null)
            {
                ConfigureFromAttributesAndElements(layoutInstance, element);
                return layoutInstance;
            }

            return null;
        }

        private Filter TryCreateFilterInstance(ValidatedConfigurationElement element, Type type)
        {
            var filter = TryCreateInstance(element, type, ConfigurationItemFactory.Default.FilterFactory);
            if (filter != null)
            {
                ConfigureFromAttributesAndElements(filter, element);
                return filter;
            }

            return null;
        }

        private T TryCreateInstance<T>(ValidatedConfigurationElement element, Type type, IFactory<T> factory)
            where T : class
        {
            // Check if correct type
            if (!typeof(T).IsAssignableFrom(type))
                return null;

            // Check if the 'type' attribute has been specified
            string classType = element.GetConfigItemTypeAttribute();
            if (classType is null)
                return null;

            return FactoryCreateInstance(classType, factory);
        }

        private T FactoryCreateInstance<T>(string typeName, IFactory<T> factory) where T : class
        {
            T newInstance = null;

            try
            {
                typeName = ExpandSimpleVariables(typeName);
                if (typeName.Contains(','))
                {
                    // Possible specification of assembly-name detected
                    var shortName = typeName.Substring(0, typeName.IndexOf(',')).Trim();
                    if (factory.TryCreateInstance(shortName, out newInstance) && newInstance != null)
                        return newInstance;

                    var assemblyName = typeName.Substring(typeName.IndexOf(',') + 1).Trim();
                    if (!string.IsNullOrEmpty(assemblyName))
                    {
                        // Attempt to load the assembly name extracted from the prefix
                        if (RegisterExtensionFromAssemblyName(assemblyName, typeName))
                        {
                            typeName = shortName;
                        }
                    }
                }

                newInstance = factory.CreateInstance(typeName);
                if (newInstance is null)
                {
                    throw new NLogConfigurationException($"Failed to create {typeof(T).Name} of type: '{typeName}' - Factory returned null");
                }
            }
            catch (NLogConfigurationException configException)
            {
                if (MustThrowConfigException(configException))
                    throw;
            }
            catch (Exception ex)
            {
                if (ex.MustBeRethrownImmediately())
                    throw;

                var configException = new NLogConfigurationException($"Failed to create {typeof(T).Name} of type: {typeName}", ex);
                if (MustThrowConfigException(configException))
                    throw configException;
            }

            return newInstance;
        }

        private void ConfigureFromAttributesAndElements<T>(T targetObject, ValidatedConfigurationElement element) where T : class
        {
            ConfigureObjectFromAttributes(targetObject, element);

            foreach (var childElement in element.ValidChildren)
            {
                SetPropertyValuesFromElement(targetObject, childElement, element);
            }
        }

        private static Target WrapWithAsyncTargetWrapper(Target target)
        {
#if !NET35
            if (target is AsyncTaskTarget)
            {
                InternalLogger.Debug("Skip wrapping target '{0}' with AsyncTargetWrapper", target.Name);
                return target;
            }
#endif

            if (target is AsyncTargetWrapper)
            {
                InternalLogger.Debug("Skip wrapping target '{0}' with AsyncTargetWrapper", target.Name);
                return target;
            }

            var asyncTargetWrapper = new AsyncTargetWrapper();
            asyncTargetWrapper.WrappedTarget = target;
            asyncTargetWrapper.Name = target.Name;
            target.Name = target.Name + "_wrapped";
            InternalLogger.Debug("Wrapping target '{0}' with AsyncTargetWrapper and renaming to '{1}'",
                asyncTargetWrapper.Name, target.Name);
            target = asyncTargetWrapper;
            return target;
        }

        private Target WrapWithDefaultWrapper(Target target, ValidatedConfigurationElement defaultWrapperElement)
        {
            string wrapperTypeName = defaultWrapperElement.GetConfigItemTypeAttribute("targets");
            Target wrapperTargetInstance = CreateTargetType(wrapperTypeName);
            WrapperTargetBase wtb = wrapperTargetInstance as WrapperTargetBase;
            if (wtb is null)
            {
                throw new NLogConfigurationException("Target type specified on <default-wrapper /> is not a wrapper.");
            }

            ParseTargetElement(wrapperTargetInstance, defaultWrapperElement);
            while (wtb.WrappedTarget != null)
            {
                wtb = wtb.WrappedTarget as WrapperTargetBase;
                if (wtb is null)
                {
                    throw new NLogConfigurationException(
                        "Child target type specified on <default-wrapper /> is not a wrapper.");
                }
            }

#if !NET35
            if (target is AsyncTaskTarget && wrapperTargetInstance is AsyncTargetWrapper && ReferenceEquals(wrapperTargetInstance, wtb))
            {
                InternalLogger.Debug("Skip wrapping target '{0}' with AsyncTargetWrapper", target.Name);
                return target;
            }
#endif

            wtb.WrappedTarget = target;
            wrapperTargetInstance.Name = target.Name;
            target.Name = target.Name + "_wrapped";

            InternalLogger.Debug("Wrapping target '{0}' with '{1}' and renaming to '{2}'", wrapperTargetInstance.Name,
                wrapperTargetInstance.GetType(), target.Name);
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

                var configException = new NLogConfigurationException($"'{propertyName}' hasn't a valid boolean value '{value}'. {defaultValue} will be used", exception);
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

        /// <summary>
        /// Config element that's validated and having extra context
        /// </summary>
        private sealed class ValidatedConfigurationElement : ILoggingConfigurationElement
        {
            private static readonly IDictionary<string, string> EmptyDefaultDictionary = new SortHelpers.ReadOnlySingleBucketDictionary<string, string>();

            private readonly ILoggingConfigurationElement _element;
            private readonly bool _throwConfigExceptions;
            private IList<ValidatedConfigurationElement> _validChildren;

            public static ValidatedConfigurationElement Create(ILoggingConfigurationElement element, LogFactory logFactory)
            {
                if (element is ValidatedConfigurationElement validConfig)
                    return validConfig;

                bool throwConfigExceptions = (logFactory.ThrowConfigExceptions ?? logFactory.ThrowExceptions) || (LogManager.ThrowConfigExceptions ?? LogManager.ThrowExceptions);
                return new ValidatedConfigurationElement(element, throwConfigExceptions);
            }

            public ValidatedConfigurationElement(ILoggingConfigurationElement element, bool throwConfigExceptions)
            {
                _throwConfigExceptions = throwConfigExceptions;
                Name = element.Name.Trim();
                ValueLookup = CreateValueLookup(element, throwConfigExceptions);
                _element = element;
            }

            public string Name { get; }

            public IDictionary<string, string> ValueLookup { get; }

            public IEnumerable<ValidatedConfigurationElement> ValidChildren
            {
                get
                {
                    if (_validChildren is null)
                        return YieldAndCacheValidChildren();
                    else
                        return _validChildren;
                }
            }

            IEnumerable<ValidatedConfigurationElement> YieldAndCacheValidChildren()
            {
                IList<ValidatedConfigurationElement> validChildren = null;
                foreach (var child in _element.Children)
                {
                    validChildren = validChildren ?? new List<ValidatedConfigurationElement>();
                    var validChild = new ValidatedConfigurationElement(child, _throwConfigExceptions);
                    validChildren.Add(validChild);
                    yield return validChild;
                }
                _validChildren = validChildren ?? ArrayHelper.Empty<ValidatedConfigurationElement>();
            }

            public IEnumerable<KeyValuePair<string, string>> Values => ValueLookup;

            /// <remarks>
            /// Explicit cast because NET35 doesn't support covariance.
            /// </remarks>
            IEnumerable<ILoggingConfigurationElement> ILoggingConfigurationElement.Children => ValidChildren.Cast<ILoggingConfigurationElement>();

            public string GetRequiredValue(string attributeName, string section)
            {
                string value = GetOptionalValue(attributeName, null);
                if (value is null)
                {
                    throw new NLogConfigurationException($"Expected {attributeName} on {Name} in {section}");
                }

                if (StringHelpers.IsNullOrWhiteSpace(value))
                {
                    throw new NLogConfigurationException(
                        $"Expected non-empty {attributeName} on {Name} in {section}");
                }

                return value;
            }

            public string GetOptionalValue(string attributeName, string defaultValue)
            {
                ValueLookup.TryGetValue(attributeName, out string value);
                return value ?? defaultValue;
            }

            private static IDictionary<string, string> CreateValueLookup(ILoggingConfigurationElement element, bool throwConfigExceptions)
            {
                IDictionary<string, string> valueLookup = null;
                List<string> warnings = null;
                foreach (var attribute in element.Values)
                {
                    var attributeKey = attribute.Key?.Trim() ?? string.Empty;
                    valueLookup = valueLookup ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    if (!string.IsNullOrEmpty(attributeKey) && !valueLookup.ContainsKey(attributeKey))
                    {
                        valueLookup[attributeKey] = attribute.Value;
                    }
                    else
                    {
                        string validationError = string.IsNullOrEmpty(attributeKey) ? $"Invalid property for '{element.Name}' without name. Value={attribute.Value}"
                            : $"Duplicate value for '{element.Name}'. PropertyName={attributeKey}. Skips Value={attribute.Value}. Existing Value={valueLookup[attributeKey]}";
                        InternalLogger.Debug("Skipping {0}", validationError);
                        if (throwConfigExceptions)
                        {
                            warnings = warnings ?? new List<string>();
                            warnings.Add(validationError);
                        }
                    }
                }
                if (throwConfigExceptions && warnings?.Count > 0)
                {
                    throw new NLogConfigurationException(StringHelpers.Join(Environment.NewLine, warnings));
                }
                return valueLookup ?? EmptyDefaultDictionary;
            }

            public override string ToString()
            {
                return Name;
            }
        }
    }
}
