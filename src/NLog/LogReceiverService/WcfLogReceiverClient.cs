﻿// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
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

#if WCF_SUPPORTED

namespace NLog.LogReceiverService
{
    using System;
    using System.ComponentModel;
    using System.Net;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    /// <summary>
    /// Log Receiver Client using WCF.
    /// </summary>
    public sealed class WcfLogReceiverClient : ClientBase<ILogReceiverClient>, ILogReceiverClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WcfLogReceiverClient"/> class.
        /// </summary>
        public WcfLogReceiverClient()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfLogReceiverClient"/> class.
        /// </summary>
        /// <param name="endpointConfigurationName">Name of the endpoint configuration.</param>
        public WcfLogReceiverClient(string endpointConfigurationName) :
            base(endpointConfigurationName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfLogReceiverClient"/> class.
        /// </summary>
        /// <param name="endpointConfigurationName">Name of the endpoint configuration.</param>
        /// <param name="remoteAddress">The remote address.</param>
        public WcfLogReceiverClient(string endpointConfigurationName, string remoteAddress) :
            base(endpointConfigurationName, remoteAddress)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfLogReceiverClient"/> class.
        /// </summary>
        /// <param name="endpointConfigurationName">Name of the endpoint configuration.</param>
        /// <param name="remoteAddress">The remote address.</param>
        public WcfLogReceiverClient(string endpointConfigurationName, EndpointAddress remoteAddress) :
            base(endpointConfigurationName, remoteAddress)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfLogReceiverClient"/> class.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <param name="remoteAddress">The remote address.</param>
        public WcfLogReceiverClient(Binding binding, EndpointAddress remoteAddress) :
            base(binding, remoteAddress)
        {
        }

        /// <summary>
        /// Occurs when the log message processing has completed.
        /// </summary>
        public event EventHandler<AsyncCompletedEventArgs> ProcessLogMessagesCompleted;

        /// <summary>
        /// Occurs when Open operation has completed.
        /// </summary>
        public event EventHandler<AsyncCompletedEventArgs> OpenCompleted;

        /// <summary>
        /// Occurs when Close operation has completed.
        /// </summary>
        public event EventHandler<AsyncCompletedEventArgs> CloseCompleted;

        /// <summary>
        /// Gets or sets if calls to the server should use the one way method or the original two way method.
        /// </summary>
        public Boolean UseOneWayCallsToServer { get; set; }

#if SILVERLIGHT
        /// <summary>
        /// Gets or sets the cookie container.
        /// </summary>
        /// <value>The cookie container.</value>
        public CookieContainer CookieContainer
        {
            get
            {
                var httpCookieContainerManager = this.InnerChannel.GetProperty<IHttpCookieContainerManager>();
                if (httpCookieContainerManager != null)
                {
                    return httpCookieContainerManager.CookieContainer;
                }

                return null;
            }
            set
            {
                var httpCookieContainerManager = this.InnerChannel.GetProperty<IHttpCookieContainerManager>();
                if (httpCookieContainerManager != null)
                {
                    httpCookieContainerManager.CookieContainer = value;
                }
                else
                {
                    throw new InvalidOperationException("Unable to set the CookieContainer. Please make sure the binding contains an HttpCookieContainerBindingElement.");
                }
            }
        }
#endif

        /// <summary>
        /// Opens the client asynchronously.
        /// </summary>
        public void OpenAsync()
        {
            this.OpenAsync(null);
        }

        /// <summary>
        /// Opens the client asynchronously.
        /// </summary>
        /// <param name="userState">User-specific state.</param>
        public void OpenAsync(object userState)
        {
            this.InvokeAsync(this.OnBeginOpen, null, this.OnEndOpen, this.OnOpenCompleted, userState);
        }

        /// <summary>
        /// Closes the client asynchronously.
        /// </summary>
        public void CloseAsync()
        {
            this.CloseAsync(null);
        }

        /// <summary>
        /// Closes the client asynchronously.
        /// </summary>
        /// <param name="userState">User-specific state.</param>
        public void CloseAsync(object userState)
        {
            this.InvokeAsync(this.OnBeginClose, null, this.OnEndClose, this.OnCloseCompleted, userState);
        }

        /// <summary>
        /// Processes the log messages asynchronously.
        /// </summary>
        /// <param name="events">The events to send.</param>
        public void ProcessLogMessagesAsync(NLogEvents events)
        {
            this.ProcessLogMessagesAsync(events, null);
        }

        /// <summary>
        /// Processes the log messages asynchronously.
        /// </summary>
        /// <param name="events">The events to send.</param>
        /// <param name="userState">User-specific state.</param>
        public void ProcessLogMessagesAsync(NLogEvents events, object userState)
        {
            this.InvokeAsync(
                this.OnBeginProcessLogMessages,
                new object[] { events },
                this.OnEndProcessLogMessages,
                this.OnProcessLogMessagesCompleted,
                userState);
        }

        /// <summary>
        /// Begins processing of log messages.
        /// </summary>
        /// <param name="events">The events to send.</param>
        /// <param name="callback">The callback.</param>
        /// <param name="asyncState">Asynchronous state.</param>
        /// <returns>
        /// IAsyncResult value which can be passed to <see cref="ILogReceiverClient.EndProcessLogMessages"/>.
        /// </returns>
        IAsyncResult ILogReceiverClient.BeginProcessLogMessages(NLogEvents events, AsyncCallback callback, object asyncState)
        {
            return this.Channel.BeginProcessLogMessages(events, callback, asyncState);
        }

        /// <summary>
        /// Begins processing of log messages.
        /// </summary>
        /// <param name="events">The events to send.</param>
        /// <param name="callback">The callback.</param>
        /// <param name="asyncState">Asynchronous state.</param>
        /// <returns>
        /// IAsyncResult value which can be passed to <see cref="ILogReceiverClient.EndProcessLogMessages"/>.
        /// </returns>
        IAsyncResult ILogReceiverClient.BeginProcessLogMessagesV2(NLogEvents events, AsyncCallback callback, object asyncState)
        {
            return this.Channel.BeginProcessLogMessagesV2(events, callback, asyncState);
        }

        /// <summary>
        /// Ends asynchronous processing of log messages.
        /// </summary>
        /// <param name="result">The result.</param>
        void ILogReceiverClient.EndProcessLogMessages(IAsyncResult result)
        {
            this.Channel.EndProcessLogMessages(result);
        }

        /// <summary>
        /// Ends asynchronous processing of log messages.
        /// </summary>
        /// <param name="result">The result.</param>
        void ILogReceiverClient.EndProcessLogMessagesV2(IAsyncResult result)
        {
            this.Channel.EndProcessLogMessagesV2(result);
        }

#if SILVERLIGHT
        /// <summary>
        /// Returns a new channel from the client to the service.
        /// </summary>
        /// <returns>
        /// A channel of type <see cref="ILogReceiverClient"/> that identifies the type 
        /// of service contract encapsulated by this client object (proxy).
        /// </returns>
        protected override ILogReceiverClient CreateChannel()
        {
            return new LogReceiverServerClientChannel(this);
        }
#endif

        private IAsyncResult OnBeginProcessLogMessages(object[] inValues, AsyncCallback callback, object asyncState)
        {
            var events = (NLogEvents)inValues[0];
            if (UseOneWayCallsToServer) {
                return ((ILogReceiverClient)this).BeginProcessLogMessagesV2(events, callback, asyncState);
            }
            else {
                return ((ILogReceiverClient)this).BeginProcessLogMessages(events, callback, asyncState);
            }
        }

        private object[] OnEndProcessLogMessages(IAsyncResult result)
        {
            if (UseOneWayCallsToServer) {
                ((ILogReceiverClient)this).EndProcessLogMessagesV2(result);
            }
            else {
                ((ILogReceiverClient)this).EndProcessLogMessages(result);
            }
            return null;
        }

        private void OnProcessLogMessagesCompleted(object state)
        {
            if (this.ProcessLogMessagesCompleted != null)
            {
                var e = (InvokeAsyncCompletedEventArgs)state;

                this.ProcessLogMessagesCompleted(this, new AsyncCompletedEventArgs(e.Error, e.Cancelled, e.UserState));
            }
        }

        private IAsyncResult OnBeginOpen(object[] inValues, AsyncCallback callback, object asyncState)
        {
            return ((ICommunicationObject)this).BeginOpen(callback, asyncState);
        }

        private object[] OnEndOpen(IAsyncResult result)
        {
            ((ICommunicationObject)this).EndOpen(result);
            return null;
        }

        private void OnOpenCompleted(object state)
        {
            if (this.OpenCompleted != null)
            {
                var e = (InvokeAsyncCompletedEventArgs)state;

                this.OpenCompleted(this, new AsyncCompletedEventArgs(e.Error, e.Cancelled, e.UserState));
            }
        }

        private IAsyncResult OnBeginClose(object[] inValues, AsyncCallback callback, object asyncState)
        {
            return ((ICommunicationObject)this).BeginClose(callback, asyncState);
        }

        private object[] OnEndClose(IAsyncResult result)
        {
            ((ICommunicationObject)this).EndClose(result);
            return null;
        }

        private void OnCloseCompleted(object state)
        {
            if (this.CloseCompleted != null)
            {
                var e = (InvokeAsyncCompletedEventArgs)state;

                this.CloseCompleted(this, new AsyncCompletedEventArgs(e.Error, e.Cancelled, e.UserState));
            }
        }

#if SILVERLIGHT
        private class LogReceiverServerClientChannel : ChannelBase<ILogReceiverClient>, ILogReceiverClient
        {
            public LogReceiverServerClientChannel(ClientBase<ILogReceiverClient> client) :
                base(client)
            {
            }

            public IAsyncResult BeginProcessLogMessages(NLogEvents events, AsyncCallback callback, object asyncState)
            {
                return this.BeginInvoke(
                    "ProcessLogMessages", 
                    new object[] { events }, 
                    callback, 
                    asyncState);
            }

            public void EndProcessLogMessages(IAsyncResult result)
            {
                this.EndInvoke(
                    "ProcessLogMessages", 
                    new object[] { }, 
                    result);
            }
        }
#endif
    }
}

#endif