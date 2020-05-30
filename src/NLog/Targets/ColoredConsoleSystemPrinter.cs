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
    /// Color formatting for <see cref="ColoredConsoleTarget"/> using <see cref="Console.ForegroundColor"/>
    /// and <see cref="Console.BackgroundColor"/>
    /// </summary>
    internal class ColoredConsoleSystemPrinter : IColoredConsolePrinter
    {
        public TextWriter AcquireTextWriter(TextWriter consoleStream, StringBuilder reusableBuilder)
        {
            return consoleStream;
        }

        public void ReleaseTextWriter(TextWriter consoleWriter, TextWriter consoleStream, ConsoleColor? oldForegroundColor, ConsoleColor? oldBackgroundColor, bool flush)
        {
            ResetDefaultColors(consoleWriter, oldForegroundColor, oldBackgroundColor);
            if (flush)
                consoleWriter.Flush();
        }

        public ConsoleColor? ChangeForegroundColor(TextWriter consoleWriter, ConsoleColor? foregroundColor, ConsoleColor? oldForegroundColor = null)
        {
            var previousForegroundColor = oldForegroundColor ?? Console.ForegroundColor;
            if (foregroundColor.HasValue && previousForegroundColor != foregroundColor.Value)
            {
                Console.ForegroundColor = foregroundColor.Value;
            }
            return previousForegroundColor;
        }

        public ConsoleColor? ChangeBackgroundColor(TextWriter consoleWriter, ConsoleColor? backgroundColor, ConsoleColor? oldBackgroundColor = null)
        {
            var previousBackgroundColor = oldBackgroundColor ?? Console.BackgroundColor;
            if (backgroundColor.HasValue && previousBackgroundColor != backgroundColor.Value)
            {
                Console.BackgroundColor = backgroundColor.Value;
            }
            return previousBackgroundColor;
        }

        public void ResetDefaultColors(TextWriter consoleWriter, ConsoleColor? foregroundColor, ConsoleColor? backgroundColor)
        {
            if (foregroundColor.HasValue)
                Console.ForegroundColor = foregroundColor.Value;
            if (backgroundColor.HasValue)
                Console.BackgroundColor = backgroundColor.Value;
        }

        public void WriteSubString(TextWriter consoleWriter, string text, int index, int endIndex)
        {
            consoleWriter.Write(text.Substring(index, endIndex - index));
        }

        public void WriteChar(TextWriter consoleWriter, char text)
        {
            consoleWriter.Write(text);
        }

        public void WriteLine(TextWriter consoleWriter, string text)
        {
            consoleWriter.WriteLine(text);  // Cannot be threadsafe, since colors are incrementally updated
        }

        public IList<ConsoleRowHighlightingRule> DefaultConsoleRowHighlightingRules { get; } = new List<ConsoleRowHighlightingRule>()
        {
            new ConsoleRowHighlightingRule("level == LogLevel.Fatal", ConsoleOutputColor.Red, ConsoleOutputColor.NoChange),
            new ConsoleRowHighlightingRule("level == LogLevel.Error", ConsoleOutputColor.Yellow, ConsoleOutputColor.NoChange),
            new ConsoleRowHighlightingRule("level == LogLevel.Warn", ConsoleOutputColor.Magenta, ConsoleOutputColor.NoChange),
            new ConsoleRowHighlightingRule("level == LogLevel.Info", ConsoleOutputColor.White, ConsoleOutputColor.NoChange),
            new ConsoleRowHighlightingRule("level == LogLevel.Debug", ConsoleOutputColor.Gray, ConsoleOutputColor.NoChange),
            new ConsoleRowHighlightingRule("level == LogLevel.Trace", ConsoleOutputColor.DarkGray, ConsoleOutputColor.NoChange),
        };
    }
}

#endif