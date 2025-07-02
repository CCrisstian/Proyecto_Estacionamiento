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
                    ddlHoraInicio_FinDeSemana.Items.Add(horaTexto);
                    ddlHoraFin_FinDeSemana.Items.Add(horaTexto);
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

            // 3. Validar horario fin de semana (opcional, si quieres validar)
            int horaInicioFinde = int.Parse(ddlHoraInicio_FinDeSemana.SelectedValue.Substring(0, 2));
            int horaFinFinde = int.Parse(ddlHoraFin_FinDeSemana.SelectedValue.Substring(0, 2));
            if (horaInicioFinde >= horaFinFinde)
            {
                lblMensaje.Text = "La hora de inicio de fin de semana debe ser menor que la hora de fin.";
                return;
            }

            // 4. Leer datos del formulario
            string nombre = txtNombre.Text.Trim();
            string direccion = txtDireccion.Text.Trim();
            string provincia = ddlProvincia.SelectedValue;
            string localidad = ddlLocalidad.SelectedValue;
            string horario = ddlHoraInicio.SelectedValue + " - " + ddlHoraFin.SelectedValue;
            string diasAtencion = ddlDiaInicio.SelectedValue + " a " + ddlDiaFin.SelectedValue;
            bool diasFeriado = chkDiasFeriado.Checked;
            bool finDeSemana = chkFinDeSemana.Checked;
            string horaFinDeSemana = ddlHoraInicio_FinDeSemana.SelectedValue + " - " + ddlHoraFin_FinDeSemana.SelectedValue;

            // 5. Validar y Obtener el legajo del Dueño desde la Sesión
            if (Session["Usu_legajo"] == null)
            {
                Response.Redirect("~/Pages/Login/Login.aspx");
                return;
            }
            int legajoDueño = (int)Session["Usu_legajo"];

            // 6. (Crear/Editar) y Guardar el nuevo Estacionamiento en la Base de Datos
            try
            {
                using (var db = new ProyectoEstacionamientoEntities())
                {
                    if (ViewState["Est_id"] != null)
                    {
                        int id = (int)ViewState["Est_id"];
                        var est = db.Estacionamiento.Find(id);
                        if (est != null)
                        {
                            est.Est_nombre = nombre;
                            est.Est_direccion = direccion;
                            est.Est_provincia = provincia;
                            est.Est_localidad = localidad;
                            est.Est_horario = horario;
                            est.Est_Dias_Atencion = diasAtencion;
                            est.Est_Dias_Feriado_Atencion = diasFeriado;
                            est.Est_Fin_de_semana_Atencion = finDeSemana;
                            est.Est_Hora_Fin_de_semana = horaFinDeSemana;
                            db.SaveChanges();
                        }
                        else
                        {
                            lblMensaje.Text = "No se encontró el estacionamiento a actualizar.";
                            return;
                        }
                    }
                    else
                    {
                        var nuevoEstacionamiento = new Proyecto_Estacionamiento.Estacionamiento
                        {
                            Dueño_Legajo = legajoDueño,
                            Est_nombre = nombre,
                            Est_direccion = direccion,
                            Est_provincia = provincia,
                            Est_localidad = localidad,
                            Est_horario = horario,
                            Est_puntaje = 0,
                            Est_Dias_Atencion = diasAtencion,
                            Est_Dias_Feriado_Atencion = diasFeriado,
                            Est_Fin_de_semana_Atencion = finDeSemana,
                            Est_Hora_Fin_de_semana = horaFinDeSemana
                        };
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

                    // Horario (Inicio - Fin)
                    var horarioSplit = est.Est_horario.Split('-');
                    ddlHoraInicio.SelectedValue = horarioSplit[0].Trim();
                    ddlHoraFin.SelectedValue = horarioSplit[1].Trim();

                    // Días de Atención (Inicio - Fin)
                    var diasSplit = est.Est_Dias_Atencion.Split('a');
                    ddlDiaInicio.SelectedValue = diasSplit[0].Trim();
                    ddlDiaFin.SelectedValue = diasSplit[1].Trim();

                    // Booleanos
                    chkDiasFeriado.Checked = est.Est_Dias_Feriado_Atencion ?? false;
                    chkFinDeSemana.Checked = est.Est_Fin_de_semana_Atencion ?? false;

                    // Horario Fin de Semana (Inicio - Fin)
                    var findeSplit = est.Est_Hora_Fin_de_semana.Split('-');
                    ddlHoraInicio_FinDeSemana.SelectedValue = findeSplit[0].Trim();
                    ddlHoraFin_FinDeSemana.SelectedValue = findeSplit[1].Trim();

                    ViewState["Est_id"] = est.Est_id; // Guardamos el ID
                }
            }
        }

    }
}