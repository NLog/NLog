using System;
using System.Collections.Generic;
using System.Text;

namespace NLog.Internal
{
    internal class StackTraceUsageUtils
    {
        public static StackTraceUsage Max(StackTraceUsage u1, StackTraceUsage u2)
        {
            return (StackTraceUsage)Math.Max((int)u1, (int)u2);
        }
    }
}
