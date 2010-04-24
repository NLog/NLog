using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace ASPNetBufferingWrapper
{
    public partial class PageWithWarnings : System.Web.UI.Page
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        protected void Page_Load(object sender, EventArgs e)
        {
            // This page simulates a page with some warning
            // This causes the postfiltering wrapper to emit all the messages, including
            // debug ones.

            logger.Info("info1");
            logger.Debug("some debug message");
            logger.Debug("some other debug message");
            logger.Debug("yet another other debug message");
            logger.Warn("warning!");
            logger.Info("info2");
        }
    }
}
