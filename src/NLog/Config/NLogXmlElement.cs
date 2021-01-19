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

namespace NLog.Config
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;
    using NLog.Internal;

    /// <summary>
    /// Represents simple XML element with case-insensitive attribute semantics.
    /// </summary>
    internal class NLogXmlElement : ILoggingConfigurationElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NLogXmlElement"/> class.
        /// </summary>
        /// <param name="reader">The reader to initialize element from.</param>
        public NLogXmlElement(XmlReader reader)
            : this(reader, false)
        {
        }

        public NLogXmlElement(XmlReader reader, bool nestedElement)
        {
            Parse(reader, nestedElement, out var attributes, out var children);
            AttributeValues = attributes ?? ArrayHelper.Empty<KeyValuePair<string, string>>();
            Children = children ?? ArrayHelper.Empty<NLogXmlElement>();
        }

        /// <summary>
        /// Gets the element name.
        /// </summary>
        public string LocalName { get; private set; }

        /// <summary>
        /// Gets the dictionary of attribute values.
        /// </summary>
        public IList<KeyValuePair<string,string>> AttributeValues { get; }

        /// <summary>
        /// Gets the collection of child elements.
        /// </summary>
        public IList<NLogXmlElement> Children { get; }

        /// <summary>
        /// Gets the value of the element.
        /// </summary>
        public string Value { get; private set; }

        public string Name => LocalName;

        public IEnumerable<KeyValuePair<string, string>> Values
        {
            get
            {
                for (int i = 0; i < Children.Count; ++i)
                {
                    var child = Children[i];
                    if (SingleValueElement(child))
                    {
                        // Values assigned using nested node-elements. Maybe in combination with attributes
                        return AttributeValues.Concat(Children.Where(item => SingleValueElement(item)).Select(item => new KeyValuePair<string, string>(item.Name, item.Value)));
                    }
                }
                return AttributeValues;
            }
        }

        private static bool SingleValueElement(NLogXmlElement child)
        {
            // Node-element that works like an attribute
            return child.Children.Count == 0 && child.AttributeValues.Count == 0 && child.Value != null;
        }

        IEnumerable<ILoggingConfigurationElement> ILoggingConfigurationElement.Children
        {
            get
            {
                for (int i = 0; i < Children.Count; ++i)
                {
                    var child = Children[i];
                    if (!SingleValueElement(child))
                        return Children.Where(item => !SingleValueElement(item)).Cast<ILoggingConfigurationElement>();
                }

                return NLog.Internal.ArrayHelper.Empty<ILoggingConfigurationElement>();
            }
        }

        /// <summary>
        /// Returns children elements with the specified element name.
        /// </summary>
        /// <param name="elementName">Name of the element.</param>
        /// <returns>Children elements with the specified element name.</returns>
        public IEnumerable<NLogXmlElement> Elements(string elementName)
        {
            var result = new List<NLogXmlElement>();

            foreach (var ch in Children)
            {
                if (ch.LocalName.Equals(elementName, StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(ch);
                }
            }

            return result;
        }

        /// <summary>
        /// Asserts that the name of the element is among specified element names.
        /// </summary>
        /// <param name="allowedNames">The allowed names.</param>
        public void AssertName(params string[] allowedNames)
        {
            foreach (var en in allowedNames)
            {
                if (LocalName.Equals(en, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }

            throw new InvalidOperationException("Assertion failed. Expected element name '" + string.Join("|", allowedNames) + "', actual: '" + LocalName + "'.");
        }

        private void Parse(XmlReader reader, bool topElement, out IList<KeyValuePair<string,string>> attributes, out IList<NLogXmlElement> children)
        {
            ParseAttributes(reader, topElement, out attributes);

            LocalName = reader.LocalName;

            children = null;

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
                        Value += reader.Value;
                        continue;
                    }

                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        children = children ?? new List<NLogXmlElement>();
                        children.Add(new NLogXmlElement(reader, true));
                    }
                }
            }
        }

        private void ParseAttributes(XmlReader reader, bool nestedElement, out IList<KeyValuePair<string, string>> attributes)
        {
            attributes = null;
            if (reader.MoveToFirstAttribute())
            {
                do
                {
                    if (!nestedElement && IsSpecialXmlAttribute(reader))
                    {
                        continue;
                    }

                    attributes = attributes ?? new List<KeyValuePair<string, string>>();
                    attributes.Add(new KeyValuePair<string, string>(reader.LocalName, reader.Value));
                }
                while (reader.MoveToNextAttribute());
                reader.MoveToElement();
            }
        }

        /// <summary>
        /// Special attribute we could ignore
        /// </summary>
        private static bool IsSpecialXmlAttribute(XmlReader reader)
        {
            if (reader.LocalName?.Equals("xmlns", StringComparison.OrdinalIgnoreCase) == true)
                return true;
            if (reader.LocalName?.Equals("schemaLocation", StringComparison.OrdinalIgnoreCase) == true && !StringHelpers.IsNullOrWhiteSpace(reader.Prefix))
                return true;
            if (reader.Prefix?.Equals("xsi", StringComparison.OrdinalIgnoreCase) == true)
                return true;
            if (reader.Prefix?.Equals("xmlns", StringComparison.OrdinalIgnoreCase) == true)
                return true;
            return false;
        }
    }
}