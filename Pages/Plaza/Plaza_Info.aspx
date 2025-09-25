<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Plaza_Info.aspx.cs" Inherits="Proyecto_Estacionamiento.Pages.Plaza.Plaza_Info" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="header-row">
        <h2>Plazas Información</h2>
        <asp:Label ID="Estacionamiento_Nombre" runat="server" CssClass="right-text"></asp:Label>
    </div>

    <br />
    <asp:Button ID="ButtonVolver" runat="server" Text="Volver"
        OnClick="btnVolver"
        CssClass="btn btn-primary" />
    <br />

    <asp:GridView ID="gvPlazas" runat="server" AutoGenerateColumns="False"
        CssClass="grid"
        Width="50%">

        <AlternatingRowStyle BackColor="White" />

        <Columns>

            <asp:BoundField DataField="Categoria" HeaderText="Categoría" />
            <asp:BoundField DataField="CantPlazasDisponibles" HeaderText="Cant. Disponible" />
        </Columns>

    </asp:GridView>

</asp:Content>
