//
// Copyright (c) 2004-2024 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    /// Condition literal expression (numeric, <b>LogLevel.XXX</b>, <b>true</b> or <b>false</b>).
    /// </summary>
    internal sealed class ConditionLiteralExpression : ConditionExpression
    {
        public static readonly ConditionLiteralExpression Null = new ConditionLiteralExpression(null);
        public static readonly ConditionLiteralExpression True = new ConditionLiteralExpression(BoxedTrue);
        public static readonly ConditionLiteralExpression False = new ConditionLiteralExpression(BoxedFalse);

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionLiteralExpression" /> class.
        /// </summary>
        /// <param name="literalValue">Literal value.</param>
        public ConditionLiteralExpression(object? literalValue)
        {
            LiteralValue = literalValue;
        }

        /// <summary>
        /// Gets the literal value.
        /// </summary>
        /// <value>The literal value.</value>
        public object? LiteralValue { get; }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (LiteralValue is null)
            {
                return "null";
            }

            if (LiteralValue is string stringValue)
            {
                return $"'{stringValue}'";
            }

            if (LiteralValue is char charValue)
            {
                return $"'{charValue}'";
            }

            return Convert.ToString(LiteralValue, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Evaluates the expression.
        /// </summary>
        /// <param name="context">Evaluation context. Ignored.</param>
        /// <returns>The literal value as passed in the constructor.</returns>
        protected override object? EvaluateNode(LogEventInfo context)
        {
            return LiteralValue;
        }
    }
}
