// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
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

namespace NLog.Config
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Xml;

    /// <summary>
    /// Represents simple XML element with case-insensitive attribute semantics.
    /// </summary>
    internal class NLogXmlElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NLogXmlElement"/> class.
        /// </summary>
        /// <param name="inputUri">The input URI.</param>
        public NLogXmlElement(string inputUri)
            : this()
        {
            using (var reader = XmlReader.Create(inputUri))
            {
                reader.MoveToContent();
                this.Parse(reader);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NLogXmlElement"/> class.
        /// </summary>
        /// <param name="reader">The reader to initialize element from.</param>
        public NLogXmlElement(XmlReader reader)
            : this()
        {
            this.Parse(reader);
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="NLogXmlElement"/> class from being created.
        /// </summary>
        private NLogXmlElement()
        {
            this.AttributeValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            this.Children = new List<NLogXmlElement>();
        }

        /// <summary>
        /// Gets the element name.
        /// </summary>
        public string LocalName { get; private set; }

        /// <summary>
        /// Gets the dictionary of attribute values.
        /// </summary>
        public Dictionary<string, string> AttributeValues { get; private set; }

        /// <summary>
        /// Gets the collection of child elements.
        /// </summary>
        public IList<NLogXmlElement> Children { get; private set; }

        /// <summary>
        /// Gets the value of the element.
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// Returns children elements with the specified element name.
        /// </summary>
        /// <param name="elementName">Name of the element.</param>
        /// <returns>Children elements with the specified element name.</returns>
        public IEnumerable<NLogXmlElement> Elements(string elementName)
        {
            var result = new List<NLogXmlElement>();

            foreach (var ch in this.Children)
            {
                if (ch.LocalName.Equals(elementName, StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(ch);
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the required attribute.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <returns>Attribute value.</returns>
        /// <remarks>Throws if the attribute is not specified.</remarks>
        public string GetRequiredAttribute(string attributeName)
        {
            string value = this.GetOptionalAttribute(attributeName, null);
            if (value == null)
            {
                throw new NLogConfigurationException("Expected " + attributeName + " on <" + this.LocalName + " />");
            }

            return value;
        }

        /// <summary>
        /// Gets the optional boolean attribute value.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="defaultValue">Default value to return if the attribute is not found.</param>
        /// <returns>Boolean attribute value or default.</returns>
        public bool GetOptionalBooleanAttribute(string attributeName, bool defaultValue)
        {
            string value;

            if (!this.AttributeValues.TryGetValue(attributeName, out value))
            {
                return defaultValue;
            }

            return Convert.ToBoolean(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Gets the optional attribute value.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>Value of the attribute or default value.</returns>
        public string GetOptionalAttribute(string attributeName, string defaultValue)
        {
            string value;

            if (!this.AttributeValues.TryGetValue(attributeName, out value))
            {
                value = defaultValue;
            }

            return value;
        }

        /// <summary>
        /// Asserts that the name of the element is among specified element names.
        /// </summary>
        /// <param name="allowedNames">The allowed names.</param>
        public void AssertName(params string[] allowedNames)
        {
            foreach (var en in allowedNames)
            {
                if (this.LocalName.Equals(en, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }

            throw new InvalidOperationException("Assertion failed. Expected element name '" + string.Join("|", allowedNames) + "', actual: '" + this.LocalName + "'.");
        }

        private void Parse(XmlReader reader)
        {
            if (reader.MoveToFirstAttribute())
            {
                do
                {
                    this.AttributeValues.Add(reader.LocalName, reader.Value);
                }
                while (reader.MoveToNextAttribute());

                reader.MoveToElement();
            }

            this.LocalName = reader.LocalName;

            if (!reader.IsEmptyElement)
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        break;
                    }

                    if (reader.NodeType == XmlNodeType.CDATA || reader.NodeType == XmlNodeType.Text)
                    {
                        this.Value += reader.Value;
                        continue;
                    }

                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        this.Children.Add(new NLogXmlElement(reader));
                    }
                }
            }
        }
    }
}