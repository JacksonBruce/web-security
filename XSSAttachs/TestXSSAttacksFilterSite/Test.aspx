<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeBehind="Test.aspx.cs" Inherits="TestXSSAttacksFilterSite.Test" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
<style>
    .column{position:fixed;top:0;bottom:50%;left:0;right:0;overflow:auto;display:block;border:none;padding:0;white-space:nowrap;}
    .column.right{border-top:solid 1px #ccc;top:50%;bottom:30px;overflow:hidden;}
    .column > textarea{display:block;width:100%;height:100%;border:none;overflow:auto;padding:0;}
</style>
</head>
<body>
    <form id="form1" runat="server">
        
    <div class="column"><%= html==null?null:HttpUtility.HtmlEncode(html).Replace("\n","<br />") %></div>
    <div class="column right">
    <asp:TextBox TextMode="MultiLine" id="txt" runat="server">


    </asp:TextBox></div>
    <div style="position:fixed;bottom:0;left:0;right:0;height:30px;text-align:center;">
        <asp:LinkButton ID="btn" runat="server" OnClick="btn_Click">submit</asp:LinkButton>
    </div>
    </form>
</body>
</html>
