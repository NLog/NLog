// 
// Copyright (c) 2004-2020 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
// 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions 
// are met:
// 
// * Redistributions of source code must retain the above copyright notice, 
//   this list of conditions and the following disclaimer. 
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution. 
// 
// * Neither the name of Jaroslaw Kowalski nor the names of its 
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
// THE POSSIBILITY OF SUCH DAMAGE.
// 

namespace NLog.UnitTests.Internal.FileAppenders
{
    using System;
    using System.IO;
    using System.Text;

    using Xunit;

    using NLog.Targets;
    using NLog.Internal.FileAppenders;

    public class FileAppenderCacheTests : NLogTestBase
    {
        [Fact]
        public void FileAppenderCache_Empty() 
        {
            FileAppenderCache cache = FileAppenderCache.Empty;
            // An empty FileAppenderCache will have Size = 0 as well as Factory and CreateFileParameters parameters equal to null.
            Assert.True(cache.Size == 0);
            Assert.Null(cache.Factory);
            Assert.Null(cache.CreateFileParameters);

        }

        [Fact]
        public void FileAppenderCache_Construction()
        {
            IFileAppenderFactory appenderFactory = SingleProcessFileAppender.TheFactory;
            ICreateFileParameters fileTarget = new FileTarget();
            FileAppenderCache cache = new FileAppenderCache(3, appenderFactory, fileTarget);

            Assert.True(cache.Size == 3);
            Assert.NotNull(cache.Factory);           
            Assert.NotNull(cache.CreateFileParameters);
        }

        [Fact]
        public void FileAppenderCache_Allocate()
        {
            // Allocate on an Empty FileAppenderCache.
            FileAppenderCache emptyCache = FileAppenderCache.Empty;
            Assert.Throws<NullReferenceException>(() => emptyCache.AllocateAppender("file.txt"));

            // Construct a on non-empty FileAppenderCache.
            IFileAppenderFactory appenderFactory = SingleProcessFileAppender.TheFactory;
            ICreateFileParameters fileTarget = new FileTarget();
            String tempFile = Path.Combine(
                    Path.GetTempPath(),
                    Path.Combine(Guid.NewGuid().ToString(), "file.txt")
            );
            
            // Allocate an appender.
            FileAppenderCache cache = new FileAppenderCache(3, appenderFactory, fileTarget);
            BaseFileAppender appender = cache.AllocateAppender(tempFile);

            //
            // Note: Encoding is ASSUMED to be Unicode. There is no explicit reference to which encoding will be used 
            //      for the file. 
            //

            // Write, flush the content into the file and release the file.
            // We need to release the file before invoking AssertFileContents() method.
            appender.Write(StringToBytes("NLog test string."));
            appender.Flush();
            appender.Close();
            // Verify the appender has been allocated correctly.
            AssertFileContents(tempFile, "NLog test string.", Encoding.Unicode);
        }

        [Fact]
        public void FileAppenderCache_InvalidateAppender()
        {
            // Invoke InvalidateAppender() on an Empty FileAppenderCache.
            FileAppenderCache emptyCache = FileAppenderCache.Empty;
            emptyCache.InvalidateAppender("file.txt");

            // Construct a on non-empty FileAppenderCache.
            IFileAppenderFactory appenderFactory = SingleProcessFileAppender.TheFactory;
            ICreateFileParameters fileTarget = new FileTarget();
            String tempFile = Path.Combine(
                    Path.GetTempPath(),
                    Path.Combine(Guid.NewGuid().ToString(), "file.txt")
            );

            // Allocate an appender.
            FileAppenderCache cache = new FileAppenderCache(3, appenderFactory, fileTarget);
            BaseFileAppender appender = cache.AllocateAppender(tempFile);

            //
            // Note: Encoding is ASSUMED to be Unicode. There is no explicit reference to which encoding will be used 
            //      for the file. 
            //

            // Write, flush the content into the file and release the file. This happens through the
            // InvalidateAppender() method. We need to release the file before invoking AssertFileContents() method.
            appender.Write(StringToBytes("NLog test string."));
            cache.InvalidateAppender(tempFile);
            // Verify the appender has been allocated correctly.
            AssertFileContents(tempFile, "NLog test string.", Encoding.Unicode);

        }

        [Fact]
        public void FileAppenderCache_CloseAppenders()
        {
            // Invoke CloseAppenders() on an Empty FileAppenderCache.
            FileAppenderCache emptyCache = FileAppenderCache.Empty;
            emptyCache.CloseAppenders(string.Empty);

            
            IFileAppenderFactory appenderFactory = SingleProcessFileAppender.TheFactory;
            ICreateFileParameters fileTarget = new FileTarget();
            FileAppenderCache cache = new FileAppenderCache(3, appenderFactory, fileTarget);
            // Invoke CloseAppenders() on non-empty FileAppenderCache - Before allocating any appenders. 
            cache.CloseAppenders(string.Empty);            

            // Invoke CloseAppenders() on non-empty FileAppenderCache - After allocating N appenders. 
            cache.AllocateAppender("file1.txt");
            cache.AllocateAppender("file2.txt");
            cache.CloseAppenders(string.Empty);

            // Invoke CloseAppenders() on non-empty FileAppenderCache - After allocating N appenders. 
            cache.AllocateAppender("file1.txt");
            cache.AllocateAppender("file2.txt");
            cache.CloseAppenders(string.Empty);

            FileAppenderCache cache2 = new FileAppenderCache(3, appenderFactory, fileTarget);
            // Invoke CloseAppenders() on non-empty FileAppenderCache - Before allocating any appenders. 
            cache2.CloseAppenders(DateTime.Now);

            // Invoke CloseAppenders() on non-empty FileAppenderCache - After allocating N appenders. 
            cache.AllocateAppender("file1.txt");
            cache.AllocateAppender("file2.txt");
            cache.CloseAppenders(DateTime.Now);
        }

        [Fact]
        public void FileAppenderCache_GetFileCharacteristics_Single()
        {
            IFileAppenderFactory appenderFactory = SingleProcessFileAppender.TheFactory;
            ICreateFileParameters fileTarget = new FileTarget() { ArchiveNumbering = ArchiveNumberingMode.Date };
            FileAppenderCache_GetFileCharacteristics(appenderFactory, fileTarget);
        }

        [Fact]
        public void FileAppenderCache_GetFileCharacteristics_Multi()
        {
            IFileAppenderFactory appenderFactory = MutexMultiProcessFileAppender.TheFactory;
            ICreateFileParameters fileTarget = new FileTarget() { ArchiveNumbering = ArchiveNumberingMode.Date, ForceManaged = true };
            FileAppenderCache_GetFileCharacteristics(appenderFactory, fileTarget);
        }

#if !MONO && !NETSTANDARD
        [Fact]
        public void FileAppenderCache_GetFileCharacteristics_Windows()
        {
            if (NLog.Internal.PlatformDetector.IsWin32)
            {
                IFileAppenderFactory appenderFactory = WindowsMultiProcessFileAppender.TheFactory;
                ICreateFileParameters fileTarget = new FileTarget() { ArchiveNumbering = ArchiveNumberingMode.Date };
                FileAppenderCache_GetFileCharacteristics(appenderFactory, fileTarget);
            }
        }
#endif

        private void FileAppenderCache_GetFileCharacteristics(IFileAppenderFactory appenderFactory, ICreateFileParameters fileParameters)
        {
            // Invoke GetFileCharacteristics() on an Empty FileAppenderCache.
            FileAppenderCache emptyCache = FileAppenderCache.Empty;
            Assert.Null(emptyCache.GetFileCreationTimeSource("file.txt"));
            Assert.Null(emptyCache.GetFileLastWriteTimeUtc("file.txt"));
            Assert.Null(emptyCache.GetFileLength("file.txt"));
          
            FileAppenderCache cache = new FileAppenderCache(3, appenderFactory, fileParameters);
            // Invoke GetFileCharacteristics() on non-empty FileAppenderCache - Before allocating any appenders. 
            Assert.Null(emptyCache.GetFileCreationTimeSource("file.txt"));
            Assert.Null(emptyCache.GetFileLastWriteTimeUtc("file.txt"));
            Assert.Null(emptyCache.GetFileLength("file.txt"));


            String tempFile = Path.Combine(
                    Path.GetTempPath(),
                    Path.Combine(Guid.NewGuid().ToString(), "file.txt")
            );

            // Allocate an appender.
            BaseFileAppender appender = cache.AllocateAppender(tempFile);
            appender.Write(StringToBytes("NLog test string."));

            //
            // Note: Encoding is ASSUMED to be Unicode. There is no explicit reference to which encoding will be used 
            //      for the file. 
            //

            // File information should be returned.

            var fileCreationTimeUtc = cache.GetFileCreationTimeSource(tempFile);
            Assert.NotNull(fileCreationTimeUtc);
            Assert.True(fileCreationTimeUtc > Time.TimeSource.Current.FromSystemTime(DateTime.UtcNow.AddMinutes(-2)),"creationtime is wrong");

            var fileLastWriteTimeUtc = cache.GetFileLastWriteTimeUtc(tempFile);
            Assert.NotNull(fileLastWriteTimeUtc);
            Assert.True(fileLastWriteTimeUtc > DateTime.UtcNow.AddMinutes(-2), "lastwrite is wrong");

            Assert.Equal(34, cache.GetFileLength(tempFile));

            // Clean up.
            appender.Flush();
            appender.Close();
        }

        /// <summary>
        /// Converts a string to byte array.
        /// </summary>
        private static byte[] StringToBytes(string text)
        {
            byte[] bytes = new byte[sizeof(char) * text.Length];
            Buffer.BlockCopy(text.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
    }
}
