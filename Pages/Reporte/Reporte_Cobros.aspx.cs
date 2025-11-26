using Microsoft.Reporting.WebForms; // Necesario
using System;
using System.Collections.Generic;
using System.Data; // Necesario para DataTable
using System.Data.Entity;
using System.Globalization; // Necesario para nombres de días
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Proyecto_Estacionamiento.Pages.Reporte
{

    public partial class Reporte_Cobros : System.Web.UI.Page
    {
        // ---  CÓDIGO REQUERIDO POR REPORTVIEWER 👇 ---
        static bool _isSqlTypesLoaded = false;

        public Reporte_Cobros()
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
            // 1. VALIDACIONES
            Page.Validate("Reporte");
            if (!Page.IsValid) return;

            // 2. OBTENER PARÁMETROS
            DateTime desde = DateTime.Parse(txtDesde.Text);
            // Ajustamos 'hasta' al final del día para incluir todos los cobros de esa fecha
            DateTime hasta = DateTime.Parse(txtHasta.Text).AddDays(1).AddSeconds(-1);

            int estacionamientoId = 0;
            if (Session["Dueño_EstId"] != null)
                estacionamientoId = (int)Session["Dueño_EstId"];
            else
                estacionamientoId = int.Parse(ddlEstacionamiento.SelectedValue);

            string nombreEstacionamiento = "";

            using (var db = new ProyectoEstacionamientoEntities())
            {
                // Obtener nombre del estacionamiento para el título
                var est = db.Estacionamiento.Find(estacionamientoId);
                nombreEstacionamiento = est != null ? est.Est_nombre : "Desconocido";

                // 3. CONSULTA 1: COBROS NORMALES (Pago_Ocupacion)
                var cobrosNormales = db.Pago_Ocupacion
                    .Where(p => p.Est_id == estacionamientoId &&
                                p.Pago_Fecha >= desde &&
                                p.Pago_Fecha <= hasta)
                    .Select(p => new
                    {
                        Fecha = p.Pago_Fecha, // Es 'date' en SQL
                        TipoCobro = p.Metodos_De_Pago.Metodo_pago_descripcion,
                        Monto = p.Pago_Importe,
                        Categoria = "Normal"
                    })
                    .ToList() // Traemos a memoria
                    .Select(x => new // Proyección a tipos compatibles
                    {
                        // Convertimos Date a DateTime
                        Fecha = x.Fecha.HasValue ? (DateTime)x.Fecha : DateTime.MinValue,
                        x.TipoCobro,
                        Monto = (decimal)x.Monto,
                        x.Categoria
                    });

                // 4. CONSULTA 2: COBROS ABONOS (Pagos_Abonados)
                var cobrosAbonos = db.Pagos_Abonados
                    .Where(p => p.Est_id == estacionamientoId &&
                                p.Fecha_Pago >= desde &&
                                p.Fecha_Pago <= hasta)
                    .Select(p => new
                    {
                        Fecha = p.Fecha_Pago, // Es 'datetime' en SQL
                        // Navegamos por la relación correcta para obtener la descripción
                        TipoCobro = p.Acepta_Metodo_De_Pago.Metodos_De_Pago.Metodo_pago_descripcion,
                        Monto = p.PA_Monto,
                        Categoria = "Abono"
                    })
                    .ToList()
                    .Select(x => new
                    {
                        x.Fecha,
                        x.TipoCobro,
                        Monto = (decimal)x.Monto,
                        x.Categoria
                    });

                // 5. UNIFICAR LISTAS
                var listaTotal = cobrosNormales.Concat(cobrosAbonos).OrderBy(x => x.Fecha).ToList();

                if (!listaTotal.Any())
                {
                    // Mostrar mensaje de error usando tu helper o un label
                    // mostrarAlerta("Sin datos", "No se encontraron cobros en el rango seleccionado.", "info");
                    lblMensaje.Text = "No se encontraron cobros.";
                    lblMensaje.Visible = true;
                    rvCobros.Visible = false;
                    return;
                }
                lblMensaje.Visible = false;

                // 6. LLENAR EL DATASET (DataTable)
                DataTable dt = new DataTable();
                dt.Columns.Add("Fecha", typeof(DateTime));
                dt.Columns.Add("TipoCobro", typeof(string));
                dt.Columns.Add("Monto", typeof(decimal));
                dt.Columns.Add("Categoria", typeof(string));

                foreach (var item in listaTotal)
                {
                    dt.Rows.Add(item.Fecha, item.TipoCobro, item.Monto, item.Categoria);
                }

                // 7. CONFIGURAR REPORTVIEWER
                rvCobros.LocalReport.ReportPath = Server.MapPath("~/Reportes/ReporteCobros.rdlc");
                rvCobros.LocalReport.DataSources.Clear();

                // "DtCobros" debe coincidir con el nombre del DataSet DENTRO del .rdlc
                rvCobros.LocalReport.DataSources.Add(new ReportDataSource("DtCobros", dt));

                // Parámetros visuales
                ReportParameter[] parametros = new ReportParameter[]
                {
                    new ReportParameter("FechaDesde", desde.ToString("dd/MM/yyyy")),
                    new ReportParameter("FechaHasta", hasta.ToString("dd/MM/yyyy")),
                    new ReportParameter("NombreEstacionamiento", nombreEstacionamiento)
                };
                rvCobros.LocalReport.SetParameters(parametros);

                // Nombre de descarga 
                string fechaArchivo = DateTime.Now.ToString("yyyy-MM-dd");
                rvCobros.LocalReport.DisplayName = $"Reporte_Cobros_{nombreEstacionamiento}_{fechaArchivo}";

                rvCobros.Visible = true;
                rvCobros.LocalReport.Refresh();
            }
        }

    }
}