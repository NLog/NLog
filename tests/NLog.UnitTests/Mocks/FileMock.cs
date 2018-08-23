using System;
using NLog.Internal.Fakeables;

namespace NLog.UnitTests
{
    public class FileMock : IFile
    {

        private readonly Func<string, bool> _exists;

        public FileMock(Func<string, bool> exists)
        {
            _exists = exists;
        }

        public bool Exists(string path)
        {
            return _exists(path);
        }
    }
}