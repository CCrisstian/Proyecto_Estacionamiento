<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Turno_Listar.aspx.cs" Inherits="Proyecto_Estacionamiento.Pages.Turnos.Turno_Listar" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="header-row">
        <h2>Turnos</h2>
        <asp:Label ID="Estacionamiento_Nombre" runat="server" CssClass="right-text"></asp:Label>
    </div>

    <br />

    <div class="form-section">
        <div class="form-row">
            <asp:Label ID="lblMontoInicio" runat="server" Text="Monto de Inicio:" CssClass="form-label" />

            <div>
                <asp:TextBox ID="txtMontoInicio" runat="server" CssClass="form-control" />
            </div>

            <asp:Button ID="btnInicioTurno" runat="server" Text="Inicio de Turno"
                CssClass="btn btn-success"
                OnClick="btnInicioTurno_Click"
                OnClientClick="return confirmarAccion(this, 'inicio');" />
        </div>

        <div class="form-row">
            <asp:Button ID="btnFinTurno" runat="server" Text="Fin de Turno"
                CssClass="btn btn-danger"
                OnClick="btnFinTurno_Click"
                OnClientClick="return confirmarAccion(this, 'fin');" />
        </div>
    </div>

    <br />

    <div class="right-align-filters">
        <div class="filtro-grupo">

            <asp:Label ID="lblEstacionamiento" runat="server" Text="Estacionamiento:" Visible="false" />
            <asp:DropDownList ID="ddlEstacionamiento" runat="server" CssClass="form-control" Visible="false"
                AutoPostBack="true" OnSelectedIndexChanged="ddlEstacionamiento_SelectedIndexChanged">
            </asp:DropDownList>

            <asp:CustomValidator ID="cvEstacionamiento" runat="server"
                OnServerValidate="CvEstacionamiento_ServerValidate"
                Display="Dynamic" ForeColor="Red"
                ValidationGroup="Turno" />

            <asp:Label ID="lblDesde" runat="server" Text="Desde:" />
            <asp:TextBox ID="txtDesde" runat="server" CssClass="form-control filtro-fecha" />
            <ajaxToolkit:CalendarExtender ID="calDesde" runat="server" TargetControlID="txtDesde" Format="dd/MM/yyyy" />

            <asp:CustomValidator ID="cvDesde" runat="server"
                OnServerValidate="CvDesde_ServerValidate"
                Display="Dynamic" ForeColor="Red"
                ValidationGroup="Turno" />

            <asp:Label ID="lblHasta" runat="server" Text="Hasta:" />
            <asp:TextBox ID="txtHasta" runat="server" CssClass="form-control filtro-fecha" />
            <ajaxToolkit:CalendarExtender ID="calHasta" runat="server" TargetControlID="txtHasta" Format="dd/MM/yyyy" />

            <asp:CustomValidator ID="cvHasta" runat="server"
                OnServerValidate="CvHasta_ServerValidate"
                Display="Dynamic" ForeColor="Red"
                ValidationGroup="Turno" />

            <asp:Button ID="btnFiltrarTurno"
                runat="server"
                Text="Buscar"
                OnClick="btnFiltrarTurno_Click"
                CssClass="btn btn-primary" />
        </div>
    </div>

    <br />

    <div class="grid-wrapper">

        <asp:GridView ID="GridViewTurnos" runat="server" AutoGenerateColumns="False"
            CssClass="grid" Width="100%" CellPadding="4" ForeColor="#333333" GridLines="None">

            <Columns>
                <asp:BoundField DataField="Estacionamiento" HeaderText="Estacionamiento" />
                <asp:BoundField DataField="Playero" HeaderText="Playero" />
                <asp:BoundField DataField="Inicio" HeaderText="Inicio de Turno" />

                <asp:BoundField DataField="Fin" HeaderText="Fin de Turno" />

                <asp:BoundField DataField="MontoInicio" HeaderText="Monto Inicio" />
                <asp:BoundField DataField="MontoFin" HeaderText="Monto Fin" />
                <asp:BoundField DataField="TotalRecaudado" HeaderText="Total Recaudado" />

                <asp:TemplateField HeaderText="Detalle" ItemStyle-CssClass="grid-cell-centered">
                    <ItemTemplate>
                        <a href="#"
                            class="btn btn-sm"
                            onclick="mostrarDetalleTurno(this); return false;"
                            data-detalle='<%# Server.HtmlEncode(Eval("DetalleHtml").ToString()) %>'>🔍
                        </a>
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField HeaderText="Descargar" ItemStyle-CssClass="grid-cell-centered">
                    <ItemTemplate>
                        <asp:HyperLink ID="hlDescargar" runat="server"
                            Text="💾"
                            ToolTip="Descargar"
                            CssClass="btn btn-sm"
                            NavigateUrl='<%# Eval("DownloadUrl") %>'
                            Target="_blank" />
                    </ItemTemplate>
                </asp:TemplateField>

            </Columns>
        </asp:GridView>
    </div>

    <%-- SweetAlert2 para mensajes de confirmación --%>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <script type="text/javascript">
        function mostrarDetalleTurno(btn) {
            // Obtenemos el HTML del atributo data-detalle
            var htmlDetalle = btn.dataset.detalle;

            Swal.fire({
                title: 'Detalle de Movimientos',
                html: htmlDetalle, // Inyectamos el HTML generado en C#
                width: '950px',    // Hacemos el modal más ancho para las tablas
                showCloseButton: true,
                showConfirmButton: false, // Solo botón de cierre X
                focusConfirm: false
            });
        }
    </script>
    <script>
        function confirmarAccion(btn, tipo) {
            let mensaje = tipo === "inicio"
                ? "¿Deseás 'Iniciar' el Turno?"
                : "¿Deseás 'Finalizar' el Turno?";

            Swal.fire({
                title: mensaje,
                icon: "question",
                showDenyButton: true,
                confirmButtonText: "Sí",
                denyButtonText: "No",
                reverseButtons: true // 👈 Esto invierte el orden de los botones
            }).then((result) => {
                if (result.isConfirmed) {
                    __doPostBack(btn.name, "");
                }
            });

            return false; // siempre detenemos el postback original
        }
    </script>
    <% 
        if (Request.QueryString["exito"] == "1")
        {
            string accion = Request.QueryString["accion"] ?? "";
            string titulo = accion == "inicio"
                ? "El Turno fue 'Iniciado' correctamente"
                : "El Turno fue 'Finalizado' correctamente";
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
