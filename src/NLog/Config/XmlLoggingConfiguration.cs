// 
// Copyright (c) 2004-2019 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using NLog.Common;
    using NLog.Internal;
    using NLog.Layouts;
    using JetBrains.Annotations;
#if SILVERLIGHT
// ReSharper disable once RedundantUsingDirective
    using System.Windows;
#endif

    /// <summary>
    /// A class for configuring NLog through an XML configuration file 
    /// (App.config style or App.nlog style).
    /// 
    /// Parsing of the XML file is also implemented in this class.
    /// </summary>
    ///<remarks>
    /// - This class is thread-safe.<c>.ToList()</c> is used for that purpose.
    /// - Update TemplateXSD.xml for changes outside targets
    /// </remarks>
    public class XmlLoggingConfiguration : LoggingConfigurationParser
    {
#if __ANDROID__

        /// <summary>
        /// Prefix for assets in Xamarin Android
        /// </summary>
        private const string AssetsPrefix = "assets/";
#endif


        //TODO: Replace with List<IXmlConfigurationSource> _currentConfigurations...
        private readonly HashSet<IXmlConfigurationSource> _configMustAutoReloadLookup = new HashSet<IXmlConfigurationSource>();

        private IXmlConfigurationSource _rootConfigurationSource;

        private readonly Stack<IXmlConfigurationSource> _currentConfigurationSource = new Stack<IXmlConfigurationSource>();

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
            : base(logFactory)
        {
            Initialize(new XmlFileConfigurationSource(fileName), ignoreErrors);
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
            : base(logFactory)
        {
            Initialize(new XmlReaderConfigurationSource(reader, fileName), ignoreErrors);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLoggingConfiguration" /> class.
        /// </summary>
        /// <param name="configurationSource">An already established <see cref="IXmlConfigurationSource"/> containing the location of the configuration data.</param>
        /// <param name="ignoreErrors">Ignore any errors during configuration.</param>
        internal XmlLoggingConfiguration(IXmlConfigurationSource configurationSource, bool ignoreErrors)
            : base(LogManager.LogFactory)
        {
            Initialize(configurationSource, ignoreErrors);
        }

#if !SILVERLIGHT
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLoggingConfiguration" /> class.
        /// </summary>
        /// <param name="element">The XML element.</param>
        /// <param name="fileName">Name of the XML file.</param>
        internal XmlLoggingConfiguration(System.Xml.XmlElement element, string fileName)
            : this(element, fileName, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLoggingConfiguration" /> class.
        /// </summary>
        /// <param name="element">The XML element.</param>
        /// <param name="fileName">Name of the XML file.</param>
        /// <param name="ignoreErrors">If set to <c>true</c> errors will be ignored during file processing.</param>
        internal XmlLoggingConfiguration(System.Xml.XmlElement element, string fileName, bool ignoreErrors)
            : this(element.OuterXml, fileName, ignoreErrors)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLoggingConfiguration" /> class.
        /// </summary>
        /// <param name="xmlContents">The XML contents.</param>
        /// <param name="fileName">Name of the XML file.</param>
        /// <param name="ignoreErrors">If set to <c>true</c> errors will be ignored during file processing.</param>
        internal XmlLoggingConfiguration(string xmlContents, string fileName, bool ignoreErrors)
            : this(xmlContents, fileName, ignoreErrors, LogManager.LogFactory)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLoggingConfiguration" /> class.
        /// </summary>
        /// <param name="xmlContents">The XML contents.</param>
        /// <param name="fileName">Name of the XML file.</param>
        /// <param name="ignoreErrors">If set to <c>true</c> errors will be ignored during file processing.</param>
        /// <param name="logFactory">The <see cref="LogFactory" /> to which to apply any applicable configuration values.</param>
        internal XmlLoggingConfiguration(string xmlContents, string fileName, bool ignoreErrors, LogFactory logFactory)
            : base(logFactory)
        {
            Initialize(new XmlStringConfigurationSource(xmlContents, fileName), ignoreErrors);
        }

        /// <summary>
        /// Parse XML string as NLog configuration
        /// </summary>
        /// <param name="xml">NLog configuration</param>
        /// <returns></returns>
        public static XmlLoggingConfiguration CreateFromXmlString(string xml)
        {
            return new XmlLoggingConfiguration(xml, null, false);
        }
        /// <summary>
        /// Parse XML string as NLog configuration
        /// </summary>
        /// <param name="xml">NLog configuration</param>
        /// <param name="logFactory">The <see cref="LogFactory" /> to which to apply any applicable configuration values.</param>
        /// <returns></returns>
        public static XmlLoggingConfiguration CreateFromXmlString(string xml, LogFactory logFactory)
        {
            return new XmlLoggingConfiguration(xml, null, false, logFactory);
        }
#endif

#if !SILVERLIGHT && !__IOS__ && !__ANDROID__ && !NETSTANDARD
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
        //TODO: Modify setter to update IXmlConfigurationSource.AutoReload
        public bool AutoReload
        {
            get
            {
                if (_configMustAutoReloadLookup.Count == 0)
                    return false;
                else
                    return _configMustAutoReloadLookup.All(x => x.AutoReload);
            }
            set
            {
                foreach (var nextFile in _configMustAutoReloadLookup)
                    nextFile.AutoReload = value;
            }
        }

        /// <summary>
        /// Gets the collection of file names which should be watched for changes by NLog.
        /// This is the list of configuration files processed.
        /// If the <c>autoReload</c> attribute is not set it returns empty collection.
        /// </summary>
        //TODO: Will be moved to XmlConfigurationSource in future commit...
        public override IEnumerable<string> FileNamesToWatch
        {
            get
            {
                return _configMustAutoReloadLookup.Where(config => config.AutoReload).Select(entry => entry.SourcePath);
            }
        }

        /// <summary>
        /// Re-reads the original configuration file and returns the new <see cref="LoggingConfiguration" /> object.
        /// </summary>
        /// <returns>The new <see cref="XmlLoggingConfiguration" /> object.</returns>
        public override LoggingConfiguration Reload()
        {
            if (_rootConfigurationSource != null)
                return new XmlLoggingConfiguration(_rootConfigurationSource, false);
            else
                return base.Reload();
        }

        /// <summary>
        /// Get file paths (including filename) for the possible NLog config files. 
        /// </summary>
        /// <returns>The filepaths to the possible config file</returns>
        //TODO: Determine if this is moved or not.  It is a public static and tested method. THis will be a breaking change.
        public static IEnumerable<string> GetCandidateConfigFilePaths()
        {
            return LogManager.LogFactory.GetCandidateConfigFilePaths();
        }

        /// <summary>
        /// Overwrite the paths (including filename) for the possible NLog config files.
        /// </summary>
        /// <param name="filePaths">The filepaths to the possible config file</param>
        //TODO: Determine if this is moved or not.  It is a public static and tested method. THis will be a breaking change.
        public static void SetCandidateConfigFilePaths(IEnumerable<string> filePaths)
        {
            LogManager.LogFactory.SetCandidateConfigFilePaths(filePaths);
        }

        /// <summary>
        /// Clear the candidate file paths and return to the defaults.
        /// </summary>
        //TODO: Determine if this is moved or not.  It is a public static and tested method. THis will be a breaking change.
        public static void ResetCandidateConfigFilePath()
        {
            LogManager.LogFactory.ResetCandidateConfigFilePath();
        }

        /// <summary>
        /// Initializes the configuration.
        /// </summary>
        /// <param name="source">The configuration source.</param>
        /// <param name="ignoreErrors">Ignore any errors during configuration.</param>
        private void Initialize([NotNull] IXmlConfigurationSource source, bool ignoreErrors)
        {
            try
            {
                InitializeSucceeded = null;
                if (_rootConfigurationSource == null)
                    _rootConfigurationSource = source;

                using (var reader = source.GetReader())
                {
                    reader.MoveToContent();
                    var content = new NLogXmlElement(reader);

                    if (!string.IsNullOrEmpty(source.SourcePath))
                        InternalLogger.Info("Configuring from an XML element in {0}...", source.SourcePath);
						
                    ParseTopLevel(content, source);

                    InitializeSucceeded = true;
                    CheckParsingErrors(content);
                }

                base.CheckUnusedTargets();
            }
            catch (Exception exception)
            {
                InitializeSucceeded = false;
                if (exception.MustBeRethrownImmediately())
                {
                    throw;
                }

                var configurationException = new NLogConfigurationException(exception, "Exception when parsing {0}. ", source.SourcePath);
                InternalLogger.Error(configurationException, "Parsing configuration from {0} failed.", source.SourcePath);

                if (!ignoreErrors && configurationException.MustBeRethrown())
                {
                    throw configurationException;
                }
            }
        }

        /// <summary>
        /// Checks whether any error during XML configuration parsing has occured.
        /// If there are any and <c>ThrowConfigExceptions</c> or <c>ThrowExceptions</c>
        /// setting is enabled - throws <c>NLogConfigurationException</c>, otherwise
        /// just write an internal log at Warn level.
        /// </summary>
        /// <param name="rootContentElement">Root NLog configuration xml element</param>
        private void CheckParsingErrors(NLogXmlElement rootContentElement)
        {
            var parsingErrors = rootContentElement.GetParsingErrors().ToArray();
            if (parsingErrors.Any())
            {
                if (LogManager.ThrowConfigExceptions ?? LogManager.ThrowExceptions)
                {
                    string exceptionMessage = string.Join(Environment.NewLine, parsingErrors);
                    throw new NLogConfigurationException(exceptionMessage);
                }
                else
                {
                    foreach (var parsingError in parsingErrors)
                    {
                        InternalLogger.Log(LogLevel.Warn, parsingError);
                    }
                }
            }
        }

        /// <summary>
        /// Parse the root
        /// </summary>
        /// <param name="content"></param>
        /// <param name="config">source to locate the configuration data.</param>
        private void ParseTopLevel(NLogXmlElement content, IXmlConfigurationSource config)
        {
            content.AssertName("nlog", "configuration");

            switch (content.LocalName.ToUpperInvariant())
            {
                case "CONFIGURATION":
                    ParseConfigurationElement(content, config);
                    break;

                case "NLOG":
                    ParseNLogElement(content, config);
                    break;
            }
        }

        /// <summary>
        /// Parse {configuration} xml element.
        /// </summary>
        /// <param name="configurationElement"></param>
        /// <param name="config">source to locate the configuration data.</param>
        private void ParseConfigurationElement(NLogXmlElement configurationElement, IXmlConfigurationSource config)
        {
            InternalLogger.Trace("ParseConfigurationElement");
            configurationElement.AssertName("configuration");

            var nlogElements = configurationElement.Elements("nlog").ToList();
            foreach (var nlogElement in nlogElements)
            {
                ParseNLogElement(nlogElement, config);
            }
        }

        /// <summary>
        /// Parse {NLog} xml element.
        /// </summary>
        /// <param name="nlogElement"></param>
        /// <param name="config">source to locate the configuration data.</param>
        private void ParseNLogElement(ILoggingConfigurationElement nlogElement, IXmlConfigurationSource config)
        {
            InternalLogger.Trace("ParseNLogElement");
            nlogElement.AssertName("nlog");

            config.AutoReload = nlogElement.GetOptionalBooleanValue("autoReload", config.AutoReload);
            _configMustAutoReloadLookup.Add(config);
			
            try
            {
                _currentConfigurationSource.Push(config);
                base.LoadConfig(nlogElement, config.LocalFolder);
            }
            finally
            {
                _currentConfigurationSource.Pop();
            }
        }

        /// <summary>
        /// Parses a single config section within the NLog-config
        /// </summary>
        /// <param name="configSection"></param>
        /// <returns>Section was recognized</returns>
        protected override bool ParseNLogSection(ILoggingConfigurationElement configSection)
        {
            if (configSection.MatchesName("include"))
            {
                var parentConfiguration = _currentConfigurationSource.Peek();
                ParseIncludeElement(configSection, parentConfiguration);
                return true;
            }
            else
            {
                return base.ParseNLogSection(configSection);
            }
        }

        private void ParseIncludeElement(ILoggingConfigurationElement includeElement, IXmlConfigurationSource parentConfiguration)
        {
            includeElement.AssertName("include");


            var ignoreErrors = includeElement.GetOptionalBooleanValue("ignoreErrors", false);


            var includedConfigurations = Enumerable.Empty<IXmlConfigurationSource>();
            var includedFileName = includeElement.GetOptionalValue("file", null);
            if (!string.IsNullOrEmpty(includedFileName))
            {
                //TODO: Maybe move this?  The only reason I left these two lines here is because the expansion is done in the base class...
                includedFileName = ExpandSimpleVariables(includedFileName);
                includedFileName = SimpleLayout.Evaluate(includedFileName);

                var configurationsToAdd = XmlFileConfigurationSource.IncludeFromPath(includedFileName, parentConfiguration, ignoreErrors);
                if(configurationsToAdd != null)
                    includedConfigurations = includedConfigurations.Union(configurationsToAdd);
            }

            //var includedUri = includeElement.GetOptionalValue("uri", null);
            //if (!string.IsNullOrEmpty(includedUri))
            //{
            //    includedUri = ExpandSimpleVariables(includedUri);
            //    includedUri = SimpleLayout.Evaluate(includedUri);
            //    includedConfigurations = includedConfigurations.Union(XmlRemoteConfigurationSource.IncludeFromUri(includedUri, parentConfiguration, ignoreErrors));
            //}

            foreach (var configuration in includedConfigurations)
            {
                TryLoadConfigurationFromSource(configuration, ignoreErrors);
            }
        }

        private void TryLoadConfigurationFromSource(IXmlConfigurationSource configuration, bool ignoreErrors)
        {
            if (_configMustAutoReloadLookup.Contains(configuration))
                return;

            try
            {
                using (var reader = configuration.GetReader())
                {
                    reader.MoveToContent();
                    var content = new NLogXmlElement(reader);

                    ParseTopLevel(content, configuration);
                }
            }
            catch (Exception exception)
            {
                InternalLogger.Error(exception, "Error when including '{0}'.", configuration.SourcePath);

                if (ignoreErrors)
                {
                    return;
                }

                if (exception.MustBeRethrown())
                {
                    throw;
                }

                throw new NLogConfigurationException("Error when including: " + configuration.SourcePath, exception);
            }
        }
    }
}
