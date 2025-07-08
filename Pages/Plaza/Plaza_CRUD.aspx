<%@ Page Title="Plazas" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Plaza_CRUD.aspx.cs" Inherits="Proyecto_Estacionamiento.Pages.Plaza.Plaza_CRUD" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Plazas</h2>
    <asp:Button ID="btnAgregar" runat="server" Text="Agregar Plaza" OnClick="btnAgregar_Click" />
    <br />
    <br />
    <asp:GridView ID="gvPlazas" runat="server" AutoGenerateColumns="False" OnRowCommand="gvPlazas_RowCommand"
        OnRowDataBound="gvPlazas_RowDataBound"
        Width="900px" CellPadding="4" ForeColor="#333333" GridLines="None">
        <AlternatingRowStyle BackColor="White" />
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
