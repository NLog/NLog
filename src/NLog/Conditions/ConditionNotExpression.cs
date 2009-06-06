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

using System.Collections.Generic;
using NLog.Layouts;

namespace NLog.Conditions
{
    /// <summary>
    /// Condition <b>not</b> expression.
    /// </summary>
    internal sealed class ConditionNotExpression : ConditionExpression 
    {
        private ConditionExpression expression;

        /// <summary>
        /// Initializes a new instance of the ConditionNotExpression class.
        /// </summary>
        /// <param name="expression">The expression.</param>
        public ConditionNotExpression(ConditionExpression expression) 
        {
            this.expression = expression;
        }

        /// <summary>
        /// Evaluates the expression.
        /// </summary>
        /// <param name="context">Evaluation context.</param>
        /// <returns>Expression result.</returns>
        public override object Evaluate(LogEventInfo context)
        {
            return !(bool)this.expression.Evaluate(context);
        }

        /// <summary>
        /// Returns a string representation of the expression.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the condition expression.
        /// </returns>
        public override string ToString()
        {
            return "not(" + this.expression + ")";
        }

        /// <summary>
        /// Adds all layouts used by this expression to the specified collection.
        /// </summary>
        /// <param name="layouts">The collection to add layouts to.</param>
        public override void PopulateLayouts(ICollection<Layout> layouts)
        {
            this.expression.PopulateLayouts(layouts);
        }
    }
}
