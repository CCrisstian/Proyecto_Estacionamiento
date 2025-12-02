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
            DateTime hasta = DateTime.Parse(txtHasta.Text).AddDays(1).AddSeconds(-1);

            int estacionamientoId = 0;
            if (Session["Dueño_EstId"] != null)
                estacionamientoId = (int)Session["Dueño_EstId"];
            else
                estacionamientoId = int.Parse(ddlEstacionamiento.SelectedValue);

            string nombreEstacionamiento = "";

            using (var db = new ProyectoEstacionamientoEntities())
            {
                // Obtener nombre del estacionamiento
                var est = db.Estacionamiento.Find(estacionamientoId);
                nombreEstacionamiento = est != null ? est.Est_nombre : "Desconocido";

                // 3. CONSULTA 1: COBROS NORMALES (Pago_Ocupacion)
                var cobrosNormales = db.Pago_Ocupacion
                    .Where(p => p.Est_id == estacionamientoId &&
                                p.Pago_Fecha >= desde &&
                                p.Pago_Fecha <= hasta)
                    .ToList()
                    .Select(p => new
                    {
                        Fecha = p.Pago_Fecha ?? DateTime.MinValue,
                        TipoCobro = p.Metodos_De_Pago.Metodo_pago_descripcion,
                        Monto = (decimal)p.Pago_Importe,
                        Categoria = "Normal",

                        Patente = p.Ocupacion.FirstOrDefault() != null
                                      ? p.Ocupacion.FirstOrDefault().Vehiculo_Patente
                                      : "-",

                        Vehiculo = (p.Ocupacion.FirstOrDefault() != null && p.Ocupacion.FirstOrDefault().Tarifa != null && p.Ocupacion.FirstOrDefault().Tarifa.Categoria_Vehiculo != null)
                                   ? p.Ocupacion.FirstOrDefault().Tarifa.Categoria_Vehiculo.Categoria_descripcion
                                   : "Varios",

                        Tarifa = (p.Ocupacion.FirstOrDefault() != null && p.Ocupacion.FirstOrDefault().Tarifa != null && p.Ocupacion.FirstOrDefault().Tarifa.Tipos_Tarifa != null)
                                 ? p.Ocupacion.FirstOrDefault().Tarifa.Tipos_Tarifa.Tipos_tarifa_descripcion
                                 : "General",

                        Playero = (p.Turno != null && p.Turno.Playero != null && p.Turno.Playero.Usuarios != null)
                                  ? p.Turno.Playero.Usuarios.Usu_nom + " " + p.Turno.Playero.Usuarios.Usu_ap
                                  : "Playero No Asignado",

                        Titular = ""
                    });

                // 4. CONSULTA 2: COBROS ABONOS (Pagos_Abonados)
                var cobrosAbonos = db.Pagos_Abonados
                    .Where(p => p.Est_id == estacionamientoId &&
                                p.Fecha_Pago >= desde &&
                                p.Fecha_Pago <= hasta)
                    .ToList()
                    .Select(p =>
                    {
                        // Obtenemos el Abono y su lista de Vehiculos_Abonados
                        var abono = p.Abono;
                        string patentesConcatenadas = "N/A";
                        string tipoVehiculo = "Mensual"; // Valor por defecto
                        string titularNombre = "";

                        if (abono != null && abono.Vehiculo_Abonado != null)
                        {
                            // 1. CONCATENACIÓN DE PATENTES
                            patentesConcatenadas = string.Join(", ",
                                abono.Vehiculo_Abonado
                                     .Select(va => va.Vehiculo_Patente)
                                     .ToList());

                            // Si no hay patentes, mostramos el nombre del abonado
                            if (string.IsNullOrEmpty(patentesConcatenadas))
                            {
                                patentesConcatenadas = p.Abono.Titular_Abono != null
                                            ? p.Abono.Titular_Abono.TAB_Nombre + " " + p.Abono.Titular_Abono.TAB_Apellido
                                            : "Abonado sin Patente";
                            }

                            // 2. OBTENER CATEGORÍA DEL VEHÍCULO
                            // Intentamos obtener la categoría del primer vehículo del abono.
                            var primerVehiculoAbonado = abono.Vehiculo_Abonado.FirstOrDefault();

                            if (primerVehiculoAbonado != null && primerVehiculoAbonado.Vehiculo != null && primerVehiculoAbonado.Vehiculo.Categoria_Vehiculo != null)
                            {
                                tipoVehiculo = primerVehiculoAbonado.Vehiculo.Categoria_Vehiculo.Categoria_descripcion;
                            }

                            titularNombre = abono.Titular_Abono.TAB_Nombre + " " + abono.Titular_Abono.TAB_Apellido;
                        }
                        else if (abono != null && abono.Titular_Abono != null)
                        {
                            // Si no hay vehículos asociados, mostramos el nombre del titular en el campo Patente
                            patentesConcatenadas = p.Abono.Titular_Abono.TAB_Nombre + " " + p.Abono.Titular_Abono.TAB_Apellido;

                            titularNombre = "Titular Desconocido";
                        }

                        return new
                        {
                            Fecha = p.Fecha_Pago,
                            TipoCobro = p.Acepta_Metodo_De_Pago.Metodos_De_Pago.Metodo_pago_descripcion,
                            Monto = (decimal)p.PA_Monto,
                            Categoria = "Abono",

                            Patente = patentesConcatenadas,

                            Vehiculo = tipoVehiculo,

                            Tarifa = (p.Tarifa != null && p.Tarifa.Tipos_Tarifa != null)
                                         ? p.Tarifa.Tipos_Tarifa.Tipos_tarifa_descripcion
                                         : "Tarifa Abono",

                            Playero = (p.Turno != null && p.Turno.Playero != null && p.Turno.Playero.Usuarios != null)
                                      ? p.Turno.Playero.Usuarios.Usu_nom + " " + p.Turno.Playero.Usuarios.Usu_ap
                                      : "Playero No Asignado",

                            Titular = titularNombre
                        };
                    });

                // 5. UNIFICAR LISTAS
                var listaTotal = cobrosNormales.Concat(cobrosAbonos).OrderBy(x => x.Fecha).ToList();

                if (!listaTotal.Any())
                {
                    lblMensaje.Text = "No se encontraron cobros.";
                    lblMensaje.Visible = true;
                    rvCobros.Visible = false;
                    return;
                }
                lblMensaje.Visible = false;

                // 6. LLENAR EL DATASET (Actualizar columnas)
                DataTable dt = new DataTable();
                dt.Columns.Add("Fecha", typeof(DateTime));
                dt.Columns.Add("TipoCobro", typeof(string));
                dt.Columns.Add("Monto", typeof(decimal));
                dt.Columns.Add("Categoria", typeof(string));

                dt.Columns.Add("Patente", typeof(string));
                dt.Columns.Add("Vehiculo", typeof(string));
                dt.Columns.Add("Tarifa", typeof(string));

                dt.Columns.Add("Playero", typeof(string));
                dt.Columns.Add("Titular", typeof(string));

                foreach (var item in listaTotal)
                {
                    dt.Rows.Add(
                        item.Fecha,
                        item.TipoCobro,
                        item.Monto,
                        item.Categoria,
                        item.Patente,
                        item.Vehiculo,
                        item.Tarifa,
                        item.Playero,
                        item.Titular
                    );
                }

                // 7. CONFIGURAR REPORTVIEWER
                rvCobros.LocalReport.ReportPath = Server.MapPath("~/Reportes/ReporteCobros.rdlc");
                rvCobros.LocalReport.DataSources.Clear();
                rvCobros.LocalReport.DataSources.Add(new ReportDataSource("DtCobros", dt));

                ReportParameter[] parametros = new ReportParameter[]
                {
            new ReportParameter("FechaDesde", desde.ToString("dd/MM/yyyy")),
            new ReportParameter("FechaHasta", hasta.ToString("dd/MM/yyyy")),
            new ReportParameter("NombreEstacionamiento", nombreEstacionamiento)
                };
                rvCobros.LocalReport.SetParameters(parametros);

                string fechaArchivo = DateTime.Now.ToString("yyyy-MM-dd");
                rvCobros.LocalReport.DisplayName = $"Reporte_Cobros_{nombreEstacionamiento}_{fechaArchivo}";

                rvCobros.Visible = true;
                rvCobros.LocalReport.Refresh();
            }
        }

    }
}