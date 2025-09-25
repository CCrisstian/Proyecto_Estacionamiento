using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Proyecto_Estacionamiento.Pages.Plaza
{
    public partial class Plaza_Info : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {

                string estacionamiento = Session["Usu_estacionamiento"] as string;

                if (!string.IsNullOrEmpty(estacionamiento))
                {
                    Estacionamiento_Nombre.Text = $"Estacionamiento: '<strong>{estacionamiento}</strong>'";
                }
                else
                {
                    Estacionamiento_Nombre.Visible = false;
                }

                CargarPlazas(); // Cargamos las Plazas al cargar la página Plaza
            }
        }

        public class PlazasDisponiblesDTO
        {
            public string Categoria { get; set; }
            public int CantPlazasDisponibles { get; set; }
        }

        private void CargarPlazas()
        {
            string tipoUsuario = Session["Usu_tipo"] as string;
            int legajo = Convert.ToInt32(Session["Usu_legajo"]);

            using (var db = new ProyectoEstacionamientoEntities())
            {
                IQueryable<Proyecto_Estacionamiento.Plaza> query = db.Plaza
                                                                     .Include("Categoria_Vehiculo")
                                                                     .Include("Estacionamiento");

                if (tipoUsuario == "Dueño")
                {
                    if (Session["Dueño_EstId"] != null)
                    {
                        int estIdSeleccionado = (int)Session["Dueño_EstId"];
                        query = query.Where(p => p.Est_id == estIdSeleccionado);
                    }
                    else
                    {
                        var estIds = db.Estacionamiento
                                       .Where(e => e.Dueño_Legajo == legajo)
                                       .Select(e => e.Est_id)
                                       .ToList();

                        query = query.Where(p => estIds.Contains(p.Est_id));
                    }
                }
                else if (tipoUsuario == "Playero")
                {
                    int estId = (int)Session["Playero_EstId"];
                    query = query.Where(p => p.Est_id == estId);
                }

                // 🔹 Agrupar por categoría y contar plazas disponibles
                var plazasDTO = query
                    .Where(p => p.Plaza_Disponibilidad == true
                             && !p.Ocupacion.Any(o => o.Ocu_fecha_Hora_Fin == null)) // libre si no tiene ocupación activa
                    .GroupBy(p => p.Categoria_Vehiculo.Categoria_descripcion)
                    .Select(g => new PlazasDisponiblesDTO
                    {
                        Categoria = g.Key,
                        CantPlazasDisponibles = g.Count()
                    })
                    .OrderBy(x => x.Categoria)
                    .ToList();

                gvPlazas.DataSource = plazasDTO;
                gvPlazas.DataBind();

                Session["DatosPlazas"] = plazasDTO;
            }
        }

        protected void btnVolver(object sender, EventArgs e)
        {
            Response.Redirect("~/Pages/Plaza/Plaza_Listar.aspx");
        }
    }
}