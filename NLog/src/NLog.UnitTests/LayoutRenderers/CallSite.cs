using System;
using System.Xml;
using System.Reflection;

using NLog;
using NLog.Config;

using NUnit.Framework;

namespace NLog.UnitTests.LayoutRenderers
{
    [TestFixture]
	public class CallSite : NLogTestBase
	{
        [Test]
        public void LineNumberTest()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${callsite:filename=true} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' appendTo='debug' />
                </rules>
            </nlog>");

            LogManager.Configuration = new XmlLoggingConfiguration(doc.DocumentElement, null);

            Logger logger = LogManager.GetLogger("A");
#line 10000
            logger.Debug("a");
            string lastMessage = GetDebugLastMessage("debug");
            Assert.IsTrue(lastMessage.ToLower().IndexOf("callsite.cs:10001") >= 0, "Invalid line number. Expected 10001, got: " + lastMessage);
#line default
        }

        [Test]
        public void MethodNameTest()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${callsite} ${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' appendTo='debug' />
                </rules>
            </nlog>");

            LogManager.Configuration = new XmlLoggingConfiguration(doc.DocumentElement, null);

            Logger logger = LogManager.GetLogger("A");
            logger.Debug("msg");
            MethodBase currentMethod = MethodBase.GetCurrentMethod();
            AssertDebugLastMessage("debug", currentMethod.DeclaringType.FullName + "." + currentMethod.Name + " msg");
        }
    }
}
