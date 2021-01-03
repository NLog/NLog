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

#if NETSTANDARD1_3 || NETSTANDARD1_5

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NLog.Internal.Fakeables
{
    internal class FakeAppDomain : IAppDomain
    {
#if NETSTANDARD1_5
        System.Runtime.Loader.AssemblyLoadContext _defaultContext;
#endif

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public FakeAppDomain()
        {
            BaseDirectory = AppContext.BaseDirectory;
#if NETSTANDARD1_5
            _defaultContext = System.Runtime.Loader.AssemblyLoadContext.Default;

            try
            {
                FriendlyName = GetFriendlyNameFromEntryAssembly() ?? GetFriendlyNameFromProcessName() ?? "UnknownAppDomain";
            }
            catch
            {
                FriendlyName = "UnknownAppDomain";
            }
#endif
        }

#region Implementation of IAppDomain

#if NETSTANDARD1_5
        private static string GetFriendlyNameFromEntryAssembly()
        {
            try
            {
                string assemblyName =  Assembly.GetEntryAssembly()?.GetName()?.Name;
                return string.IsNullOrEmpty(assemblyName) ? null : assemblyName;
            }
            catch
            {
                return null;
            }
        }

        private static string GetFriendlyNameFromProcessName()
        {
            try
            {
                string processName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
                return string.IsNullOrEmpty(processName) ? null : processName;
            }
            catch
            {
                return null;
            }
        }
#endif

        /// <summary>
        /// Gets or sets the base directory that the assembly resolver uses to probe for assemblies.
        /// </summary>
        public string BaseDirectory { get; private set; }

        /// <summary>
        /// Gets or sets the name of the configuration file for an application domain.
        /// </summary>
        public string ConfigurationFile { get; set; }

        /// <summary>
        /// Gets or sets the list of directories under the application base directory that are probed for private assemblies.
        /// </summary>
        public IEnumerable<string> PrivateBinPath { get; set; }

        /// <summary>
        /// Gets or set the friendly name.
        /// </summary>
        public string FriendlyName { get; set; }

        /// <summary>
        /// Gets an integer that uniquely identifies the application domain within the process. 
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets the assemblies that have been loaded into the execution context of this application domain.
        /// </summary>
        /// <returns>A list of assemblies in this application domain.</returns>
        public IEnumerable<Assembly> GetAssemblies()
        {
            return Internal.ArrayHelper.Empty<Assembly>();  // TODO NETSTANDARD1_6 has DependencyContext.RuntimeLibraries
        }

        /// <summary>
        /// Process exit event.
        /// </summary>
        public event EventHandler<EventArgs> ProcessExit
        {
            add
            {
#if NETSTANDARD1_5
                if (_contextUnloadingEvent == null && _defaultContext != null)
                    _defaultContext.Unloading += OnContextUnloading;
                _contextUnloadingEvent += value;
#endif
            }
            remove
            {
#if NETSTANDARD1_5
                _contextUnloadingEvent -= value;
                if (_contextUnloadingEvent == null && _defaultContext != null)
                    _defaultContext.Unloading -= OnContextUnloading;
#endif
            }
        }

        /// <summary>
        /// Domain unloaded event.
        /// </summary>
        public event EventHandler<EventArgs> DomainUnload
        {
            add
            {
#if NETSTANDARD1_5
                if (_contextUnloadingEvent == null && _defaultContext != null)
                    _defaultContext.Unloading += OnContextUnloading;
                _contextUnloadingEvent += value;
#endif
            }
            remove
            {
#if NETSTANDARD1_5
                _contextUnloadingEvent -= value;
                if (_contextUnloadingEvent == null && _defaultContext != null)
                    _defaultContext.Unloading -= OnContextUnloading;
#endif
            }
        }

#if NETSTANDARD1_5
        private event EventHandler<EventArgs> _contextUnloadingEvent;

        private void OnContextUnloading(System.Runtime.Loader.AssemblyLoadContext context)
        {
            var handler = _contextUnloadingEvent;
            if (handler != null) handler.Invoke(context, EventArgs.Empty);
        }
#endif
#endregion
    }
}

#endif