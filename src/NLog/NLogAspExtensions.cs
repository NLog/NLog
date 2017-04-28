using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using System.IO;

namespace NLog.Asp
{
    public static class NLogAspExtensions
    {
        public static void ConfigureNLog(this IHostingEnvironment env, string configFileRelativePath)
        {
            NLog.LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(Path.Combine(Directory.GetParent(env.WebRootPath).ToString(), configFileRelativePath), true);
        }

        public static void ConfigureNLog(this IApplicationEnvironment appEnv, string configFileRelativePath)
        {
            NLog.LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(Path.Combine(appEnv.ApplicationBasePath, configFileRelativePath), true);
        }

        public static void AddNLog(this ILoggerFactory loggerFactory)
        {
            loggerFactory.AddProvider(new NLogProvider());
        }
    }
}
