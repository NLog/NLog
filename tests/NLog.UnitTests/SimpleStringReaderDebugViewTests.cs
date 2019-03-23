using NLog.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLog.UnitTests
{
#if DEBUG
    public class SimpleStringReaderDebugViewTests : NLogTestBase
    {
        [Theory]
        [InlineData("", 0, "", char.MaxValue, "" )]
        [InlineData("abcdef", 0, "", 'a', "abcdef")]
        [InlineData("abcdef", 2, "ab", 'c', "cdef")]
        [InlineData("abcdef", 6, "abcdef", char.MaxValue, "")]
        [InlineData("abcdef", 7, "INVALID_CURRENT_STATE", char.MaxValue, "INVALID_CURRENT_STATE")]
        /// <summary>
        /// https://github.com/NLog/NLog/issues/3194
        /// </summary>
        public void DebugView_CurrentState(string input, Int32 position, string done, char current, string todo)
        {
            var reader = new SimpleStringReader(input);
            reader.Position = position;
            Assert.Equal(
                SimpleStringReader.BuildCurrentState(done, current, todo), 
                reader.CurrentState);
        }

        [Fact]
        public void DebugView_CurrentState_NegativePosition()
        {
            Assert.Throws<IndexOutOfRangeException>(() => new SimpleStringReader("abcdef")
            {
                Position = -1,
            }.CurrentState);
        }
    }
#endif
}
