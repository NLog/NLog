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



namespace NLog.UnitTests
{
    using System;
    using NLog.Common;
    using System.IO;
    using System.Text;

    using NLog.Layouts;
    using NLog.Config;
    using Xunit;
    using System.Reflection;
    using System.Collections.Generic;
    using System.Linq;
 
#if SILVERLIGHT
    using System.Xml.Linq;
#else
    using System.Xml;
    using System.IO.Compression;
    using System.Security.Permissions;
#endif

    public abstract class NLogTestBase
    {

        private static IDictionary<string, object> _internalLoggerProperties;
        private static IDictionary<string, object> _logmanagerProperties;

        protected NLogTestBase()
        {
            InternalLogger.LogToConsole = false;
            InternalLogger.LogToConsoleError = false;

            LogManager.ThrowExceptions = false;

            //remember the static values, so we can alter them in tests and restore them at the end.
            _internalLoggerProperties = GeetStaticProperyValues(typeof(InternalLogger));
            _logmanagerProperties = GeetStaticProperyValues(typeof(LogManager));
        }

        /// <summary>
        /// Restore the static propertie values of the <see cref="LogManager"/> and <see cref="InternalLogger"/>
        /// </summary>
        protected static void RestoreStaticPropertyValues()
        {
            SetStaticProperyValues(typeof(InternalLogger), _internalLoggerProperties);
            SetStaticProperyValues(typeof(LogManager), _logmanagerProperties);
        }

        /// <summary>
        /// Get the values of the static properties
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private static IDictionary<string, object> GeetStaticProperyValues(Type t)
        {
            var staticPropertyInfos = GetReadableAndWritableStaticPropertyInfos(t);
            return staticPropertyInfos.Select(p => new KeyValuePair<string, object>(p.Name, p.GetValue(t, null))).ToDictionary(p => p.Key, p => p.Value);
        }

        /// <summary>
        /// Set the values of the static properties
        /// </summary>
        private static void SetStaticProperyValues(Type t, IDictionary<string, object> values)
        {
            var staticPropertyInfos = GetReadableAndWritableStaticPropertyInfos(t);
            foreach (var p in staticPropertyInfos)
            {
                p.SetValue(t, values[p.Name], null);
            }
        }

        /// <summary>
        /// Get the static properties we can read and write
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private static IEnumerable<PropertyInfo> GetReadableAndWritableStaticPropertyInfos(Type t)
        {
            //bindingflags don't give any results, so checking the properties
            return t.GetProperties().Where(p => p.CanRead && p.CanWrite);
        }

        public void AssertDebugCounter(string targetName, int val)
        {
            Assert.Equal(val, GetDebugTarget(targetName).Counter);
        }

        public void AssertDebugLastMessage(string targetName, string msg)
        {
            Assert.Equal(msg, GetDebugLastMessage(targetName));
        }


        public void AssertDebugLastMessageContains(string targetName, string msg)
        {
            string debugLastMessage = GetDebugLastMessage(targetName);
            Assert.True(debugLastMessage.Contains(msg),
                string.Format("Expected to find '{0}' in last message value on '{1}', but found '{2}'", msg, targetName, debugLastMessage));
        }

        public string GetDebugLastMessage(string targetName)
        {
            return GetDebugLastMessage(targetName, LogManager.Configuration);
        }

        public string GetDebugLastMessage(string targetName, LoggingConfiguration configuration)
        {
            return GetDebugTarget(targetName, configuration).LastMessage;
        }

        public NLog.Targets.DebugTarget GetDebugTarget(string targetName)
        {
            return GetDebugTarget(targetName, LogManager.Configuration);
        }

        public NLog.Targets.DebugTarget GetDebugTarget(string targetName, LoggingConfiguration configuration)
        {
            var debugTarget = (NLog.Targets.DebugTarget)configuration.FindTargetByName(targetName);
            Assert.NotNull(debugTarget);
            return debugTarget;
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

        public void AssertFileSize(string filename, long expectedSize)
        {
            var fi = new FileInfo(filename);

            if (!fi.Exists)
            {
                Assert.True(true, string.Format("File \"{0}\" doesn't exist.", filename));
            }

            if (fi.Length != expectedSize)
            {
                Assert.True(true, string.Format("Filesize of \"{0}\" unequals {1}.", filename, expectedSize));
            }
        }

#if NET4_5
        public void AssertZipFileContents(string fileName, string contents, Encoding encoding)
        {
            FileInfo fi = new FileInfo(fileName);
            if (!fi.Exists)
                Assert.True(true, "File '" + fileName + "' doesn't exist.");

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
                RestoreStaticPropertyValues();
            }
        }

        public delegate void SyncAction();

        /// <summary>
        /// Restore the static properties of the <see cref="InternalLogger"/> and <see cref="LogManager"/> in <see cref="Dispose"/>
        /// </summary>
        public class InternalLoggerScope : IDisposable
        {


            public InternalLoggerScope()
            {

            }

            public void Dispose()
            {
                RestoreStaticPropertyValues();
            }
        }
    }
}
