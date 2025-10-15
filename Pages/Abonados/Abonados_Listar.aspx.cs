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
                    IQueryable<Vehiculo_Abonado> query = db.Vehiculo_Abonado;

                    // 1. Aplicamos el filtro de seguridad por rol (esta lógica se mantiene)
                    if (tipoUsuario == "Dueño")
                    {
                        if (Session["Dueño_EstId"] != null)
                        {
                            int estIdSeleccionado = (int)Session["Dueño_EstId"];
                            query = query.Where(va => va.Est_id == estIdSeleccionado);
                        }
                        else
                        {
                            var estIdsDelDueño = db.Estacionamiento.Where(e => e.Dueño_Legajo == legajo).Select(e => e.Est_id).ToList();
                            query = query.Where(va => estIdsDelDueño.Contains(va.Est_id));
                        }
                    }
                    else if (tipoUsuario == "Playero")
                    {
                        int estIdPlayero = (int)Session["Playero_EstId"];
                        query = query.Where(va => va.Est_id == estIdPlayero);
                    }

                    // 2. APLICAMOS EL NUEVO FILTRO POR PATENTE
                    if (!string.IsNullOrWhiteSpace(patenteFiltro))
                    {
                        // .Contains() busca coincidencias parciales (ej. "AA123" encuentra "AA123BB")
                        // .ToUpper() hace la búsqueda insensible a mayúsculas/minúsculas
                        query = query.Where(va => va.Vehiculo_Patente.ToUpper().Contains(patenteFiltro.ToUpper()));
                    }

                    // 3. Ejecutamos la consulta final
                    var abonos = query
                        .Select(va => new
                        {
                            Patente = va.Vehiculo_Patente,
                            Plaza = va.Abono.Plaza.Plaza_Nombre,
                            Desde = va.Abono.TAB_Fecha_Desde,
                            Hasta = va.Abono.Titular_Abono.TAB_Fecha_Vto
                        })
                        .OrderByDescending(a => a.Desde)
                        .ToList();

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