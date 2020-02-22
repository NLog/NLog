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

namespace NLog.Config
{
    using System;
    using System.Text.RegularExpressions;
    using NLog.Common;

    /// <summary>
    /// Encapsulates <see cref="LoggingRule.LoggerNamePattern"/> and the logic to match the actual logger name
    /// All subclasses defines immutable objects.
    /// Concrete subclasses defines various matching rules through <see cref="LoggerNameMatcher.NameMatches(string)"/>
    /// </summary>
    abstract class LoggerNameMatcher
    {
        /// <summary>
        /// Creates a concrete <see cref="LoggerNameMatcher"/> based on <paramref name="loggerNamePattern"/>.
        /// </summary>
        /// <remarks>
        /// Rules used to select the concrete implementation returned:
        /// <list type="number">
        /// <item>if <paramref name="loggerNamePattern"/> is null => returns <see cref="NoneLoggerNameMatcher"/> (never matches)</item>
        /// <item>if <paramref name="loggerNamePattern"/> doesn't contains any '*' nor '?' => returns <see cref="EqualsLoggerNameMatcher"/> (matches only on case sensitive equals)</item>
        /// <item>if <paramref name="loggerNamePattern"/> == '*' => returns <see cref="AllLoggerNameMatcher"/> (always matches)</item>
        /// <item>if <paramref name="loggerNamePattern"/> doesn't contain '?'
        /// <list type="number">
        ///     <item>if <paramref name="loggerNamePattern"/> contains exactly 2 '*' one at the beginning and one at the end (i.e. "*foobar*) => returns <see cref="ContainsLoggerNameMatcher"/></item>
        ///     <item>if <paramref name="loggerNamePattern"/> contains exactly 1 '*' at the beginning (i.e. "*foobar") => returns <see cref="EndsWithLoggerNameMatcher"/></item>
        ///     <item>if <paramref name="loggerNamePattern"/> contains exactly 1 '*' at the end (i.e. "foobar*") => returns <see cref="StartsWithLoggerNameMatcher"/></item>
        /// </list>
        /// </item>
        /// <item>returns <see cref="MultiplePatternLoggerNameMatcher"/></item>
        /// </list>
        /// </remarks>
        /// <param name="loggerNamePattern">
        /// It may include one or more '*' or '?' wildcards at any position.
        /// <list type="bullet">
        /// <item>'*' means zero or more occurrences of any character</item>
        /// <item>'?' means exactly one occurrence of any character</item>
        /// </list>
        /// </param>
        /// <returns>A concrete <see cref="LoggerNameMatcher"/></returns>
        public static LoggerNameMatcher Create(string loggerNamePattern)
        {
            if (loggerNamePattern == null)
                return NoneLoggerNameMatcher.Instance;

            int starPos1 = loggerNamePattern.IndexOf('*');
            int starPos2 = loggerNamePattern.IndexOf('*', starPos1 + 1);
            int questionPos = loggerNamePattern.IndexOf('?');
            if (starPos1 < 0 && questionPos < 0)
                return new EqualsLoggerNameMatcher(loggerNamePattern);
            if (loggerNamePattern == "*")
                return AllLoggerNameMatcher.Instance;
            if (questionPos < 0)
            {
                if (starPos1 == 0 && starPos2 == loggerNamePattern.Length - 1)
                    return new ContainsLoggerNameMatcher(loggerNamePattern);
                if (starPos2 < 0)
                {
                    var loggerNameMatcher = CreateStartsOrEndsWithLoggerNameMatcher(loggerNamePattern, starPos1);
                    if (loggerNameMatcher != null)
                    {
                        return loggerNameMatcher;
                    }
                }
            }
            return new MultiplePatternLoggerNameMatcher(loggerNamePattern);
        }

        private static LoggerNameMatcher CreateStartsOrEndsWithLoggerNameMatcher(string loggerNamePattern, int starPos1)
        {
            if (starPos1 == 0)
                return new EndsWithLoggerNameMatcher(loggerNamePattern);
            if (starPos1 == loggerNamePattern.Length - 1)
                return new StartsWithLoggerNameMatcher(loggerNamePattern);
            return null;
        }

        /// <summary>
        /// Returns the argument passed to <see cref="LoggerNameMatcher.Create(string)"/>
        /// </summary>
        public string Pattern { get; }

        protected readonly string _matchingArgument;
        private readonly string _toString;
        protected LoggerNameMatcher(string pattern, string matchingArgument)
        {
            Pattern = pattern;
            _matchingArgument = matchingArgument;
            _toString = "logNamePattern: (" + matchingArgument + ":" + Name + ")";
        }
        public override string ToString()
        {
            return _toString;
        }
        protected abstract string Name { get; }

        /// <summary>
        /// Checks whether given name matches the logger name pattern.
        /// </summary>
        /// <param name="loggerName">String to be matched.</param>
        /// <returns>A value of <see langword="true"/> when the name matches, <see langword="false" /> otherwise.</returns>
        public abstract bool NameMatches(string loggerName);

        /// <summary>
        /// Defines a <see cref="LoggerNameMatcher"/> that never matches.
        /// Used when pattern is null
        /// </summary>
        class NoneLoggerNameMatcher : LoggerNameMatcher
        {
            protected override string Name => "None";
            public static readonly NoneLoggerNameMatcher Instance = new NoneLoggerNameMatcher();
            private NoneLoggerNameMatcher() 
                : base(null, null)
            {

            }
            public override bool NameMatches(string loggerName)
            {
                return false;
            }            
        }

        /// <summary>
        /// Defines a <see cref="LoggerNameMatcher"/> that always matches.
        /// Used when pattern is '*'
        /// </summary>
        class AllLoggerNameMatcher : LoggerNameMatcher
        {
            protected override string Name => "All";
            public static readonly AllLoggerNameMatcher Instance = new AllLoggerNameMatcher();
            private AllLoggerNameMatcher() 
                : base("*", null) { }
            public override bool NameMatches(string loggerName)
            {
                return true;
            }
        }

        /// <summary>
        /// Defines a <see cref="LoggerNameMatcher"/> that matches with a case-sensitive Equals
        /// Used when pattern is a string without wildcards '?' '*'
        /// </summary>
        class EqualsLoggerNameMatcher : LoggerNameMatcher
        {
            protected override string Name => "Equals";
            public EqualsLoggerNameMatcher(string pattern) 
                : base(pattern, pattern) { }
            public override bool NameMatches(string loggerName)
            {
                if (loggerName == null) return _matchingArgument == null;
                return loggerName.Equals(_matchingArgument, StringComparison.Ordinal);
            }
        }

        /// <summary>
        /// Defines a <see cref="LoggerNameMatcher"/> that matches with a case-sensitive StartsWith
        /// Used when pattern is a string like "*foobar"
        /// </summary>
        class StartsWithLoggerNameMatcher : LoggerNameMatcher
        {
            protected override string Name => "StartsWith";
            public StartsWithLoggerNameMatcher(string pattern) 
                : base(pattern, pattern.Substring(0, pattern.Length - 1)) { }
            public override bool NameMatches(string loggerName)
            {
                if (loggerName == null) return _matchingArgument == null;
                return loggerName.StartsWith(_matchingArgument, StringComparison.Ordinal);
            }
        }

        /// <summary>
        /// Defines a <see cref="LoggerNameMatcher"/> that matches with a case-sensitive EndsWith
        /// Used when pattern is a string like "foobar*"
        /// </summary>
        class EndsWithLoggerNameMatcher : LoggerNameMatcher
        {
            protected override string Name => "EndsWith";
            public EndsWithLoggerNameMatcher(string pattern) 
                : base(pattern, pattern.Substring(1)) { }
            public override bool NameMatches(string loggerName)
            {
                if (loggerName == null) return _matchingArgument == null;
                return loggerName.EndsWith(_matchingArgument, StringComparison.Ordinal);
            }
        }

        /// <summary>
        /// Defines a <see cref="LoggerNameMatcher"/> that matches with a case-sensitive Contains
        /// Used when pattern is a string like "*foobar*"
        /// </summary>
        class ContainsLoggerNameMatcher : LoggerNameMatcher
        {
            protected override string Name => "Contains";
            public ContainsLoggerNameMatcher(string pattern) 
                : base(pattern, pattern.Substring(1, pattern.Length - 2)) { }
            public override bool NameMatches(string loggerName)
            {
                if (loggerName == null) return _matchingArgument == null;
                return loggerName.IndexOf(_matchingArgument, StringComparison.Ordinal) >= 0;
            }
        }

        /// <summary>
        /// Defines a <see cref="LoggerNameMatcher"/> that matches with a complex wildcards combinations:
        /// <list type="bullet">
        /// <item>'*' means zero or more occurrences of any character</item>
        /// <item>'?' means exactly one occurrence of any character</item>
        /// </list>
        /// used when pattern is a string containing any number of '?' or '*' in any position
        /// i.e. "*Server[*].Connection[?]"
        /// </summary>
        class MultiplePatternLoggerNameMatcher : LoggerNameMatcher
        {
            protected override string Name => "MultiplePattern";
            private readonly Regex _regex;
            private static string ConvertToRegex(string wildcardsPattern)
            {
                return
                    '^' +
                    Regex.Escape(wildcardsPattern)
                        .Replace("\\*", ".*")
                        .Replace("\\?", ".")
                    + '$';
            }
            public MultiplePatternLoggerNameMatcher(string pattern) 
                : base(pattern, ConvertToRegex(pattern))
            {
                _regex = new Regex(_matchingArgument, RegexOptions.CultureInvariant);
            }
            public override bool NameMatches(string loggerName)
            {
                if (loggerName == null) return false;
                return _regex.IsMatch(loggerName);
            }
        }
    }
}
