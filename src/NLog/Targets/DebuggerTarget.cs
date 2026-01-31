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
    using System.Diagnostics;
    using NLog.Common;
    using NLog.Layouts;

    /// <summary>
    /// Writes log messages to the attached managed debugger.
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/NLog/NLog/wiki/Debugger-target">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/NLog/NLog/wiki/Debugger-target">Documentation on NLog Wiki</seealso>
    /// <example>
    /// <p>
    /// To set up the target in the <a href="https://github.com/NLog/NLog/wiki/Configuration-file">configuration file</a>,
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" source="examples/targets/Configuration File/Debugger/NLog.config" />
    /// <p>
    /// To set up the log target programmatically use code like this:
    /// </p>
    /// <code lang="C#" source="examples/targets/Configuration API/Debugger/Simple/Example.cs" />
    /// </example>
    [Target("Debugger")]
    public sealed class DebuggerTarget : TargetWithLayoutHeaderAndFooter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DebuggerTarget" /> class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message:withexception=true}</code>
        /// </remarks>
        public DebuggerTarget()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DebuggerTarget" /> class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message:withexception=true}</code>
        /// </remarks>
        /// <param name="name">Name of the target.</param>
        public DebuggerTarget(string name) : this()
        {
            Name = name;
        }

        /// <summary>
        /// The category of the message.
        /// </summary>
        /// <remarks>
        /// The category parameter is limited to 256 characters. Strings longer than 256 characters are truncated.
        /// </remarks>
        public Layout Category { get; set; } = Layout.FromMethod(l => l.LoggerName, LayoutRenderOptions.ThreadAgnostic);

        /// <summary>
        /// Gets or sets the maximum message size in number of characters. On limit breach then <see cref="OnOverflow"/> action is activated.
        /// </summary>
        /// <remarks>
        /// Messages that are too large can cause issues for various debuggers, some have max-limit of 4091. Default: <c>0</c> (no limit).
        /// </remarks>
        public int MaxMessageSize { get; set; }

        /// <summary>
        /// Gets or sets the action that should be taken if the message is larger than <see cref="MaxMessageSize" />
        /// </summary>
        public DebuggerTargetOverflowAction OnOverflow { get; set; } = DebuggerTargetOverflowAction.Split;

        /// <inheritdoc/>
        protected override void InitializeTarget()
        {
            base.InitializeTarget();
            if (!Debugger.IsLogging())
            {
                InternalLogger.Debug("{0}: System.Diagnostics.Debugger.IsLogging()==false. Output has been disabled.", this);
            }

            if (Header != null)
            {
                DebuggerLogMessageLayout(Header);
            }
        }

        /// <inheritdoc/>
        protected override void CloseTarget()
        {
            if (Footer != null)
            {
                DebuggerLogMessageLayout(Footer);
            }

            base.CloseTarget();
        }

        /// <inheritdoc/>
        protected override void Write(LogEventInfo logEvent)
        {
            if (!Debugger.IsLogging())
                return;

            var category = RenderLogEvent(Category, logEvent);

            string logMessage;
            using (var localTarget = ReusableLayoutBuilder.Allocate())
            {
                Layout.Render(logEvent, localTarget.Result);

                if (MaxMessageSize > 0 && localTarget.Result.Length > MaxMessageSize)
                {
                    switch (OnOverflow)
                    {
                        case DebuggerTargetOverflowAction.None:
                            InternalLogger.Debug("{0}: Notice log message has length {1} > MaxMessageSize={2}", this, localTarget.Result.Length, MaxMessageSize);
                            break;
                        case DebuggerTargetOverflowAction.Truncate:
                            InternalLogger.Debug("{0}: Truncates log message because length {1} > MaxMessageSize={2}", this, localTarget.Result.Length, MaxMessageSize);
                            localTarget.Result.Length = MaxMessageSize;
                            break;
                        case DebuggerTargetOverflowAction.Split:
                            InternalLogger.Debug("{0}: Splits log message because length {1} > MaxMessageSize={2}", this, localTarget.Result.Length, MaxMessageSize);
                            for (int offset = 0; offset < localTarget.Result.Length; offset += MaxMessageSize)
                            {
                                int length = System.Math.Min(MaxMessageSize, localTarget.Result.Length - offset);
                                var partMessage = localTarget.Result.ToString(offset, length);
                                Debugger.Log(logEvent.Level.Ordinal, category, partMessage + "\n");
                            }
                            return;
                        case DebuggerTargetOverflowAction.Discard:
                            InternalLogger.Debug("{0}: Discards log message because length {1} > MaxMessageSize={2}", this, localTarget.Result.Length, MaxMessageSize);
                            return;
                    }
                }

                localTarget.Result.Append('\n');
                logMessage = localTarget.Result.ToString();
            }

            Debugger.Log(logEvent.Level.Ordinal, category, logMessage);
        }

        private void DebuggerLogMessageLayout(Layout layout)
        {
            var nullEvent = LogEventInfo.CreateNullEvent();
            var category = Category?.Render(nullEvent) ?? string.Empty;
            var message = layout?.Render(nullEvent) ?? string.Empty;
            Debugger.Log(LogLevel.Info.Ordinal, category, message + "\n");
        }
    }
}
