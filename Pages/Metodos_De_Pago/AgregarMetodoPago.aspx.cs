using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Proyecto_Estacionamiento.Pages.Metodos_De_Pago
{
    public partial class AgregarMetodoPago : System.Web.UI.Page
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
                }
                else
                {
                    lblTitulo.Text = "Agregar Método de Pago";
                }
            }
        }

        private void CargarEstacionamientos()
        {
            using (var context = new ProyectoEstacionamientoEntities())
            {
                var estacionamientos = context.Estacionamiento
                    .Select(e => new { e.Est_id, e.Est_nombre }).ToList();

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
                    txtDesde.Text = relacion.AMP_Desde?.ToString("yyyy-MM-dd") ?? "";
                    txtHasta.Text = relacion.AMP_Hasta?.ToString("yyyy-MM-dd") ?? "";
                }
            }
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
                return;

            DateTime desde = DateTime.Parse(txtDesde.Text);
            DateTime hasta = DateTime.Parse(txtHasta.Text);

            if (desde > hasta)
            {
                lblError.Text = "La fecha 'Desde' no puede ser mayor que la fecha 'Hasta'.";
                lblError.Visible = true;
                return;
            }

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
                Response.Redirect("MetodosDePago_Listar.aspx");
            }
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            Response.Redirect("MetodosDePago_Listar.aspx");
        }

    }
}