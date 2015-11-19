using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace NLogPerfTest
{
    public class CpuUsage
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetSystemTimes(out System.Runtime.InteropServices.ComTypes.FILETIME lpIdleTime, out System.Runtime.InteropServices.ComTypes.FILETIME lpKernelTime, out System.Runtime.InteropServices.ComTypes.FILETIME lpUserTime);

        System.Runtime.InteropServices.ComTypes.FILETIME _prevSysKernel;
        System.Runtime.InteropServices.ComTypes.FILETIME _prevSysUser;
        TimeSpan _prevProcTotal;
        double _cpuUsage;
        DateTime _lastRun;
        long _runCount;

        public CpuUsage()
        {
            _cpuUsage = -1;
            _lastRun = DateTime.MinValue;
            _prevSysUser.dwHighDateTime = _prevSysUser.dwLowDateTime = 0;
            _prevSysKernel.dwHighDateTime = _prevSysKernel.dwLowDateTime = 0;
            _prevProcTotal = TimeSpan.MinValue;
            _runCount = 0;
        }

        public double GetUsage()
        {
            double cpuCopy = _cpuUsage;

            if (Interlocked.Increment(ref _runCount) == 1)
            {

                if (!EnoughTimePassed)
                {
                    Interlocked.Decrement(ref _runCount);
                    return cpuCopy;
                }
                System.Runtime.InteropServices.ComTypes.FILETIME sysIdle, sysKernel, sysUser;
                TimeSpan procTime;
                Process process = Process.GetCurrentProcess();
                procTime = process.TotalProcessorTime;

                if (!GetSystemTimes(out sysIdle, out sysKernel, out sysUser))
                {
                    Interlocked.Decrement(ref _runCount);
                    return cpuCopy;
                }

                if (!IsFirstRun)
                {
                    UInt64 sysKernelDiff = SubtractTimes(sysKernel, _prevSysKernel);
                    UInt64 sysUserDiff = SubtractTimes(sysUser, _prevSysUser);
                    UInt64 sysTotal = sysKernelDiff + sysUserDiff;
                    Int64 procTotal = procTime.Ticks - _prevProcTotal.Ticks;

                    if (sysTotal > 0)
                    {
                        _cpuUsage = ((100.0 * procTotal) / sysTotal);
                    }
                }
                _prevProcTotal = procTime;
                _prevSysKernel = sysKernel;
                _prevSysUser = sysUser;
                _lastRun = DateTime.Now;
                cpuCopy = _cpuUsage;
            }
            Interlocked.Decrement(ref _runCount);
            return cpuCopy;

        }

        private UInt64 SubtractTimes(System.Runtime.InteropServices.ComTypes.FILETIME a, System.Runtime.InteropServices.ComTypes.FILETIME b)
        {
            UInt64 aInt = ((UInt64)(a.dwHighDateTime << 32)) | (UInt64)a.dwLowDateTime;
            UInt64 bInt = ((UInt64)(b.dwHighDateTime << 32)) | (UInt64)b.dwLowDateTime;
            return aInt - bInt;
        }

        private bool EnoughTimePassed
        {
            get
            {
                const int minimumElapsedMS = 250;
                TimeSpan sinceLast = DateTime.Now - _lastRun;
                return sinceLast.TotalMilliseconds > minimumElapsedMS;
            }
        }

        private bool IsFirstRun
        {
            get
            {
                return (_lastRun == DateTime.MinValue);
            }
        }
    }
   
    public class RunnerClass
    {
        public static void Main1()
        {
            CpuUsage cpu = new CpuUsage();
            double usage = cpu.GetUsage();
            Console.WriteLine(usage.ToString());
        }
    }
}
