<%@ Page Title="Plazas" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Plaza_CRUD.aspx.cs" Inherits="Proyecto_Estacionamiento.Pages.Plaza.Plaza_CRUD" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Plazas</h2>
    <asp:Button ID="btnAgregar" runat="server" Text="Agregar Plaza" OnClick="btnAgregar_Click" />
    <br /><br />
    <asp:GridView ID="gvPlazas" runat="server" AutoGenerateColumns="False" OnRowCommand="gvPlazas_RowCommand" Width="900px">
        <Columns>
            <asp:BoundField DataField="Estacionamiento.Est_nombre" HeaderText="Estacionamiento" />
            <asp:BoundField DataField="Plaza_Nombre" HeaderText="Nombre" />
            <asp:BoundField DataField="Plaza_Tipo" HeaderText="Tipo" />
            <asp:BoundField DataField="Categoria_Vehiculo.Categoria_descripcion" HeaderText="Categoría" />
            <asp:BoundField DataField="Plaza_Disponibilidad" HeaderText="Disponible" />
            <asp:TemplateField>
                <ItemTemplate>
                    <asp:Button ID="btnEditar" runat="server" Text="Editar" CommandName="Editar" CommandArgument='<%# Eval("Plaza_id") %>' />
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>
</asp:Content>