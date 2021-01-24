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
    using Layouts;
    using NLog.Internal;
    using System.Text;

    /// <summary>
    /// Condition layout expression (represented by a string literal
    /// with embedded ${}).
    /// </summary>
    internal sealed class ConditionLayoutExpression : ConditionExpression
    {
        private readonly SimpleLayout _simpleLayout;
        private StringBuilder _fastObjectPool;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionLayoutExpression" /> class.
        /// </summary>
        /// <param name="layout">The layout.</param>
        public ConditionLayoutExpression(SimpleLayout layout)
        {
            _simpleLayout = layout;
        }

        /// <summary>
        /// Gets the layout.
        /// </summary>
        /// <value>The layout.</value>
        public Layout Layout => _simpleLayout;

        /// <summary>
        /// Returns a string representation of this expression.
        /// </summary>
        /// <returns>String literal in single quotes.</returns>
        public override string ToString()
        {
            return $"'{_simpleLayout.ToString()}'";
        }

        /// <summary>
        /// Evaluates the expression by calculating the value
        /// of the layout in the specified evaluation context.
        /// </summary>
        /// <param name="context">Evaluation context.</param>
        /// <returns>The value of the layout.</returns>
        protected override object EvaluateNode(LogEventInfo context)
        {
            if (_simpleLayout.IsSimpleStringText || !_simpleLayout.ThreadAgnostic)
                return _simpleLayout.Render(context);

            var stringBuilder = System.Threading.Interlocked.Exchange(ref _fastObjectPool, null) ?? new StringBuilder();
            try
            {
                _simpleLayout.RenderAppendBuilder(context, stringBuilder);
                return stringBuilder.ToString();
            }
            finally
            {
                stringBuilder.ClearBuilder();
                System.Threading.Interlocked.Exchange(ref _fastObjectPool, stringBuilder);
            }
        }
    }
}