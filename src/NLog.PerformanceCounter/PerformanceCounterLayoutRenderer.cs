// 
// Copyright (c) 2004-2020 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using NLog.Common;
    using NLog.Config;
    using NLog.Layouts;

    /// <summary>
    /// The performance counter.
    /// </summary>
    [LayoutRenderer("performancecounter")]
    [ThreadSafe]
    public class PerformanceCounterLayoutRenderer : LayoutRenderer
    {
        private PerformanceCounterCached _fixedPerformanceCounter;
        private PerformanceCounterCached _performanceCounter;

        /// <summary>
        /// Gets or sets the name of the counter category.
        /// </summary>
        /// <docgen category='Performance Counter Options' order='10' />
        [RequiredParameter]
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the name of the performance counter.
        /// </summary>
        /// <docgen category='Performance Counter Options' order='10' />
        [RequiredParameter]
        public string Counter { get; set; }

        /// <summary>
        /// Gets or sets the name of the performance counter instance (e.g. this.Global_).
        /// </summary>
        /// <docgen category='Performance Counter Options' order='10' />
        public Layout Instance
        {
            get { return _instance; }
            set
            {
                _instance = value;
                ResetPerformanceCounters();
            }
        }
        private Layout _instance;

        /// <summary>
        /// Gets or sets the name of the machine to read the performance counter from.
        /// </summary>
        /// <docgen category='Performance Counter Options' order='10' />
        public Layout MachineName
        {
            get { return _machineName; }
            set
            {
                _machineName = value;
                ResetPerformanceCounters();
            }
        }
        private Layout _machineName;

        /// <summary>
        /// Format string for conversion from float to string.
        /// </summary>
        /// <docgen category='Rendering Options' order='50' />
        public string Format { get; set; }

        /// <summary>
        /// Gets or sets the culture used for rendering. 
        /// </summary>
        /// <docgen category='Rendering Options' order='100' />
        public CultureInfo Culture { get; set; }

        /// <inheritdoc />
        protected override void InitializeLayoutRenderer()
        {
            base.InitializeLayoutRenderer();

            if (ReferenceEquals(_instance, null) && string.Equals(Category, "Process", StringComparison.OrdinalIgnoreCase))
            {
                _instance = GetCurrentProcessInstanceName(Category) ?? string.Empty;
            }

            LookupPerformanceCounter(LogEventInfo.CreateNullEvent());
        }

        /// <inheritdoc />
        protected override void CloseLayoutRenderer()
        {
            base.CloseLayoutRenderer();
            _fixedPerformanceCounter?.Close();
            _performanceCounter?.Close();
            ResetPerformanceCounters();
        }

        /// <inheritdoc />
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            var performanceCounter = LookupPerformanceCounter(logEvent);
            var formatProvider = GetFormatProvider(logEvent, Culture);
            builder.Append(performanceCounter.GetValue().ToString(Format, formatProvider));
        }

        private void ResetPerformanceCounters()
        {
            _fixedPerformanceCounter = null;
            _performanceCounter = null;
        }

        PerformanceCounterCached LookupPerformanceCounter(LogEventInfo logEventInfo)
        {
            var perfCounterCached = _fixedPerformanceCounter;
            if (perfCounterCached != null)
                return perfCounterCached;

            perfCounterCached = _performanceCounter;
            var machineName = _machineName?.Render(logEventInfo) ?? string.Empty;
            var instanceName = _instance?.Render(logEventInfo) ?? string.Empty;
            if (perfCounterCached != null && perfCounterCached.MachineName == machineName && perfCounterCached.InstanceName == instanceName)
            {
                return perfCounterCached;
            }

            var perfCounter = CreatePerformanceCounter(machineName, instanceName);
            perfCounterCached = new PerformanceCounterCached(machineName, instanceName, perfCounter);
            if ((ReferenceEquals(_machineName, null) || (_machineName as SimpleLayout)?.IsFixedText==true) && (ReferenceEquals(_instance, null) || (_instance as SimpleLayout)?.IsFixedText == true))
            {
                _fixedPerformanceCounter = perfCounterCached;
            }
            else
            {
                _performanceCounter = perfCounterCached;
            }
            return perfCounterCached;
        }

        private PerformanceCounter CreatePerformanceCounter(string machineName, string instanceName)
        {
            if (!string.IsNullOrEmpty(machineName))
            {
                return new PerformanceCounter(Category, Counter, instanceName, machineName);
            }

            return new PerformanceCounter(Category, Counter, instanceName, true);
        }

        /// <summary>
        /// If having multiple instances with the same process-name, then they will get different instance names
        /// </summary>
        private static string GetCurrentProcessInstanceName(string category)
        {
            try
            {
                string instanceName = null;
                using (Process proc = Process.GetCurrentProcess())
                {
                    int pid = proc.Id;
                    PerformanceCounterCategory cat = new PerformanceCounterCategory(category);
                    foreach (string instanceValue in cat.GetInstanceNames())
                    {
                        using (PerformanceCounter cnt = new PerformanceCounter(category, "ID Process", instanceValue, true))
                        {
                            int val = (int)cnt.RawValue;
                            if (val == pid)
                            {
                                InternalLogger.Debug("PerformanceCounter - Found instance-name={0} from processId={1}", instanceValue, pid);
                                instanceName = instanceValue;
                                if (instanceValue?.IndexOf(proc.ProcessName, StringComparison.Ordinal) >= 0)
                                    return instanceValue;   // Most likely this process, and not old state from recycled ProcessId
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(instanceName))
                        return instanceName;

                    InternalLogger.Debug("PerformanceCounter - Failed to auto detect current process instance. ProcessId={0}", pid);
                }
            }
            catch (Exception ex)
            {
                if (LogManager.ThrowExceptions)
                    throw;

                InternalLogger.Warn(ex, "PerformanceCounter - Failed to auto detect current process instance.");
            }
            return string.Empty;
        }

        class PerformanceCounterCached
        {
            private readonly PerformanceCounter _perfCounter;
            private readonly object _lockObject = new object();
            private CounterSample _prevSample = CounterSample.Empty;
            private CounterSample _nextSample = CounterSample.Empty;

            public PerformanceCounterCached(string machineName, string instanceName, PerformanceCounter performanceCounter)
            {
                MachineName = machineName;
                InstanceName = instanceName;
                _perfCounter = performanceCounter;
                GetValue(); // Prepare Performance Counter for CounterSample.Calculate
            }

            public string MachineName { get; }
            public string InstanceName { get; }

            public float GetValue()
            {
                lock (_lockObject)
                {
                    CounterSample currentSample = _perfCounter.NextSample();
                    if (currentSample.SystemFrequency != 0)
                    {
                        // The recommended delay time between calls to the NextSample method is one second, to allow the counter to perform the next incremental read.
                        float timeDifferenceSecs = (currentSample.TimeStamp - _nextSample.TimeStamp) / (float)currentSample.SystemFrequency;
                        if (timeDifferenceSecs > 0.5F || timeDifferenceSecs < -0.5F)
                        {
                            _prevSample = _nextSample;
                            _nextSample = currentSample;
                            if (_prevSample.Equals(CounterSample.Empty))
                                _prevSample = currentSample;
                        }
                    }
                    else
                    {
                        _prevSample = _nextSample;
                        _nextSample = currentSample;
                    }
                    float sampleValue = CounterSample.Calculate(_prevSample, currentSample);
                    return sampleValue;
                }
            }

            public void Close()
            {
                _perfCounter.Close();
            }
        }
    }
}