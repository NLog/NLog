// 
// Copyright (c) 2004,2005 Jaroslaw Kowalski <jkowalski@users.sourceforge.net>
// 
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
// * Neither the name of the Jaroslaw Kowalski nor the names of its 
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

namespace NLog
{
    public class ExtensionUtils
    {
        private static ArrayList _extensionAssemblies = new ArrayList();

        static ExtensionUtils()
        {
            FindPlatformSpecificAssemblies(_extensionAssemblies);
        }

        public static ArrayList GetExtensionAssemblies()
        {
            return _extensionAssemblies;
        }

        private static void FindPlatformSpecificAssemblies(ArrayList result)
        {
            // load default targets, filters and layout renderers.

            result.Add(typeof(NLog.LogManager).Assembly);

            InternalLogger.Info("Registering platform specific extensions...");
#if NETCF
            RegisterPlatformSpecificExtensions("NLog.CompactFramework");
#else 
            if (Type.GetType("System.MonoType", false) != null)
            {
                RegisterPlatformSpecificExtensions(result, "NLog.Mono");
            }
            else
            {
                RegisterPlatformSpecificExtensions(result, "NLog.DotNet");
            }

            PlatformID platform = System.Environment.OSVersion.Platform;

            if (platform == PlatformID.Win32NT || platform == PlatformID.Win32Windows)
            {
                RegisterPlatformSpecificExtensions(result, "NLog.Win32");
            }

            if ((int)platform == 128 || (int)platform == 4)
            {
                // mono-1.0 used '128' here, net-2.0 and mono-2.0 use '4'
                RegisterPlatformSpecificExtensions(result, "NLog.Unix");
            }
#endif 
            
        }

        private static void RegisterPlatformSpecificExtensions(ArrayList result, string name)
        {
            InternalLogger.Debug("RegisterPlatformSpecificExtensions('{0}')", name);
            AssemblyName nlogAssemblyName = typeof(LogManager).Assembly.GetName();
            AssemblyName newAssemblyName = new AssemblyName();
            newAssemblyName.Name = name;
            newAssemblyName.CultureInfo = nlogAssemblyName.CultureInfo;
            newAssemblyName.Flags = nlogAssemblyName.Flags;
            newAssemblyName.SetPublicKey(nlogAssemblyName.GetPublicKey());
            newAssemblyName.Version = nlogAssemblyName.Version;

            try
            {
                InternalLogger.Info("Registering platform specific extensions from assembly '{0}'", newAssemblyName);
                Assembly asm = Assembly.Load(newAssemblyName);
                InternalLogger.Info("Loaded {0}", asm.FullName);
                result.Add(asm);
                InternalLogger.Info("Registered platform specific extensions from assembly '{0}'.", newAssemblyName);
            }
            catch (Exception ex)
            {
                InternalLogger.Error("Could not load platform specific extensions: {0}", ex);
                //if (LogManager.ThrowExceptions)
                //    throw;
            }
        }
    }
}
