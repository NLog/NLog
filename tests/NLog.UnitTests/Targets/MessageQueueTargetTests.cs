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

#if  !MONO

namespace NLog.UnitTests.Targets
{
    using System.Collections.Generic;
    using System.Messaging;
    using NLog.Targets;
    using Xunit;

    public class MessageQueueTargetTests : NLogTestBase
    {
        [Fact]
        public void QueueExists_Write_MessageIsWritten()
        {
            var messageQueueTestProxy = new MessageQueueTestProxy
                                        {
                                            QueueExists = true,
                                        };
            var target = CreateTarget(messageQueueTestProxy, false);

            target.WriteAsyncLogEvent(new LogEventInfo().WithContinuation(_ => { }));

            Assert.Equal(1, messageQueueTestProxy.SentMessages.Count);
        }

        [Fact]
        public void QueueDoesNotExistsAndDoNotCreate_Write_NothingIsWritten()
        {
            var messageQueueTestProxy = new MessageQueueTestProxy
                                        {
                                            QueueExists = false,
                                        };
            var target = CreateTarget(messageQueueTestProxy, false);

            target.WriteAsyncLogEvent(new LogEventInfo().WithContinuation(_ => { }));

            Assert.Equal(0, messageQueueTestProxy.SentMessages.Count);
        }

        [Fact]
        public void QueueDoesNotExistsAndCreatedQueue_Write_QueueIsCreated()
        {
            var messageQueueTestProxy = new MessageQueueTestProxy
                                        {
                                            QueueExists = false,
                                        };
            var target = CreateTarget(messageQueueTestProxy, true);

            target.WriteAsyncLogEvent(new LogEventInfo().WithContinuation(_ => { }));

            Assert.True(messageQueueTestProxy.QueueCreated);
        }

        [Fact]
        public void QueueDoesNotExistsAndCreatedQueue_Write_MessageIsWritten()
        {
            var messageQueueTestProxy = new MessageQueueTestProxy
                                        {
                                            QueueExists = false,
                                        };
            var target = CreateTarget(messageQueueTestProxy, true);

            target.WriteAsyncLogEvent(new LogEventInfo().WithContinuation(_ => { }));

            Assert.Equal(1, messageQueueTestProxy.SentMessages.Count);
        }

        [Fact]
        public void FormatQueueName_Write_DoesNotCheckIfQueueExists()
        {
            var messageQueueTestProxy = new MessageQueueTestProxy();
            var target = CreateTarget(messageQueueTestProxy, false, "DIRECT=http://test.com/MSMQ/queue");

            target.WriteAsyncLogEvent(new LogEventInfo().WithContinuation(_ => { }));

            Assert.False(messageQueueTestProxy.QueueExistsCalled);
        }

        [Fact]
        public void DoNotCheckIfQueueExists_Write_DoesNotCheckIfQueueExists()
        {
            var messageQueueTestProxy = new MessageQueueTestProxy();
            var target = CreateTarget(messageQueueTestProxy, false, checkIfQueueExists: false);

            target.WriteAsyncLogEvent(new LogEventInfo().WithContinuation(_ => { }));

            Assert.False(messageQueueTestProxy.QueueExistsCalled);
        }

        /// <summary>
        /// Checks if setting the CheckIfQueueExists is working
        /// </summary>
        [Fact]
        public void MessageQueueTarget_CheckIfQueueExists_setting_should_work()
        {
            var configuration = CreateConfigurationFromString(string.Format(@"
                <nlog throwExceptions='true' >
                    <targets>
                        <target type='MSMQ'
                                name='q'
                                checkIfQueueExists='False' 
                                queue='queue1' >
                        </target>
                    </targets>
                    <rules>
                      <logger name='*' writeTo='q'>
                       
                      </logger>
                    </rules>
                </nlog>"));


            LogManager.Configuration = configuration;
            var messageQueueTarget = configuration.FindTargetByName("q") as MessageQueueTarget;

            Assert.NotNull(messageQueueTarget);
            Assert.Equal(false, messageQueueTarget.CheckIfQueueExists);
        }

        private static MessageQueueTarget CreateTarget(MessageQueueProxy messageQueueTestProxy, bool createQueue, string queueName = "Test", bool checkIfQueueExists = true)
        {
            var target = new MessageQueueTarget
                         {
                             MessageQueueProxy = messageQueueTestProxy,
                             Queue = queueName,
                             CreateQueueIfNotExists = createQueue,
                             CheckIfQueueExists = checkIfQueueExists,
                         };
            target.Initialize(null);
            return target;
        }

        internal class MessageQueueTestProxy : MessageQueueProxy
        {
            public IList<Message> SentMessages { get; private set; }

            public bool QueueExists { get; set; }

            public bool QueueCreated { get; private set; }

            public bool QueueExistsCalled { get; private set; }

            public MessageQueueTestProxy()
            {
                this.SentMessages = new List<Message>();
            }

            public override bool Exists(string queue)
            {
                this.QueueExistsCalled = true;
                return this.QueueExists;
            }

            public override void Create(string queue)
            {
                this.QueueCreated = true;
            }

            public override void Send(string queue, Message message)
            {
                SentMessages.Add(message);
            }
        }
    }
}

#endif
