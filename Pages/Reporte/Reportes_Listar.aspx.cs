using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Proyecto_Estacionamiento.Reporte
{
    public partial class Reportes_Listar : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Proteger acceso a páginas
            if (!User.Identity.IsAuthenticated) { Response.Redirect("~Pages/Login/Login.aspx"); }

            string estacionamiento = Session["Usu_estacionamiento"] as string;

            if (!string.IsNullOrEmpty(estacionamiento))
            {
                Estacionamiento_Nombre.Text = $"Estacionamiento: '<strong>{estacionamiento}</strong>'";
            }
            else
            {
                Estacionamiento_Nombre.Visible = false;
            }
        }

        protected void btnReporteIngresos(object sender, EventArgs e)
        {
            Response.Redirect("~/Pages/Ingresos/Ingreso_Reporte.aspx");
        }

        protected void btnReporteIncidencias(object sender, EventArgs e)
        {
            Response.Redirect("~/Pages/Incidencia/Incidencias_Listar.aspx");
        }

        protected void btnReporteCobros(object sender, EventArgs e)
        {
            Response.Redirect("~/Pages/Reporte/Reporte_Cobros.aspx");
        }
    }
}