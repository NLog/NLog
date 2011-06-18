// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
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

#if !NET_CF && !SILVERLIGHT

namespace NLog.LayoutRenderers
{
    using System.Diagnostics;
    using System.Globalization;
    using System.Text;
    using NLog.Config;

    /// <summary>
    /// The performance counter.
    /// </summary>
    [LayoutRenderer("performancecounter")]
    public class PerformanceCounterLayoutRenderer : LayoutRenderer
    {
        private PerformanceCounter perfCounter;

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
        public string Instance { get; set; }

        /// <summary>
        /// Gets or sets the name of the machine to read the performance counter from.
        /// </summary>
        /// <docgen category='Performance Counter Options' order='10' />
        public string MachineName { get; set; }

        /// <summary>
        /// Initializes the layout renderer.
        /// </summary>
        protected override void InitializeLayoutRenderer()
        {
            base.InitializeLayoutRenderer();

            if (this.MachineName != null)
            {
                this.perfCounter = new PerformanceCounter(this.Category, this.Counter, this.Instance, this.MachineName);
            }
            else
            {
                this.perfCounter = new PerformanceCounter(this.Category, this.Counter, this.Instance, true);
            }
        }

        /// <summary>
        /// Closes the layout renderer.
        /// </summary>
        protected override void CloseLayoutRenderer()
        {
            base.CloseLayoutRenderer();
            if (this.perfCounter != null)
            {
                this.perfCounter.Close();
                this.perfCounter = null;
            }
        }

        /// <summary>
        /// Renders the specified environment variable and appends it to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            builder.Append(this.perfCounter.NextValue().ToString(CultureInfo.InvariantCulture));
        }
    }
}

#endif
