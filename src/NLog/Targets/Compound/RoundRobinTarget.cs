// 
// Copyright (c) 2004,2005 Jaroslaw Kowalski <jkowalski@users.sourceforge.net>
// 
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
// * Neither the name of the Jaroslaw Kowalski nor the names of its 
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
using System.Threading;

using NLog.Config;

namespace NLog.Targets.Compound
{
    /// <summary>
    /// A compound target that forwards writes to the sub-targets in a
    /// round-robin fashion.
    /// </summary>
    [Target("RoundRobinGroup",IgnoresLayout=true)]
    public class RoundRobinTarget: CompoundTargetBase
    {
        private int _currentTarget = 0;

        /// <summary>
        /// Creates a new instance of <see cref="RoundRobinTarget"/>.
        /// </summary>
        public RoundRobinTarget()
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="RoundRobinTarget"/> and initializes
        /// the <see cref="Targets"/> collection to the provided
        /// array of <see cref="Target"/> objects.
        /// </summary>
        public RoundRobinTarget(params Target[] targets) : base(targets)
        {
        }

        /// <summary>
        /// Forwards the write to one of the targets from 
        /// the <see cref="Targets"/> collection.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        /// <remarks>
        /// The writes are routed in a round-robin fashion.
        /// The first log event goes to the first target, the second
        /// one goes to the second target and so on looping to the
        /// first target when there are no more targets available.
        /// In general request N goes to Targets[N % Targets.Count].
        /// </remarks>
        protected internal override void Write(LogEventInfo logEvent)
        {
            int currentTarget = Interlocked.Increment(ref _currentTarget);
            Targets[currentTarget % Targets.Count].Write(logEvent);
        }
   }
}