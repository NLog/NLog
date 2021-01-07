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

namespace NLog.Internal.Fakeables
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Xml;

    internal class AppEnvironmentWrapper : IAppEnvironment
    {
#if !NETSTANDARD1_3
        private const string UnknownProcessName = "<unknown>";

        private string _entryAssemblyLocation;
        private string _entryAssemblyFileName;
        private string _currentProcessFilePath;
        private string _currentProcessBaseName;
        private int? _currentProcessId;

        /// <inheritdoc />
        public string EntryAssemblyLocation => _entryAssemblyLocation ?? (_entryAssemblyLocation = LookupEntryAssemblyLocation());
        /// <inheritdoc />
        public string EntryAssemblyFileName => _entryAssemblyFileName ?? (_entryAssemblyFileName = LookupEntryAssemblyFileName());
        /// <inheritdoc />
        public string CurrentProcessFilePath => _currentProcessFilePath ?? (_currentProcessFilePath = LookupCurrentProcessFilePathWithFallback());
        /// <inheritdoc />
        public string CurrentProcessBaseName => _currentProcessBaseName ?? (_currentProcessBaseName = LookupCurrentProcessNameWithFallback());
        /// <inheritdoc />
        public int CurrentProcessId => _currentProcessId ?? (_currentProcessId = LookupCurrentProcessIdWithFallback()).Value;
#endif
        /// <inheritdoc />
        public string AppDomainBaseDirectory => AppDomain.BaseDirectory;
        /// <inheritdoc />
        public string AppDomainConfigurationFile => AppDomain.ConfigurationFile;
        /// <inheritdoc />
        public IEnumerable<string> PrivateBinPath => AppDomain.PrivateBinPath;
        /// <inheritdoc />
        public string UserTempFilePath => Path.GetTempPath();
        /// <inheritdoc />
        public IAppDomain AppDomain { get; internal set; }

        public AppEnvironmentWrapper(IAppDomain appDomain)
        {
            AppDomain = appDomain;
        }

        /// <inheritdoc />
        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        /// <inheritdoc />
        public XmlReader LoadXmlFile(string path)
        {
            return XmlReader.Create(path);
        }

#if !NETSTANDARD1_3
        private static string LookupEntryAssemblyLocation()
        {
            return AssemblyHelpers.GetAssemblyFileLocation(System.Reflection.Assembly.GetEntryAssembly());
        }

        private static string LookupEntryAssemblyFileName()
        {
            try
            {
                return Path.GetFileName(System.Reflection.Assembly.GetEntryAssembly()?.Location ?? string.Empty);
            }
            catch (Exception ex)
            {
                if (ex.MustBeRethrownImmediately())
                    throw;

                return string.Empty;
            }
        }

        private static string LookupCurrentProcessFilePathWithFallback()
        {
            try
            {
                var processFilePath = LookupCurrentProcessFilePath();
                return processFilePath ?? LookupCurrentProcessFilePathNative();
            }
            catch (Exception ex)
            {
                if (ex.MustBeRethrownImmediately())
                    throw;

                return LookupCurrentProcessFilePathNative();
            }
        }

        private static string LookupCurrentProcessFilePath()
        {
            try
            {
                var currentProcess = Process.GetCurrentProcess();
                return currentProcess?.MainModule.FileName;
            }
            catch (Exception ex)
            {
                if (ex.MustBeRethrownImmediately())
                    throw;

                // May throw a SecurityException or Access Denied when running from an IIS app. pool process
                return null;
            }
        }

        private static int LookupCurrentProcessIdWithFallback()
        {
            try
            {
                var processId = LookupCurrentProcessId();
                return processId ?? LookupCurrentProcessIdNative();
            }
            catch (Exception ex)
            {
                if (ex.MustBeRethrownImmediately())
                    throw;

                // May throw a SecurityException if running from an IIS app. pool process (Cannot compile method)
                return LookupCurrentProcessIdNative();
            }
        }

        private static int? LookupCurrentProcessId()
        {
            try
            {
                var currentProcess = Process.GetCurrentProcess();
                return currentProcess?.Id;
            }
            catch (Exception ex)
            {
                if (ex.MustBeRethrownImmediately())
                    throw;

                // May throw a SecurityException or Access Denied when running from an IIS app. pool process
                return null;
            }
        }

        private static string LookupCurrentProcessNameWithFallback()
        {
            try
            {
                var processName = LookupCurrentProcessName();
                return processName ?? LookupCurrentProcessNameNative();
            }
            catch (Exception ex)
            {
                if (ex.MustBeRethrownImmediately())
                    throw;

                // May throw a SecurityException if running from an IIS app. pool process (Cannot compile method)
                return LookupCurrentProcessNameNative();
            }
        }

        private static string LookupCurrentProcessName()
        {
            try
            {
                var currentProcess = Process.GetCurrentProcess();
                var currentProcessName = currentProcess?.ProcessName;
                if (!string.IsNullOrEmpty(currentProcessName))
                    return currentProcessName;
            }
            catch (Exception ex)
            {
                if (ex.MustBeRethrownImmediately())
                    throw;
            }

            return null;
        }

        private static string LookupCurrentProcessNameNative()
        {
            var currentProcessFilePath = LookupCurrentProcessFilePath();
            if (!string.IsNullOrEmpty(currentProcessFilePath))
            {
                var currentProcessName = Path.GetFileNameWithoutExtension(currentProcessFilePath);
                if (!string.IsNullOrEmpty(currentProcessName))
                    return currentProcessName;
            }

            return UnknownProcessName;
        }
#endif

#if !NETSTANDARD
        private static string LookupCurrentProcessFilePathNative()
        {
            try
            {
                if (!PlatformDetector.IsWin32)
                    return string.Empty;

                return LookupCurrentProcessFilePathWin32();
            }
            catch (Exception ex)
            {
                if (ex.MustBeRethrownImmediately())
                    throw;

                return string.Empty;
            }
        }

        [System.Security.SecuritySafeCritical]
        private static string LookupCurrentProcessFilePathWin32()
        {
            try
            {
                var sb = new System.Text.StringBuilder(512);
                if (0 == NativeMethods.GetModuleFileName(IntPtr.Zero, sb, sb.Capacity))
                {
                    throw new InvalidOperationException("Cannot determine program name.");
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                if (ex.MustBeRethrownImmediately())
                    throw;

                return string.Empty;
            }
        }

        private static int LookupCurrentProcessIdNative()
        {
            try
            {
                if (!PlatformDetector.IsWin32)
                    return 0;

                return LookupCurrentProcessIdWin32();
            }
            catch (Exception ex)
            {
                if (ex.MustBeRethrownImmediately())
                    throw;

                return 0;
            }
        }

        [System.Security.SecuritySafeCritical]
        private static int LookupCurrentProcessIdWin32()
        {
            try
            {
                return NativeMethods.GetCurrentProcessId();
            }
            catch (Exception ex)
            {
                if (ex.MustBeRethrownImmediately())
                    throw;

                return 0;
            }
        }
#else
        private static string LookupCurrentProcessFilePathNative()
        {
            return string.Empty;
        }

        private static int LookupCurrentProcessIdNative()
        {
            return 0;
        }
#endif
    }
}
