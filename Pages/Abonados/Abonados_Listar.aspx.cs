using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Proyecto_Estacionamiento.Pages.Abonados
{
    public partial class Abonados_Listar : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)    // Verifica si es la primera vez que se carga la página
            {

                string tipoUsuario = Session["Usu_tipo"] as string;

                int legajo = Convert.ToInt32(Session["Usu_legajo"]);
                if (tipoUsuario != "Playero")
                {
                    btnRegistrar.Visible = false;

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

                CargarAbonos();
            }
        }

        private void CargarAbonos(string patenteFiltro = null)
        {
            try
            {
                string tipoUsuario = Session["Usu_tipo"] as string;
                int legajo = Convert.ToInt32(Session["Usu_legajo"]);

                using (var db = new ProyectoEstacionamientoEntities())
                {
                    // 1. Consulta base sobre Abono (IQueryable)
                    IQueryable<Abono> query = db.Abono;

                    // 2. Aplicamos el filtro de seguridad por 
                    if (tipoUsuario == "Dueño")
                    {
                        if (Session["Dueño_EstId"] != null){
                            int estIdSeleccionado = (int)Session["Dueño_EstId"];

                            // Filtra la consulta base 'Abono' por Est_id
                            query = query.Where(a => a.Est_id == estIdSeleccionado);
                        }
                        else{
                            var estIdsDelDueño = db.Estacionamiento
                                                   .Where(e => e.Dueño_Legajo == legajo)
                                                   .Select(e => e.Est_id).ToList();
                            // Filtra por todos los Est_id que pertenecen al dueño
                            query = query.Where(a => estIdsDelDueño.Contains(a.Est_id));
                        }
                    }
                    else if (tipoUsuario == "Playero")
                    {
                        int estIdPlayero = (int)Session["Playero_EstId"];
                        // Filtra por el Est_id específico del playero
                        query = query.Where(a => a.Est_id == estIdPlayero);
                    }

                    // Filtramos por ABONOS VIGENTES
                    query = query.Where(a => a.Titular_Abono.TAB_Fecha_Vto >= DateTime.Now);

                    // Aplicamos el filtro por PATENTE (si se proveyó una)
                    if (!string.IsNullOrWhiteSpace(patenteFiltro)){
                        // Filtra si CUALQUIER Vehiculo_Abonado asociado a este Abono contiene la patente
                        query = query.Where(a => a.Vehiculo_Abonado.Any(va => va.Vehiculo_Patente.ToUpper().Contains(patenteFiltro.ToUpper())));
                    }

                    // Proyectamos los datos para el Grid y el Modal
                    var abonos = query
                        .OrderByDescending(a => a.TAB_Fecha_Desde)
                        .Select(a => new
                        {
                            Nombre = a.Titular_Abono.TAB_Nombre,
                            Apellido = a.Titular_Abono.TAB_Apellido,
                            Plaza = a.Plaza.Plaza_Nombre,

                            TipoIdentificacion = a.Titular_Abono.Tipo_Identificacion,
                            NumeroIdentificacion = a.Titular_Abono.Numero_Identificacion,

                            // Datos para el Modal
                            FechaDesde = a.TAB_Fecha_Desde,
                            FechaVto = a.Titular_Abono.TAB_Fecha_Vto,
                            PatentesList = a.Vehiculo_Abonado.Select(va => va.Vehiculo_Patente)
                        })
                        .ToList() // Traemos los datos a memoria...
                        .Select(dto => new // ...y formateamos
                        {
                            dto.Nombre,
                            dto.Apellido,
                            dto.Plaza,
                            dto.TipoIdentificacion,
                            dto.NumeroIdentificacion,
                            PatentesStr = string.Join(", ", dto.PatentesList),
                            FechaDesdeStr = dto.FechaDesde.ToString("dd/MM/yyyy HH:mm"),
                            FechaVtoStr = dto.FechaVto.ToString("dd/MM/yyyy HH:mm")
                        });

                    gvAbonos.DataSource = abonos;
                    gvAbonos.DataBind();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }


        protected void btnBuscarPatente_Click(object sender, EventArgs e)
        {
            // Llama al método de carga pasándole el texto del buscador
            CargarAbonos(txtBuscarPatente.Text.Trim());
        }

        protected void txtBuscarPatente_TextChanged(object sender, EventArgs e)
        {
            CargarAbonos(txtBuscarPatente.Text.Trim());
        }


        private int? ObtenerEstacionamientoId()
        {
            int legajo = Convert.ToInt32(Session["Usu_legajo"]);

            using (var db = new ProyectoEstacionamientoEntities())
            {
                // Buscamos al Playero
                var playero = db.Playero.FirstOrDefault(p => p.Usuarios.Usu_legajo == legajo);

                // Obtenemos el Estacionamiento donde esta asignado el Playero
                int estacionamientoId = (int)playero.Est_id;

                return estacionamientoId;
            }
        }


        protected void btnRegistrar_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Pages/Abonados/Abonado_Registrar.aspx");
        }

    }
}