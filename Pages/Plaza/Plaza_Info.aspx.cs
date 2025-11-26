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
                    gvPlazas.Columns[3].Visible = false; // Ocultar columna de Cant. Ocupadas para Playero
                    gvPlazas.Columns[4].Visible = false; // Ocultar columna de Total para Playero
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
                    .Include("Ocupacion")
                    .Include("Abono"); // 1. IMPORTANTE: Incluir la tabla Abono

                // Variable para la fecha actual (para asegurar consistencia en la consulta)
                DateTime ahora = DateTime.Now;

                if (tipoUsuario == "Dueño")
                {
                    List<PlazaInfoDTO> plazasDTO;

                    if (Session["Dueño_EstId"] != null)
                    {
                        // --- CASO 1: DUEÑO SELECCIONÓ UN ESTACIONAMIENTO ---
                        int estIdSeleccionado = (int)Session["Dueño_EstId"];
                        query = query.Where(p => p.Est_id == estIdSeleccionado);

                        plazasDTO = query
                            .GroupBy(p => p.Categoria_Vehiculo.Categoria_descripcion)
                            .Select(g => new PlazaInfoDTO
                            {
                                Estacionamiento = g.FirstOrDefault().Estacionamiento.Est_nombre,
                                Categoria = g.Key,

                                // 2. LÓGICA ACTUALIZADA: Disponible
                                // - Flag Disponibilidad en TRUE
                                // - SIN Ocupación activa
                                // - SIN Abono vigente
                                CantPlazasDisponibles = g.Count(p => p.Plaza_Disponibilidad == true
                                                                  && !p.Ocupacion.Any(o => o.Ocu_fecha_Hora_Fin == null)
                                                                  && !p.Abono.Any(a => a.Fecha_Vto >= ahora)),

                                // 3. LÓGICA ACTUALIZADA: Ocupada
                                // - Flag Disponibilidad en FALSE
                                // - O CON Ocupación activa
                                // - O CON Abono vigente
                                CantPlazasOcupadas = g.Count(p => p.Plaza_Disponibilidad == false
                                                               || p.Ocupacion.Any(o => o.Ocu_fecha_Hora_Fin == null)
                                                               || p.Abono.Any(a => a.Fecha_Vto >= ahora)),

                                CantTotalPlazas = g.Count()
                            })
                            .OrderBy(x => x.Categoria)
                            .ToList();
                    }
                    else
                    {
                        // --- CASO 2: DUEÑO NO SELECCIONÓ (TODOS SUS ESTACIONAMIENTOS) ---
                        var estIds = db.Estacionamiento
                                       .Where(e => e.Dueño_Legajo == legajo)
                                       .Select(e => e.Est_id)
                                       .ToList();
                        query = query.Where(p => estIds.Contains(p.Est_id));

                        plazasDTO = query
                            .GroupBy(p => new {
                                NombreEstacionamiento = p.Estacionamiento.Est_nombre,
                                NombreCategoria = p.Categoria_Vehiculo.Categoria_descripcion
                            })
                            .Select(g => new PlazaInfoDTO
                            {
                                Estacionamiento = g.Key.NombreEstacionamiento,
                                Categoria = g.Key.NombreCategoria,

                                // Misma lógica actualizada de disponibilidad
                                CantPlazasDisponibles = g.Count(p => p.Plaza_Disponibilidad == true
                                                                  && !p.Ocupacion.Any(o => o.Ocu_fecha_Hora_Fin == null)
                                                                  && !p.Abono.Any(a => a.Fecha_Vto >= ahora)),

                                CantPlazasOcupadas = g.Count(p => p.Plaza_Disponibilidad == false
                                                               || p.Ocupacion.Any(o => o.Ocu_fecha_Hora_Fin == null)
                                                               || p.Abono.Any(a => a.Fecha_Vto >= ahora)),

                                CantTotalPlazas = g.Count()
                            })
                            .OrderBy(x => x.Estacionamiento)
                            .ThenBy(x => x.Categoria)
                            .ToList();
                    }

                    // Totales Generales
                    _totalDisponibles = plazasDTO.Sum(p => p.CantPlazasDisponibles);
                    _totalOcupadas = plazasDTO.Sum(p => p.CantPlazasOcupadas);
                    _totalGeneral = plazasDTO.Sum(p => p.CantTotalPlazas);

                    gvPlazas.DataSource = plazasDTO;
                    gvPlazas.DataBind();
                }
                else if (tipoUsuario == "Playero")
                {
                    // --- LÓGICA DEL PLAYERO ---
                    int estId = (int)Session["Playero_EstId"];
                    query = query.Where(p => p.Est_id == estId);

                    var plazasPlayeroDTO = query
                        // 4. LÓGICA ACTUALIZADA EN EL FILTRO WHERE
                        .Where(p => p.Plaza_Disponibilidad == true
                                 && !p.Ocupacion.Any(o => o.Ocu_fecha_Hora_Fin == null)
                                 && !p.Abono.Any(a => a.Fecha_Vto >= ahora)) // <-- Condición de Abono añadida
                        .GroupBy(p => p.Categoria_Vehiculo.Categoria_descripcion)
                        .Select(g => new PlazaInfoDTO
                        {
                            Categoria = g.Key,
                            CantPlazasDisponibles = g.Count(),
                            CantPlazasOcupadas = 0,
                            CantTotalPlazas = 0,
                            Estacionamiento = ""
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