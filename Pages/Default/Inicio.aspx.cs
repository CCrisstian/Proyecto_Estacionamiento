using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static Proyecto_Estacionamiento.Pages.Login.Login;

namespace Proyecto_Estacionamiento.Pages.Default
{
    public partial class Inicio : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                int legajoDueño = (int)Session["Usu_legajo"];
                string nombreQuery = Request.QueryString["nombre"];
                int estIdQuery = 0;
                int.TryParse(Request.QueryString["estId"], out estIdQuery);

                using (var db = new ProyectoEstacionamientoEntities())
                {
                    List<EstacionamientoDTO> estacionamientos;

                    if (!string.IsNullOrEmpty(nombreQuery) && estIdQuery > 0)
                    {
                        // Caso primer estacionamiento recién creado: mostrar solo ese
                        estacionamientos = new List<EstacionamientoDTO>
                {
                    new EstacionamientoDTO { Est_id = estIdQuery, Est_nombre = nombreQuery }
                };

                        Session["Usu_estacionamiento"] = nombreQuery;
                        Session["Dueño_EstId"] = estIdQuery;
                        Session["EstacionamientosDueño"] = estacionamientos;
                    }
                    else
                    {
                        // Cargar todos los estacionamientos del dueño
                        estacionamientos = db.Estacionamiento
                            .Where(est => est.Dueño_Legajo == legajoDueño)
                            .Select(est2 => new EstacionamientoDTO
                            {
                                Est_id = est2.Est_id,
                                Est_nombre = est2.Est_nombre
                            })
                            .ToList();

                        Session["EstacionamientosDueño"] = estacionamientos;
                    }

                    gvEstacionamientos.DataSource = estacionamientos;
                    gvEstacionamientos.DataBind();

                    // Mostrar el Estacionamiento seleccionado
                    string estacionamiento = Session["Usu_estacionamiento"] as string;
                    Estacionamiento_Nombre.Text = $"Estacionamiento: <strong>{estacionamiento}</strong>";
                }
            }
        }


        protected void gvEstacionamientos_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                int estId = Convert.ToInt32(DataBinder.Eval(e.Row.DataItem, "Est_id"));

                if (Session["Dueño_EstId"] != null && estId == (int)Session["Dueño_EstId"])
                {
                    e.Row.BackColor = System.Drawing.Color.LightBlue;
                    e.Row.Font.Bold = true;
                }
            }
        }

        protected void gvEstacionamientos_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Seleccionar")
            {
                int rowIndex = ((GridViewRow)((Button)e.CommandSource).NamingContainer).RowIndex;

                // Obtener DataKeys directamente
                int estId = Convert.ToInt32(gvEstacionamientos.DataKeys[rowIndex]["Est_id"]);
                string estNombre = gvEstacionamientos.DataKeys[rowIndex]["Est_nombre"].ToString();

                // Guardar en sesión
                Session["Dueño_EstId"] = estId;
                Session["Usu_estacionamiento"] = estNombre;

                // Redirigir pasando el nombre del estacionamiento como QueryString
                Response.Redirect($"Inicio.aspx?estSeleccionado={Server.UrlEncode(estNombre)}");
            }
        }
    }
}