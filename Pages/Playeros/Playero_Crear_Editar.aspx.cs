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

                    txtLegajo.Text = legajoEdicion.ToString();
                    txtLegajo.Enabled = false; // Edición: no se puede cambiar legajo
                    hfLegajo.Value = legajoEdicion.ToString();
                }
                else
                {
                    txtLegajo.Enabled = true;  // Alta: se puede ingresar legajo
                }
            }
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
                    txtLegajo.Text = playero.Playero_legajo.ToString();
                    txtDni.Text = playero.Usuarios.Usu_dni?.ToString();
                    txtPass.Text = playero.Usuarios.Usu_pass;
                    txtApellido.Text = playero.Usuarios.Usu_ap;
                    txtNombre.Text = playero.Usuarios.Usu_nom;
                }
            }
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            lblError.Visible = true;

            bool esAlta = string.IsNullOrEmpty(Request.QueryString["legajo"]);

            int legajo;

            if (esAlta)
            {
                // En alta, el legajo viene del TextBox (editable)
                if (string.IsNullOrWhiteSpace(txtLegajo.Text) || !int.TryParse(txtLegajo.Text, out legajo))
                {
                    lblError.Text = "El Legajo debe ser un número entero válido.";
                    return;
                }
            }
            else
            {
                // En edición, el legajo viene del HiddenField (txtLegajo está deshabilitado)
                if (string.IsNullOrWhiteSpace(hfLegajo.Value) || !int.TryParse(hfLegajo.Value, out legajo))
                {
                    lblError.Text = "No se pudo obtener el Legajo para edición.";
                    return;
                }
            }

            // Validar DNI obligatorio y válido
            if (string.IsNullOrWhiteSpace(txtDni.Text) || !int.TryParse(txtDni.Text, out int dni) || dni <= 0)
            {
                lblError.Text = "El DNI es obligatorio y debe ser un número entero válido mayor que cero.";
                return;
            }

            int estId = int.Parse(ddlEstacionamientos.SelectedValue);
            string pass = txtPass.Text;
            string apellido = txtApellido.Text;
            string nombre = txtNombre.Text;

            using (var db = new ProyectoEstacionamientoEntities())
            {
                Usuarios usuarioExistente = db.Usuarios.FirstOrDefault(u => u.Usu_legajo == legajo);

                int? legajoOriginal = null;
                if (!esAlta)
                {
                    legajoOriginal = int.Parse(Request.QueryString["legajo"]);
                    if (legajoOriginal != legajo && usuarioExistente != null)
                    {
                        lblError.Text = "Ya existe un Playero con ese Legajo. Por favor, ingrese uno diferente.";
                        return;
                    }
                }
                else
                {
                    if (usuarioExistente != null)
                    {
                        lblError.Text = "Ya existe un Playero con ese Legajo. Por favor, ingrese uno diferente.";
                        return;
                    }
                }

                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {

                        if (esAlta)
                        {
                            var nuevoUsuario = new Usuarios();
                            nuevoUsuario.Usu_legajo = legajo;
                            nuevoUsuario.Usu_dni = dni;
                            nuevoUsuario.Usu_pass = pass;
                            nuevoUsuario.Usu_ap = apellido;
                            nuevoUsuario.Usu_nom = nombre;
                            nuevoUsuario.Usu_tipo = "Playero";

                            db.Usuarios.Add(nuevoUsuario);

                            var nuevoPlayero = new Playero
                            {
                                Playero_legajo = legajo,
                                Est_id = estId
                            };

                            db.Playero.Add(nuevoPlayero);
                            db.SaveChanges();
                        }
                        else
                        {
                            Usuarios usuarioEdicion = db.Usuarios.FirstOrDefault(u => u.Usu_legajo == legajoOriginal);
                            Playero playeroEdicion = db.Playero.FirstOrDefault(p => p.Playero_legajo == legajoOriginal);

                            if (usuarioEdicion != null)
                            {
                                usuarioEdicion.Usu_legajo = legajo;
                                usuarioEdicion.Usu_dni = dni;
                                usuarioEdicion.Usu_pass = pass;
                                usuarioEdicion.Usu_ap = apellido;
                                usuarioEdicion.Usu_nom = nombre;
                                usuarioEdicion.Usu_tipo = "Playero";
                            }

                            if (playeroEdicion != null)
                            {
                                playeroEdicion.Playero_legajo = legajo;
                                playeroEdicion.Est_id = estId;
                            }

                            db.SaveChanges();
                        }

                        transaction.Commit();
                        Response.Redirect("Playero_CRUD.aspx");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        string mensajeErrorRaiz = ObtenerMensajeErrorCompleto(ex);

                        if (mensajeErrorRaiz.Contains("UQ_usuarios_usu_dni"))
                        {
                            lblError.Text = "Ya existe un Playero con ese DNI. Por favor, ingrese uno diferente.";
                        }
                        else if (mensajeErrorRaiz.Contains("PK_Usuarios"))
                        {
                            lblError.Text = "Ya existe un Playero con ese Legajo. Por favor, ingrese uno diferente.";
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