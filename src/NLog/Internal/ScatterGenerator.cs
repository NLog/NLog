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

    /// <summary>
    /// Lightweight, non-cryptographic pseudo-random-number-generator (PRNG).
    /// Intended for jitter, load balancing, and other non-security-sensitive uses.
    /// </summary>
    /// <remarks>
    /// The output is predictable and provides no uniqueness guarantees.
    /// Must not be used for cryptographic purposes, security tokens, or unique identifiers.
    /// </remarks>
    internal sealed class ScatterGenerator
    {
        // Weyl-sequence increment derived from the golden ratio. Being odd makes it
        // coprime with 2^32, giving the Weyl sequence a full 2^32-period.
        private const uint Gamma = 0x9E3779B9u;

        private uint _state;

        public ScatterGenerator()
        {
            unchecked
            {
                // Mix the current timestamp with an identity-based hash to vary the initial
                // state across instances. Neither source provides uniqueness or entropy.
                _state = (uint)System.Diagnostics.Stopwatch.GetTimestamp()
                    ^ (uint)System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this); // NOSONAR - Sealed
            }
        }

        /// <summary>
        /// Returns a pseudo-random value in the range [0, maxValue).
        /// </summary>
        public int Next(int maxValue)
        {
            if (maxValue < 0)
                throw new ArgumentOutOfRangeException(nameof(maxValue), maxValue, "MaxValue must be non-negative");

            if (maxValue == 0)
                return 0;

            // Multiply-high reduction maps the 32-bit output into [0, maxValue).
            // Bucket populations differ by at most one, avoiding the systematic
            // low-bucket bias introduced by naive modulo reduction.
            return (int)(((ulong)NextUInt() * (uint)maxValue) >> 32);
        }

        /// <summary>
        /// Returns a pseudo-random value in the range [minValue, maxValue).
        /// </summary>
        public int Next(int minValue, int maxValue)
        {
            if (minValue > maxValue)
                throw new ArgumentOutOfRangeException(nameof(maxValue), maxValue, $"MinValue={minValue} > MaxValue={maxValue}");

            if (minValue == maxValue)
                return minValue;

            unchecked
            {
                // Calculate the range as uint so the full signed Int32 range can be
                // represented, including int.MinValue to int.MaxValue.
                uint range = (uint)((long)maxValue - minValue);
                // Multiply-high reduction produces an offset in [0, range).
                uint offset = (uint)(((ulong)NextUInt() * range) >> 32);
                return minValue + (int)offset;
            }
        }

        /// <summary>
        /// Returns the next pseudo-random unsigned 32-bit value.
        /// </summary>
        private uint NextUInt()
        {
            unchecked
            {
                // Weyl sequence step: cheap deterministic state progression with a full 2^32 period.
                uint x = _state += Gamma;

                // MurmurHash3 fmix32: invertible avalanche transformation that
                // scatters the structured Weyl sequence across the output bits.
                x ^= x >> 16;
                x *= 0x85EBCA6Bu;
                x ^= x >> 13;
                x *= 0xC2B2AE35u;
                x ^= x >> 16;

                return x;
            }
        }
    }
}
