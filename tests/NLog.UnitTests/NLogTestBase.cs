// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
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

using System.Runtime.CompilerServices;

namespace NLog.UnitTests
{
    using System;
    using NLog.Common;
    using System.IO;
    using System.Text;

    using NLog.Layouts;
    using NLog.Config;
    using Xunit;
#if SILVERLIGHT
    using System.Xml.Linq;
#else
    using System.Xml;
    using System.IO.Compression;
    using System.Security.Permissions;
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
            LogManager.Configuration = null;
            InternalLogger.Reset();
            LogManager.ThrowExceptions = false;
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
            Assert.True(encodedBuf.Length <= fi.Length);
            byte[] buf = new byte[encodedBuf.Length];
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                fs.Read(buf, 0, buf.Length);
            }

            for (int i = 0; i < buf.Length; ++i)
            {
                Assert.Equal(encodedBuf[i], buf[i]);
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

#if NET4_5
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
#endif

        protected void AssertFileContents(string fileName, string contents, Encoding encoding)
        {
            FileInfo fi = new FileInfo(fileName);
            if (!fi.Exists)
                Assert.True(false, "File '" + fileName + "' doesn't exist.");

            byte[] encodedBuf = encoding.GetBytes(contents);
            Assert.Equal(encodedBuf.Length, fi.Length);
            byte[] buf = new byte[(int)fi.Length];
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                fs.Read(buf, 0, buf.Length);
            }

            for (int i = 0; i < buf.Length; ++i)
            {
                Assert.Equal(encodedBuf[i], buf[i]);
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

        protected void AssertLayoutRendererOutput(Layout l, string expected)
        {
            l.Initialize(null);
            string actual = l.Render(LogEventInfo.Create(LogLevel.Info, "loggername", "message"));
            l.Close();
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

        protected XmlLoggingConfiguration CreateConfigurationFromString(string configXml)
        {
#if SILVERLIGHT
            XElement element = XElement.Parse(configXml);
            return new XmlLoggingConfiguration(element.CreateReader(), null);
#else
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(configXml);

            return new XmlLoggingConfiguration(doc.DocumentElement, Environment.CurrentDirectory);
#endif
        }

        protected string RunAndCaptureInternalLog(SyncAction action, LogLevel internalLogLevel)
        {
            var stringWriter = new StringWriter();
            InternalLogger.LogWriter = stringWriter;
            InternalLogger.LogLevel = LogLevel.Trace;
            InternalLogger.IncludeTimestamp = false;
            action();

            return stringWriter.ToString();
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
