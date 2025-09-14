using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
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
                CargarDashboard();

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
                    gvIngresos.Columns[0].Visible = false;
                    gvIngresos.Columns[5].Visible = false;
                }

                string estacionamiento = Session["Usu_estacionamiento"] as string;
                Estacionamiento_Nombre.Text = $"Estacionamiento: <strong>{estacionamiento}</strong>";

                CargarIngresos();
            }
        }

        private void CargarDashboard()
        {
            string tipoUsuario = Session["Usu_tipo"] as string;
            int legajo = Convert.ToInt32(Session["Usu_legajo"]);

            using (var db = new ProyectoEstacionamientoEntities())
            {
                List<Plaza> plazas = new List<Plaza>();

                if (tipoUsuario == "Dueño")
                {
                    if (Session["Dueño_EstId"] != null)
                    {
                        // Caso 1: Dueño eligió un estacionamiento
                        int estId = (int)Session["Dueño_EstId"];
                        plazas = db.Plaza
                                   .Where(p => p.Est_id == estId)
                                   .ToList();
                    }
                    else
                    {
                        // Caso 2: No eligió → mostramos todos sus estacionamientos
                        var estIds = db.Estacionamiento
                                       .Where(e => e.Dueño_Legajo == legajo)
                                       .Select(e => e.Est_id)
                                       .ToList();

                        plazas = db.Plaza
                                   .Where(p => estIds.Contains(p.Est_id))
                                   .ToList();
                    }
                }
                else if (tipoUsuario == "Playero")
                {
                    // Obtener el Est_id asignado al Playero
                    int estId = (int)Session["Playero_EstId"];
                    plazas = db.Plaza
                               .Where(p => p.Est_id == estId)
                               .ToList();
                }

                int plazasDisponibles = plazas.Count(p => p.Plaza_Disponibilidad);
                int plazasOcupadas = plazas.Count(p => !p.Plaza_Disponibilidad);
                lblPlazasDisponibles.Text = $"Plazas Disponibles: {plazasDisponibles}";
                lblPlazasOcupadas.Text = $"Plazas Ocupadas: {plazasOcupadas}";
            }
        }

        public class Ocupacion_DTO
        {
            public int Est_id { get; set; }
            public int Plaza_id { get; set; }
            public DateTime Ocu_fecha_Hora_Inicio { get; set; }
            // Más campos para mostrar y uso
            public string Est_nombre { get; set; }
            public string Plaza_Nombre { get; set; }
            public string Vehiculo_Patente { get; set; }
            public int Tarifa_id { get; set; }
            public int? Pago_id { get; set; }
            public string Tarifa { get; set; }
            public string Entrada { get; set; }
            public string Salida { get; set; }
            public double? Monto { get; set; }
        }

        private void CargarIngresos()
        {
            string tipoUsuario = Session["Usu_tipo"] as string;
            int legajo = Convert.ToInt32(Session["Usu_legajo"]);

            using (var db = new ProyectoEstacionamientoEntities())
            {
                IQueryable<Ocupacion> query = db.Ocupacion
                    .Include("Vehiculo")
                    .Include("Plaza")
                    .Include("Tarifa")
                    .Include("Pago_Ocupacion");

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
                        // Caso 2: No eligió → mostramos todos sus estacionamientos
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
                    Pago_id = o.Pago_id,
                    Tarifa = o.Tarifa.Tipos_Tarifa.Tipos_tarifa_descripcion,
                    Entrada = o.Ocu_fecha_Hora_Inicio.ToString("dd/MM/yyyy HH:mm"),
                    Salida = o.Ocu_fecha_Hora_Fin.HasValue ? o.Ocu_fecha_Hora_Fin.Value.ToString("dd/MM/yyyy HH:mm") : "",
                    Monto = o.Pago_Ocupacion?.Pago_Importe
                })
                .OrderByDescending(o => o.Ocu_fecha_Hora_Inicio)
                .ToList();

                gvIngresos.DataSource = ingresos;
                gvIngresos.DataKeyNames = new string[] { "Est_id", "Plaza_id", "Ocu_fecha_Hora_Inicio" };
                gvIngresos.DataBind();
            }
        }


        protected void btnIngreso_Click(object sender, EventArgs e)
        {
            Response.Redirect("Ingreso_Registrar.aspx");
        }


        protected void gvIngresos_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Egreso")
            {
                int index = Convert.ToInt32(e.CommandArgument);

                // Recuperamos todas las claves necesarias desde DataKeyNames
                int estId = (int)gvIngresos.DataKeys[index].Values["Est_id"];
                int plazaId = (int)gvIngresos.DataKeys[index].Values["Plaza_id"];
                DateTime inicio = (DateTime)gvIngresos.DataKeys[index].Values["Ocu_fecha_Hora_Inicio"];

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

                    // 3. Obtener el importe base de Pago_Ocupacion
                    if (ocupacion.Pago_id == null)
                        throw new Exception("Pago_id nulo en ocupación");

                    var pago = db.Pago_Ocupacion.FirstOrDefault(p => p.Pago_id == ocupacion.Pago_id);

                    if (pago == null)
                        throw new Exception($"No se encontró Pago_Ocupacion con Pago_id={ocupacion.Pago_id}");


                    decimal tarifaBase = (decimal)pago.Pago_Importe;

                    // 4. Calcular el importe final según duración y tipo de tarifa
                    DateTime fin = DateTime.Now;
                    TimeSpan duracion = fin - inicio;

                    decimal montoFinal = CalcularMonto(tarifa, duracion, tarifaBase);

                    // 6. Actualizar disponibilidad de plaza
                    var plaza = db.Plaza.FirstOrDefault(p => p.Est_id == estId && p.Plaza_id == plazaId);
                    if (plaza == null)
                        throw new Exception("Plaza no encontrada para actualizar disponibilidad");

                    plaza.Plaza_Disponibilidad = true;

                    // 7. Guardar cambios en la base de datos
                    ocupacion.Ocu_fecha_Hora_Fin = fin;
                    pago.Pago_Importe = (double)montoFinal;
                    pago.Pago_Fecha = fin.Date;
                    plaza.Plaza_Disponibilidad = true;


                    // Actualizar en la BD usando SQL directo
                    string sql = @"
                    UPDATE Ocupacion
                    SET Ocu_fecha_Hora_Fin = @p0
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
                        ocupacion.Pago_id       // @p9
                    );

                }
            }
            Response.Redirect($"~/Pages/Default/Ingreso_Listar.aspx?exito=1&accion=egreso");
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