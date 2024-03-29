// 
// Copyright (c) 2004-2021 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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


namespace NLog.Layouts
{
    using System.Collections.Generic;
    using System.Text;
    using NLog.Config;

    /// <summary>
    /// A layout containing one or more nested layouts.
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/NLog/NLog/wiki/CompoundLayout">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/NLog/NLog/wiki/CompoundLayout">Documentation on NLog Wiki</seealso>
    [Layout("CompoundLayout")]
    [ThreadAgnostic]
    [AppDomainFixedOutput]
    public class CompoundLayout : Layout
    {
        private Layout[] _precalculateLayouts;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompoundLayout"/> class.
        /// </summary>
        public CompoundLayout()
        {
            Layouts = new List<Layout>();
        }

        /// <summary>
        /// Gets the inner layouts.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        [ArrayParameter(typeof(Layout), "layout")]
        public IList<Layout> Layouts { get; private set; }

        /// <inheritdoc/>
        protected override void InitializeLayout()
        {
            foreach (var layout in Layouts)
                layout.Initialize(LoggingConfiguration);

            base.InitializeLayout();

            _precalculateLayouts = ResolveLayoutPrecalculation(Layouts);
        }

        internal override void PrecalculateBuilder(LogEventInfo logEvent, StringBuilder target)
        {
            PrecalculateBuilderInternal(logEvent, target, _precalculateLayouts);
        }

        /// <inheritdoc/>
        protected override string GetFormattedMessage(LogEventInfo logEvent)
        {
            return RenderAllocateBuilder(logEvent);
        }

        /// <inheritdoc/>
        protected override void RenderFormattedMessage(LogEventInfo logEvent, StringBuilder target)
        {
            //Memory profiling pointed out that using a foreach-loop was allocating
            //an Enumerator. Switching to a for-loop avoids the memory allocation.
            for (int i = 0; i < Layouts.Count; i++)
            {
                Layout layout = Layouts[i];
                layout.Render(logEvent, target);
            }
        }

        /// <inheritdoc/>
        protected override void CloseLayout()
        {
            _precalculateLayouts = null;
            foreach (var layout in Layouts)
                layout.Close();
            base.CloseLayout();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return ToStringWithNestedItems(Layouts, l => l.ToString());
        }
    }
}