// 
// Copyright (c) 2004-2009 Jaroslaw Kowalski <jaak@jkowalski.net>
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

#if !NET_CF && !SILVERLIGHT

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace NLog.Targets.Wrappers
{
    using Internal;

    /// <summary>
    /// A target wrapper that impersonates another user for the duration of the write.
    /// </summary>
    [Target("ImpersonatingWrapper", IsWrapper = true)]
    public class ImpersonatingTargetWrapper : WrapperTargetBase
    {
        private WindowsIdentity newIdentity;
        private IntPtr existingTokenHandle = IntPtr.Zero;
        private IntPtr duplicateTokenHandle = IntPtr.Zero;

        /// <summary>
        /// Initializes a new instance of the ImpersonatingTargetWrapper class.
        /// </summary>
        public ImpersonatingTargetWrapper()
        {
            this.Domain = ".";
            this.LogonType = SecurityLogonType.Interactive;
            this.LogonProvider = LogonProviderType.Default;
            this.ImpersonationLevel = SecurityImpersonationLevel.Impersonation;
        }

        /// <summary>
        /// Initializes a new instance of the ImpersonatingTargetWrapper class.
        /// </summary>
        /// <param name="wrappedTarget">The wrapped target.</param>
        public ImpersonatingTargetWrapper(Target wrappedTarget)
        {
            this.WrappedTarget = wrappedTarget;
        }

        /// <summary>
        /// Impersonation level.
        /// </summary>
        public enum SecurityImpersonationLevel
        {
            /// <summary>
            /// Anonymous Level.
            /// </summary>
            Anonymous = 0,

            /// <summary>
            /// Identification Level.
            /// </summary>
            Identification = 1,

            /// <summary>
            /// Impersonation Level.
            /// </summary>
            Impersonation = 2,

            /// <summary>
            /// Delegation Level.
            /// </summary>
            Delegation = 3
        }

        /// <summary>
        /// Logon type.
        /// </summary>
        public enum SecurityLogonType : int
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

        /// <summary>
        /// Logon provider.
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

        /// <summary>
        /// Gets or sets username to change context to.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the user account password.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets Windows domain name to change context to.
        /// </summary>
        [DefaultValue(".")]
        public string Domain { get; set; }

        /// <summary>
        /// Gets or sets the Logon Type.
        /// </summary>
        public SecurityLogonType LogonType { get; set; }

        /// <summary>
        /// Gets or sets the type of the logon provider.
        /// </summary>
        public LogonProviderType LogonProvider { get; set; }

        /// <summary>
        /// Gets or sets the required impersonation level.
        /// </summary>
        public SecurityImpersonationLevel ImpersonationLevel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to revert to the credentials of the process instead of impersonating another user.
        /// </summary>
        [DefaultValue(false)]
        public bool RevertToSelf { get; set; }

        /// <summary>
        /// Initializes the impersonation context.
        /// </summary>
        public override void Initialize()
        {
            if (!this.RevertToSelf)
            {
                this.newIdentity = this.CreateWindowsIdentity();
            }

            using (this.DoImpersonate())
            {
                base.Initialize();
            }
        }

        /// <summary>
        /// Changes the security context, forwards the call to the <see cref="WrapperTargetBase.WrappedTarget"/>.Write()
        /// and switches the context back to original.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        protected internal override void Write(LogEventInfo logEvent)
        {
            using (this.DoImpersonate())
            {
                this.WrappedTarget.Write(logEvent);
            }
        }

        /// <summary>
        /// Changes the security context, forwards the call to the <see cref="WrapperTargetBase.WrappedTarget"/>.Write()
        /// and switches the context back to original.
        /// </summary>
        /// <param name="logEvents">Log events.</param>
        protected internal override void Write(LogEventInfo[] logEvents)
        {
            using (this.DoImpersonate())
            {
                this.WrappedTarget.Write(logEvents);
            }
        }

        /// <summary>
        /// Closes the impersonation context.
        /// </summary>
        protected internal override void Close()
        {
            using (this.DoImpersonate())
            {
                base.Close();
            }

            if (this.existingTokenHandle != IntPtr.Zero)
            {
                NativeMethods.CloseHandle(this.existingTokenHandle);
                this.existingTokenHandle = IntPtr.Zero;
            }

            if (this.duplicateTokenHandle != IntPtr.Zero)
            {
                NativeMethods.CloseHandle(this.duplicateTokenHandle);
                this.duplicateTokenHandle = IntPtr.Zero;
            }
        }

        private IDisposable DoImpersonate()
        {
            if (this.RevertToSelf)
            {
                return new ContextReverter(WindowsIdentity.Impersonate(IntPtr.Zero));
            }

            if (this.newIdentity != null)
            {
                return new ContextReverter(this.newIdentity.Impersonate());
            }

            return null;
        }

        //
        // adapted from:
        // http://www.codeproject.com/csharp/cpimpersonation1.asp
        //
        private WindowsIdentity CreateWindowsIdentity()
        {
            // initialize tokens
            this.existingTokenHandle = IntPtr.Zero;
            this.duplicateTokenHandle = IntPtr.Zero;

            if (!NativeMethods.LogonUser(
                this.UserName,
                this.Domain,
                this.Password,
                (int)this.LogonType,
                (int)this.LogonProvider,
                out this.existingTokenHandle))
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }

            if (!NativeMethods.DuplicateToken(this.existingTokenHandle, (int)this.ImpersonationLevel, out this.duplicateTokenHandle))
            {
                NativeMethods.CloseHandle(this.existingTokenHandle);
                this.existingTokenHandle = IntPtr.Zero;
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }

            // create new identity using new primary token
            return new WindowsIdentity(this.duplicateTokenHandle);
        }

        /// <summary>
        /// Helper class which reverts the given <see cref="WindowsImpersonationContext"/> 
        /// to its original value as part of <see cref="IDisposable.Dispose"/>.
        /// </summary>
        internal class ContextReverter : IDisposable
        {
            private WindowsImpersonationContext wic;

            /// <summary>
            /// Initializes a new instance of the ContextReverter class.
            /// </summary>
            /// <param name="windowsImpersonationContext">The windows impersonation context.</param>
            public ContextReverter(WindowsImpersonationContext windowsImpersonationContext)
            {
                this.wic = windowsImpersonationContext;
            }

            /// <summary>
            /// Reverts the impersonation context.
            /// </summary>
            public void Dispose()
            {
                this.wic.Undo();
            }
        }
    }
}

#endif