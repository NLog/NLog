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

    /// <summary>
    /// Detects the platform the NLog is running on.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal static class PlatformDetector
    {
        /// <summary>
        /// Gets the current runtime OS.
        /// </summary>
        public static PlatformSystem CurrentOS => _currentOS ?? (_currentOS = GetCurrentRuntimeOS()).Value;
        private static PlatformSystem? _currentOS;

        /// <summary>
        /// Gets a value indicating whether current OS is Win32-based (desktop or mobile).
        /// </summary>
        public static bool IsWin32 => CurrentOS == PlatformSystem.WindowsNT || CurrentOS == PlatformSystem.Windows9x;

        /// <summary>
        /// Gets a value indicating whether current OS is Unix-based.
        /// </summary>
        public static bool IsUnix => CurrentOS == PlatformSystem.Linux || CurrentOS == PlatformSystem.MacOSX;

#if NETFRAMEWORK
        /// <summary>
        /// Gets a value indicating whether current runtime is Mono-based
        /// </summary>
        public static bool IsMono => _isMono ?? (_isMono = Type.GetType("Mono.Runtime") != null).Value;
        private static bool? _isMono;
#endif

        private static PlatformSystem GetCurrentRuntimeOS()
        {
#if NETFRAMEWORK
            PlatformID platformID = Environment.OSVersion.Platform;
            if ((int)platformID == 4 || (int)platformID == 128)
                return PlatformSystem.Linux;
            if (platformID == PlatformID.Win32Windows)
                return PlatformSystem.Windows9x;
            if (platformID == PlatformID.Win32NT)
                return PlatformSystem.WindowsNT;
#else
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
                return PlatformSystem.Linux;
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
                return PlatformSystem.MacOSX;
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                return PlatformSystem.WindowsNT;
#endif
            return PlatformSystem.Unknown;
        }
    }
}
