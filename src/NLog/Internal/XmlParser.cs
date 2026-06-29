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
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using NLog.Config;

    /// <summary>
    /// A minimal XML reader, because .NET System.Xml.XmlReader doesn't work with AOT
    /// </summary>
    internal sealed class XmlParser
    {
        private readonly CharEnumerator _xmlSource;
        private readonly StringBuilder _stringBuilder = new StringBuilder();

        public XmlParser(TextReader xmlSource)
        {
            _xmlSource = new CharEnumerator(xmlSource);
        }

        public XmlParser(string xmlSource)
        {
            _xmlSource = new CharEnumerator(new StringReader(xmlSource));
        }

        public XmlParserElement LoadDocument(out IList<XmlParserElement>? processingInstructions)
        {
            try
            {
                TryReadProcessingInstructions(out processingInstructions);

                if (!TryReadStartElement(out var rootName, out var rootAttributes))
                    throw new XmlParserException("Invalid XML document. Cannot parse root start-tag");

                var stack = new Stack<XmlParserElement>();
                var currentRoot = new XmlParserElement(rootName ?? string.Empty, rootAttributes);
                stack.Push(currentRoot);

                bool stillReading = true;

                while (stillReading)
                {
                    stillReading = false;

                    if (TryReadEndElement(currentRoot.Name))
                    {
                        stillReading = true;
                        stack.Pop();
                        if (stack.Count == 0)
                            break;

                        currentRoot = stack.Peek();
                    }

                    try
                    {
                        if (TryReadInnerText(out var innerText))
                        {
                            stillReading = true;
                            currentRoot.InnerText += innerText;
                        }

                        if (TryReadStartElement(out var elementName, out var elementAttributes))
                        {
                            stillReading = true;
                            currentRoot = new XmlParserElement(elementName ?? string.Empty, elementAttributes);
                            stack.Peek().AddChild(currentRoot);
                            stack.Push(currentRoot);
                        }
                    }
                    catch (XmlParserException ex)
                    {
                        throw new XmlParserException(ex.Message + $" - Start-tag: {currentRoot.Name}");
                    }
                }

                if (!stillReading)
                    throw new XmlParserException($"Invalid XML document. Cannot parse end-tag: {currentRoot.Name}");

                SkipWhiteSpaces();
                while (_xmlSource.Peek() == '!' && _xmlSource.Current == '<')
                {
                    _xmlSource.MoveNext();
                    SkipXmlComment();
                }
                if (_xmlSource.MoveNext())
                    throw new XmlParserException($"Invalid XML document. Unexpected characters after end-tag: {currentRoot.Name}");

                return currentRoot;
            }
            catch (XmlParserException ex)
            {
                throw new XmlParserException($"{ex.Message} - Line: {_xmlSource.LineNumber}");
            }
        }

        public bool TryReadProcessingInstructions(out IList<XmlParserElement>? processingInstructions)
        {
            SkipWhiteSpaces();

            processingInstructions = null;

            while (_xmlSource.Current == '<')
            {
                if (_xmlSource.Peek() == '!')
                {
                    // Skip XML comments before instructions or root element
                    _xmlSource.MoveNext();
                    SkipXmlComment();
                    continue;
                }
                if (_xmlSource.Peek() != '?')
                    break;
                SkipChar('<');
                SkipChar('?');

                var instructionName = ReadEntityName();
                if (string.IsNullOrEmpty(instructionName))
                    throw new XmlParserException("Invalid XML document. Cannot parse XML processing instruction");

                List<KeyValuePair<string, string>>? instructionAttributes = null;

                if (_xmlSource.Current != '?' && _xmlSource.Peek() != '>')
                {
                    _ = TryReadAttributes(out instructionAttributes, expectsProcessingInstruction: true);
                    SkipWhiteSpaces();
                }

                if (!SkipChar('?') || !SkipChar('>'))
                    throw new XmlParserException($"Invalid XML document. Cannot parse XML processing instruction: {instructionName}");

                var xmlInstruction = new XmlParserElement(instructionName, instructionAttributes);
                processingInstructions = processingInstructions ?? new List<XmlParserElement>();
                processingInstructions.Add(xmlInstruction);

                SkipWhiteSpaces();
            }

            return processingInstructions != null;
        }

        /// <summary>
        /// Reads a start element.
        /// </summary>
        /// <returns><see langword="true"/> if start element was found.</returns>
        public bool TryReadStartElement(out string? elementName, out List<KeyValuePair<string, string>>? attributes)
        {
            SkipWhiteSpaces();

            if (_xmlSource.Current == '<' && _xmlSource.Peek() != '/' && _xmlSource.Peek() != '!')
            {
                SkipChar('<');

                elementName = ReadEntityName();
                if (string.IsNullOrEmpty(elementName))
                    throw new XmlParserException("Invalid XML document. Cannot parse XML start-tag");

                try
                {
                    _ = TryReadAttributes(out attributes);
                    SkipChar('>');
                }
                catch (XmlParserException ex)
                {
                    throw new XmlParserException(ex.Message + $" - Cannot parse attributes for Start-tag: {elementName}");
                }
                return true;
            }

            elementName = null;
            attributes = null;
            return false;
        }

        /// <summary>
        /// Skips an end element.
        /// </summary>
        /// <param name="name">The name of the element to skip.</param>
        /// <returns><see langword="true"/> if an end element was skipped; otherwise, <see langword="false"/>.</returns>
        public bool TryReadEndElement(string name)
        {
            _ = SkipWhiteSpaces();

            if (_xmlSource.Current == '<' && _xmlSource.Peek() != '/')
                return false;

            if (_xmlSource.Current == '/' && _xmlSource.Peek() == '>')
                return SkipChar('/') && SkipChar('>');    // Self-closing element

            if (!SkipChar('<'))
                return false;

            if (!SkipChar('/'))
                throw new XmlParserException($"Invalid XML document. Cannot parse end-tag: {name}");

            foreach (var chr in name)
            {
                if (_xmlSource.Current != chr || !_xmlSource.MoveNext())
                    throw new XmlParserException($"Invalid XML document. Cannot parse end-tag: {name}");
            }

            if (!SkipChar('>'))
                throw new XmlParserException($"Invalid XML document. Cannot parse end-tag: {name}");

            return true;
        }

        /// <summary>
        /// Reads content of an element.
        /// </summary>
        /// <returns>The content of the element.</returns>
        public bool TryReadInnerText(out string innerText)
        {
            var currentChar = _xmlSource.Current;

            SkipWhiteSpaces();

            innerText = ReadUntilChar('<');

            while (_xmlSource.Current == '<' && _xmlSource.Peek() == '!')
            {
                _xmlSource.MoveNext();
                currentChar = _xmlSource.Current;
                if (_xmlSource.Peek() == '-')
                {
                    // <!-- XML-Comment -->
                    SkipXmlComment();
                }
                else if (_xmlSource.Peek() == '[')
                {
                    // <![CDATA[some stuff]]>
                    innerText += ReadCDATA();
                }
                else
                {
                    throw new XmlParserException($"Invalid XML document. Cannot parse XML comment");
                }

                innerText += ReadUntilChar('<');
            }

            SkipWhiteSpaces();

            if (!string.IsNullOrEmpty(innerText))
                return true;

            return _xmlSource.Current != '<' || currentChar != '<';
        }

        private string ReadCDATA()
        {
            if (!SkipCDATA())
                throw new XmlParserException("Invalid XML document. Cannot parse XML CDATA");

            _stringBuilder.ClearBuilder();

            do
            {
                if (_xmlSource.Current == ']' && _xmlSource.Peek() == ']')
                {
                    _xmlSource.MoveNext();
                    if (_xmlSource.Peek() == '>')
                    {
                        _xmlSource.MoveNext();
                        _xmlSource.MoveNext();
                        break;
                    }

                    _stringBuilder.Append(']');
                }

                _stringBuilder.Append(_xmlSource.Current);
            } while (_xmlSource.MoveNext());

            SkipWhiteSpaces();
            return _stringBuilder.ToString();
        }

        private bool SkipCDATA()
        {
            if (!SkipChar('!'))
                return false;
            if (!SkipChar('['))
                return false;
            if (!SkipChar('C'))
                return false;
            if (!SkipChar('D'))
                return false;
            if (!SkipChar('A'))
                return false;
            if (!SkipChar('T'))
                return false;
            if (!SkipChar('A'))
                return false;
            if (!SkipChar('['))
                return false;
            return true;
        }

        private void SkipXmlComment()
        {
            if (!SkipChar('!') || !SkipChar('-') || !SkipChar('-'))
                throw new XmlParserException("Invalid XML document. Cannot parse XML comment");

            while (_xmlSource.MoveNext())
            {
                if (!SkipChar('-'))
                    continue;

                if (SkipChar('-') && SkipChar('>'))
                    break;
            }

            SkipWhiteSpaces();
        }

        private bool TryReadAttributes(out List<KeyValuePair<string, string>>? attributes, bool expectsProcessingInstruction = false)
        {
            SkipWhiteSpaces();

            attributes = null;

            while (_xmlSource.Current != '>' && _xmlSource.Current != '/' && (!expectsProcessingInstruction || _xmlSource.Current != '?'))
            {
                var attributeName = ReadEntityName();
                if (string.IsNullOrEmpty(attributeName))
                    throw new XmlParserException("Invalid XML document. Cannot parse XML attribute");

                if (!SkipChar('='))
                    throw new XmlParserException($"Invalid XML document. Cannot parse XML attribute: {attributeName}");

                SkipWhiteSpaces();

                bool isSingleQuote;
                if (SkipChar('"'))
                    isSingleQuote = false;
                else if (SkipChar('\''))
                    isSingleQuote = true;
                else
                    throw new XmlParserException($"Invalid XML document. Cannot parse XML attribute: {attributeName}");

                string attributeValue;
                try
                {
                    attributeValue = ReadUntilChar(isSingleQuote ? '\'' : '"');
                }
                catch (XmlParserException ex)
                {
                    throw new XmlParserException(ex.Message + $" - XML attribute: {attributeName}");
                }

                if (!SkipChar(isSingleQuote ? '\'' : '"'))
                    throw new XmlParserException($"Invalid XML document. Unclosed attribute value: {attributeName}");

                attributes = attributes ?? new List<KeyValuePair<string, string>>();
                attributes.Add(new KeyValuePair<string, string>(attributeName, attributeValue));

                SkipWhiteSpaces();
            }

            return attributes != null;
        }

        private string ReadEntityName()
        {
            SkipWhiteSpaces();

            _stringBuilder.ClearBuilder();

            do
            {
                char chr = _xmlSource.Current;
                if (CharIsSpace(chr) || chr == '=' || chr == '>' || chr == '/' || chr == '?')
                    break;

                if (!IsValidXmlNameChar(chr))
                    throw new XmlParserException($"Invalid XML document. Invalid XML name character: {chr}");

                _stringBuilder.Append(chr);
            } while (_xmlSource.MoveNext());

            SkipWhiteSpaces();

            return _stringBuilder.ToString();
        }

        private bool SkipChar(char c)
        {
            if (_xmlSource.Current != c)
            {
                return false;
            }

            _xmlSource.MoveNext();
            return true;
        }

        private bool SkipWhiteSpaces()
        {
            bool skipped = false;
            while (!_xmlSource.EndOfFile && CharIsSpace(_xmlSource.Current) && _xmlSource.MoveNext())
            {
                skipped = true;
            }
            return skipped;
        }

        private string ReadUntilChar(char terminator)
        {
            _stringBuilder.ClearBuilder();

            bool readingInnerText = terminator == '<';

            do
            {
                char chr = _xmlSource.Current;
                if (chr == terminator)
                    break;

                if (chr == '&')
                {
                    _xmlSource.MoveNext();

                    if (_xmlSource.Current == '#' && TryParseUnicodeChar(out var unicodeChar))
                    {
                        _stringBuilder.Append(unicodeChar);
                    }
                    else if (TryParseSpecialXmlToken(out var specialToken))
                    {
                        _stringBuilder.Append(specialToken);
                    }
                    else
                    {
                        _stringBuilder.Append('&');
                        if (_xmlSource.Current == terminator)
                            break;
                        _stringBuilder.Append(_xmlSource.Current);
                    }
                }
                else if (readingInnerText)
                {
                    if (_stringBuilder.Length == 0 && CharIsSpace(chr))
                        continue;

                    _stringBuilder.Append(chr);
                }
                else
                {
                    if (chr == '<')
                        throw new XmlParserException($"Invalid XML document. Cannot parse value with '<', maybe encode to &lt;");
                    _stringBuilder.Append(chr);
                }
            } while (_xmlSource.MoveNext());

            var value = _stringBuilder.ToString();
            return readingInnerText ? value.TrimEnd(ArrayHelper.Empty<char>()) : value;
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

        private bool TryParseUnicodeChar(out string unicodeChar)
        {
            var peekChr = _xmlSource.Peek();
            if (peekChr == 'x' || peekChr == 'X')
            {
                _xmlSource.MoveNext();
                int unicode = TryParseUnicodeValueHex();
                if (!IsLegalXmlCodePoint(unicode))
                    throw new XmlParserException("Invalid XML document. Unicode value not legal XML character");
                unicodeChar = char.ConvertFromUtf32(unicode);
                return true;
            }
            else if (peekChr >= '0' && peekChr <= '9')
            {
                int unicode = TryParseUnicodeValue();
                if (!IsLegalXmlCodePoint(unicode))
                    throw new XmlParserException("Invalid XML document. Unicode value not legal XML character");
                unicodeChar = char.ConvertFromUtf32(unicode);
                return true;
            }
            unicodeChar = string.Empty;
            return false;
        }

        private int TryParseUnicodeValue()
        {
            int unicode = 0;
            bool? terminated = null;
            while (_xmlSource.MoveNext())
            {
                var chr = _xmlSource.Current;
                if (chr == ';')
                { 
                    if (!terminated.HasValue)
                        throw new XmlParserException("Invalid XML document. Cannot parse unicode-char digit-value");
                    terminated = true;
                    break;
                }

                terminated = false;

                unicode *= 10;

                if (chr >= '0' && chr <= '9')
                    unicode += chr - '0';
                else
                    throw new XmlParserException("Invalid XML document. Cannot parse unicode-char digit-value");
            }

            if (terminated != true)
                throw new XmlParserException("Invalid XML document. Cannot parse unicode-char digit-value");

            return unicode;
        }

        private int TryParseUnicodeValueHex()
        {
            int unicode = 0;
            bool? terminated = null;
            while (_xmlSource.MoveNext())
            {
                var chr = _xmlSource.Current;
                if (chr == ';')
                {
                    if (!terminated.HasValue)
                        throw new XmlParserException("Invalid XML document. Cannot parse unicode-char hex-value");
                    terminated = true;
                    break;
                }

                terminated = false;

                unicode *= 16;

                var chrUpper = char.ToUpperInvariant(chr);
                if (chrUpper >= 'A' && chrUpper <= 'F')
                    unicode += chrUpper - 'A' + 10;
                else if (chrUpper >= '0' && chrUpper <= '9')
                    unicode += chrUpper - '0';
                else
                    throw new XmlParserException("Invalid XML document. Cannot parse unicode-char hex-value");
            }

            if (terminated != true)
                throw new XmlParserException("Invalid XML document. Cannot parse unicode-char hex-value");

            return unicode;
        }

        private static bool IsLegalXmlCodePoint(int codePoint)
        {
            // Fast reject: outside Unicode range
            if ((uint)codePoint > 0x10FFFF)
                return false;

            // Standard XML characters
            if ((codePoint >= 0x20 && codePoint <= 0xD7FF) ||
                (codePoint >= 0xE000 && codePoint <= 0xFFFD))
                return true;

            // Rare BMP control characters allowed by XML
            if (codePoint == 0x09 || codePoint == 0x0A || codePoint == 0x0D)
                return true;

            // Supplementary planes (less common, but valid)
            if (codePoint >= 0x10000)
                return true;

            return false;
        }

        private bool TryParseSpecialXmlToken(out char specialToken)
        {
            if (TryConvertSpecialXmlToken("lt;", '<', out specialToken))
                return true;
            if (TryConvertSpecialXmlToken("gt;", '>', out specialToken))
                return true;
            if (TryConvertSpecialXmlToken("amp;", '&', out specialToken))
                return true;
            if (TryConvertSpecialXmlToken("apos;", '\'', out specialToken))
                return true;
            if (TryConvertSpecialXmlToken("quot;", '\"', out specialToken))
                return true;

            return false;
        }

        private bool TryConvertSpecialXmlToken(string expectedToken, char convertToValue, out char result)
        {
            result = '\0';
            if (expectedToken is null || expectedToken.Length < 2)
                return false;

            if (_xmlSource.Current != expectedToken[0] || _xmlSource.Peek() != expectedToken[1])
                return false;

            for (int i = 0; i < expectedToken.Length - 1; ++i)
            {
                if (!SkipChar(expectedToken[i]))
                    throw new XmlParserException($"Invalid XML document. Cannot parse special token: {expectedToken}");
            }

            if (_xmlSource.Current != expectedToken[expectedToken.Length - 1])
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
            public string Name { get; set; }
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

        private sealed class CharEnumerator : IEnumerator<char>
        {
            private readonly TextReader _xmlSource;
            private int _lineNumber;
            private char _current;
            private char? _peek;
            private bool _endOfFile;

            public CharEnumerator(TextReader xmlSource)
            {
                _xmlSource = xmlSource;
                var current = xmlSource.Read();
                _current = current < 0 ? '\0' : (char)current;
                _lineNumber = current == '\n' ? 2 : 1;
            }

            public char Current
            {
                get
                {
                    if (_endOfFile)
                        throw new XmlParserException("Invalid XML document. Unexpected end of document.");
                    return _current;
                }
            }

            public int LineNumber => _lineNumber;

            object IEnumerator.Current => Current;

            public bool EndOfFile => _endOfFile;

            public bool MoveNext()
            {
                if (_peek.HasValue)
                {
                    _current = _peek.Value;
                    if (_current == '\n')
                        ++_lineNumber;
                    _peek = null;
                    return true;
                }

                var current = _xmlSource.Read();
                if (current < 0)
                {
                    _endOfFile = true;
                    return false;
                }

                _current = (char)current;
                if (_current == '\n')
                    ++_lineNumber;
                return true;
            }

            public char Peek()
            {
                if (_peek.HasValue)
                    return _peek.Value;

                var current = _xmlSource.Read();
                if (current < 0)
                    return '\0';
                _peek = (char)current;
                return _peek.Value;
            }

            void IEnumerator.Reset()
            {
                // NOSONAR: Nothing to reset
            }

            void IDisposable.Dispose()
            {
                // NOSONAR: Nothing to dispose
            }
        }
    }
}
