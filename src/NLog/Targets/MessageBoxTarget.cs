// 
// Copyright (c) 2004-2009 Jaroslaw Kowalski <jaak@jkowalski.net>
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

using System.Collections.Generic;
using System.Text;

#if SILVERLIGHT
using System.Windows.Browser;
#else
using System.Windows.Forms;
#endif

using NLog.Layouts;

namespace NLog.Targets
{
    /// <summary>
    /// Pops up logging messages as message boxes.
    /// </summary>
    /// <example>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" src="examples/targets/Configuration File/MessageBox/NLog.config" />
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
    /// <code lang="C#" src="examples/targets/Configuration API/MessageBox/Simple/Example.cs" />
    /// </example>
    [Target("MessageBox")]
    public sealed class MessageBoxTarget : TargetWithLayout
    {
        /// <summary>
        /// Initializes a new instance of the MessageBoxTarget class.
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
        public Layout Caption { get; set; }

        /// <summary>
        /// Adds all layouts used by this target to the specified collection.
        /// </summary>
        /// <param name="layouts">The collection to add layouts to.</param>
        public override void PopulateLayouts(ICollection<Layout> layouts)
        {
            base.PopulateLayouts(layouts);
            if (this.Caption != null)
            {
                this.Caption.PopulateLayouts(layouts);
            }
        }

        /// <summary>
        /// Displays the message box with the log message and caption specified in the Caption
        /// parameter.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        protected internal override void Write(LogEventInfo logEvent)
        {
#if SILVERLIGHT
            HtmlPage.Window.Alert(this.Caption.GetFormattedMessage(logEvent) + "\r\n\r\n" + this.Layout.GetFormattedMessage(logEvent));
#else
            MessageBox.Show(this.Layout.GetFormattedMessage(logEvent), this.Caption.GetFormattedMessage(logEvent));
#endif
        }

        /// <summary>
        /// Displays the message box with the array of rendered logs messages and caption specified in the Caption
        /// parameter.
        /// </summary>
        /// <param name="logEvents">The array of logging events.</param>
        protected internal override void Write(LogEventInfo[] logEvents)
        {
            if (logEvents.Length == 0)
            {
                return;
            }

            StringBuilder sb = new StringBuilder();
            LogEventInfo lastLogEvent = logEvents[logEvents.Length - 1];
            foreach (LogEventInfo ev in logEvents)
            {
                sb.Append(this.Layout.GetFormattedMessage(ev));
                sb.Append("\n");
            }

#if SILVERLIGHT
            HtmlPage.Window.Alert(this.Caption.GetFormattedMessage(lastLogEvent) + "\r\n\r\n" + sb.ToString());
#else
            MessageBox.Show(sb.ToString(), this.Caption.GetFormattedMessage(lastLogEvent));
#endif
        }
    }
}
