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

namespace NLog.Targets
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using NLog.Common;
    using NLog.Internal;
    using NLog.Layouts;

    /// <summary>
    /// Pops up log messages as message boxes.
    /// </summary>
    /// <seealso href="http://nlog-project.org/wiki/MessageBox_target">Documentation on NLog Wiki</seealso>
    /// <example>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" source="examples/targets/Configuration File/MessageBox/NLog.config" />
    /// <p>
    /// This assumes just one target and a single rule. More configuration
    /// options are described <a href="config.html">here</a>.
    /// </p>
    /// <p>
    /// The result is a message box:
    /// </p>
    /// <img src="examples/targets/Screenshots/MessageBox/MessageBoxTarget.gif" />
    /// <p>
    /// To set up the log target programmatically use code like this:
    /// </p>
    /// <code lang="C#" source="examples/targets/Configuration API/MessageBox/Simple/Example.cs" />
    /// </example>
    [Target("MessageBox")]
    public sealed class MessageBoxTarget : TargetWithLayout
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageBoxTarget" /> class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message}</code>
        /// </remarks>
        public MessageBoxTarget()
        {
            this.Caption = "NLog";
        }

        /// <summary>
        /// Gets or sets the message box title.
        /// </summary>
        /// <docgen category='UI Options' order='10' />
        public Layout Caption { get; set; }

        /// <summary>
        /// Displays the message box with the log message and caption specified in the Caption
        /// parameter.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions", Justification = "This is just debugging output.")]
        protected override void Write(LogEventInfo logEvent)
        {
            MessageBoxHelper.Show(this.Layout.Render(logEvent), this.Caption.Render(logEvent));
        }

        /// <summary>
        /// Displays the message box with the array of rendered logs messages and caption specified in the Caption
        /// parameter.
        /// </summary>
        /// <param name="logEvents">The array of logging events.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions", Justification = "This is just debugging output.")]
        protected override void Write(AsyncLogEventInfo[] logEvents)
        {
            if (logEvents.Length == 0)
            {
                return;
            }

            var sb = new StringBuilder();
            var lastLogEvent = logEvents[logEvents.Length - 1];
            foreach (var ev in logEvents)
            {
                sb.Append(this.Layout.Render(ev.LogEvent));
                sb.Append("\n");
            }

            MessageBoxHelper.Show(sb.ToString(), this.Caption.Render(lastLogEvent.LogEvent));

            for (int i = 0; i < logEvents.Length; ++i)
            {
                logEvents[i].Continuation(null);
            }
        }
    }
}
