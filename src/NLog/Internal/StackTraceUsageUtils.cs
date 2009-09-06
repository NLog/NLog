using System;

using NLog.Config;

namespace NLog.Internal
{
    /// <summary>
    /// Utilities for dealing with <see cref="StackTraceUsage"/> values.
    /// </summary>
    internal class StackTraceUsageUtils
    {
        internal static StackTraceUsage Max(StackTraceUsage u1, StackTraceUsage u2)
        {
            return (StackTraceUsage)Math.Max((int)u1, (int)u2);
        }
    }
}
