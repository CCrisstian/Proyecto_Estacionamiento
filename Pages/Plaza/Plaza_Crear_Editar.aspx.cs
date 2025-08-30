using System;
using System.Linq;

namespace Proyecto_Estacionamiento.Pages.Plaza
{
    public partial class Plaza_Crear_Editar : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarEstacionamientos();
                CargarCategorias();

                if (Request.QueryString["id"] != null)  // verificamos si es una Plaza existente mediante el parámetro id en la URL
                {
                    lblTitulo.Text = "Editar Plaza";
                    int plazaId = int.Parse(Request.QueryString["id"]); // Guardamos el id de la Plaza desde la URL
                    ddlEstacionamiento.Enabled = false; // Deshabilita el campo Estacionamiento
                    CargarPlaza(plazaId);   // Cargamos el id de la Plaza para poder Editarla
                }
                else
                {
                    lblTitulo.Text = "Agregar Plaza";
                }
            }
        }

        // Cargamos los Estacionamientos al cargar la página para poder seleccionar uno por su Nombre
        private void CargarEstacionamientos()
        {
            int legajo = Convert.ToInt32(Session["Usu_legajo"]);

            using (var context = new ProyectoEstacionamientoEntities())
            {
                // Filtrar por Dueño_Legajo y que estén disponibles
                var estacionamientos = context.Estacionamiento
                    .Where(e => e.Dueño_Legajo == legajo && e.Est_Disponibilidad == true)
                    .Select(e => new { e.Est_id, e.Est_nombre })
                    .ToList();

                ddlEstacionamiento.DataSource = estacionamientos;
                ddlEstacionamiento.DataValueField = "Est_id";
                ddlEstacionamiento.DataTextField = "Est_nombre";
                ddlEstacionamiento.DataBind();
            }
        }


        // Cargamos las Categorías de Vehículos al cargar la página para poder seleccionar una por su Descripción
        private void CargarCategorias()
        {
            using (var db = new ProyectoEstacionamientoEntities())
            {
                ddlCategoria.DataSource = db.Categoria_Vehiculo.ToList();
                ddlCategoria.DataTextField = "Categoria_descripcion";
                ddlCategoria.DataValueField = "Categoria_id";
                ddlCategoria.DataBind();
            }
        }

        // Cargamos los datos de la Plaza seleccionada para poder Editarla
        private void CargarPlaza(int plazaId)
        {
            using (var db = new ProyectoEstacionamientoEntities())
            {
                // Busca una Plaza por su ID y, si la encuentra, carga sus datos en los campos de la página para Edición.
                // Si no encuentra la Plaza, muestra un mensaje de error.
                var plaza = db.Plaza.FirstOrDefault(p => p.Plaza_id == plazaId);
                if (plaza != null)
                {
                    ddlEstacionamiento.SelectedValue = plaza.Est_id.ToString();
                    ddlCategoria.SelectedValue = plaza.Categoria_id.ToString();
                    txtNombre.Text = plaza.Plaza_Nombre;
                    txtTipo.Text = plaza.Plaza_Tipo;
                    ddlDisponible.SelectedValue = plaza.Plaza_Disponibilidad ? "true" : "false";
                }
            }
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            using (var db = new ProyectoEstacionamientoEntities())
            {
                Proyecto_Estacionamiento.Plaza plaza;

                if (Request.QueryString["id"] != null)
                {
                    // Editar
                    int plazaId = int.Parse(Request.QueryString["id"]);
                    plaza = db.Plaza.FirstOrDefault(p => p.Plaza_id == plazaId);
                    if (plaza == null)
                    {
                        lblMensaje.Text = "Plaza no encontrada.";
                        return;
                    }
                }
                else
                {
                    // Agregar
                    if (string.IsNullOrEmpty(ddlEstacionamiento.SelectedValue))
                    {
                        lblMensaje.Text = "Debe seleccionar un estacionamiento.";
                        return;
                    }

                    plaza = new Proyecto_Estacionamiento.Plaza();
                    plaza.Est_id = int.Parse(ddlEstacionamiento.SelectedValue);
                    db.Plaza.Add(plaza);
                }

                // Validaciones
                if (string.IsNullOrWhiteSpace(txtNombre.Text))
                {
                    lblMensaje.Text = "El nombre de la plaza es obligatorio.";
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtTipo.Text))
                {
                    lblMensaje.Text = "El tipo de plaza es obligatorio.";
                    return;
                }

                if (string.IsNullOrEmpty(ddlCategoria.SelectedValue))
                {
                    lblMensaje.Text = "Debe seleccionar una categoría.";
                    return;
                }

                if (string.IsNullOrEmpty(ddlDisponible.SelectedValue))
                {
                    lblMensaje.Text = "Debe seleccionar la disponibilidad.";
                    return;
                }

                // Asignación de valores
                plaza.Categoria_id = int.Parse(ddlCategoria.SelectedValue);
                plaza.Plaza_Nombre = txtNombre.Text.Trim();
                plaza.Plaza_Tipo = txtTipo.Text.Trim();
                plaza.Plaza_Disponibilidad = ddlDisponible.SelectedValue == "true";

                db.SaveChanges();

                string accion = Request.QueryString["id"] == null ? "agregado" : "editado";
                Response.Redirect($"Plaza_Listar.aspx?exito=1&accion={accion}");
            }
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Pages/Plaza/Plaza_Listar.aspx");
        }
    }
}