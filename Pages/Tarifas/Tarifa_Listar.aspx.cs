using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

namespace Proyecto_Estacionamiento.Pages.Tarifas
{
    public partial class Tarifa_Listar : System.Web.UI.Page
    {
        // DTO para poder usarlo en sesión y ordenación
        public class TarifaDTO
        {
            public int Tarifa_id { get; set; }
            public string Est_nombre { get; set; }
            public string Tipos_Tarifa_Descripcion { get; set; }
            public string Categoria_descripcion { get; set; }
            public double Tarifa_Monto { get; set; }
            public DateTime Tarifa_Desde { get; set; }
        }

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

                // Campos disponibles para ordenar
                ddlCamposOrden.Items.Clear();
                ddlCamposOrden.Items.Add(new ListItem("Tipo de Tarifa", "Tipos_Tarifa_Descripcion"));
                ddlCamposOrden.Items.Add(new ListItem("Categoría", "Categoria_descripcion"));
                ddlCamposOrden.Items.Add(new ListItem("Monto", "Tarifa_Monto"));
                ddlCamposOrden.Items.Add(new ListItem("Vigente Desde", "Tarifa_Desde"));

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

                // Convertimos a DTO para guardar en sesión
                List<TarifaDTO> tarifasDTO = query
                    .OrderBy(t => t.Estacionamiento.Est_nombre)
                    .Select(t => new TarifaDTO
                    {
                        Tarifa_id = t.Tarifa_id,
                        Est_nombre = t.Estacionamiento.Est_nombre,
                        Tipos_Tarifa_Descripcion = t.Tipos_Tarifa.Tipos_tarifa_descripcion,
                        Categoria_descripcion = t.Categoria_Vehiculo.Categoria_descripcion,
                        Tarifa_Monto = t.Tarifa_Monto,
                        Tarifa_Desde = t.Tarifa_Desde
                    })
                    .ToList();

                Session["DatosTarifas"] = tarifasDTO;

                gvTarifas.DataSource = tarifasDTO;
                gvTarifas.DataBind();
            }
        }

        protected void btnOrdenar_Click(object sender, EventArgs e)
        {
            if (Session["DatosTarifas"] != null)
            {
                // Usamos var anónimo → Object por reflección
                var lista = (List<TarifaDTO>)Session["DatosTarifas"];
                string campo = ddlCamposOrden.SelectedValue;
                string direccion = ddlDireccionOrden.SelectedValue;

                if (direccion == "ASC")
                    lista = lista.OrderBy(x => GetPropertyValue(x, campo)).ToList();
                else
                    lista = lista.OrderByDescending(x => GetPropertyValue(x, campo)).ToList();

                gvTarifas.DataSource = lista;
                gvTarifas.DataBind();
            }
        }

        // Función de utilidad para acceder a propiedades por nombre
        private object GetPropertyValue(object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName).GetValue(obj, null);
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

                if (tipoUsuario != "Dueño")
                {
                    Button btnEditar = (Button)e.Row.FindControl("btnEditar");
                    if (btnEditar != null)
                        btnEditar.Visible = false;
                }
            }
        }
    }
}