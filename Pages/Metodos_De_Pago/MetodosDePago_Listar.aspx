<%@ Page Title="Metodos de Pago" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="MetodosDePago_Listar.aspx.cs" Inherits="Proyecto_Estacionamiento.Pages.Metodos_De_Pago.MetodosDePago_Listar" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Métodos de Pago</h2>

    <asp:Button ID="btnAgregar" runat="server" Text="Agregar Método de Pago" CssClass="btn btn-success" OnClick="btnAgregar_Click" />

    <br />
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
        if (Request.QueryString["exito"] == "1")
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
    </script>
    <% } %>
</asp:Content>
