﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" AutoEventWireup="true" Inherits="System.Web.Mvc.ViewPage" %>
<asp:Content ID="Content3" ContentPlaceHolderID="HeaderContent" runat="server">
    <title>Sent Verification Code</title>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Sent Verification Code</h1>
    
    <p>Check your e-mail for a e-mail from cosmomonger.com and click the link in the e-mail</p>
    <p>You can also go <%= Html.ActionLink("here", "VerifyEmail")%> to enter in your verification code from the e-mail.</p>
    
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="FooterContent" runat="server">
</asp:Content>
