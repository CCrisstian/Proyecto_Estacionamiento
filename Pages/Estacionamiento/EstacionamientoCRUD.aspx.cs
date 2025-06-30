using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Proyecto_Estacionamiento;

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

        protected void gvEstacionamientos_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        protected void gvEstacionamientos_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "EditarCustom")
            {
                int index = Convert.ToInt32(e.CommandArgument);
                int id = Convert.ToInt32(gvEstacionamientos.DataKeys[index].Value);

                // Redirigir a la página de edición con el ID en la query string
                Response.Redirect($"Estacionamiento_Crear.aspx?id={id}");
            }
        }
        protected void gvEstacionamientos_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int id = Convert.ToInt32(gvEstacionamientos.DataKeys[e.RowIndex].Value);

            using (var db = new ProyectoEstacionamientoEntities())
            {
                var est = db.Estacionamiento.Find(id);
                if (est != null)
                {
                    db.Estacionamiento.Remove(est);
                    db.SaveChanges();
                }
            }

            CargarEstacionamientos(); // Refrescar grilla
        }


    }
}