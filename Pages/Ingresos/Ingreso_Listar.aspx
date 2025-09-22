<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Ingreso_Listar.aspx.cs" Inherits="Proyecto_Estacionamiento.Ingreso_Listar" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="header-row">
        <h2>
            <asp:Literal ID="TituloIngresos" runat="server" />
        </h2>
    </div>

    <div class="ingreso-layout">
        <asp:Button ID="btnIngreso" runat="server" Text="Registrar Ingreso" OnClick="btnIngreso_Click" CssClass="btn btn-success" />

        <div class="dashboard">
            <div class="dashboard-card">
                <asp:Label ID="lblPlazasDisponibles" runat="server" CssClass="disponibles" />
            </div>

            <div class="dashboard-card">
                <asp:Label ID="lblPlazasOcupadas" runat="server" CssClass="ocupadas" />
            </div>
        </div>

    </div>

    <br />

    <asp:DropDownList ID="ddlMetodoDePago" runat="server" Visible="false" />
    <asp:HiddenField ID="hfMetodoPago" runat="server" />

    <div class="grid-wrapper">

        <asp:GridView ID="gvIngresos" runat="server" AutoGenerateColumns="False"
            OnRowCommand="gvIngresos_RowCommand"
            DataKeyNames="Est_id,Plaza_id,Ocu_fecha_Hora_Inicio"
            CssClass="grid" Width="100%">

            <Columns>
                <asp:BoundField DataField="Est_nombre" HeaderText="Estacionamiento" />
                <asp:BoundField DataField="Entrada" HeaderText="Entrada" />
                <asp:BoundField DataField="Vehiculo_Patente" HeaderText="Patente" />
                <asp:BoundField DataField="Plaza_Nombre" HeaderText="Plaza" />
                <asp:BoundField DataField="Tarifa" HeaderText="Tarifa" />
                <asp:BoundField DataField="Salida" HeaderText="Salida" />
                <asp:BoundField DataField="Monto" HeaderText="Monto" />

                <asp:TemplateField>
                    <ItemTemplate>
                        <asp:Button ID="btnEgreso" runat="server" Text="Egreso"
                            CommandName="Egreso"
                            CommandArgument='<%# Container.DataItemIndex %>'
                            Enabled='<%# String.IsNullOrEmpty(Eval("Salida") as string) 
               && (Session["Usu_tipo"] as string) == "Playero" %>'
                            CssClass="btn btn-danger btn-grid"
                            OnClientClick="return confirmarEgreso(this);" />
                    </ItemTemplate>
                </asp:TemplateField>

            </Columns>
        </asp:GridView>
    </div>

    <%-- SweetAlert2 para mensajes de exito --%>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <script>
        function confirmarEgreso(btn) {
            event.preventDefault();

            // Traemos los métodos de pago renderizados desde el servidor
            var opciones = '<option value="0">Seleccione Método de Pago</option>';
        <% foreach (var mp in ddlMetodoDePago.Items.Cast<ListItem>())
        { %>
            opciones += '<option value="<%= mp.Value %>"><%= mp.Text %></option>';
        <% } %>

            Swal.fire({
                title: "Registrar Egreso",
                html: `
                <label>Método de Pago:</label>
                <select id="swalMetodoPago" class="swal2-input">
                    ${opciones}
                </select>
            `,
                focusConfirm: false,
                showDenyButton: true,
                confirmButtonText: "Guardar",
                denyButtonText: "Cancelar",
                reverseButtons: true, // 👈 Esto invierte el orden de los botones
                preConfirm: () => {
                    const metodo = document.getElementById('swalMetodoPago').value;
                    if (metodo === "0") {
                        Swal.showValidationMessage("Debe seleccionar un método de pago");
                        return false;
                    }
                    return metodo;
                }
            }).then((result) => {
                if (result.isConfirmed) {
                    // Guardamos en el HiddenField antes de postback
                    document.getElementById('<%= hfMetodoPago.ClientID %>').value = result.value;
                    __doPostBack(btn.name, "");
                }
            });

            return false;
        }
    </script>

    <% 
        if (Request.QueryString["exito"] == "1")
        {
            string accion = Request.QueryString["accion"];
            string titulo = accion == "ingreso"
                ? "'Ingreso' registrado correctamente"
                : "'Egreso' registrado correctamente";
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
