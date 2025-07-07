<%@ Page Title="Estacionamiento" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="EstacionamientoCRUD.aspx.cs" Inherits="Proyecto_Estacionamiento.Pages.Estacionamiento.EstacionamientoCRUD" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Estacionamientos</h2>


    <asp:Button ID="btnCrear" runat="server" Text="Crear nuevo Estacionamiento" OnClick="btnCrear_Click" />
    <br />
    <br />

    <asp:GridView ID="gvEstacionamientos" runat="server" AutoGenerateColumns="False"
        OnRowCommand="gvEstacionamientos_RowCommand"
        DataKeyNames="Est_id"
        CssClass="grid" Width="100%" OnSelectedIndexChanged="gvEstacionamientos_SelectedIndexChanged">
        
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

    </asp:GridView>

</asp:Content>
