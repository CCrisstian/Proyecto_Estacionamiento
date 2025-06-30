using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Proyecto_Estacionamiento
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Proteger acceso a páginas
            if (!User.Identity.IsAuthenticated){Response.Redirect("~Pages/Login/Login.aspx");}
        }
    }
}