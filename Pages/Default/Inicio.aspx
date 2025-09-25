<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Inicio.aspx.cs" Inherits="Proyecto_Estacionamiento.Pages.Default.Inicio" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="header-row">
        <h2>Seleccione un Estacionamiento</h2>
        <asp:Label ID="Estacionamiento_Nombre" runat="server" CssClass="right-text"></asp:Label>
    </div>

    <br />

    <asp:GridView ID="gvEstacionamientos" runat="server" AutoGenerateColumns="False"
        DataKeyNames="Est_id,Est_nombre"
        CssClass="grid"
        Width="40%"
        OnRowCommand="gvEstacionamientos_RowCommand"
        OnRowDataBound="gvEstacionamientos_RowDataBound">



        <AlternatingRowStyle BackColor="White" />

        <Columns>
            <asp:BoundField DataField="Est_nombre" HeaderText="Estacionamiento" />

            <asp:TemplateField>
                <ItemTemplate>
                    <asp:Button ID="btnSeleccionar" runat="server" Text="Seleccionar" CommandName="Seleccionar" CommandArgument='<%# Eval("Est_id") %>' CssClass="btn btn-primary" />
                </ItemTemplate>
                <ItemStyle Width="20%" HorizontalAlign="Center" />
            </asp:TemplateField>
        </Columns>
    </asp:GridView>

    <%-- SweetAlert2 --%>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <% 
        // Mostrar mensaje si se seleccionó un estacionamiento
        if (!IsPostBack && !string.IsNullOrEmpty(Request.QueryString["estSeleccionado"]))
        {
            string estNombre = Request.QueryString["estSeleccionado"];
    %>
    <script>
        Swal.fire({
            position: "center",
            icon: "success",
            title: "Estacionamiento seleccionado:<br><b><%= estNombre %></b>",
            showConfirmButton: false,
            timer: 3500
        });

        // 🔹 Limpia los parámetros de la URL sin recargar
        if (window.history.replaceState) {
            const url = new URL(window.location);
            url.search = ""; // eliminamos query string
            window.history.replaceState(null, null, url.toString());
        }
    </script>
    <% } %>

    <% 
        if (Request.QueryString["exito"] == "1" && Request.QueryString["accion"] != null)
        {
            string accion = Request.QueryString["accion"];
            string nombreEst = Request.QueryString["nombre"];

            if (!string.IsNullOrEmpty(nombreEst))
            {
    %>
    <script>
        Swal.fire({
            position: "center",
            icon: "success",
            title: "¡Bienvenido! Tu primer estacionamiento '<%= nombreEst %>' fue <%= accion %> correctamente",
            showConfirmButton: true
        });
    </script>
    <% 
            }
        }
    %>
</asp:Content>
