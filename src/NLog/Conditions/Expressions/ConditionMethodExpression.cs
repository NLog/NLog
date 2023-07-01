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
    using System.Text;
    using NLog.Internal;

    internal sealed class ConditionMethodExpression : ConditionExpression
    {
        private readonly IEvaluateMethod _method;

        public string MethodName { get; }

        /// <summary>
        /// Gets the method parameters
        /// </summary>
        public IList<ConditionExpression> MethodParameters { get; }

        private ConditionMethodExpression(string methodName, IList<ConditionExpression> methodParameters, IEvaluateMethod method)
        {
            MethodName = Guard.ThrowIfNull(methodName);
            _method = Guard.ThrowIfNull(method);
            MethodParameters = Guard.ThrowIfNull(methodParameters);
        }

        /// <inheritdoc />
        protected override object EvaluateNode(LogEventInfo context)
        {
            return _method.EvaluateNode(context);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(MethodName);
            sb.Append('(');

            string separator = string.Empty;

            foreach (var expr in MethodParameters)
            {
                sb.Append(separator);
                sb.Append(expr);
                separator = ", ";
            }

            sb.Append(')');
            return sb.ToString();
        }

        public static ConditionMethodExpression CreateMethodNoParameters(string conditionMethodName, Func<LogEventInfo, object> method)
        {
            return new ConditionMethodExpression(conditionMethodName, ArrayHelper.Empty<ConditionExpression>(), new EvaluateMethodNoParameters(method));
        }

        public static ConditionMethodExpression CreateMethodOneParameter(string conditionMethodName, Func<LogEventInfo, object, object> method, IList<ConditionExpression> methodParameters)
        {
            var methodParameter = methodParameters[0];
            return new ConditionMethodExpression(conditionMethodName, methodParameters, new EvaluateMethodOneParameter(method, (logEvent) => methodParameter.Evaluate(logEvent)));
        }

        public static ConditionMethodExpression CreateMethodTwoParameters(string conditionMethodName, Func<LogEventInfo, object, object, object> method, IList<ConditionExpression> methodParameters)
        {
            var methodParameterArg1 = methodParameters[0];
            var methodParameterArg2 = methodParameters[1];
            return new ConditionMethodExpression(conditionMethodName, methodParameters, new EvaluateMethodTwoParameters(method, (logEvent) => methodParameterArg1.Evaluate(logEvent), (logEvent) => methodParameterArg2.Evaluate(logEvent)));
        }

        public static ConditionMethodExpression CreateMethodThreeParameters(string conditionMethodName, Func<LogEventInfo, object, object, object, object> method, IList<ConditionExpression> methodParameters)
        {
            var methodParameterArg1 = methodParameters[0];
            var methodParameterArg2 = methodParameters[1];
            var methodParameterArg3 = methodParameters[2];
            return new ConditionMethodExpression(conditionMethodName, methodParameters, new EvaluateMethodThreeParameters(method, (logEvent) => methodParameterArg1.Evaluate(logEvent), (logEvent) => methodParameterArg2.Evaluate(logEvent), (logEvent) => methodParameterArg3.Evaluate(logEvent)));
        }

        public static ConditionMethodExpression CreateMethodManyParameters(string conditionMethodName, Func<object[], object> method, IList<ConditionExpression> methodParameters, bool includeLogEvent)
        {
            return new ConditionMethodExpression(conditionMethodName, methodParameters, new EvaluateMethodManyParameters(method, methodParameters, includeLogEvent));
        }

        private interface IEvaluateMethod
        {
            object EvaluateNode(LogEventInfo logEvent);
        }

        private sealed class EvaluateMethodNoParameters : IEvaluateMethod
        {
            private readonly Func<LogEventInfo, object> _method;

            public EvaluateMethodNoParameters(Func<LogEventInfo, object> method)
            {
                _method = Guard.ThrowIfNull(method);
            }

            public object EvaluateNode(LogEventInfo logEvent)
            {
                return _method(logEvent);
            }
        }

        private sealed class EvaluateMethodOneParameter : IEvaluateMethod
        {
            private readonly Func<LogEventInfo, object, object> _method;
            private readonly Func<LogEventInfo, object> _methodParameter;

            public EvaluateMethodOneParameter(Func<LogEventInfo, object, object> method, Func<LogEventInfo, object> methodParameter)
            {
                _method = Guard.ThrowIfNull(method);
                _methodParameter = Guard.ThrowIfNull(methodParameter);
            }

            public object EvaluateNode(LogEventInfo logEvent)
            {
                var inputParameter = _methodParameter(logEvent);
                return _method(logEvent, inputParameter);
            }
        }

        private sealed class EvaluateMethodTwoParameters : IEvaluateMethod
        {
            private readonly Func<LogEventInfo, object, object, object> _method;
            private readonly Func<LogEventInfo, object> _methodParameterArg1;
            private readonly Func<LogEventInfo, object> _methodParameterArg2;

            public EvaluateMethodTwoParameters(Func<LogEventInfo, object, object, object> method, Func<LogEventInfo, object> methodParameterArg1, Func<LogEventInfo, object> methodParameterArg2)
            {
                _method = Guard.ThrowIfNull(method);
                _methodParameterArg1 = Guard.ThrowIfNull(methodParameterArg1);
                _methodParameterArg2 = Guard.ThrowIfNull(methodParameterArg2);
            }

            public object EvaluateNode(LogEventInfo logEvent)
            {
                var inputParameter1 = _methodParameterArg1(logEvent);
                var inputParameter2 = _methodParameterArg2(logEvent);
                return _method(logEvent, inputParameter1, inputParameter2);
            }
        }

        private sealed class EvaluateMethodThreeParameters : IEvaluateMethod
        {
            private readonly Func<LogEventInfo, object, object, object, object> _method;
            private readonly Func<LogEventInfo, object> _methodParameterArg1;
            private readonly Func<LogEventInfo, object> _methodParameterArg2;
            private readonly Func<LogEventInfo, object> _methodParameterArg3;

            public EvaluateMethodThreeParameters(Func<LogEventInfo, object, object, object, object> method, Func<LogEventInfo, object> methodParameterArg1, Func<LogEventInfo, object> methodParameterArg2, Func<LogEventInfo, object> methodParameterArg3)
            {
                _method = Guard.ThrowIfNull(method);
                _methodParameterArg1 = Guard.ThrowIfNull(methodParameterArg1);
                _methodParameterArg2 = Guard.ThrowIfNull(methodParameterArg2);
                _methodParameterArg3 = Guard.ThrowIfNull(methodParameterArg3);
            }

            public object EvaluateNode(LogEventInfo logEvent)
            {
                var inputParameter1 = _methodParameterArg1(logEvent);
                var inputParameter2 = _methodParameterArg2(logEvent);
                var inputParameter3 = _methodParameterArg3(logEvent);
                return _method(logEvent, inputParameter1, inputParameter2, inputParameter3);
            }
        }

        private sealed class EvaluateMethodManyParameters : IEvaluateMethod
        {
            private readonly Func<object[], object> _method;
            private readonly IList<ConditionExpression> _methodParameters;
            private readonly bool _includeLogEvent;

            public EvaluateMethodManyParameters(Func<object[], object> method, IList<ConditionExpression> inputParameters, bool includeLogEvent)
            {
                _method = Guard.ThrowIfNull(method);
                _methodParameters = Guard.ThrowIfNull(inputParameters);
                _includeLogEvent = includeLogEvent;
            }

            public object EvaluateNode(LogEventInfo logEvent)
            {
                var parameterIndex = _includeLogEvent ? 1 : 0;
                var inputParameters = new object[_methodParameters.Count + parameterIndex];
                if (_includeLogEvent)
                    inputParameters[0] = logEvent;
                for (int i = 0; i < _methodParameters.Count; ++i)
                {
                    var inputParameter = _methodParameters[i].Evaluate(logEvent);
                    inputParameters[parameterIndex++] = inputParameter;
                }

                return _method.Invoke(inputParameters);
            }
        }
    }
}
