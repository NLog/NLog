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
using System.Text;
using System.Globalization;
using System.Security;
using System.Runtime.InteropServices;

using NLog.Config;

namespace NLog.LayoutRenderers
{
    /// <summary>
    /// High precision timer, based on the value returned from QueryPerformanceCounter() optionally converted to seconds.
    /// </summary>
    [LayoutRenderer("qpc")]
    [SupportedRuntime(OS=RuntimeOS.Windows)]
    [SupportedRuntime(OS=RuntimeOS.WindowsNT)]
    [SupportedRuntime(OS=RuntimeOS.WindowsCE)]
    public class QpcLayoutRenderer: LayoutRenderer
    {
        private bool _raw = false;
        private bool _normalize = true;
        private bool _diff = false;
        private ulong _firstQpcValue = 0;
        private ulong _lastQpcValue = 0;
        private bool _first = true;
        private double _frequency = 1;
        private int _precision = 6;
        private bool _alignDecimalPoint = true;

        /// <summary>
        /// Normalize the result by subtracting it from the result of the
        /// first call (so that it's effectively zero-based).
        /// </summary>
        [System.ComponentModel.DefaultValue(true)]
        private bool Normalize
        {
            get { return _normalize; }
            set { _normalize = value; }
        }

        /// <summary>
        /// Output the difference between the result of QueryPerformanceCounter 
        /// and the previous one.
        /// </summary>
        [System.ComponentModel.DefaultValue(false)]
        private bool Difference
        {
            get { return _diff; }
            set { _diff = value; }
        }

        /// <summary>
        /// Convert the result to seconds by dividing by the result of QueryPerformanceFrequency().
        /// </summary>
        [System.ComponentModel.DefaultValue(true)]
        public bool Seconds
        {
            get { return !_raw; }
            set { _raw = !value; }
        }

        /// <summary>
        /// Number of decimal digits to be included in output.
        /// </summary>
        [System.ComponentModel.DefaultValue(4)]
        public int Precision
        {
            get { return _precision; }
            set { _precision = value; }
        }

        /// <summary>
        /// Align decimal point (emit non-significant zeros)
        /// </summary>
        [System.ComponentModel.DefaultValue(true)]
        public bool AlignDecimalPoint
        {
            get { return _alignDecimalPoint; }
            set { _alignDecimalPoint = value; }
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
        /// Renders the ticks value of current time and appends it to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected internal override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            ulong qpcValue;

            if (!QueryPerformanceCounter(out qpcValue))
                return;

            if (_first)
            {
                lock (this)
                {
                    if (_first)
                    {
                        ulong frequency;

                        QueryPerformanceFrequency(out frequency);
                        _frequency = (double)frequency;
                        _firstQpcValue = qpcValue;
                        _lastQpcValue = qpcValue;
                        _first = false;
                    }
                }
            }

            ulong v = qpcValue;

            if (Difference)
                qpcValue -= _lastQpcValue;
            else if (Normalize)
                qpcValue -= _firstQpcValue;

            _lastQpcValue = v;


            string stringValue;

            if (Seconds)
            {
                double val = Math.Round((double)qpcValue / _frequency, Precision);

                stringValue = Convert.ToString(val, CultureInfo.InvariantCulture);
                if (AlignDecimalPoint)
                {
                    int p = stringValue.IndexOf('.');
                    if (p == -1)
                    {
                        stringValue += "." + new string('0', Precision);
                    }
                    else
                    {
                        stringValue += new string('0', Precision - (stringValue.Length - 1 - p));
                    }
                }
            }
            else
            {
                stringValue = Convert.ToString(qpcValue);
            }

            builder.Append(ApplyPadding(stringValue));
        }

#if !NETCF
        [DllImport("kernel32.dll"), SuppressUnmanagedCodeSecurity]
#else
        [DllImport("coredll.dll")]
#endif
        static extern bool QueryPerformanceCounter(out ulong lpPerformanceCount);

#if !NETCF
        [DllImport("kernel32.dll"), SuppressUnmanagedCodeSecurity]
#else
        [DllImport("coredll.dll")]
#endif
        static extern bool QueryPerformanceFrequency(out ulong lpPerformanceFrequency);
    }
}
