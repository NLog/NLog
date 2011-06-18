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

namespace NLog.Targets
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Xml;
    using NLog.Common;
    using NLog.Internal;
    using NLog.Layouts;

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
            this.Encoding = Encoding.UTF8;
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
        /// Gets or sets the encoding.
        /// </summary>
        /// <docgen category='Web Service Options' order='10' />
        public Encoding Encoding { get; set; }

        /// <summary>
        /// Calls the target method. Must be implemented in concrete classes.
        /// </summary>
        /// <param name="parameters">Method call parameters.</param>
        protected override void DoInvoke(object[] parameters)
        {
            // method is not used, instead asynchronous overload will be used
            throw new NotImplementedException();
        }

        /// <summary>
        /// Invokes the web service method.
        /// </summary>
        /// <param name="parameters">Parameters to be passed.</param>
        /// <param name="continuation">The continuation.</param>
        protected override void DoInvoke(object[] parameters, AsyncContinuation continuation)
        {
            var request = (HttpWebRequest)WebRequest.Create(this.Url);
            byte[] postPayload = null;

            switch (this.Protocol)
            {
                case WebServiceProtocol.Soap11:
                    postPayload = this.PrepareSoap11Request(request, parameters);
                    break;

                case WebServiceProtocol.Soap12:
                    postPayload = this.PrepareSoap12Request(request, parameters);
                    break;

                case WebServiceProtocol.HttpGet:
                    throw new NotSupportedException();

                case WebServiceProtocol.HttpPost:
                    postPayload = this.PreparePostRequest(request, parameters);
                    break;
            }

            AsyncContinuation sendContinuation =
                ex =>
                    {
                        if (ex != null)
                        {
                            continuation(ex);
                            return;
                        }

                        request.BeginGetResponse(
                            r =>
                            {
                                try
                                {
                                    using (var response = request.EndGetResponse(r))
                                    {
                                    }

                                    continuation(null);
                                }
                                catch (Exception ex2)
                                {
                                    if (ex2.MustBeRethrown())
                                    {
                                        throw;
                                    }

                                    continuation(ex2);
                                }
                            }, 
                            null);
                    };

            if (postPayload != null && postPayload.Length > 0)
            {
                request.BeginGetRequestStream(
                    r =>
                        {
                            try
                            {
                                using (Stream stream = request.EndGetRequestStream(r))
                                {
                                    stream.Write(postPayload, 0, postPayload.Length);
                                }

                                sendContinuation(null);
                            }
                            catch (Exception ex)
                            {
                                if (ex.MustBeRethrown())
                                {
                                    throw;
                                }

                                continuation(ex);
                            }
                        },
                    null);
            }
            else
            {
                sendContinuation(null);
            }
        }

        private byte[] PrepareSoap11Request(HttpWebRequest request, object[] parameters)
        {
            request.Method = "POST";
            request.ContentType = "text/xml; charset=" + this.Encoding.WebName;

            if (this.Namespace.EndsWith("/", StringComparison.Ordinal))
            {
                request.Headers["SOAPAction"] = this.Namespace + this.MethodName;
            }
            else
            {
                request.Headers["SOAPAction"] = this.Namespace + "/" + this.MethodName;
            }

            using (var ms = new MemoryStream())
            {
                XmlWriter xtw = XmlWriter.Create(ms, new XmlWriterSettings { Encoding = this.Encoding });

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

                return ms.ToArray();
            }
        }

        private byte[] PrepareSoap12Request(HttpWebRequest request, object[] parameterValues)
        {
            request.Method = "POST";
            request.ContentType = "text/xml; charset=" + this.Encoding.WebName;

            using (var ms = new MemoryStream())
            {
                XmlWriter xtw = XmlWriter.Create(ms, new XmlWriterSettings { Encoding = this.Encoding });

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

                return ms.ToArray();
            }
        }

        private byte[] PreparePostRequest(HttpWebRequest request, object[] parameterValues)
        {
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded; charset=" + this.Encoding.WebName;

            string separator = string.Empty;
            using (var ms = new MemoryStream())
            {
                var sw = new StreamWriter(ms, this.Encoding);
                sw.Write(string.Empty);
                int i = 0;
                foreach (MethodCallParameter parameter in this.Parameters)
                {
                    sw.Write(separator);
                    sw.Write(parameter.Name);
                    sw.Write("=");
                    sw.Write(UrlHelper.UrlEncode(Convert.ToString(parameterValues[i], CultureInfo.InvariantCulture), true));
                    separator = "&";
                    i++;
                }

                sw.Flush();
                return ms.ToArray();
            }
        }
    }
}
