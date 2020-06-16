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

using System;
using System.ComponentModel;
using System.Text;
using NLog.Config;
using NLog.Internal.Fakeables;

namespace NLog.LayoutRenderers
{
    /// <summary>
    ///  Used to render the application domain name.
    ///  </summary>
    [LayoutRenderer("appdomain")]
    [AppDomainFixedOutput]
    [ThreadAgnostic]
    [ThreadSafe]
    public class AppDomainLayoutRenderer : LayoutRenderer
    {
        private const string ShortFormat = "{0:00}";
        private const string LongFormat = "{0:0000}:{1}";
        private const string LongFormatCode = "Long";
        private const string ShortFormatCode = "Short";

        private readonly IAppDomain _currentDomain;

        /// <summary>
        /// Create a new renderer
        /// </summary>
        public AppDomainLayoutRenderer()
            : this(LogFactory.CurrentAppDomain)
        {
        }

        /// <summary>
        /// Create a new renderer
        /// </summary>
        public AppDomainLayoutRenderer(IAppDomain currentDomain)
        {
            _currentDomain = currentDomain;
            Format = LongFormatCode;
        }

        /// <summary>
        /// Format string. Possible values: "Short", "Long" or custom like {0} {1}. Default "Long"
        /// The first parameter is the  <see cref="IAppDomain.Id"/>, the second the second the  <see cref="IAppDomain.FriendlyName"/>
        /// This string is used in <see cref="string.Format(string,object[])"/>
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultParameter]
        [DefaultValue(LongFormatCode)]
        public string Format { get; set; }

        /// <inheritdoc/>
        protected override void InitializeLayoutRenderer()
        {
            _assemblyName = null;
            base.InitializeLayoutRenderer();
        }

        /// <inheritdoc/>
        protected override void CloseLayoutRenderer()
        {
            _assemblyName = null;
            base.CloseLayoutRenderer();
        }

        private string _assemblyName;

        /// <inheritdoc/>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            if (_assemblyName == null)
            {
                var formattingString = GetFormattingString(Format);
                _assemblyName = string.Format(formattingString, _currentDomain.Id, _currentDomain.FriendlyName);
            }
            builder.Append(_assemblyName);
        }

        private static string GetFormattingString(string format)
        {
            string formattingString;
            if (format.Equals(LongFormatCode, StringComparison.OrdinalIgnoreCase))
            {
                formattingString = LongFormat;
            }
            else if (format.Equals(ShortFormatCode, StringComparison.OrdinalIgnoreCase))
            {
                formattingString = ShortFormat;
            }
            else
            {
                //custom format string
                formattingString = format;
            }
            return formattingString;
        }
    }
}