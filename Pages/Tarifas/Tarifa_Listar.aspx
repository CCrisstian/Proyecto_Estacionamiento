<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Tarifa_Listar.aspx.cs" Inherits="Proyecto_Estacionamiento.Pages.Tarifas.Tarifa_Listar" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Tarifas</h2>
    <asp:Button ID="btnAgregarTarifa" runat="server" Text="Agregar Tarifa" OnClick="btnAgregarTarifa_Click" />
    <br />
    <br />

    <asp:GridView ID="gvTarifas" runat="server" AutoGenerateColumns="False" OnRowCommand="gvTarifas_RowCommand"
        OnRowDataBound="gvPlazas_RowDataBound"
        Width="900px" CellPadding="4" ForeColor="#333333" GridLines="None">
        <AlternatingRowStyle BackColor="White" />
        <Columns>

            <asp:BoundField DataField="Est_nombre" HeaderText="Estacionamiento" />
            <asp:BoundField DataField="Tipos_Tarifa_Descripcion" HeaderText="Tipo de Tarifa" />
            <asp:BoundField DataField="Categoria_descripcion" HeaderText="Categoría" />
            <asp:BoundField DataField="Tarifa_Monto" HeaderText="Monto" DataFormatString="{0:C}" />
            <asp:BoundField DataField="Tarifa_Desde" HeaderText="Vigente Desde" DataFormatString="{0:dd/MM/yyyy}" />

            <asp:TemplateField>
                <ItemTemplate>
                    <asp:Button ID="btnEditar" runat="server" Text="Editar" CommandName="Editar" CommandArgument='<%# Eval("Tarifa_id") %>' />
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
