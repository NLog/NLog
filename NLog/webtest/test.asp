<% @LANGUAGE="JScript" %>
<html>
<head>
</head>
<body>

<%
    Session("UserLogin") = "myuserlogin";
    Application("AppItem") = "someappitem";
    var t = new ActiveXObject("NLog.Logger");
    t.LoggerName = "zzz";
    t.Debug("aaa");
%>
POST FORM:

<form method="POST" action="test.asp">
<input type="text" name="kkk" />
<input type="submit" />
</form>

<hr />
GET FORM:
<form method="GET" action="test.asp">
<input type="text" name="kkk" />
<input type="submit" />
</form>

</body>
</html> 

