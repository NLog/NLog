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
    using System.Text;
    using System.Collections.Generic;
    
    public static class AnsiConsoleColorFormatter
    {
        public static string FormatRow(string message, ConsoleRowHighlightingRule matchingRule)
        {
            StringBuilder builder = new StringBuilder(1);
            
            if (matchingRule.BackgroundColor != ConsoleOutputColor.NoChange)
                builder.Append(AnsiConsoleColor.GetBackgroundColorEscapeCode((ConsoleColor)matchingRule.BackgroundColor));
            if (matchingRule.ForegroundColor != ConsoleOutputColor.NoChange)
                builder.Append(AnsiConsoleColor.GetForegroundColorEscapeCode((ConsoleColor)matchingRule.ForegroundColor));
            
            builder.Append(message);
            
            if (matchingRule.ForegroundColor != ConsoleOutputColor.NoChange)
                builder.Append(AnsiConsoleColor.GetTerminalDefaultForegroundColorEscapeCode());
            if (matchingRule.BackgroundColor != ConsoleOutputColor.NoChange)
                builder.Append(AnsiConsoleColor.GetTerminalDefaultBackgroundColorEscapeCode());
            
            return builder.ToString();
        }
        
        public static string ApplyWordHighlightingRules(string message, ConsoleRowHighlightingRule matchingRule, IList<ConsoleWordHighlightingRule> wordHighlightingRules)
        {
            foreach (ConsoleWordHighlightingRule hl in wordHighlightingRules)
                    message = hl.Replace(message, 
                        m => AnsiConsoleColorFormatter.FormatWord(m.Value, matchingRule.ForegroundColor, matchingRule.BackgroundColor, 
                                                                  hl.ForegroundColor, hl.BackgroundColor));

            return message;
        }
        
        public static string FormatWord(string word, ConsoleOutputColor rowForegroundColor, ConsoleOutputColor rowBackgroundColor, 
                                   ConsoleOutputColor wordForegroundColor, ConsoleOutputColor wordBackgroundColor)
        {
            var builder = new StringBuilder(5);

            if (wordBackgroundColor != ConsoleOutputColor.NoChange)
                builder.Append(AnsiConsoleColor.GetBackgroundColorEscapeCode((ConsoleColor)wordBackgroundColor));
            if (wordForegroundColor != ConsoleOutputColor.NoChange)
                builder.Append(AnsiConsoleColor.GetForegroundColorEscapeCode((ConsoleColor)wordForegroundColor));

            builder.Append(word);
            
            if (wordForegroundColor != ConsoleOutputColor.NoChange)
                if (rowForegroundColor != ConsoleOutputColor.NoChange)
                    builder.Append(AnsiConsoleColor.GetForegroundColorEscapeCode((ConsoleColor)rowForegroundColor));
                else
                    builder.Append(AnsiConsoleColor.GetTerminalDefaultForegroundColorEscapeCode());
            
            if (wordBackgroundColor != ConsoleOutputColor.NoChange)
                if (rowBackgroundColor != ConsoleOutputColor.NoChange)
                    builder.Append(AnsiConsoleColor.GetBackgroundColorEscapeCode((ConsoleColor)rowBackgroundColor));
                else
                    builder.Append(AnsiConsoleColor.GetTerminalDefaultBackgroundColorEscapeCode());

            return builder.ToString();
        }
    }
}

#endif