<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true"
    CodeBehind="Estacionamiento_CrearEditar.aspx.cs"
    Inherits="Proyecto_Estacionamiento.Pages.Estacionamiento.Estacionamiento_CrearEditar"
    Async="true" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <asp:Panel ID="pnlFormulario" runat="server" CssClass="formulario-estacionamiento">
        <h3>Ingrese los Datos del Estacionamiento:</h3>


        <div class="form-group form-inline">
            <label for="txtNombre">Nombre:</label>
            <asp:TextBox ID="txtNombre" runat="server" CssClass="form-control" />
        </div>

        <div class="form-group form-inline">
            <label for="ddlProvincia">Provincia:</label>
            <asp:DropDownList ID="ddlProvincia" runat="server" AutoPostBack="true"
                OnSelectedIndexChanged="ddlProvincia_SelectedIndexChanged" CssClass="form-control" />
        </div>

        <div class="form-group form-inline">
            <label for="ddlLocalidad">Localidad:</label>
            <asp:DropDownList ID="ddlLocalidad" runat="server" CssClass="form-control"/>
        </div>

        <div class="form-group form-inline">
            <label for="txtDireccion">Dirección:</label>
            <asp:TextBox ID="txtDireccion" runat="server" CssClass="form-control"></asp:TextBox>
        </div>

        <div class="form-group form-inline">
            <label>Días de Atención (Inicio - Fin):</label>
            <asp:DropDownList ID="ddlDiaInicio" runat="server" CssClass="form-control">
                <asp:ListItem>Lunes</asp:ListItem>
                <asp:ListItem>Martes</asp:ListItem>
                <asp:ListItem>Miércoles</asp:ListItem>
                <asp:ListItem>Jueves</asp:ListItem>
                <asp:ListItem>Viernes</asp:ListItem>
            </asp:DropDownList>
            <asp:DropDownList ID="ddlDiaFin" runat="server" CssClass="form-control">
                <asp:ListItem>Lunes</asp:ListItem>
                <asp:ListItem>Martes</asp:ListItem>
                <asp:ListItem>Miércoles</asp:ListItem>
                <asp:ListItem>Jueves</asp:ListItem>
                <asp:ListItem>Viernes</asp:ListItem>
            </asp:DropDownList>
        </div>

        <div class="form-group form-inline">
            <label>Horario (Inicio - Fin):</label>
            <asp:DropDownList ID="ddlHoraInicio" runat="server" CssClass="form-control"/>
            <asp:DropDownList ID="ddlHoraFin" runat="server" CssClass="form-control"/>
        </div>

        <div class="form-group form-inline">
            <asp:CheckBox ID="chkDiasFeriado" runat="server" Text="Atiende en días feriados" />
        </div>

        <div class="form-group form-inline">
            <asp:CheckBox ID="chkFinDeSemana" runat="server" AutoPostBack="true" OnCheckedChanged="chkFinDeSemana_CheckedChanged" Text="Atiende fines de semana" />
        </div>

        <div class="form-group form-inline">
            <label>Hora Fin de Semana (Inicio - Fin):</label>
            <asp:DropDownList ID="ddlHoraInicio_FinDeSemana" runat="server" CssClass="form-control"/>
            <asp:DropDownList ID="ddlHoraFin_FinDeSemana" runat="server" CssClass="form-control"/>
        </div>

        <div class="form-group">
            <asp:CheckBox ID="chkDisponibilidad" runat="server" Text="Disponibilidad" />
        </div>

        <!-- Mensajes de Error -->
        <div class="form-group">
            <asp:Label ID="lblMensaje" runat="server" ForeColor="Red" />
        </div>

        <div class="form-group">
            <asp:Button ID="btnGuardar" runat="server" Text="Guardar" OnClientClick="return confirmarGuardado();" OnClick="btnGuardar_Click" CssClass="btn btn-primary" />

            <asp:Button ID="btnCancelar" runat="server" Text="Cancelar" OnClick="btnCancelar_Click" CssClass="btn btn-danger" />
        </div>

    </asp:Panel>


    <%-- SweetAlert2 para mensajes de confirmación --%>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <script>
        function confirmarGuardado() {
            Swal.fire({
                title: "¿Deseás guardar los cambios?",
                showDenyButton: true,
                showCancelButton: true,
                confirmButtonText: "Guardar",
                denyButtonText: "No guardar",
                cancelButtonText: "Cancelar"
            }).then((result) => {
                if (result.isConfirmed) {
                    // Evitar loop infinito llamando el click del botón
                    // Usar __doPostBack para disparar postback sin recursión
                    __doPostBack('<%= btnGuardar.UniqueID %>', '');
                } else if (result.isDenied) {
                    Swal.fire("Los cambios no se guardaron", "", "info");
                }
                // Si canceló, no hacemos nada
            });
            return false; // Esto previene el postback original hasta que confirmemos
        }
    </script>

</asp:Content>
