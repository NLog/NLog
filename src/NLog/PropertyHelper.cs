using System;
using System.Reflection;
using System.Globalization;

namespace NLog
{
    internal class PropertyHelper
    {
        private PropertyHelper()
        {
        }

        public static void SetPropertyFromString(object o, string name, string value) {
            PropertyInfo propInfo = o.GetType().GetProperty(name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (propInfo == null)
                throw new NotSupportedException("Parameter " + name + " not supported on " + o.GetType().Name);

            object newValue = Convert.ChangeType(value, propInfo.PropertyType, CultureInfo.InvariantCulture);

            propInfo.SetValue(o, newValue, null);
        }
    }
}
