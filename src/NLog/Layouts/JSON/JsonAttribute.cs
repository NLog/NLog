// 
// Copyright (c) 2004-2021 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using System.Text;
    using NLog.Config;

    /// <summary>
    /// JSON attribute.
    /// </summary>
    [NLogConfigurationItem]
    public class JsonAttribute
    {
        private readonly ValueTypeLayoutInfo _layoutInfo = new ValueTypeLayoutInfo();

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonAttribute" /> class.
        /// </summary>
        public JsonAttribute() : this(null, null, true) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonAttribute" /> class.
        /// </summary>
        /// <param name="name">The name of the attribute.</param>
        /// <param name="layout">The layout of the attribute's value.</param>
        public JsonAttribute(string name, Layout layout): this(name, layout, true) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonAttribute" /> class.
        /// </summary>
        /// <param name="name">The name of the attribute.</param>
        /// <param name="layout">The layout of the attribute's value.</param>
        /// <param name="encode">Encode value with json-encode</param>
        public JsonAttribute(string name, Layout layout, bool encode)
        {
            Name = name;
            Layout = layout;
            Encode = encode;
            IncludeEmptyValue = false;
        }

        /// <summary>
        /// Gets or sets the name of the attribute.
        /// </summary>
        /// <docgen category='Layout Options' order='1' />
        [RequiredParameter]
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
                    Targets.DefaultJsonSerializer.AppendStringEscape(builder, value, false, false);
                    _name = builder.ToString();
                }
            }
        }
        private string _name;

        /// <summary>
        /// Gets or sets the layout that will be rendered as the attribute's value.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        [RequiredParameter]
        public Layout Layout { get => _layoutInfo.Layout; set => _layoutInfo.Layout = value; }

        /// <summary>
        /// Gets or sets the result value type, for conversion of layout rendering output
        /// </summary>
        /// <docgen category='Layout Options' order='100' />
        public Type ValueType { get => _layoutInfo.ValueType; set => _layoutInfo.ValueType = value; }

        /// <summary>
        /// Gets or sets the fallback value when result value is not available
        /// </summary>
        /// <docgen category='Layout Options' order='100' />
        public Layout DefaultValue { get => _layoutInfo.DefaultValue; set => _layoutInfo.DefaultValue = value; }

        /// <summary>
        /// Gets or sets whether output should be encoded as Json-String-Property, or be treated as valid json.
        /// </summary>
        /// <docgen category='Layout Options' order='100' />
        public bool Encode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to escape non-ascii characters
        /// </summary>
        /// <docgen category='Layout Options' order='100' />
        public bool EscapeUnicode { get; set; }

        /// <summary>
        /// Should forward slashes be escaped? If true, / will be converted to \/
        /// </summary>
        /// <remarks>
        /// If not set explicitly then the value of the parent will be used as default.
        /// </remarks>
        /// <docgen category='Layout Options' order='100' />
        public bool EscapeForwardSlash { get => EscapeForwardSlashInternal ?? false; set => EscapeForwardSlashInternal = value; }
        internal bool? EscapeForwardSlashInternal { get; private set; }

        /// <summary>
        /// Gets or sets whether an attribute with empty value should be included in the output
        /// </summary>
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
            if (ValueType is null)
            {
                if (Encode)
                {
                    // "\"{0}\":{1}\"{2}\""
                    builder.Append('"');
                }

                int orgLength = builder.Length;
                Layout?.Render(logEvent, builder);
                if (!IncludeEmptyValue && builder.Length <= orgLength)
                {
                    return false;
                }

                if (Encode)
                {
                    Targets.DefaultJsonSerializer.PerformJsonEscapeWhenNeeded(builder, orgLength, EscapeUnicode, EscapeForwardSlash);
                    builder.Append('"');
                }
            }
            else
            {
                var objectValue = _layoutInfo.RenderValue(logEvent);
                if (!IncludeEmptyValue && (objectValue is null || string.Empty.Equals(objectValue)))
                {
                    return false;
                }

                jsonConverter.SerializeObject(objectValue, builder);
            }

            return true;
        }
    }
}