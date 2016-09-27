using System;

namespace NLog.Common
{
    /// <summary>
    /// 
    /// </summary>
    [Flags]
    public enum PoolSetup
    {
        /// <summary>
        /// 
        /// </summary>
        None = 0,

        /// <summary>
        /// 
        /// </summary>
        Active = 1,

        /// <summary>
        /// 
        /// </summary>
        FixedSize = 2 | 1,

        /// <summary>
        /// 
        /// </summary>
        Large = 4 | 1,
    }
}
