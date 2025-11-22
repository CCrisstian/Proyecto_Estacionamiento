using System;
using System.Linq;
using System.Web;
using System.Web.UI;

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

                    if (Session["Dueño_EstId"] != null)
                    {
                        GridViewTurnos.Columns[0].Visible = false;
                    }

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
                }
                else
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

                CargarTurnos();
            }
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

        private void CargarTurnos()
        {
            string tipoUsuario = Session["Usu_tipo"] as string;
            int legajo = Convert.ToInt32(Session["Usu_legajo"]);

            using (var db = new ProyectoEstacionamientoEntities())
            {
                IQueryable<Turno> query = db.Turno;

                if (tipoUsuario == "Dueño")
                {
                    if (Session["Dueño_EstId"] != null)
                    {
                        // Filtrar solo por el estacionamiento seleccionado
                        int estIdSeleccionado = (int)Session["Dueño_EstId"];
                        query = query.Where(t => t.Playero.Est_id == estIdSeleccionado);
                    }
                    else
                    {
                        // Mostrar todos los turnos de los estacionamientos del Dueño
                        var estIds = db.Estacionamiento
                                       .Where(e => e.Dueño_Legajo == legajo)
                                       .Select(e => e.Est_id);

                        query = query.Where(t => t.Playero.Est_id.HasValue && estIds.Contains(t.Playero.Est_id.Value));
                    }
                }
                else if (tipoUsuario == "Playero")
                {
                    // Estacionamiento asignado al playero
                    var estId = db.Playero
                                  .Where(p => p.Playero_legajo == legajo)
                                  .Select(p => p.Est_id)
                                  .FirstOrDefault();

                    if (estId.HasValue)
                    {
                        query = query.Where(t => t.Playero.Est_id == estId.Value);
                    }
                    else
                    {
                        // No mostrar nada si no tiene estacionamiento asignado
                        query = query.Where(t => false);
                    }
                }

                // Ejecutamos la consulta principal de Turnos
                var turnosBase = query
                    .OrderByDescending(t => t.Turno_FechaHora_Inicio)
                    .ToList(); // Traemos los turnos a memoria

                // Proyectamos a DTO y construimos el HTML de detalle para cada uno
                var turnosDTO = turnosBase.Select(t => new TurnoDTO
                {
                    Turno_id = t.Turno_id,
                    Playero = $"{t.Playero.Usuarios.Usu_ap}, {t.Playero.Usuarios.Usu_nom}",
                    Inicio = t.Turno_FechaHora_Inicio.ToString("dd/MM/yyyy HH:mm"),
                    Fin = t.Turno_FechaHora_fin.HasValue ? t.Turno_FechaHora_fin.Value.ToString("dd/MM/yyyy HH:mm") : "",
                    MontoInicio = t.Caja_Monto_Inicio.HasValue ? t.Caja_Monto_Inicio.Value.ToString("C") : "$ 0.00",
                    MontoFin = t.Caja_Monto_fin.HasValue ? t.Caja_Monto_fin.Value.ToString("C") : "",
                    TotalRecaudado = t.Caja_Monto_total.HasValue ? t.Caja_Monto_total.Value.ToString("C") : "",

                    // --- CONSTRUCCIÓN DEL DETALLE HTML ---
                    DetalleHtml = ConstruirDetalleHtml(t.Turno_id),
                    DownloadUrl = $"Turno_Descargar.aspx?turnoId={t.Turno_id}"
                })
                .ToList();

                GridViewTurnos.DataSource = turnosDTO;
                GridViewTurnos.DataBind();
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

                    // 2. Calcular Totales (Sumando desde las tablas de pagos usando el Turno_id)
                    int turnoId = turnoAbierto.Turno_id;

                    // Suma de Ocupaciones (Manejo de nulos con ??)
                    double totalOcupaciones = db.Pago_Ocupacion
                                                .Where(p => p.Turno_id == turnoId)
                                                .Sum(p => (double?)p.Pago_Importe) ?? 0;

                    // Suma de Abonos
                    double totalAbonos = db.Pagos_Abonados
                                           .Where(p => p.Turno_id == turnoId)
                                           .Sum(p => (double?)p.PA_Monto) ?? 0;

                    double totalRecaudado = totalOcupaciones + totalAbonos;

                    // 3. Actualizar el Turno
                    DateTime fechaFin = DateTime.Now;
                    // Truncar milisegundos para evitar problemas de SQL
                    fechaFin = new DateTime(fechaFin.Year, fechaFin.Month, fechaFin.Day, fechaFin.Hour, fechaFin.Minute, fechaFin.Second);

                    turnoAbierto.Turno_FechaHora_fin = fechaFin;

                    // Guardamos lo que se recaudó en este turno
                    turnoAbierto.Caja_Monto_total = totalRecaudado;

                    // Calculamos cuánto dinero debería haber en la caja (Inicio + Recaudado)
                    turnoAbierto.Caja_Monto_fin = (turnoAbierto.Caja_Monto_Inicio ?? 0) + totalRecaudado;

                    db.SaveChanges();

                    // 4. Limpiar sesión del turno
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
