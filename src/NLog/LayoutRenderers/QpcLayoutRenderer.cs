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

#if !SILVERLIGHT

using System;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using NLog.Internal;

namespace NLog.LayoutRenderers
{
    /// <summary>
    /// High precision timer, based on the value returned from QueryPerformanceCounter() optionally converted to seconds.
    /// </summary>
    [LayoutRenderer("qpc")]
    public class QpcLayoutRenderer : LayoutRenderer
    {
        private bool raw;
        private ulong firstQpcValue;
        private ulong lastQpcValue;
        private bool first = true;
        private double frequency = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="QpcLayoutRenderer" /> class.
        /// </summary>
        public QpcLayoutRenderer()
        {
            this.Normalize = true;
            this.Difference = false;
            this.Precision = 4;
            this.AlignDecimalPoint = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to normalize the result by subtracting 
        /// it from the result of the first call (so that it's effectively zero-based).
        /// </summary>
        [DefaultValue(true)]
        public bool Normalize { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to output the difference between the result 
        /// of QueryPerformanceCounter and the previous one.
        /// </summary>
        [DefaultValue(false)]
        public bool Difference { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to convert the result to seconds by dividing 
        /// by the result of QueryPerformanceFrequency().
        /// </summary>
        [DefaultValue(true)]
        public bool Seconds
        {
            get { return !this.raw; }
            set { this.raw = !value; }
        }

        /// <summary>
        /// Gets or sets the number of decimal digits to be included in output.
        /// </summary>
        [DefaultValue(4)]
        public int Precision { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to align decimal point (emit non-significant zeros).
        /// </summary>
        [DefaultValue(true)]
        public bool AlignDecimalPoint { get; set; }

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
        protected override int GetEstimatedBufferSize(LogEventInfo logEvent)
        {
            return 32;
        }

        /// <summary>
        /// Renders the ticks value of current time and appends it to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            ulong qpcValue;

            if (!NativeMethods.QueryPerformanceCounter(out qpcValue))
            {
                return;
            }

            if (this.first)
            {
                lock (this)
                {
                    if (this.first)
                    {
                        ulong frequency;

                        NativeMethods.QueryPerformanceFrequency(out frequency);
                        this.frequency = (double)frequency;
                        this.firstQpcValue = qpcValue;
                        this.lastQpcValue = qpcValue;
                        this.first = false;
                    }
                }
            }

            ulong v = qpcValue;

            if (this.Difference)
            {
                qpcValue -= this.lastQpcValue;
            }
            else if (this.Normalize)
            {
                qpcValue -= this.firstQpcValue;
            }

            this.lastQpcValue = v;

            string stringValue;

            if (this.Seconds)
            {
                double val = Math.Round((double)qpcValue / this.frequency, this.Precision);

                stringValue = Convert.ToString(val, CultureInfo.InvariantCulture);
                if (this.AlignDecimalPoint)
                {
                    int p = stringValue.IndexOf('.');
                    if (p == -1)
                    {
                        stringValue += "." + new string('0', this.Precision);
                    }
                    else
                    {
                        stringValue += new string('0', this.Precision - (stringValue.Length - 1 - p));
                    }
                }
            }
            else
            {
                stringValue = Convert.ToString(qpcValue);
            }

            builder.Append(stringValue);
        }
    }
}

#endif