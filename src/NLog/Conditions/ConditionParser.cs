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
using System.Text;
using System.Collections;
using System.Globalization;

using NLog.Conditions;

namespace NLog.Conditions 
{
    /// <summary>
    /// Condition parser. Turns a string representation of condition expression
    /// into an expression tree.
    /// </summary>
    public class ConditionParser 
    {
        private ConditionTokenizer tokenizer = new ConditionTokenizer();

        private ConditionParser(string query) 
        {
            tokenizer.InitTokenizer(query);
        }

        private ConditionMethodExpression ParsePredicate(string functionName) 
        {
            ConditionExpressionCollection par = new ConditionExpressionCollection();

            while (!tokenizer.IsEOF() && tokenizer.TokenType != ConditionTokenType.RightParen) 
            {
                par.Add(ParseExpression());
                if (tokenizer.TokenType != ConditionTokenType.Comma)
                    break;
                tokenizer.GetNextToken();
            }
            tokenizer.Expect(ConditionTokenType.RightParen);

            return new ConditionMethodExpression(functionName, par);
        }

        private ConditionExpression ParseLiteralExpression() 
        {
            if (tokenizer.IsToken(ConditionTokenType.LeftParen)) 
            {
                tokenizer.GetNextToken();
                ConditionExpression e = ParseExpression();
                tokenizer.Expect(ConditionTokenType.RightParen);
                return e;
            };

            if (tokenizer.IsNumber()) 
            {
                string numberString = (string)tokenizer.TokenValue;
                tokenizer.GetNextToken();
                if (numberString.IndexOf('.') >= 0)
                {
                    return new ConditionLiteralExpression(Double.Parse(numberString, CultureInfo.InvariantCulture));
                }
                else
                {
                    return new ConditionLiteralExpression(Int32.Parse(numberString, CultureInfo.InvariantCulture));
                }
            }

            if (tokenizer.TokenType == ConditionTokenType.String) 
            {
                ConditionExpression e = new ConditionLayoutExpression(tokenizer.StringTokenValue);
                tokenizer.GetNextToken();
                return e;
            }

            if (tokenizer.TokenType == ConditionTokenType.Keyword)
            {
                string keyword = tokenizer.EatKeyword();

                if (0 == String.Compare(keyword, "level", true))
                    return new ConditionLevelExpression();

                if (0 == String.Compare(keyword, "logger", true))
                    return new ConditionLoggerNameExpression();

                if (0 == String.Compare(keyword, "message", true))
                    return new ConditionMessageExpression();

                if (0 == String.Compare(keyword, "loglevel", true))
                {
                    tokenizer.Expect(ConditionTokenType.Dot);
                    return new ConditionLiteralExpression(LogLevel.FromString(tokenizer.EatKeyword()));
                }

                if (0 == String.Compare(keyword, "true", true))
                    return new ConditionLiteralExpression(true);

                if (0 == String.Compare(keyword, "false", true))
                    return new ConditionLiteralExpression(false);

                if (tokenizer.TokenType == ConditionTokenType.LeftParen)
                {
                    tokenizer.GetNextToken();

                    ConditionMethodExpression predicateExpression = ParsePredicate(keyword);
                    return predicateExpression;
                }
            }

            throw new ConditionParseException("Unexpected token: " + tokenizer.TokenValue);
        }

        private ConditionExpression ParseBooleanRelation() 
        {
            ConditionExpression e = ParseLiteralExpression();

            if (tokenizer.IsToken(ConditionTokenType.EQ)) 
            {
                tokenizer.GetNextToken();
                return new ConditionRelationalExpression(e, ParseLiteralExpression(), ConditionRelationalOperator.Equal);
            };

            if (tokenizer.IsToken(ConditionTokenType.NE)) 
            {
                tokenizer.GetNextToken();
                return new ConditionRelationalExpression(e, ParseLiteralExpression(), ConditionRelationalOperator.NotEqual);
            };

            if (tokenizer.IsToken(ConditionTokenType.LT)) 
            {
                tokenizer.GetNextToken();
                return new ConditionRelationalExpression(e, ParseLiteralExpression(), ConditionRelationalOperator.Less);
            };

            if (tokenizer.IsToken(ConditionTokenType.GT)) 
            {
                tokenizer.GetNextToken();
                return new ConditionRelationalExpression(e, ParseLiteralExpression(), ConditionRelationalOperator.Greater);
            };

            if (tokenizer.IsToken(ConditionTokenType.LE)) 
            {
                tokenizer.GetNextToken();
                return new ConditionRelationalExpression(e, ParseLiteralExpression(), ConditionRelationalOperator.LessOrEqual);
            };

            if (tokenizer.IsToken(ConditionTokenType.GE)) 
            {
                tokenizer.GetNextToken();
                return new ConditionRelationalExpression(e, ParseLiteralExpression(), ConditionRelationalOperator.GreaterOrEqual);
            };

            return e;
        }

        private ConditionExpression ParseBooleanPredicate() 
        {
            if (tokenizer.IsKeyword("not") || tokenizer.IsToken(ConditionTokenType.Not)) 
            {
                tokenizer.GetNextToken();
                return new ConditionNotExpression((ConditionExpression)ParseBooleanPredicate());
            }

            return ParseBooleanRelation();
        }

        private ConditionExpression ParseBooleanAnd() 
        {
            ConditionExpression e = ParseBooleanPredicate();

            while (tokenizer.IsKeyword("and") || tokenizer.IsToken(ConditionTokenType.And)) 
            {
                tokenizer.GetNextToken();
                e = new ConditionAndExpression((ConditionExpression)e, (ConditionExpression)ParseBooleanPredicate());
            }
            return e;
        }

        private ConditionExpression ParseBooleanOr() 
        {
            ConditionExpression e = ParseBooleanAnd();

            while (tokenizer.IsKeyword("or") || tokenizer.IsToken(ConditionTokenType.Or)) 
            {
                tokenizer.GetNextToken();
                e = new ConditionOrExpression((ConditionExpression)e, (ConditionExpression)ParseBooleanAnd());
            }
            return e;
        }

        private ConditionExpression ParseBooleanExpression() 
        {
            return ParseBooleanOr();
        }

        private ConditionExpression ParseExpression() 
        {
            return ParseBooleanExpression();
        }

        /// <summary>
        /// Parses the specified condition string and turns it into
        /// <see cref="ConditionExpression"/> tree.
        /// </summary>
        /// <param name="expr">The expression to be parsed.</param>
        /// <returns>The root of the expression syntax tree which can be used to get the value of the condition in a specified context</returns>
        public static ConditionExpression ParseExpression(string expr) 
        {
            ConditionParser parser = new ConditionParser(expr);
            ConditionExpression e = parser.ParseExpression();
            if (!parser.tokenizer.IsEOF())
                throw new ConditionParseException("Unexpected token: " + parser.tokenizer.TokenValue);
            return e;
        }
    }
}
