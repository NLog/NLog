using System;

namespace NLog.Internal
{
    /// <summary>
    /// Sends one-way messages over the HTTP protocol.
    /// </summary>
	public class HttpNetworkSender : NetworkSender
	{
        /// <summary>
        /// Creates a new instance of <see cref="HttpNetworkSender"/> and initializes
        /// it with the specified URL.
        /// </summary>
        /// <param name="url">URL. Must start with http:// or https://</param>
        public HttpNetworkSender(string url) : base(url)
        {
        }

        /// <summary>
        /// Sends the given text over HTTP.
        /// </summary>
        /// <param name="text">The text to be sent.</param>
        /// <remarks>The method uses HTTP <c>POST</c> method to connect to the server.</remarks>
        protected override void DoSend(string text)
        {
            throw new NotImplementedException();
        }
    }
}
