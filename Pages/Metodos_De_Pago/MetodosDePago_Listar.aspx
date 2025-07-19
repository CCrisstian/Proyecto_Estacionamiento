<%@ Page Title="Metodos de Pago" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="MetodosDePago_Listar.aspx.cs" Inherits="Proyecto_Estacionamiento.Pages.Metodos_De_Pago.MetodosDePago_Listar" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Métodos de Pago</h2>

    <asp:Button ID="btnAgregar" runat="server" Text="Agregar" OnClick="btnAgregar_Click" />
    <br />
    <br />

    <asp:GridView ID="gvMetodosPago" runat="server" AutoGenerateColumns="False"
        DataKeyNames="Est_id,Metodo_Pago_id"
        OnRowCommand="gvMetodosPago_RowCommand" Width="979px" CellPadding="4" ForeColor="#333333" GridLines="None">
        <AlternatingRowStyle BackColor="White" />
        <Columns>
            
            <asp:BoundField DataField="Est_nombre" HeaderText="Estacionamiento" />
            
            <asp:BoundField DataField="Metodo_pago_descripcion" HeaderText="Método de Pago" />

            <asp:BoundField DataField="AMP_Desde" HeaderText="Desde" DataFormatString="{0:dd/MM/yyyy}" />
            
            <asp:BoundField DataField="AMP_Hasta" HeaderText="Hasta" DataFormatString="{0:dd/MM/yyyy}" />

            <asp:ButtonField CommandName="Editar" Text="Editar" ButtonType="Button"/>
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
