//
// Copyright (c) 2004-2024 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.Targets.Dispatchers
{
    using System.Threading;

    /// <summary>
    /// Scheduling rules shared by all <see cref="IChannelReader"/> implementations, in milliseconds.
    /// </summary>
    /// <remarks>
    /// timeToSleepBetweenBatches = 0 writes instantly, 1 or less (default 1) writes on new activity with a short throttle,
    /// greater than 1 writes periodically and ignores <see cref="IChannelReader.Signal"/>.
    /// </remarks>
    internal static class ChannelReaderSchedule
    {
        /// <summary>
        /// Delay before the first consumer run.
        /// </summary>
        public static int StartDelay(int timeToSleepBetweenBatches) => IsPeriodic(timeToSleepBetweenBatches) ? timeToSleepBetweenBatches : 1;

        /// <summary>
        /// Delay before the consumer run requested by a signal while idle.
        /// </summary>
        public static int SignalDelay(int timeToSleepBetweenBatches) => timeToSleepBetweenBatches == 0 ? 0 : 1;

        /// <summary>
        /// Delay before the next consumer run after <paramref name="result"/>, or <see cref="Timeout.Infinite"/> to wait for a signal.
        /// </summary>
        public static int NextDelay(ConsumeResult result, int timeToSleepBetweenBatches)
        {
            if (IsPeriodic(timeToSleepBetweenBatches))
                return timeToSleepBetweenBatches;

            switch (result)
            {
                case ConsumeResult.FullBatch: return 0;
                case ConsumeResult.MorePending: return 1;
                default: return Timeout.Infinite;
            }
        }

        /// <summary>
        /// Periodic mode runs every timeToSleepBetweenBatches and ignores signals.
        /// </summary>
        public static bool IsPeriodic(int timeToSleepBetweenBatches) => timeToSleepBetweenBatches > 1;
    }
}
