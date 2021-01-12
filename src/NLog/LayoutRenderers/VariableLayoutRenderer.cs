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
    using System.Text;
    using NLog.Common;
    using NLog.Config;
    using NLog.Layouts;

    /// <summary>
    /// Render a NLog variable (xml or config)
    /// </summary>
    [LayoutRenderer("var")]
    [ThreadSafe]
    public class VariableLayoutRenderer : LayoutRenderer
    {
        /// <summary>
        /// Gets or sets the name of the NLog variable.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [RequiredParameter]
        [DefaultParameter]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the default value to be used when the variable is not set.
        /// </summary>
        /// <remarks>Not used if Name is <c>null</c></remarks>
        /// <docgen category='Rendering Options' order='10' />
        public string Default { get; set; }

        /// <summary>
        /// Gets the configuration variable layout matching the configured Name
        /// </summary>
        /// <remarks>Mostly relevant for the scanning of active NLog Layouts</remarks>
        public Layout ActiveLayout => TryGetLayout(out var layout) ? layout : null;

        /// <summary>
        /// Initializes the layout renderer.
        /// </summary>
        protected override void InitializeLayoutRenderer()
        {
            if (TryGetLayout(out var layout) && layout != null)
            {
                //pass loggingConfiguration to layout
                layout.Initialize(LoggingConfiguration);
                if (!layout.ThreadSafe)
                {
                    if (layout is SimpleLayout)
                    {
                        InternalLogger.Warn("${{var={0}}} should be declared as <variable name=\"var_{0}\" value=\"...\" /> and used like this ${{var_{0}}}. Because of layout isn't thread safe. Layout={1}", Name, layout);
                    }
                    else
                    {
                        InternalLogger.Warn("${{var={0}}} is not thread safe. This could lead to unexpected results when two targets use the same layout", Name);
                    }
                }
            }

            base.InitializeLayoutRenderer();
        }

        /// <summary>
        /// Try lookup the configuration variable layout matching the configured Name
        /// </summary>
        private bool TryGetLayout(out Layout layout)
        {
            layout = null;
            //Note: don't use LogManager (locking, recursion)
            return Name != null && LoggingConfiguration?.Variables?.TryGetValue(Name, out layout) == true;
        }

        /// <summary>
        /// Renders the specified variable and appends it to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            if (Name != null)
            {
                if (TryGetLayout(out var layout))
                {
                    //ignore NULL, but it set, so don't use default.
                    layout?.RenderAppendBuilder(logEvent, builder);
                }
                else if (Default != null)
                {
                    //fallback
                    builder.Append(Default);
                }
            }
        }
    }
}


