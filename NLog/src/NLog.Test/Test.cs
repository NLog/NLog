// 
// Copyright (c) 2004,2005 Jaroslaw Kowalski <jkowalski@users.sourceforge.net>
// 
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
// * Neither the name of the Jaroslaw Kowalski nor the names of its 
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
            NLog.Internal.InternalLogger.LogToConsole = true;
            NLog.Internal.InternalLogger.LogLevel = LogLevel.Debug;

            NLog.Targets.ConsoleTarget t = new NLog.Targets.ConsoleTarget();
            t.Layout = "${windows-identity:domain=false}";

            SimpleConfigurator.ConfigureForTargetLogging(t, LogLevel.Trace);
            Logger p = LogManager.GetCurrentClassLogger();
            Logger p2 = LogManager.GetLogger("NLog.Tester.ABC");
            Logger p3 = LogManager.GetLogger("NLog.Def.ABC");
            MDC.Set("AAA", "b");
            MDC.Set("BBB", "C");
            using (NDC.Push("AAA"))
            {
                for (int i = 0; i < 3; ++i)
                {
                    p.Trace("This is a trace");
                    p.Debug("This is a debug");
                    p.Info("This is a info");
                    p.Warn("This is a warn");
                    p.Error("This is a error");
                    p.Fatal("This is a fatal");

                    p2.Trace("This is a trace");
                    p2.Debug("This is a debug");
                    p2.Info("This is a info");
                    p2.Warn("This is a warn");
                    p2.Error("This is a error");
                    p2.Fatal("This is a fatal");

                    p3.Trace("This is a trace");
                    p3.Debug("This is a debug");
                    p3.Info("This is a info");
                    p3.Warn("This is a warn");
                    p3.Error("This is a error");
                    p3.Fatal("This is a fatal");
                }
            }

            t.Flush(10000);

            return;

            
            //NLog.LogManager.Configuration = new XmlLoggingConfiguration("NLog.Test.exe.config");
            Logger l0 = LogManager.GetCurrentClassLogger();
            NLog.Logger l = NLog.LogManager.GetLogger("Aaa");
            NLog.Logger l2 = NLog.LogManager.GetLogger("Bbb");

            LogManager.GlobalThreshold = LogLevel.Debug;

            using(NDC.Push("aaa"))
            {
                l.Debug("this is a debug");
                l.Info("this is an info");
                MDC.Set("username", "jarek");

                l.Warn("this is a warning");
                using(NDC.Push("bbb"))
                {
                    l2.Debug("this is a debug");
                    using(NDC.Push("ccc"))
                    {
                        l2.Info("this is an info");
                    }
                }
                MDC.Set("username", "aaa");
                l2.Warn("this is a warning");
            }
            l.Error("this is an error {0}", 3);
            MDC.Remove("username");
            l.Fatal("this is a fatal");
            l2.Error("this is an error");
            l2.Fatal("this is a fatal");
            l0.Debug("Class logger!");

            Logger l3 = LogManager.GetLogger("ExceptionLogger");

            try
            {
                throw new ArgumentException("msg", "par");
            }
            catch (Exception ex)
            {
                l3.ErrorException("Exception occured", ex);
            }
        }
    }
}