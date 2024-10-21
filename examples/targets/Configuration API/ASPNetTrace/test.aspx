<%@ Page language="c#" AutoEventWireup="true" %>

<!DOCTYPE html>

<script language="C#" runat="server">
NLog.Logger logger = NLog.LogManager.GetLogger("default.aspx");

void Page_Load(Object sender, EventArgs e)
{
    logger.Info("got Page_Load() event");
    logger.Warn("Some warning...");
    logger.Error("Some error...");
}
</script>
<html lang="en">
<head>
    <title>ASP.NET Trace Test</title>
</head>
<body>
    <p>
        Page has been loaded and log events have been registered for display in ASP.NET Trace facility.
    </p>
    <p>
        <a href="Trace.axd">Click here to view trace output from Trace.axd</a>
    </p>
</body>
</html>
