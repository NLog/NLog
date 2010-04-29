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

namespace NLog.Conditions 
{
    /// <summary>
    /// Condition <b>or</b> expression.
    /// </summary>
    internal sealed class ConditionOrExpression : ConditionExpression 
    {
        public readonly ConditionExpression Left;
        public readonly ConditionExpression Right;

        private static object _boxedFalse = false;
        private static object _boxedTrue = true;

        /// <summary>
        /// Creates a new instance of <see cref="ConditionOrExpression"/> and assigns
        /// its Left and Right properties;
        /// </summary>
        /// <param name="left">Left hand side of the OR expression.</param>
        /// <param name="right">Right hand side of the OR expression.</param>
        public ConditionOrExpression(ConditionExpression left, ConditionExpression right) 
        {
            this.Left = left;
            this.Right = right;
        }

        /// <summary>
        /// Evaluates the expression by evaluating <see cref="Left"/> and <see cref="Right"/> recursively.
        /// </summary>
        /// <param name="context">Evaluation context.</param>
        /// <returns>The value of the alternative operator.</returns>
        public override object Evaluate(LogEventInfo context)
        {
            bool bval1 = (bool)Left.Evaluate(context);
            if (bval1)
                return _boxedTrue;

            bool bval2 = (bool)Right.Evaluate(context);
            if (bval2)
                return _boxedTrue;

            return _boxedFalse;
        }

        /// <summary>
        /// Returns a string representation of this expression.
        /// </summary>
        /// <returns>(Left) or (Right) string</returns>
        public override string ToString()
        {
            return "(" + Left + ") or (" + Right + ")";
        }

        /// <summary>
        /// Adds all layouts used by this expression to the specified collection.
        /// </summary>
        /// <param name="layouts">The collection to add layouts to.</param>
        public override void PopulateLayouts(LayoutCollection layouts)
        {
            Left.PopulateLayouts(layouts);
            Right.PopulateLayouts(layouts);
        }
    }
}
