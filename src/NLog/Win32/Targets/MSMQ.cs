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

#if !NETCF_1_0

using System;
using System.Messaging;
using System.Text;

using NLog.Config;
using System.ComponentModel;

namespace NLog.Win32.Targets
{
    /// <summary>
    /// Writes log message to the specified message queue handled by MSMQ.
    /// </summary>
    /// <example>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" src="examples/targets/Configuration File/MSMQ/Simple/NLog.config" />
    /// <p>
    /// You can use a single target to write to multiple queues (similar to writing to multiple files with the File target).
    /// </p>
    /// <code lang="XML" src="examples/targets/Configuration File/MSMQ/Multiple/NLog.config" />
    /// <p>
    /// The above examples assume just one target and a single rule. 
    /// More configuration options are described <a href="config.html">here</a>.
    /// </p>
    /// <p>
    /// To set up the log target programmatically use code like this:
    /// </p>
    /// <code lang="C#" src="examples/targets/Configuration API/MSMQ/Simple/Example.cs" />
    /// </example>
    [Target("MSMQ")]
    [SupportedRuntime(Framework=RuntimeFramework.DotNetFramework)]
    [SupportedRuntime(Framework=RuntimeFramework.DotNetCompactFramework,MinRuntimeVersion="2.0")]
    public class MSMQTarget : TargetWithLayout
    {
        private Layout _queue;
        private Layout _label = new Layout("NLog");
        private bool _createIfNotExists;
        private Encoding _encoding = System.Text.Encoding.UTF8;
        private bool _useXmlEncoding;
        private MessagePriority _messagePriority = MessagePriority.Normal;
        private bool _recoverableMessages;

        /// <summary>
        /// Name of the queue to write to.
        /// </summary>
        /// <remarks>
        /// To write to a private queue on a local machine use <c>.\private$\QueueName</c>.
        /// For other available queue names, consult MSMQ documentation.
        /// </remarks>
        [RequiredParameter]
        [AcceptsLayout]
        public string Queue
        {
            get { return _queue.Text; }
            set { _queue = new Layout(value); }
        }

        /// <summary>
        /// The label to associate with each message.
        /// </summary>
        /// <remarks>
        /// By default no label is associated.
        /// </remarks>
        [AcceptsLayout]
        [DefaultValue("NLog")]
        public string Label
        {
            get { return _label.Text; }
            set { _label = new Layout(value); }
        }

        /// <summary>
        /// Create the queue if it doesn't exists.
        /// </summary>
        [System.ComponentModel.DefaultValue(false)]
        public bool CreateQueueIfNotExists
        {
            get { return _createIfNotExists; }
            set { _createIfNotExists = value; }
        }

        /// <summary>
        /// Encoding to be used when writing text to the queue.
        /// </summary>
        public string Encoding
        {
            get { return _encoding.WebName; }
            set { _encoding = System.Text.Encoding.GetEncoding(value); }
        }

        /// <summary>
        /// Use the XML format when serializing message.
        /// </summary>
        [System.ComponentModel.DefaultValue(false)]
        public bool UseXmlEncoding
        {
            get { return _useXmlEncoding; }
            set { _useXmlEncoding = value; }
        }

        /// <summary>
        /// Use recoverable messages (with guaranteed delivery).
        /// </summary>
        [System.ComponentModel.DefaultValue(false)]
        public bool Recoverable
        {
            get { return _recoverableMessages; }
            set { _recoverableMessages = value; }
        }

        /// <summary>
        /// Adds all layouts used by this target to the specified collection.
        /// </summary>
        /// <param name="layouts">The collection to add layouts to.</param>
        public override void PopulateLayouts(LayoutCollection layouts)
        {
            base.PopulateLayouts (layouts);
            _queue.PopulateLayouts(layouts);
            _label.PopulateLayouts(layouts);
        }

        /// <summary>
        /// Writes the specified logging event to a queue specified in the Queue 
        /// parameter.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        protected internal override void Write(LogEventInfo logEvent)
        {
            if (_queue == null)
                return;

            string queue = _queue.GetFormattedMessage(logEvent);

            if (!MessageQueue.Exists(queue))
            {
                if (CreateQueueIfNotExists)
                    MessageQueue.Create(queue);
                else
                    return;
            }

            using (MessageQueue mq = new MessageQueue(queue))
            {
                Message msg = PrepareMessage(logEvent);
                if (msg != null)
                {
                    mq.Send(msg);
                }
            }
        }

        /// <summary>
        /// Prepares a message to be sent to the message queue.
        /// </summary>
        /// <param name="logEvent">The log event to be used when calculating label and text to be written.</param>
        /// <returns>The message to be sent</returns>
        /// <remarks>
        /// You may override this method in inheriting classes
        /// to provide services like encryption or message 
        /// authentication.
        /// </remarks>
        protected virtual Message PrepareMessage(LogEventInfo logEvent)
        {
            Message msg = new Message();
            if (_label != null)
            {
                msg.Label = _label.GetFormattedMessage(logEvent);
            }
            msg.Recoverable = _recoverableMessages;
            msg.Priority = _messagePriority;

            if (_useXmlEncoding)
            {
                msg.Body = CompiledLayout.GetFormattedMessage(logEvent);
            }
            else
            {
                byte[] dataBytes = _encoding.GetBytes(CompiledLayout.GetFormattedMessage(logEvent));

                msg.BodyStream.Write(dataBytes, 0, dataBytes.Length);
            }
            return msg;
        }
	}
}

#endif
