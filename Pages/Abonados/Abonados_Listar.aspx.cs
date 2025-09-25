using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Proyecto_Estacionamiento.Pages.Abonados
{
    public partial class Abonados_Listar : System.Web.UI.Page
    {
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
        }
    }
}