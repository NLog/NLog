// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
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

namespace NLog.Targets.Wrappers
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Security.Principal;
    using NLog.Common;
    using NLog.Internal;

    /// <summary>
    /// Impersonates another user for the duration of the write.
    /// </summary>
    /// <seealso href="http://nlog-project.org/wiki/ImpersonatingWrapper_target">Documentation on NLog Wiki</seealso>
    [Target("ImpersonatingWrapper", IsWrapper = true)]
    public class ImpersonatingTargetWrapper : WrapperTargetBase
    {
        private WindowsIdentity newIdentity;
        private IntPtr duplicateTokenHandle = IntPtr.Zero;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImpersonatingTargetWrapper" /> class.
        /// </summary>
        public ImpersonatingTargetWrapper()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImpersonatingTargetWrapper" /> class.
        /// </summary>
        /// <param name="wrappedTarget">The wrapped target.</param>
        public ImpersonatingTargetWrapper(Target wrappedTarget)
        {
            this.Domain = ".";
            this.LogOnType = SecurityLogOnType.Interactive;
            this.LogOnProvider = LogOnProviderType.Default;
            this.ImpersonationLevel = SecurityImpersonationLevel.Impersonation;
            this.WrappedTarget = wrappedTarget;
        }

        /// <summary>
        /// Gets or sets username to change context to.
        /// </summary>
        /// <docgen category='Impersonation Options' order='10' />
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the user account password.
        /// </summary>
        /// <docgen category='Impersonation Options' order='10' />
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets Windows domain name to change context to.
        /// </summary>
        /// <docgen category='Impersonation Options' order='10' />
        [DefaultValue(".")]
        public string Domain { get; set; }

        /// <summary>
        /// Gets or sets the Logon Type.
        /// </summary>
        /// <docgen category='Impersonation Options' order='10' />
        public SecurityLogOnType LogOnType { get; set; }

        /// <summary>
        /// Gets or sets the type of the logon provider.
        /// </summary>
        /// <docgen category='Impersonation Options' order='10' />
        public LogOnProviderType LogOnProvider { get; set; }

        /// <summary>
        /// Gets or sets the required impersonation level.
        /// </summary>
        /// <docgen category='Impersonation Options' order='10' />
        public SecurityImpersonationLevel ImpersonationLevel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to revert to the credentials of the process instead of impersonating another user.
        /// </summary>
        /// <docgen category='Impersonation Options' order='10' />
        [DefaultValue(false)]
        public bool RevertToSelf { get; set; }

        /// <summary>
        /// Initializes the impersonation context.
        /// </summary>
        protected override void InitializeTarget()
        {
            if (!this.RevertToSelf)
            {
                this.newIdentity = this.CreateWindowsIdentity(out this.duplicateTokenHandle);
            }

            using (this.DoImpersonate())
            {
                base.InitializeTarget();
            }
        }

        /// <summary>
        /// Closes the impersonation context.
        /// </summary>
        protected override void CloseTarget()
        {
            using (this.DoImpersonate())
            {
                base.CloseTarget();
            }

            if (this.duplicateTokenHandle != IntPtr.Zero)
            {
                NativeMethods.CloseHandle(this.duplicateTokenHandle);
                this.duplicateTokenHandle = IntPtr.Zero;
            }

            if (this.newIdentity != null)
            {
                this.newIdentity.Dispose();
                this.newIdentity = null;
            }
        }

        /// <summary>
        /// Changes the security context, forwards the call to the <see cref="WrapperTargetBase.WrappedTarget"/>.Write()
        /// and switches the context back to original.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        protected override void Write(AsyncLogEventInfo logEvent)
        {
            using (this.DoImpersonate())
            {
                this.WrappedTarget.WriteAsyncLogEvent(logEvent);
            }
        }

        /// <summary>
        /// Changes the security context, forwards the call to the <see cref="WrapperTargetBase.WrappedTarget"/>.Write()
        /// and switches the context back to original.
        /// </summary>
        /// <param name="logEvents">Log events.</param>
        protected override void Write(AsyncLogEventInfo[] logEvents)
        {
            using (this.DoImpersonate())
            {
                this.WrappedTarget.WriteAsyncLogEvents(logEvents);
            }
        }

        /// <summary>
        /// Flush any pending log messages (in case of asynchronous targets).
        /// </summary>
        /// <param name="asyncContinuation">The asynchronous continuation.</param>
        protected override void FlushAsync(AsyncContinuation asyncContinuation)
        {
            using (this.DoImpersonate())
            {
                this.WrappedTarget.Flush(asyncContinuation);
            }
        }

        private IDisposable DoImpersonate()
        {
            if (this.RevertToSelf)
            {
                return new ContextReverter(WindowsIdentity.Impersonate(IntPtr.Zero));
            }

            return new ContextReverter(this.newIdentity.Impersonate());
        }

        //
        // adapted from:
        // http://www.codeproject.com/csharp/cpimpersonation1.asp
        //
        private WindowsIdentity CreateWindowsIdentity(out IntPtr handle)
        {
            // initialize tokens
            IntPtr logonHandle;

            if (!NativeMethods.LogonUser(
                this.UserName,
                this.Domain,
                this.Password,
                (int)this.LogOnType,
                (int)this.LogOnProvider,
                out logonHandle))
            {
                throw Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
            }

            if (!NativeMethods.DuplicateToken(logonHandle, (int)this.ImpersonationLevel, out handle))
            {
                NativeMethods.CloseHandle(logonHandle);
                throw Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
            }

            NativeMethods.CloseHandle(logonHandle);

            // create new identity using new primary token)
            return new WindowsIdentity(handle);
        }

        /// <summary>
        /// Helper class which reverts the given <see cref="WindowsImpersonationContext"/> 
        /// to its original value as part of <see cref="IDisposable.Dispose"/>.
        /// </summary>
        internal class ContextReverter : IDisposable
        {
            private WindowsImpersonationContext wic;

            /// <summary>
            /// Initializes a new instance of the <see cref="ContextReverter" /> class.
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