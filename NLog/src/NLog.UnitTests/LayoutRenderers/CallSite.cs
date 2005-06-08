using System;
using System.Xml;
using System.Reflection;
using System.Diagnostics;

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
#line 100000
            logger.Debug("msg");
            string lastMessage = GetDebugLastMessage("debug");
            // There's a difference in handling line numbers between .NET and Mono
            // We're just interested in checking if it's above 100000
            Assert.IsTrue(lastMessage.ToLower().IndexOf("callsite.cs:10000") >= 0, "Invalid line number. Expected prefix of 10000, got: " + lastMessage);
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
