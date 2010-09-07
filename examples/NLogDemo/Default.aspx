<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="NLogDemo._Default" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>NLog Demo</title>
    <link rel="Stylesheet" href="style.css" />
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <img src="NLog.png" alt="NLog - Advanced .NET Logging" />
    </div>
    <h1>NLog Demo</h1>
    <p>
    This demo shows an ASP.NET web application with multiple log outputs:
    </p>
    <ul>
    <li><b>Mail</b> - logs from each HTTP request are sent to GMail account.<br />You can specify GMail username in <b><%= Server.MapPath("~/gmailusername.txt")%></b> and password <b><%= Server.MapPath("~/gmailpassword.txt")%></b></li>
    <li><b>Database</b> - logs are written to <code>LogEntries</code> tables in <code>MyLogs</code> database in local <code>SQLEXPRESS</code> database</li>
    <li><b>File</b> - log files are written to <b><a href="<%= new Uri(Server.MapPath("~/logs/")) %>"><%= Server.MapPath("~/logs") %></a></b> directory.</li>
    <li><b>Event Log</b> - logs are written to Application log using <code>NLog Demo</code> source name.</li>
    <li><b>Performance Counter</b> - each time log message is written, the performance counter <b>My Log\My Counter</b> is incremented.</li>
    </ul>
    <p>
    <b>NOTE:</b> Some of the log outputs (database, event log source) must be installed before logs can be written to them.
    To do this, open elevated command prompt and run:
    </p>
    <pre>InstallNLogConfig.exe <%= Server.MapPath("NLog.config") %></pre>
    <p>

     NLog configuration file can be found at:
    <b><%= Server.MapPath("~/NLog.config") %></b>. You can modify the file at any time and logging configuration will 
    be automatically reloaded.</p>
    <h1>Simple calculator</h1>
    <p style="padding: 10px; color: #707070">
    Enter two openrands and operator to compute the result of arithmetic operation.
    <br />
    You may introduce errors (such as divide by zero) and see what gets written to the log. 
    Under normal circumstances only messages with log level Info or above are written, but whenever the error occurs more detailed trace is logged, which includes all messages 

    </p>
    <div>
        Operand 1:
        <asp:TextBox ID="textboxOperand1" runat="server" AutoPostBack="true" />

        Operator:
        <asp:DropDownList ID="dropdownOperator" runat="server" AutoPostBack="true">
            <asp:ListItem Text="+" Value="Add" />
            <asp:ListItem Text="-" Value="Subtract" />
            <asp:ListItem Text="*" Value="Multiply" />
            <asp:ListItem Text="/" Value="Divide" />
            <asp:ListItem Text="%" Value="Modulo" />
        </asp:DropDownList>

        Operand 2:
        <asp:TextBox ID="textboxOperand2" runat="server" AutoPostBack="true" />

        <br />
        Result:
        <asp:TextBox ID="textboxResult" runat="server" />

        <asp:Label ID="labelError" runat="server" />
    </div>
    </form>
</body>
</html>
