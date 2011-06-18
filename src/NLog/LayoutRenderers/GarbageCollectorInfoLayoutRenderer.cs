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

#if !NET_CF && !MONO

namespace NLog.LayoutRenderers
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Text;

    /// <summary>
    /// The information about the garbage collector.
    /// </summary>
    [LayoutRenderer("gc")]
    public class GarbageCollectorInfoLayoutRenderer : LayoutRenderer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GarbageCollectorInfoLayoutRenderer" /> class.
        /// </summary>
        public GarbageCollectorInfoLayoutRenderer()
        {
            this.Property = GarbageCollectorProperty.TotalMemory;
        }

        /// <summary>
        /// Gets or sets the property to retrieve.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultValue("TotalMemory")]
        public GarbageCollectorProperty Property { get; set; }
        
        /// <summary>
        /// Renders the selected process information.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            object value = null;

            switch (this.Property)
            {
                case GarbageCollectorProperty.TotalMemory:
                    value = GC.GetTotalMemory(false);
                    break;

                case GarbageCollectorProperty.TotalMemoryForceCollection:
                    value = GC.GetTotalMemory(true);
                    break;

#if !SILVERLIGHT
                case GarbageCollectorProperty.CollectionCount0:
                    value = GC.CollectionCount(0);
                    break;

                case GarbageCollectorProperty.CollectionCount1:
                    value = GC.CollectionCount(1);
                    break;

                case GarbageCollectorProperty.CollectionCount2:
                    value = GC.CollectionCount(2);
                    break;

#endif
                
                case GarbageCollectorProperty.MaxGeneration:
                    value = GC.MaxGeneration;
                    break;
            }

            builder.Append(Convert.ToString(value, CultureInfo.InvariantCulture));
        }
    }
}

#endif
