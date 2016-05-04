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

namespace NLog.Time
{
    using System;
    using NLog.Config;

    /// <summary>
    /// Defines source of current time.
    /// </summary>
    [NLogConfigurationItem]
    public abstract class TimeSource
    {
        private static TimeSource currentSource = new FastLocalTimeSource();

        /// <summary>
        /// Gets current time.
        /// </summary>
        public abstract DateTime Time { get; }

        /// <summary>
        /// Gets or sets current global time source used in all log events.
        /// </summary>
        /// <remarks>
        /// Default time source is <see cref="FastLocalTimeSource"/>.
        /// </remarks>
        public static TimeSource Current
        {
            get { return currentSource; }
            set { currentSource = value; }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var targetAttribute = (TimeSourceAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(TimeSourceAttribute));
            if (targetAttribute != null)
            {
                return targetAttribute.Name + " (time source)";
            }

            return this.GetType().Name;
        }

        /// <summary>
        ///  Converts the specified system time to the same form as the time value originated from this time source.
        /// </summary>
        /// <param name="systemTime">The system originated time value to convert.</param>
        /// <returns>
        ///  The value of <paramref name="systemTime"/> converted to the same form 
        ///  as time values originated from this source.
        /// </returns>
        /// <remarks>
        ///  <para>
        ///   There are situations when NLog have to compare the time originated from TimeSource 
        ///   to the time originated externally in the system.
        ///   To be able to provide meaningful result of such comparisons the system time must be expressed in 
        ///   the same form as TimeSource time.
        /// </para>
        /// <para>
        ///   Examples:
        ///    - If the TimeSource provides time values of local time, it should also convert the provided 
        ///      <paramref name="systemTime"/> to the local time.
        ///    - If the TimeSource shifts or skews its time values, it should also apply 
        ///      the same transform to the given <paramref name="systemTime"/>.
        /// </para>
        /// </remarks>
        public abstract DateTime FromSystemTime(DateTime systemTime);
    }
}
