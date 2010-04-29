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
    /// A target wrapper that causes retries on wrapped target errors.
    /// </summary>
    /// <example>
    /// <p>This example causes each write attempt to be repeated 3 times, 
    /// sleeping 1 second between attempts if first one fails.</p>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" src="examples/targets/Configuration File/RetryingWrapper/NLog.config" />
    /// <p>
    /// The above examples assume just one target and a single rule. See below for
    /// a programmatic configuration that's equivalent to the above config file:
    /// </p>
    /// <code lang="C#" src="examples/targets/Configuration API/RetryingWrapper/Simple/Example.cs" />
    /// </example>
    [Target("RetryingWrapper", IgnoresLayout = true, IsWrapper = true)]
    public class RetryingTargetWrapper: WrapperTargetBase
    {
        private int _retryCount = 3;
        private int _retryDelayMilliseconds = 100;

        /// <summary>
        /// Creates a new instance of <see cref="RetryingTargetWrapper"/>.
        /// </summary>
        public RetryingTargetWrapper()
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="RetryingTargetWrapper"/> and 
        /// initializes the <see cref="WrapperTargetBase.WrappedTarget"/>
        /// <see cref="RetryCount"/> and <see cref="RetryDelayMilliseconds"/>
        /// properties.
        /// </summary>
        public RetryingTargetWrapper(Target writeTo, int retryCount, int retryDelayMilliseconds)
        {
            WrappedTarget = writeTo;
            RetryCount = retryCount;
            RetryDelayMilliseconds = retryDelayMilliseconds;
        }

        /// <summary>
        /// Number of retries that should be attempted on the wrapped target in case of a failure.
        /// </summary>
        [System.ComponentModel.DefaultValue(3)]
        public int RetryCount
        {
            get { return _retryCount; }
            set { _retryCount = value; }
        }

        /// <summary>
        /// The time to wait between retries in milliseconds.
        /// </summary>
        [System.ComponentModel.DefaultValue(100)]
        public int RetryDelayMilliseconds
        {
            get { return _retryDelayMilliseconds; }
            set { _retryDelayMilliseconds = value; }
        }

        /// <summary>
        /// Writes the specified log event to the wrapped target, retrying and pausing in case of an error.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        protected internal override void Write(LogEventInfo logEvent)
        {
            for (int i = 0; i < RetryCount; ++i)
            {
                try
                {
                    if (i > 0)
                        InternalLogger.Warn("Retry #{0}", i);
                    WrappedTarget.Write(logEvent);
                    // success, return
                    return;
                }
                catch (Exception ex)
                {
                    InternalLogger.Warn("Error while writing to '{0}': {1}", WrappedTarget, ex);
                    if (i == RetryCount - 1)
                        throw ex;
                    System.Threading.Thread.Sleep(RetryDelayMilliseconds);
                }
            }
        }
    }
}
