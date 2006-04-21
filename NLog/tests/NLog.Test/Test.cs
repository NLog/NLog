// 
// Copyright (c) 2004-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
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
using System.Runtime.InteropServices;

using NLog;
using NLog.Config;
using NLog.Targets.Compound;
using NLog.Targets.Wrappers;
using NLog.Conditions;
using NLog.Targets;
using NLog.Win32.Targets;

namespace NLog.Tester
{
    public class Test
    {
        public static void LogProc(string msg)
        {
            Console.WriteLine("logproc: {0}", msg);
        }

        static void Main(string[]args)
        {
            Internal.InternalLogger.LogToConsole = true;
            Internal.InternalLogger.LogLevel = LogLevel.Trace;
            StopWatch sw;


            System.Threading.Thread.CurrentThread.Name = "threadNameIsHere";

            Logger p = LogManager.GetCurrentClassLogger();
            GDC.Set("GGG", "b");
            MDC.Set("AAA", "b");
            MDC.Set("BBB", "C");

            sw = new StopWatch();
            sw.Start();
            for (int i = 0; i < 2; ++i)
            {
                p.Trace("trace {0} ala ma kota", i);
                p.Debug("debug {0} ala ma ma쿮go ma쿮go ma쿮go ma쿮go ma쿮go ma쿮go ma쿮go ma쿮go ma쿮go ma쿮go ma쿮go ma쿮go ma쿮go ma쿮go ma쿮go ma쿮go ma쿮go ma쿮go ma쿮go ma쿮go ma쿮go ma쿮go ma쿮go ma쿮go ma쿮go ma쿮go ma쿮go ma쿮go ma쿮go ma쿮go ma쿮go ma쿮go ma쿮go ma쿮go ma쿮go ma쿮go ma쿮go ma쿮go ma쿮go ma쿮go ma쿮go ma쿮go ma쿮go ma쿮go ma쿮go ma쿮go ma쿮go ma쿮go ma쿮go ma쿮go ma쿮go ma쿮go ma쿮go ma쿮go ma쿮go ma쿮go kota i niewielkiego psa", i);
                p.Info("info {0}", i);
                p.Warn("warn {0}", i);
                p.Error("error {0}", i);
                p.Fatal("fatal {0}", i);
            }
            sw.Stop();
            Console.WriteLine("t: {0}", sw.Seconds);
            return;
        }
    }
}
