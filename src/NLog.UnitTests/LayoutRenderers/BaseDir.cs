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
	public class BaseDir : NLogTestBase
	{
        [Test]
        public void BaseDirTest()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${basedir} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' appendTo='debug' />
                </rules>
            </nlog>");

            LogManager.Configuration = new XmlLoggingConfiguration(doc.DocumentElement, null);

            Logger logger = LogManager.GetLogger("A");
            logger.Debug("a");
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            AssertDebugLastMessage("debug", baseDir + " a");
        }

        [Test]
        public void BaseDirCombineTest()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${basedir:dir=..} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' appendTo='debug' />
                </rules>
            </nlog>");

            LogManager.Configuration = new XmlLoggingConfiguration(doc.DocumentElement, null);

            Logger logger = LogManager.GetLogger("A");
            logger.Debug("a");
            string baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..");
            AssertDebugLastMessage("debug", baseDir + " a");
        }
    }
}
