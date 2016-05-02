using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NLog.Common
{
    /// <summary>
    /// 
    /// </summary>
    public interface IFileCompressor
    {
        /// <summary>
        /// Create archiveFileName by compressing fileName.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="archiveFileName"></param>
        /// <returns>true if compressing was successful.</returns>
        bool Compress(string fileName, string archiveFileName);
    }
}
