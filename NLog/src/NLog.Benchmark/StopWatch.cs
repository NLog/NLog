using System;
using System.Threading;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Runtime.InteropServices;

public class StopWatch
{
    private long _startTime;
    private long _stopTime;
    private static long _overhead = 0;
    private static long _frequency;

    static StopWatch()
    {
        QueryPerformanceFrequency(out _frequency);
        StopWatch callibration = new StopWatch();
        long totalOverhead = 0;
        int loopCount = 0;
        for (int i = 0; i < 1000000; ++i)
        {
            callibration.Start();
            callibration.Stop();
            totalOverhead += callibration.Ticks;
            loopCount++;
        }
        _overhead = totalOverhead / loopCount;
        Console.WriteLine("Callibrating StopWatch: overhead {0}", _overhead);
    }
    
    public void Start()
    {
        QueryPerformanceCounter(out _startTime);
    }

    public void Stop()
    {
        QueryPerformanceCounter(out _stopTime);
    }

    public long Ticks
    {
        get { return _stopTime - _startTime - _overhead; }
    }

    public double Seconds
    {
        get { return (double)(_stopTime - _startTime - _overhead) / _frequency; }
    }

    public double Nanoseconds
    {
        get { return (double)1000000000 * (_stopTime - _startTime - _overhead) / _frequency; }
    }

    [DllImport("kernel32.dll")]
    static extern bool QueryPerformanceCounter(out long val);

    [DllImport("kernel32.dll")]
    static extern bool QueryPerformanceFrequency(out long val);
}
