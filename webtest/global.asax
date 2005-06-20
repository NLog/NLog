<%@ Application %>
<script language="C#" runat="server">
NLog.Logger logger = NLog.LogManager.GetLogger("Global.Logger");

protected void Application_Start(Object sender, EventArgs e)
{
    logger.Info("Application_Start");
    Application["av"] = "appvalue1";
}

protected void Session_Start(Object sender, EventArgs e)
{
    logger.Info("Session_Start");
    Session["sv"] = "sessionvalue1";
}

protected void Application_BeginRequest(Object sender, EventArgs e)
{
    logger.Info("Application_BeginRequest");
}

protected void Application_EndRequest(Object sender, EventArgs e)
{
    logger.Info("Application_EndRequest");
}

protected void Application_AuthenticateRequest(Object sender, EventArgs e)
{
    logger.Info("Application_AuthenticateRequest");
}

protected void Application_Error(Object sender, EventArgs e)
{
    logger.Info("Application_Error");
}

protected void Session_End(Object sender, EventArgs e)
{
    logger.Info("Session_End");
}

protected void Application_End(Object sender, EventArgs e)
{
    logger.Info("Application_End");
}

protected void Application_AuthorizeRequest(object sender, EventArgs e)
{
    logger.Info("Application_AuthorizeRequest");
}

protected void Application_PreRequestHandlerExecute(object sender, EventArgs e)
{
    logger.Info("Application_PreRequestHandlerExecute");
}

protected void Application_PostRequestHandlerExecute(object sender, EventArgs e)
{
    HttpContext.Current.Trace.Write("aaa");
//throw new Exception("ex");
    logger.Info("Application_PostRequestHandlerExecute");
}
</script>
