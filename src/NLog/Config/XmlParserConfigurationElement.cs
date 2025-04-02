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

namespace NLog.Config
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NLog.Internal;

    internal sealed class XmlParserConfigurationElement : ILoggingConfigurationElement
    {
        /// <summary>
        /// Gets the element name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the value of the element.
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// Gets the dictionary of attribute values.
        /// </summary>
        public IList<KeyValuePair<string, string>> AttributeValues { get; }

        /// <summary>
        /// Gets the collection of child elements.
        /// </summary>
        public IList<XmlParserConfigurationElement> Children { get; }

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

                return ArrayHelper.Empty<ILoggingConfigurationElement>();
            }
        }

        public XmlParserConfigurationElement(XmlParser.XmlParserElement xmlElement)
            : this(xmlElement, false)
        {
        }

        public XmlParserConfigurationElement(XmlParser.XmlParserElement xmlElement, bool nestedElement)
        {
            Parse(xmlElement, nestedElement, out var attributes, out var children);
            AttributeValues = attributes ?? ArrayHelper.Empty<KeyValuePair<string, string>>();
            Children = children ?? ArrayHelper.Empty<XmlParserConfigurationElement>();
        }

        private static bool SingleValueElement(XmlParserConfigurationElement child)
        {
            // Node-element that works like an attribute
            return child.Children.Count == 0 && child.AttributeValues.Count == 0 && child.Value != null;
        }

        private void Parse(XmlParser.XmlParserElement xmlElement, bool nestedElement, out IList<KeyValuePair<string, string>> attributes, out IList<XmlParserConfigurationElement> children)
        {
            var namePrefixIndex = xmlElement.Name.IndexOf(':');
            Name = namePrefixIndex >= 0 ? xmlElement.Name.Substring(namePrefixIndex + 1) : xmlElement.Name;
            Value = xmlElement.InnerText;
            attributes = xmlElement.Attributes;

            if (attributes?.Count > 0)
            {
                if (!nestedElement)
                {
                    for (int i = attributes.Count - 1; i >= 0; --i)
                    {
                        var attributeName = attributes[i].Key;
                        if (IsSpecialXmlRootAttribute(attributeName))
                        {
                            attributes.RemoveAt(i);
                        }
                    }
                }

                for (int j = 0; j < attributes.Count; ++j)
                {
                    var attributePrefixIndex = attributes[j].Key.IndexOf(':');
                    if (attributePrefixIndex >= 0)
                        attributes[j] = new KeyValuePair<string, string>(attributes[j].Key.Substring(attributePrefixIndex + 1), attributes[j].Value);
                }
            }

            children = null;

            if (xmlElement.Children?.Count > 0)
            {
                foreach (var child in xmlElement.Children)
                {
                    children = children ?? new List<XmlParserConfigurationElement>();
                    var nestedChild = nestedElement || !string.Equals(child.Name, "nlog", StringComparison.OrdinalIgnoreCase);
                    children.Add(new XmlParserConfigurationElement(child, nestedChild));
                }
            }
        }

        /// <summary>
        /// Special attribute we could ignore
        /// </summary>
        private static bool IsSpecialXmlRootAttribute(string attributeName)
        {
            if (attributeName?.StartsWith("xmlns", StringComparison.OrdinalIgnoreCase) == true)
                return true;
            if (attributeName?.IndexOf(":xmlns", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
            if (attributeName?.StartsWith("schemaLocation", StringComparison.OrdinalIgnoreCase) == true)
                return true;
            if (attributeName?.IndexOf(":schemaLocation", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
            if (attributeName?.StartsWith("xsi:", StringComparison.OrdinalIgnoreCase) == true)
                return true;
            return false;
        }
    }
}
