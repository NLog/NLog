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
        /// <remarks>Default: <c>null</c></remarks>
        /// <docgen category='Highlighting Rules' order='10' />
        public ConditionExpression? Condition { get; set; }

        /// <summary>
        /// Gets or sets the text to be matched. You must specify either <c>text</c> or <c>regex</c>.
        /// </summary>
        /// <remarks>Default: <see cref="string.Empty"/></remarks>
        /// <docgen category='Highlighting Rules' order='10' />
        public string Text { get => _text; set => _text = string.IsNullOrEmpty(value) ? string.Empty : value; }
        private string _text = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether to match whole words only.
        /// </summary>
        /// <remarks>Default: <c>false</c></remarks>
        /// <docgen category='Highlighting Rules' order='10' />
        public bool WholeWords { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to ignore case when comparing texts.
        /// </summary>
        /// <remarks>Default: <c>false</c></remarks>
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
                return null;

            int firstIndex = FindNextWordForHighlighting(haystack, null);
            if (firstIndex < 0)
                return null;

            int nextIndex = FindNextWordForHighlighting(haystack, firstIndex);
            if (nextIndex < 0)
                return new[] { new KeyValuePair<int, int>(firstIndex, Text.Length) };

            return YieldWordsForHighlighting(haystack, firstIndex, nextIndex);
        }

        private IEnumerable<KeyValuePair<int, int>> YieldWordsForHighlighting(string haystack, int firstIndex, int nextIndex)
        {
            yield return new KeyValuePair<int, int>(firstIndex, _text.Length);

            yield return new KeyValuePair<int, int>(nextIndex, _text.Length);

            int index = nextIndex;
            while (index >= 0)
            {
                index = FindNextWordForHighlighting(haystack, index);
                if (index >= 0)
                    yield return new KeyValuePair<int, int>(index, _text.Length);
            }
        }

        private int FindNextWordForHighlighting(string haystack, int? prevIndex)
        {
            int index = prevIndex.HasValue ? prevIndex.Value + _text.Length : 0;
            while (index >= 0)
            {
                index = IgnoreCase ? haystack.IndexOf(_text, index, System.StringComparison.CurrentCultureIgnoreCase) : haystack.IndexOf(_text, index);
                if (index < 0 || (!WholeWords || StringHelpers.IsWholeWord(haystack, _text, index)))
                    return index;

                index += _text.Length;
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
