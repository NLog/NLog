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

namespace NLog.LayoutRenderers.Wrappers
{
    using System;
    using System.ComponentModel;
    using NLog.Config;
    using NLog.Internal;
    using NLog.Layouts;

    /// <summary>
    /// Applies caching to another layout output.
    /// </summary>
    /// <remarks>
    /// The value of the inner layout will be rendered only once and reused subsequently.
    /// </remarks>
    [LayoutRenderer("cached")]
    [AmbientProperty("Cached")]
    [AmbientProperty("ClearCache")]
    [AmbientProperty("CachedSeconds")]
    [ThreadAgnostic]
    [ThreadSafe]
    public sealed class CachedLayoutRendererWrapper : WrapperLayoutRendererBase, IStringValueRenderer
    {
        /// <summary>
        /// A value indicating when the cache is cleared.
        /// </summary>
        [Flags]
        public enum ClearCacheOption 
        { 
            /// <summary>Never clear the cache.</summary>
            None = 0,
            /// <summary>Clear the cache whenever the <see cref="CachedLayoutRendererWrapper"/> is initialized.</summary>
            OnInit = 1,
            /// <summary>Clear the cache whenever the <see cref="CachedLayoutRendererWrapper"/> is closed.</summary>
            OnClose = 2
        }

        private readonly object _lockObject = new object();
        private string _cachedValue;
        private string _renderedCacheKey;
        private DateTime _cachedValueExpires;
        private TimeSpan? _cachedValueTimeout;

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedLayoutRendererWrapper"/> class.
        /// </summary>
        public CachedLayoutRendererWrapper()
        {
            Cached = true;
            ClearCache = ClearCacheOption.OnInit | ClearCacheOption.OnClose;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CachedLayoutRendererWrapper"/> is enabled.
        /// </summary>
        /// <docgen category='Caching Options' order='10' />
        [DefaultValue(true)]
        public bool Cached { get; set; }

        /// <summary>
        /// Gets or sets a value indicating when the cache is cleared.
        /// </summary>
        /// <docgen category='Caching Options' order='10' />
        public ClearCacheOption ClearCache { get; set; }

        /// <summary>
        /// Cachekey. If the cachekey changes, resets the value. For example, the cachekey would be the current day.s
        /// </summary>
        /// <docgen category='Caching Options' order='10' />
        public Layout CacheKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating how many seconds the value should stay cached until it expires
        /// </summary>
        /// <docgen category='Caching Options' order='10' />
        public int CachedSeconds
        {
            get => (int)(_cachedValueTimeout?.TotalSeconds ?? 0.0);
            set
            {
                _cachedValueTimeout = TimeSpan.FromSeconds(value);
                if (_cachedValueTimeout > TimeSpan.Zero)
                    Cached = true;
            }
        }

        /// <summary>
        /// Initializes the layout renderer.
        /// </summary>
        protected override void InitializeLayoutRenderer()
        {
            base.InitializeLayoutRenderer();
            if ((ClearCache & ClearCacheOption.OnInit) == ClearCacheOption.OnInit)
                _cachedValue = null;
        }

        /// <summary>
        /// Closes the layout renderer.
        /// </summary>
        protected override void CloseLayoutRenderer()
        {
            base.CloseLayoutRenderer();
            if ((ClearCache & ClearCacheOption.OnClose) == ClearCacheOption.OnClose)
                _cachedValue = null;
        }

        /// <summary>
        /// Transforms the output of another layout.
        /// </summary>
        /// <param name="text">Output to be transform.</param>
        /// <returns>Transformed text.</returns>
        protected override string Transform(string text)
        {
            return text;
        }

        /// <summary>
        /// Renders the inner layout contents.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        /// <returns>Contents of inner layout.</returns>
        protected override string RenderInner(LogEventInfo logEvent)
        {
            if (Cached)
            {
                var newCacheKey = CacheKey?.Render(logEvent) ?? string.Empty;
                var cachedValue = LookupValidCachedValue(logEvent, newCacheKey);

                if (cachedValue == null)
                {
                    lock (_lockObject)
                    {
                        cachedValue = LookupValidCachedValue(logEvent, newCacheKey);
                        if (cachedValue == null)
                        {
                            _cachedValue = cachedValue = base.RenderInner(logEvent);
                            _renderedCacheKey = newCacheKey;
                            if (_cachedValueTimeout.HasValue)
                                _cachedValueExpires = logEvent.TimeStamp + _cachedValueTimeout.Value;
                        }
                    }
                }

                return cachedValue;
            }
            else
            {
                return base.RenderInner(logEvent);
            }
        }

        string LookupValidCachedValue(LogEventInfo logEvent, string newCacheKey)
        {
            if (_renderedCacheKey != newCacheKey)
                return null;

            if (_cachedValueTimeout.HasValue && logEvent.TimeStamp > _cachedValueExpires)
                return null;

            return _cachedValue;
        }

        /// <inheritdoc/>
        string IStringValueRenderer.GetFormattedString(LogEventInfo logEvent) => Cached ? RenderInner(logEvent) : null;
    }
}
