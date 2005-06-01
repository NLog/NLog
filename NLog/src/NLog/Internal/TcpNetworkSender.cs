using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NLog.Internal
{
    /// <summary>
    /// Sends messages over a TCP network connection.
    /// </summary>
	public class TcpNetworkSender : NetworkSender
	{
        private Socket _socket;
        private Encoding _encoding;

        /// <summary>
        /// Creates a new instance of <see cref="TcpNetworkSender"/> and initializes
        /// it with the specified URL. Connects to the server specified in the URL.
        /// </summary>
        /// <param name="url">URL. Must start with tcp://</param>
        public TcpNetworkSender(string url) : base(url)
        {
            // tcp://hostname:port

            Uri parsedUri = new Uri(url);
            IPHostEntry host = Dns.GetHostByName(parsedUri.Host);
            int port = parsedUri.Port;

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Connect(new IPEndPoint(host.AddressList[0], port));

            _encoding = Encoding.UTF8;
        }

        /// <summary>
        /// Sends the specified text over the connected socket.
        /// </summary>
        /// <param name="text"></param>
        protected override void DoSend(string text)
        {
            lock (this)
            {
                byte[] bytes = _encoding.GetBytes(text);
                _socket.Send(bytes);
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
