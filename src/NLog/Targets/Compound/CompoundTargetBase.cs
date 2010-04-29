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
using System.IO;
using System.Text;
using System.Xml;
using System.Reflection;
using System.Diagnostics;

using NLog.Internal;
using System.Net;
using System.Net.Sockets;

using NLog.Config;

namespace NLog.Targets.Compound
{
    /// <summary>
    /// A base class for targets which wrap other (multiple) targets
    /// and provide various forms of target routing.
    /// </summary>
    public abstract class CompoundTargetBase: Target
    {
        private TargetCollection _targets = new TargetCollection();

        /// <summary>
        /// Creates a new instance of <see cref="CompoundTargetBase"/>.
        /// </summary>
        public CompoundTargetBase()
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="CompoundTargetBase"/> and
        /// initializes the <see cref="Targets"/> collection to the provided
        /// list of <see cref="Target"/> objects.
        /// </summary>
        public CompoundTargetBase(params Target[] targets)
        {
            _targets.AddRange(targets);
        }

        /// <summary>
        /// A collection of targets managed by this compound target.
        /// </summary>
        public TargetCollection Targets
        {
            get { return _targets; }
        }

        /// <summary>
        /// Adds all layouts used by this target and sub-targets.
        /// </summary>
        /// <param name="layouts">The collection to add layouts to.</param>
        public override void PopulateLayouts(LayoutCollection layouts)
        {
            base.PopulateLayouts (layouts);
            foreach (Target t in Targets)
            {
                t.PopulateLayouts(layouts);
            }
        }

        /// <summary>
        /// Initializes the target by initializing all sub-targets.
        /// </summary>
        public override void Initialize()
        {
            foreach (Target t in Targets)
            {
                t.Initialize();
            }
        }

        /// <summary>
        /// Closes the target by closing all sub-targets.
        /// </summary>
        protected internal override void Close()
        {
            base.Close ();
            foreach (Target t in Targets)
            {
                t.Close();
            }
        }
   }
}