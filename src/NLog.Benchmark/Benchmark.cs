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
using System.Threading;
using System.Globalization;
using NLog;

class Bench
{
    private const int warmup = 10;
    private static Logger logger = LogManager.GetLogger("Logger1");
    private static Logger logger2 = LogManager.GetLogger("Logger2");

    public static void Main(string[]args)
    {
        int repeat = 1000000;
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        if (args.Length > 0)
        {
            if (args[0] == "long")
                repeat *= 10;
        }
        Console.WriteLine("warming up");
        for (int i = 0; i < warmup; ++i)
        {
            logger.Debug("Warm up");
            logger2.Debug("Warm up");
            logger.Info("Warm up");
            logger2.Info("Warm up");
        }
        NoLogTest(repeat * 10);
        NullLogTest(repeat);

        /*

        using (LogManager.DisableLogging()) {
        NoLogTest(repeat * 10);
        NullLogTest(repeat);
        }
         */
    }

    private static void NoLogTest(int repeat)
    {
        Console.WriteLine("Starting not-logging test");
        DateTime dt0 = DateTime.Now;
        for (int i = 0; i < repeat; ++i)
        {
            logger.Debug("This is a simple message with no parameters");
        }
        DateTime dt1 = DateTime.Now;
        for (int i = 0; i < repeat; ++i)
        {
            logger.Debug("This is a simple message with {0} parameter", 1);
        }
        DateTime dt2 = DateTime.Now;
        for (int i = 0; i < repeat; ++i)
        {
            logger.Debug("This is a simple message with {0}{1} parameters", 2, "o");
        }
        DateTime dt3 = DateTime.Now;
        for (int i = 0; i < repeat; ++i)
        {
            logger.Debug("This is a simple message with {0}{1}{2} parameters", "thr", 3, 3);
        }
        DateTime dt4 = DateTime.Now;
        for (int i = 0; i < repeat; ++i)
        {
            if (logger.IsDebugEnabled)
            {
                logger.Debug("This is a simple message with no parameters");
            }
        }
        DateTime dt5 = DateTime.Now;
        for (int i = 0; i < repeat; ++i)
        {
            if (logger.IsDebugEnabled)
            {
                logger.Debug("This is a simple message with {0} parameter", 1);
            }
        }
        DateTime dt6 = DateTime.Now;
        for (int i = 0; i < repeat; ++i)
        {
            if (logger.IsDebugEnabled)
            {
                logger.Debug("This is a simple message with {0}{1} parameters", 2, "o");
            }
        }
        DateTime dt7 = DateTime.Now;
        for (int i = 0; i < repeat; ++i)
        {
            if (logger.IsDebugEnabled)
            {
                logger.Debug("This is a simple message with {0}{1}{2} parameters", "thr", 3, 3);
            }
        }
        DateTime dt8 = DateTime.Now;

        Console.WriteLine("no guard - no parameters:             {0} nanoseconds", 100.0 * (dt1 - dt0).Ticks / (double)repeat);
        Console.WriteLine("no guard - a single paramter:         {0} nanoseconds", 100.0 * (dt2 - dt1).Ticks / (double)repeat);
        Console.WriteLine("no guard - two paramters:             {0} nanoseconds", 100.0 * (dt3 - dt2).Ticks / (double)repeat);
        Console.WriteLine("no guard - three paramters:           {0} nanoseconds", 100.0 * (dt4 - dt3).Ticks / (double)repeat);
        Console.WriteLine("with a guard - no parameters:         {0} nanoseconds", 100.0 * (dt5 - dt4).Ticks / (double)repeat);
        Console.WriteLine("with a guard - a single paramter:     {0} nanoseconds", 100.0 * (dt6 - dt5).Ticks / (double)repeat);
        Console.WriteLine("with a guard - two paramters:         {0} nanoseconds", 100.0 * (dt7 - dt6).Ticks / (double)repeat);
        Console.WriteLine("with a guard - three paramters:       {0} nanoseconds", 100.0 * (dt8 - dt7).Ticks / (double)repeat);
    }

    private static void NullLogTest(int repeat)
    {
        Console.WriteLine("Starting null logging test");
        DateTime dt0 = DateTime.Now;
        for (int i = 0; i < repeat; ++i)
        {
            logger.Info("This is a simple message with no parameters");
        }
        DateTime dt1 = DateTime.Now;
        for (int i = 0; i < repeat; ++i)
        {
            logger.Info("This is a simple message with {0} parameter", 1);
        }
        DateTime dt2 = DateTime.Now;
        for (int i = 0; i < repeat; ++i)
        {
            logger.Info("This is a simple message with {0}{1} parameters", 2, "o");
        }
        DateTime dt3 = DateTime.Now;
        for (int i = 0; i < repeat; ++i)
        {
            logger.Info("This is a simple message with {0}{1}{2} parameters", "thr", 3, 3);
        }
        DateTime dt4 = DateTime.Now;
        for (int i = 0; i < repeat; ++i)
        {
            if (logger.IsInfoEnabled)
            {
                logger.Info("This is a simple message with no parameters");
            }
        }
        DateTime dt5 = DateTime.Now;
        for (int i = 0; i < repeat; ++i)
        {
            if (logger.IsInfoEnabled)
            {
                logger.Info("This is a simple message with {0} parameter", 1);
            }
        }
        DateTime dt6 = DateTime.Now;
        for (int i = 0; i < repeat; ++i)
        {
            if (logger.IsInfoEnabled)
            {
                logger.Info("This is a simple message with {0}{1} parameters", 2, "o");
            }
        }
        DateTime dt7 = DateTime.Now;
        for (int i = 0; i < repeat; ++i)
        {
            if (logger.IsInfoEnabled)
            {
                logger.Info("This is a simple message with {0}{1}{2} parameters", "thr", 3, 3);
            }
        }
        DateTime dt8 = DateTime.Now;

        Console.WriteLine("no guard - no parameters:             {0} nanoseconds", 100.0 * (dt1 - dt0).Ticks / (double)repeat);
        Console.WriteLine("no guard - a single paramter:         {0} nanoseconds", 100.0 * (dt2 - dt1).Ticks / (double)repeat);
        Console.WriteLine("no guard - two paramters:             {0} nanoseconds", 100.0 * (dt3 - dt2).Ticks / (double)repeat);
        Console.WriteLine("no guard - three paramters:           {0} nanoseconds", 100.0 * (dt4 - dt3).Ticks / (double)repeat);
        Console.WriteLine("with a guard - no parameters:         {0} nanoseconds", 100.0 * (dt5 - dt4).Ticks / (double)repeat);
        Console.WriteLine("with a guard - a single paramter:     {0} nanoseconds", 100.0 * (dt6 - dt5).Ticks / (double)repeat);
        Console.WriteLine("with a guard - two paramters:         {0} nanoseconds", 100.0 * (dt7 - dt6).Ticks / (double)repeat);
        Console.WriteLine("with a guard - three paramters:       {0} nanoseconds", 100.0 * (dt8 - dt7).Ticks / (double)repeat);
    }
}
