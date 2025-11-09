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
        // Variables para almacenar los totales para el footer
        private int _totalDisponibles = 0;
        private int _totalOcupadas = 0;
        private int _totalGeneral = 0;

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
                if (tipoUsuario == "Playero")
                {
                    gvPlazas.Columns[2].Visible = false; // Ocultar columna de plazas ocupadas para Playero
                    gvPlazas.Columns[3].Visible = false; // Ocultar columna de total de plazas para Playero
                }

                CargarPlazas(); // Cargamos las Plazas al cargar la página Plaza
            }
        }

        public class PlazaInfoDTO
        {
            public string Categoria { get; set; }
            public int CantPlazasDisponibles { get; set; }

            public int CantPlazasOcupadas { get; set; }
            public int CantTotalPlazas { get; set; }
        }

        private void CargarPlazas()
        {
            string tipoUsuario = Session["Usu_tipo"] as string;
            int legajo = Convert.ToInt32(Session["Usu_legajo"]);

            using (var db = new ProyectoEstacionamientoEntities())
            {
                IQueryable<Proyecto_Estacionamiento.Plaza> query = db.Plaza
                                                                     .Include("Categoria_Vehiculo")
                                                                     .Include("Estacionamiento")
                                                                     .Include("Ocupacion"); // Incluir Ocupacion para la lógica

                if (tipoUsuario == "Dueño")
                {
                    // --- LÓGICA DEL DUEÑO (NUEVA) ---

                    // 1. Filtrar por sus estacionamientos (como antes)
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

                    // 2. Agrupar por categoría y calcular Disponibles, Ocupadas y Total
                    var plazasDTO = query
                        .GroupBy(p => p.Categoria_Vehiculo.Categoria_descripcion)
                        .Select(g => new PlazaInfoDTO
                        {
                            Categoria = g.Key,
                            // Disponible: Flag en true Y sin ocupación activa
                            CantPlazasDisponibles = g.Count(p => p.Plaza_Disponibilidad == true && !p.Ocupacion.Any(o => o.Ocu_fecha_Hora_Fin == null)),
                            // Ocupada: Flag en false (ej. abono, mantenimiento) O con ocupación activa
                            CantPlazasOcupadas = g.Count(p => p.Plaza_Disponibilidad == false || p.Ocupacion.Any(o => o.Ocu_fecha_Hora_Fin == null)),
                            // Total: Todas las plazas del grupo
                            CantTotalPlazas = g.Count()
                        })
                        .OrderBy(x => x.Categoria)
                        .ToList();

                    // 3. Calcular Totales Generales para el Footer
                    _totalDisponibles = plazasDTO.Sum(p => p.CantPlazasDisponibles);
                    _totalOcupadas = plazasDTO.Sum(p => p.CantPlazasOcupadas);
                    _totalGeneral = plazasDTO.Sum(p => p.CantTotalPlazas);

                    gvPlazas.DataSource = plazasDTO;
                    gvPlazas.DataBind();
                }
                else if (tipoUsuario == "Playero")
                {
                    // --- LÓGICA DEL PLAYERO (LA ORIGINAL) ---
                    int estId = (int)Session["Playero_EstId"];
                    query = query.Where(p => p.Est_id == estId);

                    var plazasDTO = query
                        .Where(p => p.Plaza_Disponibilidad == true
                                 && !p.Ocupacion.Any(o => o.Ocu_fecha_Hora_Fin == null))
                        .GroupBy(p => p.Categoria_Vehiculo.Categoria_descripcion)
                        .Select(g => new PlazaInfoDTO // Usa el DTO original
                        {
                            Categoria = g.Key,
                            CantPlazasDisponibles = g.Count()
                        })
                        .OrderBy(x => x.Categoria)
                        .ToList();

                    gvPlazas.DataSource = plazasDTO;
                    gvPlazas.DataBind();
                }

                Session["DatosPlazas"] = gvPlazas.DataSource; // Guardar los datos (DTO)
            }
        }

        protected void btnVolver(object sender, EventArgs e)
        {
            Response.Redirect("~/Pages/Plaza/Plaza_Listar.aspx");
        }
    }
}