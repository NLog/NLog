#if !SILVERLIGHT && !NET_CF

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using NLog.Config;

namespace NLog.Internal
{
    /// <summary>
    /// Portable implementation of <see cref="ThreadIDHelper"/>.
    /// </summary>
    internal class PortableThreadIDHelper : ThreadIDHelper
    {
        private int currentProcessID;
        private string currentProcessName;
        private string currentProcessBaseName;
        private string currentProcessDirectoryName;

        /// <summary>
        /// Initializes a new instance of the PortableThreadIDHelper class.
        /// </summary>
        public PortableThreadIDHelper()
        {
            this.currentProcessID = System.Diagnostics.Process.GetCurrentProcess().Id;
            this.currentProcessName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            this.currentProcessBaseName = Path.GetFileNameWithoutExtension(this.currentProcessName);
            this.currentProcessDirectoryName = Path.GetDirectoryName(this.currentProcessName);
        }

        /// <summary>
        /// Gets current thread ID.
        /// </summary>
        /// <value></value>
        public override int CurrentThreadID
        {
            get
            {
                return System.Threading.Thread.CurrentThread.ManagedThreadId;
            }
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
    }
}

#endif