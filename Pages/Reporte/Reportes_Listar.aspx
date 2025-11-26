<%@ Page Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="Reportes_Listar.aspx.cs" Inherits="Proyecto_Estacionamiento.Reporte.Reportes_Listar" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="header-row">
        <h2>Reportes</h2>
        <asp:Label ID="Estacionamiento_Nombre" runat="server" CssClass="right-text"></asp:Label>
    </div>

    <br />

    <div class="ingreso-layout">

        <asp:Button ID="ButtonReporteIngresos" runat="server" Text="Reporte de Ingresos"
            OnClick="btnReporteIngresos"
            CssClass="btn btn-warning" />

                <asp:Button ID="ButtonReporteIncidencias" runat="server" Text="Reporte de Incidencias"
            OnClick="btnReporteIncidencias"
            CssClass="btn btn-warning" />

                        <asp:Button ID="ButtonReporteCobros" runat="server" Text="Reporte de Cobros"
            OnClick="btnReporteCobros"
            CssClass="btn btn-warning" />
    </div>
</asp:Content>