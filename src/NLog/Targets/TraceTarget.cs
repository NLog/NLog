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

#define TRACE

#if !SILVERLIGHT

namespace NLog.Targets
{
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
        /// Writes the specified logging event to the <see cref="System.Diagnostics.Trace"/> facility.
        /// If the log level is greater than or equal to <see cref="LogLevel.Error"/> it uses the
        /// <see cref="System.Diagnostics.Trace.Fail(string)"/> method, otherwise it uses
        /// <see cref="System.Diagnostics.Trace.Write(string)" /> method.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        protected override void Write(LogEventInfo logEvent)
        {
            if (logEvent.Level <= LogLevel.Debug)
            {
                Trace.WriteLine(this.Layout.Render(logEvent));
            }
            else if (logEvent.Level == LogLevel.Info)
            {
                Trace.TraceInformation(this.Layout.Render(logEvent));
            }
            else if (logEvent.Level == LogLevel.Warn)
            {
                Trace.TraceWarning(this.Layout.Render(logEvent));
            }
            else if (logEvent.Level == LogLevel.Error)
            {
                Trace.TraceError(this.Layout.Render(logEvent));
            }
            else if (logEvent.Level >= LogLevel.Fatal)
            {
                Trace.Fail(this.Layout.Render(logEvent));
            }
            else
            {
                Trace.WriteLine(this.Layout.Render(logEvent));                
            }
        }
    }
}

#endif
