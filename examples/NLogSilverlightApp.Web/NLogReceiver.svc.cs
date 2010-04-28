namespace NLogSilverlightApp.Web
{
    using System;

    using NLog;
    using NLog.LogReceiverService;

    public class NLogReceiver : ILogReceiverServer
    {
        public void ProcessLogMessages(NLogEvents events)
        {
            DateTime baseTimeUtc = new DateTime(events.BaseTimeUtc, DateTimeKind.Utc);

            foreach (var ev in events.Events)
            {
                LogLevel level = LogLevel.FromOrdinal(ev.LevelOrdinal);
                string loggerName = events.LoggerNames[ev.LoggerOrdinal];

                Logger logger = LogManager.GetLogger(loggerName);
                var logEventInfo = new LogEventInfo();
                logEventInfo.Level = level;
                logEventInfo.LoggerName = loggerName;
                logEventInfo.TimeStamp = baseTimeUtc.AddTicks(ev.TimeDelta);
                logEventInfo.Properties.Add("ClientName", events.ClientName);
                for (int i = 0; i < events.LayoutNames.Count; ++i)
                {
                    logEventInfo.Properties.Add(events.LayoutNames[i], ev.Values[i]);
                }
                logger.Log(logEventInfo);
            }
        }
    }
}
