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

namespace NLog.LayoutRenderers.Wrappers
{
    using System.ComponentModel;
    using NLog.Config;

    /// <summary>
    /// Applies caching to another layout output.
    /// </summary>
    /// <remarks>
    /// The value of the inner layout will be rendered only once and reused subsequently.
    /// </remarks>
    [LayoutRenderer("cached")]
    [AmbientProperty("Cached")]
    [ThreadAgnostic]
    public sealed class CachedLayoutRendererWrapper : WrapperLayoutRendererBase
    {
        private string cachedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedLayoutRendererWrapper"/> class.
        /// </summary>
        public CachedLayoutRendererWrapper()
        {
            this.Cached = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CachedLayoutRendererWrapper"/> is enabled.
        /// </summary>
        /// <docgen category='Caching Options' order='10' />
        [DefaultValue(true)]
        public bool Cached { get; set; }

        /// <summary>
        /// Initializes the layout renderer.
        /// </summary>
        protected override void InitializeLayoutRenderer()
        {
            base.InitializeLayoutRenderer();
            this.cachedValue = null;
        }

        /// <summary>
        /// Closes the layout renderer.
        /// </summary>
        protected override void CloseLayoutRenderer()
        {
            base.CloseLayoutRenderer();
            this.cachedValue = null;
        }

        /// <summary>
        /// Transforms the output of another layout.
        /// </summary>
        /// <param name="text">Output to be transform.</param>
        /// <returns>Transformed text.</returns>
        protected override string Transform(string text)
        {
            return text;
        }

        /// <summary>
        /// Renders the inner layout contents.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        /// <returns>Contents of inner layout.</returns>
        protected override string RenderInner(LogEventInfo logEvent)
        {
            if (this.Cached)
            {
                if (this.cachedValue == null)
                {
                    this.cachedValue = base.RenderInner(logEvent);
                }

                return this.cachedValue;
            }
            else
            {
                return base.RenderInner(logEvent);
            }
        }
    }
}
