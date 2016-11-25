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
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Xml;
    using NLog.Common;
    using NLog.Internal;
    using Config;
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
        /// dictionary that maps a concrete <see cref="HttpPostFormatterBase"/> implementation
        /// to a specific <see cref="WebServiceProtocol"/>-value.
        /// </summary>
        private static Dictionary<WebServiceProtocol, Func<WebServiceTarget, HttpPostFormatterBase>> _postFormatterFactories =
            new Dictionary<WebServiceProtocol, Func<WebServiceTarget, HttpPostFormatterBase>>()
            {
                { WebServiceProtocol.Soap11, t => new HttpPostSoap11Formatter(t)},
                { WebServiceProtocol.Soap12, t => new HttpPostSoap12Formatter(t)},
                { WebServiceProtocol.HttpPost, t => new HttpPostFormEncodedFormatter(t)},
                { WebServiceProtocol.JsonPost, t => new HttpPostJsonFormatter(t)},
                { WebServiceProtocol.XmlPost, t => new HttpPostXmlDocumentFormatter(t)},
            };

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
        /// Gets or sets a value whether escaping be done according to Rfc3986 (Supports Internationalized Resource Identifiers - IRIs)
        /// </summary>
        /// <value>A value of <c>true</c> if Rfc3986; otherwise, <c>false</c> for legacy Rfc2396.</value>
        /// <docgen category='Web Service Options' order='10' />
        public bool EscapeDataRfc3986 { get; set; }

        /// <summary>
        /// Gets or sets a value whether escaping be done according to the old NLog style (Very non-standard)
        /// </summary>
        /// <value>A value of <c>true</c> if legacy encoding; otherwise, <c>false</c> for standard UTF8 encoding.</value>
        /// <docgen category='Web Service Options' order='10' />
        public bool EscapeDataNLogLegacy { get; set; }

        /// <summary>
        /// Gets or sets the name of the root XML element,
        /// if POST of XML document chosen.
        /// If so, this property must not be <c>null</c>.
        /// (see <see cref="Protocol"/> and <see cref="WebServiceProtocol.XmlPost"/>).
        /// </summary>
        /// <docgen category='Web Service Options' order='10' />
        public string XmlRoot { get; set; }

        /// <summary>
        /// Gets or sets the (optional) root namespace of the XML document,
        /// if POST of XML document chosen.
        /// (see <see cref="Protocol"/> and <see cref="WebServiceProtocol.XmlPost"/>).
        /// </summary>
        /// <docgen category='Web Service Options' order='10' />
        public string XmlRootNamespace { get; set; }


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

            if (Protocol == WebServiceProtocol.HttpGet)
            {
                PrepareGetRequest(request);
            }
            else
            {
                postPayload = _postFormatterFactories[Protocol](this).PrepareRequest(request, parameters);
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

            UrlHelper.EscapeEncodingFlag encodingFlags = UrlHelper.GetUriStringEncodingFlags(EscapeDataNLogLegacy, false, EscapeDataRfc3986);
            
            //if the protocol is HttpGet, we need to add the parameters to the query string of the url
            var queryParameters = new StringBuilder();
            string separator = string.Empty;
            for (int i = 0; i < this.Parameters.Count; i++)
            {
                queryParameters.Append(separator);
                queryParameters.Append(this.Parameters[i].Name);
                queryParameters.Append("=");
                string parameterValue = Convert.ToString(parameterValues[i], CultureInfo.InvariantCulture);
                UrlHelper.EscapeDataEncode(parameterValue, queryParameters, encodingFlags);
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

        private void PrepareGetRequest(HttpWebRequest request)
        {
            request.Method = "GET";
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

        /// <summary>
        /// base class for POST formatters, that
        /// implement former <c>PrepareRequest()</c> method,
        /// that creates the content for
        /// the requested kind of HTTP request
        /// </summary>
        private abstract class HttpPostFormatterBase
        {
            protected HttpPostFormatterBase(WebServiceTarget target)
            {
                Target = target;
            }

            protected abstract string ContentType { get; }
            protected WebServiceTarget Target { get; private set; }

            public MemoryStream PrepareRequest(HttpWebRequest request, object[] parameterValues)
            {
                InitRequest(request);

                var ms = new MemoryStream();
                WriteContent(ms, parameterValues);
                return ms;
            }

            protected virtual void InitRequest(HttpWebRequest request)
            {
                request.Method = "POST";
                request.ContentType = string.Format("{1}; charset={0}", Target.Encoding.WebName, ContentType);
            }

            protected abstract void WriteContent(MemoryStream ms, object[] parameterValues);
        }

        private class HttpPostFormEncodedFormatter : HttpPostTextFormatterBase
        {
            UrlHelper.EscapeEncodingFlag encodingFlags;

            public HttpPostFormEncodedFormatter(WebServiceTarget target) : base(target)
            {
                encodingFlags = UrlHelper.GetUriStringEncodingFlags(target.EscapeDataNLogLegacy, true, target.EscapeDataRfc3986);
            }

            protected override string ContentType
            {
                get { return "application/x-www-form-urlencoded"; }
            }

            protected override string Separator
            {
                get { return "&"; }
            }

            protected override string GetFormattedContent(string parametersContent)
            {
                return parametersContent;
            }

            protected override string GetFormattedParameter(MethodCallParameter parameter, object value)
            {
                string parameterValue = Convert.ToString(value, CultureInfo.InvariantCulture);
                if (string.IsNullOrEmpty(parameterValue))
                {
                    return string.Concat(parameter.Name, "=");
                }

                var sb = new StringBuilder(parameter.Name.Length + parameterValue.Length + 20);
                sb.Append(parameter.Name).Append("=");
                UrlHelper.EscapeDataEncode(parameterValue, sb, encodingFlags);
                return sb.ToString();
            }
        }

        private class HttpPostJsonFormatter : HttpPostTextFormatterBase
        {
            public HttpPostJsonFormatter(WebServiceTarget target) : base(target)
            { }

            protected override string ContentType
            {
                get { return "application/json"; }
            }

            protected override string Separator
            {
                get { return ","; }
            }

            protected override string GetFormattedContent(string parametersContent)
            {
                return string.Concat("{", parametersContent, "}");
            }

            protected override string GetFormattedParameter(MethodCallParameter parameter, object value)
            {
                return string.Format("\"{0}\":{1}",
                    parameter.Name,
                    GetJsonValueString(value));
            }

            private string GetJsonValueString(object value)
            {
                return ConfigurationItemFactory.Default.JsonSerializer.SerializeObject(value);
            }
        }

        private class HttpPostSoap11Formatter : HttpPostSoapFormatterBase
        {
            public HttpPostSoap11Formatter(WebServiceTarget target) : base(target)
            {
            }

            protected override string SoapEnvelopeNamespace
            {
                get { return WebServiceTarget.SoapEnvelopeNamespace; }
            }

            protected override string SoapName
            {
                get { return "soap"; }
            }

            protected override void InitRequest(HttpWebRequest request)
            {
                base.InitRequest(request);

                string soapAction;
                if (Target.Namespace.EndsWith("/", StringComparison.Ordinal))
                {
                    soapAction = string.Concat(Target.Namespace, Target.MethodName);
                }
                else
                {
                    soapAction = string.Concat(Target.Namespace, "/", Target.MethodName);
                }

                request.Headers["SOAPAction"] = soapAction;
            }
        }

        private class HttpPostSoap12Formatter : HttpPostSoapFormatterBase
        {
            public HttpPostSoap12Formatter(WebServiceTarget target) : base(target)
            {
            }

            protected override string SoapEnvelopeNamespace
            {
                get { return WebServiceTarget.Soap12EnvelopeNamespace; }
            }

            protected override string SoapName
            {
                get { return "soap12"; }
            }
        }

        private abstract class HttpPostSoapFormatterBase : HttpPostXmlFormatterBase
        {
            protected HttpPostSoapFormatterBase(WebServiceTarget target) : base(target)
            {
            }

            protected abstract string SoapEnvelopeNamespace { get; }
            protected abstract string SoapName { get; }

            protected override void WriteContent(MemoryStream ms, object[] parameterValues)
            {
                XmlWriter xtw = XmlWriter.Create(ms, new XmlWriterSettings { Encoding = Target.Encoding });

                xtw.WriteStartElement(SoapName, "Envelope", SoapEnvelopeNamespace);
                xtw.WriteStartElement("Body", SoapEnvelopeNamespace);
                xtw.WriteStartElement(Target.MethodName, Target.Namespace);

                WriteAllParametersToCurrenElement(xtw, parameterValues);

                xtw.WriteEndElement(); // methodname
                xtw.WriteEndElement(); // Body
                xtw.WriteEndElement(); // soap:Envelope
                xtw.Flush();
            }
        }

        private abstract class HttpPostTextFormatterBase : HttpPostFormatterBase
        {
            protected HttpPostTextFormatterBase(WebServiceTarget target) : base(target)
            {
            }

            protected abstract string Separator { get; }

            protected abstract string GetFormattedContent(string parametersContent);

            protected abstract string GetFormattedParameter(MethodCallParameter parameter, object value);

            protected override void WriteContent(MemoryStream ms, object[] parameterValues)
            {
                var sw = new StreamWriter(ms, Target.Encoding);
                sw.Write(string.Empty);

                var sb = new StringBuilder();
                for (int i = 0; i < Target.Parameters.Count; i++)
                {
                    if (sb.Length > 0) sb.Append(Separator);
                    sb.Append(GetFormattedParameter(Target.Parameters[i], parameterValues[i]));
                }
                string content = GetFormattedContent(sb.ToString());
                sw.Write(content);
                sw.Flush();
            }
        }

        private class HttpPostXmlDocumentFormatter : HttpPostXmlFormatterBase
        {

            protected override string ContentType
            {
                get { return "application/xml"; }
            }

            public HttpPostXmlDocumentFormatter(WebServiceTarget target) : base(target)
            {
                if (string.IsNullOrEmpty(target.XmlRoot))
                    throw new InvalidOperationException("WebServiceProtocol.Xml requires WebServiceTarget.XmlRoot to be set.");
            }

            protected override void WriteContent(MemoryStream ms, object[] parameterValues)
            {
                XmlWriter xtw = XmlWriter.Create(ms, new XmlWriterSettings { Encoding = Target.Encoding, OmitXmlDeclaration = true, Indent = false });

                xtw.WriteStartElement(Target.XmlRoot, Target.XmlRootNamespace);

                WriteAllParametersToCurrenElement(xtw, parameterValues);

                xtw.WriteEndElement();
                xtw.Flush();
            }
        }

        private abstract class HttpPostXmlFormatterBase : HttpPostFormatterBase
        {
            protected HttpPostXmlFormatterBase(WebServiceTarget target) : base(target)
            {
            }

            protected override string ContentType
            {
                get { return "text/xml"; }
            }

            protected void WriteAllParametersToCurrenElement(XmlWriter currentXmlWriter, object[] parameterValues)
            {
                int i = 0;
                foreach (MethodCallParameter par in Target.Parameters)
                {
                    currentXmlWriter.WriteElementString(par.Name, Convert.ToString(parameterValues[i], CultureInfo.InvariantCulture));
                    i++;
                }
            }
        }

    }
}
