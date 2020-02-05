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

namespace NLog.Conditions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Text;
    using NLog.Common;
    using NLog.Internal;

    /// <summary>
    /// Condition method invocation expression (represented by <b>method(p1,p2,p3)</b> syntax).
    /// </summary>
	internal sealed class ConditionMethodExpression : ConditionExpression
    {
        private readonly string _conditionMethodName;
        private readonly bool _acceptsLogEvent;
        private readonly ConditionExpression[] _methodParameters;
        private readonly ReflectionHelpers.LateBoundMethod _lateBoundMethod;
        private readonly object[] _lateBoundMethodDefaultParameters;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionMethodExpression" /> class.
        /// </summary>
        /// <param name="conditionMethodName">Name of the condition method.</param>
        /// <param name="methodInfo"><see cref="MethodInfo"/> of the condition method.</param>
        /// <param name="lateBoundMethod">Precompiled delegate of the condition method.</param>
        /// <param name="methodParameters">The method parameters.</param>
        public ConditionMethodExpression(string conditionMethodName, MethodInfo methodInfo, ReflectionHelpers.LateBoundMethod lateBoundMethod, IEnumerable<ConditionExpression> methodParameters)
        {
            MethodInfo = methodInfo;
            _lateBoundMethod = lateBoundMethod;
            _conditionMethodName = conditionMethodName;
            _methodParameters = new List<ConditionExpression>(methodParameters).ToArray();
            ParameterInfo[] formalParameters = MethodInfo.GetParameters();
            if (formalParameters.Length > 0 && formalParameters[0].ParameterType == typeof(LogEventInfo))
            {
                _acceptsLogEvent = true;
            }
            _lateBoundMethodDefaultParameters = CreateMethodDefaultParameters(formalParameters, _methodParameters, _acceptsLogEvent ? 1 : 0);

            int actualParameterCount = _methodParameters.Length;
            if (_acceptsLogEvent)
            {
                actualParameterCount++;
            }

            // Count the number of required and optional parameters
            CountParmameters(formalParameters, out var requiredParametersCount, out var optionalParametersCount);

            if ( !( ( actualParameterCount >= requiredParametersCount ) && ( actualParameterCount <= formalParameters.Length ) ) )
            {
                string message;

                if ( optionalParametersCount > 0 )
                {
                    message = string.Format(
                        CultureInfo.InvariantCulture,
                        "Condition method '{0}' requires between {1} and {2} parameters, but passed {3}.",
                        conditionMethodName,
                        requiredParametersCount,
                        formalParameters.Length,
                        actualParameterCount );
                }
                else
                {
                    message = string.Format(
                        CultureInfo.InvariantCulture,
                        "Condition method '{0}' requires {1} parameters, but passed {2}.",
                        conditionMethodName,
                        requiredParametersCount,
                        actualParameterCount );
                }
                InternalLogger.Error(message);
                throw new ConditionParseException(message);
            }
        }

        /// <summary>
        /// Gets the method info.
        /// </summary>
        public MethodInfo MethodInfo { get; }

        private static object[] CreateMethodDefaultParameters(ParameterInfo[] formalParameters, ConditionExpression[] methodParameters, int parameterOffset)
        {
            var defaultParameterCount = formalParameters.Length - methodParameters.Length - parameterOffset;
            if (defaultParameterCount <= 0)
                return ArrayHelper.Empty<object>();

            var extraDefaultParameters = new object[defaultParameterCount];
            for (int i = methodParameters.Length + parameterOffset; i < formalParameters.Length; ++i)
            {
                ParameterInfo param = formalParameters[i];
                extraDefaultParameters[i - methodParameters.Length + parameterOffset] = param.DefaultValue;
            }

            return extraDefaultParameters;
        }

        private static void CountParmameters(ParameterInfo[] formalParameters, out int requiredParametersCount, out int optionalParametersCount)
        {
            requiredParametersCount = 0;
            optionalParametersCount = 0;

            foreach (var param in formalParameters)
            {
                if (param.IsOptional)
                    ++optionalParametersCount;
                else
                    ++requiredParametersCount;
            }
        }

        /// <summary>
        /// Returns a string representation of the expression.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the condition expression.
        /// </returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(_conditionMethodName);
            sb.Append("(");

            string separator = string.Empty;

            foreach (var expr in _methodParameters)
            {
                sb.Append(separator);
                sb.Append(expr);
                separator = ", ";
            }

            sb.Append(")");
            return sb.ToString();
        }

        /// <summary>
        /// Evaluates the expression.
        /// </summary>
        /// <param name="context">Evaluation context.</param>
        /// <returns>Expression result.</returns>
        protected override object EvaluateNode(LogEventInfo context)
        {
            object[] callParameters = GenerateCallParameters(context);
            return _lateBoundMethod(null, callParameters);  // Static-method so object-instance = null
        }

        private object[] GenerateCallParameters(LogEventInfo context)
        {
            int parameterOffset = _acceptsLogEvent ? 1 : 0;
            int callParametersCount = _methodParameters.Length + parameterOffset + _lateBoundMethodDefaultParameters.Length;
            if (callParametersCount == 0)
                return ArrayHelper.Empty<object>();

            var callParameters = new object[callParametersCount];

            if (_acceptsLogEvent)
            {
                callParameters[0] = context;
            }

            //Memory profiling pointed out that using a foreach-loop was allocating
            //an Enumerator. Switching to a for-loop avoids the memory allocation.
            for (int i = 0; i < _methodParameters.Length; i++)
            {
                ConditionExpression ce = _methodParameters[i];
                callParameters[i + parameterOffset] = ce.Evaluate(context);
            }

            if (_lateBoundMethodDefaultParameters.Length > 0)
            {
                for (int i = _lateBoundMethodDefaultParameters.Length - 1; i >= 0; --i)
                {
                    callParameters[callParameters.Length - i - 1] = _lateBoundMethodDefaultParameters[i];
                }
            }

            return callParameters;
        }
    }
}