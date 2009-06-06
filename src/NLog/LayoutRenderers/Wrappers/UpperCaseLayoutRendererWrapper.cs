// 
// Copyright (c) 2004-2009 Jaroslaw Kowalski <jaak@jkowalski.net>
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

using System.Globalization;

namespace NLog.LayoutRenderers.Wrappers
{
    /// <summary>
    /// Converts the result of another layout output to upper case.
    /// </summary>
    [LayoutRenderer("uppercase")]
    [AmbientProperty("UpperCase")]
    public sealed class UpperCaseLayoutRendererWrapper : WrapperLayoutRendererBase
    {
        /// <summary>
        /// Initializes a new instance of the UpperCaseLayoutRendererWrapper class.
        /// </summary>
        public UpperCaseLayoutRendererWrapper()
        {
            this.Culture = CultureInfo.InvariantCulture;
        }

        /// <summary>
        /// Gets or sets a value indicating whether upper case conversion should be applied.
        /// </summary>
        /// <value>A value of <c>true</c> if upper case conversion should be applied otherwise, <c>false</c>.</value>
        public bool UpperCase { get; set; }

        /// <summary>
        /// Gets or sets the culture used for rendering. 
        /// </summary>
        /// <example>
        /// The format for culture names is described in <a href="http://rfc.net/rfc1766.html">RFC 1766</a> and at <a href="http://msdn2.microsoft.com/en-us/library/system.globalization.cultureinfo.cultureinfo.aspx">MSDN</a>. 
        /// Some examples of valid culture names are:
        /// <ul>
        /// <li><b>en-US</b> - English (United States)</li>
        /// <li><b>en-UK</b> - English (United Kingdom)</li>
        /// <li><b>pl-PL</b> - Polish</li>
        /// <li><b>ar-SA</b> - Arabic (Saudi Arabia)</li>
        /// </ul>
        /// </example>
        public CultureInfo Culture { get; set; }

        /// <summary>
        /// Post-processes the rendered message. 
        /// </summary>
        /// <param name="text">The text to be post-processed.</param>
        /// <returns>Padded and trimmed string.</returns>
        protected override string Transform(string text)
        {
#if SILVERLIGHT || NET_CF
            return this.UpperCase ? text.ToUpper() : text;
#else
            return this.UpperCase ? text.ToUpperInvariant() : text;
#endif
        }
    }
}
