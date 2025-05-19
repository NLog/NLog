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

namespace NLog.LayoutRenderers.Wrappers
{
    using System;
    using NLog.Config;
    using NLog.Internal;

    /// <summary>
    /// Replaces a string in the output of another layout with another string.
    /// </summary>
    /// <example>
    /// ${replace:searchFor=foo:replaceWith=bar:inner=${message}}
    /// </example>
    /// <remarks>
    /// <a href="https://github.com/NLog/NLog/wiki/Replace-Layout-Renderer">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/NLog/NLog/wiki/Replace-Layout-Renderer">Documentation on NLog Wiki</seealso>
    [LayoutRenderer("replace")]
    [AppDomainFixedOutput]
    [ThreadAgnostic]
    public sealed class ReplaceLayoutRendererWrapper : WrapperLayoutRendererBase
    {
        /// <summary>
        /// Gets or sets the text to search for.
        /// </summary>
        /// <value>The text search for.</value>
        /// <docgen category='Layout Options' order='10' />
        public string SearchFor
        {
            get => _searchForOriginal ?? _searchFor;
            set
            {
                _searchForOriginal = value;
                _searchFor = Layouts.SimpleLayout.Evaluate(value, LoggingConfiguration, throwConfigExceptions: false);
            }
        }
        private string _searchFor = string.Empty;
        private string _searchForOriginal = string.Empty;

        /// <summary>
        /// Gets or sets the replacement string.
        /// </summary>
        /// <value>The replacement string.</value>
        /// <docgen category='Layout Options' order='10' />
        public string ReplaceWith
        {
            get => _replaceWithOriginal ?? _replaceWith;
            set
            {
                _replaceWithOriginal = value;
                _replaceWith = Layouts.SimpleLayout.Evaluate(value, LoggingConfiguration, throwConfigExceptions: false);
            }
        }
        private string _replaceWith = string.Empty;
        private string _replaceWithOriginal = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether to ignore case.
        /// </summary>
        /// <value>A value of <c>true</c> if case should be ignored when searching; otherwise, <c>false</c>.</value>
        /// <docgen category='Condition Options' order='10' />
        public bool IgnoreCase { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to search for whole words
        /// </summary>
        /// <value>A value of <c>true</c> if whole words should be searched for; otherwise, <c>false</c>.</value>
        /// <docgen category='Condition Options' order='10' />
        public bool WholeWords { get; set; }

        /// <inheritdoc/>
        protected override void InitializeLayoutRenderer()
        {
            base.InitializeLayoutRenderer();

            if (string.IsNullOrEmpty(SearchFor))
                throw new NLogConfigurationException("Replace-LayoutRenderer SearchFor-property must be assigned. Searching for blank value not supported.");

            if (_searchForOriginal != null)
                _searchFor = Layouts.SimpleLayout.Evaluate(_searchForOriginal, LoggingConfiguration);
            if (_replaceWithOriginal != null)
                _replaceWith = Layouts.SimpleLayout.Evaluate(_replaceWithOriginal, LoggingConfiguration);
        }

        /// <inheritdoc/>
        protected override string Transform(string text)
        {
            if (IgnoreCase || WholeWords)
            {
                var stringComparer = IgnoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture;
                return StringHelpers.Replace(text, _searchFor, _replaceWith, stringComparer, WholeWords);
            }
            else
            {
                return text.Replace(_searchFor, _replaceWith);
            }
        }
    }
}
