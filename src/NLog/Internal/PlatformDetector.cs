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

#if !SILVERLIGHT && !__ANDROID__ && !__IOS__ && !NETSTANDARD1_3
// Unfortunately, Xamarin Android and Xamarin iOS don't support mutexes (see https://github.com/mono/mono/blob/3a9e18e5405b5772be88bfc45739d6a350560111/mcs/class/corlib/System.Threading/Mutex.cs#L167) 
#define SupportsMutex
#endif

using System.Security;
using NLog.Common;
using NLog.Internal.FileAppenders;

namespace NLog.Internal
{
    using System;

    /// <summary>
    /// Detects the platform the NLog is running on.
    /// </summary>
    internal static class PlatformDetector
    {
        private static RuntimeOS currentOS = GetCurrentRuntimeOS();

        /// <summary>
        /// Gets the current runtime OS.
        /// </summary>
        public static RuntimeOS CurrentOS => currentOS;

        /// <summary>
        /// Gets a value indicating whether current OS is a desktop version of Windows.
        /// </summary>
        public static bool IsDesktopWin32 => currentOS == RuntimeOS.Windows || currentOS == RuntimeOS.WindowsNT;

        /// <summary>
        /// Gets a value indicating whether current OS is Win32-based (desktop or mobile).
        /// </summary>
        public static bool IsWin32 => currentOS == RuntimeOS.Windows || currentOS == RuntimeOS.WindowsNT || currentOS == RuntimeOS.WindowsCE;

        /// <summary>
        /// Gets a value indicating whether current OS is Unix-based.
        /// </summary>
        public static bool IsUnix => currentOS == RuntimeOS.Unix;

        /// <summary>
        /// Gets a value indicating whether current runtime is Mono-based
        /// </summary>
        public static bool IsMono => _isMono ?? (_isMono = Type.GetType("Mono.Runtime") != null).Value;
        private static bool? _isMono;

        /// <summary>
        /// Gets a value indicating whether current runtime supports use of mutex
        /// </summary>
        public static bool SupportsSharableMutex
        {
            get
            {
#if NETSTANDARD1_5
                return RunTimeSupportsSharableMutex;
#elif !SILVERLIGHT && !__ANDROID__ && !__IOS__ && !NETSTANDARD1_3
                // Unfortunately, Xamarin Android and Xamarin iOS don't support mutexes (see https://github.com/mono/mono/blob/3a9e18e5405b5772be88bfc45739d6a350560111/mcs/class/corlib/System.Threading/Mutex.cs#L167) 
                if (IsMono && Environment.Version.Major < 4)
                    return false;   // MONO ver. 4 is needed for named Mutex to work
                else
                    return RunTimeSupportsSharableMutex;
#else
                return false;
#endif
            }
        }

        /// <summary>
        ///  Will creating a mutex succeed runtime?
        /// "Cached" detection
        /// </summary>
        private static bool? runTimeSupportsSharableMutex;

        /// <summary>
        /// Will creating a mutex succeed runtime?
        /// </summary>
        private static bool RunTimeSupportsSharableMutex
        {
            get
            {
                if (runTimeSupportsSharableMutex.HasValue)
                {
                    return runTimeSupportsSharableMutex.Value;
                }


                try
                {
#if SupportsMutex
                    var mutex = BaseMutexFileAppender.ForceCreateSharableMutex("NLogMutexTester");
                    mutex.Close(); //"dispose"

                    runTimeSupportsSharableMutex = true;
#else
                    runTimeSupportsSharableMutex = false;
#endif
                }
                catch (Exception ex)
                {
                    InternalLogger.Debug(ex, "Failed to create sharable mutex processes");
                    runTimeSupportsSharableMutex = false;
                }

                return runTimeSupportsSharableMutex.Value;
            }

        }

        private static RuntimeOS GetCurrentRuntimeOS()
        {
#if NETSTANDARD1_0
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                return RuntimeOS.Windows;
            else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
                return RuntimeOS.Unix;
            else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
                return RuntimeOS.Unix;
            return RuntimeOS.Unknown;
#else
            PlatformID platformID = Environment.OSVersion.Platform;
            if ((int)platformID == 4 || (int)platformID == 128)
            {
                return RuntimeOS.Unix;
            }

            if ((int)platformID == 3)
            {
                return RuntimeOS.WindowsCE;
            }

            if (platformID == PlatformID.Win32Windows)
            {
                return RuntimeOS.Windows;
            }

            if (platformID == PlatformID.Win32NT)
            {
                return RuntimeOS.WindowsNT;
            }

            return RuntimeOS.Unknown;
#endif
        }
    }
}
