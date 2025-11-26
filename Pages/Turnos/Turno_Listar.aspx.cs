using System;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Proyecto_Estacionamiento.Pages.Turnos
{
    public partial class Turno_Listar : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string tipoUsuario = Session["Usu_tipo"] as string;
                if (tipoUsuario != "Playero")
                {
                    btnInicioTurno.Visible = false;
                    btnFinTurno.Visible = false;
                    lblMontoInicio.Visible = false;
                    txtMontoInicio.Visible = false;

                    // Verificar si hay turno abierto y recuperar ID si se perdió la sesión
                    if (Session["Turno_Id_Actual"] == null)
                    {
                        int legajo = Convert.ToInt32(Session["Usu_legajo"]);
                        using (var db = new ProyectoEstacionamientoEntities())
                        {
                            var turnoAbierto = db.Turno
                             .Where(t => t.Playero_Legajo == legajo && t.Turno_FechaHora_fin == null)
                             .OrderByDescending(t => t.Turno_FechaHora_Inicio)
                             .FirstOrDefault();

                            if (turnoAbierto != null)
                            {
                                Session["Turno_Id_Actual"] = turnoAbierto.Turno_id;
                            }
                        }
                    }
                }else
                {
                    GridViewTurnos.Columns[0].Visible = false;
                }

                string estacionamiento = Session["Usu_estacionamiento"] as string;

                if (!string.IsNullOrEmpty(estacionamiento))
                {
                    Estacionamiento_Nombre.Text = $"Estacionamiento: '<strong>{estacionamiento}</strong>'";
                }
                else
                {
                    Estacionamiento_Nombre.Visible = false;
                }

                CargarLogicaEstacionamiento();

                // Fechas por defecto
                txtDesde.Text = DateTime.Now.ToString("dd/MM/yyyy");
                txtHasta.Text = DateTime.Now.ToString("dd/MM/yyyy");

                CargarTurnos();
            }
        }

        private void CargarLogicaEstacionamiento()
        {
            string tipoUsuario = Session["Usu_tipo"] as string;

            // --- CASO A: PLAYERO ---
            if (tipoUsuario == "Playero")
            {
                // El playero ya tiene su estacionamiento asignado en sesión (desde el Login)
                // No debe ver el selector.
                lblEstacionamiento.Visible = false;
                ddlEstacionamiento.Visible = false;

                // El nombre del estacionamiento ya se carga en el Page_Load principal
                // con Estacionamiento_Nombre.Text, así que no necesitamos hacer nada más aquí.
                return;
            }

            // --- CASO B: DUEÑO ---
            if (Session["Dueño_EstId"] != null)
            {
                // CASO B.1: Estacionamiento YA seleccionado (vino desde el menú principal)
                string nombreEst = Session["Usu_estacionamiento"] as string;
                Estacionamiento_Nombre.Text = $"Estacionamiento: '<strong>{nombreEst}</strong>'";
                Estacionamiento_Nombre.Visible = true;

                // Ocultar selección manual
                lblEstacionamiento.Visible = false;
                ddlEstacionamiento.Visible = false;
            }
            else
            {
                // CASO B.2: NO hay estacionamiento seleccionado -> Permitir elegir de la lista
                Estacionamiento_Nombre.Visible = false;

                lblEstacionamiento.Visible = true;
                ddlEstacionamiento.Visible = true;

                CargarComboEstacionamientos(); // Este método ya funciona bien para Dueños
            }
        }

        private void CargarComboEstacionamientos()
        {
            int legajo = Convert.ToInt32(Session["Usu_legajo"]);

            using (var db = new ProyectoEstacionamientoEntities())
            {
                var lista = db.Estacionamiento
                              .Where(e => e.Dueño_Legajo == legajo)
                              .Select(e => new { e.Est_id, e.Est_nombre })
                              .ToList();

                ddlEstacionamiento.DataSource = lista;
                ddlEstacionamiento.DataTextField = "Est_nombre";
                ddlEstacionamiento.DataValueField = "Est_id";
                ddlEstacionamiento.DataBind();

                // Agregar opción por defecto
                ddlEstacionamiento.Items.Insert(0, new ListItem("-- Seleccione --", "0"));
            }
        }

        // Evento al cambiar la selección (opcional, por si quieres actualizar algo en pantalla)
        protected void ddlEstacionamiento_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Si necesitas hacer algo cuando cambia, ej. limpiar el reporte
        }


        // VALIDACIÓN - Estacionamiento
        protected void CvEstacionamiento_ServerValidate(object source, ServerValidateEventArgs args)
        {
            // Solo validamos si el control está visible (es decir, si el dueño no tenía uno preseleccionado)
            if (ddlEstacionamiento.Visible)
            {
                if (ddlEstacionamiento.SelectedValue == "0")
                {
                    ((CustomValidator)source).ErrorMessage = "Debe seleccionar un Estacionamiento.";
                    args.IsValid = false;
                    return;
                }
            }
            args.IsValid = true;
        }

        // VALIDACIÓN - Fecha Desde
        protected void CvDesde_ServerValidate(object source, ServerValidateEventArgs args)
        {
            string valor = txtDesde.Text.Trim();

            if (string.IsNullOrWhiteSpace(valor))
            {
                ((CustomValidator)source).ErrorMessage = "La fecha 'Desde' es obligatoria.";
                args.IsValid = false;
                return;
            }

            if (!DateTime.TryParse(valor, out _))
            {
                ((CustomValidator)source).ErrorMessage = "Formato de fecha inválido.";
                args.IsValid = false;
                return;
            }

            args.IsValid = true;
        }

        // VALIDACIÓN - Desde > Hasta
        protected void CvHasta_ServerValidate(object source, ServerValidateEventArgs args)
        {
            var validator = (CustomValidator)source;
            string valorHasta = txtHasta.Text.Trim();
            string valorDesde = txtDesde.Text.Trim();

            // A. Validar vacío
            if (string.IsNullOrWhiteSpace(valorHasta))
            {
                validator.ErrorMessage = "La fecha 'Hasta' es obligatoria.";
                args.IsValid = false;
                return;
            }

            // B. Validar formato
            if (!DateTime.TryParse(valorHasta, out DateTime fechaHasta))
            {
                validator.ErrorMessage = "Formato de fecha inválido.";
                args.IsValid = false;
                return;
            }

            // C. Validar Rango (Desde > Hasta)
            // Solo comparamos si la fecha 'Desde' también es válida
            if (DateTime.TryParse(valorDesde, out DateTime fechaDesde))
            {
                if (fechaDesde > fechaHasta)
                {
                    validator.ErrorMessage = "La fecha 'Desde' no puede ser mayor que 'Hasta'.";
                    args.IsValid = false;
                    return;
                }
            }

            args.IsValid = true;
        }

        public class TurnoDTO
        {
            public int Turno_id { get; set; }
            public string Playero { get; set; }
            public string Inicio { get; set; }
            public string Fin { get; set; }
            public string MontoInicio { get; set; }
            public string MontoFin { get; set; }
            public string TotalRecaudado { get; set; }
            public string DetalleHtml { get; set; }
            public string DownloadUrl { get; set; }
        }

        protected void btnFiltrarTurno_Click(object sender, EventArgs e)
        {
            // Verificar que la página antes de consultar
            if (Page.IsValid)
            {
                CargarTurnos();
            }
        }

        private void CargarTurnos()
        {

            string tipoUsuario = Session["Usu_tipo"] as string;
            // Manejo seguro de nulos para el legajo
            int legajo = Session["Usu_legajo"] != null ? Convert.ToInt32(Session["Usu_legajo"]) : 0;

            using (var db = new ProyectoEstacionamientoEntities())
            {
                IQueryable<Turno> query = db.Turno;

                // ---------------------------------------------------------
                // 1. FILTRO DE SEGURIDAD / CONTEXTO (Base lógica existente)
                // ---------------------------------------------------------
                if (tipoUsuario == "Dueño")
                {
                    if (Session["Dueño_EstId"] != null)
                    {
                        // El dueño entró a gestionar un estacionamiento específico desde el Dashboard
                        int estIdSeleccionado = (int)Session["Dueño_EstId"];
                        query = query.Where(t => t.Playero.Est_id == estIdSeleccionado);
                    }
                    else
                    {
                        // El dueño está viendo el global. 
                        // AQUI aplicamos el filtro del DropDown si el usuario seleccionó algo

                        // Primero filtramos solo lo que le pertenece al dueño
                        var estIds = db.Estacionamiento
                                       .Where(e => e.Dueño_Legajo == legajo)
                                       .Select(e => e.Est_id);

                        query = query.Where(t => t.Playero.Est_id.HasValue && estIds.Contains(t.Playero.Est_id.Value));

                        // Lógica del DropDown (ddlEstacionamiento)
                        // Solo filtramos si el combo es visible, tiene items y no es la opción por defecto (ej. valor "-1" o vacío)
                        if (ddlEstacionamiento.Visible &&
                            !string.IsNullOrEmpty(ddlEstacionamiento.SelectedValue) &&
                            ddlEstacionamiento.SelectedValue != "-1") // Asumiendo que -1 es "Todos"
                        {
                            int idEstacionamientoFiltro = int.Parse(ddlEstacionamiento.SelectedValue);
                            query = query.Where(t => t.Playero.Est_id == idEstacionamientoFiltro);
                        }
                    }
                }
                else if (tipoUsuario == "Playero")
                {
                    // El playero solo ve sus turnos
                    query = query.Where(t => t.Playero_Legajo == legajo);
                }

                // ---------------------------------------------------------
                // 2. FILTROS DE FECHAS (Nuevos parámetros)
                // ---------------------------------------------------------

                DateTime fechaDesde;
                DateTime fechaHasta;

                // Intentamos parsear la fecha Desde (formato dd/MM/yyyy)
                if (!string.IsNullOrEmpty(txtDesde.Text) &&
                    DateTime.TryParseExact(txtDesde.Text, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out fechaDesde))
                {
                    // Filtramos turnos MAYORES o IGUALES a la fecha desde (a las 00:00:00)
                    query = query.Where(t => t.Turno_FechaHora_Inicio >= fechaDesde);
                }

                // Intentamos parsear la fecha Hasta
                if (!string.IsNullOrEmpty(txtHasta.Text) &&
                    DateTime.TryParseExact(txtHasta.Text, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out fechaHasta))
                {
                    // Importante: Al filtrar "Hasta", queremos incluir todo ese día.
                    // Si el usuario pone 25/11/2023, internamente es 25/11/2023 00:00:00.
                    // Debemos comparar contra el final del día o sumar un día.
                    DateTime finDelDia = fechaHasta.AddDays(1).AddTicks(-1); // 23:59:59.999

                    query = query.Where(t => t.Turno_FechaHora_Inicio <= finDelDia);
                }

                // ---------------------------------------------------------
                // 3. EJECUCIÓN Y PROYECCIÓN
                // ---------------------------------------------------------

                var turnosBase = query
                    .OrderByDescending(t => t.Turno_FechaHora_Inicio)
                    .ToList(); // Ejecuta SQL aquí

                var turnosDTO = turnosBase.Select(t => new TurnoDTO
                {
                    Turno_id = t.Turno_id,
                    Playero = t.Playero != null && t.Playero.Usuarios != null
                              ? $"{t.Playero.Usuarios.Usu_ap}, {t.Playero.Usuarios.Usu_nom}"
                              : "Desconocido", // Null check por seguridad
                    Inicio = t.Turno_FechaHora_Inicio.ToString("dd/MM/yyyy HH:mm"),
                    Fin = t.Turno_FechaHora_fin.HasValue ? t.Turno_FechaHora_fin.Value.ToString("dd/MM/yyyy HH:mm") : "En curso",
                    MontoInicio = t.Caja_Monto_Inicio.HasValue ? t.Caja_Monto_Inicio.Value.ToString("C") : "$ 0.00",
                    MontoFin = t.Caja_Monto_fin.HasValue ? t.Caja_Monto_fin.Value.ToString("C") : "-",
                    TotalRecaudado = t.Caja_Monto_total.HasValue ? t.Caja_Monto_total.Value.ToString("C") : "-",

                    DetalleHtml = ConstruirDetalleHtml(t.Turno_id),
                    DownloadUrl = $"Turno_Descargar.aspx?turnoId={t.Turno_id}"
                })
                .ToList();

                GridViewTurnos.DataSource = turnosDTO;
                GridViewTurnos.DataBind();

                // Opcional: Mostrar mensaje si no hay resultados
                if (turnosDTO.Count == 0)
                {
                    // lblMensaje.Text = "No se encontraron turnos con los filtros seleccionados.";
                    // lblMensaje.Visible = true;
                }
            }
        }

        private string ConstruirDetalleHtml(int turnoId)
        {
            using (var db = new ProyectoEstacionamientoEntities())
            {
                // 1. Obtener Pagos por Ocupación del turno
                var pagosOcupacion = db.Pago_Ocupacion
                    .Where(p => p.Turno_id == turnoId)
                    .Select(p => new
                    {
                        DatosOcupacion = db.Ocupacion.FirstOrDefault(o => o.Pago_id == p.Pago_id),
                        Metodo = p.Metodos_De_Pago.Metodo_pago_descripcion,
                        Monto = p.Pago_Importe
                    })
                    .ToList()
                    .Select(x => new
                    {
                        Ingreso = x.DatosOcupacion?.Ocu_fecha_Hora_Inicio.ToString("HH:mm") ?? "-",
                        Egreso = x.DatosOcupacion?.Ocu_fecha_Hora_Fin?.ToString("HH:mm") ?? "-",
                        Plaza = x.DatosOcupacion?.Plaza.Plaza_Nombre ?? "-",
                        FormaPago = x.Metodo,
                        MontoStr = x.Monto.ToString("C")
                    })
                    .ToList();

                // 2. Obtener Pagos de Abonos del turno
                var pagosAbonos = db.Pagos_Abonados
                    .Where(p => p.Turno_id == turnoId)
                    .Select(p => new
                    {
                        FechaPago = p.Fecha_Pago,
                        Plaza = p.Abono.Plaza.Plaza_Nombre,
                        Metodo = p.Acepta_Metodo_De_Pago.Metodos_De_Pago.Metodo_pago_descripcion,
                        Monto = p.PA_Monto
                    })
                    .ToList()
                    .Select(x => new
                    {
                        Fecha = x.FechaPago.ToString("dd/MM HH:mm"),
                        Plaza = x.Plaza,
                        FormaPago = x.Metodo,
                        MontoStr = x.Monto.ToString("C")
                    })
                    .ToList();

                // 3. Construir el String HTML
                var sb = new System.Text.StringBuilder();
                sb.Append("<div style='text-align:left; font-size: 0.9em;'>");

                // Tabla Ocupación
                sb.Append("<h5><b>Detalle de Pagos por</b>: Ocupación</h5>");
                if (pagosOcupacion.Any())
                {
                    sb.Append("<table class='table table-sm table-striped' style='width:100%; border:1px solid #ddd;'>");
                    sb.Append("<thead style='background-color:#f2f2f2;'><tr><th>Ingreso</th><th>Egreso</th><th>Plaza</th><th>Pago</th><th style='text-align:right;'>Monto</th></tr></thead>");
                    sb.Append("<tbody>");
                    foreach (var item in pagosOcupacion)
                    {
                        sb.Append($"<tr><td>{item.Ingreso}</td><td>{item.Egreso}</td><td>{item.Plaza}</td><td>{item.FormaPago}</td><td style='text-align:right;'>{item.MontoStr}</td></tr>");
                    }
                    sb.Append("</tbody></table>");
                }
                else
                {
                    sb.Append("<p><i>No hay pagos por Ocupación en este Turno.</i></p>");
                }
                sb.Append("<br/>");

                // Tabla Abonos
                sb.Append("<h5><b>Detalle de Pagos por</b>: Abono</h5>");
                if (pagosAbonos.Any())
                {
                    sb.Append("<table class='table table-sm table-striped' style='width:100%; border:1px solid #ddd;'>");
                    sb.Append("<thead style='background-color:#f2f2f2;'><tr><th>Fecha</th><th>Plaza</th><th>Pago</th><th style='text-align:right;'>Monto</th></tr></thead>");
                    sb.Append("<tbody>");
                    foreach (var item in pagosAbonos)
                    {
                        sb.Append($"<tr><td>{item.Fecha}</td><td>{item.Plaza}</td><td>{item.FormaPago}</td><td style='text-align:right;'>{item.MontoStr}</td></tr>");
                    }
                    sb.Append("</tbody></table>");
                }
                else
                {
                    sb.Append("<p><i>No hay pagos de Abonos en este Turno.</i></p>");
                }

                sb.Append("</div>");
                return sb.ToString();
            }
        }

        protected void btnInicioTurno_Click(object sender, EventArgs e)
        {
            try
            {
                if (!double.TryParse(txtMontoInicio.Text, out double montoInicio) || montoInicio < 0)
                {
                    ScriptManager.RegisterStartupScript(
                        this, this.GetType(), "alertaMontoInicio",
                        "mostrarAlerta('Atención', 'Ingrese un monto de inicio válido y no negativo.', 'warning');",
                        true
                    );
                    return;
                }

                using (var db = new ProyectoEstacionamientoEntities())
                {
                    if (Session["Usu_legajo"] == null)
                    {
                        Response.Redirect("~/Pages/Login/Login.aspx");
                        return;
                    }

                    int legajoPlayero = (int)Session["Usu_legajo"];

                    bool hayTurnoAbierto = db.Turno.Any(t =>
                        t.Playero_Legajo == legajoPlayero &&
                        t.Turno_FechaHora_fin == null
                    );

                    if (hayTurnoAbierto)
                    {
                        ScriptManager.RegisterStartupScript(
                            this, this.GetType(), "alertaTurnoAbierto",
                            "mostrarAlerta('Atención', 'Ya hay un turno abierto.', 'warning');",
                            true
                        );
                        return;
                    }

                    DateTime fechaInicio = DateTime.Now;
                    fechaInicio = new DateTime(fechaInicio.Year, fechaInicio.Month, fechaInicio.Day,
                                               fechaInicio.Hour, fechaInicio.Minute, fechaInicio.Second);

                    var nuevoTurno = new Turno
                    {
                        Playero_Legajo = legajoPlayero,
                        Turno_FechaHora_Inicio = fechaInicio,
                        Caja_Monto_Inicio = montoInicio
                    };

                    db.Turno.Add(nuevoTurno);
                    db.SaveChanges();

                    Session["Turno_Id_Actual"] = nuevoTurno.Turno_id;
                }

                txtMontoInicio.Text = "";
                string accion = "inicio";
                Response.Redirect($"Turno_Listar.aspx?exito=1&accion={accion}");
            }
            catch (Exception ex)
            {
                var safe = HttpUtility.JavaScriptStringEncode(ex.Message);
                ScriptManager.RegisterStartupScript(
                    this, this.GetType(), "alertaErrorInicio",
                    $"mostrarAlerta('Error', 'Error al iniciar turno: {safe}', 'error');",
                    true
                );
            }
        }

        protected void btnFinTurno_Click(object sender, EventArgs e)
        {
            try
            {
                // Validar sesión
                if (Session["Usu_legajo"] == null)
                {
                    Response.Redirect("~/Pages/Login/Login.aspx");
                    return;
                }

                int legajoPlayero = (int)Session["Usu_legajo"];

                using (var db = new ProyectoEstacionamientoEntities())
                {
                    // 1. Buscar el turno abierto
                    var turnoAbierto = db.Turno
                                         .Where(t => t.Playero_Legajo == legajoPlayero && t.Turno_FechaHora_fin == null)
                                         .OrderByDescending(t => t.Turno_FechaHora_Inicio)
                                         .FirstOrDefault();

                    if (turnoAbierto == null)
                    {
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "alertaSinTurno",
                            "mostrarAlerta('Atención', 'No hay Turno abierto para finalizar.', 'warning');", true);
                        return;
                    }

                    int turnoId = turnoAbierto.Turno_id;

                    // ====================================================================
                    // 2. CÁLCULO DE TOTALES (RECAUDACIÓN GLOBAL)
                    // ====================================================================

                    // Suma de TODAS las Ocupaciones (Efectivo, Tarjeta, etc.)
                    double totalOcupaciones = db.Pago_Ocupacion
                                                .Where(p => p.Turno_id == turnoId)
                                                .Sum(p => (double?)p.Pago_Importe) ?? 0;

                    // Suma de TODOS los Abonos (Efectivo, Tarjeta, etc.)
                    double totalAbonos = db.Pagos_Abonados
                                           .Where(p => p.Turno_id == turnoId)
                                           .Sum(p => (double?)p.PA_Monto) ?? 0;

                    // TOTAL RECAUDADO (Para estadísticas y reportes de ventas)
                    double totalRecaudadoGlobal = totalOcupaciones + totalAbonos;


                    // ====================================================================
                    // 3. CÁLCULO DE EFECTIVO (CAJA FÍSICA)
                    // ====================================================================

                    // Suma de Ocupaciones SOLO en EFECTIVO
                    double efectivoOcupaciones = db.Pago_Ocupacion
                                                   .Where(p => p.Turno_id == turnoId &&
                                                               p.Metodos_De_Pago.Metodo_pago_descripcion == "Efectivo")
                                                   .Sum(p => (double?)p.Pago_Importe) ?? 0;

                    // Suma de Abonos SOLO en EFECTIVO
                    double efectivoAbonos = db.Pagos_Abonados
                                              .Where(p => p.Turno_id == turnoId &&
                                                          p.Acepta_Metodo_De_Pago.Metodos_De_Pago.Metodo_pago_descripcion == "Efectivo")
                                              .Sum(p => (double?)p.PA_Monto) ?? 0;

                    double totalEfectivoIngresado = efectivoOcupaciones + efectivoAbonos;


                    // ====================================================================
                    // 4. ACTUALIZAR EL TURNO
                    // ====================================================================

                    DateTime fechaFin = DateTime.Now;
                    // Truncar milisegundos
                    fechaFin = new DateTime(fechaFin.Year, fechaFin.Month, fechaFin.Day, fechaFin.Hour, fechaFin.Minute, fechaFin.Second);

                    turnoAbierto.Turno_FechaHora_fin = fechaFin;

                    // A. Caja_Monto_total
                    turnoAbierto.Caja_Monto_total = totalRecaudadoGlobal;

                    // B. Caja_Monto_fin = Efectivo en caja 
                    turnoAbierto.Caja_Monto_fin = (turnoAbierto.Caja_Monto_Inicio ?? 0) + totalEfectivoIngresado;

                    db.SaveChanges();

                    // 5. Limpiar sesión del turno
                    Session["Turno_Id_Actual"] = null;
                }

                string accion = "fin";
                Response.Redirect($"Turno_Listar.aspx?exito=1&accion={accion}");
            }
            catch (Exception ex)
            {
                var safe = HttpUtility.JavaScriptStringEncode(ex.Message);
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alertaErrorFin",
                    $"mostrarAlerta('Error', 'Error al finalizar turno: {safe}', 'error');", true);
            }
        }
    }
}
