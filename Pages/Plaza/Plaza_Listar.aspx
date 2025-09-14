﻿<%@ Page Title="Plazas" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Plaza_Listar.aspx.cs" Inherits="Proyecto_Estacionamiento.Pages.Plaza.Plaza_Listar" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="header-row">
        <h1>Plazas</h1>
        <asp:Label ID="Estacionamiento_Nombre" runat="server" CssClass="right-text"></asp:Label>
    </div>

    <asp:Button ID="btnAgregar" runat="server" Text="Agregar Plaza"
        OnClick="btnAgregar_Click"
        CssClass="btn btn-success" />

    <br />
    <br />

    <div class="grid-wrapper">
        <asp:GridView ID="gvPlazas" runat="server" AutoGenerateColumns="False"
            OnRowCommand="gvPlazas_RowCommand"
            OnRowDataBound="gvPlazas_RowDataBound"
            CssClass="grid"
            Width="100%">

            <AlternatingRowStyle BackColor="White" />

            <Columns>
                <asp:BoundField DataField="Estacionamiento.Est_nombre" HeaderText="Estacionamiento" />
                <asp:BoundField DataField="Plaza_Nombre" HeaderText="Nombre" />
                <asp:BoundField DataField="Plaza_Tipo" HeaderText="Tipo" />
                <asp:BoundField DataField="Categoria_Vehiculo.Categoria_descripcion" HeaderText="Categoría" />
                <asp:TemplateField HeaderText="Disponible">
                    <ItemTemplate>
                        <%# Convert.ToBoolean(Eval("Plaza_Disponibilidad")) 
            ? "<span>&#9989;</span>" 
            : "<span>&#10060;</span>" %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField>
                    <ItemTemplate>
                        <asp:Button ID="btnEditar" runat="server" Text="Editar"
                            CommandName="Editar"
                            CommandArgument='<%# Eval("Plaza_id") %>'
                            CssClass="btn btn-primary btn-grid" />
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>

        </asp:GridView>
    </div>

    <%-- SweetAlert2 para mensajes de exito --%>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <% 
        if (Request.QueryString["exito"] == "1")
        {
            string accion = Request.QueryString["accion"] ?? "guardado";
            string titulo = accion == "agregado"
                ? "La 'Plaza' fue 'Agregada' correctamente"
                : "La 'Plaza' fue 'Editada' correctamente";
    %>
    <script>
        Swal.fire({
            position: "center",
            icon: "success",
            title: "<%= titulo %>",
            showConfirmButton: false,
            timer: 3000
        });
    </script>
    <% } %>
</asp:Content>
