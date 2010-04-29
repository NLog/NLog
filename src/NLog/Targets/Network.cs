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

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading;

using NLog.Internal;
using NLog.Internal.NetworkSenders;
using System.Text;
using System.ComponentModel;

namespace NLog.Targets
{
    /// <summary>
    /// Sends logging messages over the network.
    /// </summary>
    /// <example>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" src="examples/targets/Configuration File/Network/NLog.config" />
    /// <p>
    /// This assumes just one target and a single rule. More configuration
    /// options are described <a href="config.html">here</a>.
    /// </p>
    /// <p>
    /// To set up the log target programmatically use code like this:
    /// </p>
    /// <code lang="C#" src="examples/targets/Configuration API/Network/Simple/Example.cs" />
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
    public class NetworkTarget: TargetWithLayout
    {
        private bool _newline = false;
        private bool _keepConnection = true;
        private Layout _addressLayout = null;
        private NetworkSender _sender = null;
        private Encoding _encoding = System.Text.Encoding.UTF8;
        private OverflowAction _onOverflow = OverflowAction.Split;

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
            /// Discard the entire message
            /// </summary>
            Discard
        }

        /// <summary>
        /// The network address. Can be tcp://host:port or udp://host:port
        /// </summary>
        /// <remarks>
        /// For HTTP Support use the WebService target.
        /// </remarks>
        public string Address
        {
            get { return _addressLayout.Text; }
            set 
            {
                _addressLayout = new Layout(value); 
                if (_sender != null)
                {
                    _sender.Close();
                    _sender = null;
                }
            }
        }

        /// <summary>
        /// The network address. Can be tcp://host:port, udp://host:port, http://host:port or https://host:port
        /// </summary>
        public Layout AddressLayout
        {
            get { return _addressLayout; }
            set { _addressLayout = value; }
        }

        /// <summary>
        /// Keep connection open whenever possible.
        /// </summary>
        [System.ComponentModel.DefaultValue(true)]
        public bool KeepConnection
        {
            get { return _keepConnection; }
            set { _keepConnection = value; }
        }

        /// <summary>
        /// Append newline at the end of log message.
        /// </summary>
        [System.ComponentModel.DefaultValue(false)]
        public bool NewLine
        {
            get { return _newline; }
            set { _newline = value; }
        }

        private int _maxMessageSize = 65000;

        /// <summary>
        /// Maximum message size in bytes.
        /// </summary>
        [DefaultValue(65000)]
        public int MaxMessageSize
        {
            get { return _maxMessageSize; }
            set { _maxMessageSize = value; }
        }
	

        /// <summary>
        /// Action that should be taken if the message is larger than
        /// maxMessageSize
        /// </summary>
        public OverflowAction OnOverflow
        {
            get { return _onOverflow; }
            set { _onOverflow = value; }
        }

        /// <summary>
        /// Encoding
        /// </summary>
        /// <remarks>
        /// Can be any encoding name supported by System.Text.Encoding.GetEncoding() e.g. <c>windows-1252</c>, <c>iso-8859-2</c>.
        /// </remarks>
        [System.ComponentModel.DefaultValue("utf-8")]
        public string Encoding
        {
            get { return _encoding.WebName; }
            set { _encoding = System.Text.Encoding.GetEncoding(value); }
        }

        /// <summary>
        /// Sends the provided text to the specified address.
        /// </summary>
        /// <param name="address">The address. Can be tcp://host:port, udp://host:port, http://host:port</param>
        /// <param name="bytes">The bytes to be sent.</param>
        protected virtual void NetworkSend(string address, byte[] bytes)
        {
            lock (this)
            {
                if (KeepConnection)
                {
                    if (_sender != null)
                    {
                        if (_sender.Address != address)
                        {
                            _sender.Close();
                            _sender = null;
                        }
                    };
                    if (_sender == null)
                    {
                        _sender = NetworkSender.Create(address);
                    }

                    try
                    {
                        ChunkedSend(_sender, bytes);
                    }
                    catch (Exception ex)
                    {
                        InternalLogger.Error("Error when sending {0}", ex);
                        _sender.Close();
                        _sender = null;
                        throw;
                    }
                }
                else
                {
                    NetworkSender sender = NetworkSender.Create(address);

                    try
                    {
                        ChunkedSend(sender, bytes);
                    }
                    finally
                    {
                        sender.Close();
                    }
                }
            }
        }

        /// <summary>
        /// Flushes any buffers.
        /// </summary>
        /// <param name="timeout">Flush timeout.</param>
        public override void Flush(TimeSpan timeout)
        {
            lock (this)
            {
                if (_sender != null)
                    _sender.Flush();
            }
        }

        /// <summary>
        /// Closes the target.
        /// </summary>
        protected internal override void Close()
        {
            base.Close();
            lock (this)
            {
                if (_sender != null)
                    _sender.Close();
            }
        }

        /// <summary>
        /// Adds all layouts used by this target to the specified collection.
        /// </summary>
        /// <param name="layouts">The collection to add layouts to.</param>
        public override void PopulateLayouts(LayoutCollection layouts)
        {
            base.PopulateLayouts(layouts);
            AddressLayout.PopulateLayouts(layouts);
        }

        /// <summary>
        /// Sends the 
        /// rendered logging event over the network optionally concatenating it with a newline character.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        protected internal override void Write(LogEventInfo logEvent)
        {
            NetworkSend(AddressLayout.GetFormattedMessage(logEvent), GetBytesToWrite(logEvent));
        }

        /// <summary>
        /// Gets the bytes to be written.
        /// </summary>
        /// <param name="logEvent">Log event</param>
        /// <returns>Byte array.</returns>
        protected virtual byte[] GetBytesToWrite(LogEventInfo logEvent)
        {
            string text;

            if (NewLine)
                text = CompiledLayout.GetFormattedMessage(logEvent) + "\r\n";
            else
                text = CompiledLayout.GetFormattedMessage(logEvent);

            return _encoding.GetBytes(text);
        }

        private void ChunkedSend(NetworkSender sender, byte[] buffer)
        {
            int tosend = buffer.Length;
            int pos = 0;

            while (tosend > 0)
            {
                int chunksize = tosend;
                if (chunksize > MaxMessageSize)
                {
                    if (OnOverflow == OverflowAction.Discard)
                        return;

                    if (OnOverflow == OverflowAction.Error)
                        throw new OverflowException("Attempted to send a message larger than MaxMessageSize(" + MaxMessageSize + "). Actual size was: " + buffer.Length + ". Adjust OnOverflow and MaxMessageSize parameters accordingly.");

                    chunksize = MaxMessageSize;
                }
                sender.Send(buffer, pos, chunksize);
                tosend -= chunksize;
                pos += chunksize;
            }
        }
    }
}
