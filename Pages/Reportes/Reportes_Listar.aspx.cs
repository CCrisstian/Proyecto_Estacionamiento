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

namespace Proyecto_Estacionamiento.Pages.Reportes
{
    public partial class Reportes_Listar : System.Web.UI.Page
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
                    btnReporte.Visible = false;
                }
                else
                {
                    gvReportes.Visible = false;
                }

                CargarReportes();
            }
        }

        // DTO para poblar la grilla
        public class ReporteDTO
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

        private void CargarReportes()
        {

            int legajo = Convert.ToInt32(Session["Usu_legajo"]);

            using (var db = new ProyectoEstacionamientoEntities()) // Usa el nombre de tu Contexto EF
            {
                // Consulta base sobre Incidencias, incluyendo al Playero y sus datos de Usuario
                IQueryable<Incidencias> query = db.Incidencias
                                                 .Include("Playero")
                                                 .Include("Playero.Usuarios");

                // TAREA: Filtrar por Estacionamiento
                if (Session["Dueño_EstId"] != null)
                {
                    // Caso 1: Dueño seleccionó un Estacionamiento
                    int estIdSeleccionado = (int)Session["Dueño_EstId"];
                    // Filtra incidencias donde el Est_id del Playero coincida
                    query = query.Where(i => i.Playero.Est_id == estIdSeleccionado);
                }
                else
                {
                    // Caso 2: Dueño no seleccionó -> Mostrar todos sus estacionamientos
                    var estIdsDelDueño = db.Estacionamiento
                                           .Where(e => e.Dueño_Legajo == legajo)
                                           .Select(e => e.Est_id);

                    // Filtra incidencias donde el Est_id del Playero esté en la lista de Estacionamientos del Dueño
                    query = query.Where(i => i.Playero.Est_id.HasValue && estIdsDelDueño.Contains(i.Playero.Est_id.Value));
                }

                var reportesDTO = query
                    .OrderBy(i => i.Playero.Usuarios.Usu_ap) // Ordena por Apellido de Playero
                    .ThenByDescending(i => i.Inci_fecha_Hora) // Luego por fecha (más nuevas primero)
                    .ToList() // Trae los datos a memoria
                    .Select(i => new ReporteDTO // Proyecta al DTO
                    {
                        Playero_legajo = i.Playero_legajo,
                        Inci_fecha_Hora = i.Inci_fecha_Hora,
                        PlayeroNombre = $"{i.Playero.Usuarios.Usu_nom}, {i.Playero.Usuarios.Usu_ap}",
                        FechaHoraStr = i.Inci_fecha_Hora.ToString("dd/MM/yyyy HH:mm"),
                        Inci_Motivo = i.Inci_Motivo,
                        Inci_Estado = i.Inci_Estado,
                        EstadoStr = i.Inci_Estado ? "Resuelto" : "Pendiente", // Convierte Bit a Texto
                        Inci_descripcion = i.Inci_descripcion,
                        DownloadUrl = $"Reporte_Descargar.aspx?legajo={i.Playero_legajo}&fechaTicks={i.Inci_fecha_Hora.Ticks}"
                    })
                    .ToList();

                gvReportes.DataSource = reportesDTO;
                // Usar PK compuesta para DataKeyNames
                gvReportes.DataKeyNames = new string[] { "Playero_legajo", "Inci_fecha_Hora" };
                gvReportes.DataBind();
            }
        }

        protected void btnReporte_Click(object sender, EventArgs e)
        {
            Response.Redirect("Reporte_Registrar.aspx");
        }


        protected void gvReportes_RowCreated(object sender, GridViewRowEventArgs e)
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