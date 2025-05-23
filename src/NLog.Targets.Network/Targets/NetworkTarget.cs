//
// Copyright (c) 2004-2024 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using System.Text;
    using System.Threading;
    using NLog.Common;
    using NLog.Layouts;
    using NLog.Internal.NetworkSenders;
    using System.Security.Cryptography.X509Certificates;

    /// <summary>
    /// NetworkTarget for sending messages over the network using TCP / UDP sockets
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/nlog/nlog/wiki/Network-target">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/nlog/nlog/wiki/Network-target">Documentation on NLog Wiki</seealso>
    /// <example>
    /// <p>
    /// To set up the target in the <a href="https://github.com/NLog/NLog/wiki/Configuration-file">configuration file</a>,
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" source="examples/targets/Configuration File/Network/NLog.config" />
    /// <p>
    /// To set up the log target programmatically use code like this:
    /// </p>
    /// <code lang="C#" source="examples/targets/Configuration API/Network/Simple/Example.cs" />
    /// <p>
    /// To print the results, use any application that's able to receive messages over
    /// TCP or UDP. <a href="http://m.nu/program/util/netcat/netcat.html">NetCat</a> is
    /// a simple but very powerful command-line tool that can be used for that. This image
    /// demonstrates the NetCat tool receiving log messages from Network target.
    /// </p>
    /// <img src="examples/targets/Screenshots/Network/Output.gif" />
    /// <p>
    /// There are two specialized versions of the Network target: <a href="T_NLog_Targets_ChainsawTarget.htm">Chainsaw</a>
    /// and <a href="T_NLog_Targets_NLogViewerTarget.htm">NLogViewer</a> which write to instances of Chainsaw log4j viewer
    /// or NLogViewer application respectively.
    /// </p>
    /// </example>
    [Target("Network")]
    public class NetworkTarget : TargetWithLayout
    {
        private readonly Dictionary<string, LinkedListNode<NetworkSender>> _currentSenderCache = new Dictionary<string, LinkedListNode<NetworkSender>>(StringComparer.Ordinal);
        private readonly LinkedList<NetworkSender> _openNetworkSenders = new LinkedList<NetworkSender>();

        private readonly char[] _reusableEncodingBuffer = new char[32 * 1024];
        private readonly StringBuilder _reusableStringBuilder = new StringBuilder();

        private readonly object _certificateCacheLock = new object();
        private Dictionary<string, X509Certificate2Collection>? _certificateCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkTarget" /> class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message:withexception=true}</code>
        /// </remarks>
        public NetworkTarget()
        {
            SenderFactory = NetworkSenderFactory.Default;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkTarget" /> class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message:withexception=true}</code>
        /// </remarks>
        /// <param name="name">Name of the target.</param>
        public NetworkTarget(string name) : this()
        {
            Name = name;
        }

        /// <summary>
        /// Gets or sets the network address.
        /// </summary>
        /// <remarks>
        /// The network address can be:
        /// <ul>
        /// <li>tcp://host:port - TCP (auto select IPv4/IPv6)</li>
        /// <li>tcp4://host:port - force TCP/IPv4</li>
        /// <li>tcp6://host:port - force TCP/IPv6</li>
        /// <li>udp://host:port - UDP (auto select IPv4/IPv6)</li>
        /// <li>udp4://host:port - force UDP/IPv4</li>
        /// <li>udp6://host:port - force UDP/IPv6</li>
        /// <li>http://host:port/pageName - HTTP using POST verb</li>
        /// <li>https://host:port/pageName - HTTPS using POST verb</li>
        /// </ul>
        /// For SOAP-based webservice support over HTTP use WebService target.
        /// </remarks>
        /// <docgen category='Connection Options' order='10' />
        public Layout Address { get; set; } = Layout.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether to keep connection open whenever possible.
        /// </summary>
        /// <docgen category='Connection Options' order='10' />
        public bool KeepConnection { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to append newline at the end of log message.
        /// </summary>
        /// <docgen category='Payload Options' order='10' />
        public bool NewLine { get; set; }

        /// <summary>
        /// Gets or sets the end of line value if a newline is appended at the end of log message <see cref="NewLine"/>.
        /// </summary>
        /// <docgen category='Payload Options' order='10' />
        public LineEndingMode LineEnding
        {
            get => _lineEnding;
            set
            {
                _lineEnding = value;
                NewLine = value?.NewLineCharacters?.Length > 0;
            }
        }
        private LineEndingMode _lineEnding = LineEndingMode.CRLF;

        /// <summary>
        /// Gets or sets the maximum message size in bytes. On limit breach then <see cref="OnOverflow"/> action is activated.
        /// </summary>
        /// <docgen category='Payload Options' order='10' />
        public int MaxMessageSize { get; set; } = 65000;

        /// <summary>
        /// Gets or sets the maximum simultaneous connections. Requires <see cref="KeepConnection"/> = false
        /// </summary>
        /// <remarks>
        /// When having reached the maximum limit, then <see cref="OnConnectionOverflow"/> action will apply.
        /// </remarks>
        /// <docgen category="Connection Options" order="10"/>
        public int MaxConnections { get; set; } = 100;

        /// <summary>
        /// Gets or sets the action that should be taken, when more connections than <see cref="MaxConnections"/>.
        /// </summary>
        /// <docgen category='Connection Options' order='10' />
        public NetworkTargetConnectionsOverflowAction OnConnectionOverflow { get; set; } = NetworkTargetConnectionsOverflowAction.Discard;

        /// <summary>
        /// Gets or sets the maximum queue size for a single connection. Requires <see cref="KeepConnection"/> = true
        /// </summary>
        /// <remarks>
        /// When having reached the maximum limit, then <see cref="OnQueueOverflow"/> action will apply.
        /// </remarks>
        /// <docgen category='Connection Options' order='10' />
        public int MaxQueueSize { get; set; } = 10000;

        /// <summary>
        /// Gets or sets the action that should be taken, when more pending messages than <see cref="MaxQueueSize"/>.
        /// </summary>
        /// <docgen category='Connection Options' order='10' />
        public NetworkTargetQueueOverflowAction OnQueueOverflow { get; set; } = NetworkTargetQueueOverflowAction.Discard;

        /// <summary>
        /// Occurs when LogEvent has been dropped.
        /// </summary>
        /// <remarks>
        ///  - When internal queue is full and <see cref="OnQueueOverflow"/> set to <see cref="NetworkTargetOverflowAction.Discard"/><br/>
        ///  - When connection-list is full and <see cref="OnConnectionOverflow"/> set to <see cref="NetworkTargetConnectionsOverflowAction.Discard"/><br/>
        ///  - When message is too big and <see cref="OnOverflow"/> set to <see cref="NetworkTargetOverflowAction.Discard"/><br/>
        /// </remarks>
        public event EventHandler<NetworkLogEventDroppedEventArgs>? LogEventDropped;

        /// <summary>
        /// Gets or sets the size of the connection cache (number of connections which are kept alive). Requires <see cref="KeepConnection"/> = true
        /// </summary>
        /// <docgen category="Connection Options" order="10"/>
        public int ConnectionCacheSize { get; set; } = 5;

        /// <summary>
        /// Gets or sets the action that should be taken if the message is larger than <see cref="MaxMessageSize" />
        /// </summary>
        /// <remarks>
        /// For TCP sockets then <see cref="NetworkTargetOverflowAction.Split"/> means no-limit, as TCP sockets
        /// performs splitting automatically.
        ///
        /// For UDP Network sender then <see cref="NetworkTargetOverflowAction.Split"/> means splitting the message
        /// into smaller chunks. This can be useful on networks using DontFragment, which drops network packages
        /// larger than MTU-size (1472 bytes).
        /// </remarks>
        /// <docgen category='Connection Options' order='10' />
        public NetworkTargetOverflowAction OnOverflow { get; set; } = NetworkTargetOverflowAction.Split;

        /// <summary>
        /// Gets or sets the encoding to be used.
        /// </summary>
        /// <docgen category='Payload Options' order='10' />
        public Encoding Encoding { get; set; } = Encoding.UTF8;

        /// <summary>
        /// Gets or sets the SSL/TLS protocols. Default no SSL/TLS is used. Currently only implemented for TCP.
        /// </summary>
        /// <docgen category='Connection Options' order='10' />
        public System.Security.Authentication.SslProtocols SslProtocols { get; set; } = System.Security.Authentication.SslProtocols.None;

        /// <summary>
        /// Gets or sets the file path to custom SSL certificate for TCP Socket SSL connections
        /// </summary>
        /// <docgen category='Connection Options' order='16' />
        public Layout? SslCertificateFile { get; set; }

        /// <summary>
        /// Gets or sets the password for the custom SSL certificate specified by <see cref="SslCertificateFile"/>
        /// </summary>
        /// <docgen category='Connection Options' order='16' />
        public Layout? SslCertificatePassword { get; set; }

        /// <summary>
        /// The number of seconds a connection will remain idle before the first keep-alive probe is sent
        /// </summary>
        /// <docgen category='Connection Options' order='10' />
        public int KeepAliveTimeSeconds { get; set; }

        /// <summary>
        /// The number of seconds a TCP socket send-operation will block before timeout error. Default = 100 secs (0 = wait forever when network cable unplugged and tcp-buffer becomes full).
        /// </summary>
        /// <docgen category='Connection Options' order='10' />
        public int SendTimeoutSeconds { get; set; } = 100;

        /// <summary>
        /// Gets or sets whether to disable the delayed ACK timer, and avoid delay of 200 ms. Default = true.
        /// </summary>
        public bool NoDelay { get; set; } = true;

        /// <summary>
        /// Type of compression for protocol payload. Useful for UDP where datagram max-size is 8192 bytes.
        /// </summary>
        public NetworkTargetCompressionType Compress { get; set; }

        /// <summary>
        /// Skip compression when protocol payload is below limit to reduce overhead in cpu-usage and additional headers
        /// </summary>
        public int CompressMinBytes { get; set; }

        internal INetworkSenderFactory SenderFactory { get; set; }

        /// <inheritdoc />
        protected override void InitializeTarget()
        {
            base.InitializeTarget();

            if (Address is null || ReferenceEquals(Address, Layout.Empty))
                throw new NLogConfigurationException($"{GetType()} Address-property must be assigned. Address is needed for network destination.");
        }

        /// <summary>
        /// Flush any pending log messages asynchronously (in case of asynchronous targets).
        /// </summary>
        /// <param name="asyncContinuation">The asynchronous continuation.</param>
        protected override void FlushAsync(AsyncContinuation asyncContinuation)
        {
            int remainingCount;

            void Continuation(Exception? ex)
            {
                // ignore exception
                if (Interlocked.Decrement(ref remainingCount) == 0)
                {
                    asyncContinuation(null);
                }
            }

            lock (_openNetworkSenders)
            {
                remainingCount = _openNetworkSenders.Count;
                if (remainingCount == 0)
                {
                    // nothing to flush
                    asyncContinuation(null);
                }
                else
                {
                    // otherwise call FlushAsync() on all senders
                    // and invoke continuation at the very end
                    foreach (var openSender in _openNetworkSenders)
                    {
                        openSender.FlushAsync(Continuation);
                    }
                }
            }
        }

        /// <inheritdoc/>
        protected override void CloseTarget()
        {
            base.CloseTarget();

            lock (_currentSenderCache)
            {
                lock (_openNetworkSenders)
                {
                    foreach (var openSender in _openNetworkSenders)
                    {
                        openSender.Close(ex => { });
                    }

                    _openNetworkSenders.Clear();
                }

                _currentSenderCache.Clear();
            }

            if (_certificateCache?.Count > 0)
            {
                // Safe to reset without lock, since immutable collection
                _certificateCache = null;
            }
        }

        /// <summary>
        /// Sends the
        /// rendered logging event over the network optionally concatenating it with a newline character.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        protected override void Write(AsyncLogEventInfo logEvent)
        {
            string address = RenderLogEvent(Address, logEvent.LogEvent);
            byte[] payload = GetBytesToWrite(logEvent.LogEvent);
            byte[]? header = GetHeaderToWrite(logEvent.LogEvent, address, payload);
            int messageSize = payload.Length;

            InternalLogger.Trace("{0}: Sending {1} bytes to address: '{2}'", this, messageSize, address);

            if (messageSize > MaxMessageSize)
            {
                if (OnOverflow == NetworkTargetOverflowAction.Discard)
                {
                    InternalLogger.Debug("{0}: Discarded LogEvent because MessageSize={1} is above MaxMessageSize={2}", this, messageSize, MaxMessageSize);
                    OnLogEventDropped(this, NetworkLogEventDroppedEventArgs.MaxMessageSizeOverflow);
                    logEvent.Continuation(null);
                    return;
                }

                if (OnOverflow == NetworkTargetOverflowAction.Error)
                {
                    InternalLogger.Debug("{0}: Discarded LogEvent because MessageSize={1} is above MaxMessageSize={2}", this, messageSize, MaxMessageSize);
                    OnLogEventDropped(this, NetworkLogEventDroppedEventArgs.MaxMessageSizeOverflow);
                    logEvent.Continuation(new InvalidOperationException($"NetworkTarget: Discarded LogEvent because MessageSize={messageSize} is above MaxMessageSize={MaxMessageSize}"));
                    return;
                }
            }

            if (messageSize <= 0)
            {
                logEvent.Continuation(null);
                return;
            }

            if (KeepConnection)
            {
                WriteBytesToCachedNetworkSender(address, header, payload, logEvent);
            }
            else
            {
                WriteBytesToNewNetworkSender(address, header, payload, logEvent);
            }
        }

        private void WriteBytesToCachedNetworkSender(string address, byte[]? header, byte[] payload, AsyncLogEventInfo logEvent)
        {
            LinkedListNode<NetworkSender> senderNode;
            try
            {
                senderNode = GetCachedNetworkSender(address, logEvent.LogEvent);
            }
            catch (Exception ex)
            {
                InternalLogger.Error(ex, "{0}: Failed to create sender to address: '{1}'", this, address);
                OnLogEventDropped(this, NetworkLogEventDroppedEventArgs.NetworkErrorDetected);
                throw;
            }

            WriteBytesToNetworkSender(
                senderNode.Value,
                payload,
                header,
                ex =>
                {
                    if (ex != null)
                    {
                        InternalLogger.Error(ex, "{0}: Error when sending.", this);
                        OnLogEventDropped(this, NetworkLogEventDroppedEventArgs.NetworkErrorDetected);
                        ReleaseCachedConnection(senderNode);
                    }

                    logEvent.Continuation(ex);
                });
        }

        private void WriteBytesToNewNetworkSender(string address, byte[]? header, byte[] payload, AsyncLogEventInfo logEvent)
        {
            NetworkSender sender;
            LinkedListNode<NetworkSender> linkedListNode;

            lock (_openNetworkSenders)
            {
                //handle too many connections
                var tooManyConnections = _openNetworkSenders.Count >= MaxConnections;

                if (tooManyConnections && MaxConnections > 0)
                {
                    switch (OnConnectionOverflow)
                    {
                        case NetworkTargetConnectionsOverflowAction.Discard:
                            InternalLogger.Debug("{0}: Discarding message, because too many open connections.", this);
                            OnLogEventDropped(this, NetworkLogEventDroppedEventArgs.MaxConnectionsOverflow);
                            logEvent.Continuation(null);
                            return;

                        case NetworkTargetConnectionsOverflowAction.Grow:
                            MaxConnections = MaxConnections * 2;
                            InternalLogger.Debug("{0}: Growing max connections limit, because many open connections.", this);
                            break;

                        case NetworkTargetConnectionsOverflowAction.Block:
                            while (_openNetworkSenders.Count >= MaxConnections)
                            {
                                InternalLogger.Debug("{0}: Blocking until ready, because too many open connections.", this);
                                Monitor.Wait(_openNetworkSenders);
                                InternalLogger.Trace("{0}: Entered critical section.", this);
                            }

                            InternalLogger.Trace("{0}: Limit ok.", this);
                            break;
                    }
                }

                try
                {
                    sender = CreateNetworkSender(address, logEvent.LogEvent);
                }
                catch (Exception ex)
                {
                    InternalLogger.Error(ex, "{0}: Failed to create sender to address: '{1}'", this, address);
                    OnLogEventDropped(this, NetworkLogEventDroppedEventArgs.NetworkErrorDetected);
                    throw;
                }

                linkedListNode = _openNetworkSenders.AddLast(sender);
            }

            WriteBytesToNetworkSender(
                sender,
                payload,
                header,
                ex =>
                {
                    lock (_openNetworkSenders)
                    {
                        TryRemove(_openNetworkSenders, linkedListNode);
                        if (OnConnectionOverflow == NetworkTargetConnectionsOverflowAction.Block)
                        {
                            Monitor.PulseAll(_openNetworkSenders);
                        }
                    }

                    if (ex != null)
                    {
                        InternalLogger.Error(ex, "{0}: Error when sending.", this);
                        OnLogEventDropped(this, NetworkLogEventDroppedEventArgs.NetworkErrorDetected);
                    }

                    sender.Close(ex2 => { });
                    logEvent.Continuation(ex);
                });
        }

        private void OnLogEventDropped(object sender, NetworkLogEventDroppedEventArgs logEventDroppedEventArgs)
        {
            LogEventDropped?.Invoke(this, logEventDroppedEventArgs);
        }

        /// <summary>
        /// Try to remove.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="node"></param>
        /// <returns>removed something?</returns>
        private static bool TryRemove<T>(LinkedList<T> list, LinkedListNode<T> node)
        {
            if (node is null || list != node.List)
            {
                return false;
            }
            list.Remove(node);
            return true;
        }

        /// <summary>
        /// Gets the payload bytes to be written.
        /// </summary>
        /// <param name="logEvent">Log event.</param>
        /// <returns>Byte array.</returns>
        protected virtual byte[] GetBytesToWrite(LogEventInfo logEvent)
        {
            var payload = RenderBytesToWrite(logEvent);

            if (Compress != NetworkTargetCompressionType.None)
            {
                payload = CompressBytesToWrite(payload);
            }

            return payload;
        }

        /// <summary>
        /// Gets the header bytes to be written.
        /// </summary>
        /// <param name="logEvent">Log event.</param>
        /// <param name="address">network address.</param>
        /// <param name="payload">Payload buffer.</param>
        /// <returns>Byte array.</returns>
        protected virtual byte[]? GetHeaderToWrite(LogEventInfo logEvent, string address, byte[] payload)
        {
            return null;
        }

        private byte[] RenderBytesToWrite(LogEventInfo logEvent)
        {
            lock (_reusableEncodingBuffer)
            {
                try
                {
                    _reusableStringBuilder.Length = 0;
                    Layout.Render(logEvent, _reusableStringBuilder);
                    if (NewLine)
                    {
                        _reusableStringBuilder.Append(LineEnding.NewLineCharacters);
                    }

                    return GetBytesFromStringBuilder(_reusableEncodingBuffer, _reusableStringBuilder);
                }
                finally
                {
                    _reusableStringBuilder.Length = 0;
                }
            }
        }

        private byte[] GetBytesFromStringBuilder(char[] charBuffer, StringBuilder stringBuilder)
        {
            if (stringBuilder.Length <= charBuffer.Length)
            {
                stringBuilder.CopyTo(0, charBuffer, 0, stringBuilder.Length);
                return Encoding.GetBytes(charBuffer, 0, stringBuilder.Length);
            }
            return Encoding.GetBytes(stringBuilder.ToString());
        }

        private byte[] CompressBytesToWrite(byte[] payload)
        {
            if (payload.Length > CompressMinBytes)
            {
                using (var outputStream = new System.IO.MemoryStream(Math.Max(payload.Length / 10, 256)))
                {
#if !NET35
                    using (var gzipStream = new System.IO.Compression.GZipStream(outputStream, Compress == NetworkTargetCompressionType.GZip ? System.IO.Compression.CompressionLevel.Optimal : System.IO.Compression.CompressionLevel.Fastest, true))
#else
                    using (var gzipStream = new System.IO.Compression.GZipStream(outputStream, System.IO.Compression.CompressionMode.Compress, true))
#endif
                    {
                        gzipStream.Write(payload, 0, payload.Length);
                    }
                    payload = outputStream.ToArray();
                }
            }

            return payload;
        }

        private LinkedListNode<NetworkSender> GetCachedNetworkSender(string address, LogEventInfo logEventInfo)
        {
            lock (_currentSenderCache)
            {
                // already have address
                if (_currentSenderCache.TryGetValue(address, out var senderNode))
                {
                    senderNode.Value.CheckSocket();
                    return senderNode;
                }

                if (_currentSenderCache.Count >= ConnectionCacheSize)
                {
                    // make room in the cache by closing the least recently used connection
                    int minAccessTime = int.MaxValue;
                    LinkedListNode<NetworkSender>? leastRecentlyUsed = null;

                    foreach (var pair in _currentSenderCache)
                    {
                        var networkSender = pair.Value.Value;
                        if (networkSender.LastSendTime < minAccessTime)
                        {
                            minAccessTime = networkSender.LastSendTime;
                            leastRecentlyUsed = pair.Value;
                        }
                    }

                    if (leastRecentlyUsed != null)
                    {
                        ReleaseCachedConnection(leastRecentlyUsed);
                    }
                }

                InternalLogger.Debug("{0}: Creating network sender to address: {1}", this, address);
                NetworkSender sender = CreateNetworkSender(address, logEventInfo);
                lock (_openNetworkSenders)
                {
                    senderNode = _openNetworkSenders.AddLast(sender);
                }

                _currentSenderCache.Add(address, senderNode);
                return senderNode;
            }
        }

        private NetworkSender CreateNetworkSender(string address, LogEventInfo logEventInfo)
        {
            var sslCertificateFile = SslCertificateFile?.Render(logEventInfo) ?? string.Empty;
            var sslCertificatePassword = SslCertificatePassword?.Render(logEventInfo) ?? string.Empty;
            var sslCertificateOverride = LoadSslCertificateFromFile(sslCertificateFile, sslCertificatePassword);

            var sender = SenderFactory.Create(address, sslCertificateOverride, this);
            sender.Initialize();
            if (KeepConnection || LogEventDropped != null)
            {
                sender.LogEventDropped += OnLogEventDropped;
            }
            return sender;
        }

        private void ReleaseCachedConnection(LinkedListNode<NetworkSender> senderNode)
        {
            lock (_currentSenderCache)
            {
                var networkSender = senderNode.Value;
                lock (_openNetworkSenders)
                {
                    if (TryRemove(_openNetworkSenders, senderNode))
                    {
                        // only remove it once
                        networkSender.Close(ex => { });
                    }
                }

                // make sure the current sender for this address is the one we want to remove
                if (_currentSenderCache.TryGetValue(networkSender.Address, out var sender2) && ReferenceEquals(senderNode, sender2))
                {
                    _currentSenderCache.Remove(networkSender.Address);
                }
            }
        }

        private static void WriteBytesToNetworkSender(NetworkSender sender, byte[] payload, byte[]? header, AsyncContinuation continuation)
        {
            if (header?.Length > 0)
                sender.Send(header, 0, header.Length, continuation);
            sender.Send(payload, 0, payload.Length, continuation);
        }

        internal X509Certificate2Collection? LoadSslCertificateFromFile(string sslCertificateFile, string sslCertificatePassword)
        {
            if (string.IsNullOrEmpty(sslCertificateFile))
                return null;    // NOSONAR

            if (_certificateCache != null && _certificateCache.TryGetValue(sslCertificateFile, out var clientCertificates))
                return clientCertificates;  // Safe to lookup without lock, since immutable collection

            try
            {
                lock (_certificateCacheLock)
                {
                    if (_certificateCache?.TryGetValue(sslCertificateFile, out clientCertificates) == true)
                        return clientCertificates;

                    if (string.IsNullOrEmpty(sslCertificateFile))
                    {
                        clientCertificates = new X509Certificate2Collection();
                    }
                    else if (sslCertificateFile.EndsWith(".pem", StringComparison.OrdinalIgnoreCase))
                    {
                        InternalLogger.Debug("{0}: Loading SSL certificate from PEM-file: {1}", this, sslCertificateFile);
                        var clientCertificate = LoadCertificateFromPem(sslCertificateFile);
                        clientCertificates = new X509Certificate2Collection(clientCertificate);
                    }
                    else
                    {
                        InternalLogger.Debug("{0}: Loading SSL certificate from file: {1}", this, sslCertificateFile);
                        clientCertificates = new X509Certificate2Collection(new X509Certificate2(sslCertificateFile, string.IsNullOrEmpty(sslCertificatePassword) ? null : sslCertificatePassword));
                    }

                    var certificateCache = new Dictionary<string, X509Certificate2Collection>((_certificateCache?.Count ?? 0) + 1);
                    if (_certificateCache != null)
                    {
                        foreach (var existingCertificate in _certificateCache)
                            certificateCache.Add(existingCertificate.Key, existingCertificate.Value);
                    }
                    certificateCache[sslCertificateFile] = clientCertificates;
                    _certificateCache = certificateCache;
                    return clientCertificates;
                }
            }
            catch (Exception ex)
            {
                InternalLogger.Error(ex, "{0}: Failed loading SSL certificate from file: {1}", this, sslCertificateFile);
                throw new NLogRuntimeException($"NetworkTarget: Failed loading SSL certificate from file: {sslCertificateFile}", ex);
            }
        }

        private static X509Certificate2 LoadCertificateFromPem(string fileName)
        {
            using (var reader = new System.IO.StreamReader(new System.IO.FileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read), Encoding.UTF8))
            {
                var pem = reader.ReadToEnd();
                string base64 = GetBase64FromPem(pem);
                byte[] certBytes = Convert.FromBase64String(base64);
                return new X509Certificate2(certBytes);
            }
        }

        private static string GetBase64FromPem(string pem)
        {
            const string header = "-----BEGIN CERTIFICATE-----";
            const string footer = "-----END CERTIFICATE-----";

            int start = pem.IndexOf(header, StringComparison.Ordinal) + header.Length;
            if (start <= header.Length)
            {
                throw new NLogRuntimeException("Invalid PEM format: Missing BEGIN CERTIFICATE header");
            }

            int end = pem.IndexOf(footer, start, StringComparison.Ordinal);
            if (end <= start)
            {
                throw new NLogRuntimeException("Invalid PEM format: Missing END CERTIFICATE footer");
            }
            string base64 = pem.Substring(start, end - start);
            base64 = base64.Replace("\r", "").Replace("\n", "").Trim();
            return base64;
        }
    }
}
