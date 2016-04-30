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

using NLog.Config;
using NLog.LayoutRenderers;
using NLog.Layouts;

namespace NLog.UnitTests.Targets
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using NLog.Common;
    using NLog.Targets;
    using Xunit;

    public class TargetTests : NLogTestBase
    {
        [Fact]
        public void ConstructorsTest()
        {
            var targetTypes = typeof(Target).Assembly.GetExportedTypes();
            int needed = targetTypes.Length;
            int @checked = 0;
            foreach (Type candidateType in targetTypes)
            {
                if (!candidateType.IsAbstract &&
                    typeof(Target).IsAssignableFrom(candidateType))
                {
                    Target defaultConstructedTarget;
                    Target namedConstructedTarget;
                    bool constructionFailed = false;
                    string lastPropertyName = null;
                    
                    try
                    {
                        // Check if the Target can be created using a default constructor
                        defaultConstructedTarget = (Target)Activator.CreateInstance(candidateType);

                        // Check if the Target can be created using a constructor with the name parameter
                        namedConstructedTarget = (Target)Activator.CreateInstance(candidateType, candidateType.ToString());

                        var checkedAtLeastOneProperty = false;

                        // Check that the created targets are the same except for name
                        foreach (System.Reflection.PropertyInfo pi in candidateType.GetProperties())
                        {
                           
                            lastPropertyName = pi.Name;
                            if (pi.CanRead && !pi.Name.Equals("Name"))
                            {
                                var value1 = pi.GetValue(defaultConstructedTarget, null);
                                var value2 = pi.GetValue(namedConstructedTarget, null);
                                if (value1 != null && value2 != null)
                                {
                                    if (value1 is SimpleLayout || value1 is LayoutRenderer || value1 is Layout)
                                    {
                                        Assert.Equal(value1.ToString(), value2.ToString());
                                    }
                                    else
                                    {
                                        Assert.Equal(value1, value2);
                                    }
                                  
                                }
                                else
                                {
                                    Assert.Null(value1);
                                    Assert.Null(value2);
                                }
                                checkedAtLeastOneProperty = true;
                            }

                        }

                        if (checkedAtLeastOneProperty)
                        {
                            @checked ++;
                        }
                    }
                    catch (Exception ex)
                    {
                        constructionFailed = true;
                        string failureMessage = String.Format("Error testing constructors for '{0}.{1}`\n{2}", candidateType, lastPropertyName, ex.ToString());
                        Assert.False(constructionFailed, failureMessage);
                    }
                }
            }
            Assert.Equal(needed,@checked);
        }

        [Fact]
        public void InitializeTest()
        {
            var target = new MyTarget();
            target.Initialize(null);

            // initialize was called once
            Assert.Equal(1, target.InitializeCount);
            Assert.Equal(1, target.InitializeCount + target.FlushCount + target.CloseCount + target.WriteCount + target.WriteCount2 + target.WriteCount3);
        }

        [Fact]
        public void InitializeFailedTest()
        {
            var target = new MyTarget();
            target.ThrowOnInitialize = true;

            LogManager.ThrowExceptions = true;


            Assert.Throws<InvalidOperationException>(() => target.Initialize(null));

            // after exception in Initialize(), the target becomes non-functional and all Write() operations
            var exceptions = new List<Exception>();
            target.WriteAsyncLogEvent(LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add));
            Assert.Equal(0, target.WriteCount);
            Assert.Equal(1, exceptions.Count);
            Assert.NotNull(exceptions[0]);
            Assert.Equal("Target " + target + " failed to initialize.", exceptions[0].Message);
            Assert.Equal("Init error.", exceptions[0].InnerException.Message);
        }

        [Fact]
        public void DoubleInitializeTest()
        {
            var target = new MyTarget();
            target.Initialize(null);
            target.Initialize(null);

            // initialize was called once
            Assert.Equal(1, target.InitializeCount);
            Assert.Equal(1, target.InitializeCount + target.FlushCount + target.CloseCount + target.WriteCount + target.WriteCount2 + target.WriteCount3);
        }

        [Fact]
        public void DoubleCloseTest()
        {
            var target = new MyTarget();
            target.Initialize(null);
            target.Close();
            target.Close();

            // initialize and close were called once each
            Assert.Equal(1, target.InitializeCount);
            Assert.Equal(1, target.CloseCount);
            Assert.Equal(2, target.InitializeCount + target.FlushCount + target.CloseCount + target.WriteCount + target.WriteCount2 + target.WriteCount3);
        }

        [Fact]
        public void CloseWithoutInitializeTest()
        {
            var target = new MyTarget();
            target.Close();

            // nothing was called
            Assert.Equal(0, target.InitializeCount + target.FlushCount + target.CloseCount + target.WriteCount + target.WriteCount2 + target.WriteCount3);
        }

        [Fact]
        public void WriteWithoutInitializeTest()
        {
            var target = new MyTarget();
            List<Exception> exceptions = new List<Exception>();
            target.WriteAsyncLogEvent(LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add));
            target.WriteAsyncLogEvents(new[]
            {
                LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add),
                LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add),
                LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add),
            });

            // write was not called
            Assert.Equal(0, target.InitializeCount + target.FlushCount + target.CloseCount + target.WriteCount + target.WriteCount2 + target.WriteCount3);
            Assert.Equal(4, exceptions.Count);
            exceptions.ForEach(Assert.Null);
        }

        [Fact]
        public void WriteOnClosedTargetTest()
        {
            var target = new MyTarget();
            target.Initialize(null);
            target.Close();

            var exceptions = new List<Exception>();
            target.WriteAsyncLogEvent(LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add));
            target.WriteAsyncLogEvents(
                LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add),
                LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add),
                LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add));

            Assert.Equal(1, target.InitializeCount);
            Assert.Equal(1, target.CloseCount);

            // write was not called
            Assert.Equal(2, target.InitializeCount + target.FlushCount + target.CloseCount + target.WriteCount + target.WriteCount2 + target.WriteCount3);

            // but all callbacks were invoked with null values
            Assert.Equal(4, exceptions.Count);
            exceptions.ForEach(Assert.Null);
        }

        [Fact]
        public void FlushTest()
        {
            var target = new MyTarget();
            List<Exception> exceptions = new List<Exception>();
            target.Initialize(null);
            target.Flush(exceptions.Add);

            // flush was called
            Assert.Equal(1, target.FlushCount);
            Assert.Equal(2, target.InitializeCount + target.FlushCount + target.CloseCount + target.WriteCount + target.WriteCount2 + target.WriteCount3);
            Assert.Equal(1, exceptions.Count);
            exceptions.ForEach(Assert.Null);
        }

        [Fact]
        public void FlushWithoutInitializeTest()
        {
            var target = new MyTarget();
            List<Exception> exceptions = new List<Exception>();
            target.Flush(exceptions.Add);

            Assert.Equal(1, exceptions.Count);
            exceptions.ForEach(Assert.Null);

            // flush was not called
            Assert.Equal(0, target.InitializeCount + target.FlushCount + target.CloseCount + target.WriteCount + target.WriteCount2 + target.WriteCount3);
        }

        [Fact]
        public void FlushOnClosedTargetTest()
        {
            var target = new MyTarget();
            target.Initialize(null);
            target.Close();
            Assert.Equal(1, target.InitializeCount);
            Assert.Equal(1, target.CloseCount);

            List<Exception> exceptions = new List<Exception>();
            target.Flush(exceptions.Add);

            Assert.Equal(1, exceptions.Count);
            exceptions.ForEach(Assert.Null);

            // flush was not called
            Assert.Equal(2, target.InitializeCount + target.FlushCount + target.CloseCount + target.WriteCount + target.WriteCount2 + target.WriteCount3);
        }

        [Fact]
        public void LockingTest()
        {
            var target = new MyTarget();
            target.Initialize(null);

            var mre = new ManualResetEvent(false);

            Exception backgroundThreadException = null;

            Thread t = new Thread(() =>
            {
                try
                {
                    target.BlockingOperation(1000);
                }
                catch (Exception ex)
                {
                    backgroundThreadException = ex;
                }
                finally
                {
                    mre.Set();
                }
            });


            target.Initialize(null);
            t.Start();
            Thread.Sleep(50);
            List<Exception> exceptions = new List<Exception>();
            target.WriteAsyncLogEvent(LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add));
            target.WriteAsyncLogEvents(new[]
            {
                LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add),
                LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add),
            });
            target.Flush(exceptions.Add);
            target.Close();

            exceptions.ForEach(Assert.Null);

            mre.WaitOne();
            if (backgroundThreadException != null)
            {
                Assert.True(false, backgroundThreadException.ToString());
            }
        }

        [Fact]
        public void GivenNullEvents_WhenWriteAsyncLogEvents_ThenNoExceptionAreThrown()
        {
            var target = new MyTarget();

            try
            {
                target.WriteAsyncLogEvents(null);
            }
            catch (Exception e)
            {
                Assert.True(false, "Exception thrown: " + e);
            }
        }

        [Fact]
        public void WriteFormattedStringEvent_WithNullArgument()
        {
            var target = new MyTarget();
            SimpleConfigurator.ConfigureForTargetLogging(target);
            var logger = LogManager.GetLogger("WriteFormattedStringEvent_EventWithNullArguments");
            string t = null;
            logger.Info("Testing null:{0}", t);
            Assert.Equal(1, target.WriteCount);
        }

        public class MyTarget : Target
        {
            private int inBlockingOperation;

            public int InitializeCount { get; set; }
            public int CloseCount { get; set; }
            public int FlushCount { get; set; }
            public int WriteCount { get; set; }
            public int WriteCount2 { get; set; }
            public bool ThrowOnInitialize { get; set; }
            public int WriteCount3 { get; set; }

            public MyTarget() : base()
            {
            }

            public MyTarget(string name) : this()
            {
                this.Name = name;
            }

            protected override void InitializeTarget()
            {
                if (this.ThrowOnInitialize)
                {
                    throw new InvalidOperationException("Init error.");
                }

                Assert.Equal(0, this.inBlockingOperation);
                this.InitializeCount++;
                base.InitializeTarget();
            }

            protected override void CloseTarget()
            {
                Assert.Equal(0, this.inBlockingOperation);
                this.CloseCount++;
                base.CloseTarget();
            }

            protected override void FlushAsync(AsyncContinuation asyncContinuation)
            {
                Assert.Equal(0, this.inBlockingOperation);
                this.FlushCount++;
                base.FlushAsync(asyncContinuation);
            }

            protected override void Write(LogEventInfo logEvent)
            {
                Assert.Equal(0, this.inBlockingOperation);
                this.WriteCount++;
            }

            protected override void Write(AsyncLogEventInfo logEvent)
            {
                Assert.Equal(0, this.inBlockingOperation);
                this.WriteCount2++;
                base.Write(logEvent);
            }

            protected override void Write(AsyncLogEventInfo[] logEvents)
            {
                Assert.Equal(0, this.inBlockingOperation);
                this.WriteCount3++;
                base.Write(logEvents);
            }

            public void BlockingOperation(int millisecondsTimeout)
            {
                lock (this.SyncRoot)
                {
                    this.inBlockingOperation++;
                    Thread.Sleep(millisecondsTimeout);
                    this.inBlockingOperation--;
                }
            }
        }

        [Fact]
        public void WrongMyTargetShouldNotThrowExceptionWhenThrowExceptionsIsFalse()
        {
            var target = new WrongMyTarget();
            LogManager.ThrowExceptions = false;
            SimpleConfigurator.ConfigureForTargetLogging(target);
            var logger = LogManager.GetLogger("WrongMyTargetShouldThrowException");
            logger.Info("Testing");
            var layouts = target.GetAllLayouts();
            Assert.NotNull(layouts);
        }


        public class WrongMyTarget : Target
        {
            public WrongMyTarget() : base()
            {
            }

            public WrongMyTarget(string name) : this()
            {
                this.Name = name;
            }

            /// <summary>
            /// Initializes the target. Can be used by inheriting classes
            /// to initialize logging.
            /// </summary>
            protected override void InitializeTarget()
            {
                //base.InitializeTarget() should be called
            }
        }
    }
}
