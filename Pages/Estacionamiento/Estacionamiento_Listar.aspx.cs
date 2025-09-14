using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

namespace Proyecto_Estacionamiento.Pages.Estacionamiento
{
    public partial class Estacionamiento_Listar : System.Web.UI.Page
    {
        // Evento que se ejecuta al cargar la página
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)    // Verificar si es la primera carga de la página
            {
                CargarEstacionamientos(); // Llamar al método para cargar los Estacionamientos

                if (Session["Dueño_EstId"] != null)
                {
                    gvEstacionamientos.Columns[0].Visible = false;
                }

                string estacionamiento = Session["Usu_estacionamiento"] as string;
                Estacionamiento_Nombre.Text = $"Estacionamiento: <strong>{estacionamiento}</strong>";
            }
        }

        private void CargarEstacionamientos()
        {
            string tipoUsuario = Session["Usu_tipo"] as string;
            int legajo = Convert.ToInt32(Session["Usu_legajo"]);

            using (var db = new ProyectoEstacionamientoEntities())
            {
                List<Proyecto_Estacionamiento.Estacionamiento> estacionamientos = new List<Proyecto_Estacionamiento.Estacionamiento>();

                if (tipoUsuario == "Dueño")
                {
                    if (Session["Dueño_EstId"] != null)
                    {
                        // Caso 1: Dueño eligió un estacionamiento
                        int estIdSeleccionado = (int)Session["Dueño_EstId"];
                        estacionamientos = db.Estacionamiento
                                             .Where(e => e.Est_id == estIdSeleccionado)
                                             .OrderBy(e => e.Est_provincia)
                                             .ToList();
                    }
                    else
                    {
                        // Caso 2: No eligió → mostrar todos sus estacionamientos
                        var estIds = db.Estacionamiento
                                       .Where(e => e.Dueño_Legajo == legajo)
                                       .Select(e => e.Est_id)
                                       .ToList();

                        estacionamientos = db.Estacionamiento
                                             .Where(e => estIds.Contains(e.Est_id))
                                             .OrderBy(e => e.Est_provincia)
                                             .ToList();
                    }
                }

                gvEstacionamientos.DataSource = estacionamientos;
                gvEstacionamientos.DataBind();
            }
        }


        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Pages/Estacionamiento/Estacionamiento_CrearEditar.aspx");
        }

        // Evento para manejar la Edición de un Estacionamiento desde la grilla
        protected void gvEstacionamientos_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "EditarCustom")
            {
                int index = Convert.ToInt32(e.CommandArgument);
                int est_id = Convert.ToInt32(gvEstacionamientos.DataKeys[index].Value);
                Response.Redirect($"Estacionamiento_CrearEditar.aspx?Est_id={est_id}");
            }
        }

    }
}