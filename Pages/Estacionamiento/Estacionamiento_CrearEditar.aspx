<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true"
    CodeBehind="Estacionamiento_CrearEditar.aspx.cs"
    Inherits="Proyecto_Estacionamiento.Pages.Estacionamiento.Estacionamiento_CrearEditar"
    Async="true" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <asp:Panel ID="pnlFormulario" runat="server" CssClass="formulario-estacionamiento">
        <h1>Ingrese los Datos del Estacionamiento:</h1>

        <asp:HiddenField ID="hdnEstId" runat="server" />

        <div class="form-group form-inline">
            <label for="txtNombre">Nombre:</label>
            <asp:TextBox ID="txtNombre" runat="server" CssClass="form-control" />
            <asp:CustomValidator ID="cvNombre" runat="server"
                OnServerValidate="cvNombre_ServerValidate"
                ErrorMessage="El Nombre es obligatorio."
                Display="Dynamic"
                ForeColor="Red"
                ValidationGroup="Estacionamiento" />
        </div>

        <div class="form-group form-inline">

            <!-- Provincia -->
            <div class="campo-inline">
                <label for="ddlProvincia">Provincia:</label>
                <asp:DropDownList ID="ddlProvincia" runat="server" AutoPostBack="true"
                    OnSelectedIndexChanged="ddlProvincia_SelectedIndexChanged" CssClass="form-control" />
                <div class="error-message">
                    <asp:CustomValidator ID="cvProvincia" runat="server"
                        OnServerValidate="cvProvincia_ServerValidate"
                        ErrorMessage="Debe seleccionar una Provincia."
                        Display="Dynamic"
                        ForeColor="Red"
                        ValidationGroup="Estacionamiento" />
                </div>
            </div>

            <!-- Localidad -->
            <div class="campo-inline">
                <label for="ddlLocalidad">Localidad:</label>
                <asp:DropDownList ID="ddlLocalidad" runat="server" CssClass="form-control" />
                <div class="error-message">
                    <asp:CustomValidator ID="cvLocalidad" runat="server"
                        OnServerValidate="cvLocalidad_ServerValidate"
                        ErrorMessage="Debe seleccionar una Localidad."
                        Display="Dynamic"
                        ForeColor="Red"
                        ValidationGroup="Estacionamiento" />
                </div>
            </div>

            <!-- Dirección -->
            <div class="campo-inline">
                <label for="txtDireccion">Dirección:</label>
                <asp:TextBox ID="txtDireccion" runat="server" CssClass="form-control" />
                <div class="error-message">
                    <asp:CustomValidator ID="cvDireccion" runat="server"
                        OnServerValidate="cvDireccion_ServerValidate"
                        ErrorMessage="La Dirección es obligatoria."
                        Display="Dynamic"
                        ForeColor="Red"
                        ValidationGroup="Estacionamiento" />
                </div>
            </div>

        </div>

        <div class="form-group form-inline">
            <label>Días de Atención (Inicio - Fin):</label>
            <asp:DropDownList ID="ddlDiaInicio" runat="server" CssClass="form-control">
                <asp:ListItem>Lunes</asp:ListItem>
                <asp:ListItem>Martes</asp:ListItem>
                <asp:ListItem>Miércoles</asp:ListItem>
                <asp:ListItem>Jueves</asp:ListItem>
                <asp:ListItem>Viernes</asp:ListItem>
                <asp:ListItem>Sábado</asp:ListItem>
            </asp:DropDownList>
            <asp:DropDownList ID="ddlDiaFin" runat="server" CssClass="form-control">
                <asp:ListItem>Lunes</asp:ListItem>
                <asp:ListItem>Martes</asp:ListItem>
                <asp:ListItem>Miércoles</asp:ListItem>
                <asp:ListItem>Jueves</asp:ListItem>
                <asp:ListItem>Viernes</asp:ListItem>
                <asp:ListItem>Sábado</asp:ListItem>
            </asp:DropDownList>
        </div>

        <div class="form-group form-inline">
            <label>Horario (Inicio - Fin):</label>
            <asp:DropDownList ID="ddlHoraInicio" runat="server" CssClass="form-control" />
            <asp:DropDownList ID="ddlHoraFin" runat="server" CssClass="form-control" />
            <asp:CustomValidator ID="cvHorario" runat="server"
                OnServerValidate="cvHorario_ServerValidate"
                ErrorMessage="La hora de inicio debe ser menor que la hora de fin."
                Display="Dynamic"
                ForeColor="Red"
                ValidationGroup="Estacionamiento" />
        </div>

        <div class="form-group form-inline">
            <asp:CheckBox ID="chkDomingo" runat="server" AutoPostBack="true" OnCheckedChanged="chkDomingo_CheckedChanged" Text="Atiende los Domingos" />
        </div>

        <div class="form-group form-inline">
            <label>Hora Domingo (Inicio - Fin):</label>
            <asp:DropDownList ID="ddlHoraInicio_Domingo" runat="server" CssClass="form-control" />
            <asp:DropDownList ID="ddlHoraFin_Domingo" runat="server" CssClass="form-control" />

            <asp:CustomValidator ID="cvHorarioDomingo" runat="server"
                OnServerValidate="cvHorarioDomingo_ServerValidate"
                ErrorMessage="La hora de inicio de domingo debe ser menor que la hora de fin."
                Display="Dynamic"
                ForeColor="Red"
                ValidationGroup="Estacionamiento" />
        </div>

        <div class="form-group form-inline">
            <asp:CheckBox ID="chkDiasFeriado" runat="server" Text="Atiende en días feriados" />
        </div>

        <div class="form-group form-inline">
            <asp:CheckBox ID="chkDisponibilidad" runat="server" Text="Disponibilidad" />
        </div>

        <!-- Mensajes de Error -->
        <div class="form-group">
            <asp:Label ID="lblMensaje" runat="server" ForeColor="Red" />
        </div>
        <!-- Mensajes de Error -->

        <div class="form-group">
            <asp:Button ID="btnCancelar" runat="server" Text="Cancelar" OnClick="btnCancelar_Click" CssClass="btn btn-danger" />
            <asp:Button ID="btnGuardar" runat="server" Text="Guardar"
                OnClick="btnGuardar_Click"
                CssClass="btn btn-primary"
                ValidationGroup="Estacionamiento"
                OnClientClick="return confirmarGuardado();" />
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
                reverseButtons: true // 👈 Esto invierte el orden de los botones
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
