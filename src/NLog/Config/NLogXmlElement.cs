// 
// Copyright (c) 2004-2019 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

using System.Linq;

namespace NLog.Config
{
    using System;
    using System.Collections.Generic;
    using System.Xml;

    /// <summary>
    /// Represents simple XML element with case-insensitive attribute semantics.
    /// </summary>
    internal class NLogXmlElement : ILoggingConfigurationElement
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
                Parse(reader, true);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NLogXmlElement"/> class.
        /// </summary>
        /// <param name="reader">The reader to initialize element from.</param>
        public NLogXmlElement(XmlReader reader)
            : this(reader, false)
        {
        }

        private NLogXmlElement(XmlReader reader, bool nestedElement)
            : this()
        {
            Parse(reader, nestedElement);
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="NLogXmlElement"/> class from being created.
        /// </summary>
        private NLogXmlElement()
        {
            AttributeValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            Children = new List<NLogXmlElement>();
            _parsingErrors = new List<string>();
        }

        /// <summary>
        /// Gets the element name.
        /// </summary>
        public string LocalName { get; private set; }

        /// <summary>
        /// Gets the dictionary of attribute values.
        /// </summary>
        public Dictionary<string, string> AttributeValues { get; }

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
                        return Children.Where(item => SingleValueElement(item)).Select(item => new KeyValuePair<string, string>(item.Name, item.Value)).Concat(AttributeValues);
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
        /// Last error occured during configuration read
        /// </summary>
        private readonly List<string> _parsingErrors;

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

        /// <summary>
        /// Returns all parsing errors from current and all child elements.
        /// </summary>
        public IEnumerable<string> GetParsingErrors()
        {
            foreach (var parsingError in _parsingErrors)
            {
                yield return parsingError;
            }

            foreach (var childElement in Children)
            {
                foreach (var parsingError in childElement.GetParsingErrors())
                {
                    yield return parsingError;
                }
            }
        }

        private void Parse(XmlReader reader, bool nestedElement)
        {
            ParseAttributes(reader, nestedElement);

            LocalName = reader.LocalName;

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
                        Children.Add(new NLogXmlElement(reader, true));
                    }
                }
            }
        }

        private void ParseAttributes(XmlReader reader, bool nestedElement)
        {
            if (reader.MoveToFirstAttribute())
            {
                do
                {
                    if (!nestedElement && IsSpecialXmlAttribute(reader))
                    {
                        continue;
                    }

                    if (!AttributeValues.ContainsKey(reader.LocalName))
                    {
                        AttributeValues.Add(reader.LocalName, reader.Value);
                    }
                    else
                    {
                        string message = $"Duplicate attribute detected. Attribute name: [{reader.LocalName}]. Duplicate value:[{reader.Value}], Current value:[{AttributeValues[reader.LocalName]}]";
                        _parsingErrors.Add(message);
                    }
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
            if (reader.Prefix?.Equals("xsi", StringComparison.OrdinalIgnoreCase) == true)
                return true;
            if (reader.Prefix?.Equals("xmlns", StringComparison.OrdinalIgnoreCase) == true)
                return true;
            return false;
        }
    }
}