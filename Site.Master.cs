using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Proyecto_Estacionamiento
{
    public partial class SiteMaster : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btnLogout_Click(object sender, EventArgs e)
        {
            // Cerrar sesión de FormsAuthentication
            FormsAuthentication.SignOut();

            // Limpiar la sesión
            Session.Clear();

            // Redirigir al login
            Response.Redirect("~/Pages/Login/Login.aspx");
        }

    }
}