<%@ Page language="c#" AutoEventWireup="true" %>
<script language="C#" runat="server">
NLog.Logger logger = NLog.LogManager.GetLogger("default.aspx");

void Page_Load(Object sender, EventArgs e)
{
    logger.Info("test");

    Response.Write("loaded!");
}
</script>
