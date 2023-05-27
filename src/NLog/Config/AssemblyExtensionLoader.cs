// 
// Copyright (c) 2004-2021 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using System.Linq;
    using System.Reflection;
    using NLog.Common;
    using NLog.Internal;

    [Obsolete("Instead use RegisterType<T>, as dynamic Assembly loading will be moved out. Marked obsolete with NLog v5.2")]
    internal class AssemblyExtensionLoader : IAssemblyExtensionLoader
    {
        [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming - Allow extension loading from config", "IL2072")]
        public void LoadTypeFromName(ConfigurationItemFactory factory, string typeName, string itemNamePrefix)
        {
            var configType = PropertyTypeConverter.ConvertToType(typeName, true);
            factory.RegisterType(configType, itemNamePrefix);
        }

        public void LoadAssemblyFromName(ConfigurationItemFactory factory, string assemblyName, string itemNamePrefix)
        {
            var loadedAssemblies = new HashSet<Assembly>();
            foreach (var itemType in factory.ItemTypes)
            {
                loadedAssemblies.Add(itemType.GetAssembly());
            }

            if (loadedAssemblies.Count > 1)
            {
                foreach (var assembly in loadedAssemblies)
                {
                    if (string.Equals(assembly.GetName().Name, assemblyName, StringComparison.OrdinalIgnoreCase))
                    {
                        InternalLogger.Debug("Skipped Auto loading assembly name: {0}", assemblyName);
                        return;
                    }
                }
            }

            InternalLogger.Info("Auto loading assembly name: {0}", assemblyName);
            Assembly extensionAssembly = LoadAssemblyFromName(assemblyName);
            InternalLogger.LogAssemblyVersion(extensionAssembly);
            factory.RegisterItemsFromAssembly(extensionAssembly, itemNamePrefix);
            InternalLogger.Info("Auto loading assembly name: {0} succeeded!", assemblyName);
        }

        public void LoadAssemblyFromPath(ConfigurationItemFactory factory, string assemblyPath, string baseDirectory, string itemNamePrefix)
        {
#if !NETSTANDARD1_3
            InternalLogger.Info("Auto loading assembly file: {0}", assemblyPath);
            var extensionAssembly = LoadAssemblyFromPath(assemblyPath, baseDirectory);
            InternalLogger.LogAssemblyVersion(extensionAssembly);
            factory.RegisterItemsFromAssembly(extensionAssembly, itemNamePrefix);
            InternalLogger.Info("Auto loading assembly file: {0} succeeded!", assemblyPath);
#else
            // Nothing to do for Sonar Cube
#endif
        }

        public void ScanForAutoLoadExtensions(ConfigurationItemFactory factory)
        {
#if !NETSTANDARD1_3
            try
            {
                var nlogAssembly = typeof(LogFactory).GetAssembly();
                var assemblyLocation = string.Empty;
                var extensionDlls = ArrayHelper.Empty<string>();
                var fileLocations = GetAutoLoadingFileLocations();
                foreach (var fileLocation in fileLocations)
                {
                    if (string.IsNullOrEmpty(fileLocation.Key))
                        continue;

                    if (string.IsNullOrEmpty(assemblyLocation))
                        assemblyLocation = fileLocation.Key;

                    extensionDlls = GetNLogExtensionFiles(fileLocation.Key);
                    if (extensionDlls.Length > 0)
                    {
                        assemblyLocation = fileLocation.Key;
                        break;
                    }
                }

                InternalLogger.Debug("Start auto loading, location: {0}", assemblyLocation);
                var alreadyRegistered = LoadNLogExtensionAssemblies(factory, nlogAssembly, extensionDlls);
                RegisterAppDomainAssemblies(factory, nlogAssembly, alreadyRegistered);
            }
            catch (System.Security.SecurityException ex)
            {
                InternalLogger.Warn(ex, "Seems that we do not have permission");
                if (ex.MustBeRethrown())
                {
                    throw;
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                InternalLogger.Warn(ex, "Seems that we do not have permission");
                if (ex.MustBeRethrown())
                {
                    throw;
                }
            }
            InternalLogger.Debug("Auto loading done");
#else
            // Nothing to do for Sonar Cube
#endif
        }

#if !NETSTANDARD1_3
        private static HashSet<string> LoadNLogExtensionAssemblies(ConfigurationItemFactory factory, Assembly nlogAssembly, string[] extensionDlls)
        {
            HashSet<string> alreadyRegistered = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    nlogAssembly.FullName
                };

            foreach (var extensionDll in extensionDlls)
            {
                InternalLogger.Info("Auto loading assembly file: {0}", extensionDll);

                try
                {
                    var extensionAssembly = LoadAssemblyFromPath(extensionDll);
                    InternalLogger.LogAssemblyVersion(extensionAssembly);
                    factory.RegisterItemsFromAssembly(extensionAssembly);
                    alreadyRegistered.Add(extensionAssembly.FullName);

                    InternalLogger.Info("Auto loading assembly file: {0} succeeded!", extensionDll);
                }
                catch (Exception ex)
                {
                    if (ex.MustBeRethrownImmediately())
                    {
                        throw;
                    }

                    InternalLogger.Warn(ex, "Auto loading assembly file: {0} failed! Skipping this file.", extensionDll);
                    if (ex.MustBeRethrown())
                    {
                        throw;
                    }
                }
            }

            return alreadyRegistered;
        }

        private static void RegisterAppDomainAssemblies(ConfigurationItemFactory factory, Assembly nlogAssembly, HashSet<string> alreadyRegistered)
        {
            alreadyRegistered.Add(nlogAssembly.FullName);

#if !NETSTANDARD1_5
            var allAssemblies = LogFactory.DefaultAppEnvironment.GetAppDomainRuntimeAssemblies();
#else
            var allAssemblies = new [] { nlogAssembly };
#endif
            foreach (var assembly in allAssemblies)
            {
                if (assembly.FullName.StartsWith("NLog.", StringComparison.OrdinalIgnoreCase) && !alreadyRegistered.Contains(assembly.FullName))
                {
                    factory.RegisterItemsFromAssembly(assembly);
                }

                if (IncludeAsHiddenAssembly(assembly.FullName))
                {
                    LogManager.AddHiddenAssembly(assembly);
                }
            }
        }

        private static bool IncludeAsHiddenAssembly(string assemblyFullName)
        {
            if (assemblyFullName.StartsWith("NLog.Extensions.Logging,", StringComparison.OrdinalIgnoreCase))
                return true;

            if (assemblyFullName.StartsWith("NLog.Web,", StringComparison.OrdinalIgnoreCase))
                return true;

            if (assemblyFullName.StartsWith("NLog.Web.AspNetCore,", StringComparison.OrdinalIgnoreCase))
                return true;

            if (assemblyFullName.StartsWith("Microsoft.Extensions.Logging,", StringComparison.OrdinalIgnoreCase))
                return true;

            if (assemblyFullName.StartsWith("Microsoft.Extensions.Logging.Abstractions,", StringComparison.OrdinalIgnoreCase))
                return true;

            if (assemblyFullName.StartsWith("Microsoft.Extensions.Logging.Filter,", StringComparison.OrdinalIgnoreCase))
                return true;

            if (assemblyFullName.StartsWith("Microsoft.Logging,", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        internal static IEnumerable<KeyValuePair<string, string>> GetAutoLoadingFileLocations()
        {
            var nlogAssembly = typeof(LogFactory).GetAssembly();
            var nlogAssemblyLocation = PathHelpers.TrimDirectorySeparators(AssemblyHelpers.GetAssemblyFileLocation(nlogAssembly));
            InternalLogger.Debug("Auto loading based on NLog-Assembly found location: {0}", nlogAssemblyLocation);
            if (!string.IsNullOrEmpty(nlogAssemblyLocation))
                yield return new KeyValuePair<string, string>(nlogAssemblyLocation, nameof(nlogAssemblyLocation));

            var entryAssemblyLocation = PathHelpers.TrimDirectorySeparators(LogFactory.DefaultAppEnvironment.EntryAssemblyLocation);
            InternalLogger.Debug("Auto loading based on GetEntryAssembly-Assembly found location: {0}", entryAssemblyLocation);
            if (!string.IsNullOrEmpty(entryAssemblyLocation) && !string.Equals(entryAssemblyLocation, nlogAssemblyLocation, StringComparison.OrdinalIgnoreCase))
                yield return new KeyValuePair<string, string>(entryAssemblyLocation, nameof(entryAssemblyLocation));

            var baseDirectory = PathHelpers.TrimDirectorySeparators(LogFactory.DefaultAppEnvironment.AppDomainBaseDirectory);
            InternalLogger.Debug("Auto loading based on AppDomain-BaseDirectory found location: {0}", baseDirectory);
            if (!string.IsNullOrEmpty(baseDirectory) && !string.Equals(baseDirectory, nlogAssemblyLocation, StringComparison.OrdinalIgnoreCase))
                yield return new KeyValuePair<string, string>(baseDirectory, nameof(baseDirectory));
        }

        private static string[] GetNLogExtensionFiles(string assemblyLocation)
        {
            try
            {
                InternalLogger.Debug("Search for auto loading files in location: {0}", assemblyLocation);
                if (string.IsNullOrEmpty(assemblyLocation))
                {
                    return ArrayHelper.Empty<string>();
                }

                var extensionDlls = System.IO.Directory.GetFiles(assemblyLocation, "NLog*.dll")
                .Select(System.IO.Path.GetFileName)
                .Where(x => !x.Equals("NLog.dll", StringComparison.OrdinalIgnoreCase))
                .Where(x => !x.Equals("NLog.UnitTests.dll", StringComparison.OrdinalIgnoreCase))
                .Select(x => System.IO.Path.Combine(assemblyLocation, x));
                return extensionDlls.ToArray();
            }
            catch (System.IO.DirectoryNotFoundException ex)
            {
                InternalLogger.Warn(ex, "Skipping auto loading location because assembly directory does not exist: {0}", assemblyLocation);
                if (ex.MustBeRethrown())
                {
                    throw;
                }
                return ArrayHelper.Empty<string>();
            }
            catch (System.Security.SecurityException ex)
            {
                InternalLogger.Warn(ex, "Skipping auto loading location because access not allowed to assembly directory: {0}", assemblyLocation);
                if (ex.MustBeRethrown())
                {
                    throw;
                }
                return ArrayHelper.Empty<string>();
            }
            catch (UnauthorizedAccessException ex)
            {
                InternalLogger.Warn(ex, "Skipping auto loading location because access not allowed to assembly directory: {0}", assemblyLocation);
                if (ex.MustBeRethrown())
                {
                    throw;
                }
                return ArrayHelper.Empty<string>();
            }
        }
#endif

#if !NETSTANDARD1_3
        /// <summary>
        /// Load from url
        /// </summary>
        /// <param name="assemblyFileName">file or path, including .dll</param>
        /// <param name="baseDirectory">basepath, optional</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming - Allow extension loading from config", "IL2026")]
        [Obsolete("Instead use RegisterType<T>, as dynamic Assembly loading will be moved out. Marked obsolete with NLog v5.2")]
        private static Assembly LoadAssemblyFromPath(string assemblyFileName, string baseDirectory = null)
        {
            string fullFileName = baseDirectory is null ? assemblyFileName : System.IO.Path.Combine(baseDirectory, assemblyFileName);

            InternalLogger.Info("Loading assembly file: {0}", fullFileName);
#if NETSTANDARD1_5
            try
            {
                var assemblyName = System.Runtime.Loader.AssemblyLoadContext.GetAssemblyName(fullFileName);
                return Assembly.Load(assemblyName);
            }
            catch (Exception ex)
            {
                // this doesn't usually work
                InternalLogger.Warn(ex, "Fallback to AssemblyLoadContext.Default.LoadFromAssemblyPath for file: {0}", fullFileName);
                return System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromAssemblyPath(fullFileName);
            }
#else
            Assembly asm = Assembly.LoadFrom(fullFileName);
            return asm;
#endif
        }
#endif

        /// <summary>
        /// Load from url
        /// </summary>
        private static Assembly LoadAssemblyFromName(string assemblyName)
        {
            InternalLogger.Info("Loading assembly: {0}", assemblyName);

#if !NETSTANDARD1_3 && !NETSTANDARD1_5
            try
            {
                Assembly assembly = Assembly.Load(assemblyName);
                return assembly;
            }
            catch (System.IO.FileNotFoundException)
            {
                var name = new AssemblyName(assemblyName);
                InternalLogger.Trace("Try find '{0}' in current domain", assemblyName);
                var loadedAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(domainAssembly => IsAssemblyMatch(name, domainAssembly.GetName()));
                if (loadedAssembly != null)
                {
                    InternalLogger.Trace("Found '{0}' in current domain", assemblyName);
                    return loadedAssembly;
                }

                InternalLogger.Trace("Haven't found' '{0}' in current domain", assemblyName);
                throw;
            }
#else
            var name = new AssemblyName(assemblyName);
            return Assembly.Load(name);
#endif
        }

#if !NETSTANDARD1_3 && !NETSTANDARD1_5
        private static bool IsAssemblyMatch(AssemblyName expected, AssemblyName actual)
        {
            if (expected.Name != actual.Name)
                return false;
            if (expected.Version != null && expected.Version != actual.Version)
                return false;
            if (expected.CultureInfo != null && expected.CultureInfo.Name != actual.CultureInfo.Name)
                return false;

            var expectedKeyToken = expected.GetPublicKeyToken();
            var correctToken = expectedKeyToken is null || expectedKeyToken.SequenceEqual(actual.GetPublicKeyToken());
            return correctToken;
        }
#endif
    }

    internal interface IAssemblyExtensionLoader
    {
        void ScanForAutoLoadExtensions(ConfigurationItemFactory factory);

        void LoadAssemblyFromPath(ConfigurationItemFactory factory, string assemblyPath, string baseDirectory, string itemNamePrefix);

        void LoadAssemblyFromName(ConfigurationItemFactory factory, string assemblyName, string itemNamePrefix);

        void LoadTypeFromName(ConfigurationItemFactory factory, string typeName, string itemNamePrefix);
    }
}
