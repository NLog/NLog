using System;
using System.Collections.Generic;
using System.Text;

namespace NLog
{
    /// <summary>
    /// Value indicating how stack trace should be captured when processing the log event.
    /// </summary>
    public enum StackTraceUsage
    {
        /// <summary>
        /// Stack trace should not be captured.
        /// </summary>
        None = 0, 

        /// <summary>
        /// Stack trace should be captured without source-level information.
        /// </summary>
        WithoutSource = 1,

        /// <summary>
        /// Stack trace should be captured including source-level information such as line numbers.
        /// </summary>
        WithSource = 2,
    }
}
