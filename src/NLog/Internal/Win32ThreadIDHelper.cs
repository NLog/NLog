#if !SILVERLIGHT

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace NLog.Internal
{
    /// <summary>
    /// Win32-optimized implementation of <see cref="ThreadIDHelper"/>.
    /// </summary>
    internal class Win32ThreadIDHelper : ThreadIDHelper
    {
        private int currentProcessID;
        private string currentProcessName;
        private string currentProcessBaseName;

        /// <summary>
        /// Initializes a new instance of the Win32ThreadIDHelper class.
        /// </summary>
        public Win32ThreadIDHelper()
        {
            this.currentProcessID = GetCurrentProcessId();

            StringBuilder sb = new StringBuilder(512);
            GetModuleFileName(IntPtr.Zero, sb, sb.Capacity);
            this.currentProcessName = sb.ToString();

            this.currentProcessBaseName = Path.GetFileNameWithoutExtension(this.currentProcessName);
        }

        /// <summary>
        /// Gets current thread ID.
        /// </summary>
        /// <value></value>
        public override int CurrentThreadID
        {
            get { return System.Threading.Thread.CurrentThread.ManagedThreadId; }
        }

        /// <summary>
        /// Gets current process ID.
        /// </summary>
        /// <value></value>
        public override int CurrentProcessID
        {
            get { return this.currentProcessID; }
        }

        /// <summary>
        /// Gets current process name.
        /// </summary>
        /// <value></value>
        public override string CurrentProcessName
        {
            get { return this.currentProcessName; }
        }

        /// <summary>
        /// Gets current process name (excluding filename extension, if any).
        /// </summary>
        /// <value></value>
        public override string CurrentProcessBaseName
        {
            get { return this.currentProcessBaseName; }
        }

#if !NET_CF
        [DllImport("kernel32.dll")]
        private static extern int GetCurrentProcessId();

        [DllImport("kernel32.dll", SetLastError = true, PreserveSig = true)]
        private static extern uint GetModuleFileName([In] IntPtr hModule, [Out] StringBuilder lpFilename, [In] [MarshalAs(UnmanagedType.U4)] int nSize);
#else
        [DllImport("coredll.dll")]
        private static extern int GetCurrentProcessId();

        [DllImport("coredll.dll", SetLastError=true)]
        private static extern uint GetModuleFileName([In] IntPtr hModule, [Out] StringBuilder lpFilename, [In] int nSize);
#endif
    }
}

#endif