using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Proyecto_Estacionamiento.Pages.Metodos_De_Pago
{
    public partial class MetodosDePago_CrearEditar : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarEstacionamientos();
                CargarMetodosDePago();

                if (Request.QueryString["estId"] != null && Request.QueryString["metodoId"] != null)
                {
                    int estId = int.Parse(Request.QueryString["estId"]);
                    int metodoId = int.Parse(Request.QueryString["metodoId"]);
                    CargarDatosParaEditar(estId, metodoId);
                    lblTitulo.Text = "Editar Método de Pago";
                    btnGuardar.Text = "Actualizar";
                }
            }
        }

        private void CargarEstacionamientos()
        {
            int legajo = Convert.ToInt32(Session["Usu_legajo"]);

            using (var context = new ProyectoEstacionamientoEntities())
            {
                List<object> estacionamientos;

                if (Session["Dueño_EstId"] != null && Session["Usu_estacionamiento"] != null)
                {
                    // Ya hay un estacionamiento elegido → mostrar solo ese
                    int estIdSeleccionado = (int)Session["Dueño_EstId"];
                    string estNombre = Session["Usu_estacionamiento"].ToString();

                    estacionamientos = new List<object>
                    {
                        new { Est_id = estIdSeleccionado, Est_nombre = estNombre }
                    };

                    ddlEstacionamientos.Enabled = false;
                }
                else
                {
                    // Mostrar todos los estacionamientos disponibles del Dueño
                    estacionamientos = context.Estacionamiento
                        .Where(e => e.Dueño_Legajo == legajo && e.Est_Disponibilidad == true)
                        .Select(e => new { e.Est_id, e.Est_nombre })
                        .ToList<object>();
                }

                ddlEstacionamientos.DataSource = estacionamientos;
                ddlEstacionamientos.DataValueField = "Est_id";
                ddlEstacionamientos.DataTextField = "Est_nombre";
                ddlEstacionamientos.DataBind();
            }
        }


        private void CargarMetodosDePago()
        {
            using (var context = new ProyectoEstacionamientoEntities())
            {
                var metodos = context.Metodos_De_Pago
                    .Select(m => new { m.Metodo_pago_id, m.Metodo_pago_descripcion }).ToList();

                ddlMetodoDePago.DataSource = metodos;
                ddlMetodoDePago.DataValueField = "Metodo_pago_id";
                ddlMetodoDePago.DataTextField = "Metodo_pago_descripcion";
                ddlMetodoDePago.DataBind();
            }
        }

        private void CargarDatosParaEditar(int estId, int metodoId)
        {
            ddlEstacionamientos.SelectedValue = estId.ToString();
            ddlMetodoDePago.SelectedValue = metodoId.ToString();

            ddlEstacionamientos.Enabled = false;
            ddlMetodoDePago.Enabled = false;

            using (var context = new ProyectoEstacionamientoEntities())
            {
                var relacion = context.Acepta_Metodo_De_Pago
                    .FirstOrDefault(a => a.Est_id == estId && a.Metodo_Pago_id == metodoId);

                if (relacion != null)
                {
                    txtHasta.Text = relacion.AMP_Hasta?.ToString("dd-MM-yyyy") ?? "";
                }
            }
        }

        protected void cvFechaHasta_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = ValidarFechaHasta(args.Value);
        }

        private bool ValidarFechaHasta(string fechaTexto)
        {
            if (string.IsNullOrWhiteSpace(fechaTexto))
                return true; // Campo vacío permitido

            if (!DateTime.TryParseExact(fechaTexto, "dd-MM-yyyy",
                                        System.Globalization.CultureInfo.InvariantCulture,
                                        System.Globalization.DateTimeStyles.None,
                                        out DateTime fechaHasta))
            {
                return false; // Formato inválido
            }

            if (fechaHasta < DateTime.Now.Date)
                return false; // No puede ser menor que la fecha actual

            return true;
        }


        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            // Chequeamos el Validador
            if (!Page.IsValid)
                return;

            DateTime desde = DateTime.Now;
            DateTime? hasta = null;

            if (!string.IsNullOrWhiteSpace(txtHasta.Text))
                hasta = DateTime.ParseExact(txtHasta.Text, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture);

            int estId = int.Parse(ddlEstacionamientos.SelectedValue);
            int metodoId = int.Parse(ddlMetodoDePago.SelectedValue);

            using (var context = new ProyectoEstacionamientoEntities())
            {
                var existente = context.Acepta_Metodo_De_Pago
                    .FirstOrDefault(x => x.Est_id == estId && x.Metodo_Pago_id == metodoId);

                if (existente == null)
                {
                    var nuevo = new Acepta_Metodo_De_Pago
                    {
                        Est_id = estId,
                        Metodo_Pago_id = metodoId,
                        AMP_Desde = desde,
                        AMP_Hasta = hasta
                    };
                    context.Acepta_Metodo_De_Pago.Add(nuevo);
                }
                else
                {
                    existente.AMP_Desde = desde;
                    existente.AMP_Hasta = hasta;
                }

                context.SaveChanges();

                string accion = existente == null ? "agregado" : "editado";

                // variable exito se usa para mostrar mensaje de éxito (SweetAlert)
                Response.Redirect($"MetodosDePago_Listar.aspx?exito=1&accion={accion}");
            }
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            Response.Redirect("MetodosDePago_Listar.aspx");
        }

    }
}