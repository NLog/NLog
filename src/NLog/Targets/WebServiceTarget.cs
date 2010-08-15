// 
// Copyright (c) 2004-2010 Jaroslaw Kowalski <jaak@jkowalski.net>
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

#if !SILVERLIGHT

namespace NLog.Targets
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Xml;
    using NLog.Internal;

    /// <summary>
    /// Calls the specified web service on each log message.
    /// </summary>
    /// <seealso href="http://nlog-project.org/wiki/WebService_target">Documentation on NLog Wiki</seealso>
    /// <remarks>
    /// The web service must implement a method that accepts a number of string parameters.
    /// </remarks>
    /// <example>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" source="examples/targets/Configuration File/WebService/NLog.config" />
    /// <p>
    /// This assumes just one target and a single rule. More configuration
    /// options are described <a href="config.html">here</a>.
    /// </p>
    /// <p>
    /// To set up the log target programmatically use code like this:
    /// </p>
    /// <code lang="C#" source="examples/targets/Configuration API/WebService/Simple/Example.cs" />
    /// <p>The example web service that works with this example is shown below</p>
    /// <code lang="C#" source="examples/targets/Configuration API/WebService/Simple/WebService1/Service1.asmx.cs" />
    /// </example>
    [Target("WebService")]
    public sealed class WebServiceTarget : MethodCallTargetBase
    {
        private const string SoapEnvelopeNamespace = "http://schemas.xmlsoap.org/soap/envelope/";
        private const string Soap12EnvelopeNamespace = "http://www.w3.org/2003/05/soap-envelope";

        /// <summary>
        /// Initializes a new instance of the <see cref="WebServiceTarget" /> class.
        /// </summary>
        public WebServiceTarget()
        {
            this.Protocol = WebServiceProtocol.Soap11;
        }

        /// <summary>
        /// Gets or sets the web service URL.
        /// </summary>
        /// <docgen category='Web Service Options' order='10' />
        public Uri Url { get; set; }

        /// <summary>
        /// Gets or sets the Web service method name.
        /// </summary>
        /// <docgen category='Web Service Options' order='10' />
        public string MethodName { get; set; }

        /// <summary>
        /// Gets or sets the Web service namespace.
        /// </summary>
        /// <docgen category='Web Service Options' order='10' />
        public string Namespace { get; set; }

        /// <summary>
        /// Gets or sets the protocol to be used when calling web service.
        /// </summary>
        /// <docgen category='Web Service Options' order='10' />
        [DefaultValue("Soap11")]
        public WebServiceProtocol Protocol { get; set; }

        /// <summary>
        /// Invokes the web service method.
        /// </summary>
        /// <param name="parameters">Parameters to be passed.</param>
        protected override void DoInvoke(object[] parameters)
        {
            switch (this.Protocol)
            {
                case WebServiceProtocol.Soap11:
                    this.InvokeSoap11(parameters);
                    break;

                case WebServiceProtocol.Soap12:
                    this.InvokeSoap12(parameters);
                    break;

                case WebServiceProtocol.HttpGet:
                    throw new NotSupportedException();

                case WebServiceProtocol.HttpPost:
                    this.InvokeHttpPost(parameters);
                    break;
            }
        }

        private void InvokeSoap11(object[] parameters)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.Url);
            request.Method = "POST";
            request.ContentType = "text/xml; charset=utf-8";

            string soapAction;

            if (this.Namespace.EndsWith("/", StringComparison.Ordinal))
            {
                soapAction = "SOAPAction: " + this.Namespace + this.MethodName;
            }
            else
            {
                soapAction = "SOAPAction: " + this.Namespace + "/" + this.MethodName;
            }

            request.Headers.Add(soapAction);

            using (Stream s = request.GetRequestStream())
            {
                XmlWriter xtw = XmlWriter.Create(s, new XmlWriterSettings { Encoding = Encoding.UTF8 });

                xtw.WriteStartElement("soap", "Envelope", SoapEnvelopeNamespace);
                xtw.WriteStartElement("Body", SoapEnvelopeNamespace);
                xtw.WriteStartElement(this.MethodName, this.Namespace);
                int i = 0;

                foreach (MethodCallParameter par in this.Parameters)
                {
                    xtw.WriteElementString(par.Name, Convert.ToString(parameters[i], CultureInfo.InvariantCulture));
                    i++;
                }

                xtw.WriteEndElement(); // methodname
                xtw.WriteEndElement(); // Body
                xtw.WriteEndElement(); // soap:Envelope
                xtw.Flush();
            }

            WebResponse response = request.GetResponse();
            response.Close();
        }

        private void InvokeSoap12(object[] parameterValues)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.Url);
            request.Method = "POST";
            request.ContentType = "text/xml; charset=utf-8";

            using (Stream s = request.GetRequestStream())
            {
                XmlTextWriter xtw = new XmlTextWriter(s, System.Text.Encoding.UTF8);

                xtw.WriteStartElement("soap12", "Envelope", Soap12EnvelopeNamespace);
                xtw.WriteStartElement("Body", Soap12EnvelopeNamespace);
                xtw.WriteStartElement(this.MethodName, this.Namespace);
                int i = 0;
                foreach (MethodCallParameter par in this.Parameters)
                {
                    xtw.WriteElementString(par.Name, Convert.ToString(parameterValues[i], CultureInfo.InvariantCulture));
                    i++;
                }

                xtw.WriteEndElement();
                xtw.WriteEndElement();
                xtw.WriteEndElement();
                xtw.Flush();
            }

            WebResponse response = request.GetResponse();
            response.Close();
        }

        private void InvokeHttpPost(object[] parameterValues)
        {
            string completeUrl;

            if (this.MethodName.EndsWith("/", StringComparison.Ordinal))
            {
                completeUrl = this.Url + this.MethodName;
            }
            else
            {
                completeUrl = this.Url + "/" + this.MethodName;
            }

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(completeUrl);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            string separator = string.Empty;
            using (Stream s = request.GetRequestStream())
            {
                var sw = new StreamWriter(s);
                int i = 0;
                foreach (MethodCallParameter parameter in this.Parameters)
                {
                    sw.Write(separator);
                    sw.Write(parameter.Name);
                    sw.Write("=");
                    sw.Write(UrlHelper.UrlEncode(Convert.ToString(parameterValues[i], CultureInfo.InvariantCulture), true));
                    separator = "&";
                }

                sw.Flush();
            }

            WebResponse response = request.GetResponse();
            response.Close();
        }
    }
}

#endif