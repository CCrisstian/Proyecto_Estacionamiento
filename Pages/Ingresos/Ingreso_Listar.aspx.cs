using AjaxControlToolkit;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Globalization;
using System.Linq;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Proyecto_Estacionamiento
{
    public partial class Ingreso_Listar : System.Web.UI.Page
    {

        protected void Page_Load(object sender, EventArgs e)
        {
            // Proteger acceso a páginas
            if (!User.Identity.IsAuthenticated) { Response.Redirect("~Pages/Login/Login.aspx"); }

            if (!IsPostBack)    // Verifica si es la primera vez que se carga la página
            {

                string tipoUsuario = Session["Usu_tipo"] as string;
                int legajo = Convert.ToInt32(Session["Usu_legajo"]);

                if (tipoUsuario != "Playero")
                {
                    btnIngreso.Visible = false;

                    if (Session["Dueño_EstId"] != null)
                    {
                        gvIngresos.Columns[0].Visible = false;
                    }

                    gvIngresos.Columns[8].Visible = false;
                }
                else
                {
                    if (Session["Turno_Id_Actual"] == null)
                    {
                        btnIngreso.Enabled = false;
                        btnIngreso.CssClass = "btn btn-secondary"; // Cambiar color a gris visualmente
                        btnIngreso.ToolTip = "Debe iniciar un Turno para registrar Ingresos.";
                    }
                    else
                    {
                        btnIngreso.Enabled = true;
                        btnIngreso.CssClass = "btn btn-success";
                    }

                    CargarMetodosDePagoEnDropDown(); // llena ddlMetodoDePago
                    gvIngresos.Columns[0].Visible = false;
                    gvIngresos.Columns[4].Visible = false;
                    gvIngresos.Columns[5].Visible = false;
                    gvIngresos.Columns[6].Visible = false;
                    gvIngresos.Columns[7].Visible = false;
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

                CargarIngresos();
            }
        }

        protected void btnOrdenAsc_Click(object sender, EventArgs e)
        {
            // 1. Recupera la lista de ingresos desde la sesión.
            if (Session["DatosIngresos"] != null)
            {
                var listaIngresos = (List<Ocupacion_DTO>)Session["DatosIngresos"];

                // 2. Ordena la lista por la propiedad de fecha real (`Ocu_fecha_Hora_Inicio`).
                // ¡Esto es lo que garantiza el orden correcto!
                var listaOrdenada = listaIngresos.OrderBy(ingreso => ingreso.Ocu_fecha_Hora_Inicio).ToList();
                // 3. Reasigna la lista ordenada al GridView y haz el enlace.
                gvIngresos.DataSource = listaOrdenada;
                gvIngresos.DataBind();
            }
        }

        protected void btnFiltrarPatente_Click(object sender, EventArgs e)
        {
            // Obtener y limpiar la patente (YA NO ES OBLIGATORIA)
            string patente = txtPatente.Text.Trim();

            // -------------------------------------------------------------------------
            // 1. Manejo de Fechas
            // -------------------------------------------------------------------------

            DateTime? fechaDesde = null;
            DateTime? fechaHasta = null;
            DateTime tempDesde, tempHasta;

            bool desdeValido = DateTime.TryParseExact(txtDesde.Text, "dd/MM/yyyy",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out tempDesde);
            bool hastaValido = DateTime.TryParseExact(txtHasta.Text, "dd/MM/yyyy",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out tempHasta);

            // Asignación y validación del rango de fechas
            if (desdeValido && hastaValido)
            {
                if (tempDesde > tempHasta)
                {
                    string script = "Swal.fire({icon: 'error', title: 'Error de fecha', text: 'La fecha \"Hasta\" no puede ser anterior a la fecha \"Desde\".'});";
                    ScriptManager.RegisterStartupScript(this, GetType(), "alert", script, true);
                    return;
                }
                fechaDesde = tempDesde;
                // Asigna el final del día a la fecha hasta
                fechaHasta = tempHasta.AddHours(23).AddMinutes(59).AddSeconds(59);
            }
            else if (desdeValido)
            {
                fechaDesde = tempDesde;
            }
            else if (hastaValido)
            {
                // Asigna el final del día a la fecha hasta
                fechaHasta = tempHasta.AddHours(23).AddMinutes(59).AddSeconds(59);
            }

            // -------------------------------------------------------------------------
            // 2. VALIDACIÓN: Mostrar error si NO hay ningún criterio de búsqueda.
            // -------------------------------------------------------------------------
            if (string.IsNullOrEmpty(patente) && !desdeValido && !hastaValido)
            {
                string script = "Swal.fire({icon: 'error', title: 'Filtro Requerido', text: 'Debe ingresar una patente o al menos una fecha para realizar la búsqueda.'});";
                ScriptManager.RegisterStartupScript(this, GetType(), "alert", script, true);
                return;
            }

            // -------------------------------------------------------------------------
            // 3. Llamar al método CargarIngresos, pasando fechas y patente
            // -------------------------------------------------------------------------
            CargarIngresos(fechaDesde, fechaHasta, patente);
        }

        protected void txtPatente_TextChanged(object sender, EventArgs e)
        {
            string patente = txtPatente.Text.Trim();

            if (string.IsNullOrEmpty(patente))
            {
                // Si el usuario borró la patente, se recargan todos los ingresos
                CargarIngresos();
            }
        }

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            // Verificar si ambos campos de texto están vacíos
            if (string.IsNullOrWhiteSpace(txtDesde.Text) && string.IsNullOrWhiteSpace(txtHasta.Text))
            {
                string script = "Swal.fire({icon: 'error', title: 'Error de Búsqueda', text: 'Se debe elegir al menos una fecha .'});";
                ScriptManager.RegisterStartupScript(this, GetType(), "alert", script, true);
                return; // Detiene la ejecución del método
            }

            DateTime? fechaDesde = null; // Cambiado a tipo anulable
            DateTime? fechaHasta = null; // Cambiado a tipo anulable

            DateTime tempDesde, tempHasta;

            bool desdeValido = DateTime.TryParseExact(txtDesde.Text, "dd/MM/yyyy",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out tempDesde);
            bool hastaValido = DateTime.TryParseExact(txtHasta.Text, "dd/MM/yyyy",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out tempHasta);


            if (desdeValido && hastaValido)
            {
                if (tempDesde > tempHasta)
                {
                    string script = "Swal.fire({icon: 'error', title: 'Error de fecha', text: 'La fecha \"Hasta\" no puede ser anterior a la fecha \"Desde\".'});";
                    ScriptManager.RegisterStartupScript(this, GetType(), "alert", script, true);
                    return;
                }

                fechaDesde = tempDesde;
                fechaHasta = tempHasta.AddHours(23).AddMinutes(59).AddSeconds(59);
            }
            // Si solo la fecha "Desde" es válida, solo la fecha "Desde" se utiliza
            else if (desdeValido)
            {
                fechaDesde = tempDesde;
            }
            // Si solo la fecha "Hasta" es válida
            else if (hastaValido)
            {
                fechaHasta = tempHasta.AddHours(23).AddMinutes(59).AddSeconds(59);
            }

            // **Llamada a la función de carga para aplicar el filtro**
            // Se pasa 'null' para la patente ya que este botón solo filtra por fecha
            CargarIngresos(fechaDesde, fechaHasta, null);
        }


        public class Ocupacion_DTO
        {
            public int Est_id { get; set; }
            public int Plaza_id { get; set; }
            public DateTime Ocu_fecha_Hora_Inicio { get; set; }
            public DateTime? Ocu_fecha_Hora_Fin { get; set; }
            // Más campos para mostrar y uso
            public string Est_nombre { get; set; }
            public string Plaza_Nombre { get; set; }
            public string Vehiculo_Patente { get; set; }
            public string Tarifa { get; set; }
            public double? Tarifa_Monto { get; set; }
            public string Entrada { get; set; }
            public string Salida { get; set; }
            public double? Monto { get; set; }

            // Campos para manejar el vencimiento de abonos durante el egreso
            public DateTime? Fecha_Vto_Abono { get; set; }
            public double Tarifa_Por_Hora_Fallback { get; set; }
        }

        private void CargarIngresos()
        {
            string tipoUsuario = Session["Usu_tipo"] as string;
            int legajo = Convert.ToInt32(Session["Usu_legajo"]);

            var idsAbonos = new List<int> { 3, 4, 5 };
            int idTarifaPorHora = 1;

            using (var db = new ProyectoEstacionamientoEntities())
            {
                // 1. Determinar los Est_id relevantes para este usuario
                IQueryable<int> estIdsQuery;
                if (tipoUsuario == "Dueño")
                {
                    if (Session["Dueño_EstId"] != null)
                    {
                        int estIdSeleccionado = (int)Session["Dueño_EstId"];
                        estIdsQuery = db.Estacionamiento.Where(e => e.Est_id == estIdSeleccionado).Select(e => e.Est_id);
                    }
                    else
                    {
                        estIdsQuery = db.Estacionamiento.Where(e => e.Dueño_Legajo == legajo).Select(e => e.Est_id);
                    }
                }
                else // Asumir Playero
                {
                    int estIdPlayero = (int)Session["Playero_EstId"];
                    estIdsQuery = db.Estacionamiento.Where(e => e.Est_id == estIdPlayero).Select(e => e.Est_id);
                }

                // 2. Pre-cargar tarifas por hora
                var tarifasPorHora = db.Tarifa
                            .Where(t => t.Tipos_Tarifa_Id == idTarifaPorHora && t.Categoria_id != null && t.Est_id.HasValue && estIdsQuery.Contains(t.Est_id.Value))
                            .ToDictionary(
                                t => Tuple.Create(t.Est_id.Value, t.Categoria_id.Value),
                                t => (double)t.Tarifa_Monto
                            );

                IQueryable<Ocupacion> query = db.Ocupacion
                    .Include("Vehiculo.Vehiculo_Abonado.Abono")
                    .Include("Vehiculo.Vehiculo_Abonado.Abono.Pagos_Abonados")
                    .Include("Vehiculo.Categoria_Vehiculo")
                    .Include("Vehiculo")
                    .Include("Plaza")
                    .Include("Plaza.Estacionamiento")
                    .Include("Tarifa")
                    .Include("Tarifa.Tipos_Tarifa")
                    .Include("Pago_Ocupacion");

                // 3. Aplicar filtro principal de Estacionamiento a la consulta
                query = query.Where(o => estIdsQuery.Contains(o.Est_id));

                if (tipoUsuario == "Playero")
                {
                    query = query.Where(o => !o.Ocu_fecha_Hora_Fin.HasValue);
                }

                var ocupaciones = query.ToList();

                var ingresos = ocupaciones.Select(o =>
                {

                    bool esTipoAbono = o.Tarifa.Tipos_Tarifa_Id.HasValue && idsAbonos.Contains(o.Tarifa.Tipos_Tarifa_Id.Value);
                    DateTime? fechaVtoAbono = null;
                    double tarifaFallback = 0.0;

                    string tarifaDisplay = o.Tarifa.Tipos_Tarifa != null ? o.Tarifa.Tipos_Tarifa.Tipos_tarifa_descripcion : "N/A";
                    double? tarifaMontoDisplay = (double)o.Tarifa.Tarifa_Monto;
                    double? montoDisplay = (o.Pago_Ocupacion != null ? (double?)o.Pago_Ocupacion.Pago_Importe : null);

                    if (esTipoAbono)
                    {
                        // Buscamos el abono a través del vehículo
                        var vehAbonoRelacion = o.Vehiculo.Vehiculo_Abonado.FirstOrDefault(); // Asumimos un abono activo por vehículo

                        if (vehAbonoRelacion != null)
                        {
                            // Verificamos la tarifa mirando el último pago del abono
                            var ultimoPago = vehAbonoRelacion.Abono.Pagos_Abonados
                                               .OrderByDescending(p => p.Fecha_Pago)
                                               .FirstOrDefault();

                            // Si la tarifa del último pago coincide con la tarifa de la ocupación
                            if (ultimoPago != null && ultimoPago.Tarifa_id == o.Tarifa_id)
                            {
                                fechaVtoAbono = vehAbonoRelacion.Abono.Fecha_Vto;
                            }
                        }

                        var fallbackKey = Tuple.Create(o.Est_id, o.Vehiculo.Categoria_id);
                        if (tarifasPorHora.ContainsKey(fallbackKey))
                            tarifaFallback = (double)tarifasPorHora[fallbackKey];
                    

                    if (o.Pago_Ocupacion != null)
                        {
                            // CASO: Abono Vencido que generó un Pago
                            tarifaDisplay = "Abonado (Vencido) - Por hora";
                            tarifaMontoDisplay = tarifaFallback;
                            montoDisplay = (double?)o.Pago_Ocupacion.Pago_Importe;
                        }
                        else
                        {
                            // CASO: Abono Vigente (ingreso o egreso sin pago)
                            tarifaDisplay = "Abonado";
                            tarifaMontoDisplay = null;
                            montoDisplay = (double?)null;
                        }
                    }

                    return new Ocupacion_DTO
                    {
                        Est_id = o.Est_id,
                        Plaza_id = o.Plaza_id,
                        Ocu_fecha_Hora_Inicio = o.Ocu_fecha_Hora_Inicio,
                        Ocu_fecha_Hora_Fin = o.Ocu_fecha_Hora_Fin,
                        Est_nombre = o.Plaza.Estacionamiento.Est_nombre,
                        Plaza_Nombre = o.Plaza.Plaza_Nombre,
                        Vehiculo_Patente = o.Vehiculo.Vehiculo_Patente,
                        Entrada = o.Ocu_fecha_Hora_Inicio.ToString("dd/MM/yyyy HH:mm"),
                        Salida = o.Ocu_fecha_Hora_Fin.HasValue ? o.Ocu_fecha_Hora_Fin.Value.ToString("dd/MM/yyyy HH:mm") : "",

                        Tarifa = tarifaDisplay,
                        Tarifa_Monto = tarifaMontoDisplay,
                        Monto = montoDisplay,

                        Fecha_Vto_Abono = fechaVtoAbono,
                        Tarifa_Por_Hora_Fallback = tarifaFallback
                    };
                })
        .OrderByDescending(o => o.Ocu_fecha_Hora_Inicio)
        .ToList();

                Session["DatosIngresos"] = ingresos;
                gvIngresos.DataSource = ingresos;
                gvIngresos.DataKeyNames = new string[] { "Est_id", "Plaza_id", "Ocu_fecha_Hora_Inicio" };
                gvIngresos.DataBind();
            }
        }


        private void CargarIngresos(DateTime? desde = null, DateTime? hasta = null, string patente = null)
        {
            string tipoUsuario = Session["Usu_tipo"] as string;
            int legajo = Convert.ToInt32(Session["Usu_legajo"]);

            // Lista de IDs que se consideran "Abono"
            var idsAbonos = new List<int> { 3, 4, 5 };

            int idTarifaPorHora = 1;


            using (var db = new ProyectoEstacionamientoEntities())
            {
                // 1. Determinar los Est_id relevantes para este usuario
                IQueryable<int> estIdsQuery;
                if (tipoUsuario == "Dueño")
                {
                    if (Session["Dueño_EstId"] != null)
                    {
                        int estIdSeleccionado = (int)Session["Dueño_EstId"];
                        estIdsQuery = db.Estacionamiento.Where(e => e.Est_id == estIdSeleccionado).Select(e => e.Est_id);
                    }
                    else
                    {
                        estIdsQuery = db.Estacionamiento.Where(e => e.Dueño_Legajo == legajo).Select(e => e.Est_id);
                    }
                }
                else // Asumir Playero
                {
                    int estIdPlayero = (int)Session["Playero_EstId"];
                    estIdsQuery = db.Estacionamiento.Where(e => e.Est_id == estIdPlayero).Select(e => e.Est_id);
                }

                // 2. Pre-cargar tarifas por hora
                var tarifasPorHora = db.Tarifa
                            .Where(t => t.Tipos_Tarifa_Id == idTarifaPorHora && t.Categoria_id != null && t.Est_id.HasValue && estIdsQuery.Contains(t.Est_id.Value))
                            .ToDictionary(
                                t => Tuple.Create(t.Est_id.Value, t.Categoria_id.Value),
                                t => (double)t.Tarifa_Monto
                            );

                IQueryable<Ocupacion> query = db.Ocupacion
                    .Include("Vehiculo.Vehiculo_Abonado.Abono")
                    .Include("Vehiculo.Vehiculo_Abonado.Abono.Pagos_Abonados")
                    .Include("Vehiculo.Categoria_Vehiculo")
                    .Include("Vehiculo")
                    .Include("Plaza")
                    .Include("Plaza.Estacionamiento") // Necesario para Est_nombre
                    .Include("Tarifa")
                    .Include("Tarifa.Tipos_Tarifa")   // Necesario para Tipos_tarifa_descripcion
                    .Include("Pago_Ocupacion");      // Necesario para Pago_Importe    

                // 3. Aplicar filtro principal de Estacionamiento a la consulta
                query = query.Where(o => estIdsQuery.Contains(o.Est_id));

                // Filtros
                if (desde.HasValue)
                    query = query.Where(o => o.Ocu_fecha_Hora_Inicio >= desde.Value);

                if (hasta.HasValue)
                    query = query.Where(o => o.Ocu_fecha_Hora_Inicio <= hasta.Value);

                if (!string.IsNullOrWhiteSpace(patente))
                    query = query.Where(o => o.Vehiculo.Vehiculo_Patente.Contains(patente));

                // ---- Según tipo de usuario ----
                if (tipoUsuario == "Dueño")
                {
                    if (Session["Dueño_EstId"] != null)
                    {
                        // Caso 1: Dueño eligió un estacionamiento
                        int estId = (int)Session["Dueño_EstId"];
                        query = query.Where(o => o.Est_id == estId);
                    }
                    else
                    {
                        // Caso 2: No eligió → mostrar todos sus estacionamientos
                        var estIds = db.Estacionamiento
                                       .Where(e => e.Dueño_Legajo == legajo)
                                       .Select(e => e.Est_id);
                        query = query.Where(o => estIds.Contains(o.Est_id));
                    }
                }
                else if (tipoUsuario == "Playero")
                {
                    int estId = (int)Session["Playero_EstId"];
                    query = query.Where(o => o.Est_id == estId);

                    // Solo ingresos activos (sin salida)
                    query = query.Where(o => !o.Ocu_fecha_Hora_Fin.HasValue);
                }

                // Ejecutar consulta
                var ocupaciones = query.ToList();

                var ingresos = ocupaciones.Select(o =>
                {

                    bool esTipoAbono = o.Tarifa.Tipos_Tarifa_Id.HasValue && idsAbonos.Contains(o.Tarifa.Tipos_Tarifa_Id.Value);
                    DateTime? fechaVtoAbono = null;
                    double tarifaFallback = 0.0;

                    string tarifaDisplay = o.Tarifa.Tipos_Tarifa != null ? o.Tarifa.Tipos_Tarifa.Tipos_tarifa_descripcion : "N/A";
                    double tarifaMontoDisplay = (double)o.Tarifa.Tarifa_Monto;
                    double? montoDisplay = (o.Pago_Ocupacion != null ? (double?)o.Pago_Ocupacion.Pago_Importe : null);

                    if (esTipoAbono)
                    {
                        // Buscamos el abono a través del vehículo
                        var vehAbonoRelacion = o.Vehiculo.Vehiculo_Abonado.FirstOrDefault(); // Asumimos un abono activo por vehículo

                        if (vehAbonoRelacion != null)
                        {
                            // Verificamos la tarifa mirando el último pago del abono
                            var ultimoPago = vehAbonoRelacion.Abono.Pagos_Abonados
                                               .OrderByDescending(p => p.Fecha_Pago)
                                               .FirstOrDefault();

                            // Si la tarifa del último pago coincide con la tarifa de la ocupación
                            if (ultimoPago != null && ultimoPago.Tarifa_id == o.Tarifa_id)
                            {
                                fechaVtoAbono = vehAbonoRelacion.Abono.Fecha_Vto;
                            }
                        }

                        var fallbackKey = Tuple.Create(o.Est_id, o.Vehiculo.Categoria_id);
                        if (tarifasPorHora.ContainsKey(fallbackKey))
                            tarifaFallback = (double)tarifasPorHora[fallbackKey];


                    if (o.Pago_Ocupacion != null)
                        {
                            // Abono Vencido que generó un Pago
                            tarifaDisplay = "Abonado (Vencido) - Por hora";
                            tarifaMontoDisplay = tarifaFallback;
                            montoDisplay = (double?)o.Pago_Ocupacion.Pago_Importe;
                        }
                        else
                        {
                            // Abono Vigente
                            tarifaDisplay = "Abonado";
                            tarifaMontoDisplay = 0.0;
                            montoDisplay = (double?)null;
                        }
                    }

                    return new Ocupacion_DTO
                    {
                        Est_id = o.Est_id,
                        Plaza_id = o.Plaza_id,
                        Ocu_fecha_Hora_Inicio = o.Ocu_fecha_Hora_Inicio,
                        Est_nombre = o.Plaza.Estacionamiento.Est_nombre,
                        Plaza_Nombre = o.Plaza.Plaza_Nombre,
                        Vehiculo_Patente = o.Vehiculo.Vehiculo_Patente,
                        Entrada = o.Ocu_fecha_Hora_Inicio.ToString("dd/MM/yyyy HH:mm"),
                        Salida = o.Ocu_fecha_Hora_Fin.HasValue
                                    ? o.Ocu_fecha_Hora_Fin.Value.ToString("dd/MM/yyyy HH:mm")
                                    : "",

                        Tarifa = tarifaDisplay,
                        Tarifa_Monto = tarifaMontoDisplay,
                        Monto = montoDisplay,
                        Fecha_Vto_Abono = fechaVtoAbono,
                        Tarifa_Por_Hora_Fallback = tarifaFallback,
                    };
                })
                .OrderByDescending(o => o.Ocu_fecha_Hora_Inicio)
                .ToList();

                // Guardar en sesión y bindear
                Session["DatosIngresos"] = ingresos;
                gvIngresos.DataSource = ingresos;
                gvIngresos.DataKeyNames = new string[] { "Est_id", "Plaza_id", "Ocu_fecha_Hora_Inicio" };
                gvIngresos.DataBind();
            }
        }


        protected void btnIngreso_Click(object sender, EventArgs e)
        {
            Response.Redirect("Ingreso_Registrar.aspx");
        }

        private List<Metodos_De_Pago> ObtenerMetodosDePagoFiltrados()
        {
            using (var db = new ProyectoEstacionamientoEntities())
            {
                int? estacionamientoId = (int)Session["Playero_EstId"];

                var metodosPago = db.Acepta_Metodo_De_Pago
                    .Where(a => a.Est_id == estacionamientoId)
                    .Select(a => a.Metodos_De_Pago)
                    .Distinct()
                    .ToList();

                return metodosPago;
            }
        }

        private void CargarMetodosDePagoEnDropDown()
        {
            var metodosPago = ObtenerMetodosDePagoFiltrados();

            if (metodosPago.Any())
            {
                ddlMetodoDePago.DataSource = metodosPago;
                ddlMetodoDePago.DataTextField = "Metodo_pago_descripcion";
                ddlMetodoDePago.DataValueField = "Metodo_pago_id";
                ddlMetodoDePago.DataBind();
            }
        }

        private void CargarMetodosDePagoEnJavascript()
        {
            var metodosPago = ObtenerMetodosDePagoFiltrados();

            if (metodosPago.Any())
            {
                string options = string.Join("",
                    metodosPago.Select(m => $"<option value='{m.Metodo_pago_id}'>{m.Metodo_pago_descripcion}</option>")
                );

                ClientScript.RegisterStartupScript(
                    this.GetType(),
                    "metodosPago",
                    $"window.metodosPagoOptions = `{options}`;",
                    true
                );
            }
        }


        protected void gvIngresos_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Egreso")
            {
                int index = Convert.ToInt32(e.CommandArgument);

                // 1. Recuperar claves
                int estId = (int)gvIngresos.DataKeys[index].Values["Est_id"];
                int plazaId = (int)gvIngresos.DataKeys[index].Values["Plaza_id"];
                DateTime inicio = (DateTime)gvIngresos.DataKeys[index].Values["Ocu_fecha_Hora_Inicio"];

                DateTime fin;
                if (!DateTime.TryParse(Salida.Value, out fin))
                {
                    fin = DateTime.Now; // fallback
                }

                inicio = new DateTime(inicio.Year, inicio.Month, inicio.Day,
                                      inicio.Hour, inicio.Minute, inicio.Second);

                string metodoPagoValor = hfMetodoPago.Value;

                using (var db = new ProyectoEstacionamientoEntities())
                {
                    if (metodoPagoValor == "ABONO")
                    {
                        // --- CASO 1: ES UN ABONADO (VIGENTE) ---
                        string sqlAbonado = @"
                    UPDATE Ocupacion SET Ocu_fecha_Hora_Fin = @p0
                    WHERE Est_id = @p1 AND Plaza_id = @p2
                        AND CONVERT(DATE, Ocu_fecha_Hora_Inicio) = @p3
                        AND DATEPART(HOUR, Ocu_fecha_Hora_Inicio) = @p4
                        AND DATEPART(MINUTE, Ocu_fecha_Hora_Inicio) = @p5
                        AND DATEPART(SECOND, Ocu_fecha_Hora_Inicio) = @p6;
                    UPDATE Plaza SET Plaza_Disponibilidad = 1
                    WHERE Est_id = @p1 AND Plaza_id = @p2;
                ";
                        db.Database.ExecuteSqlCommand(sqlAbonado, fin, estId, plazaId, inicio.Date, inicio.Hour, inicio.Minute, inicio.Second);
                    }
                    else
                    {
                        // --- CASO 2: INGRESO NORMAL o ABONO VENCIDO ---

                        int metodoPagoId = int.Parse(metodoPagoValor);

                        // VALIDAR QUE HAYA TURNO ABIERTO
                        if (Session["Turno_Id_Actual"] == null)
                        {
                            // Puedes mostrar un mensaje de error o redirigir
                            string script = "Swal.fire('Error', 'Debe iniciar un turno de caja para poder cobrar.', 'error');";
                            ScriptManager.RegisterStartupScript(this, GetType(), "alertaTurno", script, true);
                            return;
                        }
                        int turnoIdActual = (int)Session["Turno_Id_Actual"];

                        // Buscamos la ocupación CON TODAS LAS RELACIONES NECESARIAS
                        var ocupacion = db.Ocupacion
                            .Include("Tarifa.Tipos_Tarifa")
                            .Include("Vehiculo.Categoria_Vehiculo") // Necesario para Categoria_id
                            .Include("Vehiculo.Vehiculo_Abonado.Abono.Pagos_Abonados")
                            .Include("Vehiculo.Vehiculo_Abonado.Abono") // Necesario para Fecha_Vto
                            .FirstOrDefault(o => o.Est_id == estId
                                             && o.Plaza_id == plazaId
                                             && DbFunctions.TruncateTime(o.Ocu_fecha_Hora_Inicio) == inicio.Date
                                             && o.Ocu_fecha_Hora_Inicio.Hour == inicio.Hour
                                             && o.Ocu_fecha_Hora_Inicio.Minute == inicio.Minute
                                             && o.Ocu_fecha_Hora_Inicio.Second == inicio.Second);

                        if (ocupacion == null)
                            throw new Exception("No se encontró la ocupación para calcular el pago.");

                        string tarifaParaCalcular;
                        decimal tarifaBaseParaCalcular;
                        TimeSpan duracionParaCalcular;
                        decimal montoFinal = 0m; // Inicializar monto en 0

                        var idsAbonos = new List<int> { 3, 4, 5 };
                        bool esTipoAbono = idsAbonos.Contains(ocupacion.Tarifa.Tipos_Tarifa_Id.Value);

                        if (esTipoAbono)
                        {
                            // --- SUBCASO 2a: ES UN ABONO (VENCIDO) ---

                            // --- Obtener FechaVto desde el último pago ---
                            var vehAbonoRelacion = ocupacion.Vehiculo.Vehiculo_Abonado.FirstOrDefault();
                            DateTime fechaVto = DateTime.MinValue; // Valor por defecto seguro

                            if (vehAbonoRelacion != null)
                            {
                                var ultimoPago = vehAbonoRelacion.Abono.Pagos_Abonados
                                                    .OrderByDescending(p => p.Fecha_Pago)
                                                    .FirstOrDefault();

                                // Validamos que el último pago corresponda a la tarifa de la ocupación
                                if (ultimoPago != null && ultimoPago.Tarifa_id == ocupacion.Tarifa_id)
                                {
                                    fechaVto = vehAbonoRelacion.Abono.Fecha_Vto;
                                }
                                else
                                {
                                    // Si no coincide, algo está mal en los datos o no es el abono correcto.
                                    // Podrías lanzar error o manejarlo.
                                    throw new Exception("Error de integridad: La tarifa de la ocupación no coincide con el último pago del abono.");
                                }
                            }

                            // Calculamos la duración excedida
                            duracionParaCalcular = fin - fechaVto;

                            // --- Aplicar regla de 15 minutos ---
                            if (duracionParaCalcular.TotalMinutes >= 15)
                            {
                                tarifaParaCalcular = "Por hora";
                                int idTarifaPorHora = 1;
                                int categoriaId = ocupacion.Vehiculo.Categoria_id;
                                var tarifaFallback = db.Tarifa.FirstOrDefault(t =>
                                                        t.Est_id == ocupacion.Est_id &&
                                                        t.Tipos_Tarifa_Id == idTarifaPorHora &&
                                                        t.Categoria_id == categoriaId);

                                if (tarifaFallback == null)
                                    throw new Exception($"No se encontró tarifa 'Por hora' de fallback para Categoria ID {categoriaId}");

                                tarifaBaseParaCalcular = (decimal)tarifaFallback.Tarifa_Monto;

                                // Calculamos el monto solo si excedió los 15 min
                                montoFinal = CalcularMonto(tarifaParaCalcular, duracionParaCalcular, tarifaBaseParaCalcular);
                            }
                            // Si la duración es < 15 min, montoFinal permanece en 0m
                        }
                        else
                        {
                            // --- SUBCASO 2b: ES UN INGRESO NORMAL ---
                            tarifaParaCalcular = ocupacion.Tarifa.Tipos_Tarifa.Tipos_tarifa_descripcion;
                            tarifaBaseParaCalcular = Convert.ToDecimal(ocupacion.Tarifa.Tarifa_Monto);
                            duracionParaCalcular = fin - inicio; // Duración completa
                            montoFinal = CalcularMonto(tarifaParaCalcular, duracionParaCalcular, tarifaBaseParaCalcular);
                        }

                        // --- INICIO DE LÓGICA DE PAGO (Solo si hay monto) ---
                        int? pagoId = null; // El Pago_id será nulo por defecto

                        if (montoFinal > 0)
                        {
                            // 6. Validar que haya turno abierto
                            if (Session["Turno_Id_Actual"] == null)
                            {
                                // Mostrar error: "Debe iniciar un turno antes de cobrar."
                                return;
                            }
                            int turnoId = (int)Session["Turno_Id_Actual"];

                            // 7. Registrar Pago_Ocupacion SOLO SI HAY ALGO QUE COBRAR
                            Pago_Ocupacion pago = new Pago_Ocupacion
                            {
                                Est_id = ocupacion.Est_id,
                                Metodo_Pago_id = metodoPagoId,
                                Pago_Importe = Convert.ToDouble(montoFinal),
                                Pago_Fecha = fin.Date,
                                Turno_id = turnoId // <-- Asignacion del TURNO
                            };
                            db.Pago_Ocupacion.Add(pago);
                            db.SaveChanges(); // Obtenemos pago.Pago_id
                            pagoId = pago.Pago_id; // Asignamos el ID generado
                        }

                        // 8. SQL directo para actualizar todo
                        string sql = @"
                    UPDATE Ocupacion
                    SET Ocu_fecha_Hora_Fin = @p0, Pago_id = @p10
                    WHERE Est_id = @p1 AND Plaza_id = @p2
                        AND CONVERT(DATE, Ocu_fecha_Hora_Inicio) = @p3
                        AND DATEPART(HOUR, Ocu_fecha_Hora_Inicio) = @p4
                        AND DATEPART(MINUTE, Ocu_fecha_Hora_Inicio) = @p5
                        AND DATEPART(SECOND, Ocu_fecha_Hora_Inicio) = @p6;

                    -- Solo actualizamos Pago_Ocupacion si creamos uno
                    " + (pagoId.HasValue ? @"
                    UPDATE Pago_Ocupacion
                    SET Pago_Importe = @p7, Pago_Fecha = @p8
                    WHERE Pago_id = @p9;
                    " : "") + @"

                    UPDATE Plaza
                    SET Plaza_Disponibilidad = 1
                    WHERE Est_id = @p1 AND Plaza_id = @p2;
                ";

                        db.Database.ExecuteSqlCommand(
                            sql,
                            fin,                   // @p0
                            estId,                 // @p1
                            plazaId,               // @p2
                            inicio.Date,           // @p3
                            inicio.Hour,           // @p4
                            inicio.Minute,         // @p5
                            inicio.Second,         // @p6
                            (double)montoFinal,    // @p7
                            fin.Date,              // @p8
                            (object)pagoId ?? DBNull.Value, // @p9 (Usa el ID o DBNull si no hubo pago)
                            (object)pagoId ?? DBNull.Value  // @p10 (Usa el ID o DBNull si no hubo pago)
                        );
                        // --- FIN DE LÓGICA DE PAGO ---
                    }

                    // Redirigir (común para ambos casos)
                    Response.Redirect($"~/Pages/Ingresos/Ingreso_Listar.aspx?exito=1&accion=egreso");
                }
            }
        }


        // Método para calcular el monto según tarifa y duración
        private decimal CalcularMonto(string tarifa, TimeSpan duracion, decimal tarifaBase)
        {
            switch (tarifa)
            {
                case "Por hora":
                    int horasEnteras = (int)Math.Floor(duracion.TotalHours);
                    double minutosRestantes = (duracion.TotalHours - horasEnteras) * 60;

                    int horasCobrar = horasEnteras;

                    if (minutosRestantes >= 15)
                        horasCobrar++;

                    if (horasCobrar < 1)
                        horasCobrar = 1;

                    return horasCobrar * tarifaBase;

                case "Por día":
                    int diasCobrar = (int)Math.Ceiling(duracion.TotalHours / 24);
                    if (diasCobrar < 1) diasCobrar = 1;
                    return diasCobrar * tarifaBase;

                case "Quincenal":
                    int quincenas = (int)Math.Ceiling(duracion.TotalHours / (24 * 15)); // 15 días
                    if (quincenas < 1) quincenas = 1;
                    return quincenas * tarifaBase;

                default:
                    throw new Exception($"Tipo de tarifa desconocido: {tarifa}");
            }
        }
    }
}