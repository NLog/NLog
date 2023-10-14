// 
// Copyright (c) 2004-2021 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.Layouts
{
    using System.Collections.Generic;
    using System.Text;
    using NLog.Config;

    /// <summary>
    /// A specialized layout that renders LogEvent as JSON-Array
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/NLog/NLog/wiki/JsonArrayLayout">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/NLog/NLog/wiki/JsonArrayLayout">Documentation on NLog Wiki</seealso>
    [Layout("JsonArrayLayout")]
    [ThreadAgnostic]
    public class JsonArrayLayout : Layout
    {
        private Layout[] _precalculateLayouts;

        private IJsonConverter JsonConverter
        {
            get => _jsonConverter ?? (_jsonConverter = ResolveService<IJsonConverter>());
            set => _jsonConverter = value;
        }
        private IJsonConverter _jsonConverter;

        /// <summary>
        /// Gets the array of items to include in JSON-Array
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        [ArrayParameter(typeof(Layout), "item")]
        public IList<Layout> Items { get; } = new List<Layout>();

        /// <summary>
        /// Gets or sets the option to suppress the extra spaces in the output json
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public bool SuppressSpaces { get; set; }

        /// <summary>
        /// Gets or sets the option to render the empty object value {}
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public bool RenderEmptyObject { get; set; } = true;

        /// <inheritdoc/>
        protected override void InitializeLayout()
        {
            base.InitializeLayout();
            _precalculateLayouts = ResolveLayoutPrecalculation(Items);
        }

        /// <inheritdoc/>
        protected override void CloseLayout()
        {
            JsonConverter = null;
            _precalculateLayouts = null;
            base.CloseLayout();
        }

        internal override void PrecalculateBuilder(LogEventInfo logEvent, StringBuilder target)
        {
            PrecalculateBuilderInternal(logEvent, target, _precalculateLayouts);
        }

        /// <inheritdoc/>
        protected override void RenderFormattedMessage(LogEventInfo logEvent, StringBuilder target)
        {
            int orgLength = target.Length;
            RenderJsonFormattedMessage(logEvent, target);
            if (target.Length == orgLength && RenderEmptyObject)
            {
                target.Append(SuppressSpaces ? "[]" : "[ ]");
            }
        }

        /// <inheritdoc/>
        protected override string GetFormattedMessage(LogEventInfo logEvent)
        {
            return RenderAllocateBuilder(logEvent);
        }

        private void RenderJsonFormattedMessage(LogEventInfo logEvent, StringBuilder sb)
        {
            int orgLength = sb.Length;

            //Memory profiling pointed out that using a foreach-loop was allocating
            //an Enumerator. Switching to a for-loop avoids the memory allocation.
            for (int i = 0; i < Items.Count; i++)
            {
                var layout = Items[i];

                int beforeDelimeterLength = sb.Length;
                if (beforeDelimeterLength == orgLength)
                    sb.Append(SuppressSpaces ? "[" : "[ ");
                else
                    sb.Append(SuppressSpaces ? "," : ", ");

                if (!RenderLayoutJsonValue(logEvent, layout, sb))
                {
                    sb.Length = beforeDelimeterLength;
                }
            }

            if (sb.Length != orgLength)
            {
                sb.Append(SuppressSpaces ? "]" : " ]");
            }
        }

        private bool RenderLayoutJsonValue(LogEventInfo logEvent, Layout layout, StringBuilder sb)
        {
            int beforeValueLength = sb.Length;
            if (layout is JsonLayout)
            {
                layout.Render(logEvent, sb);
            }
            else if (layout.TryGetRawValue(logEvent, out object rawValue))
            {
                if (!JsonConverter.SerializeObject(rawValue, sb))
                {
                    return false;
                }
            }
            else
            {
                sb.Append('"');
                beforeValueLength = sb.Length;
                layout.Render(logEvent, sb);
                if (beforeValueLength != sb.Length)
                {
                    NLog.Targets.DefaultJsonSerializer.PerformJsonEscapeWhenNeeded(sb, beforeValueLength, true, false);
                    sb.Append('"');
                }
            }

            return beforeValueLength != sb.Length;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return ToStringWithNestedItems(Items, l => l.ToString());
        }
    }
}
