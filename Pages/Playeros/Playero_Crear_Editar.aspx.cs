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

                if (Request.QueryString["legajo"] != null)
                {
                    int legajo = int.Parse(Request.QueryString["legajo"]);
                    CargarDatos(legajo);
                    txtLegajo.Enabled = true;
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

            // Validaciones iniciales de entrada
            if (!int.TryParse(txtLegajo.Text, out int legajo))
            {
                lblError.Text = "El Legajo debe ser un número entero válido.";
                return;
            }

            if (!int.TryParse(txtDni.Text, out int dni))
            {
                lblError.Text = "El DNI debe ser un número entero válido.";
                return;
            }

            int estId = int.Parse(ddlEstacionamientos.SelectedValue);
            string pass = txtPass.Text;
            string apellido = txtApellido.Text;
            string nombre = txtNombre.Text;

            // Determinar si es alta o edición
            bool esAlta = Request.QueryString["legajo"] == null;

            int? legajoOriginal = null;
            if (!esAlta)
            {
                legajoOriginal = int.Parse(Request.QueryString["legajo"]);
            }

            using (var db = new ProyectoEstacionamientoEntities())
            {
                Usuarios usuarioExistente = db.Usuarios.FirstOrDefault(u => u.Usu_legajo == legajo);
                Playero playeroExistente = db.Playero.FirstOrDefault(p => p.Playero_legajo == legajo);

                // Validar si se está intentando usar un legajo ya existente en edición
                if (!esAlta && legajoOriginal != legajo && usuarioExistente != null)
                {
                    lblError.Text = "Ya existe un Playero con ese Legajo. Por favor, ingrese uno diferente.";
                    return;
                }

                // Validar si el legajo ya existe en alta
                if (esAlta && usuarioExistente != null)
                {
                    lblError.Text = "Ya existe un Playero con ese Legajo. Por favor, ingrese uno diferente.";
                    return;
                }

                // Validar si el DNI ya está en uso por otro legajo
                bool dniDuplicado = db.Usuarios.Any(u => u.Usu_dni == dni && u.Usu_legajo != legajoOriginal);
                if (dniDuplicado)
                {
                    lblError.Text = "Ya existe un Playero con ese DNI. Por favor, ingrese uno diferente.";
                    return;
                }

                if (esAlta)
                {
                    // Alta
                    var nuevoUsuario = new Usuarios
                    {
                        Usu_legajo = legajo,
                        Usu_dni = dni,
                        Usu_pass = pass,
                        Usu_ap = apellido,
                        Usu_nom = nombre,
                        Usu_tipo = "Playero"
                    };

                    var nuevoPlayero = new Playero
                    {
                        Playero_legajo = legajo,
                        Est_id = estId,
                        Usuarios = nuevoUsuario
                    };

                    db.Usuarios.Add(nuevoUsuario);
                    db.Playero.Add(nuevoPlayero);
                }
                else
                {
                    // Edición
                    Usuarios usuarioEdicion = db.Usuarios.FirstOrDefault(u => u.Usu_legajo == legajoOriginal);
                    Playero playeroEdicion = db.Playero.FirstOrDefault(p => p.Playero_legajo == legajoOriginal);

                    if (usuarioEdicion != null)
                    {
                        usuarioEdicion.Usu_legajo = legajo; // permite cambiar el legajo
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
                }

                db.SaveChanges();
            }

            Response.Redirect("Playero_CRUD.aspx");
        }
    }
}