// 
// Copyright (c) 2004-2021 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using System.Collections.Generic;
    using System.Globalization;
    using NLog.Common;
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
            _configurationItemFactory = configurationItemFactory;
            _tokenizer = new ConditionTokenizer(stringReader);
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
            if (expressionText is null)
            {
                return null;
            }

            var parser = new ConditionParser(new SimpleStringReader(expressionText), configurationItemFactories);
            ConditionExpression expression = parser.ParseExpression();
            if (!parser._tokenizer.IsEOF())
            {
                throw new ConditionParseException($"Unexpected token: {parser._tokenizer.TokenValue}");
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

        private ConditionMethodExpression ParseMethodPredicate(string functionName)
        {
            var inputParameters = new List<ConditionExpression>();

            while (!_tokenizer.IsEOF() && _tokenizer.TokenType != ConditionTokenType.RightParen)
            {
                inputParameters.Add(ParseExpression());
                if (_tokenizer.TokenType != ConditionTokenType.Comma)
                {
                    break;
                }

                _tokenizer.GetNextToken();
            }

            _tokenizer.Expect(ConditionTokenType.RightParen);

            try
            {
                return CreateMethodExpression(functionName, inputParameters);
            }
            catch (Exception exception)
            {
                InternalLogger.Warn(exception, "Failed to resolve condition method: '{0}'", functionName);

                if (exception.MustBeRethrownImmediately())
                {
                    throw;
                }

                throw new ConditionParseException($"Cannot resolve function '{functionName}'", exception);
            }
        }

        private ConditionMethodExpression CreateMethodExpression(string functionName, List<ConditionExpression> inputParameters)
        {
            // Attempt to lookup functionName that can handle the provided number of input-parameters
            if (inputParameters.Count == 0)
            {
                Func<LogEventInfo, object> method = _configurationItemFactory.ConditionMethodFactory.TryCreateInstanceWithNoParameters(functionName);
                if (method != null)
                    return ConditionMethodExpression.CreateMethodNoParameters(functionName, method);
            }
            else if (inputParameters.Count == 1)
            {
                Func<LogEventInfo, object, object> method = _configurationItemFactory.ConditionMethodFactory.TryCreateInstanceWithOneParameter(functionName);
                if (method != null)
                    return ConditionMethodExpression.CreateMethodOneParameter(functionName, method, inputParameters);
            }
            else if (inputParameters.Count == 2)
            {
                Func<LogEventInfo, object, object, object> method = _configurationItemFactory.ConditionMethodFactory.TryCreateInstanceWithTwoParameters(functionName);
                if (method != null)
                    return ConditionMethodExpression.CreateMethodTwoParameters(functionName, method, inputParameters);
            }
            else if (inputParameters.Count == 3)
            {
                Func<LogEventInfo, object, object, object, object> method = _configurationItemFactory.ConditionMethodFactory.TryCreateInstanceWithThreeParameters(functionName);
                if (method != null)
                    return ConditionMethodExpression.CreateMethodThreeParameters(functionName, method, inputParameters);
            }

            Func<object[], object> manyParameterMethod = _configurationItemFactory.ConditionMethodFactory.TryCreateInstanceWithManyParameters(functionName, out var manyParameterMinCount, out var manyParameterMaxCount, out var manyParameterWithLogEvent);
            if (manyParameterMethod is null)
                throw new ConditionParseException($"Unknown condition method '{functionName}'");
            if (manyParameterMinCount > inputParameters.Count)
                throw new ConditionParseException($"Condition method '{functionName}' requires minimum {manyParameterMinCount} parameters, but passed {inputParameters.Count}.");
            if (manyParameterMaxCount < inputParameters.Count)
                throw new ConditionParseException($"Condition method '{functionName}' requires maximum {manyParameterMaxCount} parameters, but passed {inputParameters.Count}.");
            return ConditionMethodExpression.CreateMethodManyParameters(functionName, manyParameterMethod, inputParameters, manyParameterWithLogEvent);
        }

        private ConditionExpression ParseLiteralExpression()
        {
            if (_tokenizer.IsToken(ConditionTokenType.LeftParen))
            {
                _tokenizer.GetNextToken();
                ConditionExpression e = ParseExpression();
                _tokenizer.Expect(ConditionTokenType.RightParen);
                return e;
            }

            if (_tokenizer.IsToken(ConditionTokenType.Minus))
            {
                _tokenizer.GetNextToken();
                if (!_tokenizer.IsNumber())
                {
                    throw new ConditionParseException($"Number expected, got {_tokenizer.TokenType}");
                }

                return ParseNumber(true);
            }

            if (_tokenizer.IsNumber())
            {
                return ParseNumber(false);
            }

            if (_tokenizer.TokenType == ConditionTokenType.String)
            {
                var simpleLayout = new SimpleLayout(_tokenizer.StringTokenValue, _configurationItemFactory);
                _tokenizer.GetNextToken();
                if (simpleLayout.IsFixedText)
                    return new ConditionLiteralExpression(simpleLayout.FixedText);
                else
                    return new ConditionLayoutExpression(simpleLayout);
            }

            if (_tokenizer.TokenType == ConditionTokenType.Keyword)
            {
                string keyword = _tokenizer.EatKeyword();

                if (TryPlainKeywordToExpression(keyword, out var expression))
                {
                    return expression;
                }

                if (_tokenizer.TokenType == ConditionTokenType.LeftParen)
                {
                    _tokenizer.GetNextToken();

                    var conditionMethodExpression = ParseMethodPredicate(keyword);
                    return conditionMethodExpression;
                }
            }

            throw new ConditionParseException("Unexpected token: " + _tokenizer.TokenValue);
        }

        /// <summary>
        /// Try stringed keyword to <see cref="ConditionExpression"/>
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="expression"></param>
        /// <returns>success?</returns>
        private bool TryPlainKeywordToExpression(string keyword, out ConditionExpression expression)
        {
            if (string.Equals(keyword, "level", StringComparison.OrdinalIgnoreCase))
            {
                expression = new ConditionLevelExpression();
                return true;
            }

            if (string.Equals(keyword, "logger", StringComparison.OrdinalIgnoreCase))
            {
                expression = new ConditionLoggerNameExpression();
                return true;
            }

            if (string.Equals(keyword, "message", StringComparison.OrdinalIgnoreCase))
            {
                expression = new ConditionMessageExpression();
                return true;
            }

            if (string.Equals(keyword, "exception", StringComparison.OrdinalIgnoreCase))
            {
                expression = new ConditionExceptionExpression();
                return true;
            }

            if (string.Equals(keyword, "loglevel", StringComparison.OrdinalIgnoreCase))
            {
                _tokenizer.Expect(ConditionTokenType.Dot);
                expression = new ConditionLiteralExpression(LogLevel.FromString(_tokenizer.EatKeyword()));
                return true;
            }

            if (string.Equals(keyword, "true", StringComparison.OrdinalIgnoreCase))
            {
                expression = new ConditionLiteralExpression(ConditionExpression.BoxedTrue);
                return true;
            }

            if (string.Equals(keyword, "false", StringComparison.OrdinalIgnoreCase))
            {
                expression = new ConditionLiteralExpression(ConditionExpression.BoxedFalse);
                return true;
            }

            if (string.Equals(keyword, "null", StringComparison.OrdinalIgnoreCase))
            {
                expression = new ConditionLiteralExpression(null);
                return true;
            }

            expression = null;
            return false;
        }

        /// <summary>
        /// Parse number
        /// </summary>
        /// <param name="negative">negative number? minus should be parsed first.</param>
        /// <returns></returns>
        private ConditionExpression ParseNumber(bool negative)
        {
            string numberString = _tokenizer.TokenValue;
            _tokenizer.GetNextToken();
            if (numberString.IndexOf('.') >= 0)
            {
                var d = double.Parse(numberString, CultureInfo.InvariantCulture);
                
                return new ConditionLiteralExpression(negative ? -d : d);
            }

            var i = int.Parse(numberString, CultureInfo.InvariantCulture);
            return new ConditionLiteralExpression(negative ? -i : i);
        }

        private ConditionExpression ParseBooleanRelation()
        {
            ConditionExpression e = ParseLiteralExpression();

            if (_tokenizer.IsToken(ConditionTokenType.EqualTo))
            {
                _tokenizer.GetNextToken();
                return new ConditionRelationalExpression(e, ParseLiteralExpression(), ConditionRelationalOperator.Equal);
            }

            if (_tokenizer.IsToken(ConditionTokenType.NotEqual))
            {
                _tokenizer.GetNextToken();
                return new ConditionRelationalExpression(e, ParseLiteralExpression(), ConditionRelationalOperator.NotEqual);
            }

            if (_tokenizer.IsToken(ConditionTokenType.LessThan))
            {
                _tokenizer.GetNextToken();
                return new ConditionRelationalExpression(e, ParseLiteralExpression(), ConditionRelationalOperator.Less);
            }

            if (_tokenizer.IsToken(ConditionTokenType.GreaterThan))
            {
                _tokenizer.GetNextToken();
                return new ConditionRelationalExpression(e, ParseLiteralExpression(), ConditionRelationalOperator.Greater);
            }

            if (_tokenizer.IsToken(ConditionTokenType.LessThanOrEqualTo))
            {
                _tokenizer.GetNextToken();
                return new ConditionRelationalExpression(e, ParseLiteralExpression(), ConditionRelationalOperator.LessOrEqual);
            }

            if (_tokenizer.IsToken(ConditionTokenType.GreaterThanOrEqualTo))
            {
                _tokenizer.GetNextToken();
                return new ConditionRelationalExpression(e, ParseLiteralExpression(), ConditionRelationalOperator.GreaterOrEqual);
            }

            return e;
        }

        private ConditionExpression ParseBooleanPredicate()
        {
            if (_tokenizer.IsKeyword("not") || _tokenizer.IsToken(ConditionTokenType.Not))
            {
                _tokenizer.GetNextToken();
                return new ConditionNotExpression(ParseBooleanPredicate());
            }

            return ParseBooleanRelation();
        }

        private ConditionExpression ParseBooleanAnd()
        {
            ConditionExpression expression = ParseBooleanPredicate();

            while (_tokenizer.IsKeyword("and") || _tokenizer.IsToken(ConditionTokenType.And))
            {
                _tokenizer.GetNextToken();
                expression = new ConditionAndExpression(expression, ParseBooleanPredicate());
            }

            return expression;
        }

        private ConditionExpression ParseBooleanOr()
        {
            ConditionExpression expression = ParseBooleanAnd();

            while (_tokenizer.IsKeyword("or") || _tokenizer.IsToken(ConditionTokenType.Or))
            {
                _tokenizer.GetNextToken();
                expression = new ConditionOrExpression(expression, ParseBooleanAnd());
            }

            return expression;
        }

        private ConditionExpression ParseBooleanExpression()
        {
            return ParseBooleanOr();
        }

        private ConditionExpression ParseExpression()
        {
            return ParseBooleanExpression();
        }
    }
}