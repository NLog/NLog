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

using System.Security.Permissions;

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
#endif

    public abstract class NLogTestBase
    {
        public void AssertDebugCounter(string targetName, int val)
        {
            var debugTarget = (NLog.Targets.DebugTarget)LogManager.Configuration.FindTargetByName(targetName);

            Assert.NotNull(debugTarget);
            Assert.Equal(val, debugTarget.Counter);
        }

        public void AssertDebugLastMessage(string targetName, string msg)
        {
            NLog.Targets.DebugTarget debugTarget = (NLog.Targets.DebugTarget)LogManager.Configuration.FindTargetByName(targetName);

            Assert.NotNull(debugTarget);
            Assert.Equal(msg, debugTarget.LastMessage);
        }

        public void AssertDebugLastMessageContains(string targetName, string msg)
        {
            NLog.Targets.DebugTarget debugTarget = (NLog.Targets.DebugTarget)LogManager.Configuration.FindTargetByName(targetName);

            // Console.WriteLine("lastmsg: {0}", debugTarget.LastMessage);

            Assert.NotNull(debugTarget);
            Assert.True(debugTarget.LastMessage.Contains(msg), "Unexpected last message value on '" + targetName + "'");
        }

        public string GetDebugLastMessage(string targetName)
        {
            var debugTarget = (NLog.Targets.DebugTarget)LogManager.Configuration.FindTargetByName(targetName);
            return debugTarget.LastMessage;
        }

        public void AssertFileContentsStartsWith(string fileName, string contents, Encoding encoding)
        {
            FileInfo fi = new FileInfo(fileName);
            if (!fi.Exists)
                Assert.True(true, "File '" + fileName + "' doesn't exist.");

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

        public void AssertFileContents(string fileName, string contents, Encoding encoding)
        {
            FileInfo fi = new FileInfo(fileName);
            if (!fi.Exists)
                Assert.True(true, "File '" + fileName + "' doesn't exist.");

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

        public string StringRepeat(int times, string s)
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
            var oldWriter = InternalLogger.LogWriter;
            var oldLevel = InternalLogger.LogLevel;
            var oldIncludeTimestamp = InternalLogger.IncludeTimestamp;
            try
            {
                InternalLogger.LogWriter = stringWriter;
                InternalLogger.LogLevel = LogLevel.Trace;
                InternalLogger.IncludeTimestamp = false;
                action();

                return stringWriter.ToString();
            }
            finally
            {
                InternalLogger.LogWriter = oldWriter;
                InternalLogger.LogLevel = oldLevel;
                InternalLogger.IncludeTimestamp = oldIncludeTimestamp;
            }
        }

        public delegate void SyncAction();

        public class InternalLoggerScope : IDisposable
        {
            private readonly string logFile;
            private readonly LogLevel logLevel;
            private readonly bool logToConsole;
            private readonly bool includeTimestamp;
            private readonly bool logToConsoleError;
            private readonly LogLevel globalThreshold;
            private readonly bool throwExceptions;

            public InternalLoggerScope()
            {
                this.logFile = InternalLogger.LogFile;
                this.logLevel = InternalLogger.LogLevel;
                this.logToConsole = InternalLogger.LogToConsole;
                this.includeTimestamp = InternalLogger.IncludeTimestamp;
                this.logToConsoleError = InternalLogger.LogToConsoleError;
                this.globalThreshold = LogManager.GlobalThreshold;
                this.throwExceptions = LogManager.ThrowExceptions;
            }

            public void Dispose()
            {
                InternalLogger.LogFile = this.logFile;
                InternalLogger.LogLevel = this.logLevel;
                InternalLogger.LogToConsole = this.logToConsole;
                InternalLogger.IncludeTimestamp = this.includeTimestamp;
                InternalLogger.LogToConsoleError = this.logToConsoleError;
                LogManager.GlobalThreshold = this.globalThreshold;
                LogManager.ThrowExceptions = this.throwExceptions;
            }
        }
    }
}
