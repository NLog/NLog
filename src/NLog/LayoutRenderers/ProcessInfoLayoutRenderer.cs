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
    using System.Diagnostics;
    using System.Globalization;
    using System.Text;
    using NLog.Config;
    using NLog.Internal;
    using NLog.Internal.Fakeables;

    /// <summary>
    /// The information about the running process.
    ///
    /// Obsolete because of AOT-filesize. Instead use ${processid} or ${processname} or ${processstart} or ${processtime} or ${gc}. 
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/NLog/NLog/wiki/ProcessInfo-Layout-Renderer">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/NLog/NLog/wiki/ProcessInfo-Layout-Renderer">Documentation on NLog Wiki</seealso>
    [Obsolete("Alternative use ${processid} or ${processname} or ${processstart} or ${processtime} or ${gc}. Marked obsolete with NLog v6.2 because of AOT-filesize.")]
    [LayoutRenderer("processinfo")]
    public class ProcessInfoLayoutRenderer : LayoutRenderer
    {
        private readonly IAppEnvironment _appEnvironment;

        /// <summary>
        /// Gets or sets the property to retrieve.
        /// </summary>
        /// <remarks>Default: <see cref="ProcessInfoProperty.Id"/></remarks>
        /// <docgen category='Layout Options' order='10' />
        [DefaultParameter]
        public ProcessInfoProperty Property { get; set; } = ProcessInfoProperty.Id;

        /// <summary>
        /// Gets or sets the format string used when converting the property value to a string, when the
        /// property supports formatting (e.g., <see cref="DateTime"/>, <see cref="TimeSpan"/>, or enum types).
        /// </summary>
        /// <remarks>Default: <see langword="null"/></remarks>
        /// <docgen category='Layout Options' order='50' />
        public string? Format { get; set; }

        /// <summary>
        /// Gets or sets the culture used for rendering.
        /// </summary>
        /// <remarks>Default: <see cref="CultureInfo.InvariantCulture"/></remarks>
        /// <docgen category='Layout Options' order='100' />
        public CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessInfoLayoutRenderer" /> class.
        /// </summary>
        public ProcessInfoLayoutRenderer() : this(LogFactory.DefaultAppEnvironment)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessInfoLayoutRenderer" /> class.
        /// </summary>
        internal ProcessInfoLayoutRenderer(IAppEnvironment appEnvironment)
        {
            _appEnvironment = appEnvironment;
        }

        /// <inheritdoc/>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            switch (Property)
            {
                case ProcessInfoProperty.Id:
                    if (string.IsNullOrEmpty(Format))
                        builder.AppendInvariant(_appEnvironment.CurrentProcessId);
                    else
                        AppendFormattedValue(builder, logEvent, _appEnvironment.CurrentProcessId, Format, Culture);
                    break;
                case ProcessInfoProperty.ProcessName:
                    builder.Append(_appEnvironment.CurrentProcessBaseName);
                    break;
                case ProcessInfoProperty.StartTime:
                    var startTimeLocal = LogEventInfo.ZeroDate.ToLocalTime();
                    AppendFormattedValue(builder, logEvent, startTimeLocal, Format, Culture);
                    break;
                case ProcessInfoProperty.StartTimeUtc:
                    var startTimeUtc = LogEventInfo.ZeroDate.ToUniversalTime();
                    AppendFormattedValue(builder, logEvent, startTimeUtc, Format, Culture);
                    break;
                case ProcessInfoProperty.TotalProcessorTime:
                case ProcessInfoProperty.UserProcessorTime:
                case ProcessInfoProperty.PrivilegedProcessorTime:
                    var processorTime = DateTime.UtcNow - LogEventInfo.ZeroDate;
                    AppendFormattedValue(builder, logEvent, processorTime, Format, Culture);
                    break;
                case ProcessInfoProperty.BasePriority:
                    AppendFormattedValue(builder, logEvent, (int)ProcessPriorityClass.Normal, Format, Culture);
                    break;
                case ProcessInfoProperty.ExitCode:
                    AppendFormattedValue(builder, logEvent, 0, Format, Culture);
                    break;
                case ProcessInfoProperty.ExitTime:
                    AppendFormattedValue(builder, logEvent, DateTime.MinValue, Format, Culture);
                    break;
                case ProcessInfoProperty.HasExited:
                    AppendFormattedValue(builder, logEvent, false, Format, Culture);
                    break;
                case ProcessInfoProperty.MachineName:
                    builder.Append("");
                    break;
                case ProcessInfoProperty.MainWindowHandle:
                case ProcessInfoProperty.Handle:
                    AppendFormattedValue(builder, logEvent, 0L, Format, Culture);
                    break;
                case ProcessInfoProperty.HandleCount:
                    AppendFormattedValue(builder, logEvent, 1, Format, Culture);
                    break;
                case ProcessInfoProperty.MainWindowTitle:
                    builder.Append(_appEnvironment.CurrentProcessBaseName);
                    break;
                case ProcessInfoProperty.PeakVirtualMemorySize:
                case ProcessInfoProperty.VirtualMemorySize:
                    AppendFormattedValue(builder, logEvent, int.MaxValue, Format, Culture);
                    break;
                case ProcessInfoProperty.PeakVirtualMemorySize64:
                case ProcessInfoProperty.VirtualMemorySize64:
                    if (IntPtr.Size == 4)
                        AppendFormattedValue(builder, logEvent, (long)int.MaxValue, Format, Culture);
                    else
                        AppendFormattedValue(builder, logEvent, long.MaxValue, Format, Culture);
                    break;
                case ProcessInfoProperty.MaxWorkingSet:
                case ProcessInfoProperty.MinWorkingSet:
                case ProcessInfoProperty.NonPagedSystemMemorySize:
                case ProcessInfoProperty.PagedMemorySize:
                case ProcessInfoProperty.PagedSystemMemorySize:
                case ProcessInfoProperty.PeakPagedMemorySize:
                case ProcessInfoProperty.PeakWorkingSet:
                case ProcessInfoProperty.WorkingSet:
                case ProcessInfoProperty.PrivateMemorySize:
                    var currentMemorySize = (int)Environment.WorkingSet;
                    AppendFormattedValue(builder, logEvent, currentMemorySize, Format, Culture);
                    break;
                case ProcessInfoProperty.NonPagedSystemMemorySize64:
                case ProcessInfoProperty.PagedMemorySize64:
                case ProcessInfoProperty.PagedSystemMemorySize64:
                case ProcessInfoProperty.PeakPagedMemorySize64:
                case ProcessInfoProperty.PeakWorkingSet64:
                case ProcessInfoProperty.WorkingSet64:
                case ProcessInfoProperty.PrivateMemorySize64:
                    var currentMemorySize64 = Environment.WorkingSet;
                    AppendFormattedValue(builder, logEvent, currentMemorySize64, Format, Culture);
                    break;
                case ProcessInfoProperty.PriorityBoostEnabled:
                    AppendFormattedValue(builder, logEvent, true, Format, Culture);
                    break;
                case ProcessInfoProperty.PriorityClass:
                    AppendFormattedValue(builder, logEvent, (int)ProcessPriorityClass.Normal, Format, Culture);
                    break;
                case ProcessInfoProperty.Responding:
                    AppendFormattedValue(builder, logEvent, true, Format, Culture);
                    break;
                case ProcessInfoProperty.SessionId:
                    AppendFormattedValue(builder, logEvent, 1, Format, Culture);
                    break;
            }
        }
    }
}
