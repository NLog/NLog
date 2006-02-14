using System;
using NLogViewer.Events;

namespace NLogViewer.Receivers
{
	public interface ILogEventProcessor
	{
        void ProcessLogEvent(LogEvent theEvent);
	}
}
