using System;
using System.Net;
using System.Net.Sockets;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using System.Windows.Forms;

namespace NLog.Viewer
{
    /// <summary>
    /// Summary description for LogInstanceUDP.
    /// </summary>
    public class LogInstanceUDP : LogInstance
    {
        public LogInstanceUDP(LogInstanceConfigurationInfo LogInstanceConfigurationInfo) : base(LogInstanceConfigurationInfo)
        {
        }

        public override void InputThread()
        {
            XmlSerializer ser = new XmlSerializer(typeof(LogEventInfo));

            /*
            LogEventInfo lei = new LogEventInfo();
            lei.SendTime = DateTime.Now;
            lei.ReceivedTime = DateTime.Now;
            lei.MessageText = "Ala ma kota";
            lei.Logger = "Logger";
            lei.Level = "Debug";
            lei.SourceAssembly = typeof(LogInstanceUDP).Assembly.FullName;
            lei.SourceFile = "LogInstanceUDP.cs";
            lei.SourceLine = 29;
            lei.SourceType = this.GetType().FullName;
            lei.SourceMethod = System.Reflection.MethodInfo.GetCurrentMethod().ToString();
            lei.ExtraInfo = new LogEventExtraInfo[1];
            lei.ExtraInfo[0].Name = "somename";
            lei.ExtraInfo[0].Value = "somevalue";

            using (FileStream fs = File.Create("c:\\dump.xml"))
            {
                ser.Serialize(fs, lei);
            }
            */

            try
            {
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
                {
                    socket.Bind(new IPEndPoint(IPAddress.Any, 40000));

                    IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                    EndPoint senderRemote = (EndPoint)sender;
                    byte[] buffer = new byte[65536];

                    while (!QuitInputThread)
                    {
                        if (socket.Poll(1000000, SelectMode.SelectRead))
                        {
                            int got = socket.ReceiveFrom(buffer, ref senderRemote);
                            if (got > 0)
                            {
                                try
                                {
                                    MemoryStream ms = new MemoryStream(buffer, 0, got);
                                    LogEventInfo logEventInfo = (LogEventInfo)ser.Deserialize(ms);
                                    logEventInfo.ReceivedTime = DateTime.Now;
                                    ProcessLogEvent(logEventInfo);
                                }
                                catch (Exception ex)
                                {
                                    // MessageBox.Show(ex.ToString());
                                }
                                // _listView.Items.Insert(0, System.Text.Encoding.Default.GetString(buffer, 0, got));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}
