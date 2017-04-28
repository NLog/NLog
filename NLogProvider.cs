using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace NLog.Asp
{
    public class NLogProvider : ILoggerProvider
    {
        private IDictionary<string, ILogger> loggers = new Dictionary<string, ILogger>();

        public ILogger CreateLogger(string name)
        {
            if (!loggers.ContainsKey(name))
            {
                lock (loggers)
                {
                    // Have to check again since another thread may have gotten the lock first.
                    if (!loggers.ContainsKey(name))
                    {
                        loggers[name] = new NLogAdapter(name);
                    }
                }
            }
            return loggers[name];
        }

        public void Dispose()
        {
            loggers.Clear();
            loggers = null;
        }
    }
}
