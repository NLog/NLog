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
using NLog.Config;
using NLog.Targets;
using NLog.Layouts;

namespace NLog.Test
{
    public static class ExtensionMethods
    {

    }

    public class Test
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public static void LogProc(string msg)
        {
            Console.WriteLine("logproc: {0}", msg);
        }

        static void A()
        {
            B(3);
        }

        static void B(int a)
        {
            logger.Trace("ttt");
            logger.Debug("ala ma kota");
            logger.Info("ala ma kanarka");
            logger.Warn("aaa");
            logger.Error("err");
            logger.Fatal("fff");
        }

        static void Main(string[]args)
        {
            Target t;

            // t.WriteLogEvent()
            //ConsoleTarget ct = new ConsoleTarget();
            //ct.Layout = "${message} ${longdate} ${replace:searchFor=(..):regex=true:wholeWords=true:replaceWith=[xx'$1'yy]:inner=${rot13:inner=${message}:uppercase=true:padding=-10}}";

            //CsvLayout csv = new CsvLayout();
            //csv.Columns.Add(new CsvColumn("msg", "${message}"));
            //csv.Columns.Add(new CsvColumn("date", "${longdate}"));
            //csv.Columns.Add(new CsvColumn("level", "${level}"));
            //csv.WithHeader = true;
            ////ct.Layout = csv;

            //SimpleConfigurator.ConfigureForTargetLogging(ct);

            //InternalLogger.LogToConsole = true;
            //InternalLogger.LogLevel = LogLevel.Debug;

            logger.Debug(() => "foo bar");

            for (int i = 0; i < 3; ++i)
            {
                logger.Trace("ttt");
                logger.Debug("ala ma kota {0}", i);
                logger.Info("ala ma kanarka");
                logger.Warn("aaa");
                logger.Error("err");
                logger.Fatal("fff");
            }
        }
    }
}