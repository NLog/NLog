using System;
using System.Xml;

using NLog;
using NLog.Config;

using NUnit.Framework;

namespace NLog.UnitTests
{
    [TestFixture]
	public class RoutingTests : NLogTestBase
	{
        [Test]
        public void LogThresholdTest()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Info' appendTo='debug' />
                </rules>
            </nlog>");

            LogManager.Configuration = new XmlLoggingConfiguration(doc.DocumentElement, null);

            Logger logger = LogManager.GetLogger("A");
            logger.Debug("message");
            AssertDebugCounter("debug", 0);

            logger.Info("message");
            AssertDebugCounter("debug", 1);

            logger.Warn("message");
            AssertDebugCounter("debug", 2);

            logger.Error("message");
            AssertDebugCounter("debug", 3);

            logger.Fatal("message");
            AssertDebugCounter("debug", 4);
        }

        [Test]
        public void LogThresholdTest2()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(@"
            <nlog>
                <targets>
                    <target name='debug1' type='Debug' layout='${message}' />
                    <target name='debug2' type='Debug' layout='${message}' />
                    <target name='debug3' type='Debug' layout='${message}' />
                    <target name='debug4' type='Debug' layout='${message}' />
                    <target name='debug5' type='Debug' layout='${message}' />
                </targets>
                <rules>
                    <logger name='*' minlevel='Debug' appendTo='debug1' />
                    <logger name='*' minlevel='Info' appendTo='debug2' />
                    <logger name='*' minlevel='Warn' appendTo='debug3' />
                    <logger name='*' minlevel='Error' appendTo='debug4' />
                    <logger name='*' minlevel='Fatal' appendTo='debug5' />
                </rules>
            </nlog>");

            LogManager.Configuration = new XmlLoggingConfiguration(doc.DocumentElement, null);

            Logger logger = LogManager.GetLogger("A");

            logger.Fatal("messageE");
            logger.Error("messageD");
            logger.Warn("messageC");
            logger.Info("messageB");
            logger.Debug("messageA");

            AssertDebugCounter("debug1", 5);
            AssertDebugCounter("debug2", 4);
            AssertDebugCounter("debug3", 3);
            AssertDebugCounter("debug4", 2);
            AssertDebugCounter("debug5", 1);

            AssertDebugLastMessage("debug1", "messageA");
            AssertDebugLastMessage("debug2", "messageB");
            AssertDebugLastMessage("debug3", "messageC");
            AssertDebugLastMessage("debug4", "messageD");
            AssertDebugLastMessage("debug5", "messageE");
        }

        [Test]
        public void LoggerNameMatchTest()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(@"
            <nlog>
                <targets>
                    <target name='debug1' type='Debug' layout='${message}' />
                    <target name='debug2' type='Debug' layout='${message}' />
                    <target name='debug3' type='Debug' layout='${message}' />
                    <target name='debug4' type='Debug' layout='${message}' />
                </targets>
                <rules>
                    <logger name='A' minlevel='Info' appendTo='debug1' />
                    <logger name='A*' minlevel='Info' appendTo='debug2' />
                    <logger name='*A*' minlevel='Info' appendTo='debug3' />
                    <logger name='*A' minlevel='Info' appendTo='debug4' />
                </rules>
            </nlog>");

            LogManager.Configuration = new XmlLoggingConfiguration(doc.DocumentElement, null);

            LogManager.GetLogger("A").Info("message"); // matches 1st, 2nd, 3rd and 4th rule
            LogManager.GetLogger("A2").Info("message"); // matches 2nd rule and 3rd rule
            LogManager.GetLogger("BAD").Info("message"); // matches 3rd rule
            LogManager.GetLogger("BA").Info("message"); // matches 3rd and 4th rule

            AssertDebugCounter("debug1", 1);
            AssertDebugCounter("debug2", 2);
            AssertDebugCounter("debug3", 4);
            AssertDebugCounter("debug4", 2);
        }

        [Test]
        public void MultiAppenderTest()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(@"
            <nlog>
                <targets>
                    <target name='debug1' type='Debug' layout='${message}' />
                    <target name='debug2' type='Debug' layout='${message}' />
                    <target name='debug3' type='Debug' layout='${message}' />
                    <target name='debug4' type='Debug' layout='${message}' />
                </targets>
                <rules>
                    <logger name='A' minlevel='Info' appendTo='debug1' />
                    <logger name='A' minlevel='Info' appendTo='debug2' />
                    <logger name='B' minlevel='Info' appendTo='debug1,debug2' />
                    <logger name='C' minlevel='Info' appendTo='debug1,debug2,debug3' />
                    <logger name='D' minlevel='Info' appendTo='debug1,debug2' />
                    <logger name='D' minlevel='Info' appendTo='debug3,debug4' />
                </rules>
            </nlog>");

            LogManager.Configuration = new XmlLoggingConfiguration(doc.DocumentElement, null);

            LogManager.GetLogger("A").Info("message");
            LogManager.GetLogger("B").Info("message");
            LogManager.GetLogger("C").Info("message");
            LogManager.GetLogger("D").Info("message");

            AssertDebugCounter("debug1", 4);
            AssertDebugCounter("debug2", 4);
            AssertDebugCounter("debug3", 2);
            AssertDebugCounter("debug4", 1);
        }
    }
}
