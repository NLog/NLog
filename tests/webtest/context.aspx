<%@ Page language="c#" AutoEventWireup="true" %>
<script language="C#" runat="server">
NLog.Logger logger = NLog.LogManager.GetLogger("Memory");

void Page_Load(Object sender, EventArgs e)
{
    Response.Expires = -1;
    NLog.Targets.MemoryTarget memoryTarget = NLog.LogManager.Configuration.FindTargetByName("memory") as NLog.Targets.MemoryTarget;

    memoryTarget.Logs.Clear();

    logger.Debug("message");

    foreach (string s in memoryTarget.Logs)
    {
        Response.Write(s);
    }
}
</script>
