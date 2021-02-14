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

namespace NLog.MSMQ.Tests
{
    using System.Collections.Generic;
    using System.Messaging;
    using NLog.Targets;
    using Xunit;

    public class MessageQueueTargetTests
    {
        public MessageQueueTargetTests()
        {
            LogManager.ThrowExceptions = true;
        }

        [Fact]
        public void QueueExists_Write_MessageIsWritten()
        {
            var messageQueueTestProxy = new MessageQueueTestProxy
                                        {
                                            QueueExists = true,
                                        };
            var logFactory = SetupMsmqTarget(messageQueueTestProxy, false);

            logFactory.GetCurrentClassLogger().Fatal("Test Message");

            Assert.Equal(1, messageQueueTestProxy.SentMessages.Count);
        }

        [Fact]
        public void QueueDoesNotExistsAndDoNotCreate_Write_NothingIsWritten()
        {
            var messageQueueTestProxy = new MessageQueueTestProxy
                                        {
                                            QueueExists = false,
                                        };
            var logFactory = SetupMsmqTarget(messageQueueTestProxy, false);

            logFactory.GetCurrentClassLogger().Fatal("Test Message");

            Assert.Equal(0, messageQueueTestProxy.SentMessages.Count);
        }

        [Fact]
        public void QueueDoesNotExistsAndCreatedQueue_Write_QueueIsCreated()
        {
            var messageQueueTestProxy = new MessageQueueTestProxy
                                        {
                                            QueueExists = false,
                                        };
            var logFactory = SetupMsmqTarget(messageQueueTestProxy, true);

            logFactory.GetCurrentClassLogger().Fatal("Test Message");

            Assert.True(messageQueueTestProxy.QueueCreated);
        }

        [Fact]
        public void QueueDoesNotExistsAndCreatedQueue_Write_MessageIsWritten()
        {
            var messageQueueTestProxy = new MessageQueueTestProxy
                                        {
                                            QueueExists = false,
                                        };
            var logFactory = SetupMsmqTarget(messageQueueTestProxy, true);

            logFactory.GetCurrentClassLogger().Fatal("Test Message");

            Assert.Equal(1, messageQueueTestProxy.SentMessages.Count);
        }

        [Fact]
        public void FormatQueueName_Write_DoesNotCheckIfQueueExists()
        {
            var messageQueueTestProxy = new MessageQueueTestProxy();
            var logFactory = SetupMsmqTarget(messageQueueTestProxy, false, "DIRECT=http://test.com/MSMQ/queue");

            logFactory.GetCurrentClassLogger().Fatal("Test Message");

            Assert.False(messageQueueTestProxy.QueueExistsCalled);
        }

        [Fact]
        public void DoNotCheckIfQueueExists_Write_DoesNotCheckIfQueueExists()
        {
            var messageQueueTestProxy = new MessageQueueTestProxy();
            var logFactory = SetupMsmqTarget(messageQueueTestProxy, false, checkIfQueueExists: false);

            logFactory.GetCurrentClassLogger().Fatal("Test Message");

            Assert.False(messageQueueTestProxy.QueueExistsCalled);
        }

        /// <summary>
        /// Checks if setting the CheckIfQueueExists is working
        /// </summary>
        [Fact]
        public void MessageQueueTarget_CheckIfQueueExists_setting_should_work()
        {
            var logFactory = new LogFactory().Setup().LoadConfigurationFromXml(@"
                <nlog throwExceptions='true' >
                    <extensions>
                        <add assembly='NLog.MSMQ' />
                    </extensions>
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
                </nlog>").LogFactory;
            var messageQueueTarget = logFactory.Configuration.FindTargetByName("q") as MessageQueueTarget;

            Assert.NotNull(messageQueueTarget);
            Assert.False(messageQueueTarget.CheckIfQueueExists);
        }

        private static LogFactory SetupMsmqTarget(MessageQueueProxy messageQueueTestProxy, bool createQueue, string queueName = "Test", bool checkIfQueueExists = true)
        {
            return new LogFactory().Setup().LoadConfiguration(cfg =>
            {
                var target = new MessageQueueTarget
                {
                    MessageQueueProxy = messageQueueTestProxy,
                    Queue = queueName,
                    CreateQueueIfNotExists = createQueue,
                    CheckIfQueueExists = checkIfQueueExists,
                };
                cfg.Configuration.AddRuleForAllLevels(target);
            }).LogFactory;
        }

        internal class MessageQueueTestProxy : MessageQueueProxy
        {
            public IList<Message> SentMessages { get; private set; }

            public bool QueueExists { get; set; }

            public bool QueueCreated { get; private set; }

            public bool QueueExistsCalled { get; private set; }

            public MessageQueueTestProxy()
            {
                SentMessages = new List<Message>();
            }

            public override bool Exists(string queue)
            {
                QueueExistsCalled = true;
                return QueueExists;
            }

            public override void Create(string queue)
            {
                QueueCreated = true;
            }

            public override void Send(string queue, Message message)
            {
                SentMessages.Add(message);
            }
        }
    }
}