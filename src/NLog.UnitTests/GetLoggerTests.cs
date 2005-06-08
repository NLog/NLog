using System;
using System.Xml;

using NLog;
using NLog.Config;

using NUnit.Framework;

namespace NLog.UnitTests
{
    [TestFixture]
	public class GetLoggerTests : NLogTestBase
	{
        private static Logger logger = LogManager.GetCurrentClassLogger();

        [Test]
        public void GetCurrentClassLoggerTest()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='abc' /></targets>
                <rules>
                    <logger name='NLog.UnitTests.GetLoggerTests' minlevel='Info' appendTo='debug' />
                </rules>
            </nlog>");

            LogManager.Configuration = new XmlLoggingConfiguration(doc.DocumentElement, null);

            logger.Info("message");
            AssertDebugLastMessage("debug", "abc");
        }
    }
}
