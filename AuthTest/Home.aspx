<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="AuthTest.Home" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    
    </div>
    <asp:Label ID="Label1" runat="server" Text="Testsida för inloggning"></asp:Label>
    <asp:Label ID="usrName" runat="server" Text="Oinloggad"></asp:Label>
    <p>
        <asp:HyperLink ID="HyperLink2" runat="server" 
            NavigateUrl="/ExtranetUserAuthentication?ReturnUrl=authTest/Home.aspx">Sign in</asp:HyperLink>
            &nbsp;&nbsp;&nbsp;
        <asp:HyperLink ID="HyperLink1" runat="server" 
            NavigateUrl="/ExtranetUserAuthentication/SignOut?returnUrl=/authTest/Home.aspx">Sign out</asp:HyperLink>
    </p>
    </form>
</body>
</html>
