using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Proyecto_Estacionamiento.Pages.Metodos_De_Pago
{
    public partial class MetodosDePago_Listar : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarGrilla();
            }
        }

        private void CargarGrilla()
        {
            using (var context = new ProyectoEstacionamientoEntities())
            {
                var datos = context.Acepta_Metodo_De_Pago
                    .Select(a => new
                    {
                        a.Estacionamiento.Est_nombre,
                        a.Metodos_De_Pago.Metodo_pago_descripcion,
                        a.AMP_Desde,
                        a.AMP_Hasta,
                        a.Est_id,
                        a.Metodo_Pago_id
                    })
                    .ToList();
                gvMetodosPago.DataSource = datos;
                gvMetodosPago.DataBind();
            }
        }

        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            Response.Redirect("AgregarMetodoPago.aspx");
        }

        protected void gvMetodosPago_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Editar")
            {
                int rowIndex = Convert.ToInt32(e.CommandArgument);
                GridViewRow row = gvMetodosPago.Rows[rowIndex];

                int estId = Convert.ToInt32(gvMetodosPago.DataKeys[rowIndex]["Est_id"]);
                int metodoId = Convert.ToInt32(gvMetodosPago.DataKeys[rowIndex]["Metodo_Pago_id"]);

                Response.Redirect($"AgregarMetodoPago.aspx?estId={estId}&metodoId={metodoId}");
            }
        }

    }
}