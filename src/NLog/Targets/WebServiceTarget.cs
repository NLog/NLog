// 
// Copyright (c) 2004-2018 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Xml;
    using NLog.Common;
    using NLog.Config;
    using NLog.Internal;

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
        private const string SoapEnvelopeNamespaceUri = "http://schemas.xmlsoap.org/soap/envelope/";
        private const string Soap12EnvelopeNamespaceUri = "http://www.w3.org/2003/05/soap-envelope";

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
            Protocol = WebServiceProtocol.Soap11;

            //default NO utf-8 bom 
            const bool writeBOM = false;
            Encoding = new UTF8Encoding(writeBOM);
            IncludeBOM = writeBOM;
            OptimizeBufferReuse = true;

            Headers = new List<MethodCallParameter>();

#if NETSTANDARD
            // WebRequest.GetSystemWebProxy throws PlatformNotSupportedException on NetCore, when Proxy is not configured
            ProxyType = WebServiceProxyType.NoProxy;
#endif
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebServiceTarget" /> class.
        /// </summary>
        /// <param name="name">Name of the target</param>
        public WebServiceTarget(string name) : this()
        {
            Name = name;
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
        /// Gets or sets the Web service soap action name. Only used with Soap.
        /// </summary>
        /// <docgen category='Web Service Options' order='10' />
        public string SoapAction { get; set; }

        /// <summary>
        /// Gets or sets the protocol to be used when calling web service.
        /// </summary>
        /// <docgen category='Web Service Options' order='10' />
        [DefaultValue("Soap11")]
        public WebServiceProtocol Protocol
        {
            get => _activeProtocol.Key;
            set => _activeProtocol = new KeyValuePair<WebServiceProtocol, HttpPostFormatterBase>(value, null);
        }
        private KeyValuePair<WebServiceProtocol, HttpPostFormatterBase> _activeProtocol;

#if !SILVERLIGHT
        /// <summary>
        /// Gets or sets the proxy configuration when calling web service
        /// </summary>
        /// <docgen category='Web Service Options' order='10' />
        [DefaultValue("DefaultWebProxy")]
        public WebServiceProxyType ProxyType
        {
            get => _activeProxy.Key;
            set => _activeProxy = new KeyValuePair<WebServiceProxyType, IWebProxy>(value, null);
        }
        private KeyValuePair<WebServiceProxyType, IWebProxy> _activeProxy;
#endif

        /// <summary>
        /// Gets or sets the custom proxy address, include port separated by a colon
        /// </summary>
        /// <docgen category='Web Service Options' order='10' />
        public string ProxyAddress { get; set; }

        /// <summary>
        /// Should we include the BOM (Byte-order-mark) for UTF? Influences the <see cref="Encoding"/> property.
        /// 
        /// This will only work for UTF-8.
        /// </summary>
        /// <docgen category='Web Service Options' order='10' />
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
        /// <docgen category='Web Service Options' order='100' />
        public bool EscapeDataRfc3986 { get; set; }

        /// <summary>
        /// Gets or sets a value whether escaping be done according to the old NLog style (Very non-standard)
        /// </summary>
        /// <value>A value of <c>true</c> if legacy encoding; otherwise, <c>false</c> for standard UTF8 encoding.</value>
        /// <docgen category='Web Service Options' order='100' />
        public bool EscapeDataNLogLegacy { get; set; }

        /// <summary>
        /// Gets or sets the name of the root XML element,
        /// if POST of XML document chosen.
        /// If so, this property must not be <c>null</c>.
        /// (see <see cref="Protocol"/> and <see cref="WebServiceProtocol.XmlPost"/>).
        /// </summary>
        /// <docgen category='Web Service Options' order='100' />
        public string XmlRoot { get; set; }

        /// <summary>
        /// Gets or sets the (optional) root namespace of the XML document,
        /// if POST of XML document chosen.
        /// (see <see cref="Protocol"/> and <see cref="WebServiceProtocol.XmlPost"/>).
        /// </summary>
        /// <docgen category='Web Service Options' order='100' />
        public string XmlRootNamespace { get; set; }

        /// <summary>
        /// Gets the array of parameters to be passed.
        /// </summary>
        /// <docgen category='Web Service Options' order='10' />
        [ArrayParameter(typeof(MethodCallParameter), "header")]
        public IList<MethodCallParameter> Headers { get; private set; }

#if !SILVERLIGHT
        /// <summary>
        /// Indicates whether to pre-authenticate the HttpWebRequest (Requires 'Authorization' in <see cref="Headers"/> parameters)
        /// </summary>
        /// <docgen category='Web Service Options' order='100' />
        public bool PreAuthenticate { get; set; }
#endif

        private readonly AsyncOperationCounter _pendingManualFlushList = new AsyncOperationCounter();

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
        /// Calls the target DoInvoke method, and handles AsyncContinuation callback
        /// </summary>
        /// <param name="parameters">Method call parameters.</param>
        /// <param name="continuation">The continuation.</param>
        protected override void DoInvoke(object[] parameters, AsyncContinuation continuation)
        {
            var request = (HttpWebRequest)WebRequest.Create(BuildWebServiceUrl(parameters));
            DoInvoke(parameters, request, continuation);
        }

        /// <summary>
        /// Invokes the web service method.
        /// </summary>
        /// <param name="parameters">Parameters to be passed.</param>
        /// <param name="logEvent">The logging event.</param>
        protected override void DoInvoke(object[] parameters, AsyncLogEventInfo logEvent)
        {
            var request = (HttpWebRequest)WebRequest.Create(BuildWebServiceUrl(parameters));

            if (Headers != null && Headers.Count > 0)
            {
                for (int i = 0; i < Headers.Count; i++)
                {
                    string headerValue = RenderLogEvent(Headers[i].Layout, logEvent.LogEvent);
                    if (headerValue == null)
                        continue;

                    request.Headers[Headers[i].Name] = headerValue;
                }
            }

            DoInvoke(parameters, request, logEvent.Continuation);
        }

        void DoInvoke(object[] parameters, HttpWebRequest request, AsyncContinuation continuation)
        {
            Func<AsyncCallback, IAsyncResult> begin = (r) => request.BeginGetRequestStream(r, null);
            Func<IAsyncResult, Stream> getStream = request.EndGetRequestStream;

#if !SILVERLIGHT
            switch (ProxyType)
            {
                case WebServiceProxyType.DefaultWebProxy:
                    break;
#if !NETSTANDARD1_0
                case WebServiceProxyType.AutoProxy:
                    if (_activeProxy.Value == null)
                    {
                        IWebProxy proxy = WebRequest.GetSystemWebProxy();
                        proxy.Credentials = CredentialCache.DefaultCredentials;
                        _activeProxy = new KeyValuePair<WebServiceProxyType, IWebProxy>(ProxyType, proxy);
                    }
                    request.Proxy = _activeProxy.Value;
                    break;
                case WebServiceProxyType.ProxyAddress:
                    if (!string.IsNullOrEmpty(ProxyAddress))
                    {
                        if (_activeProxy.Value == null)
                        {
                            IWebProxy proxy = new WebProxy(ProxyAddress, true);
                            _activeProxy = new KeyValuePair<WebServiceProxyType, IWebProxy>(ProxyType, proxy);
                        }
                        request.Proxy = _activeProxy.Value;
                    }
                    break;
#endif
                default:
                    request.Proxy = null;
                    break;
            }
#endif

#if !SILVERLIGHT && !NETSTANDARD1_0
            if (PreAuthenticate || ProxyType == WebServiceProxyType.AutoProxy)
            {
                request.PreAuthenticate = true;
            }
#endif

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
                if (_activeProtocol.Value == null)
                    _activeProtocol = new KeyValuePair<WebServiceProtocol, HttpPostFormatterBase>(Protocol, _postFormatterFactories[Protocol](this));
                postPayload = _activeProtocol.Value.PrepareRequest(request, parameters);
            }

            var sendContinuation = CreateSendContinuation(continuation, request);

            PostPayload(continuation, beginFunc, getStreamFunc, postPayload, sendContinuation);
        }

        private AsyncContinuation CreateSendContinuation(AsyncContinuation continuation, HttpWebRequest request)
        {
            AsyncContinuation sendContinuation =
                ex =>
                {
                    if (ex != null)
                    {
                        DoInvokeCompleted(continuation, ex);
                        return;
                    }

                    try
                    {
                        request.BeginGetResponse(
                            r =>
                            {
                                try
                                {
                                    using (var response = request.EndGetResponse(r))
                                    {
                                    }

                                    DoInvokeCompleted(continuation, null);
                                }
                                catch (Exception ex2)
                                {
                                    InternalLogger.Error(ex2, "WebServiceTarget(Name={0}): Error sending request", Name);
                                    if (ex2.MustBeRethrownImmediately())
                                    {
                                        throw; // Throwing exceptions here will crash the entire application (.NET 2.0 behavior)
                                    }

                                    DoInvokeCompleted(continuation, ex2);
                                }
                            },
                            null);
                    }
                    catch (Exception ex2)
                    {
                        InternalLogger.Error(ex2, "WebServiceTarget(Name={0}): Error starting request", Name);
                        if (ex2.MustBeRethrown())
                        {
                            throw;
                        }

                        DoInvokeCompleted(continuation, ex2);
                    }
                };
            return sendContinuation;
        }

        private void PostPayload(AsyncContinuation continuation, Func<AsyncCallback, IAsyncResult> beginFunc, Func<IAsyncResult, Stream> getStreamFunc, Stream postPayload, AsyncContinuation sendContinuation)
        {
            if (postPayload != null && postPayload.Length > 0)
            {
                postPayload.Position = 0;
                try
                {
                    _pendingManualFlushList.BeginOperation();

                    beginFunc(
                        result =>
                        {
                            try
                            {
                                using (Stream stream = getStreamFunc(result))
                                {
                                    WriteStreamAndFixPreamble(postPayload, stream, IncludeBOM, Encoding);

                                    postPayload.Dispose();
                                }

                                sendContinuation(null);
                            }
                            catch (Exception ex)
                            {
                                InternalLogger.Error(ex, "WebServiceTarget(Name={0}): Error sending post data", Name);
                                if (ex.MustBeRethrownImmediately())
                                {
                                    throw; // Throwing exceptions here will crash the entire application (.NET 2.0 behavior)
                                }

                                postPayload.Dispose();
                                DoInvokeCompleted(continuation, ex);
                            }
                        });
                }
                catch (Exception ex)
                {
                    InternalLogger.Error(ex, "WebServiceTarget(Name={0}): Error starting post data", Name);
                    if (ex.MustBeRethrown())
                    {
                        throw;
                    }

                    DoInvokeCompleted(continuation, ex);
                }
            }
            else
            {
                _pendingManualFlushList.BeginOperation();
                sendContinuation(null);
            }
        }

        private void DoInvokeCompleted(AsyncContinuation continuation, Exception ex)
        {
            _pendingManualFlushList.CompleteOperation(ex);
            continuation(ex);
        }

        /// <summary>
        /// Flush any pending log messages asynchronously (in case of asynchronous targets).
        /// </summary>
        /// <param name="asyncContinuation">The asynchronous continuation.</param>
        protected override void FlushAsync(AsyncContinuation asyncContinuation)
        {
            _pendingManualFlushList.RegisterCompletionNotification(asyncContinuation).Invoke(null);
        }

        /// <summary>
        /// Closes the target.
        /// </summary>
        protected override void CloseTarget()
        {
            _pendingManualFlushList.Clear();   // Maybe consider to wait a short while if pending requests?
            base.CloseTarget();
        }

        /// <summary>
        /// Builds the URL to use when calling the web service for a message, depending on the WebServiceProtocol.
        /// </summary>
        /// <param name="parameterValues"></param>
        /// <returns></returns>
        private Uri BuildWebServiceUrl(object[] parameterValues)
        {
            if (Protocol != WebServiceProtocol.HttpGet)
            {
                return Url;
            }

            //if the protocol is HttpGet, we need to add the parameters to the query string of the url
            string queryParameters = string.Empty;
            using (var targetBuilder = OptimizeBufferReuse ? ReusableLayoutBuilder.Allocate() : ReusableLayoutBuilder.None)
            {
                StringBuilder sb = targetBuilder.Result ?? new StringBuilder();
                UrlHelper.EscapeEncodingFlag encodingFlags = UrlHelper.GetUriStringEncodingFlags(EscapeDataNLogLegacy, false, EscapeDataRfc3986);
                BuildWebServiceQueryParameters(parameterValues, sb, encodingFlags);
                queryParameters = sb.ToString();
            }

            var builder = new UriBuilder(Url);
            //append our query string to the URL following 
            //the recommendations at https://msdn.microsoft.com/en-us/library/system.uribuilder.query.aspx
            if (builder.Query != null && builder.Query.Length > 1)
            {
                builder.Query = string.Concat(builder.Query.Substring(1), "&", queryParameters);
            }
            else
            {
                builder.Query = queryParameters;
            }

            return builder.Uri;
        }

        private void BuildWebServiceQueryParameters(object[] parameterValues, StringBuilder sb, UrlHelper.EscapeEncodingFlag encodingFlags)
        {
            string separator = string.Empty;
            for (int i = 0; i < Parameters.Count; i++)
            {
                sb.Append(separator);
                sb.Append(Parameters[i].Name);
                sb.Append("=");
                string parameterValue = XmlHelper.XmlConvertToString(parameterValues[i]);
                if (!string.IsNullOrEmpty(parameterValue))
                {
                    UrlHelper.EscapeDataEncode(parameterValue, sb, encodingFlags);
                }
                separator = "&";
            }
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
                request.ContentType = string.Concat(ContentType, "; charset=", Target.Encoding.WebName);
            }

            protected abstract void WriteContent(MemoryStream ms, object[] parameterValues);
        }

        private class HttpPostFormEncodedFormatter : HttpPostTextFormatterBase
        {
            readonly UrlHelper.EscapeEncodingFlag _encodingFlags;

            public HttpPostFormEncodedFormatter(WebServiceTarget target) : base(target)
            {
                _encodingFlags = UrlHelper.GetUriStringEncodingFlags(target.EscapeDataNLogLegacy, true, target.EscapeDataRfc3986);
            }

            protected override string ContentType => "application/x-www-form-urlencoded";

            protected override void WriteStringContent(StringBuilder builder, object[] parameterValues)
            {
                Target.BuildWebServiceQueryParameters(parameterValues, builder, _encodingFlags);
            }
        }

        private class HttpPostJsonFormatter : HttpPostTextFormatterBase
        {
            private IJsonConverter JsonConverter => _jsonConverter ?? (_jsonConverter = ConfigurationItemFactory.Default.JsonConverter);
            private IJsonConverter _jsonConverter;

            public HttpPostJsonFormatter(WebServiceTarget target) : base(target)
            {
            }

            protected override string ContentType => "application/json";

            protected override void WriteStringContent(StringBuilder builder, object[] parameterValues)
            {
                if (Target.Parameters.Count == 1 && string.IsNullOrEmpty(Target.Parameters[0].Name) && parameterValues[0] is string s)
                {
                    // JsonPost with single nameless parameter means complex JsonLayout
                    builder.Append(s);
                }
                else
                {
                    builder.Append("{");
                    string separator = string.Empty;
                    for (int i = 0; i < Target.Parameters.Count; ++i)
                    {
                        var parameter = Target.Parameters[i];
                        builder.Append(separator);
                        builder.Append('"');
                        builder.Append(parameter.Name);
                        builder.Append("\":");
                        JsonConverter.SerializeObject(parameterValues[i], builder);
                        separator = ",";
                    }
                    builder.Append('}');
                }
            }
        }

        private class HttpPostSoap11Formatter : HttpPostSoapFormatterBase
        {
            public HttpPostSoap11Formatter(WebServiceTarget target) : base(target)
            {
            }

            protected override string SoapEnvelopeNamespace => SoapEnvelopeNamespaceUri;

            protected override string SoapName => "soap";

            protected override void InitRequest(HttpWebRequest request)
            {
                base.InitRequest(request);
                request.Headers["SOAPAction"] = GetSoapAction();
            }
        }

        private class HttpPostSoap12Formatter : HttpPostSoapFormatterBase
        {
            public HttpPostSoap12Formatter(WebServiceTarget target) : base(target)
            {
            }

            protected override string SoapEnvelopeNamespace => Soap12EnvelopeNamespaceUri;

            protected override string SoapName => "soap12";

            protected override string ContentType => "application/soap+xml";

            protected override void InitRequest(HttpWebRequest request)
            {
                base.InitRequest(request);
                request.ContentType += $@"; action=""{GetSoapAction()}""";
            }
        }

        private abstract class HttpPostSoapFormatterBase : HttpPostXmlFormatterBase
        {
            private readonly XmlWriterSettings _xmlWriterSettings;

            protected HttpPostSoapFormatterBase(WebServiceTarget target) : base(target)
            {
                _xmlWriterSettings = new XmlWriterSettings { Encoding = target.Encoding };
            }

            protected abstract string SoapEnvelopeNamespace { get; }
            protected abstract string SoapName { get; }

            protected override void WriteContent(MemoryStream ms, object[] parameterValues)
            {
                XmlWriter xtw = XmlWriter.Create(ms, _xmlWriterSettings);

                xtw.WriteStartElement(SoapName, "Envelope", SoapEnvelopeNamespace);
                xtw.WriteStartElement("Body", SoapEnvelopeNamespace);
                xtw.WriteStartElement(Target.MethodName, Target.Namespace);

                WriteAllParametersToCurrenElement(xtw, parameterValues);

                xtw.WriteEndElement(); // methodname
                xtw.WriteEndElement(); // Body
                xtw.WriteEndElement(); // soap:Envelope
                xtw.Flush();
            }

            protected string GetSoapAction()
            {
                string soapAction = Target.SoapAction;
                if (string.IsNullOrEmpty(soapAction))
                {
                    soapAction = Target.Namespace.EndsWith("/", StringComparison.Ordinal)
                        ? string.Concat(Target.Namespace, Target.MethodName)
                        : string.Concat(Target.Namespace, "/", Target.MethodName);
                }

                return soapAction;
            }
        }

        private abstract class HttpPostTextFormatterBase : HttpPostFormatterBase
        {
            readonly ReusableBuilderCreator _reusableStringBuilder = new ReusableBuilderCreator();
            readonly ReusableBufferCreator _reusableEncodingBuffer = new ReusableBufferCreator(1024);
            readonly byte[] _encodingPreamble;

            protected HttpPostTextFormatterBase(WebServiceTarget target) : base(target)
            {
                _encodingPreamble = target.Encoding.GetPreamble();
            }

            protected override void WriteContent(MemoryStream ms, object[] parameterValues)
            {
                lock (_reusableStringBuilder)
                {
                    using (var targetBuilder = _reusableStringBuilder.Allocate())
                    {
                        WriteStringContent(targetBuilder.Result, parameterValues);

                        using (var transformBuffer = _reusableEncodingBuffer.Allocate())
                        {
                            if (_encodingPreamble.Length > 0)
                                ms.Write(_encodingPreamble, 0, _encodingPreamble.Length);
                            targetBuilder.Result.CopyToStream(ms, Target.Encoding, transformBuffer.Result);
                        }
                    }
                }
            }

            protected abstract void WriteStringContent(StringBuilder builder, object[] parameterValues);
        }

        private class HttpPostXmlDocumentFormatter : HttpPostXmlFormatterBase
        {
            private readonly XmlWriterSettings _xmlWriterSettings;

            protected override string ContentType => "application/xml";

            public HttpPostXmlDocumentFormatter(WebServiceTarget target) : base(target)
            {
                if (string.IsNullOrEmpty(target.XmlRoot))
                    throw new InvalidOperationException("WebServiceProtocol.Xml requires WebServiceTarget.XmlRoot to be set.");

                _xmlWriterSettings = new XmlWriterSettings { Encoding = target.Encoding, OmitXmlDeclaration = true, Indent = false };
            }

            protected override void WriteContent(MemoryStream ms, object[] parameterValues)
            {
                XmlWriter xtw = XmlWriter.Create(ms, _xmlWriterSettings);

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

            protected override string ContentType => "text/xml";

            protected void WriteAllParametersToCurrenElement(XmlWriter currentXmlWriter, object[] parameterValues)
            {
                for (int i = 0; i < Target.Parameters.Count; i++)
                {
                    string parameterValue = XmlHelper.XmlConvertToStringSafe(parameterValues[i]);
                    currentXmlWriter.WriteElementString(Target.Parameters[i].Name, parameterValue);
                }
            }
        }
    }
}
