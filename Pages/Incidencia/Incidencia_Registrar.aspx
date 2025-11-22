<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Incidencia_Registrar.aspx.cs" Inherits="Proyecto_Estacionamiento.Pages.Incidencia.Incidencia_Registrar" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="ingreso-container">

        <asp:Panel ID="pnlFormulario" runat="server" CssClass="formulario-reportes">

            <div class="header-row">
                <h2>Registrar Incidencia</h2>
            </div>
            <br />

            <div class="form-group form-inline">
                <label for="ddlMotivo">Motivo de la Incidencia:</label>
                <asp:DropDownList ID="ddlMotivo" runat="server" CssClass="form-control" />
                <asp:RequiredFieldValidator ID="rfvMotivo" runat="server"
                    ControlToValidate="ddlMotivo"
                    ErrorMessage="Debe seleccionar un motivo."
                    Display="Dynamic" ForeColor="Red" InitialValue=""
                    ValidationGroup="Incidencia" />
            </div>

            <div class="form-group">
                <label>Descripción:</label>
                <asp:TextBox ID="txtDescripcion" runat="server"
                    CssClass="form-control"
                    TextMode="MultiLine"
                    Rows="7"
                    Style="max-width: none; width: 80%;" />
                <asp:RequiredFieldValidator ID="rfvDescripcion" runat="server"
                    ControlToValidate="txtDescripcion"
                    ErrorMessage="Debe ingresar una descripción."
                    Display="Dynamic" ForeColor="Red"
                    ValidationGroup="Incidencia" />
            </div>

            <div class="form-group form-inline">
                <label for="ddlEstado">Estado:</label>
                <asp:DropDownList ID="ddlEstado" runat="server" CssClass="form-control">
                    <asp:ListItem Text="Pendiente" Value="0" Selected="True" />
                    <asp:ListItem Text="Resuelto" Value="1" />
                </asp:DropDownList>
            </div>

            <br />

            <div class="form-group">
                <asp:Button ID="btnCancelar" runat="server" Text="Cancelar" CausesValidation="False" CssClass="btn btn-danger"
                    OnClick="BtnCancelar_Click" />
                <asp:Button ID="btnGuardar" runat="server" Text="Guardar Incidencia"
                    OnClientClick="return confirmarGuardado();"
                    ValidationGroup="Incidencia" CssClass="btn btn-primary"
                    OnClick="BtnGuardar_Click" />
            </div>

        </asp:Panel>
    </div>

    <%-- SweetAlert2 para mensajes de confirmación --%>

    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <script>
        function confirmarGuardado() {
            Swal.fire({
                title: "¿Deseás registrar la 'Incidencia'?",
                showDenyButton: true,
                confirmButtonText: "Guardar",
                denyButtonText: "Cancelar",
                reverseButtons: true // 👈 Esto invierte el orden de los botones
            }).then((result) => {
                if (result.isConfirmed) {
                    // Evitar loop infinito llamando el click del botón
                    // Usar __doPostBack para disparar postback sin recursión
                    __doPostBack('<%= btnGuardar.UniqueID %>', '');
                } else if (result.isDenied) {
                    Swal.fire("'Incidencia' no registrada", "", "info");
                }
                // Si canceló, no hacemos nada
            });
            return false; // Esto previene el postback original hasta que confirmemos
        }
    </script>

</asp:Content>
