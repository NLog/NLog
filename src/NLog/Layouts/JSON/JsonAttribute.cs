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

namespace NLog.Layouts
{
    using System;
    using System.ComponentModel;
    using System.Text;
    using NLog.Config;

    /// <summary>
    /// JSON attribute.
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/NLog/NLog/wiki/JsonLayout">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/NLog/NLog/wiki/JsonLayout">Documentation on NLog Wiki</seealso>
    [NLogConfigurationItem]
    public class JsonAttribute
    {
        private readonly ValueTypeLayoutInfo _layoutInfo = new ValueTypeLayoutInfo();

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonAttribute" /> class.
        /// </summary>
        public JsonAttribute() : this(string.Empty, Layout.Empty, true) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonAttribute" /> class.
        /// </summary>
        /// <param name="name">The name of the attribute.</param>
        /// <param name="layout">The layout of the attribute's value.</param>
        public JsonAttribute(string name, Layout layout) : this(name, layout, true) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonAttribute" /> class.
        /// </summary>
        /// <param name="name">The name of the attribute.</param>
        /// <param name="layout">The layout of the attribute's value.</param>
        /// <param name="encode">Encode value with json-encode</param>
        public JsonAttribute(string name, Layout layout, bool encode)
        {
            Name = name;
            _name = Name;
            Layout = layout;
            Encode = encode;
        }

        /// <summary>
        /// Gets or sets the name of the attribute.
        /// </summary>
        /// <remarks><b>[Required]</b> Default: <see cref="string.Empty"/></remarks>
        /// <docgen category='Layout Options' order='1' />
        public string Name
        {
            get => _name;
            set
            {
                if (string.IsNullOrEmpty(value))
                    _name = value;
                else if (System.Linq.Enumerable.All(value, chr => char.IsLetterOrDigit(chr)))
                    _name = value;
                else
                {
                    var builder = new System.Text.StringBuilder();
                    Targets.DefaultJsonSerializer.AppendStringEscape(builder, value.Trim(), false);
                    _name = builder.ToString();
                }
            }
        }
        private string _name;

        /// <summary>
        /// Gets or sets the layout used for rendering the attribute value.
        /// </summary>
        /// <remarks><b>[Required]</b> Default: <see cref="Layout.Empty"/></remarks>
        /// <docgen category='Layout Options' order='10' />
        public Layout Layout { get => _layoutInfo.Layout; set => _layoutInfo.Layout = value; }

        /// <summary>
        /// Gets or sets the result value type, for conversion of layout rendering output
        /// </summary>
        /// <remarks>Default: <see langword="null"/></remarks>
        /// <docgen category='Layout Options' order='100' />
        public Type? ValueType { get => _layoutInfo.ValueType; set => _layoutInfo.ValueType = value; }

        /// <summary>
        /// Gets or sets the fallback value when result value is not available
        /// </summary>
        /// <remarks>Default: <see langword="null"/></remarks>
        /// <docgen category='Layout Options' order='100' />
        public Layout? DefaultValue { get => _layoutInfo.DefaultValue; set => _layoutInfo.DefaultValue = value; }

        /// <summary>
        /// Gets or sets whether output should be encoded as Json-String-Property, or be treated as valid json.
        /// </summary>
        /// <remarks>Default: <see langword="true"/></remarks>
        /// <docgen category='Layout Options' order='100' />
        public bool Encode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to escape non-ascii characters
        /// </summary>
        /// <remarks>Default: <see langword="false"/></remarks>
        /// <docgen category='Layout Options' order='100' />
        public bool EscapeUnicode { get; set; }

        /// <summary>
        /// Should forward slashes be escaped? If true, / will be converted to \/
        /// </summary>
        /// <remarks>Default: <see langword="false"/></remarks>
        /// <docgen category='Layout Options' order='100' />
        [Obsolete("Marked obsolete since forward slash are valid JSON. Marked obsolete with NLog v5.4")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool EscapeForwardSlash { get; set; }

        /// <summary>
        /// Gets or sets whether empty attribute value should be included in the output.
        /// </summary>
        /// <remarks>Default: <see langword="false"/> . Empty value is either null or empty string</remarks>
        /// <docgen category='Layout Options' order='100' />
        public bool IncludeEmptyValue
        {
            get => _includeEmptyValue;
            set
            {
                _includeEmptyValue = value;
                _layoutInfo.ForceDefaultValueNull = !value;
            }
        }
        private bool _includeEmptyValue;

        internal bool RenderAppendJsonValue(LogEventInfo logEvent, IJsonConverter jsonConverter, StringBuilder builder)
        {
            if (!Encode)
            {
                var valueStart = builder.Length;
                if (!RenderAppendJsonValue(logEvent, builder, valueStart))
                    return false;
            }
            else if (ValueType is null)
            {
                builder.Append('"');

                var valueStart = builder.Length;
                if (!RenderAppendJsonValue(logEvent, builder, valueStart))
                    return false;

                Targets.DefaultJsonSerializer.PerformJsonEscapeWhenNeeded(builder, valueStart, EscapeUnicode);
                builder.Append('"');
}
            else
            {
                var objectValue = _layoutInfo.RenderValue(logEvent);
                if (!IncludeEmptyValue && Internal.StringHelpers.IsNullOrEmptyString(objectValue))
                    return false;

                jsonConverter.SerializeObject(objectValue, builder);
            }

            return true;
        }

        private bool RenderAppendJsonValue(LogEventInfo logEvent, StringBuilder builder, int valueStart)
        {
            Layout.Render(logEvent, builder);
            return IncludeEmptyValue || builder.Length > valueStart;
        }
    }
}
