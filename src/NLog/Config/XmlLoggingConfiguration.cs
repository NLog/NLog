// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
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
    using System.Linq;
    using System.IO;
    using System.Reflection;
    using System.Xml;
    using NLog.Common;
    using NLog.Filters;
    using NLog.Internal;
    using NLog.Layouts;
    using NLog.Targets;
    using NLog.Targets.Wrappers;
    using NLog.LayoutRenderers;
    using NLog.Time;
    using System.Collections.ObjectModel;
#if SILVERLIGHT
// ReSharper disable once RedundantUsingDirective
    using System.Windows;
#endif

    /// <summary>
    /// A class for configuring NLog through an XML configuration file 
    /// (App.config style or App.nlog style).
    /// </summary>
    ///<remarks>This class is thread-safe.<c>.ToList()</c> is used for that purpose.</remarks>
    public class XmlLoggingConfiguration : LoggingConfiguration
    {
#if __ANDROID__

        /// <summary>
        /// Prefix for assets in Xamarin Android
        /// </summary>
        internal const string AssetsPrefix = "assets/";
#endif

        #region private fields

        private readonly Dictionary<string, bool> fileMustAutoReloadLookup = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        private string originalFileName;

        private LogFactory logFactory = null;

        private ConfigurationItemFactory ConfigurationItemFactory
        {
            get { return ConfigurationItemFactory.Default; }
        }

        #endregion

        #region contructors

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLoggingConfiguration" /> class.
        /// </summary>
        /// <param name="fileName">Configuration file to be read.</param>
        public XmlLoggingConfiguration(string fileName)
            : this(fileName, LogManager.LogFactory)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLoggingConfiguration" /> class.
        /// </summary>
        /// <param name="fileName">Configuration file to be read.</param>
        /// <param name="logFactory">The <see cref="LogFactory" /> to which to apply any applicable configuration values.</param>
        public XmlLoggingConfiguration(string fileName, LogFactory logFactory)
            : this(fileName, false, logFactory)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLoggingConfiguration" /> class.
        /// </summary>
        /// <param name="fileName">Configuration file to be read.</param>
        /// <param name="ignoreErrors">Ignore any errors during configuration.</param>
        public XmlLoggingConfiguration(string fileName, bool ignoreErrors)
            : this(fileName, ignoreErrors, LogManager.LogFactory)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLoggingConfiguration" /> class.
        /// </summary>
        /// <param name="fileName">Configuration file to be read.</param>
        /// <param name="ignoreErrors">Ignore any errors during configuration.</param>
        /// <param name="logFactory">The <see cref="LogFactory" /> to which to apply any applicable configuration values.</param>
        public XmlLoggingConfiguration(string fileName, bool ignoreErrors, LogFactory logFactory)
        {
            this.logFactory = logFactory;

            using (XmlReader reader = CreateFileReader(fileName))
            {
                this.Initialize(reader, fileName, ignoreErrors);
            }
        }

        /// <summary>
        /// Create XML reader for (xml config) file.
        /// </summary>
        /// <param name="fileName">filepath</param>
        /// <returns>reader or <c>null</c> if filename is empty.</returns>
        private static XmlReader CreateFileReader(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                fileName = fileName.Trim();
#if __ANDROID__
                //suport loading config from special assets folder in nlog.config
                if (fileName.StartsWith(AssetsPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    //remove prefix
                    fileName = fileName.Substring(AssetsPrefix.Length);
                    Stream stream = Android.App.Application.Context.Assets.Open(fileName);
                    return XmlReader.Create(stream);
                }
#endif
                return XmlReader.Create(fileName);
            }
            return null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLoggingConfiguration" /> class.
        /// </summary>
        /// <param name="reader"><see cref="XmlReader"/> containing the configuration section.</param>
        /// <param name="fileName">Name of the file that contains the element (to be used as a base for including other files).</param>
        public XmlLoggingConfiguration(XmlReader reader, string fileName)
            : this(reader, fileName, LogManager.LogFactory)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLoggingConfiguration" /> class.
        /// </summary>
        /// <param name="reader"><see cref="XmlReader"/> containing the configuration section.</param>
        /// <param name="fileName">Name of the file that contains the element (to be used as a base for including other files).</param>
        /// <param name="logFactory">The <see cref="LogFactory" /> to which to apply any applicable configuration values.</param>
        public XmlLoggingConfiguration(XmlReader reader, string fileName, LogFactory logFactory)
            : this(reader, fileName, false, logFactory)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLoggingConfiguration" /> class.
        /// </summary>
        /// <param name="reader"><see cref="XmlReader"/> containing the configuration section.</param>
        /// <param name="fileName">Name of the file that contains the element (to be used as a base for including other files).</param>
        /// <param name="ignoreErrors">Ignore any errors during configuration.</param>
        public XmlLoggingConfiguration(XmlReader reader, string fileName, bool ignoreErrors)
            : this(reader, fileName, ignoreErrors, LogManager.LogFactory)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLoggingConfiguration" /> class.
        /// </summary>
        /// <param name="reader"><see cref="XmlReader"/> containing the configuration section.</param>
        /// <param name="fileName">Name of the file that contains the element (to be used as a base for including other files).</param>
        /// <param name="ignoreErrors">Ignore any errors during configuration.</param>
        /// <param name="logFactory">The <see cref="LogFactory" /> to which to apply any applicable configuration values.</param>
        public XmlLoggingConfiguration(XmlReader reader, string fileName, bool ignoreErrors, LogFactory logFactory)
        {
            this.logFactory = logFactory;
            this.Initialize(reader, fileName, ignoreErrors);
        }

#if !SILVERLIGHT
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLoggingConfiguration" /> class.
        /// </summary>
        /// <param name="element">The XML element.</param>
        /// <param name="fileName">Name of the XML file.</param>
        internal XmlLoggingConfiguration(XmlElement element, string fileName)
        {
            logFactory = LogManager.LogFactory;

            using (var stringReader = new StringReader(element.OuterXml))
            {
                XmlReader reader = XmlReader.Create(stringReader);

                this.Initialize(reader, fileName, false);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLoggingConfiguration" /> class.
        /// </summary>
        /// <param name="element">The XML element.</param>
        /// <param name="fileName">Name of the XML file.</param>
        /// <param name="ignoreErrors">If set to <c>true</c> errors will be ignored during file processing.</param>
        internal XmlLoggingConfiguration(XmlElement element, string fileName, bool ignoreErrors)
        {
            using (var stringReader = new StringReader(element.OuterXml))
            {
                XmlReader reader = XmlReader.Create(stringReader);

                this.Initialize(reader, fileName, ignoreErrors);
            }
        }
#endif
        #endregion

        #region public properties

#if !SILVERLIGHT && !__IOS__ && !__ANDROID__
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
        /// Did the <see cref="Initialize"/> Succeeded? <c>true</c>= success, <c>false</c>= error, <c>null</c> = initialize not started yet.
        /// </summary>
        public bool? InitializeSucceeded { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether all of the configuration files
        /// should be watched for changes and reloaded automatically when changed.
        /// </summary>
        public bool AutoReload
        {
            get
            {
                return this.fileMustAutoReloadLookup.Values.All(mustAutoReload => mustAutoReload);
            }
            set
            {
                var autoReloadFiles = this.fileMustAutoReloadLookup.Keys.ToList();
                foreach (string nextFile in autoReloadFiles)
                    this.fileMustAutoReloadLookup[nextFile] = value;
            }
        }

        /// <summary>
        /// Gets the collection of file names which should be watched for changes by NLog.
        /// This is the list of configuration files processed.
        /// If the <c>autoReload</c> attribute is not set it returns empty collection.
        /// </summary>
        public override IEnumerable<string> FileNamesToWatch
        {
            get
            {
                return this.fileMustAutoReloadLookup.Where(entry => entry.Value).Select(entry => entry.Key);
            }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Re-reads the original configuration file and returns the new <see cref="LoggingConfiguration" /> object.
        /// </summary>
        /// <returns>The new <see cref="XmlLoggingConfiguration" /> object.</returns>
        public override LoggingConfiguration Reload()
        {
            return new XmlLoggingConfiguration(this.originalFileName);
        }

        #endregion

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

        /// <summary>
        /// Initializes the configuration.
        /// </summary>
        /// <param name="reader"><see cref="XmlReader"/> containing the configuration section.</param>
        /// <param name="fileName">Name of the file that contains the element (to be used as a base for including other files).</param>
        /// <param name="ignoreErrors">Ignore any errors during configuration.</param>
        private void Initialize(XmlReader reader, string fileName, bool ignoreErrors)
        {
            try
            {
                InitializeSucceeded = null;
                reader.MoveToContent();
                var content = new NLogXmlElement(reader);
                if (fileName != null)
                {
                    this.originalFileName = fileName;
                    this.ParseTopLevel(content, fileName, autoReloadDefault: false);

                    InternalLogger.Info("Configured from an XML element in {0}...", fileName);
                }
                else
                {
                    this.ParseTopLevel(content, null, autoReloadDefault: false);
                }
                InitializeSucceeded = true;

                this.CheckUnusedTargets();

            }
            catch (Exception exception)
            {
                InitializeSucceeded = false;
                if (exception.MustBeRethrownImmediately())
                {
                    throw;
                }

                var configurationException = new NLogConfigurationException("Exception occurred when loading configuration from " + fileName, exception);
                InternalLogger.Error(configurationException, "Error in Parsing Configuration File.");

                if (!ignoreErrors)
                {
                    if (exception.MustBeRethrown())
                    {
                        throw configurationException;
                    }
                }

            }
        }

        /// <summary>
        /// Checks whether unused targets exist. If found any, just write an internal log at Warn level.
        /// <remarks>If initializing not started or failed, then checking process will be canceled</remarks>
        /// </summary>
        private void CheckUnusedTargets()
        {
            if (this.InitializeSucceeded == null)
            {
                InternalLogger.Warn("Unused target checking is canceled -> initialize not started yet.");
                return;
            }
            if (!this.InitializeSucceeded.Value)
            {
                InternalLogger.Warn("Unused target checking is canceled -> initialize not succeeded.");
                return;
            }

            ReadOnlyCollection<Target> configuredNamedTargets = this.ConfiguredNamedTargets; //assign to variable because `ConfiguredNamedTargets` computes a new list every time.
            InternalLogger.Debug("Unused target checking is started... Rule Count: {0}, Target Count: {1}", this.LoggingRules.Count, configuredNamedTargets.Count);

            HashSet<string> targetNamesAtRules = new HashSet<string>(this.LoggingRules.SelectMany(r => r.Targets).Select(t => t.Name));

            int unusedCount = 0;
            configuredNamedTargets.ToList().ForEach((target) =>
            {
                if (!targetNamesAtRules.Contains(target.Name))
                {
                    InternalLogger.Warn("Unused target detected. Add a rule for this target to the configuration. TargetName: {0}", target.Name);
                    unusedCount++;
                }
            });

            InternalLogger.Debug("Unused target checking is completed. Total Rule Count: {0}, Total Target Count: {1}, Unused Target Count: {2}", this.LoggingRules.Count, configuredNamedTargets.Count, unusedCount);
        }

        private void ConfigureFromFile(string fileName, bool autoReloadDefault)
        {
            if (!this.fileMustAutoReloadLookup.ContainsKey(GetFileLookupKey(fileName)))
                this.ParseTopLevel(new NLogXmlElement(fileName), fileName, autoReloadDefault);
        }

        #region parse methods

        /// <summary>
        /// Parse the root
        /// </summary>
        /// <param name="content"></param>
        /// <param name="filePath">path to config file.</param>
        /// <param name="autoReloadDefault">The default value for the autoReload option.</param>
        private void ParseTopLevel(NLogXmlElement content, string filePath, bool autoReloadDefault)
        {
            content.AssertName("nlog", "configuration");

            switch (content.LocalName.ToUpper(CultureInfo.InvariantCulture))
            {
                case "CONFIGURATION":
                    this.ParseConfigurationElement(content, filePath, autoReloadDefault);
                    break;

                case "NLOG":
                    this.ParseNLogElement(content, filePath, autoReloadDefault);
                    break;
            }
        }

        /// <summary>
        /// Parse {configuration} xml element.
        /// </summary>
        /// <param name="configurationElement"></param>
        /// <param name="filePath">path to config file.</param>
        /// <param name="autoReloadDefault">The default value for the autoReload option.</param>
        private void ParseConfigurationElement(NLogXmlElement configurationElement, string filePath, bool autoReloadDefault)
        {
            InternalLogger.Trace("ParseConfigurationElement");
            configurationElement.AssertName("configuration");

            var nlogElements = configurationElement.Elements("nlog").ToList();
            foreach (var nlogElement in nlogElements)
            {
                this.ParseNLogElement(nlogElement, filePath, autoReloadDefault);
            }
        }

        /// <summary>
        /// Parse {NLog} xml element.
        /// </summary>
        /// <param name="nlogElement"></param>
        /// <param name="filePath">path to config file.</param>
        /// <param name="autoReloadDefault">The default value for the autoReload option.</param>
        private void ParseNLogElement(NLogXmlElement nlogElement, string filePath, bool autoReloadDefault)
        {
            InternalLogger.Trace("ParseNLogElement");
            nlogElement.AssertName("nlog");

            if (nlogElement.GetOptionalBooleanAttribute("useInvariantCulture", false))
            {
                this.DefaultCultureInfo = CultureInfo.InvariantCulture;
            }
#pragma warning disable 618
            this.ExceptionLoggingOldStyle = nlogElement.GetOptionalBooleanAttribute("exceptionLoggingOldStyle", false);
#pragma warning restore 618

            bool autoReload = nlogElement.GetOptionalBooleanAttribute("autoReload", autoReloadDefault);
            if (filePath != null)
                this.fileMustAutoReloadLookup[GetFileLookupKey(filePath)] = autoReload;

            logFactory.ThrowExceptions = nlogElement.GetOptionalBooleanAttribute("throwExceptions", logFactory.ThrowExceptions);
            logFactory.ThrowConfigExceptions = nlogElement.GetOptionalBooleanAttribute("throwConfigExceptions", logFactory.ThrowConfigExceptions);
            InternalLogger.LogToConsole = nlogElement.GetOptionalBooleanAttribute("internalLogToConsole", InternalLogger.LogToConsole);
            InternalLogger.LogToConsoleError = nlogElement.GetOptionalBooleanAttribute("internalLogToConsoleError", InternalLogger.LogToConsoleError);
            InternalLogger.LogFile = nlogElement.GetOptionalAttribute("internalLogFile", InternalLogger.LogFile);
            InternalLogger.LogLevel = LogLevel.FromString(nlogElement.GetOptionalAttribute("internalLogLevel", InternalLogger.LogLevel.Name));
#if !SILVERLIGHT && !__IOS__ && !__ANDROID__
            InternalLogger.LogToTrace = nlogElement.GetOptionalBooleanAttribute("internalLogToTrace", InternalLogger.LogToTrace);
#endif
            InternalLogger.IncludeTimestamp = nlogElement.GetOptionalBooleanAttribute("internalLogIncludeTimestamp", InternalLogger.IncludeTimestamp);
            logFactory.GlobalThreshold = LogLevel.FromString(nlogElement.GetOptionalAttribute("globalThreshold", logFactory.GlobalThreshold.Name));

            var children = nlogElement.Children.ToList();

            //first load the extensions, as the can be used in other elements (targets etc)
            var extensionsChilds = children.Where(child => child.LocalName.Equals("EXTENSIONS", StringComparison.InvariantCultureIgnoreCase)).ToList();
            foreach (var extensionsChild in extensionsChilds)
            {
                this.ParseExtensionsElement(extensionsChild, Path.GetDirectoryName(filePath));
            }

            //parse all other direct elements
            foreach (var child in children)
            {
                switch (child.LocalName.ToUpper(CultureInfo.InvariantCulture))
                {
                    case "EXTENSIONS":
                        //already parsed
                        break;

                    case "INCLUDE":
                        this.ParseIncludeElement(child, Path.GetDirectoryName(filePath), autoReloadDefault: autoReload);
                        break;

                    case "APPENDERS":
                    case "TARGETS":
                        this.ParseTargetsElement(child);
                        break;

                    case "VARIABLE":
                        this.ParseVariableElement(child);
                        break;

                    case "RULES":
                        this.ParseRulesElement(child, this.LoggingRules);
                        break;

                    case "TIME":
                        this.ParseTimeElement(child);
                        break;

                    default:
                        InternalLogger.Warn("Skipping unknown node: {0}", child.LocalName);
                        break;
                }
            }
        }

        /// <summary>
        /// Parse {Rules} xml element
        /// </summary>
        /// <param name="rulesElement"></param>
        /// <param name="rulesCollection">Rules are added to this parameter.</param>
        private void ParseRulesElement(NLogXmlElement rulesElement, IList<LoggingRule> rulesCollection)
        {
            InternalLogger.Trace("ParseRulesElement");
            rulesElement.AssertName("rules");

            var loggerElements = rulesElement.Elements("logger").ToList();
            foreach (var loggerElement in loggerElements)
            {
                this.ParseLoggerElement(loggerElement, rulesCollection);
            }
        }

        /// <summary>
        /// Parse {Logger} xml element
        /// </summary>
        /// <param name="loggerElement"></param>
        /// <param name="rulesCollection">Rules are added to this parameter.</param>
        private void ParseLoggerElement(NLogXmlElement loggerElement, IList<LoggingRule> rulesCollection)
        {
            loggerElement.AssertName("logger");

            var namePattern = loggerElement.GetOptionalAttribute("name", "*");
            var enabled = loggerElement.GetOptionalBooleanAttribute("enabled", true);
            if (!enabled)
            {
                InternalLogger.Debug("The logger named '{0}' are disabled");
                return;
            }

            var rule = new LoggingRule();
            string appendTo = loggerElement.GetOptionalAttribute("appendTo", null);
            if (appendTo == null)
            {
                appendTo = loggerElement.GetOptionalAttribute("writeTo", null);
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

            rule.Final = loggerElement.GetOptionalBooleanAttribute("final", false);

            string levelString;

            if (loggerElement.AttributeValues.TryGetValue("level", out levelString))
            {
                LogLevel level = LogLevel.FromString(levelString);
                rule.EnableLoggingForLevel(level);
            }
            else if (loggerElement.AttributeValues.TryGetValue("levels", out levelString))
            {
                levelString = CleanSpaces(levelString);

                string[] tokens = levelString.Split(',');
                foreach (string token in tokens)
                {
                    if (!string.IsNullOrEmpty(token))
                    {
                        LogLevel level = LogLevel.FromString(token);
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

                if (loggerElement.AttributeValues.TryGetValue("minLevel", out minLevelString))
                {
                    minLevel = LogLevel.FromString(minLevelString).Ordinal;
                }

                if (loggerElement.AttributeValues.TryGetValue("maxLevel", out maxLevelString))
                {
                    maxLevel = LogLevel.FromString(maxLevelString).Ordinal;
                }

                for (int i = minLevel; i <= maxLevel; ++i)
                {
                    rule.EnableLoggingForLevel(LogLevel.FromOrdinal(i));
                }
            }

            var children = loggerElement.Children.ToList();
            foreach (var child in children)
            {
                switch (child.LocalName.ToUpper(CultureInfo.InvariantCulture))
                {
                    case "FILTERS":
                        this.ParseFilters(rule, child);
                        break;

                    case "LOGGER":
                        this.ParseLoggerElement(child, rule.ChildRules);
                        break;
                }
            }

            rulesCollection.Add(rule);
        }

        private void ParseFilters(LoggingRule rule, NLogXmlElement filtersElement)
        {
            filtersElement.AssertName("filters");

            var children = filtersElement.Children.ToList();
            foreach (var filterElement in children)
            {
                string name = filterElement.LocalName;

                Filter filter = this.ConfigurationItemFactory.Filters.CreateInstance(name);
                this.ConfigureObjectFromAttributes(filter, filterElement, false);
                rule.Filters.Add(filter);
            }
        }

        private void ParseVariableElement(NLogXmlElement variableElement)
        {
            variableElement.AssertName("variable");

            string name = variableElement.GetRequiredAttribute("name");
            string value = this.ExpandSimpleVariables(variableElement.GetRequiredAttribute("value"));

            this.Variables[name] = value;
        }

        private void ParseTargetsElement(NLogXmlElement targetsElement)
        {
            targetsElement.AssertName("targets", "appenders");

            bool asyncWrap = targetsElement.GetOptionalBooleanAttribute("async", false);
            NLogXmlElement defaultWrapperElement = null;
            var typeNameToDefaultTargetParameters = new Dictionary<string, NLogXmlElement>();

            var children = targetsElement.Children.ToList();
            foreach (var targetElement in children)
            {
                string name = targetElement.LocalName;
                string typeAttributeVal = StripOptionalNamespacePrefix(targetElement.GetOptionalAttribute("type", null));

                switch (name.ToUpper(CultureInfo.InvariantCulture))
                {
                    case "DEFAULT-WRAPPER":
                        defaultWrapperElement = targetElement;
                        break;

                    case "DEFAULT-TARGET-PARAMETERS":
                        if (typeAttributeVal == null)
                        {
                            throw new NLogConfigurationException("Missing 'type' attribute on <" + name + "/>.");
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

                        Target newTarget = this.ConfigurationItemFactory.Targets.CreateInstance(typeAttributeVal);

                        NLogXmlElement defaults;
                        if (typeNameToDefaultTargetParameters.TryGetValue(typeAttributeVal, out defaults))
                        {
                            this.ParseTargetElement(newTarget, defaults);
                        }

                        this.ParseTargetElement(newTarget, targetElement);

                        if (asyncWrap)
                        {
                            newTarget = WrapWithAsyncTargetWrapper(newTarget);
                        }

                        if (defaultWrapperElement != null)
                        {
                            newTarget = this.WrapWithDefaultWrapper(newTarget, defaultWrapperElement);
                        }

                        InternalLogger.Info("Adding target {0}", newTarget);
                        AddTarget(newTarget.Name, newTarget);
                        break;
                }
            }
        }

        private void ParseTargetElement(Target target, NLogXmlElement targetElement)
        {
            var compound = target as CompoundTargetBase;
            var wrapper = target as WrapperTargetBase;

            this.ConfigureObjectFromAttributes(target, targetElement, true);

            var children = targetElement.Children.ToList();
            foreach (var childElement in children)
            {
                string name = childElement.LocalName;

                if (compound != null)
                {
                    if (IsTargetRefElement(name))
                    {
                        string targetName = childElement.GetRequiredAttribute("name");
                        Target newTarget = this.FindTargetByName(targetName);
                        if (newTarget == null)
                        {
                            throw new NLogConfigurationException("Referenced target '" + targetName + "' not found.");
                        }

                        compound.Targets.Add(newTarget);
                        continue;
                    }

                    if (IsTargetElement(name))
                    {
                        string type = StripOptionalNamespacePrefix(childElement.GetRequiredAttribute("type"));

                        Target newTarget = this.ConfigurationItemFactory.Targets.CreateInstance(type);
                        if (newTarget != null)
                        {
                            this.ParseTargetElement(newTarget, childElement);
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
                    if (IsTargetRefElement(name))
                    {
                        string targetName = childElement.GetRequiredAttribute("name");
                        Target newTarget = this.FindTargetByName(targetName);
                        if (newTarget == null)
                        {
                            throw new NLogConfigurationException("Referenced target '" + targetName + "' not found.");
                        }

                        wrapper.WrappedTarget = newTarget;
                        continue;
                    }

                    if (IsTargetElement(name))
                    {
                        string type = StripOptionalNamespacePrefix(childElement.GetRequiredAttribute("type"));

                        Target newTarget = this.ConfigurationItemFactory.Targets.CreateInstance(type);
                        if (newTarget != null)
                        {
                            this.ParseTargetElement(newTarget, childElement);
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

                this.SetPropertyFromElement(target, childElement);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Reflection.Assembly.LoadFrom", Justification = "Need to load external assembly.")]
        private void ParseExtensionsElement(NLogXmlElement extensionsElement, string baseDirectory)
        {
            extensionsElement.AssertName("extensions");

            var addElements = extensionsElement.Elements("add").ToList();
            foreach (var addElement in addElements)
            {
                string prefix = addElement.GetOptionalAttribute("prefix", null);

                if (prefix != null)
                {
                    prefix = prefix + ".";
                }

                string type = StripOptionalNamespacePrefix(addElement.GetOptionalAttribute("type", null));
                if (type != null)
                {
                    this.ConfigurationItemFactory.RegisterType(Type.GetType(type, true), prefix);
                }

                string assemblyFile = addElement.GetOptionalAttribute("assemblyFile", null);
                if (assemblyFile != null)
                {
                    try
                    {
#if SILVERLIGHT && !WINDOWS_PHONE
                                var si = Application.GetResourceStream(new Uri(assemblyFile, UriKind.Relative));
                                var assemblyPart = new AssemblyPart();
                                Assembly asm = assemblyPart.Load(si.Stream);
#else

                        string fullFileName = Path.Combine(baseDirectory, assemblyFile);
                        InternalLogger.Info("Loading assembly file: {0}", fullFileName);

                        Assembly asm = Assembly.LoadFrom(fullFileName);
#endif
                        this.ConfigurationItemFactory.RegisterItemsFromAssembly(asm, prefix);
                    }
                    catch (Exception exception)
                    {
                        if (exception.MustBeRethrownImmediately())
                        {
                            throw;
                        }

                        InternalLogger.Error(exception, "Error loading extensions.");

                        if (exception.MustBeRethrown())
                        {
                            throw new NLogConfigurationException("Error loading extensions: " + assemblyFile, exception);
                        }
                    }

                    continue;
                }

                string assemblyName = addElement.GetOptionalAttribute("assembly", null);
                if (assemblyName != null)
                {
                    try
                    {
                        InternalLogger.Info("Loading assembly name: {0}", assemblyName);
#if SILVERLIGHT && !WINDOWS_PHONE
                        var si = Application.GetResourceStream(new Uri(assemblyName + ".dll", UriKind.Relative));
                        var assemblyPart = new AssemblyPart();
                        Assembly asm = assemblyPart.Load(si.Stream);
#else
                        Assembly asm = Assembly.Load(assemblyName);
#endif

                        this.ConfigurationItemFactory.RegisterItemsFromAssembly(asm, prefix);
                    }
                    catch (Exception exception)
                    {

                        if (exception.MustBeRethrownImmediately())
                        {
                            throw;
                        }

                        InternalLogger.Error(exception, "Error loading extensions.");

                        if (exception.MustBeRethrown())
                        {
                            throw new NLogConfigurationException("Error loading extensions: " + assemblyName, exception);
                        }
                    }
                }
            }
        }

        private void ParseIncludeElement(NLogXmlElement includeElement, string baseDirectory, bool autoReloadDefault)
        {
            includeElement.AssertName("include");

            string newFileName = includeElement.GetRequiredAttribute("file");

            try
            {
                newFileName = this.ExpandSimpleVariables(newFileName);
                newFileName = SimpleLayout.Evaluate(newFileName);
                if (baseDirectory != null)
                {
                    newFileName = Path.Combine(baseDirectory, newFileName);
                }

#if SILVERLIGHT
                newFileName = newFileName.Replace("\\", "/");
                if (Application.GetResourceStream(new Uri(newFileName, UriKind.Relative)) != null)
#else
                if (File.Exists(newFileName))
#endif
                {
                    InternalLogger.Debug("Including file '{0}'", newFileName);
                    this.ConfigureFromFile(newFileName, autoReloadDefault);
                }
                else
                {
                    throw new FileNotFoundException("Included file not found: " + newFileName);
                }
            }
            catch (Exception exception)
            {
                InternalLogger.Error(exception, "Error when including '{0}'.", newFileName);

                if (exception.MustBeRethrown())
                {
                    throw;
                }

                if (includeElement.GetOptionalBooleanAttribute("ignoreErrors", false))
                {
                    return;
                }

                throw new NLogConfigurationException("Error when including: " + newFileName, exception);
            }
        }

        private void ParseTimeElement(NLogXmlElement timeElement)
        {
            timeElement.AssertName("time");

            string type = timeElement.GetRequiredAttribute("type");

            TimeSource newTimeSource = this.ConfigurationItemFactory.TimeSources.CreateInstance(type);

            this.ConfigureObjectFromAttributes(newTimeSource, timeElement, true);

            InternalLogger.Info("Selecting time source {0}", newTimeSource);
            TimeSource.Current = newTimeSource;
        }

        #endregion

        private static string GetFileLookupKey(string fileName)
        {

#if SILVERLIGHT
            // file names are relative to XAP
            return fileName;
#else
            return Path.GetFullPath(fileName);
#endif
        }

        private void SetPropertyFromElement(object o, NLogXmlElement element)
        {
            if (this.AddArrayItemFromElement(o, element))
            {
                return;
            }

            if (this.SetLayoutFromElement(o, element))
            {
                return;
            }

            PropertyHelper.SetPropertyFromString(o, element.LocalName, this.ExpandSimpleVariables(element.Value), this.ConfigurationItemFactory);
        }

        private bool AddArrayItemFromElement(object o, NLogXmlElement element)
        {
            string name = element.LocalName;

            PropertyInfo propInfo;
            if (!PropertyHelper.TryGetPropertyInfo(o, name, out propInfo))
            {
                return false;
            }

            Type elementType = PropertyHelper.GetArrayItemType(propInfo);
            if (elementType != null)
            {
                IList propertyValue = (IList)propInfo.GetValue(o, null);
                object arrayItem = FactoryHelper.CreateInstance(elementType);
                this.ConfigureObjectFromAttributes(arrayItem, element, true);
                this.ConfigureObjectFromElement(arrayItem, element);
                propertyValue.Add(arrayItem);
                return true;
            }

            return false;
        }

        private void ConfigureObjectFromAttributes(object targetObject, NLogXmlElement element, bool ignoreType)
        {
            var attributeValues = element.AttributeValues.ToList();
            foreach (var kvp in attributeValues)
            {
                string childName = kvp.Key;
                string childValue = kvp.Value;

                if (ignoreType && childName.Equals("type", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                PropertyHelper.SetPropertyFromString(targetObject, childName, this.ExpandSimpleVariables(childValue), this.ConfigurationItemFactory);
            }
        }

        private bool SetLayoutFromElement(object o, NLogXmlElement layoutElement)
        {
            PropertyInfo targetPropertyInfo;
            string name = layoutElement.LocalName;

            // if property exists
            if (PropertyHelper.TryGetPropertyInfo(o, name, out targetPropertyInfo))
            {
                // and is a Layout
                if (typeof(Layout).IsAssignableFrom(targetPropertyInfo.PropertyType))
                {
                    string layoutTypeName = StripOptionalNamespacePrefix(layoutElement.GetOptionalAttribute("type", null));

                    // and 'type' attribute has been specified
                    if (layoutTypeName != null)
                    {
                        // configure it from current element
                        Layout layout = this.ConfigurationItemFactory.Layouts.CreateInstance(this.ExpandSimpleVariables(layoutTypeName));
                        this.ConfigureObjectFromAttributes(layout, layoutElement, true);
                        this.ConfigureObjectFromElement(layout, layoutElement);
                        targetPropertyInfo.SetValue(o, layout, null);
                        return true;
                    }
                }
            }

            return false;
        }

        private void ConfigureObjectFromElement(object targetObject, NLogXmlElement element)
        {
            var children = element.Children.ToList();
            foreach (var child in children)
            {
                this.SetPropertyFromElement(targetObject, child);
            }
        }

        private Target WrapWithDefaultWrapper(Target t, NLogXmlElement defaultParameters)
        {
            string wrapperType = StripOptionalNamespacePrefix(defaultParameters.GetRequiredAttribute("type"));

            Target wrapperTargetInstance = this.ConfigurationItemFactory.Targets.CreateInstance(wrapperType);
            WrapperTargetBase wtb = wrapperTargetInstance as WrapperTargetBase;
            if (wtb == null)
            {
                throw new NLogConfigurationException("Target type specified on <default-wrapper /> is not a wrapper.");
            }

            this.ParseTargetElement(wrapperTargetInstance, defaultParameters);
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

        /// <summary>
        /// Replace a simple variable with a value. The orginal value is removed and thus we cannot redo this in a later stage.
        /// 
        /// Use for that: <see cref="VariableLayoutRenderer"/>
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private string ExpandSimpleVariables(string input)
        {
            string output = input;

            // TODO - make this case-insensitive, will probably require a different approach
            var variables = this.Variables.ToList();
            foreach (var kvp in variables)
            {
                var layout = kvp.Value;
                //this value is set from xml and that's a string. Because of that, we can use SimpleLayout here.

                if (layout != null) output = output.Replace("${" + kvp.Key + "}", layout.OriginalText);
            }

            return output;
        }
    }
}
