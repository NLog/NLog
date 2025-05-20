//
// Copyright (c) 2004-2024 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using JetBrains.Annotations;
    using NLog.Common;
    using NLog.Internal;
    using NLog.Layouts;

    /// <summary>
    /// Loads NLog LoggingConfiguration from xml-file
    /// </summary>
    /// <remarks>
    /// Make sure to update official NLog.xsd schema, when adding new config-options outside targets/layouts
    /// </remarks>
    public class XmlLoggingConfiguration : LoggingConfigurationParser
    {
        private static readonly Dictionary<LogFactory, AutoReloadConfigFileWatcher> _watchers = new Dictionary<LogFactory, AutoReloadConfigFileWatcher>();
        private readonly Dictionary<string, bool> _fileMustAutoReloadLookup = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        private string? _originalFileName;

        private readonly Stack<string> _currentFilePath = new Stack<string>();

        internal XmlLoggingConfiguration(LogFactory logFactory)
            : base(logFactory)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLoggingConfiguration" /> class.
        /// </summary>
        /// <param name="fileName">Path to the config-file to read.</param>
        public XmlLoggingConfiguration([NotNull] string fileName)
            : this(fileName, LogManager.LogFactory)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLoggingConfiguration" /> class.
        /// </summary>
        /// <param name="fileName">Path to the config-file to read.</param>
        /// <param name="logFactory">The <see cref="LogFactory" /> to which to apply any applicable configuration values.</param>
        public XmlLoggingConfiguration([NotNull] string fileName, LogFactory logFactory)
            : base(logFactory)
        {
            Guard.ThrowIfNullOrEmpty(fileName);
            LoadFromXmlFile(fileName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLoggingConfiguration" /> class.
        /// </summary>
        /// <param name="xmlSource">Configuration file to be read.</param>
        public XmlLoggingConfiguration([NotNull] TextReader xmlSource)
            : this(xmlSource, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLoggingConfiguration" /> class.
        /// </summary>
        /// <param name="xmlSource">Configuration file to be read.</param>
        /// <param name="filePath">Path to the config-file that contains the element (to be used as a base for including other files). <c>null</c> is allowed.</param>
        public XmlLoggingConfiguration([NotNull] TextReader xmlSource, string? filePath)
            : this(xmlSource, filePath, LogManager.LogFactory)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLoggingConfiguration" /> class.
        /// </summary>
        /// <param name="xmlSource">Configuration file to be read.</param>
        /// <param name="filePath">Path to the config-file that contains the element (to be used as a base for including other files). <c>null</c> is allowed.</param>
        /// <param name="logFactory">The <see cref="LogFactory" /> to which to apply any applicable configuration values.</param>
        public XmlLoggingConfiguration([NotNull] TextReader xmlSource, string? filePath, LogFactory logFactory)
            : base(logFactory)
        {
            Guard.ThrowIfNull(xmlSource);
            ParseFromTextReader(xmlSource, filePath);
        }

#if NETFRAMEWORK
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLoggingConfiguration" /> class.
        /// </summary>
        /// <param name="reader">XML reader to read from.</param>
        [Obsolete("Instead use TextReader as input. Marked obsolete with NLog 6.0")]
        public XmlLoggingConfiguration([NotNull] System.Xml.XmlReader reader)
            : this(reader, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLoggingConfiguration" /> class.
        /// </summary>
        /// <param name="reader">XmlReader containing the configuration section.</param>
        /// <param name="fileName">Path to the config-file that contains the element (to be used as a base for including other files). <c>null</c> is allowed.</param>
        [Obsolete("Instead use TextReader as input. Marked obsolete with NLog 6.0")]
        public XmlLoggingConfiguration([NotNull] System.Xml.XmlReader reader, [CanBeNull] string? fileName)
            : this(reader, fileName, LogManager.LogFactory)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLoggingConfiguration" /> class.
        /// </summary>
        /// <param name="reader">XmlReader containing the configuration section.</param>
        /// <param name="fileName">Path to the config-file that contains the element (to be used as a base for including other files). <c>null</c> is allowed.</param>
        /// <param name="logFactory">The <see cref="LogFactory" /> to which to apply any applicable configuration values.</param>
        [Obsolete("Instead use TextReader as input. Marked obsolete with NLog 6.0")]
        public XmlLoggingConfiguration([NotNull] System.Xml.XmlReader reader, [CanBeNull] string? fileName, LogFactory logFactory)
            : base(logFactory)
        {
            Guard.ThrowIfNull(reader);
            ParseFromXmlReader(reader, fileName);
        }
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLoggingConfiguration" /> class.
        /// </summary>
        /// <param name="xmlContents">NLog configuration as XML string.</param>
        /// <param name="filePath">Path to the config-file that contains the element (to be used as a base for including other files). <c>null</c> is allowed.</param>
        /// <param name="logFactory">The <see cref="LogFactory" /> to which to apply any applicable configuration values.</param>
        internal XmlLoggingConfiguration([NotNull] string xmlContents, [CanBeNull] string filePath, LogFactory logFactory)
            : base(logFactory)
        {
            Guard.ThrowIfNullOrEmpty(xmlContents);
            LoadFromXmlContent(xmlContents, filePath);
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

        /// <summary>
        /// Gets or sets a value indicating whether any of the configuration files
        /// should be watched for changes and reloaded automatically when changed.
        /// </summary>
        public bool AutoReload
        {
            get => AutoReloadFileNames.Any();
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
        public IEnumerable<string> AutoReloadFileNames
        {
            get
            {
                if (_fileMustAutoReloadLookup.Count == 0)
                    return ArrayHelper.Empty<string>();
                else
                    return _fileMustAutoReloadLookup.Where(entry => entry.Value).Select(entry => entry.Key);
            }
        }

        /// <inheritdoc/>
        [Obsolete("Replaced by AutoReloadFileNames. Marked obsolete with NLog v6")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override IEnumerable<string> FileNamesToWatch => AutoReloadFileNames;

        /// <summary>
        /// Loads the NLog LoggingConfiguration from its original configuration file and returns the new <see cref="LoggingConfiguration" /> object.
        /// </summary>
        /// <returns>The newly loaded <see cref="XmlLoggingConfiguration" /> instance.</returns>
        /// <remarks>Must assign the returned object to LogManager.Configuration to activate it</remarks>
        public override LoggingConfiguration Reload()
        {
            var originalFileName = _originalFileName ?? string.Empty;
            if (!string.IsNullOrEmpty(originalFileName))
            {
                var newConfig = new XmlLoggingConfiguration(LogFactory);
                newConfig.PrepareForReload(this);
                newConfig.LoadFromXmlFile(originalFileName);
                return newConfig;
            }

            return base.Reload();
        }

        /// <inheritdoc />
        protected internal override void OnConfigurationAssigned(LogFactory? logFactory)
        {
            base.OnConfigurationAssigned(logFactory);

            try
            {
                var configFactory = logFactory ?? LogFactory ?? NLog.LogManager.LogFactory;

                AutoReloadConfigFileWatcher? fileWatcher = null;
                lock (_watchers)
                {
                    _watchers.TryGetValue(configFactory, out fileWatcher);
                }

                if (logFactory is null || !AutoReload)
                {
                    if (fileWatcher != null)
                    {
                        InternalLogger.Info("AutoReload Config File Monitor stopping, since no active configuration");
                        fileWatcher.Dispose();
                    }
                }
                else
                {
                    InternalLogger.Debug("AutoReload Config File Monitor refreshing after configuration changed");
                    if (fileWatcher is null || fileWatcher.IsDisposed)
                    {
                        InternalLogger.Info("AutoReload Config File Monitor starting");
                        fileWatcher = new AutoReloadConfigFileWatcher(configFactory);
                        lock (_watchers)
                        {
                            _watchers[configFactory] = fileWatcher;
                        }
                    }

                    fileWatcher.RefreshFileWatcher(AutoReloadFileNames);
                }
            }
            catch (Exception ex)
            {
                InternalLogger.Error(ex, "AutoReload Config File Monitor failed to refresh after configuration changed.");
            }
        }

        /// <summary>
        /// Obsolete and replaced by <see cref="LogManager.Setup()"/> and <see cref="SetupBuilderExtensions.LoadConfigurationFromFile(ISetupBuilder, string, bool)"/> with NLog v5.2.
        ///
        /// Get file paths (including filename) for the possible NLog config files.
        /// </summary>
        /// <returns>The file paths to the possible config file</returns>
        [Obsolete("Replaced by chaining LogManager.Setup().LoadConfigurationFromFile(). Marked obsolete on NLog 5.2")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IEnumerable<string> GetCandidateConfigFilePaths()
        {
            return LogManager.LogFactory.GetCandidateConfigFilePaths();
        }

        /// <summary>
        /// Obsolete and replaced by <see cref="LogManager.Setup()"/> and <see cref="SetupBuilderExtensions.LoadConfigurationFromFile(ISetupBuilder, string, bool)"/> with NLog v5.2.
        ///
        /// Overwrite the paths (including filename) for the possible NLog config files.
        /// </summary>
        /// <param name="filePaths">The file paths to the possible config file</param>
        [Obsolete("Replaced by chaining LogManager.Setup().LoadConfigurationFromFile(). Marked obsolete on NLog 5.2")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void SetCandidateConfigFilePaths(IEnumerable<string> filePaths)
        {
            LogManager.LogFactory.SetCandidateConfigFilePaths(filePaths);
        }

        /// <summary>
        /// Obsolete and replaced by <see cref="LogManager.Setup()"/> and <see cref="SetupBuilderExtensions.LoadConfigurationFromFile(ISetupBuilder, string, bool)"/> with NLog v5.2.
        ///
        /// Clear the candidate file paths and return to the defaults.
        /// </summary>
        [Obsolete("Replaced by chaining LogManager.Setup().LoadConfigurationFromFile(). Marked obsolete on NLog 5.2")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void ResetCandidateConfigFilePath()
        {
            LogManager.LogFactory.ResetCandidateConfigFilePath();
        }

        private void LoadFromXmlFile(string filePath)
        {
            using (var textReader = LogFactory.CurrentAppEnvironment.LoadTextFile(filePath))
            {
                ParseFromTextReader(textReader, filePath);
            }
        }

        internal void LoadFromXmlContent(string xmlContents, string filePath)
        {
            using (var stringReader = new StringReader(xmlContents))
            {
                ParseFromTextReader(stringReader, filePath);
            }
        }

#if NETFRAMEWORK
        [Obsolete("Instead use TextReader as input. Marked obsolete with NLog 6.0")]
        private void ParseFromXmlReader([NotNull] System.Xml.XmlReader reader, [CanBeNull] string? filePath)
        {
            try
            {
                _originalFileName = (filePath is null || StringHelpers.IsNullOrWhiteSpace(filePath)) ? null : GetFileLookupKey(filePath);
                reader.MoveToContent();
                var content = new XmlLoggingConfigurationElement(reader);
                if (!string.IsNullOrEmpty(_originalFileName))
                {
                    InternalLogger.Info("Loading NLog config from XML file: {0}", _originalFileName);
                    ParseTopLevel(content, filePath, autoReloadDefault: false);
                }
                else
                {
                    ParseTopLevel(content, null, autoReloadDefault: false);
                }
            }
            catch (Exception exception)
            {
                var configurationException = new NLogConfigurationException($"Exception when loading configuration {filePath}", exception);
                InternalLogger.Error(exception, configurationException.Message);
                throw configurationException;
            }
        }
#endif

        private void ParseFromTextReader(TextReader textReader, string? filePath)
        {
            try
            {
                _originalFileName = (filePath is null || StringHelpers.IsNullOrWhiteSpace(filePath)) ? null : GetFileLookupKey(filePath);
                var content = new XmlParserConfigurationElement(new XmlParser(textReader).LoadDocument(out var _));
                if (!string.IsNullOrEmpty(_originalFileName))
                {
                    InternalLogger.Info("Loading NLog config from XML file: {0}", _originalFileName);
                    ParseTopLevel(content, filePath, autoReloadDefault: false);
                }
                else
                {
                    ParseTopLevel(content, null, autoReloadDefault: false);
                }
            }
            catch (Exception exception)
            {
                var configurationException = new NLogConfigurationException($"Exception when loading configuration {filePath}", exception);
                InternalLogger.Error(exception, configurationException.Message);
                throw configurationException;
            }
        }

        /// <summary>
        /// Include new file into the configuration. Check if not already included.
        /// </summary>
        private void IncludeNewConfigFile([NotNull] string filePath, bool autoReloadDefault)
        {
            if (!_fileMustAutoReloadLookup.ContainsKey(GetFileLookupKey(filePath)))
            {
                using (var textReader = LogFactory.CurrentAppEnvironment.LoadTextFile(filePath))
                {
                    var configElement = new XmlParserConfigurationElement(new XmlParser(textReader).LoadDocument(out var _), false);
                    ParseTopLevel(configElement, filePath, autoReloadDefault);
                }
            }
        }

        /// <summary>
        /// Parse the root
        /// </summary>
        /// <param name="content"></param>
        /// <param name="filePath">Path to the config-file that contains the element (to be used as a base for including other files). <c>null</c> is allowed.</param>
        /// <param name="autoReloadDefault">The default value for the autoReload option.</param>
        private void ParseTopLevel(ILoggingConfigurationElement content, [CanBeNull] string? filePath, bool autoReloadDefault)
        {
            content.AssertName("nlog", "configuration");

            switch (content.Name.ToUpperInvariant())
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
        /// <param name="filePath">Path to the config-file that contains the element (to be used as a base for including other files). <c>null</c> is allowed.</param>
        /// <param name="autoReloadDefault">The default value for the autoReload option.</param>
        private void ParseConfigurationElement(ILoggingConfigurationElement configurationElement, [CanBeNull] string? filePath, bool autoReloadDefault)
        {
            InternalLogger.Trace("ParseConfigurationElement");
            configurationElement.AssertName("configuration");

            var nlogElements = configurationElement.FilterChildren("nlog");
            foreach (var nlogElement in nlogElements)
            {
                ParseNLogElement(nlogElement, filePath, autoReloadDefault);
            }
        }

        /// <summary>
        /// Parse {NLog} xml element.
        /// </summary>
        /// <param name="nlogElement"></param>
        /// <param name="filePath">Path to the config-file that contains the element (to be used as a base for including other files). <c>null</c> is allowed.</param>
        /// <param name="autoReloadDefault">The default value for the autoReload option.</param>
        private void ParseNLogElement(ILoggingConfigurationElement nlogElement, [CanBeNull] string? filePath, bool autoReloadDefault)
        {
            InternalLogger.Trace("ParseNLogElement");
            nlogElement.AssertName("nlog");

            bool autoReload = nlogElement.GetOptionalBooleanValue("autoReload", autoReloadDefault);

            try
            {
                string? baseDirectory = null;
                if (filePath != null && !StringHelpers.IsNullOrWhiteSpace(filePath))
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
                var filePath = _currentFilePath.Peek();
                bool autoLoad = !string.IsNullOrEmpty(filePath) && _fileMustAutoReloadLookup[GetFileLookupKey(filePath)];
                ParseIncludeElement(configSection, !string.IsNullOrEmpty(filePath) ? Path.GetDirectoryName(filePath) : null, autoLoad);
                return true;
            }
            else
            {
                return base.ParseNLogSection(configSection);
            }
        }

        private void ParseIncludeElement(ILoggingConfigurationElement includeElement, string? baseDirectory, bool autoReloadDefault)
        {
            includeElement.AssertName("include");

            string newFileName = includeElement.GetRequiredValue("file", "nlog");

            var ignoreErrors = includeElement.GetOptionalBooleanValue("ignoreErrors", false);

            try
            {
                newFileName = ExpandSimpleVariables(newFileName);
                newFileName = SimpleLayout.Evaluate(newFileName, this);
                var filePath = newFileName;
                if (baseDirectory != null)
                {
                    filePath = Path.Combine(baseDirectory, newFileName);
                }

                if (File.Exists(filePath))
                {
                    InternalLogger.Debug("Including file '{0}'", filePath);
                    IncludeNewConfigFile(filePath, autoReloadDefault);
                }
                else
                {
                    //is mask?
                    if (newFileName.IndexOf('*') >= 0)
                    {
                        IncludeConfigFilesByMask(baseDirectory ?? ".", newFileName, autoReloadDefault);
                    }
                    else
                    {
                        if (ignoreErrors)
                        {
                            //quick stop for performances
                            InternalLogger.Debug("Skipping included file '{0}' as it can't be found", filePath);
                            return;
                        }

                        throw new FileNotFoundException("Included file not found: " + filePath);
                    }
                }
            }
            catch (Exception exception)
            {
                if (exception.MustBeRethrownImmediately())
                {
                    throw;
                }

                var configurationException = new NLogConfigurationException($"Error when including '{newFileName}'.", exception);
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
        private void IncludeConfigFilesByMask(string baseDirectory, string fileMask, bool autoReloadDefault)
        {
            var directory = baseDirectory;

            //if absolute, split to file mask and directory.
            if (Path.IsPathRooted(fileMask))
            {
                directory = Path.GetDirectoryName(fileMask);
                if (directory is null)
                {
                    InternalLogger.Warn("directory is empty for include of '{0}'", fileMask);
                    return;
                }

                var filename = Path.GetFileName(fileMask);
                if (filename is null)
                {
                    InternalLogger.Warn("filename is empty for include of '{0}'", fileMask);
                    return;
                }

                fileMask = filename;
            }

            var files = Directory.GetFiles(directory, fileMask);
            foreach (var filePath in files)
            {
                //note we exclude our self in ConfigureFromFile
                IncludeNewConfigFile(filePath, autoReloadDefault);
            }
        }

        private static string GetFileLookupKey(string fileName)
        {
            return Path.GetFullPath(fileName);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{base.ToString()}, FilePath={_originalFileName}";
        }

        private sealed class AutoReloadConfigFileWatcher : IDisposable
        {
            private readonly LogFactory _logFactory;
            private readonly MultiFileWatcher _fileWatcher = new MultiFileWatcher();
            private readonly object _lockObject = new object();
            private Timer? _reloadTimer;
            private bool _isDisposing;

            internal bool IsDisposed => _isDisposing;

            public AutoReloadConfigFileWatcher(LogFactory logFactory)
            {
                _logFactory = logFactory;
                _fileWatcher.FileChanged += FileWatcher_FileChanged;
            }

            private void FileWatcher_FileChanged(object sender, System.IO.FileSystemEventArgs e)
            {
                lock (_lockObject)
                {
                    if (_isDisposing)
                        return;

                    var reloadTimer = _reloadTimer;
                    if (reloadTimer is null)
                    {
                        var currentConfig = _logFactory.Configuration;
                        if (currentConfig is null)
                            return;

                        _reloadTimer = new Timer((s) => ReloadTimer(s), currentConfig, 1000, Timeout.Infinite);
                    }
                    else
                    {
                        reloadTimer.Change(1000, Timeout.Infinite);
                    }
                }
            }

            private void ReloadTimer(object state)
            {
                if (_isDisposing)
                {
                    return; //timer was disposed already.
                }

                LoggingConfiguration oldConfig = (LoggingConfiguration)state;

                InternalLogger.Info("AutoReload Config File Monitor reloading configuration...");

                lock (_lockObject)
                {
                    if (_isDisposing)
                    {
                        return; //timer was disposed already.
                    }

                    var currentTimer = _reloadTimer;
                    if (currentTimer != null)
                    {
                        _reloadTimer = null;
                        currentTimer.Dispose();
                    }
                }

                LoggingConfiguration? newConfig = null;

                try
                {
                    var currentConfig = _logFactory.Configuration;
                    if (!ReferenceEquals(currentConfig, oldConfig))
                    {
                        InternalLogger.Debug("AutoReload Config File Monitor skipping reload, since existing NLog config has changed.");
                        return;
                    }

                    newConfig = oldConfig.Reload();
                    if (newConfig is null || ReferenceEquals(newConfig, oldConfig))
                    {
                        InternalLogger.Debug("AutoReload Config File Monitor skipping reload, since new configuration has not changed.");
                        return;
                    }

                    currentConfig = _logFactory.Configuration;
                    if (!ReferenceEquals(currentConfig, oldConfig))
                    {
                        InternalLogger.Debug("AutoReload Config File Monitor skipping reload, since existing NLog config has changed.");
                        return;
                    }
                }
                catch (Exception exception)
                {
                    InternalLogger.Warn(exception, "AutoReload Config File Monitor failed to reload NLog LoggingConfiguration.");
                    return;
                }

                try
                {
                    TryUnwatchConfigFile();
                    _logFactory.Configuration = newConfig;
                }
                catch (Exception exception)
                {
                    InternalLogger.Warn(exception, "AutoReload Config File Monitor failed to activate new NLog LoggingConfiguration.");
                    _fileWatcher.Watch((oldConfig as XmlLoggingConfiguration)?.AutoReloadFileNames ?? ArrayHelper.Empty<string>());

                }
            }

            public void RefreshFileWatcher(IEnumerable<string> fileNamesToWatch)
            {
                _fileWatcher.Watch(fileNamesToWatch);
            }

            public void Dispose()
            {
                _isDisposing = true;
                _fileWatcher.FileChanged -= FileWatcher_FileChanged;
                lock (_lockObject)
                {
                    var reloadTimer = _reloadTimer;
                    _reloadTimer = null;
                    reloadTimer?.Dispose();
                }
                _fileWatcher.Dispose();
            }

            private void TryUnwatchConfigFile()
            {
                try
                {
                    _fileWatcher?.StopWatching();
                }
                catch (Exception exception)
                {
                    InternalLogger.Warn(exception, "AutoReload Config File Monitor failed to stop file watcher.");

                    if (LogManager.ThrowExceptions || _logFactory.ThrowExceptions)
                    {
                        throw;
                    }
                }
            }
        }
    }
}
