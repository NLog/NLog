// 
// Copyright (c) 2004-2018 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using NLog.Common;
    using NLog.Internal;
    using NLog.Internal.Fakeables;
    using NLog.Targets;

#if SILVERLIGHT
    using System.Windows;
#endif

    internal class LoggingConfigurationFileLoader : ILoggingConfigurationLoader
    {
        private static readonly FileWrapper DefaultFileWrapper = new FileWrapper();
        private readonly IFile _file;

        public LoggingConfigurationFileLoader()
            :this(DefaultFileWrapper)
        {
        }

        public LoggingConfigurationFileLoader(IFile file)
        {
            _file = file;
        }

        public LoggingConfiguration Load(LogFactory logFactory, string filename)
        {
            var configFile = GetConfigFile(filename);
            return LoadXmlLoggingConfiguration(logFactory, configFile);
        }

        public virtual LoggingConfiguration Load(LogFactory logFactory)
        {
            return TryLoadFromFilePaths(logFactory);
        }

        internal string GetConfigFile(string configFile)
        {
            if (FilePathLayout.DetectFilePathKind(configFile) == FilePathKind.Relative)
            {
                foreach (var path in GetDefaultCandidateConfigFilePaths(configFile))
                {
                    if (_file.Exists(path))
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
                        return LoadXmlLoggingConfiguration(logFactory, XmlLoggingConfiguration.AssetsPrefix + nlogConfigFilename);
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
                if (Application.GetResourceStream(configFileUri) != null)
                {
                    config = LoadXmlLoggingConfiguration(logFactory, configFile);
                    return true;    // File exists, and maybe the config is valid, stop search
                }
#else
                if (File.Exists(configFile))
                {
                    config = LoadXmlLoggingConfiguration(logFactory, configFile);
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

        private LoggingConfiguration LoadXmlLoggingConfiguration(LogFactory logFactory, string configFile)
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

        /// <summary>
        /// Get default file paths (including filename) for possible NLog config files. 
        /// </summary>
        public IEnumerable<string> GetDefaultCandidateConfigFilePaths(string fileName = null)
        {
            // NLog.config from application directory
            string nlogConfigFile = fileName ?? "NLog.config";
            string baseDirectory = PathHelpers.TrimDirectorySeparators(LogFactory.CurrentAppDomain?.BaseDirectory);
            if (!string.IsNullOrEmpty(baseDirectory))
                yield return Path.Combine(baseDirectory, nlogConfigFile);

            string nLogConfigFileLowerCase = nlogConfigFile.ToLower();
            bool platformFileSystemCaseInsensitive = nlogConfigFile == nLogConfigFileLowerCase || PlatformDetector.IsWin32;
            if (!platformFileSystemCaseInsensitive && !string.IsNullOrEmpty(baseDirectory))
                yield return Path.Combine(baseDirectory, nLogConfigFileLowerCase);

#if !SILVERLIGHT && !NETSTANDARD1_3
            var entryAssemblyLocation = PathHelpers.TrimDirectorySeparators(AssemblyHelpers.GetAssemblyFileLocation(System.Reflection.Assembly.GetEntryAssembly()));
            if (!string.IsNullOrEmpty(entryAssemblyLocation))
            {
                if (!string.Equals(entryAssemblyLocation, baseDirectory, StringComparison.OrdinalIgnoreCase))
                {
                    yield return Path.Combine(entryAssemblyLocation, nlogConfigFile);
                    if (!platformFileSystemCaseInsensitive)
                        yield return Path.Combine(entryAssemblyLocation, nLogConfigFileLowerCase);
                }
            }
#endif

            if (string.IsNullOrEmpty(baseDirectory))
            {
                yield return nlogConfigFile;
                if (!platformFileSystemCaseInsensitive)
                    yield return nLogConfigFileLowerCase;
            }

            if (fileName == null)
            {
                // Current config file with .config renamed to .nlog
                string configurationFile = LogFactory.CurrentAppDomain?.ConfigurationFile;
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
            }

            IEnumerable<string> privateBinPaths = LogFactory.CurrentAppDomain.PrivateBinPath;
            if (privateBinPaths != null)
            {
                foreach (var privatePath in privateBinPaths)
                {
                    var path = PathHelpers.TrimDirectorySeparators(privatePath);
                    if (!StringHelpers.IsNullOrWhiteSpace(path) && !string.Equals(path, baseDirectory, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return Path.Combine(path, nlogConfigFile);
                        if (!platformFileSystemCaseInsensitive)
                            yield return Path.Combine(path, nLogConfigFileLowerCase);
                    }
                }
            }

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

        protected virtual void Dispose(bool disposing)
        {
            // Nothing to dispose
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
