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


namespace NLog.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.IO;
    using System.IO.Compression;
    using System.Runtime.CompilerServices;
    using System.Text;
    using NLog.Common;
    using NLog.Config;
    using NLog.Layouts;
    using NLog.Targets;
    using Xunit;
#if !NETSTANDARD
    using Ionic.Zip;
#endif

    public abstract class NLogTestBase
    {
        protected NLogTestBase()
        {
            //reset before every test
            LogManager.ThrowExceptions = false; // Ignore any errors triggered by closing existing config
            LogManager.Configuration = null;    // Will close any existing config
            LogManager.LogFactory.ResetCandidateConfigFilePath();

            InternalLogger.Reset();
            InternalLogger.LogLevel = LogLevel.Off;
            LogManager.ThrowExceptions = true;  // Ensure exceptions are thrown by default during unit-testing
            LogManager.ThrowConfigExceptions = null;
            System.Diagnostics.Trace.Listeners.Clear();
#if !NETSTANDARD
            System.Diagnostics.Debug.Listeners.Clear();
#endif
        }

        protected void AssertDebugCounter(string targetName, int val)
        {
            Assert.Equal(val, GetDebugTarget(targetName).Counter);
        }

        protected void AssertDebugLastMessage(string targetName, string msg)
        {
            Assert.Equal(msg, GetDebugLastMessage(targetName));
        }

        protected void AssertDebugLastMessageContains(string targetName, string msg)
        {
            string debugLastMessage = GetDebugLastMessage(targetName);
            Assert.True(debugLastMessage.Contains(msg),
                $"Expected to find '{msg}' in last message value on '{targetName}', but found '{debugLastMessage}'");
        }

        protected string GetDebugLastMessage(string targetName)
        {
            return GetDebugLastMessage(targetName, LogManager.Configuration);
        }

        protected string GetDebugLastMessage(string targetName, LoggingConfiguration configuration)
        {
            return GetDebugTarget(targetName, configuration).LastMessage;
        }

        public DebugTarget GetDebugTarget(string targetName)
        {
            return GetDebugTarget(targetName, LogManager.Configuration);
        }

        protected DebugTarget GetDebugTarget(string targetName, LoggingConfiguration configuration)
        {
            return LogFactoryTestExtensions.GetDebugTarget(targetName, configuration);
        }

        protected void AssertFileContentsStartsWith(string fileName, string contents, Encoding encoding)
        {
            FileInfo fi = new FileInfo(fileName);
            if (!fi.Exists)
                Assert.True(false, "File '" + fileName + "' doesn't exist.");

            byte[] encodedBuf = encoding.GetBytes(contents);

            byte[] buf = File.ReadAllBytes(fileName);
            Assert.True(encodedBuf.Length <= buf.Length,
                $"File:{fileName} encodedBytes:{encodedBuf.Length} does not match file.content:{buf.Length}, file.length = {fi.Length}");

            for (int i = 0; i < encodedBuf.Length; ++i)
            {
                if (encodedBuf[i] != buf[i])
                    Assert.True(encodedBuf[i] == buf[i],
                        $"File:{fileName} content mismatch {(int)encodedBuf[i]} <> {(int)buf[i]} at index {i}");
            }
        }

        protected void AssertFileContentsEndsWith(string fileName, string contents, Encoding encoding)
        {
            if (!File.Exists(fileName))
                Assert.True(false, "File '" + fileName + "' doesn't exist.");

            string fileText = File.ReadAllText(fileName, encoding);
            Assert.True(fileText.Length >= contents.Length);
            Assert.Equal(contents, fileText.Substring(fileText.Length - contents.Length));
        }

        protected class CustomFileCompressor : IArchiveFileCompressor
        {
            public void CompressFile(string fileName, string archiveFileName)
            {
                string entryName = Path.GetFileNameWithoutExtension(archiveFileName) + Path.GetExtension(fileName);
                CompressFile(fileName, archiveFileName, entryName);
            }

            public void CompressFile(string fileName, string archiveFileName, string entryName)
            {
#if !NETSTANDARD
                using (var zip = new Ionic.Zip.ZipFile())
                {
                    ZipEntry entry = zip.AddFile(fileName);
                    entry.FileName = entryName;
                    zip.Save(archiveFileName);
                }
#endif
            }
        }

#if NET35 || NET40
        protected void AssertZipFileContents(string fileName, string expectedEntryName, string contents, Encoding encoding)
        {
            if (!File.Exists(fileName))
                Assert.True(false, "File '" + fileName + "' doesn't exist.");

            byte[] encodedBuf = encoding.GetBytes(contents);
            
            using (var zip = new Ionic.Zip.ZipFile(fileName))
            {
                Assert.Equal(1, zip.Count);
                Assert.Equal(encodedBuf.Length, zip[0].UncompressedSize);

                byte[] buf = new byte[zip[0].UncompressedSize];
                using (var fs = zip[0].OpenReader())
                {
                    fs.Read(buf, 0, buf.Length);
                }
                                
                for (int i = 0; i < buf.Length; ++i)
                {
                    Assert.Equal(encodedBuf[i], buf[i]);
                }
            }
        }
#else
        protected void AssertZipFileContents(string fileName, string expectedEntryName, string contents, Encoding encoding)
        {
            FileInfo fi = new FileInfo(fileName);
            if (!fi.Exists)
                Assert.True(false, "File '" + fileName + "' doesn't exist.");

            byte[] encodedBuf = encoding.GetBytes(contents);
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var zip = new ZipArchive(stream, ZipArchiveMode.Read))
            {
                Assert.Single(zip.Entries);
                Assert.Equal(expectedEntryName, zip.Entries[0].Name);
                Assert.Equal(encodedBuf.Length, zip.Entries[0].Length);

                byte[] buf = new byte[(int)zip.Entries[0].Length];
                using (var fs = zip.Entries[0].Open())
                {
                    fs.Read(buf, 0, buf.Length);
                }

                for (int i = 0; i < buf.Length; ++i)
                {
                    Assert.Equal(encodedBuf[i], buf[i]);
                }
            }
        }
#endif

        protected void AssertFileContents(string fileName, string expectedEntryName, string contents, Encoding encoding)
        {
            AssertFileContents(fileName, contents, encoding, false);
        }

        protected void AssertFileContents(string fileName, string contents, Encoding encoding)
        {
            AssertFileContents(fileName, contents, encoding, false);
        }

        protected void AssertFileContents(string fileName, string contents, Encoding encoding, bool addBom)
        {
            FileInfo fi = new FileInfo(fileName);
            if (!fi.Exists)
                Assert.True(false, "File '" + fileName + "' doesn't exist.");

            byte[] encodedBuf = encoding.GetBytes(contents);

            //add bom if needed
            if (addBom)
            {
                var preamble = encoding.GetPreamble();
                if (preamble.Length > 0)
                {
                    //insert before
                    encodedBuf = preamble.Concat(encodedBuf).ToArray();
                }
            }

            byte[] buf;
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            {
                int index = 0;
                int count = (int)fs.Length;
                buf = new byte[count];
                while (count > 0)
                {
                    int n = fs.Read(buf, index, count);
                    if (n == 0)
                        break;
                    index += n;
                    count -= n;
                }
            }

            Assert.True(encodedBuf.Length == buf.Length,
                $"File:{fileName} encodedBytes:{encodedBuf.Length} does not match file.content:{buf.Length}, file.length = {fi.Length}");

            for (int i = 0; i < buf.Length; ++i)
            {
                if (encodedBuf[i] != buf[i])
                    Assert.True(encodedBuf[i] == buf[i],
                        $"File:{fileName} content mismatch {(int)encodedBuf[i]} <> {(int)buf[i]} at index {i}");
            }
        }

        protected void AssertFileContains(string fileName, string contentToCheck, Encoding encoding)
        {
            if (contentToCheck.Contains(Environment.NewLine))
                Assert.True(false, "Please use only single line string to check.");

            FileInfo fi = new FileInfo(fileName);
            if (!fi.Exists)
                Assert.True(false, "File '" + fileName + "' doesn't exist.");

            using (TextReader fs = new StreamReader(fileName, encoding))
            {
                string line;
                while ((line = fs.ReadLine()) != null)
                {
                    if (line.Contains(contentToCheck))
                        return;
                }
            }

            Assert.True(false, "File doesn't contains '" + contentToCheck + "'");
        }

        protected void AssertFileNotContains(string fileName, string contentToCheck, Encoding encoding)
        {
            if (contentToCheck.Contains(Environment.NewLine))
                Assert.True(false, "Please use only single line string to check.");

            FileInfo fi = new FileInfo(fileName);
            if (!fi.Exists)
                Assert.True(false, "File '" + fileName + "' doesn't exist.");

            using (TextReader fs = new StreamReader(fileName, encoding))
            {
                string line;
                while ((line = fs.ReadLine()) != null)
                {
                    if (line.Contains(contentToCheck))
                        Assert.False(true, "File contains '" + contentToCheck + "'");
                }
            }
        }

        protected string StringRepeat(int times, string s)
        {
            StringBuilder sb = new StringBuilder(s.Length * times);
            for (int i = 0; i < times; ++i)
                sb.Append(s);
            return sb.ToString();
        }

        /// <summary>
        /// Render layout <paramref name="layout"/> with dummy <see cref="LogEventInfo" />and compare result with <paramref name="expected"/>.
        /// </summary>
        protected static void AssertLayoutRendererOutput(Layout layout, string expected)
        {
            var logEventInfo = LogEventInfo.Create(LogLevel.Info, "loggername", "message");

            AssertLayoutRendererOutput(layout, logEventInfo, expected);
        }

        /// <summary>
        /// Render layout <paramref name="layout"/> with <paramref name="logEventInfo"/> and compare result with <paramref name="expected"/>.
        /// </summary>
        protected static void AssertLayoutRendererOutput(Layout layout, LogEventInfo logEventInfo, string expected)
        {
            layout.Initialize(null);
            string actual = layout.Render(logEventInfo);
            layout.Close();
            Assert.Equal(expected, actual);
        }

#if !NET35 && !NET40
        /// <summary>
        /// Get line number of previous line.
        /// </summary>
        protected int GetPrevLineNumber([CallerLineNumber] int callingFileLineNumber = 0)
        {
            return callingFileLineNumber - 1;
        }
#else
        /// <summary>
        /// Get line number of previous line.
        /// </summary>
        protected int GetPrevLineNumber()
        {
            //fixed value set with #line 100000
            return 100001;
        }
#endif

        protected string RunAndCaptureInternalLog(SyncAction action, LogLevel internalLogLevel)
        {
            var stringWriter = new Logger();
            InternalLogger.LogWriter = stringWriter;
            InternalLogger.LogLevel = LogLevel.Trace;
            InternalLogger.IncludeTimestamp = false;
            action();

            return stringWriter.ToString();
        }
        /// <summary>
        /// To handle unstable integration tests, retry if failed
        /// </summary>
        /// <param name="tries"></param>
        /// <param name="action"></param>
        protected void RetryingIntegrationTest(int tries, Action action)
        {
            int tried = 0;
            while (tried < tries)
            {
                try
                {
                    tried++;
                    action();
                    return; //success
                }
                catch (Exception)
                {
                    if (tried >= tries)
                    {
                        throw;
                    }
                }

            }
        }

        /// <summary>
        /// This class has to be used when outputting from the InternalLogger.LogWriter.
        /// Just creating a string writer will cause issues, since string writer is not thread safe.
        /// This can cause issues when calling the ToString() on the text writer, since the underlying stringbuilder
        /// of the textwriter, has char arrays that gets fucked up by the multiple threads.
        /// this is a simple wrapper that just locks access to the writer so only one thread can access
        /// it at a time.
        /// </summary>
        private class Logger : TextWriter
        {
            private readonly StringWriter writer = new StringWriter();

            public override Encoding Encoding => writer.Encoding;

#if NETSTANDARD1_5
            public override void Write(char value)
            {
                lock (this.writer)
                {
                    this.writer.Write(value);
                }
            }
#endif

            public override void Write(string value)
            {
                lock (writer)
                {
                    writer.Write(value);
                }
            }

            public override void WriteLine(string value)
            {
                lock (writer)
                {
                    writer.WriteLine(value);
                }
            }

            public override string ToString()
            {
                lock (writer)
                {
                    return writer.ToString();
                }
            }
        }

        /// <summary>
        /// Creates <see cref="CultureInfo"/> instance for test purposes
        /// </summary>
        /// <param name="cultureName">Culture name to create</param>
        /// <remarks>
        /// Creates <see cref="CultureInfo"/> instance with non-userOverride
        /// flag to provide expected results when running tests in different
        /// system cultures(with overriden culture options)
        /// </remarks>
        protected static CultureInfo GetCultureInfo(string cultureName)
        {
            return new CultureInfo(cultureName, false);
        }

        /// <summary>
        /// Are we running on Linux environment or Windows environemtn ?
        /// </summary>
        /// <returns>true when something else than Windows</returns>
        protected static bool IsLinux()
        {
            return !NLog.Internal.PlatformDetector.IsWin32;
        }

        /// <summary>
        /// Are we running on AppVeyor?
        /// </summary>
        /// <returns></returns>
        protected static bool IsAppVeyor()
        {
            var val = Environment.GetEnvironmentVariable("APPVEYOR");
            return val != null && val.Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        public delegate void SyncAction();

        public class NoThrowNLogExceptions : IDisposable
        {
            private readonly bool throwExceptions;

            public NoThrowNLogExceptions()
            {
                throwExceptions = LogManager.ThrowExceptions;
                LogManager.ThrowExceptions = false;
            }

            public void Dispose()
            {
                LogManager.ThrowExceptions = throwExceptions;
            }
        }

        public class InternalLoggerScope : IDisposable
        {
            private readonly TextWriter oldConsoleOutputWriter;
            public StringWriter ConsoleOutputWriter { get; private set; }
            private readonly TextWriter oldConsoleErrorWriter;
            public StringWriter ConsoleErrorWriter { get; private set; }
            private readonly LogLevel globalThreshold;
            private readonly bool throwExceptions;
            private readonly bool? throwConfigExceptions;

            public InternalLoggerScope(bool redirectConsole = false)
            {
                InternalLogger.LogLevel = LogLevel.Info;

                if (redirectConsole)
                {
                    ConsoleOutputWriter = new StringWriter() { NewLine = "\n" };
                    ConsoleErrorWriter = new StringWriter() { NewLine = "\n" };

                    oldConsoleOutputWriter = Console.Out;
                    oldConsoleErrorWriter = Console.Error;

                    Console.SetOut(ConsoleOutputWriter);
                    Console.SetError(ConsoleErrorWriter);
                }

                globalThreshold = LogManager.GlobalThreshold;
                throwExceptions = LogManager.ThrowExceptions;
                throwConfigExceptions = LogManager.ThrowConfigExceptions;
            }

            public void SetConsoleError(StringWriter consoleErrorWriter)
            {
                if (ConsoleOutputWriter == null || consoleErrorWriter == null)
                    throw new InvalidOperationException("Initialize with redirectConsole=true");

                ConsoleErrorWriter = consoleErrorWriter;
                Console.SetError(consoleErrorWriter);
            }

            public void SetConsoleOutput(StringWriter consoleOutputWriter)
            {
                if (ConsoleOutputWriter == null || consoleOutputWriter == null)
                    throw new InvalidOperationException("Initialize with redirectConsole=true");

                ConsoleOutputWriter = consoleOutputWriter;
                Console.SetOut(consoleOutputWriter);
            }

            public void Dispose()
            {
                var logFile = InternalLogger.LogFile;

                InternalLogger.Reset();
                LogManager.GlobalThreshold = globalThreshold;
                LogManager.ThrowExceptions = throwExceptions;
                LogManager.ThrowConfigExceptions = throwConfigExceptions;

                if (ConsoleOutputWriter != null)
                    Console.SetOut(oldConsoleOutputWriter);
                if (ConsoleErrorWriter != null)
                    Console.SetError(oldConsoleErrorWriter);

                if (!string.IsNullOrEmpty(InternalLogger.LogFile))
                {
                    if (File.Exists(InternalLogger.LogFile))
                        File.Delete(InternalLogger.LogFile);
                }

                if (!string.IsNullOrEmpty(logFile) && logFile != InternalLogger.LogFile)
                {
                    if (File.Exists(logFile))
                        File.Delete(logFile);
                }
            }
        }

        protected static void AssertContainsInDictionary<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            Assert.Contains(key, dictionary);
            Assert.Equal(value, dictionary[key]);
        }
    }
}
