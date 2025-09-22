<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Site.Master"
    CodeBehind="Playero_Listar.aspx.cs"
    Inherits="Proyecto_Estacionamiento.Pages.Playeros.Playero_Listar" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="header-row">
        <h2>
            <asp:Literal ID="TituloPlayeros" runat="server" />
        </h2>
    </div>

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
                <asp:TemplateField HeaderText="Activo">
                    <ItemTemplate>
                        <%# Convert.ToBoolean(Eval("Playero_Activo")) 
? "<span>&#9989;</span>" 
: "<span>&#10060;</span>" %>
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField>
                    <ItemTemplate>
                        <asp:Button ID="btnEditar" runat="server" Text="Editar" CommandName="Editar"
                            CommandArgument='<%# Container.DataItemIndex %>' CssClass="btn btn-primary btn-grid" />
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
                ? "El 'Playero' fue 'Agregado' correctamente"
                : "El 'Playero' fue 'Editado' correctamente";
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
