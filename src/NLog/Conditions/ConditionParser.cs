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
using System.Collections.Generic;
using System.Globalization;
using NLog.Config;
using NLog.Layouts;

namespace NLog.Conditions
{
    /// <summary>
    /// Condition parser. Turns a string representation of condition expression
    /// into an expression tree.
    /// </summary>
    public class ConditionParser 
    {
        private readonly ConditionTokenizer tokenizer = new ConditionTokenizer();

        /// <summary>
        /// Initializes a new instance of the ConditionParser class.
        /// </summary>
        /// <param name="expressionText">The expression text.</param>
        private ConditionParser(string expressionText)
        {
            this.tokenizer.InitTokenizer(expressionText);
        }

        /// <summary>
        /// Parses the specified condition string and turns it into
        /// <see cref="ConditionExpression"/> tree.
        /// </summary>
        /// <param name="expressionText">The expression to be parsed.</param>
        /// <returns>The root of the expression syntax tree which can be used to get the value of the condition in a specified context.</returns>
        public static ConditionExpression ParseExpression(string expressionText)
        {
            ConditionParser parser = new ConditionParser(expressionText);
            ConditionExpression expression = parser.ParseExpression();
            if (!parser.tokenizer.IsEOF())
            {
                throw new ConditionParseException("Unexpected token: " + parser.tokenizer.TokenValue);
            }

            return expression;
        }

        private ConditionMethodExpression ParsePredicate(string functionName) 
        {
            ICollection<ConditionExpression> par = new List<ConditionExpression>();

            while (!this.tokenizer.IsEOF() && this.tokenizer.TokenType != ConditionTokenType.RightParen) 
            {
                par.Add(ParseExpression());
                if (this.tokenizer.TokenType != ConditionTokenType.Comma)
                {
                    break;
                }

                this.tokenizer.GetNextToken();
            }

            this.tokenizer.Expect(ConditionTokenType.RightParen);

            var methodInfo = NLogFactories.Default.ConditionMethodFactory.CreateInstance(functionName);
            return new ConditionMethodExpression(functionName, methodInfo, par);
        }

        private ConditionExpression ParseLiteralExpression() 
        {
            if (this.tokenizer.IsToken(ConditionTokenType.LeftParen)) 
            {
                this.tokenizer.GetNextToken();
                ConditionExpression e = this.ParseExpression();
                this.tokenizer.Expect(ConditionTokenType.RightParen);
                return e;
            }

            if (this.tokenizer.IsNumber()) 
            {
                string numberString = this.tokenizer.TokenValue;
                this.tokenizer.GetNextToken();
                if (numberString.IndexOf('.') >= 0)
                {
                    return new ConditionLiteralExpression(Double.Parse(numberString, CultureInfo.InvariantCulture));
                }

                return new ConditionLiteralExpression(Int32.Parse(numberString, CultureInfo.InvariantCulture));
            }

            if (this.tokenizer.TokenType == ConditionTokenType.String) 
            {
                ConditionExpression e = new ConditionLayoutExpression(new SimpleLayout(this.tokenizer.StringTokenValue));
                this.tokenizer.GetNextToken();
                return e;
            }

            if (this.tokenizer.TokenType == ConditionTokenType.Keyword)
            {
                string keyword = this.tokenizer.EatKeyword();

                if (0 == String.Compare(keyword, "level", StringComparison.OrdinalIgnoreCase))
                {
                    return new ConditionLevelExpression();
                }

                if (0 == String.Compare(keyword, "logger", StringComparison.OrdinalIgnoreCase))
                {
                    return new ConditionLoggerNameExpression();
                }

                if (0 == String.Compare(keyword, "message", StringComparison.OrdinalIgnoreCase))
                {
                    return new ConditionMessageExpression();
                }

                if (0 == String.Compare(keyword, "loglevel", StringComparison.OrdinalIgnoreCase))
                {
                    this.tokenizer.Expect(ConditionTokenType.Dot);
                    return new ConditionLiteralExpression(LogLevel.FromString(this.tokenizer.EatKeyword()));
                }

                if (0 == String.Compare(keyword, "true", StringComparison.OrdinalIgnoreCase))
                {
                    return new ConditionLiteralExpression(true);
                }

                if (0 == String.Compare(keyword, "false", StringComparison.OrdinalIgnoreCase))
                {
                    return new ConditionLiteralExpression(false);
                }

                if (this.tokenizer.TokenType == ConditionTokenType.LeftParen)
                {
                    this.tokenizer.GetNextToken();

                    ConditionMethodExpression predicateExpression = this.ParsePredicate(keyword);
                    return predicateExpression;
                }
            }

            throw new ConditionParseException("Unexpected token: " + this.tokenizer.TokenValue);
        }

        private ConditionExpression ParseBooleanRelation() 
        {
            ConditionExpression e = this.ParseLiteralExpression();

            if (this.tokenizer.IsToken(ConditionTokenType.EqualTo)) 
            {
                this.tokenizer.GetNextToken();
                return new ConditionRelationalExpression(e, this.ParseLiteralExpression(), ConditionRelationalOperator.Equal);
            }

            if (this.tokenizer.IsToken(ConditionTokenType.NotEqual)) 
            {
                this.tokenizer.GetNextToken();
                return new ConditionRelationalExpression(e, this.ParseLiteralExpression(), ConditionRelationalOperator.NotEqual);
            }

            if (this.tokenizer.IsToken(ConditionTokenType.LT)) 
            {
                this.tokenizer.GetNextToken();
                return new ConditionRelationalExpression(e, this.ParseLiteralExpression(), ConditionRelationalOperator.Less);
            }

            if (this.tokenizer.IsToken(ConditionTokenType.GT)) 
            {
                this.tokenizer.GetNextToken();
                return new ConditionRelationalExpression(e, this.ParseLiteralExpression(), ConditionRelationalOperator.Greater);
            }

            if (this.tokenizer.IsToken(ConditionTokenType.LessThanOrEqualTo)) 
            {
                this.tokenizer.GetNextToken();
                return new ConditionRelationalExpression(e, this.ParseLiteralExpression(), ConditionRelationalOperator.LessOrEqual);
            }

            if (this.tokenizer.IsToken(ConditionTokenType.GreaterThanOrEqualTo)) 
            {
                this.tokenizer.GetNextToken();
                return new ConditionRelationalExpression(e, this.ParseLiteralExpression(), ConditionRelationalOperator.GreaterOrEqual);
            }

            return e;
        }

        private ConditionExpression ParseBooleanPredicate() 
        {
            if (this.tokenizer.IsKeyword("not") || this.tokenizer.IsToken(ConditionTokenType.Not)) 
            {
                this.tokenizer.GetNextToken();
                return new ConditionNotExpression(this.ParseBooleanPredicate());
            }

            return this.ParseBooleanRelation();
        }

        private ConditionExpression ParseBooleanAnd() 
        {
            ConditionExpression expression = this.ParseBooleanPredicate();

            while (this.tokenizer.IsKeyword("and") || this.tokenizer.IsToken(ConditionTokenType.And)) 
            {
                this.tokenizer.GetNextToken();
                expression = new ConditionAndExpression(expression, this.ParseBooleanPredicate());
            }

            return expression;
        }

        private ConditionExpression ParseBooleanOr() 
        {
            ConditionExpression expression = this.ParseBooleanAnd();

            while (this.tokenizer.IsKeyword("or") || this.tokenizer.IsToken(ConditionTokenType.Or)) 
            {
                this.tokenizer.GetNextToken();
                expression = new ConditionOrExpression(expression, this.ParseBooleanAnd());
            }

            return expression;
        }

        private ConditionExpression ParseBooleanExpression() 
        {
            return this.ParseBooleanOr();
        }

        private ConditionExpression ParseExpression() 
        {
            return this.ParseBooleanExpression();
        }
    }
}
