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
                CargarTarifas(); // Llama al método para cargar las Tarifas
                string tipoUsuario = Session["Usu_tipo"] as string;
                if (tipoUsuario != "Dueño")
                {
                    // Oculta los elementos si no es Dueño
                    btnAgregarTarifa.Visible = false;
                    gvTarifas.Columns[0].Visible = false;
                }
            }
        }

        private void CargarTarifas()
        {
            string tipoUsuario = Session["Usu_tipo"] as string;
            int legajo = Convert.ToInt32(Session["Usu_legajo"]);

            using (var db = new ProyectoEstacionamientoEntities())
            {
                // Filtro para que los Dueños y los Playeros vean solo sus Tarifas
                IQueryable<Tarifa> query = db.Tarifa;

                if (tipoUsuario == "Dueño")
                {
                    var estIdList = db.Estacionamiento
                                       .Where(e => e.Dueño_Legajo == legajo)
                                       .Select(e => e.Est_id);

                    query = query.Where(t => t.Est_id.HasValue && estIdList.Contains(t.Est_id.Value));
                }
                else if (tipoUsuario == "Playero")
                {
                    var estId = db.Playero
                                  .Where(p => p.Playero_legajo == legajo)
                                  .Select(p => p.Est_id)
                                  .FirstOrDefault();

                    if (estId.HasValue)
                    {
                        query = query.Where(t => t.Est_id == estId.Value);
                    }
                    else
                    {
                        // Por seguridad, que no devuelva nada si no se encuentra Est_id
                        query = query.Where(t => false);
                    }
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