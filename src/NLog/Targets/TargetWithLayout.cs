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
using System.Collections.Generic;
using System.ComponentModel;
using NLog.Layouts;

namespace NLog.Targets
{
    /// <summary>
    /// Represents target that supports string formatting using layouts.
    /// </summary>
    public abstract class TargetWithLayout : Target
    {
        private Layout _layout;

        /// <summary>
        /// Creates a new instance of <see cref="TargetWithLayout" />
        /// </summary>
        protected TargetWithLayout()
        {
            Layout = "${longdate}|${level:uppercase=true}|${logger}|${message}";
        }

        /// <summary>
        /// The compiled layout, can be an instance of <see cref="Layout"/> or other layout type.
        /// </summary>
        [RequiredParameter]
        [DefaultValue("${longdate}|${level:uppercase=true}|${logger}|${message}")]
        public virtual Layout Layout
        {
            get { return _layout; }
            set { _layout = value; }
        }

        /// <summary>
        /// Adds all layouts used by this target to the specified collection.
        /// </summary>
        /// <param name="layouts">The collection to add layouts to.</param>
        public override void PopulateLayouts(ICollection<Layout> layouts)
        {
            if (this.Layout != null)
                this.Layout.PopulateLayouts(layouts);
        }

        /// <summary>
        /// Initializes the target.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            if (this.Layout != null)
                this.Layout.Initialize();
        }

        protected internal override void Close()
        {
            if (this.Layout != null)
                this.Layout.Close();
            base.Close();
        }
   }
}
