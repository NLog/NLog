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
    using System.Security;
    using System.Xml;
    using NLog.Common;
    using NLog.Internal;
    using NLog.Internal.Fakeables;
    using NLog.Targets;

#if SILVERLIGHT
    using System.Windows;
#endif

    /// <summary>
    /// Enables loading of NLog configuration from a file
    /// </summary>
    internal class LoggingConfigurationFileLoader : ILoggingConfigurationLoader
    {
        private static readonly AppEnvironmentWrapper DefaultAppEnvironment = new AppEnvironmentWrapper();
        private readonly IAppEnvironment _appEnvironment;

        public LoggingConfigurationFileLoader()
            :this(DefaultAppEnvironment)
        {
        }

        public LoggingConfigurationFileLoader(IAppEnvironment appEnvironment)
        {
            _appEnvironment = appEnvironment;
        }

        public LoggingConfiguration Load(LogFactory logFactory, string filename)
        {
            var configFile = GetConfigFile(filename);
            return LoadXmlLoggingConfigurationFile(logFactory, configFile);
        }

        public virtual LoggingConfiguration Load(LogFactory logFactory)
        {
            return TryLoadFromFilePaths(logFactory);
        }

        public virtual void Activated(LogFactory logFactory, LoggingConfiguration config)
        {
            // Nothing to do
        }

        internal string GetConfigFile(string configFile)
        {
            if (FilePathLayout.DetectFilePathKind(configFile) == FilePathKind.Relative)
            {
                foreach (var path in GetDefaultCandidateConfigFilePaths(configFile))
                {
                    if (_appEnvironment.FileExists(path))
                    {
                        configFile = path;
                        break;
                    }
                }
            }

            return configFile;
        }

#if __ANDROID__
        private LoggingConfiguration TryLoadFromAndroidAssets(LogFactory logFactory)
        {
            //try nlog.config in assets folder
            const string nlogConfigFilename = "NLog.config";
            try
            {
                using (var stream = Android.App.Application.Context.Assets.Open(nlogConfigFilename))
                {
                    if (stream != null)
                    {
                        InternalLogger.Debug("Loading config from Assets {0}", nlogConfigFilename);
                        using (var xmlReader = XmlReader.Create(stream))
                        {
                            return LoadXmlLoggingConfiguration(xmlReader, null, logFactory);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                InternalLogger.Trace(e, "no {0} in assets folder", nlogConfigFilename);
            }
            return null;
        }
#endif

        private LoggingConfiguration TryLoadFromFilePaths(LogFactory logFactory)
        {
            var configFileNames = logFactory.GetCandidateConfigFilePaths();
            foreach (string configFile in configFileNames)
            {
                if (TryLoadLoggingConfiguration(logFactory, configFile, out var config))
                    return config;
            }

#if __ANDROID__
            return TryLoadFromAndroidAssets(logFactory);
#else
            return null;
#endif
        }

        private bool TryLoadLoggingConfiguration(LogFactory logFactory, string configFile, out LoggingConfiguration config)
        {
            try
            {
#if SILVERLIGHT && !WINDOWS_PHONE
                Uri configFileUri = new Uri(configFile, UriKind.Relative);
                var streamResourceInfo = Application.GetResourceStream(configFileUri);
                if (streamResourceInfo != null)
                {
                    InternalLogger.Debug("Loading config from Resource {0}", configFileUri);
                    using (var xmlReader = XmlReader.Create(streamResourceInfo.Stream))
                    {
                        config = LoadXmlLoggingConfiguration(xmlReader, null, logFactory);
                        return true;
                    }
                }
#else
                if (_appEnvironment.FileExists(configFile))
                {
                    config = LoadXmlLoggingConfigurationFile(logFactory, configFile);
                    return true;    // File exists, and maybe the config is valid, stop search
                }
#endif
            }
            catch (IOException ex)
            {
                InternalLogger.Warn(ex, "Skipping invalid config file location: {0}", configFile);
            }
            catch (UnauthorizedAccessException ex)
            {
                InternalLogger.Warn(ex, "Skipping inaccessible config file location: {0}", configFile);
            }
            catch (SecurityException ex)
            {
                InternalLogger.Warn(ex, "Skipping inaccessible config file location: {0}", configFile);
            }
            catch (Exception ex)
            {
                InternalLogger.Error(ex, "Failed loading from config file location: {0}", configFile);
                if (ex.MustBeRethrown())
                    throw;
            }

            config = null;
            return false;   // No valid file found
        }

        private LoggingConfiguration LoadXmlLoggingConfigurationFile(LogFactory logFactory, string configFile)
        {
            InternalLogger.Debug("Loading config from {0}", configFile);
            var xmlConfig = new XmlLoggingConfiguration(configFile, logFactory);

            //problem: XmlLoggingConfiguration.Initialize eats exception with invalid XML. ALso XmlLoggingConfiguration.Reload never returns null.
            //therefor we check the InitializeSucceeded property.
            if (xmlConfig.InitializeSucceeded != true)
            {
                InternalLogger.Warn("Failed loading config from {0}. Invalid XML?", configFile);
            }
            return xmlConfig;
        }
        
        /// <inheritdoc/>
        public IEnumerable<string> GetDefaultCandidateConfigFilePaths()
        {
            return GetDefaultCandidateConfigFilePaths(null);
        }

        /// <summary>
        /// Get default file paths (including filename) for possible NLog config files. 
        /// </summary>
        public IEnumerable<string> GetDefaultCandidateConfigFilePaths(string fileName)
        {
            // NLog.config from application directory
            string nlogConfigFile = fileName ?? "NLog.config";
            string baseDirectory = PathHelpers.TrimDirectorySeparators(_appEnvironment.AppDomainBaseDirectory);
            if (!string.IsNullOrEmpty(baseDirectory))
                yield return Path.Combine(baseDirectory, nlogConfigFile);

            string nLogConfigFileLowerCase = nlogConfigFile.ToLower();
            bool platformFileSystemCaseInsensitive = nlogConfigFile == nLogConfigFileLowerCase || PlatformDetector.IsWin32;
            if (!platformFileSystemCaseInsensitive && !string.IsNullOrEmpty(baseDirectory))
                yield return Path.Combine(baseDirectory, nLogConfigFileLowerCase);

#if !SILVERLIGHT && !NETSTANDARD1_3
            string entryAssemblyLocation = PathHelpers.TrimDirectorySeparators(_appEnvironment.EntryAssemblyLocation);
#else
            string entryAssemblyLocation = string.Empty;
#endif
            if (!string.IsNullOrEmpty(entryAssemblyLocation) && !string.Equals(entryAssemblyLocation, baseDirectory, StringComparison.OrdinalIgnoreCase))
            {
                yield return Path.Combine(entryAssemblyLocation, nlogConfigFile);
                if (!platformFileSystemCaseInsensitive)
                    yield return Path.Combine(entryAssemblyLocation, nLogConfigFileLowerCase);
            }

            if (string.IsNullOrEmpty(baseDirectory))
            {
                yield return nlogConfigFile;
                if (!platformFileSystemCaseInsensitive)
                    yield return nLogConfigFileLowerCase;
            }

            if (fileName == null)
            {
                // Scan for process specific nlog-files
                foreach (var filePath in GetAppSpecificNLogLocations(entryAssemblyLocation))
                    yield return filePath;
            }

            foreach (var filePath in GetPrivateBinPathNLogLocations(baseDirectory, nlogConfigFile, platformFileSystemCaseInsensitive ? nLogConfigFileLowerCase : string.Empty))
                yield return filePath;

#if !SILVERLIGHT && !NETSTANDARD1_0
            if (fileName == null)
            {
                // Get path to NLog.dll.nlog only if the assembly is not in the GAC
                var nlogAssembly = typeof(LogFactory).GetAssembly();
                if (!string.IsNullOrEmpty(nlogAssembly?.Location) && !nlogAssembly.GlobalAssemblyCache)
                {
                    yield return nlogAssembly.Location + ".nlog";
                }
            }
#endif
        }

        /// <summary>
        /// Get default file paths (including filename) for possible NLog config files. 
        /// </summary>
        public IEnumerable<string> GetAppSpecificNLogLocations(string entryAssemblyLocation)
        {
            // Current config file with .config renamed to .nlog
            string configurationFile = _appEnvironment.AppDomainConfigurationFile;
            if (!StringHelpers.IsNullOrWhiteSpace(configurationFile))
            {
                yield return Path.ChangeExtension(configurationFile, ".nlog");

                // .nlog file based on the non-vshost version of the current config file
                const string vshostSubStr = ".vshost.";
                if (configurationFile.Contains(vshostSubStr))
                {
                    yield return Path.ChangeExtension(configurationFile.Replace(vshostSubStr, "."), ".nlog");
                }
            }
#if NETSTANDARD && !NETSTANDARD1_3
            else
            {
                string processFilePath = _appEnvironment.CurrentProcessFilePath;
                string processDirectory = !string.IsNullOrEmpty(processFilePath) ? PathHelpers.TrimDirectorySeparators(Path.GetDirectoryName(processFilePath)) : string.Empty;
                if (!string.IsNullOrEmpty(entryAssemblyLocation) && !string.Equals(entryAssemblyLocation, processDirectory, StringComparison.OrdinalIgnoreCase))
                {
                    // Handle dotnet-process loading .NET Core-assembly, or IIS-process loading website
                    string assemblyFileName = _appEnvironment.EntryAssemblyFileName;
                    yield return Path.Combine(entryAssemblyLocation, assemblyFileName + ".nlog");
                    // Handle unpublished .NET Core Application
                    assemblyFileName = Path.GetFileNameWithoutExtension(assemblyFileName);
                    if (!string.IsNullOrEmpty(assemblyFileName))
                        yield return Path.Combine(entryAssemblyLocation, assemblyFileName + ".exe.nlog");
                }
                else if (!string.IsNullOrEmpty(processFilePath))
                {
                    yield return processFilePath + ".nlog";
                    // Handle published .NET Core Application with assembly-nlog-file
                    if (!string.IsNullOrEmpty(entryAssemblyLocation))
                    {
                        string assemblyFileName = _appEnvironment.EntryAssemblyFileName;
                        if (!string.IsNullOrEmpty(assemblyFileName))
                            yield return Path.Combine(entryAssemblyLocation, assemblyFileName + ".nlog");
                    }
                    else
                    {
                        string processFileName = Path.GetFileNameWithoutExtension(processFilePath);
                        if (!string.IsNullOrEmpty(processFileName))
                            yield return Path.Combine(processDirectory, processFileName + ".dll.nlog");
                    }
                }
            }
#endif
        }

        public IEnumerable<string> GetPrivateBinPathNLogLocations(string baseDirectory, string nlogConfigFile, string nLogConfigFileLowerCase)
        {
            IEnumerable<string> privateBinPaths = _appEnvironment.PrivateBinPath;
            if (privateBinPaths != null)
            {
                foreach (var privatePath in privateBinPaths)
                {
                    var path = PathHelpers.TrimDirectorySeparators(privatePath);
                    if (!StringHelpers.IsNullOrWhiteSpace(path) && !string.Equals(path, baseDirectory, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return Path.Combine(path, nlogConfigFile);
                        if (!string.IsNullOrEmpty(nLogConfigFileLowerCase))
                            yield return Path.Combine(path, nLogConfigFileLowerCase);
                    }
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            // Nothing to dispose
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
