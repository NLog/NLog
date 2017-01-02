// 
// Copyright (c) 2004-2016 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;

namespace NLog.UnitTests
{
    using System;
    using NLog.Common;
    using System.IO;
    using System.Text;
    using System.Globalization;
    using NLog.Layouts;
    using NLog.Config;
    using NLog.Targets;
    using Xunit;
    using System.Xml.Linq;
    using System.Xml;
    using System.IO.Compression;
    using System.Security.Permissions;
#if NET3_5 || NET4_0 || NET4_5
    using Ionic.Zip;
#endif

    public abstract class NLogTestBase
    {
        protected NLogTestBase()
        {
            //reset before every test
            if (LogManager.Configuration != null)
            {
                //flush all events if needed.
                LogManager.Configuration.Close();
            }

            if (LogManager.LogFactory != null)
            {
                LogManager.LogFactory.ResetCandidateConfigFilePath();
            }

            LogManager.Configuration = null;
            InternalLogger.Reset();
            LogManager.ThrowExceptions = false;
            LogManager.ThrowConfigExceptions = null;
#if !SILVERLIGHT
            System.Diagnostics.Trace.Listeners.Clear();
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
                string.Format("Expected to find '{0}' in last message value on '{1}', but found '{2}'", msg, targetName, debugLastMessage));
        }

        protected string GetDebugLastMessage(string targetName)
        {
            return GetDebugLastMessage(targetName, LogManager.Configuration);
        }

        protected string GetDebugLastMessage(string targetName, LoggingConfiguration configuration)
        {
            return GetDebugTarget(targetName, configuration).LastMessage;
        }

        public NLog.Targets.DebugTarget GetDebugTarget(string targetName)
        {
            return GetDebugTarget(targetName, LogManager.Configuration);
        }

        protected NLog.Targets.DebugTarget GetDebugTarget(string targetName, LoggingConfiguration configuration)
        {
            var debugTarget = (NLog.Targets.DebugTarget)configuration.FindTargetByName(targetName);
            Assert.NotNull(debugTarget);
            return debugTarget;
        }

        protected void AssertFileContentsStartsWith(string fileName, string contents, Encoding encoding)
        {
            FileInfo fi = new FileInfo(fileName);
            if (!fi.Exists)
                Assert.True(false, "File '" + fileName + "' doesn't exist.");

            byte[] encodedBuf = encoding.GetBytes(contents);

            byte[] buf = File.ReadAllBytes(fileName);
            Assert.True(encodedBuf.Length <= buf.Length, string.Format("File:{0} encodedBytes:{1} does not match file.content:{2}, file.length = {3}", fileName, encodedBuf.Length, buf.Length, fi.Length));

            for (int i = 0; i < encodedBuf.Length; ++i)
            {
                if (encodedBuf[i] != buf[i])
                    Assert.True(encodedBuf[i] == buf[i], string.Format("File:{0} content mismatch {1} <> {2} at index {3}", fileName, (int)encodedBuf[i], (int)buf[i], i));
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

        protected class CustomFileCompressor : IFileCompressor
        {
            public void CompressFile(string fileName, string archiveFileName)
            {
#if NET3_5 || NET4_0 || NET4_5
                using (ZipFile zip = new ZipFile())
                {
                    zip.AddFile(fileName);
                    zip.Save(archiveFileName);
                }
#endif
            }
        }

#if NET3_5 || NET4_0
        protected void AssertZipFileContents(string fileName, string contents, Encoding encoding)
        {
            if (!File.Exists(fileName))
                Assert.True(false, "File '" + fileName + "' doesn't exist.");

            byte[] encodedBuf = encoding.GetBytes(contents);
            
            using (var zip = new ZipFile(fileName))
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
#elif NET4_5
        protected void AssertZipFileContents(string fileName, string contents, Encoding encoding)
        {
            FileInfo fi = new FileInfo(fileName);
            if (!fi.Exists)
                Assert.True(false, "File '" + fileName + "' doesn't exist.");

            byte[] encodedBuf = encoding.GetBytes(contents);
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var zip = new ZipArchive(stream, ZipArchiveMode.Read))
            {
                Assert.Equal(1, zip.Entries.Count);
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
#else
        protected void AssertZipFileContents(string fileName, string contents, Encoding encoding)
        {
            Assert.True(false);
        }
#endif

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

            byte[] buf = File.ReadAllBytes(fileName);
            Assert.True(encodedBuf.Length == buf.Length, string.Format("File:{0} encodedBytes:{1} does not match file.content:{2}, file.length = {3}", fileName, encodedBuf.Length, buf.Length, fi.Length));

            for (int i = 0; i < buf.Length; ++i)
            {
                if (encodedBuf[i] != buf[i])
                    Assert.True(encodedBuf[i] == buf[i], string.Format("File:{0} content mismatch {1} <> {2} at index {3}", fileName, (int)encodedBuf[i], (int)buf[i], i));
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

#if MONO || NET4_5
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

        public static XmlLoggingConfiguration CreateConfigurationFromString(string configXml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(configXml);

            string currentDirectory = null;
            try
            {
                currentDirectory = Environment.CurrentDirectory;
            }
            catch (SecurityException)
            {
                //ignore   
            }



            return new XmlLoggingConfiguration(doc.DocumentElement, currentDirectory);
        }

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

            public override Encoding Encoding
            {
                get
                {
                    return this.writer.Encoding;
                }
            }

            public override void Write(string value)
            {
                lock (this.writer)
                {
                    this.writer.Write(value);
                }
            }

            public override void WriteLine(string value)
            {
                lock (this.writer)
                {
                    this.writer.WriteLine(value);
                }
            }

            public override string ToString()
            {
                lock (this.writer)
                {
                    return this.writer.ToString();
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

        public delegate void SyncAction();

        public class InternalLoggerScope : IDisposable
        {
            private readonly LogLevel globalThreshold;
            private readonly bool throwExceptions;
            private readonly bool? throwConfigExceptions;

            public InternalLoggerScope()
            {
                this.globalThreshold = LogManager.GlobalThreshold;
                this.throwExceptions = LogManager.ThrowExceptions;
                this.throwConfigExceptions = LogManager.ThrowConfigExceptions;
            }

            public void Dispose()
            {
                if (File.Exists(InternalLogger.LogFile))
                    File.Delete(InternalLogger.LogFile);

                InternalLogger.Reset();

                //restore logmanager
                LogManager.GlobalThreshold = this.globalThreshold;
                LogManager.ThrowExceptions = this.throwExceptions;
                LogManager.ThrowConfigExceptions = this.throwConfigExceptions;
            }
        }
    }
}
