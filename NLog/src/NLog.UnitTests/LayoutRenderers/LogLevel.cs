using System;
using System.Xml;
using System.Reflection;
using System.IO;

using NLog;
using NLog.Config;

using NUnit.Framework;

namespace NLog.UnitTests.LayoutRenderers
{
    [TestFixture]
	public class LogLevel : NLogTestBase
	{
        [Test]
        public void LogLevelTest()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${level} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' appendTo='debug' />
                </rules>
            </nlog>");

            LogManager.Configuration = new XmlLoggingConfiguration(doc.DocumentElement, null);

            Logger logger = LogManager.GetLogger("A");
            logger.Debug("a");
            AssertDebugLastMessage("debug", "Debug a");
            logger.Info("a");
            AssertDebugLastMessage("debug", "Info a");
            logger.Warn("a");
            AssertDebugLastMessage("debug", "Warn a");
            logger.Error("a");
            AssertDebugLastMessage("debug", "Error a");
            logger.Fatal("a");
            AssertDebugLastMessage("debug", "Fatal a");
        }
    }
}
