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

#if !MONO

namespace NLog.LayoutRenderers
{
    using System;
    using System.ComponentModel;
    using System.Text;
    using NLog.Config;
    using NLog.Internal;

    /// <summary>
    /// The information about the garbage collector.
    /// </summary>
    [LayoutRenderer("gc")]
    [ThreadSafe]
    public class GarbageCollectorInfoLayoutRenderer : LayoutRenderer
    {
        /// <summary>
        /// Gets or sets the property to retrieve.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultValue("TotalMemory")]
        public GarbageCollectorProperty Property { get; set; } = GarbageCollectorProperty.TotalMemory;

        /// <inheritdoc />
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            var value = GetValue();
            if (value >= 0 && value < uint.MaxValue)
                builder.AppendInvariant((uint)value);
            else
                builder.Append(value.ToString());
        }

        private long GetValue()
        {
            long value = 0;

            switch (Property)
            {
                case GarbageCollectorProperty.TotalMemory:
                    value = GC.GetTotalMemory(false);
                    break;

                case GarbageCollectorProperty.TotalMemoryForceCollection:
                    value = GC.GetTotalMemory(true);
                    break;

                case GarbageCollectorProperty.CollectionCount0:
                    value = GC.CollectionCount(0);
                    break;

                case GarbageCollectorProperty.CollectionCount1:
                    value = GC.CollectionCount(1);
                    break;

                case GarbageCollectorProperty.CollectionCount2:
                    value = GC.CollectionCount(2);
                    break;

                case GarbageCollectorProperty.MaxGeneration:
                    value = GC.MaxGeneration;
                    break;
            }

            return value;
        }
    }
}

#endif
