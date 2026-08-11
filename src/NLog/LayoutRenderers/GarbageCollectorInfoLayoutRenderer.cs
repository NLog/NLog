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
    using System.Globalization;
    using System.Text;
    using NLog.Config;
    using NLog.Internal;

    /// <summary>
    /// The information about the garbage collector.
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/NLog/NLog/wiki/Gc-Layout-Renderer">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/NLog/NLog/wiki/Gc-Layout-Renderer">Documentation on NLog Wiki</seealso>
    [LayoutRenderer("gc")]
    [ThreadAgnostic]
    public class GarbageCollectorInfoLayoutRenderer : LayoutRenderer, IRawValue
    {
        /// <summary>
        /// Gets or sets the property to retrieve.
        /// </summary>
        /// <remarks>Default: <see cref="GarbageCollectorProperty.TotalMemory"/></remarks>
        /// <docgen category='Layout Options' order='10' />
        [DefaultParameter]
        public GarbageCollectorProperty Property { get; set; } = GarbageCollectorProperty.TotalMemory;

        /// <summary>
        /// Format string for conversion from object to string.
        /// </summary>
        /// <remarks>Default: <see langword="null"/></remarks>
        /// <docgen category='Layout Options' order='50' />
        public string? Format
        {
            get => _format;
            set
            {
                _format = value;
                _stringFormat = string.IsNullOrEmpty(_format) ? null : $"{{0:{_format}}}";
            }
        }
        private string? _format;
        private string? _stringFormat;

        /// <summary>
        /// Gets or sets the culture used for rendering.
        /// </summary>
        /// <remarks>Default: <see cref="CultureInfo.InvariantCulture"/></remarks>
        /// <docgen category='Layout Options' order='100' />
        public CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;

        /// <inheritdoc/>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            var value = GetValue();
            if (_stringFormat is null)
            {
#if NETFRAMEWORK
                if (value >= 0 && value < uint.MaxValue)
                    builder.AppendInvariant((uint)value);
                else
#endif
                    builder.Append(value);
            }
            else
            {
                var culture = GetCulture(logEvent, Culture);
                builder.AppendFormat(culture, _stringFormat, (object)value);
            }
        }

        bool IRawValue.TryGetRawValue(LogEventInfo logEvent, out object? value)
        {
            value = GetValue();
            return true;
        }

        private long GetValue()
        {
            long value = 0;

            switch (Property)
            {
                case GarbageCollectorProperty.TotalMemory:
#pragma warning disable CS0618 // Type or member is obsolete
                case GarbageCollectorProperty.TotalMemoryForceCollection:
#pragma warning restore CS0618 // Type or member is obsolete
                    value = GC.GetTotalMemory(false);
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

                case GarbageCollectorProperty.WorkingSet:
                    value = Environment.WorkingSet;
                    break;
            }

            return value;
        }
    }
}
