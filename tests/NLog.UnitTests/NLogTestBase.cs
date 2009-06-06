// 
// Copyright (c) 2004-2009 Jaroslaw Kowalski <jaak@jkowalski.net>
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

using System;

using NUnit.Framework;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;
using NLog.Layouts;

namespace NLog.UnitTests
{
	public class NLogTestBase
	{
        public void AssertDebugCounter(string targetName, int val)
        {
            NLog.Targets.DebugTarget debugTarget = (NLog.Targets.DebugTarget)LogManager.Configuration.FindTargetByName(targetName);

            Assert.IsNotNull(debugTarget, "Debug target '" + targetName + "' not found");
            Assert.AreEqual(val, debugTarget.Counter, "Unexpected counter value on '" + targetName + "'");
        }

        public void AssertDebugLastMessage(string targetName, string msg)
        {
            NLog.Targets.DebugTarget debugTarget = (NLog.Targets.DebugTarget)LogManager.Configuration.FindTargetByName(targetName);

            // Console.WriteLine("lastmsg: {0}", debugTarget.LastMessage);

            Assert.IsNotNull(debugTarget, "Debug target '" + targetName + "' not found");
            Assert.AreEqual(msg, debugTarget.LastMessage, "Unexpected last message value on '" + targetName + "'");
        }

        public string GetDebugLastMessage(string targetName)
        {
            NLog.Targets.DebugTarget debugTarget = (NLog.Targets.DebugTarget)LogManager.Configuration.FindTargetByName(targetName);
            return debugTarget.LastMessage;
        }

        public void AssertFileContents(string fileName, string contents, Encoding encoding)
        {
            FileInfo fi = new FileInfo(fileName);
            if (!fi.Exists)
                Assert.Fail("File '" + fileName + "' doesn't exist.");

            byte[] encodedBuf = encoding.GetBytes(contents);
            Assert.AreEqual(encodedBuf.Length, fi.Length, "File length is incorrect.");
            byte[] buf = new byte[(int)fi.Length];
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                fs.Read(buf, 0, buf.Length);
            }

            for (int i = 0; i < buf.Length; ++i)
            {
                Assert.AreEqual(encodedBuf[i], buf[i], "File contents are different at position: #" + i);
            }
        }

        public string StringRepeat(int times, string s)
        {
            StringBuilder sb = new StringBuilder(s.Length * times);
            for (int i = 0; i < times; ++i)
                sb.Append(s);
            return sb.ToString();
        }

        protected Process SpawnMethod(string methodName, params string[] p)
        {
            string assemblyName = this.GetType().Assembly.FullName;
            string typename = this.GetType().FullName;
            StringBuilder sb = new StringBuilder();
#if MONO
            sb.Append("Runner.exe ");
#endif
            sb.AppendFormat("\"{0}\" \"{1}\" \"{2}\"", assemblyName, typename, methodName);
            foreach (string s in p)
            {
                sb.Append(" ");
                sb.Append("\"");
                sb.Append(s);
                sb.Append("\"");
            }

            Process proc = new Process();
            proc.StartInfo.Arguments = sb.ToString();
#if MONO
            proc.StartInfo.FileName = "mono";
#else
            proc.StartInfo.FileName = "Runner.exe";
#endif
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            proc.StartInfo.RedirectStandardInput = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            proc.StartInfo.CreateNoWindow = true;
            proc.Start();
            return proc;
        }

        protected void AssertLayoutRendererOutput(Layout l, string expected)
        {
            string actual = l.GetFormattedMessage(LogEventInfo.Create(LogLevel.Info, "loggername", "message"));
            Assert.AreEqual(expected, actual);
        }
    }
}
