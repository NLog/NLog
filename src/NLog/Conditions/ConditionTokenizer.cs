// 
// Copyright (c) 2004-2009 Jaroslaw Kowalski <jaak@jkowalski.net>
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
using System.Text;

namespace NLog.Conditions
{
    /// <summary>
    /// Hand-written tokenizer for conditions.
    /// </summary>
    internal sealed class ConditionTokenizer
    {
        private static readonly ConditionTokenType[] charIndexToTokenType = new ConditionTokenType[128];
        private static readonly CharToTokenType[] charToTokenType =
        {
            new CharToTokenType('<', ConditionTokenType.LessThan),
            new CharToTokenType('>', ConditionTokenType.GreaterTo),
            new CharToTokenType('=', ConditionTokenType.EqualTo),
            new CharToTokenType('(', ConditionTokenType.LeftParen),
            new CharToTokenType(')', ConditionTokenType.RightParen),
            new CharToTokenType('.', ConditionTokenType.Dot),
            new CharToTokenType(',', ConditionTokenType.Comma),
            new CharToTokenType('!', ConditionTokenType.Not),
        };

        private string inputString;
        private int position;

        /// <summary>
        /// Initializes static members of the ConditionTokenizer class.
        /// </summary>
        static ConditionTokenizer()
        {
            for (int i = 0; i < 128; ++i)
            {
                charIndexToTokenType[i] = ConditionTokenType.Invalid;
            }

            foreach (CharToTokenType cht in charToTokenType)
            {
                // Console.WriteLine("Setting up {0} to {1}", cht.ch, cht.tokenType);
                charIndexToTokenType[(int)cht.Character] = cht.TokenType;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionTokenizer" /> class.
        /// </summary>
        public ConditionTokenizer()
        {
            this.IgnoreWhiteSpace = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to ignore white space.
        /// </summary>
        /// <value>A value of <c>true</c> if white space should be ignored; otherwise, <c>false</c>.</value>
        public bool IgnoreWhiteSpace { get; set; }

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
        /// Initializes the tokenizer with a given input string.
        /// </summary>
        /// <param name="inputString">The input string.</param>
        public void InitTokenizer(string inputString)
        {
            this.inputString = inputString;
            this.position = 0;
            this.TokenType = ConditionTokenType.BeginningOfInput;

            this.GetNextToken();
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
        /// Asserts that current token is a specific keyword and advances to the next token.
        /// </summary>
        /// <param name="expectedKeyword">The expected keyword.</param>
        /// <remarks>If token is not the expected keyword, an exception is thrown.</remarks>
        public void ExpectKeyword(string expectedKeyword)
        {
            if (this.TokenType != ConditionTokenType.Keyword)
            {
                throw new ConditionParseException("Expected keyword: " + expectedKeyword + ", got " + this.TokenType + ".");
            }

            if (!this.TokenValue.Equals(expectedKeyword, StringComparison.OrdinalIgnoreCase))
            {
                throw new ConditionParseException("Expected keyword: " + expectedKeyword + ", got " + this.TokenValue + ".");
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
        /// Gets or sets a value indicating whether current token is a keyword.
        /// </summary>
        /// <returns>
        /// A value of <c>true</c> if current token is a keyword; otherwise, <c>false</c>.
        /// </returns>
        public bool IsKeyword()
        {
            if (this.TokenType != ConditionTokenType.Keyword)
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
        /// Gets or sets a value indicating whether the current token is equal to one of the specified values (keyword names or token types).
        /// </summary>
        /// <param name="tokens">Possible token values.</param>
        /// <returns>A value of <c>true</c> if the specified token is equal to one of the specified values; otherwise, <c>false</c>.</returns>
        public bool IsToken(object[] tokens)
        {
            for (int i = 0; i < tokens.Length; ++i)
            {
                if (tokens[i] is string)
                {
                    if (this.IsKeyword((string)tokens[i]))
                    {
                        return true;
                    }
                }
                else
                {
                    if (this.TokenType == (ConditionTokenType)tokens[i])
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Gets or sets a value indicating whether current token is a punctuation.
        /// </summary>
        /// <returns>
        /// A value of <c>true</c> if this instance is punctuation; otherwise, <c>false</c>.
        /// </returns>
        public bool IsPunctuation()
        {
            return this.TokenType >= ConditionTokenType.FirstPunct && this.TokenType < ConditionTokenType.LastPunct;
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

            if (this.IgnoreWhiteSpace)
            {
                this.SkipWhitespace();
            }

            this.TokenPosition = this.position;

            int i = this.PeekChar();
            if (i == -1)
            {
                this.TokenType = ConditionTokenType.EndOfInput;
                return;
            }

            char ch = (char)i;

            if (!this.IgnoreWhiteSpace && Char.IsWhiteSpace(ch))
            {
                this.ParseWhitespace();
                return;
            }

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

            this.ReadChar();
            this.TokenValue = ch.ToString();

            int nextChar = this.PeekChar();

            if (ch == '<' && nextChar == '>')
            {
                this.TokenType = ConditionTokenType.NotEqual;
                this.TokenValue = "<>";
                this.ReadChar();
                return;
            }

            if (ch == '!' && nextChar == '=')
            {
                this.TokenType = ConditionTokenType.NotEqual;
                this.TokenValue = "!=";
                this.ReadChar();
                return;
            }

            if (ch == '&' && nextChar == '&')
            {
                this.TokenType = ConditionTokenType.And;
                this.TokenValue = "&&";
                this.ReadChar();
                return;
            }

            if (ch == '|' && nextChar == '|')
            {
                this.TokenType = ConditionTokenType.Or;
                this.TokenValue = "||";
                this.ReadChar();
                return;
            }

            if (ch == '<' && nextChar == '=')
            {
                this.TokenType = ConditionTokenType.LessThanOrEqualTo;
                this.TokenValue = "<=";
                this.ReadChar();
                return;
            }

            if (ch == '>' && nextChar == '=')
            {
                this.TokenType = ConditionTokenType.GreaterThanOrEqualTo;
                this.TokenValue = ">=";
                this.ReadChar();
                return;
            }

            if (ch == '=' && nextChar == '=')
            {
                this.TokenType = ConditionTokenType.EqualTo;
                this.TokenValue = "==";
                this.ReadChar();
                return;
            }

            if (ch >= 32 && ch < 128)
            {
                ConditionTokenType tt = charIndexToTokenType[ch];

                if (tt != ConditionTokenType.Invalid)
                {
                    this.TokenType = tt;
                    this.TokenValue = new string(ch, 1);
                    return;
                }

                throw new ConditionParseException("Invalid punctuation: " + ch);
            }

            throw new ConditionParseException("Invalid token: " + ch);
        }

        private void ParseWhitespace()
        {
            StringBuilder sb = new StringBuilder();
            int ch2;

            while ((ch2 = this.PeekChar()) != -1)
            {
                if (!Char.IsWhiteSpace((char)ch2))
                {
                    break;
                }

                sb.Append((char)ch2);
                this.ReadChar();
            }

            this.TokenType = ConditionTokenType.Whitespace;
            this.TokenValue = sb.ToString();
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
            if (this.position < this.inputString.Length)
            {
                return (int)this.inputString[this.position];
            }
            else
            {
                return -1;
            }
        }

        private int ReadChar()
        {
            if (this.position < this.inputString.Length)
            {
                return (int)this.inputString[this.position++];
            }
            else
            {
                return -1;
            }
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
