<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Tarifa_Listar.aspx.cs" Inherits="Proyecto_Estacionamiento.Pages.Tarifas.Tarifa_Listar" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="header-row">
        <h1>Tarifas</h1>
        <asp:Label ID="Estacionamiento_Nombre" runat="server" CssClass="right-text"></asp:Label>
    </div>

    <asp:Button ID="btnAgregarTarifa" runat="server" Text="Agregar Tarifa" CssClass="btn btn-success" OnClick="btnAgregarTarifa_Click" />

    <br />
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
