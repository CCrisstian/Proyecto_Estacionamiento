<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Abonados_Listar.aspx.cs" Inherits="Proyecto_Estacionamiento.Pages.Abonados.Abonados_Listar" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <%-- ESTILO FILA --%>
    <style type="text/css">
        .abono-vencido-hoy {
            background-color: #d24025; /* Un rojo pálido */
            color: #721c24 !important; /* Texto oscuro para contraste */
        }
    </style>
    <%-- FIN DEL BLOQUE DE ESTILO --%>

    <div class="header-row">
        <h2>Abonados</h2>
        <asp:Label ID="Estacionamiento_Nombre" runat="server" CssClass="right-text"></asp:Label>
    </div>

    <br />

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
            EmptyDataText="No se encontraron abonos vigentes..."
            OnRowDataBound="gvAbonos_RowDataBound">

            <Columns>

                <asp:BoundField DataField="TipoIdentificacion" HeaderText="Tipo Doc." SortExpression="TipoIdentificacion" />
                <asp:BoundField DataField="NumeroIdentificacion" HeaderText="N° Identificación" SortExpression="NumeroIdentificacion" />

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
                            data-hasta='<%# Eval("FechaVtoStr") %>'
                            data-vto-raw='<%# Eval("FechaVtoRaw") %>'
                            data-tipoabono='<%# Eval("TipoAbono") %>'>🔍
                        </a>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
    </div>

    <%-- SweetAlert2 para mensajes de exito --%>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <script type="text/javascript">
        function mostrarDetallesAbono(elementoBoton) {

            // 1. Leemos los datos desde el dataset
            var patentes = elementoBoton.dataset.patentes;
            var desde = elementoBoton.dataset.desde;
            var hasta = elementoBoton.dataset.hasta;
            var vtoRaw = elementoBoton.dataset.vtoRaw; // <-- Leemos la fecha raw
            var tipoAbono = elementoBoton.dataset.tipoabono;

            // 2. Comparamos si la fecha de vencimiento es hoy
            var hoy = new Date();
            var hoyStr = hoy.getFullYear() + '-' +
                ('0' + (hoy.getMonth() + 1)).slice(-2) + '-' +
                ('0' + hoy.getDate()).slice(-2); // Formato YYYY-MM-DD

            var venceHoy = (vtoRaw === hoyStr);
            var swalIcon = venceHoy ? "warning" : "info"; // Cambia el icono si vence hoy
            var mensajeVencimiento = venceHoy
                ? '<p style="color:red; font-weight:bold;">¡Este Abono vence hoy!</p>'
                : ''; // Agrega mensaje si vence hoy

            // 3. Formateamos patentes
            var patentesHtml = patentes.split(',')
                .map(p => p.trim())
                .join('<br>');
            if (!patentes || patentes.trim() === "") {
                patentesHtml = "No hay patentes asignadas.";
            }

            // 4. Creamos el HTML para el modal (añadiendo el mensaje de vencimiento)
            var modalHtml = `
            <div style="text-align:left; font-family:monospace; font-size: 1.1em;">
                ${mensajeVencimiento} <%-- Mensaje de vencimiento (si aplica) --%>
                <hr/>
                <p><b>Tipo de Abono:</b> ${tipoAbono}</p>
                <hr/>
                <h4>Vigencia del Abono</h4>
                <p><b>Desde:</b> ${desde}</p>
                <p><b>Hasta:</b> ${hasta}</p>
                <hr/>
                <h4>Patentes Asignadas:</h4>
                <div style="padding-left: 15px;">${patentesHtml}</div>
            </div>`;

            // 5. Mostramos el SweetAlert (con el icono condicional)
            Swal.fire({
                title: "Detalles del Abono",
                html: modalHtml,
                icon: swalIcon, // Icono dinámico
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

