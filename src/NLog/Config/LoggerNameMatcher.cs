// 
// Copyright (c) 2004-2018 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.Config
{
    using System;
    using System.Text.RegularExpressions;

    abstract class LoggerNameMatcher
    {

        public static LoggerNameMatcher Create(string loggerNamePattern)
        {
            if (loggerNamePattern == null)
                return LoggerNameMatcher.None.Instance;

            int starPos1 = loggerNamePattern.IndexOf('*');
            int starPos2 = loggerNamePattern.IndexOf('*', starPos1 + 1);
            int questionPos = loggerNamePattern.IndexOf('?');
            if (starPos1 < 0 && questionPos < 0)
                return new LoggerNameMatcher.Equals(loggerNamePattern);
            if (loggerNamePattern == "*")
                return LoggerNameMatcher.All.Instance;
            if (questionPos < 0)
            {
                if (starPos1 == 0 && starPos2 == loggerNamePattern.Length - 1)
                    return new LoggerNameMatcher.Contains(loggerNamePattern);
                if (starPos2 < 0)
                {
                    if (starPos1 == 0)
                        return new LoggerNameMatcher.EndsWith(loggerNamePattern);
                    if (starPos1 == loggerNamePattern.Length - 1)
                        return new LoggerNameMatcher.StartsWith(loggerNamePattern);
                }
            }
            return new LoggerNameMatcher.MultiplePattern(loggerNamePattern);
        }

        public string Pattern { get; }

        protected readonly string _matchingArgument;
        private readonly string _toString;
        protected LoggerNameMatcher(string pattern, string matchingArgument)
        {
            Pattern = pattern;
            _matchingArgument = matchingArgument;
            _toString = "logNamePattern: (" + matchingArgument + ":" + GetType().Name + ")";
        }
        public override string ToString()
        {
            return _toString;
        }
        public abstract bool NameMatches(string loggerName);
        class None : LoggerNameMatcher
        {
            public static readonly None Instance = new None();
            private None() 
                : base(null, null)
            {

            }
            public override bool NameMatches(string loggerName)
            {
                return false;
            }
        }
        class All : LoggerNameMatcher
        {
            public static readonly All Instance = new All();
            private All() 
                : base("*", null) { }
            public override bool NameMatches(string loggerName)
            {
                return true;
            }
        }
        new class Equals : LoggerNameMatcher
        {
            public Equals(string pattern) 
                : base(pattern, pattern) { }
            public override bool NameMatches(string loggerName)
            {
                return loggerName.Equals(_matchingArgument, StringComparison.Ordinal);
            }
        }
        class StartsWith : LoggerNameMatcher
        {
            public StartsWith(string pattern) 
                : base(pattern, pattern.Substring(0, pattern.Length - 1)) { }
            public override bool NameMatches(string loggerName)
            {
                return loggerName.StartsWith(_matchingArgument, StringComparison.Ordinal);
            }
        }
        class EndsWith : LoggerNameMatcher
        {
            public EndsWith(string pattern) 
                : base(pattern, pattern.Substring(1)) { }
            public override bool NameMatches(string loggerName)
            {
                return loggerName.EndsWith(_matchingArgument, StringComparison.Ordinal);
            }
        }
        class Contains : LoggerNameMatcher
        {
            public Contains(string pattern) 
                : base(pattern, pattern.Substring(1, pattern.Length - 2)) { }
            public override bool NameMatches(string loggerName)
            {
                return loggerName.IndexOf(_matchingArgument, StringComparison.Ordinal) >= 0;
            }
        }
        class MultiplePattern : LoggerNameMatcher
        {
            private readonly Regex _regex;
            private static string getRegex(string wildcardsPattern)
            {
                return
                    '^' +
                    Regex.Escape(wildcardsPattern)
                        .Replace("\\*", ".*")
                        .Replace("\\?", ".")
                    + '$';
            }
            public MultiplePattern(string pattern) 
                : base(pattern, getRegex(pattern))
            {
                _regex = new Regex(_matchingArgument, RegexOptions.CultureInvariant);
            }
            public override bool NameMatches(string loggerName)
            {
                return _regex.IsMatch(loggerName);
            }
        }
        class RegexPattern : LoggerNameMatcher
        {
            private readonly Regex _regex;
            public RegexPattern(string pattern) 
                : base(pattern, pattern.Substring(1, pattern.Length - 2))
            {
                _regex = new Regex(_matchingArgument, RegexOptions.CultureInvariant);
            }
            public override bool NameMatches(string loggerName)
            {
                return _regex.IsMatch(loggerName);
            }
        }
    }
}
