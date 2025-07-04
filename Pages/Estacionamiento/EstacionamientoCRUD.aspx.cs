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
            // Cargar los Estacionamientos desde la Base de Datos y enlazarlos a la grilla
            using (var db = new ProyectoEstacionamientoEntities())
            {
                var lista = db.Estacionamiento.ToList();
                gvEstacionamientos.DataSource = lista;
                gvEstacionamientos.DataBind();
            }
        }

        // Evento que se ejecuta al cargar la página
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)    // Verificar si es la primera carga de la página
            {
                CargarEstacionamientos(); // Llamar al método para cargar los estacionamientos
            }
        }

        protected void btnCrear_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Pages/Estacionamiento/Estacionamiento_Crear.aspx");
        }

        // Evento para manejar la selección de un Estacionamiento desde la grilla
        protected void gvEstacionamientos_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        // Evento para manejar la Edición o Eliminación de un Estacionamiento desde la grilla
        protected void gvEstacionamientos_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            // Eliminar Estacionamiento
            if (e.CommandName == "EliminarCustom")
            {
                int id = Convert.ToInt32(e.CommandArgument);

                using (var db = new ProyectoEstacionamientoEntities())
                {
                    var est = db.Estacionamiento.FirstOrDefault(x => x.Est_id == id);
                    if (est != null)
                    {
                        db.Estacionamiento.Remove(est);
                        db.SaveChanges();
                    }
                }

                CargarEstacionamientos(); // Refrescar grilla
            }

            // Redirigir a editar
            if (e.CommandName == "EditarCustom")
            {
                int index = Convert.ToInt32(e.CommandArgument);
                int est_id = Convert.ToInt32(gvEstacionamientos.DataKeys[index].Value);
                Response.Redirect($"Estacionamiento_Crear.aspx?edit={est_id}");
            }
        }




    }
}