// 
// Copyright (c) 2004-2019 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using System.Threading;

    /// <summary>
    /// Current local time retrieved from DateTime.UtcNow with local time offset
    /// </summary>
    [TimeSource("FasterAccurateLocal")]
    public class FasterAccurateLocalTimeSource : TimeSource
    {
        private long _ticksOffset;
        private long _lastUpdated;

        /// <summary>
        /// Initializes TimeSource with offset from utc to local time
        /// </summary>
        public FasterAccurateLocalTimeSource()
        {
            DateTime utc = DateTime.UtcNow;
            _ticksOffset = (DateTime.Now - utc).Ticks;
            _lastUpdated = utc.Ticks;
        }

        /// <summary>
        /// Gets current local time directly from DateTime.Now.
        /// </summary>
        public override DateTime Time => GetTime();


        /// <summary>
        ///  Converts the specified system time to the same form as the time value originated from this time source.
        /// </summary>
        /// <param name="systemTime">The system originated time value to convert.</param>
        /// <returns>
        ///  The value of <paramref name="systemTime"/> converted to local time.
        /// </returns>
        public override DateTime FromSystemTime(DateTime systemTime)
        {
            return systemTime.ToLocalTime();
        }

        private DateTime GetTime()
        {
            DateTime utc = DateTime.UtcNow;
#if !SILVERLIGHT || WINDOWS_PHONE
            long deltaTicks = utc.Ticks - Interlocked.Read(ref _lastUpdated);
#else
            long deltaTicks = utc.Ticks - Interlocked.CompareExchange(ref _lastUpdated, 0, 0);
#endif
            long ticksOffset = _ticksOffset;

            if (deltaTicks > TimeSpan.TicksPerSecond || deltaTicks < -TimeSpan.TicksPerSecond)
            {
                _ticksOffset = ticksOffset = (DateTime.Now - utc).Ticks;
                _lastUpdated = utc.Ticks;
            }

            long ticks = utc.Ticks + ticksOffset;
            return new DateTime(ticks, DateTimeKind.Local);
        }
    }
}
