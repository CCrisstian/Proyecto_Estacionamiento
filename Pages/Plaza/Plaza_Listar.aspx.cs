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
                string tipoUsuario = Session["Usu_tipo"] as string;
                if (tipoUsuario != "Dueño")
                {
                    // Oculta los elementos si no es Dueño
                    btnAgregar.Visible = false;
                    gvPlazas.Columns[0].Visible = false;
                }
                else
                {
                    if (Session["Dueño_EstId"] != null)
                    {
                        gvPlazas.Columns[0].Visible = false;
                    }
                }

                string estacionamiento = Session["Usu_estacionamiento"] as string;

                if (!string.IsNullOrEmpty(estacionamiento))
                {
                    TituloPlazas.Text = $"Plazas del Estacionamiento '<strong>{estacionamiento}</strong>'";
                }
                else
                {
                    TituloPlazas.Text = "Plazas";
                }

                // Campos disponibles para ordenar
                ddlCamposOrden.Items.Clear();
                ddlCamposOrden.Items.Add(new ListItem("Nombre", "Plaza_Nombre"));
                ddlCamposOrden.Items.Add(new ListItem("Tipo", "Plaza_Tipo"));
                ddlCamposOrden.Items.Add(new ListItem("Categoría", "Categoria_id")); // si querés mostrar nombre, se puede mapear
                ddlCamposOrden.Items.Add(new ListItem("Disponible", "Plaza_Disponibilidad"));

                CargarPlazas(); // Cargamos las Plazas al cargar la página Plaza
            }
        }

        protected void btnOrdenar_Click(object sender, EventArgs e)
        {
            if (Session["DatosPlazas"] != null)
            {
                var lista = (List<Proyecto_Estacionamiento.Plaza>)Session["DatosPlazas"];
                string campo = ddlCamposOrden.SelectedValue;
                string direccion = ddlDireccionOrden.SelectedValue;

                if (direccion == "ASC")
                {
                    lista = lista.OrderBy(x => GetPropertyValue(x, campo)).ToList();
                }
                else
                {
                    lista = lista.OrderByDescending(x => GetPropertyValue(x, campo)).ToList();
                }

                gvPlazas.DataSource = lista;
                gvPlazas.DataBind();
            }
        }

        private object GetPropertyValue(object obj, string propertyName)
        {
            var prop = obj.GetType().GetProperty(propertyName);
            if (prop == null) return null;
            return prop.GetValue(obj, null);
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
                    if (Session["Dueño_EstId"] != null)
                    {
                        // Dueño eligió un estacionamiento
                        int estIdSeleccionado = (int)Session["Dueño_EstId"];
                        plazas = db.Plaza
                                   .Include("Categoria_Vehiculo")
                                   .Include("Estacionamiento")
                                   .Where(p => p.Est_id == estIdSeleccionado)
                                   .OrderBy(p => p.Estacionamiento.Est_nombre)
                                   .ToList();
                    }
                    else
                    {
                        // No eligió → mostramos todas las plazas de sus estacionamientos
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
                }
                else if (tipoUsuario == "Playero")
                {
                    int estId = (int)Session["Playero_EstId"];
                    plazas = db.Plaza
                               .Include("Categoria_Vehiculo")
                               .Include("Estacionamiento")
                               .Where(p => p.Est_id == estId)
                               .OrderBy(p => p.Estacionamiento.Est_nombre)
                               .ToList();
                }

                gvPlazas.DataSource = plazas;
                gvPlazas.DataBind();

                // Guardamos para el orden dinámico
                Session["DatosPlazas"] = plazas; // plazas es List<Proyecto_Estacionamiento.Plaza>
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