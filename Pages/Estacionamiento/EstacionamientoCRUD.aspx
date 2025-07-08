<%@ Page Title="Estacionamiento" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="EstacionamientoCRUD.aspx.cs" Inherits="Proyecto_Estacionamiento.Pages.Estacionamiento.EstacionamientoCRUD" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Estacionamientos</h2>


    <asp:Button ID="btnCrear" runat="server" Text="Crear nuevo Estacionamiento" OnClick="btnCrear_Click" />
    <br />
    <br />

    <asp:GridView ID="gvEstacionamientos" runat="server" AutoGenerateColumns="False"
        OnRowCommand="gvEstacionamientos_RowCommand"
        DataKeyNames="Est_id"
        CssClass="grid" Width="100%" OnSelectedIndexChanged="gvEstacionamientos_SelectedIndexChanged" CellPadding="4" ForeColor="#333333" GridLines="None">
        
        <AlternatingRowStyle BackColor="White" />
        
        <Columns>
            <asp:BoundField DataField="Est_provincia" HeaderText="Provincia" />
            <asp:BoundField DataField="Est_localidad" HeaderText="Localidad" />
            <asp:BoundField DataField="Est_direccion" HeaderText="Dirección" />
            <asp:BoundField DataField="Est_nombre" HeaderText="Nombre" />
            <asp:BoundField DataField="Est_puntaje" HeaderText="Puntaje" />
            <asp:BoundField DataField="Est_Disponibilidad" HeaderText="Disponibilidad" />

            <asp:TemplateField>
                <ItemTemplate>
                    <asp:Button ID="btnEditar" runat="server" Text="Editar" CommandName="EditarCustom"
                        CommandArgument='<%# Container.DataItemIndex %>' />
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
