<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Ingreso_Registrar.aspx.cs" Inherits="Proyecto_Estacionamiento.Pages.Default.Ingreso_Registrar" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Registrar Ingreso</h2>

    <asp:Panel ID="pnlFormulario" runat="server" CssClass="formulario-ingreso">

        <div class="form-group form-inline">
            <label for="txtPatente">Patente:</label>
            <asp:TextBox ID="txtPatente" runat="server" MaxLength="100" CssClass="form-control"
                AutoPostBack="true" OnTextChanged="txtPatente_TextChanged" />
        </div>

        <div class="form-group form-inline">
            <label for="ddlCategoria">Categoría:</label>
            <asp:DropDownList ID="ddlCategoria" runat="server" CssClass="form-control" AutoPostBack="true" OnSelectedIndexChanged="ddlCategoria_SelectedIndexChanged" />
        </div>

        <div class="form-group form-inline">
            <label for="txtMarca">Marca:</label>
            <asp:TextBox ID="txtMarca" runat="server" MaxLength="100" CssClass="form-control" />
        </div>

        <div class="form-group form-inline">
            <label for="txtModelo">Modelo:</label>
            <asp:TextBox ID="txtModelo" runat="server" MaxLength="100" CssClass="form-control" />
        </div>

        <div class="form-group form-inline">
            <label>Color:</label>
            <asp:DropDownList ID="ddlColor" runat="server" CssClass="form-control">
                <asp:ListItem Text="--Seleccione Color--" Value="0" />
                <asp:ListItem>Blanco</asp:ListItem>
                <asp:ListItem>Negro</asp:ListItem>
                <asp:ListItem>Gris / Plata</asp:ListItem>
                <asp:ListItem>Azul</asp:ListItem>
                <asp:ListItem>Rojo</asp:ListItem>
                <asp:ListItem>Beige / Champagne</asp:ListItem>
                <asp:ListItem>Marrón / Café</asp:ListItem>
                <asp:ListItem>Verde</asp:ListItem>
                <asp:ListItem>Amarillo</asp:ListItem>
                <asp:ListItem>Naranja</asp:ListItem>
            </asp:DropDownList>
        </div>

        <div class="form-group form-inline">
            <label for="ddlPlaza">Plaza:</label>
            <asp:DropDownList ID="ddlPlaza" runat="server" CssClass="form-control" />
        </div>

        <div class="form-group form-inline">
            <label for="ddlTarifa">Tarifa:</label>
            <asp:DropDownList ID="ddlTarifa" runat="server" CssClass="form-control" />
        </div>

        <div class="form-group form-inline">
            <label for="ddlMetodoDePago">Método de Pago:</label>
            <asp:DropDownList ID="ddlMetodoDePago" runat="server" CssClass="form-control" />
        </div>

        <!-- Mensajes de Error -->
        <div class="form-group">
            <asp:Label ID="lblMensaje" runat="server" ForeColor="Red" />
        </div>
        <!-- Mensajes de Error -->

        <div class="form-group">
            <asp:Button ID="btnCancelar" runat="server" Text="Cancelar" OnClick="btnCancelar_Click" CssClass="btn btn-danger" />

            <asp:Button ID="btnGuardar" runat="server" Text="Guardar" OnClientClick="return confirmarGuardado();" OnClick="btnGuardar_Click" CssClass="btn btn-primary" />
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
                denyButtonText: "Cancelar"
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
