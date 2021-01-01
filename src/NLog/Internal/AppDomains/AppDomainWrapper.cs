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

#if !NETSTANDARD1_3 && !NETSTANDARD1_5

namespace NLog.Internal.Fakeables
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using NLog.Common;

    /// <summary>
    /// Adapter for <see cref="AppDomain"/> to <see cref="IAppDomain"/>
    /// </summary>
    internal class AppDomainWrapper : IAppDomain
    {
        private readonly AppDomain _currentAppDomain;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppDomainWrapper"/> class.
        /// </summary>
        /// <param name="appDomain">The <see cref="AppDomain"/> to wrap.</param>
        public AppDomainWrapper(AppDomain appDomain)
        {
            _currentAppDomain = appDomain;
            try
            {
                BaseDirectory = LookupBaseDirectory(appDomain) ?? string.Empty;
            }
            catch (Exception ex)
            {
                InternalLogger.Warn(ex, "AppDomain.BaseDirectory Failed");
                BaseDirectory = string.Empty;
            }

            try
            {
                ConfigurationFile = LookupConfigurationFile(appDomain);
            }
            catch (Exception ex)
            {
                InternalLogger.Warn(ex, "AppDomain.SetupInformation.ConfigurationFile Failed");
                ConfigurationFile = string.Empty;
            }

            try
            {
                PrivateBinPath = LookupPrivateBinPath(appDomain);
            }
            catch (Exception ex)
            {
                InternalLogger.Warn(ex, "AppDomain.SetupInformation.PrivateBinPath Failed");
                PrivateBinPath = ArrayHelper.Empty<string>();
            }

            FriendlyName = appDomain.FriendlyName;
            Id = appDomain.Id;
        }

        private static string LookupBaseDirectory(AppDomain appDomain)
        {
            return appDomain.BaseDirectory;
        }

        private static string LookupConfigurationFile(AppDomain appDomain)
        {
#if !NETSTANDARD
            return appDomain.SetupInformation.ConfigurationFile;
#else
            return string.Empty;
#endif
        }

        private static string[] LookupPrivateBinPath(AppDomain appDomain)
        {
#if !NETSTANDARD
            string privateBinPath = appDomain.SetupInformation.PrivateBinPath;
            return string.IsNullOrEmpty(privateBinPath)
                                    ? ArrayHelper.Empty<string>()
                                    : privateBinPath.SplitAndTrimTokens(';');
#else
            return ArrayHelper.Empty<string>();
#endif
        }

        /// <summary>
        /// Creates an AppDomainWrapper for the current <see cref="AppDomain"/>
        /// </summary>
        public static AppDomainWrapper CurrentDomain => new AppDomainWrapper(AppDomain.CurrentDomain);

        /// <summary>
        /// Gets or sets the base directory that the assembly resolver uses to probe for assemblies.
        /// </summary>
        public string BaseDirectory { get; private set; }

        /// <summary>
        /// Gets or sets the name of the configuration file for an application domain.
        /// </summary>
        public string ConfigurationFile { get; private set; }

        /// <summary>
        /// Gets or sets the list of directories under the application base directory that are probed for private assemblies.
        /// </summary>
        public IEnumerable<string> PrivateBinPath { get; private set; }

        /// <summary>
        /// Gets or set the friendly name.
        /// </summary>
        public string FriendlyName { get; private set; }

        /// <summary>
        /// Gets an integer that uniquely identifies the application domain within the process. 
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Gets the assemblies that have been loaded into the execution context of this application domain.
        /// </summary>
        /// <returns>A list of assemblies in this application domain.</returns>
        public IEnumerable<Assembly> GetAssemblies()
        {
            if (_currentAppDomain != null)
                return _currentAppDomain.GetAssemblies();
            else
                return Internal.ArrayHelper.Empty<Assembly>();
        }

        /// <summary>
        /// Process exit event.
        /// </summary>
        public event EventHandler<EventArgs> ProcessExit
        {
            add
            {
                if (processExitEvent == null && _currentAppDomain != null)
                    _currentAppDomain.ProcessExit += OnProcessExit;
                processExitEvent += value;
            }
            remove
            {
                processExitEvent -= value;
                if (processExitEvent == null && _currentAppDomain != null)
                    _currentAppDomain.ProcessExit -= OnProcessExit;
            }
        }
        private event EventHandler<EventArgs> processExitEvent;

        /// <summary>
        /// Domain unloaded event.
        /// </summary>
        public event EventHandler<EventArgs> DomainUnload
        {
            add
            {
                if (domainUnloadEvent == null && _currentAppDomain != null)
                    _currentAppDomain.DomainUnload += OnDomainUnload;
                domainUnloadEvent += value;

            }
            remove
            {
                domainUnloadEvent -= value;
                if (domainUnloadEvent == null && _currentAppDomain != null)
                    _currentAppDomain.DomainUnload -= OnDomainUnload;
            }
        }
        private event EventHandler<EventArgs> domainUnloadEvent;

        private void OnDomainUnload(object sender, EventArgs e)
        {
            var handler = domainUnloadEvent;
            if (handler != null) handler.Invoke(sender, e);
        }

        private void OnProcessExit(object sender, EventArgs eventArgs)
        {
            var handler = processExitEvent;
            if (handler != null) handler.Invoke(sender, eventArgs);
        }
    }
}

#endif