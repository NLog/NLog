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
    using System.Collections.Generic;
    using NLog.Internal;
    using Xunit;

    public sealed class ScatterGeneratorTests
    {
        [Fact]
        public void Next_ZeroMaxValue_ReturnsZero()
        {
            var generator = new ScatterGenerator();

            Assert.Equal(0, generator.Next(0));
        }

        [Fact]
        public void Next_One_AlwaysReturnsZero()
        {
            var generator = new ScatterGenerator();

            for (int i = 0; i < 100; i++)
            {
                Assert.Equal(0, generator.Next(1));
            }
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-100)]
        [InlineData(int.MinValue)]
        public void Next_NegativeMaxValue_Throws(int maxValue)
        {
            var generator = new ScatterGenerator();

            var exception = Assert.Throws<ArgumentOutOfRangeException>(
                () => generator.Next(maxValue));

            Assert.Equal(nameof(maxValue), exception.ParamName);
            Assert.Equal(maxValue, exception.ActualValue);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(7)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(399)]
        [InlineData(400)]
        [InlineData(401)]
        [InlineData(1_000)]
        [InlineData(1_024)]
        [InlineData(1_025)]
        [InlineData(1_000_000)]
        [InlineData(int.MaxValue - 1)]
        [InlineData(int.MaxValue)]
        public void Next_ReturnsValueWithinRange(int maxValue)
        {
            var generator = new ScatterGenerator();

            for (var i = 0; i < 1_000; i++)
            {
                var value = generator.Next(maxValue);

                Assert.InRange(value, 0, maxValue - 1);
            }
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 2)]
        [InlineData(-1, 0)]
        [InlineData(-10, 10)]
        [InlineData(-100, -10)]
        [InlineData(10, 100)]
        [InlineData(-500, 500)]
        [InlineData(-1, 2)]
        [InlineData(-100, 101)]
        [InlineData(int.MinValue, 0)]
        [InlineData(0, int.MaxValue)]
        [InlineData(int.MinValue, int.MaxValue)]
        [InlineData(int.MaxValue - 1, int.MaxValue)]
        [InlineData(int.MinValue, int.MinValue + 1)]
        [InlineData(int.MaxValue - 2, int.MaxValue)]
        [InlineData(int.MinValue, int.MinValue + 2)]
        public void Next_Range_ReturnsWithinRange(int minValue, int maxValue)
        {
            var generator = new ScatterGenerator();

            for (int i = 0; i < 1_000; i++)
            {
                int value = generator.Next(minValue, maxValue);

                Assert.True(
                    value >= minValue && value < maxValue,
                    $"Expected [{minValue}, {maxValue}), got {value}.");
            }
        }

        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(int.MaxValue - 1)]
        public void Next_UnitRange_ReturnsMinimum(int minValue)
        {
            var generator = new ScatterGenerator();

            for (int i = 0; i < 100; i++)
            {
                Assert.Equal(
                    minValue,
                    generator.Next(minValue, minValue + 1));
            }
        }

        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(-100)]
        [InlineData(0)]
        [InlineData(100)]
        [InlineData(int.MaxValue)]
        public void Next_EqualRange_ReturnsMinimum(int value)
        {
            var generator = new ScatterGenerator();

            Assert.Equal(value, generator.Next(value, value));
        }

        [Theory]
        [InlineData(1, 0)]
        [InlineData(10, 5)]
        [InlineData(0, -1)]
        [InlineData(int.MaxValue, int.MinValue)]
        public void Next_MinGreaterThanMax_Throws(int minValue, int maxValue)
        {
            var generator = new ScatterGenerator();

            var exception = Assert.Throws<ArgumentOutOfRangeException>(
                () => generator.Next(minValue, maxValue));

            Assert.Equal(nameof(maxValue), exception.ParamName);
            Assert.Equal(maxValue, exception.ActualValue);
        }

        [Fact]
        public void Next_SmallRange_ProducesAllBuckets()
        {
            var generator = new ScatterGenerator();
            var values = new HashSet<int>();

            for (var i = 0; i < 150; i++)
            {
                values.Add(generator.Next(10));
            }

            Assert.Equal(10, values.Count);
        }

        [Fact]
        public void Next_TwoBuckets_ProducesBothValues()
        {
            var generator = new ScatterGenerator();
            var values = new HashSet<int>();

            for (var i = 0; i < 20; i++)
            {
                values.Add(generator.Next(2));
            }

            Assert.Contains(0, values);
            Assert.Contains(1, values);
        }

        [Fact]
        public void Next_EvenOdd_Distribution()
        {
            var generator = new ScatterGenerator();

            var even = 0;
            var odd = 0;

            for (var i = 0; i < 1_000; i++)
            {
                if (generator.Next(int.MaxValue) % 2 == 0)
                    even++;
                else
                    odd++;
            }

            Assert.InRange(even, 300, 700);
            Assert.InRange(odd, 300, 700);
        }
    }
}
