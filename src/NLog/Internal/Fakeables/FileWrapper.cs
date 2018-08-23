using System.IO;

namespace NLog.Internal.Fakeables
{
    class FileWrapper : IFile
    {
        /// <inheritdoc />
        public bool Exists(string path)
        {
            return File.Exists(path);
        }

    }
}
