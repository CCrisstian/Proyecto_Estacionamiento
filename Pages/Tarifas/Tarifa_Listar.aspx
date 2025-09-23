<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Tarifa_Listar.aspx.cs" Inherits="Proyecto_Estacionamiento.Pages.Tarifas.Tarifa_Listar" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="header-row">
        <h2>
            <asp:Literal ID="TituloTarifas" runat="server" />
        </h2>
    </div>

    <asp:Button ID="btnAgregarTarifa" runat="server" Text="Agregar Tarifa" CssClass="btn btn-success" OnClick="btnAgregarTarifa_Click" />

    <div class="right-align-filters">
        <div class="filtro-grupo">
            <asp:Label ID="lblOrdenarPor" runat="server" Text="Ordenar por:" />

            <asp:DropDownList ID="ddlCamposOrden" runat="server" CssClass="form-control">
                <asp:ListItem Text="Tipo de Tarifa" Value="Tipos_Tarifa_Descripcion" />
                <asp:ListItem Text="Categoría" Value="Categoria_descripcion" />
                <asp:ListItem Text="Monto" Value="Tarifa_Monto" />
                <asp:ListItem Text="Vigente Desde" Value="Tarifa_Desde" />
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

        <asp:GridView ID="gvTarifas" runat="server" AutoGenerateColumns="False"
            CssClass="grid"
            OnRowCommand="gvTarifas_RowCommand"
            OnRowDataBound="gvTarifas_RowDataBound"
            Width="100%" CellPadding="4" GridLines="None">

            <Columns>

                <asp:BoundField DataField="Est_nombre" HeaderText="Estacionamiento" />
                <asp:BoundField DataField="Tipos_Tarifa_Descripcion" HeaderText="Tipo de Tarifa" />
                <asp:BoundField DataField="Categoria_descripcion" HeaderText="Categoría" />
                <asp:BoundField DataField="Tarifa_Monto" HeaderText="Monto" DataFormatString="{0:C}" />
                <asp:BoundField DataField="Tarifa_Desde" HeaderText="Vigente Desde" DataFormatString="{0:dd/MM/yyyy}" />

                <asp:TemplateField>
                    <ItemTemplate>
                        <asp:Button ID="btnEditar" runat="server" Text="Editar" CommandName="Editar" CommandArgument='<%# Eval("Tarifa_id") %>' CssClass="btn btn-primary btn-grid" />
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
                ? "La 'Tarifa' fue 'Agregada' correctamente"
                : "La 'Tarifa' fue 'Editada' correctamente";
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
