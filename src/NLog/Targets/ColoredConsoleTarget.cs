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
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using NLog.Config;

    /// <summary>
    /// Writes log messages to the console with customizable coloring.
    /// </summary>
    /// <seealso href="http://nlog-project.org/wiki/ColoredConsole_target">Documentation on NLog Wiki</seealso>
    [Target("ColoredConsole")]
    public sealed class ColoredConsoleTarget : TargetWithLayoutHeaderAndFooter
    {
        private static readonly IList<ConsoleRowHighlightingRule> defaultConsoleRowHighlightingRules = new List<ConsoleRowHighlightingRule>()
        {
            new ConsoleRowHighlightingRule("level == LogLevel.Fatal", ConsoleOutputColor.Red, ConsoleOutputColor.NoChange),
            new ConsoleRowHighlightingRule("level == LogLevel.Error", ConsoleOutputColor.Yellow, ConsoleOutputColor.NoChange),
            new ConsoleRowHighlightingRule("level == LogLevel.Warn", ConsoleOutputColor.Magenta, ConsoleOutputColor.NoChange),
            new ConsoleRowHighlightingRule("level == LogLevel.Info", ConsoleOutputColor.White, ConsoleOutputColor.NoChange),
            new ConsoleRowHighlightingRule("level == LogLevel.Debug", ConsoleOutputColor.Gray, ConsoleOutputColor.NoChange),
            new ConsoleRowHighlightingRule("level == LogLevel.Trace", ConsoleOutputColor.DarkGray, ConsoleOutputColor.NoChange),
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="ColoredConsoleTarget" /> class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message}</code>
        /// </remarks>
        public ColoredConsoleTarget()
        {
            this.WordHighlightingRules = new List<ConsoleWordHighlightingRule>();
            this.RowHighlightingRules = new List<ConsoleRowHighlightingRule>();
            this.UseDefaultRowHighlightingRules = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the error stream (stderr) should be used instead of the output stream (stdout).
        /// </summary>
        /// <docgen category='Output Options' order='10' />
        [DefaultValue(false)]
        public bool ErrorStream { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use default row highlighting rules.
        /// </summary>
        /// <remarks>
        /// The default rules are:
        /// <table>
        /// <tr>
        /// <th>Condition</th>
        /// <th>Foreground Color</th>
        /// <th>Background Color</th>
        /// </tr>
        /// <tr>
        /// <td>level == LogLevel.Fatal</td>
        /// <td>Red</td>
        /// <td>NoChange</td>
        /// </tr>
        /// <tr>
        /// <td>level == LogLevel.Error</td>
        /// <td>Yellow</td>
        /// <td>NoChange</td>
        /// </tr>
        /// <tr>
        /// <td>level == LogLevel.Warn</td>
        /// <td>Magenta</td>
        /// <td>NoChange</td>
        /// </tr>
        /// <tr>
        /// <td>level == LogLevel.Info</td>
        /// <td>White</td>
        /// <td>NoChange</td>
        /// </tr>
        /// <tr>
        /// <td>level == LogLevel.Debug</td>
        /// <td>Gray</td>
        /// <td>NoChange</td>
        /// </tr>
        /// <tr>
        /// <td>level == LogLevel.Trace</td>
        /// <td>DarkGray</td>
        /// <td>NoChange</td>
        /// </tr>
        /// </table>
        /// </remarks>
        /// <docgen category='Highlighting Rules' order='9' />
        [DefaultValue(true)]
        public bool UseDefaultRowHighlightingRules { get; set; }

        /// <summary>
        /// Gets the row highlighting rules.
        /// </summary>
        /// <docgen category='Highlighting Rules' order='10' />
        [ArrayParameter(typeof(ConsoleRowHighlightingRule), "highlight-row")]
        public IList<ConsoleRowHighlightingRule> RowHighlightingRules { get; private set; }

        /// <summary>
        /// Gets the word highlighting rules.
        /// </summary>
        /// <docgen category='Highlighting Rules' order='11' />
        [ArrayParameter(typeof(ConsoleWordHighlightingRule), "highlight-word")]
        public IList<ConsoleWordHighlightingRule> WordHighlightingRules { get; private set; }

        /// <summary>
        /// Initializes the target.
        /// </summary>
        protected override void InitializeTarget()
        {
            base.InitializeTarget();
            if (Header != null)
            {
                LogEventInfo lei = LogEventInfo.CreateNullEvent();
                this.Output(lei, Header.Render(lei));
            }
        }

        /// <summary>
        /// Closes the target and releases any unmanaged resources.
        /// </summary>
        protected override void CloseTarget()
        {
            if (Footer != null)
            {
                LogEventInfo lei = LogEventInfo.CreateNullEvent();
                this.Output(lei, Footer.Render(lei));
            }

            base.CloseTarget();
        }

            /// <summary>
        /// Writes the specified log event to the console highlighting entries
        /// and words based on a set of defined rules.
        /// </summary>
        /// <param name="logEvent">Log event.</param>
        protected override void Write(LogEventInfo logEvent)
        {
            this.Output(logEvent, this.Layout.Render(logEvent));
        }

        private static void ColorizeEscapeSequences(
            TextWriter output,
            string message,
            ColorPair startingColor,
            ColorPair defaultColor)
        {
            var colorStack = new Stack<ColorPair>();

            colorStack.Push(startingColor);

            int p0 = 0;

            while (p0 < message.Length)
            {
                int p1 = p0;
                while (p1 < message.Length && message[p1] >= 32)
                {
                    p1++;
                }

                // text
                if (p1 != p0)
                {
                    output.Write(message.Substring(p0, p1 - p0));
                }

                if (p1 >= message.Length)
                {
                    p0 = p1;
                    break;
                }

                // control characters
                char c1 = message[p1];
                char c2 = (char)0;

                if (p1 + 1 < message.Length)
                {
                    c2 = message[p1 + 1];
                }

                if (c1 == '\a' && c2 == '\a')
                {
                    output.Write('\a');
                    p0 = p1 + 2;
                    continue;
                }

                if (c1 == '\r' || c1 == '\n')
                {
                    Console.ForegroundColor = defaultColor.ForegroundColor;
                    Console.BackgroundColor = defaultColor.BackgroundColor;
                    output.Write(c1);
                    Console.ForegroundColor = colorStack.Peek().ForegroundColor;
                    Console.BackgroundColor = colorStack.Peek().BackgroundColor;
                    p0 = p1 + 1;
                    continue;
                }

                if (c1 == '\a')
                {
                    if (c2 == 'X')
                    {
                        colorStack.Pop();
                        Console.ForegroundColor = colorStack.Peek().ForegroundColor;
                        Console.BackgroundColor = colorStack.Peek().BackgroundColor;
                        p0 = p1 + 2;
                        continue;
                    }

                    var foreground = (ConsoleOutputColor)(c2 - 'A');
                    var background = (ConsoleOutputColor)(message[p1 + 2] - 'A');

                    if (foreground != ConsoleOutputColor.NoChange)
                    {
                        Console.ForegroundColor = (ConsoleColor)foreground;
                    }

                    if (background != ConsoleOutputColor.NoChange)
                    {
                        Console.BackgroundColor = (ConsoleColor)background;
                    }

                    colorStack.Push(new ColorPair(Console.ForegroundColor, Console.BackgroundColor));
                    p0 = p1 + 3;
                    continue;
                }

                output.Write(c1);
                p0 = p1 + 1;
            }

            if (p0 < message.Length)
            {
                output.Write(message.Substring(p0));
            }
        }

        private void Output(LogEventInfo logEvent, string message)
        {
            ConsoleColor oldForegroundColor = Console.ForegroundColor;
            ConsoleColor oldBackgroundColor = Console.BackgroundColor;

            try
            {
                ConsoleRowHighlightingRule matchingRule = null;

                foreach (ConsoleRowHighlightingRule cr in this.RowHighlightingRules)
                {
                    if (cr.CheckCondition(logEvent))
                    {
                        matchingRule = cr;
                        break;
                    }
                }

                if (this.UseDefaultRowHighlightingRules && matchingRule == null)
                {
                    foreach (ConsoleRowHighlightingRule cr in defaultConsoleRowHighlightingRules)
                    {
                        if (cr.CheckCondition(logEvent))
                        {
                            matchingRule = cr;
                            break;
                        }
                    }
                }

                if (matchingRule == null)
                {
                    matchingRule = ConsoleRowHighlightingRule.Default;
                }

                if (matchingRule.ForegroundColor != ConsoleOutputColor.NoChange)
                {
                    Console.ForegroundColor = (ConsoleColor)matchingRule.ForegroundColor;
                }

                if (matchingRule.BackgroundColor != ConsoleOutputColor.NoChange)
                {
                    Console.BackgroundColor = (ConsoleColor)matchingRule.BackgroundColor;
                }

                message = message.Replace("\a", "\a\a");

                foreach (ConsoleWordHighlightingRule hl in this.WordHighlightingRules)
                {
                    message = hl.ReplaceWithEscapeSequences(message);
                }

                ColorizeEscapeSequences(this.ErrorStream ? Console.Error : Console.Out, message, new ColorPair(Console.ForegroundColor, Console.BackgroundColor), new ColorPair(oldForegroundColor, oldBackgroundColor));
            }
            finally
            {
                Console.ForegroundColor = oldForegroundColor;
                Console.BackgroundColor = oldBackgroundColor;
            }

            if (this.ErrorStream)
            {
                Console.Error.WriteLine();
            }
            else
            {
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Color pair (foreground and background).
        /// </summary>
        internal struct ColorPair
        {
            private readonly ConsoleColor foregroundColor;
            private readonly ConsoleColor backgroundColor;

            internal ColorPair(ConsoleColor foregroundColor, ConsoleColor backgroundColor)
            {
                this.foregroundColor = foregroundColor;
                this.backgroundColor = backgroundColor;
            }

            internal ConsoleColor BackgroundColor
            {
                get { return this.backgroundColor; }
            }

            internal ConsoleColor ForegroundColor
            {
                get { return this.foregroundColor; }
            }
        }
    }
}

#endif
