<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Reporte_Cobros.aspx.cs" Inherits="Proyecto_Estacionamiento.Pages.Reporte.Reporte_Cobros" %>

<%@ Register Assembly="Microsoft.ReportViewer.WebForms" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="header-row">
        <h2>Reporte de Cobros</h2>
        <asp:Label ID="Estacionamiento_Nombre" runat="server" CssClass="right-text"></asp:Label>
    </div>

    <br />

    <asp:Button ID="ButtonVolver" runat="server" Text="Volver"
        OnClick="btnVolver"
        CssClass="btn btn-primary" />

    <br />

    <div class="right-align-filters">
        <div class="filtro-container">

            <div class="filtro-grupo">

                <asp:Label ID="lblEstacionamiento" runat="server" Text="Estacionamiento:" Visible="false" />
                <asp:DropDownList ID="ddlEstacionamiento" runat="server" CssClass="form-control" Visible="false"
                    AutoPostBack="true" OnSelectedIndexChanged="ddlEstacionamiento_SelectedIndexChanged">
                </asp:DropDownList>

                <asp:CustomValidator ID="cvEstacionamiento" runat="server"
                OnServerValidate="CvEstacionamiento_ServerValidate"
                Display="Dynamic" ForeColor="Red"
                ValidationGroup="Reporte" />

                <asp:Label ID="lblDesde" runat="server" Text="Desde:" />
                <asp:TextBox ID="txtDesde" runat="server" CssClass="form-control filtro-fecha" />
                <ajaxToolkit:CalendarExtender ID="calDesde" runat="server" TargetControlID="txtDesde" Format="dd/MM/yyyy" />

                <asp:CustomValidator ID="cvDesde" runat="server"
                OnServerValidate="CvDesde_ServerValidate"
                Display="Dynamic" ForeColor="Red"
                ValidationGroup="Reporte" />

                <asp:Label ID="lblHasta" runat="server" Text="Hasta:" />
                <asp:TextBox ID="txtHasta" runat="server" CssClass="form-control filtro-fecha" />
                <ajaxToolkit:CalendarExtender ID="calHasta" runat="server" TargetControlID="txtHasta" Format="dd/MM/yyyy" />

                <asp:CustomValidator ID="cvHasta" runat="server"
                OnServerValidate="CvHasta_ServerValidate"
                Display="Dynamic" ForeColor="Red"
                ValidationGroup="Reporte" />
                </div>

                <asp:Button ID="GenerarReporte" runat="server" Text="Generar Reporte" OnClick="btnGenerarReporte_Click" CssClass="btn btn-primary" 
                    ValidationGroup="Reporte"/>
            
        </div>
    </div>
    <br />

    <div align="center">
        <asp:Label ID="lblMensaje" runat="server" ForeColor="Red" Visible="false"></asp:Label>
            <div align="center">
        
        <rsweb:ReportViewer ID="rvCobros" runat="server" Width="75%" Height="800px" 
            Visible="false" ZoomMode="PageWidth" ShowPrintButton="true">
        </rsweb:ReportViewer>
    </div>

    </div>
</asp:Content>