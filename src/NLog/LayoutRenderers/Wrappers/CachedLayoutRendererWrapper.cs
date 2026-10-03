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

namespace NLog.LayoutRenderers.Wrappers
{
    using System;
    using NLog.Config;
    using NLog.Internal;
    using NLog.Layouts;

    /// <summary>
    /// Applies caching to another layout output.
    /// </summary>
    /// <remarks>
    /// The value of the inner layout will be rendered only once and reused subsequently.
    ///
    /// <a href="https://github.com/NLog/NLog/wiki/Cached-Layout-Renderer">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/NLog/NLog/wiki/Cached-Layout-Renderer">Documentation on NLog Wiki</seealso>
    [LayoutRenderer("cached")]
    [AmbientProperty(nameof(Cached))]
    [AmbientProperty(nameof(ClearCache))]
    [AmbientProperty(nameof(CachedSeconds))]
    [AmbientProperty(nameof(CachedUntil))]
    [AppDomainFixedOutput]
    [ThreadAgnostic]
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

        /// <summary>
        /// A value indicating when the cached value expires, because the time of the <see cref="LogEventInfo"/> has moved into a new period.
        /// </summary>
        public enum CachedUntilOption
        {
            /// <summary>Never expire the cached value because of the time.</summary>
            None = 0,
            /// <summary>Expire the cached value when the day changes.</summary>
            DayChange,
            /// <summary>Expire the cached value when the hour changes.</summary>
            HourChange
        }

        private readonly object _lockObject = new object();
        private string? _cachedValue;
        private string? _renderedCacheKey;
        private DateTime _cachedValueExpires;
        private TimeSpan? _cachedValueTimeout;
        private DateTime _cachedValuePeriodStart;
        private DateTime _cachedValuePeriodEnd;
        private CachedUntilOption _cachedUntil;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CachedLayoutRendererWrapper"/> is enabled.
        /// </summary>
        /// <remarks>Default: <see langword="true"/></remarks>
        /// <docgen category='Layout Options' order='10' />
        public bool Cached { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating when the cache is cleared.
        /// </summary>
        /// <remarks>Default: <see cref="ClearCacheOption.OnInit"/> | <see cref="ClearCacheOption.OnClose"/></remarks>
        /// <docgen category='Layout Options' order='10' />
        public ClearCacheOption ClearCache { get; set; } = ClearCacheOption.OnInit | ClearCacheOption.OnClose;

        /// <summary>
        /// Gets or sets whether to reset cached value when CacheKey output changes. Example CacheKey could render current day, so the cached-value is reset on day roll.
        /// </summary>
        /// <remarks>Default: <see langword="null"/></remarks>
        /// <docgen category='Layout Options' order='10' />
        public Layout? CacheKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating how many seconds the value should stay cached until it expires
        /// </summary>
        /// <remarks>Default: <see langword="0"/></remarks>
        /// <docgen category='Layout Options' order='10' />
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
        /// Gets or sets when the cached value should expire, so it is reset exactly when the hour or day of the LogEvent TimeStamp changes. Example HourChange will reset the cached value at the start of every hour.
        /// </summary>
        /// <remarks>Default: <see cref="CachedUntilOption.None"/></remarks>
        /// <docgen category='Layout Options' order='10' />
        public CachedUntilOption CachedUntil
        {
            get => _cachedUntil;
            set
            {
                _cachedUntil = value;
                if (_cachedUntil != CachedUntilOption.None)
                    Cached = true;
            }
        }

        /// <inheritdoc/>
        protected override void InitializeLayoutRenderer()
        {
            base.InitializeLayoutRenderer();
            if ((ClearCache & ClearCacheOption.OnInit) == ClearCacheOption.OnInit)
                _cachedValue = null;
        }

        /// <inheritdoc/>
        protected override void CloseLayoutRenderer()
        {
            base.CloseLayoutRenderer();
            if ((ClearCache & ClearCacheOption.OnClose) == ClearCacheOption.OnClose)
                _cachedValue = null;
        }

        /// <inheritdoc/>
        protected override string Transform(string text)
        {
            return text;
        }

        /// <inheritdoc/>
        protected override string RenderInner(LogEventInfo logEvent)
        {
            if (Cached)
            {
                var newCacheKey = CacheKey?.Render(logEvent) ?? string.Empty;
                var cachedValue = LookupValidCachedValue(logEvent, newCacheKey);

                if (cachedValue is null)
                {
                    lock (_lockObject)
                    {
                        cachedValue = LookupValidCachedValue(logEvent, newCacheKey);
                        if (cachedValue is null)
                        {
                            _cachedValue = cachedValue = base.RenderInner(logEvent);
                            _renderedCacheKey = newCacheKey;
                            if (_cachedValueTimeout.HasValue)
                                _cachedValueExpires = logEvent.TimeStamp + _cachedValueTimeout.Value;
                            if (_cachedUntil != CachedUntilOption.None)
                                UpdateCachedValuePeriod(logEvent.TimeStamp);
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

        string? LookupValidCachedValue(LogEventInfo logEvent, string newCacheKey)
        {
            if (_renderedCacheKey != newCacheKey)
                return null;

            if (_cachedValueTimeout.HasValue && logEvent.TimeStamp > _cachedValueExpires)
                return null;

            if (_cachedUntil != CachedUntilOption.None && (logEvent.TimeStamp < _cachedValuePeriodStart || logEvent.TimeStamp > _cachedValuePeriodEnd))
                return null;

            return _cachedValue;
        }

        private void UpdateCachedValuePeriod(DateTime timeStamp)
        {
            var periodTicks = _cachedUntil == CachedUntilOption.DayChange ? TimeSpan.TicksPerDay : TimeSpan.TicksPerHour;
            _cachedValuePeriodStart = new DateTime(timeStamp.Ticks - timeStamp.Ticks % periodTicks, timeStamp.Kind);
            // Inclusive end, so the last period ends at DateTime.MaxValue without overflow
            _cachedValuePeriodEnd = _cachedValuePeriodStart.AddTicks(periodTicks - 1);
        }

        string? IStringValueRenderer.GetFormattedString(LogEventInfo logEvent) => Cached ? RenderInner(logEvent) : null;
    }
}
