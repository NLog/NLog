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
