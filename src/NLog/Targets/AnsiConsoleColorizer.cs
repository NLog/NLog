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
    using System.Text.RegularExpressions;
    using System.Linq;
    using System.IO;

    /// <summary>
    /// Colorizes console output using ansi escape codes.
    /// See https://en.wikipedia.org/wiki/ANSI_escape_code#Colors for background info.
    /// </summary>
    internal class AnsiConsoleColorizer : IConsoleColorizer
    {
	    private static readonly IList<ConsoleRowHighlightingRule> defaultConsoleRowHighlightingRules = new List<ConsoleRowHighlightingRule>()
        {
            new ConsoleRowHighlightingRule("level == LogLevel.Fatal", ConsoleOutputColor.DarkRed, ConsoleOutputColor.NoChange),
            new ConsoleRowHighlightingRule("level == LogLevel.Error", ConsoleOutputColor.DarkYellow, ConsoleOutputColor.NoChange),
            new ConsoleRowHighlightingRule("level == LogLevel.Warn", ConsoleOutputColor.DarkMagenta, ConsoleOutputColor.NoChange),
            new ConsoleRowHighlightingRule("level == LogLevel.Info", ConsoleOutputColor.DarkGreen, ConsoleOutputColor.NoChange),
            new ConsoleRowHighlightingRule("level == LogLevel.Debug", ConsoleOutputColor.Gray, ConsoleOutputColor.NoChange),
            new ConsoleRowHighlightingRule("level == LogLevel.Trace", ConsoleOutputColor.Gray, ConsoleOutputColor.NoChange),
        };

        private string message;
        private ConsoleRowHighlightingRule rowHighlightingRule = ConsoleRowHighlightingRule.Default;
        private IList<ConsoleWordHighlightingRule> wordHighlightingRules = new List<ConsoleWordHighlightingRule>(0);
        
        internal AnsiConsoleColorizer(string message)
        {
            this.message = message;
        }

        public IList<ConsoleRowHighlightingRule> DefaultConsoleRowHighlightingRules
        {
            get { return defaultConsoleRowHighlightingRules; }
        }

        public ConsoleRowHighlightingRule RowHighlightingRule
        {
            set { rowHighlightingRule = value; }
        }

        public IList<ConsoleWordHighlightingRule> WordHighlightingRules
        {
            set { wordHighlightingRules = value; }
        }
        
        public void ColorizeMessage(TextWriter consoleStream)
        {
            try{
                consoleStream.WriteLine(GetColorizedMessage());
            }
            catch
            {
                consoleStream.WriteLine(AnsiConsoleColor.TerminalDefaultColorEscapeCode);
                throw;
            }
        }

        private string GetColorizedMessage()
        {
            if (string.IsNullOrEmpty(message))
                return string.Empty;

            var formattedMessage = ColorizeRow();
                if (wordHighlightingRules.Count != 0)
                    formattedMessage = ApplyWordHighlightingRules();
                    
            return formattedMessage;
        }
        
        private string ColorizeRow()
        {
            StringBuilder builder = new StringBuilder(message.Length);
            
            AppendBackgroundColorEscapeCodeIfNeeded(builder, rowHighlightingRule.BackgroundColor);
            AppendForegroundColorEscapeCodeIfNeeded(builder, rowHighlightingRule.ForegroundColor);
            builder.Append(message);
            AppendColorResetEscapeCodeIfNeeded(builder, rowHighlightingRule.ForegroundColor, rowHighlightingRule.BackgroundColor);
        
            message = builder.ToString();
            return message;
        }

        /// <summary>
        /// Colorizing a word is very similar to colorize a row. We need to apply the foreground/background code of the match before the start and
        /// need to reset the color at the end of the match. Here comes the tricky part: We need to know to what color we have to reset to. This
        /// can either be the default code, the foreground/background code or the row rule or the foreground/background code of a match of a 
        /// different word rule. For the last one we need to know if there is a match from a word rule from a layer underneath the current level that 
        /// lays at the ending position of the current match. To do that we loop the matches two times. First build a list of matches with layer and id.      
        /// Then loop again, determine at each match what the crossing underneath match is and get its color using the id. Finally do the replace.
        /// </summary>
        private string ApplyWordHighlightingRules()
        {
            var matches = BuildMatchList();
            
            int id = 1;
            foreach (ConsoleWordHighlightingRule hl in wordHighlightingRules)
            {
                message = hl.Replace(message, m => 
                { 
                    var currentMatch = matches[id];
                    var matchBelowEndOfCurrentMatch = FindMatchBelowTheEndOfTheCurrentMatch(matches, currentMatch);
                        
                    id++;
                    return AnsiConsoleColorizer.ColorizeWord(m.Value,
                            hl.ForegroundColor, hl.BackgroundColor, 
                            matchBelowEndOfCurrentMatch.Key != 0 ? matchBelowEndOfCurrentMatch.Value.ForegroundColor : rowHighlightingRule.ForegroundColor, 
                            matchBelowEndOfCurrentMatch.Key != 0 ? matchBelowEndOfCurrentMatch.Value.BackgroundColor : rowHighlightingRule.BackgroundColor);
                });
            }

            return message;
        }
        
        private Dictionary<int, HighlightedMatch> BuildMatchList()
        {
            var matchResults = new Dictionary<int, HighlightedMatch>();
            int layer = 1;
            int id = 1;
            foreach (ConsoleWordHighlightingRule hl in wordHighlightingRules)
            {
                var matches = hl.Matches(message);
                if (matches == null)
                    continue;
                
                foreach (Match m in matches)
                {
                    matchResults.Add(id, new HighlightedMatch{Layer = layer, Start = m.Index, End = m.Index + m.Length, ForegroundColor = hl.ForegroundColor, BackgroundColor = hl.BackgroundColor});
                    id++;
                }
                layer++;
            }
            return matchResults;
        }
        
        private static KeyValuePair<int,HighlightedMatch> FindMatchBelowTheEndOfTheCurrentMatch(Dictionary<int, HighlightedMatch> matches, HighlightedMatch currentMatch)
        {
            return matches.Where(hm => hm.Value.Layer < currentMatch.Layer &&
                                   hm.Value.Start < currentMatch.End && 
                                   hm.Value.End > currentMatch.End)
                          .OrderByDescending(o => o.Value.Layer)
                          .FirstOrDefault();
        }
        
        private class HighlightedMatch
        {
            public int Layer;
            public int Start;
            public int End;
            public ConsoleOutputColor ForegroundColor;
            public ConsoleOutputColor BackgroundColor;
        }
        
        internal static string ColorizeWord(string word, ConsoleOutputColor matchForegroundColor, ConsoleOutputColor matchBackgroundColor, 
                                   ConsoleOutputColor nextForegroundColor, ConsoleOutputColor nextBackgroundColor)
        {
            var builder = new StringBuilder(word.Length);

            AppendBackgroundColorEscapeCodeIfNeeded(builder, matchBackgroundColor);
            AppendForegroundColorEscapeCodeIfNeeded(builder, matchForegroundColor);
            builder.Append(word);
            AppendColorResetEscapeCodeIfNeeded(builder, matchForegroundColor, matchBackgroundColor, nextForegroundColor, nextBackgroundColor);

            return builder.ToString();
        }

        private static void AppendBackgroundColorEscapeCodeIfNeeded(StringBuilder builder, ConsoleOutputColor backgroundColor)
        {
            if (backgroundColor != ConsoleOutputColor.NoChange)
                builder.Append(AnsiConsoleColor.GetBackgroundColorEscapeCode((ConsoleColor)backgroundColor));
        }

        private static void AppendForegroundColorEscapeCodeIfNeeded(StringBuilder builder, ConsoleOutputColor foregroundColor)
        {
            if (foregroundColor != ConsoleOutputColor.NoChange)
                builder.Append(AnsiConsoleColor.GetForegroundColorEscapeCode((ConsoleColor)foregroundColor));
        }

        private static void AppendColorResetEscapeCodeIfNeeded(StringBuilder builder, ConsoleOutputColor foregroundColor, ConsoleOutputColor backgroundColor)
        {
            if (foregroundColor != ConsoleOutputColor.NoChange && backgroundColor != ConsoleOutputColor.NoChange)
            {
                builder.Append(AnsiConsoleColor.TerminalDefaultColorEscapeCode);
                return;
            }

            AppendForegroundColorResetEscapeCodeIfNeeded(builder, foregroundColor);
            AppendBackgroundColorResetEscapeCodeIfNeeded(builder, backgroundColor);
        }

        private static void AppendForegroundColorResetEscapeCodeIfNeeded(StringBuilder builder, ConsoleOutputColor foregroundColor)
        {
            if (foregroundColor != ConsoleOutputColor.NoChange)
                builder.Append(AnsiConsoleColor.TerminalDefaultForegroundColorEscapeCode);
        }

        private static void AppendBackgroundColorResetEscapeCodeIfNeeded(StringBuilder builder, ConsoleOutputColor backgroundColor)
        {
            if (backgroundColor != ConsoleOutputColor.NoChange)
                builder.Append(AnsiConsoleColor.TerminalDefaultBackgroundColorEscapeCode);
        }

        private static void AppendColorResetEscapeCodeIfNeeded(StringBuilder builder, ConsoleOutputColor foregroundColor, ConsoleOutputColor backgroundColor, 
                                                                ConsoleOutputColor nextForegroundColor, ConsoleOutputColor nextBackgroundColor)
        {
            if (foregroundColor != ConsoleOutputColor.NoChange && backgroundColor != ConsoleOutputColor.NoChange
                && nextForegroundColor == ConsoleOutputColor.NoChange && nextBackgroundColor == ConsoleOutputColor.NoChange)
            {
                builder.Append(AnsiConsoleColor.TerminalDefaultColorEscapeCode);
                return;
            }
            
            if (foregroundColor != ConsoleOutputColor.NoChange)
                AppendForegroundColorOrResetEscapeCodeIfNeeded(builder, nextForegroundColor);
            
            if (backgroundColor != ConsoleOutputColor.NoChange)
                AppendBackgroundColorOrResetEscapeCodeIfNeeded(builder, nextBackgroundColor);
        }

        private static void AppendForegroundColorOrResetEscapeCodeIfNeeded(StringBuilder builder, ConsoleOutputColor foregroundColor)
        {
            if (foregroundColor != ConsoleOutputColor.NoChange)
                builder.Append(AnsiConsoleColor.GetForegroundColorEscapeCode((ConsoleColor)foregroundColor));
            else
                builder.Append(AnsiConsoleColor.TerminalDefaultForegroundColorEscapeCode);
        }

        private static void AppendBackgroundColorOrResetEscapeCodeIfNeeded(StringBuilder builder, ConsoleOutputColor backgroundColor)
        {
            if (backgroundColor != ConsoleOutputColor.NoChange)
                builder.Append(AnsiConsoleColor.GetBackgroundColorEscapeCode((ConsoleColor)backgroundColor));
            else
                builder.Append(AnsiConsoleColor.TerminalDefaultBackgroundColorEscapeCode);
        }
    }
}

#endif