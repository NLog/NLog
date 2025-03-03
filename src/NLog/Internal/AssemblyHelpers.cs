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
namespace NLog.Internal
{
    using System;
    using System.Reflection;
    using NLog.Common;

    /// <summary>
    /// Helpers for <see cref="Assembly"/>.
    /// </summary>
    internal static class AssemblyHelpers
    {
        [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Returns empty string for assemblies embedded in a single-file app", "IL3000")]
        public static string GetAssemblyFileLocation(Assembly assembly)
        {
#if !NETFRAMEWORK
            // Notice assembly can be loaded from nuget-cache using NTFS-hard-link, and return unexpected file-location.
            return assembly?.Location ?? string.Empty;
#else
            if (assembly is null)
                return string.Empty;

            var assemblyFullName = assembly.FullName;

            try
            {
                Uri assemblyCodeBase;
                if (!Uri.TryCreate(assembly.CodeBase, UriKind.RelativeOrAbsolute, out assemblyCodeBase))
                {
                    InternalLogger.Debug("Ignoring assembly location because code base is unknown: '{0}' ({1})", assembly.CodeBase, assemblyFullName);
                    return string.Empty;
                }

                var assemblyLocation = System.IO.Path.GetDirectoryName(assemblyCodeBase.LocalPath);
                if (string.IsNullOrEmpty(assemblyLocation))
                {
                    InternalLogger.Debug("Ignoring assembly location because it is not a valid directory: '{0}' ({1})", assemblyCodeBase.LocalPath, assemblyFullName);
                    return string.Empty;
                }

                var directoryInfo = new System.IO.DirectoryInfo(assemblyLocation);
                if (!directoryInfo.Exists)
                {
                    InternalLogger.Debug("Ignoring assembly location because directory doesn't exists: '{0}' ({1})", assemblyLocation, assemblyFullName);
                    return string.Empty;
                }

                InternalLogger.Debug("Found assembly location directory: '{0}' ({1})", directoryInfo.FullName, assemblyFullName);
                var assemblyFileName = string.IsNullOrEmpty(assembly.Location) ? System.IO.Path.GetFileName(assemblyCodeBase.LocalPath) : System.IO.Path.GetFileName(assembly.Location);
                return System.IO.Path.Combine(directoryInfo.FullName, assemblyFileName);
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
#endif
        }

        /// <summary>
        /// Logs the assembly version and file version of the given Assembly.
        /// </summary>
        /// <param name="assembly">The assembly to log.</param>
        public static void LogAssemblyVersion(Assembly assembly)
        {
            try
            {
                if (!InternalLogger.IsInfoEnabled)
                    return;

                var fileVersion = assembly.GetFirstCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
                var productVersion = assembly.GetFirstCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
                var globalAssemblyCache = false;
#if NETFRAMEWORK
                if (assembly.GlobalAssemblyCache)
                    globalAssemblyCache = true;
#endif
                InternalLogger.Info("{0}. File version: {1}. Product version: {2}. GlobalAssemblyCache: {3}",
                    assembly.FullName,
                    fileVersion,
                    productVersion,
                    globalAssemblyCache);
            }
            catch (Exception ex)
            {
                InternalLogger.Error(ex, "Error logging version of assembly {0}.", assembly?.FullName);
            }
        }
    }
}
