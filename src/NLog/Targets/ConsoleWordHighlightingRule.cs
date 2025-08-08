//
// Copyright (c) 2004-2024 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.Targets
{
    using System;
    using System.Collections.Generic;
    using NLog.Conditions;
    using NLog.Config;
    using NLog.Internal;

    /// <summary>
    /// Highlighting rule for Win32 colorful console.
    /// </summary>
    [NLogConfigurationItem]
    public class ConsoleWordHighlightingRule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleWordHighlightingRule" /> class.
        /// </summary>
        public ConsoleWordHighlightingRule()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleWordHighlightingRule" /> class.
        /// </summary>
        /// <param name="text">The text to be matched..</param>
        /// <param name="foregroundColor">Color of the foreground.</param>
        /// <param name="backgroundColor">Color of the background.</param>
        public ConsoleWordHighlightingRule(string text, ConsoleOutputColor foregroundColor, ConsoleOutputColor backgroundColor)
        {
            Text = text;
            ForegroundColor = foregroundColor;
            BackgroundColor = backgroundColor;
        }

        /// <summary>
        /// Gets or sets the condition that must be met before scanning the row for highlight of words
        /// </summary>
        /// <remarks>Default: <see langword="null"/></remarks>
        /// <docgen category='Highlighting Rules' order='10' />
        public ConditionExpression? Condition { get; set; }

        /// <summary>
        /// Gets or sets the text to be matched for Highlighting.
        /// </summary>
        /// <remarks>Default: <see cref="string.Empty"/></remarks>
        /// <docgen category='Highlighting Rules' order='10' />
        public string Text { get => _text; set => _text = string.IsNullOrEmpty(value) ? string.Empty : value; }
        private string _text = string.Empty;

        /// <summary>
        /// Gets or sets the list of words to be matched for Highlighting.
        /// </summary>
        /// <docgen category='Highlighting Rules' order='10' />
        public List<string>? Words { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to match whole words only.
        /// </summary>
        /// <remarks>Default: <see langword="false"/></remarks>
        /// <docgen category='Highlighting Rules' order='10' />
        public bool WholeWords { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to ignore case when comparing texts.
        /// </summary>
        /// <remarks>Default: <see langword="false"/></remarks>
        /// <docgen category='Highlighting Rules' order='10' />
        public bool IgnoreCase { get; set; }

        /// <summary>
        /// Gets or sets the foreground color.
        /// </summary>
        /// <remarks>Default: <see cref="ConsoleOutputColor.NoChange"/></remarks>
        /// <docgen category='Highlighting Rules' order='10' />
        public ConsoleOutputColor ForegroundColor { get; set; } = ConsoleOutputColor.NoChange;

        /// <summary>
        /// Gets or sets the background color.
        /// </summary>
        /// <remarks>Default: <see cref="ConsoleOutputColor.NoChange"/></remarks>
        /// <docgen category='Highlighting Rules' order='10' />
        public ConsoleOutputColor BackgroundColor { get; set; } = ConsoleOutputColor.NoChange;

        /// <summary>
        /// Scans the <paramref name="haystack"/> for words that should be highlighted.
        /// </summary>
        /// <returns>List of words with start-position (Key) and word-length (Value)</returns>
        internal protected virtual IEnumerable<KeyValuePair<int, int>>? GetWordsForHighlighting(string haystack)
        {
            if (ReferenceEquals(_text, string.Empty))
            {
                if (Words?.Count > 0)
                {
                    return YieldWordMatchesForHighlighting(haystack, Words);
                }
                return null;
            }

            return YieldMatchesForHighlighting(_text, haystack);
        }

        private IEnumerable<KeyValuePair<int, int>>? YieldWordMatchesForHighlighting(string haystack, List<string> words)
        {
            IEnumerable<KeyValuePair<int, int>>? allMatches = null;
            foreach (var needle in words)
            {
                if (string.IsNullOrEmpty(needle))
                    continue;

                var needleMatch = YieldMatchesForHighlighting(needle, haystack);
                if (needleMatch is null)
                    continue;

                allMatches = allMatches is null ? needleMatch : MergeWordMatches(allMatches, needleMatch);
            }

            return allMatches;
        }

        private static IEnumerable<KeyValuePair<int, int>> MergeWordMatches(IEnumerable<KeyValuePair<int, int>> allMatches, IEnumerable<KeyValuePair<int, int>> needleMatch)
        {
            if (needleMatch is IList<KeyValuePair<int, int>> singleMatch && singleMatch.Count == 1)
            {
                var match = singleMatch[0];
                var allMatchesList = PrepareAllMatchesList(allMatches, 1);
                MergeAllNeedleMatches(allMatchesList, match);
                return allMatchesList;
            }
            else
            {
                var allMatchesList = PrepareAllMatchesList(allMatches, 3);
                int startIndex = 0;
                foreach (var match in needleMatch)
                {
                    startIndex = MergeAllNeedleMatches(allMatchesList, match, startIndex);
                }
                return allMatchesList;
            }
        }

        private static int MergeAllNeedleMatches(IList<KeyValuePair<int, int>> allMatchesList, KeyValuePair<int, int> newMatch, int startIndex = 0)
        {
            for (int i = startIndex; i < allMatchesList.Count; ++i)
            {
                var existingMatch = allMatchesList[i];
                if (NeedleMatchOverlaps(newMatch, existingMatch))
                {
                    newMatch = MergeNeedleMatch(newMatch, existingMatch);
                    allMatchesList[i] = newMatch;
                    // Handle that the new merged match can also overlap following matches
                    while (i < allMatchesList.Count - 1 && NeedleMatchOverlaps(newMatch, allMatchesList[i + 1]))
                    {
                        newMatch = MergeNeedleMatch(newMatch, allMatchesList[i + 1]);
                        allMatchesList[i] = newMatch;
                        allMatchesList.RemoveAt(i + 1);
                    }
                    return i;
                }
                else if (newMatch.Key < existingMatch.Key)
                {
                    allMatchesList.Insert(i, newMatch);
                    return i + 1;
                }
            }

            allMatchesList.Add(newMatch);
            return allMatchesList.Count;
        }

        private static bool NeedleMatchOverlaps(KeyValuePair<int, int> first, KeyValuePair<int, int> second)
        {
            if (first.Key < second.Key)
                return (first.Key + first.Value) > second.Key;
            else
                return (second.Key + second.Value) > first.Key;
        }

        private static KeyValuePair<int, int> MergeNeedleMatch(KeyValuePair<int, int> first, KeyValuePair<int, int> second)
        {
            if (first.Key < second.Key)
                return new KeyValuePair<int, int>(first.Key, Math.Max(first.Key + first.Value, second.Key + second.Value) - first.Key);
            else
                return new KeyValuePair<int, int>(second.Key, Math.Max(first.Key + first.Value, second.Key + second.Value) - second.Key);
        }

        private static IList<KeyValuePair<int, int>> PrepareAllMatchesList(IEnumerable<KeyValuePair<int, int>> allMatches, int extraCapacity)
        {
            int existingCapacity = 3;

            if (allMatches is IList<KeyValuePair<int, int>> allMatchesList)
            {
                if (!allMatchesList.IsReadOnly)
                    return allMatchesList;

                existingCapacity = Math.Max(allMatchesList.Count, existingCapacity);
            }

            allMatchesList = new List<KeyValuePair<int, int>>(existingCapacity + extraCapacity);
            foreach (var match in allMatches)
                allMatchesList.Add(match);
            return allMatchesList;
        }

        private IEnumerable<KeyValuePair<int, int>>? YieldMatchesForHighlighting(string needle, string haystack)
        {
            int firstIndex = FindNextWordForHighlighting(needle, haystack, null);
            if (firstIndex < 0)
                return null;

            int nextIndex = FindNextWordForHighlighting(needle, haystack, firstIndex);
            if (nextIndex < 0)
                return new[] { new KeyValuePair<int, int>(firstIndex, needle.Length) };

            return YieldWordsForHighlighting(needle, haystack, firstIndex, nextIndex);
        }

        private IEnumerable<KeyValuePair<int, int>> YieldWordsForHighlighting(string needle, string haystack, int firstIndex, int nextIndex)
        {
            yield return new KeyValuePair<int, int>(firstIndex, needle.Length);

            yield return new KeyValuePair<int, int>(nextIndex, needle.Length);

            int index = nextIndex;
            while (index >= 0)
            {
                index = FindNextWordForHighlighting(needle, haystack, index);
                if (index >= 0)
                    yield return new KeyValuePair<int, int>(index, needle.Length);
            }
        }

        private int FindNextWordForHighlighting(string needle, string haystack, int? prevIndex)
        {
            int index = prevIndex.HasValue ? prevIndex.Value + needle.Length : 0;
            while (index >= 0)
            {
                index = IgnoreCase ? haystack.IndexOf(needle, index, System.StringComparison.CurrentCultureIgnoreCase) : haystack.IndexOf(needle, index);
                if (index < 0 || (!WholeWords || StringHelpers.IsWholeWord(haystack, needle, index)))
                    return index;

                index += needle.Length;
            }
            return index;
        }

        /// <summary>
        /// Checks whether the specified log event matches the condition (if any).
        /// </summary>
        internal bool CheckCondition(LogEventInfo logEvent)
        {
            return Condition is null || true.Equals(Condition.Evaluate(logEvent));
        }
    }
}
