using System;
using System.Linq;
using System.Web.UI.WebControls;

namespace Proyecto_Estacionamiento.Pages.Tarifas
{
    public partial class Tarifa_Listar : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)    // Verifica si es la primera vez que se carga la página
            {
                string tipoUsuario = Session["Usu_tipo"] as string;
                if (tipoUsuario != "Dueño")
                {
                    // Oculta los elementos si no es Dueño
                    btnAgregarTarifa.Visible = false;
                    gvTarifas.Columns[0].Visible = false;
                }
                else
                {
                    if (Session["Dueño_EstId"] != null)
                    {
                        gvTarifas.Columns[0].Visible = false;
                    }
                }

                string estacionamiento = Session["Usu_estacionamiento"] as string;

                if (!string.IsNullOrEmpty(estacionamiento))
                {
                    TituloTarifas.Text = $"Tarifas del Estacionamiento '<strong>{estacionamiento}</strong>'";
                }
                else
                {
                    TituloTarifas.Text = "Tarifas";
                }

                CargarTarifas(); // Llama al método para cargar las Tarifas
            }
        }

        private void CargarTarifas()
        {
            string tipoUsuario = Session["Usu_tipo"] as string;
            int legajo = Convert.ToInt32(Session["Usu_legajo"]);

            using (var db = new ProyectoEstacionamientoEntities())
            {
                IQueryable<Tarifa> query = db.Tarifa;

                if (tipoUsuario == "Dueño")
                {
                    if (Session["Dueño_EstId"] != null)
                    {
                        // Dueño eligió un estacionamiento → solo ese
                        int estIdSeleccionado = (int)Session["Dueño_EstId"];
                        query = query.Where(t => t.Est_id.HasValue && t.Est_id.Value == estIdSeleccionado);
                    }
                    else
                    {
                        // No eligió → mostrar tarifas de todos sus estacionamientos
                        var estIdList = db.Estacionamiento
                                         .Where(e => e.Dueño_Legajo == legajo)
                                         .Select(e => e.Est_id);

                        query = query.Where(t => t.Est_id.HasValue && estIdList.Contains(t.Est_id.Value));
                    }
                }
                else if (tipoUsuario == "Playero")
                {
                    int estId = (int)Session["Playero_EstId"];
                    query = query.Where(t => t.Est_id.HasValue && t.Est_id.Value == estId);
                }

                var tarifas = query
                    .OrderBy(t => t.Estacionamiento.Est_nombre)
                    .Select(t => new
                    {
                        t.Tarifa_id,
                        Est_nombre = t.Estacionamiento.Est_nombre,
                        Tipos_Tarifa_Descripcion = t.Tipos_Tarifa.Tipos_tarifa_descripcion,
                        Categoria_descripcion = t.Categoria_Vehiculo.Categoria_descripcion,
                        t.Tarifa_Monto,
                        t.Tarifa_Desde
                    })
                    .ToList();

                gvTarifas.DataSource = tarifas;
                gvTarifas.DataBind();
            }
        }


        protected void btnAgregarTarifa_Click(object sender, EventArgs e)
        {
            Response.Redirect("Tarifa_Crear_Editar.aspx");
        }

        protected void gvTarifas_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Editar")
            {
                string tarifaId = e.CommandArgument.ToString();
                Response.Redirect($"Tarifa_Crear_Editar.aspx?id={tarifaId}");
            }

        }

        protected void gvTarifas_RowDataBound(object sender, GridViewRowEventArgs e)
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