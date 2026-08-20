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

namespace NLog.UnitTests.Internal
{
    using System;
    using NLog.Internal;
    using Xunit;

    public class CryptoRandomTests
    {
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(100)]
        [InlineData(int.MaxValue)]
        public void GetInt32_MaxValue_ReturnsWithinRange(int toExclusive)
        {
            for (int i = 0; i < 100; ++i)
            {
                var randomValue = CryptoRandom.GetInt32(toExclusive);
                Assert.InRange(randomValue, 0, toExclusive - 1);
            }
        }

        [Theory]
        [InlineData(-100, -10)]
        [InlineData(-10, 10)]
        [InlineData(10, 100)]
        [InlineData(int.MinValue, int.MaxValue)]
        public void GetInt32_Range_ReturnsWithinRange(int fromInclusive, int toExclusive)
        {
            for (int i = 0; i < 100; ++i)
            {
                var randomValue = CryptoRandom.GetInt32(fromInclusive, toExclusive);
                Assert.InRange(randomValue, fromInclusive, toExclusive - 1);
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void GetInt32_InvalidMaxValue_Throws(int toExclusive)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => CryptoRandom.GetInt32(toExclusive));
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 0)]
        public void GetInt32_InvalidRange_Throws(int fromInclusive, int toExclusive)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => CryptoRandom.GetInt32(fromInclusive, toExclusive));
        }
    }
}