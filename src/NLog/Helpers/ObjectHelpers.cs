using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLog.Helpers
{
    internal static class ObjectHelpers
    {
        internal static string ConvertToString(object o, IFormatProvider formatProvider)
        {
            // if no IFormatProvider is specified, use the Configuration.DefaultCultureInfo value.
            if ((formatProvider == null) && (LogManager.Configuration != null))
                formatProvider = LogManager.Configuration.DefaultCultureInfo;

            return String.Format(formatProvider, "{0}", o);
        }
    }
}
