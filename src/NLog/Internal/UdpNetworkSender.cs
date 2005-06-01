using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NLog.Internal
{
    /// <summary>
    /// Sends messages over the network as UDP datagrams.
    /// </summary>
	public class UdpNetworkSender : NetworkSender
	{
        private Socket _socket;
        private Encoding _encoding;
        private IPEndPoint _endpoint;

        /// <summary>
        /// Creates a new instance of <see cref="UdpNetworkSender"/> and initializes
        /// it with the specified URL.
        /// </summary>
        /// <param name="url">URL. Must start with udp://</param>
        public UdpNetworkSender(string url) : base(url)
        {
            // udp://hostname:port

            Uri parsedUri = new Uri(url);
            IPHostEntry host = Dns.GetHostByName(parsedUri.Host);
            int port = parsedUri.Port;

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _endpoint = new IPEndPoint(host.AddressList[0], port);
            _encoding = Encoding.UTF8;
        }

        /// <summary>
        /// Sends the specified text as a UDP datagram.
        /// </summary>
        /// <param name="text"></param>
        protected override void DoSend(string text)
        {
            lock (this)
            {
                _socket.SendTo(_encoding.GetBytes(text), _endpoint);
            }
        }

        /// <summary>
        /// Closes the socket.
        /// </summary>
        public override void Close()
        {
            lock (this)
            {
                try
                {
                    _socket.Close();
                }
                catch (Exception)
                {
                    // ignore errors
                }
                _socket = null;
            }
        }
    }
}
