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

namespace NLog.Filters
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Text;
    using NLog.Internal;

    /// <summary>
    /// Matches when the result of the calculated layout has been repeated a moment ago
    /// </summary>
    [Filter("whenRepeated")]
    public class WhenRepeatedFilter : LayoutBasedFilter
    {
        private const int MaxInitialRenderBufferLength = 16384;

        /// <summary>
        /// How long before a filter expires, and logging is accepted again
        /// </summary>
        /// <docgen category='Filtering Options' order='10' />
        [DefaultValue(10)]
        public int TimeoutSeconds { get; set; }

        /// <summary>
        /// Max length of filter values, will truncate if above limit
        /// </summary>
        /// <docgen category='Filtering Options' order='10' />
        [DefaultValue(1000)]
        public int MaxLength { get; set; }

        /// <summary>
        /// Applies the configured action to the initial logevent that starts the timeout period.
        /// Used to configure that it should ignore all events until timeout.
        /// </summary>
        /// <docgen category='Filtering Options' order='10' />
        [DefaultValue(false)]
        public bool IncludeFirst { get; set; }

        /// <summary>
        /// Max number of unique filter values to expect simultaneously
        /// </summary>
        /// <docgen category='Filtering Options' order='10' />
        [DefaultValue(50000)]
        public int MaxFilterCacheSize { get; set; }

        /// <summary>
        /// Default number of unique filter values to expect, will automatically increase if needed
        /// </summary>
        /// <docgen category='Filtering Options' order='10' />
        [DefaultValue(1000)]
        public int DefaultFilterCacheSize { get; set; }

        /// <summary>
        /// Insert FilterCount value into <see cref="LogEventInfo.Properties"/> when an event is no longer filtered
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultValue(null)]
        public string FilterCountPropertyName { get; set; }

        /// <summary>
        /// Append FilterCount to the <see cref="LogEventInfo.Message"/> when an event is no longer filtered
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultValue(null)]
        public string FilterCountMessageAppendFormat { get; set; }

        /// <summary>
        /// Reuse internal buffers, and doesn't have to constantly allocate new buffers
        /// </summary>
        /// <docgen category='Performance Options' order='10' />
        [Obsolete("No longer used, and always returns true. Marked obsolete on NLog 5.0")]
        [DefaultValue(true)]
        public bool OptimizeBufferReuse { get => true; set { } }

        /// <summary>
        /// Default buffer size for the internal buffers
        /// </summary>
        /// <docgen category='Performance Options' order='10' />
        [DefaultValue(1000)]
        public int OptimizeBufferDefaultLength { get; set; }

        internal readonly ReusableBuilderCreator ReusableLayoutBuilder = new ReusableBuilderCreator();

        private readonly Dictionary<FilterInfoKey, FilterInfo> _repeatFilter = new Dictionary<FilterInfoKey, FilterInfo>(1000);
        private readonly Stack<KeyValuePair<FilterInfoKey, FilterInfo>> _objectPool = new Stack<KeyValuePair<FilterInfoKey, FilterInfo>>(1000);

        /// <summary>
        /// Initializes a new instance of the <see cref="WhenRepeatedFilter" /> class.
        /// </summary>
        public WhenRepeatedFilter()
        {
            TimeoutSeconds = 10;
            MaxLength = 1000;
            DefaultFilterCacheSize = 1000;
            MaxFilterCacheSize = 50000;
            OptimizeBufferDefaultLength = MaxLength;
        }

        /// <summary>
        /// Checks whether log event should be logged or not. In case the LogEvent has just been repeated.
        /// </summary>
        /// <param name="logEvent">Log event.</param>
        /// <returns>
        /// <see cref="FilterResult.Ignore"/> - if the log event should be ignored<br/>
        /// <see cref="FilterResult.Neutral"/> - if the filter doesn't want to decide<br/>
        /// <see cref="FilterResult.Log"/> - if the log event should be logged<br/>
        /// .</returns>
        protected override FilterResult Check(LogEventInfo logEvent)
        {
            FilterResult filterResult = FilterResult.Neutral;
            bool obsoleteFilter = false;

            lock (_repeatFilter)
            {
                using (var targetBuilder = ReusableLayoutBuilder.Allocate())
                {
                    if (targetBuilder.Result.Capacity != OptimizeBufferDefaultLength)
                    {
                        // StringBuilder.Equals only works when StringBuilder.Capacity is the same
                        if (OptimizeBufferDefaultLength < MaxInitialRenderBufferLength)
                        {
                            OptimizeBufferDefaultLength = MaxLength;
                            while (OptimizeBufferDefaultLength < targetBuilder.Result.Capacity && OptimizeBufferDefaultLength < MaxInitialRenderBufferLength)
                            {
                                OptimizeBufferDefaultLength *= 2;
                            }
                        }
                        targetBuilder.Result.Capacity = OptimizeBufferDefaultLength;
                    }

                    FilterInfoKey filterInfoKey = RenderFilterInfoKey(logEvent, targetBuilder.Result);

                    FilterInfo filterInfo;
                    if (!_repeatFilter.TryGetValue(filterInfoKey, out filterInfo))
                    {
                        filterInfo = CreateFilterInfo(logEvent);
                        if (filterInfo.StringBuffer != null)
                        {
                            filterInfo.StringBuffer.ClearBuilder();
                            int length = Math.Min(targetBuilder.Result.Length, MaxLength);
                            for (int i = 0; i < length; ++i)
                                filterInfo.StringBuffer.Append(targetBuilder.Result[i]);
                        }
                        filterInfo.Refresh(logEvent.Level, logEvent.TimeStamp, 0);
                        _repeatFilter.Add(new FilterInfoKey(filterInfo.StringBuffer, filterInfoKey.StringValue, filterInfoKey.StringHashCode), filterInfo);
                        obsoleteFilter = true;
                    }
                    else
                    {
                        if (IncludeFirst)
                        {
                            obsoleteFilter = filterInfo.IsObsolete(logEvent.TimeStamp, TimeoutSeconds);
                        }

                        filterResult = RefreshFilterInfo(logEvent, filterInfo);
                    }
                }
            }

            // Ignore the first log-event, and wait until next timeout expiry
            if (IncludeFirst && obsoleteFilter)
            {
                filterResult = Action;
            }

            return filterResult;
        }

        /// <summary>
        /// Uses object pooling, and prunes stale filter items when the pool runs dry
        /// </summary>
        private FilterInfo CreateFilterInfo(LogEventInfo logEvent)
        {
            FilterInfo reusableObject;
            if (_objectPool.Count == 0 && _repeatFilter.Count > DefaultFilterCacheSize)
            {
                int aggressiveTimeoutSeconds = _repeatFilter.Count > MaxFilterCacheSize ? TimeoutSeconds * 2 / 3 : TimeoutSeconds;
                PruneFilterCache(logEvent, Math.Max(1, aggressiveTimeoutSeconds));
                if (_repeatFilter.Count > MaxFilterCacheSize)
                {
                    PruneFilterCache(logEvent, Math.Max(1, TimeoutSeconds / 2));
                }
            }

            if (_objectPool.Count == 0)
            {
                reusableObject = new FilterInfo(new StringBuilder(OptimizeBufferDefaultLength));
            }
            else
            {
                reusableObject = _objectPool.Pop().Value;
                // StringBuilder.Equals only works when StringBuilder.Capacity is the same
                if (reusableObject.StringBuffer != null && reusableObject.StringBuffer.Capacity != OptimizeBufferDefaultLength)
                {
                    reusableObject.StringBuffer.Capacity = OptimizeBufferDefaultLength;
                }
            }

            return reusableObject;
        }

        /// <summary>
        /// Remove stale filter-value from the cache, and fill them into the pool for reuse
        /// </summary>
        private void PruneFilterCache(LogEventInfo logEvent, int aggressiveTimeoutSeconds)
        {
            foreach (var filterPair in _repeatFilter)
            {
                if (filterPair.Value.IsObsolete(logEvent.TimeStamp, aggressiveTimeoutSeconds))
                {
                    _objectPool.Push(filterPair);
                }
            }
            foreach (var filterPair in _objectPool)
            {
                _repeatFilter.Remove(filterPair.Key);
            }
            if (_repeatFilter.Count * 2 > DefaultFilterCacheSize && DefaultFilterCacheSize < MaxFilterCacheSize)
            {
                DefaultFilterCacheSize *= 2;
            }
            while (_objectPool.Count != 0 && _objectPool.Count > DefaultFilterCacheSize)
            {
                _objectPool.Pop();
            }
        }

        /// <summary>
        /// Renders the Log Event into a filter value, that is used for checking if just repeated
        /// </summary>
        private FilterInfoKey RenderFilterInfoKey(LogEventInfo logEvent, StringBuilder targetBuilder)
        {
            if (targetBuilder != null)
            {
                Layout.RenderAppendBuilder(logEvent, targetBuilder);
                if (targetBuilder.Length > MaxLength)
                    targetBuilder.Length = MaxLength;
                return new FilterInfoKey(targetBuilder, null);
            }
            string value = Layout.Render(logEvent) ?? string.Empty;
            if (value.Length > MaxLength)
                value = value.Substring(0, MaxLength);
            return new FilterInfoKey(null, value);
        }

        /// <summary>
        /// Repeated LogEvent detected. Checks if it should activate filter-action
        /// </summary>
        private FilterResult RefreshFilterInfo(LogEventInfo logEvent, FilterInfo filterInfo)
        {
            if (filterInfo.HasExpired(logEvent.TimeStamp, TimeoutSeconds) || logEvent.Level.Ordinal > filterInfo.LogLevel.Ordinal)
            {
                int filterCount = filterInfo.FilterCount;
                if (filterCount > 0 && filterInfo.IsObsolete(logEvent.TimeStamp, TimeoutSeconds))
                {
                    filterCount = 0;
                }

                filterInfo.Refresh(logEvent.Level, logEvent.TimeStamp, 0);

                if (filterCount > 0)
                {
                    if (!string.IsNullOrEmpty(FilterCountPropertyName))
                    {
                        if (!logEvent.Properties.TryGetValue(FilterCountPropertyName, out var otherFilterCount))
                        {
                            logEvent.Properties[FilterCountPropertyName] = filterCount;
                        }
                        else if (otherFilterCount is int i)
                        {
                            filterCount = Math.Max(i, filterCount);
                            logEvent.Properties[FilterCountPropertyName] = filterCount;
                        }
                    }
                    if (!string.IsNullOrEmpty(FilterCountMessageAppendFormat) && logEvent.Message != null)
                    {
                        logEvent.Message += string.Format(FilterCountMessageAppendFormat, filterCount.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    }
                }

                return FilterResult.Neutral;
            }
            filterInfo.Refresh(logEvent.Level, logEvent.TimeStamp, filterInfo.FilterCount + 1);
            return Action;
        }

        /// <summary>
        /// Filter Value State (mutable)
        /// </summary>
        private class FilterInfo
        {
            public FilterInfo(StringBuilder stringBuilder)
            {
                StringBuffer = stringBuilder;
            }

            public void Refresh(LogLevel logLevel, DateTime logTimeStamp, int filterCount)
            {
                if (filterCount == 0)
                {
                    LastLogTime = logTimeStamp;
                    LogLevel = logLevel;
                }
                else if (LogLevel == null || logLevel.Ordinal > LogLevel.Ordinal)
                {
                    LogLevel = logLevel;
                }
                LastFilterTime = logTimeStamp;
                FilterCount = filterCount;
            }

            public bool IsObsolete(DateTime logEventTime, int timeoutSeconds)
            {
                if (FilterCount == 0)
                {
                    return HasExpired(logEventTime, timeoutSeconds);
                }
                return (logEventTime - LastFilterTime).TotalSeconds > timeoutSeconds && HasExpired(logEventTime, timeoutSeconds * 2);
            }

            public bool HasExpired(DateTime logEventTime, int timeoutSeconds)
            {
                return (logEventTime - LastLogTime).TotalSeconds > timeoutSeconds;
            }

            public StringBuilder StringBuffer { get; private set; }
            public LogLevel LogLevel { get; private set; }
            private DateTime LastLogTime { get; set; }
            private DateTime LastFilterTime { get; set; }
            public int FilterCount { get; private set; }
        }

        /// <summary>
        /// Filter Lookup Key (immutable)
        /// </summary>
        private struct FilterInfoKey : IEquatable<FilterInfoKey>
        {
            private readonly StringBuilder _stringBuffer;
            public readonly string StringValue;
            public readonly int StringHashCode;

            public FilterInfoKey(StringBuilder stringBuffer, string stringValue, int? stringHashCode = null)
            {
                _stringBuffer = stringBuffer;
                StringValue = stringValue;
                if (stringHashCode.HasValue)
                {
                    StringHashCode = stringHashCode.Value;
                }
                else if (stringBuffer != null)
                {
                    int hashCode = stringBuffer.Length.GetHashCode();
                    int hashCodeCount = Math.Min(stringBuffer.Length, 100);
                    for (int i = 0; i < hashCodeCount; ++i)
                        hashCode = hashCode ^ stringBuffer[i].GetHashCode();
                    StringHashCode = hashCode;
                }
                else
                {
                    StringHashCode = StringComparer.Ordinal.GetHashCode(StringValue);
                }
            }

            public override int GetHashCode()
            {
                return StringHashCode;
            }

            public bool Equals(FilterInfoKey other)
            {
                if (StringValue != null)
                {
                    return string.Equals(StringValue, other.StringValue, StringComparison.Ordinal);
                }
                if (_stringBuffer != null && other._stringBuffer != null)
                {
                    // StringBuilder.Equals only works when StringBuilder.Capacity is the same
                    if (_stringBuffer.Capacity != other._stringBuffer.Capacity)
                    {
                        return _stringBuffer.EqualTo(other._stringBuffer);
                    }
                    return _stringBuffer.Equals(other._stringBuffer);
                }
                return ReferenceEquals(_stringBuffer, other._stringBuffer) && ReferenceEquals(StringValue, other.StringValue);
            }

            public override bool Equals(object obj)
            {
                return obj is FilterInfoKey key && Equals(key);
            }
        }
    }
}
