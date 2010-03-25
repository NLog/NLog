// 
// Copyright (c) 2004-2010 Jaroslaw Kowalski <jaak@jkowalski.net>
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

#if !NET_CF && !MONO && !SILVERLIGHT

namespace NLog.LayoutRenderers
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Text;
    using NLog.Config;

    /// <summary>
    /// The information about the running process.
    /// </summary>
    [LayoutRenderer("processinfo")]
    public class ProcessInfoLayoutRenderer : LayoutRenderer
    {
        private Process process;

        private PropertyInfo propertyInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessInfoLayoutRenderer" /> class.
        /// </summary>
        public ProcessInfoLayoutRenderer()
        {
            this.Property = ProcessInfoProperty.Id;
        }

        /// <summary>
        /// Property of System.Diagnostics.Process to retrieve.
        /// </summary>
        public enum ProcessInfoProperty
        {
            /// <summary>
            /// Base Priority.
            /// </summary>
            BasePriority,

            /// <summary>
            /// Exit Code.
            /// </summary>
            ExitCode,

            /// <summary>
            /// Exit Time.
            /// </summary>
            ExitTime,

            /// <summary>
            /// Process Handle.
            /// </summary>
            Handle,

            /// <summary>
            /// Handle Count.
            /// </summary>
            HandleCount,

            /// <summary>
            /// Whether process has exited.
            /// </summary>
            HasExited,

            /// <summary>
            /// Process ID.
            /// </summary>
            Id,

            /// <summary>
            /// Machine name.
            /// </summary>
            MachineName,

            /// <summary>
            /// Handle of the main window.
            /// </summary>
            MainWindowHandle,

            /// <summary>
            /// Title of the main window.
            /// </summary>
            MainWindowTitle,

            /// <summary>
            /// Maximum Working Set.
            /// </summary>
            MaxWorkingSet,

            /// <summary>
            /// Minimum Working Set.
            /// </summary>
            MinWorkingSet,

            /// <summary>
            /// Non-paged System Memory Size.
            /// </summary>
            NonpagedSystemMemorySize,

            /// <summary>
            /// Non-paged System Memory Size (64-bit).
            /// </summary>
            NonpagedSystemMemorySize64,

            /// <summary>
            /// Paged Memory Size.
            /// </summary>
            PagedMemorySize,

            /// <summary>
            /// Paged Memory Size (64-bit)..
            /// </summary>
            PagedMemorySize64,

            /// <summary>
            /// Paged System Memory Size.
            /// </summary>
            PagedSystemMemorySize,

            /// <summary>
            /// Paged System Memory Size (64-bit).
            /// </summary>
            PagedSystemMemorySize64,

            /// <summary>
            /// Peak Paged Memory Size.
            /// </summary>
            PeakPagedMemorySize,

            /// <summary>
            /// Peak Paged Memory Size (64-bit).
            /// </summary>
            PeakPagedMemorySize64,

            /// <summary>
            /// Peak Vitual Memory Size.
            /// </summary>
            PeakVirtualMemorySize,

            /// <summary>
            /// Peak Virtual Memory Size (64-bit)..
            /// </summary>
            PeakVirtualMemorySize64,

            /// <summary>
            /// Peak Working Set Size.
            /// </summary>
            PeakWorkingSet,

            /// <summary>
            /// Peak Working Set Size (64-bit).
            /// </summary>
            PeakWorkingSet64,

            /// <summary>
            /// Whether priority boost is enabled.
            /// </summary>
            PriorityBoostEnabled,

            /// <summary>
            /// Priority Class.
            /// </summary>
            PriorityClass,

            /// <summary>
            /// Private Memory Size.
            /// </summary>
            PrivateMemorySize,

            /// <summary>
            /// Private Memory Size (64-bit).
            /// </summary>
            PrivateMemorySize64,

            /// <summary>
            /// Privileged Processor Time.
            /// </summary>
            PrivilegedProcessorTime,

            /// <summary>
            /// Process Name.
            /// </summary>
            ProcessName,

            /// <summary>
            /// Whether process is responding.
            /// </summary>
            Responding,

            /// <summary>
            /// Session ID.
            /// </summary>
            SessionId,

            /// <summary>
            /// Process Start Time.
            /// </summary>
            StartTime,

            /// <summary>
            /// Total Processor Time.
            /// </summary>
            TotalProcessorTime,

            /// <summary>
            /// User Processor Time.
            /// </summary>
            UserProcessorTime,

            /// <summary>
            /// Virtual Memory Size.
            /// </summary>
            VirtualMemorySize,

            /// <summary>
            /// Virtual Memory Size (64-bit).
            /// </summary>
            VirtualMemorySize64,

            /// <summary>
            /// Working Set Size.
            /// </summary>
            WorkingSet,

            /// <summary>
            /// Working Set Size (64-bit).
            /// </summary>
            WorkingSet64,
        }

        /// <summary>
        /// Gets or sets the property to retrieve.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultValue("Id"), DefaultParameter]
        public ProcessInfoProperty Property { get; set; }

        /// <summary>
        /// Initializes the layout renderer.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.propertyInfo = typeof(Process).GetProperty(this.Property.ToString());
            if (this.propertyInfo == null)
            {
                throw new ArgumentException("Property '" + this.propertyInfo + "' not found in System.Diagnostics.Process");
            }

            this.process = Process.GetCurrentProcess();
        }

        /// <summary>
        /// Closes the layout renderer.
        /// </summary>
        protected override void Close()
        {
            if (this.process != null)
            {
                this.process.Close();
                this.process = null;
            }

            base.Close();
        }

        /// <summary>
        /// Renders the selected process information.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            if (this.propertyInfo != null)
            {
                builder.Append(Convert.ToString(this.propertyInfo.GetValue(this.process, null), CultureInfo.InvariantCulture));
            }
        }
    }
}

#endif
