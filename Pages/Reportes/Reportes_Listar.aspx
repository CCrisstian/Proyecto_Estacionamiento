<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Reportes_Listar.aspx.cs" Inherits="Proyecto_Estacionamiento.Pages.Reportes.Reportes_Listar" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="header-row">
        <h2>Reportes</h2>
        <asp:Label ID="Estacionamiento_Nombre" runat="server" CssClass="right-text"></asp:Label>
    </div>

    <asp:Button ID="btnReporte" runat="server" Text="Registrar Reporte" OnClick="btnReporte_Click" CssClass="btn btn-success" />

    <div class="grid-wrapper">
        <asp:GridView ID="gvReportes" runat="server"
            AutoGenerateColumns="False"
            DataKeyNames="Playero_legajo,Inci_fecha_Hora"
            CssClass="grid"
            Width="100%"
            EmptyDataText="No se encontraron reportes de incidencias."
            OnRowCreated="gvReportes_RowCreated">

            <Columns>

                <asp:BoundField DataField="PlayeroNombre" HeaderText="Playero" SortExpression="PlayeroNombre" />

                <asp:BoundField DataField="FechaHoraStr" HeaderText="Fecha y Hora" SortExpression="Inci_fecha_Hora" />

                <asp:BoundField DataField="Inci_Motivo" HeaderText="Motivo" SortExpression="Inci_Motivo" />

                <asp:TemplateField HeaderText="Estado" SortExpression="Inci_Estado">
                    <ItemTemplate>
                        <asp:Label ID="lblEstado" runat="server"
                            Text='<%# Eval("EstadoStr") %>'
                            Font-Bold="true"
                            ForeColor='<%# (bool)Eval("Inci_Estado") ? System.Drawing.Color.Green : System.Drawing.Color.Red %>' />
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField HeaderText="Descripción" ItemStyle-CssClass="grid-cell-centered">
                    <ItemTemplate>
                        <a href="#"
                            class="btn"
                            onclick="mostrarDescripcion(this); return false;"
                            data-descripcion='<%# Eval("Inci_descripcion") %>'>🔍
                        </a>
                    </ItemTemplate>
                </asp:TemplateField>


                <asp:TemplateField HeaderText="Descargar" ItemStyle-CssClass="grid-cell-centered">
                    <ItemTemplate>

                        <asp:HyperLink ID="hlDescargar" runat="server"
                            Text="💾"
                            ToolTip="Descargar Reporte (PDF)"
                            CssClass="btn"
                            NavigateUrl='<%# Eval("DownloadUrl") %>'
                            Target="_blank" />
                    </ItemTemplate>
                </asp:TemplateField>

            </Columns>
        </asp:GridView>
    </div>

    <%-- SweetAlert2 para mensajes de exito --%>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <script type="text/javascript">
        function mostrarDescripcion(elementoBoton) {
            var descripcion = elementoBoton.dataset.descripcion;

            if (!descripcion || descripcion.trim() === "") {
                descripcion = "No se proporcionó una descripción detallada.";
            }

            Swal.fire({
                title: "Descripción de la Incidencia",
                html: `<hr/><div style="text-align:left; white-space: pre-wrap;">${descripcion}</div><hr/>`,
                icon: "info",
                confirmButtonText: "Cerrar"
            });
        }
    </script>

    <% 
        if (!IsPostBack && Request.QueryString["exito"] == "1")
        {
    %>
    <script>
        Swal.fire({
            position: "center",
            icon: "success",
            title: "Incidencia registrada correctamente",
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
