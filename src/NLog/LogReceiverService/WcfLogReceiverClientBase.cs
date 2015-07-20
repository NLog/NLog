using System;
using System.ComponentModel;
#if SILVERLIGHT
using System.Net;
#endif
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace NLog.LogReceiverService
{
    /// <summary>
    /// Base class for implementing 
    /// </summary>
    /// <typeparam name="TLogReceiverClient"></typeparam>
    public abstract class WcfLogReceiverClientBase<TLogReceiverClient> : ClientBase<TLogReceiverClient>, ILogReceiverClient
        where TLogReceiverClient : class, ILogReceiverClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WcfLogReceiverTwoWayClient"/> class.
        /// </summary>
        protected WcfLogReceiverClientBase()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfLogReceiverTwoWayClient"/> class.
        /// </summary>
        /// <param name="endpointConfigurationName">Name of the endpoint configuration.</param>
        protected WcfLogReceiverClientBase(string endpointConfigurationName) :
            base(endpointConfigurationName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfLogReceiverTwoWayClient"/> class.
        /// </summary>
        /// <param name="endpointConfigurationName">Name of the endpoint configuration.</param>
        /// <param name="remoteAddress">The remote address.</param>
        protected WcfLogReceiverClientBase(string endpointConfigurationName, string remoteAddress) :
            base(endpointConfigurationName, remoteAddress)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfLogReceiverTwoWayClient"/> class.
        /// </summary>
        /// <param name="endpointConfigurationName">Name of the endpoint configuration.</param>
        /// <param name="remoteAddress">The remote address.</param>
        protected WcfLogReceiverClientBase(string endpointConfigurationName, EndpointAddress remoteAddress) :
            base(endpointConfigurationName, remoteAddress)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfLogReceiverTwoWayClient"/> class.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <param name="remoteAddress">The remote address.</param>
        public WcfLogReceiverClientBase(Binding binding, EndpointAddress remoteAddress) :
            base(binding, remoteAddress)
        {
        }

        /// <summary>
        /// Occurs when the log message processing has completed.
        /// </summary>
        public virtual event EventHandler<AsyncCompletedEventArgs> ProcessLogMessagesCompleted;

        /// <summary>
        /// Occurs when Open operation has completed.
        /// </summary>
        public virtual event EventHandler<AsyncCompletedEventArgs> OpenCompleted;

        /// <summary>
        /// Occurs when Close operation has completed.
        /// </summary>
        public virtual event EventHandler<AsyncCompletedEventArgs> CloseCompleted;

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
        /// IAsyncResult value which can be passed to <see cref="ILogReceiverTwoWayClient.EndProcessLogMessages"/>.
        /// </returns>
        public IAsyncResult BeginProcessLogMessages(NLogEvents events, AsyncCallback callback, object asyncState)
        {
            return this.Channel.BeginProcessLogMessages(events, callback, asyncState);
        }

        /// <summary>
        /// Ends asynchronous processing of log messages.
        /// </summary>
        /// <param name="result">The result.</param>
        public void EndProcessLogMessages(IAsyncResult result)
        {
            this.Channel.EndProcessLogMessages(result);
        }

        private IAsyncResult OnBeginProcessLogMessages(object[] inValues, AsyncCallback callback, object asyncState)
        {
            var events = (NLogEvents)inValues[0];
            return ((ILogReceiverTwoWayClient)this).BeginProcessLogMessages(events, callback, asyncState);
        }

        private object[] OnEndProcessLogMessages(IAsyncResult result)
        {
            ((ILogReceiverTwoWayClient)this).EndProcessLogMessages(result);
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




#if SILVERLIGHT
        /// <summary>
        /// Helper class
        /// </summary>
        protected class LogReceiverServerClientChannel : ChannelBase<TLogReceiverClient>, ILogReceiverClient, ILogReceiverTwoWayClient, ILogReceiverOneWayClient

        {
            /// <summary>
            /// Init helper class
            /// </summary>
            /// <param name="client"></param>
            public LogReceiverServerClientChannel(ClientBase<TLogReceiverClient> client) :
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