<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Ingreso_Listar.aspx.cs" Inherits="Proyecto_Estacionamiento.Ingreso_Listar" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="header-row">
        <h2>Ingresos</h2>
        <asp:Label ID="Estacionamiento_Nombre" runat="server" CssClass="right-text"></asp:Label>
    </div>

    <br />

    <div class="ingreso-layout">

        <asp:Button ID="btnIngreso" runat="server" Text="Registrar Ingreso" OnClick="btnIngreso_Click" CssClass="btn btn-success" />

        <%--  Ana --%>

        <div class="right-align-filters">

            <div class="filtro-grupo">
                <asp:Button ID="OrdenarPorFecha" runat="server" Text="Ordenar por fecha " OnClick="btnOrdenAsc_Click" CssClass="btn btn-primary" />
            </div>
            <div class="filtro-container">
                <div class="filtro-grupo">
                    <asp:Label ID="lblDesde" runat="server" Text="Desde:" />
                    <asp:TextBox ID="txtDesde" runat="server" CssClass="form-control filtro-fecha" />
                    <ajaxToolkit:CalendarExtender ID="calDesde" runat="server" TargetControlID="txtDesde" Format="dd/MM/yyyy" />

                    <asp:Label ID="lblHasta" runat="server" Text="Hasta:" />
                    <asp:TextBox ID="txtHasta" runat="server" CssClass="form-control filtro-fecha" />
                    <ajaxToolkit:CalendarExtender ID="calHasta" runat="server" TargetControlID="txtHasta" Format="dd/MM/yyyy" />

                    <asp:Label ID="lblPatente" runat="server" Text="Patente:" />

                    <asp:TextBox ID="txtPatente"
                        runat="server"
                        CssClass="form-control"
                        placeholder="Buscar por patente"
                        OnTextChanged="txtPatente_TextChanged"
                        onkeyup="if(this.value===''){ __doPostBack(this.name,''); }"
                        AutoPostBack="true" />
                    <asp:Button ID="btnFiltrarPatente"
                        runat="server"
                        Text="Buscar"
                        OnClick="btnFiltrarPatente_Click"
                        CssClass="btn btn-primary" />

                </div>
            </div>
        </div>
    </div>

    <br />

    <asp:DropDownList ID="ddlMetodoDePago" runat="server" Visible="false" />
    <asp:HiddenField ID="hfMetodoPago" runat="server" />
    <asp:HiddenField ID="Salida" runat="server" />

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
                <asp:BoundField DataField="Tarifa_Monto" HeaderText="Monto" DataFormatString="{0:C}"
                    NullDisplayText=""
                    HtmlEncode="False" />
                <asp:BoundField DataField="Salida" HeaderText="Salida" />
                <asp:BoundField DataField="Monto" HeaderText="Total" DataFormatString="{0:C}"
                    NullDisplayText=""
                    HtmlEncode="False" />

                <asp:TemplateField>
                    <ItemTemplate>
                        <asp:Button ID="btnEgreso" runat="server" Text="Egreso"
                            CommandName="Egreso"
                            CommandArgument='<%# Container.DataItemIndex %>'
                            Enabled='<%# String.IsNullOrEmpty(Eval("Salida") as string) 
                       && (Session["Usu_tipo"] as string) == "Playero" %>'
                            CssClass="btn btn-danger btn-grid"
                            OnClientClick="return confirmarEgreso(this);"
                            data-tarifa='<%# Eval("Tarifa") %>'
                            data-tarifa-monto='<%# Eval("Tarifa_Monto") %>'
                            data-entrada='<%# Eval("Entrada") %>'
                            data-vto-abono='<%# Eval("Fecha_Vto_Abono") != null ? Eval("Fecha_Vto_Abono", "{0:o}") : "" %>'
                            data-tarifa-fallback='<%# Eval("Tarifa_Por_Hora_Fallback") %>' />
                    </ItemTemplate>
                </asp:TemplateField>

            </Columns>
        </asp:GridView>
    </div>

    <%-- SweetAlert2 para mensajes de exito --%>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <script>
        function calcularMonto(tarifa, duracionHoras, tarifaBase) {
            switch (tarifa) {
                case "Por hora":
                    let horasCobrar = Math.floor(duracionHoras);
                    let minutosRestantes = (duracionHoras - horasCobrar) * 60;
                    if (minutosRestantes >= 15) horasCobrar++;
                    if (horasCobrar < 1) horasCobrar = 1;
                    return horasCobrar * tarifaBase;

                case "Por día":
                    let diasCobrar = Math.ceil(duracionHoras / 24);
                    if (diasCobrar < 1) diasCobrar = 1;
                    return diasCobrar * tarifaBase;

                case "Quincenal":
                    let quincenas = Math.ceil(duracionHoras / (24 * 15));
                    if (quincenas < 1) quincenas = 1;
                    return quincenas * tarifaBase;

                default:
                    throw `Tipo de tarifa desconocido: ${tarifa}`;
            }
        }

        function confirmarEgreso(btn) {
            event.preventDefault();

            // Tomamos los valores desde los atributos data-*
            var tarifa = btn.dataset.tarifa;
            var tarifaMonto = btn.dataset.tarifaMonto;
            var entrada = btn.dataset.entrada;

            var vtoAbonoStr = btn.dataset.vtoAbono;
            var tarifaFallback = parseFloat(btn.dataset.tarifaFallback.replace(',', '.').trim());

            // Capturamos la fecha/hora actual
            var ahora = new Date();
            var salidaISO = ahora.toISOString();

            // Pasamos el valor al HiddenField
            document.getElementById("<%= Salida.ClientID %>").value = salidaISO;

            // Función para formatear en dd/MM/yyyy HH:mm
            function formatearFecha(fecha) {
                let dia = String(fecha.getDate()).padStart(2, '0');
                let mes = String(fecha.getMonth() + 1).padStart(2, '0');
                let anio = fecha.getFullYear();
                let horas = String(fecha.getHours()).padStart(2, '0');
                let minutos = String(fecha.getMinutes()).padStart(2, '0');
                return `${dia}/${mes}/${anio} ${horas}:${minutos}`;
            }

            function parseFecha(fechaStr) {
                // fechaStr = "dd/MM/yyyy HH:mm"
                const partes = fechaStr.split(/[/ :]/); // separa por /, espacio y :
                const dia = parseInt(partes[0], 10);
                const mes = parseInt(partes[1], 10) - 1; // JS meses 0-11
                const anio = parseInt(partes[2], 10);
                const horas = parseInt(partes[3], 10);
                const minutos = parseInt(partes[4], 10);
                return new Date(anio, mes, dia, horas, minutos);
            }

            var salida = formatearFecha(ahora);
            var entradaDate = parseFecha(entrada);

            var total = 0;
            var totalFmt = "0.00";
            var esIngresoNormal = true; // Flag para saber si pedir método de pago
            var htmlMensajeCalculo = "";

            if (tarifa === "Abonado") {
                // Es un Abono, verificar si venció
                var vtoAbonoDate = new Date(vtoAbonoStr); // JS puede parsear ISO 8601

                if (ahora <= vtoAbonoDate) {
                    // --- CASO 1: ABONO VIGENTE ---
                    esIngresoNormal = false; // Es egreso de abonado, no hay pago

                    var fila = btn.closest('tr');
                    var patente = fila.cells[2].innerText;
                    var plaza = fila.cells[3].innerText;

                    Swal.fire({
                        title: "Registrar Egreso de Abonado",
                        html: `
                    <div style="text-align:left; font-family:monospace;">
                        <hr/>
                        <p><b>Ingreso:</b> ${entrada}</p>
                        <p><b>Salida:</b> ${salida}</p>
                    </div>`,
                        icon: "info",
                        showDenyButton: true,
                        confirmButtonText: "Sí, continuar",
                        denyButtonText: "Cancelar",
                        reverseButtons: true
                    }).then((result) => {
                        if (result.isConfirmed) {
                            document.getElementById('<%= hfMetodoPago.ClientID %>').value = "ABONO";
                            __doPostBack(btn.name, "");
                        } else if (result.isDenied) {
                            Swal.fire("Egreso cancelado", "", "info");
                        }
                    });
                    return false; // Salir de la función aquí

                } else {
                    // --- CASO 2: ABONO VENCIDO ---
                    var duracionExcedidaMs = ahora - vtoAbonoDate;
                    var duracionExcedidaMinutos = duracionExcedidaMs / (1000 * 60);

                    if (duracionExcedidaMinutos < 15) {
                        // --- SUBCASO 2a: ABONO VENCIDO < 15 MIN ---

                        var fila = btn.closest('tr');
                        var patente = fila.cells[2].innerText;
                        var plaza = fila.cells[3].innerText;

                        Swal.fire({
                            title: "Registrar Egreso de Abonado",
                            html: `
                        <div style="text-align:left; font-family:monospace;">
                            <hr/>
                            <p style="color:orange; font-weight:bold;">Abono vencido recientemente, menos de 15 min.</p>
                            <hr/>
                            <p><b>Ingreso:</b> ${entrada}</p>
                            <p><b>Salida:</b> ${salida}</p>
                        </div>`,
                            icon: "info",
                            showDenyButton: true,
                            confirmButtonText: "Sí, continuar",
                            denyButtonText: "Cancelar",
                            reverseButtons: true
                        }).then((result) => {
                            if (result.isConfirmed) {
                                // Usamos "ABONO" para que el C# sepa que no debe generar pago
                                document.getElementById('<%= hfMetodoPago.ClientID %>').value = "ABONO";
                            __doPostBack(btn.name, "");
                        } else if (result.isDenied) {
                            Swal.fire("Egreso cancelado", "", "info");
                        }
                    });
                        return false; // Salir de la función aquí

                    } else {
                        // --- SUBCASO 2b: ABONO VENCIDO > 15 MIN ---
                        // Se cobra "Por hora" desde el vencimiento
                        var duracionExcedidaHoras = duracionExcedidaMs / (1000 * 60 * 60);
                        total = calcularMonto("Por hora", duracionExcedidaHoras, tarifaFallback);
                        totalFmt = total.toFixed(2);

                        htmlMensajeCalculo = `
                        <div style="text-align:left; font-family:monospace;">
                            <h2 style="text-align:center;">Registrar Egreso</h2>
                            <hr/>
                            <p style="color:red; font-weight:bold;">Su Abono ha Vencido: ${formatearFecha(vtoAbonoDate)}</p>
                            <p><b>Se le cobrará una Tarifa por hora:</b> $${tarifaFallback.toFixed(2)}</p>
                            <hr/>
                            <p><b>Ingreso:</b> ${entrada}</p>
                            <p><b>Salida:</b> ${salida}</p>
                            <hr/>
                            <h3><b>TOTAL A PAGAR:</b> $${totalFmt}</h3>
                        </div>`;
                    }
                }

            } else {
                // --- CASO 3: INGRESO NORMAL ---
                var tarifaBase = parseFloat(tarifaMonto.replace(',', '.').trim());
                var duracionHoras = (ahora - entradaDate) / (1000 * 60 * 60);

                total = calcularMonto(tarifa, duracionHoras, tarifaBase);
                totalFmt = total.toFixed(2);

                htmlMensajeCalculo = `
                <div style="text-align:left; font-family:monospace;">
                    <h2 style="text-align:center;">Registrar Egreso</h2>
                    <hr/>
                    <p><b>Tarifa:</b> ${tarifa}</p>
                    <p><b>Monto:</b> ${tarifaBase.toFixed(2)}</p>
                    <p><b>Ingreso:</b> ${entrada}</p>
                    <p><b>Salida:</b> ${salida}</p>
                    <hr/>
                    <h3><b>TOTAL A PAGAR:</b> $${totalFmt}</h3>
                </div>`;
            }

            // Primer Swal (Mostrar Total)
            Swal.fire({
                html: htmlMensajeCalculo,
                icon: "warning",
                showDenyButton: true,
                confirmButtonText: "Sí, continuar",
                denyButtonText: "Cancelar",
                reverseButtons: true
            }).then((firstResult) => {
                if (firstResult.isConfirmed) {
                    // Segundo Swal (Método de Pago)
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
                    </select>`,
                        focusConfirm: false,
                        showDenyButton: true,
                        confirmButtonText: "Guardar",
                        denyButtonText: "Cancelar",
                        reverseButtons: true,
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
                            document.getElementById('<%= hfMetodoPago.ClientID %>').value = result.value;
                            __doPostBack(btn.name, "");
                        }
                    });
                }
            });
            return false; // Prevenir postback original
        }
    </script>

    <% if (Request.QueryString["exito"] == "1")
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

        // 🔹 Limpiamos los parámetros de la URL sin recargar
        if (window.history.replaceState) {
            const url = new URL(window.location);
            url.search = ""; // eliminamos query string
            window.history.replaceState(null, null, url.toString());
        }
    </script>
    <% } %>
</asp:Content>
