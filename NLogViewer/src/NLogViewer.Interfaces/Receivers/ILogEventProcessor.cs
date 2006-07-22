using System;
using NLogViewer.Events;

namespace NLogViewer.Receivers
{
	public interface ILogEventProcessor
	{
        LogEvent CreateLogEvent();
        void ProcessLogEvent(LogEvent theEvent);
	}
}
