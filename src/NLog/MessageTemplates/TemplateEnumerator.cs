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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NLog.MessageTemplates
{
    /// <summary>
    /// Parse templates.
    /// </summary>
    internal struct TemplateEnumerator : IEnumerator<LiteralHole>
    {
        private static readonly char[] HoleDelimiters = { '}', ':', ',' };
        private static readonly char[] TextDelimiters = { '{', '}' };

        private string _template;
        private int _length;
        private int _position;
        private int _literalLength;
        private LiteralHole _current;
        private const short Zero = 0;

        /// <summary>
        /// Parse a template.
        /// </summary>
        /// <param name="template">Template to be parsed.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="template"/> is null.</exception>
        /// <returns>Template, never null</returns>
        public TemplateEnumerator(string template)
        {
            _template = template ?? throw new ArgumentNullException(nameof(template));
            _length = _template.Length;
            _position = 0;
            _literalLength = 0;
            _current = default(LiteralHole);
        }

        /// <summary>
        /// Gets the current literal/hole in the template
        /// </summary>
        public LiteralHole Current => _current;

        object System.Collections.IEnumerator.Current => _current;

        /// <summary>
        /// Clears the enumerator
        /// </summary>
        public void Dispose()
        {
            _template = string.Empty;
            _length = 0;
            Reset();
        }

        /// <summary>
        /// Restarts the enumerator of the template
        /// </summary>
        public void Reset()
        {
            _position = 0;
            _literalLength = 0;
            _current = default(LiteralHole);
        }

        /// <summary>
        /// Moves to the next literal/hole in the template
        /// </summary>
        /// <returns>Found new element [true/false]</returns>
        public bool MoveNext()
        {
            try
            {
                while (_position < _length)
                {
                    char c = Peek();
                    if (c == '{')
                    {
                        ParseOpenBracketPart();
                        return true;
                    }
                    else if (c == '}')
                    {
                        ParseCloseBracketPart();
                        return true;
                    }
                    else
                        ParseTextPart();
                }
                if (_literalLength != 0)
                {
                    AddLiteral();
                    return true;
                }
                return false;
            }
            catch (IndexOutOfRangeException)
            {
                throw new TemplateParserException("Unexpected end of template.", _position, _template);
            }
        }

        private void AddLiteral()
        {
            _current = new LiteralHole(new Literal(_literalLength, Zero), default(Hole));
            _literalLength = 0;
        }

        private void ParseTextPart()
        {
            _literalLength = SkipUntil(TextDelimiters, required: false);
        }

        private void ParseOpenBracketPart()
        {
            Skip('{');
            char c = Peek();
            switch (c)
            {
                case '{':
                    Skip('{');
                    _literalLength++;
                    AddLiteral();
                    return;
                case '@':
                    Skip('@');
                    ParseHole(CaptureType.Serialize);
                    return;
                case '$':
                    Skip('$');
                    ParseHole(CaptureType.Stringify);
                    return;
                default:
                    ParseHole(CaptureType.Normal);
                    return;
            }
        }

        private void ParseCloseBracketPart()
        {
            Skip('}');
            if (Read() != '}')
                throw new TemplateParserException("Unexpected '}}' ", _position - 2, _template);
            _literalLength++;
            AddLiteral();
        }

        private void ParseHole(CaptureType type)
        {
            int start = _position;
            string name = ParseName(out var position);
            int alignment = 0;
            string format = null;
            if (Peek() != '}')
            {
                alignment = Peek() == ',' ? ParseAlignment() : 0;
                format = Peek() == ':' ? ParseFormat() : null;
                Skip('}');
            }
            else
            {
                _position++;
            }

            int literalSkip = _position - start + (type == CaptureType.Normal ? 1 : 2);     // Account for skipped '{', '{$' or '{@'
            _current = new LiteralHole(new Literal(_literalLength, literalSkip), new Hole(
                name,
                format,
                type,
                (short)position,
                (short)alignment
            ));
            _literalLength = 0;
        }

        private string ParseName(out int parameterIndex)
        {
            parameterIndex = -1;
            char c = Peek();
            // If the name matches /^\d+ *$/ we consider it positional
            if (c >= '0' && c <= '9')
            {
                int start = _position;
                int parsed = ReadInt();
                c = Peek();
                if (parsed >= 0)
                {
                    if (c == '}' || c == ':' || c == ',')
                    {
                        // Non-allocating positional hole-name-parsing
                        parameterIndex = parsed;
                        return ParameterIndexToString(parameterIndex);
                    }

                    if (c == ' ')
                    {
                        SkipSpaces();
                        c = Peek();
                        if (c == '}' || c == ':' || c == ',')
                            parameterIndex = parsed;
                    }
                }

                _position = start;
            }

            return ReadUntil(HoleDelimiters);
        }

        private static string ParameterIndexToString(int parameterIndex)
        {
            switch (parameterIndex)
            {
                case 0: return "0";
                case 1: return "1";
                case 2: return "2";
                case 3: return "3";
                case 4: return "4";
                case 5: return "5";
                case 6: return "6";
                case 7: return "7";
                case 8: return "8";
                case 9: return "9";
            }

            return parameterIndex.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Parse format after hole name/index. Handle the escaped { and } in the format. Don't read the last }
        /// </summary>
        /// <returns></returns>
        private string ParseFormat()
        {

            Skip(':');
            string format = ReadUntil(TextDelimiters);
            while (true)
            {
                var c = Read();

                switch (c)
                {
                    case '}':
                        {
                            if (_position < _length && Peek() == '}')
                            {
                                //this is an escaped } and need to be added to the format.
                                Skip('}');
                                format += "}";
                            }
                            else
                            {
                                //done. unread the '}' .
                                _position--;
                                //done
                                return format;
                            }
                            break;
                        }
                    case '{':
                        {
                            //we need a second {, otherwise this format is wrong.
                            var next = Peek();
                            if (next == '{')
                            {
                                //this is an escaped } and need to be added to the format.
                                Skip('{');
                                format += "{";
                            }
                            else
                            {
                                throw new TemplateParserException($"Expected '{{' but found '{next}' instead in format.",
                                    _position, _template);
                            }

                            break;
                        }
                }

                format += ReadUntil(TextDelimiters);
            }
        }

        private int ParseAlignment()
        {
            Skip(',');
            SkipSpaces();
            int i = ReadInt();
            SkipSpaces();
            char next = Peek();
            if (next != ':' && next != '}')
                throw new TemplateParserException($"Expected ':' or '}}' but found '{next}' instead.", _position, _template);
            return i;
        }

        private char Peek() => _template[_position];

        private char Read() => _template[_position++];

        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private void Skip(char c)
        {
            // Can be out of bounds, but never in correct use (expects a required char).
            Debug.Assert(_template[_position] == c);
            _position++;
        }

        private void SkipSpaces()
        {
            // Can be out of bounds, but never in correct use (inside a hole).
            while (_template[_position] == ' ') _position++;
        }

        private int SkipUntil(char[] search, bool required = true)
        {
            int start = _position;
            int i = _template.IndexOfAny(search, _position);
            if (i == -1 && required)
            {
                var formattedChars = string.Join(", ", search.Select(c => string.Concat("'", c.ToString(), "'")).ToArray());
                throw new TemplateParserException($"Reached end of template while expecting one of {formattedChars}.", _position, _template);
            }
            _position = i == -1 ? _length : i;
            return _position - start;
        }

        private int ReadInt()
        {
            bool negative = false;
            int i = 0;
            for (int x = 0; x < 12; ++x)
            {
                char c = Peek();

                int digit = c - '0';
                if (digit < 0 || digit > 9)
                {
                    if (x > 0 && !negative || x > 1)
                        return negative ? -i : i;  // Found one or more digits

                    if (x == 0 && c == '-')
                    {
                        negative = true;
                        _position++;
                        continue;
                    }
                    break;
                }

                _position++;
                i = i * 10 + digit;
            }

            throw new TemplateParserException("An integer is expected", _position, _template);
        }

        private string ReadUntil(char[] search, bool required = true)
        {
            int start = _position;
            return _template.Substring(start, SkipUntil(search, required));
        }
    }
}