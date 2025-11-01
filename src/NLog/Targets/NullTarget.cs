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
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using NLog.Common;

    /// <summary>
    /// Discards log messages. Used mainly for debugging and benchmarking.
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/nlog/nlog/wiki/Null-target">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/nlog/nlog/wiki/Null-target">Documentation on NLog Wiki</seealso>
    /// <example>
    /// <p>
    /// To set up the target in the <a href="https://github.com/NLog/NLog/wiki/Configuration-file">configuration file</a>,
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" source="examples/targets/Configuration File/Null/NLog.config" />
    /// <p>
    /// To set up the log target programmatically use code like this:
    /// </p>
    /// <code lang="C#" source="examples/targets/Configuration API/Null/Simple/Example.cs" />
    /// </example>
    [Target("Null")]
    public sealed class NullTarget : TargetWithLayout
    {
        /// <summary>
        /// Gets the number of times this target has been called.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public int LogEventCounter => _logEventCounter;
        private int _logEventCounter;

        /// <summary>
        /// Gets or sets a value indicating whether to render <see cref="TargetWithLayout.Layout" /> for LogEvent
        /// </summary>
        /// <remarks>Default: <see langword="false"/></remarks>
        /// <docgen category='Layout Options' order='10' />
        public bool FormatMessage { get; set; }

        /// <summary>
        /// Gets the last message rendered by this target.
        /// </summary>
        /// <remarks>Requires <see cref="FormatMessage"/> = <see langword="true"/></remarks>
        /// <docgen category='Layout Options' order='10' />
        public string LastMessage => _lastMesageBuilder?.ToString() ?? string.Empty;
        private StringBuilder? _lastMesageBuilder;

        /// <summary>
        /// Gets the last LogEvent rendered by this target.
        /// </summary>
        /// <remarks>Requires <see cref="FormatMessage"/> = <see langword="true"/></remarks>
        /// <docgen category='Layout Options' order='10' />
        public LogEventInfo? LastLogEvent => _lastLogEvent;
        private LogEventInfo? _lastLogEvent;

        /// <summary>
        /// Initializes a new instance of the <see cref="NullTarget" /> class.
        /// </summary>
        public NullTarget()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NullTarget" /> class.
        /// </summary>
        /// <param name="name">Name of the target.</param>
        public NullTarget(string name) : this()
        {
            Name = name;
        }

        /// <inheritdoc />
        protected override void WriteAsyncThreadSafe(AsyncLogEventInfo logEvent)
        {
            if (FormatMessage)
            {
                base.WriteAsyncThreadSafe(logEvent);
            }
            else
            {
                Interlocked.Increment(ref _logEventCounter);
                logEvent.Continuation(null);
            }
        }

        /// <inheritdoc />
        protected override void WriteAsyncThreadSafe(IList<AsyncLogEventInfo> logEvents)
        {
            if (FormatMessage)
            {
                base.WriteAsyncThreadSafe(logEvents);
            }
            else
            {
                Interlocked.Add(ref _logEventCounter, logEvents.Count);
                for (int i = 0; i < logEvents.Count; i++)
                {
                    logEvents[i].Continuation(null);
                }
            }
        }

        /// <inheritdoc />
        protected override void Write(LogEventInfo logEvent)
        {
            _logEventCounter++;

            if (FormatMessage)
            {
                _lastLogEvent = logEvent;
                var stringBuilder = _lastMesageBuilder ?? (_lastMesageBuilder = new StringBuilder(128));
                stringBuilder.Length = 0;
                Layout?.Render(logEvent, stringBuilder);
            }
        }
    }
}
