#if DNX
using NLog.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NLog.UnitTests.Internal
{
    public class PlatformDetectorTests
    {

        [Fact]
        public void GetCurrentOSTest()
        {
            var actual = PlatformDetector.CurrentOS;
            Assert.NotEqual(RuntimeOS.Unknown, actual);
        }

    }
}

#endif