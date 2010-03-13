// 
// Copyright (c) 2004-2010 Jaroslaw Kowalski <jaak@jkowalski.net>
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

namespace NLog.Config
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Xml;
    using NLog.Common;
    using NLog.Filters;
    using NLog.Internal;
    using NLog.Layouts;
    using NLog.Targets;
    using NLog.Targets.Compound;
    using NLog.Targets.Wrappers;

    /// <summary>
    /// A class for configuring NLog through an XML configuration file 
    /// (App.config style or App.nlog style).
    /// </summary>
    public class XmlLoggingConfiguration : LoggingConfiguration
    {
        private NLogFactories nlogFactories = NLogFactories.Default;
        private Dictionary<string, bool> visitedFile = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, string> variables = new Dictionary<string, string>(EqualityComparer<string>.Default);

        private string originalFileName;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLoggingConfiguration" /> class.
        /// </summary>
        /// <param name="fileName">Configuration file to be read.</param>
        public XmlLoggingConfiguration(string fileName)
            : this(fileName, false)
        {
        }

#if !SILVERLIGHT

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLoggingConfiguration" /> class.
        /// </summary>
        /// <param name="element">The XML element.</param>
        /// <param name="fileName">Name of the XML file.</param>
        public XmlLoggingConfiguration(XmlElement element, string fileName)
            : this(XmlReader.Create(new StringReader(element.OuterXml)), fileName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLoggingConfiguration" /> class.
        /// </summary>
        /// <param name="element">The XML element.</param>
        /// <param name="fileName">Name of the XML file.</param>
        /// <param name="ignoreErrors">If set to <c>true</c> errors will be ignored during file processing.</param>
        public XmlLoggingConfiguration(XmlElement element, string fileName, bool ignoreErrors)
            : this(XmlReader.Create(new StringReader(element.OuterXml)), fileName, ignoreErrors)
        {
        }
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLoggingConfiguration" /> class.
        /// </summary>
        /// <param name="fileName">Configuration file to be read.</param>
        /// <param name="ignoreErrors">Ignore any errors during configuration.</param>
        public XmlLoggingConfiguration(string fileName, bool ignoreErrors)
        {
            InternalLogger.Info("Configuring from {0}...", fileName);
            this.originalFileName = fileName;
            try
            {
                this.ConfigureFromFile(fileName);
            }
            catch (Exception ex)
            {
                InternalLogger.Error("Error {0}...", ex);
                if (!ignoreErrors)
                {
                    throw new NLogConfigurationException("Exception occured when loading configuration from '" + fileName + "'", ex);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLoggingConfiguration" /> class.
        /// </summary>
        /// <param name="reader"><see cref="XmlReader"/> containing the configuration section.</param>
        /// <param name="fileName">Name of the file that contains the element (to be used as a base for including other files).</param>
        public XmlLoggingConfiguration(XmlReader reader, string fileName)
            : this(reader, fileName, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLoggingConfiguration" /> class.
        /// </summary>
        /// <param name="reader"><see cref="XmlReader"/> containing the configuration section.</param>
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
                    string key = Path.GetFullPath(fileName);
                    this.visitedFile[key] = true;

                    this.originalFileName = fileName;
                    this.ParseTopLevel(reader, Path.GetDirectoryName(fileName));
                }
                else
                {
                    this.ParseTopLevel(reader, null);
                }
            }
            catch (Exception ex)
            {
                InternalLogger.Error("Error {0}...", ex);
                if (!ignoreErrors)
                {
                    throw new NLogConfigurationException("Exception occured when loading configuration from XML Element in " + fileName, ex);
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

        /// <summary>
        /// Gets or sets a value indicating whether the configuration files
        /// should be watched for changes and reloaded automatically when changed.
        /// </summary>
        public bool AutoReload { get; set; }

        /// <summary>
        /// Gets the collection of file names which should be watched for changes by NLog.
        /// This is the list of configuration files processed.
        /// If the <c>autoReload</c> attribute is not set it returns empty collection.
        /// </summary>
        public override IEnumerable<string> FileNamesToWatch
        {
            get
            {
                if (this.AutoReload)
                {
                    return this.visitedFile.Keys;
                }
                else
                {
                    return new string[0];
                }
            }
        }

        /// <summary>
        /// Re-reads the original configuration file and returns the new <see cref="LoggingConfiguration" /> object.
        /// </summary>
        /// <returns>The new <see cref="XmlLoggingConfiguration" /> object.</returns>
        public override LoggingConfiguration Reload()
        {
            return new XmlLoggingConfiguration(this.originalFileName);
        }

        private void ConfigureFromFile(string fileName)
        {
            string key = Path.GetFullPath(fileName);
            if (this.visitedFile.ContainsKey(key))
            {
                return;
            }

            this.visitedFile[key] = true;

            using (XmlReader reader = XmlReader.Create(fileName))
            {
                reader.MoveToContent();
                this.ParseTopLevel(reader, Path.GetDirectoryName(fileName));
            }
        }

        private void ParseTopLevel(XmlReader reader, string baseDirectory)
        {
            switch (reader.LocalName.ToLower(CultureInfo.InvariantCulture))
            {
                case "configuration":
                    this.ParseConfigurationElement(reader, baseDirectory);
                    break;

                case "nlog":
                    this.ParseNLogElement(reader, baseDirectory);
                    break;

                default:
                    throw new NotSupportedException("Unrecognized configuration file element: " + reader.LocalName);
            }
        }

        private bool MoveToNextElement(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.EndElement)
                {
                    return false;
                }
                
                if (reader.NodeType == XmlNodeType.Element)
                {
                    return true;
                }
            }

            return false;
        }

        private void ParseConfigurationElement(XmlReader reader, string baseDirectory)
        {
            InternalLogger.Trace("ParseConfigurationElement");
            Debug.Assert(this.CaseInsensitiveEquals(reader.LocalName, "configuration"), "Expected <configuration /> element.");

            if (!reader.IsEmptyElement)
            {
                while (this.MoveToNextElement(reader))
                {
                    switch (reader.LocalName.ToLower(CultureInfo.InvariantCulture))
                    {
                        case "nlog":
                            this.ParseNLogElement(reader, baseDirectory);
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
            InternalLogger.Trace("ParseNLogElement");
            Debug.Assert(this.CaseInsensitiveEquals(reader.LocalName, "nlog"), "Expected <nlog/> element.");

            this.AutoReload = this.GetBooleanAttribute(reader, "autoReload", false);
            LogManager.ThrowExceptions = this.GetBooleanAttribute(reader, "throwExceptions", false);
            InternalLogger.LogToConsole = this.GetBooleanAttribute(reader, "internalLogToConsole", false);
#if !NET_CF
            InternalLogger.LogToConsoleError = this.GetBooleanAttribute(reader, "internalLogToConsoleError", false);
#endif
            InternalLogger.LogFile = this.GetCaseInsensitiveAttribute(reader, "internalLogFile", null);
            InternalLogger.LogLevel = LogLevel.FromString(this.GetCaseInsensitiveAttribute(reader, "internalLogLevel", "Off"));
            LogManager.GlobalThreshold = LogLevel.FromString(this.GetCaseInsensitiveAttribute(reader, "globalThreshold", "Trace"));

            if (!reader.IsEmptyElement)
            {
                while (this.MoveToNextElement(reader))
                {
                    switch (reader.LocalName.ToLower(CultureInfo.InvariantCulture))
                    {
                        case "extensions":
                            this.ParseExtensionsElement(reader, baseDirectory);
                            break;

                        case "include":
                            this.ParseIncludeElement(reader, baseDirectory);
                            break;

                        case "appenders":
                        case "targets":
                            this.ParseTargetsElement(reader);
                            break;

                        case "variable":
                            this.ParseVariableElement(reader);
                            break;

                        case "rules":
                            this.ParseRulesElement(reader, this.LoggingRules);
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
            Debug.Assert(this.CaseInsensitiveEquals(reader.LocalName, "rules"), "Expected <rules/> element.");
            if (!reader.IsEmptyElement)
            {
                while (this.MoveToNextElement(reader))
                {
                    if (!reader.LocalName.Equals("logger", StringComparison.OrdinalIgnoreCase))
                    {
                        reader.Skip();
                        continue;
                    }

                    this.ParseLoggerElement(reader, rulesCollection);
                }
            }
        }

        private void ParseLoggerElement(XmlReader reader, ICollection<LoggingRule> rulesCollection)
        {
            LoggingRule rule = new LoggingRule();
            string namePattern = this.GetCaseInsensitiveAttribute(reader, "name", "*");
            string appendTo = this.GetCaseInsensitiveAttribute(reader, "appendTo", null);
            if (appendTo == null)
            {
                appendTo = this.GetCaseInsensitiveAttribute(reader, "writeTo", null);
            }

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

            if (this.GetCaseInsensitiveAttribute(reader, "final", "false") == "true")
            {
                rule.Final = true;
            }

            string levelString;

            if (this.TryGetCaseInsensitiveAttribute(reader, "level", out levelString))
            {
                LogLevel level = LogLevel.FromString(levelString);
                rule.EnableLoggingForLevel(level);
            }
            else if (this.TryGetCaseInsensitiveAttribute(reader, "levels", out levelString))
            {
                levelString = this.CleanWhitespace(levelString);

                string[] tokens = levelString.Split(',');
                foreach (string s in tokens)
                {
                    if (!string.IsNullOrEmpty(s))
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

                if (this.TryGetCaseInsensitiveAttribute(reader, "minLevel", out minLevelString))
                {
                    minLevel = LogLevel.FromString(minLevelString).Ordinal;
                }

                if (this.TryGetCaseInsensitiveAttribute(reader, "maxLevel", out maxLevelString))
                {
                    maxLevel = LogLevel.FromString(maxLevelString).Ordinal;
                }

                for (int i = minLevel; i <= maxLevel; ++i)
                {
                    rule.EnableLoggingForLevel(LogLevel.FromOrdinal(i));
                }
            }

            if (!reader.IsEmptyElement)
            {
                while (this.MoveToNextElement(reader))
                {
                    switch (reader.LocalName.ToLower(CultureInfo.InvariantCulture))
                    {
                        case "filters":
                            this.ParseFilters(rule, reader);
                            break;

                        case "logger":
                            this.ParseLoggerElement(reader, rule.ChildRules);
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
                while (this.MoveToNextElement(reader))
                {
                    string name = reader.LocalName;

                    Filter filter = this.nlogFactories.FilterFactory.CreateInstance(name);
                    this.ConfigureObjectFromAttributes(filter, reader, this.variables, false);
                    rule.Filters.Add(filter);
                }
            }
        }

        private void ParseVariableElement(XmlReader reader)
        {
            reader.Skip();
        }

        private string GetCaseInsensitiveAttribute(XmlReader reader, string attributeName, string defaultValue)
        {
            string value;

            if (!this.TryGetCaseInsensitiveAttribute(reader, attributeName, out value))
            {
                return defaultValue;
            }
            else
            {
                return value;
            }
        }

        private bool GetBooleanAttribute(XmlReader reader, string attributeName, bool defaultValue)
        {
            string value;

            if (!this.TryGetCaseInsensitiveAttribute(reader, attributeName, out value))
            {
                return defaultValue;
            }

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

        private bool TryGetCaseInsensitiveAttribute(XmlReader reader, string attributeName, out string value)
        {
            if (reader.MoveToFirstAttribute())
            {
                do
                {
                    if (this.CaseInsensitiveEquals(reader.LocalName, attributeName))
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
            Debug.Assert(this.CaseInsensitiveEquals(reader.LocalName, "targets") || this.CaseInsensitiveEquals(reader.LocalName, "appenders"), "Expected <targets> or <appenders/> element.");

            bool asyncWrap = this.CaseInsensitiveEquals(this.GetCaseInsensitiveAttribute(reader, "async", "false"), "true");
            string defaultWrapperElementXml = null;
            Dictionary<string, string> typeNameToDefaultTargetParametersXml = new Dictionary<string, string>();

            if (!reader.IsEmptyElement)
            {
                while (this.MoveToNextElement(reader))
                {
                    string name = reader.LocalName;
                    string type = this.GetCaseInsensitiveAttribute(reader, "type", null);

                    switch (name.ToLower(CultureInfo.InvariantCulture))
                    {
                        case "default-wrapper":
                            defaultWrapperElementXml = reader.ReadOuterXml();
                            break;

                        case "default-target-parameters":
                            if (type == null)
                            {
                                throw new NLogConfigurationException("Missing 'type' attribute on <" + name + "/>.");
                            }

                            typeNameToDefaultTargetParametersXml[type] = reader.ReadOuterXml();
                            break;

                        case "target":
                        case "appender":
                        case "wrapper":
                        case "wrapper-target":
                        case "compound-target":
                            if (type == null)
                            {
                                throw new NLogConfigurationException("Missing 'type' attribute on <" + name + "/>.");
                            }

                            Target newTarget = this.nlogFactories.TargetFactory.CreateInstance(type);

                            string xml;
                            if (typeNameToDefaultTargetParametersXml.TryGetValue(type, out xml))
                            {
                                using (XmlReader reader1 = XmlReader.Create(new StringReader(xml)))
                                {
                                    this.ParseTargetElement(newTarget, reader1);
                                }
                            }

                            this.ParseTargetElement(newTarget, reader);

                            if (asyncWrap)
                            {
                                newTarget = this.WrapWithAsyncTarget(newTarget);
                            }

                            if (defaultWrapperElementXml != null)
                            {
                                using (XmlReader reader1 = XmlReader.Create(new StringReader(defaultWrapperElementXml)))
                                {
                                    newTarget = this.WrapWithDefaultWrapper(newTarget, reader1);
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

            CompoundTargetBase compound = target as CompoundTargetBase;
            WrapperTargetBase wrapper = target as WrapperTargetBase;

            this.ConfigureObjectFromAttributes(target, reader, this.variables, true);

            if (!reader.IsEmptyElement)
            {
                while (this.MoveToNextElement(reader))
                {
                    string name = reader.LocalName.ToLower(CultureInfo.InvariantCulture);

                    if (compound != null)
                    {
                        if (name == "target" || name == "wrapper" || name == "wrapper-target" || name == "compound-target")
                        {
                            string type;

                            if (!this.TryGetCaseInsensitiveAttribute(reader, "type", out type))
                            {
                                throw new NLogConfigurationException("Missing 'type' attribute on <" + name + " />");
                            }

                            Target newTarget = this.nlogFactories.TargetFactory.CreateInstance(type);
                            if (newTarget != null)
                            {
                                this.ParseTargetElement(newTarget, reader);
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
                        if (name == "target" || name == "wrapper" || name == "wrapper-target" || name == "compound-target")
                        {
                            string type;

                            if (!this.TryGetCaseInsensitiveAttribute(reader, "type", out type))
                            {
                                throw new NLogConfigurationException("Missing 'type' attribute on <" + name + " />");
                            }

                            Target newTarget = this.nlogFactories.TargetFactory.CreateInstance(type);
                            if (newTarget != null)
                            {
                                this.ParseTargetElement(newTarget, reader);
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

                    this.SetPropertyFromElement(target, reader);
                }
            }
        }

        private void ParseExtensionsElement(XmlReader reader, string baseDirectory)
        {
            if (!reader.IsEmptyElement)
            {
                while (this.MoveToNextElement(reader))
                {
                    if (this.CaseInsensitiveEquals(reader.LocalName, "add"))
                    {
                        string prefix;

                        if (this.TryGetCaseInsensitiveAttribute(reader, "prefix", out prefix))
                        {
                            prefix = prefix + ".";
                        }
                        else
                        {
                            prefix = null;
                        }

                        string assemblyFile;

                        if (this.TryGetCaseInsensitiveAttribute(reader, "assemblyFile", out assemblyFile))
                        {
                            try
                            {
                                string fullFileName = Path.Combine(baseDirectory, assemblyFile);
                                InternalLogger.Info("Loading assemblyFile: {0}", fullFileName);
                                Assembly asm = Assembly.LoadFrom(fullFileName);

                                this.nlogFactories.RegisterItemsFromAssembly(asm, prefix);
                            }
                            catch (Exception ex)
                            {
                                InternalLogger.Error("Error loading extensions: {0}", ex);
                                if (LogManager.ThrowExceptions)
                                {
                                    throw new NLogConfigurationException("Error loading extensions: " + assemblyFile, ex);
                                }
                            }

                            continue;
                        }

                        string assemblyName;
                        if (this.TryGetCaseInsensitiveAttribute(reader, "assembly", out assemblyName))
                        {
                            try
                            {
                                InternalLogger.Info("Loading assemblyName: {0}", assemblyName);
                                Assembly asm = Assembly.Load(assemblyName);

                                this.nlogFactories.RegisterItemsFromAssembly(asm, prefix);
                            }
                            catch (Exception ex)
                            {
                                InternalLogger.Error("Error loading extensions: {0}", ex);
                                if (LogManager.ThrowExceptions)
                                {
                                    throw new NLogConfigurationException("Error loading extensions: " + assemblyName, ex);
                                }
                            }

                            continue;
                        }
                    }
                }
            }
        }

        private void ParseIncludeElement(XmlReader reader, string baseDirectory)
        {
            string newFileName;

            if (!this.TryGetCaseInsensitiveAttribute(reader, "file", out newFileName))
            {
                throw new NLogConfigurationException("Missing 'file' argument for <include />");
            }

            try
            {
                newFileName = Path.Combine(baseDirectory, SimpleLayout.Evaluate(newFileName));

                if (File.Exists(newFileName))
                {
                    InternalLogger.Debug("Including file '{0}'", newFileName);
                    this.ConfigureFromFile(newFileName);
                }
                else
                {
                    throw new FileNotFoundException("Included file not found: " + newFileName);
                }
            }
            catch (Exception ex)
            {
                InternalLogger.Error("Error when including '{0}' {1}", newFileName, ex);

                if (this.CaseInsensitiveEquals(
                    this.GetCaseInsensitiveAttribute(reader, "ignoreErrors", "false"), 
                    "true"))
                {
                    return;
                }

                throw new NLogConfigurationException("Error when including: " + newFileName, ex);
            }
        }

        private bool CaseInsensitiveEquals(string p1, string p2)
        {
            return 0 == String.Compare(p1, p2, StringComparison.OrdinalIgnoreCase);
        }

        private bool SetPropertyFromElement(object o, XmlReader reader)
        {
            if (this.AddArrayItemFromElement(o, reader, this.variables))
            {
                return true;
            }

            if (this.SetLayoutFromElement(o, reader))
            {
                return true;
            }

            return PropertyHelper.SetPropertyFromString(o, reader.LocalName, reader.Value, this.variables);
        }

        private string CleanWhitespace(string s)
        {
            s = s.Replace(" ", string.Empty); // get rid of the whitespace
            return s;
        }

        private bool AddArrayItemFromElement(object o, XmlReader reader, IDictionary<string, string> variables)
        {
            string name = reader.Name;
            if (!PropertyHelper.IsArrayProperty(o.GetType(), name))
            {
                return false;
            }

            PropertyInfo propInfo;
            if (!PropertyHelper.TryGetPropertyInfo(o, name, out propInfo))
            {
                throw new NotSupportedException("Parameter " + name + " not supported on " + o.GetType().Name);
            }

            IList propertyValue = (IList)propInfo.GetValue(o, null);
            Type elementType = PropertyHelper.GetArrayItemType(propInfo);
            object arrayItem = FactoryHelper.CreateInstance(elementType);
            this.ConfigureObjectFromAttributes(arrayItem, reader, variables, true);
            this.ConfigureObjectFromElement(arrayItem, reader, variables);
            propertyValue.Add(arrayItem);
            return true;
        }

        private void ConfigureObjectFromAttributes(object targetObject, XmlReader reader, IDictionary<string, string> variables, bool ignoreType)
        {
            if (reader.MoveToFirstAttribute())
            {
                do
                {
                    string childName = reader.LocalName;
                    string childValue = reader.Value;

                    if (ignoreType && 0 == String.Compare(childName, "type", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    PropertyHelper.SetPropertyFromString(targetObject, childName, childValue, this.variables);
                }
                while (reader.MoveToNextAttribute());
            }

            reader.MoveToElement();
        }

        private bool SetLayoutFromElement(object o, XmlReader reader)
        {
            string name = reader.LocalName;
            if (!PropertyHelper.IsLayoutProperty(o.GetType(), name))
            {
                return false;
            }

            PropertyInfo targetPropertyInfo = o.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase);

            if (targetPropertyInfo != null && typeof(Layout).IsAssignableFrom(targetPropertyInfo.PropertyType))
            {
                string layoutTypeName;

                if (!this.TryGetCaseInsensitiveAttribute(reader, "type", out layoutTypeName))
                {
                    throw new NLogConfigurationException("No 'type' attributespecified for a layout parameter");
                }

                Layout layout = this.nlogFactories.LayoutFactory.CreateInstance(PropertyHelper.ExpandVariables(layoutTypeName, this.variables));
                this.ConfigureObjectFromAttributes(layout, reader, this.variables, true);
                this.ConfigureObjectFromElement(layout, reader, this.variables);
                targetPropertyInfo.SetValue(o, layout, null);
                return true;
            }

            if (name == "layout" && (o is TargetWithLayout))
            {
                string typeName;

                if (this.TryGetCaseInsensitiveAttribute(reader, "type", out typeName))
                {
                    typeName = PropertyHelper.ExpandVariables(typeName, this.variables);
                    Layout layout = this.nlogFactories.LayoutFactory.CreateInstance(typeName);
                    this.ConfigureObjectFromAttributes(layout, reader, this.variables, true);
                    this.ConfigureObjectFromElement(layout, reader, this.variables);
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

        private void ConfigureObjectFromElement(object targetObject, XmlReader reader, IDictionary<string, string> variables)
        {
            if (!reader.IsEmptyElement)
            {
                while (this.MoveToNextElement(reader))
                {
                    this.SetPropertyFromElement(targetObject, reader);
                }
            }
        }

        private Target WrapWithAsyncTarget(Target t)
        {
            AsyncTargetWrapper atw = new AsyncTargetWrapper();
            atw.WrappedTarget = t;
            atw.Name = t.Name;
            t.Name = t.Name + "_wrapped";
            InternalLogger.Debug("Wrapping target '{0}' with AsyncTargetWrapper and renaming to '{1}", atw.Name, t.Name);
            return atw;
        }

        private Target WrapWithDefaultWrapper(Target t, XmlReader reader)
        {
            string wrapperType;

            if (!this.TryGetCaseInsensitiveAttribute(reader, "type", out wrapperType))
            {
                // TODO - add error handling
            }

            Target wrapperTargetInstance = this.nlogFactories.TargetFactory.CreateInstance(wrapperType);
            WrapperTargetBase wtb = wrapperTargetInstance as WrapperTargetBase;
            if (wtb == null)
            {
                throw new NLogConfigurationException("Target type specified on <default-wrapper /> is not a wrapper.");
            }

            this.ParseTargetElement(wrapperTargetInstance, reader);
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
    }
}
