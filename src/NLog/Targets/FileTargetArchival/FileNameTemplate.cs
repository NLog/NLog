// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
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

using System;

namespace NLog.Targets.FileTargetArchival
{
    /// <summary>
    /// Provides archival file name pattern-matching services.
    /// </summary>
    sealed class FileNameTemplate
    {
        /// <summary>
        /// Characters determining the start of the <see cref="P:FileNameTemplate.Pattern"/>.
        /// </summary>
        public const string PatternStartCharacters = "{#";

        /// <summary>
        /// Characters determining the end of the <see cref="P:FileNameTemplate.Pattern"/>.
        /// </summary>
        public const string PatternEndCharacters = "#}";

        /// <summary>
        /// File name which is used as template for matching and replacements. 
        /// It is expected to contain a pattern to match.
        /// </summary>
        public string Template
        {
            get { return this.template; }
        }

        /// <summary>
        /// The begging position of the <see cref="P:FileNameTemplate.Pattern"/> 
        /// within the <see cref="P:FileNameTemplate.Template"/>. -1 is returned 
        /// when no pattern can be found.
        /// </summary>
        public int BeginAt
        {
            get
            {
                return startIndex;
            }
        }

        /// <summary>
        /// The ending position of the <see cref="P:FileNameTemplate.Pattern"/> 
        /// within the <see cref="P:FileNameTemplate.Template"/>. -1 is returned 
        /// when no pattern can be found.
        /// </summary>
        public int EndAt
        {
            get
            {
                return endIndex;
            }
        }

        private bool FoundPattern
        {
            get { return startIndex != -1 && endIndex != -1; }
        }

        private readonly string template;

        private readonly int startIndex;
        private readonly int endIndex;

        public FileNameTemplate(string template)
        {
            this.template = template;
            this.startIndex = template.IndexOf(PatternStartCharacters, StringComparison.Ordinal);
            if (this.startIndex != -1)
                this.endIndex = template.IndexOf(PatternEndCharacters, StringComparison.Ordinal) + PatternEndCharacters.Length;
        }

        /// <summary>
        /// Replace the pattern with the specified String.
        /// </summary>
        /// <param name="replacementValue"></param>
        /// <returns></returns>
        public string ReplacePattern(string replacementValue)
        {
            return !FoundPattern || String.IsNullOrEmpty(replacementValue) ? this.Template : template.Substring(0, this.BeginAt) + replacementValue + template.Substring(this.EndAt);
        }

        /// <summary>
        /// Determines if the file name as <see cref="String"/> contains a numeric pattern i.e. {#} in it.  
        ///
        /// Example: 
        ///     trace{#}.log        Contains the numeric pattern.
        ///     trace{###}.log      Contains the numeric pattern.
        ///     trace{#X#}.log      Contains the numeric pattern (See remarks).
        ///     trace.log           Does not contain the pattern.
        /// </summary>
        /// <remarks>Occationally, this method can identify the existance of the {#} pattern incorrectly.</remarks>
        /// <param name="fileName">File name to be checked.</param>
        /// <returns><see langword="true"/> when the pattern is found; <see langword="false"/> otherwise.</returns>
        public static bool ContainsFileNamePattern(string fileName)
        {
            int startingIndex = fileName.IndexOf("{#", StringComparison.Ordinal);
            int endingIndex = fileName.IndexOf("#}", StringComparison.Ordinal);

            return (startingIndex != -1 && endingIndex != -1 && startingIndex < endingIndex);
        }

        /// <summary>
        /// Replaces the numeric pattern i.e. {#} in a file name with the <paramref name="value"/> parameter value.
        /// </summary>
        /// <param name="pattern">File name which contains the numeric pattern.</param>
        /// <param name="value">Value which will replace the numeric pattern.</param>
        /// <returns>File name with the value of <paramref name="value"/> in the position of the numberic pattern.</returns>
        public static string ReplaceNumberPattern(string pattern, int value)
        {
            int firstPart = pattern.IndexOf("{#", StringComparison.Ordinal);
            int lastPart = pattern.IndexOf("#}", StringComparison.Ordinal) + 2;
            int numDigits = lastPart - firstPart - 2;

            return pattern.Substring(0, firstPart) + Convert.ToString(value, 10).PadLeft(numDigits, '0') + pattern.Substring(lastPart);
        }

        /// <summary>
        /// Replaces the string-based pattern i.e. {#} in a file name with the value passed in <paramref
        /// name="replacementValue"/> parameter.
        /// </summary>
        /// <param name="pattern">File name which contains the string-based pattern.</param>
        /// <param name="replacementValue">Value which will replace the string-based pattern.</param>
        /// <returns>
        /// File name with the value of <paramref name="replacementValue"/> in the position of the string-based pattern.
        /// </returns>
        public static string ReplaceFileNamePattern(string pattern, string replacementValue)
        {
            //
            // TODO: ReplaceFileNamePattern() method is nearly identical to ReplaceNumberPattern(). Consider merging.
            //

            return new FileNameTemplate(System.IO.Path.GetFileName(pattern)).ReplacePattern(replacementValue);
        }
    }
}
