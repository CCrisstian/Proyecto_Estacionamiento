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
                string tipoUsuario = Session["Usu_tipo"] as string;
                if (tipoUsuario != "Dueño")
                {
                    // Oculta los elementos si no es Dueño
                    btnAgregar.Visible = false;
                }
            }
        }

        private void CargarPlazas()
        {   // Obtenemos las Plazas desde la Base de Datos
            using (var db = new ProyectoEstacionamientoEntities())
            {
                var plazas = db.Plaza                   // Obtenemos las Plazas
                    .Include("Categoria_Vehiculo")      // Incluimos la relación con Categoria_Vehiculo
                    .Include("Estacionamiento")         // Incluimos la relación con Estacionamiento
                    .ToList();
                gvPlazas.DataSource = plazas;           // Asignamos la lista de Plazas como fuente de datos del GridView
                gvPlazas.DataBind();                    // Realizamos el enlace de datos al GridView
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