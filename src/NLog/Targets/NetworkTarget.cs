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
    using System.Text;
    using System.Threading;
    using NLog.Common;
    using NLog.Internal;
    using NLog.Internal.NetworkSenders;
    using NLog.Layouts;

    /// <summary>
    /// Sends log messages over the network.
    /// </summary>
    /// <seealso href="https://github.com/nlog/nlog/wiki/Network-target">Documentation on NLog Wiki</seealso>
    /// <example>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" source="examples/targets/Configuration File/Network/NLog.config" />
    /// <p>
    /// This assumes just one target and a single rule. More configuration
    /// options are described <a href="config.html">here</a>.
    /// </p>
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
    /// NOTE: If your receiver application is ever likely to be off-line, don't use TCP protocol
    /// or you'll get TCP timeouts and your application will be very slow. 
    /// Either switch to UDP transport or use <a href="target.AsyncWrapper.html">AsyncWrapper</a> target
    /// so that your application threads will not be blocked by the timing-out connection attempts.
    /// </p>
    /// <p>
    /// There are two specialized versions of the Network target: <a href="target.Chainsaw.html">Chainsaw</a>
    /// and <a href="target.NLogViewer.html">NLogViewer</a> which write to instances of Chainsaw log4j viewer
    /// or NLogViewer application respectively.
    /// </p>
    /// </example>
    [Target("Network")]
    public class NetworkTarget : TargetWithLayout
    {
        private readonly Dictionary<string, LinkedListNode<NetworkSender>> _currentSenderCache = new Dictionary<string, LinkedListNode<NetworkSender>>();
        private readonly LinkedList<NetworkSender> _openNetworkSenders = new LinkedList<NetworkSender>();

        private readonly ReusableBufferCreator _reusableEncodingBuffer = new ReusableBufferCreator(16 * 1024);

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkTarget" /> class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message}</code>
        /// </remarks>
        public NetworkTarget()
        {
            SenderFactory = NetworkSenderFactory.Default;
            Encoding = Encoding.UTF8;
            OnOverflow = NetworkTargetOverflowAction.Split;
            KeepConnection = true;
            MaxMessageSize = 65000;
            ConnectionCacheSize = 5;
            LineEnding = LineEndingMode.CRLF;
            OptimizeBufferReuse = GetType() == typeof(NetworkTarget);   // Class not sealed, reduce breaking changes
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkTarget" /> class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message}</code>
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
        /// <li>tcp://host:port - TCP (auto select IPv4/IPv6) (not supported on Windows Phone 7.0)</li>
        /// <li>tcp4://host:port - force TCP/IPv4 (not supported on Windows Phone 7.0)</li>
        /// <li>tcp6://host:port - force TCP/IPv6 (not supported on Windows Phone 7.0)</li>
        /// <li>udp://host:port - UDP (auto select IPv4/IPv6, not supported on Silverlight and on Windows Phone 7.0)</li>
        /// <li>udp4://host:port - force UDP/IPv4 (not supported on Silverlight and on Windows Phone 7.0)</li>
        /// <li>udp6://host:port - force UDP/IPv6  (not supported on Silverlight and on Windows Phone 7.0)</li>
        /// <li>http://host:port/pageName - HTTP using POST verb</li>
        /// <li>https://host:port/pageName - HTTPS using POST verb</li>
        /// </ul>
        /// For SOAP-based webservice support over HTTP use WebService target.
        /// </remarks>
        /// <docgen category='Connection Options' order='10' />
        public Layout Address { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to keep connection open whenever possible.
        /// </summary>
        /// <docgen category='Connection Options' order='10' />
        [DefaultValue(true)]
        public bool KeepConnection { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to append newline at the end of log message.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        [DefaultValue(false)]
        public bool NewLine { get; set; }

        /// <summary>
        /// Gets or sets the end of line value if a newline is appended at the end of log message <see cref="NewLine"/>.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        [DefaultValue("CRLF")]
        public LineEndingMode LineEnding { get; set; }

        /// <summary>
        /// Gets or sets the maximum message size in bytes.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        [DefaultValue(65000)]
        public int MaxMessageSize { get; set; }

        /// <summary>
        /// Gets or sets the size of the connection cache (number of connections which are kept alive).
        /// </summary>
        /// <docgen category="Connection Options" order="10"/>
        [DefaultValue(5)]
        public int ConnectionCacheSize { get; set; }

        /// <summary>
        /// Gets or sets the maximum current connections. 0 = no maximum.
        /// </summary>
        /// <docgen category="Connection Options" order="10"/>
        public int MaxConnections { get; set; }

        /// <summary>
        /// Gets or sets the action that should be taken if the will be more connections than <see cref="MaxConnections"/>.
        /// </summary>
        /// <docgen category='Connection Options' order='10' />
        public NetworkTargetConnectionsOverflowAction OnConnectionOverflow { get; set; }

        /// <summary>
        /// Gets or sets the maximum queue size.
        /// </summary>
        /// <docgen category='Connection Options' order='10' />
        [DefaultValue(0)]
        public int MaxQueueSize { get; set; }

        /// <summary>
        /// Gets or sets the action that should be taken if the message is larger than
        /// maxMessageSize.
        /// </summary>
        /// <docgen category='Connection Options' order='10' />
        [DefaultValue(NetworkTargetOverflowAction.Split)]
        public NetworkTargetOverflowAction OnOverflow { get; set; }

        /// <summary>
        /// Gets or sets the encoding to be used.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        [DefaultValue("utf-8")]
        public Encoding Encoding { get; set; }

        internal INetworkSenderFactory SenderFactory { get; set; }

        /// <summary>
        /// Flush any pending log messages asynchronously (in case of asynchronous targets).
        /// </summary>
        /// <param name="asyncContinuation">The asynchronous continuation.</param>
        protected override void FlushAsync(AsyncContinuation asyncContinuation)
        {
            int remainingCount = 0;

            AsyncContinuation continuation =
                ex =>
                {
                    // ignore exception
                    if (Interlocked.Decrement(ref remainingCount) == 0)
                    {
                        asyncContinuation(null);
                    }
                };

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
                        openSender.FlushAsync(continuation);
                    }
                }
            }
        }

        /// <summary>
        /// Closes the target.
        /// </summary>
        protected override void CloseTarget()
        {
            base.CloseTarget();

            lock (_openNetworkSenders)
            {
                foreach (var openSender in _openNetworkSenders)
                {
                    openSender.Close(ex => { });
                }

                _openNetworkSenders.Clear();
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
            InternalLogger.Trace("Sending to address:  '{0}'", address);

            byte[] bytes = GetBytesToWrite(logEvent.LogEvent);

            if (KeepConnection)
            {
                var senderNode = GetCachedNetworkSender(address);

                ChunkedSend(
                    senderNode.Value,
                    bytes,
                    ex =>
                    {
                        if (ex != null)
                        {
                            InternalLogger.Error(ex, "Error when sending.");
                            ReleaseCachedConnection(senderNode);
                        }

                        logEvent.Continuation(ex);
                    });
            }
            else
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
                            case NetworkTargetConnectionsOverflowAction.DiscardMessage:
                                InternalLogger.Warn("Discarding message otherwise to many connections.");
                                logEvent.Continuation(null);
                                return;

                            case NetworkTargetConnectionsOverflowAction.AllowNewConnnection:
                                InternalLogger.Debug("Too may connections, but this is allowed");
                                break;

                            case NetworkTargetConnectionsOverflowAction.Block:
                                while (_openNetworkSenders.Count >= MaxConnections)
                                {
                                    InternalLogger.Debug("Blocking networktarget otherwhise too many connections.");
                                    Monitor.Wait(_openNetworkSenders);
                                    InternalLogger.Trace("Entered critical section.");
                                }

                                InternalLogger.Trace("Limit ok.");
                                break;
                        }
                    }

                    sender = SenderFactory.Create(address, MaxQueueSize);
                    sender.Initialize();

                    linkedListNode = _openNetworkSenders.AddLast(sender);
                }
                ChunkedSend(
                    sender,
                    bytes,
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
                            InternalLogger.Error(ex, "Error when sending.");
                        }

                        sender.Close(ex2 => { });
                        logEvent.Continuation(ex);
                    });

            }
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
            if (node == null || list != node.List)
            {
                return false;
            }
            list.Remove(node);
            return true;
        }

        /// <summary>
        /// Gets the bytes to be written.
        /// </summary>
        /// <param name="logEvent">Log event.</param>
        /// <returns>Byte array.</returns>
        protected virtual byte[] GetBytesToWrite(LogEventInfo logEvent)
        {
            if (OptimizeBufferReuse)
            {
                if (!NewLine && logEvent.TryGetCachedLayoutValue(Layout, out var text))
                {
                    InternalLogger.Trace("{0} - Sending {1}", this, text);
                    return Encoding.GetBytes(text.ToString());
                }
                else
                {
                    using (var localBuilder = ReusableLayoutBuilder.Allocate())
                    {
                        Layout.RenderAppendBuilder(logEvent, localBuilder.Result, false);
                        if (NewLine)
                        {
                            localBuilder.Result.Append(LineEnding.NewLineCharacters);
                        }

                        InternalLogger.Trace("{0} - Sending {1} chars", this, localBuilder.Result.Length);

                        using (var localBuffer = _reusableEncodingBuffer.Allocate())
                        {
#if !SILVERLIGHT
                            if (localBuilder.Result.Length <= localBuffer.Result.Length)
                            {
                                localBuilder.Result.CopyTo(0, localBuffer.Result, 0, localBuilder.Result.Length);
                                return Encoding.GetBytes(localBuffer.Result, 0, localBuilder.Result.Length);
                            }
                            else
#endif
                            {
                                var rendered = localBuilder.Result.ToString();
                                return Encoding.GetBytes(rendered);
                            }
                        }
                    }
                }
            }
            else
            {
                var rendered = Layout.Render(logEvent);
                InternalLogger.Trace("{0} - Sending: {1}", this, rendered);
                if (NewLine)
                {
                    rendered += LineEnding.NewLineCharacters;
                }
                return Encoding.GetBytes(rendered);
            }
        }

        private LinkedListNode<NetworkSender> GetCachedNetworkSender(string address)
        {
            lock (_currentSenderCache)
            {
                LinkedListNode<NetworkSender> senderNode;

                // already have address
                if (_currentSenderCache.TryGetValue(address, out senderNode))
                {
                    senderNode.Value.CheckSocket();
                    return senderNode;
                }

                if (_currentSenderCache.Count >= ConnectionCacheSize)
                {
                    // make room in the cache by closing the least recently used connection
                    int minAccessTime = int.MaxValue;
                    LinkedListNode<NetworkSender> leastRecentlyUsed = null;

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

                var sender = SenderFactory.Create(address, MaxQueueSize);
                sender.Initialize();
                lock (_openNetworkSenders)
                {
                    senderNode = _openNetworkSenders.AddLast(sender);
                }

                _currentSenderCache.Add(address, senderNode);
                return senderNode;
            }
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

                LinkedListNode<NetworkSender> sender2;

                // make sure the current sender for this address is the one we want to remove
                if (_currentSenderCache.TryGetValue(networkSender.Address, out sender2))
                {
                    if (ReferenceEquals(senderNode, sender2))
                    {
                        _currentSenderCache.Remove(networkSender.Address);
                    }
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", Justification = "Using property names in message.")]
        private void ChunkedSend(NetworkSender sender, byte[] buffer, AsyncContinuation continuation)
        {
            int tosend = buffer.Length;
            if (tosend <= MaxMessageSize)
            {
                // Chunking is not needed, no need to perform delegate capture
                InternalLogger.Trace("Sending chunk, position: {0}, length: {1}", 0, tosend);
                if (tosend <= 0)
                {
                    continuation(null);
                    return;
                }

                sender.Send(buffer, 0, tosend, continuation);
            }
            else
            {
                int pos = 0;

                AsyncContinuation sendNextChunk = null;

                sendNextChunk = ex =>
                {
                    if (ex != null)
                    {
                        continuation(ex);
                        return;
                    }
                    InternalLogger.Trace("Sending chunk, position: {0}, length: {1}", pos, tosend);
                    if (tosend <= 0)
                    {
                        continuation(null);
                        return;
                    }

                    int chunksize = tosend;
                    if (chunksize > MaxMessageSize)
                    {
                        if (OnOverflow == NetworkTargetOverflowAction.Discard)
                        {
                            InternalLogger.Trace("discard because chunksize > this.MaxMessageSize");
                            continuation(null);
                            return;
                        }

                        if (OnOverflow == NetworkTargetOverflowAction.Error)
                        {
                            continuation(new OverflowException($"Attempted to send a message larger than MaxMessageSize ({MaxMessageSize}). Actual size was: {buffer.Length}. Adjust OnOverflow and MaxMessageSize parameters accordingly."));
                            return;
                        }

                        chunksize = MaxMessageSize;
                    }

                    int pos0 = pos;
                    tosend -= chunksize;
                    pos += chunksize;

                    sender.Send(buffer, pos0, chunksize, sendNextChunk);
                };

                sendNextChunk(null);
            }
        }
    }
}

