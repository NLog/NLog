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
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Color formatting for <see cref="ColoredConsoleTarget"/> using ANSI Color Codes
    /// </summary>
    internal class ColoredConsoleAnsiPrinter : IColoredConsolePrinter
    {
        public TextWriter AcquireTextWriter(TextWriter consoleStream, StringBuilder reusableBuilder)
        {
            // Writes into an in-memory console writer for sake of optimizations (Possible when not using system-colors)
            return new StringWriter(reusableBuilder ?? new StringBuilder(50), consoleStream.FormatProvider);
        }

        public void ReleaseTextWriter(TextWriter consoleWriter, TextWriter consoleStream, ConsoleColor? oldForegroundColor, ConsoleColor? oldBackgroundColor, bool flush)
        {
            // Flushes the in-memory console-writer to the actual console-stream
            var builder = (consoleWriter as StringWriter)?.GetStringBuilder();
            if (builder != null)
            {
                builder.Append(TerminalDefaultColorEscapeCode);
                ConsoleTargetHelper.WriteLineThreadSafe(consoleStream, builder.ToString(), flush);
            }
        }

        public ConsoleColor? ChangeForegroundColor(TextWriter consoleWriter, ConsoleColor? foregroundColor, ConsoleColor? oldForegroundColor = null)
        {
            if (foregroundColor.HasValue)
            {
                consoleWriter.Write(GetForegroundColorEscapeCode(foregroundColor.Value));
            }
            return null;    // There is no "old" console color
        }

        public ConsoleColor? ChangeBackgroundColor(TextWriter consoleWriter, ConsoleColor? backgroundColor, ConsoleColor? oldBackgroundColor = null)
        {
            if (backgroundColor.HasValue)
            {
                consoleWriter.Write(GetBackgroundColorEscapeCode(backgroundColor.Value));
            }
            return null;    // There is no "old" console color
        }

        public void ResetDefaultColors(TextWriter consoleWriter, ConsoleColor? foregroundColor, ConsoleColor? backgroundColor)
        {
            consoleWriter.Write(TerminalDefaultColorEscapeCode);
        }

        public void WriteSubString(TextWriter consoleWriter, string text, int index, int endIndex)
        {
            // No need to allocate SubString, because we are already writing in-memory
            for (int i = index; i < endIndex; ++i)
                consoleWriter.Write(text[i]);
        }

        public void WriteChar(TextWriter consoleWriter, char text)
        {
            consoleWriter.Write(text);
        }

        public void WriteLine(TextWriter consoleWriter, string text)
        {
            consoleWriter.Write(text);  // Newline is added when flushing to actual console-stream
        }

        /// <summary>
        /// Not using bold to get light colors, as it has to be cleared
        /// </summary>
        private static string GetForegroundColorEscapeCode(ConsoleColor color)
        {
            switch (color)
            {
                case ConsoleColor.Black:
                    return "\x1B[30m";
                case ConsoleColor.Blue:
                    return "\x1B[94m";
                case ConsoleColor.Cyan:
                    return "\x1B[96m";
                case ConsoleColor.DarkBlue:
                    return "\x1B[34m";
                case ConsoleColor.DarkCyan:
                    return "\x1B[36m";
                case ConsoleColor.DarkGray:
                    return "\x1B[90m";
                case ConsoleColor.DarkGreen:
                    return "\x1B[32m";
                case ConsoleColor.DarkMagenta:
                    return "\x1B[35m";
                case ConsoleColor.DarkRed:
                    return "\x1B[31m";
                case ConsoleColor.DarkYellow:
                    return "\x1B[33m";
                case ConsoleColor.Gray:
                    return "\x1B[37m";
                case ConsoleColor.Green:
                    return "\x1B[92m";
                case ConsoleColor.Magenta:
                    return "\x1B[95m";
                case ConsoleColor.Red:
                    return "\x1B[91m";
                case ConsoleColor.White:
                    return "\x1b[97m";
                case ConsoleColor.Yellow:
                    return "\x1B[93m";
                default:
                    return TerminalDefaultForegroundColorEscapeCode; // default foreground color
            }
        }

        private static string TerminalDefaultForegroundColorEscapeCode
        {
            get { return "\x1B[39m\x1B[22m"; }
        }

        /// <summary>
        /// Not using bold to get light colors, as it has to be cleared (And because it only works for text, and not background)
        /// </summary>
        private static string GetBackgroundColorEscapeCode(ConsoleColor color)
        {
            switch (color)
            {
                case ConsoleColor.Black:
                    return "\x1B[40m";
                case ConsoleColor.Blue:
                    return "\x1B[104m";
                case ConsoleColor.Cyan:
                    return "\x1B[106m";
                case ConsoleColor.DarkBlue:
                    return "\x1B[44m";
                case ConsoleColor.DarkCyan:
                    return "\x1B[46m";
                case ConsoleColor.DarkGray:
                    return "\x1B[100m";
                case ConsoleColor.DarkGreen:
                    return "\x1B[42m";
                case ConsoleColor.DarkMagenta:
                    return "\x1B[45m";
                case ConsoleColor.DarkRed:
                    return "\x1B[41m";
                case ConsoleColor.DarkYellow:
                    return "\x1B[43m";
                case ConsoleColor.Gray:
                    return "\x1B[47m";
                case ConsoleColor.Green:
                    return "\x1B[102m";
                case ConsoleColor.Magenta:
                    return "\x1B[105m";
                case ConsoleColor.Red:
                    return "\x1B[101m";
                case ConsoleColor.White:
                    return "\x1B[107m";
                case ConsoleColor.Yellow:
                    return "\x1B[103m";
                default:
                    return TerminalDefaultBackgroundColorEscapeCode;
            }
        }

        private static string TerminalDefaultBackgroundColorEscapeCode
        {
            get { return "\x1B[49m"; }
        }

        /// <summary>
        /// Resets both foreground and background color.
        /// </summary>
        private static string TerminalDefaultColorEscapeCode
        {
            get { return "\x1B[0m"; }
        }

        /// <summary>
        /// ANSI have 8 color-codes (30-37) by default. The "bright" (or "intense") color-codes (90-97) are extended values not supported by all terminals
        /// </summary>
        public IList<ConsoleRowHighlightingRule> DefaultConsoleRowHighlightingRules { get; } = new List<ConsoleRowHighlightingRule>()
        {
            new ConsoleRowHighlightingRule("level == LogLevel.Fatal", ConsoleOutputColor.DarkRed, ConsoleOutputColor.NoChange),
            new ConsoleRowHighlightingRule("level == LogLevel.Error", ConsoleOutputColor.DarkYellow, ConsoleOutputColor.NoChange),
            new ConsoleRowHighlightingRule("level == LogLevel.Warn", ConsoleOutputColor.DarkMagenta, ConsoleOutputColor.NoChange),
            new ConsoleRowHighlightingRule("level == LogLevel.Info", ConsoleOutputColor.NoChange, ConsoleOutputColor.NoChange),
            new ConsoleRowHighlightingRule("level == LogLevel.Debug", ConsoleOutputColor.NoChange, ConsoleOutputColor.NoChange),
            new ConsoleRowHighlightingRule("level == LogLevel.Trace", ConsoleOutputColor.NoChange, ConsoleOutputColor.NoChange),
        };
    }
}

#endif