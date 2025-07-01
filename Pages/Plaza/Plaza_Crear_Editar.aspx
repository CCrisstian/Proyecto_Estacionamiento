<%@ Page Title="Plaza" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Plaza_Crear_Editar.aspx.cs" Inherits="Proyecto_Estacionamiento.Pages.Plaza.Plaza_Crear_Editar" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2><asp:Label ID="lblTitulo" runat="server" Text="Agregar Plaza" /></h2>
    <asp:Panel ID="pnlFormulario" runat="server">
        <table>
            <tr>
                <td>Estacionamiento:</td>
                <td>
                    <asp:DropDownList ID="ddlEstacionamiento" runat="server" />
                </td>
            </tr>
            <tr>
                <td>Categoría de Vehículo:</td>
                <td>
                    <asp:DropDownList ID="ddlCategoria" runat="server" />
                </td>
            </tr>
            <tr>
                <td>Nombre:</td>
                <td>
                    <asp:TextBox ID="txtNombre" runat="server" MaxLength="100" />
                </td>
            </tr>
            <tr>
                <td>Tipo:</td>
                <td>
                    <asp:TextBox ID="txtTipo" runat="server" MaxLength="100" />
                </td>
            </tr>
            <tr>
                <td>Disponible:</td>
                <td>
                    <asp:DropDownList ID="ddlDisponible" runat="server">
                        <asp:ListItem Text="Sí" Value="true" />
                        <asp:ListItem Text="No" Value="false" />
                    </asp:DropDownList>
                </td>
            </tr>
        </table>
        <br />
        <asp:Label ID="lblMensaje" runat="server" ForeColor="Red" />
        <br />
        <asp:Button ID="btnGuardar" runat="server" Text="Guardar" OnClick="btnGuardar_Click" />
        <asp:Button ID="btnCancelar" runat="server" Text="Cancelar" OnClick="btnCancelar_Click" CausesValidation="false" />
    </asp:Panel>
</asp:Content>
