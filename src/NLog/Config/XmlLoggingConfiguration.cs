// 
// Copyright (c) 2004-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
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
using System.Reflection;
using System.Xml;
using System.IO;
using System.Globalization;
using System.Collections.Specialized;

using NLog;
using NLog.Targets;
using NLog.Filters;
using NLog.LayoutRenderers;
using NLog.Internal;
using NLog.Targets.Wrappers;

namespace NLog.Config
{
    /// <summary>
    /// A class for configuring NLog through an XML configuration file 
    /// (App.config style or App.nlog style)
    /// </summary>
    public class XmlLoggingConfiguration: LoggingConfiguration
    {
        private StringDictionary _visitedFile = new StringDictionary();
#if NET_2_API
        private NameValueCollection _variables = new NameValueCollection(StringComparer.InvariantCultureIgnoreCase);
#else
        private NameValueCollection _variables = new NameValueCollection(CaseInsensitiveHashCodeProvider.Default, CaseInsensitiveComparer.Default);
#endif

        private bool _autoReload = false;
        private string _originalFileName = null;

        /// <summary>
        /// Gets or sets the value indicating whether the configuration files
        /// should be watched for changes and reloaded automatically when changed.
        /// </summary>
        public bool AutoReload
        {
            get { return _autoReload; }
            set { _autoReload = value; }
        }

        /// <summary>
        /// Constructs a new instance of <see cref="XmlLoggingConfiguration" />
        /// class and reads the configuration from the specified config file.
        /// </summary>
        /// <param name="fileName">Configuration file to be read.</param>
        public XmlLoggingConfiguration(string fileName) : this(fileName, false)
        {
        }

        /// <summary>
        /// Constructs a new instance of <see cref="XmlLoggingConfiguration" />
        /// class and reads the configuration from the specified config file.
        /// </summary>
        /// <param name="fileName">Configuration file to be read.</param>
        /// <param name="ignoreErrors">Ignore any errors during configuration.</param>
        public XmlLoggingConfiguration(string fileName, bool ignoreErrors)
        {
            InternalLogger.Info("Configuring from {0}...", fileName);
            _originalFileName = fileName;
            try
            {
                ConfigureFromFile(fileName);
            }
            catch (Exception ex)
            {
                InternalLogger.Error("Error {0}...", ex);
                if (!ignoreErrors)
                    throw new NLogConfigurationException("Exception occured when loading configuration from '" + fileName + "'", ex);
            }
        }

        /// <summary>
        /// Constructs a new instance of <see cref="XmlLoggingConfiguration" />
        /// class and reads the configuration from the specified XML element.
        /// </summary>
        /// <param name="configElement"><see cref="XmlElement" /> containing the configuration section.</param>
        /// <param name="fileName">Name of the file that contains the element (to be used as a base for including other files).</param>
        public XmlLoggingConfiguration(XmlElement configElement, string fileName) : this(configElement, fileName, false)
        {
        }

        /// <summary>
        /// Constructs a new instance of <see cref="XmlLoggingConfiguration" />
        /// class and reads the configuration from the specified XML element.
        /// </summary>
        /// <param name="configElement"><see cref="XmlElement" /> containing the configuration section.</param>
        /// <param name="fileName">Name of the file that contains the element (to be used as a base for including other files).</param>
        /// <param name="ignoreErrors">Ignore any errors during configuration.</param>
        public XmlLoggingConfiguration(XmlElement configElement, string fileName, bool ignoreErrors)
        {
            try
            {
                if (fileName != null)
                {
                    InternalLogger.Info("Configuring from an XML element in {0}...", fileName);
                    string key = Path.GetFullPath(fileName).ToLower(CultureInfo.InvariantCulture);
                    _visitedFile[key] = key;

                    _originalFileName = fileName;
                    ConfigureFromXmlElement(configElement, Path.GetDirectoryName(fileName));
                }
                else
                {
                    ConfigureFromXmlElement(configElement, null);
                }
            }
            catch (Exception ex)
            {
                InternalLogger.Error("Error {0}...", ex);
                if (!ignoreErrors)
                    throw new NLogConfigurationException("Exception occured when loading configuration from XML Element in " + fileName, ex);
            }
        }

        /// <summary>
        /// Gets the collection of file names which should be watched for changes by NLog.
        /// This is the list of configuration files processed.
        /// If the <c>autoReload</c> attribute is not set it returns null.
        /// </summary>
        public override ICollection FileNamesToWatch
        {
            get
            {
                if (_autoReload)
                    return _visitedFile.Keys;
                else
                    return null;
            }
        }

        /// <summary>
        /// Re-reads the original configuration file and returns the new <see cref="LoggingConfiguration" /> object.
        /// </summary>
        /// <returns>The new <see cref="XmlLoggingConfiguration" /> object.</returns>
        public override LoggingConfiguration Reload()
        {
            return new XmlLoggingConfiguration(_originalFileName);
        }

        private void ConfigureFromFile(string fileName)
        {
            string key = Path.GetFullPath(fileName).ToLower(CultureInfo.InvariantCulture);
            if (_visitedFile.ContainsKey(key))
                return ;

            _visitedFile[key] = key;

            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);
            if (EqualsCI(doc.DocumentElement.LocalName,"configuration"))
            {
                foreach (XmlElement el in doc.DocumentElement.GetElementsByTagName("nlog"))
                {
                    ConfigureFromXmlElement(el, Path.GetDirectoryName(fileName));
                }
            }
            else
            {
                ConfigureFromXmlElement(doc.DocumentElement, Path.GetDirectoryName(fileName));
            }
        }

        private static bool EqualsCI(string p1, string p2)
        {
            return PropertyHelper.EqualsCI(p1, p2);
        }

        private string GetCaseInsensitiveAttribute(XmlElement element, string name)
        {
            return PropertyHelper.GetCaseInsensitiveAttribute(element, name, _variables);
        }

        private static bool HasCaseInsensitiveAttribute(XmlElement element, string name)
        {
            return PropertyHelper.HasCaseInsensitiveAttribute(element, name);
        }

        private void IncludeFileFromElement(XmlElement includeElement, string baseDirectory)
        {
            string newFileName = Layout.Evaluate(GetCaseInsensitiveAttribute(includeElement, "file"));
            newFileName = Path.Combine(baseDirectory, newFileName);

            try
            {
                if (File.Exists(newFileName))
                {
                    InternalLogger.Debug("Including file '{0}'", newFileName);
                    ConfigureFromFile(newFileName);
                }
                else
                {
                    throw new FileNotFoundException("Included file not found: " + newFileName);
                }
            }
            catch (Exception ex)
            {
                InternalLogger.Error("Error when including '{0}' {1}", newFileName, ex);

                if (EqualsCI(GetCaseInsensitiveAttribute(includeElement, "ignoreErrors"), "true"))
                    return;
                throw new NLogConfigurationException("Error when including: " + newFileName, ex);
            }
        }

        private void ConfigureFromXmlElement(XmlElement configElement, string baseDirectory)
        {
            switch (GetCaseInsensitiveAttribute(configElement, "autoReload"))
            {
                case "true":
                    AutoReload = true;
                    break;

                case "false":
                    AutoReload = false;
                    break;
            }

            switch (GetCaseInsensitiveAttribute(configElement, "throwExceptions"))
            {
                case "true":
                    LogManager.ThrowExceptions = true;
                    break;

                case "false":
                    LogManager.ThrowExceptions = false;
                    break;
            }

            switch (GetCaseInsensitiveAttribute(configElement, "internalLogToConsole"))
            {
                case "true":
                    InternalLogger.LogToConsole = true;
                    break;

                case "false":
                    InternalLogger.LogToConsole = false;
                    break;
            }

#if !NETCF
            switch (GetCaseInsensitiveAttribute(configElement, "internalLogToConsoleError"))
            {
                case "true":
                    InternalLogger.LogToConsoleError = true;
                    break;

                case "false":
                    InternalLogger.LogToConsoleError = false;
                    break;
            }
#endif

            string s = GetCaseInsensitiveAttribute(configElement, "internalLogFile");
            if (s != null)
                InternalLogger.LogFile = s;

            s = GetCaseInsensitiveAttribute(configElement, "internalLogLevel");
            if (s != null)
                InternalLogger.LogLevel = LogLevel.FromString(s);

            s = GetCaseInsensitiveAttribute(configElement, "globalThreshold");
            if (s != null)
                LogManager.GlobalThreshold = LogLevel.FromString(s);

            foreach (XmlElement el in PropertyHelper.GetChildElements(configElement))
            {
                switch (el.LocalName.ToLower())
                {
                    case "extensions":
                        AddExtensionsFromElement(el, baseDirectory);
                        break;

                    case "include":
                        IncludeFileFromElement(el, baseDirectory);
                        break;

                    case "appenders":
                    case "targets":
                        ConfigureTargetsFromElement(el);
                        break;

                    case "variable":
                        SetVariable(el);
                        break;

                    case "rules":
                        ConfigureRulesFromElement(this, LoggingRules, el);
                        break;
                }
            }
        }

        private void SetVariable(XmlElement el)
        {
            string name = GetCaseInsensitiveAttribute(el, "name");
            string value = GetCaseInsensitiveAttribute(el, "value");

            _variables[name] = value;
        }

#if !NETCF
        /// <summary>
        /// Gets the default <see cref="LoggingConfiguration" /> object by parsing 
        /// the application configuration file (<c>app.exe.config</c>).
        /// </summary>
        public static LoggingConfiguration AppConfig
        {
            get
            {
#if NET_2_API
                object o = System.Configuration.ConfigurationManager.GetSection("nlog");
#else
                object o = System.Configuration.ConfigurationSettings.GetConfig("nlog");
#endif
                return o as LoggingConfiguration;
            }
        }
#endif

        // implementation details

        private static string CleanWhitespace(string s)
        {
            s = s.Replace(" ", ""); // get rid of the whitespace
            return s;
        }

        private void ConfigureRulesFromElement(LoggingConfiguration config, LoggingRuleCollection rules, XmlElement element)
        {
            if (element == null)
                return ;

            foreach (XmlElement el in PropertyHelper.GetChildElements(element, "logger"))
            {
                XmlElement ruleElement = el;

                LoggingRule rule = new LoggingRule();
                string namePattern = GetCaseInsensitiveAttribute(ruleElement, "name");
                if (namePattern == null)
                    namePattern = "*";

                string appendTo = GetCaseInsensitiveAttribute(ruleElement, "appendTo");
                if (appendTo == null)
                    appendTo = GetCaseInsensitiveAttribute(ruleElement, "writeTo");

                rule.LoggerNamePattern = namePattern;
                if (appendTo != null)
                {
                    foreach (string t in appendTo.Split(','))
                    {
                        string targetName = t.Trim();
                        Target target = config.FindTargetByName(targetName);

                        if (target != null)
                        {
                            rule.Targets.Add(target);
                        }
                        else
                        {
                            throw new NLogConfigurationException("Target " + targetName + " not found.");
                        }
                    }
                }
                rule.Final = false;

                if (HasCaseInsensitiveAttribute(ruleElement, "final"))
                {
                    rule.Final = true;
                }

                if (HasCaseInsensitiveAttribute(ruleElement, "level"))
                {
                    LogLevel level = LogLevel.FromString(GetCaseInsensitiveAttribute(ruleElement, "level"));
                    rule.EnableLoggingForLevel(level);
                }
                else if (HasCaseInsensitiveAttribute(ruleElement, "levels"))
                {
                    string levelsString = GetCaseInsensitiveAttribute(ruleElement, "levels");
                    levelsString = CleanWhitespace(levelsString);

                    string[]tokens = levelsString.Split(',');
                    foreach (string s in tokens)
                    {
                        if (s != "")
                        {
                            LogLevel level = LogLevel.FromString(s);
                            rule.EnableLoggingForLevel(level);
                        }
                    }
                }
                else
                {
                    int minLevel = 0;
                    int maxLevel = LogLevel.MaxLevel.Ordinal;

                    if (HasCaseInsensitiveAttribute(ruleElement, "minlevel"))
                    {
                        minLevel = LogLevel.FromString(GetCaseInsensitiveAttribute(ruleElement, "minlevel")).Ordinal;
                    }

                    if (HasCaseInsensitiveAttribute(ruleElement, "maxlevel"))
                    {
                        maxLevel = LogLevel.FromString(GetCaseInsensitiveAttribute(ruleElement, "maxlevel")).Ordinal;
                    }

                    for (int i = minLevel; i <= maxLevel; ++i)
                    {
                        rule.EnableLoggingForLevel(LogLevel.FromOrdinal(i));
                    }
                }

                foreach (XmlElement el2 in PropertyHelper.GetChildElements(ruleElement,"filters"))
                {
                    ConfigureRuleFiltersFromXmlElement(rule, el2);
                }

                ConfigureRulesFromElement(config, rule.ChildRules, ruleElement);

                rules.Add(rule);
            }
        }

        private void AddExtensionsFromElement(XmlElement element, string baseDirectory)
        {
            if (element == null)
                return ;

            foreach (XmlElement targetElement in PropertyHelper.GetChildElements(element))
            {
                if (EqualsCI(targetElement.LocalName,"add"))
                {
                    string assemblyFile = GetCaseInsensitiveAttribute(targetElement, "assemblyFile");
                    string extPrefix = GetCaseInsensitiveAttribute(targetElement, "prefix");
                    string prefix;
                    if (extPrefix != null && extPrefix.Length != 0)
                    {
                        prefix = extPrefix + ".";
                    }
                    else
                    {
                        prefix = String.Empty;
                    }

                    if (assemblyFile != null && assemblyFile.Length > 0)
                    {
                        try
                        {
                            string fullFileName = Path.Combine(baseDirectory, assemblyFile);
                            InternalLogger.Info("Loading assemblyFile: {0}", fullFileName);
                            Assembly asm = Assembly.LoadFrom(fullFileName);

                            TargetFactory.AddTargetsFromAssembly(asm, prefix);
                            LayoutRendererFactory.AddLayoutRenderersFromAssembly(asm, prefix);
                            FilterFactory.AddFiltersFromAssembly(asm, prefix);
                            LayoutFactory.AddLayoutsFromAssembly(asm, prefix);
                            ConditionMethodFactory.AddConditionMethodsFromAssembly(asm, prefix);
                        }
                        catch (Exception ex)
                        {
                            InternalLogger.Error("Error loading extensions: {0}", ex);
                            if (LogManager.ThrowExceptions)
                                throw new NLogConfigurationException("Error loading extensions: " + assemblyFile, ex);
                        }
                        continue;
                    };

                    string assemblyName = GetCaseInsensitiveAttribute(targetElement, "assembly");

                    if (assemblyName != null && assemblyName.Length > 0)
                    {
                        try
                        {
                            InternalLogger.Info("Loading assemblyName: {0}", assemblyName);
                            Assembly asm = Assembly.Load(assemblyName);

                            TargetFactory.AddTargetsFromAssembly(asm, prefix);
                            LayoutRendererFactory.AddLayoutRenderersFromAssembly(asm, prefix);
                            FilterFactory.AddFiltersFromAssembly(asm, prefix);
                            LayoutFactory.AddLayoutsFromAssembly(asm, prefix);
                            ConditionMethodFactory.AddConditionMethodsFromAssembly(asm, prefix);
                        }
                        catch (Exception ex)
                        {
                            InternalLogger.Error("Error loading extensions: {0}", ex);
                            if (LogManager.ThrowExceptions)
                                throw new NLogConfigurationException("Error loading extensions: " + assemblyName, ex);
                        }
                        continue;
                    };
                }

            }
        }

        private Target WrapWithAsyncTarget(Target t)
        {
            NLog.Targets.Wrappers.AsyncTargetWrapper atw = new NLog.Targets.Wrappers.AsyncTargetWrapper();
            atw.WrappedTarget = t;
            atw.Name = t.Name;
            t.Name = t.Name + "_wrapped";
            InternalLogger.Debug("Wrapping target '{0}' with AsyncTargetWrapper and renaming to '{1}", atw.Name, t.Name);
            return atw;
        }

        private Target WrapWithDefaultWrapper(Target t, XmlElement defaultWrapperElement)
        {
            string wrapperType = GetCaseInsensitiveAttribute(defaultWrapperElement, "type");
            Target wrapperTargetInstance = TargetFactory.CreateTarget(wrapperType);
            WrapperTargetBase wtb = wrapperTargetInstance as WrapperTargetBase;
            if (wtb == null)
                throw new NLogConfigurationException("Target type specified on <default-wrapper /> is not a wrapper.");
            ConfigureTargetFromXmlElement(wrapperTargetInstance, defaultWrapperElement);
            while (wtb.WrappedTarget != null)
            {
                wtb = wtb.WrappedTarget as WrapperTargetBase;
                if (wtb == null)
                    throw new NLogConfigurationException("Child target type specified on <default-wrapper /> is not a wrapper.");
            }
            wtb.WrappedTarget = t;
            wrapperTargetInstance.Name = t.Name;
            t.Name = t.Name + "_wrapped";

            InternalLogger.Debug("Wrapping target '{0}' with '{1}' and renaming to '{2}", wrapperTargetInstance.Name, wrapperTargetInstance.GetType().Name, t.Name);
            return wrapperTargetInstance;
        }

        private void ConfigureTargetsFromElement(XmlElement element)
        {
            if (element == null)
                return ;

            bool asyncWrap = EqualsCI(GetCaseInsensitiveAttribute(element, "async"),"true");
            XmlElement defaultWrapperElement = null;
            Hashtable typeNameToDefaultTargetParametersElement = new Hashtable();

            foreach (XmlElement targetElement in PropertyHelper.GetChildElements(element))
            {
                string name = targetElement.LocalName.ToLower();
                string type = GetCaseInsensitiveAttribute(targetElement, "type");

                switch (name)
                {
                    case "default-wrapper":
                        defaultWrapperElement = targetElement;
                        break;

                    case "default-target-parameters":
                        typeNameToDefaultTargetParametersElement[type] = targetElement;
                        break;

                    case "target":
                    case "appender":
                    case "wrapper":
                    case "wrapper-target":
                    case "compound-target":
                        Target newTarget = TargetFactory.CreateTarget(type);

                        XmlElement defaultParametersElement = typeNameToDefaultTargetParametersElement[type] as XmlElement;
                        if (defaultParametersElement != null)
                            ConfigureTargetFromXmlElement(newTarget, defaultParametersElement);

                        ConfigureTargetFromXmlElement(newTarget, targetElement);

                        if (asyncWrap)
                            newTarget = WrapWithAsyncTarget(newTarget);

                        if (defaultWrapperElement != null)
                            newTarget = WrapWithDefaultWrapper(newTarget, defaultWrapperElement);

                        InternalLogger.Info("Adding target {0}", newTarget);
                        AddTarget(newTarget.Name, newTarget);
                        break;
                }
            }
        }

        private void ConfigureRuleFiltersFromXmlElement(LoggingRule rule, XmlElement element)
        {
            if (element == null)
                return ;

            foreach (XmlElement el in PropertyHelper.GetChildElements(element))
            {
                string name = el.LocalName;

                Filter filter = FilterFactory.CreateFilter(name);
                PropertyHelper.ConfigureObjectFromAttributes(filter, el.Attributes, _variables, false);
                rule.Filters.Add(filter);
            }
        }

        private void ConfigureTargetFromXmlElement(Target target, XmlElement element)
        {
            NLog.Targets.Compound.CompoundTargetBase compound = target as NLog.Targets.Compound.CompoundTargetBase;
            NLog.Targets.Wrappers.WrapperTargetBase wrapper = target as NLog.Targets.Wrappers.WrapperTargetBase;

            PropertyHelper.ConfigureObjectFromAttributes(target, element.Attributes, _variables, true);

            foreach (XmlElement el in PropertyHelper.GetChildElements(element))
            {
                string name = el.LocalName;

                if (compound != null)
                {
                    if ((name == "target" || name == "wrapper" || name == "wrapper-target" || name == "compound-target"))
                    {
                        string type = GetCaseInsensitiveAttribute(el, "type");
                        Target newTarget = TargetFactory.CreateTarget(type);
                        if (newTarget != null)
                        {
                            ConfigureTargetFromXmlElement(newTarget, el);
                            if (newTarget.Name != null)
                            {
                                // if the new target has name, register it
                                AddTarget(newTarget.Name, newTarget);
                            }
                            compound.Targets.Add(newTarget);
                        }
                        continue;
                    }
                }

                if (wrapper != null)
                {
                    if ((name == "target" || name == "wrapper" || name == "wrapper-target" || name == "compound-target"))
                    {
                        string type = GetCaseInsensitiveAttribute(el, "type");
                        Target newTarget = TargetFactory.CreateTarget(type);
                        if (newTarget != null)
                        {
                            ConfigureTargetFromXmlElement(newTarget, el);
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
                        continue;
                    }
                }

                PropertyHelper.SetPropertyFromElement(target, el, _variables);
            }
        }
    }
}
