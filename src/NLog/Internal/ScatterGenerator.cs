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

namespace NLog.Internal
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    internal sealed class ScatterGenerator
    {
        // An odd Weyl-sequence increment produces a full 2^32 period.
        private const uint Gamma = 0x9E3779B9u;

        private static int _seed;

        private uint _state;

        internal ScatterGenerator()
        {
            unchecked
            {
                _state = (uint)Stopwatch.GetTimestamp() ^ (uint)Interlocked.Increment(ref _seed);
            }
        }

        internal uint NextUInt()
        {
            unchecked
            {
                uint value = _state += Gamma;
                value ^= value >> 16;
                value *= 0x85EBCA6Bu;
                value ^= value >> 13;
                value *= 0xC2B2AE35u;
                value ^= value >> 16;
                return value;
            }
        }

        internal int Next(int maxValue)
        {
            if (maxValue < 0)
                throw new ArgumentOutOfRangeException(nameof(maxValue), maxValue, "MaxValue must be non-negative");

            if (maxValue == 0)
                return 0;

            return (int)NextUInt32((uint)maxValue);
        }

        internal int Next(int minValue, int maxValue)
        {
            if (minValue > maxValue)
                throw new ArgumentOutOfRangeException(nameof(maxValue), maxValue, $"MinValue={minValue} > MaxValue={maxValue}");

            if (minValue == maxValue)
                return minValue;

            unchecked
            {
                uint range = (uint)((long)maxValue - minValue);
                uint offset = NextUInt32(range);
                return minValue + (int)offset;
            }
        }

        private uint NextUInt32(uint range)
        {
            uint threshold = unchecked(0u - range) % range;
            ulong product;
            uint remainder;
            do
            {
                product = (ulong)NextUInt() * range;
                remainder = (uint)product;
            }
            while (remainder < threshold);

            return (uint)(product >> 32);
        }
    }
}
