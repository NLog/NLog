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

using System.Linq;

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
    /// <seealso href="https://github.com/nlog/nlog/wiki/WebService-target">Documentation on NLog Wiki</seealso>
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

            //default NO utf-8 bom 
            const bool writeBOM = false;
            this.Encoding = new UTF8Encoding(writeBOM);
            this.IncludeBOM = writeBOM;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebServiceTarget" /> class.
        /// </summary>
        /// <param name="name">Name of the target</param>
        public WebServiceTarget(string name) : this()
        {
            this.Name = name;
        }

        /// <summary>
        /// Gets or sets the web service URL.
        /// </summary>
        /// <docgen category='Web Service Options' order='10' />
        public Uri Url { get; set; }

        /// <summary>
        /// Gets or sets the Web service method name. Only used with Soap.
        /// </summary>
        /// <docgen category='Web Service Options' order='10' />
        public string MethodName { get; set; }

        /// <summary>
        /// Gets or sets the Web service namespace. Only used with Soap.
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
        /// Should we include the BOM (Byte-order-mark) for UTF? Influences the <see cref="Encoding"/> property.
        /// 
        /// This will only work for UTF-8.
        /// </summary>
        public bool? IncludeBOM { get; set; }

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
            var request = (HttpWebRequest)WebRequest.Create(BuildWebServiceUrl(parameters));
            Func<AsyncCallback, IAsyncResult> begin = (r) => request.BeginGetRequestStream(r, null);
            Func<IAsyncResult, Stream> getStream = request.EndGetRequestStream;

            DoInvoke(parameters, continuation, request, begin, getStream);
        }

        internal void DoInvoke(object[] parameters, AsyncContinuation continuation, HttpWebRequest request, Func<AsyncCallback, IAsyncResult> beginFunc, 
            Func<IAsyncResult, Stream> getStreamFunc)
        {
            Stream postPayload = null;

            switch (this.Protocol)
            {
                case WebServiceProtocol.Soap11:
                    postPayload = this.PrepareSoap11Request(request, parameters);
                    break;

                case WebServiceProtocol.Soap12:
                    postPayload = this.PrepareSoap12Request(request, parameters);
                    break;

                case WebServiceProtocol.HttpGet:
                    this.PrepareGetRequest(request);
                    break;

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
                                InternalLogger.Error(ex2, "Error when sending to Webservice.");

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
                postPayload.Position = 0;
                beginFunc(
                    result =>
                    {
                        try
                        {
                            using (Stream stream = getStreamFunc(result))
                            {
                                WriteStreamAndFixPreamble(postPayload, stream, this.IncludeBOM, this.Encoding);

                                postPayload.Dispose();
                            }

                            sendContinuation(null);
                        }
                        catch (Exception ex)
                        {
                            postPayload.Dispose();
                            InternalLogger.Error(ex, "Error when sending to Webservice.");

                            if (ex.MustBeRethrown())
                            {
                                throw;
                            }

                            continuation(ex);
                        }
                    });
            }
            else
            {
                sendContinuation(null);
            }
        }

        /// <summary>
        /// Builds the URL to use when calling the web service for a message, depending on the WebServiceProtocol.
        /// </summary>
        /// <param name="parameterValues"></param>
        /// <returns></returns>
        private Uri BuildWebServiceUrl(object[] parameterValues)
        {
            if (this.Protocol != WebServiceProtocol.HttpGet)
            {
                return this.Url;
            }
            
            //if the protocol is HttpGet, we need to add the parameters to the query string of the url
            var queryParameters = new StringBuilder();
            string separator = string.Empty;
            for (int i = 0; i < this.Parameters.Count; i++)
            {
                queryParameters.Append(separator);
                queryParameters.Append(this.Parameters[i].Name);
                queryParameters.Append("=");
                queryParameters.Append(UrlHelper.UrlEncode(Convert.ToString(parameterValues[i], CultureInfo.InvariantCulture), false));
                separator = "&";
            }

            var builder = new UriBuilder(this.Url);
            //append our query string to the URL following 
            //the recommendations at https://msdn.microsoft.com/en-us/library/system.uribuilder.query.aspx
            if (builder.Query != null && builder.Query.Length > 1)
            {
                builder.Query = builder.Query.Substring(1) + "&" + queryParameters.ToString();
            }
            else
            {
                builder.Query = queryParameters.ToString();
            }

            return builder.Uri;
        }

        private MemoryStream PrepareSoap11Request(HttpWebRequest request, object[] parameterValues)
        {
            string soapAction;
            if (this.Namespace.EndsWith("/", StringComparison.Ordinal))
            {
                soapAction = this.Namespace + this.MethodName;
            }
            else
            {
                soapAction = this.Namespace + "/" + this.MethodName;
            }
            request.Headers["SOAPAction"] = soapAction;

            return PrepareSoapRequestPost(request, parameterValues, SoapEnvelopeNamespace, "soap");

        }

        private MemoryStream PrepareSoap12Request(HttpWebRequest request, object[] parameterValues)
        {
            return PrepareSoapRequestPost(request, parameterValues, Soap12EnvelopeNamespace, "soap12");
        }

        /// <summary>
        /// Helper for creating soap POST-XML request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="parameterValues"></param>
        /// <param name="soapEnvelopeNamespace"></param>
        /// <param name="soapname"></param>
        /// <returns></returns>
        private MemoryStream PrepareSoapRequestPost(WebRequest request, object[] parameterValues, string soapEnvelopeNamespace, string soapname)
        {
            request.Method = "POST";
            request.ContentType = "text/xml; charset=" + this.Encoding.WebName;

            var ms = new MemoryStream();
            XmlWriter xtw = XmlWriter.Create(ms, new XmlWriterSettings { Encoding = this.Encoding });

            xtw.WriteStartElement(soapname, "Envelope", soapEnvelopeNamespace);
            xtw.WriteStartElement("Body", soapEnvelopeNamespace);
            xtw.WriteStartElement(this.MethodName, this.Namespace);
            int i = 0;
            foreach (MethodCallParameter par in this.Parameters)
            {
                xtw.WriteElementString(par.Name, Convert.ToString(parameterValues[i], CultureInfo.InvariantCulture));
                i++;
            }

            xtw.WriteEndElement(); // methodname
            xtw.WriteEndElement(); // Body
            xtw.WriteEndElement(); // soap:Envelope
            xtw.Flush();

            return ms;
        }

        private MemoryStream PreparePostRequest(HttpWebRequest request, object[] parameterValues)
        {
            request.Method = "POST";
            return PrepareHttpRequest(request, parameterValues);
        }

        private void PrepareGetRequest(HttpWebRequest request)
        {
            request.Method = "GET";
        }

        private MemoryStream PrepareHttpRequest(HttpWebRequest request, object[] parameterValues)
        {
            request.ContentType = "application/x-www-form-urlencoded; charset=" + this.Encoding.WebName;

            var ms = new MemoryStream();
            string separator = string.Empty;
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
            return ms;
        }


        /// <summary>
        /// Write from input to output. Fix the UTF-8 bom
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="writeUtf8BOM"></param>
        /// <param name="encoding"></param>
        private static void WriteStreamAndFixPreamble(Stream input, Stream output, bool? writeUtf8BOM, Encoding encoding)
        {
            //only when utf-8 encoding is used, the Encoding preamble is optional
            var nothingToDo = writeUtf8BOM == null || !(encoding is UTF8Encoding);

            const int preambleSize = 3;
            if (!nothingToDo)
            {
                //it's UTF-8
                var hasBomInEncoding = encoding.GetPreamble().Length == preambleSize;

                //BOM already in Encoding.
                nothingToDo = writeUtf8BOM.Value && hasBomInEncoding;

                //Bom already not in Encoding
                nothingToDo = nothingToDo || !writeUtf8BOM.Value && !hasBomInEncoding;
            }
            var offset = nothingToDo ? 0 : preambleSize;
            input.CopyWithOffset(output, offset);

        }


    }
}
