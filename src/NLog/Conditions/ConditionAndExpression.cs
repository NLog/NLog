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
    /// <summary>
    /// Condition <b>and</b> expression.
    /// </summary>
    internal sealed class ConditionAndExpression : ConditionExpression
    {
        private static readonly object BoxedFalse = false;
        private static readonly object BoxedTrue = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionAndExpression" /> class.
        /// </summary>
        /// <param name="left">Left hand side of the AND expression.</param>
        /// <param name="right">Right hand side of the AND expression.</param>
        public ConditionAndExpression(ConditionExpression left, ConditionExpression right)
        {
            Left = left;
            Right = right;
        }

        /// <summary>
        /// Gets the left hand side of the AND expression.
        /// </summary>
        public ConditionExpression Left { get; private set; }

        /// <summary>
        /// Gets the right hand side of the AND expression.
        /// </summary>
        public ConditionExpression Right { get; private set; }

        /// <summary>
        /// Returns a string representation of this expression.
        /// </summary>
        /// <returns>A concatenated '(Left) and (Right)' string.</returns>
        public override string ToString()
        {
            return $"({Left} and {Right})";
        }

        /// <summary>
        /// Evaluates the expression by evaluating <see cref="Left"/> and <see cref="Right"/> recursively.
        /// </summary>
        /// <param name="context">Evaluation context.</param>
        /// <returns>The value of the conjunction operator.</returns>
        protected override object EvaluateNode(LogEventInfo context)
        {
            var bval1 = (bool)Left.Evaluate(context);
            if (!bval1)
            {
                return BoxedFalse;
            }

            var bval2 = (bool)Right.Evaluate(context);
            if (!bval2)
            {
                return BoxedFalse;
            }

            return BoxedTrue;
        }
    }
}