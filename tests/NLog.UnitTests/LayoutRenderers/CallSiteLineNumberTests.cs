#region

using System;
using System.Linq;
using Xunit;

#endregion

namespace NLog.UnitTests.LayoutRenderers
{
    #region

    using System;

    #endregion

    public class CallSiteLineNumberTests : NLogTestBase
    {

#if !SILVERLIGHT
        [Fact]
        public void LineNumberOnlyTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${callsite-linenumber} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            ILogger logger = LogManager.GetLogger("A");
#line 100000
            logger.Debug("msg");
            var lastMessage = GetDebugLastMessage("debug");
            // There's a difference in handling line numbers between .NET and Mono
            // We're just interested in checking if it's above 100000
            Assert.True(lastMessage.IndexOf("10000", StringComparison.OrdinalIgnoreCase) == 0, "Invalid line number. Expected prefix of 10000, got: " + lastMessage);
#line default
        }
#endif
    }
}