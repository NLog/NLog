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
namespace NLog.Internal
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using NLog.Common;

    /// <summary>
    /// Helpers for <see cref="Assembly"/>.
    /// </summary>
    internal static class AssemblyHelpers
    {
#if !NETSTANDARD1_3
        /// <summary>
        /// Load from url
        /// </summary>
        /// <param name="assemblyFileName">file or path, including .dll</param>
        /// <param name="baseDirectory">basepath, optional</param>
        /// <returns></returns>
        public static Assembly LoadFromPath(string assemblyFileName, string baseDirectory = null)
        {
            string fullFileName = baseDirectory == null ? assemblyFileName : Path.Combine(baseDirectory, assemblyFileName);

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
        /// <param name="assemblyName">name without .dll</param>
        /// <returns></returns>
        public static Assembly LoadFromName(string assemblyName)
        {
            InternalLogger.Info("Loading assembly: {0}", assemblyName);

#if !NETSTANDARD1_3 && !NETSTANDARD1_5
            try
            {
                Assembly assembly = Assembly.Load(assemblyName);
                return assembly;
            }
            catch (FileNotFoundException)
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

        private static bool IsAssemblyMatch(AssemblyName expected, AssemblyName actual)
        {
            if (expected.Name != actual.Name)
                return false;
            if (expected.Version != null && expected.Version != actual.Version)
                return false;
#if !NETSTANDARD1_3 && !NETSTANDARD1_5
            if (expected.CultureInfo != null && expected.CultureInfo.Name != actual.CultureInfo.Name)
                return false;
#endif
            var expectedKeyToken = expected.GetPublicKeyToken();
            var correctToken = expectedKeyToken == null || expectedKeyToken.SequenceEqual(actual.GetPublicKeyToken());
            return correctToken;
        }

#if !NETSTANDARD1_3
        public static string GetAssemblyFileLocation(Assembly assembly)
        {
            string fullName = string.Empty;

            try
            {
                if (assembly == null)
                {
                    return string.Empty;
                }

                fullName = assembly.FullName;

#if NETSTANDARD
                if (string.IsNullOrEmpty(assembly.Location))
                {
                    // Assembly with no actual location should be skipped (Avoid PlatformNotSupportedException)
                    InternalLogger.Warn("Ignoring assembly location because location is empty: {0}", fullName);
                    return string.Empty;
                }
#endif

                Uri assemblyCodeBase;
                if (!Uri.TryCreate(assembly.CodeBase, UriKind.RelativeOrAbsolute, out assemblyCodeBase))
                {
                    InternalLogger.Warn("Ignoring assembly location because code base is unknown: '{0}' ({1})", assembly.CodeBase, fullName);
                    return string.Empty;
                }

                var assemblyLocation = Path.GetDirectoryName(assemblyCodeBase.LocalPath);
                if (string.IsNullOrEmpty(assemblyLocation))
                {
                    InternalLogger.Warn("Ignoring assembly location because it is not a valid directory: '{0}' ({1})", assemblyCodeBase.LocalPath, fullName);
                    return string.Empty;
                }

                DirectoryInfo directoryInfo = new DirectoryInfo(assemblyLocation);
                if (!directoryInfo.Exists)
                {
                    InternalLogger.Warn("Ignoring assembly location because directory doesn't exists: '{0}' ({1})", assemblyLocation, fullName);
                    return string.Empty;
                }

                InternalLogger.Debug("Found assembly location directory: '{0}' ({1})", directoryInfo.FullName, fullName);
                return directoryInfo.FullName;
            }
            catch (System.PlatformNotSupportedException ex)
            {
                InternalLogger.Warn(ex, "Ignoring assembly location because assembly lookup is not supported: {0}", fullName);
                if (ex.MustBeRethrown())
                {
                    throw;
                }
                return string.Empty;
            }
            catch (System.Security.SecurityException ex)
            {
                InternalLogger.Warn(ex, "Ignoring assembly location because assembly lookup is not allowed: {0}", fullName);
                if (ex.MustBeRethrown())
                {
                    throw;
                }
                return string.Empty;
            }
            catch (UnauthorizedAccessException ex)
            {
                InternalLogger.Warn(ex, "Ignoring assembly location because assembly lookup is not allowed: {0}", fullName);
                if (ex.MustBeRethrown())
                {
                    throw;
                }
                return string.Empty;
            }
        }
#endif
    }
}