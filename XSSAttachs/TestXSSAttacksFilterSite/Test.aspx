<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeBehind="Test.aspx.cs" Inherits="TestXSSAttacksFilterSite.Test" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
<style>
    .column{position:fixed;top:0;bottom:50%;left:0;right:0;overflow:auto;display:block;border:none;padding:0;white-space:nowrap;}
    .column.right{border-top:solid 1px #ccc;top:50%;bottom:30px;overflow:hidden;}
    .richtext {position:absolute;top:35px;bottom:0;left:0;right:0;}
    .richtext > textarea{display:block;width:100%;height:100%;border:none;overflow:auto;padding:0;}
    .policy {line-height:35px;position:relative;}
    .policy > .txt {position:absolute;top:0;left:100px;bottom:0;right:0;}
    #txtPolicy {border:none;border-bottom:solid 1px #ccc;width:100%; }
</style>
</head>
<body>
    <form id="form1" runat="server">
        
    <div class="column"><%= html==null?null:HttpUtility.HtmlEncode(html).Replace("\n","<br />") %></div>
    <div class="column right">
    <div class="policy"><label for="txtPolicy">过滤策略：</label><div class="txt"><asp:TextBox ID="txtPolicy" runat="server"></asp:TextBox></div> </div>
        <div class="richtext"><asp:TextBox TextMode="MultiLine" id="txt" runat="server"></asp:TextBox></div>
    </div>
    <div style="position:fixed;bottom:0;left:0;right:0;height:30px;text-align:center;">
        <asp:LinkButton ID="btn" runat="server" OnClick="btn_Click">submit</asp:LinkButton>
    </div>
    </form>
</body>
</html>
