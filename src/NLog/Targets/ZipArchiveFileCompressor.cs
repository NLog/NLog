namespace NLog.Targets
{
using System.IO;
using System.IO.Compression;

    /// <summary>
    /// Builtin IFileCompressor implementation utilizing the .Net4.5 specific <see cref="ZipArchive"/> 
    /// and is used as the default value for <see cref="FileTarget.DefaultCompressor"/>.
    /// So log files created via <see cref="FileTarget"/> can be zipped when archived
    /// w/o 3rd party zip library.
    /// </summary>
    internal class ZipArchiveFileCompressor : IFileCompressor
    {
        /// <summary>
        /// Implements <see cref="IFileCompressor.Compress(string, string)"/>
        /// </summary>
        public void Compress(string fileName, string archiveFileName)
        {
            using (var archiveStream = new FileStream(archiveFileName, FileMode.Create))
            using (var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create))
            using (var originalFileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite ))
            {
                var zipArchiveEntry = archive.CreateEntry(Path.GetFileName(fileName));
                using (var destination = zipArchiveEntry.Open())
                {
                    originalFileStream.CopyTo(destination);
                }
            }
        }
    }
}
