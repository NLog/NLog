// 
// Copyright (c) 2004-2010 Jaroslaw Kowalski <jaak@jkowalski.net>
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

namespace NLog.Targets
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Text;
    using NLog.Common;
    using NLog.Internal;
    using NLog.Internal.NetworkSenders;
    using NLog.Layouts;

    /// <summary>
    /// Sends log messages over the network.
    /// </summary>
    /// <seealso href="http://nlog-project.org/wiki/Network_target">Documentation on NLog Wiki</seealso>
    /// <example>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" source="examples/targets/Configuration File/Network/NLog.config" />
    /// <p>
    /// This assumes just one target and a single rule. More configuration
    /// options are described <a href="config.html">here</a>.
    /// </p>
    /// <p>
    /// To set up the log target programmatically use code like this:
    /// </p>
    /// <code lang="C#" source="examples/targets/Configuration API/Network/Simple/Example.cs" />
    /// <p>
    /// To print the results, use any application that's able to receive messages over
    /// TCP or UDP. <a href="http://m.nu/program/util/netcat/netcat.html">NetCat</a> is
    /// a simple but very powerful command-line tool that can be used for that. This image
    /// demonstrates the NetCat tool receiving log messages from Network target.
    /// </p>
    /// <img src="examples/targets/Screenshots/Network/Output.gif" />
    /// <p>
    /// NOTE: If your receiver application is ever likely to be off-line, don't use TCP protocol
    /// or you'll get TCP timeouts and your application will crawl. 
    /// Either switch to UDP transport or use <a href="target.AsyncWrapper.html">AsyncWrapper</a> target
    /// so that your application threads will not be blocked by the timing-out connection attempts.
    /// </p>
    /// <p>
    /// There are two specialized versions of the Network target: <a href="target.Chainsaw.html">Chainsaw</a>
    /// and <a href="target.NLogViewer.html">NLogViewer</a> which write to instances of Chainsaw log4j viewer
    /// or NLogViewer application respectively.
    /// </p>
    /// </example>
    [Target("Network")]
    public class NetworkTarget : TargetWithLayout
    {
        private NetworkSender sender;

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkTarget" /> class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message}</code>
        /// </remarks>
        public NetworkTarget()
        {
            this.Encoding = System.Text.Encoding.UTF8;
            this.OnOverflow = OverflowAction.Split;
            this.KeepConnection = true;
            this.MaxMessageSize = 65000;
        }

        /// <summary>
        /// Action that should be taken if the message overflows.
        /// </summary>
        public enum OverflowAction
        {
            /// <summary>
            /// Report an error.
            /// </summary>
            Error,

            /// <summary>
            /// Split the message into smaller pieces.
            /// </summary>
            Split,

            /// <summary>
            /// Discard the entire message.
            /// </summary>
            Discard
        }

        /// <summary>
        /// Gets or sets the network address. Can be tcp://host:port or udp://host:port.
        /// </summary>
        /// <remarks>
        /// For HTTP Support use the WebService target.
        /// </remarks>
        /// <docgen category='Connection Options' order='10' />
        public Layout Address { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to keep connection open whenever possible.
        /// </summary>
        /// <docgen category='Connection Options' order='10' />
        [DefaultValue(true)]
        public bool KeepConnection { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to append newline at the end of log message.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        [DefaultValue(false)]
        public bool NewLine { get; set; }

        /// <summary>
        /// Gets or sets the maximum message size in bytes.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        [DefaultValue(65000)]
        public int MaxMessageSize { get; set; }

        /// <summary>
        /// Gets or sets the action that should be taken if the message is larger than
        /// maxMessageSize.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public OverflowAction OnOverflow { get; set; }

        /// <summary>
        /// Gets or sets the encoding to be used.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        [DefaultValue("utf-8")]
        public Encoding Encoding { get; set; }

        /// <summary>
        /// Flush any pending log messages asynchronously (in case of asynchronous targets).
        /// </summary>
        /// <param name="asyncContinuation">The asynchronous continuation.</param>
        protected override void FlushAsync(AsyncContinuation asyncContinuation)
        {
            try
            {
                if (this.sender != null)
                {
                    this.sender.Flush();
                }
            }
            catch (Exception ex)
            {
                asyncContinuation(ex);
            }
        }

        /// <summary>
        /// Closes the target.
        /// </summary>
        protected override void Close()
        {
            base.Close();
            if (this.sender != null)
            {
                this.sender.Close();
            }
        }

        /// <summary>
        /// Sends the 
        /// rendered logging event over the network optionally concatenating it with a newline character.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        protected override void Write(LogEventInfo logEvent)
        {
            this.NetworkSend(this.Address.Render(logEvent), this.GetBytesToWrite(logEvent));
        }

        /// <summary>
        /// Sends the provided text to the specified address.
        /// </summary>
        /// <param name="address">The address. Can be tcp://host:port, udp://host:port, http://host:port.</param>
        /// <param name="bytes">The bytes to be sent.</param>
        protected virtual void NetworkSend(string address, byte[] bytes)
        {
            if (this.KeepConnection)
            {
                if (this.sender != null)
                {
                    if (this.sender.Address != address)
                    {
                        this.sender.Close();
                        this.sender = null;
                    }
                }

                if (this.sender == null)
                {
                    this.sender = NetworkSender.Create(address);
                }

                try
                {
                    this.ChunkedSend(this.sender, bytes);
                }
                catch (Exception ex)
                {
                    InternalLogger.Error("Error when sending {0}", ex);
                    this.sender.Close();
                    this.sender = null;
                    throw;
                }
            }
            else
            {
                NetworkSender sender = NetworkSender.Create(address);

                try
                {
                    this.ChunkedSend(sender, bytes);
                }
                finally
                {
                    sender.Close();
                }
            }
        }

        /// <summary>
        /// Gets the bytes to be written.
        /// </summary>
        /// <param name="logEvent">Log event.</param>
        /// <returns>Byte array.</returns>
        protected virtual byte[] GetBytesToWrite(LogEventInfo logEvent)
        {
            string text;

            if (this.NewLine)
            {
                text = this.Layout.Render(logEvent) + "\r\n";
            }
            else
            {
                text = this.Layout.Render(logEvent);
            }

            return this.Encoding.GetBytes(text);
        }

        private void ChunkedSend(NetworkSender sender, byte[] buffer)
        {
            int tosend = buffer.Length;
            int pos = 0;

            while (tosend > 0)
            {
                int chunksize = tosend;
                if (chunksize > this.MaxMessageSize)
                {
                    if (this.OnOverflow == OverflowAction.Discard)
                    {
                        return;
                    }

                    if (this.OnOverflow == OverflowAction.Error)
                    {
                        throw new OverflowException("Attempted to send a message larger than this.MaxMessageSize(" + this.MaxMessageSize + "). Actual size was: " + buffer.Length + ". Adjust OnOverflow and this.MaxMessageSize parameters accordingly.");
                    }

                    chunksize = this.MaxMessageSize;
                }

                sender.Send(buffer, pos, chunksize);
                tosend -= chunksize;
                pos += chunksize;
            }
        }
    }
}
