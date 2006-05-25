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
    /// <code lang="XML" src="examples/targets/Network/NetworkTarget.nlog" />
    /// <p>
    /// This assumes just one target and a single rule. More configuration
    /// options are described <a href="config.html">here</a>.
    /// </p>
    /// <p>
    /// To set up the log target programmatically use code like this:
    /// </p>
    /// <code lang="C#" src="examples/targets/Network/NetworkTarget.cs" />
    /// <p>
    /// To print the results, use any application that's able to receive messages over
    /// TCP or UDP. <a href="http://m.nu/program/util/netcat/netcat.html">NetCat</a> is
    /// a simple but very powerful command-line tool that can be used for that. This image
    /// demonstrates the NetCat tool receiving log messages from Network target.
    /// </p>
    /// <img src="examples/targets/Network/Output.gif" />
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
    public class NetworkTarget: Target
    {
        private bool _newline = false;
        private bool _keepConnection = true;
        private Layout _addressLayout = null;
        private NetworkSender _sender = null;

        /// <summary>
        /// The network address. Can be tcp://host:port, udp://host:port, http://host:port or https://host:port
        /// </summary>
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

        /// <summary>
        /// Sends the provided text to the specified address.
        /// </summary>
        /// <param name="address">The address. Can be tcp://host:port, udp://host:port, http://host:port or https://host:port</param>
        /// <param name="text">The text to be sent.</param>
        protected void NetworkSend(string address, string text)
        {
            NetworkSender sender;
            bool keep;

            lock (this)
            {
                keep = KeepConnection;
                
                
                if (_sender != null)
                {
                    if (_sender.Address != address)
                    {
                        _sender.Close();
                        _sender = null;
                    }
                };

                if (_sender != null)
                {
                    sender = _sender;
                }
                else
                {
                    sender = NetworkSender.Create(address);
                    if (keep)
                        _sender = sender;
                }
            }

            try
            {
                sender.Send(text);
            }
            catch (Exception)
            {
                lock (this)
                {
                    sender.Close();
                    sender = null;
                    _sender = null;
                }
            }

            if (!keep && sender != null)
            {
                sender.Close();
                sender = null;
            }
        }

        /// <summary>
        /// Sends the 
        /// rendered logging event over the network optionally concatenating it with a newline character.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        protected internal override void Write(LogEventInfo logEvent)
        {
            if (NewLine)
            {
                NetworkSend(AddressLayout.GetFormattedMessage(logEvent), CompiledLayout.GetFormattedMessage(logEvent) + "\r\n");
            }
            else
            {
                NetworkSend(AddressLayout.GetFormattedMessage(logEvent), CompiledLayout.GetFormattedMessage(logEvent));
            }
        }
    }
}
