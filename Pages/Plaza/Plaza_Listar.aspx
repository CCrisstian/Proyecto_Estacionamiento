<%@ Page Title="Plazas" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Plaza_Listar.aspx.cs" Inherits="Proyecto_Estacionamiento.Pages.Plaza.Plaza_Listar" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="header-row">
        <h2>
            <asp:Literal ID="TituloPlazas" runat="server" />
        </h2>
    </div>

    <asp:Button ID="btnAgregar" runat="server" Text="Agregar Plaza"
        OnClick="btnAgregar_Click"
        CssClass="btn btn-success" />

    <div class="right-align-filters">

        <div class="filtro-grupo">
            <asp:Label ID="lblOrdenarPor" runat="server" Text="Ordenar por:" />

            <asp:DropDownList ID="ddlCamposOrden" runat="server" CssClass="form-control">

            </asp:DropDownList>

            <asp:DropDownList ID="ddlDireccionOrden" runat="server" CssClass="form-control">
                <asp:ListItem Text="ASC" Value="ASC" />
                <asp:ListItem Text="DESC" Value="DESC" />
            </asp:DropDownList>

            <asp:Button ID="btnOrdenar" runat="server" Text="Ordenar" CssClass="btn btn-primary"
                OnClick="btnOrdenar_Click" />
        </div>
    </div>

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
