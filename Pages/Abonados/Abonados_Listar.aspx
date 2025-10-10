﻿<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Abonados_Listar.aspx.cs" Inherits="Proyecto_Estacionamiento.Pages.Abonados.Abonados_Listar" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="header-row">
        <h2>Abonados</h2>
        <asp:Label ID="Estacionamiento_Nombre" runat="server" CssClass="right-text"></asp:Label>
    </div>
    
    <asp:Button ID="btnRegistrar" runat="server" Text="Registrar Abonado" OnClick="btnRegistrar_Click" CssClass="btn btn-success" />
    <br />
    <br />

    <div style="text-align: center; margin-top: 20px;">
        <img src='<%= ResolveUrl("~/Images/EnConstruccion.png") %>'
            alt="En construcción"
            style="max-width: 700px; width: 100%; height: auto;" />
    </div>
</asp:Content>

