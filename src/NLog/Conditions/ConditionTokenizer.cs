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

namespace NLog.Conditions
{
    using System;
    using System.Text;
    using NLog.Internal;

    /// <summary>
    /// Hand-written tokenizer for conditions.
    /// </summary>
    internal sealed class ConditionTokenizer
    {
        private static readonly ConditionTokenType[] charIndexToTokenType = BuildCharIndexToTokenType();
        private readonly SimpleStringReader stringReader;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionTokenizer"/> class.
        /// </summary>
        /// <param name="stringReader">The string reader.</param>
        public ConditionTokenizer(SimpleStringReader stringReader)
        {
            this.stringReader = stringReader;
            this.TokenType = ConditionTokenType.BeginningOfInput;
            this.GetNextToken();
        }

        /// <summary>
        /// Gets the token position.
        /// </summary>
        /// <value>The token position.</value>
        public int TokenPosition { get; private set; }

        /// <summary>
        /// Gets the type of the token.
        /// </summary>
        /// <value>The type of the token.</value>
        public ConditionTokenType TokenType { get; private set; }

        /// <summary>
        /// Gets the token value.
        /// </summary>
        /// <value>The token value.</value>
        public string TokenValue { get; private set; }

        /// <summary>
        /// Gets the value of a string token.
        /// </summary>
        /// <value>The string token value.</value>
        public string StringTokenValue
        {
            get
            {
                string s = this.TokenValue;

                return s.Substring(1, s.Length - 2).Replace("''", "'");
            }
        }

        /// <summary>
        /// Asserts current token type and advances to the next token.
        /// </summary>
        /// <param name="tokenType">Expected token type.</param>
        /// <remarks>If token type doesn't match, an exception is thrown.</remarks>
        public void Expect(ConditionTokenType tokenType)
        {
            if (this.TokenType != tokenType)
            {
                throw new ConditionParseException("Expected token of type: " + tokenType + ", got " + this.TokenType + " (" + this.TokenValue + ").");
            }

            this.GetNextToken();
        }

        /// <summary>
        /// Asserts that current token is a keyword and returns its value and advances to the next token.
        /// </summary>
        /// <returns>Keyword value.</returns>
        public string EatKeyword()
        {
            if (this.TokenType != ConditionTokenType.Keyword)
            {
                throw new ConditionParseException("Identifier expected");
            }

            string s = (string)this.TokenValue;
            this.GetNextToken();
            return s;
        }

        /// <summary>
        /// Gets or sets a value indicating whether current keyword is equal to the specified value.
        /// </summary>
        /// <param name="keyword">The keyword.</param>
        /// <returns>
        /// A value of <c>true</c> if current keyword is equal to the specified value; otherwise, <c>false</c>.
        /// </returns>
        public bool IsKeyword(string keyword)
        {
            if (this.TokenType != ConditionTokenType.Keyword)
            {
                return false;
            }

            if (!this.TokenValue.Equals(keyword, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the tokenizer has reached the end of the token stream.
        /// </summary>
        /// <returns>
        /// A value of <c>true</c> if the tokenizer has reached the end of the token stream; otherwise, <c>false</c>.
        /// </returns>
        public bool IsEOF()
        {
            if (this.TokenType != ConditionTokenType.EndOfInput)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether current token is a number.
        /// </summary>
        /// <returns>
        /// A value of <c>true</c> if current token is a number; otherwise, <c>false</c>.
        /// </returns>
        public bool IsNumber()
        {
            return this.TokenType == ConditionTokenType.Number;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the specified token is of specified type.
        /// </summary>
        /// <param name="tokenType">The token type.</param>
        /// <returns>
        /// A value of <c>true</c> if current token is of specified type; otherwise, <c>false</c>.
        /// </returns>
        public bool IsToken(ConditionTokenType tokenType)
        {
            return this.TokenType == tokenType;
        }

        /// <summary>
        /// Gets the next token and sets <see cref="TokenType"/> and <see cref="TokenValue"/> properties.
        /// </summary>
        public void GetNextToken()
        {
            if (this.TokenType == ConditionTokenType.EndOfInput)
            {
                throw new ConditionParseException("Cannot read past end of stream.");
            }

            this.SkipWhitespace();

            this.TokenPosition = this.TokenPosition;

            int i = this.PeekChar();
            if (i == -1)
            {
                this.TokenType = ConditionTokenType.EndOfInput;
                return;
            }

            char ch = (char)i;

            if (Char.IsDigit(ch))
            {
                this.ParseNumber(ch);
                return;
            }

            if (ch == '\'')
            {
                this.ParseSingleQuotedString(ch);
                return;
            }

            if (ch == '_' || Char.IsLetter(ch))
            {
                this.ParseKeyword(ch);
                return;
            }

            if (ch == '}' || ch == ':')
            {
                // when condition is embedded
                this.TokenType = ConditionTokenType.EndOfInput;
                return;
            }

            this.TokenValue = ch.ToString();

            if (ch == '<')
            {
                this.ReadChar();
                int nextChar = this.PeekChar();

                if (nextChar == '>')
                {
                    this.TokenType = ConditionTokenType.NotEqual;
                    this.TokenValue = "<>";
                    this.ReadChar();
                    return;
                }

                if (nextChar == '=')
                {
                    this.TokenType = ConditionTokenType.LessThanOrEqualTo;
                    this.TokenValue = "<=";
                    this.ReadChar();
                    return;
                }

                this.TokenType = ConditionTokenType.LessThan;
                this.TokenValue = "<";
                return;
            }

            if (ch == '>')
            {
                this.ReadChar();
                int nextChar = this.PeekChar();

                if (nextChar == '=')
                {
                    this.TokenType = ConditionTokenType.GreaterThanOrEqualTo;
                    this.TokenValue = ">=";
                    this.ReadChar();
                    return;
                }

                this.TokenType = ConditionTokenType.GreaterThan;
                this.TokenValue = ">";
                return;
            }

            if (ch == '!')
            {
                this.ReadChar();
                int nextChar = this.PeekChar();

                if (nextChar == '=')
                {
                    this.TokenType = ConditionTokenType.NotEqual;
                    this.TokenValue = "!=";
                    this.ReadChar();
                    return;
                }

                this.TokenType = ConditionTokenType.Not;
                this.TokenValue = "!";
                return;
            }

            if (ch == '&')
            {
                this.ReadChar();
                int nextChar = this.PeekChar();
                if (nextChar == '&')
                {
                    this.TokenType = ConditionTokenType.And;
                    this.TokenValue = "&&";
                    this.ReadChar();
                    return;
                }

                throw new ConditionParseException("Expected '&&' but got '&'");
            }

            if (ch == '|')
            {
                this.ReadChar();
                int nextChar = this.PeekChar();
                if (nextChar == '|')
                {
                    this.TokenType = ConditionTokenType.Or;
                    this.TokenValue = "||";
                    this.ReadChar();
                    return;
                }

                throw new ConditionParseException("Expected '||' but got '|'");
            }

            if (ch == '=')
            {
                this.ReadChar();
                int nextChar = this.PeekChar();

                if (nextChar == '=')
                {
                    this.TokenType = ConditionTokenType.EqualTo;
                    this.TokenValue = "==";
                    this.ReadChar();
                    return;
                }

                this.TokenType = ConditionTokenType.EqualTo;
                this.TokenValue = "=";
                return;
            }

            if (ch >= 32 && ch < 128)
            {
                ConditionTokenType tt = charIndexToTokenType[ch];

                if (tt != ConditionTokenType.Invalid)
                {
                    this.TokenType = tt;
                    this.TokenValue = new string(ch, 1);
                    this.ReadChar();
                    return;
                }

                throw new ConditionParseException("Invalid punctuation: " + ch);
            }

            throw new ConditionParseException("Invalid token: " + ch);
        }

        private static ConditionTokenType[] BuildCharIndexToTokenType()
        {
            CharToTokenType[] charToTokenType =
            {
                new CharToTokenType('(', ConditionTokenType.LeftParen),
                new CharToTokenType(')', ConditionTokenType.RightParen),
                new CharToTokenType('.', ConditionTokenType.Dot),
                new CharToTokenType(',', ConditionTokenType.Comma),
                new CharToTokenType('!', ConditionTokenType.Not),
                new CharToTokenType('-', ConditionTokenType.Minus),
            };

            var result = new ConditionTokenType[128];

            for (int i = 0; i < 128; ++i)
            {
                result[i] = ConditionTokenType.Invalid;
            }

            foreach (CharToTokenType cht in charToTokenType)
            {
                // Console.WriteLine("Setting up {0} to {1}", cht.ch, cht.tokenType);
                result[(int)cht.Character] = cht.TokenType;
            }

            return result;
        }

        private void ParseSingleQuotedString(char ch)
        {
            int i;
            this.TokenType = ConditionTokenType.String;

            StringBuilder sb = new StringBuilder();

            sb.Append(ch);
            this.ReadChar();

            while ((i = this.PeekChar()) != -1)
            {
                ch = (char)i;

                sb.Append((char)this.ReadChar());

                if (ch == '\'')
                {
                    if (this.PeekChar() == (int)'\'')
                    {
                        sb.Append('\'');
                        this.ReadChar();
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (i == -1)
            {
                throw new ConditionParseException("String literal is missing a closing quote character.");
            }

            this.TokenValue = sb.ToString();
        }

        private void ParseKeyword(char ch)
        {
            int i;
            this.TokenType = ConditionTokenType.Keyword;

            StringBuilder sb = new StringBuilder();

            sb.Append((char)ch);

            this.ReadChar();

            while ((i = this.PeekChar()) != -1)
            {
                if ((char)i == '_' || (char)i == '-' || Char.IsLetterOrDigit((char)i))
                {
                    sb.Append((char)this.ReadChar());
                }
                else
                {
                    break;
                }
            }

            this.TokenValue = sb.ToString();
        }

        private void ParseNumber(char ch)
        {
            int i;
            this.TokenType = ConditionTokenType.Number;
            StringBuilder sb = new StringBuilder();

            sb.Append(ch);
            this.ReadChar();

            while ((i = this.PeekChar()) != -1)
            {
                ch = (char)i;

                if (Char.IsDigit(ch) || (ch == '.'))
                {
                    sb.Append((char)this.ReadChar());
                }
                else
                {
                    break;
                }
            }

            this.TokenValue = sb.ToString();
        }

        private void SkipWhitespace()
        {
            int ch;

            while ((ch = this.PeekChar()) != -1)
            {
                if (!Char.IsWhiteSpace((char)ch))
                {
                    break;
                }

                this.ReadChar();
            }
        }

        private int PeekChar()
        {
            return this.stringReader.Peek();
        }

        private int ReadChar()
        {
            return this.stringReader.Read();
        }

        /// <summary>
        /// Mapping between characters and token types for punctuations.
        /// </summary>
        private struct CharToTokenType
        {
            public readonly char Character;
            public readonly ConditionTokenType TokenType;

            /// <summary>
            /// Initializes a new instance of the CharToTokenType struct.
            /// </summary>
            /// <param name="character">The character.</param>
            /// <param name="tokenType">Type of the token.</param>
            public CharToTokenType(char character, ConditionTokenType tokenType)
            {
                this.Character = character;
                this.TokenType = tokenType;
            }
        }
    }
}
