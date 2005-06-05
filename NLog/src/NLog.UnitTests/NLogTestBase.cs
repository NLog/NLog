using System;

using NUnit.Framework;

namespace NLog.UnitTests
{
	public class NLogTestBase
	{
        public void AssertDebugCounter(string targetName, int val)
        {
            NLog.Targets.DebugTarget debugTarget = (NLog.Targets.DebugTarget)LogManager.Configuration.FindTargetByName(targetName);

            Assert.IsNotNull(debugTarget, "Debug target '" + targetName + "' not found");
            Assert.AreEqual(val, debugTarget.Counter, "Unexpected counter value on '" + targetName + "'");
        }

        public void AssertDebugLastMessage(string targetName, string msg)
        {
            NLog.Targets.DebugTarget debugTarget = (NLog.Targets.DebugTarget)LogManager.Configuration.FindTargetByName(targetName);

            Console.WriteLine("lastmsg: {0}", debugTarget.LastMessage);

            Assert.IsNotNull(debugTarget, "Debug target '" + targetName + "' not found");
            Assert.AreEqual(msg, debugTarget.LastMessage, "Unexpected last message value on '" + targetName + "'");
        }

        public string GetDebugLastMessage(string targetName)
        {
            NLog.Targets.DebugTarget debugTarget = (NLog.Targets.DebugTarget)LogManager.Configuration.FindTargetByName(targetName);
            return debugTarget.LastMessage;
        }
    }
}
