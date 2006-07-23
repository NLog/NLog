// 
// Copyright (c) 2004,2005 Jaroslaw Kowalski <jkowalski@users.sourceforge.net>
// 
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
// * Neither the name of the Jaroslaw Kowalski nor the names of its 
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

using System;
using System.Net;
using System.Net.Sockets;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using System.Windows.Forms;
using System.Collections.Specialized;
using System.Messaging;

using NLogViewer.Configuration;
using NLogViewer.Events;
using NLogViewer.Parsers;
using System.ComponentModel;

namespace NLogViewer.Receivers
{
    [LogEventReceiver("MSMQ", "MSMQ Receiver", "Receives events from the MSMQ Queue")]
    public class MsmqReceiver : LogEventReceiverWithParserSkeleton
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private string _queueName = ".\\private$\\NLogViewer";

        public MsmqReceiver()
        {
        }

        [DefaultValue(".\\private$\\NLogViewer")]
        public string QueueName
        {
            get { return _queueName; }
            set { _queueName = value; }
        }

        public override void InputThread()
        {
            try
            {
                using (MessageQueue mq = new MessageQueue(QueueName))
                {
                    while (!InputThreadQuitRequested())
                    {
                        System.Messaging.Message msg;

                        try
                        {
                            msg = mq.Receive(TimeSpan.FromMilliseconds(100));
                        }
                        catch (Exception ex)
                        {
                            continue;
                        }

                        using (Stream s = msg.BodyStream)
                        {
                            using (ILogEventParserInstance parserInstance = Parser.Begin(s))
                            {
                                LogEvent logEventInfo = CreateLogEvent();
                                if (parserInstance.ReadNext(logEventInfo))
                                    EventReceived(logEventInfo);
                            }
                        }
                    }
                }
            }
            finally
            {
            }
        }
    }
}
