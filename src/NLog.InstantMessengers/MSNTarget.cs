using System;
using System.Collections;
using System.Text;
using NLog;

namespace NLog.InstantMessengers
{
    [Target("MSN")]
    public class MSNTarget : NLog.Target
    {
        protected override void Write(LogEventInfo logEvent)
        {
        }
    }
}
