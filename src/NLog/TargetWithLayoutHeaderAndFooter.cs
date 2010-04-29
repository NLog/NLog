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
using System.Collections;

using NLog.Config;
using NLog.Layouts;

namespace NLog
{
    /// <summary>
    /// Represents target that supports string formatting using layouts.
    /// </summary>
    public abstract class TargetWithLayoutHeaderAndFooter : TargetWithLayout
    {
        /// <summary>
        /// Creates a new instance of <see cref="TargetWithLayout" />
        /// </summary>
        protected TargetWithLayoutHeaderAndFooter()
        {
            LayoutWithHeaderAndFooter h = new LayoutWithHeaderAndFooter();
            h.Layout = new Layout("${longdate}|${level:uppercase=true}|${logger}|${message}");
            CompiledLayout = h;
        }

        /// <summary>
        /// The text to be rendered.
        /// </summary>
        [RequiredParameter]
        [AcceptsLayout]
        [System.ComponentModel.DefaultValue("${longdate}|${level:uppercase=true}|${logger}|${message}")]
        public override string Layout
        {
            get { return Convert.ToString(CompiledLayoutWithHeaderAndFooter.Layout); }
            set
            {
                if (CompiledLayoutWithHeaderAndFooter != null)
                    CompiledLayoutWithHeaderAndFooter.Layout = new Layout(value);
            }
        }

        /// <summary>
        /// Header
        /// </summary>
        [AcceptsLayout]
        public string Header
        {
            get { return Convert.ToString(CompiledHeader); }
            set { CompiledHeader = new Layout(value); }
        }

        /// <summary>
        /// Footer
        /// </summary>
        [AcceptsLayout]
        public string Footer
        {
            get { return Convert.ToString(CompiledFooter); }
            set { CompiledFooter = new Layout(value); }
        }


        /// <summary>
        /// Compiled header.
        /// </summary>
        /// <value>The compiled header.</value>
        /// <remarks>
        /// The header can be of any layout type.
        /// </remarks>
        public ILayout CompiledHeader
        {
            get
            {
                ILayoutWithHeaderAndFooter h = CompiledLayoutWithHeaderAndFooter;
                if (h != null)
                    return h.Header;
                return null;
            }
            set
            {
                ILayoutWithHeaderAndFooter h = CompiledLayoutWithHeaderAndFooter;
                h.Header = value;
            }
        }

        /// <summary>
        /// Compiled footer.
        /// </summary>
        /// <value>The compiled footer.</value>
        /// <remarks>
        /// The header can be of any layout type.
        /// </remarks>
        public ILayout CompiledFooter
        {
            get
            {
                ILayoutWithHeaderAndFooter h = CompiledLayoutWithHeaderAndFooter;
                if (h != null)
                    return h.Footer;
                return null;
            }
            set
            {
                ILayoutWithHeaderAndFooter h = CompiledLayoutWithHeaderAndFooter;
                h.Footer = value;
            }
        }

        /// <summary>
        /// Gets the compiled layout with header and footer.
        /// </summary>
        /// <value>The compiled layout with header and footer.</value>
        public ILayoutWithHeaderAndFooter CompiledLayoutWithHeaderAndFooter
        {
            get { return base.CompiledLayout as ILayoutWithHeaderAndFooter; }
        }

        /// <summary>
        /// Adds all layouts used by this target to the specified collection.
        /// </summary>
        /// <param name="layouts">The collection to add layouts to.</param>
        public override void PopulateLayouts(LayoutCollection layouts)
        {
            this.CompiledLayout.PopulateLayouts(layouts);
        }

        /// <summary>
        /// Initializes the target.
        /// </summary>
        public override void Initialize()
        {
            this.CompiledLayout.Initialize();
        }
   }
}
