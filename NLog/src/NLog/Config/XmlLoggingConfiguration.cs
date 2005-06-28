// 
// Copyright (c) 2004,2005 Jaroslaw Kowalski <jkowalski@users.sourceforge.net>
// 
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
// * Neither the name of the Jaroslaw Kowalski nor the names of its 
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

namespace NLog.Config
{
    /// <summary>
    /// A class for configuring NLog through an XML configuration file 
    /// (App.config style or App.nlog style)
    /// </summary>
    public class XmlLoggingConfiguration: LoggingConfiguration
    {
        private StringDictionary _visitedFile = new StringDictionary();
        private NameValueCollection _variables = new NameValueCollection(CaseInsensitiveHashCodeProvider.DefaultInvariant, CaseInsensitiveComparer.DefaultInvariant);

        private bool _autoReload = false;
        private string _originalFileName = null;

        /// <summary>
        /// Gets or sets the value indicating whether the configuration files
        /// should be watched for changes and reloaded automatically when changed.
        /// </summary>
        public bool AutoReload
        {
            get
            {
                return _autoReload;
            }
            set
            {
                _autoReload = value;
            }
        }

        /// <summary>
        /// Constructs a new instance of <see cref="XmlLoggingConfiguration" />
        /// class and reads the configuration from the specified config file.
        /// </summary>
        /// <param name="fileName">Configuration file to be read.</param>
        public XmlLoggingConfiguration(string fileName)
        {
            InternalLogger.Info("Configuring from {0}...", fileName);
            _originalFileName = fileName;
            ConfigureFromFile(fileName);
        }

        /// <summary>
        /// Constructs a new instance of <see cref="XmlLoggingConfiguration" />
        /// class and reads the configuration from the specified XML element.
        /// </summary>
        /// <param name="configElement"><see cref="XmlElement" /> containing the configuration section.</param>
        /// <param name="fileName">Name of the file that contains the element (to be used as a base for including other files).</param>
        public XmlLoggingConfiguration(XmlElement configElement, string fileName)
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
            if (0 == String.Compare(doc.DocumentElement.LocalName, "configuration", true))
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

        private string GetCaseInsensitiveAttribute(XmlElement element, string name)
        {
            // first try a case-sensitive match
            string s = element.GetAttribute(name);
            if (s != null && s != "")
                return PropertyHelper.ExpandVariables(s, _variables);

            // then look through all attributes and do a case-insensitive compare
            // this isn't very fast, but we don't need ultra speed here

            foreach (XmlAttribute a in element.Attributes)
            {
                if (0 == String.Compare(a.LocalName, name, true))
                    return PropertyHelper.ExpandVariables(a.Value, _variables);
            }

            return null;
        }

        private static bool HasCaseInsensitiveAttribute(XmlElement element, string name)
        {
            // first try a case-sensitive match
            if (element.HasAttribute(name))
                return true;

            // then look through all attributes and do a case-insensitive compare
            // this isn't very fast, but we don't need ultra speed here because usually we have about
            // 3 attributes per element

            foreach (XmlAttribute a in element.Attributes)
            {
                if (0 == String.Compare(a.LocalName, name, true))
                    return true;
            }

            return false;
        }

        private void IncludeFileFromElement(XmlElement includeElement, string baseDirectory)
        {
            Layout layout = new Layout(GetCaseInsensitiveAttribute(includeElement, "file"));

            string newFileName = layout.GetFormattedMessage(LogEventInfo.Empty);
            newFileName = Path.Combine(baseDirectory, newFileName);
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

            string s = GetCaseInsensitiveAttribute(configElement, "internalLogFile");
            if (s != null)
                    InternalLogger.LogFile = s;

            s = GetCaseInsensitiveAttribute(configElement, "internalLogLevel");
            if (s != null)
                InternalLogger.LogLevel = LogLevel.FromString(s);

            RegisterPlatformSpecificExtensions();

            foreach (XmlNode node in configElement.ChildNodes)
            {
                XmlElement el = node as XmlElement;
                if (el == null)
                    continue;

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
                object o = System.Configuration.ConfigurationSettings.GetConfig("nlog");
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

            foreach (XmlNode n1 in element.ChildNodes)
            {
                XmlElement el = n1 as XmlElement;
                if (el == null)
                    continue;

                if (0 != String.Compare(el.LocalName, "logger", true))
                    continue;
                
                XmlElement ruleElement = el;

                LoggingRule rule = new LoggingRule();
                string namePattern = GetCaseInsensitiveAttribute(ruleElement, "name");
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
                            throw new Exception("Target " + targetName + " not found.");
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

                foreach (XmlNode n in ruleElement.ChildNodes)
                {
                    if (n is XmlElement)
                    {
                        el = (XmlElement)n;

                        if (0 == String.Compare(el.LocalName, "filters", true))
                        {
                            ConfigureRuleFiltersFromXmlElement(rule, el);
                        }
                    }
                }

                ConfigureRulesFromElement(config, rule.ChildRules, ruleElement);

                rules.Add(rule);
            }
        }

        private void AddExtensionsFromElement(XmlElement element, string baseDirectory)
        {
            if (element == null)
                return ;

            foreach (XmlNode node in element.ChildNodes)
            {
                XmlElement targetElement = node as XmlElement;
                if (targetElement == null)
                    continue;

                if (0 == String.Compare(targetElement.LocalName, "add", true))
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
                            LoadExtensionsFromAssembly(asm, prefix);
                        }
                        catch (Exception ex)
                        {
                            InternalLogger.Error("Error loading extensions: {0}", ex);
                            if (LogManager.ThrowExceptions)
                                throw;
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
                            LoadExtensionsFromAssembly(asm, prefix);
                        }
                        catch (Exception ex)
                        {
                            InternalLogger.Error("Error loading extensions: {0}", ex);
                            if (LogManager.ThrowExceptions)
                                throw;
                        }
                        continue;
                    };
                }

            }
        }
        private static void LoadExtensionsFromAssembly(Assembly asm, string prefix)
        {
            FilterFactory.AddFiltersFromAssembly(asm, prefix);
            LayoutRendererFactory.AddLayoutRenderersFromAssembly(asm, prefix);
            TargetFactory.AddTargetsFromAssembly(asm, prefix);
        }

        private void ConfigureTargetsFromElement(XmlElement element)
        {
            if (element == null)
                return ;

            foreach (XmlNode n in element.ChildNodes)
            {
                XmlElement targetElement = n as XmlElement;
                
                if (targetElement == null)
                    continue;
                
                if (0 == String.Compare(targetElement.LocalName, "target", true) || 
                    0 == String.Compare(targetElement.LocalName, "appender", true))
                {
                    string type = GetCaseInsensitiveAttribute(targetElement, "type");
                    Target newTarget = TargetFactory.CreateTarget(type);
                    if (newTarget != null)
                    {
                        ConfigureTargetFromXmlElement(newTarget, targetElement);
                        AddTarget(newTarget.Name, newTarget);
                    }
                }
            }
        }

        private void ConfigureRuleFiltersFromXmlElement(LoggingRule rule, XmlElement element)
        {
            if (element == null)
                return ;

            foreach (XmlNode node in element.ChildNodes)
            {
                if (node is XmlElement)
                {
                    string name = node.LocalName;

                    Filter filter = FilterFactory.CreateFilter(name);

                    foreach (XmlAttribute attrib in((XmlElement)node).Attributes)
                    {
                        string attribName = attrib.LocalName;
                        string attribValue = attrib.InnerText;

                        PropertyHelper.SetPropertyFromString(filter, attribName, attribValue, _variables);
                    }

                    rule.Filters.Add(filter);
                }
            }
        }

        private void ConfigureTargetFromXmlElement(Target target, XmlElement element)
        {
            Type targetType = target.GetType();

            foreach (XmlAttribute attrib in element.Attributes)
            {
                string name = attrib.LocalName;
                string value = attrib.InnerText;

                if (0 == String.Compare(name, "type", true))
                    continue;

                PropertyHelper.SetPropertyFromString(target, name, value, _variables);
            }

            foreach (XmlNode node in element.ChildNodes)
            {
                if (node is XmlElement)
                {
                    XmlElement el = (XmlElement)node;
                    string name = el.LocalName;

                    if (PropertyHelper.IsArrayProperty(targetType, name))
                    {
                        PropertyHelper.AddArrayItemFromElement(target, el, _variables);
                    }
                    else
                    {
                        string value = el.InnerXml;
                        PropertyHelper.SetPropertyFromString(target, name, value, _variables);
                    }
                }
            }
        }

        private static bool _platformSpecificExtensionsRegistered = false;

        private void RegisterPlatformSpecificExtensions()
        {
            if (_platformSpecificExtensionsRegistered)
                return ;
            _platformSpecificExtensionsRegistered = true;

            InternalLogger.Info("Registering platform specific extensions...");
#if NETCF
            RegisterPlatformSpecificExtensions("NLog.CompactFramework");
#else 
            if (Type.GetType("System.MonoType", false) != null)
            {
                RegisterPlatformSpecificExtensions("NLog.Mono");
            }
            else
            {
                RegisterPlatformSpecificExtensions("NLog.DotNet");
            }

            PlatformID platform = System.Environment.OSVersion.Platform;

            if (platform == PlatformID.Win32NT || platform == PlatformID.Win32Windows)
            {
                RegisterPlatformSpecificExtensions("NLog.Win32");
            }

            if ((int)platform == 128 || (int)platform == 4)
            {
                // mono-1.0 used '128' here, net-2.0 and mono-2.0 use '4'
                RegisterPlatformSpecificExtensions("NLog.Unix");
            }
#endif 
        }

        private void RegisterPlatformSpecificExtensions(string name)
        {
            InternalLogger.Debug("RegisterPlatformSpecificExtensions('{0}')", name);
            AssemblyName nlogAssemblyName = typeof(LogManager).Assembly.GetName();
            AssemblyName newAssemblyName = new AssemblyName();
            newAssemblyName.Name = name;
            newAssemblyName.CultureInfo = nlogAssemblyName.CultureInfo;
            newAssemblyName.Flags = nlogAssemblyName.Flags;
            newAssemblyName.SetPublicKey(nlogAssemblyName.GetPublicKey());
            newAssemblyName.Version = nlogAssemblyName.Version;

            try
            {
                InternalLogger.Info("Registering platform specific extensions from assembly '{0}'", newAssemblyName);
                Assembly asm = Assembly.Load(newAssemblyName);
                InternalLogger.Info("Loaded {0}", asm.FullName);
                LoadExtensionsFromAssembly(asm, String.Empty);
                InternalLogger.Info("Registered platform specific extensions from assembly '{0}'.", newAssemblyName);
            }
            catch (Exception ex)
            {
                InternalLogger.Error("Could not load platform specific extensions: {0}", ex);
                //if (LogManager.ThrowExceptions)
                //    throw;
            }
        }
    }
}
