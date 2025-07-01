<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" 
    AutoEventWireup="true" 
    CodeBehind="Estacionamiento_Crear.aspx.cs"
    Inherits="Proyecto_Estacionamiento.Pages.Estacionamiento.Estacionamiento_Crear" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div>
        <asp:Panel ID="pnlFormulario" runat="server" CssClass="formulario-estacionamiento">
            <h3>Ingrese los Datos del Estacionamiento:</h3>

            <asp:Label runat="server" AssociatedControlID="txtNombre" Text="Nombre:" />
            <asp:TextBox ID="txtNombre" runat="server" Width="285px"></asp:TextBox>
            <br />
            <br />

            <asp:Label runat="server" AssociatedControlID="txtDireccion" Text="Dirección:" />
            <asp:TextBox ID="txtDireccion" runat="server" Width="284px"></asp:TextBox>
            <br />

            <asp:Label runat="server" Text="Provincia:" />
            <asp:DropDownList ID="ddlProvincia" runat="server">
                <asp:ListItem>Chaco</asp:ListItem>
                <asp:ListItem>Corrientes</asp:ListItem>
                <asp:ListItem>Formosa</asp:ListItem>
            </asp:DropDownList>

            <asp:Label runat="server" Text="Localidad:" />
            <asp:DropDownList ID="ddlLocalidad" runat="server">
                <asp:ListItem>Resistencia</asp:ListItem>
                <asp:ListItem>Barranqueras</asp:ListItem>
                <asp:ListItem>Fontana</asp:ListItem>
                <asp:ListItem>Vilelas</asp:ListItem>
            </asp:DropDownList>
            <br />

            <asp:Label runat="server" Text="Horario (Inicio - Fin):" />
            <asp:DropDownList ID="ddlHoraInicio" runat="server" />
            <asp:DropDownList ID="ddlHoraFin" runat="server" />
            <br />
            
            <!-- Días de Atención
            <asp:Label runat="server" Text="Días de Atención:" />
            <asp:ListBox ID="lstDias" runat="server" SelectionMode="Multiple">
                <asp:ListItem>Lunes</asp:ListItem>
                <asp:ListItem>Martes</asp:ListItem>
                <asp:ListItem>Miercoles</asp:ListItem>
                <asp:ListItem>Jueves</asp:ListItem>
                <asp:ListItem>Viernes</asp:ListItem>
                <asp:ListItem>Sábados</asp:ListItem>
                <asp:ListItem>Domingos</asp:ListItem>
            </asp:ListBox>
            -->

            <br />
            <br />
            
            <asp:Label ID="lblMensaje" runat="server" ForeColor="Red" />    <!-- Mensajes de Error -->

            <asp:Button ID="btnCancelar" runat="server" Text="Cancelar" OnClick="btnCancelar_Click" />

            <asp:Button ID="btnGuardar" runat="server" Text="Guardar" OnClick="btnGuardar_Click" />

        </asp:Panel>

    </div>
</asp:Content>
