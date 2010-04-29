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

namespace NLog.Targets.Wrappers
{
    /// <summary>
    /// A target wrapper that causes a flush after each write on a wrapped target.
    /// </summary>
    /// <example>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" src="examples/targets/Configuration File/AutoFlushWrapper/NLog.config" />
    /// <p>
    /// The above examples assume just one target and a single rule. See below for
    /// a programmatic configuration that's equivalent to the above config file:
    /// </p>
    /// <code lang="C#" src="examples/targets/Configuration API/AutoFlushWrapper/Simple/Example.cs" />
    /// </example>
    [Target("AutoFlushWrapper", IgnoresLayout = true, IsWrapper = true)]
    public class AutoFlushTargetWrapper: WrapperTargetBase
    {
        /// <summary>
        /// Creates a new instance of <see cref="AutoFlushTargetWrapper"/>.
        /// </summary>
        public AutoFlushTargetWrapper()
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="AutoFlushTargetWrapper"/> 
        /// and initializes the <see cref="WrapperTargetBase.WrappedTarget"/> to the specified <see cref="Target"/> value.
        /// </summary>
        public AutoFlushTargetWrapper(Target writeTo)
        {
            WrappedTarget = writeTo;
        }

        /// <summary>
        /// Forwards the call to the <see cref="WrapperTargetBase.WrappedTarget"/>.Write()
        /// and calls <see cref="Target.Flush()"/> on it.
        /// </summary>
        /// <param name="logEvent"></param>
        protected internal override void Write(LogEventInfo logEvent)
        {
            WrappedTarget.Write(logEvent);
            WrappedTarget.Flush();
        }
    }
}
