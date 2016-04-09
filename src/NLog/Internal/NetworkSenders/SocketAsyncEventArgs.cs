// 
// Copyright (c) 2004-2016 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

#if USE_LEGACY_ASYNC_API

namespace NLog.Internal.NetworkSenders
{
    using System;
    using System.Net;
    using System.Net.Sockets;

    /// <summary>
    /// Emulate missing functionality from .NET Compact Framework
    /// </summary>
    internal class SocketAsyncEventArgs : EventArgs, IDisposable
    {
        public EventHandler<SocketAsyncEventArgs> Completed;
        public EndPoint RemoteEndPoint { get; set; }
        public object UserToken { get; set; }
        public SocketError SocketError { get; set; }

        public byte[] Buffer { get; set; }
        public int Offset { get; set; }
        public int Count { get; set; }
        public SocketFlags SocketFlags { get; set; }

        public void EndConnect(IAsyncResult result)
        {
            var socket = (Socket)result.AsyncState;

            try
            {
                socket.EndConnect(result);
                this.SocketError = SocketError.Success;
            }
            catch (SocketException)
            {
                this.SocketError = SocketError.SocketError;
            }

            this.OnCompleted(this);
        }

        public void EndSend(IAsyncResult result)
        {
            var socket = (Socket)result.AsyncState;

            try
            {
                int sendResult = socket.EndSend(result);
                this.SocketError = SocketError.Success;
            }
            catch (SocketException)
            {
                this.SocketError = SocketError.SocketError;
            }

            this.OnCompleted(this);
        }


        public void EndSendTo(IAsyncResult result)
        {
            var socket = (Socket)result.AsyncState;

            try
            {
                int sendResult = socket.EndSendTo(result);
                this.SocketError = SocketError.Success;
            }
            catch (SocketException)
            {
                this.SocketError = SocketError.SocketError;
            }

            this.OnCompleted(this);
        }

        public void Dispose()
        {
            // not needed
        }

        internal void SetBuffer(byte[] bytes, int offset, int length)
        {
            this.Buffer = bytes;
            this.Offset = offset;
            this.Count = length;
        }

        internal void OnCompleted(SocketAsyncEventArgs args)
        {
            var cb = this.Completed;
            if (cb != null)
            {
                cb(this, this);
            }
        }
    }
}

#endif