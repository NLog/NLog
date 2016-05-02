namespace NLog.Common
{
#if NET4_5
using System.IO;
using System.IO.Compression;
#endif

    /// <summary>
    /// Uses .Net4.5 ZipArchive
    /// </summary>
    public class DefaultFileCompressor : IFileCompressor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="archiveFileName"></param>
        /// <returns></returns>
        public bool Compress(string fileName, string archiveFileName)
        {
#if NET4_5
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
            return true;
#else
            return false;
#endif
        }
    }
}
