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
using System.Threading;

namespace NLog.Internal.NetworkSenders
{
    /// <summary>
    /// A base class for all network senders. Supports one-way sending of messages
    /// over various protocols.
    /// </summary>
	public abstract class NetworkSender : IDisposable
	{
        private string _url;

        /// <summary>
        /// Creates a new instance of the <see cref="NetworkSender"/> and initializes
        /// it with the specified URL.
        /// </summary>
        /// <param name="url">URL.</param>
        protected NetworkSender(string url)
        {
            _url = url;
        }

        /// <summary>
        /// Disposes the <see cref="NetworkSender"/> object.
        /// </summary>
        ~NetworkSender()
        {
            Dispose(false);
        }

        /// <summary>
        /// Creates a new instance of the network sender based on a network URL:
        /// </summary>
        /// <param name="url">URL that determines the network sender to be created.</param>
        /// <returns>A newly created network sender.</returns>
        /// <remarks>
        /// If the url starts with <c>tcp://</c> - a new <see cref="TcpNetworkSender" /> is created.<br/>
        /// If the url starts with <c>udp://</c> - a new <see cref="UdpNetworkSender" /> is created.<br/>
        /// </remarks>
        public static NetworkSender Create(string url)
        {
            if (url.StartsWith("tcp://"))
            {
                return new TcpNetworkSender(url);
            }
            if (url.StartsWith("udp://"))
            {
                return new UdpNetworkSender(url);
            }
            throw new ArgumentException("Unrecognized network address", "url");
        }

        /// <summary>
        /// Closes the sender and releases any unmanaged resources.
        /// </summary>
        public virtual void Close()
        {
        }

        /// <summary>
        /// Flushes any buffers.
        /// </summary>
        public virtual void Flush()
        {
        }

        /// <summary>
        /// The address of the network endpoint.
        /// </summary>
        public string Address
        {
            get { return _url; }
        }

        /// <summary>
        /// Send the given text over the specified protocol.
        /// </summary>
        /// <param name="bytes">Bytes to be sent.</param>
        /// <param name="offset">Offset in buffer</param>
        /// <param name="length">Number of bytes to send</param>
        public void Send(byte[] bytes, int offset, int length)
        {
            DoSend(bytes, offset, length);
        }

        /// <summary>
        /// Actually sends the given text over the specified protocol.
        /// </summary>
        /// <param name="bytes">The bytes to be sent.</param>
        /// <param name="offset">Offset in buffer</param>
        /// <param name="length">Number of bytes to send</param>
        /// <remarks>To be overridden in inheriting classes.</remarks>
        protected abstract void DoSend(byte[] bytes, int offset, int length);

        /// <summary>
        /// Closes the sender and releases any unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                Close();
            }
        }
    }
}
