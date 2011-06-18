// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
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

namespace NLog.Internal.NetworkSenders
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using NLog.Common;

    /// <summary>
    /// A base class for all network senders. Supports one-way sending of messages
    /// over various protocols.
    /// </summary>
    internal abstract class NetworkSender : IDisposable
    {
        private static int currentSendTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkSender" /> class.
        /// </summary>
        /// <param name="url">The network URL.</param>
        protected NetworkSender(string url)
        {
            this.Address = url;
            this.LastSendTime = Interlocked.Increment(ref currentSendTime);
        }

        /// <summary>
        /// Finalizes an instance of the NetworkSender class.
        /// </summary>
        ~NetworkSender()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets the address of the network endpoint.
        /// </summary>
        public string Address { get; private set; }

        /// <summary>
        /// Gets the last send time.
        /// </summary>
        public int LastSendTime { get; private set; }

        /// <summary>
        /// Initializes this network sender.
        /// </summary>
        public void Initialize()
        {
            this.DoInitialize();
        }

        /// <summary>
        /// Closes the sender and releases any unmanaged resources.
        /// </summary>
        /// <param name="continuation">The continuation.</param>
        public void Close(AsyncContinuation continuation)
        {
            this.DoClose(continuation);
        }

        /// <summary>
        /// Flushes any pending messages and invokes a continuation.
        /// </summary>
        /// <param name="continuation">The continuation.</param>
        public void FlushAsync(AsyncContinuation continuation)
        {
            this.DoFlush(continuation);
        }

        /// <summary>
        /// Send the given text over the specified protocol.
        /// </summary>
        /// <param name="bytes">Bytes to be sent.</param>
        /// <param name="offset">Offset in buffer.</param>
        /// <param name="length">Number of bytes to send.</param>
        /// <param name="asyncContinuation">The asynchronous continuation.</param>
        public void Send(byte[] bytes, int offset, int length, AsyncContinuation asyncContinuation)
        {
            this.LastSendTime = Interlocked.Increment(ref currentSendTime);
            this.DoSend(bytes, offset, length, asyncContinuation);
        }

        /// <summary>
        /// Closes the sender and releases any unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs sender-specific initialization.
        /// </summary>
        protected virtual void DoInitialize()
        {
        }

        /// <summary>
        /// Performs sender-specific close operation.
        /// </summary>
        /// <param name="continuation">The continuation.</param>
        protected virtual void DoClose(AsyncContinuation continuation)
        {
            continuation(null);
        }

        /// <summary>
        /// Performs sender-specific flush.
        /// </summary>
        /// <param name="continuation">The continuation.</param>
        protected virtual void DoFlush(AsyncContinuation continuation)
        {
            continuation(null);
        }

        /// <summary>
        /// Actually sends the given text over the specified protocol.
        /// </summary>
        /// <param name="bytes">The bytes to be sent.</param>
        /// <param name="offset">Offset in buffer.</param>
        /// <param name="length">Number of bytes to send.</param>
        /// <param name="asyncContinuation">The async continuation to be invoked after the buffer has been sent.</param>
        /// <remarks>To be overridden in inheriting classes.</remarks>
        protected abstract void DoSend(byte[] bytes, int offset, int length, AsyncContinuation asyncContinuation);

#if !WINDOWS_PHONE_7
        /// <summary>
        /// Parses the URI into an endpoint address.
        /// </summary>
        /// <param name="uri">The URI to parse.</param>
        /// <param name="addressFamily">The address family.</param>
        /// <returns>Parsed endpoint.</returns>
        protected virtual EndPoint ParseEndpointAddress(Uri uri, AddressFamily addressFamily)
        {
#if SILVERLIGHT
            return new DnsEndPoint(uri.Host, uri.Port, addressFamily);
#else
            switch (uri.HostNameType)
            {
                case UriHostNameType.IPv4:
                case UriHostNameType.IPv6:
                    return new IPEndPoint(IPAddress.Parse(uri.Host), uri.Port);

                default:
                    {
                        var addresses = Dns.GetHostEntry(uri.Host).AddressList;
                        foreach (var addr in addresses)
                        {
                            if (addr.AddressFamily == addressFamily || addressFamily == AddressFamily.Unspecified)
                            {
                                return new IPEndPoint(addr, uri.Port);
                            }
                        }

                        throw new IOException("Cannot resolve '" + uri.Host + "' to an address in '" + addressFamily + "'");
                    }
            }
#endif
        }
#endif

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Close(ex => { });
            }
        }
    }
}
