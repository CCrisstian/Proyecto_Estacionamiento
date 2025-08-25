<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Tarifa_Crear_Editar.aspx.cs" Inherits="Proyecto_Estacionamiento.Pages.Tarifas.Tarifa_Crear_Editar" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <h2>
        <asp:Label ID="lblTitulo" runat="server" Text="Agregar Tarifa" />
    </h2>

    <asp:Panel ID="pnlFormulario" runat="server" CssClass="formulario-tarifa">

        <div class="form-group form-inline">
            <label for="ddlEstacionamientos">Estacionamiento:</label>
            <asp:DropDownList ID="ddlEstacionamientos" runat="server" CssClass="form-control" />
            <asp:RequiredFieldValidator ID="rfvEstacionamientos" runat="server"
                ControlToValidate="ddlEstacionamientos" InitialValue=""
                ErrorMessage="Debe seleccionar un Estacionamiento" ForeColor="Red" Display="Dynamic" />
        </div>

        <div class="form-group form-inline">
            <label for="ddlTiposTarifa">Tipos de Tarifa:</label>
            <asp:DropDownList ID="ddlTiposTarifa" runat="server" CssClass="form-control" />
            <asp:RequiredFieldValidator ID="rfvTiposTarifa" runat="server"
                ControlToValidate="ddlTiposTarifa" InitialValue=""
                ErrorMessage="Debe seleccionar un Tipo de Tarifa" ForeColor="Red" Display="Dynamic" />
        </div>

        <div class="form-group form-inline">
            <label for="ddlCategorias">Categoría de Vehículo:</label>
            <asp:DropDownList ID="ddlCategorias" runat="server" CssClass="form-control" />
            <asp:RequiredFieldValidator ID="rfvCategorias" runat="server"
                ControlToValidate="ddlCategorias" InitialValue=""
                ErrorMessage="Debe seleccionar una Categoría" ForeColor="Red" Display="Dynamic" />
        </div>

        <div class="form-group form-inline">
            <label for="txtTarifaMonto">Monto:</label>
            <asp:TextBox ID="txtTarifaMonto" runat="server" CssClass="form-control" />
            <asp:RequiredFieldValidator ID="rfvMonto" runat="server"
                ControlToValidate="txtTarifaMonto"
                ErrorMessage="Debe ingresar un Monto" ForeColor="Red" Display="Dynamic" />
            <asp:RegularExpressionValidator ID="revMonto" runat="server"
                ControlToValidate="txtTarifaMonto"
                ValidationExpression="^\d+(\.\d{1,2})?$"
                ErrorMessage="El monto debe ser un número válido" ForeColor="Red" Display="Dynamic" />
        </div>

    </asp:Panel>

    <br />

    <div class="form-group form-inline">
        <asp:Button ID="btnGuardar" runat="server" Text="Guardar"
            OnClientClick="return confirmarGuardado();"
            OnClick="btnGuardar_Click" CssClass="btn btn-primary" />

        <asp:Button ID="btnCancelar" runat="server" Text="Cancelar" OnClick="btnCancelar_Click" CausesValidation="False" CssClass="btn btn-danger" />
    </div>

    <%-- SweetAlert2 para mensajes de confirmación --%>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <script>
        function confirmarGuardado() {
            Swal.fire({
                title: "¿Deseás guardar los cambios?",
                showDenyButton: true,
                confirmButtonText: "Guardar",
                denyButtonText: "Cancelar",
            }).then((result) => {
                if (result.isConfirmed) {
                    __doPostBack('<%= btnGuardar.UniqueID %>', '');
                } else if (result.isDenied) {
                    Swal.fire("Los cambios no se guardaron", "", "info");
                }
            });
            return false;
        }
    </script>
</asp:Content>

