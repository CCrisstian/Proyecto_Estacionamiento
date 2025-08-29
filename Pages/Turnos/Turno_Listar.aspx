<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Turno_Listar.aspx.cs" Inherits="Proyecto_Estacionamiento.Pages.Turnos.Turno_Listar" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Lista de Turnos</h2>

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
            <asp:Label ID="lblMontoFin" runat="server" Text="Monto de Fin:" CssClass="form-label" />

            <div>
                <asp:TextBox ID="txtMontoFin" runat="server" CssClass="form-control" />
            </div>

            <asp:Button ID="btnFinTurno" runat="server" Text="Fin de Turno"
                CssClass="btn btn-danger"
                OnClick="btnFinTurno_Click"
                OnClientClick="return confirmarAccion(this, 'fin');" />
        </div>
    </div>

    <br />

    <div class="grid-wrapper">

        <asp:GridView ID="GridViewTurnos" runat="server" AutoGenerateColumns="False"
            CssClass="grid" Width="100%" CellPadding="4" ForeColor="#333333" GridLines="None">

            <Columns>
                <asp:BoundField DataField="Estacionamiento" HeaderText="Estacionamiento" />

                <asp:TemplateField HeaderText="Playero">
                    <ItemTemplate>
                        <%# Eval("ApellidoPlayero") %>, <%# Eval("NombrePlayero") %>
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:BoundField DataField="Turno_FechaHora_Inicio" HeaderText="Inicio de Turno"
                    DataFormatString="{0:dd/MM/yyyy HH:mm}" HtmlEncode="false" />
                <asp:BoundField DataField="Turno_FechaHora_fin" HeaderText="Fin de Turno"
                    DataFormatString="{0:dd/MM/yyyy HH:mm}" HtmlEncode="false" />
                <asp:BoundField DataField="Caja_Monto_Inicio" HeaderText="Monto Inicio"
                    DataFormatString="{0:C}" HtmlEncode="false" />
                <asp:BoundField DataField="Caja_Monto_fin" HeaderText="Monto Fin"
                    DataFormatString="{0:C}" HtmlEncode="false" />
                <asp:BoundField DataField="Caja_Monto_total" HeaderText="Total"
                    DataFormatString="{0:C}" HtmlEncode="false" />
            </Columns>

        </asp:GridView>
    </div>

    <%-- SweetAlert2 para mensajes de confirmación --%>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
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
