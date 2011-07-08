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

#if !WINDOWS_PHONE_7

namespace NLog.Internal.NetworkSenders
{
    using System;
    using System.Net.Sockets;
    using System.Security;

    /// <summary>
    /// Socket proxy for mocking Socket code.
    /// </summary>
    internal sealed class SocketProxy : ISocket, IDisposable
    {
        private readonly Socket socket;

        /// <summary>
        /// Initializes a new instance of the <see cref="SocketProxy"/> class.
        /// </summary>
        /// <param name="addressFamily">The address family.</param>
        /// <param name="socketType">Type of the socket.</param>
        /// <param name="protocolType">Type of the protocol.</param>
        internal SocketProxy(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
        {
            this.socket = new Socket(addressFamily, socketType, protocolType);
        }

        /// <summary>
        /// Closes the wrapped socket.
        /// </summary>
        public void Close()
        {
            this.socket.Close();
        }

#if USE_LEGACY_ASYNC_API || NET_CF
        // emulate missing .NET CF behavior

        /// <summary>
        /// Invokes ConnectAsync method on the wrapped socket.
        /// </summary>
        /// <param name="args">The <see cref="SocketAsyncEventArgs"/> instance containing the event data.</param>
        /// <returns>Result of original method.</returns>
        public bool ConnectAsync(SocketAsyncEventArgs args)
        {
            this.socket.BeginConnect(args.RemoteEndPoint, args.EndConnect, this.socket);
            return true;
        }

        /// <summary>
        /// Invokes SendAsync method on the wrapped socket.
        /// </summary>
        /// <param name="args">The <see cref="SocketAsyncEventArgs"/> instance containing the event data.</param>
        /// <returns>Result of original method.</returns>
        public bool SendAsync(SocketAsyncEventArgs args)
        {
            this.socket.BeginSend(args.Buffer, args.Offset, args.Count, args.SocketFlags, args.EndSend, this.socket);
            return true;
        }

        /// <summary>
        /// Invokes SendToAsync method on the wrapped socket.
        /// </summary>
        /// <param name="args">The <see cref="SocketAsyncEventArgs"/> instance containing the event data.</param>
        /// <returns>Result of original method.</returns>
        public bool SendToAsync(SocketAsyncEventArgs args)
        {
            this.socket.BeginSendTo(args.Buffer, args.Offset, args.Count, args.SocketFlags, args.RemoteEndPoint, args.EndSendTo, this.socket);
            return true;
        }
#else
        /// <summary>
        /// Invokes ConnectAsync method on the wrapped socket.
        /// </summary>
        /// <param name="args">The <see cref="SocketAsyncEventArgs"/> instance containing the event data.</param>
        /// <returns>Result of original method.</returns>
        public bool ConnectAsync(SocketAsyncEventArgs args)
        {
            return this.socket.ConnectAsync(args);
        }

        /// <summary>
        /// Invokes SendAsync method on the wrapped socket.
        /// </summary>
        /// <param name="args">The <see cref="SocketAsyncEventArgs"/> instance containing the event data.</param>
        /// <returns>Result of original method.</returns>
        public bool SendAsync(SocketAsyncEventArgs args)
        {
            return this.socket.SendAsync(args);
        }

#if !SILVERLIGHT || (WINDOWS_PHONE && !WINDOWS_PHONE_7)
        /// <summary>
        /// Invokes SendToAsync method on the wrapped socket.
        /// </summary>
        /// <param name="args">The <see cref="SocketAsyncEventArgs"/> instance containing the event data.</param>
        /// <returns>Result of original method.</returns>
        public bool SendToAsync(SocketAsyncEventArgs args)
        {
            return this.socket.SendToAsync(args);
        }
#endif

#endif

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            ((IDisposable)this.socket).Dispose();
        }
    }
}

#endif