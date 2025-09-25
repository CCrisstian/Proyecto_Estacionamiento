using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using static Proyecto_Estacionamiento.Pages.Metodos_De_Pago.MetodosDePago_Listar;

namespace Proyecto_Estacionamiento.Pages.Playeros
{
    public partial class Playero_Listar : System.Web.UI.Page
    {
        public class PlayeroDTO
        {
            public string Est_nombre { get; set; }
            public int Playero_legajo { get; set; }
            public int Usu_dni { get; set; }
            public string Usu_pass { get; set; }
            public string Usu_ap { get; set; }
            public string Usu_nom { get; set; }
            public bool Playero_activo { get; set; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Session["Dueño_EstId"] != null)
                {
                    gvPlayeros.Columns[0].Visible = false;
                }

                string estacionamiento = Session["Usu_estacionamiento"] as string;

                if (!string.IsNullOrEmpty(estacionamiento))
                {
                    Estacionamiento_Nombre.Text = $"Estacionamiento: '<strong>{estacionamiento}</strong>'";
                }
                else
                {
                    Estacionamiento_Nombre.Visible = false;
                }

                ddlCamposOrden.Items.Clear();
                ddlCamposOrden.Items.Add(new ListItem("Estacionamiento", "Est_nombre"));
                ddlCamposOrden.Items.Add(new ListItem("Legajo", "Playero_legajo"));
                ddlCamposOrden.Items.Add(new ListItem("DNI", "Usu_dni"));
                ddlCamposOrden.Items.Add(new ListItem("Apellido", "Usu_ap"));
                ddlCamposOrden.Items.Add(new ListItem("Nombre", "Usu_nom"));
                ddlCamposOrden.Items.Add(new ListItem("Activo", "Playero_activo"));

                CargarGrilla();
            }
        }

        private void CargarGrilla()
        {
            string tipoUsuario = Session["Usu_tipo"] as string;
            int legajo = Convert.ToInt32(Session["Usu_legajo"]);

            using (var db = new ProyectoEstacionamientoEntities())
            {
                IQueryable<Playero> query = db.Playero;

                if (Session["Dueño_EstId"] != null)
                {
                    int estIdSeleccionado = (int)Session["Dueño_EstId"];
                    query = query.Where(pl => pl.Est_id == estIdSeleccionado);
                }
                else
                {
                    var estIdList = db.Estacionamiento
                                       .Where(e => e.Dueño_Legajo == legajo)
                                       .Select(e => e.Est_id);

                    query = query.Where(pl => pl.Est_id.HasValue && estIdList.Contains(pl.Est_id.Value));
                }

                var playeros = query
                    .Select(p => new PlayeroDTO
                    {
                        Est_nombre = p.Estacionamiento.Est_nombre,
                        Playero_legajo = p.Playero_legajo,
                        Usu_dni = p.Usuarios.Usu_dni ?? 0,
                        Usu_pass = p.Usuarios.Usu_pass,
                        Usu_ap = p.Usuarios.Usu_ap,
                        Usu_nom = p.Usuarios.Usu_nom,
                        Playero_activo = p.Playero_activo
                    })
                    .OrderBy(p => p.Est_nombre)
                    .ToList();

                Session["DatosPlayeros"] = playeros;

                gvPlayeros.DataSource = playeros;
                gvPlayeros.DataBind();
            }
        }

        protected void btnOrdenar_Click(object sender, EventArgs e)
        {
            if (Session["DatosPlayeros"] != null)
            {
                var lista = Session["DatosPlayeros"] as List<PlayeroDTO>;
                string campo = ddlCamposOrden.SelectedValue;
                string direccion = ddlDireccionOrden.SelectedValue;

                if (direccion == "ASC")
                    lista = lista.OrderBy(x => GetPropertyValue(x, campo)).ToList();
                else
                    lista = lista.OrderByDescending(x => GetPropertyValue(x, campo)).ToList();

                gvPlayeros.DataSource = lista;
                gvPlayeros.DataBind();
            }
        }

        // Función de utilidad para acceder a propiedades por nombre
        private object GetPropertyValue(object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName).GetValue(obj, null);
        }


        protected void btnAgregarPlayero_Click(object sender, EventArgs e)
        {
            // Redirige al formulario de creación sin parámetro (alta)
            Response.Redirect("Playero_Crear_Editar.aspx");
        }

        protected void gvPlayeros_RowCommand(object sender, GridViewCommandEventArgs e)
        {

            if (e.CommandName == "Editar")
            {
                int index = Convert.ToInt32(e.CommandArgument);
                GridViewRow row = gvPlayeros.Rows[index];
                int playeroLegajo = Convert.ToInt32(row.Cells[1].Text); // Ajusta el índice si cambias el orden de columnas

                // Redirige al formulario de edición con el legajo como parámetro
                Response.Redirect($"Playero_Crear_Editar.aspx?legajo={playeroLegajo}");
            }
        }
    }
}