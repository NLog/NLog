// 
// Copyright (c) 2004-2017 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

using NLog.Common;

namespace NLog.Conditions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using NLog.Config;
    using NLog.Internal;
    using NLog.Layouts;

    /// <summary>
    /// Condition parser. Turns a string representation of condition expression
    /// into an expression tree.
    /// </summary>
    public class ConditionParser
    {
        private readonly ConditionTokenizer _tokenizer;
        private readonly ConfigurationItemFactory _configurationItemFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionParser"/> class.
        /// </summary>
        /// <param name="stringReader">The string reader.</param>
        /// <param name="configurationItemFactory">Instance of <see cref="ConfigurationItemFactory"/> used to resolve references to condition methods and layout renderers.</param>
        private ConditionParser(SimpleStringReader stringReader, ConfigurationItemFactory configurationItemFactory)
        {
            this._configurationItemFactory = configurationItemFactory;
            this._tokenizer = new ConditionTokenizer(stringReader);
        }

        /// <summary>
        /// Parses the specified condition string and turns it into
        /// <see cref="ConditionExpression"/> tree.
        /// </summary>
        /// <param name="expressionText">The expression to be parsed.</param>
        /// <returns>The root of the expression syntax tree which can be used to get the value of the condition in a specified context.</returns>
        public static ConditionExpression ParseExpression(string expressionText)
        {
            return ParseExpression(expressionText, ConfigurationItemFactory.Default);
        }

        /// <summary>
        /// Parses the specified condition string and turns it into
        /// <see cref="ConditionExpression"/> tree.
        /// </summary>
        /// <param name="expressionText">The expression to be parsed.</param>
        /// <param name="configurationItemFactories">Instance of <see cref="ConfigurationItemFactory"/> used to resolve references to condition methods and layout renderers.</param>
        /// <returns>The root of the expression syntax tree which can be used to get the value of the condition in a specified context.</returns>
        public static ConditionExpression ParseExpression(string expressionText, ConfigurationItemFactory configurationItemFactories)
        {
            if (expressionText == null)
            {
                return null;
            }

            var parser = new ConditionParser(new SimpleStringReader(expressionText), configurationItemFactories);
            ConditionExpression expression = parser.ParseExpression();
            if (!parser._tokenizer.IsEOF())
            {
                throw new ConditionParseException("Unexpected token: " + parser._tokenizer.TokenValue);
            }

            return expression;
        }

        /// <summary>
        /// Parses the specified condition string and turns it into
        /// <see cref="ConditionExpression"/> tree.
        /// </summary>
        /// <param name="stringReader">The string reader.</param>
        /// <param name="configurationItemFactories">Instance of <see cref="ConfigurationItemFactory"/> used to resolve references to condition methods and layout renderers.</param>
        /// <returns>
        /// The root of the expression syntax tree which can be used to get the value of the condition in a specified context.
        /// </returns>
        internal static ConditionExpression ParseExpression(SimpleStringReader stringReader, ConfigurationItemFactory configurationItemFactories)
        {
            var parser = new ConditionParser(stringReader, configurationItemFactories);
            ConditionExpression expression = parser.ParseExpression();
        
            return expression;
        }

        private ConditionMethodExpression ParsePredicate(string functionName)
        {
            var par = new List<ConditionExpression>();

            while (!this._tokenizer.IsEOF() && this._tokenizer.TokenType != ConditionTokenType.RightParen)
            {
                par.Add(ParseExpression());
                if (this._tokenizer.TokenType != ConditionTokenType.Comma)
                {
                    break;
                }

                this._tokenizer.GetNextToken();
            }

            this._tokenizer.Expect(ConditionTokenType.RightParen);

            try
            {
                var methodInfo = this._configurationItemFactory.ConditionMethods.CreateInstance(functionName);
                return new ConditionMethodExpression(functionName, methodInfo, par);
            }
            catch (Exception exception)
            {
                InternalLogger.Warn(exception, "Cannot resolve function '{0}'", functionName);

                if (exception.MustBeRethrownImmediately())
                {
                    throw;
                }

                throw new ConditionParseException("Cannot resolve function '" + functionName + "'", exception);
            }
        }

        private ConditionExpression ParseLiteralExpression()
        {
            if (this._tokenizer.IsToken(ConditionTokenType.LeftParen))
            {
                this._tokenizer.GetNextToken();
                ConditionExpression e = this.ParseExpression();
                this._tokenizer.Expect(ConditionTokenType.RightParen);
                return e;
            }

            if (this._tokenizer.IsToken(ConditionTokenType.Minus))
            {
                this._tokenizer.GetNextToken();
                if (!this._tokenizer.IsNumber())
                {
                    throw new ConditionParseException("Number expected, got " + this._tokenizer.TokenType);
                }

                string numberString = this._tokenizer.TokenValue;
                this._tokenizer.GetNextToken();
                if (numberString.IndexOf('.') >= 0)
                {
                    return new ConditionLiteralExpression(-double.Parse(numberString, CultureInfo.InvariantCulture));
                }

                return new ConditionLiteralExpression(-int.Parse(numberString, CultureInfo.InvariantCulture));
            }

            if (this._tokenizer.IsNumber())
            {
                string numberString = this._tokenizer.TokenValue;
                this._tokenizer.GetNextToken();
                if (numberString.IndexOf('.') >= 0)
                {
                    return new ConditionLiteralExpression(double.Parse(numberString, CultureInfo.InvariantCulture));
                }

                return new ConditionLiteralExpression(int.Parse(numberString, CultureInfo.InvariantCulture));
            }

            if (this._tokenizer.TokenType == ConditionTokenType.String)
            {
                ConditionExpression e = new ConditionLayoutExpression(Layout.FromString(this._tokenizer.StringTokenValue, this._configurationItemFactory));
                this._tokenizer.GetNextToken();
                return e;
            }

            if (this._tokenizer.TokenType == ConditionTokenType.Keyword)
            {
                string keyword = this._tokenizer.EatKeyword();

                if (0 == string.Compare(keyword, "level", StringComparison.OrdinalIgnoreCase))
                {
                    return new ConditionLevelExpression();
                }

                if (0 == string.Compare(keyword, "logger", StringComparison.OrdinalIgnoreCase))
                {
                    return new ConditionLoggerNameExpression();
                }

                if (0 == string.Compare(keyword, "message", StringComparison.OrdinalIgnoreCase))
                {
                    return new ConditionMessageExpression();
                }

                if (0 == string.Compare(keyword, "loglevel", StringComparison.OrdinalIgnoreCase))
                {
                    this._tokenizer.Expect(ConditionTokenType.Dot);
                    return new ConditionLiteralExpression(LogLevel.FromString(this._tokenizer.EatKeyword()));
                }

                if (0 == string.Compare(keyword, "true", StringComparison.OrdinalIgnoreCase))
                {
                    return new ConditionLiteralExpression(true);
                }

                if (0 == string.Compare(keyword, "false", StringComparison.OrdinalIgnoreCase))
                {
                    return new ConditionLiteralExpression(false);
                }

                if (0 == string.Compare(keyword, "null", StringComparison.OrdinalIgnoreCase))
                {
                    return new ConditionLiteralExpression(null);
                }

                if (this._tokenizer.TokenType == ConditionTokenType.LeftParen)
                {
                    this._tokenizer.GetNextToken();

                    ConditionMethodExpression predicateExpression = this.ParsePredicate(keyword);
                    return predicateExpression;
                }
            }

            throw new ConditionParseException("Unexpected token: " + this._tokenizer.TokenValue);
        }

        private ConditionExpression ParseBooleanRelation()
        {
            ConditionExpression e = this.ParseLiteralExpression();

            if (this._tokenizer.IsToken(ConditionTokenType.EqualTo))
            {
                this._tokenizer.GetNextToken();
                return new ConditionRelationalExpression(e, this.ParseLiteralExpression(), ConditionRelationalOperator.Equal);
            }

            if (this._tokenizer.IsToken(ConditionTokenType.NotEqual))
            {
                this._tokenizer.GetNextToken();
                return new ConditionRelationalExpression(e, this.ParseLiteralExpression(), ConditionRelationalOperator.NotEqual);
            }

            if (this._tokenizer.IsToken(ConditionTokenType.LessThan))
            {
                this._tokenizer.GetNextToken();
                return new ConditionRelationalExpression(e, this.ParseLiteralExpression(), ConditionRelationalOperator.Less);
            }

            if (this._tokenizer.IsToken(ConditionTokenType.GreaterThan))
            {
                this._tokenizer.GetNextToken();
                return new ConditionRelationalExpression(e, this.ParseLiteralExpression(), ConditionRelationalOperator.Greater);
            }

            if (this._tokenizer.IsToken(ConditionTokenType.LessThanOrEqualTo))
            {
                this._tokenizer.GetNextToken();
                return new ConditionRelationalExpression(e, this.ParseLiteralExpression(), ConditionRelationalOperator.LessOrEqual);
            }

            if (this._tokenizer.IsToken(ConditionTokenType.GreaterThanOrEqualTo))
            {
                this._tokenizer.GetNextToken();
                return new ConditionRelationalExpression(e, this.ParseLiteralExpression(), ConditionRelationalOperator.GreaterOrEqual);
            }

            return e;
        }

        private ConditionExpression ParseBooleanPredicate()
        {
            if (this._tokenizer.IsKeyword("not") || this._tokenizer.IsToken(ConditionTokenType.Not))
            {
                this._tokenizer.GetNextToken();
                return new ConditionNotExpression(this.ParseBooleanPredicate());
            }

            return this.ParseBooleanRelation();
        }

        private ConditionExpression ParseBooleanAnd()
        {
            ConditionExpression expression = this.ParseBooleanPredicate();

            while (this._tokenizer.IsKeyword("and") || this._tokenizer.IsToken(ConditionTokenType.And))
            {
                this._tokenizer.GetNextToken();
                expression = new ConditionAndExpression(expression, this.ParseBooleanPredicate());
            }

            return expression;
        }

        private ConditionExpression ParseBooleanOr()
        {
            ConditionExpression expression = this.ParseBooleanAnd();

            while (this._tokenizer.IsKeyword("or") || this._tokenizer.IsToken(ConditionTokenType.Or))
            {
                this._tokenizer.GetNextToken();
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