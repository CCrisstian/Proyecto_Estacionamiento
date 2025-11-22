using Proyecto_Estacionamiento;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Proyecto_Estacionamiento.Pages.Incidencia
{
    public partial class Incidencia_Registrar : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarMotivos();
            }
        }

        private void CargarMotivos()
        {
            // 1. Crear la lista de motivos
            var motivos = new List<ListItem>
        {
            // El primer ítem con Value="" es crucial para el RequiredFieldValidator
            new ListItem("-- Seleccione Motivo --", ""),
            new ListItem("Robo / Hurto de vehículo", "Robo / Hurto de vehículo"),
            new ListItem("Robo / Hurto en vehículo", "Robo / Hurto en vehículo"),
            new ListItem("Choque / Colisión", "Choque / Colisión"),
            new ListItem("Daños a vehículo (Rayón, Vandalismo)", "Daños a vehículo"),
            new ListItem("Daños a la propiedad", "Daños a la propiedad"),
            new ListItem("Extravío de objetos", "Extravío"),
            new ListItem("Falla mecánica / Batería", "Falla mecánica"),
            new ListItem("Incidente con cliente", "Incidente con cliente"),
            new ListItem("Otro", "Otro")
        };

            // 2. Asignar la lista al DropDownList
            ddlMotivo.DataSource = motivos;
            ddlMotivo.DataTextField = "Text"; // La parte visible (ej. "Robo / Hurto...")
            ddlMotivo.DataValueField = "Value"; // El valor a guardar (ej. "Robo / Hurto...")
            ddlMotivo.DataBind();
        }


        protected void BtnGuardar_Click(object sender, EventArgs e)
        {
            // 1. Validar que los campos requeridos estén llenos
            Page.Validate("Incidencia");
            if (!Page.IsValid)
            {
                return;
            }

            try
            {
                // 2. Obtener todos los datos del formulario y la sesión
                int legajoPlayero = Convert.ToInt32(Session["Usu_legajo"]);
                DateTime fechaHoraIncidencia = DateTime.Now; // Captura automática
                string motivo = ddlMotivo.SelectedValue;
                string descripcion = txtDescripcion.Text.Trim();
                bool estado = (ddlEstado.SelectedValue == "1"); // 1 = Resuelto, 0 = Pendiente

                // 3. Crear la nueva Incidencia
                var nuevaIncidencia = new Incidencias
                {
                    Playero_legajo = legajoPlayero,
                    Inci_fecha_Hora = fechaHoraIncidencia,
                    Inci_Motivo = motivo,
                    Inci_descripcion = descripcion,
                    Inci_Estado = estado
                };

                // 4. Guardar en la base de datos
                using (var db = new ProyectoEstacionamientoEntities())
                {
                    db.Incidencias.Add(nuevaIncidencia);
                    db.SaveChanges();
                }

                // 5. Redirigir a una página de éxito o listado
                Response.Redirect("Incidencias_Listar.aspx?exito=1");
            }
            catch (Exception ex)
            {
                // Manejo de errores
                System.Diagnostics.Debug.WriteLine(ex.ToString()); // Para depuración
            }
        }

        protected void BtnCancelar_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Pages/Incidencia/Incidencias_Listar.aspx");
        }
    }
}