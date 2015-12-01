#if UWP10

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Win32.SafeHandles;

namespace NLog
{
    using System.Threading.Tasks;
    public static class Uapp10Ext
    {
        public static void Close(this SafeFileHandle handle)
        {
            //todo
            handle.Dispose();
        }

        public static void Close(this IDisposable handle)
        {
            //todo
            handle.Dispose();
        }

        public static string ToUpper(this string s, IFormatProvider provider)
        {
            //todo
            return s.ToUpper();
        }

        public static string ToLower(this string s, IFormatProvider provider)
        {
            //todo
            return s.ToLower();
        }
    }
}
#endif
