// 
// Copyright (c) 2004-2016 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.Common
{
    /// <summary>
    /// Represents the logging event with asynchronous continuation.
    /// </summary>
    public struct AsyncLogEventInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncLogEventInfo"/> struct.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        /// <param name="continuation">The continuation.</param>
        public AsyncLogEventInfo(LogEventInfo logEvent, AsyncContinuation continuation)
            : this()
        {
            this.LogEvent = logEvent;
            this.Continuation = continuation;
        }

        /// <summary>
        /// Gets the log event.
        /// </summary>
        public LogEventInfo LogEvent { get; private set; }

        /// <summary>
        /// Gets the continuation.
        /// </summary>
        public AsyncContinuation Continuation { get; internal set; }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="eventInfo1">The event info1.</param>
        /// <param name="eventInfo2">The event info2.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(AsyncLogEventInfo eventInfo1, AsyncLogEventInfo eventInfo2)
        {
            return ReferenceEquals(eventInfo1.Continuation, eventInfo2.Continuation)
                   && ReferenceEquals(eventInfo1.LogEvent, eventInfo2.LogEvent);
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="eventInfo1">The event info1.</param>
        /// <param name="eventInfo2">The event info2.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(AsyncLogEventInfo eventInfo1, AsyncLogEventInfo eventInfo2)
        {
            return !ReferenceEquals(eventInfo1.Continuation, eventInfo2.Continuation)
                   || !ReferenceEquals(eventInfo1.LogEvent, eventInfo2.LogEvent);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// A value of <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            var other = (AsyncLogEventInfo)obj;
            return this == other;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return this.LogEvent.GetHashCode() ^ this.Continuation.GetHashCode();
        }
    }
}
