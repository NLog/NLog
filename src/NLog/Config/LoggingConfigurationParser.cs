// 
// Copyright (c) 2004-2020 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
                            DefaultCultureInfo = CultureInfo.InvariantCulture;
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
                        var internalLogFile = configItem.Value?.Trim();
                        if (!string.IsNullOrEmpty(internalLogFile))
                        {
                            internalLogFile = ExpandFilePathVariables(internalLogFile);
                            InternalLogger.LogFile = internalLogFile;
                        }
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

            if (!internalLoggerEnabled && !InternalLogger.HasActiveLoggers())
            {
                InternalLogger.LogLevel = LogLevel.Off; // Reduce overhead of the InternalLogger when not configured
            }

            if (autoLoadExtensions)
            {
                ConfigurationItemFactory.ScanForAutoLoadExtensions(LogFactory);
            }

            _serviceRepository.ConfigurationItemFactory.ParseMessageTemplates = parseMessageTemplates;
        }

        /// <summary>
        /// Builds list with unique keys, using last value of duplicates. High priority keys placed first.
        /// </summary>
        /// <param name="nlogConfig"></param>
        /// <returns></returns>
        private ICollection<KeyValuePair<string, string>> CreateUniqueSortedListFromConfig(ILoggingConfigurationElement nlogConfig)
        {
            var dict = ValidatedConfigurationElement.Create(nlogConfig, LogFactory).ValueLookup;
            if (dict.Count == 0)
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

        private string ExpandFilePathVariables(string internalLogFile)
        {
            try
            {
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

                var configException = new NLogConfigurationException(exception, $"Property '{propertyName}' assigned invalid LogLevel value '{propertyValue}'. Fallback to '{fallbackValue}'");
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
                    ParseExtensionWithAssembly(assemblyName, prefix);
                }
            }
        }

        private void RegisterExtension(string type, string itemNamePrefix)
        {
            try
            {
                _serviceRepository.ConfigurationItemFactory.RegisterType(Type.GetType(type, true), itemNamePrefix);
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
                _serviceRepository.ConfigurationItemFactory.RegisterItemsFromAssembly(asm, prefix);
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
                _serviceRepository.ConfigurationItemFactory.RegisterItemsFromAssembly(asm, prefix);
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

            Layout variableLayout = variableValue != null
                ? CreateSimpleLayout(ExpandSimpleVariables(variableValue))
                : ParseVariableLayoutValue(variableElement);

            if (!AssertNotNullValue(variableLayout, "value", variableElement.Name, "variables"))
                return;

            InsertParsedConfigVariable(variableName, variableLayout);
        }

        private Layout ParseVariableLayoutValue(ValidatedConfigurationElement variableElement)
        {
            var childElement = variableElement.ValidChildren.FirstOrDefault();
            if (childElement != null)
            {
                var variableLayout = TryCreateLayoutInstance(childElement, typeof(Layout));
                if (variableLayout != null)
                {
                    ConfigureFromAttributesAndElements(variableLayout, childElement);
                    return variableLayout;
                }
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

            TimeSource newTimeSource = FactoryCreateInstance(timeSourceType, _serviceRepository.ConfigurationItemFactory.TimeSources);
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
            if (value != null)
                return true;

            return AssertNonEmptyValue(string.Empty, propertyName, elementName, sectionName);
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

            EnableLevelsForRule(rule, enableLevels, minLevel, maxLevel);

            ParseLoggingRuleTargets(writeTargets, rule);

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
                    foreach (var logLevel in enableLevels.SplitAndTrimTokens(','))
                    {
                        rule.EnableLoggingForLevel(LogLevelFromString(logLevel));
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
            SimpleLayout simpleLayout = !StringHelpers.IsNullOrWhiteSpace(levelLayout) ? CreateSimpleLayout(levelLayout) : null;
            simpleLayout?.Initialize(this);
            return simpleLayout;
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

        private void ParseLoggingRuleChildren(ValidatedConfigurationElement loggerElement, LoggingRule rule)
        {
            foreach (var child in loggerElement.ValidChildren)
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
                    var configException = new NLogConfigurationException($"Unrecognized child element '{child.Name}' for element '{loggerElement.Name}' in section 'rules'");
                    if (MustThrowConfigException(configException))
                        throw configException;
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

        private void ParseFilters(LoggingRule rule, ValidatedConfigurationElement filtersElement)
        {
            filtersElement.AssertName("filters");

            var defaultActionResult = filtersElement.GetOptionalValue("defaultAction", null);
            if (defaultActionResult != null)
            {
                SetPropertyValueFromString(rule, nameof(rule.DefaultFilterResult), defaultActionResult, filtersElement);
            }

            foreach (var filterElement in filtersElement.ValidChildren)
            {
                string name = filterElement.Name;

                Filter filter = FactoryCreateInstance(name, _serviceRepository.ConfigurationItemFactory.Filters);
                ConfigureFromAttributesAndElements(filter, filterElement, false);
                rule.Filters.Add(filter);
            }
        }

        private void ParseTargetsElement(ValidatedConfigurationElement targetsElement)
        {
            targetsElement.AssertName("targets", "appenders");

            bool asyncWrap = ParseBooleanValue("async", targetsElement.GetOptionalValue("async", "false"), false);

            ValidatedConfigurationElement defaultWrapperElement = null;
            var typeNameToDefaultTargetParameters =
                new Dictionary<string, ValidatedConfigurationElement>(StringComparer.OrdinalIgnoreCase);

            foreach (var targetElement in targetsElement.ValidChildren)
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
                        if (AssertNonEmptyValue(targetTypeName, "type", targetValueName, targetsElement.Name))
                        {
                            defaultWrapperElement = targetElement;
                        }
                        break;

                    case "DEFAULT-TARGET-PARAMETERS":
                        if (AssertNonEmptyValue(targetTypeName, "type", targetValueName, targetsElement.Name))
                        {
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
                            newTarget = CreateTargetType(targetTypeName);
                            if (newTarget != null)
                            {
                                ParseTargetElement(newTarget, targetElement, typeNameToDefaultTargetParameters);
                            }
                        }
                        break;

                    default:
                        var configException = new NLogConfigurationException($"Unrecognized element '{targetValueName}' in section '{targetsElement.Name}'");
                        if (MustThrowConfigException(configException))
                            throw configException;
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
            return FactoryCreateInstance(targetTypeName, _serviceRepository.ConfigurationItemFactory.Targets);
        }

        private void ParseTargetElement(Target target, ValidatedConfigurationElement targetElement,
            Dictionary<string, ValidatedConfigurationElement> typeNameToDefaultTargetParameters = null)
        {
            string targetTypeName = targetElement.GetConfigItemTypeAttribute("targets");
            if (typeNameToDefaultTargetParameters != null &&
                typeNameToDefaultTargetParameters.TryGetValue(targetTypeName, out var defaults))
            {
                ParseTargetElement(target, defaults, null);
            }

            var compound = target as CompoundTargetBase;
            var wrapper = target as WrapperTargetBase;

            ConfigureObjectFromAttributes(target, targetElement, true);

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
                if (newTarget == null)
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
                if (newTarget == null)
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

        private void ConfigureObjectFromAttributes(object targetObject, ValidatedConfigurationElement element, bool ignoreType)
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

        private void SetPropertyValueFromString(object targetObject, string propertyName, string propertyValue, ValidatedConfigurationElement element)
        {
            try
            {
                var propertyValueExpanded = ExpandSimpleVariables(propertyValue, out var matchingVariableName);
                if (matchingVariableName != null && propertyValueExpanded == propertyValue && TrySetPropertyFromConfigVariableLayout(targetObject, propertyName, matchingVariableName))
                    return;

                PropertyHelper.SetPropertyFromString(targetObject, propertyName, propertyValueExpanded, _serviceRepository.ConfigurationItemFactory);
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

                var configException = new NLogConfigurationException($"Error when setting property '{propertyName}'='{propertyValue}' on {targetObject?.GetType()} in section '{element.Name}'", ex);
                if (MustThrowConfigException(configException))
                    throw;
            }
        }

        private bool TrySetPropertyFromConfigVariableLayout(object targetObject, string propertyName, string configVariableName)
        {
            if (TryLookupDynamicVariable(configVariableName, out var matchingLayout) && PropertyHelper.TryGetPropertyInfo(targetObject, propertyName, out var propInfo) && propInfo.PropertyType.IsAssignableFrom(matchingLayout.GetType()))
            {
                propInfo.SetValue(targetObject, matchingLayout, null);
                return true;
            }

            return false;
        }

        private void SetPropertyValuesFromElement(object o, ValidatedConfigurationElement childElement, ILoggingConfigurationElement parentElement)
        {
            if (!PropertyHelper.TryGetPropertyInfo(o, childElement.Name, out var propInfo))
            {
                var configException = new NLogConfigurationException($"Unknown property '{childElement.Name}' for '{o?.GetType()}' in section '{parentElement.Name}'");
                if (MustThrowConfigException(configException))
                    throw configException;

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

            object item = propInfo.GetValue(o, null);
            ConfigureFromAttributesAndElements(item, childElement);
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
            if (arrayItem == null)
                arrayItem = _serviceRepository.GetService(elementType);

            ConfigureFromAttributesAndElements(arrayItem, element);
            return arrayItem;
        }

        private bool SetLayoutFromElement(object o, PropertyInfo propInfo, ValidatedConfigurationElement element)
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

        private bool SetFilterFromElement(object o, PropertyInfo propInfo, ValidatedConfigurationElement element)
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

        private SimpleLayout CreateSimpleLayout(string layoutText)
        {
            return new SimpleLayout(layoutText, _serviceRepository.ConfigurationItemFactory, LogFactory.ThrowConfigExceptions);
        }

        private Layout TryCreateLayoutInstance(ValidatedConfigurationElement element, Type type)
        {
            return TryCreateInstance(element, type, _serviceRepository.ConfigurationItemFactory.Layouts);
        }

        private Filter TryCreateFilterInstance(ValidatedConfigurationElement element, Type type)
        {
            return TryCreateInstance(element, type, _serviceRepository.ConfigurationItemFactory.Filters);
        }

        private T TryCreateInstance<T>(ValidatedConfigurationElement element, Type type, INamedItemFactory<T, Type> factory)
            where T : class
        {
            // Check if correct type
            if (!typeof(T).IsAssignableFrom(type))
                return null;

            // Check if the 'type' attribute has been specified
            string classType = element.GetConfigItemTypeAttribute();
            if (classType == null)
                return null;

            return FactoryCreateInstance(classType, factory);
        }

        private T FactoryCreateInstance<T>(string classType, INamedItemFactory<T, Type> factory) where T : class
        {
            T newInstance = null;

            try
            {
                classType = ExpandSimpleVariables(classType);
                if (classType.Contains(','))
                {
                    // Possible specification of assemlby-name detected
                    if (factory.TryCreateInstance(classType, out newInstance) && newInstance != null)
                        return newInstance;

                    // Attempt to load the assembly name extracted from the prefix
                    classType = RegisterExtensionFromAssemblyName(classType);
                }

                newInstance = factory.CreateInstance(classType);
                if (newInstance == null)
                {
                    throw new NLogConfigurationException($"Factory returned null for {typeof(T).Name} of type: {classType}");
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

                var configException = new NLogConfigurationException($"Failed to create {typeof(T).Name} of type: {classType}", ex);
                if (MustThrowConfigException(configException))
                    throw configException;
            }

            return newInstance;
        }

        private string RegisterExtensionFromAssemblyName(string classType)
        {
            var assemblyName = classType.Substring(classType.IndexOf(',') + 1).Trim();
            if (!string.IsNullOrEmpty(assemblyName))
            {
                try
                {
                    ParseExtensionWithAssembly(assemblyName, string.Empty);
                    return classType.Substring(0, classType.IndexOf(',')).Trim() + ", " + assemblyName; // uniform format
                }
                catch (Exception ex)
                {
                    if (ex.MustBeRethrownImmediately())
                        throw;
                }
            }

            return classType;
        }

        private void SetItemOnProperty(object o, PropertyInfo propInfo, ValidatedConfigurationElement element, object properyValue)
        {
            ConfigureFromAttributesAndElements(properyValue, element);
            propInfo.SetValue(o, properyValue, null);
        }

        private void ConfigureFromAttributesAndElements(object targetObject, ValidatedConfigurationElement element, bool ignoreTypeProperty = true)
        {
            ConfigureObjectFromAttributes(targetObject, element, ignoreTypeProperty);

            foreach (var childElement in element.ValidChildren)
            {
                SetPropertyValuesFromElement(targetObject, childElement, element);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability",
            "CA2000:Dispose objects before losing scope", Justification = "Target is disposed elsewhere.")]
        private static Target WrapWithAsyncTargetWrapper(Target target)
        {
#if !NET35
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

        private Target WrapWithDefaultWrapper(Target target, ValidatedConfigurationElement defaultWrapperElement)
        {
            string wrapperTypeName = defaultWrapperElement.GetConfigItemTypeAttribute("targets");
            Target wrapperTargetInstance = CreateTargetType(wrapperTypeName);
            WrapperTargetBase wtb = wrapperTargetInstance as WrapperTargetBase;
            if (wtb == null)
            {
                throw new NLogConfigurationException("Target type specified on <default-wrapper /> is not a wrapper.");
            }

            ParseTargetElement(wrapperTargetInstance, defaultWrapperElement);
            while (wtb.WrappedTarget != null)
            {
                wtb = wtb.WrappedTarget as WrapperTargetBase;
                if (wtb == null)
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

        /// <summary>
        /// Config element that's validated and having extra context
        /// </summary>
        private class ValidatedConfigurationElement : ILoggingConfigurationElement
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
                    if (_validChildren != null)
                        return _validChildren;
                    else
                        return YieldAndCacheValidChildren();
                }
            }

            IEnumerable<ValidatedConfigurationElement> YieldAndCacheValidChildren()
            {
                foreach (var child in _element.Children)
                {
                    _validChildren = _validChildren ?? new List<ValidatedConfigurationElement>();
                    var validChild = new ValidatedConfigurationElement(child, _throwConfigExceptions);
                    _validChildren.Add(validChild);
                    yield return validChild;
                }
                _validChildren = _validChildren ?? ArrayHelper.Empty<ValidatedConfigurationElement>();
            }

            public IEnumerable<KeyValuePair<string, string>> Values => ValueLookup;

            /// <remarks>
            /// Explicit cast because NET35 doesn't support covariance.
            /// </remarks>
            IEnumerable<ILoggingConfigurationElement> ILoggingConfigurationElement.Children => ValidChildren.Cast<ILoggingConfigurationElement>();

            public string GetRequiredValue(string attributeName, string section)
            {
                string value = GetOptionalValue(attributeName, null);
                if (value == null)
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
        }
    }
}