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

namespace NLog.Targets
{
    /// <summary>
    /// Web Service Proxy Configuration Type
    /// </summary>
    public enum WebServiceProxyType
    {
        /// <summary>
        /// Default proxy configuration from app.config (System.Net.WebRequest.DefaultWebProxy)
        /// </summary>
        /// <example>
        /// Example of how to configure default proxy using app.config
        /// <code>
        /// &lt;system.net&gt;
        ///    &lt;defaultProxy enabled = "true" useDefaultCredentials = "true" &gt;
        ///       &lt;proxy usesystemdefault = "True" /&gt;
        ///    &lt;/defaultProxy&gt;
        /// &lt;/system.net&gt;
        /// </code>
        /// </example>
        DefaultWebProxy,
        /// <summary>
        /// Automatic use of proxy with authentication (cached)
        /// </summary>
        AutoProxy,
        /// <summary>
        /// Disables use of proxy (fast)
        /// </summary>
        NoProxy,
        /// <summary>
        /// Custom proxy address (cached)
        /// </summary>
        ProxyAddress,
    }
}
