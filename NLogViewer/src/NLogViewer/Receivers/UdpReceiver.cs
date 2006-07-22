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

using NLogViewer.Configuration;
using NLogViewer.Events;
using NLogViewer.Parsers;

namespace NLogViewer.Receivers
{
    [LogEventReceiver("UDP", "UDP Receiver", "Receives events over the network using the UDP protocol")]
    public class UdpReceiver : NetworkBaseReceiver
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public UdpReceiver()
        {
            Port = 4000;
        }

        public override void InputThread()
        {
            try
            {
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
                {
                    socket.Bind(new IPEndPoint(IPAddress.Any, this.Port));
                    logger.Debug("UDP listening on port {0}", this.Port);

                    IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                    EndPoint senderRemote = (EndPoint)sender;
                    byte[] buffer = new byte[65536];

                    while (!InputThreadQuitRequested())
                    {
                        if (socket.Poll(100000, SelectMode.SelectRead))
                        {
                            int got = socket.ReceiveFrom(buffer, ref senderRemote);
                            if (got > 0)
                            {
                                try
                                {
                                    MemoryStream ms = new MemoryStream(buffer, 0, got);
                                    using (ILogEventParserInstance parserInstance = Parser.Begin(ms))
                                    {
                                        LogEvent logEventInfo = CreateLogEvent();
                                        if (parserInstance.ReadNext(logEventInfo))
                                            EventReceived(logEventInfo);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show(ex.ToString());
                                }
                                // _listView.Items.Insert(0, System.Text.Encoding.Default.GetString(buffer, 0, got));
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
