﻿<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Ingreso_Listar.aspx.cs" Inherits="Proyecto_Estacionamiento.Ingreso_Listar" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="header-row">
        <h2>
            <asp:Literal ID="TituloIngresos" runat="server" />
        </h2>

        <div class="dashboard">
            <div class="dashboard-card">
                <asp:Label ID="lblPlazasDisponibles" runat="server" CssClass="disponibles" />
            </div>

            <div class="dashboard-card">
                <asp:Label ID="lblPlazasOcupadas" runat="server" CssClass="ocupadas" />
            </div>
        </div>
    </div>

    <div class="ingreso-layout">

        <asp:Button ID="btnIngreso" runat="server" Text="Registrar Ingreso" OnClick="btnIngreso_Click" CssClass="btn btn-success" />

        <div class="right-align-filters">

            <div class="filtro-grupo">
                <asp:Label ID="lblOrdenarPor" runat="server" Text="Ordenar por:" />

                <asp:DropDownList ID="ddlCamposOrden" runat="server" CssClass="form-control">
                    <asp:ListItem Text="Patente" Value="Vehiculo_Patente" />
                    <asp:ListItem Text="Entrada" Value="Ocu_fecha_Hora_Inicio" />
                    <asp:ListItem Text="Salida" Value="Ocu_fecha_Hora_Fin" />
                    <asp:ListItem Text="Monto" Value="Monto" />
                    <asp:ListItem Text="Plaza" Value="Plaza_Nombre" />
                    <asp:ListItem Text="Tarifa" Value="Tarifa" />
                </asp:DropDownList>

                <asp:DropDownList ID="ddlDireccionOrden" runat="server" CssClass="form-control">
                    <asp:ListItem Text="ASC" Value="ASC" />
                    <asp:ListItem Text="DESC" Value="DESC" />
                </asp:DropDownList>

                <asp:Button ID="btnOrdenar" runat="server" Text="Ordenar" CssClass="btn btn-primary"
                    OnClick="btnOrdenar_Click" />
            </div>

            <div class="filtro-grupo">
                <asp:Label ID="lblDesde" runat="server" Text="Desde:" />
                <asp:TextBox ID="txtDesde" runat="server" CssClass="form-control filtro-fecha" />
                <asp:Label ID="lblHasta" runat="server" Text="Hasta:" />
                <asp:TextBox ID="txtHasta" runat="server" CssClass="form-control filtro-fecha" />
                <asp:Button ID="btnFiltrarFechas" runat="server" Text="Buscar" OnClick="btnFiltrar_Click"
                    CssClass="btn btn-primary" />
            </div>

            <div class="filtro-grupo">
                <asp:Label ID="lblPatente" runat="server" Text="Patente:" />
                <asp:TextBox ID="txtPatente" runat="server" CssClass="form-control" placeholder="Buscar por patente" />
                <asp:Button ID="btnFiltrarPatente" runat="server" Text="Buscar" OnClick="btnFiltrarPatente_Click"
                    CssClass="btn btn-primary" />
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

                <asp:BoundField DataField="Vehiculo_Patente" HeaderText="Patente" SortExpression="Vehiculo_Patente" />

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
