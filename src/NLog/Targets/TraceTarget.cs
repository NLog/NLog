// 
// Copyright (c) 2004-2020 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

#if !NETSTANDARD1_3

namespace NLog.Targets
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;

    /// <summary>
    /// Sends log messages through System.Diagnostics.Trace.
    /// </summary>
    /// <seealso href="https://github.com/nlog/nlog/wiki/Trace-target">Documentation on NLog Wiki</seealso>
    /// <example>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" source="examples/targets/Configuration File/Trace/NLog.config" />
    /// <p>
    /// This assumes just one target and a single rule. More configuration
    /// options are described <a href="config.html">here</a>.
    /// </p>
    /// <p>
    /// To set up the log target programmatically use code like this:
    /// </p>
    /// <code lang="C#" source="examples/targets/Configuration API/Trace/Simple/Example.cs" />
    /// </example>
    [Target("Trace")]
    public sealed class TraceTarget : TargetWithLayout
    {
        /// <summary>
        /// Report based on <see cref="NLog.LogLevel"/> to the matching <see cref="System.Diagnostics.Trace"/> facility-method:
        /// 
        ///  - <see cref="NLog.LogLevel.Fatal"/> writes to <see cref="Trace.TraceError(string)" />
        ///  - <see cref="NLog.LogLevel.Error"/> writes to <see cref="Trace.TraceError(string)" />
        ///  - <see cref="NLog.LogLevel.Warn"/> writes to <see cref="Trace.TraceWarning(string)" />
        ///  - <see cref="NLog.LogLevel.Info"/> writes to <see cref="Trace.TraceInformation(string)" />
        ///  - <see cref="NLog.LogLevel.Debug"/> writes to <see cref="Trace.WriteLine(string)" />
        ///  - <see cref="NLog.LogLevel.Trace"/> writes to <see cref="Trace.WriteLine(string)" />
        /// </summary>
        /// <remarks>Trace Listeners might give unwanted side-effects and output, when using level-specific Trace-methods.</remarks>
        /// <docgen category='Output Options' order='100' />
        [DefaultValue(false)]
        public bool ReportLogLevel { get; set; }

        /// <summary>
        /// Always use <see cref="Trace.WriteLine(string)"/> independent of <see cref="LogLevel"/>
        /// </summary>
        /// <docgen category='Output Options' order='100' />
        [Obsolete("Replaced by ReportLogLevel as the property-name is more meaningful. Marked obsolete on NLog 5.0")]
        [DefaultValue(true)]
        public bool RawWrite { get => !ReportLogLevel; set => ReportLogLevel = !value; }

        /// <summary>
        /// Forward <see cref="NLog.LogLevel.Fatal" /> to <see cref="Trace.Fail(string)" /> (Instead of <see cref="Trace.TraceError(string)" />)
        /// </summary>
        /// <remarks>
        /// Trace.Fail can have special side-effects, and give fatal exceptions, message dialogs or Environment.FailFast
        /// </remarks>
        /// <docgen category='Output Options' order='100' />
        [DefaultValue(false)]
        public bool EnableTraceFail { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TraceTarget" /> class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message:withexception=true}</code>
        /// </remarks>
        public TraceTarget() : base()
        {
            OptimizeBufferReuse = true;
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

        /// <summary>
        /// Writes the specified logging event to the <see cref="System.Diagnostics.Trace"/> facility.
        /// </summary>
        protected override void Write(LogEventInfo logEvent)
        {
            string logMessage = RenderLogEvent(Layout, logEvent);
            if (!ReportLogLevel || logEvent.Level <= LogLevel.Debug)
            {
                Trace.WriteLine(logMessage);
            }
            else if (logEvent.Level == LogLevel.Info)
            {
                Trace.TraceInformation(logMessage);
            }
            else if (logEvent.Level == LogLevel.Warn)
            {
                Trace.TraceWarning(logMessage);
            }
            else if (logEvent.Level == LogLevel.Error)
            {
                Trace.TraceError(logMessage);
            }
            else if (logEvent.Level >= LogLevel.Fatal)
            {
                if (EnableTraceFail)
                    Trace.Fail(logMessage); // Can throw exceptions, show message dialog or perform Environment.FailFast
                else
                    Trace.TraceError(logMessage);
            }
            else
            {
                Trace.WriteLine(logMessage);                
            }
        }
    }
}

#endif
