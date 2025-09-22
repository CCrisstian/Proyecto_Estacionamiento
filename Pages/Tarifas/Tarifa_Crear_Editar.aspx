<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Tarifa_Crear_Editar.aspx.cs" Inherits="Proyecto_Estacionamiento.Pages.Tarifas.Tarifa_Crear_Editar" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <asp:Panel ID="pnlFormulario" runat="server" CssClass="formulario-tarifa">

        <h2>
            <asp:Label ID="lblTitulo" runat="server" Text="Agregar Tarifa" />
        </h2>

        <br />

        <div class="form-group form-inline">
            <label for="ddlEstacionamientos">Estacionamiento:</label>
            <asp:DropDownList ID="ddlEstacionamientos" runat="server" CssClass="form-control" />
            <asp:CustomValidator ID="cvEstacionamientos" runat="server"
                OnServerValidate="cvEstacionamientos_ServerValidate"
                Display="Dynamic" ForeColor="Red"
                ValidationGroup="Tarifa" />
        </div>

        <div class="form-group form-inline">
            <label for="ddlTiposTarifa">Tipos de Tarifa:</label>
            <asp:DropDownList ID="ddlTiposTarifa" runat="server" CssClass="form-control" />
            <asp:CustomValidator ID="cvTiposTarifa" runat="server"
                OnServerValidate="cvTiposTarifa_ServerValidate"
                Display="Dynamic" ForeColor="Red"
                ValidationGroup="Tarifa" />
        </div>

        <div class="form-group form-inline">
            <label for="ddlCategorias">Categoría de Vehículo:</label>
            <asp:DropDownList ID="ddlCategorias" runat="server" CssClass="form-control" />
            <asp:CustomValidator ID="cvCategorias" runat="server"
                OnServerValidate="cvCategorias_ServerValidate"
                Display="Dynamic" ForeColor="Red"
                ValidationGroup="Tarifa" />
        </div>

        <div class="form-group form-inline">
            <label for="txtTarifaMonto">Monto:</label>
            <asp:TextBox ID="txtTarifaMonto" runat="server" CssClass="form-control" />
            <asp:CustomValidator ID="cvMonto" runat="server"
                OnServerValidate="cvMonto_ServerValidate"
                Display="Dynamic" ForeColor="Red"
                ValidationGroup="Tarifa" />
        </div>

    </asp:Panel>

    <br />

    <div class="form-group form-inline">
        <asp:Button ID="btnCancelar" runat="server" Text="Cancelar" OnClick="btnCancelar_Click" CausesValidation="False" CssClass="btn btn-danger" />

        <asp:Button ID="btnGuardar" runat="server" Text="Guardar"
            OnClientClick="return confirmarGuardado();"
            OnClick="btnGuardar_Click" CssClass="btn btn-primary"
            ValidationGroup="Tarifa" />
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
                reverseButtons: true // 👈 Esto invierte el orden de los botones
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

