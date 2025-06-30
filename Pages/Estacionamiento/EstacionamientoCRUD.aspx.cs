using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Proyecto_Estacionamiento;  // donde está definida la clase ProyectoEstacionamientoEntities

namespace Proyecto_Estacionamiento.Pages.Estacionamiento
{
    public partial class EstacionamientoCRUD : System.Web.UI.Page
    {
        private void CargarEstacionamientos()
        {
            using (var db = new ProyectoEstacionamientoEntities())
            {
                var lista = db.Estacionamiento.ToList();
                gvEstacionamientos.DataSource = lista;
                gvEstacionamientos.DataBind();
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarEstacionamientos();
            }
        }

        protected void btnCrear_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Pages/Estacionamiento/Estacionamiento_Crear.aspx");
        }
    }
}