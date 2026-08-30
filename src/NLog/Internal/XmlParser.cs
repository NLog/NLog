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

namespace NLog.Internal
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using NLog.Config;

    /// <summary>
    /// A minimal XML reader, because .NET System.Xml.XmlReader doesn't work with AOT.
    /// </summary>
    internal sealed class XmlParser
    {
        private readonly InputCursor _xmlSource;
        private readonly StringBuilder _stringBuilder = new StringBuilder();

        public XmlParser(TextReader xmlSource)
        {
            _xmlSource = new InputCursor(xmlSource);
        }

        public XmlParser(string xmlSource)
        {
            _xmlSource = new InputCursor(new StringReader(xmlSource));
        }

        public XmlParserElement LoadDocument(out IList<XmlParserElement>? processingInstructions)
        {
            try
            {
                TryReadProcessingInstructions(out processingInstructions);

                if (!TryReadStartElement(out var rootName, out var rootAttributes))
                    throw new XmlParserException("Invalid XML document. Cannot parse root start-tag");

                var stack = new Stack<XmlParserElement>();
                var currentNode = new XmlParserElement(rootName ?? string.Empty, rootAttributes);
                stack.Push(currentNode);

                bool stillReading = true;

                while (stillReading)
                {
                    stillReading = false;

                    if (TryReadEndElement(currentNode.Name))
                    {
                        stillReading = true;
                        stack.Pop();
                        if (stack.Count == 0)
                            break;

                        currentNode = stack.Peek();
                    }

                    try
                    {
                        if (TryReadInnerText(out var innerText))
                        {
                            stillReading = true;
                            currentNode.InnerText += innerText;
                        }

                        if (TryReadStartElement(out var elementName, out var elementAttributes))
                        {
                            stillReading = true;
                            currentNode = new XmlParserElement(elementName ?? string.Empty, elementAttributes);
                            stack.Peek().AddChild(currentNode);
                            stack.Push(currentNode);
                        }
                    }
                    catch (XmlParserException ex)
                    {
                        throw new XmlParserException($"{ex.Message} - Start-tag: {currentNode.Name}");
                    }
                }

                if (!stillReading)
                    throw new XmlParserException($"Invalid XML document. Cannot parse end-tag: {currentNode.Name}");

                _xmlSource.SkipWhiteSpace();

                while (!_xmlSource.EndOfInput && _xmlSource.StartsWith('<', '!'))
                {
                    SkipXmlComment();
                    _xmlSource.SkipWhiteSpace();
                }

                if (!_xmlSource.EndOfInput)
                    throw new XmlParserException($"Invalid XML document. Unexpected characters after end-tag: {currentNode.Name}");

                return currentNode;
            }
            catch (XmlParserException ex)
            {
                throw new XmlParserException($"{ex.Message} - Line: {_xmlSource.LineNumber}");
            }
        }

        public bool TryReadProcessingInstructions(out IList<XmlParserElement>? processingInstructions)
        {
            _xmlSource.SkipWhiteSpace();

            processingInstructions = null;

            while (_xmlSource.StartsWith('<'))
            {
                if (_xmlSource.StartsWith('<', '!'))
                {
                    SkipXmlComment();
                    _xmlSource.SkipWhiteSpace();
                    continue;
                }

                if (!_xmlSource.TryConsume('<', '?'))
                    break;

                var instructionName = ReadEntityName();
                if (string.IsNullOrEmpty(instructionName))
                    throw new XmlParserException("Invalid XML document. Cannot parse XML processing instruction");

                List<KeyValuePair<string, string>>? instructionAttributes = null;

                try
                {
                    instructionAttributes = TryReadAttributes(expectsProcessingInstruction: true);
                }
                catch (XmlParserException ex)
                {
                    throw new XmlParserException($"{ex.Message} - Cannot parse attributes for XML processing instruction: {instructionName}");
                }

                _xmlSource.SkipWhiteSpace();
                if (!_xmlSource.TryConsume('?','>'))
                    throw new XmlParserException($"Invalid XML document. Cannot parse XML processing instruction: {instructionName}");

                var xmlInstruction = new XmlParserElement(instructionName, instructionAttributes);
                processingInstructions = processingInstructions ?? new List<XmlParserElement>();
                processingInstructions.Add(xmlInstruction);

                _xmlSource.SkipWhiteSpace();
            }

            return processingInstructions != null;
        }

        /// <summary>
        /// Reads a start element.
        /// </summary>
        /// <returns><see langword="true"/> if start element was found.</returns>
        public bool TryReadStartElement(out string? elementName, out List<KeyValuePair<string, string>>? attributes)
        {
            _xmlSource.SkipWhiteSpace();

            if (!_xmlSource.StartsWith('<') ||
                _xmlSource.StartsWith('<', '/') ||
                _xmlSource.StartsWith('<', '!') ||
                _xmlSource.StartsWith('<', '?'))
            {
                elementName = null;
                attributes = null;
                return false;
            }

            _xmlSource.TryConsume('<');

            elementName = ReadEntityName();
            if (string.IsNullOrEmpty(elementName))
                throw new XmlParserException("Invalid XML document. Cannot parse XML start-tag");

            try
            {
                attributes = TryReadAttributes();
                _xmlSource.SkipWhiteSpace();

                // Leave "/>" for TryReadEndElement().
                if (_xmlSource.StartsWith('/', '>'))
                    return true;
            }
            catch (XmlParserException ex)
            {
                throw new XmlParserException($"{ex.Message} - Cannot parse attributes for Start-tag: {elementName}");
            }

            if (!_xmlSource.TryConsume('>'))
                throw new XmlParserException($"Invalid XML document. Cannot parse XML start-tag: {elementName}");
            return true;
        }

        /// <summary>
        /// Reads an end element.
        /// </summary>
        public bool TryReadEndElement(string name)
        {
            _xmlSource.SkipWhiteSpace();

            // Self-closing element.
            if (_xmlSource.TryConsume('/', '>'))
                return true;

            if (!_xmlSource.TryConsume('<', '/'))
                return false;

            if (_xmlSource.Consume(name))
            {
                _xmlSource.SkipWhiteSpace();
                if (_xmlSource.TryConsume('>'))
                    return true;
            }

            throw new XmlParserException($"Invalid XML document. Cannot parse end-tag: {name}");
        }

        /// <summary>
        /// Reads content of an element.
        /// </summary>
        /// <returns>Whether any content was found (including ignored white-spaces and xml comments).</returns>
        public bool TryReadInnerText(out string innerText)
        {
            var parsedSomething = _xmlSource.SkipWhiteSpace();

            innerText = ReadInnerText();

            while (_xmlSource.TryConsume('<', '!'))
            {
                parsedSomething = true;

                if (_xmlSource.TryConsume('-', '-'))
                {
                    // <!-- XML-Comment -->
                    SkipXmlComment(expectHeader: false);
                }
                else if (_xmlSource.StartsWith('[', 'C'))
                {
                    // <![CDATA[some stuff]]>
                    innerText += ReadCDATA();
                }
                else
                {
                    throw new XmlParserException("Invalid XML document. Cannot parse XML comment");
                }

                innerText += ReadInnerText();
            }

            return parsedSomething || !string.IsNullOrEmpty(innerText);
        }

        private string ReadCDATA()
        {
            if (!_xmlSource.Consume("[CDATA["))
                throw new XmlParserException("Invalid XML document. Cannot parse XML CDATA");

            _stringBuilder.ClearBuilder();

            do
            {
                while (_xmlSource.TryConsume(']'))
                {
                    if (_xmlSource.TryConsume(']', '>'))
                        return _stringBuilder.ToString();

                    _stringBuilder.Append(']');
                }

                _stringBuilder.Append(_xmlSource.Current);
            } while (_xmlSource.Read());

            throw new XmlParserException("Invalid XML document. Unclosed XML CDATA");
        }

        private void SkipXmlComment(bool expectHeader = true)
        {
            if (expectHeader && !_xmlSource.Consume("<!--"))
                throw new XmlParserException("Invalid XML document. Cannot parse XML comment");

            do
            {
                while (_xmlSource.TryConsume('-'))
                {
                    if (_xmlSource.TryConsume('-', '>'))
                        return;
                }
            } while (_xmlSource.Read());

            throw new XmlParserException("Invalid XML document. Unexpected end of document. Expected '-->'.");
        }

        private List<KeyValuePair<string, string>>? TryReadAttributes(bool expectsProcessingInstruction = false)
        {
            List<KeyValuePair<string, string>>? attributes = null;

            _xmlSource.SkipWhiteSpace();

            while (!_xmlSource.StartsWith('>')
                && !_xmlSource.StartsWith('/', '>')
                && !(expectsProcessingInstruction && _xmlSource.StartsWith('?', '>')))
            {
                var attributeName = ReadEntityName();
                if (string.IsNullOrEmpty(attributeName))
                    throw new XmlParserException("Invalid XML document. Cannot parse XML attribute");

                _xmlSource.SkipWhiteSpace();

                if (!_xmlSource.TryConsume('='))
                    throw new XmlParserException($"Invalid XML document. Cannot parse XML attribute: {attributeName}"); 

                _xmlSource.SkipWhiteSpace();

                try
                {
                    var attributeValue = ReadAttributeValue();
                    attributes = attributes ?? new List<KeyValuePair<string, string>>();
                    attributes.Add(new KeyValuePair<string, string>(attributeName, attributeValue));
                }
                catch (XmlParserException ex)
                {
                    throw new XmlParserException($"{ex.Message} - XML attribute: {attributeName}");
                }

                _xmlSource.SkipWhiteSpace();
            }

            return attributes;
        }

        private string ReadEntityName()
        {
            _xmlSource.SkipWhiteSpace();

            _stringBuilder.ClearBuilder();

            do
            {
                char chr = _xmlSource.Current;
                if (CharIsSpace(chr) ||
                    chr == '=' ||
                    chr == '>' ||
                    chr == '/' ||
                    chr == '?')
                    break;

                if (!IsValidXmlNameChar(chr))
                    throw new XmlParserException($"Invalid XML document. Invalid XML name character: {chr}");

                _stringBuilder.Append(chr);
            } while (_xmlSource.Read());

            return _stringBuilder.ToString();
        }

        private string ReadAttributeValue()
        {
            char quote;

            if (_xmlSource.TryConsume('"'))
            {
                quote = '"';
            }
            else if (_xmlSource.TryConsume('\''))
            {
                quote = '\'';
            }
            else
            {
                throw new XmlParserException("Invalid XML document. Expected quoted value");
            }

            _stringBuilder.ClearBuilder();

            while (!_xmlSource.TryConsume(quote))
            {
                char chr = _xmlSource.Current;
                if (chr == '<')
                    throw new XmlParserException("Invalid XML document. Cannot parse value with '<', maybe encode to &lt;");

                if (_xmlSource.TryConsume('&'))
                {
                    _stringBuilder.Append(ParseSpecialXmlToken());
                    continue;
                }

                _stringBuilder.Append(chr);
                _xmlSource.Read();
            }

            return _stringBuilder.ToString();
        }

        private string ReadInnerText()
        {
            _stringBuilder.ClearBuilder();

            while (!_xmlSource.StartsWith('<'))
            {
                if (_xmlSource.TryConsume('&'))
                {
                    _stringBuilder.Append(ParseSpecialXmlToken());
                    continue;
                }

                char chr = _xmlSource.Current;
                _xmlSource.Read();
                if (_stringBuilder.Length == 0 && CharIsSpace(chr))
                    continue;   // Trim leading white-spaces

                _stringBuilder.Append(chr);
            }

            return _stringBuilder.ToString(0, TrimEndWhitespace(_stringBuilder));
        }

        private static int TrimEndWhitespace(StringBuilder sb)
        {
            int i = sb.Length - 1;
            while (i >= 0 && CharIsSpace(sb[i]))
                --i;
            return i + 1;
        }

        private static bool IsValidXmlNameChar(char chr)
        {
            if (char.IsLetterOrDigit(chr))
                return true;

            switch (chr)
            {
                case '_':
                case '-':
                case '.':
                case ':':
                    return true;
                default:
                    return false;
            }
        }

        private string ReadUnicodeValue()
        {
            var hexadecimal = _xmlSource.TryConsume('x') || _xmlSource.TryConsume('X');
            var unicode = hexadecimal ? ReadUnicodeHexValue() : ReadUnicodeInteger();
            if ((uint)unicode > 0x10FFFF)
                throw new XmlParserException($"Invalid XML document. Unicode value {unicode} not legal XML character");

            try
            {
                return char.ConvertFromUtf32(unicode);
            }
            catch (ArgumentException ex)
            {
                throw new XmlParserException($"Invalid XML document. Unicode value {unicode} not legal XML character", ex);
            }
        }

        private int ReadUnicodeInteger()
        {
            int unicode = 0;
            bool hasDigit = false;

            do
            {
                char chr = _xmlSource.Current;
                if (chr == ';')
                {
                    if (!hasDigit)
                        throw new XmlParserException("Invalid XML document. Cannot parse unicode-char digit-value");

                    _xmlSource.Read();
                    return unicode;
                }

                unicode *= 10;

                if (chr >= '0' && chr <= '9')
                    unicode += chr - '0';
                else
                    throw new XmlParserException("Invalid XML document. Cannot parse unicode-char digit-value");

                hasDigit = true;
            } while (_xmlSource.Read());

            throw new XmlParserException("Invalid XML document. Cannot parse unicode-char digit-value");
        }

        private int ReadUnicodeHexValue()
        {
            int unicode = 0;
            bool hasDigit = false;

            do
            {
                char chr = _xmlSource.Current;
                if (chr == ';')
                {
                    if (!hasDigit)
                        throw new XmlParserException("Invalid XML document. Cannot parse unicode-char hex-value");

                    _xmlSource.Read();
                    return unicode;
                }

                unicode *= 16;

                if (chr >= '0' && chr <= '9')
                    unicode += chr - '0';
                else if (chr >= 'a' && chr <= 'f')
                    unicode += chr - 'a' + 10;
                else if (chr >= 'A' && chr <= 'F')
                    unicode += chr - 'A' + 10;
                else
                    throw new XmlParserException("Invalid XML document. Cannot parse unicode-char hex-value");

                hasDigit = true;
            } while (_xmlSource.Read());

            throw new XmlParserException("Invalid XML document. Cannot parse unicode-char hex-value");
        }

        private string ParseSpecialXmlToken()
        {
            if (_xmlSource.TryConsume('#'))
                return ReadUnicodeValue();

            // At this point the '&' has already been consumed.
            if (TryConvertSpecialXmlToken("lt;", "<", out var specialToken))
                return specialToken;
            if (TryConvertSpecialXmlToken("gt;", ">", out specialToken))
                return specialToken;
            if (TryConvertSpecialXmlToken("amp;", "&", out specialToken))
                return specialToken;
            if (TryConvertSpecialXmlToken("apos;", "'", out specialToken))
                return specialToken;
            if (TryConvertSpecialXmlToken("quot;", "\"", out specialToken))
                return specialToken;

            return "&"; // Unrecognized special token, return the '&' character as-is.
        }

        private bool TryConvertSpecialXmlToken(string expectedToken, string convertToValue, out string result)
        {
            result = string.Empty;
            if (expectedToken is null || expectedToken.Length < 2)
                return false;

            if (!_xmlSource.StartsWith(expectedToken[0], expectedToken[1]))
                return false;

            if (!_xmlSource.Consume(expectedToken))
                throw new XmlParserException($"Invalid XML document. Cannot parse special token: {expectedToken}");

            result = convertToValue;
            return true;
        }

        private static bool CharIsSpace(char c)
        {
            switch (c)
            {
                case ' ':
                case '\t':
                case '\r':
                case '\n':
                    return true;
                default:
                    return char.IsWhiteSpace(c);
            }
        }

        public sealed class XmlParserElement
        {
            public string Name { get; }
            public string? InnerText { get; set; }
            public IList<XmlParserElement> Children => _children ?? ArrayHelper.Empty<XmlParserElement>();
            private IList<XmlParserElement>? _children;
            public IList<KeyValuePair<string, string>> Attributes => _attributes ?? ArrayHelper.Empty<KeyValuePair<string, string>>();
            private readonly IList<KeyValuePair<string, string>>? _attributes;

            public XmlParserElement(string name, IList<KeyValuePair<string, string>>? attributes)
            {
                Name = name;
                _attributes = attributes;
            }

            public void AddChild(XmlParserElement child)
            {
                if (_children is null)
                    _children = new List<XmlParserElement>();
                _children.Add(child);
            }
        }

        private sealed class InputCursor
        {
            private readonly TextReader _xmlSource;
            private int _lineNumber;
            private char _current;
            private char? _peek;
            private bool _endOfInput;

            public InputCursor(TextReader xmlSource)
            {
                _xmlSource = xmlSource ?? throw new ArgumentNullException(nameof(xmlSource));
                var current = _xmlSource.Read();
                _endOfInput = current < 0;
                _current = _endOfInput ? '\0' : (char)current;
                _lineNumber = current == '\n' ? 2 : 1;
            }

            public char Current
            {
                get
                {
                    if (_endOfInput)
                        throw new XmlParserException("Invalid XML document. Unexpected end of document.");
                    return _current;
                }
            }

            public int LineNumber => _lineNumber;

            public bool EndOfInput => _endOfInput;

            public bool Read()
            {
                if (_peek.HasValue)
                {
                    _current = _peek.Value;
                    _peek = null;
                }
                else
                {
                    var current = _endOfInput ? -1 : _xmlSource.Read();
                    if (current < 0)
                    {
                        _endOfInput = true;
                        return false;
                    }

                    _current = (char)current;
                }

                if (_current == '\n')
                    ++_lineNumber;
                return true;
            }

            private char? Peek()
            {
                if (_peek.HasValue)
                    return _peek.Value;

                var current = _xmlSource.Read();
                if (current < 0)
                    return null;

                _peek = (char)current;
                return _peek.Value;
            }

            public bool StartsWith(char value)
            {
                if (_endOfInput)
                    throw new XmlParserException("Invalid XML document. Unexpected end of document.");

                return _current == value;
            }

            public bool StartsWith(char first, char second)
            {
                if (_endOfInput)
                    throw new XmlParserException("Invalid XML document. Unexpected end of document.");

                return _current == first && Peek() == second;
            }

            public bool TryConsume(char value)
            {
                if (!StartsWith(value))
                    return false;

                Read();
                return true;
            }

            public bool TryConsume(char first, char second)
            {
                if (!StartsWith(first, second))
                    return false;

                Read();
                Read();
                return true;
            }

            public bool Consume(string value)
            {
                foreach (var chr in value)
                {
                    if (chr != _current)
                        return false;
                    if (!Read())
                        return false;
                }
                return true;
            }

            public bool SkipWhiteSpace()
            {
                var skipped = false;

                while (CharIsSpace(_current))
                {
                    skipped = true;
                    if (!Read())
                        break;
                }

                return skipped;
            }
        }
    }
}
