<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Tarifa_Crear_Editar.aspx.cs" Inherits="Proyecto_Estacionamiento.Pages.Tarifas.Tarifa_Crear_Editar" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <h2>
        <asp:Label ID="lblTitulo" runat="server" Text="Agregar Tarifa" /></h2>

    <asp:Panel ID="pnlFormulario" runat="server">
        <table>
            <tr>
                <td>Estacionamiento:</td>
                <td>
                    <asp:DropDownList ID="ddlEstacionamientos" runat="server" />
                </td>
            </tr>
            <tr>
                <td>Tipos de Tarifa:</td>
                <td>
                    <asp:DropDownList ID="ddlTiposTarifa" runat="server" />
                </td>
            </tr>
            <tr>
                <td>Categoria de Vehiculo:</td>
                <td>
                    <asp:DropDownList ID="ddlCategorias" runat="server" />
                </td>
            </tr>
            <tr>
                <td>Monto:</td>
                <td>
                    <asp:TextBox ID="txtTarifaMonto" runat="server" />
                </td>
            </tr>
            <tr>
                <td>Fecha:</td>
                <td>
                    <asp:TextBox ID="txtTarifaDesde" runat="server" />
                    <asp:Calendar ID="calTarifaDesde" runat="server" OnSelectionChanged="calTarifaDesde_SelectionChanged" Visible="false" />

                    <asp:Button ID="btnMostrarCalendario" runat="server" Text="Elegir Fecha" OnClick="btnMostrarCalendario_Click" />
                </td>
            </tr>
        </table>
    </asp:Panel>
    <br />
    <asp:Button ID="btnGuardar" runat="server" Text="Guardar" OnClick="btnGuardar_Click" />
    <asp:Button ID="btnCancelar" runat="server" Text="Cancelar" OnClick="btnCancelar_Click" CausesValidation="False" />
</asp:Content>
