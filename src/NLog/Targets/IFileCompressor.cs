using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NLog.Targets
{
    /// <summary>
    /// <see cref="FileTarget"/> instances may be configured to compress archived files in a custom way
    /// by setting <see cref="FileTarget.Compressor"/> per instance
    /// or <see cref="FileTarget.DefaultCompressor"/> for all instances.
    /// </summary>
    public interface IFileCompressor
    {
        /// <summary>
        /// Create archiveFileName by compressing fileName.
        /// </summary>
        /// <param name="fileName">Absolute path to the log file to compress/zip.</param>
        /// <param name="archiveFileName">Absolute path to the archive zip file to create.</param>
        void Compress(string fileName, string archiveFileName);
    }
}
