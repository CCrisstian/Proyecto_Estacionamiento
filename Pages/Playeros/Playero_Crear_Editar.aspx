<%@ Page Language="C#" AutoEventWireup="true"
    MasterPageFile="~/Site.Master"
    CodeBehind="Playero_Crear_Editar.aspx.cs"
    Inherits="Proyecto_Estacionamiento.Pages.Playeros.Playero_Crear_Editar" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <h2><%: Request.QueryString["legajo"] == null ? "Agregar Playero" : "Editar Playero" %></h2>

    <asp:Label ID="lblError" runat="server" ForeColor="Red" Visible="true" />

    <table>
        <tr>
            <td>
                <asp:Label ID="lblEstacionamiento" runat="server" Text="Estacionamiento:" /></td>
            <td>
                <asp:DropDownList ID="ddlEstacionamientos" runat="server" required="required" />
            </td>
        </tr>
        <tr>
            <td>
                <asp:Label ID="lblLegajo" runat="server" Text="Legajo:" /></td>
            <td>
                <asp:TextBox ID="txtLegajo" runat="server" />
                <asp:RequiredFieldValidator ID="rfvLegajo" runat="server" ControlToValidate="txtLegajo"
                    ErrorMessage="El legajo es obligatorio." Display="Dynamic" CssClass="text-danger" />
            </td>
        </tr>
        <tr>
            <td>
                <asp:Label ID="lblDni" runat="server" Text="DNI:" /></td>
            <td>
                <asp:TextBox ID="txtDni" runat="server" />
                <asp:RequiredFieldValidator ID="rfvDNI" runat="server" ControlToValidate="txtDni"
                    ErrorMessage="El DNI es obligatorio." Display="Dynamic" CssClass="text-danger" />
            </td>
        </tr>
        <tr>
            <td>
                <asp:Label ID="lblPass" runat="server" Text="Contraseña:" /></td>
            <td>
                <asp:TextBox ID="txtPass" runat="server" TextMode="Password" />
                <asp:RequiredFieldValidator ID="rfvPass" runat="server" ControlToValidate="txtPass"
                    ErrorMessage="La Contraseña es obligatoria." Display="Dynamic" CssClass="text-danger" />
            </td>
        </tr>
        <tr>
            <td>
                <asp:Label ID="lblApellido" runat="server" Text="Apellido:" /></td>
            <td>
                <asp:TextBox ID="txtApellido" runat="server" />
                <asp:RequiredFieldValidator ID="rfvApellido" runat="server" ControlToValidate="txtApellido"
                    ErrorMessage="El Apellido es obligatorio." Display="Dynamic" CssClass="text-danger" />
            </td>
        </tr>
        <tr>
            <td>
                <asp:Label ID="lblNombre" runat="server" Text="Nombre:" /></td>
            <td>
                <asp:TextBox ID="txtNombre" runat="server" />
                <asp:RequiredFieldValidator ID="rfvNombre" runat="server" ControlToValidate="txtNombre"
                    ErrorMessage="El Nombre es obligatorio." Display="Dynamic" CssClass="text-danger" />
            </td>
        </tr>
        <tr>
            <td>
                <asp:Button ID="btnGuardar" runat="server" Text="Guardar" OnClick="btnGuardar_Click" />
                <asp:Button ID="btnCancelar" runat="server" Text="Cancelar" PostBackUrl="~/Pages/Playeros/Playero_CRUD.aspx" CausesValidation="false" />
            </td>
        </tr>
    </table>

</asp:Content>
