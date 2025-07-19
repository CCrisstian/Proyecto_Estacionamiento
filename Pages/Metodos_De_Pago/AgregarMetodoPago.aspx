<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AgregarMetodoPago.aspx.cs" Inherits="Proyecto_Estacionamiento.Pages.Metodos_De_Pago.AgregarMetodoPago" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <h2>
        <asp:Label ID="lblTitulo" runat="server" Text="Agregar Método de Pago" />
    </h2>

    <asp:Panel ID="pnlFormulario" runat="server">

            <table>
                <tr>
                    <td>Estacionamiento:</td>
                    <td>
                        <asp:DropDownList ID="ddlEstacionamientos" runat="server" />
                        <asp:RequiredFieldValidator ID="rfvEstacionamientos" runat="server"
                            ControlToValidate="ddlEstacionamientos"
                            InitialValue=""
                            ErrorMessage="Debe seleccionar un Estacionamiento" ForeColor="Red" />
                    </td>
                </tr>
                <tr>
                    <td>Método de Pago:</td>
                    <td>
                        <asp:DropDownList ID="ddlMetodoDePago" runat="server" />
                        <asp:RequiredFieldValidator ID="rfvMetodoPago" runat="server"
                            ControlToValidate="ddlMetodoDePago"
                            InitialValue=""
                            ErrorMessage="Debe seleccionar un Método de Pago" ForeColor="Red" />
                    </td>
                </tr>
                <tr>
                    <td>Desde:</td>
                    <td>
                        <asp:TextBox ID="txtDesde" runat="server" />

                        <ajaxToolkit:CalendarExtender ID="calDesde" runat="server"
                            TargetControlID="txtDesde" Format="yyyy-MM-dd" />

                        <asp:RequiredFieldValidator ID="rfvDesde" runat="server"
                            ControlToValidate="txtDesde" ErrorMessage="Campo obligatorio"
                            ForeColor="Red" Display="Dynamic" />
                    </td>
                </tr>
                <tr>
                    <td>Hasta:</td>
                    <td>
                        <asp:TextBox ID="txtHasta" runat="server" />

                        <ajaxToolkit:CalendarExtender ID="calHasta" runat="server"
                            TargetControlID="txtHasta" Format="yyyy-MM-dd" />

                        <asp:RequiredFieldValidator ID="rfvHasta" runat="server"
                            ControlToValidate="txtHasta" ErrorMessage="Campo obligatorio"
                            ForeColor="Red" Display="Dynamic" />
                    </td>
                </tr>
            </table>

        <asp:Button ID="btnGuardar" runat="server" Text="Guardar" OnClick="btnGuardar_Click" />
        
        <ajaxToolkit:ConfirmButtonExtender ID="cbeGuardar" runat="server"
            TargetControlID="btnGuardar"
            ConfirmText="¿Estás seguro de que deseas guardar los cambios?" />

        <asp:Button ID="btnCancelar" runat="server" Text="Cancelar" OnClick="btnCancelar_Click" CausesValidation="False" />

        <asp:Label ID="lblError" runat="server" ForeColor="Red" />

    </asp:Panel>

</asp:Content>