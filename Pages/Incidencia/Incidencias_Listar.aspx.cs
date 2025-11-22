using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO; // Necesario para MemoryStream
using System.Data.Entity;

namespace Proyecto_Estacionamiento.Pages.Incidencia
{
    public partial class Incidencias_Listar : System.Web.UI.Page
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

                string tipoUsuario = Session["Usu_tipo"] as string;
                int legajo = Convert.ToInt32(Session["Usu_legajo"]);

                if (tipoUsuario != "Playero")
                {
                    btnIncidencia.Visible = false;
                }

                CargarIncidencias();
            }
        }

        // DTO para poblar la grilla
        public class IncidenciaDTO
        {
            public int Playero_legajo { get; set; }
            public DateTime Inci_fecha_Hora { get; set; }
            public string PlayeroNombre { get; set; }
            public string FechaHoraStr { get; set; }
            public string Inci_Motivo { get; set; }
            public bool Inci_Estado { get; set; }
            public string EstadoStr { get; set; }
            public string Inci_descripcion { get; set; }
            public string DownloadUrl { get; set; }
        }

        private void CargarIncidencias()
        {
            // 1. Obtenemos el tipo de usuario y el legajo
            string tipoUsuario = Session["Usu_tipo"] as string;
            int legajo = Convert.ToInt32(Session["Usu_legajo"]);

            using (var db = new ProyectoEstacionamientoEntities())
            {
                // Consulta base
                IQueryable<Incidencias> query = db.Incidencias
                                                 .Include("Playero")
                                                 .Include("Playero.Usuarios");

                // 2. Aplicamos el filtro según el Rol
                if (tipoUsuario == "Dueño")
                {
                    // --- Lógica del Dueño ---
                    if (Session["Dueño_EstId"] != null)
                    {
                        int estIdSeleccionado = (int)Session["Dueño_EstId"];
                        query = query.Where(i => i.Playero.Est_id == estIdSeleccionado);
                    }
                    else
                    {
                        var estIdsDelDueño = db.Estacionamiento
                                               .Where(e => e.Dueño_Legajo == legajo)
                                               .Select(e => e.Est_id);

                        query = query.Where(i => i.Playero.Est_id.HasValue && estIdsDelDueño.Contains(i.Playero.Est_id.Value));
                    }
                }
                else if (tipoUsuario == "Playero")
                {
                    // --- Lógica del Playero ---
                    // El playero solo ve las incidencias donde SU legajo sea el creador
                    query = query.Where(i => i.Playero_legajo == legajo);
                }

                // 3. Proyección y Ordenamiento (Se mantiene igual)
                var incidenciasDTO = query
                    .OrderBy(i => i.Playero.Usuarios.Usu_ap)
                    .ThenByDescending(i => i.Inci_fecha_Hora)
                    .ToList()
                    .Select(i => new IncidenciaDTO
                    {
                        Playero_legajo = i.Playero_legajo,
                        Inci_fecha_Hora = i.Inci_fecha_Hora,
                        PlayeroNombre = $"{i.Playero.Usuarios.Usu_nom}, {i.Playero.Usuarios.Usu_ap}",
                        FechaHoraStr = i.Inci_fecha_Hora.ToString("dd/MM/yyyy HH:mm"),
                        Inci_Motivo = i.Inci_Motivo,
                        Inci_Estado = i.Inci_Estado,
                        EstadoStr = i.Inci_Estado ? "Resuelto" : "Pendiente",
                        Inci_descripcion = i.Inci_descripcion,
                        DownloadUrl = $"Incidencia_Descargar.aspx?legajo={i.Playero_legajo}&fechaTicks={i.Inci_fecha_Hora.Ticks}"
                    })
                    .ToList();

                gvIncidencias.DataSource = incidenciasDTO;
                gvIncidencias.DataKeyNames = new string[] { "Playero_legajo", "Inci_fecha_Hora" };
                gvIncidencias.DataBind();
            }
        }

        protected void btnIncidencia_Click(object sender, EventArgs e)
        {
            Response.Redirect("Incidencia_Registrar.aspx");
        }


        protected void gvIncidencias_RowCreated(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // Encontrar el LinkButton de descarga
                LinkButton btnDescargar = e.Row.FindControl("btnDescargar") as LinkButton;

                if (btnDescargar != null)
                {
                    // Obtener el ScriptManager (que está en el Site.Master)
                    ScriptManager sm = ScriptManager.GetCurrent(this.Page);

                    if (sm != null)
                    {
                        // Registrar el botón para que haga un PostBack completo (no AJAX)
                        sm.RegisterPostBackControl(btnDescargar);
                    }
                }
            }
        }
    }
}