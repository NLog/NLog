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
    using NLog.Config;

    /// <summary>
    /// A XML Element
    /// </summary>
    [ThreadAgnostic]
    public class XmlElement : XmlElementBase
    {
        private const string DefaultElementName = "item";

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlElement"/> class.
        /// </summary>
        public XmlElement() : this(DefaultElementName, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlElement"/> class.
        /// </summary>
        public XmlElement(string elementName, Layout elementValue) : base(elementName, elementValue)
        {
        }

        /// <summary>
        /// Name of the element
        /// </summary>
        /// <remarks>
        /// Default value "item"
        /// </remarks>
        /// <docgen category='Layout Options' order='1' />
        [RequiredParameter]
        public string Name
        {
            get => base.ElementNameInternal;
            set => base.ElementNameInternal = value;
        }

        /// <summary>
        /// Value inside the element
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public Layout Value
        {
            get => base.LayoutWrapper.Inner;
            set => base.LayoutWrapper.Inner = value;
        }

        /// <summary>
        /// Value inside the element
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public Layout Layout { get => Value; set => Value = value; }

        /// <summary>
        /// Gets or sets whether output should be encoded with Xml-string escaping, or be treated as valid xml-element-value
        /// </summary>
        /// <docgen category='Layout Options' order='50' />
        public bool Encode
        {
            get => base.LayoutWrapper.XmlEncode;
            set => base.LayoutWrapper.XmlEncode = value;
        }

        /// <summary>
        /// Wraps the element value in a CDATA section instead of escaping XML characters.
        /// </summary>
        public bool WrapValueInCData
        {
            get => LayoutWrapper.WrapInCData;
            set => LayoutWrapper.WrapInCData = value;
        }

    }
}
