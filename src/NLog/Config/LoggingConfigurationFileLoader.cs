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
    using System.Security;
    using System.Xml;
    using NLog.Common;
    using NLog.Internal;
    using NLog.Internal.Fakeables;
    using NLog.Targets;

    /// <summary>
    /// Enables loading of NLog configuration from a file
    /// </summary>
    internal class LoggingConfigurationFileLoader : ILoggingConfigurationLoader
    {
        private readonly IAppEnvironment _appEnvironment;

        public LoggingConfigurationFileLoader(IAppEnvironment appEnvironment)
        {
            _appEnvironment = appEnvironment;
        }

        public virtual LoggingConfiguration Load(LogFactory logFactory, string filename = null)
        {
            if (string.IsNullOrEmpty(filename) || FilePathLayout.DetectFilePathKind(filename) == FilePathKind.Relative)
            {
                return TryLoadFromFilePaths(logFactory, filename);
            }
            else if (TryLoadLoggingConfiguration(logFactory, filename, out var config))
            {
                return config;
            }

            return null;
        }

        public virtual void Activated(LogFactory logFactory, LoggingConfiguration config)
        {
            // Nothing to do
        }
        
        private LoggingConfiguration TryLoadFromFilePaths(LogFactory logFactory, string filename)
        {
            var configFileNames = logFactory.GetCandidateConfigFilePaths(filename);
            foreach (string configFile in configFileNames)
            {
                if (TryLoadLoggingConfiguration(logFactory, configFile, out var config))
                    return config;
            }

            return null;
        }

        private bool TryLoadLoggingConfiguration(LogFactory logFactory, string configFile, out LoggingConfiguration config)
        {
            try
            {
                if (_appEnvironment.FileExists(configFile))
                {
                    config = LoadXmlLoggingConfigurationFile(logFactory, configFile);
                    return true;    // File exists, and maybe the config is valid, stop search
                }
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
                if (logFactory.ThrowConfigExceptions ?? logFactory.ThrowExceptions)
                    throw;

                if (ex.MustBeRethrown())
                    throw;
            }

            config = null;
            return false;   // No valid file found
        }

        private LoggingConfiguration LoadXmlLoggingConfigurationFile(LogFactory logFactory, string configFile)
        {
            InternalLogger.Debug("Loading config from {0}", configFile);

            using (var xmlReader = _appEnvironment.LoadXmlFile(configFile))
            {
                return LoadXmlLoggingConfiguration(xmlReader, configFile, logFactory);
            }
        }

        private LoggingConfiguration LoadXmlLoggingConfiguration(XmlReader xmlReader, string configFile, LogFactory logFactory)
        {
            try
            {
                var newConfig = new XmlLoggingConfiguration(xmlReader, configFile, logFactory);
                if (newConfig.InitializeSucceeded != true)
                {
                    if (ThrowXmlConfigExceptions(configFile, xmlReader, logFactory, out var autoReload))
                    {
                        using (var xmlReaderRetry = _appEnvironment.LoadXmlFile(configFile))
                        {
                            newConfig = new XmlLoggingConfiguration(xmlReaderRetry, configFile, logFactory); // Scan again after having updated LogFactory, to throw correct exception
                        }
                    }
                    else if (autoReload && !newConfig.AutoReload)
                    {
                        return CreateEmptyDefaultConfig(configFile, logFactory, autoReload);
                    }
                }
                return newConfig;
            }
            catch (Exception ex)
            {
                if (ex.MustBeRethrownImmediately() || ex.MustBeRethrown() || (logFactory.ThrowConfigExceptions ?? logFactory.ThrowExceptions))
                    throw;
                if (ThrowXmlConfigExceptions(configFile, xmlReader, logFactory, out var autoReload))
                    throw;

                return CreateEmptyDefaultConfig(configFile, logFactory, autoReload);
            }
        }

        private static LoggingConfiguration CreateEmptyDefaultConfig(string configFile, LogFactory logFactory, bool autoReload)
        {
            return new XmlLoggingConfiguration($"<nlog autoReload='{autoReload}'></nlog>", configFile, logFactory);    // Empty default config, but monitors file
        }

        private bool ThrowXmlConfigExceptions(string configFile, XmlReader xmlReader, LogFactory logFactory, out bool autoReload)
        {
            autoReload = false;

            try
            {
                if (string.IsNullOrEmpty(configFile))
                    return false;

                var fileContent = File.ReadAllText(configFile);

                if (xmlReader.ReadState == ReadState.Error)
                {
                    // Avoid reacting to throwExceptions="true" that only exists in comments, only check when invalid xml
                    if (ScanForBooleanParameter(fileContent, "throwExceptions", true))
                    {
                        logFactory.ThrowExceptions = true;
                        return true;
                    }

                    if (ScanForBooleanParameter(fileContent, "throwConfigExceptions", true))
                    {
                        logFactory.ThrowConfigExceptions = true;
                        return true;
                    }
                }

                if (ScanForBooleanParameter(fileContent, "autoReload", true))
                {
                    autoReload = true;
                }

                return false;
            }
            catch (Exception ex)
            {
                InternalLogger.Error(ex, "Failed to scan content of config file: {0}", configFile);
                return false;
            }
        }

        private static bool ScanForBooleanParameter(string fileContent, string parameterName, bool parameterValue)
        {
            return fileContent.IndexOf($"{parameterName}=\"{parameterValue}", StringComparison.OrdinalIgnoreCase) >= 0
                || fileContent.IndexOf($"{parameterName}='{parameterValue}", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// Get default file paths (including filename) for possible NLog config files. 
        /// </summary>
        public IEnumerable<string> GetDefaultCandidateConfigFilePaths(string filename = null)
        {
            string baseDirectory = PathHelpers.TrimDirectorySeparators(_appEnvironment.AppDomainBaseDirectory);
#if !NETSTANDARD1_3
            string entryAssemblyLocation = PathHelpers.TrimDirectorySeparators(_appEnvironment.EntryAssemblyLocation);
#else
            string entryAssemblyLocation = string.Empty;
#endif
            if (filename == null)
            {
                // Scan for process specific nlog-files
                foreach (var filePath in GetAppSpecificNLogLocations(baseDirectory, entryAssemblyLocation))
                    yield return filePath;
            }

            // NLog.config from application directory
            string nlogConfigFile = filename ?? "NLog.config";
            if (!string.IsNullOrEmpty(baseDirectory))
                yield return Path.Combine(baseDirectory, nlogConfigFile);

            string nLogConfigFileLowerCase = nlogConfigFile.ToLower();
            bool platformFileSystemCaseInsensitive = nlogConfigFile == nLogConfigFileLowerCase || PlatformDetector.IsWin32;
            if (!platformFileSystemCaseInsensitive && !string.IsNullOrEmpty(baseDirectory))
                yield return Path.Combine(baseDirectory, nLogConfigFileLowerCase);

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

            foreach (var filePath in GetPrivateBinPathNLogLocations(baseDirectory, nlogConfigFile, platformFileSystemCaseInsensitive ? nLogConfigFileLowerCase : string.Empty))
                yield return filePath;

            string nlogAssemblyLocation = filename != null ? null : LookupNLogAssemblyLocation();
            if (nlogAssemblyLocation != null)
                yield return nlogAssemblyLocation + ".nlog";
        }

        private static string LookupNLogAssemblyLocation()
        {
#if !NETSTANDARD1_3 && !NETSTANDARD1_5
            // Get path to NLog.dll.nlog only if the assembly is not in the GAC
            var nlogAssembly = typeof(LogFactory).GetAssembly();
            var nlogAssemblyLocation = nlogAssembly?.Location;
            if (!string.IsNullOrEmpty(nlogAssemblyLocation) && !nlogAssembly.GlobalAssemblyCache)
            {
                return nlogAssemblyLocation;
            }
#endif
            return null;
        }

        /// <summary>
        /// Get default file paths (including filename) for possible NLog config files. 
        /// </summary>
        public IEnumerable<string> GetAppSpecificNLogLocations(string baseDirectory, string entryAssemblyLocation)
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
                if (string.IsNullOrEmpty(entryAssemblyLocation))
                    entryAssemblyLocation = baseDirectory;

                if (PathHelpers.IsTempDir(entryAssemblyLocation, _appEnvironment.UserTempFilePath))
                {
                    // Handle Single File Published on NetCore 3.1 and loading side-by-side exe.nlog (Not relevant for Net5.0)
                    string processFilePath = _appEnvironment.CurrentProcessFilePath;
                    if (!string.IsNullOrEmpty(processFilePath))
                    {
                        yield return processFilePath + ".nlog";
                    }
                }

                if (!string.IsNullOrEmpty(entryAssemblyLocation))
                {
                    string assemblyFileName = _appEnvironment.EntryAssemblyFileName;
                    if (!string.IsNullOrEmpty(assemblyFileName))
                    {
                        // Handle unpublished .NET Core Application
                        var assemblyBaseName = Path.GetFileNameWithoutExtension(assemblyFileName);
                        if (!string.IsNullOrEmpty(assemblyBaseName))
                            yield return Path.Combine(entryAssemblyLocation, assemblyBaseName + ".exe.nlog");

                        yield return Path.Combine(entryAssemblyLocation, assemblyFileName + ".nlog");
                    }
                }
            }
#endif
        }

        private IEnumerable<string> GetPrivateBinPathNLogLocations(string baseDirectory, string nlogConfigFile, string nLogConfigFileLowerCase)
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
