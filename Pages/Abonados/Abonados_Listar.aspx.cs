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
                else
                {
                    if (Session["Turno_Id_Actual"] == null)
                    {
                        btnRegistrar.Enabled = false;
                        btnRegistrar.CssClass = "btn btn-secondary";
                        btnRegistrar.ToolTip = "Debe iniciar un Turno para registrar Abonados.";
                    }
                    else
                    {
                        btnRegistrar.Enabled = true;
                        btnRegistrar.CssClass = "btn btn-success";
                    }
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
                    // 3. Filtramos por ABONOS VIGENTES (Usando Abono.Fecha_Vto)
                    query = query.Where(a => a.Fecha_Vto >= DateTime.Now);

                    // 4. Aplicamos el filtro por PATENTE (si se proveyó una)
                    if (!string.IsNullOrWhiteSpace(patenteFiltro))
                    {
                        query = query.Where(a => a.Vehiculo_Abonado.Any(va => va.Vehiculo_Patente.ToUpper().Contains(patenteFiltro.ToUpper())));
                    }

                    // 5. Proyectamos los datos para el Grid y el Modal
                    var abonos = query
                        // Ordenamos por la fecha de inicio del Abono
                        .OrderByDescending(a => a.Fecha_Desde)
                        .Select(a => new // 'a' es un objeto 'Abono'
                        {
                            // Datos Grid
                            Nombre = a.Titular_Abono.TAB_Nombre,
                            Apellido = a.Titular_Abono.TAB_Apellido,
                            Plaza = a.Plaza.Plaza_Nombre,
                            TipoIdentificacion = a.Titular_Abono.Tipo_Identificacion,
                            NumeroIdentificacion = a.Titular_Abono.Numero_Identificacion,

                            // Datos Modal
                            FechaDesde = a.Fecha_Desde,
                            FechaVto = a.Fecha_Vto,
                            PatentesList = a.Vehiculo_Abonado.Select(va => va.Vehiculo_Patente),
                            TipoAbono = a.Pagos_Abonados.OrderByDescending(p => p.Fecha_Pago)
                            .FirstOrDefault().Tarifa.Tipos_Tarifa.Tipos_tarifa_descripcion
                        })
                        .ToList() // Traemos los datos a memoria...
                        .Select(dto => new // ...y formateamos
                        {
                            dto.Nombre,
                            dto.Apellido,
                            dto.Plaza,
                            dto.TipoIdentificacion,
                            dto.NumeroIdentificacion,
                            dto.TipoAbono,
                            PatentesStr = string.Join(", ", dto.PatentesList),
                            FechaDesdeStr = dto.FechaDesde.ToString("dd/MM/yyyy HH:mm"),
                            FechaVtoStr = dto.FechaVto.ToString("dd/MM/yyyy HH:mm"),
                            FechaVtoRaw = dto.FechaVto.ToString("yyyy-MM-dd"),

                            FechaVto = dto.FechaVto
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

        protected void gvAbonos_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            // Asegurarse de que estamos en una fila de datos (no en el header o footer)
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                try
                {
                    // 1. Obtener el objeto de datos de la fila
                    // Usamos 'dynamic' porque estamos bindeando a un tipo anónimo
                    dynamic dataItem = e.Row.DataItem;

                    // 2. Obtener la fecha de vencimiento (el objeto DateTime real)
                    DateTime fechaVto = (DateTime)dataItem.FechaVto;

                    // 3. Comparar solo la parte de la FECHA (ignorando la hora)
                    if (fechaVto.Date == DateTime.Today)
                    {
                        // 4. Aplicar la clase CSS a toda la fila
                        e.Row.CssClass += " abono-vencido-hoy";
                    }
                }
                catch (Exception ex)
                {
                    // Manejar cualquier error
                    System.Diagnostics.Debug.WriteLine("Error en RowDataBound: " + ex.Message);
                }
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