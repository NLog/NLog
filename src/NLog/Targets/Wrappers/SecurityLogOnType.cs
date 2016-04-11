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

#if !SILVERLIGHT

namespace NLog.Targets.Wrappers
{
    /// <summary>
    /// Logon type.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue", Justification = "Valid security logon type is required.")]
    public enum SecurityLogOnType : int
    {
        /// <summary>
        /// Interactive Logon.
        /// </summary>
        /// <remarks>
        /// This logon type is intended for users who will be interactively using the computer, such as a user being logged on  
        /// by a terminal server, remote shell, or similar process.
        /// This logon type has the additional expense of caching logon information for disconnected operations;
        /// therefore, it is inappropriate for some client/server applications,
        /// such as a mail server.
        /// </remarks>
        Interactive = 2,

        /// <summary>
        /// Network Logon.
        /// </summary>
        /// <remarks>
        /// This logon type is intended for high performance servers to authenticate plaintext passwords.
        /// The LogonUser function does not cache credentials for this logon type.
        /// </remarks>
        Network = 3,

        /// <summary>
        /// Batch Logon.
        /// </summary>
        /// <remarks>
        /// This logon type is intended for batch servers, where processes may be executing on behalf of a user without
        /// their direct intervention. This type is also for higher performance servers that process many plaintext
        /// authentication attempts at a time, such as mail or Web servers.
        /// The LogonUser function does not cache credentials for this logon type.
        /// </remarks>
        Batch = 4,

        /// <summary>
        /// Logon as a Service.
        /// </summary>
        /// <remarks>
        /// Indicates a service-type logon. The account provided must have the service privilege enabled.
        /// </remarks>
        Service = 5,

        /// <summary>
        /// Network Clear Text Logon.
        /// </summary>
        /// <remarks>
        /// This logon type preserves the name and password in the authentication package, which allows the server to make
        /// connections to other network servers while impersonating the client. A server can accept plaintext credentials
        /// from a client, call LogonUser, verify that the user can access the system across the network, and still
        /// communicate with other servers.
        /// NOTE: Windows NT:  This value is not supported.
        /// </remarks>
        NetworkClearText = 8,

        /// <summary>
        /// New Network Credentials.
        /// </summary>
        /// <remarks>
        /// This logon type allows the caller to clone its current token and specify new credentials for outbound connections.
        /// The new logon session has the same local identifier but uses different credentials for other network connections.
        /// NOTE: This logon type is supported only by the LOGON32_PROVIDER_WINNT50 logon provider.
        /// NOTE: Windows NT:  This value is not supported.
        /// </remarks>
        NewCredentials = 9,
    }
}

#endif