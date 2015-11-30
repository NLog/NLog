using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NLog.Config
{
    /// <summary>
    /// Format of the excpetion output to the specific target.
    /// </summary>
    public enum ExceptionRenderingFormat
    {
        /// <summary>
        /// This is for internal NLog Library usage. This is required we would a default value in switch cases. 
        /// </summary>
        NotSet = -1,
        /// <summary>
        /// Appends the Message of an Exception to the specified target.
        /// </summary>
        Message = 0,

        /// <summary>
        /// Appends the type of an Exception to the specified target.
        /// </summary>
        Type = 1,
        /// <summary>
        /// Appends the short type of an Exception to the specified target.
        /// </summary>
        ShortType = 2,
        /// <summary>
        /// Appends the result of calling ToString() on an Exception to the specified target.
        /// </summary>
        ToString = 3,
        /// <summary>
        /// Appends the method name from Exception's stack trace to the specified target.
        /// </summary>
        Method = 4,
        /// <summary>
        /// Appends the stack trace from an Exception to the specified target.
        /// </summary>
        StackTrace = 5,
        /// <summary>
        /// Appends the contents of an Exception's Data property to the specified target.
        /// </summary>
        Data = 6
    }
}
