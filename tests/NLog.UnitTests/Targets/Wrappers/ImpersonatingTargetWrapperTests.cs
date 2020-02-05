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

#if !MONO && !NETSTANDARD

namespace NLog.UnitTests.Targets.Wrappers
{
    using NLog.Common;
    using NLog.Targets;
    using NLog.Targets.Wrappers;
    using System;
    using System.Collections.Generic;
    using System.DirectoryServices.AccountManagement;
    using System.Runtime.InteropServices;
    using System.Security.Principal;
    using Xunit;

    public class ImpersonatingTargetWrapperTests : NLogTestBase, IDisposable
    {
        private const string NLogTestUser = "NLogTestUser";
        private const string NLogTestUserPassword = "BC@57acasd123";
        private string Localhost = Environment.MachineName;

        public ImpersonatingTargetWrapperTests()
        {
            CreateUserIfNotPresent();
        }

        [Fact]
        public void ImpersonatingWrapperTest()
        {
            var wrapped = new MyTarget()
            {
                ExpectedUser = Environment.MachineName + "\\" + NLogTestUser,
            };

            var wrapper = new ImpersonatingTargetWrapper()
            {
                UserName = NLogTestUser,
                Password = NLogTestUserPassword,
                Domain = Environment.MachineName,
                WrappedTarget = wrapped,
            };

            // wrapped.Initialize(null);
            wrapper.Initialize(null);

            var exceptions = new List<Exception>();
            wrapper.WriteAsyncLogEvent(LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add));
            Assert.Single(exceptions);
            wrapper.WriteAsyncLogEvents(
                LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add),
                LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add),
                LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add));
            Assert.Equal(4, exceptions.Count);
            wrapper.Flush(exceptions.Add);
            Assert.Equal(5, exceptions.Count);
            foreach (var ex in exceptions)
            {
                Assert.Null(ex);
            }

            wrapper.Close();
        }

        [Fact]
        public void RevertToSelfTest()
        {
            var wrapped = new MyTarget()
            {
                ExpectedUser = Environment.UserDomainName + "\\" + Environment.UserName,
            };

            WindowsIdentity originalIdentity = WindowsIdentity.GetCurrent();

            try
            {
                var id = CreateWindowsIdentity(NLogTestUser, Environment.MachineName, NLogTestUserPassword, SecurityLogOnType.Interactive, LogOnProviderType.Default, SecurityImpersonationLevel.Identification);
                id.Impersonate();

                WindowsIdentity changedIdentity = WindowsIdentity.GetCurrent();
                Assert.Contains(NLogTestUser.ToLowerInvariant(), changedIdentity.Name.ToLowerInvariant(), StringComparison.InvariantCulture);

                var wrapper = new ImpersonatingTargetWrapper()
                {
                    WrappedTarget = wrapped,
                    RevertToSelf = true,
                };

                // wrapped.Initialize(null);
                wrapper.Initialize(null);

                var exceptions = new List<Exception>();
                wrapper.WriteAsyncLogEvent(LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add));
                Assert.Single(exceptions);
                wrapper.WriteAsyncLogEvents(
                    LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add),
                    LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add),
                    LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add));
                Assert.Equal(4, exceptions.Count);
                wrapper.Flush(exceptions.Add);
                Assert.Equal(5, exceptions.Count);
                foreach (var ex in exceptions)
                {
                    Assert.Null(ex);
                }

                wrapper.Close();
            }
            finally
            {
                // revert to self
                NativeMethods.RevertToSelf();

                WindowsIdentity currentIdentity = WindowsIdentity.GetCurrent();
                Assert.Equal(originalIdentity.Name.ToLowerInvariant(), currentIdentity.Name.ToLowerInvariant());
            }
        }

        [Fact]
        public void ImpersonatingWrapperNegativeTest()
        {
            var wrapped = new MyTarget()
            {
                ExpectedUser = NLogTestUser,
            };

            LogManager.ThrowExceptions = true;

            var wrapper = new ImpersonatingTargetWrapper()
            {
                UserName = NLogTestUser,
                Password = Guid.NewGuid().ToString("N"), // wrong password
                Domain = Environment.MachineName,
                WrappedTarget = wrapped,
            };

            Assert.Throws<COMException>(() =>
            {
                wrapper.Initialize(null);
            });

            wrapper.Close(); // will not fail because Initialize() failed
        }

        [Fact]
        public void ImpersonatingWrapperNegativeTest2()
        {
            var wrapped = new MyTarget()
            {
                ExpectedUser = NLogTestUser,
            };

            LogManager.ThrowExceptions = true;


            var wrapper = new ImpersonatingTargetWrapper()
            {
                UserName = NLogTestUser,
                Password = NLogTestUserPassword,
                Domain = Environment.MachineName,
                ImpersonationLevel = (SecurityImpersonationLevel)1234,
                WrappedTarget = wrapped,
            };

            Assert.Throws<COMException>(() =>
                {
                    wrapper.Initialize(null);
                });

            wrapper.Close(); // will not fail because Initialize() failed
        }

        private WindowsIdentity CreateWindowsIdentity(string username, string domain, string password, SecurityLogOnType logonType, LogOnProviderType logonProviderType, SecurityImpersonationLevel impersonationLevel)
        {
            // initialize tokens
            var existingTokenHandle = IntPtr.Zero;
            var duplicateTokenHandle = IntPtr.Zero;

            if (!NativeMethods.LogonUser(
                username,
                domain,
                password,
                (int)logonType,
                (int)logonProviderType,
                out existingTokenHandle))
            {
                throw Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
            }

            if (!NativeMethods.DuplicateToken(existingTokenHandle, (int)impersonationLevel, out duplicateTokenHandle))
            {
                NativeMethods.CloseHandle(existingTokenHandle);
                throw Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
            }

            // create new identity using new primary token
            return new WindowsIdentity(duplicateTokenHandle);
        }

        private static class NativeMethods
        {
            // obtains user token
            [DllImport("advapi32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool LogonUser(string pszUsername, string pszDomain, string pszPassword, int dwLogonType, int dwLogonProvider, out IntPtr phToken);

            // closes open handles returned by LogonUser
            [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool CloseHandle(IntPtr handle);

            // creates duplicate token handle
            [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool DuplicateToken(IntPtr existingTokenHandle, int impersonationLevel, out IntPtr duplicateTokenHandle);

            [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool RevertToSelf();
        }

        public class MyTarget : Target
        {
            public MyTarget()
            {
                Events = new List<LogEventInfo>();
            }

            public MyTarget(string name) : this()
            {
                Name = name;
            }

            public List<LogEventInfo> Events { get; set; }

            public string ExpectedUser { get; set; }

            protected override void InitializeTarget()
            {
                base.InitializeTarget();
                AssertExpectedUser();
            }

            protected override void CloseTarget()
            {
                base.CloseTarget();
                AssertExpectedUser();
            }

            protected override void Write(LogEventInfo logEvent)
            {
                AssertExpectedUser();
                Events.Add(logEvent);
            }

            protected override void Write(IList<AsyncLogEventInfo> logEvents)
            {
                AssertExpectedUser();
                base.Write(logEvents);
            }

            protected override void FlushAsync(AsyncContinuation asyncContinuation)
            {
                AssertExpectedUser();
                base.FlushAsync(asyncContinuation);
            }

            private void AssertExpectedUser()
            {
                if (ExpectedUser != null)
                {
                    var windowsIdentity = WindowsIdentity.GetCurrent();
                    Assert.True(windowsIdentity.IsAuthenticated);
                    Assert.Equal(Environment.MachineName + "\\" + ExpectedUser, windowsIdentity.Name);
                }
            }
        }

        private void CreateUserIfNotPresent()
        {
            using (var context = new PrincipalContext(ContextType.Machine, Localhost))
            {
                if (UserPrincipal.FindByIdentity(context, IdentityType.Name, NLogTestUser) != null)
                    return;

                var user = new UserPrincipal(context);
                user.SetPassword(NLogTestUserPassword);
                user.Name = NLogTestUser;
                user.Save();

                var group = GroupPrincipal.FindByIdentity(context, "Users");
                group.Members.Add(user);
                group.Save();
            }
        }

        public void Dispose()
        {
            DeleteUser();
        }

        private void DeleteUser()
        {
            using (var context = new PrincipalContext(ContextType.Machine, Localhost))
            using (var up = UserPrincipal.FindByIdentity(context, IdentityType.Name, NLogTestUser))
            {
                if (up != null)
                    up.Delete();
            }
        }
    }
}

#endif