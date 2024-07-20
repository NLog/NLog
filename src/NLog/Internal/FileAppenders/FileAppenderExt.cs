using System.IO;

namespace NLog.Internal.FileAppenders
{
    internal static class FileAppenderExt
    {
        internal static void Write(this BaseFileAppender appender, Stream sourceStream)
        {
            var position = sourceStream.Position;
            sourceStream.Position = 0;
            var buffer = new byte[4096];
            int read;
            
            while ((read = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                appender.Write(buffer, 0, read);

            sourceStream.Position = position;
        }
    }
}