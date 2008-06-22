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
using System.Reflection;

using System.Xml.Serialization;
using System.Collections.Generic;
using NLog.Config;
using NLog.Layouts;

namespace NLog.Conditions
{
    /// <summary>
    /// Condition method invocation expression (represented by <b>method(p1,p2,p3)</b> syntax).
    /// </summary>
    internal sealed class ConditionMethodExpression : ConditionExpression
    {
        private string _name;
        private ICollection<ConditionExpression> _parameters;
        private MethodInfo _methodInfo;
        private bool _acceptsLogEvent;

        public ConditionMethodExpression() {}
        public ConditionMethodExpression(string name, ICollection<ConditionExpression> parameters) 
        {
            _name = name;
            _parameters = parameters;

            _methodInfo = NLogFactories.ConditionMethodFactory.Create(_name);
            ParameterInfo[] formalParameters = _methodInfo.GetParameters();
            if (formalParameters.Length >= 0)
            {
                _acceptsLogEvent = (formalParameters[0].ParameterType == typeof(LogEventInfo));
            }
            else
            {
                _acceptsLogEvent = false;
            }

            int actualParameterCount = _parameters.Count;
            if (_acceptsLogEvent)
                actualParameterCount++;
            if (formalParameters.Length != actualParameterCount)
            {
                Internal.InternalLogger.Error("Condition method: '{0}' expects {1} parameters. Passed {2}", name, formalParameters.Length, actualParameterCount);
                throw new ConditionParseException(String.Format("Condition method: '{0}' expects {1} parameters. Passed {2}", name, formalParameters.Length, actualParameterCount));
            }
        }

        public override object Evaluate(LogEventInfo context)
        {
            object[] callParameters;
            int parameterOffset = _acceptsLogEvent ? 1 : 0;

            callParameters = new object[_parameters.Count + parameterOffset];
            int i = 0;
            foreach (ConditionExpression ce in _parameters)
            {
                callParameters[i++ + parameterOffset] = ce.Evaluate(context);
            }

            if (_acceptsLogEvent)
                callParameters[0] = context;

            try
            {
                return _methodInfo.Invoke(null, callParameters);
            }
            catch (Exception ex)
            {
                Internal.InternalLogger.Error("Error: {0}", ex);
                return "";
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(_name);
            sb.Append("(");
            string separator = "";

            foreach (ConditionExpression expr in _parameters)
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
            foreach (ConditionExpression expr in _parameters)
            {
                expr.PopulateLayouts(layouts);
            }
        }
    }
}
