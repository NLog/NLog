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

#if !NET_CF && !SILVERLIGHT

namespace NLog.Targets
{
    using System.Web;

    /// <summary>
    /// Writes log messages to the ASP.NET trace.
    /// </summary>
    /// <seealso href="http://nlog-project.org/wiki/AspNetTrace_target">Documentation on NLog Wiki</seealso>
    /// <remarks>
    /// Log entries can then be viewed by navigating to http://server/path/Trace.axd.
    /// </remarks>
    [Target("AspNetTrace")]
    public class AspNetTraceTarget : TargetWithLayout
    {
        /// <summary>
        /// Writes the specified logging event to the ASP.NET Trace facility. 
        /// If the log level is greater than or equal to <see cref="LogLevel.Warn"/> it uses the
        /// System.Web.TraceContext.Warn method, otherwise it uses
        /// System.Web.TraceContext.Write method.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        protected override void Write(LogEventInfo logEvent)
        {
            HttpContext context = HttpContext.Current;

            if (context == null)
            {
                return;
            }

            if (logEvent.Level >= LogLevel.Warn)
            {
                context.Trace.Warn(logEvent.LoggerName, this.Layout.Render(logEvent));
            }
            else
            {
                context.Trace.Write(logEvent.LoggerName, this.Layout.Render(logEvent));
            }
        }
    }
}

#endif
