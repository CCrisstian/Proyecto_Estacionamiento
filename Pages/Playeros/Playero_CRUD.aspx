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
        OnRowDeleting="gvPlayeros_RowDeleting" CellPadding="4" ForeColor="#333333" GridLines="None">
        <AlternatingRowStyle BackColor="White" />
        <Columns>
            <asp:BoundField DataField="Est_nombre" HeaderText="Estacionamiento" />
            <asp:BoundField DataField="Playero_legajo" HeaderText="Legajo" />
            <asp:BoundField DataField="Usu_pass" HeaderText="Contraseña" />
            <asp:BoundField DataField="Usu_dni" HeaderText="DNI" />
            <asp:BoundField DataField="Usu_ap" HeaderText="Apellido" />
            <asp:BoundField DataField="Usu_nom" HeaderText="Nombre" />

            <asp:ButtonField ButtonType="Button" CommandName="Editar" Text="Editar" />

            <asp:TemplateField>
                <ItemTemplate>
                    <asp:Button ID="btnEliminar" runat="server" Text="Eliminar" CommandName="Delete"
                        OnClientClick="return confirm('¿Está seguro que desea eliminar este playero?');" />
                </ItemTemplate>
            </asp:TemplateField>

        </Columns>

        <EditRowStyle BackColor="#7C6F57" />
        <FooterStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
        <HeaderStyle BackColor="#009900" Font-Bold="True" ForeColor="White" />
        <PagerStyle BackColor="#666666" ForeColor="White" HorizontalAlign="Center" />
        <RowStyle BackColor="#E3EAEB" />
        <SelectedRowStyle BackColor="#C5BBAF" Font-Bold="True" ForeColor="#333333" />
        <SortedAscendingCellStyle BackColor="#F8FAFA" />
        <SortedAscendingHeaderStyle BackColor="#246B61" />
        <SortedDescendingCellStyle BackColor="#D4DFE1" />
        <SortedDescendingHeaderStyle BackColor="#15524A" />

    </asp:GridView>

</asp:Content>
