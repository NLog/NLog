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
using System.IO;
using System.Runtime.InteropServices;

using NLog.Config;
using NLog.Targets;
using NLog.Win32.Targets;

namespace NLog.Win32.Targets
{
    /// <summary>
    /// Writes logging messages to the console with customizable coloring.
    /// </summary>
    /// <example>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" src="examples/targets/Configuration File/ColoredConsole/Simple/NLog.config" />
    /// <p>
    /// This assumes just one target and a single rule. More configuration
    /// options are described <a href="config.html">here</a>.
    /// </p>
    /// <p>
    /// To set up the log target programmatically use code like this:
    /// </p>
    /// <code lang="C#" src="examples/targets/Configuration API/ColoredConsole/Simple/Example.cs" />
    /// <p>
    /// The result is a colorful console, where each color represents a single log level.
    /// </p>
    /// <img src="examples/targets/Screenshots/ColoredConsole/Simple.gif" />
    /// <p>
    /// In addition you can configure your own word highlighting rules so that 
    /// particular words or regular expressions will be marked with 
    /// a distinguished color:
    /// </p>
    /// <code lang="XML" src="examples/targets/Configuration File/ColoredConsole/Word Highlighting/NLog.config" />
    /// <p>Programmatic equivalent of the above configuration:</p>
    /// <code lang="C#" src="examples/targets/Configuration API/ColoredConsole/Word Highlighting/Example.cs" />
    /// <p>
    /// Here's the result:
    /// </p>
    /// <img src="examples/targets/Screenshots/ColoredConsole/Word Highlighting.gif" />
    /// <p>
    /// Custom row highlighting lets you colorize the output by any <a href="conditions.html">condition</a>.
    /// This example shows how to mark all entries containing the word "serious" with white color on red background
    /// and mark all entries coming from a particular logger with yellow on blue.
    /// </p>
    /// <code lang="XML" src="examples/targets/Configuration File/ColoredConsole/Row Highlighting/NLog.config" />
    /// <p>Programmatic equivalent of the above configuration:</p>
    /// <code lang="C#" src="examples/targets/Configuration API/ColoredConsole/Row Highlighting/Example.cs" />
    /// <p>
    /// Here's the result:
    /// </p>
    /// <img src="examples/targets/Screenshots/ColoredConsole/Row Highlighting.gif" />
    /// </example>
    [Target("ColoredConsole")]
    [SupportedRuntime(OS=RuntimeOS.Windows)]
    [SupportedRuntime(OS=RuntimeOS.WindowsNT)]
    [NotSupportedRuntime(Framework=RuntimeFramework.DotNetCompactFramework)]
    public sealed class ColoredConsoleTarget: TargetWithLayoutHeaderAndFooter
    {
        private bool _errorStream = false;
        private bool _useDefaultRowHighlightingRules = true;
        private ConsoleRowHighlightingRuleCollection _ConsoleRowHighlightingRules = new ConsoleRowHighlightingRuleCollection();
        private ConsoleWordHighlightingRuleCollection _consoleWordHighlightingRules = new ConsoleWordHighlightingRuleCollection();
        private static ConsoleRowHighlightingRuleCollection _defaultConsoleRowHighlightingRules = new ConsoleRowHighlightingRuleCollection();
        private ushort[] _colorStack = new ushort[10];

        static ColoredConsoleTarget()
        {
            _defaultConsoleRowHighlightingRules.Add(new ConsoleRowHighlightingRule("level == LogLevel.Fatal", ConsoleOutputColor.Red, ConsoleOutputColor.NoChange));
            _defaultConsoleRowHighlightingRules.Add(new ConsoleRowHighlightingRule("level == LogLevel.Error", ConsoleOutputColor.Yellow, ConsoleOutputColor.NoChange));
            _defaultConsoleRowHighlightingRules.Add(new ConsoleRowHighlightingRule("level == LogLevel.Warn", ConsoleOutputColor.Magenta, ConsoleOutputColor.NoChange));
            _defaultConsoleRowHighlightingRules.Add(new ConsoleRowHighlightingRule("level == LogLevel.Info", ConsoleOutputColor.White, ConsoleOutputColor.NoChange));
            _defaultConsoleRowHighlightingRules.Add(new ConsoleRowHighlightingRule("level == LogLevel.Debug", ConsoleOutputColor.Gray, ConsoleOutputColor.NoChange));
            _defaultConsoleRowHighlightingRules.Add(new ConsoleRowHighlightingRule("level == LogLevel.Trace", ConsoleOutputColor.DarkGray, ConsoleOutputColor.NoChange));
        }

        /// <summary>
        /// Determines whether the error stream (stderr) should be used instead of the output stream (stdout).
        /// </summary>
        [System.ComponentModel.DefaultValue(false)]
        public bool ErrorStream
        {
            get { return _errorStream; }
            set { _errorStream = value; }
        }

        /// <summary>
        /// Use default row highlighting rules.
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
        [System.ComponentModel.DefaultValue(true)]
        public bool UseDefaultRowHighlightingRules
        {
            get { return _useDefaultRowHighlightingRules; }
            set { _useDefaultRowHighlightingRules = value; }
        }

        /// <summary>
        /// Row highlighting rules.
        /// </summary>
        [ArrayParameter(typeof(ConsoleRowHighlightingRule), "highlight-row")]
        public ConsoleRowHighlightingRuleCollection RowHighlightingRules
        {
            get { return _ConsoleRowHighlightingRules; }
        }

        /// <summary>
        /// Word highlighting rules.
        /// </summary>
        [ArrayParameter(typeof(ConsoleWordHighlightingRule), "highlight-word")]
        public ConsoleWordHighlightingRuleCollection WordHighlightingRules
        {
            get { return _consoleWordHighlightingRules; }
        }

        /// <summary>
        /// Writes the specified log event to the console highlighting entries
        /// and words based on a set of defined rules.
        /// </summary>
        /// <param name="logEvent">Log event.</param>
        protected internal override void Write(LogEventInfo logEvent)
        {
            Output(logEvent, CompiledLayout.GetFormattedMessage(logEvent));
        }

        private void Output(LogEventInfo logEvent, string message)
        {
            IntPtr hConsole = ConsoleWin32Api.GetStdHandle(ErrorStream ? ConsoleWin32Api.STD_ERROR_HANDLE : ConsoleWin32Api.STD_OUTPUT_HANDLE);

            ConsoleWin32Api.CONSOLE_SCREEN_BUFFER_INFO csbi;
            ConsoleWin32Api.GetConsoleScreenBufferInfo(hConsole, out csbi);

            ConsoleRowHighlightingRule matchingRule = null;

            foreach (ConsoleRowHighlightingRule cr in RowHighlightingRules)
            {
                if (cr.CheckCondition(logEvent))
                {
                    matchingRule = cr;
                    break;
                }
            }

            if (UseDefaultRowHighlightingRules && matchingRule == null)
            {
                foreach (ConsoleRowHighlightingRule cr in _defaultConsoleRowHighlightingRules)
                {
                    if (cr.CheckCondition(logEvent))
                    {
                        matchingRule = cr;
                        break;
                    }
                }
            }

            if (matchingRule == null)
                matchingRule = ConsoleRowHighlightingRule.Default;

            ushort newColor = ColorFromForegroundAndBackground(csbi.wAttributes, matchingRule.ForegroundColor, matchingRule.BackgroundColor);

            message = message.Replace("\a","\a\a");

            foreach (ConsoleWordHighlightingRule hl in WordHighlightingRules)
            {
                message = hl.ReplaceWithEscapeSequences(message);
            }

            if (ErrorStream)
                ColorizeEscapeSequences(Console.Error, hConsole, message, newColor, csbi.wAttributes);
            else
                ColorizeEscapeSequences(Console.Out, hConsole, message, newColor, csbi.wAttributes);
            
            ConsoleWin32Api.CONSOLE_SCREEN_BUFFER_INFO csbi2;
            ConsoleWin32Api.GetConsoleScreenBufferInfo(hConsole, out csbi2);
            ConsoleWin32Api.SetConsoleTextAttribute(hConsole, csbi.wAttributes);

            int xsize = csbi2.dwSize.x;
            int xpos = csbi2.dwCursorPosition.x;
            uint written = 0;
            ConsoleWin32Api.FillConsoleOutputAttribute(hConsole, newColor, xsize - xpos, csbi2.dwCursorPosition, out written);
            ConsoleWin32Api.SetConsoleTextAttribute(hConsole, csbi.wAttributes);
            if (ErrorStream)
                Console.Error.WriteLine();
            else
                Console.WriteLine();
        }

        private void ColorizeEscapeSequences(TextWriter output, IntPtr hConsole, string message, ushort startingAttribute, ushort defaultAttribute)
        {
            lock (this)
            {
                int colorStackLength = 0;
                _colorStack[colorStackLength++] = startingAttribute;

                ConsoleWin32Api.SetConsoleTextAttribute(hConsole, startingAttribute);

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
                        c2 = message[p1 + 1];

                    if (c1 == '\a' && c2 == '\a')
                    {
                        output.Write('\a');
                        p0 = p1 + 2;
                        continue;
                    }
                    if (c1 == '\r' || c1 == '\n')
                    {
                        ConsoleWin32Api.SetConsoleTextAttribute(hConsole, defaultAttribute);
                        output.Write(c1);
                        ConsoleWin32Api.SetConsoleTextAttribute(hConsole, _colorStack[colorStackLength - 1]);
                        p0 = p1 + 1;
                        continue;
                    }
                    if (c1 == '\a')
                    {
                        if (c2 == 'X')
                        {
                            colorStackLength--;
                            ConsoleWin32Api.SetConsoleTextAttribute(hConsole, _colorStack[colorStackLength - 1]);
                            p0 = p1 + 2;
                            continue;
                        }
                        else
                        {
                            ConsoleOutputColor foreground = (ConsoleOutputColor)(int)(c2 - 'A');
                            ConsoleOutputColor background = (ConsoleOutputColor)(int)(message[p1 + 2] - 'A');
                            ushort newColor = ColorFromForegroundAndBackground(
                                _colorStack[colorStackLength - 1], foreground, background);

                            _colorStack[colorStackLength++] = newColor;
                            ConsoleWin32Api.SetConsoleTextAttribute(hConsole, newColor);
                            p0 = p1 + 3;
                            continue;
                        }
                    }

                    output.Write(c1);
                    p0 = p1 + 1;
                }
                if (p0 < message.Length)
                    output.Write(message.Substring(p0));
            }
        }

        private ushort ColorFromForegroundAndBackground(ushort current, ConsoleOutputColor foregroundColor, ConsoleOutputColor backgroundColor)
        {
            ushort newColor = current;

            if (foregroundColor != ConsoleOutputColor.NoChange)
            {
                newColor = (ushort)(newColor & ~0xF);
                newColor |= (ushort )foregroundColor;
            }

            if (backgroundColor != ConsoleOutputColor.NoChange)
            {
                newColor = (ushort)(newColor & ~0xF0);
                newColor |= (ushort)(((int)backgroundColor) << 4);
            }

            return newColor;
        }

        /// <summary>
        /// Initializes the target.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            if (CompiledHeader != null)
            {
                LogEventInfo lei = LogEventInfo.CreateNullEvent();
                Output(lei, CompiledHeader.GetFormattedMessage(lei));
            }
        }

        /// <summary>
        /// Closes the target and releases any unmanaged resources.
        /// </summary>
        protected internal override void Close()
        {
            if (CompiledFooter != null)
            {
                LogEventInfo lei = LogEventInfo.CreateNullEvent();
                Output(lei, CompiledFooter.GetFormattedMessage(lei));
            }
            base.Close();
        }
    }
}

#endif
