using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLog.Targets
{
    /// <summary>
    /// Type of filepath
    /// </summary>
    public enum FilePathKind : byte
    {
        /// <summary>
        /// Detect of relative or absolute
        /// </summary>
        Unknown,
        /// <summary>
        /// Relative path
        /// </summary>
        Relative,

        /// <summary>
        /// Absolute path
        /// </summary>
        /// <remarks>Best for performance</remarks>
        Absolute
    }
}
