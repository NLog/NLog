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
using System.Reflection;
using System.Text;
using NLog.Internal;
using NLog.Layouts;

namespace NLog.Conditions
{
    /// <summary>
    /// Condition method invocation expression (represented by <b>method(p1,p2,p3)</b> syntax).
    /// </summary>
    internal sealed class ConditionMethodExpression : ConditionExpression
    {
        private readonly string conditionMethodName;
        private readonly bool acceptsLogEvent;
        private readonly MethodInfo methodInfo;
        private readonly ICollection<ConditionExpression> methodParameters;

        /// <summary>
        /// Initializes a new instance of the ConditionMethodExpression class.
        /// </summary>
        public ConditionMethodExpression()
        {
        }

        /// <summary>
        /// Initializes a new instance of the ConditionMethodExpression class.
        /// </summary>
        /// <param name="conditionMethodName">Name of the condition method.</param>
        /// <param name="methodInfo"><see cref="MethodInfo"/> of the condition method.</param>
        /// <param name="methodParameters">The method parameters.</param>
        public ConditionMethodExpression(string conditionMethodName, MethodInfo methodInfo, ICollection<ConditionExpression> methodParameters)
        {
            this.methodInfo = methodInfo;
            this.conditionMethodName = conditionMethodName;
            this.methodParameters = methodParameters;

            ParameterInfo[] formalParameters = this.methodInfo.GetParameters();
            if (formalParameters.Length >= 0 && formalParameters[0].ParameterType == typeof(LogEventInfo))
            {
                this.acceptsLogEvent = true;
            }

            int actualParameterCount = this.methodParameters.Count;
            if (this.acceptsLogEvent)
            {
                actualParameterCount++;
            }

            if (formalParameters.Length != actualParameterCount)
            {
                string message = String.Format(
                    CultureInfo.InvariantCulture,
                    "Condition method: '{0}' expects {1} parameters. Passed {2}",
                    conditionMethodName,
                    formalParameters.Length,
                    actualParameterCount);

                InternalLogger.Error(message);
                throw new ConditionParseException(message);
            }
        }

        /// <summary>
        /// Evaluates the expression.
        /// </summary>
        /// <param name="context">Evaluation context.</param>
        /// <returns>Expression result.</returns>
        public override object Evaluate(LogEventInfo context)
        {
            object[] callParameters;
            int parameterOffset = this.acceptsLogEvent ? 1 : 0;

            callParameters = new object[this.methodParameters.Count + parameterOffset];
            int i = 0;
            foreach (ConditionExpression ce in this.methodParameters)
            {
                callParameters[i++ + parameterOffset] = ce.Evaluate(context);
            }

            if (this.acceptsLogEvent)
            {
                callParameters[0] = context;
            }

            try
            {
                return this.methodInfo.Invoke(null, callParameters);
            }
            catch (Exception ex)
            {
                InternalLogger.Error(CultureInfo.InvariantCulture, "Error: {0}", ex);
                return string.Empty;
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
            sb.Append(this.conditionMethodName);
            sb.Append("(");

            string separator = string.Empty;
            foreach (ConditionExpression expr in this.methodParameters)
            {
                sb.Append(separator);
                sb.Append(expr);
                separator = ",";
            }

            sb.Append(")");
            return sb.ToString();
        }

        /// <summary>
        /// Adds all layouts used by this expression to the specified collection.
        /// </summary>
        /// <param name="layouts">The collection to add layouts to.</param>
        public override void PopulateLayouts(ICollection<Layout> layouts)
        {
            foreach (ConditionExpression expr in this.methodParameters)
            {
                expr.PopulateLayouts(layouts);
            }
        }
    }
}