using System;
using System.Web;

using NLog;
using NLog.Config;
using NLog.Targets;

namespace SomeWebApplication
{
    public class Global : System.Web.HttpApplication
    {
        //
        // this event handler is executed at the very start of the web application
        // so this is a good place to configure targets programmatically
        // 
        // alternative you could place this code in a static type constructor
        //
        protected void Application_Start(Object sender, EventArgs e)
        {
            ASPNetTraceTarget target = new ASPNetTraceTarget();
            target.Layout = "${logger} ${message}";

            LoggingConfiguration nlogConfig = new LoggingConfiguration();
            nlogConfig.AddRuleForAllLevels(target);
            LogManager.Configuration = nlogConfig;
        }
    }
}
