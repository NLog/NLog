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

using System;
using JetBrains.Annotations;
using NLog.Config;
using NLog.Layouts;

namespace NLog.Internal
{
    /// <summary>
    /// Transformation on a layout. Pre-calculated if the layout has a fixed value.
    /// </summary>
    internal class TransformedLayout : IRenderable, ISupportsInitialize
    {
        private readonly string _fixedValue;
        [CanBeNull] private readonly Func<Layout, LogEventInfo, string> _renderLogEvent;
        private readonly Func<string, string> _transformation;

        /// <summary>
        /// Create TransformedLayout.
        /// </summary>
        /// <param name="layout">Layout</param>
        /// <param name="transformation">Transformation method</param>
        /// <param name="renderLogEvent">Optional renderer, otherwise <see cref="Layouts.Layout.Render" /> is used</param>
        private TransformedLayout([NotNull] Layout layout, [NotNull] Func<string, string> transformation, [CanBeNull] Func<Layout, LogEventInfo, string> renderLogEvent)
        {
            _transformation = transformation ?? throw new ArgumentNullException(nameof(transformation));
            _renderLogEvent = renderLogEvent;
            Layout = layout ?? throw new ArgumentNullException(nameof(layout));

            if (layout is SimpleLayout s && s.IsFixedText)
            {
                _fixedValue = transformation(s.FixedText);
            }
        }

        public Layout Layout { get; }

        #region Implementation of IRenderable

        /// <inheritdoc />
        public string Render(LogEventInfo logEvent)
        {
            if (_fixedValue != null)
            {
                return _fixedValue;
            }

            var value = _renderLogEvent != null ? _renderLogEvent(Layout, logEvent) : Layout.Render(logEvent);

            return _transformation(value);
        }

        #endregion

        /// <summary>
        /// Create TransformedLayout. If <paramref name="layout" /> is null, then the returns null
        /// </summary>
        /// <param name="layout">Layout</param>
        /// <param name="transformation">Transformation method</param>
        /// <param name="renderLogEvent">Optional renderer, otherwise <see cref="Layouts.Layout.Render" /> is used</param>
        /// <returns>null if <paramref name="layout" /> is null</returns>
        public static TransformedLayout Create([CanBeNull] Layout layout,
            [NotNull] Func<string, string> transformation, [CanBeNull] Func<Layout, LogEventInfo, string> renderLogEvent)
        {
            if (transformation == null)
            {
                throw new ArgumentNullException(nameof(transformation));
            }

            if (layout == null)
            {
                return null;
            }

            return new TransformedLayout(layout, transformation, renderLogEvent);
        }

        #region Implementation of ISupportsInitialize

        /// <inheritdoc />
        public void Initialize(LoggingConfiguration configuration)
        {
            Layout.Initialize(configuration);
        }

        /// <inheritdoc />
        public void Close()
        {
            Layout.Close();
        }

        #endregion
    }
}