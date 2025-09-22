using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

namespace Proyecto_Estacionamiento.Pages.Playeros
{
    public partial class Playero_Crear_Editar : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            this.PreRender += Page_PreRender;

            if (!IsPostBack)
            {
                CargarEstacionamientos();

                if (!string.IsNullOrEmpty(Request.QueryString["legajo"]))
                {
                    lblTitulo.Text = "Editar Playero";
                    btnGuardar.Text = "Actualizar";
                    int legajoEdicion = int.Parse(Request.QueryString["legajo"]);
                    CargarDatos(legajoEdicion);
                }
                else
                {
                    // Modo alta: limpiar los campos
                    LimpiarCampos();
                    // Modo Alta
                    chkActivo.Checked = true;  // marcado por defecto
                    chkActivo.Visible = false; // no se muestra
                }
            }
        }

        private void LimpiarCampos()
        {
            txtDni.Text = string.Empty;
            txtPass.Text = string.Empty;
            txtApellido.Text = string.Empty;
            txtNombre.Text = string.Empty;
            chkActivo.Checked = true; // Valor por defecto
            ddlEstacionamientos.ClearSelection(); // Limpia el DropDownList si es necesario
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtPass.Text))
            {
                txtPass.Attributes["value"] = txtPass.Text;
            }
        }

        private void CargarEstacionamientos()
        {
            int legajo = Convert.ToInt32(Session["Usu_legajo"]);

            using (var context = new ProyectoEstacionamientoEntities())
            {
                List<object> estacionamientos;

                if (Session["Dueño_EstId"] != null)
                {
                    // Mostrar solo el estacionamiento previamente seleccionado
                    int estIdSeleccionado = (int)Session["Dueño_EstId"];
                    estacionamientos = context.Estacionamiento
                        .Where(e => e.Est_id == estIdSeleccionado)
                        .Select(e => new { e.Est_id, e.Est_nombre })
                        .ToList<object>();
                    ddlEstacionamientos.Enabled = false; // Deshabilitar el DropDownList
                }
                else
                {
                    // Mostrar todos los estacionamientos disponibles del Dueño
                    estacionamientos = context.Estacionamiento
                        .Where(e => e.Dueño_Legajo == legajo && e.Est_Disponibilidad == true)
                        .Select(e => new { e.Est_id, e.Est_nombre })
                        .ToList<object>();

                }

                ddlEstacionamientos.DataSource = estacionamientos;
                ddlEstacionamientos.DataValueField = "Est_id";
                ddlEstacionamientos.DataTextField = "Est_nombre";
                ddlEstacionamientos.DataBind();
            }
        }


        private void CargarDatos(int legajo)
        {
            using (var db = new ProyectoEstacionamientoEntities())
            {
                var playero = db.Playero.FirstOrDefault(p => p.Playero_legajo == legajo);
                if (playero != null && playero.Usuarios != null)
                {
                    ddlEstacionamientos.SelectedValue = playero.Est_id?.ToString();
                    txtDni.Text = playero.Usuarios.Usu_dni?.ToString();
                    txtPass.Text = playero.Usuarios.Usu_pass;
                    txtApellido.Text = playero.Usuarios.Usu_ap;
                    txtNombre.Text = playero.Usuarios.Usu_nom;
                    chkActivo.Checked = playero.Playero_activo;
                }
            }
        }

        //Validadores
        protected void cvEstacionamiento_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = !string.IsNullOrEmpty(ddlEstacionamientos.SelectedValue);
        }

        protected void cvDni_ServerValidate(object source, ServerValidateEventArgs args)
        {
            string dniTexto = txtDni.Text;

            args.IsValid = !string.IsNullOrWhiteSpace(dniTexto) &&
                           int.TryParse(dniTexto, out int dni) &&
                           dni > 0 &&
                           dniTexto.Length == 8;

            if (!args.IsValid)
            {
                cvDni.ErrorMessage = "El DNI es obligatorio y debe ser un número válido de 8 dígitos.";
            }
        }

        protected void cvApellido_ServerValidate(object source, ServerValidateEventArgs args)
        {
            string apellido = txtApellido.Text;
            args.IsValid = !string.IsNullOrWhiteSpace(apellido) &&
                           System.Text.RegularExpressions.Regex.IsMatch(apellido, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$");
        }

        protected void cvNombre_ServerValidate(object source, ServerValidateEventArgs args)
        {
            string nombre = txtNombre.Text;
            args.IsValid = !string.IsNullOrWhiteSpace(nombre) &&
                           System.Text.RegularExpressions.Regex.IsMatch(nombre, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$");
        }

        //Guardar
        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
                return;

            bool esAlta = string.IsNullOrEmpty(Request.QueryString["legajo"]);

            int dni = int.Parse(txtDni.Text);
            int estId = int.Parse(ddlEstacionamientos.SelectedValue);
            string pass = txtPass.Text;
            string apellido = txtApellido.Text;
            string nombre = txtNombre.Text;
            bool disponibilidad = chkActivo.Checked;

            using (var db = new ProyectoEstacionamientoEntities())
            {

                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {

                        if (esAlta)
                        {
                            var nuevoUsuario = new Usuarios();
                            nuevoUsuario.Usu_dni = dni;
                            nuevoUsuario.Usu_pass = pass;
                            nuevoUsuario.Usu_ap = apellido;
                            nuevoUsuario.Usu_nom = nombre;
                            nuevoUsuario.Usu_tipo = "Playero";

                            db.Usuarios.Add(nuevoUsuario);
                            db.SaveChanges();   // Se guarda y se genera Usu_legajo automáticamente

                            var nuevoPlayero = new Playero
                            {
                                Playero_legajo = nuevoUsuario.Usu_legajo, // Usar el legajo recién generado
                                Playero_activo = disponibilidad,
                                Est_id = estId
                            };

                            db.Playero.Add(nuevoPlayero);
                            db.SaveChanges();
                        }
                        else
                        {
                            int legajoEdicion = int.Parse(Request.QueryString["legajo"]);
                            Usuarios usuarioEdicion = db.Usuarios.FirstOrDefault(u => u.Usu_legajo == legajoEdicion);
                            Playero playeroEdicion = db.Playero.FirstOrDefault(p => p.Playero_legajo == legajoEdicion);

                            if (usuarioEdicion != null)
                            {
                                usuarioEdicion.Usu_dni = dni;
                                usuarioEdicion.Usu_pass = pass;
                                usuarioEdicion.Usu_ap = apellido;
                                usuarioEdicion.Usu_nom = nombre;
                                usuarioEdicion.Usu_tipo = "Playero";
                            }

                            if (playeroEdicion != null)
                            {
                                playeroEdicion.Playero_activo = disponibilidad;
                                playeroEdicion.Est_id = estId;
                            }

                            db.SaveChanges();
                        }

                        transaction.Commit();
                        string accion = esAlta == true ? "agregado" : "editado";
                        Response.Redirect($"Playero_Listar.aspx?exito=1&accion={accion}");
                    }
                    catch (Exception ex)
                    {
                        if (transaction != null && transaction.UnderlyingTransaction.Connection != null)
                        {
                            transaction.Rollback();
                        }

                        string mensajeErrorRaiz = ObtenerMensajeErrorCompleto(ex);

                        if (mensajeErrorRaiz.Contains("UQ_usuarios_usu_dni"))
                        {
                            // Forzar error en el validador del DNI
                            cvDni.IsValid = false;
                            cvDni.ErrorMessage = "Ya existe un Playero con ese DNI. Por favor, ingrese uno diferente.";
                        }
                    }
                }
            }
        }

        private string ObtenerMensajeErrorCompleto(Exception ex)
        {
            if (ex.InnerException == null)
                return ex.Message;
            else
                return ObtenerMensajeErrorCompleto(ex.InnerException);
        }

    }
}