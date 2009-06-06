using System;

namespace NLog.Config
{
    /// <summary>
    /// Arguments for <see cref="LogFactory.ConfigurationChanged"/> events.
    /// </summary>
    public class LoggingConfigurationChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the LoggingConfigurationChangedEventArgs class.
        /// </summary>
        /// <param name="oldConfiguration">The old configuration.</param>
        /// <param name="newConfiguration">The new configuration.</param>
        internal LoggingConfigurationChangedEventArgs(LoggingConfiguration oldConfiguration, LoggingConfiguration newConfiguration)
        {
            this.OldConfiguration = oldConfiguration;
            this.NewConfiguration = newConfiguration;
        }

        /// <summary>
        /// Gets the old configuration.
        /// </summary>
        /// <value>The old configuration.</value>
        public LoggingConfiguration OldConfiguration { get; private set; }

        /// <summary>
        /// Gets the new configuration.
        /// </summary>
        /// <value>The new configuration.</value>
        public LoggingConfiguration NewConfiguration { get; private set; }
    }
}
