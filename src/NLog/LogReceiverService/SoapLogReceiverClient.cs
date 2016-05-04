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

#if !WCF_SUPPORTED && !__IOS__ && !WINDOWS_PHONE && !__ANDROID__

namespace NLog.LogReceiverService
{
    using System;
    using System.Web.Services;
    using System.Web.Services.Description;
    using System.Web.Services.Protocols;
    using System.Xml.Serialization;

    /// <summary>
    /// Log Receiver Client using legacy SOAP client.
    /// </summary>
    [WebServiceBindingAttribute(Name = "BasicHttpBinding_ILogReceiverServer", Namespace = "http://tempuri.org/")]
    public class SoapLogReceiverClient : SoapHttpClientProtocol, ILogReceiverClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SoapLogReceiverClient"/> class.
        /// </summary>
        /// <param name="url">The service URL.</param>
        public SoapLogReceiverClient(string url)
        {
            this.Url = url;
        }

        /// <summary>
        /// Processes the log messages.
        /// </summary>
        /// <param name="events">The events.</param>
        [SoapDocumentMethodAttribute("http://nlog-project.org/ws/ILogReceiverServer/ProcessLogMessages", 
            RequestNamespace = LogReceiverServiceConfig.WebServiceNamespace, 
            ResponseNamespace = LogReceiverServiceConfig.WebServiceNamespace, 
            Use = SoapBindingUse.Literal, 
            ParameterStyle = SoapParameterStyle.Wrapped)]
        public void ProcessLogMessages(NLogEvents events)
        {
            this.Invoke("ProcessLogMessages", new object[] { events });
        }

        /// <summary>
        /// Begins processing of log messages.
        /// </summary>
        /// <param name="events">The events.</param>
        /// <param name="callback">The callback.</param>
        /// <param name="asyncState">Asynchronous state.</param>
        /// <returns>
        /// IAsyncResult value which can be passed to <see cref="ILogReceiverClient.EndProcessLogMessages"/>.
        /// </returns>
        public IAsyncResult BeginProcessLogMessages(NLogEvents events, AsyncCallback callback, object asyncState)
        {
            return this.BeginInvoke("ProcessLogMessages", new object[] { events }, callback, asyncState);
        }

        /// <summary>
        /// Ends asynchronous processing of log messages.
        /// </summary>
        /// <param name="result">The result.</param>
        public void EndProcessLogMessages(IAsyncResult result)
        {
            this.EndInvoke(result);
        }
    }
}

#endif