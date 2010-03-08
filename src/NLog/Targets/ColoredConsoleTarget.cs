// 
// Copyright (c) 2004-2010 Jaroslaw Kowalski <jaak@jkowalski.net>
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using NLog.Config;
using NLog.Internal;

namespace NLog.Targets
{
    /// <summary>
    /// Writes logging messages to the console with customizable coloring.
    /// </summary>
    /// <example>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" source="examples/targets/Configuration File/ColoredConsole/Simple/NLog.config" />
    /// <p>
    /// This assumes just one target and a single rule. More configuration
    /// options are described <a href="config.html">here</a>.
    /// </p>
    /// <p>
    /// To set up the log target programmatically use code like this:
    /// </p>
    /// <code lang="C#" source="examples/targets/Configuration API/ColoredConsole/Simple/Example.cs" />
    /// <p>
    /// The result is a colorful console, where each color represents a single log level.
    /// </p>
    /// <img src="examples/targets/Screenshots/ColoredConsole/Simple.gif" />
    /// <p>
    /// In addition you can configure your own word highlighting rules so that 
    /// particular words or regular expressions will be marked with 
    /// a distinguished color:
    /// </p>
    /// <code lang="XML" source="examples/targets/Configuration File/ColoredConsole/Word Highlighting/NLog.config" />
    /// <p>Programmatic equivalent of the above configuration:</p>
    /// <code lang="C#" source="examples/targets/Configuration API/ColoredConsole/Word Highlighting/Example.cs" />
    /// <p>
    /// Here's the result:
    /// </p>
    /// <img src="examples/targets/Screenshots/ColoredConsole/Word Highlighting.gif" />
    /// <p>
    /// Custom row highlighting lets you colorize the output by any <a href="conditions.html">condition</a>.
    /// This example shows how to mark all entries containing the word "serious" with white color on red background
    /// and mark all entries coming from a particular logger with yellow on blue.
    /// </p>
    /// <code lang="XML" source="examples/targets/Configuration File/ColoredConsole/Row Highlighting/NLog.config" />
    /// <p>Programmatic equivalent of the above configuration:</p>
    /// <code lang="C#" source="examples/targets/Configuration API/ColoredConsole/Row Highlighting/Example.cs" />
    /// <p>
    /// Here's the result:
    /// </p>
    /// <img src="examples/targets/Screenshots/ColoredConsole/Row Highlighting.gif" />
    /// </example>
    [Target("ColoredConsole")]
    public sealed class ColoredConsoleTarget : TargetWithLayoutHeaderAndFooter
    {
        private static readonly ICollection<ConsoleRowHighlightingRule> defaultConsoleRowHighlightingRules = new List<ConsoleRowHighlightingRule>()
        {
            new ConsoleRowHighlightingRule("level == LogLevel.Fatal", ConsoleOutputColor.Red, ConsoleOutputColor.NoChange),
            new ConsoleRowHighlightingRule("level == LogLevel.Error", ConsoleOutputColor.Yellow, ConsoleOutputColor.NoChange),
            new ConsoleRowHighlightingRule("level == LogLevel.Warn", ConsoleOutputColor.Magenta, ConsoleOutputColor.NoChange),
            new ConsoleRowHighlightingRule("level == LogLevel.Info", ConsoleOutputColor.White, ConsoleOutputColor.NoChange),
            new ConsoleRowHighlightingRule("level == LogLevel.Debug", ConsoleOutputColor.Gray, ConsoleOutputColor.NoChange),
            new ConsoleRowHighlightingRule("level == LogLevel.Trace", ConsoleOutputColor.DarkGray, ConsoleOutputColor.NoChange),
        };

        private readonly ushort[] colorStack = new ushort[10];

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
        [DefaultValue(false)]
        public bool ErrorStream { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use default row highlighting rules.
        /// </summary>
        /// <remarks>
        /// The default rules are:
        /// <table class="subparamtable">
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
        [DefaultValue(true)]
        public bool UseDefaultRowHighlightingRules { get; set; }

        /// <summary>
        /// Gets the row highlighting rules.
        /// </summary>
        [ArrayParameter(typeof(ConsoleRowHighlightingRule), "highlight-row")]
        public ICollection<ConsoleRowHighlightingRule> RowHighlightingRules { get; private set; }

        /// <summary>
        /// Gets the word highlighting rules.
        /// </summary>
        [ArrayParameter(typeof(ConsoleWordHighlightingRule), "highlight-word")]
        public ICollection<ConsoleWordHighlightingRule> WordHighlightingRules { get; private set; }

        /// <summary>
        /// Initializes the target.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            if (Header != null)
            {
                LogEventInfo lei = LogEventInfo.CreateNullEvent();
                this.Output(lei, Header.GetFormattedMessage(lei));
            }
        }

        /// <summary>
        /// Closes the target and releases any unmanaged resources.
        /// </summary>
        public override void Close()
        {
            if (Footer != null)
            {
                LogEventInfo lei = LogEventInfo.CreateNullEvent();
                this.Output(lei, Footer.GetFormattedMessage(lei));
            }

            base.Close();
        }

            /// <summary>
        /// Writes the specified log event to the console highlighting entries
        /// and words based on a set of defined rules.
        /// </summary>
        /// <param name="logEvent">Log event.</param>
        protected override void Write(LogEventInfo logEvent)
        {
            this.Output(logEvent, this.Layout.GetFormattedMessage(logEvent));
        }

        private void Output(LogEventInfo logEvent, string message)
        {
            IntPtr consoleHandle = NativeMethods.GetStdHandle(this.ErrorStream ? NativeMethods.STD_ERROR_HANDLE : NativeMethods.STD_OUTPUT_HANDLE);

            NativeMethods.CONSOLE_BUFFER_INFO csbi;
            NativeMethods.GetConsoleScreenBufferInfo(consoleHandle, out csbi);

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

            ushort newColor = this.ColorFromForegroundAndBackground(csbi.wAttributes, matchingRule.ForegroundColor, matchingRule.BackgroundColor);

            message = message.Replace("\a", "\a\a");

            foreach (ConsoleWordHighlightingRule hl in this.WordHighlightingRules)
            {
                message = hl.ReplaceWithEscapeSequences(message);
            }

            if (this.ErrorStream)
            {
                this.ColorizeEscapeSequences(Console.Error, consoleHandle, message, newColor, csbi.wAttributes);
            }
            else
            {
                this.ColorizeEscapeSequences(Console.Out, consoleHandle, message, newColor, csbi.wAttributes);
            }

            NativeMethods.CONSOLE_BUFFER_INFO csbi2;
            NativeMethods.GetConsoleScreenBufferInfo(consoleHandle, out csbi2);
            NativeMethods.SetConsoleTextAttribute(consoleHandle, csbi.wAttributes);

            int xsize = csbi2.dwSize.x;
            int xpos = csbi2.dwCursorPosition.x;
            uint written = 0;
            NativeMethods.FillConsoleOutputAttribute(consoleHandle, newColor, xsize - xpos, csbi2.dwCursorPosition, out written);
            NativeMethods.SetConsoleTextAttribute(consoleHandle, csbi.wAttributes);
            if (this.ErrorStream)
            {
                Console.Error.WriteLine();
            }
            else
            {
                Console.WriteLine();
            }
        }

        private void ColorizeEscapeSequences(TextWriter output, IntPtr consoleHandle, string message, ushort startingAttribute, ushort defaultAttribute)
        {
            lock (this)
            {
                int colorStackLength = 0;
                this.colorStack[colorStackLength++] = startingAttribute;

                NativeMethods.SetConsoleTextAttribute(consoleHandle, startingAttribute);

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
                        NativeMethods.SetConsoleTextAttribute(consoleHandle, defaultAttribute);
                        output.Write(c1);
                        NativeMethods.SetConsoleTextAttribute(consoleHandle, this.colorStack[colorStackLength - 1]);
                        p0 = p1 + 1;
                        continue;
                    }

                    if (c1 == '\a')
                    {
                        if (c2 == 'X')
                        {
                            colorStackLength--;
                            NativeMethods.SetConsoleTextAttribute(consoleHandle, this.colorStack[colorStackLength - 1]);
                            p0 = p1 + 2;
                            continue;
                        }
                        else
                        {
                            ConsoleOutputColor foreground = (ConsoleOutputColor)(int)(c2 - 'A');
                            ConsoleOutputColor background = (ConsoleOutputColor)(int)(message[p1 + 2] - 'A');
                            ushort newColor = this.ColorFromForegroundAndBackground(
                                this.colorStack[colorStackLength - 1],
                                foreground,
                                background);

                            this.colorStack[colorStackLength++] = newColor;
                            NativeMethods.SetConsoleTextAttribute(consoleHandle, newColor);
                            p0 = p1 + 3;
                            continue;
                        }
                    }

                    output.Write(c1);
                    p0 = p1 + 1;
                }

                if (p0 < message.Length)
                {
                    output.Write(message.Substring(p0));
                }
            }
        }

        private ushort ColorFromForegroundAndBackground(ushort current, ConsoleOutputColor foregroundColor, ConsoleOutputColor backgroundColor)
        {
            ushort newColor = current;

            if (foregroundColor != ConsoleOutputColor.NoChange)
            {
                newColor = (ushort)(newColor & ~0xF);
                newColor |= (ushort)foregroundColor;
            }

            if (backgroundColor != ConsoleOutputColor.NoChange)
            {
                newColor = (ushort)(newColor & ~0xF0);
                newColor |= (ushort)(((int)backgroundColor) << 4);
            }

            return newColor;
        }
    }
}

#endif
