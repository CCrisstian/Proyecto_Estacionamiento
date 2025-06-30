using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Proyecto_Estacionamiento.Pages.Estacionamiento
{
    public partial class Estacionamiento_Crear : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                for (int hora = 0; hora < 24; hora++)
                {
                    string horaTexto = hora.ToString("D2") + ":00";
                    ddlHoraInicio.Items.Add(horaTexto);
                    ddlHoraFin.Items.Add(horaTexto);
                }

                // Modo Edición
                if (Request.QueryString["id"] != null)
                {
                    int id = int.Parse(Request.QueryString["id"]);
                    CargarEstacionamiento(id);
                    btnGuardar.Text = "Actualizar";
                }
            }

        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            lblMensaje.Text = ""; // Limpiar Mensaje de Error previo
            
            // 1. Validación de campos obligatorios
            if (string.IsNullOrWhiteSpace(txtNombre.Text) || string.IsNullOrWhiteSpace(txtDireccion.Text))
            {
                lblMensaje.Text = "Los campos Nombre y Dirección son obligatorios.";
                return;
            }

            // 2. Validar horario
            int horaInicio = int.Parse(ddlHoraInicio.SelectedValue.Substring(0, 2));
            int horaFin = int.Parse(ddlHoraFin.SelectedValue.Substring(0, 2));

            if (horaInicio >= horaFin)
            {
                lblMensaje.Text = "La hora de inicio debe ser menor que la hora de fin.";
                return;
            }

            // 3. Leer datos del formulario
            string nombre = txtNombre.Text.Trim();
            string direccion = txtDireccion.Text.Trim();
            string provincia = ddlProvincia.SelectedValue;
            string localidad = ddlLocalidad.SelectedValue;
            string horario = ddlHoraInicio.SelectedValue + " - " + ddlHoraFin.SelectedValue;

            // 4. Validar y Obtener el legajo del Dueño desde la Sesión
            if (Session["Usu_legajo"] == null)
            {
                Response.Redirect("~/Pages/Login/Login.aspx");
                return;
            }
            int legajoDueño = (int)Session["Usu_legajo"];

            // 5. (Crear/Editar) y Guardar el nuevo Estacionamiento en la Base de Datos
            try
            {
                using (var db = new ProyectoEstacionamientoEntities())
                {
                    // Si es "Edición"
                    if (ViewState["Est_id"] != null)
                    {
                        int id = (int)ViewState["Est_id"];
                        var est = db.Estacionamiento.Find(id);  // Buscamos el estacionamiento por ID
                        if (est != null)
                        {   // Poblamos los campos del Estacionamiento
                            est.Est_nombre = nombre;
                            est.Est_direccion = direccion;
                            est.Est_provincia = provincia;
                            est.Est_localidad = localidad;
                            est.Est_horario = horario;
                            // Guardamos el Estacionamiento actualizado
                            db.SaveChanges();
                        }
                        else
                        {
                            lblMensaje.Text = "No se encontró el estacionamiento a actualizar.";
                            return;
                        }
                    }
                    else // Si es "Creación"
                    {
                        var nuevoEstacionamiento = new Proyecto_Estacionamiento.Estacionamiento // Creamos un nuevo objeto Estacionamiento
                        {
                            Dueño_Legajo = legajoDueño,
                            Est_nombre = nombre,
                            Est_direccion = direccion,
                            Est_provincia = provincia,
                            Est_localidad = localidad,
                            Est_horario = horario,
                            Est_puntaje = 0
                        };
                        // Agregamos el nuevo Estacionamiento a la Base de Datos
                        db.Estacionamiento.Add(nuevoEstacionamiento);
                        db.SaveChanges();
                    }
                }
                Response.Redirect("EstacionamientoCRUD.aspx");
            }
            catch (Exception ex)
            {
                lblMensaje.Text = "Error al guardar: " + ex.Message;
            }
        }
        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Pages/Estacionamiento/EstacionamientoCRUD.aspx");
        }

        private void CargarEstacionamiento(int id)
        {
            using (var db = new ProyectoEstacionamientoEntities())
            {
                var est = db.Estacionamiento.Find(id);
                if (est != null)
                {
                    txtNombre.Text = est.Est_nombre;
                    txtDireccion.Text = est.Est_direccion;
                    ddlProvincia.SelectedValue = est.Est_provincia;
                    ddlLocalidad.SelectedValue = est.Est_localidad;

                    var horarioSplit = est.Est_horario.Split('-');
                    ddlHoraInicio.SelectedValue = horarioSplit[0].Trim();
                    ddlHoraFin.SelectedValue = horarioSplit[1].Trim();

                    ViewState["Est_id"] = est.Est_id; // Guardamos el ID
                }
            }
        }

    }
}