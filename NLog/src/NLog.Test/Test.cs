// 
// Copyright (c) 2004 Jaroslaw Kowalski <jkowalski@users.sourceforge.net>
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

using NLog;
using NLog.Config;

public class Test
{
    public static void LogProc(string msg)
    {
        Console.WriteLine("logproc: {0}", msg);
    }
    static void Main(string[]args)
    {
        Console.WriteLine("zzz");
        NLog.LogManager.Configuration = new XmlLoggingConfiguration("NLog.Test.exe.config");
        NLog.Logger l = NLog.LogManager.GetLogger("Aaa");
        NLog.Logger l2 = NLog.LogManager.GetLogger("Bbb");

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
        l.Error("this is an error");
        MDC.Remove("username");
        l.Fatal("this is a fatal");
        l2.Error("this is an error");
        l2.Fatal("this is a fatal");
    }
}
