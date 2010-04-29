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

#if !NETCF && !MONO

using System;
using System.Text;
using System.Runtime.InteropServices;

using NLog.Internal;
using NLog.Config;
using System.Reflection;
using System.Diagnostics;
using System.ComponentModel;

namespace NLog.LayoutRenderers
{
    /// <summary>
    /// The information about the running process.
    /// </summary>
    [LayoutRenderer("processinfo")]
    [SupportedRuntime(Framework=RuntimeFramework.DotNetFramework)]
    public class ProcessInfoLayoutRenderer: LayoutRenderer
    {
        /// <summary>
        /// The property of System.Diagnostics.Process to retrieve
        /// </summary>
        public enum ProcessInfoProperty
        {
            /// <summary></summary>
            BasePriority,
            /// <summary></summary>
            ExitCode,
            /// <summary></summary>
            ExitTime,
            /// <summary></summary>
            Handle,
            /// <summary></summary>
            HandleCount,
            /// <summary></summary>
            HasExited,
            /// <summary></summary>
            Id,
            /// <summary></summary>
            MachineName,
            /// <summary></summary>
            MainModule,
            /// <summary></summary>
            MainWindowHandle,
            /// <summary></summary>
            MainWindowTitle,
            /// <summary></summary>
            MaxWorkingSet,
            /// <summary></summary>
            MinWorkingSet,
            /// <summary></summary>
            NonpagedSystemMemorySize,
            /// <summary></summary>
            NonpagedSystemMemorySize64,
            /// <summary></summary>
            PagedMemorySize,
            /// <summary></summary>
            PagedMemorySize64,
            /// <summary></summary>
            PagedSystemMemorySize,
            /// <summary></summary>
            PagedSystemMemorySize64,
            /// <summary></summary>
            PeakPagedMemorySize,
            /// <summary></summary>
            PeakPagedMemorySize64,
            /// <summary></summary>
            PeakVirtualMemorySize,
            /// <summary></summary>
            PeakVirtualMemorySize64,
            /// <summary></summary>
            PeakWorkingSet,
            /// <summary></summary>
            PeakWorkingSet64,
            /// <summary></summary>
            PriorityBoostEnabled,
            /// <summary></summary>
            PriorityClass,
            /// <summary></summary>
            PrivateMemorySize,
            /// <summary></summary>
            PrivateMemorySize64,
            /// <summary></summary>
            PrivilegedProcessorTime,
            /// <summary></summary>
            ProcessName,
            /// <summary></summary>
            Responding,
            /// <summary></summary>
            SessionId,
            /// <summary></summary>
            StartTime,
            /// <summary></summary>
            TotalProcessorTime,
            /// <summary></summary>
            UserProcessorTime,
            /// <summary></summary>
            VirtualMemorySize,
            /// <summary></summary>
            VirtualMemorySize64,
            /// <summary></summary>
            WorkingSet,
            /// <summary></summary>
            WorkingSet64
        }

        private ProcessInfoProperty _property = ProcessInfoProperty.Id;
        private PropertyInfo _propertyInfo;
        private Process _process;

        /// <summary>
        /// The property to retrieve.
        /// </summary>
        [DefaultValue("Id")]
        [DefaultParameter]
        public ProcessInfoProperty Property
        {
            get { return _property; }
            set { _property = value; }
        }

        /// <summary>
        /// Returns the estimated number of characters that are needed to
        /// hold the rendered value for the specified logging event.
        /// </summary>
        /// <param name="logEvent">Logging event information.</param>
        /// <returns>The number of characters.</returns>
        /// <remarks>
        /// If the exact number is not known or
        /// expensive to calculate this function should return a rough estimate
        /// that's big enough in most cases, but not too big, in order to conserve memory.
        /// </remarks>
        protected internal override int GetEstimatedBufferSize(LogEventInfo logEvent)
        {
            return 32;
        }

        /// <summary>
        /// Renders the selected process information.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected internal override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            if (_propertyInfo != null)
                builder.Append(ApplyPadding(Convert.ToString(_propertyInfo.GetValue(_process, null), CultureInfo)));
        }

        /// <summary>
        /// Initializes the layout renderer.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            _propertyInfo = typeof(Process).GetProperty(Property.ToString());
            if (_propertyInfo == null)
                throw new ArgumentException("Property '" + _propertyInfo + "' not found in System.Diagnostics.Process");
            _process = Process.GetCurrentProcess();
        }

        /// <summary>
        /// Closes the layout renderer.
        /// </summary>
        public override void Close()
        {
            if (_process != null)
            {
                _process.Close();
                _process = null;
            }
            base.Close();
        }
    }
}

#endif
