// 
// Copyright (c) 2004,2005 Jaroslaw Kowalski <jkowalski@users.sourceforge.net>
// 
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
// * Neither the name of the Jaroslaw Kowalski nor the names of its 
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

namespace NLog.Targets
{
    /// <summary>
    /// Writes logging messages to the ASP.NET trace.
    /// </summary>
    /// <remarks>
    /// Resulting log entries can be viewed by navigating to http://server/path/Trace.axd
    /// </remarks>
    [Target("ASPNetTrace")]
    public class ASPNetTraceTarget: Target
    {
        /// <summary>
        /// Writes the specified logging event to the ASP.NET Trace facility. Log entries
        /// can then be viewed by navigating to http://server/path/Trace.axd
        /// If the log level is greater than or equal to <see cref="LogLevel.Warn"/> it uses the
        /// <see cref="System.Web.TraceContext.Warn"/> method, otherwise it uses
        /// <see cref="System.Web.TraceContext.Write" /> method.
        /// </summary>
        /// <param name="ev">The logging event.</param>
        protected internal override void Append(LogEventInfo ev)
        {
            HttpContext context = HttpContext.Current;

            if (context == null)
            {
                return ;
            }

            if (ev.Level >= LogLevel.Warn)
            {
                context.Trace.Warn(ev.LoggerName, CompiledLayout.GetFormattedMessage(ev));
            }
            else
            {
                context.Trace.Write(ev.LoggerName, CompiledLayout.GetFormattedMessage(ev));
            }
        }
    }
}

#endif
