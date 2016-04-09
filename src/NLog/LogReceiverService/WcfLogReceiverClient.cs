// 
// Copyright (c) 2004-2016 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

using System.Net;

#if WCF_SUPPORTED

namespace NLog.LogReceiverService
{
    using System.ServiceModel.Description;
    using System;
    using System.ComponentModel;
    using System.ServiceModel;
    using System.ServiceModel.Channels;


    /// <summary>
    /// Log Receiver Client facade. It allows the use either of the one way or two way 
    /// service contract using WCF through its unified interface.
    /// </summary>
    /// <remarks>
    /// Delegating methods are generated with Resharper.
    /// 1. change ProxiedClient to private field (instead of public property)
    /// 2. delegate members
    /// 3. change ProxiedClient back to public property.
    /// 
    /// </remarks>
    public sealed class WcfLogReceiverClient : IWcfLogReceiverClient
    {
        /// <summary>
        /// The client getting proxied
        /// </summary>
        public IWcfLogReceiverClient ProxiedClient { get; private set; }

        /// <summary>
        /// Do we use one-way or two-way messaging?
        /// </summary>
        public bool UseOneWay { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfLogReceiverClient"/> class.
        /// </summary>
        /// <param name="useOneWay">Whether to use the one way or two way WCF client.</param>
        public WcfLogReceiverClient(bool useOneWay)
        {
            UseOneWay = useOneWay;
            ProxiedClient = useOneWay ? (IWcfLogReceiverClient)new WcfLogReceiverOneWayClient() : new WcfLogReceiverTwoWayClient();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfLogReceiverClient"/> class.
        /// </summary>
        /// <param name="useOneWay">Whether to use the one way or two way WCF client.</param>
        /// <param name="endpointConfigurationName">Name of the endpoint configuration.</param>
        public WcfLogReceiverClient(bool useOneWay, string endpointConfigurationName)
        {
            UseOneWay = useOneWay;
            ProxiedClient = useOneWay ? (IWcfLogReceiverClient)new WcfLogReceiverOneWayClient(endpointConfigurationName) : new WcfLogReceiverTwoWayClient(endpointConfigurationName);

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfLogReceiverClient"/> class.
        /// </summary>
        /// <param name="useOneWay">Whether to use the one way or two way WCF client.</param>
        /// <param name="endpointConfigurationName">Name of the endpoint configuration.</param>
        /// <param name="remoteAddress">The remote address.</param>
        public WcfLogReceiverClient(bool useOneWay, string endpointConfigurationName, string remoteAddress)
        {
            UseOneWay = useOneWay;
            ProxiedClient = useOneWay ? (IWcfLogReceiverClient)new WcfLogReceiverOneWayClient(endpointConfigurationName, remoteAddress) : new WcfLogReceiverTwoWayClient(endpointConfigurationName, remoteAddress);

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfLogReceiverClient"/> class.
        /// </summary>
        /// <param name="useOneWay">Whether to use the one way or two way WCF client.</param>
        /// <param name="endpointConfigurationName">Name of the endpoint configuration.</param>
        /// <param name="remoteAddress">The remote address.</param>
        public WcfLogReceiverClient(bool useOneWay, string endpointConfigurationName, EndpointAddress remoteAddress)
        {
            UseOneWay = useOneWay;
            ProxiedClient = useOneWay ? (IWcfLogReceiverClient)new WcfLogReceiverOneWayClient(endpointConfigurationName, remoteAddress) : new WcfLogReceiverTwoWayClient(endpointConfigurationName, remoteAddress);

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfLogReceiverClient"/> class.
        /// </summary>
        /// <param name="useOneWay">Whether to use the one way or two way WCF client.</param>
        /// <param name="binding">The binding.</param>
        /// <param name="remoteAddress">The remote address.</param>
        public WcfLogReceiverClient(bool useOneWay, Binding binding, EndpointAddress remoteAddress)
        {
            UseOneWay = useOneWay;
            ProxiedClient = useOneWay ? (IWcfLogReceiverClient)new WcfLogReceiverOneWayClient(binding, remoteAddress) : new WcfLogReceiverTwoWayClient(binding, remoteAddress);

        }

        #region delegating

        /// <summary>
        /// Causes a communication object to transition immediately from its current state into the closed state.  
        /// </summary>
        public void Abort()
        {
            ProxiedClient.Abort();
        }

        /// <summary>
        /// Begins an asynchronous operation to close a communication object.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.IAsyncResult"/> that references the asynchronous close operation. 
        /// </returns>
        /// <param name="callback">The <see cref="T:System.AsyncCallback"/> delegate that receives notification of the completion of the asynchronous close operation.</param><param name="state">An object, specified by the application, that contains state information associated with the asynchronous close operation.</param><exception cref="T:System.ServiceModel.CommunicationObjectFaultedException"><see cref="M:System.ServiceModel.ICommunicationObject.BeginClose"/> was called on an object in the <see cref="F:System.ServiceModel.CommunicationState.Faulted"/> state.</exception><exception cref="T:System.TimeoutException">The default timeout elapsed before the <see cref="T:System.ServiceModel.ICommunicationObject"/> was able to close gracefully.</exception>
        public IAsyncResult BeginClose(AsyncCallback callback, object state)
        {
            return ProxiedClient.BeginClose(callback, state);
        }

        /// <summary>
        /// Begins an asynchronous operation to close a communication object with a specified timeout.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.IAsyncResult"/> that references the asynchronous close operation.
        /// </returns>
        /// <param name="timeout">The <see cref="T:System.Timespan"/> that specifies how long the send operation has to complete before timing out.</param><param name="callback">The <see cref="T:System.AsyncCallback"/> delegate that receives notification of the completion of the asynchronous close operation.</param><param name="state">An object, specified by the application, that contains state information associated with the asynchronous close operation.</param><exception cref="T:System.ServiceModel.CommunicationObjectFaultedException"><see cref="M:System.ServiceModel.ICommunicationObject.BeginClose"/> was called on an object in the <see cref="F:System.ServiceModel.CommunicationState.Faulted"/> state.</exception><exception cref="T:System.TimeoutException">The specified timeout elapsed before the <see cref="T:System.ServiceModel.ICommunicationObject"/> was able to close gracefully.</exception>
        public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return ProxiedClient.BeginClose(timeout, callback, state);
        }

        /// <summary>
        /// Begins an asynchronous operation to open a communication object.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.IAsyncResult"/> that references the asynchronous open operation. 
        /// </returns>
        /// <param name="callback">The <see cref="T:System.AsyncCallback"/> delegate that receives notification of the completion of the asynchronous open operation.</param><param name="state">An object, specified by the application, that contains state information associated with the asynchronous open operation.</param><exception cref="T:System.ServiceModel.CommunicationException">The <see cref="T:System.ServiceModel.ICommunicationObject"/> was unable to be opened and has entered the <see cref="F:System.ServiceModel.CommunicationState.Faulted"/> state.</exception><exception cref="T:System.TimeoutException">The default open timeout elapsed before the <see cref="T:System.ServiceModel.ICommunicationObject"/> was able to enter the <see cref="F:System.ServiceModel.CommunicationState.Opened"/> state and has entered the <see cref="F:System.ServiceModel.CommunicationState.Faulted"/> state.</exception>
        public IAsyncResult BeginOpen(AsyncCallback callback, object state)
        {
            return ProxiedClient.BeginOpen(callback, state);
        }

        /// <summary>
        /// Begins an asynchronous operation to open a communication object within a specified interval of time.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.IAsyncResult"/> that references the asynchronous open operation. 
        /// </returns>
        /// <param name="timeout">The <see cref="T:System.Timespan"/> that specifies how long the send operation has to complete before timing out.</param><param name="callback">The <see cref="T:System.AsyncCallback"/> delegate that receives notification of the completion of the asynchronous open operation.</param><param name="state">An object, specified by the application, that contains state information associated with the asynchronous open operation.</param><exception cref="T:System.ServiceModel.CommunicationException">The <see cref="T:System.ServiceModel.ICommunicationObject"/> was unable to be opened and has entered the <see cref="F:System.ServiceModel.CommunicationState.Faulted"/> state.</exception><exception cref="T:System.TimeoutException">The specified timeout elapsed before the <see cref="T:System.ServiceModel.ICommunicationObject"/> was able to enter the <see cref="F:System.ServiceModel.CommunicationState.Opened"/> state and has entered the <see cref="F:System.ServiceModel.CommunicationState.Faulted"/> state.</exception>
        public IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return ProxiedClient.BeginOpen(timeout, callback, state);
        }

        /// <summary>
        /// Begins processing of log messages.
        /// </summary>
        /// <param name="events">The events to send.</param>
        /// <param name="callback">The callback.</param>
        /// <param name="asyncState">Asynchronous state.</param>
        /// <returns>
        /// IAsyncResult value which can be passed to <see cref="ILogReceiverOneWayClient.EndProcessLogMessages"/>.
        /// </returns>
        public IAsyncResult BeginProcessLogMessages(NLogEvents events, AsyncCallback callback, object asyncState)
        {
            return ProxiedClient.BeginProcessLogMessages(events, callback, asyncState);
        }

        /// <summary>
        /// Enables the user to configure client and service credentials as well as service credential authentication settings for use on the client side of communication.
        /// </summary>
        public ClientCredentials ClientCredentials
        {
            get { return ProxiedClient.ClientCredentials; }
        }



        /// <summary>
        /// Causes a communication object to transition from its current state into the closed state.  
        /// </summary>
        /// <param name="timeout">The <see cref="T:System.Timespan"/> that specifies how long the send operation has to complete before timing out.</param><exception cref="T:System.ServiceModel.CommunicationObjectFaultedException"><see cref="M:System.ServiceModel.ICommunicationObject.Close"/> was called on an object in the <see cref="F:System.ServiceModel.CommunicationState.Faulted"/> state.</exception><exception cref="T:System.TimeoutException">The timeout elapsed before the <see cref="T:System.ServiceModel.ICommunicationObject"/> was able to close gracefully.</exception>
        public void Close(TimeSpan timeout)
        {
            ProxiedClient.Close(timeout);
        }

        /// <summary>
        /// Causes a communication object to transition from its current state into the closed state.  
        /// </summary>
        /// <exception cref="T:System.ServiceModel.CommunicationObjectFaultedException"><see cref="M:System.ServiceModel.ICommunicationObject.Close"/> was called on an object in the <see cref="F:System.ServiceModel.CommunicationState.Faulted"/> state.</exception><exception cref="T:System.TimeoutException">The default close timeout elapsed before the <see cref="T:System.ServiceModel.ICommunicationObject"/> was able to close gracefully.</exception>
        public void Close()
        {
            ProxiedClient.Close();
        }

        /// <summary>
        /// Closes the client asynchronously.
        /// </summary>
        /// <param name="userState">User-specific state.</param>
        public void CloseAsync(object userState)
        {
            ProxiedClient.CloseAsync(userState);
        }

        /// <summary>
        /// Closes the client asynchronously.
        /// </summary>
        public void CloseAsync()
        {
            ProxiedClient.CloseAsync();
        }

        /// <summary>
        /// Occurs when Close operation has completed.
        /// </summary>
        public event EventHandler<AsyncCompletedEventArgs> CloseCompleted
        {
            add { ProxiedClient.CloseCompleted += value; }
            remove { ProxiedClient.CloseCompleted -= value; }
        }

        /// <summary>
        /// Occurs when the communication object completes its transition from the closing state into the closed state.
        /// </summary>
        public event EventHandler Closed
        {
            add { ProxiedClient.Closed += value; }
            remove { ProxiedClient.Closed -= value; }
        }

        /// <summary>
        /// Occurs when the communication object first enters the closing state.
        /// </summary>
        public event EventHandler Closing
        {
            add { ProxiedClient.Closing += value; }
            remove { ProxiedClient.Closing -= value; }
        }

#if !SILVERLIGHT

        /// <summary>
        /// Instructs the inner channel to display a user interface if one is required to initialize the channel prior to using it.
        /// </summary>
        public void DisplayInitializationUI()
        {
            ProxiedClient.DisplayInitializationUI();
        }

#endif

#if !NET4_0 && !NET3_5
        /// <summary>
        /// Gets or sets the cookie container.
        /// </summary>
        /// <value>The cookie container.</value>
        public CookieContainer CookieContainer
        {
            get { return ProxiedClient.CookieContainer; }
            set { ProxiedClient.CookieContainer = value; }
        }

#endif


        /// <summary>
        /// Completes an asynchronous operation to close a communication object.
        /// </summary>
        /// <param name="result">The <see cref="T:System.IAsyncResult"/> that is returned by a call to the <see cref="M:System.ServiceModel.ICommunicationObject.BeginClose"/> method.</param><exception cref="T:System.ServiceModel.CommunicationObjectFaultedException"><see cref="M:System.ServiceModel.ICommunicationObject.BeginClose"/> was called on an object in the <see cref="F:System.ServiceModel.CommunicationState.Faulted"/> state.</exception><exception cref="T:System.TimeoutException">The timeout elapsed before the <see cref="T:System.ServiceModel.ICommunicationObject"/> was able to close gracefully.</exception>
        public void EndClose(IAsyncResult result)
        {
            ProxiedClient.EndClose(result);
        }

        /// <summary>
        /// Completes an asynchronous operation to open a communication object.
        /// </summary>
        /// <param name="result">The <see cref="T:System.IAsyncResult"/> that is returned by a call to the <see cref="M:System.ServiceModel.ICommunicationObject.BeginOpen"/> method.</param><exception cref="T:System.ServiceModel.CommunicationException">The <see cref="T:System.ServiceModel.ICommunicationObject"/> was unable to be opened and has entered the <see cref="F:System.ServiceModel.CommunicationState.Faulted"/> state.</exception><exception cref="T:System.TimeoutException">The timeout elapsed before the <see cref="T:System.ServiceModel.ICommunicationObject"/> was able to enter the <see cref="F:System.ServiceModel.CommunicationState.Opened"/> state and has entered the <see cref="F:System.ServiceModel.CommunicationState.Faulted"/> state.</exception>
        public void EndOpen(IAsyncResult result)
        {
            ProxiedClient.EndOpen(result);
        }

        /// <summary>
        /// Gets the target endpoint for the service to which the WCF client can connect.
        /// </summary>
        public ServiceEndpoint Endpoint
        {
            get { return ProxiedClient.Endpoint; }
        }

        /// <summary>
        /// Ends asynchronous processing of log messages.
        /// </summary>
        /// <param name="result">The result.</param>
        public void EndProcessLogMessages(IAsyncResult result)
        {
            ProxiedClient.EndProcessLogMessages(result);
        }

        /// <summary>
        /// Occurs when the communication object first enters the faulted state.
        /// </summary>
        public event EventHandler Faulted
        {
            add { ProxiedClient.Faulted += value; }
            remove { ProxiedClient.Faulted -= value; }
        }

        /// <summary>
        /// Gets the underlying <see cref="IClientChannel"/> implementation.
        /// </summary>
        public IClientChannel InnerChannel
        {
            get { return ProxiedClient.InnerChannel; }
        }

        /// <summary>
        /// Causes a communication object to transition from the created state into the opened state.  
        /// </summary>
        /// <exception cref="T:System.ServiceModel.CommunicationException">The <see cref="T:System.ServiceModel.ICommunicationObject"/> was unable to be opened and has entered the <see cref="F:System.ServiceModel.CommunicationState.Faulted"/> state.</exception><exception cref="T:System.TimeoutException">The default open timeout elapsed before the <see cref="T:System.ServiceModel.ICommunicationObject"/> was able to enter the <see cref="F:System.ServiceModel.CommunicationState.Opened"/> state and has entered the <see cref="F:System.ServiceModel.CommunicationState.Faulted"/> state.</exception>
        public void Open()
        {
            ProxiedClient.Open();
        }

        /// <summary>
        /// Causes a communication object to transition from the created state into the opened state within a specified interval of time.
        /// </summary>
        /// <param name="timeout">The <see cref="T:System.Timespan"/> that specifies how long the send operation has to complete before timing out.</param><exception cref="T:System.ServiceModel.CommunicationException">The <see cref="T:System.ServiceModel.ICommunicationObject"/> was unable to be opened and has entered the <see cref="F:System.ServiceModel.CommunicationState.Faulted"/> state.</exception><exception cref="T:System.TimeoutException">The specified timeout elapsed before the <see cref="T:System.ServiceModel.ICommunicationObject"/> was able to enter the <see cref="F:System.ServiceModel.CommunicationState.Opened"/> state and has entered the <see cref="F:System.ServiceModel.CommunicationState.Faulted"/> state.</exception>
        public void Open(TimeSpan timeout)
        {
            ProxiedClient.Open(timeout);
        }

        /// <summary>
        /// Opens the client asynchronously.
        /// </summary>
        public void OpenAsync()
        {
            ProxiedClient.OpenAsync();
        }

        /// <summary>
        /// Opens the client asynchronously.
        /// </summary>
        /// <param name="userState">User-specific state.</param>
        public void OpenAsync(object userState)
        {
            ProxiedClient.OpenAsync(userState);
        }

        /// <summary>
        /// Occurs when Open operation has completed.
        /// </summary>
        public event EventHandler<AsyncCompletedEventArgs> OpenCompleted
        {
            add { ProxiedClient.OpenCompleted += value; }
            remove { ProxiedClient.OpenCompleted -= value; }
        }

        /// <summary>
        /// Occurs when the communication object completes its transition from the opening state into the opened state.
        /// </summary>
        public event EventHandler Opened
        {
            add { ProxiedClient.Opened += value; }
            remove { ProxiedClient.Opened -= value; }
        }

        /// <summary>
        /// Occurs when the communication object first enters the opening state.
        /// </summary>
        public event EventHandler Opening
        {
            add { ProxiedClient.Opening += value; }
            remove { ProxiedClient.Opening -= value; }
        }

        /// <summary>
        /// Processes the log messages asynchronously.
        /// </summary>
        /// <param name="events">The events to send.</param>
        public void ProcessLogMessagesAsync(NLogEvents events)
        {
            ProxiedClient.ProcessLogMessagesAsync(events);
        }

        /// <summary>
        /// Processes the log messages asynchronously.
        /// </summary>
        /// <param name="events">The events to send.</param>
        /// <param name="userState">User-specific state.</param>
        public void ProcessLogMessagesAsync(NLogEvents events, object userState)
        {
            ProxiedClient.ProcessLogMessagesAsync(events, userState);
        }

        /// <summary>
        /// Occurs when the log message processing has completed.
        /// </summary>
        public event EventHandler<AsyncCompletedEventArgs> ProcessLogMessagesCompleted
        {
            add { ProxiedClient.ProcessLogMessagesCompleted += value; }
            remove { ProxiedClient.ProcessLogMessagesCompleted -= value; }
        }

        /// <summary>
        /// Gets the current state of the communication-oriented object.
        /// </summary>
        /// <returns>
        /// The value of the <see cref="T:System.ServiceModel.CommunicationState"/> of the object.
        /// </returns>
        public CommunicationState State
        {
            get { return ProxiedClient.State; }
        }

        #endregion


        /// <summary>
        /// Causes a communication object to transition from its current state into the closed state.
        /// </summary>
        public void CloseCommunicationObject()
        {
            ProxiedClient.Close();
        }
    }
}

#endif