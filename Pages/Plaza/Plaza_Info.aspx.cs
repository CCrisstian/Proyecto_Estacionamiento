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
                    gvPlazas.Columns[0].Visible = false; // Ocultar columna de estacionamiento
                }
                else
                {
                    Estacionamiento_Nombre.Visible = false;
                }

                string tipoUsuario = Session["Usu_tipo"] as string;

                int legajo = Convert.ToInt32(Session["Usu_legajo"]);
                if (tipoUsuario == "Playero")
                {
                    gvPlazas.Columns[0].Visible = false; // Ocultar columna de estacionamiento para Playero
                    gvPlazas.Columns[2].Visible = false; // Ocultar columna de plazas ocupadas para Playero
                    gvPlazas.Columns[3].Visible = false; // Ocultar columna de total de plazas para Playero
                }

                CargarPlazas(); // Cargamos las Plazas al cargar la página Plaza
            }
        }

        public class PlazaInfoDTO
        {
            public string Estacionamiento { get; set; }
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
                                                                      .Include("Ocupacion");

                if (tipoUsuario == "Dueño")
                {
                    List<PlazaInfoDTO> plazasDTO;

                    if (Session["Dueño_EstId"] != null)
                    {
                        // --- CASO 1: DUEÑO SELECCIONÓ UN ESTACIONAMIENTO (Lógica casi igual) ---
                        int estIdSeleccionado = (int)Session["Dueño_EstId"];
                        query = query.Where(p => p.Est_id == estIdSeleccionado);

                        plazasDTO = query
                            .GroupBy(p => p.Categoria_Vehiculo.Categoria_descripcion)
                            .Select(g => new PlazaInfoDTO
                            {
                                // Estacionamiento es el mismo para todos en este grupo
                                Estacionamiento = g.FirstOrDefault().Estacionamiento.Est_nombre,
                                Categoria = g.Key,
                                CantPlazasDisponibles = g.Count(p => p.Plaza_Disponibilidad == true && !p.Ocupacion.Any(o => o.Ocu_fecha_Hora_Fin == null)),
                                CantPlazasOcupadas = g.Count(p => p.Plaza_Disponibilidad == false || p.Ocupacion.Any(o => o.Ocu_fecha_Hora_Fin == null)),
                                CantTotalPlazas = g.Count()
                            })
                            .OrderBy(x => x.Categoria)
                            .ToList();
                    }
                    else
                    {
                        // --- CASO 2: DUEÑO NO SELECCIONÓ (NUEVA LÓGICA) ---
                        var estIds = db.Estacionamiento
                                       .Where(e => e.Dueño_Legajo == legajo)
                                       .Select(e => e.Est_id)
                                       .ToList();
                        query = query.Where(p => estIds.Contains(p.Est_id));

                        plazasDTO = query
                            // Agrupamos por Estacionamiento Y luego por Categoría
                            .GroupBy(p => new {
                                NombreEstacionamiento = p.Estacionamiento.Est_nombre,
                                NombreCategoria = p.Categoria_Vehiculo.Categoria_descripcion
                            })
                            .Select(g => new PlazaInfoDTO
                            {
                                Estacionamiento = g.Key.NombreEstacionamiento, // Usamos la clave de agrupación
                                Categoria = g.Key.NombreCategoria,             // Usamos la clave de agrupación
                                CantPlazasDisponibles = g.Count(p => p.Plaza_Disponibilidad == true && !p.Ocupacion.Any(o => o.Ocu_fecha_Hora_Fin == null)),
                                CantPlazasOcupadas = g.Count(p => p.Plaza_Disponibilidad == false || p.Ocupacion.Any(o => o.Ocu_fecha_Hora_Fin == null)),
                                CantTotalPlazas = g.Count()
                            })
                            // Ordenamos por Estacionamiento, luego por Categoría
                            .OrderBy(x => x.Estacionamiento)
                            .ThenBy(x => x.Categoria)
                            .ToList();
                    }

                    // El resto de la lógica es común para ambos casos del Dueño
                    _totalDisponibles = plazasDTO.Sum(p => p.CantPlazasDisponibles);
                    _totalOcupadas = plazasDTO.Sum(p => p.CantPlazasOcupadas);
                    _totalGeneral = plazasDTO.Sum(p => p.CantTotalPlazas);

                    gvPlazas.DataSource = plazasDTO;
                    gvPlazas.DataBind();
                }
                else if (tipoUsuario == "Playero")
                {
                    // --- LÓGICA DEL PLAYERO (SE MANTIENE IGUAL) ---
                    int estId = (int)Session["Playero_EstId"];
                    query = query.Where(p => p.Est_id == estId);

                    var plazasPlayeroDTO = query
                        .Where(p => p.Plaza_Disponibilidad == true && !p.Ocupacion.Any(o => o.Ocu_fecha_Hora_Fin == null))
                        .GroupBy(p => p.Categoria_Vehiculo.Categoria_descripcion)
                        .Select(g => new
                        {
                            Categoria = g.Key,
                            CantPlazasDisponibles = g.Count()
                        })
                        .OrderBy(x => x.Categoria)
                        .ToList();

                    gvPlazas.DataSource = plazasPlayeroDTO;
                    gvPlazas.DataBind();
                }

                Session["DatosPlazas"] = gvPlazas.DataSource;
            }
        }

        protected void btnVolver(object sender, EventArgs e)
        {
            Response.Redirect("~/Pages/Plaza/Plaza_Listar.aspx");
        }
    }
}