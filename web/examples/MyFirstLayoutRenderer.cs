using System;
using System.Text;

using NLog;

namespace MyNamespace
{
    [LayoutRenderer("hour")]
    public sealed class HourLayoutRenderer: LayoutRenderer
    {
        private bool _showMinutes = false;
        
        // this is an example of a configurable parameter
        public bool ShowMinutes
        {
            get { return _showMinutes; }
            set { _showMinutes = value; }
            
        }
        protected override int GetEstimatedBufferSize(LogEventInfo ev)
        {
            // since hour is expressed by 2 digits we need at most 2-character
            // buffer for it
            return 2;
        }

        protected override void Append(StringBuilder builder, LogEventInfo ev)
        {
            // get current hour or minute, convert it to string, apply padding
            // and append to the specified StringBuilder
            if (ShowMinutes)
            {
                builder.Append(ApplyPadding(DateTime.Now.Minute.ToString()));
            }
            else
            {
                builder.Append(ApplyPadding(DateTime.Now.Hour.ToString()));
            }
        }
    }
}
