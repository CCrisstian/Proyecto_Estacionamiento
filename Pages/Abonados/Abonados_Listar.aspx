<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Abonados_Listar.aspx.cs" Inherits="Proyecto_Estacionamiento.Pages.Abonados.Abonados_Listar" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="header-row">
        <h2>
            <asp:Literal ID="TituloAbonados" runat="server" />
        </h2>
    </div>

    <div style="text-align: center; margin-top: 20px;">
        <img src='<%= ResolveUrl("~/Images/EnConstruccion.png") %>'
            alt="En construcción"
            style="max-width: 700px; width: 100%; height: auto;" />
    </div>
</asp:Content>

