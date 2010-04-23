<%@ language="JScript" %>
<%
Response.Expires = -1;

<!-- this uses a ASP response target --> 
var logger = new ActiveXObject("NLog.Logger");
logger.LoggerName = "logger";
logger.Debug("message");
%>
