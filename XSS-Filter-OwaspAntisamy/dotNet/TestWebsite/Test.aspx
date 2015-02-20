<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Test.aspx.cs" Inherits="TestWebsite.Test" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
<style>
    .txt
    {height:600px;width:90%;margin:0 auto;display:block;border:none;}
</style>
</head>
<body>
    <form id="form1" runat="server">
    <div>
 <textarea class="txt">
     <%= txt %>
 </textarea>
    </div>
    </form>
</body>
</html>
