<%@ Page Title="Estacionamiento" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Estacionamiento_Listar.aspx.cs" Inherits="Proyecto_Estacionamiento.Pages.Estacionamiento.Estacionamiento_Listar" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    
    <div class="header-row">
        <h1>Estacionamientos</h1>
        <asp:Label ID="Estacionamiento_Nombre" runat="server" CssClass="right-text"></asp:Label>
    </div>

    <asp:Button ID="btnAgregar" runat="server" Text="Agregar Estacionamiento" OnClick="btnAgregar_Click" CssClass="btn btn-success" />
    <br />
    <br />

    <div class="grid-wrapper">

        <asp:GridView ID="gvEstacionamientos" runat="server" AutoGenerateColumns="False"
            OnRowCommand="gvEstacionamientos_RowCommand"
            DataKeyNames="Est_id"
            CssClass="grid" Width="100%">

            <Columns>
                <asp:BoundField DataField="Est_nombre" HeaderText="Nombre" />
                <asp:BoundField DataField="Est_provincia" HeaderText="Provincia" />
                <asp:BoundField DataField="Est_localidad" HeaderText="Localidad" />
                <asp:BoundField DataField="Est_direccion" HeaderText="Dirección" />
                <asp:BoundField DataField="Est_puntaje" HeaderText="Puntaje" />
                <asp:TemplateField HeaderText="Disponible">
                    <ItemTemplate>
                        <%# Convert.ToBoolean(Eval("Est_Disponibilidad")) 
? "<span>&#9989;</span>" 
: "<span>&#10060;</span>" %>
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField>
                    <ItemTemplate>
                        <asp:Button ID="btnEditar" runat="server" Text="Editar"
                            CommandName="EditarCustom"
                            CommandArgument='<%# Container.DataItemIndex %>'
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
                ? "El Estacionamiento fue 'Agregado' correctamente"
                : "El Estacionamiento fue 'Editado' correctamente";
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
