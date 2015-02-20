<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TestPolicy.aspx.cs" Inherits="TestXSSAttacksFilterSite.TestPolicy" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
    <style>
    .container
    {position:absolute;top:0;left:0;right:0;overflow:hidden;bottom:0;}
    .txt{height:100%;width:100%;margin:0;display:block;border:none;}
</style>
</head>
<body>
    <form id="form1" runat="server">
    <div class="container">
    <textarea class="txt">
        <%= txt %>
    </textarea>
    </div>
    </form>
</body>
</html>
