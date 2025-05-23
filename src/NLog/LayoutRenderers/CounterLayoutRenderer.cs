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

namespace NLog.LayoutRenderers
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using NLog.Config;
    using NLog.Internal;
    using NLog.Layouts;

    /// <summary>
    /// A counter value (increases on each layout rendering).
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/NLog/NLog/wiki/Counter-Layout-Renderer">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/NLog/NLog/wiki/Counter-Layout-Renderer">Documentation on NLog Wiki</seealso>
    [LayoutRenderer("counter")]
    [ThreadAgnostic]
    public class CounterLayoutRenderer : LayoutRenderer, IRawValue
    {
        private static readonly Dictionary<string, long> Sequences = new Dictionary<string, long>(StringComparer.Ordinal);

        /// <summary>
        /// Gets or sets the initial value of the counter.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public long Value { get; set; } = 1;

        /// <summary>
        /// Gets or sets the value to be added to the counter after each layout rendering.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public int Increment { get; set; } = 1;

        /// <summary>
        /// Gets or sets the name of the sequence. Different named sequences can have individual values.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public Layout? Sequence { get; set; }

        /// <inheritdoc/>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            var v = GetNextValue(logEvent);
            if (v < int.MaxValue && v > int.MinValue)
                builder.AppendInvariant((int)v);
            else
                builder.Append(v);
        }

        private long GetNextValue(LogEventInfo logEvent)
        {
            if (Sequence is null)
            {
                long currentValue = Value;
                Value += Increment;
                return currentValue;
            }

            var sequenceName = Sequence.Render(logEvent);
            lock (Sequences)
            {
                if (!Sequences.TryGetValue(sequenceName, out var nextValue))
                {
                    nextValue = Value;
                }

                var currentValue = nextValue;
                nextValue += Increment;
                Sequences[sequenceName] = nextValue;
                return currentValue;
            }
        }

        bool IRawValue.TryGetRawValue(LogEventInfo logEvent, out object value)
        {
            value = GetNextValue(logEvent);
            return true;
        }
    }
}
