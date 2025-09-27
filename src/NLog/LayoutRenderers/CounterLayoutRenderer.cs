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
    using System.Text;
    using System.Threading;
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
        private sealed class GlobalSequence
        {
            private long _value;
            public string Name { get; }

            public GlobalSequence(string sequenceName, long initialValue)
            {
                Name = sequenceName;
                _value = initialValue;
            }

            public long NextValue(int increment) => Interlocked.Add(ref _value, increment);
        };
#if NET35
        private static readonly System.Collections.Generic.Dictionary<string, GlobalSequence> Sequences = new System.Collections.Generic.Dictionary<string, GlobalSequence>(StringComparer.Ordinal);
#else
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, GlobalSequence> Sequences = new System.Collections.Concurrent.ConcurrentDictionary<string, GlobalSequence>(StringComparer.Ordinal);
#endif
        private static GlobalSequence? _firstSequence;

        /// <summary>
        /// Gets or sets the initial value of the counter.
        /// </summary>
        /// <remarks>Default: <see langword="0"/></remarks>
        /// <docgen category='Layout Options' order='10' />
        public long Value { get => _value; set => _value = value; }
        private long _value;

        /// <summary>
        /// Gets or sets the value for incrementing the counter for every layout rendering.
        /// </summary>
        /// <remarks>Default: <see langword="1"/></remarks>
        /// <docgen category='Layout Options' order='10' />
        public int Increment { get; set; } = 1;

        /// <summary>
        /// Gets or sets the name of the sequence. Different named sequences can have individual values.
        /// </summary>
        /// <remarks>Default: <see langword="null"/></remarks>
        /// <docgen category='Layout Options' order='10' />
        public Layout? Sequence { get; set; }

        /// <inheritdoc/>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            var v = GetNextValue(logEvent);
#if NETFRAMEWORK
            if (v < int.MaxValue && v > int.MinValue)
                builder.AppendInvariant((int)v);
            else
#endif
                builder.Append(v);
        }

        private long GetNextValue(LogEventInfo logEvent)
        {
            if (Sequence is null)
            {
                return Interlocked.Add(ref _value, Increment);
            }

            var sequenceName = Sequence.Render(logEvent);
            return GetNextGlobalValue(sequenceName);
        }

        private long GetNextGlobalValue(string sequenceName)
        {
            var globalSequence = _firstSequence;
            if (globalSequence is null)
            {
                globalSequence = new GlobalSequence(sequenceName, Value);
                Interlocked.CompareExchange(ref _firstSequence, globalSequence, null);
                globalSequence = _firstSequence;
            }
            if (globalSequence.Name.Equals(sequenceName, StringComparison.Ordinal))
                return globalSequence.NextValue(Increment);

#if NET35
            lock (Sequences)
            {
                if (!Sequences.TryGetValue(sequenceName, out globalSequence))
                {
                    globalSequence = new GlobalSequence(sequenceName, Value);
                    Sequences[sequenceName] = globalSequence;
                }
            }
#else
            if (!Sequences.TryGetValue(sequenceName, out globalSequence))
            {
                globalSequence = new GlobalSequence(sequenceName, Value);
                if (!Sequences.TryAdd(sequenceName, globalSequence))
                {
                    Sequences.TryGetValue(sequenceName, out globalSequence);
                }
            }
#endif
            return globalSequence.NextValue(Increment);
        }

        bool IRawValue.TryGetRawValue(LogEventInfo logEvent, out object value)
        {
            value = GetNextValue(logEvent);
            return true;
        }
    }
}
