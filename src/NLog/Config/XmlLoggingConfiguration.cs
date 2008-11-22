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
using System.Collections.Generic;
using NLog.Config;
using NLog.Layouts;
using System.Diagnostics;

namespace NLog.Config
{
    /// <summary>
    /// A class for configuring NLog through an XML configuration file 
    /// (App.config style or App.nlog style)
    /// </summary>
    public class XmlLoggingConfiguration : LoggingConfiguration
    {
        private Dictionary<string, bool> _visitedFile = new Dictionary<string, bool>();
        private Dictionary<string, string> _variables = new Dictionary<string, string>(EqualityComparer<string>.Default);

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
        public XmlLoggingConfiguration(string fileName)
            : this(fileName, false)
        {
        }

#if !SILVERLIGHT
        public XmlLoggingConfiguration(XmlElement element, string fileName)
            : this(XmlReader.Create(new StringReader(element.OuterXml)), fileName)
        {
        }

        public XmlLoggingConfiguration(XmlElement element, string fileName, bool ignoreErrors)
            : this(XmlReader.Create(new StringReader(element.OuterXml)), fileName, ignoreErrors)
        {
        }
#endif

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
        /// class and reads the configuration from the specified XML reader.
        /// </summary>
        /// <param name="reader"><see cref="XmlReader" /> containing the configuration section.</param>
        /// <param name="fileName">Name of the file that contains the element (to be used as a base for including other files).</param>
        public XmlLoggingConfiguration(XmlReader reader, string fileName)
            : this(reader, fileName, false)
        {
        }

        /// <summary>
        /// Constructs a new instance of <see cref="XmlLoggingConfiguration" />
        /// class and reads the configuration from the specified XML reader.
        /// </summary>
        /// <param name="reader"><see cref="XmlReader" /> containing the configuration section.</param>
        /// <param name="fileName">Name of the file that contains the element (to be used as a base for including other files).</param>
        /// <param name="ignoreErrors">Ignore any errors during configuration.</param>
        public XmlLoggingConfiguration(XmlReader reader, string fileName, bool ignoreErrors)
        {
            try
            {
                reader.MoveToContent();
                if (fileName != null)
                {
                    InternalLogger.Info("Configuring from an XML element in {0}...", fileName);
                    string key = Path.GetFullPath(fileName).ToLower(CultureInfo.InvariantCulture);
                    _visitedFile[key] = true;

                    _originalFileName = fileName;
                    ParseTopLevel(reader, Path.GetDirectoryName(fileName));
                }
                else
                {
                    ParseTopLevel(reader, null);
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
                return;

            _visitedFile[key] = true;

            using (XmlReader reader = XmlReader.Create(fileName))
            {
                reader.MoveToContent();
                ParseTopLevel(reader, Path.GetDirectoryName(fileName));
            }
        }

        private void ParseTopLevel(XmlReader reader, string baseDirectory)
        {
            switch (reader.LocalName.ToLower(CultureInfo.InvariantCulture))
            {
                case "configuration":
                    ParseConfigurationElement(reader, baseDirectory);
                    break;

                case "nlog":
                    ParseNLogElement(reader, baseDirectory);
                    break;

                default:
                    throw new NotSupportedException("Unrecognized configuration file element: " + reader.LocalName);
            }
        }

        private static bool MoveToNextElement(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.EndElement)
                    return false;
                if (reader.NodeType == XmlNodeType.Element)
                    return true;
            }

            return false;
        }

        private void ParseConfigurationElement(XmlReader reader, string baseDirectory)
        {
            InternalLogger.Trace("ParseConfigurationElement");
            Debug.Assert(CaseInsensitiveEquals(reader.LocalName, "configuration"));

            if (!reader.IsEmptyElement)
            {
                while (MoveToNextElement(reader))
                {
                    switch (reader.LocalName.ToLower(CultureInfo.InvariantCulture))
                    {
                        case "nlog":
                            ParseNLogElement(reader, baseDirectory);
                            break;

                        default:
                            reader.Skip();
                            break;
                    }
                }
            }
        }

        private void ParseNLogElement(XmlReader reader, string baseDirectory)
        {
            InternalLogger.LogToConsole = true;
            InternalLogger.LogLevel = LogLevel.Trace;
            InternalLogger.Trace("ParseNLogElement");
            Debug.Assert(CaseInsensitiveEquals(reader.LocalName, "nlog"));

            AutoReload = GetBooleanAttribute(reader, "autoReload", false);
            LogManager.ThrowExceptions = GetBooleanAttribute(reader, "throwExceptions", false);
            InternalLogger.LogToConsole = GetBooleanAttribute(reader, "internalLogToConsole", false);
#if !NET_CF
            InternalLogger.LogToConsoleError = GetBooleanAttribute(reader, "internalLogToConsoleError", false);
#endif
            InternalLogger.LogFile = GetCaseInsensitiveAttribute(reader, "internalLogFile", null);
            InternalLogger.LogLevel = LogLevel.FromString(GetCaseInsensitiveAttribute(reader, "internalLogLevel", "Off"));
            LogManager.GlobalThreshold = LogLevel.FromString(GetCaseInsensitiveAttribute(reader, "globalThreshold", "Trace"));

            if (!reader.IsEmptyElement)
            {
                while (MoveToNextElement(reader))
                {
                    switch (reader.LocalName.ToLower(CultureInfo.InvariantCulture))
                    {
                        case "extensions":
                            ParseExtensionsElement(reader, baseDirectory);
                            break;

                        case "include":
                            ParseIncludeElement(reader, baseDirectory);
                            break;

                        case "appenders":
                        case "targets":
                            ParseTargetsElement(reader);
                            break;

                        case "variable":
                            ParseVariableElement(reader);
                            break;

                        case "rules":
                            ParseRulesElement(reader, this.LoggingRules);
                            break;

                        default:
                            InternalLogger.Warn("Skipping unknown node: {0}", reader.Name);
                            reader.Skip();
                            break;
                    }
                }
            }
        }

        private void ParseRulesElement(XmlReader reader, ICollection<LoggingRule> rulesCollection)
        {
            InternalLogger.Trace("ParseRulesElement");
            Debug.Assert(CaseInsensitiveEquals(reader.LocalName, "rules"));
            if (!reader.IsEmptyElement)
            {
                while (MoveToNextElement(reader))
                {
                    if (reader.LocalName.ToLower(CultureInfo.InvariantCulture) != "logger")
                    {
                        reader.Skip();
                        continue;
                    }

                    ParseLoggerElement(reader, rulesCollection);
                }
            }
        }

        private void ParseLoggerElement(XmlReader reader, ICollection<LoggingRule> rulesCollection)
        {
            LoggingRule rule = new LoggingRule();
            string namePattern = GetCaseInsensitiveAttribute(reader, "name", "*");
            string appendTo = GetCaseInsensitiveAttribute(reader, "appendTo", null);
            if (appendTo == null)
                appendTo = GetCaseInsensitiveAttribute(reader, "writeTo", null);

            rule.LoggerNamePattern = namePattern;
            if (appendTo != null)
            {
                foreach (string t in appendTo.Split(','))
                {
                    string targetName = t.Trim();
                    Target target = FindTargetByName(targetName);

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

            if (GetCaseInsensitiveAttribute(reader, "final", "false") == "true")
            {
                rule.Final = true;
            }

            string levelString;

            if (TryGetCaseInsensitiveAttribute(reader, "level", out levelString))
            {
                LogLevel level = LogLevel.FromString(levelString);
                rule.EnableLoggingForLevel(level);
            }
            else if (TryGetCaseInsensitiveAttribute(reader, "levels", out levelString))
            {
                levelString = CleanWhitespace(levelString);

                string[] tokens = levelString.Split(',');
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
                string minLevelString;
                string maxLevelString;

                if (TryGetCaseInsensitiveAttribute(reader, "minLevel", out minLevelString))
                    minLevel = LogLevel.FromString(minLevelString).Ordinal;

                if (TryGetCaseInsensitiveAttribute(reader, "maxLevel", out maxLevelString))
                    maxLevel = LogLevel.FromString(maxLevelString).Ordinal;

                for (int i = minLevel; i <= maxLevel; ++i)
                {
                    rule.EnableLoggingForLevel(LogLevel.FromOrdinal(i));
                }
            }

            if (!reader.IsEmptyElement)
            {
                while (MoveToNextElement(reader))
                {
                    switch (reader.LocalName.ToLower(CultureInfo.InvariantCulture))
                    {
                        case "filters":
                            ParseFilters(rule, reader);
                            break;

                        case "logger":
                            ParseLoggerElement(reader, rule.ChildRules);
                            break;

                        default:
                            reader.Skip();
                            break;
                    }
                }
            }

            rulesCollection.Add(rule);
        }

        private void ParseFilters(LoggingRule rule, XmlReader reader)
        {
            if (!reader.IsEmptyElement)
            {
                while (MoveToNextElement(reader))
                {
                    string name = reader.LocalName;

                    Filter filter = NLogFactories.FilterFactory.Create(name);
                    ConfigureObjectFromAttributes(filter, reader, _variables, false);
                    rule.Filters.Add(filter);
                }
            }
        }

        private void ParseVariableElement(XmlReader reader)
        {
            reader.Skip();
        }

        private static string GetCaseInsensitiveAttribute(XmlReader reader, string attributeName, string defaultValue)
        {
            string value;

            if (!TryGetCaseInsensitiveAttribute(reader, attributeName, out value))
                return defaultValue;
            else
                return value;
        }

        private static bool GetBooleanAttribute(XmlReader reader, string attributeName, bool defaultValue)
        {
            string value;

            if (!TryGetCaseInsensitiveAttribute(reader, attributeName, out value))
                return defaultValue;

            switch (value.ToLower(CultureInfo.InvariantCulture))
            {
                case "true":
                    return true;

                case "false":
                    return false;

                default:
                    throw new NLogConfigurationException("Invalid value specified for '" + attributeName + "' attribute. Must be 'true' or 'false'");
            }
        }

        private static bool TryGetCaseInsensitiveAttribute(XmlReader reader, string attributeName, out string value)
        {
            if (reader.MoveToFirstAttribute())
            {
                do
                {
                    if (CaseInsensitiveEquals(reader.LocalName, attributeName))
                    {
                        value = reader.Value;
                        reader.MoveToElement();
                        return true;
                    }
                }
                while (reader.MoveToNextAttribute());
                reader.MoveToElement();
                value = null;
                return false;
            }
            else
            {
                value = null;
                return false;
            }
        }

        private void ParseTargetsElement(XmlReader reader)
        {
            InternalLogger.Trace("ParseTargetsElement");
            Debug.Assert(CaseInsensitiveEquals(reader.LocalName, "targets") || CaseInsensitiveEquals(reader.LocalName, "appenders"));

            bool asyncWrap = CaseInsensitiveEquals(GetCaseInsensitiveAttribute(reader, "async", "false"), "true");
            string defaultWrapperElementXml = null;
            Dictionary<string, string> typeNameToDefaultTargetParametersXml = new Dictionary<string, string>();

            if (!reader.IsEmptyElement)
            {
                while (MoveToNextElement(reader))
                {
                    string name = reader.LocalName;
                    string type = GetCaseInsensitiveAttribute(reader, "type", null);

                    switch (name.ToLower(CultureInfo.InvariantCulture))
                    {
                        case "default-wrapper":
                            defaultWrapperElementXml = reader.ReadOuterXml();
                            break;

                        case "default-target-parameters":
                            if (type == null)
                                throw new NLogConfigurationException("Missing 'type' attribute on <" + name + "/>.");
                            typeNameToDefaultTargetParametersXml[type] = reader.ReadOuterXml();
                            break;

                        case "target":
                        case "appender":
                        case "wrapper":
                        case "wrapper-target":
                        case "compound-target":
                            if (type == null)
                                throw new NLogConfigurationException("Missing 'type' attribute on <" + name + "/>.");

                            Target newTarget = NLogFactories.TargetFactory.Create(type);

                            string xml;
                            if (typeNameToDefaultTargetParametersXml.TryGetValue(type, out xml))
                            {
                                using (XmlReader reader1 = XmlReader.Create(new StringReader(xml)))
                                {
                                    ParseTargetElement(newTarget, reader1);
                                }
                            }

                            ParseTargetElement(newTarget, reader);

                            if (asyncWrap)
                                newTarget = WrapWithAsyncTarget(newTarget);

                            if (defaultWrapperElementXml != null)
                            {
                                using (XmlReader reader1 = XmlReader.Create(new StringReader(defaultWrapperElementXml)))
                                {
                                    newTarget = WrapWithDefaultWrapper(newTarget, reader1);
                                }
                            }

                            InternalLogger.Info("Adding target {0}", newTarget);
                            AddTarget(newTarget.Name, newTarget);
                            break;

                        default:
                            reader.Skip();
                            break;
                    }
                }
            }
        }

        private void ParseTargetElement(Target target, XmlReader reader)
        {
            InternalLogger.Trace("ParseTargetElement name={0} type={1}", reader.GetAttribute("name"), reader.GetAttribute("type"));

            NLog.Targets.Compound.CompoundTargetBase compound = target as NLog.Targets.Compound.CompoundTargetBase;
            NLog.Targets.Wrappers.WrapperTargetBase wrapper = target as NLog.Targets.Wrappers.WrapperTargetBase;

            ConfigureObjectFromAttributes(target, reader, _variables, true);

            if (!reader.IsEmptyElement)
            {
                while (MoveToNextElement(reader))
                {
                    string name = reader.LocalName.ToLower(CultureInfo.InvariantCulture);

                    if (compound != null)
                    {
                        if ((name == "target" || name == "wrapper" || name == "wrapper-target" || name == "compound-target"))
                        {
                            string type;

                            if (!TryGetCaseInsensitiveAttribute(reader, "type", out type))
                                throw new NLogConfigurationException("Missing 'type' attribute on <" + name + " />");

                            Target newTarget = NLogFactories.TargetFactory.Create(type);
                            if (newTarget != null)
                            {
                                ParseTargetElement(newTarget, reader);
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
                            string type;

                            if (!TryGetCaseInsensitiveAttribute(reader, "type", out type))
                                throw new NLogConfigurationException("Missing 'type' attribute on <" + name + " />");

                            Target newTarget = NLogFactories.TargetFactory.Create(type);
                            if (newTarget != null)
                            {
                                ParseTargetElement(newTarget, reader);
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

                    SetPropertyFromElement(target, reader, _variables);
                }
            }
        }

        private void ParseExtensionsElement(XmlReader reader, string baseDirectory)
        {
            if (!reader.IsEmptyElement)
            {
                while (MoveToNextElement(reader))
                {
                    if (CaseInsensitiveEquals(reader.LocalName, "add"))
                    {
                        string prefix;

                        if (TryGetCaseInsensitiveAttribute(reader, "prefix", out prefix))
                        {
                            prefix = prefix + ".";
                        }
                        else
                        {
                            prefix = null;
                        }

                        string assemblyFile;

                        if (TryGetCaseInsensitiveAttribute(reader, "assemblyFile", out assemblyFile))
                        {
                            try
                            {
                                string fullFileName = Path.Combine(baseDirectory, assemblyFile);
                                InternalLogger.Info("Loading assemblyFile: {0}", fullFileName);
                                Assembly asm = Assembly.LoadFrom(fullFileName);

                                NLogFactories.ScanAssembly(asm, prefix);
                            }
                            catch (Exception ex)
                            {
                                InternalLogger.Error("Error loading extensions: {0}", ex);
                                if (LogManager.ThrowExceptions)
                                    throw new NLogConfigurationException("Error loading extensions: " + assemblyFile, ex);
                            }
                            continue;
                        };

                        string assemblyName;
                        if (TryGetCaseInsensitiveAttribute(reader, "assembly", out assemblyName))
                        {
                            try
                            {
                                InternalLogger.Info("Loading assemblyName: {0}", assemblyName);
                                Assembly asm = Assembly.Load(assemblyName);

                                NLogFactories.ScanAssembly(asm, prefix);
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
        }

        private void ParseIncludeElement(XmlReader reader, string baseDirectory)
        {
            string newFileName;

            if (!TryGetCaseInsensitiveAttribute(reader, "file", out newFileName))
                throw new NLogConfigurationException("Missing 'file' argument for <include />");

            try
            {
                newFileName = Path.Combine(baseDirectory, SimpleLayout.Evaluate(newFileName));

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

                if (CaseInsensitiveEquals(GetCaseInsensitiveAttribute(reader, "ignoreErrors", "false"), "true"))
                    return;
                throw new NLogConfigurationException("Error when including: " + newFileName, ex);
            }
        }

        private static bool CaseInsensitiveEquals(string p1, string p2)
        {
            return 0 == String.Compare(p1, p2, StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool SetPropertyFromElement(object o, XmlReader reader, IDictionary<string, string> variables)
        {
            if (AddArrayItemFromElement(o, reader, variables))
                return true;

            if (SetLayoutFromElement(o, reader, variables))
                return true;

            return PropertyHelper.SetPropertyFromString(o, reader.LocalName, reader.Value, variables);
        }

        private static string CleanWhitespace(string s)
        {
            s = s.Replace(" ", ""); // get rid of the whitespace
            return s;
        }

        private static bool AddArrayItemFromElement(object o, XmlReader reader, IDictionary<string, string> variables)
        {
            string name = reader.Name;
            if (!PropertyHelper.IsArrayProperty(o.GetType(), name))
                return false;
            PropertyInfo propInfo;
            if (!PropertyHelper.TryGetPropertyInfo(o, name, out propInfo))
                throw new NotSupportedException("Parameter " + name + " not supported on " + o.GetType().Name);

            IList propertyValue = (IList)propInfo.GetValue(o, null);
            Type elementType = PropertyHelper.GetArrayItemType(propInfo);
            object arrayItem = FactoryHelper.CreateInstance(elementType);
            ConfigureObjectFromAttributes(arrayItem, reader, variables, true);
            ConfigureObjectFromElement(arrayItem, reader, variables);
            propertyValue.Add(arrayItem);
            return true;
        }

        private static void ConfigureObjectFromAttributes(object targetObject, XmlReader reader, IDictionary<string, string> variables, bool ignoreType)
        {
            if (reader.MoveToFirstAttribute())
            {
                do
                {
                    string childName = reader.LocalName;
                    string childValue = reader.Value;

                    if (ignoreType && 0 == String.Compare(childName, "type", true))
                        continue;

                    PropertyHelper.SetPropertyFromString(targetObject, childName, childValue, variables);
                } while (reader.MoveToNextAttribute());
            }
            reader.MoveToElement();
        }

        internal static bool SetLayoutFromElement(object o, XmlReader reader, IDictionary<string, string> variables)
        {
            string name = reader.LocalName;
            if (!PropertyHelper.IsLayoutProperty(o.GetType(), name))
                return false;

            PropertyInfo targetPropertyInfo = o.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase);

            if (targetPropertyInfo != null && typeof(Layout).IsAssignableFrom(targetPropertyInfo.PropertyType))
            {
                string layoutTypeName;

                if (!TryGetCaseInsensitiveAttribute(reader, "type", out layoutTypeName))
                {
                    throw new NLogConfigurationException("No 'type' attributespecified for a layout parameter");
                }
                Layout layout = NLogFactories.LayoutFactory.Create(PropertyHelper.ExpandVariables(layoutTypeName, variables));
                ConfigureObjectFromAttributes(layout, reader, variables, true);
                ConfigureObjectFromElement(layout, reader, variables);
                targetPropertyInfo.SetValue(o, layout, null);
                return true;
            }

            if (name == "layout" && (o is TargetWithLayout))
            {
                string typeName;
                
                if (TryGetCaseInsensitiveAttribute(reader, "type", out typeName))
                {
                    typeName = PropertyHelper.ExpandVariables(typeName, variables);
                    Layout layout = NLogFactories.LayoutFactory.Create(typeName);
                    ConfigureObjectFromAttributes(layout, reader, variables, true);
                    ConfigureObjectFromElement(layout, reader, variables);
                    ((TargetWithLayout)o).Layout = layout;
                }
                else
                {
                    ((TargetWithLayout)o).Layout = reader.Value;
                }
                return true;
            }

            return false;
        }

        private static void ConfigureObjectFromElement(object targetObject, XmlReader reader, IDictionary<string, string> variables)
        {
            if (!reader.IsEmptyElement)
            {
                while (MoveToNextElement(reader))
                {
                    SetPropertyFromElement(targetObject, reader, variables);
                }
            }
        }

#if !NET_CF && !SILVERLIGHT
        /// <summary>
        /// Gets the default <see cref="LoggingConfiguration" /> object by parsing 
        /// the application configuration file (<c>app.exe.config</c>).
        /// </summary>
        public static LoggingConfiguration AppConfig
        {
            get
            {
                object o = System.Configuration.ConfigurationManager.GetSection("nlog");
                return o as LoggingConfiguration;
            }
        }
#endif

        private Target WrapWithAsyncTarget(Target t)
        {
            NLog.Targets.Wrappers.AsyncTargetWrapper atw = new NLog.Targets.Wrappers.AsyncTargetWrapper();
            atw.WrappedTarget = t;
            atw.Name = t.Name;
            t.Name = t.Name + "_wrapped";
            InternalLogger.Debug("Wrapping target '{0}' with AsyncTargetWrapper and renaming to '{1}", atw.Name, t.Name);
            return atw;
        }

        private Target WrapWithDefaultWrapper(Target t, XmlReader reader)
        {
            string wrapperType;

            if (!TryGetCaseInsensitiveAttribute(reader, "type", out wrapperType))
            {
                // TODO - add error handling
            }
            Target wrapperTargetInstance = NLogFactories.TargetFactory.Create(wrapperType);
            WrapperTargetBase wtb = wrapperTargetInstance as WrapperTargetBase;
            if (wtb == null)
                throw new NLogConfigurationException("Target type specified on <default-wrapper /> is not a wrapper.");
            ParseTargetElement(wrapperTargetInstance, reader);
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
    }
}
