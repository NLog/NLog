// 
// Copyright (c) 2004-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
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
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Reflection;
using System.Web.Services.Protocols;

using NLog.Config;
using System.IO;
using System.Xml;
using System.Net;

namespace NLog.Targets
{
    /// <summary>
    /// Calls the specified web service on each logging message. 
    /// </summary>
    /// <remarks>
    /// The web service must implement a method that accepts a number of string parameters.
    /// </remarks>
    /// <example>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" src="examples/targets/Configuration File/WebService/NLog.config" />
    /// <p>
    /// This assumes just one target and a single rule. More configuration
    /// options are described <a href="config.html">here</a>.
    /// </p>
    /// <p>
    /// To set up the log target programmatically use code like this:
    /// </p>
    /// <code lang="C#" src="examples/targets/Configuration API/WebService/Simple/Example.cs" />
    /// <p>The example web service that works with this example is shown below</p>
    /// <code lang="C#" src="examples/targets/Configuration API/WebService/Simple/WebService1/Service1.asmx.cs" />
    /// </example>
    [Target("WebService")]
    public sealed class WebServiceTarget: MethodCallTargetBase
    {
        const string soapEnvelopeNamespace = "http://schemas.xmlsoap.org/soap/envelope/";
        const string soap12EnvelopeNamespace = "http://www.w3.org/2003/05/soap-envelope";

        /// <summary>
        /// Web service protocol
        /// </summary>
        public enum WebServiceProtocol
        {
            /// <summary>
            /// SOAP 1.1
            /// </summary>
            Soap11,

            /// <summary>
            /// SOAP 1.2
            /// </summary>
            Soap12,

            /// <summary>
            /// HTTP POST
            /// </summary>
            HttpPost,

            /// <summary>
            /// HTTP GET
            /// </summary>
            HttpGet,
        }

        private string _methodName = null;
        private string _url = null;
        private string _namespace = null;
        private WebServiceProtocol _protocol = WebServiceProtocol.Soap11;

        /// <summary>
        /// Web service URL.
        /// </summary>
        public string Url
        {
            get { return _url; }
            set { _url = value; } 
        }

        /// <summary>
        /// Web service method name.
        /// </summary>
        public string MethodName
        {
            get { return _methodName; }
            set { _methodName = value; }
        }

        /// <summary>
        /// Web service namespace.
        /// </summary>
        public string Namespace
        {
            get { return _namespace; }
            set { _namespace = value; }
        }

        /// <summary>
        /// The protocol to be used when calling web service.
        /// </summary>
        [System.ComponentModel.DefaultValue("Soap11")]
        public WebServiceProtocol Protocol
        {
            get { return _protocol; }
            set { _protocol = value; }
        }

        /// <summary>
        /// Invokes the web service method.
        /// </summary>
        /// <param name="parameters">Parameters to be passed.</param>
        protected override void DoInvoke(object[]parameters)
        {
            switch (_protocol)
            {
                case WebServiceProtocol.Soap11:
                    InvokeSoap11(parameters);
                    break;

                case WebServiceProtocol.Soap12:
                    InvokeSoap12(parameters);
                    break;

                case WebServiceProtocol.HttpGet:
                    InvokeHttpGet(parameters);
                    break;

                case WebServiceProtocol.HttpPost:
                    InvokeHttpPost(parameters);
                    break;
            }
            //_client.DoInvoke(MethodName, parameters);
        }

        private void InvokeSoap11(object[] parameters)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Method = "POST";
            request.ContentType = "text/xml; charset=utf-8";

            string soapAction;

            if (Namespace.EndsWith("/"))
                soapAction = "SOAPAction: " + Namespace + MethodName;
            else
                soapAction = "SOAPAction: " + Namespace + "/" + MethodName;
            request.Headers.Add(soapAction);

            using (Stream s = request.GetRequestStream())
            {
                XmlTextWriter xtw = new XmlTextWriter(s, System.Text.Encoding.UTF8);

                xtw.WriteStartElement("soap", "Envelope", soapEnvelopeNamespace);
                xtw.WriteStartElement("Body", soapEnvelopeNamespace);
                xtw.WriteStartElement(MethodName, Namespace);
                for (int i = 0; i < Parameters.Count; ++i)
                {
                    xtw.WriteElementString(Parameters[i].Name, Convert.ToString(parameters[i]));
                }
                xtw.WriteEndElement();
                xtw.WriteEndElement();
                xtw.WriteEndElement();
                xtw.Flush();
            }

            WebResponse response = request.GetResponse();
            response.Close();
        }

        private void InvokeSoap12(object[] parameters)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Method = "POST";
            request.ContentType = "text/xml; charset=utf-8";

            using (Stream s = request.GetRequestStream())
            {
                XmlTextWriter xtw = new XmlTextWriter(s, System.Text.Encoding.UTF8);

                xtw.WriteStartElement("soap12", "Envelope", soap12EnvelopeNamespace);
                xtw.WriteStartElement("Body", soap12EnvelopeNamespace);
                xtw.WriteStartElement(MethodName, Namespace);
                for (int i = 0; i < Parameters.Count; ++i)
                {
                    xtw.WriteElementString(Parameters[i].Name, Convert.ToString(parameters[i]));
                }
                xtw.WriteEndElement();
                xtw.WriteEndElement();
                xtw.WriteEndElement();
                xtw.Flush();
            }

            WebResponse response = request.GetResponse();
            response.Close();
        }

        private void InvokeHttpPost(object[] parameters)
        {
            string CompleteUrl;

            if (MethodName.EndsWith("/"))
                CompleteUrl = Url + MethodName;
            else
                CompleteUrl = Url + "/" + MethodName;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(CompleteUrl);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            using (Stream s = request.GetRequestStream())
            using (StreamWriter sw = new StreamWriter(s))
            {
                for (int i = 0; i < Parameters.Count; ++i)
                {
                    sw.Write(Parameters[i].Name + "=" + System.Web.HttpUtility.UrlEncodeUnicode(Convert.ToString(parameters[i])) + ((i < (Parameters.Count - 1)) ? "&" : ""));
                }
                sw.Flush();
            }

            WebResponse response = request.GetResponse();
            response.Close();
        }

        private void InvokeHttpGet(object[] parameters)
        {
            throw new NotSupportedException();
        }
    }
}
