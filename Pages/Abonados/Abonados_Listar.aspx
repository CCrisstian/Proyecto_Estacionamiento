<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Abonados_Listar.aspx.cs" Inherits="Proyecto_Estacionamiento.Pages.Abonados.Abonados_Listar" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="header-row">
        <h2>Abonados</h2>
        <asp:Label ID="Estacionamiento_Nombre" runat="server" CssClass="right-text"></asp:Label>
    </div>

    <asp:Button ID="btnRegistrar" runat="server" Text="Registrar Abonado" OnClick="btnRegistrar_Click" CssClass="btn btn-success" />

    <br />
    <br />

    <div class="search-bar-container" style="margin-bottom: 20px; display: flex; align-items: center; max-width: 450px;">
        <asp:TextBox ID="txtBuscarPatente" runat="server"
            CssClass="form-control"
            placeholder="Buscar por patente..."
            OnTextChanged="txtBuscarPatente_TextChanged"
            AutoPostBack="true"
            Style="margin-right: 10px;" />
        <asp:Button ID="btnBuscarPatente" runat="server"
            Text="Buscar"
            OnClick="btnBuscarPatente_Click"
            CssClass="btn btn-primary" />
    </div>

    <div class="grid-wrapper">
        <asp:GridView ID="gvAbonos" runat="server"
            AutoGenerateColumns="False"
            CssClass="grid"
            Width="100%"
            EmptyDataText="No se encontraron abonos vigentes...">

            <Columns>
                <asp:BoundField DataField="Nombre" HeaderText="Nombre" SortExpression="Nombre" />
                <asp:BoundField DataField="Apellido" HeaderText="Apellido" SortExpression="Apellido" />
                <asp:BoundField DataField="Plaza" HeaderText="Plaza" SortExpression="Plaza" />

                <asp:TemplateField HeaderText="Detalles" ItemStyle-CssClass="grid-cell-centered">
                    <ItemTemplate>
                        <a href="#"
                            class="btn btn-info btn-sm"
                            onclick="mostrarDetallesAbono(this); return false;"
                            data-patentes='<%# Eval("PatentesStr") %>'
                            data-desde='<%# Eval("FechaDesdeStr") %>'
                            data-hasta='<%# Eval("FechaVtoStr") %>'>🔍
                        </a>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>

        </asp:GridView>
    </div>

    <%-- SweetAlert2 para mensajes de exito --%>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <script type="text/javascript">

        // Función JS que lee los datos y muestra el modal
        function mostrarDetallesAbono(elementoBoton) {

            // Leemos los datos desde el dataset
            var patentes = elementoBoton.dataset.patentes;
            var desde = elementoBoton.dataset.desde;
            var hasta = elementoBoton.dataset.hasta;

            // Formateamos el string de patentes para que se vea como una lista
            var patentesHtml = patentes.split(',')
                .map(p => p.trim())
                .join('<br>');

            if (!patentes || patentes.trim() === "") { // Mejor validación para patentes vacías
                patentesHtml = "No hay patentes asignadas.";
            }

            // Creamos el HTML para el modal
            var modalHtml = `
        <div style="text-align:left; font-family:monospace; font-size: 1.1em;">
            <p><b>Vigencia del Abono</b></p>
            <p><b>Desde:</b> ${desde}</p>
            <p><b>Hasta:</b> ${hasta}</p>
            <hr/>
            <p><b>Patentes Asignadas:</b></p>
            <div style="padding-left: 15px;">${patentesHtml}</div>
        </div>`;

            // Mostramos el SweetAlert
            Swal.fire({
                title: "Detalles del Abono",
                html: modalHtml,
                icon: "info",
                confirmButtonText: "Cerrar"
            });
        }
    </script>

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

