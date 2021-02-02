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

namespace NLog.WindowsIdentity.Tests
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

    public sealed class ImpersonatingTargetWrapperTests : IDisposable
    {
        private const string NLogTestUser = "NLogTestUser";
        private const string NLogTestUserPassword = "BC@57acasd123";
        private readonly string LocalMachineName = Environment.MachineName;
        private bool? _userCreated;

        public ImpersonatingTargetWrapperTests()
        {
            LogManager.ThrowExceptions = true;
        }

#if !NETSTANDARD
        [Fact]
#else
        [Fact(Skip = "CreateUserIfNotPresent fails with NetCore")]
#endif
        public void ImpersonatingWrapperTest()
        {
            CreateUserIfNotPresent();

            var wrapped = new MyTarget()
            {
                ExpectedUser = LocalMachineName + "\\" + NLogTestUser,
            };

            var wrapper = new ImpersonatingTargetWrapper()
            {
                UserName = NLogTestUser,
                Password = NLogTestUserPassword,
                Domain = LocalMachineName,
                WrappedTarget = wrapped,
            };

            var logFactory = new LogFactory().Setup().LoadConfiguration(cfg =>
            {
                cfg.Configuration.AddRuleForAllLevels(wrapper);
            }).LogFactory;

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

            logFactory.Shutdown();
        }

#if !NETSTANDARD
        [Fact]
#else
        [Fact(Skip = "CreateUserIfNotPresent fails with NetCore")]
#endif
        public void RevertToSelfTest()
        {
            CreateUserIfNotPresent();

            var wrapped = new MyTarget()
            {
                ExpectedUser = LocalMachineName + "\\" + Environment.UserName,
            };

            WindowsIdentity originalIdentity = WindowsIdentity.GetCurrent();

            var newIdentity = new ImpersonatingTargetWrapper.NewIdentityHandle(
                NLogTestUser,
                LocalMachineName,
                NLogTestUserPassword,
                SecurityLogOnType.Interactive,
                LogOnProviderType.Default,
                SecurityImpersonationLevel.Identification
                );

            try
            {
                ImpersonatingTargetWrapper.NewIdentityHandle.RunImpersonated(newIdentity, (s) =>
                {
                    WindowsIdentity changedIdentity = WindowsIdentity.GetCurrent();
                    Assert.Contains(NLogTestUser.ToLowerInvariant(), changedIdentity.Name.ToLowerInvariant(), StringComparison.InvariantCulture);

                    var wrapper = new ImpersonatingTargetWrapper()
                    {
                        WrappedTarget = wrapped,
                        RevertToSelf = true,
                    };

                    var logFactory = new LogFactory().Setup().LoadConfiguration(cfg =>
                    {
                        cfg.Configuration.AddRuleForAllLevels(wrapper);
                    }).LogFactory;

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

                    logFactory.Shutdown();
                }, (object)null);
            }
            finally
            {
                newIdentity.Dispose();

                WindowsIdentity currentIdentity = WindowsIdentity.GetCurrent();
                Assert.Equal(originalIdentity.Name.ToLowerInvariant(), currentIdentity.Name.ToLowerInvariant());
            }
        }

        [Fact]
        public void RevertToSameIdentity()
        {
            var wrapped = new MyTarget()
            {
                ExpectedUser = LocalMachineName + "\\" + Environment.UserName,
            };

            var wrapper = new ImpersonatingTargetWrapper()
            {
                WrappedTarget = wrapped,
                RevertToSelf = true,
            };

            var logFactory = new LogFactory().Setup().LoadConfiguration(cfg =>
            {
                cfg.Configuration.AddRuleForAllLevels(wrapper);
            }).LogFactory;

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

            logFactory.Shutdown();
        }

#if !NETSTANDARD
        [Fact]
#else
        [Fact(Skip = "CreateUserIfNotPresent fails with NetCore")]
#endif
        public void ImpersonatingWrapperNegativeTest()
        {
            CreateUserIfNotPresent();

            var wrapped = new MyTarget()
            {
                ExpectedUser = LocalMachineName + "\\" + NLogTestUser,
            };

            var wrapper = new ImpersonatingTargetWrapper()
            {
                UserName = NLogTestUser,
                Password = Guid.NewGuid().ToString("N"), // wrong password
                Domain = LocalMachineName,
                WrappedTarget = wrapped,
            };

            var logFactory = new LogFactory();

            Assert.Throws<COMException>(() =>
            {
                logFactory.Setup().LoadConfiguration(cfg =>
                {
                    cfg.Configuration.AddRuleForAllLevels(wrapper);
                });
            });

            logFactory.Shutdown(); // will not fail because Initialize() failed
        }

#if !NETSTANDARD
        [Fact]
#else
        [Fact(Skip = "CreateUserIfNotPresent fails with NetCore")]
#endif
        public void ImpersonatingWrapperNegativeTest2()
        {
            CreateUserIfNotPresent();

            var wrapped = new MyTarget()
            {
                ExpectedUser = LocalMachineName + "\\" + NLogTestUser,
            };

            LogManager.ThrowExceptions = true;

            var wrapper = new ImpersonatingTargetWrapper()
            {
                UserName = NLogTestUser,
                Password = NLogTestUserPassword,
                Domain = LocalMachineName,
                ImpersonationLevel = (SecurityImpersonationLevel)1234,
                WrappedTarget = wrapped,
            };

            var logFactory = new LogFactory();

            Assert.Throws<COMException>(() =>
                {
                    logFactory.Setup().LoadConfiguration(cfg =>
                    {
                        cfg.Configuration.AddRuleForAllLevels(wrapper);
                    });
                });

            logFactory.Shutdown(); // will not fail because Initialize() failed
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
                    Assert.Equal(ExpectedUser, windowsIdentity.Name);
                }
            }
        }

        private void CreateUserIfNotPresent()
        {
            if (_userCreated.HasValue)
                return;

            using (var context = new PrincipalContext(ContextType.Machine, LocalMachineName))
            {
                if (UserPrincipal.FindByIdentity(context, IdentityType.Name, NLogTestUser) != null)
                {
                    _userCreated = false;
                    return;
                }

#if !NETSTANDARD
                var user = new UserPrincipal(context);
                user.SetPassword(NLogTestUserPassword);
                user.Name = NLogTestUser;
                user.Save();

                var group = GroupPrincipal.FindByIdentity(context, "Users");
                group.Members.Add(user);
                group.Save();
#endif
            }

            _userCreated = true;
        }

        public void Dispose()
        {
            DeleteUser();
        }

        private void DeleteUser()
        {
            using (var context = new PrincipalContext(ContextType.Machine, LocalMachineName))
            using (var up = UserPrincipal.FindByIdentity(context, IdentityType.Name, NLogTestUser))
            {
                if (up != null)
                    up.Delete();
            }
        }
    }
}