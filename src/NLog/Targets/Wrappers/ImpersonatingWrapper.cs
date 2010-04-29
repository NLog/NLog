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

#if !NETCF

using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Reflection;
using System.Diagnostics;

using NLog.Config;
using NLog.Internal;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.ComponentModel;

namespace NLog.Targets.Wrappers
{
    /// <summary>
    /// A target wrapper that impersonates another user for the duration of the write.
    /// </summary>
    [Target("ImpersonatingWrapper", IgnoresLayout = true, IsWrapper = true)]
    [SupportedRuntime(OS = RuntimeOS.WindowsNT)]
    public class ImpersonatingTargetWrapper : WrapperTargetBase
    {
        private string _username;
        private string _password;
        private string _domain = ".";
        private WindowsIdentity _newIdentity;
        private SecurityLogonType _logonType = SecurityLogonType.Interactive;
        private LogonProviderType _logonProvider = LogonProviderType.Default;
        private SecurityImpersonationLevel _impersonationLevel = SecurityImpersonationLevel.Impersonation;
        private IntPtr _existingTokenHandle = IntPtr.Zero;
        private IntPtr _duplicateTokenHandle = IntPtr.Zero;
        private bool _revertToSelf = false;

        /// <summary>
        /// Creates a new instance of <see cref="ImpersonatingTargetWrapper"/>.
        /// </summary>
        public ImpersonatingTargetWrapper()
        {
        }

        /// <summary>
        /// Username to change context to
        /// </summary>
        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }

        /// <summary>
        /// Password
        /// </summary>
        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        /// <summary>
        /// Windows domain name to change context to.
        /// </summary>
        [DefaultValue(".")]
        public string Domain
        {
            get { return _domain; }
            set { _domain = value; }
        }

        /// <summary>
        /// Logon Type.
        /// </summary>
        public SecurityLogonType LogonType
        {
            get { return _logonType; }
            set { _logonType = value; }
        }

        /// <summary>
        /// Logon Provider.
        /// </summary>
        public LogonProviderType LogonProvider
        {
            get { return _logonProvider; }
            set { _logonProvider = value; }
        }

        /// <summary>
        /// Impersonation level.
        /// </summary>
        public SecurityImpersonationLevel ImpersonationLevel
        {
            get { return _impersonationLevel; }
            set { _impersonationLevel = value; }
        }

        /// <summary>
        /// Revert to the credentials of the process instead of impersonating another user.
        /// </summary>
        [DefaultValue(false)]
        public bool RevertToSelf
        {
            get { return _revertToSelf; }
            set { _revertToSelf = value; }
        }
        
        /// <summary>
        /// Creates a new instance of <see cref="ImpersonatingTargetWrapper"/> 
        /// and initializes the <see cref="WrapperTargetBase.WrappedTarget"/> to the specified <see cref="Target"/> value.
        /// </summary>
        public ImpersonatingTargetWrapper(Target writeTo)
        {
            WrappedTarget = writeTo;
        }

        /// <summary>
        /// Changes the security context, forwards the call to the <see cref="WrapperTargetBase.WrappedTarget"/>.Write()
        /// and switches the context back to original.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        protected internal override void Write(LogEventInfo logEvent)
        {
            using (DoImpersonate())
            {
                WrappedTarget.Write(logEvent);
            }
        }

        /// <summary>
        /// Changes the security context, forwards the call to the <see cref="WrapperTargetBase.WrappedTarget"/>.Write()
        /// and switches the context back to original.
        /// </summary>
        /// <param name="logEvents">Log events.</param>
        protected internal override void Write(LogEventInfo[] logEvents)
        {
            using (DoImpersonate())
            {
                WrappedTarget.Write(logEvents);
            }
        }

        /// <summary>
        /// Initializes the impersonation context.
        /// </summary>
        public override void Initialize()
        {
            if (!RevertToSelf)
                _newIdentity = CreateWindowsIdentity();
            using (DoImpersonate())
            {
                base.Initialize();
            }
        }

        /// <summary>
        /// Closes the impersonation context.
        /// </summary>
        protected internal override void Close()
        {
            using (DoImpersonate())
            {
                base.Close();
            }
            if (_existingTokenHandle != IntPtr.Zero)
            {
                CloseHandle(_existingTokenHandle);
                _existingTokenHandle = IntPtr.Zero;
            }
            if (_duplicateTokenHandle != IntPtr.Zero)
            {
                CloseHandle(_duplicateTokenHandle);
                _duplicateTokenHandle = IntPtr.Zero;
            }
        }

        private IDisposable DoImpersonate()
        {
            if (RevertToSelf)
                return new ContextReverter(WindowsIdentity.Impersonate(IntPtr.Zero));

            if (_newIdentity != null)
                return new ContextReverter(_newIdentity.Impersonate());

            return null;
        }

        /// <summary>
        /// Impersonation level.
        /// </summary>
        public enum SecurityImpersonationLevel
        {
            /// <summary>
            /// Anonymous
            /// </summary>
            Anonymous = 0,

            /// <summary>
            /// Identification
            /// </summary>
            Identification = 1,

            /// <summary>
            /// Impersonation
            /// </summary>
            Impersonation = 2,

            /// <summary>
            /// Delegation
            /// </summary>
            Delegation = 3
        }

        /// <summary>
        /// Logon type.
        /// </summary>
        public enum SecurityLogonType : int
        {
            /// <summary>
            /// Interactive Logon
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
            /// Network Logon
            /// </summary>
            /// <remarks>
            /// This logon type is intended for high performance servers to authenticate plaintext passwords.
            /// The LogonUser function does not cache credentials for this logon type.
            /// </remarks>
            Network = 3,

            /// <summary>
            /// Batch Logon
            /// </summary>
            /// <remarks>
            /// This logon type is intended for batch servers, where processes may be executing on behalf of a user without
            /// their direct intervention. This type is also for higher performance servers that process many plaintext
            /// authentication attempts at a time, such as mail or Web servers.
            /// The LogonUser function does not cache credentials for this logon type.
            /// </remarks>
            Batch = 4,

            /// <summary>
            /// Logon as a Service
            /// </summary>
            /// <remarks>
            /// Indicates a service-type logon. The account provided must have the service privilege enabled.
            /// </remarks>
            Service = 5,

            /// <summary>
            /// Network Clear Text Logon
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
            /// New Network Credentials
            /// </summary>
            /// <remarks>
            /// This logon type allows the caller to clone its current token and specify new credentials for outbound connections.
            /// The new logon session has the same local identifier but uses different credentials for other network connections.
            /// NOTE: This logon type is supported only by the LOGON32_PROVIDER_WINNT50 logon provider.
            /// NOTE: Windows NT:  This value is not supported.
            /// </remarks>
            NewCredentials = 9,
        }

        /// <summary>
        /// Logon provider.
        /// 
        /// </summary>
        public enum LogonProviderType : int
        {
            /// <summary>
            /// Use the standard logon provider for the system.
            /// </summary>
            /// <remarks>
            /// The default security provider is negotiate, unless you pass NULL for the domain name and the user name
            /// is not in UPN format. In this case, the default provider is NTLM.
            /// NOTE: Windows 2000/NT:   The default security provider is NTLM.
            /// </remarks>
            Default = 0,
        }

        // obtains user token
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool LogonUser(string pszUsername, string pszDomain, string pszPassword, int dwLogonType, int dwLogonProvider, out IntPtr phToken);

        // closes open handes returned by LogonUser
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private extern static bool CloseHandle(IntPtr handle);

        // creates duplicate token handle
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private extern static bool DuplicateToken(IntPtr existingTokenHandle, int impersonationLevel, out IntPtr duplicateTokenHandle);

        //
        // adapted from:
        // http://www.codeproject.com/csharp/cpimpersonation1.asp
        //
        private WindowsIdentity CreateWindowsIdentity()
        {
            // initialize tokens
            _existingTokenHandle = IntPtr.Zero;
            _duplicateTokenHandle = IntPtr.Zero;

            if (!LogonUser(Username, Domain, Password,
                (int)_logonType, (int)_logonProvider,
                out _existingTokenHandle))
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());

            if (!DuplicateToken(_existingTokenHandle, (int)_impersonationLevel, out _duplicateTokenHandle))
            {
                CloseHandle(_existingTokenHandle);
                _existingTokenHandle = IntPtr.Zero;
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }

            // create new identity using new primary token
            return new WindowsIdentity(_duplicateTokenHandle);
        }

        internal class ContextReverter : IDisposable
        {
            private WindowsImpersonationContext _wic;

            public ContextReverter(WindowsImpersonationContext wic)
            {
                _wic = wic;
            }

            public void Dispose()
            {
                _wic.Undo();
            }
        }
    }
}

#endif