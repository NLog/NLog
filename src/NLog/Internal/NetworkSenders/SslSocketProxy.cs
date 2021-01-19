// 
// Copyright (c) 2004-2020 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
#if !NETSTANDARD1_3 && !NETSTANDARD1_5
    using System;
    using System.Net.Security;
    using System.Net.Sockets;
    using System.Security.Authentication;

    class SslSocketProxy : ISocket, IDisposable
    {
        readonly AsyncCallback _sendCompleted;
        readonly SocketProxy _socketProxy;
        readonly string _host;
        readonly SslProtocols _sslProtocol;
        SslStream _sslStream;

        public SslSocketProxy(string host, SslProtocols sslProtocol, SocketProxy socketProxy)
        {
            _socketProxy = socketProxy;
            _host = host;
            _sslProtocol = sslProtocol;
            _sendCompleted = (ar) => SocketProxySendCompleted(ar);
        }

        public void Close()
        {
            if (_sslStream != null)
                _sslStream.Close();
            else
                _socketProxy.Close();
        }

        public bool ConnectAsync(SocketAsyncEventArgs args)
        {
            var proxyArgs = new TcpNetworkSender.MySocketAsyncEventArgs();
            proxyArgs.RemoteEndPoint = args.RemoteEndPoint;
            proxyArgs.Completed += SocketProxyConnectCompleted;
            proxyArgs.UserToken = args;
            if (!_socketProxy.ConnectAsync(proxyArgs))
            {
                SocketProxyConnectCompleted(this, proxyArgs);
                return false;
            }
            return true;
        }

        private void SocketProxySendCompleted(IAsyncResult asyncResult)
        {
            var proxyArgs = asyncResult.AsyncState as TcpNetworkSender.MySocketAsyncEventArgs;
            try
            {
                _sslStream.EndWrite(asyncResult);
            }
            catch (SocketException ex)
            {
                if (proxyArgs != null)
                    proxyArgs.SocketError = ex.SocketErrorCode;
            }
            catch (Exception ex)
            {
                if (proxyArgs != null)
                {
                    if (ex.InnerException is SocketException socketException)
                        proxyArgs.SocketError = socketException.SocketErrorCode;
                    else
                        proxyArgs.SocketError = SocketError.OperationAborted;
                }
            }
            finally
            {
                proxyArgs?.RaiseCompleted();
            }
        }

        private void SocketProxyConnectCompleted(object sender, SocketAsyncEventArgs e)
        {
            var proxyArgs = e.UserToken as TcpNetworkSender.MySocketAsyncEventArgs;
            if (e.SocketError != SocketError.Success)
            {
                if (proxyArgs != null)
                {
                    proxyArgs.SocketError = e.SocketError;
                    proxyArgs.RaiseCompleted();
                }
            }
            else
            {
                try
                {
                    _sslStream = new SslStream(new NetworkStream(_socketProxy.UnderlyingSocket), false, UserCertificateValidationCallback)
                    {
                        // Wait 20 secs before giving up on SSL-handshake
                        ReadTimeout = 20000
                    };

                    AuthenticateAsClient(_sslStream, _host, _sslProtocol);
                }
                catch (SocketException ex)
                {
                    if (proxyArgs != null)
                        proxyArgs.SocketError = ex.SocketErrorCode;
                }
                catch (Exception ex)
                {
                    if (proxyArgs != null)
                    {
                        proxyArgs.SocketError = GetSocketError(ex);
                    }
                }
                finally
                {
                    proxyArgs?.RaiseCompleted();
                }
            }
        }

        private static SocketError GetSocketError(Exception ex)
        {
            SocketError socketError;
            if (ex.InnerException is SocketException socketException)
            {
                socketError = socketException.SocketErrorCode;
            }
            else
            {
                socketError = SocketError.ConnectionRefused;
            }

            return socketError;
        }

        private static void AuthenticateAsClient(SslStream sslStream, string targetHost, SslProtocols enabledSslProtocols)
        {
            if (enabledSslProtocols != SslProtocols.Default)
                sslStream.AuthenticateAsClient(targetHost, null, enabledSslProtocols, false);
            else
                sslStream.AuthenticateAsClient(targetHost);
        }

        private static bool UserCertificateValidationCallback(object sender, object certificate, object chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            Common.InternalLogger.Debug("SSL certificate errors were encountered when establishing connection to the server: {0}, Certificate: {1}", sslPolicyErrors, certificate);
            return false;
        }

        public bool SendAsync(SocketAsyncEventArgs args)
        {
            _sslStream.BeginWrite(args.Buffer, args.Offset, args.Count, _sendCompleted, args);
            return true;
        }

        public bool SendToAsync(SocketAsyncEventArgs args)
        {
            return SendAsync(args);
        }

        public void Dispose()
        {
            if (_sslStream != null)
                _sslStream.Dispose();
            else
                _socketProxy.Dispose();
        }
    }
#endif
}
