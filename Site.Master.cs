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
            if (!IsPostBack)
            {
                string tipoUsuario = Session["Usu_tipo"] as string;

                if (tipoUsuario != "Dueño")
                {
                    // Oculta los elementos si no es Dueño
                    menuEstacionamiento.Visible = false;
                    menuPlayero.Visible = false;
                }
            }
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