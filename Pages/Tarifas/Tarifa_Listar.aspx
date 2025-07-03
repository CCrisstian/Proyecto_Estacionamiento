<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Tarifa_Listar.aspx.cs" Inherits="Proyecto_Estacionamiento.Pages.Tarifas.Tarifa_Listar" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Tarifas</h2>
    <asp:Button ID="btnAgregarTarifa" runat="server" Text="Agregar Tarifa" OnClick="btnAgregarTarifa_Click" />
    <br />
    <br />

    <asp:GridView ID="gvTarifas" runat="server" AutoGenerateColumns="False" OnRowCommand="gvTarifas_RowCommand" Width="900px">
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
    </asp:GridView>
</asp:Content>
