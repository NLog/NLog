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

using System.Globalization;
using System.Linq;
using System.Text;
using NLog.Conditions;
using NLog.Config;
using NLog.Internal;
using NLog.Layouts;
using NLog.Targets.Wrappers;
using NLog.UnitTests.Config;

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
        /// <summary>
        /// Test the following things:
        /// - Target has default ctor
        /// - Target has ctor with name (string) arg.
        /// - Both ctors are creating the same instances
        /// </summary>
        [Fact]
        public void TargetContructorWithNameTest()
        {
            var targetTypes = typeof(Target).Assembly.GetTypes().Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(Target))).ToList();
            int neededCheckCount = targetTypes.Count;
            int checkCount = 0;
            Target fileTarget = new FileTarget();
            Target memoryTarget = new MemoryTarget();

            foreach (Type targetType in targetTypes)
            {
                string lastPropertyName = null;

                try
                {
                    // Check if the Target can be created using a default constructor
                    var name = targetType + "_name";

                    var isWrapped = targetType.IsSubclassOf(typeof(WrapperTargetBase));
                    var isCompound = targetType.IsSubclassOf(typeof(CompoundTargetBase));

                    if (isWrapped)
                    {
                        neededCheckCount++;

                        var args = new List<object> { fileTarget };

                        //default ctor
                        var defaultConstructedTarget = (WrapperTargetBase)Activator.CreateInstance(targetType);
                        defaultConstructedTarget.Name = name;
                        defaultConstructedTarget.WrappedTarget = fileTarget;

                        //specials cases
                        if (targetType == typeof(FilteringTargetWrapper))
                        {
                            ConditionLoggerNameExpression cond = null;
                            args.Add(cond);
                            var target = (FilteringTargetWrapper)defaultConstructedTarget;
                            target.Condition = cond;
                        }
                        else if (targetType == typeof(RepeatingTargetWrapper))
                        {
                            var repeatCount = 5;
                            args.Add(repeatCount);
                            var target = (RepeatingTargetWrapper)defaultConstructedTarget;
                            target.RepeatCount = repeatCount;
                        }
                        else if (targetType == typeof(RetryingTargetWrapper))
                        {
                            var retryCount = 10;
                            var retryDelayMilliseconds = 100;
                            args.Add(retryCount);
                            args.Add(retryDelayMilliseconds);
                            var target = (RetryingTargetWrapper)defaultConstructedTarget;
                            target.RetryCount = retryCount;
                            target.RetryDelayMilliseconds = retryDelayMilliseconds;
                        }

                        //ctor: target
                        var targetConstructedTarget = (WrapperTargetBase)Activator.CreateInstance(targetType, args.ToArray());
                        targetConstructedTarget.Name = name;

                        args.Insert(0, name);

                        //ctor: target+name
                        var namedConstructedTarget = (WrapperTargetBase)Activator.CreateInstance(targetType, args.ToArray());

                        CheckEquals(targetType, targetConstructedTarget, namedConstructedTarget, ref lastPropertyName, ref checkCount);

                        CheckEquals(targetType, defaultConstructedTarget, namedConstructedTarget, ref lastPropertyName, ref checkCount);
                    }
                    else if (isCompound)
                    {
                        neededCheckCount++;

                        //multiple targets
                        var args = new List<object> { fileTarget, memoryTarget };

                        //specials cases


                        //default ctor
                        var defaultConstructedTarget = (CompoundTargetBase)Activator.CreateInstance(targetType);
                        defaultConstructedTarget.Name = name;
                        defaultConstructedTarget.Targets.Add(fileTarget);
                        defaultConstructedTarget.Targets.Add(memoryTarget);

                        //ctor: target
                        var targetConstructedTarget = (CompoundTargetBase)Activator.CreateInstance(targetType, args.ToArray());
                        targetConstructedTarget.Name = name;

                        args.Insert(0, name);

                        //ctor: target+name
                        var namedConstructedTarget = (CompoundTargetBase)Activator.CreateInstance(targetType, args.ToArray());

                        CheckEquals(targetType, targetConstructedTarget, namedConstructedTarget, ref lastPropertyName, ref checkCount);

                        CheckEquals(targetType, defaultConstructedTarget, namedConstructedTarget, ref lastPropertyName, ref checkCount);
                    }
                    else
                    {
                        //default ctor
                        var targetConstructedTarget = (Target)Activator.CreateInstance(targetType);
                        targetConstructedTarget.Name = name;

                        // ctor: name
                        var namedConstructedTarget = (Target)Activator.CreateInstance(targetType, name);

                        CheckEquals(targetType, targetConstructedTarget, namedConstructedTarget, ref lastPropertyName, ref checkCount);
                    }


                }
                catch (Exception ex)
                {
                    var constructionFailed = true;
                    string failureMessage =
                        $"Error testing constructors for '{targetType}.{lastPropertyName}`\n{ex.ToString()}";
                    Assert.False(constructionFailed, failureMessage);
                }
            }
            Assert.Equal(neededCheckCount, checkCount);
        }

        private static void CheckEquals(Type targetType, Target defaultConstructedTarget, Target namedConstructedTarget,
            ref string lastPropertyName, ref int @checked)
        {
            var checkedAtLeastOneProperty = false;

            var properties = targetType.GetProperties(
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.FlattenHierarchy |
                System.Reflection.BindingFlags.Default |

                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Static);
            foreach (System.Reflection.PropertyInfo pi in properties)
            {
                lastPropertyName = pi.Name;
                if (pi.CanRead && !pi.Name.Equals("SyncRoot"))
                {
                    var value1 = pi.GetValue(defaultConstructedTarget, null);
                    var value2 = pi.GetValue(namedConstructedTarget, null);
                    if (value1 != null && value2 != null)
                    {
                        if (value1 is IRenderable)
                        {
                            Assert.Equal((IRenderable)value1, (IRenderable)value2, new RenderableEq());
                        }
                        else if (value1 is AsyncRequestQueue)
                        {
                            Assert.Equal((AsyncRequestQueue)value1, (AsyncRequestQueue)value2, new AsyncRequestQueueEq());
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
                @checked++;
            }
        }

        private class RenderableEq : EqualityComparer<IRenderable>
        {
            /// <summary>
            /// Determines whether the specified objects are equal.
            /// </summary>
            /// <returns>
            /// true if the specified objects are equal; otherwise, false.
            /// </returns>
            /// <param name="x">The first object of type <paramref name="T"/> to compare.</param><param name="y">The second object of type <paramref name="T"/> to compare.</param>
            public override bool Equals(IRenderable x, IRenderable y)
            {
                if (x == null) return y == null;
                var nullEvent = LogEventInfo.CreateNullEvent();
                return x.Render(nullEvent) == y.Render(nullEvent);
            }

            /// <summary>
            /// Returns a hash code for the specified object.
            /// </summary>
            /// <returns>
            /// A hash code for the specified object.
            /// </returns>
            /// <param name="obj">The <see cref="T:System.Object"/> for which a hash code is to be returned.</param><exception cref="T:System.ArgumentNullException">The type of <paramref name="obj"/> is a reference type and <paramref name="obj"/> is null.</exception>
            public override int GetHashCode(IRenderable obj)
            {
                return obj.ToString().GetHashCode();
            }
        }

        private class AsyncRequestQueueEq : EqualityComparer<AsyncRequestQueue>
        {
            /// <summary>
            /// Determines whether the specified objects are equal.
            /// </summary>
            /// <returns>
            /// true if the specified objects are equal; otherwise, false.
            /// </returns>
            /// <param name="x">The first object of type <paramref name="T"/> to compare.</param><param name="y">The second object of type <paramref name="T"/> to compare.</param>
            public override bool Equals(AsyncRequestQueue x, AsyncRequestQueue y)
            {
                if (x == null) return y == null;

                return x.RequestLimit == y.RequestLimit && x.OnOverflow == y.OnOverflow;
            }

            /// <summary>
            /// Returns a hash code for the specified object.
            /// </summary>
            /// <returns>
            /// A hash code for the specified object.
            /// </returns>
            /// <param name="obj">The <see cref="T:System.Object"/> for which a hash code is to be returned.</param><exception cref="T:System.ArgumentNullException">The type of <paramref name="obj"/> is a reference type and <paramref name="obj"/> is null.</exception>
            public override int GetHashCode(AsyncRequestQueue obj)
            {
                unchecked
                {
                    return (obj.RequestLimit * 397) ^ (int)obj.OnOverflow;
                }
            }
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
        public void WriteAsyncLogEvent_InitializeThrowsException_LogContinuationCalledWithCorrectExceptions()
        {
            // Arrange
            var target = new ThrowingInitializeTarget(true);
            Exception retrievedException = null;
            var logevent = LogEventInfo.CreateNullEvent().WithContinuation(ex => { retrievedException = ex; });
            LogManager.ThrowExceptions = false;
            target.Initialize(new LoggingConfiguration());
            LogManager.ThrowExceptions = true;

            // Act
            target.WriteAsyncLogEvent(logevent);

            // Assert
            Assert.NotNull(retrievedException);
            var runtimeException = Assert.IsType<NLogRuntimeException>(retrievedException);
            var innerException = Assert.IsType<TestException>(runtimeException.InnerException);
            Assert.Equal("Initialize says no", innerException.Message);
        }

        [Fact]
        public void Flush_ThrowsException_LogContinuationCalledWithCorrectExceptions()
        {
            // Arrange
            var target = new ThrowingInitializeTarget(false);
            Exception retrievedException = null;
            AsyncContinuation asyncContinuation = ex => { retrievedException = ex; };

            target.Initialize(new LoggingConfiguration());
            LogManager.ThrowExceptions = false;

            // Act
            target.Flush(asyncContinuation);

            // Assert
            Assert.NotNull(retrievedException);

            //note: not wrapped in NLogRuntimeException, not sure if by design.
            Assert.IsType<TestException>(retrievedException);
        }

        [Fact]
        public void WriteAsyncLogEvent_WriteAsyncLogEventThrowsException_LogContinuationCalledWithCorrectExceptions()
        {
            // Arrange
            var target = new ThrowingInitializeTarget(false);
            Exception retrievedException = null;
            var logevent = LogEventInfo.CreateNullEvent().WithContinuation(ex => { retrievedException = ex; });
            target.Initialize(new LoggingConfiguration());
            LogManager.ThrowExceptions = false;

            // Act
            target.WriteAsyncLogEvent(logevent);

            // Assert
            Assert.NotNull(retrievedException);
            //note: not wrapped in NLogRuntimeException, not sure if by design.
            Assert.IsType<TestException>(retrievedException);
            Assert.Equal("Write oops", retrievedException.Message);
        }

        private class ThrowingInitializeTarget : Target
        {
            private readonly bool _throwsOnInit;

            #region Overrides of Target

            /// <inheritdoc />
            public ThrowingInitializeTarget(bool throwsOnInit)
            {
                _throwsOnInit = throwsOnInit;
            }

            /// <inheritdoc />
            protected override void InitializeTarget()
            {
                if (_throwsOnInit)
                    throw new TestException("Initialize says no");
            }

            /// <inheritdoc />
            protected override void FlushAsync(AsyncContinuation asyncContinuation)
            {
                throw new TestException("No flush");
            }

            /// <inheritdoc />
            protected override void WriteAsyncThreadSafe(AsyncLogEventInfo logEvent)
            {
                throw new TestException("Write oops");
            }

            #endregion
        }

        private class TestException : Exception
        {
            /// <inheritdoc />
            public TestException(string message) : base(message)
            {
            }
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
            Assert.Single(exceptions);
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
            Assert.Single(exceptions);
            exceptions.ForEach(Assert.Null);
        }

        [Fact]
        public void FlushWithoutInitializeTest()
        {
            var target = new MyTarget();
            List<Exception> exceptions = new List<Exception>();
            target.Flush(exceptions.Add);

            Assert.Single(exceptions);
            exceptions.ForEach(Assert.Null);

            // flush was not called
            Assert.Equal(0, target.InitializeCount + target.FlushCount + target.CloseCount + target.WriteCount + target.WriteCount2 + target.WriteCount3);
        }

        [Theory]
        [InlineData("Trace")]
        [InlineData("TRACE")]
        [InlineData("TraceSystem")]
        [InlineData("TraceSYSTEM")]
        public void TargetAliasShouldWork(string typeName)
        {
            LoggingConfiguration c = XmlLoggingConfiguration.CreateFromXmlString($@"
            <nlog>
                <targets>
                    <target name='d' type='{typeName}' />
                </targets>
            </nlog>");

            var t = c.FindTargetByName<TraceTarget>("d");
            Assert.NotNull(t);
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

            Assert.Single(exceptions);
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
                    target.BlockingOperation(500);
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
                Name = name;
            }

            protected override void InitializeTarget()
            {
                if (ThrowOnInitialize)
                {
                    throw new InvalidOperationException("Init error.");
                }

                Assert.Equal(0, inBlockingOperation);
                InitializeCount++;
                base.InitializeTarget();
            }

            protected override void CloseTarget()
            {
                Assert.Equal(0, inBlockingOperation);
                CloseCount++;
                base.CloseTarget();
            }

            protected override void FlushAsync(AsyncContinuation asyncContinuation)
            {
                Assert.Equal(0, inBlockingOperation);
                FlushCount++;
                base.FlushAsync(asyncContinuation);
            }

            protected override void Write(LogEventInfo logEvent)
            {
                Assert.Equal(0, inBlockingOperation);
                WriteCount++;
            }

            protected override void Write(AsyncLogEventInfo logEvent)
            {
                Assert.Equal(0, inBlockingOperation);
                WriteCount2++;
                base.Write(logEvent);
            }

            protected override void Write(IList<AsyncLogEventInfo> logEvents)
            {
                Assert.Equal(0, inBlockingOperation);
                WriteCount3++;
                base.Write(logEvents);
            }

            public void BlockingOperation(int millisecondsTimeout)
            {
                lock (SyncRoot)
                {
                    inBlockingOperation++;
                    Thread.Sleep(millisecondsTimeout);
                    inBlockingOperation--;
                }
            }
        }

        [Fact]
        public void WrongMyTargetShouldNotThrowExceptionWhenThrowExceptionsIsFalse()
        {
            using (new NoThrowNLogExceptions())
            {
                var target = new WrongMyTarget();
                SimpleConfigurator.ConfigureForTargetLogging(target);
                var logger = LogManager.GetLogger("WrongMyTargetShouldThrowException");
                logger.Info("Testing");
            }
        }


        public class WrongMyTarget : Target
        {
            public WrongMyTarget() : base()
            {
            }

            public WrongMyTarget(string name) : this()
            {
                Name = name;
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


        [Fact]
        public void TypedLayoutTargetTest()
        {
            LoggingConfiguration c = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <extensions>
                    <add type='" + typeof(MyTypedLayoutTarget).AssemblyQualifiedName + @"' />
                </extensions>

                <targets>
                    <target type='MyTypedLayoutTarget' name='myTarget'
                        byteProperty='42' 
                        int16Property='43' 
                        int32Property='44' 
                        int64Property='45000000000' 
                        stringProperty='foobar'
                        boolProperty='true'
                        doubleProperty='3.14159'
                        floatProperty='3.24159'
                        enumProperty='Value3'
                        flagsEnumProperty='Value1,Value3'
                        encodingProperty='utf-8'
                        cultureProperty='en-US'
                        typeProperty='System.Int32'
                        uriProperty='https://nlog-project.org'
                        lineEndingModeProperty='default'
                        />
                </targets>
            </nlog>");

            var nullEvent = LogEventInfo.CreateNullEvent();
            var myTarget = c.FindTargetByName("myTarget") as MyTypedLayoutTarget;
            Assert.NotNull(myTarget);
            Assert.Equal((byte)42, myTarget.ByteProperty.StaticValue);
            Assert.Equal((short)43, myTarget.Int16Property.StaticValue);
            Assert.Equal(44, myTarget.Int32Property.StaticValue);
            Assert.Equal(45000000000L, myTarget.Int64Property.StaticValue);
            Assert.Equal("foobar", myTarget.StringProperty.StaticValue);
            Assert.True(myTarget.BoolProperty.StaticValue);
            Assert.Equal(3.14159, myTarget.DoubleProperty.StaticValue);
            Assert.Equal(3.24159f, myTarget.FloatProperty.StaticValue);
            Assert.Equal(TargetConfigurationTests.MyEnum.Value3, myTarget.EnumProperty.StaticValue);
            Assert.Equal(TargetConfigurationTests.MyFlagsEnum.Value1 | TargetConfigurationTests.MyFlagsEnum.Value3, myTarget.FlagsEnumProperty.StaticValue);
            Assert.Equal(Encoding.UTF8, myTarget.EncodingProperty.StaticValue);
            Assert.Equal("en-US", myTarget.CultureProperty.StaticValue.Name);
            Assert.Equal(typeof(int), myTarget.TypeProperty.StaticValue);
            Assert.Equal(new Uri("https://nlog-project.org"), myTarget.UriProperty.StaticValue);
            Assert.Equal(LineEndingMode.Default, myTarget.LineEndingModeProperty.StaticValue);
        }

        [Fact]
        public void TypedLayoutTargetAsyncTest()
        {
            // Arrange
            LogFactory logFactory = new LogFactory();
            LoggingConfiguration c = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog throwExceptions='true'>
                <extensions>
                    <add type='" + typeof(MyTypedLayoutTarget).AssemblyQualifiedName + @"' />
                </extensions>

                <variable name='value3' value='Value3' />

                <targets async='true'>
                    <target type='MyTypedLayoutTarget' name='myTarget'
                        byteProperty='42' 
                        int16Property='43' 
                        int32Property='${threadid}' 
                        int64Property='${sequenceid}' 
                        stringProperty='${appdomain:format=\{1\}}'
                        boolProperty='true'
                        doubleProperty='3.14159'
                        floatProperty='3.24159'
                        enumProperty='${var:value3}'
                        flagsEnumProperty='Value1,Value3'
                        encodingProperty='utf-8'
                        cultureProperty='en-US'
                        typeProperty='System.Int32'
                        uriProperty='https://nlog-project.org'
                        lineEndingModeProperty='default'
                        />
                </targets>

                <rules>
                    <logger minlevel='trace' writeTo='myTarget' />
                </rules>
            </nlog>", logFactory);
            logFactory.Configuration = c;

            // Act
            var logger = logFactory.GetLogger(nameof(TypedLayoutTargetAsyncTest));
            var logEvent = new LogEventInfo(LogLevel.Info, null, "Hello");
            logger.Log(logEvent);
            logFactory.Flush();

            // Assert
            Assert.Equal((byte)42, logEvent.Properties["ByteProperty"]);
            Assert.Equal((short)43, logEvent.Properties["Int16Property"]);
            Assert.Equal(Thread.CurrentThread.ManagedThreadId, logEvent.Properties["Int32Property"]);
            Assert.Equal((long)logEvent.SequenceID, logEvent.Properties["Int64Property"]);
            Assert.Equal(AppDomain.CurrentDomain.FriendlyName, logEvent.Properties["StringProperty"]);
            Assert.Equal(true, logEvent.Properties["BoolProperty"]);
            Assert.Equal(3.14159, logEvent.Properties["DoubleProperty"]);
            Assert.Equal(3.24159f, logEvent.Properties["FloatProperty"]);
            Assert.Equal(TargetConfigurationTests.MyEnum.Value3, logEvent.Properties["EnumProperty"]);
            Assert.Equal(TargetConfigurationTests.MyFlagsEnum.Value1 | TargetConfigurationTests.MyFlagsEnum.Value3, logEvent.Properties["FlagsEnumProperty"]);
            Assert.Equal(Encoding.UTF8, logEvent.Properties["EncodingProperty"]);
            Assert.Equal(CultureInfo.GetCultureInfo("en-US"), logEvent.Properties["CultureProperty"]);
            Assert.Equal(typeof(int), logEvent.Properties["TypeProperty"]);
            Assert.Equal(new Uri("https://nlog-project.org"), logEvent.Properties["UriProperty"]);
            Assert.Equal(LineEndingMode.Default, logEvent.Properties["LineEndingModeProperty"]);
        }

        [Target("MyTypedLayoutTarget")]
        public class MyTypedLayoutTarget : Target
        {
            public Layout<byte> ByteProperty { get; set; }

            public Layout<short> Int16Property { get; set; }

            public Layout<int> Int32Property { get; set; }

            public Layout<long> Int64Property { get; set; }

            public Layout<string> StringProperty { get; set; }

            public Layout<bool> BoolProperty { get; set; }

            public Layout<double> DoubleProperty { get; set; }

            public Layout<float> FloatProperty { get; set; }

            public Layout<TargetConfigurationTests.MyEnum> EnumProperty { get; set; }

            public Layout<TargetConfigurationTests.MyFlagsEnum> FlagsEnumProperty { get; set; }

            public Layout<Encoding> EncodingProperty { get; set; }

            public Layout<CultureInfo> CultureProperty { get; set; }

            public Layout<Type> TypeProperty { get; set; }

            public Layout<Uri> UriProperty { get; set; }

            public Layout<LineEndingMode> LineEndingModeProperty { get; set; }

            protected override void Write(LogEventInfo logEvent)
            {
                logEvent.Properties[nameof(ByteProperty)] = RenderLogEvent(ByteProperty, logEvent);
                logEvent.Properties[nameof(Int16Property)] = RenderLogEvent(Int16Property, logEvent);
                logEvent.Properties[nameof(Int32Property)] = RenderLogEvent(Int32Property, logEvent);
                logEvent.Properties[nameof(Int64Property)] = RenderLogEvent(Int64Property, logEvent);
                logEvent.Properties[nameof(StringProperty)] = RenderLogEvent(StringProperty, logEvent);
                logEvent.Properties[nameof(BoolProperty)] = RenderLogEvent(BoolProperty, logEvent);
                logEvent.Properties[nameof(DoubleProperty)] = RenderLogEvent(DoubleProperty, logEvent);
                logEvent.Properties[nameof(FloatProperty)] = RenderLogEvent(FloatProperty, logEvent);
                logEvent.Properties[nameof(EnumProperty)] = RenderLogEvent(EnumProperty, logEvent);
                logEvent.Properties[nameof(FlagsEnumProperty)] = RenderLogEvent(FlagsEnumProperty, logEvent);
                logEvent.Properties[nameof(EncodingProperty)] = RenderLogEvent(EncodingProperty, logEvent);
                logEvent.Properties[nameof(CultureProperty)] = RenderLogEvent(CultureProperty, logEvent);
                logEvent.Properties[nameof(TypeProperty)] = RenderLogEvent(TypeProperty, logEvent);
                logEvent.Properties[nameof(UriProperty)] = RenderLogEvent(UriProperty, logEvent);
                logEvent.Properties[nameof(LineEndingModeProperty)] = RenderLogEvent(LineEndingModeProperty, logEvent);
            }
        }
    }
}
