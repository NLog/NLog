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

namespace NLog.Layouts
{
    using System.ComponentModel;
    using NLog.Config;

    /// <summary>
    /// JSON attribute.
    /// </summary>
    [NLogConfigurationItem]
    [ThreadAgnostic]
    [ThreadSafe]
    [AppDomainFixedOutput]
    public class JsonAttribute
    {
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
        /// <docgen category='JSON Attribute Options' order='10' />
        [RequiredParameter]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the layout that will be rendered as the attribute's value.
        /// </summary>
        /// <docgen category='JSON Attribute Options' order='10' />
        [RequiredParameter]
        public Layout Layout
        {
            get => LayoutWrapper.Inner;
            set => LayoutWrapper.Inner = value;
        }

        /// <summary>
        /// Determines whether or not this attribute will be Json encoded.
        /// </summary>
        /// <docgen category='JSON Attribute Options' order='100' />
        public bool Encode
        {
            get => LayoutWrapper.JsonEncode;
            set => LayoutWrapper.JsonEncode = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to escape non-ascii characters
        /// </summary>
        /// <docgen category='JSON Attribute Options' order='100' />
        public bool EscapeUnicode
        {
            get => LayoutWrapper.EscapeUnicode;
            set => LayoutWrapper.EscapeUnicode = value;
        }

        /// <summary>
        /// Should forward slashes be escaped? If true, / will be converted to \/ 
        /// </summary>
        /// <docgen category='JSON Attribute Options' order='100' />
        [DefaultValue(true)] // TODO NLog 5 change to nullable (with default fallback to false)
        public bool EscapeForwardSlash 
        {
            get => LayoutWrapper.EscapeForwardSlash;
            set => LayoutWrapper.EscapeForwardSlash = value;
        }

        /// <summary>
        /// Gets or sets whether an attribute with empty value should be included in the output
        /// </summary>
        /// <docgen category='JSON Attribute Options' order='100' />
        public bool IncludeEmptyValue { get; set; }

        internal readonly LayoutRenderers.Wrappers.JsonEncodeLayoutRendererWrapper LayoutWrapper = new LayoutRenderers.Wrappers.JsonEncodeLayoutRendererWrapper();
    }
}