﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" AutoEventWireup="true" Inherits="System.Web.Mvc.ViewPage" %>
<asp:Content ID="Content3" ContentPlaceHolderID="HeaderContent" runat="server">
    <title>Create Player</title>
    <script type="text/javascript">
    //<![CDATA[
        function UpdateRaceImage() {
            var raceSelect = document.getElementById('raceId');
            var raceOption = raceSelect.options[raceSelect.selectedIndex];
            document.getElementById('RaceImg').src = '../Content/Races/' + raceOption.text + '.jpg';
            //setting the display style to "none" for all racial divs
            document.getElementById("SkummWeapon").style.display = "none";
            document.getElementById("SkummShield").style.display = "none";
            document.getElementById("SkummAccuracy").style.display = "none";
            document.getElementById("SkummEngine").style.display = "none";
            document.getElementById("SkummHomeSystem").style.display = "none";
            document.getElementById("SkummDescription").style.display = "none";
            document.getElementById("SkummRacialEnemy").style.display = "none";
            document.getElementById("SkummRacialPreference").style.display = "none";

            document.getElementById("DecapodianWeapon").style.display = "none";
            document.getElementById("DecapodianShield").style.display = "none";
            document.getElementById("DecapodianAccuracy").style.display = "none";
            document.getElementById("DecapodianEngine").style.display = "none";
            document.getElementById("DecapodianHomeSystem").style.display = "none";
            document.getElementById("DecapodianDescription").style.display = "none";
            document.getElementById("DecapodianRacialEnemy").style.display = "none";
            document.getElementById("DecapodianRacialPreference").style.display = "none";

            document.getElementById("BinariteWeapon").style.display = "none";
            document.getElementById("BinariteShield").style.display = "none";
            document.getElementById("BinariteAccuracy").style.display = "none";
            document.getElementById("BinariteEngine").style.display = "none";
            document.getElementById("BinariteHomeSystem").style.display = "none";
            document.getElementById("BinariteDescription").style.display = "none";
            document.getElementById("BinariteRacialEnemy").style.display = "none";
            document.getElementById("BinariteRacialPreference").style.display = "none";

            document.getElementById("ShrodinoidWeapon").style.display = "none";
            document.getElementById("ShrodinoidShield").style.display = "none";
            document.getElementById("ShrodinoidAccuracy").style.display = "none";
            document.getElementById("ShrodinoidEngine").style.display = "none";
            document.getElementById("ShrodinoidHomeSystem").style.display = "none";
            document.getElementById("ShrodinoidDescription").style.display = "none";
            document.getElementById("ShrodinoidRacialEnemy").style.display = "none";
            document.getElementById("ShrodinoidRacialPreference").style.display = "none";

            document.getElementById("HumanWeapon").style.display = "none";
            document.getElementById("HumanShield").style.display = "none";
            document.getElementById("HumanAccuracy").style.display = "none";
            document.getElementById("HumanEngine").style.display = "none";
            document.getElementById("HumanHomeSystem").style.display = "none";
            document.getElementById("HumanDescription").style.display = "none";
            document.getElementById("HumanRacialEnemy").style.display = "none";
            document.getElementById("HumanRacialPreference").style.display = "none";

            //setting the display style to "inline" for the selected race's divs
            if (raceOption.value == 1)//if true, Skumm has been selected
            {
                document.getElementById("SkummWeapon").style.display = "inline";
                document.getElementById("SkummShield").style.display = "inline";
                document.getElementById("SkummAccuracy").style.display = "inline";
                document.getElementById("SkummEngine").style.display = "inline";
                document.getElementById("SkummHomeSystem").style.display = "inline";
                document.getElementById("SkummDescription").style.display = "inline";
                document.getElementById("SkummRacialEnemy").style.display = "inline";
                document.getElementById("SkummRacialPreference").style.display = "inline";
            }
            else if (raceOption.value == 2)//if true, Decapodian has been selected
            {
                document.getElementById("DecapodianWeapon").style.display = "inline";
                document.getElementById("DecapodianShield").style.display = "inline";
                document.getElementById("DecapodianAccuracy").style.display = "inline";
                document.getElementById("DecapodianEngine").style.display = "inline";
                document.getElementById("DecapodianHomeSystem").style.display = "inline";
                document.getElementById("DecapodianDescription").style.display = "inline";
                document.getElementById("DecapodianRacialEnemy").style.display = "inline";
                document.getElementById("DecapodianRacialPreference").style.display = "inline";
            }
            else if (raceOption.value == 4)//if true, Shrodinoid has been selected
            {
                document.getElementById("ShrodinoidWeapon").style.display = "inline";
                document.getElementById("ShrodinoidShield").style.display = "inline";
                document.getElementById("ShrodinoidAccuracy").style.display = "inline";
                document.getElementById("ShrodinoidEngine").style.display = "inline";
                document.getElementById("ShrodinoidHomeSystem").style.display = "inline";
                document.getElementById("ShrodinoidDescription").style.display = "inline";
                document.getElementById("ShrodinoidRacialEnemy").style.display = "inline";
                document.getElementById("ShrodinoidRacialPreference").style.display = "inline";
            }
            else if (raceOption.value == 3)//if true, Binarite has been selected
            {
                document.getElementById("BinariteWeapon").style.display = "inline";
                document.getElementById("BinariteShield").style.display = "inline";
                document.getElementById("BinariteAccuracy").style.display = "inline";
                document.getElementById("BinariteEngine").style.display = "inline";
                document.getElementById("BinariteHomeSystem").style.display = "inline";
                document.getElementById("BinariteDescription").style.display = "inline";
                document.getElementById("BinariteRacialEnemy").style.display = "inline";
                document.getElementById("BinariteRacialPreference").style.display = "inline";
            }
            else if (raceOption.value == 5)//if true, Human has been selected
            {
                document.getElementById("HumanWeapon").style.display = "inline";
                document.getElementById("HumanShield").style.display = "inline";
                document.getElementById("HumanAccuracy").style.display = "inline";
                document.getElementById("HumanEngine").style.display = "inline";
                document.getElementById("HumanHomeSystem").style.display = "inline";
                document.getElementById("HumanDescription").style.display = "inline";
                document.getElementById("HumanRacialEnemy").style.display = "inline";
                document.getElementById("HumanRacialPreference").style.display = "inline";
            }

        }
        window.onload = UpdateRaceImage;
     //]]>
    </script>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div>
        <p>
            <span class="cp-topLines">
            Welcome! Before you can begin playing, you 
            need to set up a player profile.
            </span>
        </p>
        <p>
            <span class="cp-topLines">
            We strongly suggest that you click on "Help" before you click the "Create Player" button at the bottom of this page.  
            The "Help" page contains important information that you need to know before you create your player profile.
            </span>
        </p>
        <%= Html.ValidationSummary()%>
        <% using (Html.BeginForm())
           { %>
            <table class="cp-table">
                <tr>
                    <td class="cp-leftHeaders">Name:</td>
                    <td class="cp-leftData">
                        <%= Html.TextBox("name", ViewData["name"], new { maxlength = 255})%>
                        <%= Html.ValidationMessage("name")%>
                    </td>
                    <td rowspan="3" style="width: 40%" align="center">
                        <img id="RaceImg" src="" alt="Race Image" width="100" height="100" style="margin-left: 0px"/>
                    </td>
                    <td  class="cp-rightHeaders">Home System:</td>
                    <% 
string[,] races = new string[6, 9];
int i = 1;
foreach (Race race in (Race[])ViewData["Races"])
{
    races[i, 1] = race.Weapons.ToString();
    races[i, 2] = race.Shields.ToString();
    races[i, 3] = race.Engine.ToString();
    races[i, 4] = race.Accuracy.ToString();
    races[i, 5] = race.HomeSystem.Name;
    races[i, 6] = race.Description;
    if (race.RacialEnemy != null)
    {
        races[i, 7] = race.RacialEnemy.Name;
    }
    else
    {
        races[i, 7] = "None";
    }
    if (race.RacialPreference != null)
    {
        races[i, 8] = race.RacialPreference.Name;
    }
    else
    {
        races[i, 8] = "None";
    }
    i++;
}
                    %>
                    <td class="cp-rightData">
                        <div class="race" id="SkummHomeSystem"><%= races[1, 5]%></div>
                        <div class="race" id="DecapodianHomeSystem"><%= races[2, 5]%></div>
                        <div class="race" id="ShrodinoidHomeSystem"><%= races[4, 5]%></div>
                        <div class="race" id="BinariteHomeSystem"><%= races[3, 5]%></div>
                        <div class="race" id="HumanHomeSystem"><%= races[5, 5]%></div>
                    </td>
                </tr>
                <tr>
                    <td></td>
                    <td></td>
                    <td class="cp-rightHeaders">Racial Preference</td>
                    <td class="cp-rightData">
                        <div class="race" id="SkummRacialPreference"><%= races[1, 8]%></div>
                        <div class="race" id="DecapodianRacialPreference"><%= races[2, 8]%></div>
                        <div class="race" id="ShrodinoidRacialPreference"><%= races[4, 8]%></div>
                        <div class="race" id="BinariteRacialPreference"><%= races[3, 8]%></div>
                        <div class="race" id="HumanRacialPreference"><%= races[5, 8]%></div>                    
                    </td>
                </tr>
                <tr>
                    <td class=".cp-leftHeaders">Race:</td>
                    <td class="cp-leftData">
                        <%= Html.DropDownList("raceId", (SelectList)ViewData["raceId"], new { onchange = "UpdateRaceImage();" })%>
                        <%= Html.ValidationMessage("raceId")%> 
                    </td>
                    <td class="cp-rightHeaders">Racial Enemy</td>
                    <td class="cp-rightData">
                        <div class="race" id="SkummRacialEnemy"><%= races[1, 7]%></div>
                        <div class="race" id="DecapodianRacialEnemy"><%= races[2, 7]%></div>
                        <div class="race" id="ShrodinoidRacialEnemy"><%= races[4, 7]%></div>
                        <div class="race" id="BinariteRacialEnemy"><%= races[3, 7]%></div>
                        <div class="race" id="HumanRacialEnemy"><%= races[5, 7]%></div>
                    </td>
                </tr>     
              </table>
              <table class="cp-table">    
               <tr><td>&nbsp;</td></tr> 
                <%--Racial Modifiers --%>
                <tr>
                    <td class="cp-racialMods">Racial Modifiers</td>
                    <td class="cp-racialMods">Weapon</td>
                    <td class="cp-racialMods">Shield</td>
                    <td class="cp-racialMods">Accuracy</td>
                    <td class="cp-racialMods">Engine</td> 
                </tr>
                <tr>
                    <td class="cp-racialMods">Value</td>
                    <td class="cp-racialMods">
                        <div class="race" id="SkummWeapon"><%= races[1, 1]%></div>
                        <div class="race" id="DecapodianWeapon"><%= races[2, 1]%></div>
                        <div class="race" id="ShrodinoidWeapon"><%= races[4, 1]%></div>
                        <div class="race" id="BinariteWeapon"><%= races[3, 1]%></div>
                        <div class="race" id="HumanWeapon"><%= races[5, 1]%></div>
                    </td>
                    <td class="cp-racialMods">
                        <div class="race" id="SkummShield"><%= races[1, 2]%></div>
                        <div class="race" id="DecapodianShield"><%= races[2, 2]%></div>
                        <div class="race" id="ShrodinoidShield"><%= races[4, 2]%></div>
                        <div class="race" id="BinariteShield"><%= races[3, 2]%></div>
                        <div class="race" id="HumanShield"><%= races[5, 2]%></div>
                    </td>
                    <td class="cp-racialMods">
                        <div class="race" id="SkummAccuracy"><%= races[1, 4]%></div>
                        <div class="race" id="DecapodianAccuracy"><%= races[2, 4]%></div>
                        <div class="race" id="ShrodinoidAccuracy"><%= races[4, 4]%></div>
                        <div class="race" id="BinariteAccuracy"><%= races[3, 4]%></div>
                        <div class="race" id="HumanAccuracy"><%= races[5, 4]%></div>
                    </td>
                    <td class="cp-racialMods">
                        <div class="race" id="SkummEngine"><%= races[1, 3]%></div>
                        <div class="race" id="DecapodianEngine"><%= races[2, 3]%></div>
                        <div class="race" id="ShrodinoidEngine"><%= races[4, 3]%></div>
                        <div class="race" id="BinariteEngine"><%= races[3, 3]%></div>
                        <div class="race" id="HumanEngine"><%= races[5, 3]%></div>
                    </td> 
                </tr>
                <tr><td>&nbsp;</td></tr>
                <tr><td  align="center" colspan="5">Description</td></tr>
                <tr>
                    <td colspan="5" align="left">
                        <div class="race" id="SkummDescription"><%= races[1, 6]%></div>
                        <div class="race" id="DecapodianDescription"><%= races[2, 6]%></div>
                        <div class="race" id="ShrodinoidDescription"><%= races[4, 6]%></div>
                        <div class="race" id="BinariteDescription"><%= races[3, 6]%></div>
                        <div class="race" id="HumanDescription"><%= races[5, 6]%></div>
                    </td> 
                </tr>
                <tr><td>&nbsp;</td></tr>
                <tr>
                    <td colspan="5" align="center"><input type="submit" value="Create Player" /></td>
                </tr>
              </table>
        <% } %>
    </div>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="FooterContent" runat="server">
</asp:Content>
