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
using System.Net;
using System.Net.Sockets;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using System.Windows.Forms;
using System.Collections.Specialized;
using System.Threading;

using NLogViewer.Configuration;
using NLogViewer.Events;

namespace NLogViewer.Receivers
{
    [LogEventReceiver("TCP", "TCP Event Receiver", "Receives events from the network using the TCP protocol and log4j XML schema")]
    public class TCPEventReceiver : NetworkEventReceiver
    {
        public TCPEventReceiver()
        {
        }

        public void ConnectionClosed(TCPConnection conn)
        {
            Log.Write("{0} closed.", conn);
        }

        public override void InputThread()
        {
            try
            {
                using (Socket mainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    mainSocket.Bind(new IPEndPoint(IPAddress.Any, this.Port));
                    mainSocket.Listen(10);
                    Log.Write("TCP listening on port: {0}", this.Port);

                    byte[] buffer = new byte[65536];

                    while (!QuitInputThread)
                    {
                        if (mainSocket.Poll(1000000, SelectMode.SelectRead))
                        {
                            Socket childSocket = mainSocket.Accept();
                            TCPConnection conn = new TCPConnection(this, childSocket);
                            Log.Write("{0} accepted.", conn);
                            conn.Start();
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            finally
            {
            }
        }

        public class TCPConnection
        {
            private Socket _socket;
            private TCPEventReceiver _receiver;
            private Thread _thread = null;
            private string _connectionInfo;

            public TCPConnection(TCPEventReceiver receiver, Socket socket)
            {
                _socket = socket;
                _receiver = receiver;
                _connectionInfo = String.Format("TCP connection from {0} to {1}", _socket.RemoteEndPoint.ToString(),  _socket.LocalEndPoint.ToString());
            }

            public void InputThread()
            {
                try
                {
                    using (_socket)
                    {
                        //
                        // a trick to handle multiple-root xml streams
                        // as described by Oleg Tkachenko in his blog:
                        //
                        // http://www.tkachenko.com/blog/archives/000053.html
                        //

                        NetworkStream ns = new NetworkStream(_socket, FileAccess.Read, false);
                        XmlParserContext context = new XmlParserContext(new NameTable(), null, null, XmlSpace.Default);
                        XmlTextReader reader = new XmlTextReader(ns, XmlNodeType.Element, context);
                        reader.Namespaces = false;

                        while (!_receiver.QuitInputThread)
                        {
                            bool pollResult = _socket.Poll(1000000, SelectMode.SelectRead);
                            //Log.Write("dataAvailable: {0} pollResult: {1}", ns.DataAvailable, pollResult);
                            if (ns.DataAvailable || pollResult)
                            {
                                if (!reader.Read())
                                {
                                    // Log.Write("No more XML tokens in stream. Quitting.");
                                    break;
                                }

                                if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "log4j:event")
                                {
                                    LogEvent logEvent;

                                    logEvent = LogEvent.ParseLog4JEvent(reader);
                                    logEvent.ReceivedTime = DateTime.Now;
                                    _receiver.EventReceived(logEvent);
                                    continue;
                                }
                                else
                                {
                                    // Log.Write("Skipping {0}", reader.NodeType);
                                }
                            }
                        }
                    }
                }
                catch (IOException ex)
                {
                    Log.Write("IO Exception: {0}", ex.Message);
                }
                catch (Exception ex)
                {
                    Log.Write("Exception: {0}", ex.ToString());
                }
                finally
                {
                    _receiver.ConnectionClosed(this);
                }
            }

            public void Start()
            {
                _thread = new Thread(new ThreadStart(this.InputThread));
                _thread.IsBackground = true;
                _thread.Start();
            }

            public void Stop()
            {
                _thread.Join(TimeSpan.FromSeconds(5));
            }

            public override string ToString()
            {
                return _connectionInfo;
            }

        }
    }
}
