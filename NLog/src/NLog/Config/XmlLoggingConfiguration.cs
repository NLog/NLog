// 
// Copyright (c) 2004 Jaroslaw Kowalski <jaak@polbox.com>
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
using NLog.Appenders;
using NLog.Filters;
using NLog.LayoutAppenders;
using NLog.Internal;

namespace NLog.Config
{
    public class XmlLoggingConfiguration : LoggingConfiguration
    {
        private StringDictionary _visitedFile = new StringDictionary();

        private bool _autoReload = false;
        private string _originalFileName = null;

        public bool AutoReload
        {
            get { return _autoReload; }
            set { _autoReload = value; }
        }

        public XmlLoggingConfiguration() { }
        public XmlLoggingConfiguration(string fileName) {
            _originalFileName = fileName;
            ConfigureFromFile(fileName);
        }

        public override ICollection FileNamesToWatch
        {
            get {
                if (_autoReload)
                    return _visitedFile.Keys;
                else
                    return null;
            }
        }

        public override LoggingConfiguration Reload()
        {
            return new XmlLoggingConfiguration(_originalFileName);
        }

        private void ConfigureFromFile(string fileName) {
            string key = Path.GetFullPath(fileName).ToLower(CultureInfo.InvariantCulture);
            if (_visitedFile.ContainsKey(key))
                return;

            _visitedFile[key] = key;

            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);
            if (doc.DocumentElement.LocalName == "configuration") {
                foreach (XmlElement el in doc.DocumentElement.GetElementsByTagName("nlog")) {
                    ConfigureFromXmlElement(el, Path.GetDirectoryName(fileName));
                }
            } else {
                ConfigureFromXmlElement(doc.DocumentElement, Path.GetDirectoryName(fileName));
            }
        }

        private void ConfigureFromXmlElement(XmlElement configElement, string baseDirectory) {
            if (configElement.HasAttribute("autoReload")) {
                AutoReload = true;
            }

            foreach (XmlElement el in configElement.GetElementsByTagName("include"))
            {
                Layout layout = new Layout(el.GetAttribute("file"));

                string newFileName = layout.GetFormattedMessage(LogEventInfo.Empty);
                newFileName = Path.Combine(baseDirectory, newFileName);
                if (File.Exists(newFileName)) {
                    ConfigureFromFile(newFileName);
                } else {
                    throw new FileNotFoundException("Included fine not found: " + newFileName);
                }
            }

            foreach (XmlElement el in configElement.GetElementsByTagName("extensions"))
            {
                AddExtensionsFromElement(el, baseDirectory);
            }

            foreach (XmlElement el in configElement.GetElementsByTagName("appenders"))
            {
                ConfigureAppendersFromElement(el);
            }

            foreach (XmlElement el in configElement.GetElementsByTagName("rules"))
            {
                ConfigureRulesFromElement(el);
            }

            ResolveAppenders();
        }

#if !NETCF
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

        private static string CleanWhitespace(string s) {
            s = s.Replace(" ", ""); // get rid of the whitespace
            return s;
        }

        private void ConfigureRulesFromElement(XmlElement element) {
            if (element == null)
                return;
            foreach (XmlElement ruleElement in element.GetElementsByTagName("logger"))
            {
                AppenderRule rule = new AppenderRule();
                string namePattern = ruleElement.GetAttribute("name");
                string appendTo = ruleElement.GetAttribute("appendTo");

                rule.LoggerNamePattern = namePattern;
                foreach (string appenderName in appendTo.Split(','))
                {
                    rule.AppenderNames.Add(appenderName.Trim());
                }
                rule.Final = false;

                if (ruleElement.HasAttribute("final")) {
                    rule.Final = true;
                }

                if (ruleElement.HasAttribute("level")) {
                    LogLevel level = Logger.LogLevelFromString(ruleElement.GetAttribute("level"));
                    rule.EnableLoggingForLevel(level);
                } else if (ruleElement.HasAttribute("levels")) {
                    string levelsString = ruleElement.GetAttribute("levels");
                    levelsString = CleanWhitespace(levelsString);

                    string[] tokens = levelsString.Split(',');
                    foreach (string s in tokens) {
                        LogLevel level = Logger.LogLevelFromString(s);
                        rule.EnableLoggingForLevel(level);
                    }
                } else {
                    int minLevel = 0;
                    int maxLevel = (int)LogLevel.MaxLevel;

                    if (ruleElement.HasAttribute("minlevel")) {
                        minLevel = (int)Logger.LogLevelFromString(ruleElement.GetAttribute("minlevel"));
                    }

                    if (ruleElement.HasAttribute("maxlevel")) {
                        maxLevel = (int)Logger.LogLevelFromString(ruleElement.GetAttribute("maxlevel"));
                    }

                    for (int i = minLevel; i <= maxLevel; ++i) {
                        rule.EnableLoggingForLevel((LogLevel)i);
                    }
                }

                foreach (XmlNode n in ruleElement.ChildNodes)
                {
                    if (n is XmlElement)
                    {
                        XmlElement el = (XmlElement)n;

                        if (el.Name == "filters") {
                            ConfigureRuleFiltersFromXmlElement(rule, el);
                        }
                    }
                }

                AppenderRules.Add(rule);
            }
        }

        private static void AddExtensionsFromElement(XmlElement element, string baseDirectory) {
            if (element == null)
                return;

            foreach (XmlElement appenderElement in element.GetElementsByTagName("add")) {
                string assemblyFile = appenderElement.GetAttribute("assemblyFile");
                string extPrefix = appenderElement.GetAttribute("prefix");
                string prefix;
                if (extPrefix != null && extPrefix.Length != 0) {
                    prefix = extPrefix + ".";
                } else {
                    prefix = String.Empty;
                }

                if (assemblyFile != null && assemblyFile.Length > 0) {
                    try {
                        string fullFileName = Path.Combine(baseDirectory, assemblyFile);
                        InternalLogger.Info("Loading assemblyFile: {0}", fullFileName);
                        Assembly asm = Assembly.LoadFrom(fullFileName);
                        LoadExtensionFromAssembly(asm, prefix);
                    }
                    catch (Exception ex) {
                        InternalLogger.Error("Error loading layout-appenders: {0}", ex);
                    }
                    continue;
                };

#if !NETCF
                string assemblyPartialName = appenderElement.GetAttribute("assemblyPartialName");

                if (assemblyPartialName != null && assemblyPartialName.Length > 0) {
                    try {
                        InternalLogger.Info("Loading assemblyPartialName: {0}", assemblyPartialName);
                        Assembly asm = Assembly.LoadWithPartialName(assemblyPartialName);
                        if (asm != null) 
                        {
                            LoadExtensionFromAssembly(asm, prefix);
                        }
                        else
                        {
                            throw new ApplicationException("Assembly with partial name " + assemblyPartialName + " not found.");
                        }
                    }
                    catch (Exception ex) {
                        InternalLogger.Error("Error loading layout-appenders: {0}", ex);
                    }
                    continue;
                };
#endif

                string assemblyName = appenderElement.GetAttribute("assembly");

                if (assemblyName != null && assemblyName.Length > 0) {
                    try {
                        InternalLogger.Info("Loading assemblyName: {0}", assemblyName);
                        Assembly asm = Assembly.Load(assemblyName);
                        LoadExtensionFromAssembly(asm, prefix);
                    }
                    catch (Exception ex) {
                        InternalLogger.Error("Error loading layout-appenders: {0}", ex);
                    }
                    continue;
                };
            }
        }

        private static void LoadExtensionFromAssembly(Assembly asm, string prefix)
        {
            LayoutAppenderFactory.AddLayoutAppendersFromAssembly(asm, prefix);
            AppenderFactory.AddAppendersFromAssembly(asm, prefix);
        }

        private void ConfigureAppendersFromElement(XmlElement element) {
            if (element == null)
                return;

            foreach (XmlElement appenderElement in element.GetElementsByTagName("appender")) {
                string type = appenderElement.GetAttribute("type");
                Appender newAppender = AppenderFactory.CreateAppender(type);
                if (newAppender != null) {
                    ConfigureAppenderFromXmlElement(newAppender, appenderElement);
                    AddAppender(newAppender.Name, newAppender);
                }
            }
        }

        private void ConfigureRuleFiltersFromXmlElement(AppenderRule rule, XmlElement element) {
            if (element == null)
                return;

            foreach (XmlNode node in element.ChildNodes)
            {
                if (node is XmlElement)
                {
                    string name = node.Name;

                    Filter filter = FilterFactory.CreateFilter(name);

                    foreach (XmlAttribute attrib in ((XmlElement)node).Attributes) {
                        string attribName = attrib.LocalName;
                        string attribValue = attrib.InnerText;

                        PropertyHelper.SetPropertyFromString(filter, attribName, attribValue);
                    }

                    rule.Filters.Add(filter);
                }
            }
        }

        private void ConfigureAppenderFromXmlElement(Appender appender, XmlElement element) {
            Type appenderType = appender.GetType();

            foreach (XmlAttribute attrib in element.Attributes) {
                string name = attrib.LocalName;
                string value = attrib.InnerText;

                if (name == "type")
                    continue;

                PropertyHelper.SetPropertyFromString(appender, name, value);
            }

            foreach (XmlNode node in element.ChildNodes)
            {
                if (node is XmlElement)
                {
                    XmlElement el = (XmlElement)node;
                    string name = el.Name;
                    string value = el.InnerXml;

                    PropertyHelper.SetPropertyFromString(appender, name, value);
                }
            }
        }
    }
}
