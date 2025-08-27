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
                TargetControlID="txtHasta" Format="dd-MM-yyyy" />

            <!-- Validación en cliente para solo fechas válidas o vacío -->
            <asp:RegularExpressionValidator
                ID="revFechaHasta"
                runat="server"
                ControlToValidate="txtHasta"
                ValidationExpression="^$|^(0[1-9]|[12][0-9]|3[01])-(0[1-9]|1[0-2])-(\d{4})$"
                ErrorMessage="Ingrese una fecha válida (dd-MM-yyyy) o seleccione del calendario."
                CssClass="text-danger"
                Display="Dynamic" />

        </div>

        <div class="form-group">
            <asp:Button ID="btnCancelar" runat="server" Text="Cancelar" OnClick="btnCancelar_Click"
                CausesValidation="False" CssClass="btn btn-danger" />

            <asp:Button ID="btnGuardar" runat="server" Text="Guardar"
                OnClientClick="return confirmarGuardado();"
                OnClick="btnGuardar_Click" CssClass="btn btn-primary" />
        </div>

        <div class="form-group">
            <asp:Label ID="lblError" runat="server" ForeColor="Red" />
        </div>

    </asp:Panel>

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
