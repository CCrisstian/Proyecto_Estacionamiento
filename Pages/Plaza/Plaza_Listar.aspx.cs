using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

namespace Proyecto_Estacionamiento.Pages.Plaza
{
    public partial class Plaza_Listar : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarPlazas(); // Cargamos las Plazas al cargar la página Plaza
                string tipoUsuario = Session["Usu_tipo"] as string;
                if (tipoUsuario != "Dueño")
                {
                    // Oculta los elementos si no es Dueño
                    btnAgregar.Visible = false;
                    gvPlazas.Columns[0].Visible = false;
                }
            }
        }

        private void CargarPlazas()
        {
            string tipoUsuario = Session["Usu_tipo"] as string;
            int legajo = Convert.ToInt32(Session["Usu_legajo"]);

            using (var db = new ProyectoEstacionamientoEntities())
            {
                List<Proyecto_Estacionamiento.Plaza> plazas = new List<Proyecto_Estacionamiento.Plaza>();


                if (tipoUsuario == "Dueño")
                {
                    // Obtener los Est_id asociados al legajo del Dueño
                    var estIds = db.Estacionamiento
                                   .Where(e => e.Dueño_Legajo == legajo)
                                   .Select(e => e.Est_id)
                                   .ToList();

                    plazas = db.Plaza
                        .Include("Categoria_Vehiculo")
                        .Include("Estacionamiento")
                        .Where(p => estIds.Contains(p.Est_id))
                        .OrderBy(p => p.Estacionamiento.Est_nombre)
                        .ToList();
                }
                else if (tipoUsuario == "Playero")
                {
                    // Obtener el Est_id asignado al Playero
                    var estId = db.Playero
                                  .Where(p => p.Playero_legajo == legajo)
                                  .Select(p => p.Est_id)
                                  .FirstOrDefault();

                    plazas = db.Plaza
                        .Include("Categoria_Vehiculo")
                        .Include("Estacionamiento")
                        .Where(p => p.Est_id == estId)
                        .OrderBy(p => p.Estacionamiento.Est_nombre)
                        .ToList();
                }

                // Mostrar las plazas si se encontraron
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
                // Redirigimos a la página de edición de Plaza con el ID de la Plaza seleccionada
            }
        }

        protected void gvPlazas_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                string tipoUsuario = Session["Usu_tipo"] as string;

                // Si no es "Dueño", ocultar el botón Editar
                if (tipoUsuario != "Dueño")
                {
                    Button btnEditar = (Button)e.Row.FindControl("btnEditar");
                    if (btnEditar != null)
                    {
                        btnEditar.Visible = false;
                    }
                }
            }
        }

    }
}