<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="getData.aspx.cs" Inherits="ShowData.getData" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    数据库的连接<br />
    <br />
    <asp:GridView ID="showData" runat="server" AutoGenerateColumns="False" Height="183px" Width="447px" >
        <Columns>
            <asp:BoundField DataField="TNAME" HeaderText="公司名称" />
            <asp:BoundField DataField="TINTRO" HeaderText="公司简介" />
        </Columns>
    </asp:GridView>

    </form>
</body>
</html>