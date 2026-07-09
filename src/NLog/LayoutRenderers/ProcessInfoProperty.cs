//
// Copyright (c) 2004-2024 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.LayoutRenderers
{
    using System;

    /// <summary>
    /// Property of System.Diagnostics.Process to retrieve.
    /// </summary>
    /// <remarks>
    /// Retained for backward compatibility with the obsolete <see cref="ProcessInfoLayoutRenderer"/>.
    /// </remarks>
    [Obsolete("Alternative use ${processid} or ${processname} or ${processstart} or ${processtime} or ${gc:WorkingSet}. Marked obsolete with NLog v6.2 because of AOT-filesize.")]
    public enum ProcessInfoProperty
    {
        /// <summary>
        /// Base Priority.
        /// </summary>
        /// <remarks>Compatibility value: Normal priority.</remarks>
        BasePriority,

        /// <summary>
        /// Exit Code.
        /// </summary>
        /// <remarks>Compatibility value: 0.</remarks>
        ExitCode,

        /// <summary>
        /// Exit Time.
        /// </summary>
        /// <remarks>Compatibility value: <see cref="DateTime.MinValue"/>.</remarks>
        ExitTime,

        /// <summary>
        /// Process Handle.
        /// </summary>
        /// <remarks>Compatibility value: 0.</remarks>
        Handle,

        /// <summary>
        /// Handle Count.
        /// </summary>
        /// <remarks>Compatibility value: 1.</remarks>
        HandleCount,

        /// <summary>
        /// Whether process has exited.
        /// </summary>
        /// <remarks>Compatibility value: <see langword="false"/>.</remarks>
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
        /// <remarks>Compatibility value: 0.</remarks>
        MainWindowHandle,

        /// <summary>
        /// Title of the main window.
        /// </summary>
        /// <remarks>Compatibility value: process base name.</remarks>
        MainWindowTitle,

        /// <summary>
        /// Maximum Working Set.
        /// </summary>
        /// <remarks>Compatibility value: current working set.</remarks>
        MaxWorkingSet,

        /// <summary>
        /// Minimum Working Set.
        /// </summary>
        /// <remarks>Compatibility value: current working set.</remarks>
        MinWorkingSet,

        /// <summary>
        /// Non-paged System Memory Size.
        /// </summary>
        /// <remarks>Compatibility value: current working set. Notice never worked before because of incorrect case-insensitivity on property-name.</remarks>
        NonPagedSystemMemorySize,

        /// <summary>
        /// Non-paged System Memory Size (64-bit).
        /// </summary>
        /// <remarks>Compatibility value: current working set. Notice never worked before because of incorrect case-insensitivity on property-name.</remarks>
        NonPagedSystemMemorySize64,

        /// <summary>
        /// Paged Memory Size.
        /// </summary>
        /// <remarks>Compatibility value: current working set.</remarks>
        PagedMemorySize,

        /// <summary>
        /// Paged Memory Size (64-bit).
        /// </summary>
        /// <remarks>Compatibility value: current working set.</remarks>
        PagedMemorySize64,

        /// <summary>
        /// Paged System Memory Size.
        /// </summary>
        /// <remarks>Compatibility value: current working set.</remarks>
        PagedSystemMemorySize,

        /// <summary>
        /// Paged System Memory Size (64-bit).
        /// </summary>
        /// <remarks>Compatibility value: current working set.</remarks>
        PagedSystemMemorySize64,

        /// <summary>
        /// Peak Paged Memory Size.
        /// </summary>
        /// <remarks>Compatibility value: current working set.</remarks>
        PeakPagedMemorySize,

        /// <summary>
        /// Peak Paged Memory Size (64-bit).
        /// </summary>
        /// <remarks>Compatibility value: current working set.</remarks>
        PeakPagedMemorySize64,

        /// <summary>
        /// Peak Virtual Memory Size.
        /// </summary>
        /// <remarks>Compatibility value: <see cref="int.MaxValue"/>.</remarks>
        PeakVirtualMemorySize,

        /// <summary>
        /// Peak Virtual Memory Size (64-bit).
        /// </summary>
        /// <remarks>Compatibility value: architecture-dependent large value.</remarks>
        PeakVirtualMemorySize64,

        /// <summary>
        /// Peak Working Set Size.
        /// </summary>
        /// <remarks>Compatibility value: current working set.</remarks>
        PeakWorkingSet,

        /// <summary>
        /// Peak Working Set Size (64-bit).
        /// </summary>
        /// <remarks>Compatibility value: current working set.</remarks>
        PeakWorkingSet64,

        /// <summary>
        /// Whether priority boost is enabled.
        /// </summary>
        /// <remarks>Compatibility value: <see langword="true"/>.</remarks>
        PriorityBoostEnabled,

        /// <summary>
        /// Priority Class.
        /// </summary>
        /// <remarks>Compatibility value: Normal priority.</remarks>
        PriorityClass,

        /// <summary>
        /// Private Memory Size.
        /// </summary>
        /// <remarks>Compatibility value: current working set.</remarks>
        PrivateMemorySize,

        /// <summary>
        /// Private Memory Size (64-bit).
        /// </summary>
        /// <remarks>Compatibility value: current working set.</remarks>
        PrivateMemorySize64,

        /// <summary>
        /// Privileged Processor Time.
        /// </summary>
        /// <remarks>Compatibility value: elapsed time since NLog initialization.</remarks>
        PrivilegedProcessorTime,

        /// <summary>
        /// Process Name.
        /// </summary>
        ProcessName,

        /// <summary>
        /// Whether process is responding.
        /// </summary>
        /// <remarks>Compatibility value: <see langword="true"/>.</remarks>
        Responding,

        /// <summary>
        /// Session ID.
        /// </summary>
        /// <remarks>Compatibility value: 1.</remarks>
        SessionId,

        /// <summary>
        /// Process Start Time (Local Time).
        /// </summary>
        /// <remarks>Compatibility value: NLog initialization time in local time.</remarks>
        StartTime,

        /// <summary>
        /// Total Processor Time.
        /// </summary>
        /// <remarks>Compatibility value: elapsed time since NLog initialization.</remarks>
        TotalProcessorTime,

        /// <summary>
        /// User Processor Time.
        /// </summary>
        /// <remarks>Compatibility value: elapsed time since NLog initialization.</remarks>
        UserProcessorTime,

        /// <summary>
        /// Virtual Memory Size.
        /// </summary>
        /// <remarks>Compatibility value: <see cref="int.MaxValue"/>.</remarks>
        VirtualMemorySize,

        /// <summary>
        /// Virtual Memory Size (64-bit).
        /// </summary>
        /// <remarks>Compatibility value: architecture-dependent large value.</remarks>
        VirtualMemorySize64,

        /// <summary>
        /// Working Set Size.
        /// </summary>
        /// <remarks>Compatibility value: Current working set.</remarks>
        WorkingSet,

        /// <summary>
        /// Working Set Size (64-bit).
        /// </summary>
        /// <remarks>Compatibility value: Current working set.</remarks>
        WorkingSet64,

        /// <summary>
        /// Process Start Time (UTC Time).
        /// </summary>
        /// <remarks>Compatibility value: NLog initialization time in UTC.</remarks>
        StartTimeUtc,
    }
}
