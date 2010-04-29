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
using System.Collections;
using System.ComponentModel;

using System.Globalization;

using System.Xml.Serialization;

namespace NLog.Conditions 
{
    /// <summary>
    /// Condition relational (<b>==</b>, <b>!=</b>, <b>&lt;</b>, <b>&lt;=</b>, 
    /// <b>&gt;</b> or <b>&gt;=</b>) expression.
    /// </summary>
    internal sealed class ConditionRelationalExpression : ConditionExpression 
    {
        public ConditionExpression par1;
        public ConditionExpression par2;
        public ConditionRelationalOperator op;

        public ConditionRelationalExpression() {}

        public ConditionRelationalExpression(ConditionExpression par1, ConditionExpression par2, ConditionRelationalOperator op) 
        {
            this.par1 = par1;
            this.par2 = par2;
            this.op = op;
        }

        private static void PromoteTypes(ref object val1, ref object val2)
        {
            if (val1.GetType() == val2.GetType())
                return;

            if (val1 is DateTime || val2 is DateTime)
            {
                val1 = Convert.ToDateTime(val1);
                val2 = Convert.ToDateTime(val2);
                return;
            }

            if (val1 is string || val2 is string)
            {
                val1 = Convert.ToString(val1);
                val2 = Convert.ToString(val2);
                return;
            }
            if (val1 is double || val2 is double)
            {
                val1 = Convert.ToDouble(val1);
                val2 = Convert.ToDouble(val2);
                return;
            }

            if (val1 is float || val2 is float)
            {
                val1 = Convert.ToSingle(val1);
                val2 = Convert.ToSingle(val2);
                return;
            }
            if (val1 is decimal || val2 is decimal)
            {
                val1 = Convert.ToDecimal(val1);
                val2 = Convert.ToDecimal(val2);
                return;
            }
            if (val1 is long || val2 is long)
            {
                val1 = Convert.ToInt64(val1);
                val2 = Convert.ToInt64(val2);
                return;
            }
            if (val1 is int || val2 is int)
            {
                val1 = Convert.ToInt32(val1);
                val2 = Convert.ToInt32(val2);
                return;
            }

            if (val1 is bool || val2 is bool)
            {
                val1 = Convert.ToBoolean(val1);
                val2 = Convert.ToBoolean(val2);
                return;
            }
            throw new Exception("Cannot promote types " + val1.GetType().Name + " and " + val2.GetType().Name + " to one type.");
        }

        public static object Compare(object v1, object v2, ConditionRelationalOperator op) 
        {
            if (v1 == null || v2 == null)
                return null;

            IComparer comparer = Comparer.Default;
            PromoteTypes(ref v1, ref v2);
            switch (op)
            {
                case ConditionRelationalOperator.Equal:
                    return comparer.Compare(v1, v2) == 0;

                case ConditionRelationalOperator.NotEqual:
                    return comparer.Compare(v1, v2) != 0;

                case ConditionRelationalOperator.Greater:
                    return comparer.Compare(v1, v2) > 0;

                case ConditionRelationalOperator.GreaterOrEqual:
                    return comparer.Compare(v1, v2) >= 0;

                case ConditionRelationalOperator.LessOrEqual:
                    return comparer.Compare(v1, v2) <= 0;

                case ConditionRelationalOperator.Less:
                    return comparer.Compare(v1, v2) < 0;

                default:
                    throw new NotSupportedException("Relational operator " + op + " is not supported.");
            }
        }

        public override object Evaluate(LogEventInfo context)
        {
            object v1 = par1.Evaluate(context);
            object v2 = par2.Evaluate(context);

            return Compare(v1, v2, op);
        }

        public string OperatorString
        {
            get 
            {
                switch (op)
                {
                    case ConditionRelationalOperator.Equal: return "==";
                    case ConditionRelationalOperator.NotEqual: return "!=";
                    case ConditionRelationalOperator.Greater: return ">";
                    case ConditionRelationalOperator.Less: return "<";
                    case ConditionRelationalOperator.GreaterOrEqual: return ">=";
                    case ConditionRelationalOperator.LessOrEqual: return "<=";
                }
                return "";
            }
        }

        public override string ToString()
        {
            return par1.ToString() + " " + OperatorString + " " + par2.ToString();
        }

        /// <summary>
        /// Adds all layouts used by this expression to the specified collection.
        /// </summary>
        /// <param name="layouts">The collection to add layouts to.</param>
        public override void PopulateLayouts(LayoutCollection layouts)
        {
            par1.PopulateLayouts(layouts);
            par2.PopulateLayouts(layouts);
        }
    }
}
