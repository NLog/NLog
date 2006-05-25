using System;
using System.Text;

using NLog;

namespace MyNamespace
{
    [Filter("hourRange")]
    public sealed class HourRangeFilter: Filter
    {
        private int _fromHour = 0;
        private int _toHour = -1;
        
        public int FromHour
        {
            get { return _fromHour; }
            set { _fromHour = value; }
            
        }
        public int ToHour
        {
            get { return _toHour; }
            set { _toHour = value; }
            
        }

        protected override FilterResult Check(LogEventInfo ev)
        {
            if (ev.TimeStamp.Hour >= FromHour && ev.TimeStamp.Hour <= ToHour)
                return Result;
            else
                return FilterResult.Neutral;
        }
    }
}
