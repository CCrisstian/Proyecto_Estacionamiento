using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static Proyecto_Estacionamiento.Ingreso_Listar;
using Microsoft.Reporting.WebForms; // Necesario
using System.Data; // Necesario para DataTable
using System.Data.Entity;
using System.Globalization; // Necesario para nombres de días

namespace Proyecto_Estacionamiento.Pages.Ingresos
{
    public partial class Ingreso_Reporte : System.Web.UI.Page
    {
        // ---  CÓDIGO REQUERIDO POR REPORTVIEWER 👇 ---
        static bool _isSqlTypesLoaded = false;

        public Ingreso_Reporte()
        {
            if (!_isSqlTypesLoaded)
            {
                // Usamos HttpContext.Current.Server porque 'Server' a veces no está listo en el constructor
                SqlServerTypes.Utilities.LoadNativeAssemblies(System.Web.HttpContext.Current.Server.MapPath("~"));
                _isSqlTypesLoaded = true;
            }
        }
        // --- FIN DEL CÓDIGO REQUERIDO ---

        protected void Page_Load(object sender, EventArgs e)
        {
            string estacionamiento = Session["Usu_estacionamiento"] as string;

            if (!string.IsNullOrEmpty(estacionamiento))
            {
                Estacionamiento_Nombre.Text = $"Estacionamiento: '<strong>{estacionamiento}</strong>'";
            }
            else
            {
                Estacionamiento_Nombre.Visible = false;
            }

            if (!IsPostBack)
            {
                CargarLogicaEstacionamiento();

                // Fechas por defecto
                txtDesde.Text = DateTime.Now.ToString("dd/MM/yyyy");
                txtHasta.Text = DateTime.Now.ToString("dd/MM/yyyy");
            }
        }

        private void CargarLogicaEstacionamiento()
        {
            // Verificar si ya hay un estacionamiento seleccionado en sesión
            if (Session["Dueño_EstId"] != null)
            {
                // CASO 1: Estacionamiento YA seleccionado
                string nombreEst = Session["Usu_estacionamiento"] as string;
                Estacionamiento_Nombre.Text = $"Estacionamiento: '<strong>{nombreEst}</strong>'";
                Estacionamiento_Nombre.Visible = true;

                // Ocultar selección manual
                lblEstacionamiento.Visible = false;
                ddlEstacionamiento.Visible = false;
            }
            else
            {
                // CASO 2: NO hay estacionamiento seleccionado -> Permitir elegir
                Estacionamiento_Nombre.Visible = false;

                lblEstacionamiento.Visible = true;
                ddlEstacionamiento.Visible = true;

                CargarComboEstacionamientos();
            }
        }

        protected void btnVolver(object sender, EventArgs e)
        {
            Response.Redirect("~/Pages/Reporte/Reportes_Listar.aspx");
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


        protected void btnGenerarReporte_Click(object sender, EventArgs e)
        {
            // 1. Validaciones (Usa las que ya implementamos con CustomValidator o Page.IsValid)
            Page.Validate("Reporte");
            if (!Page.IsValid) return;

            // 2. Obtener parámetros
            DateTime desde = DateTime.Parse(txtDesde.Text);
            DateTime hasta = DateTime.Parse(txtHasta.Text).AddDays(1).AddSeconds(-1); // Final del día

            int estacionamientoId = 0;
            if (Session["Dueño_EstId"] != null)
                estacionamientoId = (int)Session["Dueño_EstId"];
            else
                estacionamientoId = int.Parse(ddlEstacionamiento.SelectedValue);

            string nombreEstacionamiento = ""; // Lo buscaremos abajo

            // Lista de IDs de Abonos (Semanal, Mensual, Anual)
            var idsAbonos = new List<int> { 3, 4, 5 };

            using (var db = new ProyectoEstacionamientoEntities())
            {
                // 3. Obtener Nombre del Estacionamiento (para el título)
                var est = db.Estacionamiento.Find(estacionamientoId);
                nombreEstacionamiento = est != null ? est.Est_nombre : "Desconocido";

                // 4. CONSULTA PRINCIPAL (Tabla Ocupacion)
                var ocupaciones = db.Ocupacion
                    .Include("Plaza")
                    .Include("Tarifa")
                    .Where(o => o.Est_id == estacionamientoId &&
                                o.Ocu_fecha_Hora_Inicio >= desde &&
                                o.Ocu_fecha_Hora_Inicio <= hasta)
                    .ToList(); // Traemos a memoria para procesar fechas y tipos

                if (!ocupaciones.Any())
                {
                    lblMensaje.Text = "No se encontraron registros para el período seleccionado.";
                    lblMensaje.Visible = true;
                    rvIngresos.Visible = false;
                    return;
                }
                lblMensaje.Visible = false;

                // 5. PREPARAR DATOS PARA EL DATASET (Transformación)
                DataTable dt = new DataTable();
                dt.Columns.Add("Plaza", typeof(string));
                dt.Columns.Add("FechaHora", typeof(DateTime));
                dt.Columns.Add("DiaSemana", typeof(string));
                dt.Columns.Add("Hora", typeof(int));
                dt.Columns.Add("Tipo", typeof(string));

                // Cultura española para los nombres de los días
                var culturaEs = new CultureInfo("es-ES");

                foreach (var item in ocupaciones)
                {
                    // Determinar si es Abono o Normal
                    bool esAbono = false;
                    if (item.Tarifa != null && item.Tarifa.Tipos_Tarifa_Id.HasValue)
                    {
                        esAbono = idsAbonos.Contains(item.Tarifa.Tipos_Tarifa_Id.Value);
                    }

                    // Obtener nombre del día (Primera letra mayúscula)
                    string nombreDia = culturaEs.DateTimeFormat.GetDayName(item.Ocu_fecha_Hora_Inicio.DayOfWeek);
                    nombreDia = char.ToUpper(nombreDia[0]) + nombreDia.Substring(1);

                    dt.Rows.Add(
                        item.Plaza.Plaza_Nombre,
                        item.Ocu_fecha_Hora_Inicio,
                        nombreDia,
                        item.Ocu_fecha_Hora_Inicio.Hour,
                        esAbono ? "Abonado" : "Normal"
                    );
                }

                // 6. CONFIGURAR REPORTVIEWER
                rvIngresos.LocalReport.ReportPath = Server.MapPath("~/Reportes/ReporteIngresos.rdlc"); // Ajusta la ruta
                rvIngresos.LocalReport.DataSources.Clear();

                // "DtOcupacion" debe ser el nombre EXACTO de tu DataSet en el archivo .rdlc
                rvIngresos.LocalReport.DataSources.Add(new ReportDataSource("DtOcupacion", dt));

                // Pasar parámetros simples (Fechas y Título)
                ReportParameter[] parametros = new ReportParameter[]
                {
                    new ReportParameter("FechaDesde", desde.ToString("dd/MM/yyyy")),
                    new ReportParameter("FechaHasta", hasta.ToString("dd/MM/yyyy")),
                    new ReportParameter("NombreEstacionamiento", nombreEstacionamiento)
                };
                rvIngresos.LocalReport.SetParameters(parametros);

                rvIngresos.LocalReport.DisplayName = $"Reporte_de_Ingresos_de_Vehículos_{nombreEstacionamiento}_{desde:yyyy-MM-dd}";

                rvIngresos.Visible = true;
                rvIngresos.LocalReport.Refresh();
            }
        }
    }
}