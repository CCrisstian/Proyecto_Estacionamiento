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

        private void CargarAbonos()
        {
            try
            {
                using (var db = new ProyectoEstacionamientoEntities())
                {
                    int? estId = ObtenerEstacionamientoId();
                    if (estId == null)
                    {
                        // Manejar el caso donde no hay un estacionamiento seleccionado
                        gvAbonos.DataSource = null;
                        gvAbonos.DataBind();
                        return;
                    }

                    // Preparamos la consulta a la base de datos usando LINQ
                    var abonos = db.Vehiculo_Abonado
                        .Where(va => va.Est_id == estId) // Filtramos por el estacionamiento actual
                        .Select(va => new
                        {
                            Patente = va.Vehiculo_Patente,
                            Plaza = va.Abono.Plaza.Plaza_Nombre, // Navegamos a través de las relaciones para obtener el nombre
                            Desde = va.Abono.TAB_Fecha_Desde,
                            Hasta = va.Abono.Titular_Abono.TAB_Fecha_Vto // La fecha de Vto está en Titular_Abono
                        })
                        .OrderByDescending(a => a.Desde) // Mostramos los más recientes primero
                        .ToList();

                    // Enlazamos los resultados con el GridView
                    gvAbonos.DataSource = abonos;
                    gvAbonos.DataBind();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
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