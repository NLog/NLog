using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using NLog.Targets.Wrappers;
using NLog.Targets;
using NLog.Config;
using NLog;

namespace ASPNetBufferingWrapper
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            FileTarget fileTarget = new FileTarget();
            fileTarget.FileName = "${basedir}/logfile.txt";

            PostFilteringTargetWrapper postfilteringTarget = new PostFilteringTargetWrapper();
            ASPNetBufferingTargetWrapper aspnetBufferingTarget = new ASPNetBufferingTargetWrapper();
            aspnetBufferingTarget.WrappedTarget = postfilteringTarget;
            postfilteringTarget.WrappedTarget = fileTarget;

            postfilteringTarget.DefaultFilter = "level >= LogLevel.Info";
            FilteringRule rule = new FilteringRule();
            rule.Exists = "level >= LogLevel.Warn";
            rule.Filter = "level >= LogLevel.Debug";
            postfilteringTarget.Rules.Add(rule);

            SimpleConfigurator.ConfigureForTargetLogging(aspnetBufferingTarget, LogLevel.Debug);
        }

        protected void Application_End(object sender, EventArgs e)
        {
        }
    }
}