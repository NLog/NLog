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

#define TRACE

namespace NLog.Targets
{
    using System.Diagnostics;

    /// <summary>
    /// Sends log messages through System.Diagnostics.Trace.
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/nlog/nlog/wiki/Trace-target">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/nlog/nlog/wiki/Trace-target">Documentation on NLog Wiki</seealso>
    /// <example>
    /// <p>
    /// To set up the target in the <a href="https://github.com/NLog/NLog/wiki/Configuration-file">configuration file</a>,
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" source="examples/targets/Configuration File/Trace/NLog.config" />
    /// <p>
    /// To set up the log target programmatically use code like this:
    /// </p>
    /// <code lang="C#" source="examples/targets/Configuration API/Trace/Simple/Example.cs" />
    /// </example>
    [Target("Trace")]
    [Target("TraceSystem")]
    public sealed class TraceTarget : TargetWithLayoutHeaderAndFooter
    {
        /// <summary>
        /// Force use <see cref="Trace.WriteLine(string)"/> independent of <see cref="LogLevel"/>
        /// </summary>
        /// <docgen category='Output Options' order='100' />
        public bool RawWrite { get; set; }

        /// <summary>
        /// Forward <see cref="LogLevel.Fatal" /> to <see cref="Trace.Fail(string)" /> (Instead of <see cref="Trace.TraceError(string)" />)
        /// </summary>
        /// <remarks>
        /// Trace.Fail can have special side-effects, and give fatal exceptions, message dialogs or Environment.FailFast
        /// </remarks>
        /// <docgen category='Output Options' order='100' />
        public bool EnableTraceFail { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TraceTarget" /> class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message:withexception=true}</code>
        /// </remarks>
        public TraceTarget()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TraceTarget" /> class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message:withexception=true}</code>
        /// </remarks>
        /// <param name="name">Name of the target.</param>
        public TraceTarget(string name) : this()
        {
            Name = name;
        }

        /// <inheritdoc/>
        protected override void InitializeTarget()
        {
            base.InitializeTarget();

            if (Header != null)
            {
                string logMessage = RenderLogEvent(Header, LogEventInfo.CreateNullEvent());
                TraceWrite(LogLevel.Debug, logMessage);
            }
        }

        /// <inheritdoc/>
        protected override void CloseTarget()
        {
            if (Footer != null)
            {
                string logMessage = RenderLogEvent(Footer, LogEventInfo.CreateNullEvent());
                TraceWrite(LogLevel.Debug, logMessage);
            }

            base.CloseTarget();
        }

        /// <summary>
        /// Writes the specified logging event to the <see cref="System.Diagnostics.Trace"/> facility.
        ///
        /// Redirects the log message depending on <see cref="LogLevel"/> and  <see cref="RawWrite"/>.
        /// When <see cref="RawWrite"/> is <c>false</c>:
        ///  - <see cref="LogLevel.Fatal"/> writes to <see cref="Trace.TraceError(string)" />
        ///  - <see cref="LogLevel.Error"/> writes to <see cref="Trace.TraceError(string)" />
        ///  - <see cref="LogLevel.Warn"/> writes to <see cref="Trace.TraceWarning(string)" />
        ///  - <see cref="LogLevel.Info"/> writes to <see cref="Trace.TraceInformation(string)" />
        ///  - <see cref="LogLevel.Debug"/> writes to <see cref="Trace.WriteLine(string)" />
        ///  - <see cref="LogLevel.Trace"/> writes to <see cref="Trace.WriteLine(string)" />
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        protected override void Write(LogEventInfo logEvent)
        {
            string logMessage = RenderLogEvent(Layout, logEvent);
            TraceWrite(logEvent.Level, logMessage);
        }

        private void TraceWrite(LogLevel logLevel, string logMessage)
        {
            if (RawWrite || logLevel <= LogLevel.Debug)
            {
                Trace.WriteLine(logMessage);    // NOSONAR
            }
            else if (logLevel == LogLevel.Info)
            {
                Trace.TraceInformation(logMessage);
            }
            else if (logLevel == LogLevel.Warn)
            {
                Trace.TraceWarning(logMessage);
            }
            else if (logLevel == LogLevel.Error)
            {
                Trace.TraceError(logMessage);
            }
            else if (logLevel >= LogLevel.Fatal)
            {
                if (EnableTraceFail)
                    Trace.Fail(logMessage); // Can throw exceptions, show message dialog or perform Environment.FailFast
                else
                    Trace.TraceError(logMessage);
            }
            else
            {
                Trace.WriteLine(logMessage);    // NOSONAR
            }
        }
    }
}
