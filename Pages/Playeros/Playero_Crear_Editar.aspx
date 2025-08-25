<%@ Page Language="C#" AutoEventWireup="true"
    MasterPageFile="~/Site.Master"
    CodeBehind="Playero_Crear_Editar.aspx.cs"
    Inherits="Proyecto_Estacionamiento.Pages.Playeros.Playero_Crear_Editar" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <h2><%: Request.QueryString["legajo"] == null ? "Agregar Playero" : "Editar Playero" %></h2>

   <div class="form-group">
        <asp:Label ID="lblError" runat="server" ForeColor="Red" Visible="true" />
    </div>

    <div class="form-group form-inline">
        <label for="ddlEstacionamientos">Estacionamiento:</label>
        <asp:DropDownList ID="ddlEstacionamientos" runat="server" CssClass="form-control" />
    </div>

    <div class="form-group form-inline">
        <label for="txtDni">DNI:</label>
        <asp:TextBox ID="txtDni" runat="server" CssClass="form-control" autocomplete="off" />
        <asp:RequiredFieldValidator ID="rfvDNI" runat="server" ControlToValidate="txtDni"
            ErrorMessage="El DNI es obligatorio." Display="Dynamic" CssClass="text-danger" />
    </div>

    <div class="form-group form-inline">
        <label for="txtPass">Contraseña:</label>
        <asp:TextBox ID="txtPass" runat="server" TextMode="Password" CssClass="form-control" />
        <asp:RequiredFieldValidator ID="rfvPass" runat="server" ControlToValidate="txtPass"
            ErrorMessage="La Contraseña es obligatoria." Display="Dynamic" CssClass="text-danger" />
    </div>

    <div class="form-group form-inline">
        <label for="txtApellido">Apellido:</label>
        <asp:TextBox ID="txtApellido" runat="server" CssClass="form-control" />
        <asp:RequiredFieldValidator ID="rfvApellido" runat="server" ControlToValidate="txtApellido"
            ErrorMessage="El Apellido es obligatorio." Display="Dynamic" CssClass="text-danger" />
    </div>

    <div class="form-group form-inline">
        <label for="txtNombre">Nombre:</label>
        <asp:TextBox ID="txtNombre" runat="server" CssClass="form-control" />
        <asp:RequiredFieldValidator ID="rfvNombre" runat="server" ControlToValidate="txtNombre"
            ErrorMessage="El Nombre es obligatorio." Display="Dynamic" CssClass="text-danger" />
    </div>

    <div class="form-group form-inline">
        <label for="chkActivo">Activo:</label>
        <asp:CheckBox ID="chkActivo" runat="server" />
    </div>

    <div class="form-group form-inline">
        <asp:Button ID="btnGuardar" runat="server" Text="Guardar" 
            OnClientClick="return confirmarGuardado();"
            OnClick="btnGuardar_Click" CssClass="btn btn-primary" />
        <asp:Button ID="btnCancelar" runat="server" Text="Cancelar" PostBackUrl="~/Pages/Playeros/Playero_Listar.aspx" CausesValidation="false" CssClass="btn btn-danger" />
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
