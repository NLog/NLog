// 
// Copyright (c) 2004-2018 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    /// Interface for accessing configuration details
    /// </summary>
    public interface ILoggingConfigurationSection
    {
        /// <summary>
        /// Name of the config section
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Configuration Key/Value Pairs
        /// </summary>
        IEnumerable<KeyValuePair<string,string>> Values { get; }
        /// <summary>
        /// Child config sections
        /// </summary>
        IEnumerable<ILoggingConfigurationSection> Children { get; }
    }

    static class ILoggingConfigurationSectionExtensions
    {
        public static void AssertName(this ILoggingConfigurationSection section, params string[] allowedNames)
        {
            foreach (var en in allowedNames)
            {
                if (string.Equals(section?.Name, en, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }

            throw new InvalidOperationException($"Assertion failed. Expected element name '{string.Join("|", allowedNames)}', actual: '{section?.Name}'.");
        }

        public static string GetRequiredValue(this ILoggingConfigurationSection section,  string attributeName)
        {
            string value = section.GetOptionalValue(attributeName, null);
            if (value == null)
            {
                throw new NLogConfigurationException($"Expected {attributeName} on <{section.Name}/>");
            }

            return value;
        }

        public static string GetOptionalValue(this ILoggingConfigurationSection section, string attributeName, string defaultValue)
        {
            string value = section.Values.Where(configItem => string.Equals(configItem.Key, attributeName, StringComparison.OrdinalIgnoreCase)).Select(configItem => configItem.Value).FirstOrDefault();
            if (value == null)
            {
                return defaultValue;
            }

            return value;
        }

        public static bool GetOptionalBooleanValue(this ILoggingConfigurationSection section, string attributeName, bool defaultValue)
        {
            string value = section.GetOptionalValue(attributeName, null);
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }
            return Convert.ToBoolean(value.Trim(), CultureInfo.InvariantCulture);
        }
    }

    /// <summary>
    /// Loads NLog configuration from <see cref="ILoggingConfigurationSection"/>
    /// </summary>
    public abstract class LoggingConfigurationReader : LoggingConfiguration
    {
        private ConfigurationItemFactory _configurationItemFactory;
        private readonly bool _xmlConfigMode;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logFactory"></param>
        protected LoggingConfigurationReader(LogFactory logFactory)
            : this(logFactory, false)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logFactory"></param>
        /// <param name="xmlConfigMode"></param>
        internal LoggingConfigurationReader(LogFactory logFactory, bool xmlConfigMode)
            :base(logFactory)
        {
            _xmlConfigMode = xmlConfigMode;
        }

        /// <summary>
        /// Loads NLog configuration from provided config section
        /// </summary>
        /// <param name="nlogConfig"></param>
        /// <param name="basePath"></param>
        protected void LoadConfig(ILoggingConfigurationSection nlogConfig, string basePath)
        {
            InternalLogger.Trace("ParseNLogConfig");
            nlogConfig.AssertName("nlog");

            bool? parseMessageTemplates = null;

            string internalLogFile = null;

            foreach (var configItem in nlogConfig.Values)
            {
                switch (configItem.Key?.Trim()?.ToUpperInvariant())
                {
                    case "USEINVARIANTCULTURE": if (ParseBooleanValue(configItem.Value)) DefaultCultureInfo = CultureInfo.InvariantCulture; break;
                    case "INTERNALLOGLEVEL": InternalLogger.LogLevel = LogLevel.FromString(configItem.Value); break;
#pragma warning disable 618
                    case "EXCEPTIONLOGGINGOLDSTYLE": ExceptionLoggingOldStyle = ParseBooleanValue(configItem.Value); break;
#pragma warning restore 618
                    case "THROWEXCEPTIONS": LogFactory.ThrowExceptions = ParseBooleanValue(configItem.Value); break;
                    case "THROWCONFIGEXCEPTIONS": LogFactory.ThrowConfigExceptions = string.IsNullOrEmpty(configItem.Value) ? (bool?)null : ParseBooleanValue(configItem.Value); break;
                    case "KEEPVARIABLESONRELOAD": LogFactory.KeepVariablesOnReload = ParseBooleanValue(configItem.Value); break;
                    case "INTERNALLOGTOCONSOLE": InternalLogger.LogToConsole = ParseBooleanValue(configItem.Value); break;
                    case "INTERNALLOGTOCONSOLEERROR": InternalLogger.LogToConsoleError = ParseBooleanValue(configItem.Value); break;
                    case "INTERNALLOGFILE": internalLogFile = configItem.Value?.Trim(); break;
#if !SILVERLIGHT && !__IOS__ && !__ANDROID__
                    case "INTERNALLOGTOTRACE": InternalLogger.LogToTrace = ParseBooleanValue(configItem.Value); break;
#endif
                    case "INTERNALLOGINCLUDETIMESTAMP": InternalLogger.IncludeTimestamp = ParseBooleanValue(configItem.Value); break;
                    case "GLOBALTHRESHOLD": LogFactory.GlobalThreshold = LogLevel.FromString(configItem.Value); break;
                    case "PARSEMESSAGETEMPLATES": parseMessageTemplates = string.IsNullOrEmpty(configItem.Value) ? (bool?)null : ParseBooleanValue(configItem.Value); break;
                }
            }

            if (internalLogFile != null)
                InternalLogger.LogFile = internalLogFile;

            _configurationItemFactory = ConfigurationItemFactory.Default;
            _configurationItemFactory.ParseMessageTemplates = parseMessageTemplates;

            var children = nlogConfig.Children.ToList();

            //first load the extensions, as the can be used in other elements (targets etc)
            var extensionsChilds = children.Where(child => string.Equals(child.Name, "extensions", StringComparison.OrdinalIgnoreCase)).ToList();
            foreach (var extensionsChild in extensionsChilds)
            {
                ParseExtensionsElement(extensionsChild, basePath);
            }

            if (!_xmlConfigMode)
            {
                // Variables can be used in other elements (targets etc)
                var variablesChilds = children.Where(child => string.Equals(child.Name, "variables", StringComparison.OrdinalIgnoreCase)).ToList();
                foreach (var variablesChild in variablesChilds)
                {
                    ParseVariablesElement(variablesChild);
                }
            }

            var rulesList = new List<ILoggingConfigurationSection>();

            //parse all other direct elements
            foreach (var child in children)
            {
                if (string.Equals(child.Name, "rules", StringComparison.OrdinalIgnoreCase))
                {
                    //postpone parsing <rules> to the end
                    rulesList.Add(child);
                }
                else if (string.Equals(child.Name, "extensions", StringComparison.OrdinalIgnoreCase))
                {
                    continue;   //already parsed
                }
                else if (!_xmlConfigMode && string.Equals(child.Name, "variables", StringComparison.OrdinalIgnoreCase))
                {
                    continue;   //already parsed
                }
                else
                {
                    if (!ParseNLogSection(child))
                    {
                        InternalLogger.Warn("Skipping unknown 'NLog' child node: {0}", child.Name);
                    }
                }
            }

            foreach (var ruleChild in rulesList)
            {
                ParseRulesElement(ruleChild, LoggingRules);
            }
        }

        /// <summary>
        /// Parses a single config section within the NLog-config
        /// </summary>
        /// <param name="configSection"></param>
        /// <returns>Section was recognized</returns>
        protected virtual bool ParseNLogSection(ILoggingConfigurationSection configSection)
        {
            switch (configSection.Name.ToUpperInvariant())
            {
                case "EXTENSIONS":
                    //already parsed
                    return true;

                case "APPENDERS":
                case "TARGETS":
                    ParseTargetsElement(configSection);
                    return true;

                case "VARIABLE":
                    ParseVariableElement(configSection);
                    return true;

                case "VARIABLES":
                    ParseVariablesElement(configSection);
                    return true;

                case "TIME":
                    ParseTimeElement(configSection);
                    return true;
            }
            return false;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Reflection.Assembly.LoadFrom", Justification = "Need to load external assembly.")]
        private void ParseExtensionsElement(ILoggingConfigurationSection extensionsElement, string baseDirectory)
        {
            extensionsElement.AssertName("extensions");

            foreach (var addElement in extensionsElement.Children)
            {
                if (_xmlConfigMode && !string.Equals(addElement.Name, "add", StringComparison.OrdinalIgnoreCase))
                {
                    InternalLogger.Warn("Skipping unknown 'Extensions' child node: {0}", addElement.Name);
                    continue;
                }

                string prefix = null;
                string type = null;
                string assemblyFile = null;
                string assemblyName = null;
                foreach (var configItem in addElement.Values)
                {
                    if (MatchesName(configItem.Key, "prefix"))
                    {
                        prefix = configItem.Value + ".";
                    }
                    else if (MatchesName(configItem.Key, "type"))
                    {
                        type = configItem.Value;
                    }
                    else if (MatchesName(configItem.Key, "assemblyFile"))
                    {
                        assemblyFile = configItem.Value;
                    }
                    else if (MatchesName(configItem.Key, "assembly"))
                    {
                        assemblyName = configItem.Value;
                    }
                }

                if (type != null)
                {
                    try
                    {
                        _configurationItemFactory.RegisterType(Type.GetType(type, true), prefix);
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
                if (assemblyFile != null)
                {
                    ParseExtensionWithAssemblyFile(baseDirectory, assemblyFile, prefix);
                    continue;
                }
#endif

                if (assemblyName != null)
                {
                    ParseExtensionWithAssembly(assemblyName, prefix);
                    continue;
                }

                InternalLogger.Warn("Skipping empty 'Extensions' child node: {0}", addElement.Name);
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

        private void ParseVariableElement(ILoggingConfigurationSection variableElement)
        {
            variableElement.AssertName("variable");

            string name = variableElement.GetRequiredValue("name");
            string value = ExpandSimpleVariables(variableElement.GetRequiredValue("value"));

            Variables[name] = value;
        }

        private void ParseVariablesElement(ILoggingConfigurationSection variableElement)
        {
            variableElement.AssertName("variables");

            foreach (var configItem in variableElement.Values)
            {
                string value = ExpandSimpleVariables(configItem.Value);
                Variables[configItem.Key] = value;
            }
        }

        private void ParseTimeElement(ILoggingConfigurationSection timeElement)
        {
            timeElement.AssertName("time");

            string type = timeElement.GetRequiredValue("type");

            TimeSource newTimeSource = _configurationItemFactory.TimeSources.CreateInstance(type);

            ConfigureObjectFromAttributes(newTimeSource, timeElement, true);

            InternalLogger.Info("Selecting time source {0}", newTimeSource);
            TimeSource.Current = newTimeSource;
        }

        /// <summary>
        /// Parse {Rules} xml element
        /// </summary>
        /// <param name="rulesElement"></param>
        /// <param name="rulesCollection">Rules are added to this parameter.</param>
        private void ParseRulesElement(ILoggingConfigurationSection rulesElement, IList<LoggingRule> rulesCollection)
        {
            InternalLogger.Trace("ParseRulesElement");
            rulesElement.AssertName("rules");

            foreach (var ruleElement in rulesElement.Children)
            {
                LoggingRule loggingRule = null;

                if (_xmlConfigMode && string.Equals(ruleElement.Name, "logger", StringComparison.OrdinalIgnoreCase))
                {
                    loggingRule = ParseRuleElement(ruleElement, null);  // Legacy xml mode
                }
                else if (string.Equals(ruleElement.Name, "rule", StringComparison.OrdinalIgnoreCase))
                {
                    loggingRule = ParseRuleElement(ruleElement, null);
                }
                else if (!_xmlConfigMode)
                {
                    loggingRule = ParseRuleElement(ruleElement, ruleElement.Name);  // Json mode
                }
                else
                {
                    InternalLogger.Warn("Skipping unknown 'Rules' child node: {0}", ruleElement.Name);
                }

                if (loggingRule != null)
                {
                    lock (rulesCollection)
                    {
                        rulesCollection.Add(loggingRule);
                    }
                }
            }
        }

        /// <summary>
        /// Parse {Logger} xml element
        /// </summary>
        /// <param name="loggerElement"></param>
        /// <param name="ruleName">Predefined name of the logging-rule.</param>
        private LoggingRule ParseRuleElement(ILoggingConfigurationSection loggerElement, string ruleName)
        {
            string namePattern = "*";
            bool enabled = true;
            bool final = false;
            string writeTargets = null;
            foreach (var configItem in loggerElement.Values)
            {
                switch (configItem.Key.ToUpperInvariant())
                {
                    case "NAME":
                        {
                            if (_xmlConfigMode && string.Equals(loggerElement.Name, "logger", StringComparison.OrdinalIgnoreCase))
                                namePattern = configItem.Value; // Legacy xml mode
                            else
                                ruleName = configItem.Value;
                        } break;
                    case "LOGGER": namePattern = configItem.Value; break;
                    case "ENABLED": enabled = ParseBooleanValue(configItem.Value); break;
                    case "APPENDTO": writeTargets = configItem.Value; break;
                    case "WRITETO": writeTargets = configItem.Value; break;
                    case "FINAL": final = ParseBooleanValue(configItem.Value); break;
                }
            }

            if (!enabled)
            {
                InternalLogger.Debug("The logger named '{0}' is disabled", namePattern);
                return null;
            }

            var rule = new LoggingRule(ruleName);

            rule.LoggerNamePattern = namePattern;
            if (writeTargets != null)
            {
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
                        throw new NLogConfigurationException($"Target '{targetName}' not found for logging rule: {(string.IsNullOrEmpty(ruleName) ? namePattern : ruleName)}.");
                    }
                }
            }

            rule.Final = final;

            ParseLevels(loggerElement, rule);

            foreach (var child in loggerElement.Children)
            {
                switch (child.Name.ToUpperInvariant())
                {
                    case "FILTERS":
                        ParseFilters(rule, child);
                        break;

                    case "LOGGER":
                    case "RULE":
                        var childRule = ParseRuleElement(child, null);
                        if (childRule != null)
                        {
                            lock (rule.ChildRules)
                            {
                                rule.ChildRules.Add(childRule);
                            }
                        }
                        break;
                }
            }

            return rule;
        }

        private static void ParseLevels(ILoggingConfigurationSection loggerElement, LoggingRule rule)
        {
            int minLevel = 0;
            int maxLevel = LogLevel.MaxLevel.Ordinal;

            foreach (var configItem in loggerElement.Values)
            {
                switch (configItem.Key.ToUpperInvariant())
                {
                    case "LEVEL":
                        {
                            LogLevel level = LogLevel.FromString(configItem.Value);
                            rule.EnableLoggingForLevel(level);
                            return;
                        }
                    case "LEVELS":
                        {
                            var levelString = CleanSpaces(configItem.Value);

                            string[] tokens = levelString.Split(',');
                            foreach (string token in tokens)
                            {
                                if (!string.IsNullOrEmpty(token))
                                {
                                    LogLevel level = LogLevel.FromString(token);
                                    rule.EnableLoggingForLevel(level);
                                }
                            }
                            return;
                        }
                    case "MINLEVEL": minLevel = LogLevel.FromString(configItem.Value).Ordinal; break;
                    case "MAXLEVEL": maxLevel = LogLevel.FromString(configItem.Value).Ordinal; break;
                }
            }

            for (int i = minLevel; i <= maxLevel; ++i)
            {
                rule.EnableLoggingForLevel(LogLevel.FromOrdinal(i));
            }
        }

        private void ParseFilters(LoggingRule rule, ILoggingConfigurationSection filtersElement)
        {
            filtersElement.AssertName("filters");

            foreach (var filterElement in filtersElement.Children)
            {
                string name = filterElement.Name;

                Filter filter = _configurationItemFactory.Filters.CreateInstance(name);
                ConfigureObjectFromAttributes(filter, filterElement, false);
                rule.Filters.Add(filter);
            }
        }

        private void ParseTargetsElement(ILoggingConfigurationSection targetsElement)
        {
            targetsElement.AssertName("targets", "appenders");

            string asyncValue = targetsElement.Values.Where(configItem => configItem.Key.ToUpper() == "ASYNC").Select(configItem => configItem.Value).FirstOrDefault();
            bool asyncWrap = string.IsNullOrEmpty(asyncValue) ? false : ParseBooleanValue(asyncValue);
            ILoggingConfigurationSection defaultWrapperElement = null;
            var typeNameToDefaultTargetParameters = new Dictionary<string, ILoggingConfigurationSection>();

            var children = targetsElement.Children.ToList();
            foreach (var targetElement in children)
            {
                string name = targetElement.Name;
                string typeAttributeVal = StripOptionalNamespacePrefix(targetElement.GetOptionalValue("type", null));
                Target newTarget = null;

                switch (name.ToUpperInvariant())
                {
                    case "DEFAULT-WRAPPER":
                        defaultWrapperElement = targetElement;
                        break;

                    case "DEFAULT-TARGET-PARAMETERS":
                        if (typeAttributeVal == null)
                        {
                            throw new NLogConfigurationException($"Missing 'type' attribute on <{name}/>.");
                        }

                        typeNameToDefaultTargetParameters[typeAttributeVal] = targetElement;
                        break;

                    case "TARGET":
                    case "APPENDER":
                    case "WRAPPER":
                    case "WRAPPER-TARGET":
                    case "COMPOUND-TARGET":
                        if (typeAttributeVal == null)
                        {
                            throw new NLogConfigurationException("Missing 'type' attribute on <" + name + "/>.");
                        }

                        newTarget = _configurationItemFactory.Targets.CreateInstance(typeAttributeVal);
                        ParseTargetElement(newTarget, targetElement, typeNameToDefaultTargetParameters);
                        break;
                    default:
                        if (typeAttributeVal != null && !_xmlConfigMode)
                        {
                            // Json mode
                            newTarget = _configurationItemFactory.Targets.CreateInstance(typeAttributeVal);
                            newTarget.Name = targetElement.Name;
                            ParseTargetElement(newTarget, targetElement, typeNameToDefaultTargetParameters);
                        }
                        else
                        {
                            InternalLogger.Warn("Skipping unknown 'Targets' child node: {0}", targetElement.Name);
                        }
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

                    InternalLogger.Info("Adding target {0}", newTarget);
                    AddTarget(newTarget.Name, newTarget);
                }
            }
        }

        private void ParseTargetElement(Target target, ILoggingConfigurationSection targetElement, Dictionary<string, ILoggingConfigurationSection> typeNameToDefaultTargetParameters = null)
        {
            string targetType = StripOptionalNamespacePrefix(targetElement.GetRequiredValue("type"));
            ILoggingConfigurationSection defaults;
            if (typeNameToDefaultTargetParameters != null && typeNameToDefaultTargetParameters.TryGetValue(targetType, out defaults))
            {
                ParseTargetElement(target, defaults, null);
            }

            var compound = target as CompoundTargetBase;
            var wrapper = target as WrapperTargetBase;

            ConfigureObjectFromAttributes(target, targetElement, true);

            foreach (var childElement in targetElement.Children)
            {
                string name = childElement.Name;

                if (compound != null && ParseCompoundTarget(typeNameToDefaultTargetParameters, name, childElement, compound, null))
                {
                    continue;
                }

                if (wrapper != null && ParseTargetWrapper(typeNameToDefaultTargetParameters, name, childElement, wrapper))
                {
                    continue;
                }

                SetPropertyFromElement(target, childElement);
            }
        }

        private bool ParseTargetWrapper(Dictionary<string, ILoggingConfigurationSection> typeNameToDefaultTargetParameters, string name, ILoggingConfigurationSection childElement,
    WrapperTargetBase wrapper)
        {
            string targetName = null;
            if (!childElement.Values.Any() && childElement.Children.Count() == 1)
            {
                childElement = childElement.Children.First();
                targetName = childElement.Name;
            }

            if (IsTargetRefElement(name))
            {
                if (targetName == null)
                    targetName = childElement.GetRequiredValue("name");
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
                string type = StripOptionalNamespacePrefix(childElement.GetRequiredValue("type"));

                Target newTarget = _configurationItemFactory.Targets.CreateInstance(type);
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

        private bool ParseCompoundTarget(Dictionary<string, ILoggingConfigurationSection> typeNameToDefaultTargetParameters, string name, ILoggingConfigurationSection childElement,
            CompoundTargetBase compound, string targetName)
        {
            if (!_xmlConfigMode && string.Equals(name, "targets", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var child in childElement.Children)
                {
                    targetName = string.Equals(child.Name, "target", StringComparison.OrdinalIgnoreCase) ? null : child.Name;
                    ParseCompoundTarget(typeNameToDefaultTargetParameters, "target", child, compound, targetName);
                }
                return true;
            }

            if (IsTargetRefElement(name))
            {
                targetName = childElement.GetRequiredValue("name");
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
                string type = StripOptionalNamespacePrefix(childElement.GetRequiredValue("type"));

                Target newTarget = _configurationItemFactory.Targets.CreateInstance(type);
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

        private void ConfigureObjectFromAttributes(object targetObject, ILoggingConfigurationSection element, bool ignoreType)
        {
            foreach (var kvp in element.Values)
            {
                string childName = kvp.Key;
                string childValue = kvp.Value;

                if (ignoreType && childName.Equals("type", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                try
                {
                    PropertyHelper.SetPropertyFromString(targetObject, childName, ExpandSimpleVariables(childValue), _configurationItemFactory);
                }
                catch (NLogConfigurationException)
                {
                    InternalLogger.Warn("Error when setting '{0}' on attibute '{1}'", childValue, childName);
                    throw;
                }
            }
        }


        private void SetPropertyFromElement(object o, ILoggingConfigurationSection element)
        {
            PropertyInfo propInfo;
            if (!PropertyHelper.TryGetPropertyInfo(o, element.Name, out propInfo))
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

            if (SetItemFromElement(o, propInfo, element))
            {
                return;
            }
        }

        private bool AddArrayItemFromElement(object o, PropertyInfo propInfo, ILoggingConfigurationSection element)
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

        private object ParseArrayItemFromElement(Type elementType, ILoggingConfigurationSection element)
        {
            object arrayItem = TryCreateLayoutInstance(element, elementType);
            // arrayItem is not a layout
            if (arrayItem == null)
                arrayItem = FactoryHelper.CreateInstance(elementType);

            ConfigureObjectFromAttributes(arrayItem, element, true);
            ConfigureObjectFromElement(arrayItem, element);
            return arrayItem;
        }

        private bool SetLayoutFromElement(object o, PropertyInfo propInfo, ILoggingConfigurationSection layoutElement)
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

        private Layout TryCreateLayoutInstance(ILoggingConfigurationSection element, Type type)
        {
            // Check if it is a Layout
            if (!typeof(Layout).IsAssignableFrom(type))
                return null;

            string layoutTypeName = StripOptionalNamespacePrefix(element.GetOptionalValue("type", null));

            // Check if the 'type' attribute has been specified
            if (layoutTypeName == null)
                return null;

            return _configurationItemFactory.Layouts.CreateInstance(ExpandSimpleVariables(layoutTypeName));
        }

        private bool SetItemFromElement(object o, PropertyInfo propInfo, ILoggingConfigurationSection element)
        {
            object item = propInfo.GetValue(o, null);
            ConfigureObjectFromAttributes(item, element, true);
            ConfigureObjectFromElement(item, element);
            return true;
        }

        private void ConfigureObjectFromElement(object targetObject, ILoggingConfigurationSection element)
        {
            foreach (var child in element.Children)
            {
                SetPropertyFromElement(targetObject, child);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Target is disposed elsewhere.")]
        private static Target WrapWithAsyncTargetWrapper(Target target)
        {
            var asyncTargetWrapper = new AsyncTargetWrapper();
            asyncTargetWrapper.WrappedTarget = target;
            asyncTargetWrapper.Name = target.Name;
            target.Name = target.Name + "_wrapped";
            InternalLogger.Debug("Wrapping target '{0}' with AsyncTargetWrapper and renaming to '{1}", asyncTargetWrapper.Name, target.Name);
            target = asyncTargetWrapper;
            return target;
        }

        private Target WrapWithDefaultWrapper(Target t, ILoggingConfigurationSection defaultParameters)
        {
            string wrapperType = StripOptionalNamespacePrefix(defaultParameters.GetRequiredValue("type"));
            Target wrapperTargetInstance = _configurationItemFactory.Targets.CreateInstance(wrapperType);
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
                    throw new NLogConfigurationException("Child target type specified on <default-wrapper /> is not a wrapper.");
                }
            }

            wtb.WrappedTarget = t;
            wrapperTargetInstance.Name = t.Name;
            t.Name = t.Name + "_wrapped";

            InternalLogger.Debug("Wrapping target '{0}' with '{1}' and renaming to '{2}", wrapperTargetInstance.Name, wrapperTargetInstance.GetType().Name, t.Name);
            return wrapperTargetInstance;
        }

        private bool MatchesName(string key, string expectedKey)
        {
            return string.Equals(key?.Trim(), expectedKey, StringComparison.OrdinalIgnoreCase);
        }

        private bool ParseBooleanValue(string value)
        {
            return Convert.ToBoolean(value?.Trim(), CultureInfo.InvariantCulture);
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
    }
}
