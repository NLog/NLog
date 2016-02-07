using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog.Config;

namespace NLog.Internal
{
    /// <summary>
    /// Helpers for <see cref="StringBuilder"/>, which is used in e.g. layout renderers.
    /// </summary>
    internal static class StringBuilderExt
    {
        /// <summary>
        /// Append a value and use formatProvider of <paramref name="logEvent"/> or <paramref name="configuration"/> to convert to string.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="o">value to append.</param>
        /// <param name="logEvent">current logEvent for FormatProvider.</param>
        /// <param name="configuration">Configuration for DefaultCultureInfo</param>
        public static void Append(this StringBuilder builder, object o, LogEventInfo logEvent, LoggingConfiguration configuration)
        {
            var formatProvider = logEvent.FormatProvider;
            if (formatProvider == null && configuration != null)
            {
                formatProvider = configuration.DefaultCultureInfo;
            }
            builder.Append(Convert.ToString(o, formatProvider));
        }

    }
}
