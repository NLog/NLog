using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NLog.Internal
{
#if NETSTANDARD

    internal static class MemoryStreamHelpers
    {

        public static byte[] GetBuffer(this MemoryStream stream)
        {
            ArraySegment<byte> bytes;
            if (stream.TryGetBuffer(out bytes))
            {
                return bytes.Array;
            }
            return new byte[0];
        }


    }

#endif
}
