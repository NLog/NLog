// 
// Copyright (c) 2004-2016 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

    /// <summary>
    /// Adapter for <see cref="AppDomain"/> to <see cref="IAppDomain"/>
    /// </summary>
    public class AppDomainWrapper : IAppDomain
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppDomainWrapper"/> class.
        /// </summary>
        /// <param name="appDomain">The <see cref="AppDomain"/> to wrap.</param>
        public AppDomainWrapper(AppDomain appDomain)
        {
#if !SILVERLIGHT
            BaseDirectory = appDomain.BaseDirectory;
            ConfigurationFile = appDomain.SetupInformation.ConfigurationFile;

            string privateBinPath = appDomain.SetupInformation.PrivateBinPath;
            PrivateBinPath = string.IsNullOrEmpty(privateBinPath)
                                 ? new string[] {}
                                 : appDomain.SetupInformation.PrivateBinPath.Split(new[] {';'},
                                                                                   StringSplitOptions.RemoveEmptyEntries);
            FriendlyName = appDomain.FriendlyName;
            Id = appDomain.Id;
            
#endif
#if !SILVERLIGHT
            appDomain.ProcessExit += OnProcessExit;
            appDomain.DomainUnload += OnDomainUnload;
#endif
        }

        /// <summary>
        /// Gets a the current <see cref="AppDomain"/> wrappered in a <see cref="AppDomainWrapper"/>.
        /// </summary>
        public static AppDomainWrapper CurrentDomain { get { return new AppDomainWrapper(AppDomain.CurrentDomain); } }

#if !SILVERLIGHT
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
#endif

#if !SILVERLIGHT
        /// <summary>
        /// Process exit event.
        /// </summary>
        public event EventHandler<EventArgs> ProcessExit;

        /// <summary>
        /// Domain unloaded event.
        /// </summary>
        public event EventHandler<EventArgs> DomainUnload;

        private void OnDomainUnload(object sender, EventArgs e)
        {
            var handler = DomainUnload;
            if (handler != null) handler(sender, e);
        }

        private void OnProcessExit(object sender, EventArgs eventArgs)
        {
            var handler = ProcessExit;
            if (handler != null) handler(sender, eventArgs);
        }
#endif
    }
}