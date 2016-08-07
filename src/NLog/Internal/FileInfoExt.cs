using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog.Layouts;

namespace NLog.Internal
{
    internal static class FileInfoExt
    {
        public static DateTime GetLastWriteTimeUtc(this FileInfo fileInfo)
        {
#if !SILVERLIGHT
            return fileInfo.LastWriteTimeUtc;
#else
            return fileInfo.LastWriteTime;
#endif
        }
        public static DateTime GetCreationTimeUtc(this FileInfo fileInfo)
        {
#if !SILVERLIGHT
            return fileInfo.CreationTimeUtc;
#else
            return fileInfo.CreationTime;
#endif
        }

       

    }
}
