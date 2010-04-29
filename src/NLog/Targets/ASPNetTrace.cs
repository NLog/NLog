// 
// Copyright (c) 2004-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
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

#if !NETCF
using System;
using System.Web;

using NLog.Targets;
using NLog.Config;

namespace NLog.Targets
{
    /// <summary>
    /// Writes logging messages to the ASP.NET trace.
    /// </summary>
    /// <example>
    /// <p>To set up the ASP.NET Trace target in the <a href="config.html">configuration file</a>, put
    /// the following in <c>web.nlog</c> file in your web application directory.
    /// </p>
    /// <code lang="XML" src="examples/targets/Configuration File/ASPNetTrace/web.nlog" />
    /// <p>
    /// This assumes just one target and a single rule. More configuration
    /// options are described <a href="config.html">here</a>.
    /// </p>
    /// <p>
    /// To configure the target programmatically, put the following
    /// piece of code in your <c>Application_OnStart()</c> handler in Global.asax.cs 
    /// or some other place that gets executed at the very beginning of your code:
    /// </p>
    /// <code lang="C#" src="examples/targets/Configuration API/ASPNetTrace/Global.asax.cs" />
    /// <p>
    /// Fully working C# project can be found in the <c>Examples/Targets/Configuration API/ASPNetTrace</c>
    /// directory along with usage instructions.
    /// </p>
    /// Resulting log entries can be viewed by navigating to http://server/path/Trace.axd.
    /// <br/>
    /// <b>HTTP Request List:</b><br/>
    /// <img src="examples/targets/Screenshots/ASPNetTrace/ASPNetTraceOutput1.gif" />
    /// <p/>
    /// <b>HTTP Request Details:</b>
    /// <br/>
    /// <img src="examples/targets/Screenshots/ASPNetTrace/ASPNetTraceOutput2.gif" />
    /// <p/>
    /// </example>
    [Target("ASPNetTrace")]
    [NotSupportedRuntime(Framework=RuntimeFramework.DotNetCompactFramework)]
    public class ASPNetTraceTarget: TargetWithLayout
    {
        /// <summary>
        /// Writes the specified logging event to the ASP.NET Trace facility. Log entries
        /// can then be viewed by navigating to http://server/path/Trace.axd
        /// If the log level is greater than or equal to <see cref="LogLevel.Warn"/> it uses the
        /// <see cref="System.Web.TraceContext.Warn(String,String)"/> method, otherwise it uses
        /// <see cref="System.Web.TraceContext.Write(String,String)" /> method.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        protected internal override void Write(LogEventInfo logEvent)
        {
            HttpContext context = HttpContext.Current;

            if (context == null)
            {
                return ;
            }

            if (logEvent.Level >= LogLevel.Warn)
            {
                context.Trace.Warn(logEvent.LoggerName, CompiledLayout.GetFormattedMessage(logEvent));
            }
            else
            {
                context.Trace.Write(logEvent.LoggerName, CompiledLayout.GetFormattedMessage(logEvent));
            }
        }
    }
}

#endif
