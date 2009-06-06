using System;

namespace NLog.Config
{
    public class LoggingConfigurationChangedEventArgs : EventArgs
    {
        internal LoggingConfigurationChangedEventArgs(LoggingConfiguration oldConfiguration, LoggingConfiguration newConfiguration)
        {
            this.OldConfiguration = oldConfiguration;
            this.NewConfiguration = newConfiguration;
        }

        public LoggingConfiguration OldConfiguration { get; private set; }

        public LoggingConfiguration NewConfiguration { get; private set; }
    }
}
