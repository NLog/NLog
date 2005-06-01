using System;

namespace NLog.Viewer
{
	public class LogInstanceFactory
	{
        public static LogInstance CreateLogInstance(LogInstanceConfigurationInfo LogInstanceConfigurationInfo)
        {
            return new LogInstanceUDP(LogInstanceConfigurationInfo);
        }
	}
}
