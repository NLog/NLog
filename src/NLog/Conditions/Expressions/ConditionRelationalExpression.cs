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
    using System.Globalization;
    using System.Collections.Generic;
    using NLog.Common;

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
            LeftExpression = leftExpression;
            RightExpression = rightExpression;
            RelationalOperator = relationalOperator;
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
            return $"({LeftExpression} {GetOperatorString()} {RightExpression})";
        }

        /// <summary>
        /// Evaluates the expression.
        /// </summary>
        /// <param name="context">Evaluation context.</param>
        /// <returns>Expression result.</returns>
        protected override object EvaluateNode(LogEventInfo context)
        {
            object v1 = LeftExpression.Evaluate(context);
            object v2 = RightExpression.Evaluate(context);

            return Compare(v1, v2, RelationalOperator) ? BoxedTrue : BoxedFalse;
        }

        /// <summary>
        /// Compares the specified values using specified relational operator.
        /// </summary>
        /// <param name="leftValue">The first value.</param>
        /// <param name="rightValue">The second value.</param>
        /// <param name="relationalOperator">The relational operator.</param>
        /// <returns>Result of the given relational operator.</returns>
        private static bool Compare(object leftValue, object rightValue, ConditionRelationalOperator relationalOperator)
        {
#if !NETSTANDARD1_3 && !NETSTANDARD1_5
            System.Collections.IComparer comparer = StringComparer.InvariantCulture;
#else
            System.Collections.IComparer comparer = StringComparer.Ordinal;
#endif
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
                    throw new NotSupportedException($"Relational operator {relationalOperator} is not supported.");
            }
        }
        
        /// <summary>
        /// Promote values to the type needed for the comparision, e.g. parse a string to int.
        /// </summary>
        /// <param name="leftValue"></param>
        /// <param name="rightValue"></param>
        private static void PromoteTypes(ref object leftValue, ref object rightValue)
        {
            if (ReferenceEquals(leftValue, rightValue) || leftValue == null || rightValue == null)
            {
                return;
            }

            var leftType = leftValue.GetType();
            var rightType = rightValue.GetType();
            if (leftType == rightType)
            {
                return;
            }

            //types are not equal
            var leftTypeOrder = GetOrder(leftType);
            var rightTypeOrder = GetOrder(rightType);

            if (leftTypeOrder < rightTypeOrder)
            {
                // first try promote right value with left type
                if (TryPromoteTypes(ref rightValue, leftType, ref leftValue, rightType)) return;
            }
            else
            {
                // otherwise try promote leftValue with right type
                if (TryPromoteTypes(ref leftValue, rightType, ref rightValue, leftType)) return;
            }

            throw new ConditionEvaluationException($"Cannot find common type for '{leftType.Name}' and '{rightType.Name}'.");
        }
        
        /// <summary>
        /// Promotes <paramref name="val"/> to type
        /// </summary>
        /// <param name="val"></param>
        /// <param name="type1"></param>
        /// <returns>success?</returns>
        private static bool TryPromoteType(ref object val, Type type1)
        {
            try
            {
                if (type1 == typeof(DateTime))
                {
                    val = Convert.ToDateTime(val, CultureInfo.InvariantCulture);
                    return true;
                }

                if (type1 == typeof(double))
                {
                    val = Convert.ToDouble(val, CultureInfo.InvariantCulture);
                    return true;
                }
                if (type1 == typeof(float))
                {
                    val = Convert.ToSingle(val, CultureInfo.InvariantCulture);
                    return true;
                }

                if (type1 == typeof(decimal))
                {
                    val = Convert.ToDecimal(val, CultureInfo.InvariantCulture);
                    return true;
                }

                if (type1 == typeof(long))
                {
                    val = Convert.ToInt64(val, CultureInfo.InvariantCulture);
                    return true;
                }

                if (type1 == typeof(int))
                {
                    val = Convert.ToInt32(val, CultureInfo.InvariantCulture);
                    return true;
                }

                if (type1 == typeof(bool))
                {
                    val = Convert.ToBoolean(val, CultureInfo.InvariantCulture);
                    return true;
                }

                if (type1 == typeof(LogLevel))
                {
                    string strval = Convert.ToString(val, CultureInfo.InvariantCulture);
                    val = LogLevel.FromString(strval);
                    return true;
                }

                if (type1 == typeof(string))
                {
                    val = Convert.ToString(val, CultureInfo.InvariantCulture);
                    InternalLogger.Debug("Using string comparision");
                    return true;
                }
            }
            catch (Exception)
            {
                InternalLogger.Debug("conversion of {0} to {1} failed", val, type1.Name);
            }
            return false;
        }

        /// <summary>
        /// Try to promote both values. First try to promote <paramref name="val1"/> to <paramref name="type1"/>,
        ///  when failed, try <paramref name="val2"/> to <paramref name="type2"/>.
        /// </summary>
        /// <returns></returns>
        private static bool TryPromoteTypes(ref object val1, Type type1, ref object val2, Type type2)
        {
            return TryPromoteType(ref val1, type1) || TryPromoteType(ref val2, type2);
        }

        /// <summary>
        /// Get the order for the type for comparision.
        /// </summary>
        /// <param name="type1"></param>
        /// <returns>index, 0 to max int. Lower is first</returns>
        private static int GetOrder(Type type1)
        {
            int order;
            var success = TypePromoteOrder.TryGetValue(type1, out order);
            if (success)
            {
                return order;
            }
            //not found, try as last
            return int.MaxValue;
        }

        /// <summary>
        /// Dictionary from type to index. Lower index should be tested first.
        /// </summary>
        private static Dictionary<Type, int> TypePromoteOrder = BuildTypeOrderDictionary();

        /// <summary>
        /// Build the dictionary needed for the order of the types.
        /// </summary>
        /// <returns></returns>
        private static Dictionary<Type, int> BuildTypeOrderDictionary()
        {
            var list = new List<Type>
            {
                typeof(DateTime),
                typeof(double),
                typeof(float),
                typeof(decimal),
                typeof(long),
                typeof(int),
                typeof(bool),
                typeof(LogLevel),
                typeof(string),
            };

            var dict = new Dictionary<Type, int>(list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                dict.Add(list[i], i);
            }
            return dict;

        }

        /// <summary>
        /// Get the string representing the current <see cref="ConditionRelationalOperator"/>
        /// </summary>
        /// <returns></returns>
        private string GetOperatorString()
        {
            switch (RelationalOperator)
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
                    throw new NotSupportedException($"Relational operator {RelationalOperator} is not supported.");
            }
        }
    }
}