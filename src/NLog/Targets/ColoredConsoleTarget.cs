// 
// Copyright (c) 2004-2020 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

#if !NETSTANDARD1_3

namespace NLog.Targets
{
    using System;
    using System.Text;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using NLog.Config;
    using NLog.Common;

    /// <summary>
    /// Writes log messages to the console with customizable coloring.
    /// </summary>
    /// <seealso href="https://github.com/nlog/nlog/wiki/ColoredConsole-target">Documentation on NLog Wiki</seealso>
    [Target("ColoredConsole")]
    public sealed class ColoredConsoleTarget : TargetWithLayoutHeaderAndFooter
    {
        /// <summary>
        /// Should logging being paused/stopped because of the race condition bug in Console.Writeline?
        /// </summary>
        /// <remarks>
        ///   Console.Out.Writeline / Console.Error.Writeline could throw 'IndexOutOfRangeException', which is a bug.
        /// See https://stackoverflow.com/questions/33915790/console-out-and-console-error-race-condition-error-in-a-windows-service-written
        /// and https://connect.microsoft.com/VisualStudio/feedback/details/2057284/console-out-probable-i-o-race-condition-issue-in-multi-threaded-windows-service
        ///
        /// Full error:
        ///   Error during session close: System.IndexOutOfRangeException: Probable I/ O race condition detected while copying memory.
        ///   The I/ O package is not thread safe by default.In multithreaded applications,
        ///   a stream must be accessed in a thread-safe way, such as a thread - safe wrapper returned by TextReader's or
        ///   TextWriter's Synchronized methods.This also applies to classes like StreamWriter and StreamReader.
        ///
        /// </remarks>
        private bool _pauseLogging;

        private bool _disableColors;

        private IColoredConsolePrinter _consolePrinter;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColoredConsoleTarget" /> class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message:withexception=true}</code>
        /// </remarks>
        public ColoredConsoleTarget()
        {
            WordHighlightingRules = new List<ConsoleWordHighlightingRule>();
            RowHighlightingRules = new List<ConsoleRowHighlightingRule>();
            UseDefaultRowHighlightingRules = true;
            _consolePrinter = CreateConsolePrinter(EnableAnsiOutput);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ColoredConsoleTarget" /> class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message:withexception=true}</code>
        /// </remarks>
        /// <param name="name">Name of the target.</param>
        public ColoredConsoleTarget(string name) : this()
        {
            Name = name;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the error stream (stderr) should be used instead of the output stream (stdout).
        /// </summary>
        /// <docgen category='Console Options' order='10' />
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
        /// The encoding for writing messages to the <see cref="Console"/>.
        ///  </summary>
        /// <remarks>Has side effect</remarks>
        /// <docgen category='Console Options' order='10' />
        public Encoding Encoding
        {
            get => ConsoleTargetHelper.GetConsoleOutputEncoding(_encoding, IsInitialized, _pauseLogging);
            set
            {
                if (ConsoleTargetHelper.SetConsoleOutputEncoding(value, IsInitialized, _pauseLogging))
                    _encoding = value;
            }
        }
        private Encoding _encoding;

        /// <summary>
        /// Gets or sets a value indicating whether to auto-check if the console is available.
        ///  - Disables console writing if Environment.UserInteractive = False (Windows Service)
        ///  - Disables console writing if Console Standard Input is not available (Non-Console-App)
        /// </summary>
        /// <docgen category='Console Options' order='10' />
        [DefaultValue(false)]
        public bool DetectConsoleAvailable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to auto-check if the console has been redirected to file
        ///   - Disables coloring logic when System.Console.IsOutputRedirected = true
        /// </summary>
        /// <docgen category='Console Options' order='11' />
        [DefaultValue(false)]
        public bool DetectOutputRedirected { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to auto-flush after <see cref="Console.WriteLine()"/>
        /// </summary>
        /// <remarks>
        /// Normally not required as standard Console.Out will have <see cref="StreamWriter.AutoFlush"/> = true, but not when pipe to file
        /// </remarks>
        /// <docgen category='Console Options' order='11' />
        [DefaultValue(false)]
        public bool AutoFlush { get; set; }

        /// <summary>
        /// Enables output using ANSI Color Codes
        /// </summary>
        /// <docgen category='Console Options' order='10' />
        [DefaultValue(false)]
        public bool EnableAnsiOutput { get; set; }

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
            _pauseLogging = false;
            _disableColors = false;

            if (DetectConsoleAvailable)
            {
                string reason;
                _pauseLogging = !ConsoleTargetHelper.IsConsoleAvailable(out reason);
                if (_pauseLogging)
                {
                    InternalLogger.Info("{0}: Console detected as turned off. Disable DetectConsoleAvailable to skip detection. Reason: {1}", this, reason);
                }
            }

            if (_encoding != null)
                ConsoleTargetHelper.SetConsoleOutputEncoding(_encoding, true, _pauseLogging);

#if !NET35 && !NET40
            if (DetectOutputRedirected)
            {
                try
                {
                    _disableColors = ErrorStream ? Console.IsErrorRedirected : Console.IsOutputRedirected;
                    if (_disableColors)
                    {
                        InternalLogger.Info("{0}: Console output is redirected so no colors. Disable DetectOutputRedirected to skip detection.", this);
                        if (!AutoFlush && GetOutput() is StreamWriter streamWriter && !streamWriter.AutoFlush)
                        {
                            AutoFlush = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    InternalLogger.Error(ex, "{0}: Failed checking if Console Output Redirected.", this);
                }
            }
#endif

            base.InitializeTarget();

            if (Header != null)
            {
                LogEventInfo lei = LogEventInfo.CreateNullEvent();
                WriteToOutput(lei, RenderLogEvent(Header, lei));
            }

            _consolePrinter = CreateConsolePrinter(EnableAnsiOutput);
        }

        private static IColoredConsolePrinter CreateConsolePrinter(bool enableAnsiOutput)
        {
            if (!enableAnsiOutput)
                return new ColoredConsoleSystemPrinter();
            else
                return new ColoredConsoleAnsiPrinter();
        }

        /// <summary>
        /// Closes the target and releases any unmanaged resources.
        /// </summary>
        protected override void CloseTarget()
        {
            if (Footer != null)
            {
                LogEventInfo lei = LogEventInfo.CreateNullEvent();
                WriteToOutput(lei, RenderLogEvent(Footer, lei));
            }
            ExplicitConsoleFlush();
            base.CloseTarget();
        }

        /// <inheritdoc />
        protected override void FlushAsync(AsyncContinuation asyncContinuation)
        {
            try
            {
                ExplicitConsoleFlush();
                base.FlushAsync(asyncContinuation);
            }
            catch (Exception ex)
            {
                asyncContinuation(ex);
            }
        }

        private void ExplicitConsoleFlush()
        {
            if (!_pauseLogging && !AutoFlush)
            {
                var output = GetOutput();
                output.Flush();
            }
        }

        /// <summary>
        /// Writes the specified log event to the console highlighting entries
        /// and words based on a set of defined rules.
        /// </summary>
        /// <param name="logEvent">Log event.</param>
        protected override void Write(LogEventInfo logEvent)
        {
            if (_pauseLogging)
            {
                //check early for performance (See also DetectConsoleAvailable)
                return;
            }

            WriteToOutput(logEvent, RenderLogEvent(Layout, logEvent));
        }

        private void WriteToOutput(LogEventInfo logEvent, string message)
        {
            try
            {
                WriteToOutputWithColor(logEvent, message ?? string.Empty);
            }
            catch (Exception ex) when (ex is OverflowException || ex is IndexOutOfRangeException || ex is ArgumentOutOfRangeException)
            {
                // This is a bug and will therefore stop the logging. For docs, see the PauseLogging property.
                _pauseLogging = true;
                InternalLogger.Warn(ex, "{0}: {1} has been thrown and this is probably due to a race condition." +
                                        "Logging to the console will be paused. Enable by reloading the config or re-initialize the targets", this, ex.GetType());
            }
        }

        private void WriteToOutputWithColor(LogEventInfo logEvent, string message)
        {
            string colorMessage = message;
            ConsoleColor? newForegroundColor = null;
            ConsoleColor? newBackgroundColor = null;

            if (!_disableColors)
            {
                var matchingRule = GetMatchingRowHighlightingRule(logEvent);
                if (WordHighlightingRules.Count > 0)
                {
                    colorMessage = GenerateColorEscapeSequences(logEvent, message);
                }

                newForegroundColor = matchingRule.ForegroundColor != ConsoleOutputColor.NoChange ? (ConsoleColor)matchingRule.ForegroundColor : default(ConsoleColor?);
                newBackgroundColor = matchingRule.BackgroundColor != ConsoleOutputColor.NoChange ? (ConsoleColor)matchingRule.BackgroundColor : default(ConsoleColor?);
            }

            var consoleStream = GetOutput();
            if (ReferenceEquals(colorMessage, message) && !newForegroundColor.HasValue && !newBackgroundColor.HasValue)
            {
                ConsoleTargetHelper.WriteLineThreadSafe(consoleStream, message, AutoFlush);
            }
            else
            {
                bool wordHighlighting = !ReferenceEquals(colorMessage, message);
                if (!wordHighlighting && message.IndexOf('\n') >= 0)
                {
                    wordHighlighting = true;    // Newlines requires additional handling when doing colors
                    colorMessage = EscapeColorCodes(message);
                }
                WriteToOutputWithPrinter(consoleStream, colorMessage, newForegroundColor, newBackgroundColor, wordHighlighting);
            }
        }

        private void WriteToOutputWithPrinter(TextWriter consoleStream, string colorMessage, ConsoleColor? newForegroundColor, ConsoleColor? newBackgroundColor, bool wordHighlighting)
        {
            using (var targetBuilder = ReusableLayoutBuilder.Allocate())
            {
                TextWriter consoleWriter = _consolePrinter.AcquireTextWriter(consoleStream, targetBuilder.Result);

                ConsoleColor? oldForegroundColor = null;
                ConsoleColor? oldBackgroundColor = null;

                try
                {
                    if (wordHighlighting)
                    {
                        oldForegroundColor = _consolePrinter.ChangeForegroundColor(consoleWriter, newForegroundColor);
                        oldBackgroundColor = _consolePrinter.ChangeBackgroundColor(consoleWriter, newBackgroundColor);
                        var rowForegroundColor = newForegroundColor ?? oldForegroundColor;
                        var rowBackgroundColor = newBackgroundColor ?? oldBackgroundColor;
                        ColorizeEscapeSequences(_consolePrinter, consoleWriter, colorMessage, oldForegroundColor, oldBackgroundColor, rowForegroundColor, rowBackgroundColor);
                        _consolePrinter.WriteLine(consoleWriter, string.Empty);
                    }
                    else
                    {
                        if (newForegroundColor.HasValue)
                        {
                            oldForegroundColor = _consolePrinter.ChangeForegroundColor(consoleWriter, newForegroundColor.Value);
                            if (oldForegroundColor == newForegroundColor)
                                oldForegroundColor = null;  // No color restore is needed
                        }
                        if (newBackgroundColor.HasValue)
                        {
                            oldBackgroundColor = _consolePrinter.ChangeBackgroundColor(consoleWriter, newBackgroundColor.Value);
                            if (oldBackgroundColor == newBackgroundColor)
                                oldBackgroundColor = null;  // No color restore is needed
                        }
                        _consolePrinter.WriteLine(consoleWriter, colorMessage);
                    }
                }
                finally
                {
                    _consolePrinter.ReleaseTextWriter(consoleWriter, consoleStream, oldForegroundColor, oldBackgroundColor, AutoFlush);
                }
            }
        }

        private ConsoleRowHighlightingRule GetMatchingRowHighlightingRule(LogEventInfo logEvent)
        {
            var matchingRule = GetMatchingRowHighlightingRule(RowHighlightingRules, logEvent);
            if (matchingRule == null && UseDefaultRowHighlightingRules)
            {
                matchingRule = GetMatchingRowHighlightingRule(_consolePrinter.DefaultConsoleRowHighlightingRules, logEvent);
            }
            return matchingRule ?? ConsoleRowHighlightingRule.Default;
        }

        private ConsoleRowHighlightingRule GetMatchingRowHighlightingRule(IList<ConsoleRowHighlightingRule> rules, LogEventInfo logEvent)
        {
            for (int i = 0; i < rules.Count; ++i)
            {
                var rule = rules[i];
                if (rule.CheckCondition(logEvent))
                    return rule;
            }
            return null;
        }

        private string GenerateColorEscapeSequences(LogEventInfo logEvent, string message)
        {
            if (string.IsNullOrEmpty(message))
                return message;

            message = EscapeColorCodes(message);

            using (var targetBuilder = ReusableLayoutBuilder.Allocate())
            {
                StringBuilder sb = targetBuilder.Result;

                for (int i = 0; i < WordHighlightingRules.Count; ++i)
                {
                    var hl = WordHighlightingRules[i];
                    var matches = hl.Matches(logEvent, message);
                    if (matches == null || matches.Count == 0)
                        continue;

                    if (sb != null)
                        sb.Length = 0;

                    int previousIndex = 0;
                    foreach (System.Text.RegularExpressions.Match match in matches)
                    {
                        sb = sb ?? new StringBuilder(message.Length + 5);
                        sb.Append(message, previousIndex, match.Index - previousIndex);

                        sb.Append('\a');
                        sb.Append((char)((int)hl.ForegroundColor + 'A'));
                        sb.Append((char)((int)hl.BackgroundColor + 'A'));
                        sb.Append(match.Value);
                        sb.Append('\a');
                        sb.Append('X');

                        previousIndex = match.Index + match.Length;
                    }

                    if (sb?.Length > 0)
                    {
                        sb.Append(message, previousIndex, message.Length - previousIndex);
                        message = sb.ToString();
                    }
                }
            }

            return message;
        }

        private static string EscapeColorCodes(string message)
        {
            if (message.IndexOf("\a", StringComparison.Ordinal) >= 0)
                message = message.Replace("\a", "\a\a");
            return message;
        }

        private static void ColorizeEscapeSequences(
            IColoredConsolePrinter consolePrinter,
            TextWriter consoleWriter,
            string message,
            ConsoleColor? defaultForegroundColor,
            ConsoleColor? defaultBackgroundColor,
            ConsoleColor? rowForegroundColor,
            ConsoleColor? rowBackgroundColor)
        {
            var colorStack = new Stack<KeyValuePair<ConsoleColor?, ConsoleColor?>>();

            colorStack.Push(new KeyValuePair<ConsoleColor?, ConsoleColor?>(rowForegroundColor, rowBackgroundColor));

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
                    consolePrinter.WriteSubString(consoleWriter, message, p0, p1);
                }

                if (p1 >= message.Length)
                {
                    p0 = p1;
                    break;
                }

                // control characters
                char c1 = message[p1];
                if (c1 == '\r' || c1 == '\n')
                {
                    // Newline control characters
                    var currentColorConfig = colorStack.Peek();
                    var resetForegroundColor = currentColorConfig.Key != defaultForegroundColor ? defaultForegroundColor : null;
                    var resetBackgroundColor = currentColorConfig.Value != defaultBackgroundColor ? defaultBackgroundColor : null;
                    consolePrinter.ResetDefaultColors(consoleWriter, resetForegroundColor, resetBackgroundColor);
                    if (p1 + 1 < message.Length && message[p1 + 1] == '\n')
                    {
                        consolePrinter.WriteSubString(consoleWriter, message, p1, p1 + 2);
                        p0 = p1 + 2;
                    }
                    else
                    {
                        consolePrinter.WriteChar(consoleWriter, c1);
                        p0 = p1 + 1;
                    }
                    consolePrinter.ChangeForegroundColor(consoleWriter, currentColorConfig.Key, defaultForegroundColor);
                    consolePrinter.ChangeBackgroundColor(consoleWriter, currentColorConfig.Value, defaultBackgroundColor);
                    continue;
                }

                if (c1 != '\a' || p1 + 1 >= message.Length)
                {
                    // Other control characters
                    consolePrinter.WriteChar(consoleWriter, c1);
                    p0 = p1 + 1;
                    continue;
                }

                // coloring control characters
                char c2 = message[p1 + 1];
                if (c2 == '\a')
                {
                    consolePrinter.WriteChar(consoleWriter, '\a');
                    p0 = p1 + 2;
                    continue;
                }

                if (c2 == 'X')
                {
                    var oldColorConfig = colorStack.Pop();
                    var newColorConfig = colorStack.Peek();
                    if (newColorConfig.Key != oldColorConfig.Key || newColorConfig.Value != oldColorConfig.Value)
                    {
                        if ((oldColorConfig.Key.HasValue && !newColorConfig.Key.HasValue) || (oldColorConfig.Value.HasValue && !newColorConfig.Value.HasValue))
                        {
                            consolePrinter.ResetDefaultColors(consoleWriter, defaultForegroundColor, defaultBackgroundColor);
                        }
                        consolePrinter.ChangeForegroundColor(consoleWriter, newColorConfig.Key, oldColorConfig.Key);
                        consolePrinter.ChangeBackgroundColor(consoleWriter, newColorConfig.Value, oldColorConfig.Value);
                    }
                    p0 = p1 + 2;
                    continue;
                }

                var currentForegroundColor = colorStack.Peek().Key;
                var currentBackgroundColor = colorStack.Peek().Value;

                var foreground = (ConsoleOutputColor)(c2 - 'A');
                var background = (ConsoleOutputColor)(message[p1 + 2] - 'A');

                if (foreground != ConsoleOutputColor.NoChange)
                {
                    currentForegroundColor = (ConsoleColor)foreground;
                    consolePrinter.ChangeForegroundColor(consoleWriter, currentForegroundColor);
                }

                if (background != ConsoleOutputColor.NoChange)
                {
                    currentBackgroundColor = (ConsoleColor)background;
                    consolePrinter.ChangeBackgroundColor(consoleWriter, currentBackgroundColor);
                }

                colorStack.Push(new KeyValuePair<ConsoleColor?, ConsoleColor?>(currentForegroundColor, currentBackgroundColor));
                p0 = p1 + 3;
            }

            if (p0 < message.Length)
            {
                consolePrinter.WriteSubString(consoleWriter, message, p0, message.Length);
            }
        }

        private TextWriter GetOutput()
        {
            return ErrorStream ? Console.Error : Console.Out;
        }
    }
}

#endif