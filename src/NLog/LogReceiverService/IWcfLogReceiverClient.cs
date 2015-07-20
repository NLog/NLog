using System;
using System.ComponentModel;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace NLog.LogReceiverService
{
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

    
        ClientCredentials ClientCredentials { get; }
  
        IClientChannel InnerChannel { get; }
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

     
        void DisplayInitializationUI();
    }
}