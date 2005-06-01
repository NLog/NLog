using System;
using System.Threading;

namespace NLog.Internal
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
        /// Creates a new instance of the network sender based on a network URL:
        /// </summary>
        /// <param name="url">URL that determines the network sender to be created.</param>
        /// <returns>A newly created network sender.</returns>
        /// <remarks>
        /// If the url starts with <c>tcp://</c> - a new <see cref="TcpNetworkSender" /> is created.<br/>
        /// If the url starts with <c>udp://</c> - a new <see cref="UdpNetworkSender" /> is created.<br/>
        /// If the url starts with <c>http://</c> or <c>https://</c>- a new <see cref="HttpNetworkSender" /> is created.<br/>
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
            if (url.StartsWith("http://") || url.StartsWith("https://"))
            {
                return new HttpNetworkSender(url);
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
        /// The address of the network endpoint.
        /// </summary>
        public string Address
        {
            get { return _url; }
        }

        /// <summary>
        /// Send the given text over the specified protocol optionally using asynchronous invocation.
        /// </summary>
        /// <param name="text">Text to be sent.</param>
        /// <param name="async">Use asynchronous invocation (ignored yet).</param>
        public void Send(string text, bool async)
        {
            DoSend(text);
        }

        /// <summary>
        /// Actually sends the given text over the specified protocol.
        /// </summary>
        /// <param name="text">The text to be sent.</param>
        /// <remarks>To be overridden in inheriting classes.</remarks>
        protected abstract void DoSend(string text);

        /// <summary>
        /// Closes the sender and releases any unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Close();
        }
    }
}
