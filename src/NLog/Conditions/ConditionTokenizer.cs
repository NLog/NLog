// 
// Copyright (c) 2004-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
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
using System.IO;
using System.Text;
using System.Collections;

namespace NLog.Conditions 
{
    /// <summary>
    /// Hand-written tokenizer for conditions.
    /// </summary>
    internal sealed class ConditionTokenizer 
    {
        private string _inputString = null;
        private int _position = 0;
        private int _tokenPosition = 0;

        private ConditionTokenType _tokenType;
        private string _tokenValue;
        private string _tokenValueLowercase;

        public bool IgnoreWhiteSpace = true;

        public int TokenPosition
        {
            get { return _tokenPosition; }
        }

        public ConditionTokenType TokenType
        {
            get { return _tokenType; }
            set { _tokenType = value; }
        }

        public string TokenValue
        {
            get { return _tokenValue; }
        }

        public string StringTokenValue
        {
            get 
            {
                string s = _tokenValue;

                return s.Substring(1, s.Length - 2).Replace("''", "'");
            }
        }

        public ConditionTokenizer() {}

        void SkipWhitespace() 
        {
            int ch;

            while ((ch = PeekChar()) != -1) 
            {
                if (!Char.IsWhiteSpace((char)ch))
                    break;
                ReadChar();
            };
        }

        public void InitTokenizer(string s) 
        {
            _inputString = s;
            _position = 0;
            _tokenType = ConditionTokenType.BOF;

            GetNextToken();
        }

        int PeekChar() 
        {
            if (_position < _inputString.Length) 
            {
                return (int)_inputString[_position];
            } 
            else 
            {
                return -1;
            }
        }

        int ReadChar() 
        {
            if (_position < _inputString.Length) 
            {
                return (int)_inputString[_position++];
            } 
            else 
            {
                return -1;
            }
        }

        public void Expect(ConditionTokenType type) 
        {
            if (_tokenType != type)
                throw new ConditionParseException("Expected token of type: " + type + ", got " + _tokenType + " (" + _tokenValue + ").");

            GetNextToken();
        }


        public void ExpectKeyword(string s) 
        {
            if (_tokenType != ConditionTokenType.Keyword)
                throw new ConditionParseException("Expected keyword: " + s + ", got " + _tokenType + ".");

            if (_tokenValueLowercase != s)
                throw new ConditionParseException("Expected keyword: " + s + ", got " + _tokenValueLowercase + ".");

            GetNextToken();
        }

        public string EatKeyword() 
        {
            if (_tokenType != ConditionTokenType.Keyword)
                throw new ConditionParseException("Identifier expected");

            string s = (string)_tokenValue;
            GetNextToken();
            return s;
        }

        public bool IsKeyword(string s) 
        {
            if (_tokenType != ConditionTokenType.Keyword)
                return false;

            if (_tokenValueLowercase != s)
                return false;

            return true;
        }

        public bool IsKeyword() 
        {
            if (_tokenType != ConditionTokenType.Keyword)
                return false;

            return true;
        }

        public bool IsEOF() 
        {
            if (_tokenType != ConditionTokenType.EOF)
                return false;
            return true;
        }

        public bool IsNumber() 
        {
            return _tokenType == ConditionTokenType.Number;
        }

        public bool IsToken(ConditionTokenType token) 
        {
            return _tokenType == token;
        }

        public bool IsToken(object[] tokens) 
        {
            for (int i = 0; i < tokens.Length; ++i) 
            {
                if (tokens[i] is string) 
                {
                    if (IsKeyword((string)tokens[i]))
                        return true;
                } 
                else 
                {
                    if (_tokenType == (ConditionTokenType)tokens[i])
                        return true;
                }
            }
            return false;
        }

        public bool IsPunctuation() 
        {
            return (_tokenType >= ConditionTokenType.FirstPunct && _tokenType < ConditionTokenType.LastPunct);
        }

        struct CharToTokenType 
        {
            public char ch;
            public ConditionTokenType tokenType;

            public CharToTokenType(char ch, ConditionTokenType tokenType) 
            {
                this.ch = ch;
                this.tokenType = tokenType;
            }
        }

        static CharToTokenType[] charToTokenType =
            {
                new CharToTokenType('<', ConditionTokenType.LT),
                new CharToTokenType('>', ConditionTokenType.GT),
                new CharToTokenType('=', ConditionTokenType.EQ),
                new CharToTokenType('(', ConditionTokenType.LeftParen),
                new CharToTokenType(')', ConditionTokenType.RightParen),
                new CharToTokenType('.', ConditionTokenType.Dot),
                new CharToTokenType(',', ConditionTokenType.Comma),
                new CharToTokenType('!', ConditionTokenType.Not),
        };

        static ConditionTokenType[] charIndexToTokenType = new ConditionTokenType[128];
        
        static ConditionTokenizer() 
        {
            for (int i = 0; i < 128; ++i) 
            {
                charIndexToTokenType[i] = ConditionTokenType.Invalid;
            };

            foreach (CharToTokenType cht in charToTokenType) 
            {
                // Console.WriteLine("Setting up {0} to {1}", cht.ch, cht.tokenType);
                charIndexToTokenType[(int)cht.ch] = cht.tokenType;
            }
        }

        public void GetNextToken() 
        {
            if (_tokenType == ConditionTokenType.EOF)
                throw new Exception("Cannot read past end of stream.");

            if (IgnoreWhiteSpace) 
            {
                SkipWhitespace();
            };

            _tokenPosition = _position;

            int i = PeekChar();
            if (i == -1) 
            {
                TokenType = ConditionTokenType.EOF;
                return ;
            }

            char ch = (char)i;

            if (!IgnoreWhiteSpace && Char.IsWhiteSpace(ch)) 
            {
                StringBuilder sb = new StringBuilder();
                int ch2;

                while ((ch2 = PeekChar()) != -1) 
                {
                    if (!Char.IsWhiteSpace((char)ch2))
                        break;

                    sb.Append((char)ch2);
                    ReadChar();
                };

                TokenType = ConditionTokenType.Whitespace;
                _tokenValue = sb.ToString();
                return ;
            }

            if (Char.IsDigit(ch)) 
            {
                TokenType = ConditionTokenType.Number;
                string s = "";

                s += ch;
                ReadChar();

                while ((i = PeekChar()) != -1) 
                {
                    ch = (char)i;

                    if (Char.IsDigit(ch) || (ch == '.')) 
                    {
                        s += (char)ReadChar();
                    } 
                    else 
                    {
                        break;
                    };
                };

                _tokenValue = s;
                return ;
            }

            if (ch == '\'') 
            {
                TokenType = ConditionTokenType.String;

                StringBuilder sb = new StringBuilder();

                sb.Append(ch);
                ReadChar();

                while ((i = PeekChar()) != -1) 
                {
                    ch = (char)i;

                    sb.Append((char)ReadChar());

                    if (ch == '\'') 
                    {
                        if (PeekChar() == (int)'\'') 
                        {
                            sb.Append('\'');
                            ReadChar();
                        } 
                        else
                            break;
                    }
                };

                _tokenValue = sb.ToString();
                return ;
            }

            if (ch == '_' || Char.IsLetter(ch)) 
            {
                TokenType = ConditionTokenType.Keyword;

                StringBuilder sb = new StringBuilder();

                sb.Append((char)ch);

                ReadChar();

                while ((i = PeekChar()) != -1) 
                {
                    if ((char)i == '_' || (char)i == '-' || Char.IsLetterOrDigit((char)i)) 
                    {
                        sb.Append((char)ReadChar());
                    } 
                    else 
                    {
                        break;
                    };
                };

                _tokenValue = sb.ToString();
                _tokenValueLowercase = _tokenValue.ToLower();
                return ;
            }

            ReadChar();
            _tokenValue = ch.ToString();

            if (ch == '<' && PeekChar() == (int)'>') 
            {
                TokenType = ConditionTokenType.NE;
                _tokenValue = "<>";
                ReadChar();
                return ;
            }

            if (ch == '!' && PeekChar() == (int)'=') 
            {
                TokenType = ConditionTokenType.NE;
                _tokenValue = "!=";
                ReadChar();
                return ;
            }

            if (ch == '&' && PeekChar() == (int)'&') 
            {
                TokenType = ConditionTokenType.And;
                _tokenValue = "&&";
                ReadChar();
                return ;
            }

            if (ch == '|' && PeekChar() == (int)'|') 
            {
                TokenType = ConditionTokenType.Or;
                _tokenValue = "||";
                ReadChar();
                return ;
            }

            if (ch == '<' && PeekChar() == (int)'=') 
            {
                TokenType = ConditionTokenType.LE;
                _tokenValue = "<=";
                ReadChar();
                return ;
            }

            if (ch == '>' && PeekChar() == (int)'=') 
            {
                TokenType = ConditionTokenType.GE;
                _tokenValue = ">=";
                ReadChar();
                return ;
            }

            if (ch == '=' && PeekChar() == (int)'=') 
            {
                TokenType = ConditionTokenType.EQ;
                _tokenValue = "==";
                ReadChar();
                return ;
            }

            if (ch >= 32 && ch < 128) 
            {
                ConditionTokenType tt = charIndexToTokenType[ch];

                if (tt != ConditionTokenType.Invalid) 
                {
                    TokenType = tt;
                    _tokenValue = new String(ch, 1);
                    return ;
                } 
                else 
                {
                    throw new Exception("Invalid punctuation: " + ch);
                }
            }
            throw new Exception("Invalid token: " + ch);
        }
    }
}
