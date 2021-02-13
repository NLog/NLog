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
    public class XmlLoggingConfiguration : LoggingConfigurationParser, IInitializeSucceeded
    {
        private readonly Dictionary<string, bool> _fileMustAutoReloadLookup = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        private string _originalFileName;

        private readonly Stack<string> _currentFilePath = new Stack<string>();

        internal XmlLoggingConfiguration(LogFactory logFactory)
            : base(logFactory)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLoggingConfiguration" /> class.
        /// </summary>
        /// <param name="fileName">Configuration file to be read.</param>
        public XmlLoggingConfiguration([NotNull] string fileName)
            : this(fileName, LogManager.LogFactory)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLoggingConfiguration" /> class.
        /// </summary>
        /// <param name="fileName">Configuration file to be read.</param>
        /// <param name="logFactory">The <see cref="LogFactory" /> to which to apply any applicable configuration values.</param>
        public XmlLoggingConfiguration([NotNull] string fileName, LogFactory logFactory)
            : base(logFactory)
        {
            LoadFromXmlFile(fileName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLoggingConfiguration" /> class.
        /// </summary>
        /// <param name="fileName">Configuration file to be read.</param>
        /// <param name="ignoreErrors">Ignore any errors during configuration.</param>
        [Obsolete("Constructor with parameter ignoreErrors has limited effect. Instead use LogManager.ThrowConfigExceptions. Marked obsolete in NLog 4.7")]
        public XmlLoggingConfiguration([NotNull] string fileName, bool ignoreErrors)
            : this(fileName, ignoreErrors, LogManager.LogFactory)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLoggingConfiguration" /> class.
        /// </summary>
        /// <param name="fileName">Configuration file to be read.</param>
        /// <param name="ignoreErrors">Ignore any errors during configuration.</param>
        /// <param name="logFactory">The <see cref="LogFactory" /> to which to apply any applicable configuration values.</param>
        [Obsolete("Constructor with parameter ignoreErrors has limited effect. Instead use LogManager.ThrowConfigExceptions. Marked obsolete in NLog 4.7")]
        public XmlLoggingConfiguration([NotNull] string fileName, bool ignoreErrors, LogFactory logFactory)
            : base(logFactory)
        {
            using (XmlReader reader = CreateFileReader(fileName))
            {
                Initialize(reader, fileName, ignoreErrors);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLoggingConfiguration" /> class.
        /// </summary>
        /// <param name="reader">XML reader to read from.</param>
        public XmlLoggingConfiguration([NotNull] XmlReader reader)
            : this(reader, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLoggingConfiguration" /> class.
        /// </summary>
        /// <param name="reader"><see cref="XmlReader"/> containing the configuration section.</param>
        /// <param name="fileName">Name of the file that contains the element (to be used as a base for including other files). <c>null</c> is allowed.</param>
        public XmlLoggingConfiguration([NotNull] XmlReader reader, [CanBeNull] string fileName)
            : this(reader, fileName, LogManager.LogFactory)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLoggingConfiguration" /> class.
        /// </summary>
        /// <param name="reader"><see cref="XmlReader"/> containing the configuration section.</param>
        /// <param name="fileName">Name of the file that contains the element (to be used as a base for including other files). <c>null</c> is allowed.</param>
        /// <param name="logFactory">The <see cref="LogFactory" /> to which to apply any applicable configuration values.</param>
        public XmlLoggingConfiguration([NotNull] XmlReader reader, [CanBeNull] string fileName, LogFactory logFactory)
            : base(logFactory)
        {
            Initialize(reader, fileName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLoggingConfiguration" /> class.
        /// </summary>
        /// <param name="reader"><see cref="XmlReader"/> containing the configuration section.</param>
        /// <param name="fileName">Name of the file that contains the element (to be used as a base for including other files). <c>null</c> is allowed.</param>
        /// <param name="ignoreErrors">Ignore any errors during configuration.</param>
        [Obsolete("Constructor with parameter ignoreErrors has limited effect. Instead use LogManager.ThrowConfigExceptions. Marked obsolete in NLog 4.7")]
        public XmlLoggingConfiguration([NotNull] XmlReader reader, [CanBeNull] string fileName, bool ignoreErrors)
            : this(reader, fileName, ignoreErrors, LogManager.LogFactory)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLoggingConfiguration" /> class.
        /// </summary>
        /// <param name="reader"><see cref="XmlReader"/> containing the configuration section.</param>
        /// <param name="fileName">Name of the file that contains the element (to be used as a base for including other files). <c>null</c> is allowed.</param>
        /// <param name="ignoreErrors">Ignore any errors during configuration.</param>
        /// <param name="logFactory">The <see cref="LogFactory" /> to which to apply any applicable configuration values.</param>
        [Obsolete("Constructor with parameter ignoreErrors has limited effect. Instead use LogManager.ThrowConfigExceptions. Marked obsolete in NLog 4.7")]
        public XmlLoggingConfiguration([NotNull] XmlReader reader, [CanBeNull] string fileName, bool ignoreErrors, LogFactory logFactory)
            : base(logFactory)
        {
            Initialize(reader, fileName, ignoreErrors);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLoggingConfiguration" /> class.
        /// </summary>
        /// <param name="xmlContents">NLog configuration as XML string.</param>
        /// <param name="fileName">Name of the XML file.</param>
        /// <param name="logFactory">The <see cref="LogFactory" /> to which to apply any applicable configuration values.</param>
        internal XmlLoggingConfiguration([NotNull] string xmlContents, [CanBeNull] string fileName, LogFactory logFactory)
            : base(logFactory)
        {
            LoadFromXmlContent(xmlContents, fileName);
        }

        /// <summary>
        /// Parse XML string as NLog configuration
        /// </summary>
        /// <param name="xml">NLog configuration in XML to be parsed</param>
        public static XmlLoggingConfiguration CreateFromXmlString(string xml)
        {
            return CreateFromXmlString(xml, LogManager.LogFactory);
        }

        /// <summary>
        /// Parse XML string as NLog configuration
        /// </summary>
        /// <param name="xml">NLog configuration in XML to be parsed</param>
        /// <param name="logFactory">NLog LogFactory</param>
        public static XmlLoggingConfiguration CreateFromXmlString(string xml, LogFactory logFactory)
        {
            return new XmlLoggingConfiguration(xml, string.Empty, logFactory);
        }

#if !NETSTANDARD
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
                if (_fileMustAutoReloadLookup.Count == 0)
                    return false;
                else
                    return _fileMustAutoReloadLookup.Values.All(mustAutoReload => mustAutoReload);
            }
            set
            {
                var autoReloadFiles = _fileMustAutoReloadLookup.Keys.ToList();
                foreach (string nextFile in autoReloadFiles)
                    _fileMustAutoReloadLookup[nextFile] = value;
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
                return _fileMustAutoReloadLookup.Where(entry => entry.Value).Select(entry => entry.Key);
            }
        }

        /// <summary>
        /// Re-reads the original configuration file and returns the new <see cref="LoggingConfiguration" /> object.
        /// </summary>
        /// <returns>The new <see cref="XmlLoggingConfiguration" /> object.</returns>
        public override LoggingConfiguration Reload()
        {
            if (!string.IsNullOrEmpty(_originalFileName))
            {
                var newConfig = new XmlLoggingConfiguration(LogFactory);
                newConfig.PrepareForReload(this);
                newConfig.LoadFromXmlFile(_originalFileName);
                return newConfig;
            }

            return base.Reload();
        }

        /// <summary>
        /// Get file paths (including filename) for the possible NLog config files. 
        /// </summary>
        /// <returns>The file paths to the possible config file</returns>
        public static IEnumerable<string> GetCandidateConfigFilePaths()
        {
            return LogManager.LogFactory.GetCandidateConfigFilePaths();
        }

        /// <summary>
        /// Overwrite the paths (including filename) for the possible NLog config files.
        /// </summary>
        /// <param name="filePaths">The file paths to the possible config file</param>
        public static void SetCandidateConfigFilePaths(IEnumerable<string> filePaths)
        {
            LogManager.LogFactory.SetCandidateConfigFilePaths(filePaths);
        }

        /// <summary>
        /// Clear the candidate file paths and return to the defaults.
        /// </summary>
        public static void ResetCandidateConfigFilePath()
        {
            LogManager.LogFactory.ResetCandidateConfigFilePath();
        }

        private void LoadFromXmlFile(string fileName)
        {
            using (XmlReader reader = CreateFileReader(fileName))
            {
                Initialize(reader, fileName);
            }
        }

        internal void LoadFromXmlContent(string xmlContent, string fileName)
        {
            using (var stringReader = new StringReader(xmlContent))
            {
                using (XmlReader reader = XmlReader.Create(stringReader))
                {
                    Initialize(reader, fileName);
                }
            }
        }

        /// <summary>
        /// Create XML reader for (xml config) file.
        /// </summary>
        /// <param name="fileName">filepath</param>
        /// <returns>reader or <c>null</c> if filename is empty.</returns>
        private XmlReader CreateFileReader(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                fileName = fileName.Trim();
                return LogFactory.CurrentAppEnvironment.LoadXmlFile(fileName);
            }
            return null;
        }

        /// <summary>
        /// Initializes the configuration.
        /// </summary>
        /// <param name="reader"><see cref="XmlReader"/> containing the configuration section.</param>
        /// <param name="fileName">Name of the file that contains the element (to be used as a base for including other files). <c>null</c> is allowed.</param>
        /// <param name="ignoreErrors">Ignore any errors during configuration.</param>
        private void Initialize([NotNull] XmlReader reader, [CanBeNull] string fileName, bool ignoreErrors = false)
        {
            try
            {
                InitializeSucceeded = null;
                _originalFileName = fileName;
                reader.MoveToContent();
                var content = new NLogXmlElement(reader);
                if (!string.IsNullOrEmpty(fileName))
                {
                    InternalLogger.Info("Configuring from an XML element in {0}...", fileName);
                    ParseTopLevel(content, fileName, autoReloadDefault: false);
                }
                else
                {
                    ParseTopLevel(content, null, autoReloadDefault: false);
                }
                InitializeSucceeded = true;
            }
            catch (Exception exception)
            {
                InitializeSucceeded = false;

                if (exception.MustBeRethrownImmediately())
                {
                    throw;
                }

                var configurationException = new NLogConfigurationException(exception, "Exception when loading configuration {0}", fileName);
                InternalLogger.Error(exception, configurationException.Message);
                if (!ignoreErrors && (LogFactory.ThrowConfigExceptions ?? LogFactory.ThrowExceptions || configurationException.MustBeRethrown()))
                    throw configurationException;
            }
        }

        /// <summary>
        /// Add a file with configuration. Check if not already included.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="autoReloadDefault"></param>
        private void ConfigureFromFile([NotNull] string fileName, bool autoReloadDefault)
        {
            if (!_fileMustAutoReloadLookup.ContainsKey(GetFileLookupKey(fileName)))
            {
                using (var reader = LogFactory.CurrentAppEnvironment.LoadXmlFile(fileName))
                {
                    reader.MoveToContent();
                    ParseTopLevel(new NLogXmlElement(reader, true), fileName, autoReloadDefault);
                }
            }
        }

        /// <summary>
        /// Parse the root
        /// </summary>
        /// <param name="content"></param>
        /// <param name="filePath">path to config file.</param>
        /// <param name="autoReloadDefault">The default value for the autoReload option.</param>
        private void ParseTopLevel(NLogXmlElement content, [CanBeNull] string filePath, bool autoReloadDefault)
        {
            content.AssertName("nlog", "configuration");

            switch (content.LocalName.ToUpperInvariant())
            {
                case "CONFIGURATION":
                    ParseConfigurationElement(content, filePath, autoReloadDefault);
                    break;

                case "NLOG":
                    ParseNLogElement(content, filePath, autoReloadDefault);
                    break;
            }
        }

        /// <summary>
        /// Parse {configuration} xml element.
        /// </summary>
        /// <param name="configurationElement"></param>
        /// <param name="filePath">path to config file.</param>
        /// <param name="autoReloadDefault">The default value for the autoReload option.</param>
        private void ParseConfigurationElement(NLogXmlElement configurationElement, [CanBeNull] string filePath, bool autoReloadDefault)
        {
            InternalLogger.Trace("ParseConfigurationElement");
            configurationElement.AssertName("configuration");

            var nlogElements = configurationElement.Elements("nlog").ToList();
            foreach (var nlogElement in nlogElements)
            {
                ParseNLogElement(nlogElement, filePath, autoReloadDefault);
            }
        }

        /// <summary>
        /// Parse {NLog} xml element.
        /// </summary>
        /// <param name="nlogElement"></param>
        /// <param name="filePath">path to config file.</param>
        /// <param name="autoReloadDefault">The default value for the autoReload option.</param>
        private void ParseNLogElement(ILoggingConfigurationElement nlogElement, [CanBeNull] string filePath, bool autoReloadDefault)
        {
            InternalLogger.Trace("ParseNLogElement");
            nlogElement.AssertName("nlog");

            bool autoReload = nlogElement.GetOptionalBooleanValue("autoReload", autoReloadDefault);

            try
            {
                string baseDirectory = null;
                if (!string.IsNullOrEmpty(filePath))
                {
                    _fileMustAutoReloadLookup[GetFileLookupKey(filePath)] = autoReload;
                    _currentFilePath.Push(filePath);
                    baseDirectory = Path.GetDirectoryName(filePath);
                }
                base.LoadConfig(nlogElement, baseDirectory);
            }
            finally
            {
                if (!string.IsNullOrEmpty(filePath))
                    _currentFilePath.Pop();
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
                string filePath = _currentFilePath.Peek();
                bool autoLoad = !string.IsNullOrEmpty(filePath) && _fileMustAutoReloadLookup[GetFileLookupKey(filePath)];
                ParseIncludeElement(configSection, !string.IsNullOrEmpty(filePath) ? Path.GetDirectoryName(filePath) : null, autoLoad);
                return true;
            }
            else
            {
                return base.ParseNLogSection(configSection);
            }
        }

        private void ParseIncludeElement(ILoggingConfigurationElement includeElement, string baseDirectory, bool autoReloadDefault)
        {
            includeElement.AssertName("include");

            string newFileName = includeElement.GetRequiredValue("file", "nlog");

            var ignoreErrors = includeElement.GetOptionalBooleanValue("ignoreErrors", false);

            try
            {
                newFileName = ExpandSimpleVariables(newFileName);
                newFileName = SimpleLayout.Evaluate(newFileName);
                var fullNewFileName = newFileName;
                if (baseDirectory != null)
                {
                    fullNewFileName = Path.Combine(baseDirectory, newFileName);
                }

                if (File.Exists(fullNewFileName))
                {
                    InternalLogger.Debug("Including file '{0}'", fullNewFileName);
                    ConfigureFromFile(fullNewFileName, autoReloadDefault);
                }
                else
                {
                    //is mask?

                    if (newFileName.Contains("*"))
                    {
                        ConfigureFromFilesByMask(baseDirectory, newFileName, autoReloadDefault);
                    }
                    else
                    {
                        if (ignoreErrors)
                        {
                            //quick stop for performances
                            InternalLogger.Debug("Skipping included file '{0}' as it can't be found", fullNewFileName);
                            return;
                        }

                        throw new FileNotFoundException("Included file not found: " + fullNewFileName);
                    }
                }
            }
            catch (Exception exception)
            {
                if (exception.MustBeRethrownImmediately())
                {
                    throw;
                }

                var configurationException = new NLogConfigurationException(exception, "Error when including '{0}'.", newFileName);
                InternalLogger.Error(exception, configurationException.Message);
                if (!ignoreErrors)
                    throw configurationException;
            }
        }

        /// <summary>
        /// Include (multiple) files by filemask, e.g. *.nlog
        /// </summary>
        /// <param name="baseDirectory">base directory in case if <paramref name="fileMask"/> is relative</param>
        /// <param name="fileMask">relative or absolute fileMask</param>
        /// <param name="autoReloadDefault"></param>
        private void ConfigureFromFilesByMask(string baseDirectory, string fileMask, bool autoReloadDefault)
        {
            var directory = baseDirectory;

            //if absolute, split to file mask and directory.
            if (Path.IsPathRooted(fileMask))
            {
                directory = Path.GetDirectoryName(fileMask);
                if (directory == null)
                {
                    InternalLogger.Warn("directory is empty for include of '{0}'", fileMask);
                    return;
                }

                var filename = Path.GetFileName(fileMask);

                if (filename == null)
                {
                    InternalLogger.Warn("filename is empty for include of '{0}'", fileMask);
                    return;
                }
                fileMask = filename;
            }

            var files = Directory.GetFiles(directory, fileMask);
            foreach (var file in files)
            {
                //note we exclude our self in ConfigureFromFile
                ConfigureFromFile(file, autoReloadDefault);
            }
        }

        private static string GetFileLookupKey([NotNull] string fileName)
        {
            return Path.GetFullPath(fileName);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{base.ToString()}, FilePath={_originalFileName}";
        }
    }
}
