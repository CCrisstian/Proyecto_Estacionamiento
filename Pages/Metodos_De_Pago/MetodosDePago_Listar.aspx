<%@ Page Title="Metodos de Pago" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="MetodosDePago_Listar.aspx.cs" Inherits="Proyecto_Estacionamiento.Pages.Metodos_De_Pago.MetodosDePago_Listar" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="header-row">
        <h2>Métodos de Pago</h2>
        <asp:Label ID="Estacionamiento_Nombre" runat="server" CssClass="right-text"></asp:Label>
    </div>

    <asp:Button ID="btnAgregar" runat="server" Text="Agregar Método de Pago" CssClass="btn btn-success" OnClick="btnAgregar_Click" />

    <div class="right-align-filters">
        <div class="filtro-grupo">
            <asp:Label ID="lblOrdenarPor" runat="server" Text="Ordenar por:" />

            <asp:DropDownList ID="ddlCamposOrden" runat="server" CssClass="form-control"></asp:DropDownList>
            
            <asp:DropDownList ID="ddlDireccionOrden" runat="server" CssClass="form-control">
                <asp:ListItem Text="ASC" Value="ASC" />
                <asp:ListItem Text="DESC" Value="DESC" />
            </asp:DropDownList>

            <asp:Button ID="btnOrdenar" runat="server" Text="Ordenar" OnClick="btnOrdenar_Click" CssClass="btn btn-primary" />
        </div>
    </div>

    <br />

    <div class="grid-wrapper">
        <asp:GridView ID="gvMetodosPago" runat="server" AutoGenerateColumns="False"
            CssClass="grid"
            DataKeyNames="Est_id,Metodo_Pago_id"
            OnRowDataBound="gvMetodosPago_RowDataBound"
            OnRowCommand="gvMetodosPago_RowCommand"
            Width="100%" CellPadding="4" GridLines="None">
            <Columns>

                <asp:BoundField DataField="Est_nombre" HeaderText="Estacionamiento" />

                <asp:BoundField DataField="Metodo_pago_descripcion" HeaderText="Método de Pago" />

                <asp:BoundField DataField="AMP_Desde" HeaderText="Desde" DataFormatString="{0:dd/MM/yyyy}" />

                <asp:BoundField DataField="AMP_Hasta" HeaderText="Hasta" DataFormatString="{0:dd/MM/yyyy}" />

                <asp:TemplateField>
                    <ItemTemplate>
                        <asp:Button ID="btnEditar" runat="server" Text="Editar" CommandName="Editar"
                            CommandArgument='<%# Container.DataItemIndex %>'
                            CssClass="btn btn-primary" />
                    </ItemTemplate>
                </asp:TemplateField>

            </Columns>
        </asp:GridView>
    </div>


    <%-- SweetAlert2 para mensajes de exito --%>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <% 
        if (!IsPostBack && Request.QueryString["exito"] == "1")
        {
            string accion = Request.QueryString["accion"] ?? "guardado";
            string titulo = accion == "agregado"
                ? "El Método de Pago fue 'Agregado' correctamente"
                : "El Método de Pago fue 'Editado' correctamente";
    %>
    <script>
        Swal.fire({
            position: "center",
            icon: "success",
            title: "<%= titulo %>",
            showConfirmButton: false,
            timer: 3000
        });

        // 🔹 Limpia los parámetros de la URL sin recargar
        if (window.history.replaceState) {
            const url = new URL(window.location);
            url.search = ""; // eliminamos query string
            window.history.replaceState(null, null, url.toString());
        }
    </script>
    <% } %>
</asp:Content>
