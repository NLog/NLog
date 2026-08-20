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
    using System.Security.Cryptography;

    internal static class CryptoRandom
    {
        private const ulong UInt32ValueCount = (ulong)uint.MaxValue + 1;
        [ThreadStatic]
        private static RandomNumberGenerator? _randomNumberGenerator;
        [ThreadStatic]
        private static byte[]? _randomBytes;

        internal static int GetInt32(int toExclusive)
        {
            if (toExclusive <= 0)
                throw new ArgumentOutOfRangeException(nameof(toExclusive));

            return (int)GetUInt32((uint)toExclusive);
        }

        internal static int GetInt32(int fromInclusive, int toExclusive)
        {
            if (fromInclusive >= toExclusive)
                throw new ArgumentOutOfRangeException(nameof(toExclusive));

            var range = (uint)((long)toExclusive - fromInclusive);
            return (int)(fromInclusive + (long)GetUInt32(range));
        }

        private static uint GetUInt32(uint toExclusive)
        {
            var upperBound = UInt32ValueCount - (UInt32ValueCount % toExclusive);
            var randomNumberGenerator = _randomNumberGenerator ?? (_randomNumberGenerator = RandomNumberGenerator.Create());
            var randomBytes = _randomBytes ?? (_randomBytes = new byte[sizeof(uint)]);

            uint randomValue;
            do
            {
                randomNumberGenerator.GetBytes(randomBytes);
                randomValue = ((uint)randomBytes[0] << 24)
                              | ((uint)randomBytes[1] << 16)
                              | ((uint)randomBytes[2] << 8)
                              | randomBytes[3];
            }
            while ((ulong)randomValue >= upperBound);

            return randomValue % toExclusive;
        }
    }
}
