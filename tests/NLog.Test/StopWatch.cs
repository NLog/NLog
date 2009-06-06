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
using System.Threading;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

public class StopWatch
{
    private long startTime;
    private long stopTime;
    private static long overhead = 0;
    private static long frequency;

    static StopWatch()
    {
        QueryPerformanceFrequency(out frequency);
        StopWatch callibration = new StopWatch();
        long totalOverhead = 0;
        int loopCount = 0;
        for (int i = 0; i < 10000; ++i)
        {
            callibration.Start();
            callibration.Stop();
            totalOverhead += callibration.Ticks;
            loopCount++;
        }
        overhead = totalOverhead / loopCount;
        //Console.WriteLine("Callibrating StopWatch: overhead {0}", overhead);
    }
    
    public void Start()
    {
        QueryPerformanceCounter(out this.startTime);
    }

    public void Stop()
    {
        QueryPerformanceCounter(out this.stopTime);
    }

    public long Ticks
    {
        get { return this.stopTime - this.startTime - overhead; }
    }

    public double Seconds
    {
        get { return (double)(this.stopTime - this.startTime - overhead) / frequency; }
    }

    public double Nanoseconds
    {
        get { return (double)1000000000 * (this.stopTime - this.startTime - overhead) / frequency; }
    }

    [DllImport("kernel32.dll")]
    static extern bool QueryPerformanceCounter(out long val);

    [DllImport("kernel32.dll")]
    static extern bool QueryPerformanceFrequency(out long val);
}
