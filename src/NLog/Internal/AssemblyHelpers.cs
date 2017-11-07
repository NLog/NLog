// 
// Copyright (c) 2004-2017 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using System.Reflection;
    using NLog.Common;

#if SILVERLIGHT && !WINDOWS_PHONE
using System.Windows;
#endif

    /// <summary>
    /// Helpers for <see cref="Assembly"/>.
    /// </summary>
    internal class AssemblyHelpers
    {
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
#elif SILVERLIGHT && !WINDOWS_PHONE
            var stream = Application.GetResourceStream(new Uri(assemblyFileName, UriKind.Relative));
            var assemblyPart = new AssemblyPart();
            Assembly assembly = assemblyPart.Load(stream.Stream);
            return assembly;
#else
            Assembly asm = Assembly.LoadFrom(fullFileName);
            return asm;
#endif
        }

        /// <summary>
        /// Load from url
        /// </summary>
        /// <param name="assemblyName">name without .dll</param>
        /// <returns></returns>
        public static Assembly LoadFromName(string assemblyName)
        {
            InternalLogger.Info("Loading assembly: {0}", assemblyName);

#if NETSTANDARD1_5 || WINDOWS_PHONE
            var name = new AssemblyName(assemblyName);
            return Assembly.Load(name);
#elif SILVERLIGHT && !WINDOWS_PHONE
            //as embedded resource
            var assemblyFile = assemblyName + ".dll";
            var stream = Application.GetResourceStream(new Uri(assemblyFile, UriKind.Relative));
            var assemblyPart = new AssemblyPart();
            Assembly assembly = assemblyPart.Load(stream.Stream);
            return assembly;
#else
            Assembly assembly = Assembly.Load(assemblyName);
            return assembly;
#endif
        }

    }
}