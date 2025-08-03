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

                string currentUrl = Request.Url.AbsolutePath.ToLower();

                if (currentUrl.Contains("/default/"))
                {
                    menuInicio.Attributes["class"] = "tab active";
                }
                else if (currentUrl.Contains("/estacionamiento/"))
                {
                    menuEstacionamiento.Attributes["class"] = "tab active";
                }
                else if (currentUrl.Contains("/plaza/"))
                {
                    menuPlaza.Attributes["class"] = "tab active";
                }
                else if (currentUrl.Contains("/tarifas/"))
                {
                    menuTarifa.Attributes["class"] = "tab active";
                }
                else if (currentUrl.Contains("/metodos_de_pago/"))
                {
                    menuMetodosDePago.Attributes["class"] = "tab active";
                }
                else if (currentUrl.Contains("/playeros/"))
                {
                    menuPlayero.Attributes["class"] = "tab active";
                }
                else if (currentUrl.Contains("/turnos/"))
                {
                    menuTurno.Attributes["class"] = "tab active";
                }
                else if (currentUrl.Contains("/abonados/"))
                {
                    menuAbonado.Attributes["class"] = "tab active";
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