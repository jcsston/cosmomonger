﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" AutoEventWireup="true" Inherits="System.Web.Mvc.ViewPage" CompilerOptions="/nowarn:0618" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeaderContent" runat="server">
    <title>Combat Complete</title>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<h1><%=Html.Encode(ViewData["Message"]) %></h1>
<h2 class="center">Results of Combat</h2>
<%
if (ViewData["CargoLooted"] != null)
{
    Html.Grid<CombatGood>(
        "CargoLooted",
        new Hash(empty => "No cargo looted", @class => "goods"),
        column =>
        {
            column.For(g => g.Good.Name, "Good Type");
            column.For(g => g.Quantity, "Quantity Found");
            column.For(g => g.QuantityPickedUp, "Quantity Picked Up");
            column.For(g => (g.QuantityPickedUp * g.Good.BasePrice).ToString("C0"), "Picked Up Cargo Worth");
        }
    );
    %><p class="center">Credits Looted: <%=((int)ViewData["CreditsLooted"]).ToString("C0")%></p><%
}
else if (ViewData["CargoLost"] != null)
{
    Html.Grid<CombatGood>(
        "CargoLost",
        new Hash(empty => "No cargo lost", @class => "goods"),
        column =>
        {
            column.For(g => g.Good.Name, "Good Type");
            column.For(g => g.Quantity, "Quantity Lost");
            column.For(g => (g.Quantity * g.Good.BasePrice).ToString("C0"), "Lost Cargo Worth");
        }
    );
    %><p class="center">Credits Lost: <%=((int)ViewData["CreditsLooted"]).ToString("C0")%></p><%
}
else if (ViewData["CargoSeized"] != null)
{
    Html.Grid<CombatGood>(
        "CargoSeized",
        new Hash(empty => "No contraband cargo found", @class => "goods"),
        column =>
        {
            column.For(g => g.Good.Name, "Good Type");
            column.For(g => g.Quantity, "Quantity Seized");
            column.For(g => (g.Quantity * g.Good.BasePrice).ToString("C0"), "Seized Cargo Worth");
        }
    );
    %><p class="center">Fine: <%=((int)ViewData["CreditsLooted"]).ToString("C0")%></p><%
}

if (ViewData["FinalImage"] != null)
{
    %><div class="center"><img src="/Content/<%=Url.Encode((string)ViewData["FinalImage"])%>" alt="Combat End" /></div><%
}
%>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="FooterContent" runat="server">
</asp:Content>
