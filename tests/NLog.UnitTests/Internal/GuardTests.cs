// 
// Copyright (c) 2004-2021 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

using NLog.Internal;
using System;
using Xunit;

namespace NLog.UnitTests.Internal
{
    public class GuardTests
    {
        [Fact]
        public void ThrowIfNull_WhenArgumentIsNull_ThrowArgumentNullException()
        {
            object argument = null;

            Assert.Throws<ArgumentNullException>(() => Guard.ThrowIfNull(argument));
        }

        [Fact]
        public void ThrowIfNull_WhenArgumentIsNotNull_ReturnArgument()
        {
            var argument = "test";

            var result = Guard.ThrowIfNull(argument);

            Assert.Equal(argument, result);
        }

        [Fact]
        public void ThrowIfNullOrEmpty_WhenArgumentIsNull_ThrowArgumentNullException()
        {
            string argument = null;

            Assert.Throws<ArgumentNullException>(() => Guard.ThrowIfNullOrEmpty(argument));
        }

        [Fact]
        public void ThrowIfNullOrEmpty_WhenArgumentIsEmpty_ThrowArgumentNullException()
        {
            var argument = string.Empty;

            Assert.Throws<ArgumentNullException>(() => Guard.ThrowIfNullOrEmpty(argument));
        }

        [Fact]
        public void ThrowIfNullOrEmpty_WhenArgumentIsValid_ReturnArgument()
        {
            var argument = "test";

            var result = Guard.ThrowIfNull(argument);

            Assert.Equal(argument, result);
        }
    }
}
