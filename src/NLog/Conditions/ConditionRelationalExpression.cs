// 
// Copyright (c) 2004-2016 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using System.Globalization;

    /// <summary>
    /// Condition relational (<b>==</b>, <b>!=</b>, <b>&lt;</b>, <b>&lt;=</b>,
    /// <b>&gt;</b> or <b>&gt;=</b>) expression.
    /// </summary>
    internal sealed class ConditionRelationalExpression : ConditionExpression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionRelationalExpression" /> class.
        /// </summary>
        /// <param name="leftExpression">The left expression.</param>
        /// <param name="rightExpression">The right expression.</param>
        /// <param name="relationalOperator">The relational operator.</param>
        public ConditionRelationalExpression(ConditionExpression leftExpression, ConditionExpression rightExpression, ConditionRelationalOperator relationalOperator)
        {
            this.LeftExpression = leftExpression;
            this.RightExpression = rightExpression;
            this.RelationalOperator = relationalOperator;
        }

        /// <summary>
        /// Gets the left expression.
        /// </summary>
        /// <value>The left expression.</value>
        public ConditionExpression LeftExpression { get; private set; }

        /// <summary>
        /// Gets the right expression.
        /// </summary>
        /// <value>The right expression.</value>
        public ConditionExpression RightExpression { get; private set; }

        /// <summary>
        /// Gets the relational operator.
        /// </summary>
        /// <value>The operator.</value>
        public ConditionRelationalOperator RelationalOperator { get; private set; }

        /// <summary>
        /// Returns a string representation of the expression.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the condition expression.
        /// </returns>
        public override string ToString()
        {
            return "(" + this.LeftExpression + " " + this.GetOperatorString() + " " + this.RightExpression + ")";
        }

        /// <summary>
        /// Evaluates the expression.
        /// </summary>
        /// <param name="context">Evaluation context.</param>
        /// <returns>Expression result.</returns>
        protected override object EvaluateNode(LogEventInfo context)
        {
            object v1 = this.LeftExpression.Evaluate(context);
            object v2 = this.RightExpression.Evaluate(context);

            return Compare(v1, v2, this.RelationalOperator);
        }

        /// <summary>
        /// Compares the specified values using specified relational operator.
        /// </summary>
        /// <param name="leftValue">The first value.</param>
        /// <param name="rightValue">The second value.</param>
        /// <param name="relationalOperator">The relational operator.</param>
        /// <returns>Result of the given relational operator.</returns>
        private static object Compare(object leftValue, object rightValue, ConditionRelationalOperator relationalOperator)
        {
            StringComparer comparer = StringComparer.InvariantCulture;
            PromoteTypes(ref leftValue, ref rightValue);
            switch (relationalOperator)
            {
                case ConditionRelationalOperator.Equal:
                    return comparer.Compare(leftValue, rightValue) == 0;

                case ConditionRelationalOperator.NotEqual:
                    return comparer.Compare(leftValue, rightValue) != 0;

                case ConditionRelationalOperator.Greater:
                    return comparer.Compare(leftValue, rightValue) > 0;

                case ConditionRelationalOperator.GreaterOrEqual:
                    return comparer.Compare(leftValue, rightValue) >= 0;

                case ConditionRelationalOperator.LessOrEqual:
                    return comparer.Compare(leftValue, rightValue) <= 0;

                case ConditionRelationalOperator.Less:
                    return comparer.Compare(leftValue, rightValue) < 0;

                default:
                    throw new NotSupportedException("Relational operator " + relationalOperator + " is not supported.");
            }
        }

        private static void PromoteTypes(ref object val1, ref object val2)
        {
            if (val1 == null || val2 == null)
            {
                return;
            }

            if (val1.GetType() == val2.GetType())
            {
                return;
            }

            if (val1 is DateTime || val2 is DateTime)
            {
                val1 = Convert.ToDateTime(val1, CultureInfo.InvariantCulture);
                val2 = Convert.ToDateTime(val2, CultureInfo.InvariantCulture);
                return;
            }

            if (val1 is string || val2 is string)
            {
                val1 = Convert.ToString(val1, CultureInfo.InvariantCulture);
                val2 = Convert.ToString(val2, CultureInfo.InvariantCulture);
                return;
            }

            if (val1 is double || val2 is double)
            {
                val1 = Convert.ToDouble(val1, CultureInfo.InvariantCulture);
                val2 = Convert.ToDouble(val2, CultureInfo.InvariantCulture);
                return;
            }

            if (val1 is float || val2 is float)
            {
                val1 = Convert.ToSingle(val1, CultureInfo.InvariantCulture);
                val2 = Convert.ToSingle(val2, CultureInfo.InvariantCulture);
                return;
            }

            if (val1 is decimal || val2 is decimal)
            {
                val1 = Convert.ToDecimal(val1, CultureInfo.InvariantCulture);
                val2 = Convert.ToDecimal(val2, CultureInfo.InvariantCulture);
                return;
            }

            if (val1 is long || val2 is long)
            {
                val1 = Convert.ToInt64(val1, CultureInfo.InvariantCulture);
                val2 = Convert.ToInt64(val2, CultureInfo.InvariantCulture);
                return;
            }

            if (val1 is int || val2 is int)
            {
                val1 = Convert.ToInt32(val1, CultureInfo.InvariantCulture);
                val2 = Convert.ToInt32(val2, CultureInfo.InvariantCulture);
                return;
            }

            if (val1 is bool || val2 is bool)
            {
                val1 = Convert.ToBoolean(val1, CultureInfo.InvariantCulture);
                val2 = Convert.ToBoolean(val2, CultureInfo.InvariantCulture);
                return;
            }

            throw new ConditionEvaluationException("Cannot find common type for '" + val1.GetType().Name + "' and '" + val2.GetType().Name + "'.");
        }

        private string GetOperatorString()
        {
            switch (this.RelationalOperator)
            {
                case ConditionRelationalOperator.Equal:
                    return "==";

                case ConditionRelationalOperator.NotEqual:
                    return "!=";

                case ConditionRelationalOperator.Greater:
                    return ">";

                case ConditionRelationalOperator.Less:
                    return "<";

                case ConditionRelationalOperator.GreaterOrEqual:
                    return ">=";

                case ConditionRelationalOperator.LessOrEqual:
                    return "<=";

                default:
                    throw new NotSupportedException("Relational operator " + this.RelationalOperator + " is not supported.");
            }
        }
    }
}