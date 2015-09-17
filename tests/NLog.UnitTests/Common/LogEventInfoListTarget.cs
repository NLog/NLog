using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog.Targets;

namespace NLog.UnitTests.Common
{
    /// <summary>
    /// Target for unit testing the last written LogEvent (non rendered!)
    /// </summary>
    [Target("LastLogEvent")]
    public class LastLogEventListTarget : TargetWithLayout
    {
        /// <summary>
        /// Increases the number of messages.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        protected override void Write(LogEventInfo logEvent)
        {
            this.LastLogEvent = logEvent;
        }

        public LogEventInfo LastLogEvent { get; set; }
    }
}
