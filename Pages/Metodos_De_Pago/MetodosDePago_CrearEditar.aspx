<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="MetodosDePago_CrearEditar.aspx.cs" Inherits="Proyecto_Estacionamiento.Pages.Metodos_De_Pago.MetodosDePago_CrearEditar" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <h2>
        <asp:Label ID="lblTitulo" runat="server" Text="Agregar Método de Pago" />
    </h2>

    <asp:Panel ID="pnlFormulario" runat="server" CssClass="formulario-metodospago">

        <div class="form-group form-inline">
            <label for="ddlEstacionamientos">Estacionamiento:</label>
            <asp:DropDownList ID="ddlEstacionamientos" runat="server" CssClass="form-control" />
            <asp:RequiredFieldValidator ID="rfvEstacionamientos" runat="server"
                ControlToValidate="ddlEstacionamientos"
                InitialValue=""
                ErrorMessage="Debe seleccionar un Estacionamiento" ForeColor="Red" Display="Dynamic" />
        </div>

        <div class="form-group form-inline">
            <label for="ddlMetodoDePago">Método de Pago:</label>
            <asp:DropDownList ID="ddlMetodoDePago" runat="server" CssClass="form-control" />
            <asp:RequiredFieldValidator ID="rfvMetodoPago" runat="server"
                ControlToValidate="ddlMetodoDePago"
                InitialValue=""
                ErrorMessage="Debe seleccionar un Método de Pago" ForeColor="Red" Display="Dynamic" />
        </div>

        <div class="form-group form-inline">
            <label for="txtHasta">Hasta:</label>
            <asp:TextBox ID="txtHasta" runat="server" CssClass="form-control" />
            <ajaxToolkit:CalendarExtender ID="calHasta" runat="server"
                TargetControlID="txtHasta" Format="yyyy-MM-dd" />
            <asp:RequiredFieldValidator ID="rfvHasta" runat="server"
                ControlToValidate="txtHasta" ErrorMessage="Campo obligatorio"
                ForeColor="Red" Display="Dynamic" />
        </div>

        <div class="form-group">
            <asp:Button ID="btnGuardar" runat="server" Text="Guardar" OnClick="btnGuardar_Click" CssClass="btn btn-primary" />
            <ajaxToolkit:ConfirmButtonExtender ID="cbeGuardar" runat="server"
                TargetControlID="btnGuardar"
                ConfirmText="¿Estás seguro de que deseas guardar los cambios?" />
            
            <asp:Button ID="btnCancelar" runat="server" Text="Cancelar" OnClick="btnCancelar_Click"
                CausesValidation="False" CssClass="btn btn-danger" />
        </div>

        <div class="form-group">
            <asp:Label ID="lblError" runat="server" ForeColor="Red" />
        </div>

    </asp:Panel>

</asp:Content>
