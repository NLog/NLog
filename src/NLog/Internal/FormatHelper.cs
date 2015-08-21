#region

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace NLog.Internal
{
    internal static class FormatHelper
    {
        /// <summary>
        /// toString(format) if the object is a <see cref="IFormattable"/>
        /// </summary>
        /// <param name="value">value to be converted</param>
        /// <param name="format">format value</param>
        /// <param name="formatProvider">provider, for example culture</param>
        /// <returns></returns>
        public static string ToStringWithOptionalFormat(this object value, string format, IFormatProvider formatProvider)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if (format == null)
            {
                return Convert.ToString(value, formatProvider);
            }

            var formattable = value as IFormattable;
            if (formattable != null)
            {
                return formattable.ToString(format, formatProvider);
            }

            return Convert.ToString(value, formatProvider);
        }
    }
}