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
    using System.Linq;
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

        public XmlParserElement LoadDocument(out IList<XmlParserElement> processingInstructions)
        {
            try
            {
                TryReadProcessingInstructions(out processingInstructions);

                if (!TryReadStartElement(out var rootName, out var rootAttributes))
                    throw new XmlParserException("Invalid XML document. Cannot parse root start-tag");

                Stack<XmlParserElement> stack = new Stack<XmlParserElement>();

                var currentRoot = new XmlParserElement(rootName, rootAttributes);
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
                            currentRoot = new XmlParserElement(elementName, elementAttributes);
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
                if (_xmlSource.MoveNext())
                    throw new XmlParserException($"Invalid XML document. Unexpected characters after end-tag: {currentRoot.Name}");

                return currentRoot;
            }
            catch (XmlParserException ex)
            {
                throw new XmlParserException(ex.Message + $" - Line: {_xmlSource.LineNumber}");
            }
        }

        public bool TryReadProcessingInstructions(out IList<XmlParserElement> processingInstructions)
        {
            SkipWhiteSpaces();

            processingInstructions = null;

            while (_xmlSource.Current == '<' && _xmlSource.Peek() == '?')
            {
                if (!TryBeginReadStartElement(out var instructionName, processingInstruction: true))
                    throw new XmlParserException("Invalid XML document. Cannot parse XML processing instruction");

                if (string.IsNullOrEmpty(instructionName) || instructionName.Length == 1 || instructionName[0] != '?')
                    throw new XmlParserException("Invalid XML document. Cannot parse XML processing instruction");

                instructionName = instructionName.Substring(1);

                TryReadAttributes(out var instructionAttributes, expectsProcessingInstruction: true);

                if (!SkipChar('?'))
                    throw new XmlParserException($"Invalid XML document. Cannot parse XML processing instruction: {instructionName}");
                if (!SkipChar('>'))
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
        /// <returns>True if start element was found.</returns>
        /// <exception cref="Exception">Something unexpected has failed.</exception>
        public bool TryReadStartElement(out string name, out List<KeyValuePair<string, string>> attributes)
        {
            SkipWhiteSpaces();

            if (TryBeginReadStartElement(out name))
            {
                try
                {
                    TryReadAttributes(out attributes);
                    SkipChar('>');
                }
                catch (XmlParserException ex)
                {
                    throw new XmlParserException(ex.Message + $" - Cannot parse attributes for Start-tag: {name}");
                }
                return true;
            }

            name = default;
            attributes = default;
            return false;
        }

        /// <summary>
        /// Skips an end element.
        /// </summary>
        /// <param name="name">The name of the element to skip.</param>
        /// <returns>True if an end element was skipped; otherwise, false.</returns>
        /// <exception cref="Exception">Something unexpected has failed.</exception>
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

            for (var i = 0; i < name.Length; i++)
            {
                var chr = _xmlSource.Current;
                if (chr != name[i])
                    throw new XmlParserException($"Invalid XML document. Cannot parse end-tag: {name}");

                if (!_xmlSource.MoveNext())
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
        /// <exception cref="Exception">Something unexpected has failed.</exception>
        public bool TryReadInnerText(out string innerText)
        {
            var currentChar = _xmlSource.Current;

            SkipWhiteSpaces();

            innerText = ReadUntilChar('<', includeSpaces: true);

            while (_xmlSource.Current == '<' && _xmlSource.Peek() == '!')
            {
                _xmlSource.MoveNext();
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

                innerText += ReadUntilChar('<', includeSpaces: true);
            }

            SkipWhiteSpaces();
            if (string.IsNullOrEmpty(innerText) && _xmlSource.Current == '<')
                return currentChar != '<';
            else
                return true;
        }

        private string ReadCDATA()
        {
            string contentValue;
            if (!SkipChar('!') || !SkipChar('[') || !SkipChar('C') || !SkipChar('D') || !SkipChar('A') || !SkipChar('T') || !SkipChar('A') || !SkipChar('['))
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

            contentValue = _stringBuilder.ToString();
            SkipWhiteSpaces();
            return contentValue;
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

        /// <exception cref="Exception">Something unexpected has failed.</exception>
        private bool TryReadAttributes(out List<KeyValuePair<string, string>> attributes, bool expectsProcessingInstruction = false)
        {
            SkipWhiteSpaces();

            attributes = null;

            while (_xmlSource.Current != '>' && _xmlSource.Current != '/' && (!expectsProcessingInstruction || _xmlSource.Current != '?'))
            {
                var attName = ReadUntilChar('=').Trim();
                if (string.IsNullOrEmpty(attName))
                    throw new XmlParserException("Invalid XML document. Cannot parse XML attribute");

                if (!SkipChar('='))
                    throw new XmlParserException("Invalid XML document. Cannot parse XML attribute");

                var isApostrophe = false;

                SkipWhiteSpaces();

                if (!SkipChar('"'))
                {
                    if (SkipChar('\''))
                    {
                        isApostrophe = true;
                    }
                    else
                    {
                        throw new XmlParserException($"Invalid XML document. Cannot parse XML attribute: {attName}");
                    }
                }

                try
                {
                    var attValue = ReadUntilChar(isApostrophe ? '\'' : '"', includeSpaces: true);
                    _xmlSource.MoveNext();

                    attributes = attributes ?? new List<KeyValuePair<string, string>>();
                    attributes.Add(new KeyValuePair<string, string>(attName, attValue));

                    SkipWhiteSpaces();
                }
                catch (XmlParserException ex)
                {
                    throw new XmlParserException(ex.Message + $" - XML attribute: {attName}");
                }
            }

            return attributes != null;
        }

        /// <summary>
        /// Consumer of this method should handle safe position.
        /// </summary>
        /// <exception cref="Exception">Something unexpected has failed.</exception>
        private bool TryBeginReadStartElement(out string name, bool processingInstruction = false)
        {
            if (_xmlSource.Current != '<' || _xmlSource.Peek() == '/' || _xmlSource.Peek() == '!')
            {
                name = default;
                return false;
            }

            _xmlSource.MoveNext();

            SkipWhiteSpaces();

            _stringBuilder.ClearBuilder();

            do
            {
                var chr = _xmlSource.Current;
                if (CharIsSpace(chr) || chr == '/' || chr == '>')
                {
                    break;
                }

                if (processingInstruction && chr == '?')
                {
                    if (_stringBuilder.Length != 0)
                        throw new XmlParserException($"Invalid XML document. Cannot parse XML start-tag with character: {chr}");
                }
                else if (!IsValidXmlNameChar(chr))
                {
                    throw new XmlParserException($"Invalid XML document. Cannot parse XML start-tag with character: {chr}");
                }

                _stringBuilder.Append(chr);
            } while (_xmlSource.MoveNext());

            name = _stringBuilder.ToString();
            if (string.IsNullOrEmpty(name))
                throw new XmlParserException($"Invalid XML document. Cannot parse XML start-tag");

            return true;
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
            var skipped = false;
            while (!_xmlSource.EndOfFile && CharIsSpace(_xmlSource.Current) && _xmlSource.MoveNext())
            {
                skipped = true;
            }
            return skipped;
        }

        /// <exception cref="Exception">Something unexpected has failed.</exception>
        private string ReadUntilChar(char expectedChar, bool includeSpaces = false)
        {
            _stringBuilder.ClearBuilder();

            bool trimEnd = false;

            do
            {
                var chr = _xmlSource.Current;
                if (chr == expectedChar)
                {
                    break;
                }

                if (!includeSpaces && CharIsSpace(chr))
                {
                    SkipWhiteSpaces();
                    if (_xmlSource.Current == expectedChar)
                        break;
                    throw new XmlParserException($"Invalid XML document. Cannot parse attribute-name with white-space");
                }
                else if (!includeSpaces && !IsValidXmlNameChar(chr))
                {
                    throw new XmlParserException($"Invalid XML document. Cannot parse attribute-name with character: {chr}");
                }

                if (chr == '<' && (!includeSpaces || expectedChar == '<'))
                    throw new XmlParserException($"Invalid XML document. Cannot parse value with '<'");

                if (chr == '>' && (!includeSpaces || expectedChar == '<'))
                    throw new XmlParserException($"Invalid XML document. Cannot parse value with '>'");

                if (includeSpaces && chr == '&')
                {
                    _xmlSource.MoveNext();
                    if (_xmlSource.Current == '#' && char.IsDigit(_xmlSource.Peek()))
                    {
                        int unicode = TryParseUnicodeValue();
                        if (unicode != 0)
                            _stringBuilder.Append((char)unicode);
                    }
                    else if (_xmlSource.Current == '#' && (_xmlSource.Peek() == 'x' || _xmlSource.Peek() == 'X'))
                    {
                        _xmlSource.MoveNext();
                        int unicode = TryParseUnicodeValueHex();
                        if (unicode != 0)
                            _stringBuilder.Append((char)unicode);
                    }
                    else if (TryParseSpecialXmlToken(out var specialToken))
                    {
                        _stringBuilder.Append(specialToken);
                    }
                    else
                    {
                        _stringBuilder.Append('&');
                        if (_xmlSource.Current == expectedChar)
                            break;
                        _stringBuilder.Append(_xmlSource.Current);
                    }
                }
                else
                {
                    if (includeSpaces && expectedChar == '<')
                    {
                        if (_stringBuilder.Length == 0 && CharIsSpace(chr))
                            continue;

                        trimEnd = !trimEnd && CharIsSpace(chr);
                    }
                    _stringBuilder.Append(chr);
                }
            } while (_xmlSource.MoveNext());

            var value = _stringBuilder.ToString();
            return trimEnd ? value.TrimEnd(ArrayHelper.Empty<char>()) : value;
        }

        private static bool IsValidXmlNameChar(char chr)
        {
            if (char.IsLetter(chr) || char.IsDigit(chr))
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

        private int TryParseUnicodeValue()
        {
            int unicode = '\0';
            while (_xmlSource.MoveNext())
            {
                if (_xmlSource.Current == ';')
                    break;

                if (_xmlSource.Current < '0' || _xmlSource.Current > '9')
                    throw new XmlParserException("Invalid XML document. Cannot parse unicode-char digit-value");

                unicode *= 10;
                unicode += _xmlSource.Current - '0';
            }

            if (unicode >= '\uffff')
                throw new XmlParserException("Invalid XML document. Unicode value exceeds maximum allowed value");

            return unicode;
        }

        private int TryParseUnicodeValueHex()
        {
            int unicode = '\0';
            while (_xmlSource.MoveNext())
            {
                if (_xmlSource.Current == ';')
                    break;

                unicode *= 16;
                if ("abcdef".Contains(char.ToLower(_xmlSource.Current)))
                    unicode += int.Parse(_xmlSource.Current.ToString(), System.Globalization.NumberStyles.HexNumber);
                else if (_xmlSource.Current < '0' || _xmlSource.Current > '9')
                    throw new XmlParserException("Invalid XML document. Cannot parse unicode-char hex-value");
                else
                    unicode += _xmlSource.Current - '0';
            }

            if (unicode >= '\uffff')
                throw new XmlParserException("Invalid XML document. Unicode value exceeds maximum allowed value");

            return unicode;
        }

        private bool TryParseSpecialXmlToken(out string xmlToken)
        {
            foreach (var token in _specialTokens)
            {
                if (_xmlSource.Current == token.Key[0] && _xmlSource.Peek() == token.Key[1])
                {
                    foreach (var tokenChr in token.Key)
                        if (!SkipChar(tokenChr))
                            throw new XmlParserException($"Invalid XML document. Cannot parse special token: {token.Key}");
                    if (_xmlSource.Current != ';')
                        throw new XmlParserException($"Invalid XML document. Cannot parse special token: {token.Key}");
                    xmlToken = token.Value;
                    return true;
                }
            }

            xmlToken = null;
            return false;
        }

        private static readonly Dictionary<string, string> _specialTokens = new Dictionary<string, string>()
        {
            { "amp", "&" },
            { "AMP", "&" },
            { "apos", "\'" },
            { "APOS", "\'" },
            { "quot", "\"" },
            { "QUOT", "\"" },
            { "lt", "<" },
            { "LT", "<" },
            { "gt", ">" },
            { "GT", ">" },
        };

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
            public string InnerText { get; set; }
            public IList<XmlParserElement> Children { get; private set; }
            public IList<KeyValuePair<string, string>> Attributes { get; }

            public XmlParserElement(string name, IList<KeyValuePair<string, string>> attributes)
            {
                Name = name;
                Attributes = attributes;
            }

            public void AddChild(XmlParserElement child)
            {
                if (Children is null)
                    Children = new List<XmlParserElement>();
                Children.Add(child);
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
                        throw new XmlParserException($"Invalid XML document. Unexpected end of document.");
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
