using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Linq;
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

                    gvIngresos.Columns[7].Visible = false;
                }
                else
                {
                    CargarMetodosDePagoEnDropDown(); // llena ddlMetodoDePago
                    gvIngresos.Columns[0].Visible = false;
                    gvIngresos.Columns[5].Visible = false;
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
            // Obtener el valor del campo de texto de la patente
            string patente = txtPatente.Text.Trim();

            // Verificar si el campo de patente está vacío
            if (string.IsNullOrEmpty(patente))
            {
                string script = "Swal.fire({icon: 'error', title: 'Campo vacío', text: 'Por favor, ingrese una patente para buscar.'});";
                ScriptManager.RegisterStartupScript(this, GetType(), "alert", script, true);
                return; // Detener la ejecución si el campo está vacío
            }

            // Llamar al método CargarIngresos, pasando solo la patente
            CargarIngresos(null, null, patente);
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
            public int Tarifa_id { get; set; }
            public string Tarifa { get; set; }
            public string Entrada { get; set; }
            public string Salida { get; set; }
            public double? Monto { get; set; }
        }

        private void CargarIngresos()
        {
            string tipoUsuario = Session["Usu_tipo"] as string;
            int legajo = Convert.ToInt32(Session["Usu_legajo"]);
            int estId;

            using (var db = new ProyectoEstacionamientoEntities())
            {
                IQueryable<Ocupacion> query = db.Ocupacion
                    .Include("Vehiculo")
                    .Include("Plaza")
                    .Include("Tarifa");

                if (tipoUsuario == "Dueño")
                {
                    if (Session["Dueño_EstId"] != null)
                    {
                        // Caso 1: Dueño eligió un estacionamiento
                        estId = (int)Session["Dueño_EstId"];
                        query = query.Where(o => o.Est_id == estId);
                    }
                    else
                    {
                        // Caso 2: No eligió → mostramos todos sus estacionamientos
                        var estIds = db.Estacionamiento
                                       .Where(e => e.Dueño_Legajo == legajo)
                                       .Select(e => e.Est_id);
                        query = query.Where(o => estIds.Contains(o.Est_id));
                    }
                }
                else if (tipoUsuario == "Playero")
                {
                    estId = (int)Session["Playero_EstId"];
                    query = query.Where(o => o.Est_id == estId);

                    query = query.Where(o =>
                        o.Est_id == estId 
                    );

                    // 🔹 Solo ingresos sin salida
                    query = query.Where(o => !o.Ocu_fecha_Hora_Fin.HasValue);
                }

                var ocupaciones = query.ToList();

                var ingresos = ocupaciones.Select(o => new Ocupacion_DTO
                {
                    Est_id = o.Est_id,
                    Plaza_id = o.Plaza_id,
                    Ocu_fecha_Hora_Inicio = o.Ocu_fecha_Hora_Inicio,
                    Est_nombre = o.Plaza.Estacionamiento.Est_nombre,
                    Plaza_Nombre = o.Plaza.Plaza_Nombre,
                    Vehiculo_Patente = o.Vehiculo.Vehiculo_Patente,
                    Tarifa_id = o.Tarifa_id,
                    Tarifa = o.Tarifa.Tipos_Tarifa.Tipos_tarifa_descripcion,
                    Entrada = o.Ocu_fecha_Hora_Inicio.ToString("dd/MM/yyyy HH:mm"),
                    Salida = o.Ocu_fecha_Hora_Fin.HasValue ? o.Ocu_fecha_Hora_Fin.Value.ToString("dd/MM/yyyy HH:mm") : "",
                    Monto = o.Pago_Ocupacion != null ? (double?)o.Pago_Ocupacion.Pago_Importe : null
                })
                .OrderByDescending(o => o.Ocu_fecha_Hora_Inicio)
                .ToList();

                // Guardamos en sesión para reutilizar al ordenar
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

            using (var db = new ProyectoEstacionamientoEntities())
            {
                IQueryable<Ocupacion> query = db.Ocupacion
                    .Include("Vehiculo")
                    .Include("Plaza")
                    .Include("Tarifa");

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

                var ingresos = ocupaciones.Select(o => new Ocupacion_DTO
                {
                    Est_id = o.Est_id,
                    Plaza_id = o.Plaza_id,
                    Ocu_fecha_Hora_Inicio = o.Ocu_fecha_Hora_Inicio,
                    Est_nombre = o.Plaza.Estacionamiento.Est_nombre,
                    Plaza_Nombre = o.Plaza.Plaza_Nombre,
                    Vehiculo_Patente = o.Vehiculo.Vehiculo_Patente,
                    Tarifa_id = o.Tarifa_id,
                    Tarifa = o.Tarifa.Tipos_Tarifa.Tipos_tarifa_descripcion,
                    Entrada = o.Ocu_fecha_Hora_Inicio.ToString("dd/MM/yyyy HH:mm"),
                    Salida = o.Ocu_fecha_Hora_Fin.HasValue
                                ? o.Ocu_fecha_Hora_Fin.Value.ToString("dd/MM/yyyy HH:mm")
                                : "",
                    Monto = o.Pago_Ocupacion != null ? (double?)o.Pago_Ocupacion.Pago_Importe : null
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
            decimal montoFinal = 0M; // <-- declarar fuera

            if (e.CommandName == "Egreso")
            {
                int index = Convert.ToInt32(e.CommandArgument);

                // Recuperamos todas las claves necesarias desde DataKeyNames
                int estId = (int)gvIngresos.DataKeys[index].Values["Est_id"];
                int plazaId = (int)gvIngresos.DataKeys[index].Values["Plaza_id"];
                DateTime inicio = (DateTime)gvIngresos.DataKeys[index].Values["Ocu_fecha_Hora_Inicio"];

                //Obtenemos el id del método de pago seleccionado
                int metodoPagoId = int.Parse(hfMetodoPago.Value);

                using (var db = new ProyectoEstacionamientoEntities())
                {
                    db.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);

                    // 1. Buscar la ocupación con las 3 claves, incluyendo tablas relacionadas
                    inicio = new DateTime(inicio.Year, inicio.Month, inicio.Day,
                                          inicio.Hour, inicio.Minute, inicio.Second);

                    var ocupacion = db.Ocupacion
                        .Include("Tarifa.Tipos_Tarifa")
                        .Include("Pago_Ocupacion")
                        .FirstOrDefault(o => o.Est_id == estId
                                          && o.Plaza_id == plazaId
                                          && DbFunctions.TruncateTime(o.Ocu_fecha_Hora_Inicio) == inicio.Date
                                          && o.Ocu_fecha_Hora_Inicio.Hour == inicio.Hour
                                          && o.Ocu_fecha_Hora_Inicio.Minute == inicio.Minute
                                          && o.Ocu_fecha_Hora_Inicio.Second == inicio.Second);

                    if (ocupacion == null)
                        throw new Exception($"No se encontró ocupacion con: Est_id={estId}, Plaza_id={plazaId}, Ocu_fecha_Hora_Inicio={inicio:yyyy-MM-dd HH:mm:ss.fff}");

                    // 2. Obtener descripción de la tarifa
                    string tarifa = ocupacion.Tarifa.Tipos_Tarifa.Tipos_tarifa_descripcion;
                    if (string.IsNullOrEmpty(tarifa))
                        throw new Exception("Descripción de tarifa no encontrada");

                    // 4. Calcular el importe final según duración y tipo de tarifa
                    decimal tarifaBase = Convert.ToDecimal(ocupacion.Tarifa.Tarifa_Monto);
                    DateTime fin = DateTime.Now;
                    TimeSpan duracion = fin - inicio;
                    montoFinal = CalcularMonto(tarifa, duracion, tarifaBase);

                    // 3. Obtener el importe base de Pago_Ocupacion
                    Pago_Ocupacion pago;
                    pago = new Pago_Ocupacion
                    {
                        Est_id = ocupacion.Est_id,
                        Metodo_Pago_id = metodoPagoId,
                        Pago_Importe = Convert.ToDouble(montoFinal),
                        Pago_Fecha = fin.Date
                    };
                    db.Pago_Ocupacion.Add(pago);
                    db.SaveChanges(); // <-- aquí obtenemos pago.Pago_id (identity)

                    // asociar pago a la ocupación
                    ocupacion.Pago_id = pago.Pago_id;
                    ocupacion.Ocu_fecha_Hora_Fin = fin;

                    // 6. Actualizar disponibilidad de plaza
                    var plaza = db.Plaza.FirstOrDefault(p => p.Est_id == estId && p.Plaza_id == plazaId);
                    if (plaza == null)
                        throw new Exception("Plaza no encontrada para actualizar disponibilidad");

                    plaza.Plaza_Disponibilidad = true;

                    // Actualizar en la BD usando SQL directo
                    string sql = @"
                    UPDATE Ocupacion
                    SET Ocu_fecha_Hora_Fin = @p0,Pago_id = @p10
                    WHERE Est_id = @p1 AND Plaza_id = @p2
                        AND CONVERT(DATE, Ocu_fecha_Hora_Inicio) = @p3
                        AND DATEPART(HOUR, Ocu_fecha_Hora_Inicio) = @p4
                        AND DATEPART(MINUTE, Ocu_fecha_Hora_Inicio) = @p5
                        AND DATEPART(SECOND, Ocu_fecha_Hora_Inicio) = @p6;

                    UPDATE Pago_Ocupacion
                    SET Pago_Importe = @p7, Pago_Fecha = @p8
                    WHERE Pago_id = @p9;

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
                        fin.Date,               // @p8
                        pago.Pago_id,          // @p9 (Pago_Ocupacion update)
                        pago.Pago_id           // @p10 (Ocupacion.Pago_id)
                    );

                }
            }
            Response.Redirect($"~/Pages/Ingresos/Ingreso_Listar.aspx?exito=1&accion=egreso&monto={montoFinal:0.00}");
        }


        // Método separado para calcular el monto según tarifa y duración
        private decimal CalcularMonto(string tarifa, TimeSpan duracion, decimal tarifaBase)
        {
            switch (tarifa)
            {
                case "Por hora":
                    var horasEnteras = (int)duracion.TotalHours;
                    var minutosRestantes = duracion.Minutes;

                    int horasCobrar = horasEnteras;

                    if (minutosRestantes > 15)
                    {
                        horasCobrar++;
                    }

                    if (horasCobrar < 1)
                    {
                        horasCobrar = 1;
                    }

                    return horasCobrar * tarifaBase;

                case "Por día":
                    int diasCobrar = (int)Math.Ceiling(duracion.TotalDays - 0.5); // 12 horas = 0.5 días
                    if (diasCobrar < 1) diasCobrar = 1;
                    return diasCobrar * tarifaBase;

                case "Semanal":
                    int semanas = (int)Math.Ceiling(duracion.TotalDays / 7);
                    if (semanas < 1) semanas = 1;
                    return semanas * tarifaBase;

                case "Mensual":
                    int meses = (int)Math.Ceiling(duracion.TotalDays / 30);
                    if (meses < 1) meses = 1;
                    return meses * tarifaBase;

                case "Anual":
                    int anios = (int)Math.Ceiling(duracion.TotalDays / 365);
                    if (anios < 1) anios = 1;
                    return anios * tarifaBase;

                default:
                    throw new Exception($"Tipo de tarifa desconocido: {tarifa}");
            }
        }

    }
}