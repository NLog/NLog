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

namespace NLog.LayoutRenderers
{
    using System.Globalization;
    using System.Text;
    using NLog.Config;
    using NLog.Internal;

    /// <summary>
    /// Renders specified property-item from <see cref="ScopeContext"/>
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/NLog/NLog/wiki/ScopeProperty-Layout-Renderer">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/NLog/NLog/wiki/ScopeProperty-Layout-Renderer">Documentation on NLog Wiki</seealso>
    [LayoutRenderer("scopeproperty")]
    [LayoutRenderer("mdc")]
    [LayoutRenderer("mdlc")]
    public sealed class ScopeContextPropertyLayoutRenderer : LayoutRenderer, IStringValueRenderer
    {
        /// <summary>
        /// Gets or sets the name of the item.
        /// </summary>
        /// <remarks><b>[Required]</b> Default: <see cref="string.Empty"/></remarks>
        /// <docgen category='Layout Options' order='10' />
        [DefaultParameter]
        public string Item { get; set; } = string.Empty;

        /// <summary>
        /// Format string for conversion from object to string.
        /// </summary>
        /// <remarks>Default: <see langword="null"/></remarks>
        /// <docgen category='Layout Options' order='50' />
        public string? Format { get; set; }

        /// <summary>
        /// Gets or sets the culture used for rendering.
        /// </summary>
        /// <remarks>Default: <see cref="CultureInfo.InvariantCulture"/></remarks>
        /// <docgen category='Layout Options' order='100' />
        public CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;

        /// <inheritdoc/>
        protected override void InitializeLayoutRenderer()
        {
            base.InitializeLayoutRenderer();

            if (string.IsNullOrEmpty(Item))
                throw new NLogConfigurationException("ScopeProperty-LayoutRenderer Item-property must be assigned. Lookup blank value not supported.");
        }

        /// <inheritdoc/>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            var value = GetValue();
            AppendFormattedValue(builder, logEvent, value, Format, Culture);
        }

        string? IStringValueRenderer.GetFormattedString(LogEventInfo logEvent)
        {
            if (!MessageTemplates.ValueFormatter.FormatAsJson.Equals(Format))
            {
                var value = GetValue();
                string stringValue = FormatHelper.TryFormatToString(value, Format, GetFormatProvider(logEvent, Culture));
                return stringValue;
            }
            return null;
        }

        private object? GetValue()
        {
            ScopeContext.TryGetProperty(Item, out var value);
            return value;
        }
    }
}
