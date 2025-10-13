<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Abonados_Listar.aspx.cs" Inherits="Proyecto_Estacionamiento.Pages.Abonados.Abonados_Listar" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="header-row">
        <h2>Abonados</h2>
        <asp:Label ID="Estacionamiento_Nombre" runat="server" CssClass="right-text"></asp:Label>
    </div>

    <asp:Button ID="btnRegistrar" runat="server" Text="Registrar Abonado" OnClick="btnRegistrar_Click" CssClass="btn btn-success" />
    
    <br />
    <br />

    <div class="grid-wrapper">
        <asp:GridView ID="gvAbonos" runat="server"
            AutoGenerateColumns="False"
            CssClass="grid"
            Width="100%"
            EmptyDataText="No hay abonos registrados para este estacionamiento.">

            <Columns>
                <asp:BoundField DataField="Patente" HeaderText="Patente" />
                <asp:BoundField DataField="Plaza" HeaderText="Plaza" />
                <asp:BoundField DataField="Desde" HeaderText="Desde" DataFormatString="{0:dd/MM/yyyy HH:mm}" />
                <asp:BoundField DataField="Hasta" HeaderText="Hasta" DataFormatString="{0:dd/MM/yyyy HH:mm}" />
            </Columns>

        </asp:GridView>
    </div>

    <%-- SweetAlert2 para mensajes de exito --%>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <% 
        if (!IsPostBack && Request.QueryString["exito"] == "1")
        {
            string titulo = "El Abono fue registrado correctamente";
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

