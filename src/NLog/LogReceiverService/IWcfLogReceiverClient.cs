#if WCF_SUPPORTED

using System;
using System.ComponentModel;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace NLog.LogReceiverService
{
    /// <summary>
    /// Client of <see cref="ILogReceiverServer"/>
    /// </summary>
    public interface IWcfLogReceiverClient : ICommunicationObject
    {
        /// <summary>
        /// Occurs when the log message processing has completed.
        /// </summary>
        event EventHandler<AsyncCompletedEventArgs> ProcessLogMessagesCompleted;

        /// <summary>
        /// Occurs when Open operation has completed.
        /// </summary>
        event EventHandler<AsyncCompletedEventArgs> OpenCompleted;

        /// <summary>
        /// Occurs when Close operation has completed.
        /// </summary>
        event EventHandler<AsyncCompletedEventArgs> CloseCompleted;

        /// <summary>
        /// Enables the user to configure client and service credentials as well as service credential authentication settings for use on the client side of communication.
        /// </summary>
        ClientCredentials ClientCredentials { get; }

        /// <summary>
        /// Gets the underlying <see cref="IClientChannel"/> implementation.
        /// </summary>
        IClientChannel InnerChannel { get; }

        /// <summary>
        /// Gets the target endpoint for the service to which the WCF client can connect.
        /// </summary>
        ServiceEndpoint Endpoint { get; }

        /// <summary>
        /// Opens the client asynchronously.
        /// </summary>
        void OpenAsync();

        /// <summary>
        /// Opens the client asynchronously.
        /// </summary>
        /// <param name="userState">User-specific state.</param>
        void OpenAsync(object userState);

        /// <summary>
        /// Closes the client asynchronously.
        /// </summary>
        void CloseAsync();

        /// <summary>
        /// Closes the client asynchronously.
        /// </summary>
        /// <param name="userState">User-specific state.</param>
        void CloseAsync(object userState);

        /// <summary>
        /// Processes the log messages asynchronously.
        /// </summary>
        /// <param name="events">The events to send.</param>
        void ProcessLogMessagesAsync(NLogEvents events);

        /// <summary>
        /// Processes the log messages asynchronously.
        /// </summary>
        /// <param name="events">The events to send.</param>
        /// <param name="userState">User-specific state.</param>
        void ProcessLogMessagesAsync(NLogEvents events, object userState);

        /// <summary>
        /// Begins processing of log messages.
        /// </summary>
        /// <param name="events">The events to send.</param>
        /// <param name="callback">The callback.</param>
        /// <param name="asyncState">Asynchronous state.</param>
        /// <returns>
        /// IAsyncResult value which can be passed to <see cref="ILogReceiverOneWayClient.EndProcessLogMessages"/>.
        /// </returns>
        IAsyncResult BeginProcessLogMessages(NLogEvents events, AsyncCallback callback, object asyncState);

        /// <summary>
        /// Ends asynchronous processing of log messages.
        /// </summary>
        /// <param name="result">The result.</param>
        void EndProcessLogMessages(IAsyncResult result);

#if !SILVERLIGHT
        /// <summary>
        /// Instructs the inner channel to display a user interface if one is required to initialize the channel prior to using it.
        /// </summary>
        void DisplayInitializationUI();
#endif
      
        /// <summary>
        /// Gets or sets the cookie container.
        /// </summary>
        /// <value>The cookie container.</value>
        CookieContainer CookieContainer { get; set; }
    }
}

#endif