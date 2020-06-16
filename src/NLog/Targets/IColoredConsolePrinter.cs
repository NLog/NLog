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
    /// Controls the text and color formatting for <see cref="ColoredConsoleTarget"/>
    /// </summary>
    internal interface IColoredConsolePrinter
    {
        /// <summary>
        /// Creates a TextWriter for the console to start building a colored text message
        /// </summary>
        /// <param name="consoleStream">Active console stream</param>
        /// <param name="reusableBuilder">Optional StringBuilder to optimize performance</param>
        /// <returns>TextWriter for the console</returns>
        TextWriter AcquireTextWriter(TextWriter consoleStream, StringBuilder reusableBuilder);

        /// <summary>
        /// Releases the TextWriter for the console after having built a colored text message (Restores console colors)
        /// </summary>
        /// <param name="consoleWriter">Colored TextWriter</param>
        /// <param name="consoleStream">Active console stream</param>
        /// <param name="oldForegroundColor">Original foreground color for console (If changed)</param>
        /// <param name="oldBackgroundColor">Original background color for console (If changed)</param>
        /// <param name="flush">Flush TextWriter</param>
        void ReleaseTextWriter(TextWriter consoleWriter, TextWriter consoleStream, ConsoleColor? oldForegroundColor, ConsoleColor? oldBackgroundColor, bool flush);

        /// <summary>
        /// Changes foreground color for the Colored TextWriter
        /// </summary>
        /// <param name="consoleWriter">Colored TextWriter</param>
        /// <param name="foregroundColor">New foreground color for the console</param>
        /// <param name="oldForegroundColor">Old previous backgroundColor color for the console</param>
        /// <returns>Old foreground color for the console</returns>
        ConsoleColor? ChangeForegroundColor(TextWriter consoleWriter, ConsoleColor? foregroundColor, ConsoleColor? oldForegroundColor = null);

        /// <summary>
        /// Changes backgroundColor color for the Colored TextWriter
        /// </summary>
        /// <param name="consoleWriter">Colored TextWriter</param>
        /// <param name="backgroundColor">New backgroundColor color for the console</param>
        /// <param name="oldBackgroundColor">Old previous backgroundColor color for the console</param>
        /// <returns>Old backgroundColor color for the console</returns>
        ConsoleColor? ChangeBackgroundColor(TextWriter consoleWriter, ConsoleColor? backgroundColor, ConsoleColor? oldBackgroundColor = null);

        /// <summary>
        /// Restores console colors back to their original state
        /// </summary>
        /// <param name="consoleWriter">Colored TextWriter</param>
        /// <param name="foregroundColor">Original foregroundColor color for the console</param>
        /// <param name="backgroundColor">Original backgroundColor color for the console</param>
        void ResetDefaultColors(TextWriter consoleWriter, ConsoleColor? foregroundColor, ConsoleColor? backgroundColor);

        /// <summary>
        /// Writes multiple characters to console in one operation (faster)
        /// </summary>
        /// <param name="consoleWriter">Colored TextWriter</param>
        /// <param name="text">Output Text</param>
        /// <param name="index">Start Index</param>
        /// <param name="endIndex">End Index</param>
        void WriteSubString(TextWriter consoleWriter, string text, int index, int endIndex);

        /// <summary>
        /// Writes single character to console
        /// </summary>
        /// <param name="consoleWriter">Colored TextWriter</param>
        /// <param name="text">Output Text</param>
        void WriteChar(TextWriter consoleWriter, char text);

        /// <summary>
        /// Writes whole string and completes with newline
        /// </summary>
        /// <param name="consoleWriter">Colored TextWriter</param>
        /// <param name="text">Output Text</param>
        void WriteLine(TextWriter consoleWriter, string text);

        /// <summary>
        /// Default row highlight rules for the console printer
        /// </summary>
        IList<ConsoleRowHighlightingRule> DefaultConsoleRowHighlightingRules { get; }
    }

}

#endif