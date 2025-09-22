<%@ Page Language="C#" AutoEventWireup="true"
    MasterPageFile="~/Site.Master"
    CodeBehind="Playero_Crear_Editar.aspx.cs"
    Inherits="Proyecto_Estacionamiento.Pages.Playeros.Playero_Crear_Editar" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">


    <asp:Panel ID="pnlFormulario" runat="server" CssClass="formulario-playero">

        <h2>
            <asp:Label ID="lblTitulo" runat="server" Text="Agregar Playero" />
        </h2>

        <br />

        <div class="form-group form-inline">
            <label for="ddlEstacionamientos">Estacionamiento:</label>
            <asp:DropDownList ID="ddlEstacionamientos" runat="server" CssClass="form-control" />
            <asp:CustomValidator ID="cvEstacionamiento" runat="server"
                OnServerValidate="cvEstacionamiento_ServerValidate"
                ErrorMessage="Debe seleccionar un Estacionamiento."
                Display="Dynamic" ForeColor="Red" ValidationGroup="Playero" />
        </div>

        <div class="form-group form-inline">
            <label for="txtDni">DNI:</label>
            <asp:TextBox ID="txtDni" runat="server" CssClass="form-control" autocomplete="off" />
            <asp:CustomValidator ID="cvDni" runat="server"
                OnServerValidate="cvDni_ServerValidate"
                Display="Dynamic" ForeColor="Red" ValidationGroup="Playero" />
        </div>

        <div class="form-group form-inline">
            <label for="txtPass">Contraseña:</label>
            <asp:TextBox ID="txtPass" runat="server" TextMode="Password" CssClass="form-control" />
            <asp:RequiredFieldValidator ID="rfvPass" runat="server"
                ControlToValidate="txtPass"
                ErrorMessage="La Contraseña es obligatoria." Display="Dynamic" ForeColor="Red" ValidationGroup="Playero" />
        </div>


        <div class="form-group form-inline">
            <label for="txtApellido">Apellido:</label>
            <asp:TextBox ID="txtApellido" runat="server" CssClass="form-control" />
            <asp:CustomValidator ID="cvApellido" runat="server"
                OnServerValidate="cvApellido_ServerValidate"
                ErrorMessage="El Apellido no debe estar vacio y solo debe contener letras."
                Display="Dynamic" ForeColor="Red" ValidationGroup="Playero" />
        </div>

        <div class="form-group form-inline">
            <label for="txtNombre">Nombre:</label>
            <asp:TextBox ID="txtNombre" runat="server" CssClass="form-control" />
            <asp:CustomValidator ID="cvNombre" runat="server"
                OnServerValidate="cvNombre_ServerValidate"
                ErrorMessage="El Nombre no debe estar vacio y solo debe contener letras."
                Display="Dynamic" ForeColor="Red" ValidationGroup="Playero" />
        </div>

        <div class="form-group form-inline">
            <asp:CheckBox ID="chkActivo" runat="server" Text="Activo" />
        </div>

        <div class="form-group form-inline">
            <asp:Button ID="btnCancelar" runat="server" Text="Cancelar" PostBackUrl="~/Pages/Playeros/Playero_Listar.aspx" CausesValidation="false" CssClass="btn btn-danger" />

            <asp:Button ID="btnGuardar" runat="server" Text="Guardar"
                OnClientClick="return confirmarGuardado();"
                OnClick="btnGuardar_Click" CssClass="btn btn-primary" ValidationGroup="Playero" />
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
                    __doPostBack('<%= btnGuardar.UniqueID %>', '');
                } else if (result.isDenied) {
                    Swal.fire("Los cambios no se guardaron", "", "info");
                }
            });
            return false;
        }
    </script>
</asp:Content>
