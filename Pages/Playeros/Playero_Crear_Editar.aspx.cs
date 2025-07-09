using System;
using System.Linq;
using System.Web.UI;
using Proyecto_Estacionamiento;

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
                    int legajoEdicion = int.Parse(Request.QueryString["legajo"]);
                    CargarDatos(legajoEdicion);
                }
                else
                {
                    // Modo alta: limpiar los campos
                    LimpiarCampos();
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
            using (var db = new ProyectoEstacionamientoEntities())
            {
                var estacionamientos = db.Estacionamiento
                    .Select(e => new { e.Est_id, e.Est_nombre })
                    .ToList();

                ddlEstacionamientos.DataSource = estacionamientos;
                ddlEstacionamientos.DataTextField = "Est_nombre";
                ddlEstacionamientos.DataValueField = "Est_id";
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

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            lblError.Visible = true;

            bool esAlta = string.IsNullOrEmpty(Request.QueryString["legajo"]);


            // DNI obligatorio, numérico, y de exactamente 8 dígitos
            if (string.IsNullOrWhiteSpace(txtDni.Text) || !int.TryParse(txtDni.Text, out int dni) || dni <= 0 || txtDni.Text.Length != 8)
            {
                lblError.Text = "El DNI es obligatorio y debe ser un número válido de exactamente 8 dígitos.";
                return;
            }

            // Validar que Apellido y Nombre contengan solo letras
            if (!System.Text.RegularExpressions.Regex.IsMatch(txtApellido.Text, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
            {
                lblError.Text = "El Apellido solo debe contener letras.";
                return;
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(txtNombre.Text, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
            {
                lblError.Text = "El Nombre solo debe contener letras.";
                return;
            }

            // Obtener valores
            int estId = int.Parse(ddlEstacionamientos.SelectedValue);
            string pass = txtPass.Text;
            string apellido = txtApellido.Text;
            string nombre = txtNombre.Text;

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
                                Playero_activo = chkActivo.Checked,
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
                                playeroEdicion.Playero_activo = chkActivo.Checked;
                                playeroEdicion.Est_id = estId;
                            }

                            db.SaveChanges();
                        }

                        transaction.Commit();
                        Response.Redirect("Playero_CRUD.aspx");
                    }
                    catch (Exception ex)
                    {
                        // Solo hacer rollback si transaction y su conexión existen
                        if (transaction != null && transaction.UnderlyingTransaction.Connection != null)
                        {
                            transaction.Rollback();
                        }

                        string mensajeErrorRaiz = ObtenerMensajeErrorCompleto(ex);

                        if (mensajeErrorRaiz.Contains("UQ_usuarios_usu_dni"))
                        {
                            lblError.Text = "Ya existe un Playero con ese DNI. Por favor, ingrese uno diferente.";
                        }
                        else
                        {
                            lblError.Text = "Ocurrió un error al guardar los datos. Detalles: " + mensajeErrorRaiz;
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