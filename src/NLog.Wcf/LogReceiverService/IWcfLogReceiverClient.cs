// 
// Copyright (c) 2004-2020 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

#if !NETSTANDARD
        /// <summary>
        /// Instructs the inner channel to display a user interface if one is required to initialize the channel prior to using it.
        /// </summary>
        void DisplayInitializationUI();
#endif

#if !NET35 && !NET40 && !NETSTANDARD

        /// <summary>
        /// Gets or sets the cookie container.
        /// </summary>
        /// <value>The cookie container.</value>
        CookieContainer CookieContainer { get; set; }
#endif
    }
}