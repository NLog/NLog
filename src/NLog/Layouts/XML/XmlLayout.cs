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
    using NLog.Config;

    /// <summary>
    /// A specialized layout that renders XML-formatted events.
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/NLog/NLog/wiki/XmlLayout">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/NLog/NLog/wiki/XmlLayout">Documentation on NLog Wiki</seealso>
    [Layout("XmlLayout")]
    [ThreadAgnostic]
    public class XmlLayout : XmlElementBase
    {
        private const string DefaultRootElementName = "logevent";

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLayout"/> class.
        /// </summary>
        public XmlLayout()
            : this(DefaultRootElementName, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLayout"/> class.
        /// </summary>
        /// <param name="elementName">The name of the top XML node</param>
        /// <param name="elementValue">The value of the top XML node</param>
        public XmlLayout(string elementName, Layout elementValue) : base(elementName, elementValue)
        {
        }

        /// <summary>
        /// Name of the root XML element
        /// </summary>
        /// <remarks>
        /// Default value "logevent"
        /// </remarks>
        /// <docgen category='Layout Options' order='10' />
        public string ElementName
        {
            get => base.ElementNameInternal;
            set => base.ElementNameInternal = value;
        }

        /// <summary>
        /// Value inside the root XML element
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public Layout ElementValue
        {
            get => base.LayoutWrapper.Inner;
            set => base.LayoutWrapper.Inner = value;
        }

        /// <summary>
        /// Determines whether or not this attribute will be Xml encoded.
        /// </summary>
        /// <docgen category='Layout Options' order='100' />
        public bool ElementEncode
        {
            get => base.LayoutWrapper.XmlEncode;
            set => base.LayoutWrapper.XmlEncode = value;
        }
    }
}
