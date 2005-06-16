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
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading;

using NLog.Internal;

namespace NLog.Targets
{
    /// <summary>
    /// Sends logging messages over network.
    /// </summary>
    [Target("Network")]
    public class NetworkTarget: AsyncTarget
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
            if (Async)
            {
                RequestQueue.Enqueue(new NetworkWriteRequest(address, text));
                return;
            }
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
        /// <param name="ev">The logging event.</param>
        protected internal override void Append(LogEventInfo ev)
        {
            if (NewLine)
            {
                NetworkSend(AddressLayout.GetFormattedMessage(ev), CompiledLayout.GetFormattedMessage(ev) + "\r\n");
            }
            else
            {
                NetworkSend(AddressLayout.GetFormattedMessage(ev), CompiledLayout.GetFormattedMessage(ev));
            }
        }

#if !NETCF

        protected override void LoggingThreadProc()
        {
            ArrayList pendingNetworkRequests = new ArrayList();
            NetworkSender currentSender = null;
            string currentSenderAddress = "";

            while (!LoggingThreadStopRequested)
            {
                pendingNetworkRequests.Clear();
                RequestQueue.DequeueBatch(pendingNetworkRequests, 100);

                if (pendingNetworkRequests.Count == 0)
                {
                    Thread.Sleep(100);
                    continue;
                }

                // sort the network requests by the address and 
                // the sequence to maximize socket reuse

                pendingNetworkRequests.Sort(NetworkWriteRequest.GetComparer());

                /*
                    InternalLogger.Debug("---");
                    foreach (FileWriteRequest fwr in pendingNetworkRequests)
                    {
                        InternalLogger.Debug("request: {0} {1}", fwr.FileName, fwr.Sequence);
                    }
                    */

                int requests = 0;
                int reopens = 0;

                for (int i = 0; i < pendingNetworkRequests.Count; ++i)
                {
                    NetworkWriteRequest fwr = (NetworkWriteRequest)pendingNetworkRequests[i];

                    if (fwr.Address != currentSenderAddress)
                    {
                        if (currentSender != null)
                        {
                            currentSender.Close();
                            currentSender = null;
                        }
                        currentSenderAddress = fwr.Address;
                        currentSender = NetworkSender.Create(fwr.Address);
                        reopens++;
                    }
                    requests++;
                    if (currentSender != null)
                        currentSender.Send(fwr.Text);
                }
            }
            if (currentSender != null)
            {
                currentSender.Close();
                currentSender = null;
            }
        }

        /// <summary>
        /// Represents a single async request to write to a network place.
        /// </summary>
        class NetworkWriteRequest
        {
            private string _address;
            private string _text;
            private long _sequence;

            private static long _globalSequence;

            public NetworkWriteRequest(string address, string text)
            {
                _address = address;
                _text = text;
                _sequence = Interlocked.Increment(ref _globalSequence);
            }

            public string Address
            {
                get { return _address; }
            }

            public string Text
            {
                get { return _text; }
            }

            public long Sequence
            {
                get { return _sequence; }
            }

            private static IComparer _comparer = new Comparer();

            public static IComparer GetComparer()
            {
                return _comparer;
            }

            class Comparer : IComparer
            {
                public int Compare(object x, object y)
                {
                    NetworkWriteRequest fwr1 = (NetworkWriteRequest)x;
                    NetworkWriteRequest fwr2 = (NetworkWriteRequest)y;

                    int val = String.CompareOrdinal(fwr1.Address, fwr2.Address);
                    if (val != 0)
                        return val;

                    if (fwr1.Sequence < fwr2.Sequence)
                        return -1;
                    if (fwr1.Sequence > fwr2.Sequence)
                        return 1;
                    return 0;
                }
            }
        }
#endif

    }
}
