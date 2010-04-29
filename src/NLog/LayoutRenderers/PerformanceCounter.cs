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

#if !NETCF

using System;
using System.Text;
using System.IO;
using System.Diagnostics;

using NLog.Config;
using NLog.Internal;

namespace NLog.LayoutRenderers
{
    /// <summary>
    /// The performance counter.
    /// </summary>
    [LayoutRenderer("performancecounter")]
    [NotSupportedRuntime(Framework=RuntimeFramework.DotNetCompactFramework)]
    public class PerformanceCounterLayoutRenderer: LayoutRenderer
    {
        private string _categoryName = null;
        private string _counterName = null;
        private string _instanceName = null;
        private string _machineName = null;
        private PerformanceCounter _perfCounter = null;

        /// <summary>
        /// Name of the counter category.
        /// </summary>
        [RequiredParameter]
        public string Category
        {
            get { return _categoryName; }
            set
            {
                _categoryName = value;
                InvalidatePerformanceCounter();
            }
        }

        /// <summary>
        /// Name of the performance counter.
        /// </summary>
        [RequiredParameter]
        public string Counter
        {
            get { return _counterName; }
            set
            {
                _counterName = value;
                InvalidatePerformanceCounter();
            }
        }

        /// <summary>
        /// Name of the performance counter instance (e.g. _Global_).
        /// </summary>
        public string Instance
        {
            get { return _instanceName; }
            set
            {
                _instanceName = value;
                InvalidatePerformanceCounter();
            }
        }

        /// <summary>
        /// Name of the machine to read the performance counter from.
        /// </summary>
        public string MachineName
        {
            get { return _machineName; }
            set
            {
                _machineName = value;
                InvalidatePerformanceCounter();
            }
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

        private void CreatePerformanceCounter()
        {
            if (_perfCounter == null)
            {
                lock (this)
                {
                    if (_perfCounter == null)
                    {
                        if (_machineName != null)
                        {
                            _perfCounter = new PerformanceCounter(_categoryName, _counterName, _instanceName, _machineName);
                        }
                        else
                        {
                            _perfCounter = new PerformanceCounter(_categoryName, _counterName, _instanceName, true);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Closes currently open performance counter (if any).
        /// </summary>
        private void InvalidatePerformanceCounter()
        {
            if (_perfCounter != null)
            {
                lock (this)
                {
                    if (_perfCounter != null)
                    {
                        _perfCounter.Close();
                        _perfCounter = null;
                    }
                }
            }
        }

        /// <summary>
        /// Renders the specified environment variable and appends it to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected internal override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            CreatePerformanceCounter();
            builder.Append(ApplyPadding(_perfCounter.NextValue().ToString(CultureInfo)));
        }
    }
}

#endif
