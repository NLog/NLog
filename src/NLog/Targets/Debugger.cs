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
using System.Diagnostics;
using NLog.Config;

namespace NLog.Targets
{
    /// <summary>
    /// Writes logging messages to the attached managed debugger (for example Visual Studio .NET or DbgCLR).
    /// </summary>
    /// <example>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" src="examples/targets/Configuration File/Debugger/NLog.config" />
    /// <p>
    /// This assumes just one target and a single rule. More configuration
    /// options are described <a href="config.html">here</a>.
    /// </p>
    /// <p>
    /// To set up the log target programmatically use code like this:
    /// </p>
    /// <code lang="C#" src="examples/targets/Configuration API/Debugger/Simple/Example.cs" />
    /// </example>
    [Target("Debugger")]
    [NotSupportedRuntime(Framework=RuntimeFramework.DotNetCompactFramework)]
    public sealed class DebuggerTarget: TargetWithLayoutHeaderAndFooter
    {
        static DebuggerTarget()
        {
        }
        /// <summary>
        /// Writes the specified logging event to the attached debugger.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        protected internal override void Write(LogEventInfo logEvent)
        {
            if (Debugger.IsLogging())
            {
                Debugger.Log(logEvent.Level.Ordinal, logEvent.LoggerName, CompiledLayout.GetFormattedMessage(logEvent) + "\n");
            }
        }

        /// <summary>
        /// Initializes the target.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            if (CompiledHeader != null)
            {
                Debugger.Log(LogLevel.Off.Ordinal, "", CompiledHeader.GetFormattedMessage(LogEventInfo.CreateNullEvent()) + "\n");
            }
        }

        /// <summary>
        /// Closes the target and releases any unmanaged resources.
        /// </summary>
        protected internal override void Close()
        {
            if (CompiledFooter != null)
            {
                Debugger.Log(LogLevel.Off.Ordinal, "", CompiledFooter.GetFormattedMessage(LogEventInfo.CreateNullEvent()) + "\n");
            }
            base.Close();
        }
    }
}

#endif
