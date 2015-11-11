using System;
using System.Collections.Generic;
using System.Linq;

namespace NLog.Targets
{
    /// <summary>
    /// The action to be taken when there are more connections then the max.
    /// </summary>
    public enum NetworkTargetConnectionsOverflowAction
    {
        /// <summary>
        /// Just allow it.
        /// </summary>
        AllowNewConnnection,

        /// <summary>
        /// Discard the connection item.
        /// </summary>
        DiscardMessage,

        /// <summary>
        /// Block until there's more room in the queue.
        /// </summary>
        Block,
    }
}