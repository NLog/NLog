// 
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
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    /// <summary>
    /// Log Receiver Client facade. It allows the use either of the one way or two way 
    /// service contract using WCF through its unified interface.
    /// </summary>
    public sealed class WcfLogReceiverClientFacade
    {
        private WcfLogReceiverClient m_twoWayClient;
        private WcfLogReceiverOneWayClient m_oneWayClient;
        private bool m_useOneWay;

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfLogReceiverClientFacade"/> class.
        /// </summary>
        /// <param name="useOneWay">Whether to use the one way or two way WCF client.</param>
        public WcfLogReceiverClientFacade(bool useOneWay)
        {
            m_useOneWay = useOneWay;
            if (useOneWay)
            {
                m_twoWayClient = null;
                m_oneWayClient = new WcfLogReceiverOneWayClient();
                HookOneWayEvents();
            }
            else
            {
                m_twoWayClient = new WcfLogReceiverClient();
                m_oneWayClient = null;
                HookTwoWayEvents();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfLogReceiverClientFacade"/> class.
        /// </summary>
        /// <param name="useOneWay">Whether to use the one way or two way WCF client.</param>
        /// <param name="endpointConfigurationName">Name of the endpoint configuration.</param>
        public WcfLogReceiverClientFacade(bool useOneWay, string endpointConfigurationName)
        {
            m_useOneWay = useOneWay;
            if (useOneWay)
            {
                m_twoWayClient = null;
                m_oneWayClient = new WcfLogReceiverOneWayClient(endpointConfigurationName);
                HookOneWayEvents();
            }
            else
            {
                m_twoWayClient = new WcfLogReceiverClient(endpointConfigurationName);
                m_oneWayClient = null;
                HookTwoWayEvents();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfLogReceiverClientFacade"/> class.
        /// </summary>
        /// <param name="useOneWay">Whether to use the one way or two way WCF client.</param>
        /// <param name="endpointConfigurationName">Name of the endpoint configuration.</param>
        /// <param name="remoteAddress">The remote address.</param>
        public WcfLogReceiverClientFacade(bool useOneWay, string endpointConfigurationName, string remoteAddress)
        {
            m_useOneWay = useOneWay;
            if (useOneWay)
            {
                m_twoWayClient = null;
                m_oneWayClient = new WcfLogReceiverOneWayClient(endpointConfigurationName, remoteAddress);
                HookOneWayEvents();
            }
            else
            {
                m_twoWayClient = new WcfLogReceiverClient(endpointConfigurationName, remoteAddress);
                m_oneWayClient = null;
                HookTwoWayEvents();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfLogReceiverClientFacade"/> class.
        /// </summary>
        /// <param name="useOneWay">Whether to use the one way or two way WCF client.</param>
        /// <param name="endpointConfigurationName">Name of the endpoint configuration.</param>
        /// <param name="remoteAddress">The remote address.</param>
        public WcfLogReceiverClientFacade(bool useOneWay, string endpointConfigurationName, EndpointAddress remoteAddress) 
        {
             m_useOneWay = useOneWay;
            if (useOneWay)
            {
                m_twoWayClient = null;
                m_oneWayClient = new WcfLogReceiverOneWayClient(endpointConfigurationName, remoteAddress);
                HookOneWayEvents();
            }
            else
            {
                m_twoWayClient = new WcfLogReceiverClient(endpointConfigurationName, remoteAddress);
                m_oneWayClient = null;
                HookTwoWayEvents();
            }
       }

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfLogReceiverClientFacade"/> class.
        /// </summary>
        /// <param name="useOneWay">Whether to use the one way or two way WCF client.</param>
        /// <param name="binding">The binding.</param>
        /// <param name="remoteAddress">The remote address.</param>
        public WcfLogReceiverClientFacade(bool useOneWay, Binding binding, EndpointAddress remoteAddress)
        {
            m_useOneWay = useOneWay;
            if (useOneWay)
            {
                m_twoWayClient = null;
                m_oneWayClient = new WcfLogReceiverOneWayClient(binding, remoteAddress);
                HookOneWayEvents();
            }
            else
            {
                m_twoWayClient = new WcfLogReceiverClient(binding, remoteAddress);
                m_oneWayClient = null;
                HookTwoWayEvents();
            }
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
        /// Gets the current state of the System.ServiceModel.ClientBase&lt;TChannel&gt; object.
        /// </summary>
        public CommunicationState State
        {
            get
            {
                if (m_useOneWay)
                {
                    return m_oneWayClient.State;
                }
                else
                {
                    return m_twoWayClient.State;
                }
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
                if (m_useOneWay)
                {
                    return m_oneWayClient.CookieContainer;
                }
                else
                {
                    return m_twoWayClient.CookieContainer;
                }
            }
            set
            {
                if (m_useOneWay)
                {
                    m_oneWayClient.CookieContainer = value;
                }
                else
                {
                    m_twoWayClient.CookieContainer = value;
                }
            }
        }
#endif

        /// <summary>
        /// Opens the client asynchronously.
        /// </summary>
        public void OpenAsync()
        {
            if (m_useOneWay)
            {
                m_oneWayClient.OpenAsync();
            }
            else
            {
                m_twoWayClient.OpenAsync();
            }
        }

        /// <summary>
        /// Opens the client asynchronously.
        /// </summary>
        /// <param name="userState">User-specific state.</param>
        public void OpenAsync(object userState)
        {
            if (m_useOneWay)
            {
                m_oneWayClient.OpenAsync(userState);
            }
            else
            {
                m_twoWayClient.OpenAsync(userState);
            }
        }

        /// <summary>
        /// Closes the client asynchronously.
        /// </summary>
        public void CloseAsync()
        {
            if (m_useOneWay)
            {
                m_oneWayClient.CloseAsync();
            }
            else
            {
                m_twoWayClient.CloseAsync();
            }
        }

        /// <summary>
        /// Closes the client asynchronously.
        /// </summary>
        /// <param name="userState">User-specific state.</param>
        public void CloseAsync(object userState)
        {
            if (m_useOneWay)
            {
                m_oneWayClient.CloseAsync(userState);
            }
            else
            {
                m_twoWayClient.CloseAsync(userState);
            }
        }

        /// <summary>
        /// Processes the log messages asynchronously.
        /// </summary>
        /// <param name="events">The events to send.</param>
        public void ProcessLogMessagesAsync(NLogEvents events)
        {
            if (m_useOneWay)
            {
                m_oneWayClient.ProcessLogMessagesAsync(events);
            }
            else
            {
                m_twoWayClient.ProcessLogMessagesAsync(events);
            }
        }

        /// <summary>
        /// Processes the log messages asynchronously.
        /// </summary>
        /// <param name="events">The events to send.</param>
        /// <param name="userState">User-specific state.</param>
        public void ProcessLogMessagesAsync(NLogEvents events, object userState)
        {
            if (m_useOneWay)
            {
                m_oneWayClient.ProcessLogMessagesAsync(events, userState);
            }
            else
            {
                m_twoWayClient.ProcessLogMessagesAsync(events, userState);
            }
        }

        /// <summary>
        /// Causes a communication object to transition from its current state into the closed state.
        /// </summary>
        public void CloseCommunicationObject()
        {
            if (m_useOneWay)
            {
                ((ICommunicationObject)m_oneWayClient).Close();
            }
            else
            {
                ((ICommunicationObject)m_twoWayClient).Close();
            }
        }


        private void OnProcessLogMessagesCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (this.ProcessLogMessagesCompleted != null)
            {
                this.ProcessLogMessagesCompleted(this, e);
            }
        }

        private void OnOpenCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (this.OpenCompleted != null)
            {
                this.OpenCompleted(this, e);
            }
        }

        private void OnCloseCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (this.CloseCompleted != null)
            {
                this.CloseCompleted(this, e);
            }
        }

        private void HookOneWayEvents()
        {
            m_oneWayClient.CloseCompleted += this.OnCloseCompleted;
            m_oneWayClient.OpenCompleted += this.OnOpenCompleted;
            m_oneWayClient.ProcessLogMessagesCompleted += this.OnProcessLogMessagesCompleted;
        }

        private void HookTwoWayEvents()
        {
            m_twoWayClient.CloseCompleted += this.OnCloseCompleted;
            m_twoWayClient.OpenCompleted += this.OnOpenCompleted;
            m_twoWayClient.ProcessLogMessagesCompleted += this.OnProcessLogMessagesCompleted;
        }
    }
}

#endif