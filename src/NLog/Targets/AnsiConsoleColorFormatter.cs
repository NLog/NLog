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
    internal class AnsiConsoleColorFormatter
    {
        private string message;
        private ConsoleRowHighlightingRule matchingRule;
        private IList<ConsoleWordHighlightingRule> wordHighlightingRules;
        
        internal AnsiConsoleColorFormatter(string message, ConsoleRowHighlightingRule matchingRule, IList<ConsoleWordHighlightingRule> wordHighlightingRules)
        {
            this.message = message;
            this.matchingRule = matchingRule;
            this. wordHighlightingRules = wordHighlightingRules;                        
        }
        
        internal string FormatRow()
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
        
        internal string ApplyWordHighlightingRules()
        {
            var matches = BuildMatchList();
            
            int id = 1;
            foreach (ConsoleWordHighlightingRule hl in wordHighlightingRules)
            {
                message = hl.Replace(message, m => 
                { 
                    var currentMatch = matches.First(hw => hw.Id == id);
                    var matchBelowEndOfCurrentMatch = FindMatchBelowTheEndOfTheCurrentMatch(matches, currentMatch);
                        
                    id++;
                    return AnsiConsoleColorFormatter.FormatWord(m.Value,
                            hl.ForegroundColor, hl.BackgroundColor, 
                            matchBelowEndOfCurrentMatch != null ? matchBelowEndOfCurrentMatch.ForegroundColor : matchingRule.ForegroundColor, 
                            matchBelowEndOfCurrentMatch != null ? matchBelowEndOfCurrentMatch.BackgroundColor : matchingRule.BackgroundColor);
                });
            }

            return message;
        }
        
        private List<HighlightedMatch> BuildMatchList()
        {
            var matchResults = new List<HighlightedMatch>();
            int layer = 1;
            int id = 1;
            foreach (ConsoleWordHighlightingRule hl in wordHighlightingRules)
            {
                var matches = hl.Matches(message);
                if (matches == null)
                    continue;
                
                foreach (Match m in matches)
                {
                    matchResults.Add(new HighlightedMatch{Id = id, Layer = layer, Start = m.Index, End = m.Index + m.Length, ForegroundColor = hl.ForegroundColor, BackgroundColor = hl.BackgroundColor});
                    id++;
                }
                layer++;
            }
            return matchResults;
        }
        
        private static HighlightedMatch FindMatchBelowTheEndOfTheCurrentMatch(List<HighlightedMatch> matches, HighlightedMatch currentMatch)
        {
            return matches.FindAll(hw => hw.Layer < currentMatch.Layer &&
                                   hw.Start < currentMatch.End && 
                                   hw.End > currentMatch.End)
                          .OrderByDescending(o => o.Layer)
                          .FirstOrDefault();
        }
        
        private class HighlightedMatch
        {
            public int Id;
            public int Layer;
            public int Start;
            public int End;
            public ConsoleOutputColor ForegroundColor;
            public ConsoleOutputColor BackgroundColor;
        }
        
        internal static string FormatWord(string word, ConsoleOutputColor matchForegroundColor, ConsoleOutputColor matchBackgroundColor, 
                                   ConsoleOutputColor nextForegroundColor, ConsoleOutputColor nextBackgroundColor)
        {
            var builder = new StringBuilder(5);

            if (matchBackgroundColor != ConsoleOutputColor.NoChange)
                builder.Append(AnsiConsoleColor.GetBackgroundColorEscapeCode((ConsoleColor)matchBackgroundColor));
            if (matchForegroundColor != ConsoleOutputColor.NoChange)
                builder.Append(AnsiConsoleColor.GetForegroundColorEscapeCode((ConsoleColor)matchForegroundColor));

            builder.Append(word);
            
            if (matchForegroundColor != ConsoleOutputColor.NoChange)
                if (nextForegroundColor != ConsoleOutputColor.NoChange)
                    builder.Append(AnsiConsoleColor.GetForegroundColorEscapeCode((ConsoleColor)nextForegroundColor));
                else
                    builder.Append(AnsiConsoleColor.GetTerminalDefaultForegroundColorEscapeCode());
            
            if (matchBackgroundColor != ConsoleOutputColor.NoChange)
                if (nextBackgroundColor != ConsoleOutputColor.NoChange)
                    builder.Append(AnsiConsoleColor.GetBackgroundColorEscapeCode((ConsoleColor)nextBackgroundColor));
                else
                    builder.Append(AnsiConsoleColor.GetTerminalDefaultBackgroundColorEscapeCode());

            return builder.ToString();
        }
    }
}

#endif