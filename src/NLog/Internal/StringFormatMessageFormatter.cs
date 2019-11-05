using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog.MessageTemplates;

namespace NLog.Internal
{
    class StringFormatMessageFormatter : ILogMessageFormatter
    {
        #region Implementation of ILogMessageFormatter

        /// <inheritdoc />
        public string FormatMessage(LogEventInfo logEvent, TemplateRenderer templateRenderer)
        {
            if (logEvent.Parameters == null || logEvent.Parameters.Length == 0)
            {
                return logEvent.Message;
            }
            else
            {
                return string.Format(logEvent.FormatProvider ?? CultureInfo.CurrentCulture, logEvent.Message, logEvent.Parameters);
            }
        }

        /// <inheritdoc />
        public bool HasProperties(LogEventInfo logEvent)
        {
            return false; //todo
        }

        /// <inheritdoc />
        public void AppendFormattedMessage(LogEventInfo logEvent, StringBuilder builder, TemplateRenderer templateRenderer)
        {
            builder.AppendFormat(logEvent.FormatProvider ?? CultureInfo.CurrentCulture, logEvent.Message, logEvent.Parameters);
        }

        #endregion
    }
}
