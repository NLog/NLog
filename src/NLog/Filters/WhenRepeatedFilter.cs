// 
// Copyright (c) 2004-2016 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
        [DefaultValue(10)]
        public int TimeoutSeconds { get; set; }

        /// <summary>
        /// Max length of filter values, will truncate if above limit
        /// </summary>
        [DefaultValue(1000)]
        public int MaxLength { get; set; }

        /// <summary>
        /// Max number of unique filter values to expect simultaneously
        /// </summary>
        [DefaultValue(50000)]
        public int MaxFilterCacheSize { get; set; }

        /// <summary>
        /// Default number of unique filter values to expect, will automatically increase if needed
        /// </summary>
        [DefaultValue(1000)]
        public int DefaultFilterCacheSize { get; set; }

        /// <summary>
        /// Insert FilterCount value into <see cref="LogEventInfo.Properties"/> when an event is no longer filtered
        /// </summary>
        [DefaultValue(null)]
        public string FilterCountPropertyName { get; set; }

        /// <summary>
        /// Append FilterCount to the <see cref="LogEventInfo.Message"/> when an event is no longer filtered
        /// </summary>
        [DefaultValue(null)]
        public string FilterCountMessageAppendFormat { get; set; }

        /// <summary>
        /// Reuse internal buffers, and doesn't have to constantly allocate new buffers
        /// </summary>
        [DefaultValue(true)]
        public bool OptimizeBufferReuse { get; set; }

        /// <summary>
        /// Default buffer size for the internal buffers
        /// </summary>
        [DefaultValue(1000)]
        public int OptimizeBufferDefaultLength { get; set; }

        /// <summary>
        /// Can be used if <see cref="OptimizeBufferReuse"/> has been enabled.
        /// </summary>
        internal readonly ReusableBuilderCreator ReusableLayoutBuilder = new ReusableBuilderCreator();

        private readonly Dictionary<FilterInfoKey, FilterInfo> _repeatFilter = new Dictionary<FilterInfoKey, FilterInfo>(1000);
        private readonly Stack<KeyValuePair<FilterInfoKey, FilterInfo>> _objectPool = new Stack<KeyValuePair<FilterInfoKey, FilterInfo>>(1000);

        /// <summary>
        /// Constructor
        /// </summary>
        public WhenRepeatedFilter()
        {
            TimeoutSeconds = 10;
            MaxLength = 1000;
            DefaultFilterCacheSize = 1000;
            MaxFilterCacheSize = 50000;
            OptimizeBufferReuse = true;
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
            lock (_repeatFilter)
            {
                using (var targetBuilder = OptimizeBufferReuse ? ReusableLayoutBuilder.Allocate() : ReusableLayoutBuilder.None)
                {
                    if (OptimizeBufferReuse && targetBuilder.Result.Capacity != OptimizeBufferDefaultLength)
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

                    FilterInfoKey filterInfoKey = RenderFilterInfoKey(logEvent, OptimizeBufferReuse ? targetBuilder.Result : null);

                    FilterInfo filterInfo;
                    if (_repeatFilter.TryGetValue(filterInfoKey, out filterInfo))
                    {
                        return RefreshFilterInfo(logEvent, filterInfo);
                    }

                    filterInfo = CreateFilterInfo(logEvent);
                    if (OptimizeBufferReuse && filterInfo.StringBuffer != null)
                    {
                        filterInfo.StringBuffer.ClearBuilder();
                        int length = Math.Min(targetBuilder.Result.Length, MaxLength);
                        for (int i = 0; i < length; ++i)
                            filterInfo.StringBuffer.Append(targetBuilder.Result[i]);
                    }
                    filterInfo.Refresh(logEvent.Level, logEvent.TimeStamp, 0);
                    _repeatFilter.Add(new FilterInfoKey(filterInfo.StringBuffer, filterInfoKey.StringValue, filterInfoKey.StringHashCode), filterInfo);
                }
            }

            return FilterResult.Neutral;
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
                reusableObject = new FilterInfo(OptimizeBufferReuse ? new StringBuilder(OptimizeBufferDefaultLength) : null);
            }
            else
            {
                reusableObject = _objectPool.Pop().Value;
                // StringBuilder.Equals only works when StringBuilder.Capacity is the same
                if (OptimizeBufferReuse && reusableObject.StringBuffer != null && reusableObject.StringBuffer.Capacity != OptimizeBufferDefaultLength)
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
                Layout.RenderAppendBuilder(logEvent, targetBuilder, false);
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
                        object otherFilterCount;
                        if (!logEvent.Properties.TryGetValue(FilterCountPropertyName, out otherFilterCount))
                        {
                            logEvent.Properties[FilterCountPropertyName] = filterCount;
                        }
                        else if (otherFilterCount is int)
                        {
                            filterCount = Math.Max((int)otherFilterCount, filterCount);
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
            private readonly StringBuilder StringBuffer;
            public readonly string StringValue;
            public readonly int StringHashCode;

            public FilterInfoKey(StringBuilder stringBuffer, string stringValue, int? stringHashCode = null)
            {
                StringBuffer = stringBuffer;
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
                if (StringBuffer != null && other.StringBuffer != null)
                {
                    // StringBuilder.Equals only works when StringBuilder.Capacity is the same
                    if (StringBuffer.Capacity != other.StringBuffer.Capacity)
                    {
                        if (StringBuffer.Length != other.StringBuffer.Length)
                            return false;

                        for (int x = 0; x < StringBuffer.Length; ++x)
                        {
                            if (StringBuffer[x] != other.StringBuffer[x])
                            {
                                return false;
                            }
                        }

                        return true;
                    }
                    return StringBuffer.Equals(other.StringBuffer);
                }
                return ReferenceEquals(StringBuffer, other.StringBuffer) && ReferenceEquals(StringValue, other.StringValue);
            }

            public override bool Equals(object other)
            {
                return other is FilterInfoKey && Equals((FilterInfoKey)other);
            }
        }
    }
}
