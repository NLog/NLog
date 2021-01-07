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

namespace NLog.Internal
{
    using System;
    using System.Collections.Generic;
    using NLog.Config;
    using NLog.Filters;
    using NLog.Targets;

    /// <summary>
    /// Represents target with a chain of filters which determine
    /// whether logging should happen.
    /// </summary>
    [NLogConfigurationItem]
    internal class TargetWithFilterChain
    {
        /// <summary>
        /// cached result as calculating is expensive.
        /// </summary>
        private StackTraceUsage? _stackTraceUsage;

        private MruCache<CallSiteKey, string> _callSiteClassNameCache;

        struct CallSiteKey : IEquatable<CallSiteKey>
        {
            public CallSiteKey(string methodName, string fileSourceName, int fileSourceLineNumber)
            {
                MethodName = methodName ?? string.Empty;
                FileSourceName = fileSourceName ?? string.Empty;
                FileSourceLineNumber = fileSourceLineNumber;
            }

            public readonly string MethodName;
            public readonly string FileSourceName;
            public readonly int FileSourceLineNumber;

            /// <summary>
            /// Serves as a hash function for a particular type.
            /// </summary>
            /// <returns>
            /// A hash code for the current <see cref="T:System.Object"/>.
            /// </returns>
            public override int GetHashCode()
            {
                return MethodName.GetHashCode() ^ FileSourceName.GetHashCode() ^ FileSourceLineNumber;
            }

            /// <summary>
            /// Determines if two objects are equal in value.
            /// </summary>
            /// <param name="obj">Other object to compare to.</param>
            /// <returns>True if objects are equal, false otherwise.</returns>
            public override bool Equals(object obj)
            {
                return obj is CallSiteKey key && Equals(key);
            }

            /// <summary>
            /// Determines if two objects of the same type are equal in value.
            /// </summary>
            /// <param name="other">Other object to compare to.</param>
            /// <returns>True if objects are equal, false otherwise.</returns>
            public bool Equals(CallSiteKey other)
            {
                return FileSourceLineNumber == other.FileSourceLineNumber
                    && string.Equals(FileSourceName, other.FileSourceName, StringComparison.Ordinal)
                    && string.Equals(MethodName, other.MethodName, StringComparison.Ordinal);
            }
        }

        internal bool TryCallSiteClassNameOptimization(StackTraceUsage stackTraceUsage, LogEventInfo logEvent)
        {
            if ((stackTraceUsage & (StackTraceUsage.WithCallSiteClassName | StackTraceUsage.WithStackTrace)) != StackTraceUsage.WithCallSiteClassName)
                return false;

            if (string.IsNullOrEmpty(logEvent.CallSiteInformation?.CallerFilePath))
                return false;

            if (logEvent.HasStackTrace)
                return false;

            return true;
        }

        internal bool MustCaptureStackTrace(StackTraceUsage stackTraceUsage, LogEventInfo logEvent)
        {
            if (logEvent.HasStackTrace)
                return false;

            if ((stackTraceUsage & StackTraceUsage.WithStackTrace) != StackTraceUsage.None)
                return true;

            if ((stackTraceUsage & StackTraceUsage.WithCallSite) != StackTraceUsage.None && string.IsNullOrEmpty(logEvent.CallSiteInformation?.CallerMethodName) && string.IsNullOrEmpty(logEvent.CallSiteInformation?.CallerFilePath))
                return true;    // We don't have enough CallSiteInformation

            return false;
        }

        internal bool TryRememberCallSiteClassName(LogEventInfo logEvent)
        {
            if (string.IsNullOrEmpty(logEvent.CallSiteInformation?.CallerFilePath))
                return false;

            string className = logEvent.CallSiteInformation.GetCallerClassName(null, true, true, true);
            if (string.IsNullOrEmpty(className))
                return false;

            if (_callSiteClassNameCache == null)
                return false;

            string internClassName = logEvent.LoggerName == className ?
                logEvent.LoggerName :
#if !NETSTANDARD1_3 && !NETSTANDARD1_5
                string.Intern(className);   // Single string-reference for all logging-locations for the same class
#else
                className;
#endif
            CallSiteKey callSiteKey = new CallSiteKey(logEvent.CallerMemberName, logEvent.CallerFilePath, logEvent.CallerLineNumber);
            return _callSiteClassNameCache.TryAddValue(callSiteKey, internClassName);
        }

        internal bool TryLookupCallSiteClassName(LogEventInfo logEvent, out string callSiteClassName)
        {
            callSiteClassName = logEvent.CallSiteInformation?.CallerClassName;
            if (!string.IsNullOrEmpty(callSiteClassName))
                return true;

            if (_callSiteClassNameCache == null)
            {
                System.Threading.Interlocked.CompareExchange(ref _callSiteClassNameCache, new MruCache<CallSiteKey, string>(1000), null);
            }
            CallSiteKey callSiteKey = new CallSiteKey(logEvent.CallerMemberName, logEvent.CallerFilePath, logEvent.CallerLineNumber);
            return _callSiteClassNameCache.TryGetValue(callSiteKey, out callSiteClassName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TargetWithFilterChain" /> class.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="filterChain">The filter chain.</param>
        /// <param name="defaultResult">Default action if none of the filters match.</param>
        public TargetWithFilterChain(Target target, IList<Filter> filterChain, FilterResult defaultResult)
        {
            Target = target;
            FilterChain = filterChain;
            DefaultResult = defaultResult;
        }

        /// <summary>
        /// Gets the target.
        /// </summary>
        /// <value>The target.</value>
        public Target Target { get; }

        /// <summary>
        /// Gets the filter chain.
        /// </summary>
        /// <value>The filter chain.</value>
        public IList<Filter> FilterChain { get;  }

        /// <summary>
        /// Default action if none of the filters match.
        /// </summary>
        public FilterResult DefaultResult { get; }

        /// <summary>
        /// Gets or sets the next <see cref="TargetWithFilterChain"/> item in the chain.
        /// </summary>
        /// <value>The next item in the chain.</value>
        /// <example>This is for example the 'target2' logger in writeTo='target1,target2'  </example>
        public TargetWithFilterChain NextInChain { get; set; }

        /// <summary>
        /// Gets the stack trace usage.
        /// </summary>
        /// <returns>A <see cref="StackTraceUsage" /> value that determines stack trace handling.</returns>
        public StackTraceUsage GetStackTraceUsage()
        {
            return _stackTraceUsage ?? StackTraceUsage.None;
        }

        internal StackTraceUsage PrecalculateStackTraceUsage()
        {
            var stackTraceUsage = StackTraceUsage.None;

            // find all objects which may need stack trace
            // and determine maximum
            if (Target != null)
            {
                stackTraceUsage = Target.StackTraceUsage;
            }

            //recurse into chain if not max
            if (NextInChain != null && (stackTraceUsage & StackTraceUsage.Max) != StackTraceUsage.Max)
            {
                var stackTraceUsageForChain = NextInChain.PrecalculateStackTraceUsage();
                stackTraceUsage |= stackTraceUsageForChain;
            }

            _stackTraceUsage = stackTraceUsage;
            return stackTraceUsage;
        }
    }
}
