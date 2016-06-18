// 
// Copyright (c) 2004-2016 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

#if !SILVERLIGHT && !__IOS__ && !__ANDROID__ && !UWP10 || NETSTANDARD1_3

namespace NLog.Targets
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Colorizes console output using Console.ForegroundColor and Console.BackgroundColor.
    /// </summary>
    internal class ConsoleColorizer
    {
        private TextWriter consoleStream;
        private string message;
        private ConsoleRowHighlightingRule rowHighlightingRule;
        private IList<ConsoleWordHighlightingRule> wordHighlightingRules;
        
        internal ConsoleColorizer(TextWriter consoleStream, string message, ConsoleRowHighlightingRule rowHighlightingRule, IList<ConsoleWordHighlightingRule> wordHighlightingRules)
        {
            if (rowHighlightingRule == null)
                throw new ArgumentNullException("rowHighlightingRule");
            if (wordHighlightingRules == null)
                throw new ArgumentNullException("wordHighlightingRules");

            this.consoleStream = consoleStream;
            this.message = message;
            this.rowHighlightingRule = rowHighlightingRule;
            this.wordHighlightingRules = wordHighlightingRules;
        }
        
        internal void ColorizeMessage()
        {
            ConsoleColor oldForegroundColor = Console.ForegroundColor;
            ConsoleColor oldBackgroundColor = Console.BackgroundColor;
            bool didChangeForegroundColor = false, didChangeBackgroundColor = false;

            try
            {
                didChangeForegroundColor = IsColorChange(rowHighlightingRule.ForegroundColor, oldForegroundColor);
                if (didChangeForegroundColor)
                    Console.ForegroundColor = (ConsoleColor)rowHighlightingRule.ForegroundColor;

                didChangeBackgroundColor = IsColorChange(rowHighlightingRule.BackgroundColor, oldBackgroundColor);
                if (didChangeBackgroundColor)
                    Console.BackgroundColor = (ConsoleColor)rowHighlightingRule.BackgroundColor;

                if (wordHighlightingRules.Count == 0)
                {
                    consoleStream.WriteLine(message);
                }
                else
                {
                    message = message.Replace("\a", "\a\a");
                    foreach (ConsoleWordHighlightingRule hl in wordHighlightingRules)
                    {
                        message = hl.ReplaceWithEscapeSequences(message);
                    }

                    ColorizeEscapeSequences(consoleStream, message, new ColorPair(Console.ForegroundColor, Console.BackgroundColor), new ColorPair(oldForegroundColor, oldBackgroundColor));
                    consoleStream.WriteLine();

                    didChangeForegroundColor = didChangeBackgroundColor = true;
                }
            }
            finally
            {
                if (didChangeForegroundColor)
                    Console.ForegroundColor = oldForegroundColor;
                if (didChangeBackgroundColor)
                    Console.BackgroundColor = oldBackgroundColor;
            }
        }

        private static bool IsColorChange(ConsoleOutputColor targetColor, ConsoleColor oldColor)
        {
            return (targetColor != ConsoleOutputColor.NoChange) && ((ConsoleColor)targetColor != oldColor);
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