// 
// Copyright (c) 2004-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
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

using System;
using System.Text;
using System.Reflection;
using System.Collections;

using NLog.Config;
using NLog.Internal;

namespace NLog.Internal
{
    internal class PlatformDetector
    {
        private static IDictionary _currentFrameworkCompatibleWith = new Hashtable();
        private static IDictionary _currentOSCompatibleWith = new Hashtable();

        public static bool IsCurrentOSCompatibleWith(RuntimeOS os)
        {
            return _currentOSCompatibleWith.Contains(os);
        }

        public static bool IsCurrentFrameworkCompatibleWith(RuntimeFramework framework)
        {
            return _currentFrameworkCompatibleWith.Contains(framework);
        }

        public static bool IsSupportedOnCurrentRuntime(Type t)
        {
            SupportedRuntimeAttribute[] supportedRuntimes = 
                (SupportedRuntimeAttribute[])t.GetCustomAttributes(typeof(SupportedRuntimeAttribute), true);

            // no attributes defined - we default to Yes.
            if (supportedRuntimes == null || supportedRuntimes.Length == 0)
                return true;

            foreach (SupportedRuntimeAttribute sr in supportedRuntimes)
            {
                if (_currentFrameworkCompatibleWith.Contains(sr.Framework)
                    && _currentOSCompatibleWith.Contains(sr.OS))
                {
                    return true;
                }
            }

            InternalLogger.Debug("{0} is not supported on current runtime.", t.FullName);

            return false;
        }

        public static bool IsSupportedOnCurrentRuntime(MethodInfo mi)
        {
            SupportedRuntimeAttribute[] supportedRuntimes = 
                (SupportedRuntimeAttribute[])mi.GetCustomAttributes(typeof(SupportedRuntimeAttribute), true);

            // no attributes defined - we default to Yes.
            if (supportedRuntimes == null || supportedRuntimes.Length == 0)
                return true;

            foreach (SupportedRuntimeAttribute sr in supportedRuntimes)
            {
                if (_currentFrameworkCompatibleWith.Contains(sr.Framework)
                    && _currentOSCompatibleWith.Contains(sr.OS))
                {
                    return true;
                }
            }

            InternalLogger.Debug("{0} is not supported on current runtime.", mi.Name);

            return false;
        }

        static PlatformDetector()
        {
            FindCompatibleFrameworks();
            FindCompatibleOSes();
        }

        public static RuntimeFramework GetCurrentRuntimeFramework()
        {
#if NETCF
            return RuntimeFramework.DotNetCompactFramework;
#else
            Version v = Environment.Version;

            if (Type.GetType("System.MonoType", false) != null)
            {
                return RuntimeFramework.Mono;
            }
            else
            {
                return RuntimeFramework.DotNetFramework;
            }
#endif
        }

        public static RuntimeOS GetCurrentRuntimeOS()
        {
            PlatformID platformID = Environment.OSVersion.Platform;
            if ((int)platformID == 4 || (int)platformID == 128)
                return RuntimeOS.Unix;

            if ((int)platformID == 3)
                return RuntimeOS.WindowsCE;

            if (platformID == PlatformID.Win32Windows)
                return RuntimeOS.Windows;

            if (platformID == PlatformID.Win32NT)
                return RuntimeOS.WindowsNT;

            return RuntimeOS.Unknown;
        }

        private static void Compatibility(RuntimeFramework framework, RuntimeFramework impliesCompatibilityWith)
        {
            if (_currentFrameworkCompatibleWith.Contains(framework))
                _currentFrameworkCompatibleWith[impliesCompatibilityWith] = true;
        }

        private static void Compatibility(RuntimeOS os, RuntimeOS impliesCompatibilityWith)
        {
            if (_currentOSCompatibleWith.Contains(os))
                _currentOSCompatibleWith[impliesCompatibilityWith] = true;
        }

        private static void FindCompatibleFrameworks()
        {
            _currentFrameworkCompatibleWith[GetCurrentRuntimeFramework()] = true;
            _currentFrameworkCompatibleWith[RuntimeFramework.Any] = true;
        }

        private static void FindCompatibleOSes()
        {
            _currentOSCompatibleWith[GetCurrentRuntimeOS()] = true;
            _currentOSCompatibleWith[RuntimeOS.Any] = true;

            Compatibility(RuntimeOS.WindowsNT, RuntimeOS.Windows);
            Compatibility(RuntimeOS.Windows, RuntimeOS.Win32);
            Compatibility(RuntimeOS.WindowsCE, RuntimeOS.Win32);
        }
    }
}
