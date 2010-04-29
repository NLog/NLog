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
    /// The information about the garbage collector.
    /// </summary>
    [LayoutRenderer("gc")]
    [SupportedRuntime(Framework=RuntimeFramework.DotNetFramework)]
    public class GCLayoutRenderer: LayoutRenderer
    {
        /// <summary>
        /// The property of System.GC to retrieve
        /// </summary>
        public enum GCProperty
        {
            /// <summary>
            /// Total memory allocated
            /// </summary>
            TotalMemory,

            /// <summary>
            /// Total memory allocated (perform full garbage collection first)
            /// </summary>
            TotalMemoryForceCollection,

            /// <summary>
            /// Number of Gen0 collections.
            /// </summary>
            CollectionCount0,

            /// <summary>
            /// Number of Gen1 collections.
            /// </summary>
            CollectionCount1,

            /// <summary>
            /// Number of Gen2 collections.
            /// </summary>
            CollectionCount2,

            /// <summary>
            /// Maximum generation number supported by GC.
            /// </summary>
            MaxGeneration,
        }

        private GCProperty _property = GCProperty.TotalMemory;

        /// <summary>
        /// The property to retrieve.
        /// </summary>
        [DefaultValue("TotalMemory")]
        public GCProperty Property
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
            object value = null;

            switch (_property)
            {
                case GCProperty.TotalMemory:
                    value = GC.GetTotalMemory(false);
                    break;

                case GCProperty.TotalMemoryForceCollection:
                    value = GC.GetTotalMemory(true);
                    break;

#if DOTNET2
                case GCProperty.CollectionCount0:
                    value = GC.CollectionCount(0);
                    break;

                case GCProperty.CollectionCount1:
                    value = GC.CollectionCount(1);
                    break;

                case GCProperty.CollectionCount2:
                    value = GC.CollectionCount(2);
                    break;
#endif

                case GCProperty.MaxGeneration:
                    value = GC.MaxGeneration;
                    break;
            }

            builder.Append(ApplyPadding(Convert.ToString(value, CultureInfo)));
        }
    }
}

#endif
