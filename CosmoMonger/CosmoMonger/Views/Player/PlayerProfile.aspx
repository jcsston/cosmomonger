<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" AutoEventWireup="true" Inherits="System.Web.Mvc.ViewPage" %>
<asp:Content ID="Content3" ContentPlaceHolderID="HeaderContent" runat="server">
    <title>View Player Profile</title>
    <script type="text/javascript" src="/Scripts/jquery.confirm-1.2.js"></script>
    <script type="text/javascript">
    //<![CDATA[
        $(document).ready(function() {
            $(":submit").confirm();
        });
    //]]>
    </script>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<h1>Player Profile</h1>
<table style="width: 100%">
<% CosmoMonger.Models.Player player = (CosmoMonger.Models.Player)ViewData["Player"]; %>
<tr>
    <td class="vp-playerName" colspan="4"><%= Html.Encode(player.Name)%></td>
</tr>
<tr>
    <td colspan="4">&nbsp;</td>
</tr>
<tr>
    <td class="vp-headers" colspan="2">Financial Data</td>
    <td class="vp-headers" colspan="2">Racial Data</td>
</tr>
<tr>
    <td class="vp-columnData"><u>Net Worth:</u></td>
    <td class="vp-columnData"><u><%= player.NetWorth.ToString("C0")%></u></td>
    <td class="vp-columnData">Player's Race:</td>
    <td class="vp-columnData"><span id="playerRace"><%=Html.Encode(player.Race.Name) %></span></td>
</tr>
<tr>
    <td class="vp-columnData">Credits:</td>
    <td class="vp-columnData"><%= player.Ship.Credits.ToString("C0")%></td>
    <td class="vp-columnData">Racial Preference:</td>
    <% string racePref;
       if (player.Race.RacialPreference == null)
       {
           racePref = "None";
       }
       else
       {
           racePref = player.Race.RacialPreference.Name + "s";
       }
           
    %>
    <td class="vp-columnData"><%=racePref%></td>
</tr>
<tr>
    <td class="vp-columnData">Bank Credits:</td>
    <td class="vp-columnData"><%= player.BankCredits.ToString("C0") %></td>
    <td class="vp-columnData">Racial Enemy:</td>
    <% string raceEnemy;
           if (player.Race.RacialEnemy == null)
       {
           raceEnemy = "None";
       }
       else
       {
           raceEnemy = player.Race.RacialEnemy.Name + "s";
       }
           
    %>
    <td class="vp-columnData"><%=raceEnemy%></td>
</tr>
<tr>
    <td class="vp-columnData">Ship Trade-In Value:</td>
    <td class="vp-columnData"><%= player.Ship.TradeInValue.ToString("C0") %></td>
</tr>
<tr>
    <td class="vp-columnData">Cargo Value:</td>
    <td class="vp-columnData"><%= player.Ship.CargoWorth.ToString("C0") %></td>
</tr>
<tr>
    <td colspan="4">&nbsp;</td>
</tr>
<tr>
    <td class="vp-headers" colspan="2">Want A New Player?</td>
    <td class="vp-headers" colspan="2">Time Played</td>
</tr>
<tr>
    <td align="center" colspan="2">Warning! This is irrevisible!</td>
    <td align="center" colspan="2">Time Limit = 168 hours</td>
</tr>
<tr>
    <td align="center" colspan="2">
<%
using (Html.BeginForm("KillPlayer", "Player", FormMethod.Post))
{ 
%>
    <div>
        <input type="hidden" name="playerId" value="<%=player.PlayerId %>" /> 
        <input type="submit" value="Kill Current Player" />
    </div>
<% 
}
%>
    </td>
    <% 
    double hours = (player.TimePlayed) / (3600);
    string fHours = hours.ToString("n1"); 
    %>
    <td class="vp-columnData"><%=fHours%></td>
    <td class="vp-columnData">hours</td>
</tr>
</table>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="FooterContent" runat="server">
</asp:Content>
