using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Proyecto_Estacionamiento.Pages.Plaza
{
    public partial class Plaza_CRUD : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarPlazas(); // Cargamos las Plazas al cargar la página Plaza
            }
        }

        private void CargarPlazas()
        {   // Obtenemos las Plazas desde la Base de Datos
            using (var db = new ProyectoEstacionamientoEntities())
            {
                var plazas = db.Plaza
                    .Include("Categoria_Vehiculo")
                    .Include("Estacionamiento")
                    .ToList();
                gvPlazas.DataSource = plazas;
                gvPlazas.DataBind();
            }
        }

        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Pages/Plaza/Plaza_Crear_Editar.aspx");
        }

        protected void gvPlazas_RowCommand(object sender, System.Web.UI.WebControls.GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Editar")
            {
                int plazaId = Convert.ToInt32(e.CommandArgument);
                Response.Redirect("~/Pages/Plaza/Plaza_Crear_Editar.aspx?id=" + plazaId);
            }
        }
    }
}