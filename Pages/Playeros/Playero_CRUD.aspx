<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Site.Master"
    CodeBehind="Playero_CRUD.aspx.cs"
    Inherits="Proyecto_Estacionamiento.Pages.Playeros.Playero_CRUD" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Lista de Playeros</h2>

    <asp:Button ID="btnAgregarPlayero" runat="server" Text="Agregar Playero" OnClick="btnAgregarPlayero_Click" />
    <br />
    <br />

    <asp:GridView ID="gvPlayeros" runat="server" AutoGenerateColumns="False"
        DataKeyNames="Playero_legajo"
        OnRowCommand="gvPlayeros_RowCommand"
        OnRowDataBound="gvPlayeros_RowDataBound"
        OnRowDeleting="gvPlayeros_RowDeleting">
        <Columns>
            <asp:BoundField DataField="Est_id" HeaderText="Estacionamiento" />
            <asp:BoundField DataField="Playero_legajo" HeaderText="Legajo" />
            <asp:BoundField DataField="Usu_dni" HeaderText="DNI" />
            <asp:BoundField DataField="Usu_pass" HeaderText="Contraseña" />
            <asp:BoundField DataField="Usu_ap" HeaderText="Apellido" />
            <asp:BoundField DataField="Usu_nom" HeaderText="Nombre" />
            <asp:BoundField DataField="Usu_tipo" HeaderText="Tipo Usuario" />

            <asp:ButtonField ButtonType="Button" CommandName="Editar" Text="Editar" />

            <asp:TemplateField>
    <ItemTemplate>
        <asp:Button ID="btnEliminar" runat="server" Text="Eliminar" CommandName="Delete"
            OnClientClick="return confirm('¿Está seguro que desea eliminar este playero?');" />
    </ItemTemplate>
</asp:TemplateField>

        </Columns>
    </asp:GridView>

</asp:Content>
