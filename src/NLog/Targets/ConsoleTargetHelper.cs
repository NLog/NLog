using System;
using System.IO;
using NLog.Common;

namespace NLog.Targets
{
    internal static class ConsoleTargetHelper
    {
        public static bool IsConsoleAvailable(out string reason)
        {
#if !SILVERLIGHT && !__IOS__ && !__ANDROID__ && !MONO
            try
            {
                if (!Environment.UserInteractive)
                {
                    reason = "Environment.UserInteractive = False";
                    return false;
                }
                else if (Console.OpenStandardInput(1) == Stream.Null)
                {
                    reason = "Console.OpenStandardInput = Null";
                    return false;
                }
            }
            catch (Exception ex)
            {
                reason = string.Format("Unexpected exception: {0}:{1}", ex.GetType().Name, ex.Message);
                InternalLogger.Warn(ex, "Failed to detect whether console is available.");
                return false;
            }
#endif
            reason = string.Empty;
            return true;
        }
    }
}
