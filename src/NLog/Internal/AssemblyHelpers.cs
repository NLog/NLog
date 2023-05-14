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
        /// <summary>
        /// Gets all usable exported types from the given assembly.
        /// </summary>
        /// <param name="assembly">Assembly to scan.</param>
        /// <returns>Usable types from the given assembly.</returns>
        /// <remarks>Types which cannot be loaded are skipped.</remarks>
        [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming - Allow extension loading from config", "IL2026")]
        [Obsolete("Instead use NLog.LogManager.Setup().SetupExtensions(). Marked obsolete with NLog v5.2")]
        public static Type[] SafeGetTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException typeLoadException)
            {
                var result = typeLoadException.Types?.Where(t => t != null)?.ToArray() ?? ArrayHelper.Empty<Type>();
                InternalLogger.Warn(typeLoadException, "Loaded {0} valid types from Assembly: {1}", result.Length, assembly.FullName);
                foreach (var ex in typeLoadException.LoaderExceptions ?? ArrayHelper.Empty<Exception>())
                {
                    InternalLogger.Warn(ex, "Type load exception.");
                }
                return result;
            }
            catch (Exception ex)
            {
                InternalLogger.Warn(ex, "Failed to load types from Assembly: {0}", assembly.FullName);
                return ArrayHelper.Empty<Type>();
            }
        }

#if !NETSTANDARD1_3
        [Obsolete("Instead use RegisterType<T>, as dynamic Assembly loading will be moved out. Marked obsolete with NLog v5.2")]
        public static string GetAssemblyFileLocation(Assembly assembly)
        {
            string assemblyFullName = string.Empty;

            try
            {
                if (assembly is null)
                {
                    return string.Empty;
                }

                assemblyFullName = assembly.FullName;

#if NETSTANDARD
                if (string.IsNullOrEmpty(assembly.Location))
                {
                    // Assembly with no actual location should be skipped (Avoid PlatformNotSupportedException)
                    InternalLogger.Debug("Ignoring assembly location because location is empty: {0}", assemblyFullName);
                    return string.Empty;
                }
#endif

                Uri assemblyCodeBase;
                if (!Uri.TryCreate(assembly.CodeBase, UriKind.RelativeOrAbsolute, out assemblyCodeBase))
                {
                    InternalLogger.Debug("Ignoring assembly location because code base is unknown: '{0}' ({1})", assembly.CodeBase, assemblyFullName);
                    return string.Empty;
                }

                var assemblyLocation = Path.GetDirectoryName(assemblyCodeBase.LocalPath);
                if (string.IsNullOrEmpty(assemblyLocation))
                {
                    InternalLogger.Debug("Ignoring assembly location because it is not a valid directory: '{0}' ({1})", assemblyCodeBase.LocalPath, assemblyFullName);
                    return string.Empty;
                }

                DirectoryInfo directoryInfo = new DirectoryInfo(assemblyLocation);
                if (!directoryInfo.Exists)
                {
                    InternalLogger.Debug("Ignoring assembly location because directory doesn't exists: '{0}' ({1})", assemblyLocation, assemblyFullName);
                    return string.Empty;
                }

                InternalLogger.Debug("Found assembly location directory: '{0}' ({1})", directoryInfo.FullName, assemblyFullName);
                return directoryInfo.FullName;
            }
            catch (System.PlatformNotSupportedException ex)
            {
                InternalLogger.Warn(ex, "Ignoring assembly location because assembly lookup is not supported: {0}", assemblyFullName);
                if (ex.MustBeRethrown())
                {
                    throw;
                }
                return string.Empty;
            }
            catch (System.Security.SecurityException ex)
            {
                InternalLogger.Warn(ex, "Ignoring assembly location because assembly lookup is not allowed: {0}", assemblyFullName);
                if (ex.MustBeRethrown())
                {
                    throw;
                }
                return string.Empty;
            }
            catch (UnauthorizedAccessException ex)
            {
                InternalLogger.Warn(ex, "Ignoring assembly location because assembly lookup is not allowed: {0}", assemblyFullName);
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