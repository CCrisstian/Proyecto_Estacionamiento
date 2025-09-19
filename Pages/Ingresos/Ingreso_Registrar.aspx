<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Ingreso_Registrar.aspx.cs" Inherits="Proyecto_Estacionamiento.Pages.Default.Ingreso_Registrar" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Registrar Ingreso</h1>

    <asp:Panel ID="pnlFormulario" runat="server" CssClass="formulario-ingreso">

        <div class="form-group form-inline">
            <label for="txtPatente">Patente:</label>
            <asp:TextBox ID="txtPatente" runat="server" MaxLength="100" CssClass="form-control"
                AutoPostBack="true" OnTextChanged="txtPatente_TextChanged" />
            <asp:CustomValidator ID="cvPatente" runat="server"
                OnServerValidate="cvPatente_ServerValidate" Display="Dynamic" ForeColor="Red" ValidationGroup="Ingreso" />
        </div>

        <div class="form-group form-inline">
            <label for="ddlCategoria">Categoría:</label>
            <asp:DropDownList ID="ddlCategoria" runat="server" CssClass="form-control" AutoPostBack="true"
                OnSelectedIndexChanged="ddlCategoria_SelectedIndexChanged">
                <asp:ListItem Text="--Seleccione Categoría--" Value="0" />
            </asp:DropDownList>
            <asp:CustomValidator ID="cvCategoria" runat="server"
                OnServerValidate="cvCategoria_ServerValidate" Display="Dynamic" ForeColor="Red" ValidationGroup="Ingreso" />
        </div>

        <div class="form-group form-inline">
            <label for="ddlPlaza">Plaza:</label>
            <asp:DropDownList ID="ddlPlaza" runat="server" CssClass="form-control">
                <asp:ListItem Text="--Seleccione Plaza--" Value="0" />
            </asp:DropDownList>
            <asp:CustomValidator ID="cvPlaza" runat="server"
                OnServerValidate="cvPlaza_ServerValidate" Display="Dynamic" ForeColor="Red" ValidationGroup="Ingreso" />
        </div>

        <div class="form-group form-inline">
            <label for="ddlTarifa">Tarifa:</label>
            <asp:DropDownList ID="ddlTarifa" runat="server" CssClass="form-control">
                <asp:ListItem Text="--Seleccione Tarifa--" Value="0" />
            </asp:DropDownList>
            <asp:CustomValidator ID="cvTarifa" runat="server"
                OnServerValidate="cvTarifa_ServerValidate" Display="Dynamic" ForeColor="Red" ValidationGroup="Ingreso" />
        </div>

        <div class="form-group">
            <asp:Button ID="btnCancelar" runat="server" Text="Cancelar" OnClick="btnCancelar_Click" CssClass="btn btn-danger" />

            <asp:Button ID="btnGuardar" runat="server" Text="Guardar" OnClientClick="return confirmarGuardado();" OnClick="btnGuardar_Click" CssClass="btn btn-primary"
                ValidationGroup="Ingreso" />
        </div>
    </asp:Panel>

    <%-- SweetAlert2 para mensajes de confirmación --%>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <script>
        function confirmarGuardado() {
            Swal.fire({
                title: "¿Deseás registrar el 'Ingreso'?",
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
                    Swal.fire("'Ingreso' no registrado", "", "info");
                }
                // Si canceló, no hacemos nada
            });
            return false; // Esto previene el postback original hasta que confirmemos
        }
    </script>
</asp:Content>
