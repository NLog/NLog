// 
// Copyright (c) 2004-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
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
using System.Text;
using System.IO;
using NLog.Internal;
using System.ComponentModel;
using NLog.Config;
using NLog.Layouts;
using System.Text.RegularExpressions;

namespace NLog.LayoutRenderers.Wrappers
{
    /// <summary>
    /// Replaces a string in the output of another layout with another string.
    /// </summary>
    [LayoutRenderer("replace")]
    public sealed class ReplaceLayoutRendererWrapper: WrapperLayoutRendererBase
    {
        private string _searchFor;
        private string _replaceWith;
        private bool _ignoreCase = false;
        private bool _useRegex = false;
        private bool _wholeWords = false;
        private Regex _regex = null;

        public string SearchFor
        {
            get { return _searchFor; }
            set { _searchFor = value; }
        }

        public bool RegEx
        {
            get { return _useRegex; }
            set { _useRegex = value; }
        }

        public string ReplaceWith
        {
            get { return _replaceWith; }
            set { _replaceWith = value; }
        }

        public bool IgnoreCase
        {
            get { return _ignoreCase; }
            set { _ignoreCase = value; }
        }

        public bool WholeWords
        {
            get { return _wholeWords; }
            set { _wholeWords = value; }
        }

        /// <summary>
        /// Post-processes the rendered message. 
        /// </summary>
        /// <param name="text">The text to be post-processed.</param>
        /// <returns>Post-processed text.</returns>
        protected override string Transform(string text)
        {
            return _regex.Replace(text, ReplaceWith);
        }

        public override void Initialize()
        {
            base.Initialize();
            string regexString = SearchFor;

            if (!_useRegex)
            {
                regexString = Regex.Escape(regexString);
            }

            RegexOptions regexOptions = RegexOptions.Compiled;
            if (IgnoreCase)
                regexOptions |= RegexOptions.IgnoreCase;
            if (WholeWords)
                regexString = "\\b" + regexString + "\\b";

            _regex = new Regex(regexString, regexOptions);
        }
    }
}
