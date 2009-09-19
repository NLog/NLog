using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NLog.Config;
using NLog.Targets;

namespace NLog.BinaryCompatTests
{
    public static class TargetTests
    {
        public static void CreateTargetTest()
        {
            Target t = new ConsoleTarget();
        }
    }
}
