﻿<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Site.Master"
    CodeBehind="Playero_Listar.aspx.cs"
    Inherits="Proyecto_Estacionamiento.Pages.Playeros.Playero_Listar" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Lista de Playeros</h2>

    <asp:Button ID="btnAgregarPlayero" runat="server" Text="Agregar Playero"
        CssClass="btn btn-success" OnClick="btnAgregarPlayero_Click" />

    <br />
    <br />

    <div class="grid-wrapper">

        <asp:GridView ID="gvPlayeros" runat="server" AutoGenerateColumns="False"
            CssClass="grid"
            DataKeyNames="Playero_legajo"
            OnRowCommand="gvPlayeros_RowCommand" Width="100%">

            <Columns>
                <asp:BoundField DataField="Est_nombre" HeaderText="Estacionamiento" />
                <asp:BoundField DataField="Playero_legajo" HeaderText="Legajo" />
                <asp:BoundField DataField="Usu_pass" HeaderText="Contraseña" />
                <asp:BoundField DataField="Usu_dni" HeaderText="DNI" />
                <asp:BoundField DataField="Usu_ap" HeaderText="Apellido" />
                <asp:BoundField DataField="Usu_nom" HeaderText="Nombre" />
                <asp:BoundField DataField="Playero_Activo" HeaderText="Activo" />

                <asp:TemplateField>
                    <ItemTemplate>
                        <asp:Button ID="btnEditar" runat="server" Text="Editar" CommandName="Editar"
                            CommandArgument='<%# Container.DataItemIndex %>' CssClass="btn btn-primary btn-grid" />
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
    </div>

</asp:Content>
