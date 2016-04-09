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
    /// Interface for fakeable the current <see cref="AppDomain"/>. Not fully implemented, please methods/properties as necessary.
    /// </summary>
    public interface IAppDomain
    {
#if !SILVERLIGHT
        /// <summary>
        /// Gets or sets the base directory that the assembly resolver uses to probe for assemblies.
        /// </summary>
        string BaseDirectory { get; }

        /// <summary>
        /// Gets or sets the name of the configuration file for an application domain.
        /// </summary>
        string ConfigurationFile { get; }

        /// <summary>
        /// Gets or sets the list of directories under the application base directory that are probed for private assemblies.
        /// </summary>
        IEnumerable<string> PrivateBinPath { get; }

        /// <summary>
        /// Gets or set the friendly name.
        /// </summary>
        string FriendlyName { get; }

        /// <summary>
        /// Gets an integer that uniquely identifies the application domain within the process. 
        /// </summary>
        int Id { get; }
#endif

#if !SILVERLIGHT
        /// <summary>
        /// Process exit event.
        /// </summary>
        event EventHandler<EventArgs> ProcessExit;
        
        /// <summary>
        /// Domain unloaded event.
        /// </summary>
        event EventHandler<EventArgs> DomainUnload;
#endif
    }
}