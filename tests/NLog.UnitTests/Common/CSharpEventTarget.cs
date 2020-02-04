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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NLog.Layouts;
using NLog.Targets;

namespace NLog.UnitTests.Common
{
    /// <summary>
    /// Target for unit testing the last written LogEvent with a event
    /// </summary>
    [Target("CSharpEventTarget")]
    public class CSharpEventTarget : TargetWithLayout
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventInfo">event</param>
        /// <param name="rendered">rendered <see cref="Layout"/></param>
        /// <param name="threadId">current thread</param>
        public delegate void EventHandler(LogEventInfo eventInfo, string rendered, int threadId);

        public event EventHandler BeforeWrite;

        /// <summary>
        /// An event has been written
        /// 
        /// </summary>
        public event EventHandler EventWritten;

        /// <summary>
        /// Increases the number of messages.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        protected override void Write(LogEventInfo logEvent)
        {
            if (BeforeWrite != null)
            {
                BeforeWrite.Invoke(null, null, Thread.CurrentThread.ManagedThreadId);
            }

            if (EventWritten != null)
            {
                var rendered = Layout == null ? null : Layout.Render(logEvent);
                EventWritten.Invoke(logEvent, rendered, Thread.CurrentThread.ManagedThreadId);
            }
        }

    }
}